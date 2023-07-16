using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Lamb.UI;
using MMTools;
using UnityEngine;

public class TimeManager : BaseMonoBehaviour
{
	public const int SIMULATION_FRAME_SPREAD = 10;

	public const int REAL_SECONDS_PER_DAY = 480;

	public const int GAME_HOURS_PER_PHASE = 4;

	public const int GAME_MINUTES_PER_PHASE = 240;

	public const int GAME_HOURS_PER_DAY = 20;

	public const int GAME_MINUTES_PER_DAY = 1200;

	public static Action OnNewDayStarted;

	public static Action OnNewPhaseStarted;

	public static Action OnScheduleChanged;

	public static Dictionary<Vector3Int, List<LongGrass>> GrassRegions = new Dictionary<Vector3Int, List<LongGrass>>();

	public const int GrassRegionSize = 3;

	private static Vector3 Position;

	private static float _gameTimeSinceLastProgress = 0f;

	private static ITaskProvider _currentOverrideTaskProvider;

	public static float TotalElapsedGameTime
	{
		get
		{
			return (float)(CurrentDay * 1200) + CurrentGameTime;
		}
	}

	public static float CurrentGameTime
	{
		get
		{
			return DataManager.Instance.CurrentGameTime;
		}
		set
		{
			DataManager.Instance.CurrentGameTime = value;
		}
	}

	public static float DeltaGameTime { get; private set; }

	public static int CurrentDay
	{
		get
		{
			return DataManager.Instance.CurrentDayIndex;
		}
		private set
		{
			DataManager.Instance.CurrentDayIndex = value;
		}
	}

	public static DayPhase CurrentPhase
	{
		get
		{
			return (DayPhase)DataManager.Instance.CurrentPhaseIndex;
		}
		private set
		{
			DataManager.Instance.CurrentPhaseIndex = (int)value;
		}
	}

	public static DayPhase NextPhase
	{
		get
		{
			return (DayPhase)((int)(CurrentPhase + 1) % 5);
		}
	}

	public static int CurrentHour { get; private set; }

	public static int CurrentMinute { get; private set; }

	public static bool IsDay
	{
		get
		{
			if (CurrentPhase != 0 && CurrentPhase != DayPhase.Morning)
			{
				return CurrentPhase == DayPhase.Afternoon;
			}
			return true;
		}
	}

	public static bool IsNight
	{
		get
		{
			return CurrentPhase == DayPhase.Night;
		}
	}

	public static float CurrentPhaseProgress
	{
		get
		{
			return (CurrentGameTime - (float)((int)CurrentPhase * 240)) / 240f;
		}
	}

	public static float CurrentDayProgress
	{
		get
		{
			return (CurrentGameTime - 0f) / 1200f;
		}
	}

	public static float TimeSinceLastComplaint
	{
		get
		{
			return DataManager.Instance.TimeSinceLastComplaint;
		}
		set
		{
			DataManager.Instance.TimeSinceLastComplaint = value;
		}
	}

	public static float TimeSinceLastQuest
	{
		get
		{
			return DataManager.Instance.TimeSinceLastQuest;
		}
		set
		{
			DataManager.Instance.TimeSinceLastQuest = value;
		}
	}

	public static bool PauseGameTime
	{
		get
		{
			return DataManager.Instance.PauseGameTime;
		}
		set
		{
			DataManager.Instance.PauseGameTime = value;
		}
	}

	public static float TimeRemainingInCurrentPhase()
	{
		return (float)(240 * (int)(CurrentPhase + 1)) - CurrentGameTime;
	}

	public static float TimeRemainingUntilPhase(DayPhase phase)
	{
		float num = (float)(240 * (int)phase) - CurrentGameTime;
		if (phase <= CurrentPhase)
		{
			num += 1200f;
		}
		return num;
	}

