using System;
using System.Collections.Generic;
using System.Text;
using Spine;
using UnityEngine;

public class SimulationManager : BaseMonoBehaviour
{
	public delegate bool SkipDelegate();

	private class DropdownData
	{
		public bool show;

		public int selectedIndex;

		public Vector2 scrollViewVector;
	}

	private static bool UpdatedThisFrame = false;

	public const float DEFAULT_SKIP_SPEED = 100f;

	public static bool ShowFollowerDebugInfo = false;

	private static bool _showFollowerDebugInfo;

	public static bool ShowStructureDebugInfo = false;

	private static bool _showStructureDebugInfo;

	private static bool _showDetailedLocationDebugInfo = false;

	public static Dictionary<int, bool> ShowDetailedFollowerDebugInfo = new Dictionary<int, bool>();

	public static Dictionary<int, bool> ShowDetailedStructureDebugInfo = new Dictionary<int, bool>();

	public float SimulationSpeed = 1f;

	private static bool _isPaused = false;

	private static float _gameMinutesToSkip = 0f;

	private static float _currentSkipSpeed = 100f;

	private static SkipDelegate _shouldEndSkipDelegate;

	private static Action _onSkipComplete;

	private static List<FollowerLocation> _skipLocations = new List<FollowerLocation>();

	private static DropdownData _followerLocationDropdown = new DropdownData();

	private static DropdownData _structureTypeDropdown = new DropdownData();

	private static DropdownData _structureLocationDropdown = new DropdownData();

	public static bool IsPaused
	{
		get
		{
			return _isPaused;
		}
	}

	private void Update()
	{
		if (UpdatedThisFrame)
		{
			return;
		}
		TrinketManager.UpdateCooldowns(Time.deltaTime);
		if (_gameMinutesToSkip > 0f)
		{
			float num = Time.deltaTime * _currentSkipSpeed / 480f * 1200f;
			if (num > _gameMinutesToSkip)
			{
				num = _gameMinutesToSkip;
			}
			TimeManager.Simulate(num);
			_gameMinutesToSkip -= TimeManager.DeltaGameTime;
			if (_shouldEndSkipDelegate != null && _shouldEndSkipDelegate())
			{
				_gameMinutesToSkip = 0f;
			}
			if (_gameMinutesToSkip <= 0f)
			{
				foreach (FollowerLocation skipLocation in _skipLocations)
				{
					LocationManager.ActivateLocation(skipLocation);
				}
				_skipLocations.Clear();
				_shouldEndSkipDelegate = null;
				Action onSkipComplete = _onSkipComplete;
				_onSkipComplete = null;
				if (onSkipComplete != null)
				{
					onSkipComplete();
				}
			}
			UpdatedThisFrame = true;
		}
		else if (!_isPaused)
		{
			TimeManager.Simulate(Time.deltaTime * SimulationSpeed / 480f * 1200f);
			UpdatedThisFrame = true;
		}
	}

	private void LateUpdate()
	{
		UpdatedThisFrame = false;
	}

	public static void Pause()
	{
		_isPaused = true;
	}

	public static void UnPause()
	{
		_isPaused = false;
	}

	private static float GetSkipSpeed(float desiredDuration, float gameMinutesToSkip)
	{
		return gameMinutesToSkip / 1200f * 480f / desiredDuration;
	}

	public static void SkipToPhase(float desiredDuration, DayPhase phase, Action onComplete)
	{
		float num = TimeManager.TimeRemainingUntilPhase(phase) - 1f;
		Skip(GetSkipSpeed(desiredDuration, num), null, num, onComplete);
	}

	public static void SkipToPhase(DayPhase phase, Action onComplete)
	{
		float maxGameMinutesToSkip = TimeManager.TimeRemainingUntilPhase(phase) - 1f;
		Skip(100f, null, maxGameMinutesToSkip, onComplete);
	}

	public static void SkipWithDuration(float desiredDuration, SkipDelegate check, float maxGameMinutesToSkip, Action onComplete)
	{
		Skip(GetSkipSpeed(desiredDuration, maxGameMinutesToSkip), check, maxGameMinutesToSkip, onComplete);
	}

	public static void Skip(SkipDelegate check, float maxGameMinutesToSkip, Action onComplete)
	{
		Skip(100f, check, maxGameMinutesToSkip, onComplete);
	}

