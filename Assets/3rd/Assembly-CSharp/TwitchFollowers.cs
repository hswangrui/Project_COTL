using System;
using System.Collections.Generic;
using UnityEngine;

public static class TwitchFollowers
{
	//[Serializable]
	//public class RaffleData
	//{
	//	public long channel_id;

	//	public int winning_viewer_id;

	//	public string created_at;

	//	public string updated_at;

	//	public int participants;

	//	public ViewerFollowerData created_follower;
	//}

	//[Serializable]
	//public class ViewerFollowerData
	//{
	//	public long channel_id;

	//	public string viewer_id;

	//	public string viewer_display_name;

	//	public string status;

	//	public FollowerData customisations;

	//	public string premium_bits_transaction_id;

	//	public string created_at;

	//	public float updated_at;

	//	public string customisation_step;

	//	public string recent_chat_message;

	//	public string id;

	//	public string save_id;
	//}

	//[Serializable]
	//public class FollowerData
	//{
	//	public string skin_name;

	//	public ColorData color;
	//}

	//[Serializable]
	//public class ColorData
	//{
	//	public int colorOptionIndex;

	//	public string HEAD_SKIN_TOP;

	//	public string HEAD_SKIN_BTM;

	//	public string ARM_LEFT_SKIN;

	//	public string ARM_RIGHT_SKIN;

	//	public string LEG_LEFT_SKIN;

	//	public string LEG_RIGHT_SKIN;

	//	public string MARKINGS;
	//}

	//[Serializable]
	//public class UnlockedSkinsData
	//{
	//	public string[] enabled_skin_names;
	//}

	//[Serializable]
	//public class FollowerInfoData
	//{
	//	public string type;

	//	public FollowerStatsData stats;
	//}

	//[Serializable]
	//public class FollowerStatsData
	//{
	//	public int level;

	//	public string reason_of_death;

	//	public FollowerTraitData[] traits;
	//}

	//[Serializable]
	//public class FollowerTraitData
	//{
	//	public string id;

	//	public string name;

	//	public string type;
	//}

	//[Serializable]
	//public class FollowerData_Send
	//{
	//	public FollowerInfoData_Send[] data;
	//}

	//[Serializable]
	//public class FollowerInfoData_Send
	//{
	//	public string id;

	//	public string name;

	//	public stats stats;

	//	public FollowerData customisations;
	//}

	//[Serializable]
	//public class stats
	//{
	//	public int level;
	//}

	//public delegate void RaffleResponse(TwitchRequest.ResponseType response, RaffleData data);

	//public delegate void FollowerAllResponse(ViewerFollowerData[] data);

	//public delegate void FollowerResponse(ViewerFollowerData data);

	//public static bool Active = false;

	//public static bool WaitingForCreation = false;

	//private static bool Request_Active = false;

	//public static RaffleData CurrentData;

	//public const string CREATED = "CREATED";

	//public const string READY_FOR_CREATION = "READY_FOR_CREATION";

	//public const string INTRO = "INTRO";

	//public const string SKIN_SELECTION = "SKIN_SELECTION";

	//public const string COLOR_SELECTION = "COLOR_SELECTION";

	//public const string VARIATION_SELECTION = "VARIATION_SELECTION";

	//public static bool Deactivated = false;

	//private static float timer = 0f;

	//public static float Interval = 120f;

	//public static event RaffleResponse RaffleUpdated;

	//public static event FollowerResponse FollowerCreated;

	//public static event FollowerResponse FollowerCreationProgress;

