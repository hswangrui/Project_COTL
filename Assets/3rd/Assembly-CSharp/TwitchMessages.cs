using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TwitchMessages
{
	//[Serializable]
	//public class MessageData_Send
	//{
	//	public long channel_id;

	//	public string viewer_id;

	//	public string follower_id;

	//	public long save_id;

	//	public string message;

	//	public bool is_read;

	//	public bool is_approved;

	//	public string approver_id;

	//	public string id;
	//}

	//[Serializable]
	//public class MessageData_Read
	//{
	//	public string[] message_ids;
	//}

	//private static List<MessageData_Send> queuedMessages = new List<MessageData_Send>();

	//private static float timeSinceLastShownMessage;

	//private const float timeBetweenMessages = 60f;

	//public static bool Deactivated = false;

	//public static void Initialise()
	//{
	//	TwitchRequest.OnSocketReceived -= TwitchRequest_OnSocketReceived;
	//	TwitchRequest.OnSocketReceived += TwitchRequest_OnSocketReceived;
	//	LoadAllMessages();
	//}

	//public static void LoadAllMessages()
	//{
	//	if (!DataManager.Instance.TwitchSettings.TwitchMessagesEnabled || Deactivated)
	//	{
	//		return;
	//	}
	//	TwitchRequest.Request(TwitchRequest.uri + "followers/messages", delegate(TwitchRequest.ResponseType response, string result)
	//	{
	//		try
	//		{
	//			List<string> list = new List<string> { "" };
	//			for (int i = 0; i < result.Length; i++)
	//			{
	//				if (result[i] != '[')
	//				{
	//					if (result[i] == ',' && result[i - 1] == '}' && result[i + 1] == '{')
	//					{
	//						list.Insert(0, "");
	//					}
	//					else
	//					{
	//						if (result[i] == ']')
	//						{
	//							break;
	//						}
	//						list[0] += result[i];
	//					}
	//				}
	//			}
	//			MessageData_Send[] array = new MessageData_Send[list.Count];
	//			for (int j = 0; j < array.Length; j++)
	//			{
	//				array[j] = JsonUtility.FromJson<MessageData_Send>(list[j]);
	//			}
	//			queuedMessages = array.ToList();
	//			for (int num = queuedMessages.Count - 1; num >= 0; num--)
	//			{
	//				if (!queuedMessages[num].is_approved || queuedMessages[num].is_read)
	//				{
	//					queuedMessages.RemoveAt(num);
	//				}
	//			}
	//		}
	//		catch (Exception)
	//		{
	//		}
	//	}, TwitchRequest.RequestType.GET, "", new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//}

	//private static void TwitchRequest_OnSocketReceived(string key, string data)
	//{
	//	if (!(key == "followerMessages.add"))
	//	{
	//		return;
	//	}
	//	try
	//	{
	//		MessageData_Send messageData_Send = JsonUtility.FromJson<MessageData_Send>(data);
	//		if (messageData_Send.is_approved)
	//		{
	//			queuedMessages.Add(messageData_Send);
	//		}
	//	}
	//	catch (Exception)
	//	{
	//	}
	//}

	//public static void ReadMessage(string messageID)
	//{
	//	MessageData_Read messageData_Read = new MessageData_Read();
	//	messageData_Read.message_ids = new string[1] { messageID };
	//	string data = JsonUtility.ToJson(messageData_Read);
	//	TwitchRequest.Request(TwitchRequest.uri + "followers/messages/read", delegate
	//	{
	//	}, TwitchRequest.RequestType.POST, data, new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//}

	//public static void ReadMessages(List<string> messageIDs)
	//{
	//	MessageData_Read messageData_Read = new MessageData_Read();
	//	messageData_Read.message_ids = new string[messageIDs.Count];
	//	for (int i = 0; i < messageIDs.Count; i++)
	//	{
	//		messageData_Read.message_ids[i] = messageIDs[i];
	//	}
	//	string data = JsonUtility.ToJson(messageData_Read);
	//	TwitchRequest.Request(TwitchRequest.uri + "followers/messages/read", delegate
	//	{
	//	}, TwitchRequest.RequestType.POST, data, new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//}

	//public static void Update()
	//{
	//	if (PlayerFarming.Location != FollowerLocation.Base || !(Time.time > timeSinceLastShownMessage) || queuedMessages.Count <= 0 || !DataManager.Instance.TwitchSettings.TwitchMessagesEnabled || Deactivated)
	//	{
	//		return;
	//	}
	//	timeSinceLastShownMessage = Time.time + 60f;
	//	if (queuedMessages[queuedMessages.Count - 1] != null)
	//	{
	//		MessageData_Send messageData_Send = queuedMessages[queuedMessages.Count - 1];
	//		Follower follower = FollowerManager.FindFollowerByViewerID(messageData_Send.viewer_id);
	//		if (follower != null)
	//		{
	//			follower.ShowBarkMessage(messageData_Send.message);
	//		}
	//		ReadMessage(messageData_Send.id);
	//		queuedMessages.Remove(messageData_Send);
	//	}
	//	else
	//	{
	//		queuedMessages.RemoveAt(queuedMessages.Count - 1);
	//	}
	//}

	//public static void Abort()
	//{
	//	TwitchRequest.OnSocketReceived -= TwitchRequest_OnSocketReceived;
	//}
}
