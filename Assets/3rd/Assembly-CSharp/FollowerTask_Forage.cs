using UnityEngine;

public class FollowerTask_Forage : FollowerTask_AssistPlayerBase
{
	private float _removalProgress;

	private float _gameTimeSinceLastProgress;

	private Structures_BerryBush _berries;

	private int _berryID;

	private FollowerLocation _location;

	private float WaitTimer;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Forage;
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
			return 3f;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		if (FollowerRole == FollowerRole.Forager || FollowerRole == FollowerRole.Berries)
		{
			return PriorityCategory.WorkPriority;
		}
		return PriorityCategory.Low;
	}

	public FollowerTask_Forage(int BerryID)
	{
		_helpingPlayer = false;
		_berryID = BerryID;
		_berries = StructureManager.GetStructureByID<Structures_BerryBush>(BerryID);
		_location = _berries.Data.Location;
	}

	public override void ClaimReservations()
	{
		base.ClaimReservations();
		_berries = StructureManager.GetStructureByID<Structures_BerryBush>(_berryID);
		if (_berries != null)
		{
			_berries.ReservedForTask = true;
		}
	}

	public override void ReleaseReservations()
	{
		base.ReleaseReservations();
		_berries = StructureManager.GetStructureByID<Structures_BerryBush>(_berryID);
		if (_berries != null)
		{
			_berries.ReservedForTask = false;
		}
	}

	public FollowerTask_Forage()
	{
		_helpingPlayer = true;
	}

	protected override void OnStart()
	{
		ReleaseReservations();
		Loop(true);
		ClearDestination();
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void AssistPlayerTick(float deltaGameTime)
	{
		if (base.State == FollowerTaskState.Wait)
		{
			if (PlayerFarming.Location == _brain.Location)
			{
				Loop();
			}
			else if ((WaitTimer += deltaGameTime) > 60f)
			{
				Loop();
			}
			return;
		}
		if (LocationManager.GetLocationState(_location) == LocationState.Active)
		{
			Interaction_Berries interaction_Berries = FindBush();
			if ((base.State == FollowerTaskState.GoingTo || base.State == FollowerTaskState.Doing) && (interaction_Berries == null || _berries.BerryPicked || _berries.IsCrop))
			{
				_berries = null;
				_berryID = -1;
				SetState(FollowerTaskState.Idle);
				Loop();
			}
			if (_berries != null && interaction_Berries != null && (bool)interaction_Berries.GetComponentInParent<CropController>())
			{
				_berries.IsCrop = true;
				End();
			}
		}
		else if (_berries == null)
		{
			SetState(FollowerTaskState.Idle);
			Loop();
		}
		if (base.State == FollowerTaskState.Doing)
		{
			_gameTimeSinceLastProgress += deltaGameTime;
			ProgressTask();
		}
	}

	public override void ProgressTask()
	{
		if (_berries == null || _berries.Data.Picked)
		{
			End();
			return;
		}
		_berries.PickBerries(_gameTimeSinceLastProgress * _brain.Info.ProductivityMultiplier * 0.5f);
		_gameTimeSinceLastProgress = 0f;
		if (!_berries.BerryPicked)
		{
			return;
		}
		if (_brain.Location != PlayerFarming.Location)
		{
			_berries.AddBerriesToChest(_brain.Location);
			if (!_berries.Data.CanRegrow)
			{
				_berries.Remove();
			}
		}
		_berries = null;
		_brain.GetXP(0.5f);
		if (_brain.Location != PlayerFarming.Location)
		{
			WaitTimer = 0f;
			SetState(FollowerTaskState.Wait);
		}
		else
		{
			Loop();
		}
	}

	private void Loop(bool force = false)
	{
		if (force || !_helpingPlayer || !EndIfPlayerIsDistant())
		{
			Structures_BerryBush nextBush = GetNextBush();
			if (nextBush == null)
			{
				End();
				return;
			}
			ReleaseReservations();
			_berryID = nextBush.Data.ID;
			_berries = nextBush;
			_location = nextBush.Data.Location;
			_berries.ReservedForTask = true;
			ClearDestination();
			SetState(FollowerTaskState.GoingTo);
		}
	}

	private Structures_BerryBush GetNextBush()
	{
		ReleaseReservations();
		Structures_BerryBush result = null;
		float num = float.MaxValue;
		float num2 = (_helpingPlayer ? AssistRange : float.MaxValue);
		PlayerFarming instance = PlayerFarming.Instance;
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		foreach (Structures_BerryBush allAvailableBush in StructureManager.GetAllAvailableBushes(Location))
		{
			if (follower == null)
			{
				result = allAvailableBush;
				break;
			}
			float num3 = Vector3.Distance(_helpingPlayer ? instance.transform.position : follower.transform.position, allAvailableBush.Data.Position);
			if (num3 < num2)
			{
				float num4 = num3 + (allAvailableBush.Data.Prioritised ? 0f : 1000f);
				if (num4 < num)
				{
					result = allAvailableBush;
					num = num4;
				}
			}
		}
		return result;
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		if (_berries == null)
		{
			return follower.transform.position;
		}
		return _berries.Data.Position + new Vector3((Random.value < 0.5f) ? (-0.4f) : 0.4f, -0.2f, 0f);
	}

	public override void OnDoingBegin(Follower follower)
	{
		if (_berryID != 0)
		{
			if (FindBush() != null)
			{
				follower.FacePosition(_berries.Data.Position);
			}
			follower.FacePosition(_berries.Data.Position);
			follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
			follower.SetBodyAnimation("action", true);
		}
	}

	private Interaction_Berries FindBush()
	{
		Interaction_Berries result = null;
		foreach (Interaction_Berries berry in Interaction_Berries.Berries)
		{
			if (berry.StructureInfo.ID == _berryID)
			{
				result = berry;
				break;
			}
		}
		return result;
	}
}
