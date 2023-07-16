using System;

public class UnlockManager : BaseMonoBehaviour
{
	public enum UnlockType
	{
		Starting,
		Illness,
		Dissenters,
		Hunger,
		FollowerDeath
	}

	public struct UnlockNotificationData
	{
		public SermonsAndRituals.SermonRitualType SermonRitualType;

		public StructureBrain.TYPES StructureType;
	}

	public HUD_ProblemUnlock ProblemUnlockPrefab;

	private void OnEnable()
	{
		foreach (object value in Enum.GetValues(typeof(UnlockType)))
		{
			if (!DataManager.Instance.MechanicsUnlocked.Contains((UnlockType)value))
			{
				TriggerProblemUnlock((UnlockType)value);
			}
		}
	}

	private void OnDisable()
	{
	}

	private void OnNewDayStarted()
	{
		if (DataManager.Instance.CurrentDayIndex >= 1)
		{
			TriggerProblemUnlock(UnlockType.Starting);
			TimeManager.OnNewDayStarted = (Action)Delegate.Remove(TimeManager.OnNewDayStarted, new Action(OnNewDayStarted));
		}
	}

	private void OnIllnessChanged(Villager_Info.StatusState state)
	{
		if (state == Villager_Info.StatusState.On)
		{
			TriggerProblemUnlock(UnlockType.Illness);
			Villager_Info.OnIllnessChanged = (Villager_Info.StatusEffectEvent)Delegate.Remove(Villager_Info.OnIllnessChanged, new Villager_Info.StatusEffectEvent(OnIllnessChanged));
		}
	}

	private void OnDissenterChanged(Villager_Info.StatusState state)
	{
		if (state == Villager_Info.StatusState.On)
		{
			TriggerProblemUnlock(UnlockType.Dissenters);
			Villager_Info.OnDissenterChanged = (Villager_Info.StatusEffectEvent)Delegate.Remove(Villager_Info.OnDissenterChanged, new Villager_Info.StatusEffectEvent(OnDissenterChanged));
		}
	}

	private void OnStarveChanged(Villager_Info.StatusState state)
	{
		if (state == Villager_Info.StatusState.On)
		{
			TriggerProblemUnlock(UnlockType.Hunger);
			Villager_Info.OnStarveChanged = (Villager_Info.StatusEffectEvent)Delegate.Remove(Villager_Info.OnStarveChanged, new Villager_Info.StatusEffectEvent(OnStarveChanged));
		}
	}

	private void OnWorshipperDied()
	{
		TriggerProblemUnlock(UnlockType.FollowerDeath);
		WorshipperInfoManager.OnWorshipperDied = (Action)Delegate.Remove(WorshipperInfoManager.OnWorshipperDied, new Action(OnWorshipperDied));
	}

	private void TriggerProblemUnlock(UnlockType type)
	{
		DataManager.Instance.MechanicsUnlocked.Add(type);
		UnlockNotificationData[] unlockNotifications = GetUnlockNotifications(type);
		for (int i = 0; i < unlockNotifications.Length; i++)
		{
			UnlockNotificationData unlockNotificationData = unlockNotifications[i];
			if (unlockNotificationData.SermonRitualType != 0 && !DataManager.Instance.UnlockedSermonsAndRituals.Contains(unlockNotificationData.SermonRitualType))
			{
				DataManager.Instance.UnlockedSermonsAndRituals.Add(unlockNotificationData.SermonRitualType);
			}
			if (unlockNotificationData.StructureType != 0 && !DataManager.Instance.UnlockedStructures.Contains(unlockNotificationData.StructureType))
			{
				DataManager.Instance.UnlockedStructures.Add(unlockNotificationData.StructureType);
			}
		}
	}

	public static UnlockNotificationData[] GetUnlockNotifications(UnlockType type)
	{
		switch (type)
		{
		case UnlockType.Starting:
			return new UnlockNotificationData[1]
			{
				new UnlockNotificationData
				{
					SermonRitualType = SermonsAndRituals.SermonRitualType.SERMON_OF_ENLIGHTENMENT
				}
			};
		case UnlockType.Illness:
			return new UnlockNotificationData[1]
			{
				new UnlockNotificationData
				{
					SermonRitualType = SermonsAndRituals.SermonRitualType.SERMON_PURGE_SICKNESS
				}
			};
		case UnlockType.Dissenters:
			return new UnlockNotificationData[2]
			{
				new UnlockNotificationData
				{
					SermonRitualType = SermonsAndRituals.SermonRitualType.RITUAL_SACRIFICE_FOLLOWER
				},
				new UnlockNotificationData
				{
					SermonRitualType = SermonsAndRituals.SermonRitualType.SERMON_THREAT_DISSENTERS
				}
			};
		case UnlockType.Hunger:
			return new UnlockNotificationData[1]
			{
				new UnlockNotificationData
				{
					SermonRitualType = SermonsAndRituals.SermonRitualType.SERMON_RENOUNCE_FOOD
				}
			};
		case UnlockType.FollowerDeath:
			return new UnlockNotificationData[1]
			{
				new UnlockNotificationData
				{
					SermonRitualType = SermonsAndRituals.SermonRitualType.RITUAL_REBIRTH
				}
			};
		default:
			return new UnlockNotificationData[0];
		}
	}

	public static string GetUnlockAnimationName(UnlockType type)
	{
		switch (type)
		{
		case UnlockType.Starting:
			return "Avatars/avatar-happy";
		case UnlockType.Illness:
			return "Avatars/avatar-sick";
		case UnlockType.Dissenters:
			return "Avatars/avatar-dissenter1";
		case UnlockType.Hunger:
			return "Avatars/avatar-unhappy";
		case UnlockType.FollowerDeath:
			return "Avatars/avatar-dead";
		default:
			return "";
		}
	}

	public static string GetUnlockTitle(UnlockType type)
	{
		switch (type)
		{
		case UnlockType.Starting:
			return "A Gift...";
		case UnlockType.Illness:
			return "Sickness";
		case UnlockType.Dissenters:
			return "Dissenters";
		case UnlockType.Hunger:
			return "Hunger";
		case UnlockType.FollowerDeath:
			return "Death";
		default:
			return "";
		}
	}

	public static string GetUnlockSubtitle(UnlockType type)
	{
		switch (type)
		{
		case UnlockType.Starting:
			return "To help keep your Followers happy";
		case UnlockType.Illness:
			return "A Follower has become ill";
		case UnlockType.Dissenters:
			return "A Follower is spreading dissent";
		case UnlockType.Hunger:
			return "A Follower is starving";
		case UnlockType.FollowerDeath:
			return "A Follower has died";
		default:
			return "";
		}
	}
}