	private static void Skip(float skipSpeed, SkipDelegate check, float maxGameMinutesToSkip, Action onComplete)
	{
		_skipLocations.AddRange(LocationManager.LocationsInState(LocationState.Active));
		foreach (FollowerLocation skipLocation in _skipLocations)
		{
			LocationManager.DeactivateLocation(skipLocation);
		}
		_shouldEndSkipDelegate = check;
		_gameMinutesToSkip = maxGameMinutesToSkip;
		_currentSkipSpeed = skipSpeed;
		_onSkipComplete = onComplete;
	}

	private void OnGUI()
	{
		if (PerformanceTest.ReduceGUI)
		{
			return;
		}
		if (UnityEngine.Event.current.type == EventType.Layout)
		{
			if (!_showFollowerDebugInfo && ShowFollowerDebugInfo)
			{
				_followerLocationDropdown.selectedIndex = (int)PlayerFarming.Location;
			}
			_showFollowerDebugInfo = ShowFollowerDebugInfo;
			_showStructureDebugInfo = ShowStructureDebugInfo;
		}
		GUILayout.BeginArea(new Rect(935f, 200f, 1000f, 1000f));
		GUILayout.BeginVertical();
		if (_showFollowerDebugInfo)
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(_showDetailedLocationDebugInfo ? "-" : "+", GUILayout.Width(50f)))
			{
				_showDetailedLocationDebugInfo = !_showDetailedLocationDebugInfo;
			}
			StringBuilder stringBuilder = new StringBuilder();
			IEnumerable<FollowerLocation> values = LocationManager.LocationsInState(LocationState.Active);
			stringBuilder.AppendLine(string.Format("Player Location: {0}, Prev: {1}", PlayerFarming.Location, PlayerFarming.LastLocation));
			stringBuilder.AppendLine("Active Locations: " + string.Join(", ", values));
			if (_showDetailedLocationDebugInfo)
			{
				IEnumerable<FollowerLocation> values2 = LocationManager.LocationsInState(LocationState.Inactive);
				stringBuilder.AppendLine("Inactive Locations: " + string.Join(", ", values2));
				IEnumerable<FollowerLocation> values3 = LocationManager.LocationsInState(LocationState.Unloaded);
				stringBuilder.AppendLine("Unloaded Locations: " + string.Join(", ", values3));
			}
			GUILayout.Label(stringBuilder.ToString());
			GUILayout.EndHorizontal();
			Array values4 = Enum.GetValues(typeof(FollowerLocation));
			FollowerLocation[] array = Enum.GetValues(typeof(FollowerLocation)) as FollowerLocation[];
			if (!GUIDropdown(_followerLocationDropdown, values4, "Home Location", "Here", delegate
			{
				SetLocationToHere(_followerLocationDropdown);
			}))
			{
				FollowerLocation followerLocation = array[_followerLocationDropdown.selectedIndex];
				GUILayout.Label("Override Task: " + TimeManager.GetOverrideTaskString());
				foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
				{
					if (allBrain.HomeLocation != followerLocation)
					{
						continue;
					}
					GUILayout.BeginHorizontal();
					bool value = false;
					ShowDetailedFollowerDebugInfo.TryGetValue(allBrain.Info.ID, out value);
					if (GUILayout.Button(value ? "-" : "+", GUILayout.Width(25f)))
					{
						ShowDetailedFollowerDebugInfo[allBrain.Info.ID] = !value;
					}
					StringBuilder stringBuilder2 = new StringBuilder();
					if (allBrain.Location == allBrain.DesiredLocation)
					{
						stringBuilder2.AppendLine(string.Format("{0} {1} cursed state: {2}: {3} ({4})", allBrain.Info.Name, allBrain.Info.ID, allBrain.Info.CursedState.ToString(), (allBrain.CurrentTask == null) ? "(null)" : allBrain.CurrentTask.ToDebugString(), allBrain.Location));
					}
					else
					{
						stringBuilder2.AppendLine(string.Format("{0} {1} cursed state: {2}: {3} ({4}, {5})", allBrain.Info.Name, allBrain.Info.ID, allBrain.Info.CursedState.ToString(), (allBrain.CurrentTask == null) ? "(null)" : allBrain.CurrentTask.ToDebugString(), allBrain.Location, allBrain.DesiredLocation));
					}
					Follower follower = FindFollower(allBrain);
					int num = CountSimFollowers(allBrain);
					if (num > 1)
					{
						stringBuilder2.AppendLine(string.Format("<color=red><b>DUPLICATE SIM FOLLOWERS FOUND ({0}), TELL MATT!</b></color>", num));
					}
					if (follower != null && !follower.IsPaused && num > 0)
					{
						stringBuilder2.AppendLine("<color=red><b>FOLLOWER AND SIM FOLLOWERS FOUND, TELL MATT!</b></color>");
					}
					if (value)
					{
						FollowerState currentState = allBrain.CurrentState;
						stringBuilder2.AppendLine(string.Format("State: {0}", (currentState != null) ? new FollowerStateType?(currentState.Type) : null));
						stringBuilder2.AppendLine(string.Format("Hunger: {0:F2}, Rest: {1:F2}, Happiness: {2:F2}, Bathroom: {3:F2} -> {4:F2}", allBrain.Stats.Satiation, allBrain.Stats.Rest, allBrain.Stats.Happiness, allBrain.Stats.Bathroom, allBrain.Stats.TargetBathroom));
						stringBuilder2.AppendLine(string.Format("Illness: {0:F2}, Adoration: {1}", allBrain.Stats.Illness, allBrain.Stats.Adoration));
						Dwelling.DwellingAndSlot dwellingAndSlot = allBrain.GetDwellingAndSlot();
						if (dwellingAndSlot != null)
						{
							stringBuilder2.AppendLine(string.Format("Dwelling ID: {0}, Slot: {1}, Level: {2}", dwellingAndSlot.ID, dwellingAndSlot.dwellingslot, dwellingAndSlot.dwellingLevel));
						}
						else
						{
							stringBuilder2.AppendLine("Dwelling: Homeless");
						}
						if (follower != null)
						{
							TrackEntry current2 = follower.Spine.state.GetCurrent(0);
							string text = ((current2 != null) ? current2.Animation.Name : null);
							TrackEntry current3 = follower.Spine.state.GetCurrent(1);
							string text2 = ((current3 != null) ? current3.Animation.Name : null);
							stringBuilder2.AppendLine("FaceAnim: " + text + ", BodyAnim: " + text2);
							SimpleSpineAnimator.SpineChartacterAnimationData animationData = follower.SimpleAnimator.GetAnimationData(StateMachine.State.Moving);
							SimpleSpineAnimator.SpineChartacterAnimationData animationData2 = follower.SimpleAnimator.GetAnimationData(StateMachine.State.Idle);
							stringBuilder2.AppendLine(string.Format("State: {0}; Anims, Move: {1}, Idle: {2}", follower.State.CURRENT_STATE, animationData.Animation.Animation.Name, animationData2.Animation.Animation.Name));
						}
					}
					GUILayout.Label(stringBuilder2.ToString());
					GUILayout.EndHorizontal();
				}
				StringBuilder stringBuilder3 = new StringBuilder();
				GUILayout.Label("Real Followers At Location:");
				foreach (Follower item in FollowerManager.FollowersAtLocation(followerLocation))
				{
					stringBuilder3.Append(string.Format("{0} {1}, ", item.Brain.Info.Name, item.Brain.Info.ID));
				}
				GUILayout.Label(stringBuilder3.ToString());
				stringBuilder3.Clear();
				GUILayout.Label("Sim Followers At Location:");
				foreach (SimFollower item2 in FollowerManager.SimFollowersAtLocation(followerLocation))
				{
					stringBuilder3.Append(string.Format("{0} {1}, ", item2.Brain.Info.Name, item2.Brain.Info.ID));
				}
				GUILayout.Label(stringBuilder3.ToString());
			}
		}
		if (_showStructureDebugInfo)
		{
			GUILayout.Label("Structures Debug:");
			Array values5 = Enum.GetValues(typeof(StructureBrain.TYPES));
			StructureBrain.TYPES[] array2 = Enum.GetValues(typeof(StructureBrain.TYPES)) as StructureBrain.TYPES[];
			if (!GUIDropdown(_structureTypeDropdown, values5, "Type"))
			{
				Array values6 = Enum.GetValues(typeof(FollowerLocation));
				FollowerLocation[] array3 = Enum.GetValues(typeof(FollowerLocation)) as FollowerLocation[];
				if (!GUIDropdown(_structureLocationDropdown, values6, "Location", "Here", delegate
				{
					SetLocationToHere(_structureLocationDropdown);
				}))
				{
					StructureBrain.TYPES type = array2[_structureTypeDropdown.selectedIndex];
					FollowerLocation followerLocation2 = array3[_structureLocationDropdown.selectedIndex];
					List<StructureBrain> list = ((followerLocation2 != FollowerLocation.None) ? StructureManager.GetAllStructuresOfType(followerLocation2, type) : StructureManager.GetAllStructuresOfType(type));
					GUILayout.Label(string.Format("Count: {0}", list.Count));
					int num2 = Mathf.Min(10, list.Count);
					for (int i = 0; i < num2; i++)
					{
						StructureBrain structureBrain = list[i];
						GUILayout.BeginHorizontal();
						bool value2 = false;
						ShowDetailedStructureDebugInfo.TryGetValue(structureBrain.Data.ID, out value2);
						if (GUILayout.Button(value2 ? "-" : "+", GUILayout.Width(25f)))
						{
							ShowDetailedStructureDebugInfo[structureBrain.Data.ID] = !value2;
						}
						StringBuilder stringBuilder4 = new StringBuilder();
						stringBuilder4.AppendLine(string.Format("ID: {0}; Type: {1}", structureBrain.Data.ID, structureBrain.Data.Type));
						if (value2)
						{
							structureBrain.ToDebugString(stringBuilder4);
						}
						GUILayout.Label(stringBuilder4.ToString());
						GUILayout.EndHorizontal();
					}
				}
			}
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	public static int CountSimFollowers(FollowerBrain brain)
	{
		int num = 0;
		for (int i = 0; i < 85; i++)
		{
			foreach (SimFollower item in FollowerManager.SimFollowersAtLocation((FollowerLocation)i))
			{
				if (item.Brain == brain && !item.Retired)
				{
					num++;
				}
			}
		}
		return num;
	}

	private Follower FindFollower(FollowerBrain brain)
	{
		Follower result = null;
		for (int i = 0; i < 85; i++)
		{
			foreach (Follower item in FollowerManager.FollowersAtLocation((FollowerLocation)i))
			{
				if (item.Brain == brain)
				{
					result = item;
					goto end_IL_0046;
				}
			}
			continue;
			end_IL_0046:
			break;
		}
		return result;
	}

	private bool GUIDropdown(DropdownData data, Array values, string label = null, string secondaryButtonLabel = null, Action onSecondaryButton = null)
	{
		GUILayout.BeginHorizontal(GUILayout.Width(250f));
		if (!string.IsNullOrEmpty(label))
		{
			GUILayout.Label(label + ":");
		}
		if (GUILayout.Button(values.GetValue(data.selectedIndex).ToString()))
		{
			data.show = !data.show;
		}
		if (secondaryButtonLabel != null && GUILayout.Button(secondaryButtonLabel) && onSecondaryButton != null)
		{
			onSecondaryButton();
		}
		GUILayout.EndHorizontal();
		Rect lastRect = GUILayoutUtility.GetLastRect();
		lastRect.height = 300f;
		if (data.show)
		{
			data.scrollViewVector = GUI.BeginScrollView(new Rect(lastRect.x, lastRect.y + 25f, lastRect.width, lastRect.height), data.scrollViewVector, new Rect(0f, 0f, lastRect.width, Mathf.Max(lastRect.height, values.Length * 25)));
			GUI.Box(new Rect(0f, 0f, lastRect.width, Mathf.Max(lastRect.height, values.Length * 25)), "");
			for (int i = 0; i < values.Length; i++)
			{
				if (GUI.Button(new Rect(0f, i * 25, lastRect.width, 25f), values.GetValue(i).ToString()))
				{
					data.show = false;
					data.selectedIndex = i;
				}
			}
			GUI.EndScrollView();
		}
		return data.show;
	}

	private void SetLocationToHere(DropdownData locationDropdownData)
	{
		FollowerLocation[] array = Enum.GetValues(typeof(FollowerLocation)) as FollowerLocation[];
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == PlayerFarming.Location)
			{
				locationDropdownData.selectedIndex = i;
				break;
			}
		}
	}
}
