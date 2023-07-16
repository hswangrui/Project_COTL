using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Rendering;

public class SimpleSpineAnimator : BaseMonoBehaviour
{
	public delegate void SpineEvent(string EventName);

	[Serializable]
	public class SpineChartacterAnimationData
	{
		public StateMachine.State State;

		[HideInInspector]
		public AnimationReferenceAsset DefaultAnimation;

		public AnimationReferenceAsset Animation;

		public AnimationReferenceAsset AddAnimation;

		public bool DisableMixDuration;

		public bool Looping;

		public void InitDefaults()
		{
			DefaultAnimation = Animation;
		}
	}

	public int AnimationTrack;

	private StateMachine state;

	private SkeletonAnimation _anim;

	public bool AutomaticallySetFacing = true;

	public AnimationReferenceAsset DefaultLoop;

	public AnimationReferenceAsset StartMoving;

	public AnimationReferenceAsset StopMoving;

	public AnimationReferenceAsset TurnAnimation;

	public AnimationReferenceAsset NorthMoving;

	public AnimationReferenceAsset NorthDiagonalMoving;

	public AnimationReferenceAsset SouthMoving;

	public AnimationReferenceAsset SouthDiagonalMoving;

	public AnimationReferenceAsset NorthIdle;

	public AnimationReferenceAsset Dodging;

	public AnimationReferenceAsset NorthDodging;

	public AnimationReferenceAsset SouthDodging;

	public AnimationReferenceAsset Aiming;

	public List<SpineChartacterAnimationData> Animations = new List<SpineChartacterAnimationData>();

	public AnimationReferenceAsset Idle;

	public AnimationReferenceAsset IdleWithItem;

	public AnimationReferenceAsset MovingWithItem;

	public AnimationReferenceAsset Moving;

	public AnimationReferenceAsset Inactive;

	public AnimationReferenceAsset Action;

	public AnimationReferenceAsset Sleeping;

	public AnimationReferenceAsset PreAttack;

	public AnimationReferenceAsset PostAttack;

	public AnimationReferenceAsset Defending;

	public AnimationReferenceAsset HitLeft;

	public AnimationReferenceAsset HitRight;

	public AnimationReferenceAsset Dodge;

	public SimpleInventory inventory;

	private TrackEntry Track;

	private StateMachine.State cs;

	private TrackEntry t;

	public bool isFillWhite;

	public bool UpdateAnimsOnStateChange = true;

	private MaterialPropertyBlock BlockWhite;

	private MeshRenderer meshRenderer;

	private int fillAlpha;

	private int fillColor;

	private Color WarningColour = new Color(0.7490196f, 35f / 51f, 0.1372549f, 1f);

	private float flashTickTimer;

	private bool FlashingRed;

	private Coroutine cFlashFillRoutine;

	private float FlashRedMultiplier = 0.01f;

	private float xScaleSpeed;

	private float yScaleSpeed;

	private float moveSquish;

	private float xScale;

	private float yScale;

	private bool flipX;

	public bool ReverseFacing;

	public bool StartOnDefault = true;

	private int _Dir;

	public bool ForceDirectionalMovement;

	public SkeletonAnimation anim
	{
		get
		{
			if (_anim == null)
			{
				_anim = GetComponent<SkeletonAnimation>();
			}
			return _anim;
		}
	}

	public bool IsVisible
	{
		get
		{
			if (meshRenderer == null)
			{
				return true;
			}
			return meshRenderer.isVisible;
		}
	}

	public StateMachine.State GetCurrentState
	{
		get
		{
			return cs;
		}
	}

	private StateMachine.State CurrentState
	{
		get
		{
			return cs;
		}
		set
		{
			if (cs != value)
			{
				cs = value;
				UpdateAnimFromState();
			}
		}
	}

	public int Dir
	{
		get
		{
			return _Dir;
		}
		set
		{
			if (_Dir != value && anim.timeScale != 0.0001f)
			{
				if (state != null && state.CURRENT_STATE == StateMachine.State.Moving && TurnAnimation != null)
				{
					anim.AnimationState.SetAnimation(AnimationTrack, TurnAnimation, true);
					anim.AnimationState.AddAnimation(AnimationTrack, Moving, true, 0f);
				}
				_Dir = value;
				anim.skeleton.ScaleX = Dir;
			}
		}
	}

