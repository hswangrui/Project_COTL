using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using I2.Loc;
using Spine;
using Spine.Unity;
using UnityEngine;

public class FirstMiniBossIntro : BossIntro
{
	public SkeletonAnimation LeshySpine;

	public SkeletonAnimation SpawnEffectSpine;

	public GameObject SpawnEffect;

	[SerializeField]
	private Health enemy;

	[SerializeField]
	private Transform playerPosition;

	[SpineAnimation("", "", true, false, dataField = "BossSpine")]
	public string IntroAnimation;

	[SpineAnimation("", "", true, false, dataField = "BossSpine")]
	public string IdleAnimation;

	public List<SkeletonAnimation> BossSpines;

	[SerializeField]
	private Material leaderOldMaterial;

	[SerializeField]
	private Material leaderNewMaterial;

	[SerializeField]
	private GameObject goopFloorParticle;

	[SerializeField]
	private LightingManagerVolume lightOverride;

	[TermsPopup("")]
	public string DisplayName;

	[EventRef]
	public string RoarSfx;

	public GameObject Leshy;

	public Interaction_SimpleConversation Conversation;

	private bool WaitingForAnimationToComplete = true;

	private bool WaitingForConversationToComplete = true;

	public GameObject Follower;

	public SkeletonAnimation FollowerSpine;

	private SkeletonAnimation goopSkeletonAnimation;

	private bool triggered;

	private EventInstance LoopedSound;

	private EventInstance roarInstance;

	private void OnDisable()
	{
		AudioManager.Instance.StopLoop(LoopedSound);
	}

	private void OnEnable()
	{
		if (DataManager.Instance.CheckKilledBosses(GetComponentInParent<MiniBossController>().name) || DungeonSandboxManager.Active)
		{
			Follower.gameObject.SetActive(false);
			foreach (SkeletonAnimation bossSpine in BossSpines)
			{
				bossSpine.gameObject.SetActive(true);
				bossSpine.AnimationState.SetAnimation(0, "animation", true);
			}
			BossSpine.AnimationState.SetAnimation(0, IdleAnimation, true);
		}
		goopSkeletonAnimation = goopFloorParticle.GetComponent<SkeletonAnimation>();
	}

	public override IEnumerator PlayRoutine(bool skipped = false)
	{
		if (!DataManager.Instance.CheckKilledBosses(GetComponentInParent<MiniBossController>().name) && !DungeonSandboxManager.Active)
		{
			while (Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) > 2f)
			{
				if (PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.CustomAnimation)
				{
					PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
				}
				base.transform.position = Vector3.zero;
				yield return null;
			}
			AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.CultLeaderAmbience);
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(Conversation.gameObject);
			PlayerFarming.Instance.GoToAndStop(playerPosition.position, base.gameObject);
			LeshySpine.CustomMaterialOverride.Add(leaderOldMaterial, leaderNewMaterial);
			yield return new WaitForSeconds(0.5f);
			goopFloorParticle.gameObject.SetActive(true);
			goopSkeletonAnimation.AnimationState.AddAnimation(0, "leader-loop", true, 0f);
			AudioManager.Instance.PlayOneShot("event:/enemy/teleport_appear", goopFloorParticle.transform.position);
			lightOverride.gameObject.SetActive(true);
			yield return new WaitForSeconds(1.5f);
			LeshySpine.transform.parent.gameObject.SetActive(true);
			yield return new WaitForEndOfFrame();
			LeshySpine.AnimationState.AddAnimation(0, "idle", true, 0f);
			yield return new WaitForSeconds(2f);
			foreach (SkeletonAnimation bossSpine in BossSpines)
			{
				bossSpine.gameObject.SetActive(false);
			}
			foreach (SkeletonAnimation bossSpine2 in BossSpines)
			{
				bossSpine2.AnimationState.SetAnimation(0, "hidden", false);
			}
			Leshy.SetActive(true);
			Conversation.Play();
			while (WaitingForConversationToComplete)
			{
				yield return null;
			}
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(Follower, 6f);
			float Progress = 0f;
			float Duration = 5f;
			float StartingZoom = GameManager.GetInstance().CamFollowTarget.distance;
			FollowerSpine.AnimationState.SetAnimation(0, "mutate", false);
			SpawnEffect.SetActive(true);
			SpawnEffectSpine.AnimationState.SetAnimation(0, "boss-transform", false);
			float f = 0f;
			DOTween.To(() => f, delegate(float x)
			{
				f = x;
			}, 1f, 1.5f).OnComplete(delegate
			{
				AudioManager.Instance.PlayOneShot("event:/enemy/summoned");
			});
			while (true)
			{
				float num;
				Progress = (num = Progress + Time.deltaTime);
				if (!(num < Duration - 0.5f))
				{
					break;
				}
				GameManager.GetInstance().CameraSetZoom(Mathf.Lerp(StartingZoom, 4f, Progress / Duration));
				yield return null;
			}
			CameraManager.instance.ShakeCameraForDuration(0.8f, 1f, 0.3f);
			foreach (SkeletonAnimation bossSpine3 in BossSpines)
			{
				bossSpine3.AnimationState.SetAnimation(0, "transform", false);
				bossSpine3.AnimationState.AddAnimation(0, "animation", true, 0f);
			}
			Vector3 position = Follower.transform.position;
			AudioManager.Instance.PlayOneShot("event:/enemy/vocals/humanoid/death", position);
			AudioManager.Instance.PlayOneShot("event:/enemy/impact_squishy", position);
			AudioManager.Instance.PlayOneShot("event:/enemy/summon", position);
			AudioManager.Instance.PlayOneShot(" event:/enemy/vocals/worm_large/warning", position);
			BiomeConstants.Instance.EmitSmokeExplosionVFX(position);
			RumbleManager.Instance.Rumble();
			foreach (SkeletonAnimation bossSpine4 in BossSpines)
			{
				bossSpine4.gameObject.SetActive(true);
			}
			Follower.SetActive(false);
			SpawnEffect.SetActive(false);
			LeshySpine.AnimationState.SetAnimation(0, "exit", false);
			AudioManager.Instance.StopLoop(LoopedSound);
			lightOverride.gameObject.SetActive(false);
			yield return new WaitForSeconds(1.5f);
			goopSkeletonAnimation.AnimationState.SetAnimation(0, "leader-stop", false);
			AudioManager.Instance.PlayOneShot("event:/enemy/teleport_away", goopFloorParticle.transform.position);
		}
		else
		{
			Follower.SetActive(false);
			Leshy.SetActive(false);
			BossSpine.AnimationState.SetAnimation(0, IntroAnimation, false);
		}
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(CameraTarget, 5f);
		HUD_DisplayName.Play(DisplayName, 2, HUD_DisplayName.Positions.Centre);
		yield return new WaitForSeconds(0.5f);
		CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
		BossSpine.AnimationState.SetAnimation(0, IntroAnimation, false);
		BossSpine.AnimationState.Complete += AnimationState_Complete;
		BossSpine.AnimationState.AddAnimation(0, IdleAnimation, true, 0f);
		roarInstance = AudioManager.Instance.CreateLoop(RoarSfx, BossSpine.gameObject);
		roarInstance.setParameterByName("roar_layers", 1f);
		AudioManager.Instance.PlayLoop(roarInstance);
		while (WaitingForAnimationToComplete)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationEnd();
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.MainBossA);
	}

	public void ConversationComplete()
	{
		WaitingForConversationToComplete = false;
	}

	private void AnimationState_Complete(TrackEntry trackEntry)
	{
		WaitingForAnimationToComplete = false;
	}
}
