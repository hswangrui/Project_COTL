using System;
using System.Collections;
using DG.Tweening;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using Spine;
using src.Extensions;
using UnityEngine;

public class RitualConsumeFollower : Ritual
{
	private Follower sacrificeFollower;

	private int NumGivingDevotion;

	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_ConsumeFollower;
		}
	}

	public override void Play()
	{
		base.Play();
		GameManager.GetInstance().StartCoroutine(HeartsOfTheFaithfulRitual());
	}

	private IEnumerator HeartsOfTheFaithfulRitual()
	{
		Interaction_TempleAltar.Instance.SimpleSetCamera.Play();
		PlayerFarming.Instance.GoToAndStop(ChurchFollowerManager.Instance.RitualCenterPosition.position + Vector3.left * 0.75f, ChurchFollowerManager.Instance.RitualCenterPosition.gameObject, false, false, delegate
		{
			Interaction_TempleAltar.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
			PlayerFarming.Instance.state.transform.DOMove(ChurchFollowerManager.Instance.RitualCenterPosition.position + Vector3.left * 0.75f, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true);
			AudioManager.Instance.PlayOneShot("event:/sermon/sermon_speech_bubble", PlayerFarming.Instance.transform.position);
		});
		yield return StartCoroutine(WaitFollowersFormCircle());
		yield return new WaitForSeconds(1f);
		UIFollowerSelectMenuController followerSelectInstance = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
		followerSelectInstance.Show(Ritual.GetFollowersAvailableToAttendSermon(), null, false, RitualType);
		UIFollowerSelectMenuController uIFollowerSelectMenuController = followerSelectInstance;
		uIFollowerSelectMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIFollowerSelectMenuController.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
		{
			AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/select_follower", PlayerFarming.Instance.gameObject);
			sacrificeFollower = FollowerManager.FindFollowerByID(followerInfo.ID);
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController2 = followerSelectInstance;
		uIFollowerSelectMenuController2.OnCancel = (Action)Delegate.Combine(uIFollowerSelectMenuController2.OnCancel, (Action)delegate
		{
			EndRitual();
			CompleteRitual(true);
			CancelFollowers();
			Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController3 = followerSelectInstance;
		uIFollowerSelectMenuController3.OnHidden = (Action)Delegate.Combine(uIFollowerSelectMenuController3.OnHidden, (Action)delegate
		{
			followerSelectInstance = null;
		});
		while (followerSelectInstance != null)
		{
			yield return null;
		}
		sacrificeFollower.Brain.CompleteCurrentTask();
		FollowerTask_ManualControl Task = new FollowerTask_ManualControl();
		sacrificeFollower.Brain.HardSwapToTask(Task);
		sacrificeFollower.Brain.InRitual = true;
		Ritual.FollowerToAttendSermon.Remove(sacrificeFollower.Brain);
		yield return null;
		bool WaitForFollower = true;
		sacrificeFollower.HoodOff("idle", false, delegate
		{
			ChurchFollowerManager.Instance.RemoveBrainFromAudience(sacrificeFollower.Brain);
			foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
			{
				if (allBrain.CurrentTaskType == FollowerTaskType.AttendTeaching)
				{
					allBrain.CurrentTask.RecalculateDestination();
					allBrain.CurrentTask.Setup(FollowerManager.FindFollowerByID(allBrain.Info.ID));
				}
			}
			Task.GoToAndStop(sacrificeFollower, PlayerFarming.Instance.transform.position + Vector3.right * 1.75f, delegate
			{
				WaitForFollower = false;
			});
		});
		Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		GameManager.GetInstance().OnConversationNext(sacrificeFollower.gameObject, 8f);
		GameManager.GetInstance().AddPlayerToCamera();
		while (WaitForFollower)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationNext(sacrificeFollower.gameObject, 12f);
		GameManager.GetInstance().AddPlayerToCamera();
		PlayerFarming.Instance.Spine.AnimationState.Event += HandleEvent;
		PlayerFarming.Instance.simpleSpineAnimator.Animate("sacrifice", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		sacrificeFollower.SetBodyAnimation("sacrifice", false);
		sacrificeFollower.State.LookAngle = (sacrificeFollower.State.facingAngle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position));
		ChurchFollowerManager.Instance.StartRitualOverlay();
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.targetDistance, delegate(float x)
		{
			GameManager.GetInstance().CamFollowTarget.targetDistance = x;
		}, 4f, 1f).SetEase(Ease.InOutSine).SetUpdate(true);
		yield return new WaitForSeconds(3.2333333f);
		Interaction_TempleAltar.Instance.PulseDisplacementObject(sacrificeFollower.transform.position);
		CameraManager.instance.ShakeCameraForDuration(1.5f, 2f, 0.75f);
		foreach (FollowerBrain item in Ritual.FollowerToAttendSermon)
		{
			if (item.CurrentTask is FollowerTask_AttendRitual)
			{
				(item.CurrentTask as FollowerTask_AttendRitual).Cheer();
			}
		}
		Vector3 position = sacrificeFollower.transform.position;
		for (int i = 0; i < UnityEngine.Random.Range(3, 4); i++)
		{
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.FOLLOWER_MEAT, 1, position);
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BONE, 1, position);
			yield return new WaitForSeconds(0.2f);
		}
		PlayerFarming.Instance.Spine.AnimationState.Event -= HandleEvent;
		int followerID = sacrificeFollower.Brain.Info.ID;
		FollowerManager.FollowerDie(sacrificeFollower.Brain.Info.ID, NotificationCentre.NotificationType.ConsumeFollower);
		for (int j = 0; j < 20; j++)
		{
			SoulCustomTarget.Create(PlayerFarming.Instance.gameObject, sacrificeFollower.gameObject.transform.position, Color.white, null);
		}
		UnityEngine.Object.Destroy(sacrificeFollower.gameObject);
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().CamFollowTarget.targetDistance += 2f;
		EndRitual();
		HUD_Manager.Instance.XPBarTransitions.gameObject.SetActive(true);
		HUD_Manager.Instance.XPBarTransitions.MoveBackInFunction();
		yield return new WaitForSeconds(1f);
		int num = Mathf.CeilToInt(DataManager.GetTargetXP(Mathf.Min(DataManager.Instance.Level, DataManager.TargetXP.Count - 1)) - DataManager.Instance.XP);
		PlayerFarming.Instance.GetXP(num);
		yield return new WaitForSeconds(1.5f);
		ChurchFollowerManager.Instance.EndRitualOverlay();
		HUD_Manager.Instance.XPBarTransitions.MoveBackOutFunction();
		yield return new WaitForSeconds(0.5f);
		CompleteRitual(false, followerID);
		yield return new WaitForSeconds(1f);
		CultFaithManager.AddThought(Thought.Cult_ConsumeFollower, followerID, 1f);
	}

	private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.Data.Name == "sfxTrigger")
		{
			AudioManager.Instance.PlayOneShot("event:/rituals/consume_follower", PlayerFarming.Instance.transform.position);
		}
	}

	private void EndRitual()
	{
		PlayerFarming.Instance.Spine.AnimationState.Event -= HandleEvent;
		foreach (FollowerBrain item in Ritual.FollowerToAttendSermon)
		{
			item.CompleteCurrentTask();
		}
	}
}
