using System;
using UnityEngine;

public class FollowerTask_SleepBedRest : FollowerTask
{
	public const int SLEEP_DURATION_GAME_MINUTES = 240;

	public const int HEAL_DURATION_GAME_MINUTES = 2400;

	public static Action<int> OnHomelessSleep;

	public static Action<int> OnWake;

	private bool sleepingOnFloor;

	private int overrideHealingBayID = -1;

	private float HealingDelay;

	private PlacementRegion.TileGridTile ClosestTile;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.SleepBedRest;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			if (_sleepingInBed)
			{
				return _brain.GetAssignedDwellingStructure().Data.Location;
			}
			return _brain.Location;
		}
	}

	public override bool BlockTaskChanges
	{
		get
		{
			return true;
		}
	}

	public override bool BlockReactTasks
	{
		get
		{
			return true;
		}
	}

	public override bool BlockSermon
	{
		get
		{
			return true;
		}
	}

	public override int UsingStructureID
	{
		get
		{
			int result = 0;
			if (_sleepingInBed)
			{
				result = _brain.GetAssignedDwellingStructure().Data.ID;
			}
			return result;
		}
	}

	private bool _sleepingInBed
	{
		get
		{
			if (_brain.HasHome && _brain.GetAssignedDwellingStructure() != null)
			{
				return !_brain.GetAssignedDwellingStructure().IsCollapsed;
			}
			return false;
		}
	}

	public FollowerTask_SleepBedRest(int healingBayID = -1)
	{
		overrideHealingBayID = healingBayID;
	}

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	public override void ClaimReservations()
	{
		if (ClosestTile != null)
		{
			ClosestTile.ReservedForWaste = true;
		}
	}

	public override void ReleaseReservations()
	{
		if (ClosestTile != null)
		{
			ClosestTile.ReservedForWaste = false;
		}
	}

	protected override void OnStart()
	{
		Debug.Log(_brain.Info.Name + " OnStart!");
		if (_sleepingInBed)
		{
			Structures_Bed assignedDwellingStructure = _brain.GetAssignedDwellingStructure();
			foreach (int multipleFollowerID in assignedDwellingStructure.Data.MultipleFollowerIDs)
			{
				FollowerInfo infoByID = FollowerInfo.GetInfoByID(multipleFollowerID);
				if (infoByID == null || infoByID.DwellingSlot != _brain._directInfoAccess.DwellingSlot)
				{
					continue;
				}
				for (int i = 0; i < assignedDwellingStructure.SlotCount; i++)
				{
					if (!assignedDwellingStructure.CheckIfSlotIsOccupied(i))
					{
						_brain._directInfoAccess.DwellingSlot = i;
						break;
					}
				}
				break;
			}
		}
		SetState(FollowerTaskState.GoingTo);
		ObjectiveManager.CheckObjectives(Objectives.TYPES.SEND_FOLLOWER_BED_REST);
	}

	protected override void OnArrive()
	{
		Debug.Log(_brain.Info.Name + " OnArrive!");
		if (!_brain.HasHome)
		{
			Action<int> onHomelessSleep = OnHomelessSleep;
			if (onHomelessSleep != null)
			{
				onHomelessSleep(_brain.Info.ID);
			}
		}
		base.OnArrive();
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		if (_brain.CurrentOverrideTaskType == FollowerTaskType.SleepBedRest)
		{
			_brain.ClearPersonalOverrideTaskProvider();
		}
	}

	protected override void OnComplete()
	{
		Debug.Log(_brain.Info.Name + " OnComplete!");
		Action<int> onWake = OnWake;
		if (onWake != null)
		{
			onWake(_brain.Info.ID);
		}
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (base.State == FollowerTaskState.Doing)
		{
			if (_brain.Stats.Illness > 0f && (HealingDelay += deltaGameTime) > 15f)
			{
				ThoughtData thought = _brain.GetThought(Thought.Ill);
				if (thought == null)
				{
					Debug.Log("ill thought missing");
					_brain.AddThought(Thought.Ill);
				}
				else if (TimeManager.TotalElapsedGameTime - thought.TimeStarted[0] < 1200f)
				{
					Debug.Log("update thought duration");
					thought.TimeStarted[0] = TimeManager.TotalElapsedGameTime;
				}
				else
				{
					Debug.Log("C  " + thought.Duration);
				}
				HealingDelay = 0f;
				if (_brain.Stats.Illness <= 0f && _brain.CurrentOverrideTaskType == FollowerTaskType.SleepBedRest)
				{
					_brain.ClearPersonalOverrideTaskProvider();
				}
			}
			if (_brain.Stats.Illness <= 0f && TimeManager.CurrentPhase != DayPhase.Night)
			{
				Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
				FollowerBrainStats.StatStateChangedEvent onIllnessStateChanged = FollowerBrainStats.OnIllnessStateChanged;
				if (onIllnessStateChanged != null)
				{
					onIllnessStateChanged(_brain.Info.ID, FollowerStatState.Off, FollowerStatState.On);
				}
				if (follower != null)
				{
					SetState(FollowerTaskState.Wait);
					Debug.Log(_brain.Info.Name + " Do reactions!");
					follower.TimedAnimation("Reactions/react-happy1", 2.1f, delegate
					{
						Debug.Log(_brain.Info.Name + " Second reactions!");
						follower.TimedAnimation("Reactions/react-enlightened2", 2.1f, delegate
						{
							if (_brain.CurrentOverrideTaskType == FollowerTaskType.SleepBedRest)
							{
								_brain.Stats.Illness = 0f;
								_brain.ClearPersonalOverrideTaskProvider();
							}
							End();
						});
					});
				}
				else
				{
					End();
				}
			}
		}
		if (sleepingOnFloor && _sleepingInBed && base.State == FollowerTaskState.Doing)
		{
			ClearDestination();
			SetState(FollowerTaskState.GoingTo);
		}
		if (!_brain.HasHome)
		{
			Dwelling.DwellingAndSlot freeDwellingAndSlot = StructureManager.GetFreeDwellingAndSlot(Location, _brain._directInfoAccess);
			if (freeDwellingAndSlot != null && !StructureManager.GetStructureByID<Structures_Bed>(freeDwellingAndSlot.ID).ReservedForTask)
			{
				StructureManager.GetStructureByID<Structures_Bed>(freeDwellingAndSlot.ID).ReservedForTask = true;
				_brain.HardSwapToTask(new FollowerTask_ClaimDwelling(freeDwellingAndSlot));
			}
		}
	}

	protected override float RestChange(float deltaGameTime)
	{
		if (base.State == FollowerTaskState.Doing)
		{
			float num = 1f;
			return 100f * num * (deltaGameTime / 240f);
		}
		return base.RestChange(deltaGameTime);
	}

	protected override float IllnessChange(float deltaGameTime)
	{
		if (base.State == FollowerTaskState.Doing)
		{
			float num = 1f;
			if (_brain.HasTrait(FollowerTrait.TraitType.Sickly))
			{
				num *= 0.85f;
			}
			if (_brain.HasTrait(FollowerTrait.TraitType.IronStomach))
			{
				num *= 1.15f;
			}
			return 0f - 100f * num * (deltaGameTime / 2400f);
		}
		return 100f * (deltaGameTime / 3600f);
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		Vector3 zero = Vector3.zero;
		if (_brain.HasHome)
		{
			Structures_Bed assignedDwellingStructure = _brain.GetAssignedDwellingStructure();
			zero = assignedDwellingStructure.Data.Position;
			if (assignedDwellingStructure != null && assignedDwellingStructure.IsCollapsed)
			{
				zero = TownCentre.RandomPositionInCachedTownCentre();
				sleepingOnFloor = true;
			}
			else
			{
				Dwelling dwelling = FindDwelling();
				if (dwelling != null)
				{
					zero = dwelling.GetDwellingSlotPosition(_brain._directInfoAccess.DwellingSlot);
				}
				sleepingOnFloor = false;
			}
		}
		else
		{
			ClosestTile = StructureManager.GetCloseTile(follower.transform.position, _brain.Location);
			if (ClosestTile != null)
			{
				zero = ClosestTile.WorldPosition;
				ClaimReservations();
			}
			else
			{
				zero = TownCentre.RandomPositionInCachedTownCentre();
			}
			sleepingOnFloor = true;
		}
		return zero;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "Sick/idle-sick");
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Sick/walk-sick");
	}

	public override void OnDoingBegin(Follower follower)
	{
		Debug.Log(_brain.Info.Name + " OnDoingBegin!");
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		if (_sleepingInBed)
		{
			follower.SimpleAnimator.Animate("sleep_bedrest_justhead", 1, true, 0f);
		}
		else
		{
			follower.SimpleAnimator.AddAnimate("sleep_bedrest", 1, true, 0f);
		}
		if (_sleepingInBed)
		{
			Dwelling dwelling = FindDwelling();
			Dwelling.DwellingAndSlot dwellingAndSlot = _brain.GetDwellingAndSlot();
			if (dwelling != null)
			{
				dwelling.SetBedImage(dwellingAndSlot.dwellingslot, Dwelling.SlotState.IN_USE);
			}
		}
	}

	public override void Cleanup(Follower follower)
	{
		Debug.Log(_brain.Info.Name + " Cleanup!");
		base.Cleanup(follower);
		if (_brain.HasHome)
		{
			Dwelling dwelling = FindDwelling();
			if (dwelling != null)
			{
				Dwelling.DwellingAndSlot dwellingAndSlot = _brain.GetDwellingAndSlot();
				dwelling.SetBedImage(dwellingAndSlot.dwellingslot, Dwelling.SlotState.CLAIMED);
			}
		}
	}

	protected override float ExhaustionChange(float deltaGameTime)
	{
		float num = 1f;
		if (_sleepingInBed)
		{
			switch (_brain.GetAssignedDwellingStructure().Data.Type)
			{
			case StructureBrain.TYPES.BED:
				num = 1f;
				break;
			case StructureBrain.TYPES.BED_2:
			case StructureBrain.TYPES.BED_3:
			case StructureBrain.TYPES.SHARED_HOUSE:
				num = 1.25f;
				break;
			}
		}
		if (base.State == FollowerTaskState.Doing)
		{
			return 100f * (deltaGameTime / 480f) * num * -1f;
		}
		return 0f;
	}

	public override void SimCleanup(SimFollower simFollower)
	{
		base.SimCleanup(simFollower);
	}

	private Dwelling FindDwelling()
	{
		Dwelling result = null;
		Structures_Bed assignedDwellingStructure = _brain.GetAssignedDwellingStructure();
		if (assignedDwellingStructure != null)
		{
			result = Dwelling.GetDwellingByID(assignedDwellingStructure.Data.ID);
		}
		return result;
	}

	protected override float SocialChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override float SatiationChange(float deltaGameTime)
	{
		return 0f;
	}
}