	//public static void Update()
	//{
	//	if (Active)
	//	{
	//		if (Request_Active || !(Time.unscaledTime > timer))
	//		{
	//			return;
	//		}
	//		Request_Active = true;
	//		GetRaffle(delegate(TwitchRequest.ResponseType response, RaffleData data)
	//		{
	//			if (data != null)
	//			{
	//				if (Active)
	//				{
	//					RaffleResponse raffleUpdated = TwitchFollowers.RaffleUpdated;
	//					if (raffleUpdated != null)
	//					{
	//						raffleUpdated(response, data);
	//					}
	//				}
	//				CurrentData = data;
	//			}
	//			Request_Active = false;
	//		});
	//		timer = Time.unscaledTime + Interval / 60f;
	//	}
	//	else
	//	{
	//		if (!WaitingForCreation || Request_Active || !(Time.unscaledTime > timer))
	//		{
	//			return;
	//		}
	//		Request_Active = true;
	//		GetFollowersAll(delegate(ViewerFollowerData[] data)
	//		{
	//			if (WaitingForCreation && CurrentData != null && data != null && data.Length != 0)
	//			{
	//				foreach (ViewerFollowerData viewerFollowerData in data)
	//				{
	//					if (viewerFollowerData != null)
	//					{
	//						string item = viewerFollowerData.viewer_id + viewerFollowerData.created_at;
	//						string created_at = CurrentData.created_at;
	//						if (viewerFollowerData.created_at == created_at)
	//						{
	//							if (viewerFollowerData.status == "CREATED" && !DataManager.Instance.TwitchFollowerViewerIDs.Contains(item))
	//							{
	//								FollowerResponse followerCreated = TwitchFollowers.FollowerCreated;
	//								if (followerCreated != null)
	//								{
	//									followerCreated(viewerFollowerData);
	//								}
	//							}
	//							else if (viewerFollowerData.status != "CREATED")
	//							{
	//								FollowerResponse followerCreationProgress = TwitchFollowers.FollowerCreationProgress;
	//								if (followerCreationProgress != null)
	//								{
	//									followerCreationProgress(viewerFollowerData);
	//								}
	//							}
	//						}
	//					}
	//				}
	//			}
	//			Request_Active = false;
	//		});
	//		timer = Time.unscaledTime + Interval / 60f;
	//	}
	//}

	//public static void GetRaffle(RaffleResponse raffleResponse)
	//{
	//	TwitchRequest.Request(TwitchRequest.uri + "followers/raffle", delegate(TwitchRequest.ResponseType response, string result)
	//	{
	//		if (response == TwitchRequest.ResponseType.Success)
	//		{
	//			try
	//			{
	//				RaffleData data = JsonUtility.FromJson<RaffleData>(result);
	//				RaffleResponse raffleResponse2 = raffleResponse;
	//				if (raffleResponse2 != null)
	//				{
	//					raffleResponse2(response, data);
	//				}
	//				return;
	//			}
	//			catch (Exception)
	//			{
	//				Debug.Log(result);
	//				RaffleResponse raffleResponse3 = raffleResponse;
	//				if (raffleResponse3 != null)
	//				{
	//					raffleResponse3(TwitchRequest.ResponseType.Failure, null);
	//				}
	//				return;
	//			}
	//		}
	//		RaffleResponse raffleResponse4 = raffleResponse;
	//		if (raffleResponse4 != null)
	//		{
	//			raffleResponse4(TwitchRequest.ResponseType.Failure, null);
	//		}
	//	}, TwitchRequest.RequestType.GET, "", new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//}

	//public static void StartRaffle(RaffleResponse raffleResponse)
	//{
	//	Request_Active = false;
	//	Active = false;
	//	TwitchRequest.Request(TwitchRequest.uri + "followers/raffle", delegate(TwitchRequest.ResponseType response, string result)
	//	{
	//		if (response == TwitchRequest.ResponseType.Success)
	//		{
	//			Active = true;
	//			try
	//			{
	//				RaffleData data = JsonUtility.FromJson<RaffleData>(result);
	//				RaffleResponse raffleResponse2 = raffleResponse;
	//				if (raffleResponse2 != null)
	//				{
	//					raffleResponse2(response, data);
	//				}
	//				return;
	//			}
	//			catch (Exception)
	//			{
	//				Debug.Log(result);
	//				RaffleResponse raffleResponse3 = raffleResponse;
	//				if (raffleResponse3 != null)
	//				{
	//					raffleResponse3(TwitchRequest.ResponseType.Failure, null);
	//				}
	//				return;
	//			}
	//		}
	//		RaffleResponse raffleResponse4 = raffleResponse;
	//		if (raffleResponse4 != null)
	//		{
	//			raffleResponse4(TwitchRequest.ResponseType.Failure, null);
	//		}
	//	}, TwitchRequest.RequestType.POST, "", new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//}

