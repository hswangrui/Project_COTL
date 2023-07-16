using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

[Serializable]
public class Objectives_TalkToFollower : ObjectivesData
{
	[Serializable]
	public class FinalizedData_TalkToFollower : ObjectivesDataFinalized
	{
		public string LocKey;

		public string TargetFollowerName;

		public override string GetText()
		{
			return string.Format(LocalizationManager.GetTranslation(LocKey), TargetFollowerName);
		}
	}

	public bool Done;

	public string ResponseTerm = "";

	public int TargetFollower = -1;

	public override string Text
	{
		get
		{
			if (string.IsNullOrEmpty(ResponseTerm))
			{
				string collectReward = ScriptLocalization.Objectives.CollectReward;
				FollowerInfo infoByID = FollowerInfo.GetInfoByID(base.Follower, true);
				return string.Format(collectReward, (infoByID != null) ? infoByID.Name : null);
			}
			string talkToFollower = ScriptLocalization.Objectives.TalkToFollower;
			FollowerInfo infoByID2 = FollowerInfo.GetInfoByID(TargetFollower, true);
			return string.Format(talkToFollower, (infoByID2 != null) ? infoByID2.Name : null);
		}
	}

	public Objectives_TalkToFollower()
	{
	}

	public Objectives_TalkToFollower(string groupId, string term = "", float expireTimestamp = -1f)
		: base(groupId, expireTimestamp)
	{
		Type = Objectives.TYPES.TALK_TO_FOLLOWER;
		ResponseTerm = term;
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		FinalizedData_TalkToFollower finalizedData_TalkToFollower = new FinalizedData_TalkToFollower
		{
			GroupId = GroupId,
			Index = Index,
			UniqueGroupID = UniqueGroupID
		};
		if (string.IsNullOrEmpty(ResponseTerm))
		{
			finalizedData_TalkToFollower.LocKey = "Objectives/CollectReward";
			FollowerInfo infoByID = FollowerInfo.GetInfoByID(base.Follower, true);
			finalizedData_TalkToFollower.TargetFollowerName = ((infoByID != null) ? infoByID.Name : null);
		}
		else
		{
			finalizedData_TalkToFollower.LocKey = "Objectives/TalkToFollower";
			FollowerInfo infoByID2 = FollowerInfo.GetInfoByID(TargetFollower, true);
			finalizedData_TalkToFollower.TargetFollowerName = ((infoByID2 != null) ? infoByID2.Name : null);
		}
		return finalizedData_TalkToFollower;
	}

	protected override bool CheckComplete()
	{
		if (string.IsNullOrEmpty(ResponseTerm))
		{
			FollowerInfo infoByID = FollowerInfo.GetInfoByID(base.Follower);
			if (!IsFailed && base.Follower != -1 && (infoByID == null || DataManager.Instance.Followers_Dead.Contains(infoByID) || (!TargetFollowerAllowOldAge && infoByID.CursedState == Thought.OldAge)))
			{
				if (DataManager.Instance.Followers.Count > 0)
				{
					int follower = base.Follower;
					List<FollowerInfo> list = new List<FollowerInfo>();
					foreach (FollowerInfo follower3 in DataManager.Instance.Followers)
					{
						if (!FollowerManager.FollowerLocked(follower3.ID))
						{
							list.Add(follower3);
						}
					}
					base.Follower = ((list.Count > 0) ? list[UnityEngine.Random.Range(0, list.Count)].ID : DataManager.Instance.Followers[UnityEngine.Random.Range(0, DataManager.Instance.Followers.Count)].ID);
					if (DataManager.Instance.CompletedQuestFollowerIDs.Contains(follower))
					{
						DataManager.Instance.CompletedQuestFollowerIDs.Remove(follower);
					}
					DataManager.Instance.CompletedQuestFollowerIDs.Add(base.Follower);
					Follower follower2 = FollowerManager.FindFollowerByID(base.Follower);
					if ((object)follower2 != null)
					{
						follower2.ShowCompletedQuestIcon(true);
					}
					ObjectiveManager.UpdateObjective(this);
					return false;
				}
				if (!FailLocked)
				{
					Failed();
					return false;
				}
			}
			if (base.CheckComplete())
			{
				return Done;
			}
			return false;
		}
		if (base.CheckComplete())
		{
			return Done;
		}
		if (FollowerInfo.GetInfoByID(TargetFollower) == null)
		{
			Failed();
			return false;
		}
		return false;
	}

	public override void Update()
	{
		base.Update();
		if (!string.IsNullOrEmpty(ResponseTerm) && !IsFailed && TargetFollower != -1 && FollowerInfo.GetInfoByID(TargetFollower) == null)
		{
			Failed();
		}
	}

	public override void Complete()
	{
		if (!string.IsNullOrEmpty(ResponseTerm))
		{
			base.Complete();
		}
		else if (!IsComplete)
		{
			DataManager.Instance.AddToCompletedQuestHistory(GetFinalizedData());
		}
		IsComplete = true;
	}
}
