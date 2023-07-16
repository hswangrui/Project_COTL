using System;
using UnityEngine;

public class FollowerTask_Fisherman : FollowerTask
{
	public const int FISH_DURATION_GAME_MINUTES = 40;

	private int _fishingHutID;

	private Structures_FishingHut _fishingHut;

	private float _progress;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Fisherman;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _fishingHut.Data.Location;
		}
	}

	public override int UsingStructureID
	{
		get
		{
			return _fishingHutID;
		}
	}

	public override float Priorty
	{
		get
		{
			return 20f;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		switch (FollowerRole)
		{
		case FollowerRole.Worker:
			return PriorityCategory.WorkPriority;
		case FollowerRole.Worshipper:
		case FollowerRole.Lumberjack:
		case FollowerRole.Farmer:
		case FollowerRole.Monk:
			return PriorityCategory.Low;
		default:
			return PriorityCategory.Low;
		}
	}

	public FollowerTask_Fisherman(int fishingHutID)
	{
		_fishingHutID = fishingHutID;
		_fishingHut = StructureManager.GetStructureByID<Structures_FishingHut>(_fishingHutID);
	}

	protected override int GetSubTaskCode()
	{
		return _fishingHutID;
	}

	public override void ClaimReservations()
	{
		StructureManager.GetStructureByID<Structures_FishingHut>(_fishingHutID).ReservedForTask = true;
	}

	public override void ReleaseReservations()
	{
		StructureManager.GetStructureByID<Structures_FishingHut>(_fishingHutID).ReservedForTask = false;
	}

	protected override void OnStart()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void OnArrive()
	{
		SetState(FollowerTaskState.Idle);
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (base.State != FollowerTaskState.Idle && base.State != FollowerTaskState.Doing)
		{
			return;
		}
		_progress += deltaGameTime * _brain.Info.ProductivityMultiplier;
		if (!(_progress >= 40f))
		{
			return;
		}
		_progress = 0f;
		Structures_FishingHut structureByID = StructureManager.GetStructureByID<Structures_FishingHut>(_fishingHutID);
		if (structureByID.Data.Inventory.Count < 5)
		{
			structureByID.Data.Inventory.Add(new InventoryItem(InventoryItem.ITEM_TYPE.FISH));
			if (_brain.ThoughtExists(Thought.FishingRitual))
			{
				structureByID.Data.Inventory.Add(new InventoryItem(InventoryItem.ITEM_TYPE.FISH));
			}
			Debug.Log(string.Format("{0} collected fish, cached now = {1}", _brain.Info.Name, structureByID.Data.Inventory.Count));
		}
		SetState(FollowerTaskState.Doing);
	}

	private void OnNewPhaseStarted()
	{
		End();
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		FishingHut fishingHut = FindFishingHut();
		if (!(fishingHut == null))
		{
			return fishingHut.FollowerPosition.transform.position;
		}
		return follower.transform.position;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "Fishing/fishing");
	}

	public override void OnIdleBegin(Follower follower)
	{
		base.OnIdleBegin(follower);
		follower.TimedAnimation("Fishing/fishing-start", 1.8333334f, delegate
		{
			follower.State.CURRENT_STATE = StateMachine.State.Idle;
		});
	}

	public override void OnDoingBegin(Follower follower)
	{
		string text = (new string[3] { "small", "medium", "big" })[UnityEngine.Random.Range(0, 3)];
		follower.TimedAnimation("Fishing/fishing-catch-" + text, 2.3333333f, delegate
		{
			follower.State.CURRENT_STATE = StateMachine.State.Idle;
			SetState(FollowerTaskState.Idle);
		});
	}

	private FishingHut FindFishingHut()
	{
		FishingHut result = null;
		foreach (FishingHut fishingHut in FishingHut.FishingHuts)
		{
			if (fishingHut.StructureInfo.ID == _fishingHutID)
			{
				result = fishingHut;
				break;
			}
		}
		return result;
	}

	public override void SimDoingBegin(SimFollower simFollower)
	{
		SetState(FollowerTaskState.Idle);
	}
}