	//public static void EndRaffle(RaffleResponse raffleResponse)
	//{
	//	Active = false;
	//	Request_Active = false;
	//	WaitingForCreation = true;
	//	TwitchRequest.Request(TwitchRequest.uri + "followers/raffle/end", delegate(TwitchRequest.ResponseType response, string result)
	//	{
	//		if (response == TwitchRequest.ResponseType.Success)
	//		{
	//			try
	//			{
	//				RaffleData data = JsonUtility.FromJson<RaffleData>(result);
	//				RaffleResponse raffleResponse2 = raffleResponse;
	//				if (raffleResponse2 != null)
	//				{
	//					raffleResponse2(response, data);
	//				}
	//				RaffleResponse raffleUpdated = TwitchFollowers.RaffleUpdated;
	//				if (raffleUpdated != null)
	//				{
	//					raffleUpdated(response, data);
	//				}
	//				return;
	//			}
	//			catch (Exception)
	//			{
	//				Debug.Log(result);
	//				RaffleResponse raffleResponse3 = raffleResponse;
	//				if (raffleResponse3 != null)
	//				{
	//					raffleResponse3(TwitchRequest.ResponseType.Failure, null);
	//				}
	//				RaffleResponse raffleUpdated2 = TwitchFollowers.RaffleUpdated;
	//				if (raffleUpdated2 != null)
	//				{
	//					raffleUpdated2(TwitchRequest.ResponseType.Failure, null);
	//				}
	//				return;
	//			}
	//		}
	//		RaffleResponse raffleResponse4 = raffleResponse;
	//		if (raffleResponse4 != null)
	//		{
	//			raffleResponse4(TwitchRequest.ResponseType.Failure, null);
	//		}
	//	}, TwitchRequest.RequestType.POST, "", new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//}

	//public static void GetFollowerVariations()
	//{
	//	TwitchRequest.Request(TwitchRequest.uri + "followers/variations", delegate
	//	{
	//	}, TwitchRequest.RequestType.GET, "", new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//}

	//public static void SetFollowerVariations(List<string> followerNames)
	//{
	//	UnlockedSkinsData unlockedSkinsData = new UnlockedSkinsData();
	//	unlockedSkinsData.enabled_skin_names = new string[followerNames.Count];
	//	for (int i = 0; i < followerNames.Count; i++)
	//	{
	//		unlockedSkinsData.enabled_skin_names[i] = followerNames[i];
	//	}
	//	string data = JsonUtility.ToJson(unlockedSkinsData);
	//	TwitchRequest.Request(TwitchRequest.uri + "channel", delegate
	//	{
	//	}, TwitchRequest.RequestType.PATCH, data, new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//}

	//public static void GetFollowersAll(FollowerAllResponse response)
	//{
	//	TwitchRequest.Request(TwitchRequest.uri + "followers", delegate(TwitchRequest.ResponseType res, string result)
	//	{
	//		if (res == TwitchRequest.ResponseType.Success)
	//		{
	//			List<string> list = new List<string> { "" };
	//			for (int i = 0; i < result.Length; i++)
	//			{
	//				if (result[i] != '[' || i != 0)
	//				{
	//					if (result[i] == ',' && result[i - 1] == '}' && result[i + 1] == '{' && result[i - 2] != '"')
	//					{
	//						list.Insert(0, "");
	//					}
	//					else
	//					{
	//						if (result[i] == ']' && i == result.Length - 1)
	//						{
	//							break;
	//						}
	//						list[0] += result[i];
	//					}
	//				}
	//			}
	//			ViewerFollowerData[] array = new ViewerFollowerData[list.Count];
	//			for (int j = 0; j < array.Length; j++)
	//			{
	//				array[j] = JsonUtility.FromJson<ViewerFollowerData>(list[j]);
	//			}
	//			FollowerAllResponse followerAllResponse = response;
	//			if (followerAllResponse != null)
	//			{
	//				followerAllResponse(array);
	//			}
	//		}
	//	}, TwitchRequest.RequestType.GET, "", new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//}

