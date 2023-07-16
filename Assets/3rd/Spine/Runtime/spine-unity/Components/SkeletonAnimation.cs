/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif

using UnityEngine;

namespace Spine.Unity
{

#if NEW_PREFAB_SYSTEM
    [ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
    [AddComponentMenu("Spine/SkeletonAnimation")]
    public class SkeletonAnimation : SkeletonRenderer, ISkeletonAnimation, IAnimationStateComponent
    {

        public delegate void Created(SkeletonAnimation skeletonAnimation);

        public bool ForceVisible;

        public static bool StopUpdates;

        public bool ShowReflection = true;

        public bool HasReflection;

        public bool randomOffset;

        public float offsetAmount = 0.2f;

        public static Created OnCreated;

        public bool ForceNoLimitUpdate;

        public Spine.AnimationState state;

        private bool wasUpdatedAfterInit = true;

        [SerializeField]
        [SpineAnimation("", "", true, false)]
        private string _animationName;

        public bool loop;

        public float timeScale = 1f;

        private int FrameIntervalOffset;

        public int UpdateInterval = 2;

        private bool Visible = true;

        private float _accumDeltaTime;

        private float _accumUnscaledDeltaTime;

        public bool UseDeltaTime = true;

        public Spine.AnimationState AnimationState
        {
            get
            {
                return state;
            }
        }

        public string AnimationName
        {
            get
            {
                if (!valid)
                {
                    return _animationName;
                }
                TrackEntry current = state.GetCurrent(0);
                if (current != null)
                {
                    return current.Animation.Name;
                }
                return null;
            }
            set
            {
                if (_animationName == value)
                {
                    TrackEntry current = state.GetCurrent(0);
                    if (current != null && current.loop == loop)
                    {
                        return;
                    }
                }
                _animationName = value;
                if (string.IsNullOrEmpty(value))
                {
                    state.ClearTrack(0);
                    return;
                }
                Spine.Animation animation = skeletonDataAsset.GetSkeletonData(false).FindAnimation(value);
                if (animation != null)
                {
                    state.SetAnimation(0, animation, loop);
                }
            }
        }

        private bool LimitUpdate
        {
            get
            {
                return false;
            }
        }

        protected event UpdateBonesDelegate _UpdateLocal;

        protected event UpdateBonesDelegate _UpdateWorld;

        protected event UpdateBonesDelegate _UpdateComplete;

        public event UpdateBonesDelegate UpdateLocal
        {
            add
            {
                _UpdateLocal += value;
            }
            remove
            {
                _UpdateLocal -= value;
            }
        }

        public event UpdateBonesDelegate UpdateWorld
        {
            add
            {
                _UpdateWorld += value;
            }
            remove
            {
                _UpdateWorld -= value;
            }
        }

        public event UpdateBonesDelegate UpdateComplete
        {
            add
            {
                _UpdateComplete += value;
            }
            remove
            {
                _UpdateComplete -= value;
            }
        }

        private void Start()
        {
            if (Application.isPlaying && OnCreated != null)
            {
                OnCreated(this);
            }
            if (randomOffset)
            {
                timeScale += Random.Range(offsetAmount * -1f, offsetAmount);
            }
            else
            {
                timeScale = 1f;
            }
            FrameIntervalOffset = Random.Range(0, UpdateInterval);
        }

        public static SkeletonAnimation AddToGameObject(GameObject gameObject, SkeletonDataAsset skeletonDataAsset)
        {
            return SkeletonRenderer.AddSpineComponent<SkeletonAnimation>(gameObject, skeletonDataAsset);
        }

        public static SkeletonAnimation NewSkeletonAnimationGameObject(SkeletonDataAsset skeletonDataAsset)
        {
            return SkeletonRenderer.NewSpineGameObject<SkeletonAnimation>(skeletonDataAsset);
        }

        public override void ClearState()
        {
            base.ClearState();
            if (state != null)
            {
                state.ClearTracks();
            }
        }

        public override void Initialize(bool overwrite)
        {
            if (valid && !overwrite)
            {
                return;
            }
            base.Initialize(overwrite);
            if (!valid)
            {
                return;
            }
            state = new Spine.AnimationState(skeletonDataAsset.GetAnimationStateData());
            wasUpdatedAfterInit = false;
            if (!string.IsNullOrEmpty(_animationName))
            {
                Spine.Animation animation = skeletonDataAsset.GetSkeletonData(false).FindAnimation(_animationName);
                if (animation != null)
                {
                    state.SetAnimation(0, animation, loop);
                }
            }
        }

        private void OnBecameInvisible()
        {
            Visible = false;
        }

        private void OnBecameVisible()
        {
            Visible = true;
        }

        private void Update()
        {
            if (StopUpdates)
            {
                return;
            }
            if (!LimitUpdate || ForceNoLimitUpdate)
            {
                Update(UseDeltaTime ? Time.deltaTime : Time.unscaledDeltaTime);
            }
            else if (Visible || ForceVisible)
            {
                _accumDeltaTime += Time.deltaTime;
                _accumUnscaledDeltaTime += Time.unscaledDeltaTime;
                if ((Time.frameCount + FrameIntervalOffset) % UpdateInterval == 0)
                {
                    Update(UseDeltaTime ? _accumDeltaTime : _accumUnscaledDeltaTime);
                    _accumDeltaTime = 0f;
                    _accumUnscaledDeltaTime = 0f;
                }
            }
        }

        public void Update(float deltaTime)
        {
            if (!StopUpdates && (!UseDeltaTime || Time.timeScale != 0f) && valid && state != null)
            {
                deltaTime *= timeScale;
                skeleton.Update(deltaTime);
                state.Update(deltaTime);
                state.Apply(skeleton);
                if (this._UpdateLocal != null)
                {
                    this._UpdateLocal(this);
                }
                skeleton.UpdateWorldTransform();
                if (this._UpdateWorld != null)
                {
                    this._UpdateWorld(this);
                    skeleton.UpdateWorldTransform();
                }
                if (this._UpdateComplete != null)
                {
                    this._UpdateComplete(this);
                }
                wasUpdatedAfterInit = true;
            }
        }

        public void BaseLateUpdate()
        {
            if (!wasUpdatedAfterInit)
            {
                Update(0f);
            }
            base.LateUpdate();
        }

        public override void LateUpdate()
        {
            if (StopUpdates)
            {
                return;
            }
            if (!LimitUpdate)
            {
                if (!wasUpdatedAfterInit)
                {
                    Update(0f);
                }
                base.LateUpdate();
            }
            else if ((Time.frameCount + FrameIntervalOffset) % UpdateInterval == 0 && (Visible || ForceVisible))
            {
                if (!wasUpdatedAfterInit)
                {
                    Update(0f);
                }
                base.LateUpdate();
            }
        }

    }
}
