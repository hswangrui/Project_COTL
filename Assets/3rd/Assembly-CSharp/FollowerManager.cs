using System;
using System.Collections;
using System.Collections.Generic;
using MMTools;
using UnityEngine;

public class FollowerManager
{
	public delegate void FollowerChanged(int followerID);

	public delegate void FollowerGoneEvent(int followerID, NotificationCentre.NotificationType notificationType);

	public struct SpawnedFollower
	{
		public FollowerBrain FollowerBrain;

		public FollowerBrain FollowerFakeBrain;

		public FollowerInfo FollowerFakeInfo;

		public Follower Follower;
	}

	public static Dictionary<FollowerLocation, List<Follower>> Followers = new Dictionary<FollowerLocation, List<Follower>>();

	public static Dictionary<FollowerLocation, List<SimFollower>> SimFollowers = new Dictionary<FollowerLocation, List<SimFollower>>();

	public static FollowerChanged OnFollowerAdded;

	public static FollowerChanged OnFollowerRemoved;

	public static FollowerGoneEvent OnFollowerDie;

	public static FollowerGoneEvent OnFollowerLeave;

	public static int DeathCatID = 666;

	public static int LeshyID = 99990;

	public static int HeketID = 99991;

	public static int KallamarID = 99992;

	public static int ShamuraID = 99993;

	public static int BaalID = 99994;

	public static int AymID = 99995;

	public static List<int> UniqueFollowerIDs = new List<int> { DeathCatID, LeshyID, HeketID, KallamarID, ShamuraID, BaalID, AymID };

	public static List<List<int>> SiblingIDs = new List<List<int>>
	{
		new List<int> { DeathCatID, LeshyID, HeketID, KallamarID, ShamuraID },
		new List<int> { BaalID, AymID }
	};

	public static int CopyFollowersActive = 0;

	public static Follower FollowerPrefab
	{
		get
		{
			return Resources.Load<Follower>("Prefabs/Units/Villagers/Follower");
		}
	}

	public static Follower CombatFollowerPrefab
	{
		get
		{
			return Resources.Load<Follower>("Prefabs/Units/Villagers/Enemy Follower");
		}
	}

	public static FollowerRecruit RecruitPrefab
	{
		get
		{
			return Resources.Load<FollowerRecruit>("Prefabs/Units/Villagers/Recruit Variant");
		}
	}

	public static string GetSpecialFollowerFallback(int followerId)
	{
		foreach (int uniqueFollowerID in UniqueFollowerIDs)
		{
			if (uniqueFollowerID == followerId)
			{
				return MMConversation.GetFallBackVO(followerId);
			}
		}
		return null;
	}

	public static IEnumerable<Follower> ActiveLocationFollowers()
	{
		foreach (FollowerLocation item in LocationManager.LocationsInState(LocationState.Active))
		{
			List<Follower> list = FollowersAtLocation(item);
			for (int i = list.Count - 1; i >= 0; i--)
			{
				yield return list[i];
			}
		}
	}

