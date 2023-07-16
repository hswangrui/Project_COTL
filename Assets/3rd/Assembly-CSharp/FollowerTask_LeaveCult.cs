using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FollowerTask_LeaveCult : FollowerTask
{
	private NotificationCentre.NotificationType _leaveNotificationType;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.LeaveCult;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Base;
		}
	}

	public override bool DisablePickUpInteraction
	{
		get
		{
			return true;
		}
	}

	public override bool BlockTaskChanges
	{
		get
		{
			return true;
		}
	}

	public FollowerTask_LeaveCult(NotificationCentre.NotificationType leaveNotificationType)
	{
		_leaveNotificationType = leaveNotificationType;
	}

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	protected override void OnStart()
	{
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.Interaction_FollowerInteraction.Interactable = false;
		follower.TimedAnimation("tantrum-big", 6f, delegate
		{
			_003C_003En__0(follower);
			SetState(FollowerTaskState.GoingTo);
		});
	}

	public override void OnGoingToBegin(Follower follower)
	{
		base.OnGoingToBegin(follower);
		follower.Interaction_FollowerInteraction.Interactable = false;
	}

	public override void SimSetup(SimFollower simFollower)
	{
		base.SimSetup(simFollower);
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void TaskTick(float deltaGameTime)
	{
	}

	protected override void OnArrive()
	{
		base.OnArrive();
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if (follower != null)
		{
			follower.TimedAnimation("wave-angry", 1.9333333f, delegate
			{
				follower.StartCoroutine(FrameDelay(delegate
				{
					follower.TimedAnimation("spawn-out-angry", 5f / 6f, delegate
					{
						int num = Mathf.Min(Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD), Mathf.FloorToInt(_brain.Stats.DissentGold));
						Inventory.ChangeItemQuantity(20, -num);
						End();
					});
				}));
			});
		}
		else
		{
			End();
		}
	}

	private IEnumerator FrameDelay(Action callback)
	{
		yield return new WaitForEndOfFrame();
		if (callback != null)
		{
			callback();
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		if (follower != null)
		{
			return BaseLocationManager.Instance.GetExitPosition(FollowerLocation.Lumberjack);
		}
		return Vector3.zero;
	}

	public override void OnFinaliseEnd(Follower follower)
	{
		follower.Brain.LeftCult = true;
		follower.Brain.LeftCultWithReason = _leaveNotificationType;
	}

	public override void SimFinaliseEnd(SimFollower simFollower)
	{
		simFollower.Brain.LeftCult = true;
		simFollower.Brain.LeftCultWithReason = _leaveNotificationType;
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0(Follower follower)
	{
		base.OnIdleBegin(follower);
	}
}
