using UnityEngine;

public class FollowerTask_CleanWaste : FollowerTask_AssistPlayerBase
{
	public const float REMOVAL_DURATION_GAME_MINUTES = 4f;

	public const float REMOVAL_DURATION_GAME_MINUTES_PLAYER = 2f;

	private int _wasteID;

	private FollowerLocation _location;

	private float _removalProgress;

	private float _gameTimeSinceLastProgress;

	private Structures_Waste waste;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.CleanWaste;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _location;
		}
	}

	public override float Priorty
	{
		get
		{
			if (!waste.Data.Prioritised)
			{
				return 3f;
			}
			return 6f;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		switch (FollowerRole)
		{
		case FollowerRole.Worker:
		case FollowerRole.Farmer:
			return PriorityCategory.Medium;
		case FollowerRole.Worshipper:
		case FollowerRole.Lumberjack:
		case FollowerRole.Monk:
			return PriorityCategory.Low;
		default:
			return PriorityCategory.Low;
		}
	}

	public FollowerTask_CleanWaste(int wasteID)
	{
		_wasteID = wasteID;
		waste = StructureManager.GetStructureByID<Structures_Waste>(_wasteID);
		_location = waste.Data.Location;
	}

	public override void ClaimReservations()
	{
		waste = StructureManager.GetStructureByID<Structures_Waste>(_wasteID);
		if (waste != null)
		{
			waste.ReservedForTask = true;
		}
	}

	public override void ReleaseReservations()
	{
		waste = StructureManager.GetStructureByID<Structures_Waste>(_wasteID);
		if (waste != null)
		{
			waste.ReservedForTask = false;
		}
	}

	protected override void OnStart()
	{
		if (_wasteID == 0)
		{
			Loop(true);
		}
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void AssistPlayerTick(float deltaGameTime)
	{
		if (base.State == FollowerTaskState.GoingTo)
		{
			if (LocationManager.GetLocationState(_location) == LocationState.Active)
			{
				if (FindWaste() == null)
				{
					SetState(FollowerTaskState.Idle);
					Loop();
				}
			}
			else
			{
				waste = StructureManager.GetStructureByID<Structures_Waste>(_wasteID);
				if (waste == null)
				{
					SetState(FollowerTaskState.Idle);
					Loop();
				}
			}
		}
		if (base.State == FollowerTaskState.Doing)
		{
			float num = 1f;
			_gameTimeSinceLastProgress += deltaGameTime * num;
		}
	}

	public override void ProgressTask()
	{
		waste = StructureManager.GetStructureByID<Structures_Waste>(_wasteID);
		if (waste == null)
		{
			End();
			return;
		}
		_removalProgress += _gameTimeSinceLastProgress;
		_gameTimeSinceLastProgress = 0f;
		if (_removalProgress >= 4f)
		{
			_removalProgress = 0f;
			waste.Remove();
			Loop();
		}
	}

	private void Loop(bool force = false)
	{
		if (force || !_helpingPlayer || !EndIfPlayerIsDistant())
		{
			Structures_Waste nextWaste = GetNextWaste();
			if (nextWaste == null)
			{
				End();
				return;
			}
			ClearDestination();
			_wasteID = nextWaste.Data.ID;
			nextWaste.ReservedForTask = true;
			SetState(FollowerTaskState.GoingTo);
		}
	}

	private Structures_Waste GetNextWaste()
	{
		Structures_Waste result = null;
		float num = float.MaxValue;
		float num2 = (_helpingPlayer ? AssistRange : float.MaxValue);
		PlayerFarming instance = PlayerFarming.Instance;
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		foreach (Structures_Waste item in StructureManager.GetAllAvailableWaste(Location))
		{
			if (follower == null)
			{
				result = item;
				break;
			}
			float num3 = Vector3.Distance(_helpingPlayer ? instance.transform.position : follower.transform.position, item.Data.Position);
			if (num3 < num2)
			{
				float num4 = num3 + (item.Data.Prioritised ? 0f : 1000f);
				if (num4 < num)
				{
					result = item;
					num = num4;
				}
			}
		}
		return result;
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		waste = StructureManager.GetStructureByID<Structures_Waste>(_wasteID);
		return waste.Data.Position + new Vector3(-0.2f, 0f, 0f);
	}

	public override void OnDoingBegin(Follower follower)
	{
		if (_wasteID == 0)
		{
			ProgressTask();
			return;
		}
		Waste waste = FindWaste();
		follower.FacePosition(waste.transform.position);
		follower.TimedAnimation("action", 3.5f, delegate
		{
			ProgressTask();
		});
	}

	private Waste FindWaste()
	{
		Waste result = null;
		foreach (Waste waste in Waste.Wastes)
		{
			if (waste.StructureInfo.ID == _wasteID)
			{
				result = waste;
				break;
			}
		}
		return result;
	}
}
