using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlType(Namespace = "Objectives")]
public abstract class ObjectivesData
{
	public Objectives.TYPES Type;

	public string GroupId;

	public bool IsComplete;

	public bool IsFailed;

	public bool FailLocked;

	public bool AutoRemoveQuestOnceComplete = true;

	public bool TargetFollowerAllowOldAge = true;

	public float QuestCooldown = 12000f;

	public float QuestExpireDuration = -1f;

	public float ExpireTimestamp = -1f;

	public int ID;

	public int Index = -1;

	public string UniqueGroupID = "";

	public string CompleteTerm = "";

	protected bool initialised;

	[CompilerGenerated]
	private readonly bool _003CAutoTrack_003Ek__BackingField;

	public abstract string Text { get; }

	public bool HasExpiry
	{
		get
		{
			if (ExpireTimestamp > -1f && !IsFailed)
			{
				return !IsComplete;
			}
			return false;
		}
	}

	public float ExpiryTimeNormalized
	{
		get
		{
			return (ExpireTimestamp - TimeManager.TotalElapsedGameTime) / QuestExpireDuration;
		}
	}

	public int Follower { get; set; } = -1;


	public virtual bool AutoTrack
	{
		[CompilerGenerated]
		get
		{
			return _003CAutoTrack_003Ek__BackingField;
		}
	}

	public ObjectivesData()
	{
	}

	public ObjectivesData(string groupId, float questExpireDuration = -1f)
	{
		GroupId = groupId;
		QuestExpireDuration = questExpireDuration;
		ID = UnityEngine.Random.Range(0, int.MaxValue);
		initialised = false;
	}

	public virtual void Init(bool initialAssigning)
	{
		initialised = true;
		IsComplete = false;
		IsFailed = false;
		if (QuestExpireDuration != -1f && ExpireTimestamp == -1f)
		{
			ExpireTimestamp = TimeManager.TotalElapsedGameTime + QuestExpireDuration;
		}
	}

	public abstract ObjectivesDataFinalized GetFinalizedData();

	public bool TryComplete()
	{
		bool result = false;
		if (CheckComplete())
		{
			Complete();
			result = true;
		}
		return result;
	}

	protected virtual bool CheckComplete()
	{
		FollowerInfo infoByID = FollowerInfo.GetInfoByID(Follower);
		if (!IsFailed && Follower != -1 && (infoByID == null || DataManager.Instance.Followers_Dead.Contains(infoByID) || (!TargetFollowerAllowOldAge && infoByID.CursedState == Thought.OldAge)) && !FailLocked)
		{
			Failed();
			return false;
		}
		return true;
	}

	public virtual void Complete()
	{
		if (!IsComplete && Follower != -1)
		{
			List<ObjectivesData> list = new List<ObjectivesData>(Quests.GetUnCompletedFollowerQuests(Follower, GroupId));
			if (list.Contains(this))
			{
				list.Remove(this);
			}
			if (list.Count == 0)
			{
				Objectives_TalkToFollower obj = new Objectives_TalkToFollower(GroupId)
				{
					CompleteTerm = CompleteTerm,
					Follower = Follower
				};
				ObjectiveManager.Add(obj);
				Follower follower = FollowerManager.FindFollowerByID(obj.Follower);
				if ((object)follower != null)
				{
					follower.ShowCompletedQuestIcon(true);
				}
				DataManager.Instance.CompletedQuestFollowerIDs.Add(Follower);
				obj.CheckComplete();
			}
		}
		if (!IsComplete)
		{
			DataManager.Instance.AddToCompletedQuestHistory(GetFinalizedData());
		}
		IsComplete = true;
		ExpireTimestamp = -1f;
	}

	public virtual void Failed()
	{
		if (IsComplete)
		{
			return;
		}
		IsFailed = true;
		ObjectiveManager.UpdateObjective(this);
		if (Follower != -1)
		{
			AutoRemoveQuestOnceComplete = true;
			FollowerInfo infoByID = FollowerInfo.GetInfoByID(Follower);
			if (infoByID != null)
			{
				CultFaithManager.AddThought(Thought.Cult_FailQuest, -1, 1f);
				FollowerBrain orCreateBrain = FollowerBrain.GetOrCreateBrain(infoByID);
				if (orCreateBrain != null)
				{
					orCreateBrain.AddThought(Thought.LeaderFailedQuest);
				}
			}
		}
		ExpireTimestamp = -1f;
		ObjectiveManager.ObjectiveFailed(this);
		DataManager.Instance.AddToFailedQuestHistory(GetFinalizedData());
	}

	public virtual void Update()
	{
		if (ExpireTimestamp != -1f && TimeManager.TotalElapsedGameTime >= ExpireTimestamp)
		{
			Debug.Log("UPDATE in Objectives");
			Failed();
		}
	}

	public bool IsInitialised()
	{
		return initialised;
	}

	public void ResetInitialisation()
	{
		initialised = false;
	}
}
