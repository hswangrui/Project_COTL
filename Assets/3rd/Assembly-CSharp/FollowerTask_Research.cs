using UnityEngine;

public class FollowerTask_Research : FollowerTask
{
	private int _researchStationID;

	private Structures_Research _researchStation;

	private int _slotIndex = -1;

	private float _gameTimeSinceLastProgress;

	public const float HighPriorityValue = 15f;

	public const float LowPriorityValue = 8f;

	private float OverridePriority;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Research;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _researchStation.Data.Location;
		}
	}

	public override int UsingStructureID
	{
		get
		{
			return _researchStationID;
		}
	}

	public override float Priorty
	{
		get
		{
			return OverridePriority;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		switch (FollowerRole)
		{
		case FollowerRole.Worker:
			return PriorityCategory.Medium;
		case FollowerRole.Worshipper:
		case FollowerRole.Monk:
			return PriorityCategory.Medium;
		case FollowerRole.Lumberjack:
			return PriorityCategory.Medium;
		case FollowerRole.Farmer:
			return PriorityCategory.Medium;
		default:
			return PriorityCategory.Low;
		}
	}

	public FollowerTask_Research(int researchStationID, float Priority)
	{
		OverridePriority = Priority;
		_researchStationID = researchStationID;
		_researchStation = StructureManager.GetStructureByID<Structures_Research>(_researchStationID);
	}

	protected override int GetSubTaskCode()
	{
		return _researchStationID * 100 + _slotIndex;
	}

	public override void ClaimReservations()
	{
		StructureManager.GetStructureByID<Structures_Research>(_researchStationID).TryClaimSlot(ref _slotIndex);
		if (_slotIndex >= 0)
		{
			Debug.Log(string.Format("{0} reserving Research {1}.{2}", _brain.Info.Name, _researchStationID, _slotIndex));
		}
	}

	public override void ReleaseReservations()
	{
		if (_slotIndex >= 0)
		{
			Debug.Log(string.Format("{0} releasing Research {1}.{2}", _brain.Info.Name, _researchStationID, _slotIndex));
			StructureManager.GetStructureByID<Structures_Research>(_researchStationID).ReleaseSlot(_slotIndex);
		}
	}

	protected override void OnStart()
	{
		if (_slotIndex >= 0)
		{
			SetState(FollowerTaskState.GoingTo);
		}
		else
		{
			End();
		}
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (base.State == FollowerTaskState.Doing)
		{
			_gameTimeSinceLastProgress += deltaGameTime;
			for (int i = 0; i < DataManager.Instance.CurrentResearch.Count; i++)
			{
				StructuresData.ResearchObject researchObject = DataManager.Instance.CurrentResearch[i];
				researchObject.Progress += _gameTimeSinceLastProgress;
				if (researchObject.Progress >= researchObject.TargetProgress)
				{
					StructuresData.CompleteResearch(researchObject.Type);
				}
			}
			_gameTimeSinceLastProgress = 0f;
		}
		if (!StructuresData.GetAnyResearchExists())
		{
			End();
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return StructureManager.GetStructureByID<Structures_Research>(_researchStationID).GetResearchPosition(_slotIndex);
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		if (base.State == FollowerTaskState.Doing)
		{
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "astrologer");
		}
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "astrologer");
	}
}