	//public static void WaitingForCreationCancelled()
	//{
	//	Abort();
	//}

	//public static void SendFollowers(Action callback)
	//{
	//	if (!TwitchAuthentication.IsAuthenticated || Deactivated)
	//	{
	//		return;
	//	}
	//	List<FollowerInfo> list = new List<FollowerInfo>(DataManager.Instance.Followers);
	//	list.AddRange(DataManager.Instance.Followers_Dead);
	//	for (int num = list.Count - 1; num >= 0; num--)
	//	{
	//		if (!string.IsNullOrEmpty(list[num].ViewerID))
	//		{
	//			list.RemoveAt(num);
	//		}
	//	}
	//	FollowerInfoData_Send[] array = new FollowerInfoData_Send[list.Count];
	//	for (int i = 0; i < array.Length; i++)
	//	{
	//		array[i] = new FollowerInfoData_Send();
	//		array[i].id = list[i].ID.ToString();
	//		array[i].name = list[i].Name;
	//		array[i].stats = new stats
	//		{
	//			level = list[i].XPLevel
	//		};
	//		array[i].customisations = new FollowerData
	//		{
	//			skin_name = list[i].SkinName,
	//			color = new ColorData()
	//		};
	//		WorshipperData.SkinAndData colourData = WorshipperData.Instance.GetColourData(list[i].SkinName);
	//		if (colourData == null)
	//		{
	//			continue;
	//		}
	//		foreach (WorshipperData.SlotAndColor slotAndColour in colourData.SlotAndColours[Mathf.Clamp(list[i].SkinColour, 0, colourData.SlotAndColours.Count - 1)].SlotAndColours)
	//		{
	//			if (slotAndColour.Slot == "HEAD_SKIN_TOP")
	//			{
	//				array[i].customisations.color.HEAD_SKIN_TOP = string.Format("{0}, {1}, {2}", Mathf.RoundToInt(slotAndColour.color.r * 255f), Mathf.RoundToInt(slotAndColour.color.g * 255f), Mathf.RoundToInt(slotAndColour.color.b * 255f));
	//			}
	//			else if (slotAndColour.Slot == "HEAD_SKIN_BTM")
	//			{
	//				array[i].customisations.color.HEAD_SKIN_BTM = string.Format("{0}, {1}, {2}", Mathf.RoundToInt(slotAndColour.color.r * 255f), Mathf.RoundToInt(slotAndColour.color.g * 255f), Mathf.RoundToInt(slotAndColour.color.b * 255f));
	//			}
	//			else if (slotAndColour.Slot == "ARM_LEFT_SKIN")
	//			{
	//				array[i].customisations.color.ARM_LEFT_SKIN = string.Format("{0}, {1}, {2}", Mathf.RoundToInt(slotAndColour.color.r * 255f), Mathf.RoundToInt(slotAndColour.color.g * 255f), Mathf.RoundToInt(slotAndColour.color.b * 255f));
	//			}
	//			else if (slotAndColour.Slot == "ARM_RIGHT_SKIN")
	//			{
	//				array[i].customisations.color.ARM_RIGHT_SKIN = string.Format("{0}, {1}, {2}", Mathf.RoundToInt(slotAndColour.color.r * 255f), Mathf.RoundToInt(slotAndColour.color.g * 255f), Mathf.RoundToInt(slotAndColour.color.b * 255f));
	//			}
	//			else if (slotAndColour.Slot == "LEG_LEFT_SKIN")
	//			{
	//				array[i].customisations.color.LEG_LEFT_SKIN = string.Format("{0}, {1}, {2}", Mathf.RoundToInt(slotAndColour.color.r * 255f), Mathf.RoundToInt(slotAndColour.color.g * 255f), Mathf.RoundToInt(slotAndColour.color.b * 255f));
	//			}
	//			else if (slotAndColour.Slot == "LEG_RIGHT_SKIN")
	//			{
	//				array[i].customisations.color.LEG_RIGHT_SKIN = string.Format("{0}, {1}, {2}", Mathf.RoundToInt(slotAndColour.color.r * 255f), Mathf.RoundToInt(slotAndColour.color.g * 255f), Mathf.RoundToInt(slotAndColour.color.b * 255f));
	//			}
	//			else if (slotAndColour.Slot == "MARKINGS")
	//			{
	//				array[i].customisations.color.MARKINGS = string.Format("{0}, {1}, {2}", Mathf.RoundToInt(slotAndColour.color.r * 255f), Mathf.RoundToInt(slotAndColour.color.g * 255f), Mathf.RoundToInt(slotAndColour.color.b * 255f));
	//			}
	//		}
	//	}
	//	string text = JsonUtility.ToJson(new FollowerData_Send
	//	{
	//		data = array
	//	});
	//	text = text.Remove(0, 8);
	//	text = text.Remove(text.Length - 1, 1);
	//	TwitchRequest.Request(TwitchRequest.uri + "followers/npcs", delegate
	//	{
	//		Action action = callback;
	//		if (action != null)
	//		{
	//			action();
	//		}
	//	}, TwitchRequest.RequestType.POST, text, new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//}

