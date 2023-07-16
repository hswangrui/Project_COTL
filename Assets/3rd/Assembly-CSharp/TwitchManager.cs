using System;
using UnityEngine;

public class TwitchManager : MonoSingleton<TwitchManager>
{
	//public delegate void NotificationResponse(string viewerDisplayName, string notificationType);

	//public static Action<bool> OnHelpHinderEnabledChanged;

	//public static Action<float> OnHelpHinderFrequencyChanged;

	//public static Action<bool> OnTotemEnabledChanged;

	//public static Action<bool> OnFollowerNamesEnabledChanged;

	//public static Action<bool> TwitchMessagesEnabledChanged;

	//public const string FOLLOWER_CREATED = "FOLLOWER_CREATED";

	//public const string FOLLOWER_RAFFLE_WINNER = "FOLLOWER_RAFFLE_WINNER";

	//public const string TOTEM_CONTRIBUTION = "TOTEM_CONTRIBUTION";

	//public static string SecretKey
	//{
	//	get
	//	{
	//		return DataManager.Instance.TwitchSecretKey;
	//	}
	//	set
	//	{
	//		DataManager.Instance.TwitchSecretKey = value;
	//	}
	//}

	//public static string ChannelID
	//{
	//	get
	//	{
	//		return DataManager.Instance.ChannelID;
	//	}
	//	set
	//	{
	//		DataManager.Instance.ChannelID = value;
	//	}
	//}

	//public static string ChannelName
	//{
	//	get
	//	{
	//		return DataManager.Instance.ChannelName;
	//	}
	//	set
	//	{
	//		DataManager.Instance.ChannelName = value;
	//	}
	//}

	//public static bool HelpHinderEnabled
	//{
	//	get
	//	{
	//		return DataManager.Instance.TwitchSettings.HelpHinderEnabled;
	//	}
	//	set
	//	{
	//		if (DataManager.Instance.TwitchSettings.HelpHinderEnabled != value)
	//		{
	//			Debug.Log(string.Format("Twitch Settings - Help/Hinder value changed to {0}", value).Colour(Color.yellow));
	//			DataManager.Instance.TwitchSettings.HelpHinderEnabled = value;
	//			Action<bool> onHelpHinderEnabledChanged = OnHelpHinderEnabledChanged;
	//			if (onHelpHinderEnabledChanged != null)
	//			{
	//				onHelpHinderEnabledChanged(value);
	//			}
	//		}
	//	}
	//}

	//public static float HelpHinderFrequency
	//{
	//	get
	//	{
	//		return DataManager.Instance.TwitchSettings.HelpHinderFrequency;
	//	}
	//	set
	//	{
	//		if (!DataManager.Instance.TwitchSettings.HelpHinderFrequency.Equals(value))
	//		{
	//			Debug.Log(string.Format("Twitch Settings - Help/Hinder Frequency value changed to {0}", value).Colour(Color.yellow));
	//			DataManager.Instance.TwitchSettings.HelpHinderFrequency = value;
	//			Action<float> onHelpHinderFrequencyChanged = OnHelpHinderFrequencyChanged;
	//			if (onHelpHinderFrequencyChanged != null)
	//			{
	//				onHelpHinderFrequencyChanged(value);
	//			}
	//		}
	//	}
	//}

	//public static bool TotemEnabled
	//{
	//	get
	//	{
	//		return DataManager.Instance.TwitchSettings.TotemEnabled;
	//	}
	//	set
	//	{
	//		if (DataManager.Instance.TwitchSettings.TotemEnabled != value)
	//		{
	//			Debug.Log(string.Format("Twitch Settings - Totem Enabled value changed to {0}", value).Colour(Color.yellow));
	//			DataManager.Instance.TwitchSettings.TotemEnabled = value;
	//			Action<bool> onTotemEnabledChanged = OnTotemEnabledChanged;
	//			if (onTotemEnabledChanged != null)
	//			{
	//				onTotemEnabledChanged(value);
	//			}
	//		}
	//	}
	//}

	//public static bool FollowerNamesEnabled
	//{
	//	get
	//	{
	//		return DataManager.Instance.TwitchSettings.FollowerNamesEnabled;
	//	}
	//	set
	//	{
	//		if (DataManager.Instance.TwitchSettings.FollowerNamesEnabled != value)
	//		{
	//			Debug.Log(string.Format("Twitch Settings - Show Twitch Follower Names value changed to {0}", value).Colour(Color.yellow));
	//			DataManager.Instance.TwitchSettings.FollowerNamesEnabled = value;
	//			Action<bool> onFollowerNamesEnabledChanged = OnFollowerNamesEnabledChanged;
	//			if (onFollowerNamesEnabledChanged != null)
	//			{
	//				onFollowerNamesEnabledChanged(value);
	//			}
	//		}
	//	}
	//}

	//public static bool MessagesEnabled
	//{
	//	get
	//	{
	//		return DataManager.Instance.TwitchSettings.TwitchMessagesEnabled;
	//	}
	//	set
	//	{
	//		if (DataManager.Instance.TwitchSettings.TwitchMessagesEnabled != value)
	//		{
	//			Debug.Log(string.Format("Twitch Settings - Twitch Messages value changed to {0}", value).Colour(Color.yellow));
	//			DataManager.Instance.TwitchSettings.TwitchMessagesEnabled = value;
	//			Action<bool> twitchMessagesEnabledChanged = TwitchMessagesEnabledChanged;
	//			if (twitchMessagesEnabledChanged != null)
	//			{
	//				twitchMessagesEnabledChanged(value);
	//			}
	//		}
	//	}
	//}

	//public static event NotificationResponse OnNotificationReceived;

	//[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	//public static void InitializeTwitchManager()
	//{
	//	GameObject obj = new GameObject();
	//	obj.name = "Twitch Manager";
	//	obj.AddComponent<TwitchManager>();
	//}

	//public static void UpdateEvents()
	//{
	//	if (TwitchAuthentication.IsAuthenticated)
	//	{
	//		TwitchHelpHinder.Update();
	//		TwitchTotem.Update();
	//		TwitchFollowers.Update();
	//		TwitchMessages.Update();
	//		TwitchVoting.Update();
	//	}
	//}

	//public static void LocationChanged(FollowerLocation location)
	//{
	//	if (TwitchAuthentication.IsAuthenticated)
	//	{
	//		TwitchHelpHinder.LocationChanged(location);
	//	}
	//}

	//public static void NotificationReceived(string viewerDisplayName, string notificationType)
	//{
	//	NotificationResponse onNotificationReceived = TwitchManager.OnNotificationReceived;
	//	if (onNotificationReceived != null)
	//	{
	//		onNotificationReceived(viewerDisplayName, notificationType);
	//	}
	//}

	//public static void Abort()
	//{
	//	if (TwitchAuthentication.IsAuthenticated)
	//	{
	//		TwitchVoting.Abort();
	//		TwitchFollowers.Abort();
	//		TwitchHelpHinder.Abort();
	//		TwitchTotem.Abort();
	//		TwitchRequest.Abort();
	//		TwitchMessages.Abort();
	//	}
	//}
}