	public event SpineEvent OnSpineEvent;

	public void Initialize(bool overwrite)
	{
		anim.Initialize(overwrite);
	}

	public SpineChartacterAnimationData GetAnimationData(StateMachine.State State)
	{
		foreach (SpineChartacterAnimationData animation in Animations)
		{
			if (animation.State == State)
			{
				return animation;
			}
		}
		return null;
	}

	private void UpdateAnimFromState()
	{
		if (anim == null || !UpdateAnimsOnStateChange)
		{
			return;
		}
		cs = state.CURRENT_STATE;
		if (Animations.Count > 0)
		{
			foreach (SpineChartacterAnimationData animation in Animations)
			{
				if (animation.State != cs)
				{
					continue;
				}
				if (!(animation.Animation != null))
				{
					return;
				}
				if (animation.State == StateMachine.State.Idle)
				{
					if (StopMoving != null)
					{
						anim.AnimationState.SetAnimation(AnimationTrack, StopMoving, false);
						Track = anim.AnimationState.AddAnimation(AnimationTrack, animation.Animation, true, 0f);
					}
					else
					{
						Track = anim.AnimationState.SetAnimation(AnimationTrack, animation.Animation, true);
					}
					return;
				}
				if (animation.State == StateMachine.State.Moving)
				{
					if (StartMoving != null)
					{
						anim.AnimationState.SetAnimation(AnimationTrack, StartMoving, false);
						Track = anim.AnimationState.AddAnimation(AnimationTrack, animation.Animation, true, 0f);
					}
					else
					{
						Track = anim.AnimationState.SetAnimation(AnimationTrack, animation.Animation, animation.Looping);
					}
					return;
				}
				if (animation.AddAnimation == null)
				{
					Track = anim.AnimationState.SetAnimation(AnimationTrack, animation.Animation, animation.Looping);
					if (animation.DisableMixDuration)
					{
						Track.MixDuration = 0f;
					}
					return;
				}
				Track = anim.AnimationState.SetAnimation(AnimationTrack, animation.Animation, false);
				if (animation.DisableMixDuration)
				{
					Track.MixDuration = 0f;
				}
				anim.AnimationState.AddAnimation(AnimationTrack, animation.AddAnimation, animation.Looping, 0f);
				return;
			}
			if (DefaultLoop != null)
			{
				Track = anim.AnimationState.SetAnimation(AnimationTrack, DefaultLoop, true);
			}
			return;
		}
		switch (cs)
		{
		case StateMachine.State.Idle:
			if (inventory != null && inventory.Item != 0 && IdleWithItem != null)
			{
				anim.AnimationState.SetAnimation(AnimationTrack, IdleWithItem, true);
			}
			else
			{
				anim.AnimationState.SetAnimation(AnimationTrack, Idle, true);
			}
			break;
		case StateMachine.State.Moving:
			if (inventory != null && inventory.Item != 0 && MovingWithItem != null)
			{
				anim.AnimationState.SetAnimation(AnimationTrack, MovingWithItem, true);
			}
			else if (StartMoving != null)
			{
				anim.AnimationState.SetAnimation(AnimationTrack, StartMoving, true);
				anim.AnimationState.AddAnimation(AnimationTrack, Moving, true, 0f);
			}
			else if (Moving != null)
			{
				anim.AnimationState.SetAnimation(AnimationTrack, Moving, true);
			}
			break;
		case StateMachine.State.CustomAction0:
			if (Action != null)
			{
				anim.AnimationState.SetAnimation(AnimationTrack, Action, true);
			}
			break;
		case StateMachine.State.BeingCarried:
			if (Idle != null)
			{
				anim.AnimationState.SetAnimation(AnimationTrack, Idle, true);
			}
			break;
		case StateMachine.State.Sleeping:
			if (Sleeping != null)
			{
				anim.AnimationState.SetAnimation(AnimationTrack, Sleeping, true);
			}
			break;
		case StateMachine.State.SignPostAttack:
			if (PreAttack != null)
			{
				anim.AnimationState.SetAnimation(AnimationTrack, PreAttack, false);
			}
			break;
		case StateMachine.State.RecoverFromAttack:
			if (PostAttack != null)
			{
				anim.AnimationState.SetAnimation(AnimationTrack, PostAttack, false);
			}
			break;
		case StateMachine.State.Defending:
			if (Defending != null)
			{
				anim.AnimationState.SetAnimation(AnimationTrack, Defending, true);
			}
			break;
		case StateMachine.State.HitLeft:
			if (HitLeft != null)
			{
				anim.AnimationState.SetAnimation(AnimationTrack, HitLeft, true);
			}
			break;
		case StateMachine.State.HitRight:
			if (HitRight != null)
			{
				anim.AnimationState.SetAnimation(AnimationTrack, HitRight, true);
			}
			break;
		case StateMachine.State.Dodging:
			if (Dodge != null)
			{
				anim.AnimationState.SetAnimation(AnimationTrack, Dodge, true);
			}
			break;
		default:
			if (Idle != null && anim.AnimationState != null)
			{
				anim.AnimationState.SetAnimation(AnimationTrack, Idle, true);
			}
			break;
		case StateMachine.State.TimedAction:
		case StateMachine.State.Aiming:
			break;
		}
	}

