using System;
using UnityEngine;

public class FollowerTask_Study : FollowerTask
{
	private static FollowerBrain[] assignedSeats = new FollowerBrain[4];

	private float progress;

	private int structureID;

	private Structures_Temple temple;

	private Follower follower;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Study;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Church;
		}
	}

	public override bool BlockReactTasks
	{
		get
		{
			return true;
		}
	}

	public override float Priorty
	{
		get
		{
			return 20f;
		}
	}

	public FollowerTask_Study(int structID)
	{
		structureID = structID;
		temple = StructureManager.GetStructureByID<Structures_Temple>(structureID);
	}

	public override void ClaimReservations()
	{
		temple.AddStudier(this);
	}

	public override void ReleaseReservations()
	{
		temple.RemoveStudier(this);
	}

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		switch (FollowerRole)
		{
		case FollowerRole.Worshipper:
		case FollowerRole.Worker:
		case FollowerRole.Lumberjack:
		case FollowerRole.Farmer:
			return PriorityCategory.Low;
		case FollowerRole.Monk:
			if (TimeManager.GetOverrideScheduledActivity() != 0)
			{
				return PriorityCategory.Medium;
			}
			return PriorityCategory.Low;
		default:
			return PriorityCategory.Low;
		}
	}

	protected override void OnStart()
	{
		Debug.Log("START! " + _brain.Info.CacheXP);
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void OnArrive()
	{
		SetState(FollowerTaskState.Doing);
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (base.State != FollowerTaskState.Doing)
		{
			return;
		}
		if (DataManager.Instance.TempleStudyXP < Structures_Temple.TempleMaxStudyXP)
		{
			progress += deltaGameTime * _brain.Info.ProductivityMultiplier;
			if ((bool)follower)
			{
				follower.FollowerRadialProgress.UpdateBar(DataManager.Instance.TempleStudyXP / Structures_Temple.TempleMaxStudyXP);
				if (follower.State.CURRENT_STATE != StateMachine.State.CustomAnimation)
				{
					follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
					follower.SetBodyAnimation("studying", true);
				}
			}
			if (progress >= 12f)
			{
				progress = 0f;
				DataManager.Instance.TempleStudyXP += 0.05f;
			}
		}
		else
		{
			Complete();
		}
	}

	private void OnNewPhaseStarted()
	{
		if (TimeManager.CurrentPhase == DayPhase.Night)
		{
			End();
		}
	}

	private void AssignSeat(Follower follower)
	{
		assignedSeats[ChurchFollowerManager.Instance.GetClosestSlotIndex(follower.Brain.LastPosition)] = follower.Brain;
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		if ((bool)ChurchFollowerManager.Instance)
		{
			for (int i = 0; i < Structures_Temple.AvailableStudySlots; i++)
			{
				if (i < Structures_Temple.AvailableStudySlots && (assignedSeats[i] == null || assignedSeats[i].Info.ID == follower.Brain.Info.ID || !(assignedSeats[i].CurrentTask is FollowerTask_Study)))
				{
					assignedSeats[i] = follower.Brain;
					return ChurchFollowerManager.Instance.GetSlotPosition(i);
				}
			}
			Debug.Log("Whoops! No appropriate place to go");
			Complete();
			return new Vector3(0f, -12f, 0f);
		}
		return new Vector3(0f, -12f, 0f);
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
	}

	public override void OnDoingBegin(Follower follower)
	{
		this.follower = follower;
		follower.FollowerRadialProgress.Show();
		AssignSeat(follower);
	}

	public override void OnFinaliseBegin(Follower follower)
	{
		follower.SetBodyAnimation("idle", true);
		follower.FollowerRadialProgress.Hide();
		this.follower = null;
		base.OnFinaliseBegin(follower);
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		follower.FollowerRadialProgress.Hide();
		follower.State.CURRENT_STATE = StateMachine.State.Idle;
		follower.SetBodyAnimation("idle", true);
		this.follower = null;
	}
}
