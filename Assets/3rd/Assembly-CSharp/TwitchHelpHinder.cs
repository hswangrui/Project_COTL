using System;
using System.Collections;
using System.Collections.Generic;
using Map;
using MMBiomeGeneration;
using UnityEngine;

public static class TwitchHelpHinder
{
	//[Serializable]
	//public class HHData_Send
	//{
	//	public string status;

	//	public HHOption[] help_outcome_options;

	//	public HHOption[] hinder_outcome_options;
	//}

	//[Serializable]
	//public class HHData
	//{
	//	public long channel_id;

	//	public string status;

	//	public HHOption[] help_outcome_options;

	//	public HHOption[] hinder_outcome_options;

	//	public HHVotes hh_votes;

	//	public HHOptionResult[] outcome_votes;

	//	public float created_at;

	//	public float updated_at;

	//	public string hh_vote_result;

	//	public string hh_winning_option;

	//	public string outcome_winning_option;
	//}

	//[Serializable]
	//public class HHOption
	//{
	//	public string label;

	//	public string value;
	//}

	//[Serializable]
	//public class HHVotes
	//{
	//	public int help;

	//	public int hinder;
	//}

	//[Serializable]
	//public class HHOptionResult
	//{
	//	public string value;

	//	public int votes;
	//}

	//[Serializable]
	//public class FollowerTimerData
	//{
	//	public int raffle;

	//	public int creation;
	//}

	//[Serializable]
	//public class HelpHinderTimerData
	//{
	//	public int hhVote;

	//	public int outcomeVote;
	//}

	//[Serializable]
	//public class FollowerMessagesTimerData
	//{
	//	public int globalMessageCooldown;
	//}

	//[Serializable]
	//public class TimerData
	//{
	//	public FollowerTimerData followers;

	//	public HelpHinderTimerData helpHinder;

	//	public FollowerMessagesTimerData followerMessages;
	//}

	//public delegate void HHResponse(HHData data);

	//public static bool Active = false;

	//public static Vector2 TimeBetweenEvent = new Vector2(DataManager.Instance.TwitchSettings.HelpHinderFrequency * 0.75f, DataManager.Instance.TwitchSettings.HelpHinderFrequency);

	//public const string COMPLETED = "COMPLETED";

	//public const string VOTING = "HH_VOTING";

	//public const string OUTCOME_VOTING = "OUTCOME_VOTING";

	//public const string HELP = "help";

	//public const string HINDER = "hinder";

	//public const int MaxChoices = 3;

	//public static HHData CurrentData;

	//private static HHOption[] currentHelpOptions;

	//private static HHOption[] currentHinderOptions;

	//private static bool initialLoad = true;

	//public static bool Deactivated = false;

	//public const float kMinHelpHinderMinutes = 20f;

	//public const float kMaxHelpHinderMinutes = 30f;

	//public static bool Available
	//{
	//	get
	//	{
	//		return TimeManager.TotalElapsedGameTime > DataManager.Instance.TwitchNextHHEvent;
	//	}
	//}

	//public static float Timer { get; private set; }

	//public static float VotingPhaseDuration { get; private set; } = 30f;


	//public static float ChoicePhaseDuration { get; private set; } = 30f;


	//public static event HHResponse HHStatusChanged;

	//public static event HHResponse HHUpdated;

	//public static void Initialise()
	//{
	//	TwitchRequest.OnSocketReceived -= TwitchRequest_OnSocketReceived;
	//	TwitchRequest.OnSocketReceived += TwitchRequest_OnSocketReceived;
	//}

	//public static void StartHHEvent(bool isDungeon)
	//{
	//	if (Deactivated || !TwitchRequest.SocketConnected || !DataManager.Instance.TwitchSettings.HelpHinderEnabled)
	//	{
	//		return;
	//	}
	//	Active = true;
	//	Timer = 0f;
	//	SetNextEventTime();
	//	List<WorldManipulatorManager.Manipulations> list = (isDungeon ? WorldManipulatorManager.GetPossibleDungeonPositiveManipulations() : WorldManipulatorManager.GetPossibleBasePositiveManipulations());
	//	if (list.Count < 3)
	//	{
	//		return;
	//	}
	//	list.RemoveRange(3, list.Count - 3);
	//	List<WorldManipulatorManager.Manipulations> list2 = (isDungeon ? WorldManipulatorManager.GetPossibleDungeonNegativeManipulations() : WorldManipulatorManager.GetPossibleBaseNegativeManipulations());
	//	if (list2.Count < 3)
	//	{
	//		return;
	//	}
	//	list2.RemoveRange(3, list2.Count - 3);
	//	currentHelpOptions = new HHOption[list.Count];
	//	currentHinderOptions = new HHOption[list2.Count];
	//	for (int i = 0; i < currentHelpOptions.Length; i++)
	//	{
	//		currentHelpOptions[i] = new HHOption
	//		{
	//			label = WorldManipulatorManager.GetLocalisation(list[i]),
	//			value = list[i].ToString()
	//		};
	//	}
	//	for (int j = 0; j < currentHinderOptions.Length; j++)
	//	{
	//		currentHinderOptions[j] = new HHOption
	//		{
	//			label = WorldManipulatorManager.GetLocalisation(list2[j]),
	//			value = list2[j].ToString()
	//		};
	//	}
	//	SendHHEvent(new HHData_Send
	//	{
	//		status = "HH_VOTING",
	//		help_outcome_options = currentHelpOptions,
	//		hinder_outcome_options = currentHinderOptions
	//	}, delegate
	//	{
	//		if (BaseGoopDoor.Instance != null)
	//		{
	//			BaseGoopDoor.Instance.DoorUp("UI/Twitch/HH/DecidingFate");
	//			BaseGoopDoor.Instance.LockDoor = true;
	//		}
	//		NotificationCentre.Instance.PlayHelpHinderNotification("UI/Twitch/HH/DecidingFate");
	//	});
	//	TwitchRequest.Request(TwitchRequest.uri + "timers", delegate(TwitchRequest.ResponseType response, string result)
	//	{
	//		if (response == TwitchRequest.ResponseType.Success)
	//		{
	//			try
	//			{
	//				TimerData timerData = JsonUtility.FromJson<TimerData>(result);
	//				VotingPhaseDuration = timerData.helpHinder.hhVote;
	//				ChoicePhaseDuration = timerData.helpHinder.outcomeVote;
	//				TwitchFollowers.Interval = timerData.followerMessages.globalMessageCooldown;
	//				MonoSingleton<TwitchManager>.Instance.StartCoroutine(ForceEndEvent((VotingPhaseDuration + ChoicePhaseDuration) * 1.5f));
	//			}
	//			catch (Exception)
	//			{
	//			}
	//		}
	//	}, TwitchRequest.RequestType.GET, "", new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//}