	private static void StartNewDay()
	{
		Debug.Log("NEW DAY!");
		CurrentDay++;
		CurrentGameTime -= 1200f;
		Action onNewDayStarted = OnNewDayStarted;
		if (onNewDayStarted != null)
		{
			onNewDayStarted();
		}
		DataManager.Instance.EndlessModeOnCooldown = false;
		StructureManager.UpdateWeeds(FollowerLocation.Base);
		if (DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.Libertarian))
		{
			List<StructureBrain> allStructuresOfType = StructureManager.GetAllStructuresOfType(StructureBrain.TYPES.PRISON);
			bool flag = true;
			foreach (StructureBrain item in allStructuresOfType)
			{
				if (item.Data.FollowerID != -1)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				CultFaithManager.AddThought(Thought.Cult_Libertarian, -1, 1f);
			}
		}
		if (NotificationCentre.Instance != null)
		{
			NotificationCentre.Instance.PlayGenericNotification("Notifications/Cult_Sermon/Notification/Ready");
		}
	}

	private static void StartNewPhase(DayPhase phase)
	{
		if (GetScheduledActivity(phase) == ScheduledActivity.Sleep)
		{
			SetOverrideScheduledActivity(ScheduledActivity.None);
		}
		CurrentPhase = phase;
		Action onNewPhaseStarted = OnNewPhaseStarted;
		if (onNewPhaseStarted != null)
		{
			onNewPhaseStarted();
		}
		if (CurrentPhase == DayPhase.Night)
		{
			AudioManager.Instance.ToggleFilter(SoundParams.Night, true);
		}
		else if (CurrentPhase == DayPhase.Dawn)
		{
			AudioManager.Instance.ToggleFilter(SoundParams.Night, false);
			StartNewDay();
		}
		if (DataManager.Instance.GameOverEnabled && DataManager.Instance.Followers.Count <= 0 && !DataManager.Instance.InGameOver)
		{
			DataManager.Instance.DisplayGameOverWarning = true;
			Debug.Break();
		}
	}

	private static void CheckNewPhase()
	{
		int num = (int)(CurrentPhase + 1) * 240;
		if (CurrentGameTime > (float)num)
		{
			StartNewPhase(NextPhase);
		}
	}

	public static void AddToRegion(LongGrass l)
	{
		Vector3Int key = PositionToRegions(l.Position);
		if (!GrassRegions.ContainsKey(key))
		{
			GrassRegions.Add(key, new List<LongGrass> { l });
		}
		else
		{
			GrassRegions[key].Add(l);
		}
	}

	public static void RemoveLongGrass(LongGrass l)
	{
		Vector3Int key = PositionToRegions(l.Position);
		if (GrassRegions.ContainsKey(key))
		{
			GrassRegions[key].Remove(l);
			if (GrassRegions[key].Count <= 0)
			{
				GrassRegions.Remove(key);
			}
		}
	}

	private static Vector3Int PositionToRegions(Vector3 Position)
	{
		return Vector3Int.FloorToInt(Position) / 3;
	}

	private static void UpdateGrassRegions()
	{
		if (PlayerFarming.Instance != null)
		{
			Position = PlayerFarming.Instance.transform.position;
			if (GrassRegions.ContainsKey(PositionToRegions(Position)))
			{
				foreach (LongGrass item in GrassRegions[PositionToRegions(Position)])
				{
					if ((Time.frameCount + item.FrameIntervalOffset) % item.UpdateInterval == 0 && Vector3.Distance(item.Position, Position) < 1f)
					{
						item.Colliding(PlayerFarming.Instance.gameObject);
					}
				}
			}
		}
		foreach (Health item2 in Health.team2)
		{
			if (item2 == null)
			{
				continue;
			}
			Position = item2.transform.position;
			if (!GrassRegions.ContainsKey(PositionToRegions(Position)))
			{
				continue;
			}
			foreach (LongGrass item3 in GrassRegions[PositionToRegions(Position)])
			{
				if ((Time.frameCount + item3.FrameIntervalOffset) % item3.UpdateInterval == 0 && Vector3.Distance(item3.Position, Position) < 1f)
				{
					item3.Colliding(item2.gameObject);
				}
			}
		}
		foreach (Follower follower in Follower.Followers)
		{
			Position = follower.transform.position;
			if (!GrassRegions.ContainsKey(PositionToRegions(Position)))
			{
				continue;
			}
			foreach (LongGrass item4 in GrassRegions[PositionToRegions(Position)])
			{
				if ((Time.frameCount + item4.FrameIntervalOffset) % item4.UpdateInterval == 0 && Vector3.Distance(item4.Position, Position) < 1f)
				{
					item4.Colliding(follower.gameObject);
				}
			}
		}
	}

	public static void Simulate(float deltaGameTime)
	{
		if (deltaGameTime <= 0f)
		{
			return;
		}
		float num12 = deltaGameTime / 1200f;
		DeltaGameTime = deltaGameTime;
		if (!PauseGameTime)
		{
			CurrentGameTime += DeltaGameTime;
		}
		CheckNewPhase();
		int currentHour = CurrentHour;
		CurrentHour = Mathf.FloorToInt(CurrentGameTime / 60f);
		CurrentMinute = Mathf.FloorToInt(CurrentGameTime % 60f);
		StructureEffectManager.Tick();
		if (CurrentHour != currentHour)
		{
			InstantActivity instantActivity = GetInstantActivity(CurrentHour);
		}
		if (_currentOverrideTaskProvider != null && _currentOverrideTaskProvider.CheckOverrideComplete())
		{
			ClearOverrideTaskProvider();
			Debug.Log("Override Task Finished");
		}
		UpdateGrassRegions();
		for (int i = 0; i < 85; i++)
		{
			List<SimFollower> list = FollowerManager.SimFollowersAtLocation((FollowerLocation)i);
			for (int j = 0; j < list.Count; j++)
			{
				SimFollower simFollower = list[j];
				if (simFollower.Brain.LeftCult)
				{
					simFollower.Brain.Leave(simFollower.Brain.LeftCultWithReason);
				}
			}
		}
		for (int k = 0; k < 85; k++)
		{
			List<Follower> list2 = FollowerManager.FollowersAtLocation((FollowerLocation)k);
			for (int l = 0; l < list2.Count; l++)
			{
				Follower follower = list2[l];
				if (follower.Brain.LeftCult)
				{
					follower.Leave(follower.Brain.LeftCultWithReason);
				}
			}
		}
		if (DataManager.Instance.dungeonRun >= 3 && CurrentDay >= 3 && DataManager.Instance.ShowCultFaith)
		{
			List<Follower> list3 = FollowerManager.FollowersAtLocation(PlayerFarming.Location);
			if (list3.Count > 0)
			{
				if (TimeSinceLastQuest > 480f && DataManager.Instance.CurrentOnboardingFollowerID == -1 && ObjectiveManager.GetNumberOfObjectivesInGroup("Objectives/GroupTitles/Quest") < 1)
				{
					List<Follower> list4 = new List<Follower>();
					List<Follower> list5 = new List<Follower>();
					foreach (Follower item in list3)
					{
						if (item != null && item.Brain != null && item.Brain.Location == FollowerLocation.Base && !FollowerManager.FollowerLocked(item.Brain.Info.ID) && (item.Brain.CurrentTask == null || (!item.Brain.CurrentTask.BlockTaskChanges && item.Brain.CurrentTaskType != FollowerTaskType.Sleep)) && item.Brain.Info.CursedState == Thought.None && item.Brain._directInfoAccess.CurrentPlayerQuest == null)
						{
							if (Vector3.Distance(item.transform.position, PlayerFarming.Instance.transform.position) < 6f)
							{
								list4.Add(item);
							}
							if (Quests.GetCurrentStoryObjective(item.Brain.Info.ID) != null)
							{
								list5.Add(item);
							}
						}
					}
					Follower follower2 = null;
					if (list5.Count > 0)
					{
						follower2 = list5[UnityEngine.Random.Range(0, list5.Count)];
					}
					else if (list4.Count > 0)
					{
						follower2 = list4[UnityEngine.Random.Range(0, list4.Count)];
					}
					if (follower2 != null)
					{
						follower2.Brain.HardSwapToTask(new FollowerTask_GetAttention(Follower.ComplaintType.GiveQuest));
						TimeSinceLastQuest = 0f;
					}
				}
				TimeSinceLastQuest += deltaGameTime;
			}
		}
		for (int m = 0; m < 85; m++)
		{
			FollowerLocation followerLocation = (FollowerLocation)m;
			int num = 0;
			foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
			{
				if (allBrain.HomeLocation == followerLocation && allBrain.ShouldReconsiderTask && allBrain.BeginReconsider())
				{
					num++;
				}
			}
			if (num <= 0)
			{
				continue;
			}
			ScheduledActivity selectedActivity;
			List<FollowerTask> topPriorityFollowerTasks = FollowerBrain.GetTopPriorityFollowerTasks(followerLocation, out selectedActivity);
			List<FollowerTask> list6 = new List<FollowerTask>();
			if (selectedActivity != 0)
			{
				list6 = FollowerBrain.GetTopPriorityFollowerTasks(ScheduledActivity.Work, followerLocation);
			}
			else
			{
				list6.AddRange(topPriorityFollowerTasks);
			}
			List<int> list7 = new List<int>();
			foreach (FollowerBrain allBrain2 in FollowerBrain.AllBrains)
			{
				if (allBrain2.HomeLocation != followerLocation)
				{
					continue;
				}
				foreach (int item2 in list7)
				{
					if (item2 != -1)
					{
						list6[item2] = null;
					}
				}
				if (allBrain2.ShouldReconsiderTask && GetScheduledActivity(followerLocation) == ScheduledActivity.Sleep && allBrain2.Info.CursedState == Thought.None && allBrain2._directInfoAccess.WorkThroughNight)
				{
					if (FollowerBrainStats.IsHoliday)
					{
						allBrain2.ClaimNextAvailableTask(new List<FollowerTask>
						{
							new FollowerTask_FakeLeisure()
						});
					}
					else
					{
						list7.Add(allBrain2.ClaimNextAvailableTask(list6));
					}
				}
				else if (allBrain2.ShouldReconsiderTask)
				{
					if (topPriorityFollowerTasks.Count == 0)
					{
						allBrain2.TryClaimExistingTask(topPriorityFollowerTasks);
					}
					else
					{
						list7.Add(allBrain2.TryClaimExistingTask(list6));
					}
				}
			}
			int num2 = 5;
			bool flag = false;
			do
			{
				flag = false;
				num2--;
				for (int n = 0; n < topPriorityFollowerTasks.Count; n++)
				{
					FollowerTask followerTask = topPriorityFollowerTasks[n];
					if (followerTask == null || list7.Contains(n))
					{
						continue;
					}
					FollowerBrain followerBrain = null;
					PriorityCategory priorityCategory = PriorityCategory.ExtremelyUrgent;
					float num3 = float.MaxValue;
					foreach (FollowerBrain allBrain3 in FollowerBrain.AllBrains)
					{
						if (allBrain3.HomeLocation != followerLocation)
						{
							continue;
						}
						FollowerTask followerTask2 = null;
						if (allBrain3.PendingTask.Task != null)
						{
							followerTask2 = ((!allBrain3.PendingTask.KeepExistingTask) ? allBrain3.PendingTask.Task : allBrain3.CurrentTask);
						}
						if (followerTask2 != null)
						{
							if (!allBrain3.ShouldReconsiderTask || !FollowerTask.RequiredFollowerLevel(allBrain3.Info.FollowerRole, followerTask.Type))
							{
								continue;
							}
							PriorityCategory priorityCategory2 = followerTask2.GetPriorityCategory(allBrain3.Info.FollowerRole, allBrain3.Info.WorkerPriority, allBrain3);
							bool flag2 = false;
							if (followerTask.GetPriorityCategory(allBrain3.Info.FollowerRole, allBrain3.Info.WorkerPriority, allBrain3) < priorityCategory2)
							{
								flag2 = true;
							}
							else if (followerTask.GetPriorityCategory(allBrain3.Info.FollowerRole, allBrain3.Info.WorkerPriority, allBrain3) == priorityCategory2)
							{
								flag2 = followerTask.Priorty > followerTask2.Priorty;
							}
							if (flag2)
							{
								if (allBrain3.PendingTask.Task == null)
								{
									followerBrain = allBrain3;
									break;
								}
								if (priorityCategory2 > priorityCategory || (priorityCategory2 == priorityCategory && followerTask2.Priorty < num3))
								{
									followerBrain = allBrain3;
									priorityCategory = priorityCategory2;
									num3 = followerTask2.Priorty;
								}
							}
						}
						else if (allBrain3.PendingTask.Task == null)
						{
							followerBrain = allBrain3;
							break;
						}
					}
					if (followerBrain != null)
					{
						flag = true;
						if (followerBrain.PendingTask.Task != null)
						{
							topPriorityFollowerTasks[followerBrain.PendingTask.ListIndex] = followerBrain.PendingTask.Task;
						}
						followerBrain.PendingTask.KeepExistingTask = false;
						followerBrain.PendingTask.Task = followerTask;
						followerBrain.PendingTask.ListIndex = n;
						topPriorityFollowerTasks[n] = null;
					}
				}
			}
			while (num2 > 0 && flag);
			foreach (FollowerBrain allBrain4 in FollowerBrain.AllBrains)
			{
				if (allBrain4.HomeLocation == followerLocation && allBrain4.ShouldReconsiderTask && allBrain4.CurrentTaskType != FollowerTaskType.ManualControl)
				{
					allBrain4.EndReconsider();
				}
			}
		}
		CultFaithManager.UpdateSimulation(DeltaGameTime);
		HungerBar.UpdateSimulation(DeltaGameTime);
		IllnessBar.UpdateSimulation(DeltaGameTime);
		foreach (Follower item3 in FollowerManager.ActiveLocationFollowers())
		{
			if (Time.frameCount % 10 == item3.Brain.Info.ID % 10)
			{
				item3.Tick(DeltaGameTime * 10f);
			}
		}
		for (int num4 = 0; num4 < 85; num4++)
		{
			List<Follower> list8 = FollowerManager.FollowersAtLocation((FollowerLocation)num4);
			for (int num5 = 0; num5 < list8.Count; num5++)
			{
				Follower follower3 = list8[num5];
				if (follower3.Brain.DesiredLocation == follower3.Brain.Location)
				{
					continue;
				}
				list8.RemoveAt(num5--);
				LocationManager value;
				if (LocationManager.GetLocationState(follower3.Brain.DesiredLocation) == LocationState.Unloaded)
				{
					SimFollower simFollower2 = FollowerManager.FindSimFollowerByID(follower3.Brain.Info.ID);
					if (simFollower2 == null)
					{
						simFollower2 = new SimFollower(follower3.Brain);
					}
					else
					{
						FollowerManager.SimFollowersAtLocation(follower3.Brain.Location).Remove(simFollower2);
					}
					follower3.Brain.Location = follower3.Brain.DesiredLocation;
					FollowerManager.SimFollowersAtLocation(follower3.Brain.Location).Add(simFollower2);
					if (follower3 != null)
					{
						UnityEngine.Object.Destroy(follower3.gameObject);
					}
				}
				else if (!LocationManager.LocationManagers.TryGetValue(follower3.Brain.DesiredLocation, out value))
				{
					Debug.LogError(string.Format("No LocationManager for Location.{0}, move failed", follower3.Brain.DesiredLocation));
					follower3.Brain.DesiredLocation = follower3.Brain.Location;
				}
				else
				{
					value.AddFollower(follower3);
				}
			}
		}
		for (int num6 = 0; num6 < 85; num6++)
		{
			FollowerLocation followerLocation2 = (FollowerLocation)num6;
			List<SimFollower> list9 = FollowerManager.SimFollowersAtLocation(followerLocation2);
			for (int num7 = 0; num7 < list9.Count; num7++)
			{
				SimFollower simFollower3 = list9[num7];
				if (simFollower3.Brain.DesiredLocation == simFollower3.Brain.Location)
				{
					continue;
				}
				list9.RemoveAt(num7--);
				FollowerLocation desiredLocation = simFollower3.Brain.DesiredLocation;
				FollowerLocation desiredLocation2 = simFollower3.Brain.DesiredLocation;
				int num13 = 2;
				LocationState locationState = LocationManager.GetLocationState(simFollower3.Brain.DesiredLocation);
				if (locationState != 0)
				{
					LocationManager.LocationManagers[simFollower3.Brain.DesiredLocation].SpawnFollower(simFollower3, locationState == LocationState.Active);
				}
				if (locationState != LocationState.Active)
				{
					if (simFollower3.Brain.DesiredLocation == FollowerLocation.None)
					{
						throw new InvalidOperationException(string.Format("Invalid desired FollowerLocation.{0} !!", followerLocation2));
					}
					Debug.Log(string.Format("Moving (sim) {0} from {1} to {2}", simFollower3.Brain.Info.Name, simFollower3.Brain.Location, simFollower3.Brain.DesiredLocation));
					simFollower3.Brain.Location = simFollower3.Brain.DesiredLocation;
					FollowerManager.SimFollowersAtLocation(simFollower3.Brain.DesiredLocation).Add(simFollower3);
				}
			}
		}
		for (int num8 = 0; num8 < 85; num8++)
		{
			FollowerLocation location = (FollowerLocation)num8;
			if (LocationManager.GetLocationState(location) == LocationState.Active)
			{
				continue;
			}
			foreach (SimFollower item4 in FollowerManager.SimFollowersAtLocation(location))
			{
				if (Time.frameCount % 10 == item4.Brain.Info.ID % 10)
				{
					item4.Tick(DeltaGameTime * 10f);
				}
			}
		}
		for (int num9 = 0; num9 < 85; num9++)
		{
			List<SimFollower> list10 = FollowerManager.SimFollowersAtLocation((FollowerLocation)num9);
			for (int num10 = 0; num10 < list10.Count; num10++)
			{
				if (list10[num10].Retired)
				{
					list10.RemoveAt(num10--);
				}
			}
		}
		_gameTimeSinceLastProgress += deltaGameTime;
		for (int num11 = 0; num11 < DataManager.Instance.CurrentResearch.Count; num11++)
		{
			StructuresData.ResearchObject researchObject = DataManager.Instance.CurrentResearch[num11];
			researchObject.Progress += _gameTimeSinceLastProgress;
			if (researchObject.Progress >= researchObject.TargetProgress)
			{
				StructuresData.CompleteResearch(researchObject.Type);
			}
		}
		_gameTimeSinceLastProgress = 0f;
	}

	public static int GetSermonRitualCooldownRemaining(SermonsAndRituals.SermonRitualType type)
	{
		int num = SermonsAndRituals.CooldownDays(type);
		int lastUsedDayIndex = GetLastUsedDayIndex(type);
		if (lastUsedDayIndex == -1)
		{
			return 0;
		}
		return Mathf.Max(0, lastUsedDayIndex + num - DataManager.Instance.CurrentDayIndex);
	}

	private static int GetLastUsedDayIndex(SermonsAndRituals.SermonRitualType type)
	{
		CheckResizeCooldownArray();
		return DataManager.Instance.LastUsedSermonRitualDayIndex[(int)type];
	}

	public static void SetSermonRitualUsed(SermonsAndRituals.SermonRitualType type)
	{
		CheckResizeCooldownArray();
		DataManager.Instance.LastUsedSermonRitualDayIndex[(int)type] = DataManager.Instance.CurrentDayIndex;
	}

	private static void CheckResizeCooldownArray()
	{
		int num = DataManager.Instance.LastUsedSermonRitualDayIndex.Length;
		int num2 = 23;
		if (num <= num2)
		{
			Array.Resize(ref DataManager.Instance.LastUsedSermonRitualDayIndex, 23);
			for (int i = num; i < num2; i++)
			{
				DataManager.Instance.LastUsedSermonRitualDayIndex[i] = -1;
			}
		}
	}

	public static void SetScheduledActivity(DayPhase phase, ScheduledActivity activity)
	{
		DataManager.Instance.ScheduledActivityIndexes[(int)phase] = (int)activity;
	}

	public static void SetOverrideScheduledActivity(ScheduledActivity activity)
	{
		DataManager.Instance.OverrideScheduledActivity = (int)activity;
		Action onScheduleChanged = OnScheduleChanged;
		if (onScheduleChanged != null)
		{
			onScheduleChanged();
		}
	}

	public static ScheduledActivity GetOverrideScheduledActivity()
	{
		return (ScheduledActivity)DataManager.Instance.OverrideScheduledActivity;
	}

	public static ScheduledActivity GetScheduledActivity(FollowerLocation location)
	{
		ScheduledActivity scheduledActivity = GetOverrideScheduledActivity();
		if (scheduledActivity == ScheduledActivity.None)
		{
			scheduledActivity = GetScheduledActivity(CurrentPhase);
		}
		if (!DataManager.Instance.HappinessEnabled && scheduledActivity == ScheduledActivity.Leisure)
		{
			scheduledActivity = ScheduledActivity.Work;
		}
		else if ((!DataManager.Instance.TeachingsEnabled || location != FollowerLocation.Base) && scheduledActivity == ScheduledActivity.Study)
		{
			scheduledActivity = ScheduledActivity.Work;
		}
		else if ((!DataManager.Instance.PrayerEnabled || !DataManager.Instance.PrayerOrdered) && scheduledActivity == ScheduledActivity.Pray)
		{
			scheduledActivity = ScheduledActivity.Work;
		}
		return scheduledActivity;
	}

	public static ScheduledActivity GetScheduledActivity(DayPhase phase)
	{
		int num = DataManager.Instance.ScheduledActivityIndexes[(int)phase];
		if (num >= 5)
		{
			num = 0;
		}
		ScheduledActivity scheduledActivity = (ScheduledActivity)num;
		if (!DataManager.Instance.HappinessEnabled && scheduledActivity == ScheduledActivity.Leisure)
		{
			scheduledActivity = ScheduledActivity.Work;
		}
		else if (!DataManager.Instance.TeachingsEnabled && scheduledActivity == ScheduledActivity.Study)
		{
			scheduledActivity = ScheduledActivity.Work;
		}
		return scheduledActivity;
	}

	public static void SetInstantActivity(InstantActivity activity, int hour)
	{
		DataManager.Instance.InstantActivityIndexes[(int)activity] = hour;
	}

	public static InstantActivity GetInstantActivity(int hour)
	{
		InstantActivity result = InstantActivity.None;
		for (int i = 0; i < 1; i++)
		{
			if (DataManager.Instance.InstantActivityIndexes[i] == hour)
			{
				result = (InstantActivity)i;
				break;
			}
		}
		return result;
	}

	public static int GetInstantActivityHour(InstantActivity activity)
	{
		return DataManager.Instance.InstantActivityIndexes[(int)activity];
	}

	public static void SetOverrideTaskProvider(ITaskProvider provider)
	{
		ClearOverrideTaskProvider();
		_currentOverrideTaskProvider = provider;
		Action onScheduleChanged = OnScheduleChanged;
		if (onScheduleChanged != null)
		{
			onScheduleChanged();
		}
	}

	private static void ClearOverrideTaskProvider()
	{
		_currentOverrideTaskProvider = null;
	}

	public static FollowerTask GetOverrideTask(FollowerBrain brain)
	{
		FollowerTask result = null;
		if (_currentOverrideTaskProvider != null)
		{
			FollowerTask overrideTask = _currentOverrideTaskProvider.GetOverrideTask(brain);
			if (overrideTask != null && (overrideTask.Location == brain.HomeLocation || (overrideTask.Location == FollowerLocation.Church && brain.HomeLocation == FollowerLocation.Base)))
			{
				result = overrideTask;
			}
		}
		return result;
	}

	public static string GetOverrideTaskString()
	{
		string result = "";
		if (_currentOverrideTaskProvider != null)
		{
			result = _currentOverrideTaskProvider.GetType().ToString();
		}
		return result;
	}

	public static void SkipTime(float time)
	{
		GameManager.GetInstance().StartCoroutine(SkipTimeIE(time));
	}

	private static IEnumerator SkipTimeIE(float duration)
	{
		while (PlayerFarming.Location != FollowerLocation.Base || LetterBox.IsPlaying || MMConversation.isPlaying || SimulationManager.IsPaused || (PlayerFarming.Instance.state.CURRENT_STATE != 0 && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.Moving))
		{
			yield return null;
		}
		float targetTime = TotalElapsedGameTime + duration;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(TownCentre.Instance.gameObject, 20f);
		GameManager.GetInstance().StartCoroutine(_003CSkipTimeIE_003Eg__Delay_007C83_0());
		float maxTimescale = 20f;
		float timer2 = 0f;
		while (TotalElapsedGameTime < targetTime)
		{
			if (MonoSingleton<UIManager>.Instance.MenusBlocked)
			{
				Time.timeScale = 0f;
			}
			else
			{
				Time.timeScale = Mathf.Lerp(1f, maxTimescale, timer2 / 1f);
			}
			timer2 += Time.fixedDeltaTime;
			yield return null;
		}
		timer2 = 0f;
		while (Time.timeScale != 1f)
		{
			if (MonoSingleton<UIManager>.Instance.MenusBlocked)
			{
				Time.timeScale = 0f;
			}
			else
			{
				Time.timeScale = Mathf.Lerp(maxTimescale, 1f, timer2 / 1f);
			}
			timer2 += Time.fixedDeltaTime;
			yield return null;
		}
		GameManager.GetInstance().OnConversationEnd();
		Time.timeScale = 1f;
	}

	[CompilerGenerated]
	internal static IEnumerator _003CSkipTimeIE_003Eg__Delay_007C83_0()
	{
		yield return new WaitForSeconds(1.2f);
		HUD_Manager.Instance.TimeTransitions.MoveBackInFunction();
	}
}