	public static IEnumerable<FollowerBrain> FollowerBrainsByHomeLocation(FollowerLocation homeLocation)
	{
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.HomeLocation == homeLocation)
			{
				yield return allBrain;
			}
		}
	}

	public static List<Follower> FollowersAtLocation(FollowerLocation location)
	{
		List<Follower> value = null;
		if (location == FollowerLocation.None)
		{
			value = new List<Follower>();
		}
		else if (!Followers.TryGetValue(location, out value))
		{
			value = new List<Follower>();
			Followers[location] = value;
		}
		return value;
	}

	public static List<SimFollower> SimFollowersAtLocation(FollowerLocation location)
	{
		List<SimFollower> value = null;
		if (location == FollowerLocation.None)
		{
			value = new List<SimFollower>();
		}
		else if (!SimFollowers.TryGetValue(location, out value))
		{
			value = new List<SimFollower>();
			SimFollowers[location] = value;
		}
		return value;
	}

	public static FollowerInfo FindFollowerInfo(int ID)
	{
		FollowerInfo result = null;
		for (int i = 0; i < DataManager.Instance.Followers.Count; i++)
		{
			if (DataManager.Instance.Followers[i].ID == ID)
			{
				result = DataManager.Instance.Followers[i];
				break;
			}
		}
		return result;
	}

	public static FollowerInfo RemoveFollower(int ID)
	{
		FollowerInfo result = null;
		for (int i = 0; i < DataManager.Instance.Followers.Count; i++)
		{
			if (DataManager.Instance.Followers[i].ID == ID)
			{
				result = DataManager.Instance.Followers[i];
				DataManager.Instance.Followers.RemoveAt(i);
				FollowerChanged onFollowerRemoved = OnFollowerRemoved;
				if (onFollowerRemoved != null)
				{
					onFollowerRemoved(ID);
				}
				DestroyPausedFollowerByID(ID);
				RetireSimFollowerByID(ID);
				break;
			}
		}
		return result;
	}

	public static bool FollowerLocked(int ID, bool exludeStarving = false)
	{
		FollowerInfo infoByID = FollowerInfo.GetInfoByID(ID);
		if (infoByID != null && !DataManager.Instance.Followers_Imprisoned_IDs.Contains(ID) && !DataManager.Instance.Followers_OnMissionary_IDs.Contains(ID) && !DataManager.Instance.Followers_Demons_IDs.Contains(ID) && !DataManager.Instance.Followers_Transitioning_IDs.Contains(ID) && !DataManager.Instance.Followers_Recruit.Contains(infoByID) && (!infoByID.IsStarving || exludeStarving))
		{
			return infoByID.LeavingCult;
		}
		return true;
	}

	public static void ConsumeFollower(int ID)
	{
		FollowerInfo infoByID = FollowerInfo.GetInfoByID(ID);
		if (infoByID != null)
		{
			DataManager.Instance.Followers_Dead.Add(infoByID);
		}
		DataManager.Instance.Followers_Dead_IDs.Add(ID);
		CompleteKillFollowerObjective(ID);
		RemoveFollower(ID);
		FollowerBrain.RemoveBrain(ID);
	}

	public static void FollowerLeave(int ID, NotificationCentre.NotificationType leaveNotificationType)
	{
		FollowerGoneEvent onFollowerLeave = OnFollowerLeave;
		if (onFollowerLeave != null)
		{
			onFollowerLeave(ID, leaveNotificationType);
		}
	}

	public static void FollowerDie(int ID, NotificationCentre.NotificationType deathNotificationType)
	{
		switch (deathNotificationType)
		{
		case NotificationCentre.NotificationType.DiedFromOldAge:
			DataManager.Instance.STATS_NaturalDeaths++;
			CultFaithManager.AddThought(Thought.Cult_FollowerDiedOfOldAge, ID, 1f);
			if (IsHateElderlyTraitActiveWithElderlyFollower(ID) && !DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.LoveElderly))
			{
				CultFaithManager.AddThought(Thought.Cult_HateElderly_Trait_Cons, -1, 1f);
			}
			break;
		case NotificationCentre.NotificationType.MurderedByYou:
		{
			bool flag = false;
			foreach (ObjectivesData completedObjective in DataManager.Instance.CompletedObjectives)
			{
				if (completedObjective is Objectives_Custom && completedObjective.Follower == ID && (((Objectives_Custom)completedObjective).CustomQuestType == Objectives.CustomQuestTypes.MurderFollower || ((Objectives_Custom)completedObjective).CustomQuestType == Objectives.CustomQuestTypes.KillFollower || (((Objectives_Custom)completedObjective).CustomQuestType == Objectives.CustomQuestTypes.MurderFollowerAtNight && TimeManager.IsNight)))
				{
					flag = true;
				}
			}
			if (TimeManager.IsNight)
			{
				int num = 0;
				foreach (Follower follower in Follower.Followers)
				{
					if (!(follower == null) && follower.Brain != null && follower.Brain.CurrentTask != null && (follower.Brain.CurrentTaskType == FollowerTaskType.Sleep || follower.Brain.CurrentTaskType == FollowerTaskType.SleepBedRest) && follower.Brain.CurrentTask.State == FollowerTaskState.Doing)
					{
						num++;
					}
				}
				if (num >= Follower.Followers.Count - 1)
				{
					CultFaithManager.AddThought(Thought.Cult_MurderAtNightNoWitnesses, ID, (!flag) ? 1 : 0);
				}
				else
				{
					CultFaithManager.AddThought(Thought.Cult_MurderAtNightFewWitnesses, ID, (!flag) ? 1 : 0);
				}
			}
			else
			{
				CultFaithManager.AddThought(Thought.Cult_Murder, ID, (!flag) ? 1 : 0);
			}
			if (IsHateElderlyTraitActiveWithElderlyFollower(ID))
			{
				CultFaithManager.AddThought(Thought.Cult_HateElderly_Trait_Pros, -1, 1f);
			}
			break;
		}
		case NotificationCentre.NotificationType.SacrificeFollower:
			if (IsHateElderlyTraitActiveWithElderlyFollower(ID))
			{
				CultFaithManager.AddThought(Thought.Cult_HateElderly_Trait_Pros, -1, 1f);
			}
			break;
		case NotificationCentre.NotificationType.KilledInAFightPit:
		case NotificationCentre.NotificationType.ConsumeFollower:
			if (IsHateElderlyTraitActiveWithElderlyFollower(ID))
			{
				CultFaithManager.AddThought(Thought.Cult_HateElderly_Trait_Pros, -1, 1f);
			}
			break;
		case NotificationCentre.NotificationType.DiedFromIllness:
			CultFaithManager.AddThought(Thought.DiedFromIllness, ID, 1f);
			break;
		case NotificationCentre.NotificationType.DiedFromStarvation:
			DataManager.Instance.STATS_FollowersStarvedToDeath++;
			if (DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.DesensitisedToDeath))
			{
				CultFaithManager.AddThought(Thought.Cult_FollowerDied_Trait, ID, 1f);
			}
			else
			{
				CultFaithManager.AddThought(Thought.Cult_FollowerDied, ID, 1f);
			}
			if (IsHateElderlyTraitActiveWithElderlyFollower(ID) && !DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.LoveElderly))
			{
				CultFaithManager.AddThought(Thought.Cult_HateElderly_Trait_Cons, -1, 1f);
			}
			break;
		default:
			if (DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.DesensitisedToDeath))
			{
				CultFaithManager.AddThought(Thought.Cult_FollowerDied_Trait, ID, 1f);
			}
			else
			{
				CultFaithManager.AddThought(Thought.Cult_FollowerDied, ID, 1f);
			}
			if (IsHateElderlyTraitActiveWithElderlyFollower(ID) && !DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.LoveElderly))
			{
				CultFaithManager.AddThought(Thought.Cult_HateElderly_Trait_Cons, -1, 1f);
			}
			break;
		case NotificationCentre.NotificationType.Ascended:
			break;
		}
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain == null || allBrain.Info == null || allBrain.Info.CursedState == Thought.Zombie || allBrain.Info.ID == ID)
			{
				continue;
			}
			switch (deathNotificationType)
			{
			case NotificationCentre.NotificationType.MurderedByYou:
				if (allBrain.HasTrait(FollowerTrait.TraitType.HateElderly) && DataManager.Instance.Followers_Elderly_IDs.Contains(ID))
				{
					allBrain.AddThought(Thought.LeaderMurderedAFollowerHateElderly);
				}
				else if (allBrain.HasTrait(FollowerTrait.TraitType.LoveElderly) && DataManager.Instance.Followers_Elderly_IDs.Contains(ID))
				{
					allBrain.AddThought(Thought.LeaderMurderedAFollowerLoveElderly);
				}
				else
				{
					allBrain.AddThought(Thought.LeaderMurderedAFollower);
				}
				continue;
			case NotificationCentre.NotificationType.SacrificeFollower:
				if (allBrain.HasTrait(FollowerTrait.TraitType.HateElderly) && DataManager.Instance.Followers_Elderly_IDs.Contains(ID))
				{
					allBrain.AddThought(Thought.ElderlySacrificedHateElderly);
				}
				else if (allBrain.HasTrait(FollowerTrait.TraitType.LoveElderly) && DataManager.Instance.Followers_Elderly_IDs.Contains(ID))
				{
					allBrain.AddThought(Thought.ElderlySacrificedLoveElderly);
				}
				else if (allBrain.HasTrait(FollowerTrait.TraitType.AgainstSacrifice))
				{
					allBrain.AddThought(Thought.CultMemberWasSacrificedAgainstSacrificeTrait);
				}
				else if (allBrain.HasTrait(FollowerTrait.TraitType.SacrificeEnthusiast))
				{
					allBrain.AddThought(Thought.CultMemberWasSacrificedSacrificeEnthusiastTrait);
				}
				else
				{
					allBrain.AddThought(Thought.CultMemberWasSacrificed);
				}
				if (allBrain.HasTrait(FollowerTrait.TraitType.AgainstSacrifice) && FollowerInfo.GetInfoByID(allBrain.Info.ID) != null)
				{
					CultFaithManager.AddThought(Thought.Cult_Sacrifice_Trait_Scared, allBrain.Info.ID, 1f);
				}
				continue;
			case NotificationCentre.NotificationType.Ascended:
				allBrain.AddThought(Thought.FollowerAscended);
				continue;
			case NotificationCentre.NotificationType.KilledInAFightPit:
				allBrain.AddThought(Thought.FightPitExecution);
				continue;
			}
			if (allBrain.HasTrait(FollowerTrait.TraitType.FearOfDeath) && allBrain.Info.ID != ID && FollowerInfo.GetInfoByID(allBrain.Info.ID) != null)
			{
				allBrain.AddThought(Thought.CultMemberDiedScaredOfDeathTrait);
				CultFaithManager.AddThought(Thought.Cult_FollowerDied_Trait_Scared, allBrain.Info.ID, 1f);
				continue;
			}
			if (allBrain.HasTrait(FollowerTrait.TraitType.DesensitisedToDeath))
			{
				allBrain.AddThought(Thought.CultMemberDiedScaredOfDesensitized);
				continue;
			}
			switch (allBrain.Info.GetOrCreateRelationship(ID).CurrentRelationshipState)
			{
			case IDAndRelationship.RelationshipState.Strangers:
				allBrain.AddThought(Thought.StrangerDied);
				break;
			case IDAndRelationship.RelationshipState.Friends:
				allBrain.AddThought(Thought.FriendDied);
				break;
			case IDAndRelationship.RelationshipState.Lovers:
				allBrain.AddThought(Thought.LoverDied);
				break;
			case IDAndRelationship.RelationshipState.Enemies:
				allBrain.AddThought(Thought.EnemyDied);
				break;
			}
		}
		FollowerGoneEvent onFollowerDie = OnFollowerDie;
		if (onFollowerDie != null)
		{
			onFollowerDie(ID, deathNotificationType);
		}
		FollowerInfo followerInfo = RemoveFollower(ID);
		if (followerInfo != null && followerInfo.CursedState == Thought.Dissenter)
		{
			ObjectiveManager.FailLockCustomObjective(Objectives.CustomQuestTypes.CureDissenter, true);
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.CureDissenter, ID);
			ObjectiveManager.FailLockCustomObjective(Objectives.CustomQuestTypes.CureDissenter, false);
		}
		if (followerInfo != null)
		{
			CompleteKillFollowerObjective(ID);
		}
		ObjectiveManager.FailCustomObjective(Objectives.CustomQuestTypes.FollowerRecoverIllness, ID);
		if (followerInfo != null)
		{
			DataManager.Instance.Followers_Dead.Insert(0, followerInfo);
			DataManager.Instance.Followers_Dead_IDs.Insert(0, followerInfo.ID);
		}
		RemoveFollowerBrain(ID);
		if (followerInfo == null)
		{
			return;
		}
		foreach (FollowerBrain allBrain2 in FollowerBrain.AllBrains)
		{
			if (allBrain2.HasTrait(FollowerTrait.TraitType.LoveElderly))
			{
				allBrain2.RemoveThought(Thought.FollowerIsElderlyLoveElderly, false);
			}
			else if (allBrain2.HasTrait(FollowerTrait.TraitType.HateElderly))
			{
				allBrain2.RemoveThought(Thought.FollowerIsElderlyHateElderly, false);
			}
		}
		if (followerInfo.CursedState == Thought.Zombie)
		{
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.ZombieExists);
			foreach (FollowerBrain allBrain3 in FollowerBrain.AllBrains)
			{
				if (allBrain3.Info.CursedState != Thought.Zombie)
				{
					allBrain3.AddThought(Thought.ZombieDied);
				}
			}
		}
		//TwitchFollowers.SendFollowerInformation(followerInfo);
	}

	private static bool IsHateElderlyTraitActiveWithElderlyFollower(int followerID)
	{
		if (FollowerInfo.GetInfoByID(followerID, true).CursedState == Thought.OldAge)
		{
			return DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.HateElderly);
		}
		return false;
	}

	private static void CompleteKillFollowerObjective(int followerID)
	{
		ObjectiveManager.FailLockCustomObjective(Objectives.CustomQuestTypes.KillFollower, true);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.KillFollower, followerID);
		ObjectiveManager.FailLockCustomObjective(Objectives.CustomQuestTypes.KillFollower, false);
	}

	public static void RemoveFollowerBrain(int ID)
	{
		FollowerInfo followerInfo = RemoveFollower(ID);
		if (followerInfo != null)
		{
			FollowerBrain followerBrain = FollowerBrain.FindBrainByID(followerInfo.ID);
			if (followerBrain != null)
			{
				followerBrain.Cleanup();
				followerBrain.ClearDwelling();
			}
			DataManager.Instance.Followers_Imprisoned_IDs.Remove(followerInfo.ID);
			DataManager.Instance.Followers_OnMissionary_IDs.Remove(ID);
			DataManager.Instance.Followers_Elderly_IDs.Remove(followerInfo.ID);
			DataManager.Instance.Followers_Transitioning_IDs.Remove(followerInfo.ID);
		}
		FollowerBrain.RemoveBrain(ID);
		ObjectiveManager.CheckObjectives();
	}

	public static FollowerBrain GetHungriestFollowerBrain()
	{
		float num = float.MinValue;
		FollowerBrain result = null;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (!FollowerLocked(allBrain.Info.ID, true))
			{
				float hungerScore = allBrain.GetHungerScore();
				if (hungerScore > num)
				{
					num = hungerScore;
					result = allBrain;
				}
			}
		}
		return result;
	}

	public static FollowerBrain GetHungriestFollowerBrainNotStarving()
	{
		float num = float.MinValue;
		FollowerBrain result = null;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (!FollowerLocked(allBrain.Info.ID) && !allBrain.HasTrait(FollowerTrait.TraitType.DontStarve))
			{
				float hungerScoreNotStarving = allBrain.GetHungerScoreNotStarving();
				if (hungerScoreNotStarving > num)
				{
					num = hungerScoreNotStarving;
					result = allBrain;
				}
			}
		}
		return result;
	}

	public static FollowerBrain GetHungriestFollowerBrainNoCursedState()
	{
		float num = float.MinValue;
		FollowerBrain result = null;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (!FollowerLocked(allBrain.Info.ID))
			{
				float hungerScoreNoCursedState = allBrain.GetHungerScoreNoCursedState();
				if (hungerScoreNoCursedState > num)
				{
					num = hungerScoreNoCursedState;
					result = allBrain;
				}
			}
		}
		return result;
	}

	public static int GetHungerScoreIndex(FollowerBrain brain)
	{
		int num = 0;
		float hungerScore = brain.GetHungerScore();
		if (hungerScore == 0f)
		{
			num = -1;
		}
		else
		{
			foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
			{
				if (allBrain != brain && allBrain.GetHungerScore() > hungerScore)
				{
					num++;
				}
			}
		}
		return num;
	}

	public static void DestroyPausedFollowerByID(int followerID)
	{
		for (int i = 0; i < 85; i++)
		{
			List<Follower> list = FollowersAtLocation((FollowerLocation)i);
			foreach (Follower item in list)
			{
				if (item.Brain.Info.ID == followerID)
				{
					list.Remove(item);
					FollowerChanged onFollowerRemoved = OnFollowerRemoved;
					if (onFollowerRemoved != null)
					{
						onFollowerRemoved(item.Brain.Info.ID);
					}
					if (item.IsPaused)
					{
						UnityEngine.Object.Destroy(item.gameObject);
					}
					return;
				}
			}
		}
	}

	public static void RetireSimFollowerByID(int followerID)
	{
		for (int i = 0; i < 85; i++)
		{
			foreach (SimFollower item in SimFollowersAtLocation((FollowerLocation)i))
			{
				if (item.Brain.Info.ID == followerID)
				{
					item.Retire();
					return;
				}
			}
		}
	}

	public static SpawnedFollower SpawnCopyFollower(Follower followerPrefab, Vector3 position, Transform parent, FollowerLocation location)
	{
		FollowerInfo followerInfo = FollowerInfo.NewCharacter(location);
		return SpawnCopyFollower(followerPrefab, followerInfo, position, parent, location);
	}

	public static SpawnedFollower SpawnCopyFollower(FollowerInfo followerInfo, Vector3 position, Transform parent, FollowerLocation location)
	{
		return SpawnCopyFollower(FollowerPrefab, followerInfo, position, parent, location);
	}

	public static SpawnedFollower SpawnCopyFollower(Follower followerPrefab, FollowerInfo followerInfo, Vector3 position, Transform parent, FollowerLocation location)
	{
		SpawnedFollower result = default(SpawnedFollower);
		CopyFollowersActive++;
		result.FollowerFakeInfo = FollowerInfo.NewCharacter(location);
		result.FollowerFakeInfo.IsFakeBrain = true;
		result.FollowerFakeInfo.ViewerID = followerInfo.ViewerID;
		result.FollowerFakeInfo.Name = followerInfo.Name;
		result.Follower = UnityEngine.Object.Instantiate((followerPrefab != null) ? followerPrefab : FollowerPrefab, position, Quaternion.identity, parent);
		result.FollowerBrain = FollowerBrain.GetOrCreateBrain(followerInfo);
		result.FollowerFakeBrain = FollowerBrain.GetOrCreateBrain(result.FollowerFakeInfo);
		result.FollowerFakeBrain.Stats = result.FollowerBrain.Stats;
		result.Follower.Init(result.FollowerFakeBrain, new FollowerOutfit(followerInfo));
		result.Follower.Brain.CheckChangeState();
		result.Follower.gameObject.SetActive(true);
		result.Follower.Interaction_FollowerInteraction.enabled = false;
		result.Follower.Brain.HardSwapToTask(new FollowerTask_ManualControl());
		result.Follower.State.LookAngle = Utils.GetAngle(result.Follower.transform.position, position);
		result.Follower.State.facingAngle = Utils.GetAngle(result.Follower.transform.position, position);
		result.Follower.HideAllFollowerIcons();
		return result;
	}

	public static void CleanUpCopyFollower(SpawnedFollower spawnedFollower)
	{
		CopyFollowersActive = Mathf.Clamp(CopyFollowersActive - 1, 0, int.MaxValue);
		if (spawnedFollower.Follower != null)
		{
			UnityEngine.Object.Destroy(spawnedFollower.Follower.gameObject);
		}
		FollowerBrain.RemoveBrain(spawnedFollower.FollowerFakeBrain.Info.ID);
	}

	public static FollowerInfo GetDeadFollowerInfoByID(int followerID)
	{
		FollowerInfo result = null;
		foreach (FollowerInfo item in DataManager.Instance.Followers_Dead)
		{
			if (item.ID == followerID)
			{
				result = item;
				break;
			}
		}
		return result;
	}

	public static Follower FindFollowerByViewerID(string viewerID)
	{
		Follower result = null;
		for (int i = 0; i < 85; i++)
		{
			foreach (Follower item in FollowersAtLocation((FollowerLocation)i))
			{
				if (item.Brain.Info.ViewerID == viewerID)
				{
					result = item;
					goto end_IL_0055;
				}
			}
			continue;
			end_IL_0055:
			break;
		}
		return result;
	}

	public static Follower FindFollowerByID(int ID)
	{
		Follower result = null;
		for (int i = 0; i < 85; i++)
		{
			foreach (Follower item in FollowersAtLocation((FollowerLocation)i))
			{
				if (item.Brain.Info.ID == ID)
				{
					result = item;
					goto end_IL_0050;
				}
			}
			continue;
			end_IL_0050:
			break;
		}
		return result;
	}

	public static List<FollowerInfo> FindFollowersByID(List<int> ids)
	{
		List<FollowerInfo> list = new List<FollowerInfo>();
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			if (ids.Contains(follower.ID))
			{
				list.Add(follower);
			}
		}
		return list;
	}

	public static SimFollower FindSimFollowerByID(int ID)
	{
		SimFollower result = null;
		for (int i = 0; i < 85; i++)
		{
			foreach (SimFollower item in SimFollowersAtLocation((FollowerLocation)i))
			{
				if (item.Brain.Info.ID == ID)
				{
					result = item;
					goto end_IL_0050;
				}
			}
			continue;
			end_IL_0050:
			break;
		}
		return result;
	}

	public static void KillFollowerOrSimByID(int ID, NotificationCentre.NotificationType Notification)
	{
		Follower follower = FindFollowerByID(ID);
		if (follower != null)
		{
			follower.Die(Notification);
			return;
		}
		SimFollower simFollower = FindSimFollowerByID(ID);
		if (simFollower != null)
		{
			simFollower.Die(Notification, simFollower.Brain.LastPosition);
		}
	}

	public static void SpawnExistingRecruits(Vector3 position)
	{
		if (DataManager.Instance.Followers_Recruit.Count > 0)
		{
			SpawnRecruit(DataManager.Instance.Followers_Recruit[0], position);
		}
	}

	public static float GetTotalNonLockedFollowers()
	{
		int num = 0;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (!FollowerLocked(allBrain.Info.ID))
			{
				num++;
			}
		}
		return num;
	}

	public static FollowerBrain GetRandomNonLockedFollower()
	{
		List<FollowerBrain> list = new List<FollowerBrain>();
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (!FollowerLocked(allBrain.Info.ID))
			{
				list.Add(allBrain);
			}
		}
		if (list.Count > 0)
		{
			return list[UnityEngine.Random.Range(0, list.Count)];
		}
		return null;
	}

	public static void CreateNewRecruit(FollowerLocation location, NotificationCentre.NotificationType Notification)
	{
		FollowerInfo item = FollowerInfo.NewCharacter(location);
		DataManager.Instance.Followers_Recruit.Add(item);
		NotificationCentre.Instance.PlayGenericNotification(Notification);
	}

	public static FollowerInfo CreateNewRecruit(FollowerLocation location, string ForceSkin, NotificationCentre.NotificationType Notification)
	{
		FollowerInfo followerInfo = FollowerInfo.NewCharacter(location, ForceSkin);
		DataManager.Instance.Followers_Recruit.Add(followerInfo);
		NotificationCentreScreen.Play(Notification);
		return followerInfo;
	}

	public static FollowerRecruit CreateNewRecruit(FollowerLocation location, Vector3 position)
	{
		return CreateNewRecruit(FollowerInfo.NewCharacter(location), position);
	}

	public static void CreateNewRecruit(FollowerInfo f, NotificationCentre.NotificationType Notification)
	{
		DataManager.Instance.Followers_Recruit.Add(f);
		DataManager.SetFollowerSkinUnlocked(f.SkinName);
		NotificationCentreScreen.Play(Notification);
		if (GameManager.IsDungeon(PlayerFarming.Location))
		{
			DataManager.Instance.FollowersRecruitedThisNode++;
		}
	}

	public static FollowerRecruit CreateNewRecruit(FollowerInfo f, Vector3 position)
	{
		DataManager.Instance.Followers_Recruit.Add(f);
		DataManager.SetFollowerSkinUnlocked(f.SkinName);
		return SpawnRecruit(f, position);
	}

	private static FollowerRecruit SpawnRecruit(FollowerInfo f, Vector3 position, bool Force = false)
	{
		FollowerRecruit followerRecruit = null;
		FollowerBrain orCreateBrain = FollowerBrain.GetOrCreateBrain(f);
		LocationManager locationManager = LocationManager.LocationManagers[f.Location];
		if (locationManager != null)
		{
			followerRecruit = locationManager.SpawnRecruit(orCreateBrain, position);
		}
		orCreateBrain.TransitionToTask(new FollowerTask_ManualControl());
		followerRecruit.SpawnAnim(true);
		return followerRecruit;
	}

	public static FollowerInfo RemoveRecruit(int ID)
	{
		FollowerInfo result = null;
		for (int i = 0; i < DataManager.Instance.Followers_Recruit.Count; i++)
		{
			if (DataManager.Instance.Followers_Recruit[i].ID == ID)
			{
				result = DataManager.Instance.Followers_Recruit[i];
				DataManager.Instance.Followers_Recruit.RemoveAt(i);
				break;
			}
		}
		return result;
	}

	public static void RecruitFollower(FollowerRecruit recruit, bool followPlayer)
	{
		FollowerInfo followerInfo = RemoveRecruit(recruit.Follower.Brain.Info.ID);
		AddFollower(followerInfo, recruit.transform.position);
		FollowersAtLocation(followerInfo.Location).Add(recruit.Follower);
		recruit.Follower.Brain.FollowingPlayer = followPlayer;
		recruit.Follower.Resume();
		recruit.Follower.Brain.CompleteCurrentTask();
	}

	public static Follower CreateNewFollower(FollowerLocation location, Vector3 position, bool followPlayer = false)
	{
		return CreateNewFollower(FollowerInfo.NewCharacter(location), position, followPlayer);
	}

	public static Follower CreateNewFollower(FollowerInfo f, Vector3 position, bool followPlayer = false)
	{
		AddFollower(f, position);
		return SpawnFollower(f, position, followPlayer);
	}

	private static void AddFollower(FollowerInfo f, Vector3 position)
	{
		DataManager.Instance.Followers.Add(f);
		DataManager.Instance.GetFirstFollower = true;
		FollowerChanged onFollowerAdded = OnFollowerAdded;
		if (onFollowerAdded != null)
		{
			onFollowerAdded(f.ID);
		}
	}

	public static Follower SpawnFollower(FollowerInfo f, Vector3 position, bool followPlayer = false)
	{
		Follower result = null;
		FollowerBrain orCreateBrain = FollowerBrain.GetOrCreateBrain(f);
		orCreateBrain.FollowingPlayer = followPlayer;
		LocationManager locationManager = LocationManager.LocationManagers[f.Location];
		if (locationManager != null)
		{
			Follower follower = locationManager.SpawnFollower(orCreateBrain, position);
			FollowersAtLocation(f.Location).Add(follower);
			follower.Resume();
			result = follower;
		}
		else
		{
			SimFollower item = new SimFollower(orCreateBrain);
			SimFollowersAtLocation(f.Location).Add(item);
		}
		orCreateBrain.CompleteCurrentTask();
		return result;
	}

	public static void ReviveFollower(int ID, FollowerLocation location, Vector3 position)
	{
		FollowerInfo followerInfo = null;
		for (int i = 0; i < DataManager.Instance.Followers_Dead.Count; i++)
		{
			if (DataManager.Instance.Followers_Dead[i].ID == ID)
			{
				followerInfo = DataManager.Instance.Followers_Dead[i];
				DataManager.Instance.Followers_Dead.RemoveAt(i);
				DataManager.Instance.Followers_Dead_IDs.RemoveAt(i);
				break;
			}
		}
		if (followerInfo != null)
		{
			followerInfo.Location = location;
			AddFollower(followerInfo, position);
		}
	}

	public static void Reset()
	{
		foreach (KeyValuePair<FollowerLocation, List<Follower>> follower in Followers)
		{
			if (follower.Value == null)
			{
				continue;
			}
			foreach (Follower item in follower.Value)
			{
				if (item != null)
				{
					item.gameObject.SetActive(false);
				}
			}
		}
		if (Followers != null)
		{
			Followers.Clear();
		}
		foreach (FollowerBrain item2 in new List<FollowerBrain>(FollowerBrain.AllBrains))
		{
			item2.Cleanup();
			if (item2.Info != null)
			{
				FollowerBrain.RemoveBrain(item2.Info.ID);
			}
		}
		if (SimFollowers != null)
		{
			SimFollowers.Clear();
		}
		PlayerFarming.LastLocation = FollowerLocation.None;
		PlayerFarming.Location = FollowerLocation.None;
	}

	public static void GiveFollowersRandomCurse(int amountOfFollowers, Thought curseType = Thought.None)
	{
		List<FollowerBrain> list = new List<FollowerBrain>(FollowerBrain.AllBrains);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (FollowerLocked(list[num].Info.ID) || list[num].Info.CursedState != 0 || (curseType == Thought.BecomeStarving && list[num].HasTrait(FollowerTrait.TraitType.DontStarve)) || (curseType == Thought.Dissenter && list[num].Info.Necklace == InventoryItem.ITEM_TYPE.Necklace_Loyalty))
			{
				list.Remove(list[num]);
			}
		}
		for (int i = 0; i < amountOfFollowers; i++)
		{
			Thought thought = curseType;
			if (thought == Thought.None)
			{
				switch (UnityEngine.Random.Range(0, 3))
				{
				case 0:
					thought = Thought.Ill;
					break;
				case 1:
					thought = Thought.Dissenter;
					break;
				case 2:
					thought = Thought.BecomeStarving;
					break;
				}
			}
			if (list.Count > 0)
			{
				FollowerBrain followerBrain = list[UnityEngine.Random.Range(0, list.Count)];
				list.Remove(followerBrain);
				switch (thought)
				{
				case Thought.Ill:
					followerBrain.MakeSick();
					break;
				case Thought.Dissenter:
					followerBrain.MakeDissenter();
					break;
				case Thought.BecomeStarving:
					followerBrain.MakeStarve();
					break;
				}
				continue;
			}
			break;
		}
	}

	public static void MakeAllFollowersFallAsleep()
	{
		foreach (FollowerBrain brain in FollowerBrain.AllBrains)
		{
			if (FollowerLocked(brain.Info.ID))
			{
				continue;
			}
			GameManager.GetInstance().StartCoroutine(Delay(UnityEngine.Random.Range(0f, 1f), delegate
			{
				if (!FollowerLocked(brain.Info.ID))
				{
					brain.HardSwapToTask(new FollowerTask_Sleep(true));
				}
			}));
		}
	}

	private static IEnumerator Delay(float delay, Action callback)
	{
		yield return new WaitForSeconds(delay);
		if (callback != null)
		{
			callback();
		}
	}

	public static void KillRandomFollower(bool diedFromTwitchChat)
	{
		List<FollowerBrain> list = new List<FollowerBrain>();
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (!FollowerLocked(allBrain.Info.ID))
			{
				list.Add(allBrain);
			}
		}
		FollowerBrain followerBrain = list[UnityEngine.Random.Range(0, list.Count)];
		followerBrain.DiedFromTwitchChat = diedFromTwitchChat;
		if (FindFollowerByID(followerBrain.Info.ID) != null)
		{
			FindFollowerByID(followerBrain.Info.ID).Die();
		}
		else
		{
			followerBrain.Die(NotificationCentre.NotificationType.Died);
		}
	}

	public static void MakeAllFollowersPoopOrVomit()
	{
		foreach (FollowerBrain brain in FollowerBrain.AllBrains)
		{
			if (FollowerLocked(brain.Info.ID))
			{
				continue;
			}
			GameManager.GetInstance().StartCoroutine(Delay(UnityEngine.Random.Range(0f, 1f), delegate
			{
				if (!FollowerLocked(brain.Info.ID))
				{
					if (UnityEngine.Random.value < 0.5f)
					{
						brain.HardSwapToTask(new FollowerTask_InstantPoop());
					}
					else
					{
						brain.HardSwapToTask(new FollowerTask_Vomit());
					}
				}
			}));
		}
	}

	public static void CureAllCursedFollowers()
	{
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.Info.CursedState == Thought.Dissenter)
			{
				if (allBrain.Info.CursedState == Thought.Dissenter)
				{
					allBrain.Stats.Reeducation = 0f;
				}
			}
			else if (allBrain.Info.CursedState == Thought.Ill)
			{
				allBrain.Stats.Illness = 0f;
				FollowerBrainStats.StatStateChangedEvent onIllnessStateChanged = FollowerBrainStats.OnIllnessStateChanged;
				if (onIllnessStateChanged != null)
				{
					onIllnessStateChanged(allBrain.Info.ID, FollowerStatState.Off, FollowerStatState.On);
				}
			}
			else if (allBrain.Info.CursedState == Thought.BecomeStarving)
			{
				allBrain.Stats.Starvation = 0f;
			}
			else if (allBrain.Stats.Exhaustion > 0f)
			{
				allBrain.Stats.Rest = 100f;
				allBrain.Stats.Exhaustion = 0f;
			}
		}
	}

	public static void ResurrectBurriedFollower()
	{
		GameManager.GetInstance().StartCoroutine(ResurrectBurriedFollowerIE());
	}

	private static IEnumerator ResurrectBurriedFollowerIE()
	{
		while (PlayerFarming.Location != FollowerLocation.Base || LetterBox.IsPlaying || MMConversation.isPlaying || SimulationManager.IsPaused || (PlayerFarming.Instance.state.CURRENT_STATE != 0 && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.Moving))
		{
			yield return null;
		}
		List<Structures_Grave> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_Grave>(FollowerLocation.Base);
		List<FollowerInfo> list = new List<FollowerInfo>(DataManager.Instance.Followers_Dead);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			bool flag = false;
			foreach (Structures_Grave item in allStructuresOfType)
			{
				if (item.Data.FollowerID == list[num].ID)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.RemoveAt(num);
			}
		}
		FollowerInfo followerInfo = list[UnityEngine.Random.Range(0, list.Count)];
		DataManager.Instance.Followers_Dead.Remove(followerInfo);
		DataManager.Instance.Followers_Dead_IDs.Remove(followerInfo.ID);
		Structures_Grave structures_Grave = null;
		foreach (Structures_Grave item2 in allStructuresOfType)
		{
			if (item2.Data.FollowerID == followerInfo.ID)
			{
				structures_Grave = item2;
				break;
			}
		}
		foreach (Grave grafe in Grave.Graves)
		{
			if (grafe.structureBrain.Data.FollowerID == structures_Grave.Data.FollowerID)
			{
				structures_Grave.Data.FollowerID = -1;
				grafe.SetGameObjects();
				break;
			}
		}
		FollowerBrain resurrectingFollower = FollowerBrain.GetOrCreateBrain(followerInfo);
		resurrectingFollower.ResetStats();
		if (resurrectingFollower.Info.Age > resurrectingFollower.Info.LifeExpectancy)
		{
			resurrectingFollower.Info.LifeExpectancy = resurrectingFollower.Info.Age + UnityEngine.Random.Range(20, 30);
		}
		else
		{
			resurrectingFollower.Info.LifeExpectancy += UnityEngine.Random.Range(20, 30);
		}
		Follower revivedFollower = CreateNewFollower(resurrectingFollower._directInfoAccess, structures_Grave.Data.Position);
		resurrectingFollower.Location = FollowerLocation.Base;
		resurrectingFollower.DesiredLocation = FollowerLocation.Base;
		resurrectingFollower.HardSwapToTask(new FollowerTask_ManualControl());
		revivedFollower.SetOutfit(FollowerOutfitType.Worker, false);
		revivedFollower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(revivedFollower.gameObject, 7f);
		yield return new WaitForEndOfFrame();
		revivedFollower.SetBodyAnimation("Sermons/resurrect", false);
		revivedFollower.AddBodyAnimation("Reactions/react-enlightened1", false, 0f);
		revivedFollower.AddBodyAnimation("idle", true, 0f);
		yield return new WaitForSeconds(6f);
		resurrectingFollower.CompleteCurrentTask();
		GameManager.GetInstance().OnConversationEnd();
	}

	public static FollowerBrain MakeFollowerGainLevel()
	{
		List<FollowerBrain> list = new List<FollowerBrain>(FollowerBrain.AllBrains);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (FollowerLocked(list[num].Info.ID) || list[num].Stats.Adoration >= list[num].Stats.MAX_ADORATION)
			{
				list.RemoveAt(num);
			}
		}
		FollowerBrain followerBrain = ((list.Count > 0) ? list[UnityEngine.Random.Range(0, list.Count)] : null);
		if (followerBrain != null)
		{
			followerBrain.AddAdoration(FollowerBrain.AdorationActions.LevelUp, null);
		}
		return followerBrain;
	}

	public static FollowerBrain MakeFollowerLoseLevel()
	{
		List<FollowerBrain> list = new List<FollowerBrain>(FollowerBrain.AllBrains);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (FollowerLocked(list[num].Info.ID))
			{
				list.RemoveAt(num);
			}
		}
		FollowerBrain followerBrain = ((list.Count > 0) ? list[UnityEngine.Random.Range(0, list.Count)] : null);
		if (followerBrain != null)
		{
			followerBrain.LevelDown();
		}
		return followerBrain;
	}

	public static bool AreSiblings(int followerID_1, int followerID_2)
	{
		foreach (List<int> siblingID in SiblingIDs)
		{
			if (siblingID.Contains(followerID_1) && siblingID.Contains(followerID_2))
			{
				return true;
			}
		}
		return false;
	}
}