	//public static void SendFollowersAllData()
	//{
	//	SendFollowers(delegate
	//	{
	//		List<FollowerInfo> list = new List<FollowerInfo>(DataManager.Instance.Followers);
	//		list.AddRange(DataManager.Instance.Followers_Dead);
	//		for (int i = 0; i < list.Count; i++)
	//		{
	//			SendFollowerInformation(list[i]);
	//		}
	//	});
	//}

	//public static void SendFollowerInformation(FollowerInfo info)
	//{
	//	if (TwitchAuthentication.IsAuthenticated && !Deactivated)
	//	{
	//		bool flag = string.IsNullOrEmpty(info.ViewerID);
	//		string text = (flag ? info.ID.ToString() : info.ViewerID);
	//		FollowerStatsData followerStatsData = new FollowerStatsData
	//		{
	//			level = info.XPLevel,
	//			reason_of_death = (DataManager.Instance.Followers_Dead_IDs.Contains(info.ID) ? info.GetDeathText(false) : ""),
	//			traits = new FollowerTraitData[info.Traits.Count]
	//		};
	//		for (int i = 0; i < info.Traits.Count; i++)
	//		{
	//			followerStatsData.traits[i] = new FollowerTraitData
	//			{
	//				id = info.Traits[i].ToString(),
	//				name = FollowerTrait.GetLocalizedTitle(info.Traits[i]),
	//				type = (FollowerTrait.IsPositiveTrait(info.Traits[i]) ? "positive" : "negative")
	//			};
	//		}
	//		string data = JsonUtility.ToJson(new FollowerInfoData
	//		{
	//			type = (flag ? "npc" : "viewer"),
	//			stats = followerStatsData
	//		});
	//		TwitchRequest.Request(TwitchRequest.uri + "followers/" + text, delegate
	//		{
	//			int num = 1;
	//		}, TwitchRequest.RequestType.PATCH, data, new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//	}
	//}

	//public static void Abort()
	//{
	//	EndRaffle(null);
	//	WaitingForCreation = false;
	//}
}
