using System;
using Spine;
using UnityEngine;

public class FollowerTask_ResourceStation : FollowerTask
{
	public const float MAX_TREE_DISTANCE = 30f;

	private int _resourceStationID;

	private Structures_LumberjackStation _resourceStation;

	private bool CarryingResource;

	public override FollowerTaskType Type
	{
		get
		{
			if (_resourceStation.Data.LootToDrop != InventoryItem.ITEM_TYPE.LOG)
			{
				return FollowerTaskType.ClearRubble;
			}
			return FollowerTaskType.ChopTrees;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _resourceStation.Data.Location;
		}
	}

	public override int UsingStructureID
	{
		get
		{
			return _resourceStationID;
		}
	}

	public override bool BlockSocial
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

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		switch (_resourceStation.Data.LootToDrop)
		{
		case InventoryItem.ITEM_TYPE.LOG:
			if (FollowerRole == FollowerRole.Lumberjack)
			{
				return PriorityCategory.WorkPriority;
			}
			break;
		case InventoryItem.ITEM_TYPE.STONE:
			if (FollowerRole == FollowerRole.StoneMiner)
			{
				return PriorityCategory.WorkPriority;
			}
			break;
		}
		return PriorityCategory.Low;
	}

	public FollowerTask_ResourceStation(int resourceStationID)
	{
		_resourceStationID = resourceStationID;
		_resourceStation = StructureManager.GetStructureByID<Structures_LumberjackStation>(_resourceStationID);
	}

	protected override int GetSubTaskCode()
	{
		return _resourceStationID;
	}

	public override void ClaimReservations()
	{
		Structures_LumberjackStation structureByID = StructureManager.GetStructureByID<Structures_LumberjackStation>(_resourceStationID);
		if (structureByID != null)
		{
			structureByID.ReservedForTask = true;
		}
	}

	public override void ReleaseReservations()
	{
		Structures_LumberjackStation structureByID = StructureManager.GetStructureByID<Structures_LumberjackStation>(_resourceStationID);
		if (structureByID != null)
		{
			structureByID.ReservedForTask = false;
		}
	}

	protected override void OnStart()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void OnArrive()
	{
		if (CarryingResource)
		{
			Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
			if (follower != null)
			{
				string animation = "Buildings/add-wood";
				switch (_resourceStation.Data.LootToDrop)
				{
				case InventoryItem.ITEM_TYPE.LOG:
					animation = "Buildings/add-wood";
					break;
				case InventoryItem.ITEM_TYPE.STONE:
					animation = "Buildings/add-stone";
					break;
				}
				follower.TimedAnimation(animation, 11f / 12f, delegate
				{
					follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run");
					CarryingResource = false;
					DepositResource();
					RecalculateDestination();
					SetState(FollowerTaskState.GoingTo);
				});
			}
			else
			{
				CarryingResource = false;
				DepositResource();
				RecalculateDestination();
				SetState(FollowerTaskState.GoingTo);
			}
		}
		else
		{
			SetState(FollowerTaskState.Idle);
		}
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (_resourceStation.Data.Exhausted && !CarryingResource)
		{
			End();
		}
		if (base.State != FollowerTaskState.Idle && base.State != FollowerTaskState.Doing)
		{
			return;
		}
		_resourceStation.Data.Progress += deltaGameTime * _brain.Info.ProductivityMultiplier;
		if (!(_resourceStation.Data.Progress >= (float)_resourceStation.DURATION_GAME_MINUTES))
		{
			return;
		}
		_resourceStation.Data.Progress = 0f;
		if (_resourceStation.Data.Inventory.Count >= _resourceStation.ResourceMax)
		{
			return;
		}
		CarryingResource = true;
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if (follower != null)
		{
			switch (_resourceStation.Data.LootToDrop)
			{
			case InventoryItem.ITEM_TYPE.LOG:
				follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Buildings/run-wood");
				break;
			case InventoryItem.ITEM_TYPE.STONE:
				follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Buildings/run-stone");
				break;
			}
		}
		RecalculateDestination();
		SetState(FollowerTaskState.GoingTo);
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		follower.Spine.AnimationState.Event -= HandleAnimationStateEvent;
		follower.SimpleAnimator.ResetAnimationsToDefaults();
		follower.SetHat(HatType.None);
	}

	private void DepositResource()
	{
		_resourceStation.Data.Inventory.Add(new InventoryItem(_resourceStation.Data.LootToDrop));
		LumberjackStation lumberjackStation = FindResourceStation();
		if (lumberjackStation != null)
		{
			lumberjackStation.DepositItem();
		}
		_resourceStation.IncreaseAge();
		if (_resourceStation.Data.Inventory.Count >= _resourceStation.ResourceMax)
		{
			End();
		}
	}

	private void OnNewPhaseStarted()
	{
		End();
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		LumberjackStation lumberjackStation = FindResourceStation();
		if (!(lumberjackStation == null))
		{
			if (!CarryingResource)
			{
				return lumberjackStation.FollowerPosition.transform.position;
			}
			return lumberjackStation.ChestPosition.transform.position;
		}
		return _resourceStation.Data.Position;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.Spine.AnimationState.Event += HandleAnimationStateEvent;
		switch (_resourceStation.Data.LootToDrop)
		{
		case InventoryItem.ITEM_TYPE.LOG:
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "chop-wood");
			break;
		case InventoryItem.ITEM_TYPE.STONE:
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "mining");
			break;
		}
	}

	public override void OnDoingBegin(Follower follower)
	{
		base.OnDoingBegin(follower);
		switch (_resourceStation.Data.LootToDrop)
		{
		case InventoryItem.ITEM_TYPE.LOG:
			follower.SetHat(HatType.Lumberjack);
			break;
		case InventoryItem.ITEM_TYPE.STONE:
			follower.SetHat(HatType.Miner);
			break;
		}
	}

	public override void SimDoingBegin(SimFollower simFollower)
	{
		SetState(FollowerTaskState.Idle);
	}

	private LumberjackStation FindResourceStation()
	{
		LumberjackStation result = null;
		foreach (LumberjackStation lumberjackStation in LumberjackStation.LumberjackStations)
		{
			if (lumberjackStation != null && lumberjackStation.StructureInfo != null && lumberjackStation.StructureInfo.ID == _resourceStationID)
			{
				result = lumberjackStation;
				break;
			}
		}
		return result;
	}

	private void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
	{
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if (!(follower == null) && e.Data.Name == "Chop")
		{
			switch (_resourceStation.Data.LootToDrop)
			{
			case InventoryItem.ITEM_TYPE.LOG:
				AudioManager.Instance.PlayOneShot("event:/material/tree_chop", follower.transform.position);
				break;
			case InventoryItem.ITEM_TYPE.STONE:
				AudioManager.Instance.PlayOneShot("event:/material/stone_impact", follower.transform.position);
				break;
			}
		}
	}

	public override void OnIdleBegin(Follower follower)
	{
		base.OnIdleBegin(follower);
		follower.SetHat((_resourceStation.Data.LootToDrop == InventoryItem.ITEM_TYPE.LOG) ? HatType.Lumberjack : HatType.Miner);
	}
}
