using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowerBrain
{
	public enum AdorationActions
	{
		Sermon,
		BigGift,
		Gift,
		Necklace,
		Quest,
		Inspire,
		Bribe,
		Intimidate,
		ConfessionBooth,
		Bless,
		Ritual_AlmsToPoor,
		FaithEnforce,
		AscendedFollower_Lvl2,
		HappyFollowerNPC,
		FirePit,
		FightPitMercy,
		FightPitDeath,
		AscendedFollower_Lvl3,
		AscendedFollower_Lvl4,
		AscendedFollower_Lvl5,
		SmoochSpouse,
		AssignEnforcer,
		PetDog,
		BlessLvl1,
		SermonLvl1,
		BribeLvl1,
		InspireLvl1,
		IntimidateLvl1,
		BigGiftLvl1,
		GiftLvl1,
		NecklaceLvl1,
		LevelUp
	}

	public struct PendingTaskData
	{
		public bool KeepExistingTask;

		public FollowerTask Task;

		public int ListIndex;
	}

	public delegate void CursedEvent();

	public delegate void DwellingAssignmentChanged(int followerID, Dwelling.DwellingAndSlot d);

	public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
	{
		public int Compare(TKey x, TKey y)
		{
			int num = x.CompareTo(y);
			if (num == 0)
			{
				return -1;
			}
			return -num;
		}
	}

	public FollowerBrainInfo Info;

	public FollowerBrainStats Stats;

	public FollowerInfo _directInfoAccess;

	public FollowerLocation DesiredLocation;

	private bool _followingPlayer;

	private bool _inRitual;

	public NotificationCentre.NotificationType LeftCultWithReason;

	private float bathroomOffset = -1f;

	private string ReturnString;

	private float ModifierTotal;

	private static List<int> MultipleSpouseFaith = new List<int> { 30, 15, 10, 5, 3, -2, -5, -7, -10 };

	public Action<float> OnNewThought;

	public Action OnAdoration;

	public static Dictionary<AdorationActions, int> AdorationsAndActions = new Dictionary<AdorationActions, int>
	{
		{
			AdorationActions.Sermon,
			5
		},
		{
			AdorationActions.Bless,
			13
		},
		{
			AdorationActions.BigGift,
			30
		},
		{
			AdorationActions.Quest,
			50
		},
		{
			AdorationActions.Gift,
			20
		},
		{
			AdorationActions.Necklace,
			25
		},
		{
			AdorationActions.Inspire,
			16
		},
		{
			AdorationActions.Intimidate,
			10
		},
		{
			AdorationActions.Bribe,
			15
		},
		{
			AdorationActions.Ritual_AlmsToPoor,
			30
		},
		{
			AdorationActions.FaithEnforce,
			5
		},
		{
			AdorationActions.HappyFollowerNPC,
			60
		},
		{
			AdorationActions.ConfessionBooth,
			75
		},
		{
			AdorationActions.FirePit,
			30
		},
		{
			AdorationActions.FightPitMercy,
			20
		},
		{
			AdorationActions.FightPitDeath,
			50
		},
		{
			AdorationActions.AscendedFollower_Lvl2,
			20
		},
		{
			AdorationActions.AscendedFollower_Lvl3,
			30
		},
		{
			AdorationActions.AscendedFollower_Lvl4,
			40
		},
		{
			AdorationActions.AscendedFollower_Lvl5,
			50
		},
		{
			AdorationActions.SmoochSpouse,
			10
		},
		{
			AdorationActions.AssignEnforcer,
			30
		},
		{
			AdorationActions.PetDog,
			5
		},
		{
			AdorationActions.SermonLvl1,
			10
		},
		{
			AdorationActions.BribeLvl1,
			20
		},
		{
			AdorationActions.BlessLvl1,
			35
		},
		{
			AdorationActions.InspireLvl1,
			40
		},
		{
			AdorationActions.IntimidateLvl1,
			30
		},
		{
			AdorationActions.BigGiftLvl1,
			35
		},
		{
			AdorationActions.GiftLvl1,
			25
		},
		{
			AdorationActions.NecklaceLvl1,
			30
		},
		{
			AdorationActions.LevelUp,
			100
		}
	};

	public Action OnGetXP;

	public int SpeakersInRange;

	private Dictionary<int, int> ReactionDictionary = new Dictionary<int, int>();

	public Action<FollowerState, FollowerState> OnStateChanged;

	private FollowerState _currentState;

	public PendingTaskData PendingTask;

	public Action<FollowerTask, FollowerTask> OnTaskChanged;

	private FollowerTask _nextTask;

	private FollowerTask _currentTask;

	public Action OnBecomeDissenter;

	public static DwellingAssignmentChanged OnDwellingAssigned;

	public static DwellingAssignmentChanged OnDwellingCleared;

	public static DwellingAssignmentChanged OnDwellingAssignedAwaitClaim;

	public static Action<int> OnBrainAdded;

	public static Action<int> OnBrainRemoved;

	private static Dictionary<int, FollowerBrain> _brainsByID = new Dictionary<int, FollowerBrain>();

	public static List<FollowerBrain> AllBrains = new List<FollowerBrain>();

	public List<TaskAndTime> _taskMemory
	{
		get
		{
			return _directInfoAccess.TaskMemory;
		}
		set
		{
			_directInfoAccess.TaskMemory = value;
		}
	}

	public FollowerLocation HomeLocation
	{
		get
		{
			return _directInfoAccess.HomeLocation;
		}
	}

	public FollowerLocation Location
	{
		get
		{
			return _directInfoAccess.Location;
		}
		set
		{
			_directInfoAccess.Location = value;
		}
	}

	public Vector3 LastPosition
	{
		get
		{
			return _directInfoAccess.LastPosition;
		}
		set
		{
			_directInfoAccess.LastPosition = value;
		}
	}

	public FollowerTaskType SavedFollowerTaskType
	{
		get
		{
			return _directInfoAccess.SavedFollowerTaskType;
		}
		set
		{
			_directInfoAccess.SavedFollowerTaskType = value;
		}
	}

	public FollowerLocation SavedFollowerTaskLocation
	{
		get
		{
			return _directInfoAccess.SavedFollowerTaskLocation;
		}
		set
		{
			_directInfoAccess.SavedFollowerTaskLocation = value;
		}
	}

	public Vector3 SavedFollowerTaskDestination
	{
		get
		{
			return _directInfoAccess.SavedFollowerTaskDestination;
		}
		set
		{
			_directInfoAccess.SavedFollowerTaskDestination = value;
		}
	}

	public bool FollowingPlayer
	{
		get
		{
			return _followingPlayer;
		}
		set
		{
			if (_followingPlayer != value)
			{
				_followingPlayer = value;
				CheckChangeState();
			}
		}
	}

	public bool InRitual
	{
		get
		{
			return _inRitual;
		}
		set
		{
			if (_inRitual != value)
			{
				_inRitual = value;
				CheckChangeState();
			}
		}
	}

	public bool DiedFromMurder
	{
		get
		{
			return _directInfoAccess.DiedFromMurder;
		}
		set
		{
			if (value)
			{
				_directInfoAccess.TimeOfDeath = TimeManager.CurrentDay;
			}
			_directInfoAccess.DiedFromMurder = value;
		}
	}

	public bool DiedInPrison
	{
		get
		{
			return _directInfoAccess.DiedInPrison;
		}
		set
		{
			if (value)
			{
				_directInfoAccess.TimeOfDeath = TimeManager.CurrentDay;
			}
			_directInfoAccess.DiedInPrison = value;
		}
	}

	public bool DiedOfIllness
	{
		get
		{
			return _directInfoAccess.DiedOfIllness;
		}
		set
		{
			if (value)
			{
				_directInfoAccess.TimeOfDeath = TimeManager.CurrentDay;
			}
			_directInfoAccess.DiedOfIllness = value;
		}
	}

	public bool DiedOfOldAge
	{
		get
		{
			return _directInfoAccess.DiedOfOldAge;
		}
		set
		{
			if (value)
			{
				_directInfoAccess.TimeOfDeath = TimeManager.CurrentDay;
			}
			_directInfoAccess.DiedOfOldAge = value;
		}
	}

	public bool DiedOfStarvation
	{
		get
		{
			return _directInfoAccess.DiedOfStarvation;
		}
		set
		{
			if (value)
			{
				_directInfoAccess.TimeOfDeath = TimeManager.CurrentDay;
			}
			_directInfoAccess.DiedOfStarvation = value;
		}
	}

	public bool DiedFromTwitchChat
	{
		get
		{
			return _directInfoAccess.DiedFromTwitchChat;
		}
		set
		{
			if (value)
			{
				_directInfoAccess.TimeOfDeath = TimeManager.CurrentDay;
			}
			_directInfoAccess.DiedFromTwitchChat = value;
		}
	}

	public bool DiedFromDeadlyDish
	{
		get
		{
			return _directInfoAccess.DiedFromDeadlyDish;
		}
		set
		{
			_directInfoAccess.DiedFromDeadlyDish = value;
		}
	}

	private bool _leavingCult
	{
		get
		{
			return _directInfoAccess.LeavingCult;
		}
		set
		{
			_directInfoAccess.LeavingCult = value;
		}
	}

	public bool LeavingCult
	{
		get
		{
			return _leavingCult;
		}
		set
		{
			if (_leavingCult != value)
			{
				_leavingCult = value;
				CheckChangeTask();
			}
		}
	}

	public bool LeftCult { get; set; }

	public float DevotionToGive
	{
		get
		{
			float num = 1f;
			num += (HasTrait(FollowerTrait.TraitType.Faithful) ? 0.15f : 0f);
			num -= (HasTrait(FollowerTrait.TraitType.Faithless) ? 0.15f : 0f);
			num += (HasTrait(FollowerTrait.TraitType.MushroomBanned) ? 0.05f : 0f);
			num += (ThoughtExists(Thought.Intimidated) ? 0.1f : 0f);
			num -= (HasTrait(FollowerTrait.TraitType.Lazy) ? 0.1f : 0f);
			num += CultFaithManager.CurrentFaith / 500f;
			num += (FollowerBrainStats.IsEnlightened ? 0.25f : 0f);
			num += ((Info.Necklace == InventoryItem.ITEM_TYPE.Necklace_1) ? 0.15f : 0f);
			float b = 1f * num;
			return Mathf.Max(1f, b);
		}
	}

	public float ResourceHarvestingMultiplier
	{
		get
		{
			float num = 1f;
			num += ((Info.Necklace == InventoryItem.ITEM_TYPE.Necklace_4) ? 0.25f : 0f);
			float b = 1f * num;
			return Mathf.Max(1f, b);
		}
	}

	public FollowerState CurrentState
	{
		get
		{
			return _currentState;
		}
		private set
		{
			FollowerState currentState = _currentState;
			_currentState = value;
			Action<FollowerState, FollowerState> onStateChanged = OnStateChanged;
			if (onStateChanged != null)
			{
				onStateChanged(_currentState, currentState);
			}
		}
	}

	public bool ShouldReconsiderTask { get; set; }

	public FollowerTask CurrentTask
	{
		get
		{
			FollowerTask currentTask = _currentTask;
			return ((currentTask != null) ? currentTask.ChangeLocationTask : null) ?? _currentTask;
		}
		private set
		{
			if (_currentTask != null && _currentTask.State != FollowerTaskState.Done)
			{
				Debug.Log(_currentTask.Type);
			}
			FollowerTask currentTask = _currentTask;
			_currentTask = value;
			if (currentTask != null)
			{
				currentTask.ReleaseReservations();
			}
			Action<FollowerTask, FollowerTask> onTaskChanged = OnTaskChanged;
			if (onTaskChanged != null)
			{
				onTaskChanged(_currentTask, currentTask);
			}
			if (_currentTask != null)
			{
				_currentTask.Start();
			}
			if (currentTask != null)
			{
				TaskAndTime.SetTaskTime(TimeManager.TotalElapsedGameTime, currentTask.Type, this);
			}
		}
	}

	public FollowerTaskType CurrentTaskType
	{
		get
		{
			if (CurrentTask != null)
			{
				return CurrentTask.Type;
			}
			return FollowerTaskType.None;
		}
	}

	public int CurrentTaskUsingStructureID
	{
		get
		{
			if (CurrentTask != null)
			{
				return CurrentTask.UsingStructureID;
			}
			return 0;
		}
	}

	public FollowerTaskType CurrentOverrideTaskType
	{
		get
		{
			return _directInfoAccess.CurrentOverrideTaskType;
		}
		set
		{
			_directInfoAccess.CurrentOverrideTaskType = value;
		}
	}

	public StructureBrain.TYPES CurrentOverrideStructureType
	{
		get
		{
			return _directInfoAccess.CurrentOverrideStructureType;
		}
		set
		{
			_directInfoAccess.CurrentOverrideStructureType = value;
		}
	}

	private int OverrideDayIndex
	{
		get
		{
			return _directInfoAccess.OverrideDayIndex;
		}
		set
		{
			_directInfoAccess.OverrideDayIndex = value;
		}
	}

	private bool OverrideTaskCompleted
	{
		get
		{
			return _directInfoAccess.OverrideTaskCompleted;
		}
		set
		{
			_directInfoAccess.OverrideTaskCompleted = value;
		}
	}

	public bool HasHome
	{
		get
		{
			return _directInfoAccess.DwellingID != Dwelling.NO_HOME;
		}
	}

	public bool HomeIsCorrectLevel
	{
		get
		{
			return _directInfoAccess.DwellingLevel >= Info.XPLevel;
		}
	}

	public int HomeID
	{
		get
		{
			return _directInfoAccess.DwellingID;
		}
	}

	public event CursedEvent OnCursedStateRemoved;

	private FollowerBrain(FollowerInfo info)
	{
		if (info.HomeLocation == FollowerLocation.Church)
		{
			info.HomeLocation = FollowerLocation.Base;
		}
		Info = new FollowerBrainInfo(info, this);
		Stats = new FollowerBrainStats(info, this);
		_directInfoAccess = info;
		DesiredLocation = Location;
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
		StructureManager.OnStructureMoved = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureMoved, new StructureManager.StructureChanged(OnStructureMoved));
		StructureManager.OnStructureUpgraded = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureUpgraded, new StructureManager.StructureChanged(OnStructureUpgraded));
		StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(OnStructureRemoved));
		FollowerBrainStats.OnExhaustionStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Combine(FollowerBrainStats.OnExhaustionStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnExhaustionStateChanged));
		FollowerBrainStats.OnIllnessStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Combine(FollowerBrainStats.OnIllnessStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnIllnessStateChanged));
		FollowerBrainStats.OnHappinessStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Combine(FollowerBrainStats.OnHappinessStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnHappinessStateChanged));
		FollowerBrainStats.OnSatiationStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Combine(FollowerBrainStats.OnSatiationStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnSatiationStateChanged));
		FollowerBrainStats.OnStarvationStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Combine(FollowerBrainStats.OnStarvationStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnStarvationStateChanged));
		FollowerBrainStats.OnRestStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Combine(FollowerBrainStats.OnRestStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnRestStateChanged));
		FollowerBrainStats.OnMotivatedChanged = (FollowerBrainStats.StatusChangedEvent)Delegate.Combine(FollowerBrainStats.OnMotivatedChanged, new FollowerBrainStats.StatusChangedEvent(OnMotivatedChanged));
		FollowerBrainStats.OnStarvationStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Combine(FollowerBrainStats.OnStarvationStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnStarvationChanged));
		StructuresData.OnResearchBegin = (Action)Delegate.Combine(StructuresData.OnResearchBegin, new Action(OnResearchBegin));
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
		TimeManager.OnScheduleChanged = (Action)Delegate.Combine(TimeManager.OnScheduleChanged, new Action(OnScheduleChanged));
		TimeManager.OnNewDayStarted = (Action)Delegate.Combine(TimeManager.OnNewDayStarted, new Action(OnNewDay));
		if (!_directInfoAccess.TraitsSet)
		{
			AddTrait(FollowerTrait.GetStartingTrait(), false);
			AddTrait(FollowerTrait.GetStartingTrait(), false);
			if (UnityEngine.Random.value <= 0.2f)
			{
				AddTrait(FollowerTrait.GetRareTrait(), false);
			}
			else if (UnityEngine.Random.value <= 0.4f)
			{
				AddTrait(FollowerTrait.GetStartingTrait(), false);
			}
			_directInfoAccess.TraitsSet = true;
		}
	}

	public void Cleanup()
	{
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
		StructureManager.OnStructureMoved = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureMoved, new StructureManager.StructureChanged(OnStructureMoved));
		StructureManager.OnStructureUpgraded = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureUpgraded, new StructureManager.StructureChanged(OnStructureUpgraded));
		StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(OnStructureRemoved));
		FollowerBrainStats.OnIllnessStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Remove(FollowerBrainStats.OnIllnessStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnIllnessStateChanged));
		FollowerBrainStats.OnHappinessStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Remove(FollowerBrainStats.OnHappinessStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnHappinessStateChanged));
		FollowerBrainStats.OnSatiationStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Remove(FollowerBrainStats.OnSatiationStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnSatiationStateChanged));
		FollowerBrainStats.OnRestStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Remove(FollowerBrainStats.OnRestStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnRestStateChanged));
		FollowerBrainStats.OnMotivatedChanged = (FollowerBrainStats.StatusChangedEvent)Delegate.Remove(FollowerBrainStats.OnMotivatedChanged, new FollowerBrainStats.StatusChangedEvent(OnMotivatedChanged));
		FollowerBrainStats.OnStarvationStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Remove(FollowerBrainStats.OnStarvationStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnStarvationChanged));
		FollowerBrainStats.OnStarvationStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Remove(FollowerBrainStats.OnStarvationStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnStarvationStateChanged));
		FollowerBrainStats.OnExhaustionStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Remove(FollowerBrainStats.OnExhaustionStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnExhaustionStateChanged));
		StructuresData.OnResearchBegin = (Action)Delegate.Remove(StructuresData.OnResearchBegin, new Action(OnResearchBegin));
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
		TimeManager.OnScheduleChanged = (Action)Delegate.Remove(TimeManager.OnScheduleChanged, new Action(OnScheduleChanged));
		TimeManager.OnNewDayStarted = (Action)Delegate.Remove(TimeManager.OnNewDayStarted, new Action(OnNewDay));
		if (CurrentTask != null && CurrentTask.State != FollowerTaskState.Done)
		{
			CurrentTask.Abort();
		}
	}

	public void ResetStats()
	{
		_directInfoAccess.ResetStats();
		Info.CursedState = Thought.None;
		RemoveThought(Thought.Dissenter, true);
		RemoveThought(Thought.OldAge, true);
		RemoveThought(Thought.BecomeStarving, true);
		RemoveThought(Thought.Ill, true);
		Stats.Reeducation = 0f;
		Stats.Illness = 0f;
		Stats.Starvation = 0f;
		Stats.Exhaustion = 0f;
		LeavingCult = false;
		DiedInPrison = false;
		DiedOfIllness = false;
		DiedFromMurder = false;
		DiedOfOldAge = false;
		DiedOfStarvation = false;
		DiedFromTwitchChat = false;
		DiedFromDeadlyDish = false;
		_directInfoAccess.TaxEnforcer = false;
		_directInfoAccess.FaithEnforcer = false;
		ClearThought(Thought.OldAge);
		ClearThought(Thought.Dissenter);
		ClearThought(Thought.Ill);
		ClearThought(Thought.BecomeStarving);
	//	TwitchFollowers.SendFollowerInformation(_directInfoAccess);
	}

	public FollowerOutfit CreateOutfit()
	{
		return new FollowerOutfit(_directInfoAccess);
	}

	private void OnNewDay()
	{
		Stats.PaidTithes = false;
		Stats.ReceivedBlessing = false;
		Stats.Bribed = false;
		_directInfoAccess.TaxedToday = false;
		_directInfoAccess.FaithedToday = false;
		Stats.Inspired = false;
		Stats.PetDog = false;
		Stats.Intimidated = false;
		Stats.ReeducatedAction = false;
		Stats.KissedAction = false;
		if (CurrentTask == null || (!(CurrentTask is FollowerTask_OnMissionary) && !(CurrentTask is FollowerTask_MissionaryComplete)))
		{
			_directInfoAccess.MissionaryExhaustion = Mathf.Clamp(_directInfoAccess.MissionaryExhaustion - 0.5f, 1f, 2.1474836E+09f);
		}
		if (CurrentTask == null)
		{
			if (!HasHome)
			{
				if (HasTrait(FollowerTrait.TraitType.Materialistic))
				{
					AddThought(Thought.SleptOutisdeMaterialisticTrait);
				}
				else
				{
					AddThought(Thought.SleptOutisde);
				}
			}
			else
			{
				StructureBrain.TYPES structureTypeByID = StructureManager.GetStructureTypeByID(HomeID);
				if (HasTrait(FollowerTrait.TraitType.Materialistic))
				{
					switch (structureTypeByID)
					{
					case StructureBrain.TYPES.BED:
						AddThought(Thought.SleptHouse1MaterialisticTrait);
						break;
					case StructureBrain.TYPES.BED_2:
						AddThought(Thought.SleptHouse2MaterialisticTrait);
						break;
					case StructureBrain.TYPES.BED_3:
						AddThought(Thought.SleptHouse3MaterialisticTrait);
						break;
					case StructureBrain.TYPES.SHARED_HOUSE:
						AddThought(Thought.SleptHouse3MaterialisticTrait);
						break;
					}
				}
				else
				{
					switch (structureTypeByID)
					{
					case StructureBrain.TYPES.BED:
						AddThought(Thought.SleptHouse1);
						break;
					case StructureBrain.TYPES.BED_2:
						AddThought(Thought.SleptHouse2);
						break;
					case StructureBrain.TYPES.BED_3:
						AddThought(Thought.SleptHouse3);
						break;
					case StructureBrain.TYPES.SHARED_HOUSE:
						AddThought(Thought.SleptHouse3);
						break;
					}
				}
			}
		}
		if (Info.Age++ >= Info.LifeExpectancy && Info.CursedState != Thought.OldAge && Info.CursedState != Thought.Zombie && !HasTrait(FollowerTrait.TraitType.Immortal) && DataManager.Instance.OnboardedOldFollower)
		{
			if (TimeManager.TotalElapsedGameTime - DataManager.Instance.LastFollowerToReachOldAge < 600f / DifficultyManager.GetTimeBetweenOldAgeMultiplier())
			{
				return;
			}
			MakeOld();
		}
		if (_directInfoAccess.WakeUpDay == TimeManager.CurrentDay - 1 && !FollowerManager.FollowerLocked(Info.ID))
		{
			MakeExhausted();
		}
	}

	public void Tick(float deltaGameTime)
	{
		if (_currentState == null || _currentState.Type == FollowerStateType.Default)
		{
			CheckChangeState();
		}
		if (CurrentTask == null)
		{
			CheckChangeTask();
		}
		UpdateThoughts();
		if (Stats.Bathroom < Stats.TargetBathroom && !TimeManager.IsNight)
		{
			float num = 1f / 6f;
			Stats.Bathroom += deltaGameTime * num * Stats.BathroomFillRate;
		}
		if (Stats.Reeducation != 0f && !LeavingCult && CurrentTaskType != FollowerTaskType.Imprisoned && Info.CursedState == Thought.Dissenter)
		{
			if (Stats.Reeducation > 0f && !Stats.GivenDissentWarning)
			{
				float num2 = Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD);
				Stats.DissentGold = Mathf.Floor(UnityEngine.Random.Range(num2 * 0.15f, num2 * 0.4f));
				if (NotificationCentre.Instance != null)
				{
					NotificationCentre.Instance.PlayGenericNotificationNonLocalizedParams("Notifications/DissenterLeavingTomorrow", Info.Name, Stats.DissentGold.ToString());
				}
				Stats.GivenDissentWarning = true;
			}
			if (Stats.Reeducation >= 100f && PlayerFarming.Location == FollowerLocation.Base && DataManager.Instance.OnboardingFinished)
			{
				LeavingCult = true;
			}
		}
		if (CurrentTask != null)
		{
			CurrentTask.Tick(deltaGameTime);
		}
	}

	public string GetThoughtString(float SubTextSize)
	{
		ReturnString = "";
		foreach (ThoughtData thought in Stats.Thoughts)
		{
			ModifierTotal = 0f;
			int num = -1;
			while (++num < thought.Quantity)
			{
				ModifierTotal += ((num <= 0) ? thought.Modifier : ((float)thought.StackModifier));
			}
			ReturnString = ReturnString + ((ModifierTotal >= 0f) ? "<color=green><b>+" : "<color=red><b>") + ModifierTotal + "</b></color> " + FollowerThoughts.GetLocalisedName(thought.ThoughtType) + ((thought.Quantity > 1) ? (string.Format(" <size={0}>(x", SubTextSize) + thought.Quantity + ")") : "") + "</size>\n" + string.Format("<size={0}><i>", SubTextSize) + FollowerThoughts.GetLocalisedDescription(thought.ThoughtType) + "</i></size>\n\n";
		}
		return ReturnString;
	}

	public static void AddMarriageThoughts()
	{
		List<FollowerBrain> list = new List<FollowerBrain>();
		foreach (FollowerBrain allBrain in AllBrains)
		{
			if (allBrain.Info.MarriedToLeader)
			{
				list.Add(allBrain);
			}
		}
		Debug.Log("MarriedBrains.Count: " + list.Count);
		switch (list.Count)
		{
		case 0:
			return;
		case 1:
			list[0].AddThought(Thought.MarriedToLeader);
			return;
		}
		Debug.Log("Add married thoughts!");
		int num = -1;
		while (++num < list.Count)
		{
			int num2 = -1;
			while (++num2 < list.Count)
			{
				if (num != num2)
				{
					IDAndRelationship orCreateRelationship = list[num].Info.GetOrCreateRelationship(list[num2].Info.ID);
					if (orCreateRelationship.Relationship > -11)
					{
						orCreateRelationship.Relationship = -11;
						orCreateRelationship.CurrentRelationshipState = IDAndRelationship.RelationshipState.Enemies;
					}
				}
			}
			ThoughtData data = FollowerThoughts.GetData(Thought.MultiMarriedToLeader);
			data.Init();
			data.Modifier = MultipleSpouseFaith[Mathf.Clamp(list.Count, 0, MultipleSpouseFaith.Count - 1)];
			list[num].AddThought(data);
		}
	}

	public void AddThought(ThoughtData thought, bool forced = false)
	{
		if (CurrentTask != null && CurrentTask.BlockThoughts && !forced)
		{
			return;
		}
		float happiness = Stats.Happiness;
		if (OnNewThought != null)
		{
			OnNewThought(thought.Modifier);
		}
		int num = -1;
		while (++num < Stats.Thoughts.Count)
		{
			ThoughtData thoughtData = Stats.Thoughts[num];
			if (thoughtData.ThoughtGroup == thought.ThoughtGroup && thoughtData.Stacking <= 0)
			{
				Debug.Log(string.Concat("REPLACE! ", thought.ThoughtGroup, "  ", thought.ThoughtType));
				Stats.Thoughts[num] = thought;
				Stats.OnThoughtsChanged(this, happiness, Stats.Happiness);
				return;
			}
			if (thoughtData.ThoughtType == thought.ThoughtType && thoughtData.Stacking > 0)
			{
				if (thoughtData.Quantity < thoughtData.Stacking)
				{
					thoughtData.Quantity++;
				}
				else
				{
					thoughtData.CoolDowns.RemoveAt(0);
					thoughtData.TimeStarted.RemoveAt(0);
				}
				thoughtData.CoolDowns.Add(thought.Duration);
				thoughtData.TimeStarted.Add(TimeManager.TotalElapsedGameTime);
				Stats.OnThoughtsChanged(this, happiness, Stats.Happiness);
				return;
			}
		}
		Stats.Thoughts.Add(thought);
		Stats.OnThoughtsChanged(this, happiness, Stats.Happiness);
	}

	public void AddThought(Thought thought, bool InsetAtZero = false, bool forced = false)
	{
		if (CurrentTask != null && CurrentTask.BlockThoughts && !forced)
		{
			return;
		}
		float happiness = Stats.Happiness;
		ThoughtData data = FollowerThoughts.GetData(thought);
		data.Init();
		if (data != null && OnNewThought != null)
		{
			OnNewThought(data.Modifier);
		}
		int num = -1;
		while (++num < Stats.Thoughts.Count)
		{
			ThoughtData thoughtData = Stats.Thoughts[num];
			if (thoughtData.ThoughtGroup == data.ThoughtGroup && thoughtData.Stacking <= 0)
			{
				Stats.Thoughts[num] = data;
				Stats.OnThoughtsChanged(this, happiness, Stats.Happiness);
				return;
			}
			if (thoughtData.ThoughtType == thought && thoughtData.Stacking > 0)
			{
				if (thoughtData.Quantity < thoughtData.Stacking)
				{
					thoughtData.Quantity++;
				}
				else
				{
					thoughtData.CoolDowns.RemoveAt(0);
					thoughtData.TimeStarted.RemoveAt(0);
				}
				thoughtData.CoolDowns.Add(data.Duration);
				thoughtData.TimeStarted.Add(TimeManager.TotalElapsedGameTime);
				Stats.OnThoughtsChanged(this, happiness, Stats.Happiness);
				return;
			}
		}
		if (InsetAtZero)
		{
			Stats.Thoughts.Insert(0, data);
		}
		else
		{
			Stats.Thoughts.Add(data);
		}
		Stats.OnThoughtsChanged(this, happiness, Stats.Happiness);
	}

	public bool HasThought(Thought thought)
	{
		foreach (ThoughtData thought2 in Stats.Thoughts)
		{
			if (thought2.ThoughtType == thought)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateThoughts()
	{
		float happiness = Stats.Happiness;
		foreach (ThoughtData thought in Stats.Thoughts)
		{
			if (thought.Duration == -1f || !(TimeManager.TotalElapsedGameTime - thought.TimeStarted[0] > thought.CoolDowns[0]))
			{
				continue;
			}
			if (thought.Quantity > 1)
			{
				thought.Quantity--;
				thought.CoolDowns.RemoveAt(0);
				thought.TimeStarted.RemoveAt(0);
			}
			else
			{
				Stats.Thoughts.Remove(thought);
			}
			Stats.OnThoughtsChanged(this, happiness, Stats.Happiness);
			switch (thought.ThoughtType)
			{
			case Thought.Brainwashed:
			{
				float value = UnityEngine.Random.value;
				if (value <= 0.2f)
				{
					AddThought(Thought.BrainwashedHangOverLight);
				}
				else if (value <= 0.2f)
				{
					AddThought(Thought.BrainwashedHangOverMild);
				}
				else
				{
					AddThought(Thought.BrainwashedHangOverPounding);
				}
				float num = (DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.MushroomBanned) ? 0.5f : 0.7f);
				if (UnityEngine.Random.value > num)
				{
					DataManager.Instance.LastFollowerToBecomeIll = TimeManager.TotalElapsedGameTime;
					ApplyCurseState(Thought.Ill);
				}
				break;
			}
			case Thought.FeelingUnwell:
				if (!(TimeManager.TotalElapsedGameTime - DataManager.Instance.LastFollowerToBecomeIll < 600f / DifficultyManager.GetTimeBetweenIllnessMultiplier()))
				{
					DataManager.Instance.LastFollowerToBecomeIll = TimeManager.TotalElapsedGameTime;
					ApplyCurseState(Thought.Ill);
				}
				break;
			case Thought.Ill:
				Debug.Log("REMOVE ILL THOUGHT AND DIE");
				CheckChangeTask();
				break;
			case Thought.OldAge:
				if (!HasTrait(FollowerTrait.TraitType.Immortal))
				{
					DiedOfOldAge = DataManager.Instance.OnboardingFinished;
				}
				CheckChangeTask();
				break;
			}
			break;
		}
	}

	public void RemoveThought(Thought Thought, bool RemoveAllStack)
	{
		foreach (ThoughtData thought in Stats.Thoughts)
		{
			if (thought.ThoughtType == Thought)
			{
				thought.Duration = 0f;
				thought.CoolDowns[0] = 0f;
				if (RemoveAllStack)
				{
					thought.Quantity = 1;
				}
			}
		}
	}

	public void ClearThought(Thought thought)
	{
		for (int num = Stats.Thoughts.Count - 1; num >= 0; num--)
		{
			if (Stats.Thoughts[num].ThoughtType == thought)
			{
				Stats.Thoughts.RemoveAt(num);
			}
		}
	}

	public ThoughtData GetThought(Thought Thought)
	{
		foreach (ThoughtData thought in Stats.Thoughts)
		{
			if (thought.ThoughtType == Thought)
			{
				return thought;
			}
		}
		return null;
	}

	public bool HasTrait(FollowerTrait.TraitType TraitType)
	{
		if (_directInfoAccess.Traits.Contains(TraitType))
		{
			return true;
		}
		if (DataManager.Instance.CultTraits.Contains(TraitType))
		{
			return true;
		}
		return false;
	}

	public void AddTrait(FollowerTrait.TraitType TraitType, bool ShowNotification = true)
	{
		if (_directInfoAccess.Traits.Contains(TraitType))
		{
			return;
		}
		FollowerTrait.RemoveExclusiveTraits(this, TraitType);
		foreach (FollowerTrait.TraitType cultTrait in DataManager.Instance.CultTraits)
		{
			FollowerTrait.TraitType traitType = cultTrait;
			FollowerTrait.RemoveExclusiveTraits(this, TraitType);
		}
		switch (TraitType)
		{
		case FollowerTrait.TraitType.DesensitisedToDeath:
			RemoveThought(Thought.EnemyDied, true);
			RemoveThought(Thought.FriendDied, true);
			RemoveThought(Thought.LoverDied, true);
			RemoveThought(Thought.StrangerDied, true);
			RemoveThought(Thought.GrievedAtUnburiedBody, true);
			RemoveThought(Thought.GrievedAtRottenUnburiedBody, true);
			break;
		case FollowerTrait.TraitType.Cannibal:
			RemoveThought(Thought.AteFollowerMeal, true);
			RemoveThought(Thought.AteRottenFollowerMeal, true);
			break;
		case FollowerTrait.TraitType.FearOfDeath:
			RemoveThought(Thought.EnemyDied, true);
			RemoveThought(Thought.FriendDied, true);
			RemoveThought(Thought.LoverDied, true);
			RemoveThought(Thought.StrangerDied, true);
			RemoveThought(Thought.GrievedAtUnburiedBody, true);
			RemoveThought(Thought.GrievedAtRottenUnburiedBody, true);
			break;
		case FollowerTrait.TraitType.Disciplinarian:
			RemoveThought(Thought.InnocentImprisoned, true);
			RemoveThought(Thought.InnocentImprisonedSleeping, true);
			break;
		case FollowerTrait.TraitType.Libertarian:
			RemoveThought(Thought.InnocentImprisoned, true);
			RemoveThought(Thought.InnocentImprisonedSleeping, true);
			break;
		case FollowerTrait.TraitType.SacrificeEnthusiast:
			RemoveThought(Thought.CultMemberWasSacrificed, true);
			RemoveThought(Thought.CultMemberWasSacrificedAgainstSacrificeTrait, true);
			break;
		case FollowerTrait.TraitType.AgainstSacrifice:
			RemoveThought(Thought.CultMemberWasSacrificed, true);
			RemoveThought(Thought.CultMemberWasSacrificedSacrificeEnthusiastTrait, true);
			break;
		}
		_directInfoAccess.Traits.Add(TraitType);
		//TwitchFollowers.SendFollowerInformation(_directInfoAccess);
	}

	public void RemoveTrait(FollowerTrait.TraitType TraitType, bool ShowNotification)
	{
		if (_directInfoAccess.Traits.Contains(TraitType))
		{
			_directInfoAccess.Traits.Remove(TraitType);
		//	TwitchFollowers.SendFollowerInformation(_directInfoAccess);
		}
	}

	public bool ThoughtExists(Thought thought)
	{
		foreach (ThoughtData thought2 in Stats.Thoughts)
		{
			if (thought2.ThoughtType == thought)
			{
				return true;
			}
		}
		return false;
	}

	public bool ThoughtGroupExists(Thought thought)
	{
		foreach (ThoughtData thought2 in Stats.Thoughts)
		{
			if (thought2.ThoughtGroup == thought)
			{
				return true;
			}
		}
		return false;
	}

	public void Die(NotificationCentre.NotificationType deathNotificationType)
	{
		Debug.Log("Brain Die");
		Cleanup();
		ClearDwelling();
		FollowerManager.FollowerDie(Info.ID, deathNotificationType);
	}

	public void Leave(NotificationCentre.NotificationType leaveNotificationType)
	{
		FollowerManager.FollowerLeave(Info.ID, leaveNotificationType);
		Debug.Log("Follower Brain Leave()");
		NotificationCentre.Instance.PlayGenericNotificationNonLocalizedParams("Notifications/DissenterLeftCult", Info.Name, Stats.DissentGold.ToString());
		Cleanup();
		ClearDwelling();
		FollowerManager.RemoveFollower(Info.ID);
		FollowerManager.RemoveFollowerBrain(Info.ID);
	}

	public void AddAdoration(AdorationActions Action, Action Callback)
	{
		Follower follower = FollowerManager.FindFollowerByID(Info.ID);
		if (Info.XPLevel <= 1)
		{
			switch (Action)
			{
			case AdorationActions.Bless:
				Action = AdorationActions.BlessLvl1;
				break;
			case AdorationActions.Sermon:
				Action = AdorationActions.SermonLvl1;
				break;
			}
		}
		AddAdoration(follower, Action, Callback);
	}

	public void AddAdoration(Follower follower, AdorationActions Action, Action Callback)
	{
		if (Stats.HasLevelledUp || !DataManager.Instance.ShowLoyaltyBars)
		{
			if (Callback != null)
			{
				Callback();
			}
			return;
		}
		int num = AdorationsAndActions[Action];
		if (HasTrait(FollowerTrait.TraitType.Cynical))
		{
			num = Mathf.RoundToInt((float)num * 0.85f);
		}
		else if (HasTrait(FollowerTrait.TraitType.Gullible))
		{
			num = Mathf.RoundToInt((float)num * 1.15f);
		}
		if (Info.Necklace == InventoryItem.ITEM_TYPE.Necklace_Loyalty)
		{
			num = Mathf.RoundToInt((float)num * 1.25f);
		}
		float adoration = Stats.Adoration;
		Stats.Adoration += num;
		if ((bool)follower)
		{
			follower.AdorationUI.BarController.SetBarSize(adoration / Stats.MAX_ADORATION, false, true);
			follower.StartCoroutine(AddAdorationIE(follower, Action, Callback));
		}
		else if (Callback != null)
		{
			Callback();
		}
	}

	private IEnumerator AddAdorationIE(Follower follower, AdorationActions Action, Action Callback)
	{
		yield return follower.StartCoroutine(follower.AdorationUI.IncreaseAdorationIE());
		if (Stats.Adoration >= Stats.MAX_ADORATION)
		{
			yield return follower.StartCoroutine(follower.UpgradeToDiscipleRoutine());
		}
		if (Callback != null)
		{
			Callback();
		}
	}

	public bool GetWillLevelUp(AdorationActions Action)
	{
		int num = AdorationsAndActions[Action];
		if (HasTrait(FollowerTrait.TraitType.Cynical))
		{
			num = Mathf.RoundToInt((float)num * 0.85f);
		}
		else if (HasTrait(FollowerTrait.TraitType.Gullible))
		{
			num = Mathf.RoundToInt((float)num * 1.15f);
		}
		return Stats.Adoration + (float)num >= Stats.MAX_ADORATION;
	}

	public void GetXP(float Delta)
	{
	}

	public void LevelUp()
	{
		Action onPromotion = Info.OnPromotion;
		if (onPromotion != null)
		{
			onPromotion();
		}
		Info.XPLevel++;
		NotificationCentre.Instance.PlayFollowerNotification(NotificationCentre.NotificationType.LevelUp, Info, NotificationFollower.Animation.Happy);
		NotificationCentreScreen.Play(NotificationCentre.NotificationType.LevelUpCentreScreen);
	}

	public void LevelDown()
	{
		Info.XPLevel--;
	}

	public bool CheckForInteraction(FollowerBrain otherBrain)
	{
		if (Stats.Social <= 0f && (CurrentTask == null || !CurrentTask.BlockSocial) && Info.CursedState == Thought.None && otherBrain.Info.CursedState == Thought.None && otherBrain.Stats.Social <= 0f && (otherBrain.CurrentTask == null || (!otherBrain.CurrentTask.BlockReactTasks && !otherBrain.CurrentTask.BlockTaskChanges && !otherBrain.CurrentTask.BlockSocial)) && otherBrain.CurrentTask != null)
		{
			Debug.Log("=========================================");
			Debug.Log("otherBrain.CurrentTask.BlockSocial: " + otherBrain.CurrentTask.BlockSocial);
			if (CurrentTask != null)
			{
				Debug.Log("CurrentTask.BlockSocial: " + CurrentTask.BlockSocial);
			}
			Debug.Log(string.Concat(CurrentTaskType, "  ", otherBrain.CurrentTaskType));
			HardSwapToTask(new FollowerTask_Chat(otherBrain.Info.ID, true));
		}
		if (otherBrain.CurrentTask is FollowerTask_Dissent && otherBrain.CurrentTask.State == FollowerTaskState.Doing && TimeManager.TotalElapsedGameTime - StructureAndTime.GetOrAddTime(otherBrain.Info.ID, this, StructureAndTime.IDTypes.Follower) > 360f && CurrentTaskType != FollowerTaskType.DissentListen && !HasTrait(FollowerTrait.TraitType.Zealous))
		{
			StructureAndTime.SetTime(otherBrain.Info.ID, this, StructureAndTime.IDTypes.Follower);
			HardSwapToTask(new FollowerTask_DissentListen(otherBrain.Info.ID));
			return true;
		}
		if (TimeManager.CurrentPhase != DayPhase.Night && Info.FaithEnforcer && TimeManager.TotalElapsedGameTime - StructureAndTime.GetOrAddTime(otherBrain.Info.ID, this, StructureAndTime.IDTypes.Follower) > 360f && !otherBrain._directInfoAccess.FaithedToday && Vector3.Distance(LastPosition, otherBrain.LastPosition) < 5f && (CurrentTask == null || !(CurrentTask is FollowerTask_FaithEnforce)))
		{
			StructureAndTime.SetTime(otherBrain.Info.ID, this, StructureAndTime.IDTypes.Follower);
			HardSwapToTask(new FollowerTask_FaithEnforce(otherBrain));
			return true;
		}
		if (TimeManager.CurrentPhase != DayPhase.Night && Info.TaxEnforcer && TimeManager.TotalElapsedGameTime - StructureAndTime.GetOrAddTime(otherBrain.Info.ID, this, StructureAndTime.IDTypes.Follower) > 360f && !otherBrain._directInfoAccess.TaxedToday && Vector3.Distance(LastPosition, otherBrain.LastPosition) < 5f && (CurrentTask == null || !(CurrentTask is FollowerTask_TaxEnforce)))
		{
			StructureAndTime.SetTime(otherBrain.Info.ID, this, StructureAndTime.IDTypes.Follower);
			HardSwapToTask(new FollowerTask_TaxEnforce(otherBrain));
			return true;
		}
		return false;
	}

	public bool CheckForSpeakers(Structure structure)
	{
		if (structure.Type == StructureBrain.TYPES.PROPAGANDA_SPEAKER && structure.Structure_Info.Fuel > 0)
		{
			BoxCollider2D boxCollider2D = GameManager.GetInstance().GetComponent<BoxCollider2D>();
			if (boxCollider2D == null)
			{
				boxCollider2D = GameManager.GetInstance().gameObject.AddComponent<BoxCollider2D>();
				boxCollider2D.isTrigger = true;
			}
			boxCollider2D.size = Vector2.one * Structures_PropagandaSpeaker.EFFECTIVE_DISTANCE;
			boxCollider2D.transform.position = structure.Brain.Data.Position;
			boxCollider2D.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -45f));
			if (boxCollider2D.OverlapPoint(LastPosition))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckForInteraction(Structure structure, float CheckDistance)
	{
		StructureBrain brain = structure.Brain;
		if (TimeManager.IsNight || CurrentOverrideTaskType != 0)
		{
			return false;
		}
		switch (structure.Type)
		{
		case StructureBrain.TYPES.DEAD_WORSHIPPER:
			if (CurrentTaskType != FollowerTaskType.BuryBody && TimeManager.TotalElapsedGameTime - StructureAndTime.GetOrAddTime(brain.Data.ID, this, StructureAndTime.IDTypes.Structure) > 600f && GetTimeSinceTask(FollowerTaskType.ReactCorpse) > 60f && !brain.ReservedForTask)
			{
				StructureAndTime.SetTime(structure.Structure_Info.ID, this, StructureAndTime.IDTypes.Structure);
				HardSwapToTask(new FollowerTask_ReactCorpse(structure.Structure_Info.ID));
				return true;
			}
			break;
		case StructureBrain.TYPES.VOMIT:
			if (CurrentTaskType != FollowerTaskType.Ill && CurrentTaskType != FollowerTaskType.CleanWaste && GetTimeSinceTask(FollowerTaskType.ReactVomit) > 60f && TimeManager.TotalElapsedGameTime - StructureAndTime.GetOrAddTime(brain.Data.ID, this, StructureAndTime.IDTypes.Structure) > 600f)
			{
				StructureAndTime.SetTime(structure.Structure_Info.ID, this, StructureAndTime.IDTypes.Structure);
				HardSwapToTask(new FollowerTask_ReactVomit(structure.Structure_Info.ID));
				return true;
			}
			break;
		case StructureBrain.TYPES.GRAVE:
		case StructureBrain.TYPES.GRAVE2:
			if (brain.Data.FollowerID != -1 && CurrentTaskType != FollowerTaskType.BuryBody && GetTimeSinceTask(FollowerTaskType.ReactGrave) > 120f && TimeManager.TotalElapsedGameTime - StructureAndTime.GetOrAddTime(brain.Data.ID, this, StructureAndTime.IDTypes.Structure) > 1200f)
			{
				StructureAndTime.SetTime(structure.Structure_Info.ID, this, StructureAndTime.IDTypes.Structure);
				HardSwapToTask(new FollowerTask_ReactGrave(structure.Structure_Info.ID));
				return true;
			}
			break;
		case StructureBrain.TYPES.PRISON:
			if (brain.Data.FollowerID != -1 && brain.Data.FollowerID != _directInfoAccess.ID && CurrentTaskType != FollowerTaskType.ReactPrisoner && GetTimeSinceTask(FollowerTaskType.ReactPrisoner) > 120f && TimeManager.TotalElapsedGameTime - StructureAndTime.GetOrAddTime(brain.Data.ID, this, StructureAndTime.IDTypes.Structure) > 1200f)
			{
				StructureAndTime.SetTime(structure.Structure_Info.ID, this, StructureAndTime.IDTypes.Structure);
				HardSwapToTask(new FollowerTask_ReactPrisoner(structure.Structure_Info.ID));
				return true;
			}
			break;
		case StructureBrain.TYPES.POOP:
			if (structure.Structure_Info.FollowerID != Info.ID && CurrentTaskType != FollowerTaskType.CleanWaste && GetTimeSinceTask(FollowerTaskType.ReactPoop) > 60f && TimeManager.TotalElapsedGameTime - StructureAndTime.GetOrAddTime(brain.Data.ID, this, StructureAndTime.IDTypes.Structure) > 600f)
			{
				StructureAndTime.SetTime(structure.Structure_Info.ID, this, StructureAndTime.IDTypes.Structure);
				HardSwapToTask(new FollowerTask_ReactPoop(structure.Structure_Info.ID));
				return true;
			}
			break;
		case StructureBrain.TYPES.OUTHOUSE:
			if (brain.IsFull && GetTimeSinceTask(FollowerTaskType.ReactOuthouse) > 60f && TimeManager.TotalElapsedGameTime - StructureAndTime.GetOrAddTime(brain.Data.ID, this, StructureAndTime.IDTypes.Structure) > 600f)
			{
				StructureAndTime.SetTime(structure.Structure_Info.ID, this, StructureAndTime.IDTypes.Structure);
				HardSwapToTask(new FollowerTask_ReactOuthouse(structure.Structure_Info.ID));
				return true;
			}
			break;
		case StructureBrain.TYPES.SHRINE_PASSIVE:
		case StructureBrain.TYPES.SHRINE_PASSIVE_II:
		case StructureBrain.TYPES.SHRINE_PASSIVE_III:
			if (((Structures_Shrine_Passive)brain).SoulCount < ((Structures_Shrine_Passive)brain).SoulMax && ((Structures_Shrine_Passive)brain).PrayAvailable(structure.Type) && !brain.ReservedForTask && TimeManager.TotalElapsedGameTime - StructureAndTime.GetOrAddTime(brain.Data.ID, this, StructureAndTime.IDTypes.Structure) > 600f && Vector3.Distance(LastPosition, brain.Data.Position) < Structures_Shrine_Passive.Range(brain.Data.Type))
			{
				StructureAndTime.SetTime(structure.Structure_Info.ID, this, StructureAndTime.IDTypes.Structure);
				HardSwapToTask(new FollowerTask_PrayPassive(structure.Structure_Info.ID));
				return false;
			}
			break;
		case StructureBrain.TYPES.DECORATION_LAMB_STATUE:
		case StructureBrain.TYPES.DECORATION_FLAG_SCRIPTURE:
		case StructureBrain.TYPES.DECORATION_LAMB_FLAG_STATUE:
		case StructureBrain.TYPES.DECORATION_BELL_STATUE:
		case StructureBrain.TYPES.DECORATION_BONE_ARCH:
		case StructureBrain.TYPES.DECORATION_BONE_CANDLE:
		case StructureBrain.TYPES.DECORATION_BONE_FLAG:
		case StructureBrain.TYPES.DECORATION_BONE_LANTERN:
		case StructureBrain.TYPES.DECORATION_BONE_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_ROCK:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_STATUE:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_TREE:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_WINDOW:
		case StructureBrain.TYPES.DECORATION_FLOWER_ARCH:
		case StructureBrain.TYPES.DECORATION_FOUNTAIN:
		case StructureBrain.TYPES.DECORATION_BONE_SKULL_BIG:
		case StructureBrain.TYPES.DECORATION_BONE_SKULL_PILE:
		case StructureBrain.TYPES.DECORATION_LEAFY_FLOWER_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_LEAFY_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_LARGE:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_SPIDER_PILLAR:
		case StructureBrain.TYPES.DECORATION_SPIDER_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_SPIDER_WEB_CROWN_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_STONE_HENGE:
		case StructureBrain.TYPES.DECORATION_POND:
			if (GetTimeSinceTask(FollowerTaskType.ReactDecoration) > 120f && TimeManager.TotalElapsedGameTime - StructureAndTime.GetOrAddTime(brain.Data.ID, this, StructureAndTime.IDTypes.Structure) > 600f)
			{
				StructureAndTime.SetTime(structure.Structure_Info.ID, this, StructureAndTime.IDTypes.Structure);
				HardSwapToTask(new FollowerTask_ReactDecorations(structure.Structure_Info.ID));
				return true;
			}
			break;
		default:
			return false;
		}
		return false;
	}

	public bool CheckForSimInteraction(StructureBrain structureBrain)
	{
		if (structureBrain.Data.Type == StructureBrain.TYPES.PROPAGANDA_SPEAKER && structureBrain.Data.Fuel > 0 && Vector3.Distance(structureBrain.Data.Position, LastPosition) < Structures_PropagandaSpeaker.EFFECTIVE_DISTANCE)
		{
			SpeakersInRange++;
		}
		switch (structureBrain.Data.Type)
		{
		case StructureBrain.TYPES.SHRINE_PASSIVE:
		case StructureBrain.TYPES.SHRINE_PASSIVE_II:
		case StructureBrain.TYPES.SHRINE_PASSIVE_III:
			if (((Structures_Shrine_Passive)structureBrain).SoulCount < ((Structures_Shrine_Passive)structureBrain).SoulMax && ((Structures_Shrine_Passive)structureBrain).PrayAvailable(structureBrain.Data.Type) && !structureBrain.ReservedForTask && TimeManager.TotalElapsedGameTime - StructureAndTime.GetOrAddTime(structureBrain.Data.ID, this, StructureAndTime.IDTypes.Structure) > 600f && Vector3.Distance(LastPosition, structureBrain.Data.Position) < Structures_Shrine_Passive.Range(structureBrain.Data.Type))
			{
				StructureAndTime.SetTime(structureBrain.Data.ID, this, StructureAndTime.IDTypes.Structure);
				HardSwapToTask(new FollowerTask_PrayPassive(structureBrain.Data.ID));
				return true;
			}
			break;
		case StructureBrain.TYPES.DECORATION_LAMB_STATUE:
		case StructureBrain.TYPES.DECORATION_FLAG_SCRIPTURE:
		case StructureBrain.TYPES.DECORATION_LAMB_FLAG_STATUE:
		case StructureBrain.TYPES.DECORATION_BELL_STATUE:
		case StructureBrain.TYPES.DECORATION_BONE_ARCH:
		case StructureBrain.TYPES.DECORATION_BONE_CANDLE:
		case StructureBrain.TYPES.DECORATION_BONE_FLAG:
		case StructureBrain.TYPES.DECORATION_BONE_LANTERN:
		case StructureBrain.TYPES.DECORATION_BONE_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_ROCK:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_STATUE:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_TREE:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_WINDOW:
		case StructureBrain.TYPES.DECORATION_FLOWER_ARCH:
		case StructureBrain.TYPES.DECORATION_FOUNTAIN:
		case StructureBrain.TYPES.DECORATION_BONE_SKULL_BIG:
		case StructureBrain.TYPES.DECORATION_BONE_SKULL_PILE:
		case StructureBrain.TYPES.DECORATION_LEAFY_FLOWER_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_LEAFY_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_LARGE:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_SPIDER_PILLAR:
		case StructureBrain.TYPES.DECORATION_SPIDER_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_SPIDER_WEB_CROWN_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_STONE_HENGE:
		case StructureBrain.TYPES.DECORATION_POND:
			if (GetTimeSinceTask(FollowerTaskType.ReactDecoration) > 120f && TimeManager.TotalElapsedGameTime - StructureAndTime.GetOrAddTime(structureBrain.Data.ID, this, StructureAndTime.IDTypes.Structure) > 600f)
			{
				StructureAndTime.SetTime(structureBrain.Data.ID, this, StructureAndTime.IDTypes.Structure);
				HardSwapToTask(new FollowerTask_ReactDecorations(structureBrain.Data.ID));
				return true;
			}
			break;
		default:
			return false;
		}
		return false;
	}

	public Follower.ComplaintType GetMostPressingComplaint()
	{
		if (Info.FirstTimeSpeakingToPlayer)
		{
			return Follower.ComplaintType.FirstTimeSpeakingToPlayer;
		}
		Follower.ComplaintType result = Follower.ComplaintType.None;
		using (IEnumerator<Follower.ComplaintType> enumerator = GetOrderedComplaints().GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				result = enumerator.Current;
			}
		}
		return result;
	}

	public IEnumerable<Follower.ComplaintType> GetOrderedComplaints()
	{
		if (Stats.Illness > 0f)
		{
			yield return Follower.ComplaintType.Sick;
		}
		if (Stats.Satiation <= 30f)
		{
			yield return Follower.ComplaintType.Hunger;
		}
		if (!HasHome)
		{
			yield return Follower.ComplaintType.Homeless;
		}
		if (HasHome && !HomeIsCorrectLevel)
		{
			yield return Follower.ComplaintType.NeedBetterHouse;
		}
	}

	private void OnStructureAdded(StructuresData structure)
	{
		if (structure.Type == StructureBrain.TYPES.TEMPLE)
		{
			RemoveThought(Thought.NoTemple, true);
		}
		CheckChangeTask();
	}

	private void OnStructureMoved(StructuresData structure)
	{
		if (structure.ID == CurrentTaskUsingStructureID)
		{
			CurrentTask.Abort();
		}
	}

	private void OnStructureUpgraded(StructuresData structure)
	{
		if (structure.ID == CurrentTaskUsingStructureID)
		{
			CurrentTask.Abort();
		}
	}

	private void OnStructureRemoved(StructuresData structure)
	{
		if (structure.ID == CurrentTaskUsingStructureID)
		{
			CurrentTask.Abort();
		}
		if (structure.ID == HomeID)
		{
			ClearDwelling();
		}
		CheckChangeTask();
	}

	private void OnHappinessStateChanged(int followerId, FollowerStatState newState, FollowerStatState oldState)
	{
	}

	private void OnStarvationChanged(int followerId, FollowerStatState newState, FollowerStatState oldState)
	{
		if (followerId == Info.ID && newState != oldState)
		{
			switch (newState)
			{
			case FollowerStatState.On:
				AddThought(Thought.BecomeStarving);
				break;
			case FollowerStatState.Off:
				RemoveCurseState(Thought.BecomeStarving);
				RemoveThought(Thought.BecomeStarving, true);
				AddThought(Thought.NoLongerStarving);
				break;
			}
		}
	}

	public void MakeOld()
	{
		if (Info.CursedState == Thought.None)
		{
			ApplyCurseState(Thought.OldAge);
			if (Info.CursedState == Thought.OldAge)
			{
				Info.OldAge = true;
				Info.Outfit = FollowerOutfitType.Old;
				DataManager.Instance.LastFollowerToReachOldAge = TimeManager.TotalElapsedGameTime;
			}
		}
	}

	public void MakeExhausted()
	{
		if (!(Stats.Exhaustion > 0f))
		{
			Stats.Exhaustion = 50f;
			FollowerBrainStats.StatStateChangedEvent onExhaustionStateChanged = FollowerBrainStats.OnExhaustionStateChanged;
			if (onExhaustionStateChanged != null)
			{
				onExhaustionStateChanged(Info.ID, FollowerStatState.On, FollowerStatState.Off);
			}
			CheckChangeState();
			if (DataManager.Instance.WokeUpEveryoneDay == -3)
			{
				DataManager.Instance.WokeUpEveryoneDay = -2;
				CultFaithManager.AddThought(Thought.Cult_ExhaustedEveryone, -1, 1f);
			}
			else if (DataManager.Instance.WokeUpEveryoneDay == -1)
			{
				CultFaithManager.AddThought(Thought.Cult_ExhaustedFollower, Info.ID, 1f);
			}
		}
	}

	public void MakeSick()
	{
		if (Info.CursedState != 0)
		{
			return;
		}
		ApplyCurseState(Thought.Ill);
		foreach (FollowerBrain allBrain in AllBrains)
		{
			if (allBrain.Info.ID != Info.ID && !DataManager.Instance.Followers_Recruit.Contains(allBrain._directInfoAccess))
			{
				if (allBrain.HasTrait(FollowerTrait.TraitType.FearOfSickPeople))
				{
					CultFaithManager.AddThought(Thought.Cult_FearSick, allBrain.Info.ID, 1f);
				}
				else if (allBrain.HasTrait(FollowerTrait.TraitType.LoveOfSickPeople))
				{
					CultFaithManager.AddThought(Thought.Cult_LoveSick, allBrain.Info.ID, 1f);
				}
			}
		}
		Stats.Illness = 50f;
		FollowerBrainStats.StatStateChangedEvent onIllnessStateChanged = FollowerBrainStats.OnIllnessStateChanged;
		if (onIllnessStateChanged != null)
		{
			onIllnessStateChanged(Info.ID, FollowerStatState.On, FollowerStatState.Off);
		}
		CheckChangeState();
	}

	public void MakeStarve()
	{
		if (Info.CursedState == Thought.None && !HasTrait(FollowerTrait.TraitType.DontStarve))
		{
			ApplyCurseState(Thought.BecomeStarving);
			Stats.IsStarving = true;
			_directInfoAccess.Satiation = 0f;
			Stats.Starvation = 37.5f;
			FollowerBrainStats.StatStateChangedEvent onStarvationStateChanged = FollowerBrainStats.OnStarvationStateChanged;
			if (onStarvationStateChanged != null)
			{
				onStarvationStateChanged(Info.ID, FollowerStatState.On, FollowerStatState.Off);
			}
			NotificationCentre.Instance.PlayFollowerNotification(NotificationCentre.NotificationType.Starving, Info, NotificationFollower.Animation.Unhappy);
			CheckChangeState();
			CheckChangeTask();
		}
	}

	public void MakeDissenter()
	{
		if (Info.CursedState != 0 || Info.Necklace == InventoryItem.ITEM_TYPE.Necklace_Loyalty)
		{
			return;
		}
		ApplyCurseState(Thought.Dissenter);
		CheckChangeState();
		Follower follower = FollowerManager.FindFollowerByID(Info.ID);
		if ((object)follower != null)
		{
			follower.SetOutfit(Info.Outfit, false);
		}
		if (Info.CursedState == Thought.Dissenter)
		{
			FollowerBrainStats.StatStateChangedEvent onReeducationStateChanged = FollowerBrainStats.OnReeducationStateChanged;
			if (onReeducationStateChanged != null)
			{
				onReeducationStateChanged(Info.ID, FollowerStatState.On, FollowerStatState.Off);
			}
		}
	}

	private void OnExhaustionStateChanged(int followerId, FollowerStatState newState, FollowerStatState oldState)
	{
		if (followerId != Info.ID)
		{
			return;
		}
		if (newState != oldState)
		{
			switch (newState)
			{
			case FollowerStatState.On:
				Debug.Log("EXHAUSTION ON!!!");
				break;
			case FollowerStatState.Off:
				Debug.Log("EXHAUSTION OFF!!!");
				if (DataManager.Instance.WokeUpEveryoneDay == -4)
				{
					CultFaithManager.AddThought(Thought.Cult_NoLongerExhaustedEveryone, -1, 1f);
					DataManager.Instance.WokeUpEveryoneDay = -5;
				}
				else if (DataManager.Instance.WokeUpEveryoneDay == -1)
				{
					CultFaithManager.AddThought(Thought.Cult_NoLongerExhaustedFollower, followerId, 1f);
				}
				break;
			}
		}
		CheckChangeState();
	}

	private void OnIllnessStateChanged(int followerId, FollowerStatState newState, FollowerStatState oldState)
	{
		if (followerId != Info.ID)
		{
			return;
		}
		if (newState != oldState && newState == FollowerStatState.Off)
		{
			Debug.Log("ILLNESS OFF!!!");
			RemoveThought(Thought.Ill, true);
			RemoveCurseState(Thought.Ill);
			foreach (FollowerBrain allBrain in AllBrains)
			{
				allBrain.RemoveThought(Thought.SomeoneIllGermophobe, false);
				allBrain.RemoveThought(Thought.SomeoneIllLoveOfSickTrait, false);
			}
			NotificationCentre.Instance.PlayFollowerNotification(NotificationCentre.NotificationType.NoLongerIll, Info, NotificationFollower.Animation.Happy);
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.FollowerRecoverIllness, followerId);
		}
		CheckChangeState();
	}

	private void OnSatiationStateChanged(int followerID, FollowerStatState newState, FollowerStatState oldState)
	{
		if (followerID == Info.ID)
		{
			CheckChangeTask();
		}
	}

	private void OnStarvationStateChanged(int followerID, FollowerStatState newState, FollowerStatState oldState)
	{
		if (followerID == Info.ID)
		{
			CheckChangeTask();
		}
	}

	private void OnRestStateChanged(int followerID, FollowerStatState newState, FollowerStatState oldState)
	{
		if (followerID == Info.ID)
		{
			CheckChangeState();
			CheckChangeTask();
		}
	}

	private void OnBathroomStateChanged(int followerID, FollowerStatState newState, FollowerStatState oldState)
	{
		if (followerID == Info.ID)
		{
			CheckChangeTask();
		}
	}

	private void OnMotivatedChanged(int followerID)
	{
		if (followerID == Info.ID)
		{
			CheckChangeState();
		}
	}

	private void OnResearchBegin()
	{
		CheckChangeTask();
	}

	private void OnNewPhaseStarted()
	{
		CheckChangeTask();
		if (TimeManager.CurrentPhase == DayPhase.Night)
		{
			if (DataManager.Instance.WokeUpEveryoneDay == -2)
			{
				DataManager.Instance.WokeUpEveryoneDay = -4;
			}
			else if (DataManager.Instance.WokeUpEveryoneDay == -5)
			{
				DataManager.Instance.WokeUpEveryoneDay = -1;
			}
		}
		else if (TimeManager.CurrentPhase == DayPhase.Afternoon)
		{
			_directInfoAccess.WakeUpDay = -1;
		}
	}

	private void OnScheduleChanged()
	{
		CheckChangeTask();
	}

	public float GetHungerScore()
	{
		float result = 0f;
		if (!FollowerBrainStats.Fasting)
		{
			result = (float)Mathf.FloorToInt(Stats.Starvation + (100f - Stats.Satiation)) + (float)Info.ID / 1000f;
		}
		return result;
	}

	public float GetHungerScoreNotStarving()
	{
		float result = 0f;
		if (!Stats.IsStarving && !FollowerBrainStats.Fasting && CurrentTaskType != FollowerTaskType.EatMeal && CurrentTaskType != FollowerTaskType.EatStoredFood)
		{
			result = (float)Mathf.FloorToInt(Stats.Starvation + (100f - Stats.Satiation)) + (float)Info.ID / 1000f;
		}
		return result;
	}

	public float GetHungerScoreNoCursedState()
	{
		float result = 0f;
		if (Info.CursedState == Thought.None && !FollowerBrainStats.Fasting && CurrentTaskType != FollowerTaskType.EatMeal && CurrentTaskType != FollowerTaskType.EatStoredFood)
		{
			result = (float)Mathf.FloorToInt(Stats.Starvation + (100f - Stats.Satiation)) + (float)Info.ID / 1000f;
		}
		return result;
	}

	public void CheckChangeState()
	{
		FollowerState followerState = GetFollowerState();
		if (followerState != null)
		{
			CurrentState = followerState;
		}
	}

	private FollowerState GetFollowerState()
	{
		if (Info.CursedState == Thought.OldAge)
		{
			return new FollowerState_OldAge();
		}
		if (Info.CursedState == Thought.Ill)
		{
			return new FollowerState_Ill();
		}
		if (FollowingPlayer)
		{
			return new FollowerState_Following();
		}
		if (InRitual)
		{
			return new FollowerState_Ritual();
		}
		if (Stats.Exhaustion > 0f)
		{
			return new FollowerState_Exhausted();
		}
		if (Stats.Motivated)
		{
			return new FollowerState_Motivated();
		}
		return new FollowerState_Default();
	}

	public void CheckChangeTask()
	{
		if (CurrentTask == null || !CurrentTask.BlockTaskChanges)
		{
			ShouldReconsiderTask = true;
		}
	}

	public bool BeginReconsider()
	{
		FollowerTask currentTask = _currentTask;
		if (currentTask != null)
		{
			currentTask.ReleaseReservations();
		}
		FollowerTask nextTask = _nextTask;
		if (nextTask != null)
		{
			nextTask.ReleaseReservations();
		}
		FollowerTask personalTask = GetPersonalTask(HomeLocation);
		if (personalTask != null)
		{
			FollowerTask currentTask2 = _currentTask;
			if (currentTask2 != null)
			{
				currentTask2.ClaimReservations();
			}
			FollowerTask nextTask2 = _nextTask;
			if (nextTask2 != null)
			{
				nextTask2.ClaimReservations();
			}
			FollowerTaskType followerTaskType = ((CurrentTaskType != FollowerTaskType.ChangeLocation) ? CurrentTaskType : ((FollowerTask_ChangeLocation)CurrentTask).ParentType);
			if (personalTask.Type != followerTaskType)
			{
				TransitionToTask(personalTask);
			}
			ShouldReconsiderTask = false;
		}
		else
		{
			PendingTask = default(PendingTaskData);
		}
		return ShouldReconsiderTask;
	}

	public int TryClaimExistingTask(List<FollowerTask> availableTasks)
	{
		if (CurrentTask != null)
		{
			for (int i = 0; i < availableTasks.Count; i++)
			{
				FollowerTask followerTask = availableTasks[i];
				if (followerTask != null && followerTask.GetUniqueTaskCode() == CurrentTask.GetUniqueTaskCode())
				{
					PendingTask.KeepExistingTask = true;
					PendingTask.Task = followerTask;
					PendingTask.ListIndex = i;
					availableTasks[i] = null;
					return i;
				}
			}
		}
		return -1;
	}

	public int ClaimNextAvailableTask(List<FollowerTask> availableTasks)
	{
		for (int i = 0; i < 5; i++)
		{
			for (int j = 0; j < availableTasks.Count; j++)
			{
				FollowerTask followerTask = availableTasks[j];
				if (followerTask != null && FollowerTask.RequiredFollowerLevel(Info.FollowerRole, followerTask.Type) && followerTask.GetPriorityCategory(Info.FollowerRole, Info.WorkerPriority, this) == (PriorityCategory)i)
				{
					PendingTask.KeepExistingTask = false;
					PendingTask.Task = followerTask;
					PendingTask.ListIndex = j;
					availableTasks[j] = null;
					return j;
				}
			}
		}
		return -1;
	}

	public void EndReconsider()
	{
		FollowerTask currentTask = _currentTask;
		if (currentTask != null)
		{
			currentTask.ClaimReservations();
		}
		FollowerTask nextTask = _nextTask;
		if (nextTask != null)
		{
			nextTask.ClaimReservations();
		}
		if (PendingTask.Task != null)
		{
			if (!PendingTask.KeepExistingTask)
			{
				TransitionToTask(PendingTask.Task);
			}
		}
		else
		{
			ScheduledActivity scheduledActivity = TimeManager.GetScheduledActivity(HomeLocation);
			FollowerTask fallbackTask = GetFallbackTask(scheduledActivity);
			if (CurrentTaskType != fallbackTask.Type)
			{
				TransitionToTask(fallbackTask);
			}
		}
		ShouldReconsiderTask = false;
	}

	public void CompleteCurrentTask()
	{
		TransitionToTask(null);
	}

	public void TransitionToTask(FollowerTask nextTask)
	{
		if (nextTask != null)
		{
			nextTask.Init(this);
		}
		if (CurrentTask == null)
		{
			CurrentTask = nextTask;
			return;
		}
		FollowerTask nextTask2 = _nextTask;
		if (nextTask2 != null)
		{
			nextTask2.ReleaseReservations();
		}
		_nextTask = nextTask;
		FollowerTask nextTask3 = _nextTask;
		if (nextTask3 != null)
		{
			nextTask3.ClaimReservations();
		}
		CurrentTask.End();
	}

	public void HardSwapToTask(FollowerTask nextTask)
	{
		if (nextTask != null)
		{
			nextTask.Init(this);
		}
		if (CurrentTask == null)
		{
			CurrentTask = nextTask;
			return;
		}
		FollowerTask nextTask2 = _nextTask;
		if (nextTask2 != null)
		{
			nextTask2.ReleaseReservations();
		}
		_nextTask = nextTask;
		FollowerTask nextTask3 = _nextTask;
		if (nextTask3 != null)
		{
			nextTask3.ClaimReservations();
		}
		CurrentTask.Abort();
	}

	public void ContinueToNextTask()
	{
		FollowerTask nextTask = _nextTask;
		_nextTask = null;
		if (nextTask != null)
		{
			nextTask.ReleaseReservations();
		}
		CurrentTask = nextTask;
	}

	public void SetPersonalOverrideTask(FollowerTaskType Type, StructureBrain.TYPES OverrideStructureType = StructureBrain.TYPES.NONE)
	{
		CompleteCurrentTask();
		OverrideTaskCompleted = false;
		OverrideDayIndex = TimeManager.CurrentDay;
		ClearPersonalOverrideTaskProvider();
		CurrentOverrideTaskType = Type;
		CurrentOverrideStructureType = OverrideStructureType;
		CheckChangeTask();
	}

	public void ClearPersonalOverrideTaskProvider()
	{
		CurrentOverrideTaskType = FollowerTaskType.None;
	}

	private bool CheckOverrideComplete()
	{
		switch (CurrentOverrideTaskType)
		{
		case FollowerTaskType.Sleep:
			return TimeManager.CurrentDay > OverrideDayIndex;
		case FollowerTaskType.EatMeal:
			if (Stats.Satiation > 60f)
			{
				return TimeManager.CurrentDay > OverrideDayIndex;
			}
			return false;
		default:
			return false;
		}
	}

	public FollowerTask GetPersonalOverrideTask()
	{
		FollowerTask result = null;
		if (CurrentOverrideTaskType != 0)
		{
			if (!CheckOverrideComplete())
			{
				switch (CurrentOverrideTaskType)
				{
				case FollowerTaskType.Sleep:
					if (Info.CursedState == Thought.Ill && CurrentTaskType != FollowerTaskType.ClaimDwelling)
					{
						return new FollowerTask_SleepBedRest();
					}
					return new FollowerTask_Sleep();
				case FollowerTaskType.SleepBedRest:
					if (CurrentTaskType != FollowerTaskType.ClaimDwelling)
					{
						return new FollowerTask_SleepBedRest();
					}
					return null;
				case FollowerTaskType.EatMeal:
					return CheckEatTask(true);
				default:
					return null;
				}
			}
			ClearPersonalOverrideTaskProvider();
		}
		return result;
	}

	public float GetTimeSinceTask(FollowerTaskType taskType)
	{
		float result = 0f;
		if (CurrentTaskType != taskType)
		{
			result = TimeManager.TotalElapsedGameTime - TaskAndTime.GetLastTaskTime(taskType, this);
		}
		return result;
	}

	public FollowerTask GetPersonalTask(FollowerLocation location)
	{
		if (LeavingCult)
		{
			return new FollowerTask_LeaveCult(NotificationCentre.NotificationType.LeaveCultUnhappy);
		}
		if (Info.CursedState != Thought.Zombie)
		{
			if (DiedOfStarvation)
			{
				return new FollowerTask_FindPlaceToDie(NotificationCentre.NotificationType.DiedFromStarvation);
			}
			if (DiedOfIllness)
			{
				return new FollowerTask_FindPlaceToDie(NotificationCentre.NotificationType.DiedFromIllness);
			}
			if (DiedOfOldAge)
			{
				return new FollowerTask_FindPlaceToDie(NotificationCentre.NotificationType.DiedFromOldAge);
			}
		}
		ScheduledActivity scheduledActivity = TimeManager.GetScheduledActivity(location);
		if (scheduledActivity == ScheduledActivity.Sleep && _directInfoAccess.WorkThroughNight)
		{
			scheduledActivity = ScheduledActivity.Work;
		}
		if (scheduledActivity == ScheduledActivity.Work && FollowerBrainStats.IsHoliday)
		{
			scheduledActivity = ScheduledActivity.Leisure;
		}
		FollowerTask stateTask = GetStateTask(location);
		if (stateTask != null)
		{
			FollowingPlayer = false;
			return stateTask;
		}
		if (Stats.Exhaustion > 0f && (TimeManager.TotalElapsedGameTime - DataManager.Instance.LastFollowerToPassOut > 1200f || Stats.Exhaustion >= 100f) && UnityEngine.Random.value < 0.025f)
		{
			DataManager.Instance.LastFollowerToPassOut = TimeManager.TotalElapsedGameTime;
			return new FollowerTask_Sleep(true);
		}
		if (FollowingPlayer)
		{
			return new FollowerTask_FollowPlayer();
		}
		if (CurrentTaskType == FollowerTaskType.ManualControl)
		{
			return new FollowerTask_ManualControl();
		}
		FollowerTask personalOverrideTask = GetPersonalOverrideTask();
		if (personalOverrideTask != null)
		{
			return personalOverrideTask;
		}
		if (!Stats.WorkerBeenGivenOrders && Info.FollowerRole == FollowerRole.Worker && scheduledActivity != ScheduledActivity.Sleep)
		{
			return new FollowerTask_AwaitInstruction();
		}
		if (Stats.CachedLumber > 0 && CurrentTaskType != FollowerTaskType.Lumberjack)
		{
			return new FollowerTask_DepositWood(Stats.CachedLumberjackStationID);
		}
		personalOverrideTask = TimeManager.GetOverrideTask(this);
		if (personalOverrideTask != null)
		{
			return personalOverrideTask;
		}
		if (!HasHome && (scheduledActivity == ScheduledActivity.Work || scheduledActivity == ScheduledActivity.Sleep || scheduledActivity == ScheduledActivity.Leisure || CurrentTaskType == FollowerTaskType.SleepBedRest))
		{
			Dwelling.DwellingAndSlot freeDwellingAndSlot = StructureManager.GetFreeDwellingAndSlot(location, _directInfoAccess);
			if (freeDwellingAndSlot != null && !StructureManager.GetStructureByID<Structures_Bed>(freeDwellingAndSlot.ID).ReservedForTask)
			{
				AssignDwelling(freeDwellingAndSlot, Info.ID, false);
				StructureManager.GetStructureByID<Structures_Bed>(freeDwellingAndSlot.ID).ReservedForTask = true;
				return new FollowerTask_ClaimDwelling(freeDwellingAndSlot);
			}
		}
		if (HasHome && (scheduledActivity == ScheduledActivity.Work || scheduledActivity == ScheduledActivity.Sleep || scheduledActivity == ScheduledActivity.Leisure))
		{
			Structures_Bed structureByID = StructureManager.GetStructureByID<Structures_Bed>(_directInfoAccess.DwellingID);
			if (structureByID != null)
			{
				if (!structureByID.Data.FollowersClaimedSlots.Contains(Info.ID))
				{
					Dwelling.DwellingAndSlot dwellingAndSlot = new Dwelling.DwellingAndSlot(_directInfoAccess.DwellingID, _directInfoAccess.DwellingSlot, _directInfoAccess.DwellingLevel);
					AssignDwelling(dwellingAndSlot, Info.ID, false);
					StructureManager.GetStructureByID<Structures_Bed>(dwellingAndSlot.ID).ReservedForTask = true;
					return new FollowerTask_ClaimDwelling(dwellingAndSlot);
				}
				if (structureByID.IsCollapsed)
				{
					Dwelling.DwellingAndSlot freeDwellingAndSlot2 = StructureManager.GetFreeDwellingAndSlot(location, _directInfoAccess);
					if (freeDwellingAndSlot2 != null && !StructureManager.GetStructureByID<Structures_Bed>(freeDwellingAndSlot2.ID).ReservedForTask)
					{
						ClearDwelling();
						AssignDwelling(freeDwellingAndSlot2, Info.ID, true);
						StructureManager.GetStructureByID<Structures_Bed>(freeDwellingAndSlot2.ID).ReservedForTask = true;
						return new FollowerTask_ClaimDwelling(freeDwellingAndSlot2);
					}
				}
			}
			else
			{
				_directInfoAccess.DwellingID = Dwelling.NO_HOME;
			}
		}
		if (Info.CursedState != 0 && (CurrentTask == null || !(CurrentTask is FollowerTask_Imprisoned)))
		{
			switch (Info.CursedState)
			{
			case Thought.OldAge:
				if (scheduledActivity != ScheduledActivity.Sleep)
				{
					return new FollowerTask_OldAge();
				}
				break;
			case Thought.Zombie:
				if (scheduledActivity != ScheduledActivity.Sleep)
				{
					return new FollowerTask_Zombie();
				}
				break;
			case Thought.Dissenter:
				if (scheduledActivity != ScheduledActivity.Sleep)
				{
					return new FollowerTask_Dissent();
				}
				break;
			case Thought.BecomeStarving:
				if (scheduledActivity != ScheduledActivity.Sleep)
				{
					return new FollowerTask_Starving();
				}
				break;
			case Thought.Ill:
				if (Stats.Illness == 0f)
				{
					RemoveThought(Thought.Ill, true);
					RemoveCurseState(Thought.Ill);
					return null;
				}
				if (scheduledActivity != ScheduledActivity.Sleep)
				{
					if (_directInfoAccess.CursedStateVariant == 0)
					{
						return new FollowerTask_Ill();
					}
					if (_directInfoAccess.CursedStateVariant == 1)
					{
						return new FollowerTask_IllPoopy();
					}
				}
				break;
			}
		}
		return null;
	}

	public static List<FollowerTask> GetTopPriorityFollowerTasks(FollowerLocation location, out ScheduledActivity selectedActivity)
	{
		selectedActivity = TimeManager.GetScheduledActivity(location);
		if (FollowerBrainStats.IsHoliday)
		{
			selectedActivity = ScheduledActivity.Leisure;
		}
		if (FollowerBrainStats.IsWorkThroughTheNight)
		{
			selectedActivity = ScheduledActivity.Work;
		}
		return GetTopPriorityFollowerTasks(selectedActivity, location);
	}

	public static List<FollowerTask> GetTopPriorityFollowerTasks(ScheduledActivity activity, FollowerLocation location)
	{
		switch (activity)
		{
		case ScheduledActivity.Work:
			return GetDesiredTask_Work(location);
		case ScheduledActivity.Study:
			return GetDesiredTask_Study(location);
		case ScheduledActivity.Sleep:
			return GetDesiredTask_Sleep();
		case ScheduledActivity.Pray:
			return GetDesiredTask_Pray();
		case ScheduledActivity.Leisure:
			return GetDesiredTask_Leisure();
		default:
			throw new ArgumentException(string.Format("Unrecognised ScheduledActivity.{0}", activity));
		}
	}

	private FollowerTask GetStateTask(FollowerLocation location)
	{
		if (Stats.Bathroom > 15f && !TimeManager.IsNight && CurrentTaskType != FollowerTaskType.Bathroom && CurrentOverrideTaskType == FollowerTaskType.None && CurrentTaskType != FollowerTaskType.Sleep)
		{
			if (bathroomOffset == -1f)
			{
				bathroomOffset = TimeManager.TotalElapsedGameTime + (float)UnityEngine.Random.Range(0, 240);
			}
			else if (TimeManager.TotalElapsedGameTime > bathroomOffset)
			{
				bathroomOffset = -1f;
				List<Structures_Outhouse> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_Outhouse>(location);
				if (allStructuresOfType.Count > 0)
				{
					foreach (Structures_Outhouse item in allStructuresOfType)
					{
						if (!item.ReservedForTask && !item.IsFull)
						{
							return new FollowerTask_Bathroom(item.Data.ID);
						}
					}
					return new FollowerTask_Bathroom(allStructuresOfType[UnityEngine.Random.Range(0, allStructuresOfType.Count)].Data.ID);
				}
				return new FollowerTask_Bathroom();
			}
		}
		FollowerTask followerTask = CheckEatTask();
		if (followerTask != null)
		{
			return followerTask;
		}
		return null;
	}

	public void ApplyCurseState(Thought Curse, Thought SpecialThought = Thought.None)
	{
		if ((CurrentTask != null && CurrentTask.BlockThoughts) || (Curse == Thought.OldAge && Info.HasTrait(FollowerTrait.TraitType.Immortal)) || Info.CursedState != 0)
		{
			return;
		}
		Info.CursedState = Curse;
		switch (Curse)
		{
		case Thought.Dissenter:
		{
			RemoveThought(Thought.NoLongerDissenting, true);
			AddThought(Thought.Dissenter, true);
			if (NotificationCentre.Instance != null)
			{
				NotificationCentre.Instance.PlayFollowerNotification(NotificationCentre.NotificationType.BecomeDissenter, Info, NotificationFollower.Animation.Dissenting);
			}
			Stats.Reeducation = 50f;
			Stats.GivenDissentWarning = false;
			foreach (FollowerBrain allBrain in AllBrains)
			{
				if (allBrain != this && allBrain.HasTrait(FollowerTrait.TraitType.Zealous))
				{
					allBrain.AddThought(Thought.SomeoneDissenterZealotTrait);
				}
			}
			Action onBecomeDissenter = OnBecomeDissenter;
			if (onBecomeDissenter != null)
			{
				onBecomeDissenter();
			}
			break;
		}
		case Thought.Ill:
			AddThought(Thought.Ill, true);
			Stats.Illness = 50f;
			if (SpecialThought == Thought.Cult_FollowerBecameIllSleepingNextToIllFollower)
			{
				if (HasTrait(FollowerTrait.TraitType.Germophobe))
				{
					CultFaithManager.AddThought(Thought.Cult_FollowerBecameIllSleepingNextToIllFollower_Germophobe, Info.ID, 1f);
				}
				else if (HasTrait(FollowerTrait.TraitType.Coprophiliac))
				{
					CultFaithManager.AddThought(Thought.Cult_FollowerBecameIllSleepingNextToIllFollower_Coprophiliac, Info.ID, 1f);
				}
				else
				{
					CultFaithManager.AddThought(Thought.Cult_FollowerBecameIllSleepingNextToIllFollower, Info.ID, 1f);
				}
			}
			else if (HasTrait(FollowerTrait.TraitType.Germophobe))
			{
				CultFaithManager.AddThought(Thought.Cult_GermophobeBecameSick, Info.ID, 1f);
			}
			else if (HasTrait(FollowerTrait.TraitType.Coprophiliac))
			{
				CultFaithManager.AddThought(Thought.Cult_CoprophiliacBecameSick, Info.ID, 1f);
			}
			else
			{
				NotificationCentre.Instance.PlayFollowerNotification(NotificationCentre.NotificationType.BecomeIll, Info, NotificationFollower.Animation.Sick);
			}
			_directInfoAccess.CursedStateVariant = UnityEngine.Random.Range(0, 2);
			break;
		case Thought.BecomeStarving:
			AddThought(Thought.BecomeStarving, true);
			break;
		case Thought.Zombie:
			NotificationCentre.Instance.PlayFollowerNotification(NotificationCentre.NotificationType.ZombieSpawned, Info, NotificationFollower.Animation.Unhappy);
			break;
		case Thought.OldAge:
			AddThought(Thought.OldAge, true);
			DataManager.Instance.Followers_Elderly_IDs.Add(Info.ID);
			if (DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.LoveElderly))
			{
				CultFaithManager.AddThought(Thought.Cult_LoveElderly_Trait, Info.ID, 1f);
			}
			else
			{
				CultFaithManager.AddThought(Thought.Cult_FollowerReachedOldAge, Info.ID, 1f);
			}
			break;
		}
		CheckChangeState();
		if (CurrentTask == null || CurrentTask.Type != FollowerTaskType.Imprisoned)
		{
			CompleteCurrentTask();
		}
	}

	public void RemoveCurseState(Thought Thought)
	{
		Thought cursedState = Info.CursedState;
		if (Info.CursedState == Thought)
		{
			Info.CursedState = Thought.None;
		}
		if (!ThoughtExists(Thought))
		{
			return;
		}
		switch (Thought)
		{
		case Thought.Dissenter:
			RemoveThought(Thought.Dissenter, true);
			LeavingCult = false;
			AddThought(Thought.NoLongerDissenting);
			Stats.Reeducation = 0f;
			foreach (FollowerBrain allBrain in AllBrains)
			{
				if (allBrain != this)
				{
					allBrain.RemoveThought(Thought.SomeoneDissenterZealotTrait, false);
				}
			}
			break;
		case Thought.Ill:
			Stats.Illness = 0f;
			if (cursedState == Thought.Ill)
			{
				FollowerBrainStats.StatStateChangedEvent onIllnessStateChanged = FollowerBrainStats.OnIllnessStateChanged;
				if (onIllnessStateChanged != null)
				{
					onIllnessStateChanged(Info.ID, FollowerStatState.Off, FollowerStatState.On);
				}
			}
			CompleteCurrentTask();
			break;
		}
		if (ThoughtExists(Thought.OldAge))
		{
			HardSwapToTask(new FollowerTask_OldAge());
		}
		CursedEvent onCursedStateRemoved = this.OnCursedStateRemoved;
		if (onCursedStateRemoved != null)
		{
			onCursedStateRemoved();
		}
	}

	private FollowerTask GetFallbackTask(ScheduledActivity activity)
	{
		if (CurrentTaskType == FollowerTaskType.ManualControl)
		{
			return new FollowerTask_ManualControl();
		}
		if (Info.CursedState != 0 && _directInfoAccess.WorkThroughNight && activity == ScheduledActivity.Sleep)
		{
			return GetPersonalTask(Location);
		}
		if (activity == ScheduledActivity.Sleep && !FollowerBrainStats.IsHoliday && _directInfoAccess.WorkThroughNight)
		{
			activity = ScheduledActivity.Work;
		}
		if (FollowerBrainStats.IsHoliday && activity != ScheduledActivity.Sleep)
		{
			activity = ScheduledActivity.Leisure;
		}
		switch (activity)
		{
		case ScheduledActivity.Work:
		{
			Structures_Shrine structures_Shrine2 = null;
			List<Structures_Shrine> allStructuresOfType2 = StructureManager.GetAllStructuresOfType<Structures_Shrine>(HomeLocation);
			if (allStructuresOfType2.Count > 0)
			{
				structures_Shrine2 = allStructuresOfType2[0];
			}
			if (structures_Shrine2 != null && structures_Shrine2.Prayers.Count < structures_Shrine2.PrayersMax && structures_Shrine2.SoulCount < structures_Shrine2.SoulMax)
			{
				return new FollowerTask_Pray(structures_Shrine2.Data.ID);
			}
			return new FollowerTask_FakeLeisure();
		}
		case ScheduledActivity.Study:
			return new FollowerTask_Study(StructureManager.GetAllStructuresOfType<Structures_Temple>(HomeLocation)[0].Data.ID);
		case ScheduledActivity.Sleep:
			if (Info.CursedState == Thought.Ill)
			{
				return new FollowerTask_SleepBedRest();
			}
			return new FollowerTask_Sleep();
		case ScheduledActivity.Pray:
		{
			Structures_Shrine structures_Shrine = null;
			List<Structures_Shrine> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_Shrine>(HomeLocation);
			if (allStructuresOfType.Count > 0)
			{
				structures_Shrine = allStructuresOfType[0];
			}
			if (structures_Shrine != null && structures_Shrine.Prayers.Count < structures_Shrine.PrayersMax && structures_Shrine.SoulCount < structures_Shrine.SoulMax)
			{
				return new FollowerTask_Pray(structures_Shrine.Data.ID);
			}
			return new FollowerTask_Idle();
		}
		case ScheduledActivity.Leisure:
			return new FollowerTask_FakeLeisure();
		default:
			throw new ArgumentException(string.Format("Unrecognised ScheduledActivity.{0}", activity));
		}
	}

	public void NewRoleSet(FollowerRole followerRole)
	{
		FollowerTaskType followerTaskFromRole = FollowerTask.GetFollowerTaskFromRole(followerRole);
		List<FollowerTask> allUnoccupiedTasksOfType = GetAllUnoccupiedTasksOfType(followerTaskFromRole);
		List<FollowerTask> allOccupiedTasksOfType = GetAllOccupiedTasksOfType(followerTaskFromRole);
		Info.FollowerRole = followerRole;
		if (allUnoccupiedTasksOfType.Count <= 0 && allOccupiedTasksOfType.Count > 0)
		{
			FollowerTask followerTask = allOccupiedTasksOfType[UnityEngine.Random.Range(0, allOccupiedTasksOfType.Count)];
			followerTask.Brain.Info.FollowerRole = FollowerRole.Worker;
			followerTask.Abort();
		}
	}

	public void CancelTargetedMeal(StructureBrain.TYPES mealType)
	{
		int num = 0;
		foreach (Structures_Meal item in StructureManager.GetAllStructuresOfType<Structures_Meal>())
		{
			if (item.Data.Type == mealType)
			{
				num++;
			}
		}
		foreach (Structures_Kitchen item2 in StructureManager.GetAllStructuresOfType<Structures_Kitchen>(Location))
		{
			foreach (InventoryItem item3 in item2.FoodStorage.Data.Inventory)
			{
				if (item3.type == (int)CookingData.GetMealFromStructureType(mealType))
				{
					num++;
				}
			}
		}
		int num2 = 0;
		List<FollowerTask> allOccupiedTasksOfType = GetAllOccupiedTasksOfType(FollowerTaskType.EatMeal);
		allOccupiedTasksOfType.AddRange(GetAllOccupiedTasksOfType(FollowerTaskType.EatStoredFood));
		foreach (FollowerTask item4 in allOccupiedTasksOfType)
		{
			if ((item4.Type == FollowerTaskType.EatMeal && ((FollowerTask_EatMeal)item4).MealType == mealType) || (item4.Type == FollowerTaskType.EatStoredFood && ((FollowerTask_EatStoredFood)item4)._foodType == CookingData.GetMealFromStructureType(mealType)))
			{
				num2++;
			}
		}
		if (num2 < num)
		{
			return;
		}
		for (int i = 0; i < allOccupiedTasksOfType.Count; i++)
		{
			if ((allOccupiedTasksOfType[i].Type == FollowerTaskType.EatMeal || allOccupiedTasksOfType[i].Type == FollowerTaskType.EatStoredFood) && allOccupiedTasksOfType[i].State != FollowerTaskState.Doing)
			{
				FollowerBrain brain = allOccupiedTasksOfType[0].Brain;
				allOccupiedTasksOfType[0].Abort();
				brain.HardSwapToTask(new FollowerTask_FakeLeisure());
				break;
			}
		}
	}

	public static List<FollowerTask> GetDesiredTask_Work(FollowerLocation location)
	{
		SortedList<float, FollowerTask> sortedList = new SortedList<float, FollowerTask>(new DuplicateKeyComparer<float>());
		List<StructureBrain> list = StructureManager.StructuresAtLocation(location);
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			if (list[i] is ITaskProvider)
			{
				((ITaskProvider)list[i]).GetAvailableTasks(ScheduledActivity.Work, sortedList);
			}
		}
		return new List<FollowerTask>(sortedList.Values);
	}

	public static bool IsTaskAvailable(FollowerTaskType taskType)
	{
		List<FollowerTask> list = new List<FollowerTask>(GetDesiredTask_Work(FollowerLocation.Base));
		foreach (FollowerBrain allBrain in AllBrains)
		{
			if (allBrain.CurrentTask != null)
			{
				list.Add(allBrain.CurrentTask);
			}
		}
		foreach (FollowerTask item in list)
		{
			if (item.Type == taskType)
			{
				return true;
			}
		}
		return false;
	}

	public static List<FollowerTask> GetAllUnoccupiedTasksOfType(FollowerTaskType taskType)
	{
		List<FollowerTask> list = new List<FollowerTask>(GetDesiredTask_Work(FollowerLocation.Base));
		List<FollowerTask> list2 = new List<FollowerTask>();
		foreach (FollowerTask item in list)
		{
			if (item.Type == taskType)
			{
				list2.Add(item);
			}
		}
		return list2;
	}

	private static List<FollowerTask> GetAllOccupiedTasksOfType(FollowerTaskType taskType)
	{
		List<FollowerTask> list = new List<FollowerTask>(GetDesiredTask_Work(FollowerLocation.Base));
		List<FollowerTask> list2 = new List<FollowerTask>();
		foreach (FollowerBrain allBrain in AllBrains)
		{
			if (allBrain.CurrentTask != null)
			{
				list.Add(allBrain.CurrentTask);
			}
		}
		foreach (FollowerTask item in list)
		{
			if (item.Type == taskType)
			{
				list2.Add(item);
			}
		}
		return list2;
	}

	private static List<FollowerTask> GetDesiredTask_Study(FollowerLocation location)
	{
		return new List<FollowerTask>();
	}

	private static List<FollowerTask> GetDesiredTask_Sleep()
	{
		return new List<FollowerTask>();
	}

	private static List<FollowerTask> GetDesiredTask_Pray()
	{
		return new List<FollowerTask>();
	}

	private static List<FollowerTask> GetDesiredTask_Leisure()
	{
		return new List<FollowerTask>();
	}

	private FollowerTask CheckEatTask(bool Force = false)
	{
		if (CurrentTaskType == FollowerTaskType.Sleep)
		{
			return null;
		}
		InventoryItem.ITEM_TYPE iTEM_TYPE = InventoryItem.ITEM_TYPE.NONE;
		List<Objectives_EatMeal> list = new List<Objectives_EatMeal>();
		foreach (ObjectivesData objective in DataManager.Instance.Objectives)
		{
			if (!(objective is Objectives_EatMeal))
			{
				continue;
			}
			FollowerInfo infoByID = FollowerInfo.GetInfoByID(((Objectives_EatMeal)objective).TargetFollower);
			if (infoByID != null && infoByID.Location == FollowerLocation.Base)
			{
				list.Add((Objectives_EatMeal)objective);
				if (((Objectives_EatMeal)objective).TargetFollower == Info.ID)
				{
					iTEM_TYPE = CookingData.GetMealFromStructureType(((Objectives_EatMeal)objective).MealType);
				}
			}
		}
		bool flag = Force;
		float num = HungerBar.MAX_HUNGER - 10f;
		if (GameManager.IsDungeon(PlayerFarming.Location))
		{
			num = HungerBar.MAX_HUNGER - 30f;
		}
		if (!FollowerBrainStats.Fasting)
		{
			if (Stats.Starvation > 0f)
			{
				flag = true;
			}
			else if (HungerBar.Count < num || DataManager.Instance.MealsCooked <= 1)
			{
				int num2 = StructureManager.GetAllStructuresOfType<Structures_Meal>(Location).Count;
				foreach (Structures_Kitchen item in StructureManager.GetAllStructuresOfType<Structures_Kitchen>(Location))
				{
					if (!item.IsContainingFoodStorage)
					{
						continue;
					}
					foreach (InventoryItem item2 in item.FoodStorage.Data.Inventory)
					{
						if (item2.UnreservedQuantity > 0)
						{
							num2 += item2.UnreservedQuantity;
						}
					}
				}
				FollowerBrain[] array = AllBrains.ToArray();
				Array.Sort(array, (FollowerBrain a, FollowerBrain b) => a._directInfoAccess.Starvation.CompareTo(b._directInfoAccess.Starvation));
				Array.Sort(array, (FollowerBrain a, FollowerBrain b) => a._directInfoAccess.Satiation.CompareTo(b._directInfoAccess.Satiation));
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] == this && i < num2)
					{
						flag = true;
					}
				}
			}
		}
		else if (Stats.IsStarving)
		{
			flag = true;
		}
		if (flag || iTEM_TYPE != 0)
		{
			List<Structures_Meal> list2 = new List<Structures_Meal>();
			foreach (Structures_Meal item3 in StructureManager.GetAllStructuresOfType<Structures_Meal>(Location))
			{
				if (item3.ReservedForTask || item3.Data.Rotten || item3.Data.Burned || (CurrentOverrideStructureType != 0 && item3.Data.Type != CurrentOverrideStructureType) || (Info.CursedState == Thought.Zombie && item3.Data.Type != StructureBrain.TYPES.MEAL_FOLLOWER_MEAT))
				{
					continue;
				}
				if (iTEM_TYPE != 0 && CookingData.GetMealFromStructureType(item3.Data.Type) != iTEM_TYPE)
				{
					list2.Add(item3);
					continue;
				}
				bool flag2 = true;
				foreach (Objectives_EatMeal item4 in list)
				{
					if (item4.MealType == item3.Data.Type && Info.ID != item4.TargetFollower)
					{
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					return new FollowerTask_EatMeal(item3.Data.ID);
				}
			}
			if (list2.Count > 0 && flag)
			{
				return new FollowerTask_EatMeal(list2[0].Data.ID);
			}
			foreach (Structures_Kitchen item5 in StructureManager.GetAllStructuresOfType<Structures_Kitchen>(Location))
			{
				if (!item5.IsContainingFoodStorage)
				{
					continue;
				}
				List<InventoryItem.ITEM_TYPE> list3 = new List<InventoryItem.ITEM_TYPE>();
				foreach (InventoryItem item6 in item5.FoodStorage.Data.Inventory)
				{
					if (item6.UnreservedQuantity <= 0 || (CurrentOverrideStructureType != 0 && StructuresData.GetMealStructureType((InventoryItem.ITEM_TYPE)item6.type) != CurrentOverrideStructureType) || (Info.CursedState == Thought.Zombie && item6.type != 133))
					{
						continue;
					}
					if (iTEM_TYPE != 0 && item6.type != (int)iTEM_TYPE)
					{
						list3.Add((InventoryItem.ITEM_TYPE)item6.type);
						continue;
					}
					bool flag3 = true;
					foreach (Objectives_EatMeal item7 in list)
					{
						if (item7.MealType == CookingData.GetStructureFromMealType((InventoryItem.ITEM_TYPE)item6.type) && Info.ID != item7.TargetFollower)
						{
							flag3 = false;
							break;
						}
					}
					if (flag3)
					{
						return new FollowerTask_EatStoredFood(item5.Data.ID, (InventoryItem.ITEM_TYPE)item6.type);
					}
				}
				if (list3.Count > 0 && flag)
				{
					return new FollowerTask_EatStoredFood(item5.Data.ID, list3[0]);
				}
			}
			CurrentOverrideStructureType = StructureBrain.TYPES.NONE;
		}
		return null;
	}

	public void AssignDwelling(Dwelling.DwellingAndSlot d, int followerID, bool Claimed)
	{
		Structures_Bed structureByID = StructureManager.GetStructureByID<Structures_Bed>(d.ID);
		if (structureByID == null)
		{
			d = StructureManager.GetFreeDwellingAndSlot(Location, _directInfoAccess);
			structureByID = StructureManager.GetStructureByID<Structures_Bed>(d.ID);
		}
		_directInfoAccess.PreviousDwellingID = _directInfoAccess.DwellingID;
		_directInfoAccess.DwellingID = d.ID;
		_directInfoAccess.DwellingSlot = Mathf.Clamp(d.dwellingslot, 0, structureByID.SlotCount - 1);
		_directInfoAccess.DwellingLevel = d.dwellingLevel;
		if (Claimed && !structureByID.Data.FollowersClaimedSlots.Contains(followerID))
		{
			structureByID.Data.FollowersClaimedSlots.Add(followerID);
		}
		structureByID.Data.FollowerID = followerID;
		if (!structureByID.Data.MultipleFollowerIDs.Contains(followerID))
		{
			structureByID.Data.MultipleFollowerIDs.Add(followerID);
		}
		if (Claimed)
		{
			DwellingAssignmentChanged onDwellingAssigned = OnDwellingAssigned;
			if (onDwellingAssigned != null)
			{
				onDwellingAssigned(Info.ID, d);
			}
		}
		else
		{
			DwellingAssignmentChanged onDwellingAssignedAwaitClaim = OnDwellingAssignedAwaitClaim;
			if (onDwellingAssignedAwaitClaim != null)
			{
				onDwellingAssignedAwaitClaim(Info.ID, d);
			}
		}
	}

	public void ClearDwelling()
	{
		Dwelling.DwellingAndSlot dwellingAndSlot = new Dwelling.DwellingAndSlot(_directInfoAccess.DwellingID, _directInfoAccess.DwellingSlot, _directInfoAccess.DwellingLevel);
		Structures_Bed structureByID = StructureManager.GetStructureByID<Structures_Bed>(_directInfoAccess.DwellingID);
		if (structureByID != null)
		{
			if (structureByID.Data.FollowerID == _directInfoAccess.ID)
			{
				structureByID.Data.FollowerID = -1;
			}
			structureByID.Data.FollowersClaimedSlots.Remove(_directInfoAccess.ID);
			structureByID.Data.MultipleFollowerIDs.Remove(_directInfoAccess.ID);
		}
		_directInfoAccess.PreviousDwellingID = _directInfoAccess.DwellingID;
		_directInfoAccess.DwellingID = Dwelling.NO_HOME;
		_directInfoAccess.DwellingSlot = 0;
		_directInfoAccess.DwellingLevel = 0;
		if (dwellingAndSlot.ID != Dwelling.NO_HOME)
		{
			DwellingAssignmentChanged onDwellingCleared = OnDwellingCleared;
			if (onDwellingCleared != null)
			{
				onDwellingCleared(Info.ID, dwellingAndSlot);
			}
		}
		if (CurrentTask != null && CurrentTask is FollowerTask_Sleep)
		{
			CurrentTask.Abort();
		}
	}

	public Structures_Bed GetAssignedDwellingStructure()
	{
		return StructureManager.GetStructureByID<Structures_Bed>(_directInfoAccess.DwellingID);
	}

	public Dwelling.DwellingAndSlot GetDwellingAndSlot()
	{
		if (HasHome && _directInfoAccess != null)
		{
			return new Dwelling.DwellingAndSlot(_directInfoAccess.DwellingID, _directInfoAccess.DwellingSlot, _directInfoAccess.DwellingLevel);
		}
		return null;
	}

	public void SetNewHomeLocation(FollowerLocation location)
	{
		_directInfoAccess.HomeLocation = location;
		if (HasHome && GetAssignedDwellingStructure().Data.Location != HomeLocation)
		{
			ClearDwelling();
		}
	}

	public static List<FollowerBrain> AllAvailableFollowerBrains()
	{
		List<FollowerBrain> list = new List<FollowerBrain>();
		foreach (FollowerBrain allBrain in AllBrains)
		{
			if (!FollowerManager.FollowerLocked(allBrain.Info.ID))
			{
				list.Add(allBrain);
			}
		}
		return list;
	}

	public static bool CanFollowerGiveQuest(FollowerInfo follower)
	{
		if (FollowerManager.FollowerLocked(follower.ID))
		{
			return false;
		}
		if (follower.CursedState != 0)
		{
			return false;
		}
		FollowerBrain orCreateBrain = GetOrCreateBrain(follower);
		if (orCreateBrain != null && orCreateBrain.CurrentTask != null && orCreateBrain.CurrentTask.BlockTaskChanges)
		{
			return false;
		}
		if (orCreateBrain.Stats.Adoration >= orCreateBrain.Stats.MAX_ADORATION)
		{
			return false;
		}
		foreach (ObjectivesData objective in DataManager.Instance.Objectives)
		{
			if (objective.Follower == follower.ID)
			{
				return false;
			}
		}
		foreach (ObjectivesData completedObjective in DataManager.Instance.CompletedObjectives)
		{
			if (completedObjective.Follower == follower.ID)
			{
				return false;
			}
		}
		return true;
	}

	public static bool CanContinueToGiveQuest(FollowerInfo follower)
	{
		if (FollowerManager.FollowerLocked(follower.ID))
		{
			return false;
		}
		if (follower.CursedState != 0)
		{
			return false;
		}
		FollowerBrain orCreateBrain = GetOrCreateBrain(follower);
		if (orCreateBrain != null && orCreateBrain.Stats.Adoration >= orCreateBrain.Stats.MAX_ADORATION)
		{
			return false;
		}
		return true;
	}

	public static FollowerBrain RandomAvailableBrainNoCurseState()
	{
		List<FollowerBrain> list = new List<FollowerBrain>();
		foreach (FollowerBrain allBrain in AllBrains)
		{
			if (!FollowerManager.FollowerLocked(allBrain.Info.ID) && allBrain.Info.CursedState == Thought.None && TimeManager.CurrentDay - allBrain._directInfoAccess.DayJoined >= 2)
			{
				list.Add(allBrain);
			}
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public static FollowerBrain ClosestAvailableBrainNoCurse()
	{
		List<FollowerBrain> list = new List<FollowerBrain>();
		foreach (FollowerBrain allBrain in AllBrains)
		{
			if (!FollowerManager.FollowerLocked(allBrain.Info.ID) && allBrain.Info.CursedState == Thought.None && allBrain.CurrentTaskType != FollowerTaskType.GetPlayerAttention)
			{
				list.Add(allBrain);
			}
		}
		float num = float.MaxValue;
		FollowerBrain result = null;
		foreach (FollowerBrain item in list)
		{
			float num2 = Vector3.Distance(item.LastPosition, PlayerFarming.Instance.transform.position);
			if (num2 < num)
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}

	public static FollowerBrain AddBrain(FollowerInfo info)
	{
		FollowerBrain followerBrain = new FollowerBrain(info);
		AllBrains.Add(followerBrain);
		Action<int> onBrainAdded = OnBrainAdded;
		if (onBrainAdded != null)
		{
			onBrainAdded(info.ID);
		}
		return followerBrain;
	}

	public static void RemoveBrain(int ID)
	{
		for (int num = AllBrains.Count - 1; num >= 0; num--)
		{
			if (AllBrains[num].Info != null && AllBrains[num].Info.ID == ID)
			{
				AllBrains.RemoveAt(num);
				break;
			}
		}
		Structures_Missionary.RemoveFollower(ID);
		Structures_Prison.RemoveFollower(ID);
		Structures_Demon_Summoner.RemoveFollower(ID);
		Action<int> onBrainRemoved = OnBrainRemoved;
		if (onBrainRemoved != null)
		{
			onBrainRemoved(ID);
		}
	}

	public static int DiscipleCount()
	{
		int num = 0;
		foreach (FollowerBrain allBrain in AllBrains)
		{
			if (allBrain.Stats.HasLevelledUp)
			{
				num++;
			}
		}
		return num;
	}

	public static FollowerBrain FindBrainByID(int ID)
	{
		for (int i = 0; i < AllBrains.Count; i++)
		{
			if (AllBrains[i].Info != null && AllBrains[i].Info.ID == ID)
			{
				return AllBrains[i];
			}
		}
		return null;
	}

	public static bool FindBrainByLevelExists(int Level)
	{
		foreach (FollowerBrain allBrain in AllBrains)
		{
			if (allBrain.Info.XPLevel >= Level)
			{
				return true;
			}
		}
		return false;
	}

	public static FollowerBrain GetOrCreateBrain(FollowerInfo info)
	{
		FollowerBrain followerBrain = ((info != null) ? FindBrainByID(info.ID) : null);
		if (followerBrain == null)
		{
			followerBrain = AddBrain(info);
		}
		return followerBrain;
	}

	public static List<FollowerBrain> GetBrainsWithinRadius(Vector3 position, float radius)
	{
		List<FollowerBrain> list = new List<FollowerBrain>();
		foreach (FollowerBrain allBrain in AllBrains)
		{
			if (Vector3.Distance(allBrain.LastPosition, position) < radius)
			{
				list.Add(allBrain);
			}
		}
		return list;
	}
}
