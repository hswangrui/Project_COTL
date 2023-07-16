using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FollowerTask_Sleep : FollowerTask
{
	public const int SLEEP_DURATION_GAME_MINUTES = 240;

	private bool _exhausted;

	public static Action<int> OnHomelessSleep;

	public static Action<int> OnWake;

	private bool isSleeping;

	private bool targetingCollapsedBed = true;

	private bool sleepingOnFloor;

	private bool passedOut;

	private float HealingDelay;

	private float DevotionProgress;

	private float DevotionDuration = 60f;

	private PlacementRegion.TileGridTile ClosestTile;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Sleep;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			if (_sleepingInBed)
			{
				Structures_Bed assignedDwellingStructure = _brain.GetAssignedDwellingStructure();
				if (assignedDwellingStructure == null)
				{
					return _brain.HomeLocation;
				}
				return assignedDwellingStructure.Data.Location;
			}
			if (!_exhausted)
			{
				return _brain.HomeLocation;
			}
			return _brain.Location;
		}
	}

	public override bool BlockTaskChanges
	{
		get
		{
			if (_brain != null && _brain.Stats != null)
			{
				return _brain.Stats.Exhaustion > 0f;
			}
			return false;
		}
	}

	public override bool BlockReactTasks
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
				Structures_Bed assignedDwellingStructure = _brain.GetAssignedDwellingStructure();
				if (assignedDwellingStructure != null)
				{
					result = assignedDwellingStructure.Data.ID;
				}
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

	public FollowerTask_Sleep(bool passedOut = false)
	{
		this.passedOut = passedOut;
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
		if (TimeManager.CurrentPhase != DayPhase.Night)
		{
			_exhausted = _brain.Stats.Exhaustion >= 100f;
		}
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
	}

	protected override void OnArrive()
	{
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

	protected override void OnComplete()
	{
		isSleeping = false;
		Action<int> onWake = OnWake;
		if (onWake != null)
		{
			onWake(_brain.Info.ID);
		}
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (!_exhausted && _brain.Stats.Exhaustion >= 100f && TimeManager.CurrentPhase != DayPhase.Night)
		{
			_exhausted = true;
			if (!isSleeping)
			{
				RecalculateDestination();
			}
			_exhausted = false;
		}
		if (_brain.Stats.Exhaustion <= 0f && TimeManager.CurrentPhase != DayPhase.Night)
		{
			_exhausted = false;
			End();
		}
		if (sleepingOnFloor && _sleepingInBed && isSleeping)
		{
			ClearDestination();
			SetState(FollowerTaskState.GoingTo);
			isSleeping = false;
		}
		if (!_brain.HasHome && !_exhausted)
		{
			Dwelling.DwellingAndSlot freeDwellingAndSlot = StructureManager.GetFreeDwellingAndSlot(Location, _brain._directInfoAccess);
			if (freeDwellingAndSlot != null && !StructureManager.GetStructureByID<Structures_Bed>(freeDwellingAndSlot.ID).ReservedForTask)
			{
				StructureManager.GetStructureByID<Structures_Bed>(freeDwellingAndSlot.ID).ReservedForTask = true;
				_brain.HardSwapToTask(new FollowerTask_ClaimDwelling(freeDwellingAndSlot));
			}
		}
		if (!sleepingOnFloor && _sleepingInBed && isSleeping && (DevotionProgress += deltaGameTime) > DevotionDuration)
		{
			DevotionProgress = 0f;
			if (_brain.GetAssignedDwellingStructure().Data.Type == StructureBrain.TYPES.BED_3)
			{
				DepositSoul(1);
			}
		}
	}

	protected virtual void DepositSoul(int DevotionToGive)
	{
		_brain.GetAssignedDwellingStructure().SoulCount += DevotionToGive;
	}

	protected override float RestChange(float deltaGameTime)
	{
		if (base.State == FollowerTaskState.Doing)
		{
			return 100f * (deltaGameTime / 240f);
		}
		return base.RestChange(deltaGameTime);
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

	protected override float SatiationChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		Vector3 result;
		if (_exhausted || passedOut)
		{
			result = follower.transform.position;
		}
		else if (_brain.HasHome)
		{
			Structures_Bed assignedDwellingStructure = _brain.GetAssignedDwellingStructure();
			result = ((assignedDwellingStructure != null) ? assignedDwellingStructure.Data.Position : TownCentre.RandomPositionInCachedTownCentre());
			if (assignedDwellingStructure != null && assignedDwellingStructure.IsCollapsed)
			{
				if (!targetingCollapsedBed)
				{
					result = TownCentre.RandomPositionInCachedTownCentre();
					sleepingOnFloor = true;
				}
			}
			else
			{
				Dwelling dwelling = FindDwelling();
				if (dwelling != null)
				{
					result = dwelling.GetDwellingSlotPosition(_brain._directInfoAccess.DwellingSlot);
				}
				sleepingOnFloor = false;
			}
		}
		else
		{
			if (follower != null)
			{
				ClosestTile = StructureManager.GetCloseTile(follower.transform.position, _brain.Location);
				if (ClosestTile != null)
				{
					result = ClosestTile.WorldPosition;
					ClaimReservations();
				}
				else
				{
					result = TownCentre.RandomPositionInCachedTownCentre();
				}
			}
			else
			{
				result = TownCentre.RandomPositionInCachedTownCentre();
			}
			sleepingOnFloor = true;
		}
		return result;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		if (!isSleeping)
		{
			return;
		}
		SetFollowerAnimation(follower);
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

	public override void SimDoingBegin(SimFollower simFollower)
	{
		base.SimDoingBegin(simFollower);
		if (!isSleeping)
		{
			isSleeping = true;
		}
	}

	public override void OnDoingBegin(Follower follower)
	{
		base.OnDoingBegin(follower);
		if (targetingCollapsedBed && _brain.GetAssignedDwellingStructure() != null && _brain.GetAssignedDwellingStructure().IsCollapsed)
		{
			follower.TimedAnimation("Conversations/react-hate" + UnityEngine.Random.Range(1, 3), 2f, delegate
			{
				ClearDestination();
				targetingCollapsedBed = false;
				SetState(FollowerTaskState.GoingTo);
			});
			return;
		}
		if (isSleeping)
		{
			SetFollowerAnimation(follower);
		}
		if (isSleeping)
		{
			return;
		}
		isSleeping = true;
		follower.TimedAnimation("sleepy", 1f, delegate
		{
			SetFollowerAnimation(follower);
		});
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

	private void SetFollowerAnimation(Follower follower)
	{
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		if (_sleepingInBed && !passedOut)
		{
			follower.SimpleAnimator.Animate("sleep_justhead", 1, true, 0f);
		}
		else
		{
			follower.SimpleAnimator.AddAnimate("sleep", 1, true, 0f);
		}
		follower.IllnessAura.SetActive(follower.Brain.Info.CursedState == Thought.Ill);
	}

	public override void OnFinaliseBegin(Follower follower)
	{
		if (!_brain.HasHome)
		{
			_brain._directInfoAccess.DaysSleptOutside++;
			if (_brain._directInfoAccess.DaysSleptOutside > 3)
			{
				DataManager.Instance.OnboardedBuildingHouse = false;
			}
		}
		else
		{
			_brain._directInfoAccess.DaysSleptOutside = 0;
		}
		if (_brain.Info.CursedState == Thought.Ill && TimeManager.TotalElapsedGameTime - DataManager.Instance.LastFollowerToBecomeIllFromSleepingNearIllFollower > 2400f / DifficultyManager.GetTimeBetweenIllnessMultiplier())
		{
			foreach (FollowerBrain item in FollowerBrain.GetBrainsWithinRadius(_brain.LastPosition, 3f))
			{
				if (item != _brain && item.Info.CursedState == Thought.None)
				{
					item.MakeSick();
					DataManager.Instance.LastFollowerToBecomeIllFromSleepingNearIllFollower = TimeManager.TotalElapsedGameTime;
					break;
				}
			}
		}
		follower.StartCoroutine(RandomDelay(follower));
	}

	private IEnumerator RandomDelay(Follower follower)
	{
		float seconds = UnityEngine.Random.Range(0f, 1f);
		yield return new WaitForSeconds(seconds);
		follower.TimedAnimation("morning", 4.7f, delegate
		{
			_003C_003En__0(follower);
		});
	}

	public override void Cleanup(Follower follower)
	{
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
		if ((bool)follower)
		{
			follower.IllnessAura.SetActive(false);
		}
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

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0(Follower follower)
	{
		base.OnFinaliseBegin(follower);
	}
}
