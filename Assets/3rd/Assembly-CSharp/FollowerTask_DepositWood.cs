using UnityEngine;

public class FollowerTask_DepositWood : FollowerTask
{
	public const float DEPOSIT_DURATION_GAME_MINUTES = 5f;

	private int _lumberjackStationID;

	private Structures_LumberjackStation _lumberjackStation;

	private float _progress;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.DepositWood;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _lumberjackStation.Data.Location;
		}
	}

	public override int UsingStructureID
	{
		get
		{
			return _lumberjackStationID;
		}
	}

	public FollowerTask_DepositWood(int lumberjackStationID)
	{
		_lumberjackStationID = lumberjackStationID;
		_lumberjackStation = StructureManager.GetStructureByID<Structures_LumberjackStation>(_lumberjackStationID);
	}

	protected override int GetSubTaskCode()
	{
		return _lumberjackStationID;
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void OnEnd()
	{
		Structures_LumberjackStation structureByID = StructureManager.GetStructureByID<Structures_LumberjackStation>(_lumberjackStationID);
		for (int i = 0; i < _brain.Stats.CachedLumber; i++)
		{
			structureByID.Data.Inventory.Add(new InventoryItem(InventoryItem.ITEM_TYPE.LOG));
		}
		_brain.Stats.CachedLumber = 0;
		_brain.Stats.CachedLumberjackStationID = 0;
		base.OnEnd();
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (base.State == FollowerTaskState.Doing && (_progress += deltaGameTime) >= 5f)
		{
			End();
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		LumberjackStation lumberjackStation = FindLumberjackStation();
		if (!(lumberjackStation == null))
		{
			return lumberjackStation.FollowerPosition.transform.position;
		}
		return follower.transform.position;
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.State.CURRENT_STATE = StateMachine.State.CustomAction0;
	}

	private LumberjackStation FindLumberjackStation()
	{
		LumberjackStation result = null;
		foreach (LumberjackStation lumberjackStation in LumberjackStation.LumberjackStations)
		{
			if (lumberjackStation.StructureInfo.ID == _lumberjackStationID)
			{
				result = lumberjackStation;
				break;
			}
		}
		return result;
	}
}
