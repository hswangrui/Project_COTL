using System;
using System.Collections.Generic;
using UnityEngine;

public static class TwitchTotem
{
	//[Serializable]
	//public class TotemData
	//{
	//	public long channel_id;

	//	public string save_id;

	//	public string type;

	//	public string message;

	//	public int contributions;

	//	public MetaData metadata;
	//}

	//[Serializable]
	//public class MetaData
	//{
	//	public string viewer_id;

	//	public string viewer_display_name;

	//	public TotemMetaData totem;
	//}

	//[Serializable]
	//public class TotemMetaData
	//{
	//	public long channel_id;

	//	public int contributions;

	//	public string save_id;

	//	public string created_at;
	//}

	//[Serializable]
	//public class NotificationsReadData
	//{
	//	public int[] notification_ids;
	//}

	//public delegate void TotemResponse(int contributions);

	//private static bool Request_Active;

	//public const int ContributionTarget = 10;

	//public static int CurrentContributions;

	//private static float heartbeatTimestamp;

	//public static bool Disabled;

	//public static bool TotemUnlockAvailable
	//{
	//	get
	//	{
	//		return Contributions >= 10;
	//	}
	//}

	//public static int Contributions
	//{
	//	get
	//	{
	//		return CurrentContributions - 10 * TwitchTotemsCompleted;
	//	}
	//}

	//public static int TwitchTotemsCompleted
	//{
	//	get
	//	{
	//		return DataManager.Instance.TwitchTotemsCompleted;
	//	}
	//	set
	//	{
	//		DataManager.Instance.TwitchTotemsCompleted = value;
	//	}
	//}

	//public static bool Deactivated
	//{
	//	get
	//	{
	//		if (!Disabled)
	//		{
	//			return !DataManager.Instance.TwitchSettings.TotemEnabled;
	//		}
	//		return true;
	//	}
	//}

	//public static event TotemResponse TotemUpdated;

	//public static void Initialise()
	//{
	//	TwitchRequest.OnSocketReceived -= TwitchRequest_OnSocketReceived;
	//	TwitchRequest.OnSocketReceived += TwitchRequest_OnSocketReceived;
	//	GetTotemStatus();
	//}

	//public static void GetTotemStatus()
	//{
	//	if (Deactivated)
	//	{
	//		return;
	//	}
	//	TwitchRequest.Request(TwitchRequest.uri + "totem", delegate(TwitchRequest.ResponseType response, string result)
	//	{
	//		try
	//		{
	//			CurrentContributions = JsonUtility.FromJson<TotemData>(result).contributions;
	//			TotemResponse totemUpdated = TwitchTotem.TotemUpdated;
	//			if (totemUpdated != null)
	//			{
	//				totemUpdated(CurrentContributions);
	//			}
	//		}
	//		catch (Exception)
	//		{
	//		}
	//	}, TwitchRequest.RequestType.GET, "", new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//}

	//private static void TwitchRequest_OnSocketReceived(string key, string data)
	//{
	//	if (!(key == "notifications.add") || Deactivated)
	//	{
	//		return;
	//	}
	//	try
	//	{
	//		TotemData totemData = JsonUtility.FromJson<TotemData>(data);
	//		if (totemData.type == "TOTEM_CONTRIBUTION")
	//		{
	//			CurrentContributions = totemData.metadata.totem.contributions;
	//			TotemResponse totemUpdated = TwitchTotem.TotemUpdated;
	//			if (totemUpdated != null)
	//			{
	//				totemUpdated(CurrentContributions);
	//			}
	//			TwitchManager.NotificationReceived(totemData.metadata.viewer_display_name, "TOTEM_CONTRIBUTION");
	//		}
	//	}
	//	catch (Exception)
	//	{
	//	}
	//}

	//public static void Update()
	//{
	//	if (!Request_Active && Time.unscaledTime > heartbeatTimestamp && !Disabled)
	//	{
	//		Request_Active = true;
	//		heartbeatTimestamp = Time.unscaledTime + 300f;
	//		TwitchRequest.Request(TwitchRequest.uri + "channel/heartbeat", delegate
	//		{
	//			Request_Active = false;
	//		}, TwitchRequest.RequestType.POST, "", new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//	}
	//}

	//public static void SetNotificationRead(int notificationID)
	//{
	//	NotificationsReadData notificationsReadData = new NotificationsReadData();
	//	notificationsReadData.notification_ids = new int[1] { notificationID };
	//	string data = JsonUtility.ToJson(notificationsReadData);
	//	TwitchRequest.Request(TwitchRequest.uri + "channel/notifications/read", delegate
	//	{
	//	}, TwitchRequest.RequestType.POST, data, new KeyValuePair<string, string>("x-cotl-debug-twitch-channel-id", TwitchManager.ChannelID));
	//}

	//public static void TotemRewardClaimed()
	//{
	//	TwitchTotemsCompleted++;
	//	TotemResponse totemUpdated = TwitchTotem.TotemUpdated;
	//	if (totemUpdated != null)
	//	{
	//		totemUpdated(CurrentContributions);
	//	}
	//}

	//public static void Abort()
	//{
	//	TwitchRequest.OnSocketReceived -= TwitchRequest_OnSocketReceived;
	//}
}
