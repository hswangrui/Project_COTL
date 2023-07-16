using System.Collections;
using FMOD.Studio;
using Spine;
using Spine.Unity;
using UnityEngine;

public class BasicBossIntro : BossIntro
{
	public enum EnemyType
	{
		NoLayer,
		Worm,
		Frog,
		ScreechBat,
		JellyFish,
		Spider
	}

	[SpineAnimation("", "", true, false, dataField = "BossSpine")]
	public string IntroAnimation;

	[SpineAnimation("", "", true, false, dataField = "BossSpine")]
	public string IdleAnimation;

	private string RoarSfx = "event:/enemy/patrol_boss/miniboss_intro_roar";

	public bool useRoarSpineEvent;

	private bool WaitingForAnimationToComplete = true;

	public EnemyType typeEnemy;

	private EventInstance roarInstance;

	public override IEnumerator PlayRoutine(bool skipped = false)
	{
		WaitingForAnimationToComplete = true;
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(CameraTarget, 5f);
		yield return new WaitForSeconds(0.5f);
		BossSpine.AnimationState.SetAnimation(0, IntroAnimation, false);
		BossSpine.AnimationState.Complete += AnimationState_Complete;
		BossSpine.AnimationState.AddAnimation(0, IdleAnimation, true, 0f);
		if (RoarSfx != null)
		{
			if (useRoarSpineEvent)
			{
				BossSpine.AnimationState.Event += HandleEvent;
			}
			else
			{
				PlaySound();
				CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
				MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
			}
		}
		while (WaitingForAnimationToComplete)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationEnd();
	}

	private void PlaySound()
	{
		AudioManager.Instance.PlayOneShotAndSetParameterValue(RoarSfx, "roar_layers", (float)typeEnemy, BossSpine.transform);
	}

	private void AnimationState_Complete(TrackEntry trackEntry)
	{
		WaitingForAnimationToComplete = false;
		if (useRoarSpineEvent)
		{
			BossSpine.AnimationState.Event -= HandleEvent;
		}
	}

	private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.Data.Name == "roar")
		{
			PlaySound();
			CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
			MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
		}
	}
}