	public string CurrentAnimation()
	{
		return anim.AnimationName;
	}

	public TrackEntry Animate(string Animation, int Track, bool Loop)
	{
		if (anim.AnimationState.Data.SkeletonData.FindAnimation(Animation) != null)
		{
			return anim.AnimationState.SetAnimation(Track, Animation, Loop);
		}
		return null;
	}

	public TrackEntry Animate(string Animation, int Track, bool Loop, float MixTime)
	{
		if (anim.AnimationState.Data.SkeletonData.FindAnimation(Animation) != null)
		{
			t = anim.AnimationState.SetAnimation(Track, Animation, Loop);
			t.MixTime = MixTime;
			return t;
		}
		return null;
	}

	public void AddAnimate(string Animation, int Track, bool Loop, float Delay)
	{
		anim.AnimationState.AddAnimation(Track, Animation, Loop, Delay);
	}

	public void ClearTrackAfterAnimation(int Track)
	{
		anim.AnimationState.AddEmptyAnimation(Track, 0.1f, 0f);
	}

	public void SetAttachment(string slotName, string attachmentName)
	{
		anim.skeleton.SetAttachment(slotName, attachmentName);
	}

	public void SetSkin(string Skin)
	{
		anim.skeleton.SetSkin(Skin);
		anim.skeleton.SetSlotsToSetupPose();
	}

	public void SetColor(Color color)
	{
		anim.skeleton.SetColor(color);
	}

	public void FlashMeWhite()
	{
		if (flashTickTimer >= 0.12f)
		{
			FlashWhite(!isFillWhite);
			flashTickTimer = 0f;
		}
		flashTickTimer += Time.deltaTime;
	}

	public void FlashWhite(bool toggle)
	{
		if (SettingsManager.Settings.Accessibility.FlashingLights && !(meshRenderer == null))
		{
			if (BlockWhite == null)
			{
				BlockWhite = new MaterialPropertyBlock();
			}
			BlockWhite.SetColor(fillColor, WarningColour);
			BlockWhite.SetFloat(fillAlpha, toggle ? 0.33f : 0f);
			meshRenderer.SetPropertyBlock(BlockWhite);
			isFillWhite = toggle;
		}
	}

	public void FillWhite(bool toggle)
	{
		if (!(meshRenderer == null))
		{
			if (BlockWhite == null)
			{
				BlockWhite = new MaterialPropertyBlock();
			}
			BlockWhite.SetColor(fillColor, Color.white);
			BlockWhite.SetFloat(fillAlpha, toggle ? 1f : 0f);
			meshRenderer.SetPropertyBlock(BlockWhite);
			isFillWhite = toggle;
		}
	}

