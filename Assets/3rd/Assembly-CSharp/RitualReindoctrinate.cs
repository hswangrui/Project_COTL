using System;
using System.Collections;
using Lamb.UI.FollowerSelect;
using src.Extensions;
using UnityEngine;

public class RitualReindoctrinate : Ritual
{
	private Follower sacrificeFollower;

	public Light RitualLight;

	private UIFollowerSelectMenuController _followerSelectTemplate;

	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_Reindoctrinate;
		}
	}

	public override void Play()
	{
		base.Play();
		StartCoroutine(SacrificeFollowerRoutine());
	}

	private IEnumerator SacrificeFollowerRoutine()
	{
		Interaction_TempleAltar.Instance.SimpleSetCamera.Play();
		PlayerFarming.Instance.GoToAndStop(ChurchFollowerManager.Instance.RitualCenterPosition.position + Vector3.left * 0.5f, ChurchFollowerManager.Instance.RitualCenterPosition.gameObject, false, false, delegate
		{
			Interaction_TempleAltar.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
		});
		yield return StartCoroutine(WaitFollowersFormCircle());
		yield return new WaitForSeconds(1f);
		UIFollowerSelectMenuController followerSelectInstance = _followerSelectTemplate.Instantiate();
		followerSelectInstance.Show(Ritual.GetFollowersAvailableToAttendSermon(), null, false, RitualType);
		UIFollowerSelectMenuController uIFollowerSelectMenuController = followerSelectInstance;
		uIFollowerSelectMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIFollowerSelectMenuController.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
		{
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
			Task.GoToAndStop(sacrificeFollower, PlayerFarming.Instance.transform.position + Vector3.right, delegate
			{
				WaitForFollower = false;
				sacrificeFollower.State.LookAngle = (sacrificeFollower.State.facingAngle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position));
			});
		});
		Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		GameManager.GetInstance().OnConversationNext(sacrificeFollower.gameObject, 8f);
		GameManager.GetInstance().AddPlayerToCamera();
		while (WaitForFollower)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationNext(sacrificeFollower.gameObject, 10f);
		GameManager.GetInstance().AddPlayerToCamera();
		FollowerRecruit FollowerRecruit = sacrificeFollower.gameObject.AddComponent<FollowerRecruit>();
		FollowerRecruit.Follower = sacrificeFollower;
		FollowerRecruit.CameraBone = sacrificeFollower.CameraBone;
		FollowerRecruit.RecruitOnComplete = false;
		FollowerRecruit.triggered = true;
		FollowerRecruit.DoRecruit(PlayerFarming.Instance.state, true, false);
		while (FollowerRecruit != null)
		{
			yield return null;
		}
		sacrificeFollower.SetBodyAnimation("cheer", true);
		foreach (FollowerBrain item in Ritual.GetFollowersAvailableToAttendSermon())
		{
			if (item.CurrentTask is FollowerTask_AttendRitual)
			{
				(item.CurrentTask as FollowerTask_AttendRitual).Cheer();
			}
		}
		yield return new WaitForSeconds(2f);
		sacrificeFollower.Brain.CompleteCurrentTask();
		ChurchFollowerManager.Instance.RemoveBrainFromAudience(sacrificeFollower.Brain);
		EndRitual();
		yield return new WaitForSeconds(0.5f);
		CompleteRitual(false, sacrificeFollower.Brain.Info.ID);
	}

	private void EndRitual()
	{
		ChurchFollowerManager.Instance.EndRitualOverlay();
		foreach (FollowerBrain item in Ritual.GetFollowersAvailableToAttendSermon())
		{
			item.CompleteCurrentTask();
		}
	}
}
