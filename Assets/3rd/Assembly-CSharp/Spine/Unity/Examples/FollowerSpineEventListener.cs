using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;

namespace Spine.Unity.Examples
{
	public class FollowerSpineEventListener : BaseMonoBehaviour
	{
		public List<followerSpineEventListeners> spineEventListeners = new List<followerSpineEventListeners>();

		public List<SoundOnAnimationData> Sounds = new List<SoundOnAnimationData>();

		[SerializeField]
		private SkeletonAnimation spine;

		[SerializeField]
		private interaction_FollowerInteraction follower;

		[SerializeField]
		private bool isRecruit;

		[SerializeField]
		private Interaction_Follower followerRecruit;

		[SerializeField]
		private Interaction_FollowerSpawn followerRecruitSpawn;

		[SerializeField]
		private FollowerRecruit followerRecruitSecond;

		[SerializeField]
		private Interaction_FollowerInSpiderWeb followerRecruitSpider;

		[SerializeField]
		private Interaction_FollowerDessentingChoice followerRecruitDissenter;

		private string _pitchParameter = "follower_pitch";

		private string _vibratoParameter = "follower_vibrato";

		private float _pitchValue;

		private float _vibratoValue;

		private int _followerID;

		public int AnimationTrack;

		public string currentAnimationDebug;

		private string cs;

		public EventInstance _loopInstance;

		private bool changed;

		private string CurrentAnimation
		{
			set
			{
				if (cs != value)
				{
					foreach (SoundOnAnimationData sound in Sounds)
					{
						if (sound == null || sound.SkeletonData == null || sound.SkeletonData.state == null || sound.SkeletonData.state.GetCurrent(AnimationTrack) == null)
						{
							Debug.Log("Error: Cant get spine animation to set audio source");
							continue;
						}
						AudioManager.Instance.StopLoop(sound.LoopedSound);
						if ((sound.SkeletonData.state.GetCurrent(AnimationTrack).ToString() == cs && sound.position == SoundOnAnimationData.Position.Beginning) || (sound.SkeletonData.state.GetCurrent(AnimationTrack).ToString() == cs && sound.position == SoundOnAnimationData.Position.End))
						{
							AudioManager.Instance.PlayOneShotAndSetParametersValue(sound.AudioSourcePath, _pitchParameter, _pitchValue, _vibratoParameter, _vibratoValue, spine.transform);
						}
						else if (sound.SkeletonData.state.GetCurrent(AnimationTrack).ToString() == sound.SkeletonsAnimations && sound.position == SoundOnAnimationData.Position.Loop)
						{
							sound.LoopedSound = AudioManager.Instance.CreateLoop(sound.AudioSourcePath, spine.gameObject);
							AudioManager.Instance.SetEventInstanceParameter(sound.LoopedSound, _pitchParameter, _pitchValue);
							AudioManager.Instance.SetEventInstanceParameter(sound.LoopedSound, _vibratoParameter, _vibratoValue);
							AudioManager.Instance.PlayLoop(sound.LoopedSound);
						}
					}
				}
				cs = value;
			}
		}

		private void OnDestroy()
		{
			DisableLoops();
		}

		private void OnDisable()
		{
			DisableLoops();
		}

		private void DisableLoops()
		{
			AudioManager.Instance.StopLoop(_loopInstance);
			foreach (SoundOnAnimationData sound in Sounds)
			{
				if (sound.position == SoundOnAnimationData.Position.Loop)
				{
					AudioManager.Instance.StopLoop(sound.LoopedSound);
				}
			}
		}

		private void Update()
		{
			if (spine != null && spine.state.GetCurrent(AnimationTrack) != null)
			{
				CurrentAnimation = spine.state.GetCurrent(AnimationTrack).ToString();
				currentAnimationDebug = spine.state.GetCurrent(AnimationTrack).ToString();
			}
		}

		public void UpdateSpine()
		{
			if (!(spine != null))
			{
				return;
			}
			foreach (followerSpineEventListeners spineEventListener in spineEventListeners)
			{
				spineEventListener.skeletonAnimation = spine;
			}
			foreach (SoundOnAnimationData sound in Sounds)
			{
				sound.SkeletonData = spine;
			}
		}

		public void PlayFollowerVO(string soundPath)
		{
			if (follower.follower.Brain.Info.ID == FollowerManager.HeketID)
			{
				AudioManager.Instance.PlayOneShotAndSetParametersValue(soundPath, _pitchParameter, _pitchValue, _vibratoParameter, _vibratoValue, "fol_heket", 1f, spine.transform);
			}
			else
			{
				AudioManager.Instance.PlayOneShotAndSetParametersValue(soundPath, _pitchParameter, _pitchValue, _vibratoParameter, _vibratoValue, spine.transform, follower.follower.Brain.Info.ID);
			}
		}