	public void FillColor(Color color, float Alpha = 1f)
	{
		if (!(meshRenderer == null))
		{
			if (BlockWhite == null)
			{
				BlockWhite = new MaterialPropertyBlock();
			}
			BlockWhite.SetColor(fillColor, color);
			BlockWhite.SetFloat(fillAlpha, Alpha);
			meshRenderer.SetPropertyBlock(BlockWhite);
		}
	}

	public void FlashFillRed(float opacity = 0.5f)
	{
		if (SettingsManager.Settings.Accessibility.FlashingLights)
		{
			FlashingRed = true;
			if (cFlashFillRoutine != null)
			{
				StopCoroutine(cFlashFillRoutine);
			}
			cFlashFillRoutine = StartCoroutine(FlashOnHitRoutine(opacity));
		}
	}

	private IEnumerator FlashOnHitRoutine(float opacity)
	{
		MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
		meshRenderer.receiveShadows = false;
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		meshRenderer.SetPropertyBlock(propertyBlock);
		SetColor(new Color(1f, 1f, 1f, opacity));
		yield return new WaitForSeconds(6f * FlashRedMultiplier);
		SetColor(new Color(0f, 0f, 0f, opacity));
		yield return new WaitForSeconds(3f * FlashRedMultiplier);
		SetColor(new Color(1f, 0f, 0f, opacity));
		yield return new WaitForSeconds(2f * FlashRedMultiplier);
		SetColor(new Color(0f, 0f, 0f, opacity));
		yield return new WaitForSeconds(2f * FlashRedMultiplier);
		SetColor(new Color(1f, 0f, 0f, opacity));
		yield return new WaitForSeconds(2f * FlashRedMultiplier);
		SetColor(new Color(1f, 1f, 1f, 1f));
		FlashingRed = false;
		meshRenderer.receiveShadows = true;
		meshRenderer.shadowCastingMode = ShadowCastingMode.On;
	}

	private IEnumerator DoFlashFillRed()
	{
		if (meshRenderer == null)
		{
			yield break;
		}
		MaterialPropertyBlock block = new MaterialPropertyBlock();
		meshRenderer.SetPropertyBlock(block);
		block.SetColor(fillColor, Color.red);
		block.SetFloat(fillAlpha, 1f);
		meshRenderer.SetPropertyBlock(block);
		yield return new WaitForSeconds(0.1f);
		float Progress = 1f;
		while (true)
		{
			float num;
			Progress = (num = Progress - 0.05f * anim.timeScale);
			if (num >= 0f)
			{
				if (Progress <= 0f)
				{
					Progress = 0f;
				}
				block.SetFloat(fillAlpha, Progress);
				meshRenderer.SetPropertyBlock(block);
				yield return null;
				continue;
			}
			break;
		}
	}

	public void FlashRedTint()
	{
		if (SettingsManager.Settings.Accessibility.FlashingLights)
		{
			StopCoroutine(DoFlashTintRed());
			StartCoroutine(DoFlashTintRed());
		}
	}

