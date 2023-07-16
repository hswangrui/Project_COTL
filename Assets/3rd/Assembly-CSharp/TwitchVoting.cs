using System;
using System.Collections.Generic;
using UnityEngine;

public static class TwitchVoting
{
	//[Serializable]
	//public class VotingData_Result
	//{
	//	public string selected_follower;
	//}

	//[Serializable]
	//public class VotingData_Send
	//{
	//	public string reason;

	//	public string[] allowed_follower_ids;
	//}

	//[Serializable]
	//public class Voting_Update
	//{
	//	public int amount_of_votes;
	//}

	//public enum VotingType
	//{
	//	DEMON,
	//	PRISON,
	//	MISSIONARY,
	//	CONFESSION,
	//	FOLLOWER_TO_GAIN_XP,
	//	HEALING_BAY,
	//	SACRIFICE_TO_NIGHT_FOX,
	//	SARIFICE_TO_MIDAS,
	//	RITUAL_ASCEND,
	//	RITUAL_ENFORCER,
	//	RITUAL_FIGHT_PIT,
	//	RITUAL_FUNERAL,
	//	RITUAL_RESURRECT,
	//	RITUAL_MARRY,
	//	RITUAL_SACRIFICE,
	//	BED,
	//	SACRIFICE_TO_MIDAS,
	//	SACRIFICE_TO_DOOR
	//}

	//public delegate void VotingReadyResponse(bool ready);

	//public delegate void VotingResponse(FollowerBrain result);

	//public delegate void VotingEvent(int totalVotes);

	//public static bool Active;

	//public static bool Deactivated;

	//private static bool Request_Active;

	//private static float timer;

	//private const float interval = 3f;

	//public static event VotingEvent OnVotingUpdated;

	//public static void StartVoting(VotingType reason, List<FollowerInfo> followers, VotingReadyResponse votingReadyResponse)
	//{
	//	List<FollowerBrain> list = new List<FollowerBrain>();
	//	foreach (FollowerInfo follower in followers)
	//	{
	//		list.Add(FollowerBrain.GetOrCreateBrain(follower));
	//	}
	//	StartVoting(reason, list, votingReadyResponse);
	//}

	//public static void StartVoting(VotingType reason, List<Follower> followers, VotingReadyResponse votingReadyResponse)
	//{
	//	List<FollowerBrain> list = new List<FollowerBrain>();
	//	foreach (Follower follower in followers)
	//	{
	//		list.Add(follower.Brain);
	//	}
	//	StartVoting(reason, list, votingReadyResponse);
	//}

	//public static void StartVoting(VotingType reason, List<FollowerBrain> followers, VotingReadyResponse votingReadyResponse)
	//{
	//	if (!TwitchAuthentication.IsAuthenticated || Deactivated)
	//	{
	//		return;
	//	}
	//	Active = true;
	//	VotingData_Send votingData_Send = new VotingData_Send();
	//	votingData_Send.reason = reason.ToString();
	//	votingData_Send.allowed_follower_ids = new string[followers.Count];
	//	for (int i = 0; i < followers.Count; i++)
	//	{
	//		votingData_Send.allowed_follower_ids[i] = (string.IsNullOrEmpty(followers[i].Info.ViewerID) ? followers[i].Info.ID.ToString() : followers[i].Info.ViewerID);
	//	}
	//	Request_Active = false;
	//	string data = JsonUtility.ToJson(votingData_Send);
	//	TwitchRequest.Request(TwitchRequest.uri + "followers/selection-event", delegate(TwitchRequest.ResponseType response, string result)
	//	{
	//		if (response == TwitchRequest.ResponseType.Success)
	//		{
	//			VotingReadyResponse votingReadyResponse2 = votingReadyResponse;
	//			if (votingReadyResponse2 != null)
	//			{
	//				votingReadyResponse2(true);
	//			}
	//		}
	//		else
	//		{
	//			VotingReadyResponse votingReadyResponse3 = votingReadyResponse;
	//			if (votingReadyResponse3 != null)
	//			{
	//				votingReadyResponse3(false);
	//			}
	//		}
	//	}, TwitchRequest.RequestType.POST, data, new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//}

	//public static void EndVoting(VotingResponse raffleResponse)
	//{
	//	if (!TwitchAuthentication.IsAuthenticated || Deactivated)
	//	{
	//		return;
	//	}
	//	Active = false;
	//	TwitchRequest.Request(TwitchRequest.uri + "followers/selection-event/end", delegate(TwitchRequest.ResponseType response, string result)
	//	{
	//		if (response == TwitchRequest.ResponseType.Success)
	//		{
	//			try
	//			{
	//				VotingData_Result votingData_Result = JsonUtility.FromJson<VotingData_Result>(result);
	//				FollowerInfo followerInfo = null;
	//				followerInfo = FollowerInfo.GetInfoByViewerID(votingData_Result.selected_follower, true);
	//				if (followerInfo == null)
	//				{
	//					followerInfo = FollowerInfo.GetInfoByID(int.Parse(votingData_Result.selected_follower), true);
	//				}
	//				VotingResponse votingResponse = raffleResponse;
	//				if (votingResponse != null)
	//				{
	//					votingResponse((followerInfo != null) ? FollowerBrain.GetOrCreateBrain(followerInfo) : null);
	//				}
	//				return;
	//			}
	//			catch (Exception)
	//			{
	//				Debug.Log(result);
	//				VotingResponse votingResponse2 = raffleResponse;
	//				if (votingResponse2 != null)
	//				{
	//					votingResponse2(null);
	//				}
	//				return;
	//			}
	//		}
	//		VotingResponse votingResponse3 = raffleResponse;
	//		if (votingResponse3 != null)
	//		{
	//			votingResponse3(null);
	//		}
	//	}, TwitchRequest.RequestType.POST, "", new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//}

	//public static void Update()
	//{
	//	if (Request_Active || !(Time.unscaledTime > timer) || !Active)
	//	{
	//		return;
	//	}
	//	Request_Active = true;
	//	TwitchRequest.Request(TwitchRequest.uri + "followers/selection-event", delegate(TwitchRequest.ResponseType response, string result)
	//	{
	//		Voting_Update voting_Update = JsonUtility.FromJson<Voting_Update>(result);
	//		VotingEvent onVotingUpdated = TwitchVoting.OnVotingUpdated;
	//		if (onVotingUpdated != null)
	//		{
	//			onVotingUpdated(voting_Update.amount_of_votes);
	//		}
	//		Request_Active = false;
	//	}, TwitchRequest.RequestType.GET, "", new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//	timer = Time.unscaledTime + 3f;
	//}

	//public static void Abort()
	//{
	//	EndVoting(null);
	//}
}
