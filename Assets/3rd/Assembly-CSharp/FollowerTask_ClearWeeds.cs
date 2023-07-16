using System.Collections.Generic;
using Spine;
using UnityEngine;

public class FollowerTask_ClearWeeds : FollowerTask_AssistPlayerBase
{
	public const float REMOVAL_DURATION_GAME_MINUTES = 4f;

	public const float REMOVAL_DURATION_GAME_MINUTES_PLAYER = 2f;

	private int _weedID;

	private FollowerLocation _location;

	private float _removalProgress;

	private float _gameTimeSinceLastProgress;

	private Structures_Weeds _weed;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.ClearWeeds;
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
			if (_weed != null && _weed.Data.PrioritisedAsBuildingObstruction)
			{
				return 100f;
			}
			return 2f;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		if (_weed != null && _weed.Data.PrioritisedAsBuildingObstruction)
		{
			return PriorityCategory.OverrideWorkPriority;
		}
		return PriorityCategory.Low;
	}

	public FollowerTask_ClearWeeds(int weedID)
	{
		_helpingPlayer = false;
		_weedID = weedID;
		_weed = StructureManager.GetStructureByID<Structures_Weeds>(_weedID);
		_location = _weed.Data.Location;
	}

	public FollowerTask_ClearWeeds(Interaction_Weed weed)
	{
		_helpingPlayer = true;
		_location = weed.StructureInfo.Location;
	}

	public override void ClaimReservations()
	{
		base.ClaimReservations();
		_weed = StructureManager.GetStructureByID<Structures_Weeds>(_weedID);
		if (_weed != null)
		{
			_weed.ReservedForTask = true;
		}
	}

	public override void ReleaseReservations()
	{
		_weed = StructureManager.GetStructureByID<Structures_Weeds>(_weedID);
		if (_weed != null)
		{
			_weed.ReservedForTask = false;
		}
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
		if (LocationManager.GetLocationState(_location) == LocationState.Active)
		{
			Interaction_Weed interaction_Weed = FindWeed();
			if (interaction_Weed == null || interaction_Weed.Activating)
			{
				_weed = null;
				_weedID = -1;
				SetState(FollowerTaskState.Idle);
				Loop();
			}
		}
		else if (_weed == null)
		{
			SetState(FollowerTaskState.Idle);
			Loop();
		}
		if (base.State == FollowerTaskState.Doing)
		{
			_gameTimeSinceLastProgress += deltaGameTime;
			_weed.PickWeeds(1f);
			_removalProgress += _gameTimeSinceLastProgress * _brain.Info.ProductivityMultiplier;
			_gameTimeSinceLastProgress = 0f;
			if (_weed.PickedWeeds)
			{
				ProgressTask();
			}
		}
	}

	public override void ProgressTask()
	{
		_weed = StructureManager.GetStructureByID<Structures_Weeds>(_weedID);
		if (_weed == null)
		{
			End();
			return;
		}
		if (_brain.Location != PlayerFarming.Location && _weed.DropWeed)
		{
			List<Structures_CollectedResourceChest> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_CollectedResourceChest>(_brain.Location);
			if (allStructuresOfType.Count > 0)
			{
				allStructuresOfType[0].AddItem(InventoryItem.ITEM_TYPE.GRASS, 1);
			}
		}
		_removalProgress = 0f;
		_weed.Remove();
		_brain.GetXP(0.01f);
		Loop();
	}

	private void Loop(bool force = false)
	{
		if (force || !_helpingPlayer || !EndIfPlayerIsDistant())
		{
			Structures_Weeds nextWeed = GetNextWeed();
			if (nextWeed == null)
			{
				End();
				return;
			}
			ReleaseReservations();
			ClearDestination();
			_weedID = nextWeed.Data.ID;
			_weed = nextWeed;
			_location = nextWeed.Data.Location;
			nextWeed.ReservedForTask = true;
			SetState(FollowerTaskState.GoingTo);
		}
	}

	private Structures_Weeds GetNextWeed()
	{
		ReleaseReservations();
		Structures_Weeds result = null;
		float num = float.MaxValue;
		float num2 = (_helpingPlayer ? AssistRange : float.MaxValue);
		PlayerFarming instance = PlayerFarming.Instance;
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		foreach (Structures_Weeds allAvailableWeed in StructureManager.GetAllAvailableWeeds(Location))
		{
			if (follower == null)
			{
				result = allAvailableWeed;
				break;
			}
			if (!allAvailableWeed.Data.PrioritisedAsBuildingObstruction)
			{
				continue;
			}
			float num3 = Vector3.Distance(_helpingPlayer ? instance.transform.position : follower.transform.position, allAvailableWeed.Data.Position);
			if (num3 < num2)
			{
				float num4 = num3 + (allAvailableWeed.Data.Prioritised ? 0f : 1000f);
				if (num4 < num)
				{
					result = allAvailableWeed;
					num = num4;
				}
			}
		}
		return result;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.Spine.AnimationState.Event += HandleAnimationStateEvent;
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		Structures_Weeds structureByID = StructureManager.GetStructureByID<Structures_Weeds>(_weedID);
		if (structureByID != null)
		{
			return structureByID.Data.Position + new Vector3(-0.2f, 0f, 0f);
		}
		return Vector3.zero;
	}

	public override void OnDoingBegin(Follower follower)
	{
		if (_weedID == 0)
		{
			ProgressTask();
			return;
		}
		Interaction_Weed interaction_Weed = FindWeed();
		if (interaction_Weed != null)
		{
			follower.FacePosition(interaction_Weed.transform.position);
		}
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.SetBodyAnimation("action", true);
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		follower.Spine.AnimationState.Event -= HandleAnimationStateEvent;
	}

	private void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
	{
		string name = e.Data.Name;
		if (name == "Chop")
		{
			ProgressTask();
		}
	}

	private Interaction_Weed FindWeed()
	{
		Interaction_Weed result = null;
		foreach (Interaction_Weed weed in Interaction_Weed.Weeds)
		{
			if (weed.StructureInfo.ID == _weedID)
			{
				result = weed;
				break;
			}
		}
		return result;
	}
}