	private IEnumerator DoFlashTintRed()
	{
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + 0.05f * anim.timeScale);
			if (!(num <= 1f))
			{
				break;
			}
			SetColor(Color.Lerp(Color.red, Color.white, Progress));
			yield return null;
		}
		SetColor(Color.white);
	}

	public void FlashFillBlack(bool ignoreSpineTimeScale)
	{
		if (SettingsManager.Settings.Accessibility.FlashingLights)
		{
			StopCoroutine(DoFlashFillBlack(ignoreSpineTimeScale));
			StartCoroutine(DoFlashFillBlack(ignoreSpineTimeScale));
		}
	}

	private IEnumerator DoFlashFillBlack(bool ignoreSpineTimeScale)
	{
		MaterialPropertyBlock block = new MaterialPropertyBlock();
		meshRenderer.SetPropertyBlock(block);
		SetColor(Color.black);
		yield return new WaitForSeconds(0.1f);
		SetColor(Color.white);
		float Progress = 1f;
		while (Progress > 0f)
		{
			Progress -= Time.deltaTime * (ignoreSpineTimeScale ? 1f : anim.timeScale);
			block.SetFloat(fillAlpha, Progress);
			meshRenderer.SetPropertyBlock(block);
			yield return null;
		}
	}

	public void FlashFillGreen()
	{
		if (SettingsManager.Settings.Accessibility.FlashingLights)
		{
			StopCoroutine(DoFlashFillGreen());
			StartCoroutine(DoFlashFillGreen());
		}
	}

	private IEnumerator DoFlashFillGreen()
	{
		MaterialPropertyBlock block = new MaterialPropertyBlock();
		meshRenderer.SetPropertyBlock(block);
		SetColor(Color.green);
		yield return new WaitForSeconds(0.1f);
		SetColor(Color.white);
		float Progress = 1f;
		while (Progress > 0f)
		{
			Progress -= Time.deltaTime * anim.timeScale;
			block.SetFloat(fillAlpha, Progress);
			meshRenderer.SetPropertyBlock(block);
			yield return null;
		}
	}

	public void ChangeStateAnimation(StateMachine.State state, string NewAnimation)
	{
		SpineChartacterAnimationData animationData = GetAnimationData(state);
		AnimationReferenceAsset animationReferenceAsset = ScriptableObject.CreateInstance<AnimationReferenceAsset>();
		animationReferenceAsset.Animation = anim.skeleton.Data.FindAnimation(NewAnimation);
		animationReferenceAsset.name = NewAnimation;
		animationData.Animation = animationReferenceAsset;
		if (CurrentState == state && this.state.CURRENT_STATE == state)
		{
			UpdateAnimFromState();
		}
	}

	public AnimationReferenceAsset GetAnimationReference(string AnimationName)
	{
		return new AnimationReferenceAsset
		{
			Animation = anim.skeleton.Data.FindAnimation(AnimationName),
			name = AnimationName
		};
	}

	public void ResetAnimationsToDefaults()
	{
		foreach (SpineChartacterAnimationData animation in Animations)
		{
			animation.Animation = animation.DefaultAnimation;
		}
		UpdateAnimFromState();
	}

	public float Duration()
	{
		return anim.AnimationState.GetCurrent(AnimationTrack).Animation.Duration;
	}

	private void Awake()
	{
		state = GetComponentInParent<StateMachine>();
		UpdateIdleAndMoving();
		if (StartOnDefault && anim != null)
		{
			if (DefaultLoop == null && Idle == null)
			{
				return;
			}
			if (anim.AnimationState.Data.SkeletonData.FindAnimation((DefaultLoop != null) ? DefaultLoop.Animation.Name : Idle.Animation.Name) != null)
			{
				Track = anim.AnimationState.SetAnimation(AnimationTrack, (DefaultLoop != null) ? DefaultLoop : Idle, true);
			}
		}
		else
		{
			if(anim)
				Track = anim.AnimationState.GetCurrent(0);
		}
		Dir = 1;
		meshRenderer = GetComponent<MeshRenderer>();
		fillColor = Shader.PropertyToID("_FillColor");
		fillAlpha = Shader.PropertyToID("_FillAlpha");
	}

	private void OnEnable()
	{
		anim.AnimationState.Event += SpineEventHandler;
	}

	private void OnDisable()
	{
		anim.AnimationState.Event -= SpineEventHandler;
	}

	public void SetDefault(StateMachine.State State, string Animation)
	{
		foreach (SpineChartacterAnimationData animation in Animations)
		{
			if (animation.State == State)
			{
				animation.Animation = GetAnimationReference(Animation);
				animation.DefaultAnimation = GetAnimationReference(Animation);
			}
		}
	}

	public void UpdateIdleAndMoving()
	{
		foreach (SpineChartacterAnimationData animation in Animations)
		{
			animation.InitDefaults();
			if (animation.State == StateMachine.State.Moving)
			{
				Moving = animation.Animation;
			}
			if (animation.State == StateMachine.State.Idle)
			{
				Idle = animation.Animation;
			}
		}
	}

	private void OnDestroy()
	{
		if(anim)
			anim.AnimationState.Event -= SpineEventHandler;
	}

	private void SpineEventHandler(TrackEntry trackEntry, Spine.Event e)
	{
		if (this.OnSpineEvent != null)
		{
			this.OnSpineEvent(e.Data.Name);
		}
	}

	private void Update()
	{
		if (!(state != null))
		{
			return;
		}
		CurrentState = state.CURRENT_STATE;
		if (AutomaticallySetFacing)
		{
			Dir = ((state.facingAngle > 90f && state.facingAngle < 270f) ? 1 : (-1)) * ((!ReverseFacing) ? 1 : (-1));
		}
		if (NorthIdle != null && (CurrentState == StateMachine.State.InActive || CurrentState == StateMachine.State.Idle))
		{
			if (state.facingAngle > 40f && state.facingAngle < 140f)
			{
				if (Track != null && Track.Animation != NorthIdle.Animation)
				{
					Track = anim.AnimationState.SetAnimation(AnimationTrack, NorthIdle, true);
				}
			}
			else if (Track != null && Track.Animation != Idle.Animation)
			{
				Track = anim.AnimationState.SetAnimation(AnimationTrack, Idle, true);
			}
		}
		if (NorthDodging != null && CurrentState == StateMachine.State.Dodging)
		{
			if (state.facingAngle > 40f && state.facingAngle < 140f)
			{
				if (Track.Animation != NorthDodging.Animation)
				{
					Track = anim.AnimationState.SetAnimation(AnimationTrack, NorthDodging, true);
				}
			}
			else if (state.facingAngle > 220f && state.facingAngle < 320f)
			{
				if (Track.Animation != SouthDodging.Animation)
				{
					Track = anim.AnimationState.SetAnimation(AnimationTrack, SouthDodging, true);
				}
			}
			else if (Track.Animation != Dodging.Animation)
			{
				Track = anim.AnimationState.SetAnimation(AnimationTrack, Dodging, true);
			}
		}
		SpineChartacterAnimationData animationData = GetAnimationData(StateMachine.State.Moving);
		if (animationData != null && !(animationData.Animation == animationData.DefaultAnimation) && !ForceDirectionalMovement)
		{
			return;
		}
		if (NorthMoving != null && CurrentState == StateMachine.State.Moving)
		{
			if (state.facingAngle > 40f && state.facingAngle < 140f)
			{
				if (NorthDiagonalMoving != null)
				{
					if (state.facingAngle > 70f && state.facingAngle < 110f)
					{
						if (Track.Animation != NorthMoving.Animation)
						{
							Track = anim.AnimationState.SetAnimation(AnimationTrack, NorthMoving, true);
						}
					}
					else if (Track.Animation != NorthDiagonalMoving.Animation)
					{
						Track = anim.AnimationState.SetAnimation(AnimationTrack, NorthDiagonalMoving, true);
					}
				}
				else if (Track.Animation != NorthMoving.Animation)
				{
					Track = anim.AnimationState.SetAnimation(AnimationTrack, NorthMoving, true);
				}
			}
			else if ((SouthMoving == null || (SouthMoving != null && state.facingAngle < 220f && state.facingAngle > 320f)) && Track.Animation != Moving.Animation)
			{
				Track = anim.AnimationState.SetAnimation(AnimationTrack, Moving, true);
			}
		}
		if (!(SouthMoving != null) || CurrentState != StateMachine.State.Moving)
		{
			return;
		}
		if (state.facingAngle > 220f && state.facingAngle < 320f)
		{
			if (SouthDiagonalMoving != null)
			{
				if (state.facingAngle > 250f && state.facingAngle < 290f)
				{
					if (Track.Animation != SouthMoving.Animation)
					{
						Track = anim.AnimationState.SetAnimation(AnimationTrack, SouthMoving, true);
					}
				}
				else if (Track.Animation != SouthDiagonalMoving.Animation)
				{
					Track = anim.AnimationState.SetAnimation(AnimationTrack, SouthDiagonalMoving, true);
				}
			}
			else if (Track.Animation != SouthMoving.Animation)
			{
				Track = anim.AnimationState.SetAnimation(AnimationTrack, SouthMoving, true);
			}
		}
		else if ((NorthMoving == null || (NorthMoving != null && (state.facingAngle < 40f || state.facingAngle > 140f))) && Track.Animation != Moving.Animation)
		{
			Track = anim.AnimationState.SetAnimation(AnimationTrack, Moving, true);
		}
	}
}
