using System.Collections;
using I2.Loc;
using Spine;
using Spine.Unity;
using UnityEngine;

public class Interaction_RelicFish : Interaction
{
	[SerializeField]
	private SkeletonAnimation fishSpine;

	private bool waitingForEventListener;

	public override void GetLabel()
	{
		base.GetLabel();
		base.Label = LocalizationManager.GetTranslation("Interactions/Tickle");
	}

	private void Awake()
	{
		CheckSkin();
	}

	private void CheckSkin()
	{
		if (DataManager.Instance.FoundRelicInFish)
		{
			fishSpine.skeleton.SetSkin("happyEye");
		}
		else
		{
			fishSpine.skeleton.SetSkin("defaultEye");
		}
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Interactable = false;
		StartCoroutine(InteractionIE());
	}

	private IEnumerator InteractionIE()
	{
		fishSpine.AnimationState.Event += HandleAnimationStateEvent;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "pet-dog", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		yield return new WaitForSeconds(1.5f);
		CameraManager.instance.ShakeCameraForDuration(0.75f, 1f, 2f);
		GameManager.GetInstance().CameraSetZoom(6f);
		AudioManager.Instance.PlayOneShot("event:/doctrine_stone/doctrine_shake", fishSpine.gameObject);
		fishSpine.AnimationState.SetAnimation(0, "toothpulled", false);
		fishSpine.AnimationState.AddAnimation(0, "animation", true, 0f);
		while (!waitingForEventListener)
		{
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/player/harvest_meat_done", base.gameObject);
		MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
		CameraManager.shakeCamera(0.5f, Random.Range(0, 360));
		DataManager.Instance.FoundRelicInFish = true;
		CheckSkin();
		bool waiting = true;
		GameObject gameObject = RelicCustomTarget.Create(base.transform.position, PlayerFarming.Instance.transform.position, 1f, RelicType.RerollWeapon, delegate
		{
			waiting = false;
		});
		AudioManager.Instance.PlayOneShot("event:/relics/heart_convert_blessed", gameObject);
		GameManager.GetInstance().OnConversationNext(gameObject);
		while (waiting)
		{
			yield return null;
		}
		fishSpine.AnimationState.Event -= HandleAnimationStateEvent;
		GameManager.GetInstance().OnConversationEnd();
		base.gameObject.SetActive(false);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.FindKudaiiRelic);
	}

	private void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.ToString() == "relic")
		{
			waitingForEventListener = true;
		}
		else
		{
			AudioManager.Instance.PlayOneShot("event:/fishing/splash", fishSpine.gameObject);
		}
	}
}