		public void PlayFollowerVOLoop(string soundPath)
		{
			_loopInstance = AudioManager.Instance.CreateLoop(soundPath, spine.gameObject, true);
			_loopInstance.setParameterByName(_pitchParameter, _pitchValue);
			_loopInstance.setParameterByName(_vibratoParameter, _vibratoValue);
		}

		public void StopFollowerVOLoop()
		{
			AudioManager.Instance.StopLoop(_loopInstance);
		}

		public void SetPitchAndVibrator(float pitch, float vibrato, int followerID)
		{
			foreach (followerSpineEventListeners spineEventListener in spineEventListeners)
			{
				spineEventListener.Start();
				spineEventListener._pitchParameter = _pitchParameter;
				spineEventListener._vibratoParameter = _vibratoParameter;
				spineEventListener._pitchValue = pitch;
				spineEventListener._vibratoValue = vibrato;
				spineEventListener._followerID = followerID;
			}
		}

		private void Start()
		{
			if (!isRecruit)
			{
				if (follower != null && follower.follower != null && follower.follower.Brain != null)
				{
					_pitchValue = follower.follower.Brain._directInfoAccess.follower_pitch;
					_vibratoValue = follower.follower.Brain._directInfoAccess.follower_vibrato;
					_followerID = follower.follower.Brain._directInfoAccess.ID;
				}
				{
					foreach (followerSpineEventListeners spineEventListener in spineEventListeners)
					{
						spineEventListener.Start();
						spineEventListener._pitchParameter = _pitchParameter;
						spineEventListener._vibratoParameter = _vibratoParameter;
						spineEventListener._pitchValue = _pitchValue;
						spineEventListener._vibratoValue = _vibratoValue;
					}
					return;
				}
			}
			followerRecruitAssigned();
		}

		private void followerRecruitAssigned()
		{
			StartCoroutine(FollowerRecruitAssigned());
		}

		private IEnumerator FollowerRecruitAssigned()
		{
			yield return new WaitForEndOfFrame();
			changed = false;
			if (followerRecruit != null)
			{
				while (followerRecruit.followerInfo == null)
				{
					yield return null;
				}
				_pitchValue = followerRecruit.followerInfo.follower_pitch;
				_vibratoValue = followerRecruit.followerInfo.follower_vibrato;
				_followerID = followerRecruit.followerInfo.ID;
				changed = true;
			}
			else if (followerRecruitSpawn != null && followerRecruitSpawn._followerInfo != null)
			{
				_pitchValue = followerRecruitSpawn._followerInfo.follower_pitch;
				_vibratoValue = followerRecruitSpawn._followerInfo.follower_vibrato;
				_followerID = followerRecruitSpawn._followerInfo.ID;
				changed = true;
			}
			else if (followerRecruitSecond != null)
			{
				_pitchValue = followerRecruitSecond.FollowerInteraction.follower.Brain._directInfoAccess.follower_pitch;
				_vibratoValue = followerRecruitSecond.FollowerInteraction.follower.Brain._directInfoAccess.follower_vibrato;
				_followerID = followerRecruitSecond.FollowerInteraction.follower.Brain._directInfoAccess.ID;
				changed = true;
			}
			else if (followerRecruitSpider != null)
			{
				_pitchValue = followerRecruitSpider._followerInfo.follower_pitch;
				_vibratoValue = followerRecruitSpider._followerInfo.follower_vibrato;
				_followerID = followerRecruitSpider._followerInfo.ID;
				changed = true;
			}
			else if (followerRecruitDissenter != null)
			{
				_pitchValue = followerRecruitDissenter.followerInfo.follower_pitch;
				_vibratoValue = followerRecruitDissenter.followerInfo.follower_vibrato;
				_followerID = followerRecruitDissenter.followerInfo.ID;
				changed = true;
			}
			if (!changed)
			{
				yield break;
			}
			foreach (followerSpineEventListeners spineEventListener in spineEventListeners)
			{
				spineEventListener.Start();
				spineEventListener._pitchParameter = _pitchParameter;
				spineEventListener._vibratoParameter = _vibratoParameter;
				spineEventListener._pitchValue = _pitchValue;
				spineEventListener._vibratoValue = _vibratoValue;
				spineEventListener._followerID = _followerID;
			}
		}
	}
}