	//private static IEnumerator ForceEndEvent(float delay)
	//{
	//	yield return new WaitForSecondsRealtime(delay);
	//	EndHHEvent(null);
	//}

	//public static void EndHHEvent(HHData data)
	//{
	//	if (Active)
	//	{
	//		NotificationHelpHinder.CloseNotification();
	//		Active = false;
	//		Timer = 0f;
	//		SetNextEventTime();
	//		if (BaseGoopDoor.Instance != null)
	//		{
	//			BaseGoopDoor.Instance.LockDoor = false;
	//			BaseGoopDoor.Instance.DoorDown();
	//		}
	//		CurrentData = data;
	//		HHResponse hHStatusChanged = TwitchHelpHinder.HHStatusChanged;
	//		if (hHStatusChanged != null)
	//		{
	//			hHStatusChanged(data);
	//		}
	//		WorldManipulatorManager.Manipulations result;
	//		if (data != null && Enum.TryParse<WorldManipulatorManager.Manipulations>(data.outcome_winning_option, out result))
	//		{
	//			WorldManipulatorManager.TriggerManipulation(result, 0f, true);
	//		}
	//	}
	//}

	//public static void SendHHEvent(HHData_Send data, Action sentCallback = null)
	//{
	//	string data2 = JsonUtility.ToJson(data);
	//	TwitchRequest.Request(TwitchRequest.uri + "help-or-hinder", delegate
	//	{
	//		Action action = sentCallback;
	//		if (action != null)
	//		{
	//			action();
	//		}
	//	}, TwitchRequest.RequestType.POST, data2, new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//}

	//public static void Update()
	//{
	//	if (Active)
	//	{
	//		Timer += Time.unscaledDeltaTime;
	//	}
	//}

	//private static void TwitchRequest_OnSocketReceived(string key, string data)
	//{
	//	if (key == "hh.completed")
	//	{
	//		try
	//		{
	//			EndHHEvent(JsonUtility.FromJson<HHData>(data));
	//		}
	//		catch (Exception)
	//		{
	//		}
	//	}
	//}

	//public static void LocationChanged(FollowerLocation location)
	//{
	//	if (DataManager.Instance.OnboardingFinished)
	//	{
	//		if (DataManager.Instance.TwitchNextHHEvent == -1f || initialLoad)
	//		{
	//			SetNextEventTime();
	//		}
	//		initialLoad = false;
	//		if (Available && !Active && location == FollowerLocation.Base)
	//		{
	//			StartHHEvent(false);
	//			BiomeGenerator.OnBiomeChangeRoom -= OnChangedRoom;
	//		}
	//		if (GameManager.IsDungeon(location))
	//		{
	//			BiomeGenerator.OnBiomeChangeRoom += OnChangedRoom;
	//		}
	//		else
	//		{
	//			BiomeGenerator.OnBiomeChangeRoom -= OnChangedRoom;
	//		}
	//	}
	//}

	//private static void OnChangedRoom()
	//{
	//	if (Available && !Active && MapManager.Instance != null && MapManager.Instance.CurrentNode != null && MapManager.Instance.CurrentNode.nodeType != NodeType.MiniBossFloor && UnityEngine.Random.Range(0, 100) < 30)
	//	{
	//		StartHHEvent(true);
	//		BiomeGenerator.OnBiomeChangeRoom -= OnChangedRoom;
	//	}
	//}

	//public static void SetNextEventTime()
	//{
	//	DataManager.Instance.TwitchNextHHEvent = TimeManager.TotalElapsedGameTime + UnityEngine.Random.Range(TimeBetweenEvent.x, TimeBetweenEvent.y) * 60f;
	//	BiomeGenerator.OnBiomeChangeRoom -= OnChangedRoom;
	//}

	//public static void Abort()
	//{
	//	EndHHEvent(null);
	//	initialLoad = false;
	//	TwitchRequest.OnSocketReceived -= TwitchRequest_OnSocketReceived;
	//}
}
