using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;
using UnityEngine.U2D;

public class SermonsAndRituals
{
	public enum SermonRitualType
	{
		NONE,
		RITUAL_REBIRTH,
		RITUAL_CULT_ACCESSION_I,
		RITUAL_CULT_ACCESSION_II,
		SERMON_PURGE_SICKNESS,
		SERMON_THREAT_DISSENTERS,
		SERMON_DENOUNCE_NONBELEIVERS,
		SERMON_LOVE_THY_NEIGHBOUR,
		SERMON_RENOUNCE_FOOD,
		SERMON_OF_ENLIGHTENMENT,
		SERMON_UTOPIANISTS,
		SERMON_FUNDAMENTALISTS,
		SERMON_MISFITS,
		SERMON_DENOUNCE_GOAT,
		SERMON_DENOUNCE_OWL,
		SERMON_DENOUNCE_SNAKE,
		SERMON_DENOUNCE_FOLLOWER,
		SERMON_DILLIGENCE,
		RITUAL_PROMOTE_FOLLOWER,
		RITUAL_SACRIFICE_FOLLOWER,
		RITUAL_WEDDING,
		RITUAL_HEAL_SICK,
		RITUAL_NATURES_BOUNTY,
		Count
	}

	public static SermonRitualType CurrentSelectedType;

	public static FollowerInfo CurrentSelectedFollowerInfo;

	public static List<SermonRitualType> GetVisibleRituals()
	{
		List<SermonRitualType> list = new List<SermonRitualType>
		{
			SermonRitualType.RITUAL_SACRIFICE_FOLLOWER,
			SermonRitualType.RITUAL_REBIRTH
		};
		for (int i = 0; i < list.Count; i++)
		{
			if (!CheckUnlocked(list[i]))
			{
				list.RemoveAt(i--);
			}
		}
		return list;
	}

	public static List<SermonRitualType> GetAvailableRituals()
	{
		List<SermonRitualType> list = new List<SermonRitualType>
		{
			SermonRitualType.RITUAL_SACRIFICE_FOLLOWER,
			SermonRitualType.RITUAL_REBIRTH
		};
		for (int i = 0; i < list.Count; i++)
		{
			if (!CheckAvailabilityRequirementsMet(list[i]))
			{
				list.RemoveAt(i--);
			}
		}
		return list;
	}

	public static List<SermonRitualType> GetVisibleSermons()
	{
		List<SermonRitualType> list = new List<SermonRitualType>();
		for (int i = 0; i < list.Count; i++)
		{
			if (!CheckUnlocked(list[i]))
			{
				list.RemoveAt(i--);
			}
		}
		return list;
	}

	public static List<SermonRitualType> GetAvailableSermons()
	{
		List<SermonRitualType> list = new List<SermonRitualType>();
		for (int i = 0; i < list.Count; i++)
		{
			if (!CheckAvailabilityRequirementsMet(list[i]))
			{
				list.RemoveAt(i--);
			}
		}
		return list;
	}

	public static Sprite Sprite(SermonRitualType type)
	{
		SpriteAtlas spriteAtlas = Resources.Load<SpriteAtlas>("Atlases/Ritual&SermonIcons");
		string name = "";
		switch (type)
		{
		case SermonRitualType.RITUAL_SACRIFICE_FOLLOWER:
			name = "Ritual_Sacrifice";
			break;
		case SermonRitualType.RITUAL_CULT_ACCESSION_I:
			name = "Ritual_Cult Accension";
			break;
		case SermonRitualType.RITUAL_CULT_ACCESSION_II:
			name = "Ritual_Cult Accension";
			break;
		case SermonRitualType.RITUAL_REBIRTH:
			name = "Ritual_Rebirth";
			break;
		case SermonRitualType.SERMON_OF_ENLIGHTENMENT:
			name = "Sermon_Enlightenment";
			break;
		case SermonRitualType.SERMON_LOVE_THY_NEIGHBOUR:
			name = "Sermon_LoveThyNeighbour";
			break;
		case SermonRitualType.SERMON_PURGE_SICKNESS:
			name = "Sermon_PurgeSickness";
			break;
		case SermonRitualType.SERMON_RENOUNCE_FOOD:
			name = "Sermon_RenounceFood";
			break;
		case SermonRitualType.SERMON_THREAT_DISSENTERS:
			name = "Sermon_ThreatDissenters";
			break;
		case SermonRitualType.SERMON_DENOUNCE_NONBELEIVERS:
			name = "Sermon_NonBeleivers";
			break;
		}
		return spriteAtlas.GetSprite(name);
	}

	public static string LocalisedName(SermonRitualType type)
	{
		return LocalizationManager.GetTranslation(string.Format("SermonsAndRituals/{0}/Name", type));
	}

	public static string LocalisedLore(SermonRitualType type)
	{
		return LocalizationManager.GetTranslation(string.Format("SermonsAndRituals/{0}/Lore", type));
	}

	public static string LocalisedDescription(SermonRitualType type)
	{
		return LocalizationManager.GetTranslation(string.Format("SermonsAndRituals/{0}/Description", type));
	}

	public static string LocalisedEffects(SermonRitualType type)
	{
		return LocalizationManager.GetTranslation(string.Format("SermonsAndRituals/{0}/Effects", type));
	}

	public static string LocalisedPros(SermonRitualType type)
	{
		return LocalizationManager.GetTranslation(string.Format("SermonsAndRituals/{0}/Pros", type));
	}

	public static string LocalisedCons(SermonRitualType type)
	{
		return LocalizationManager.GetTranslation(string.Format("SermonsAndRituals/{0}/Cons", type));
	}

	public static int CooldownDays(SermonRitualType type)
	{
		switch (type)
		{
		case SermonRitualType.SERMON_OF_ENLIGHTENMENT:
			return 2;
		case SermonRitualType.SERMON_RENOUNCE_FOOD:
			return 5;
		case SermonRitualType.SERMON_LOVE_THY_NEIGHBOUR:
			return 7;
		case SermonRitualType.SERMON_DENOUNCE_NONBELEIVERS:
			return 7;
		case SermonRitualType.SERMON_THREAT_DISSENTERS:
			return 7;
		case SermonRitualType.SERMON_PURGE_SICKNESS:
			return 0;
		default:
			return 0;
		}
	}

	public static int CostDevotion(SermonRitualType type)
	{
		switch (type)
		{
		case SermonRitualType.RITUAL_REBIRTH:
			return 25;
		case SermonRitualType.SERMON_OF_ENLIGHTENMENT:
			return 0;
		case SermonRitualType.SERMON_RENOUNCE_FOOD:
			return 7;
		case SermonRitualType.SERMON_LOVE_THY_NEIGHBOUR:
			return 7;
		case SermonRitualType.SERMON_DENOUNCE_NONBELEIVERS:
			return 7;
		case SermonRitualType.SERMON_THREAT_DISSENTERS:
			return 7;
		case SermonRitualType.SERMON_PURGE_SICKNESS:
			return 10;
		default:
			return 0;
		}
	}

	public static string SkinAnimationIndex(SermonRitualType type)
	{
		switch (type)
		{
		case SermonRitualType.RITUAL_SACRIFICE_FOLLOWER:
			return "1";
		case SermonRitualType.RITUAL_CULT_ACCESSION_I:
			return "1";
		case SermonRitualType.RITUAL_CULT_ACCESSION_II:
			return "1";
		case SermonRitualType.RITUAL_REBIRTH:
			return "2";
		case SermonRitualType.SERMON_OF_ENLIGHTENMENT:
			return "2";
		case SermonRitualType.SERMON_LOVE_THY_NEIGHBOUR:
			return "3";
		case SermonRitualType.SERMON_PURGE_SICKNESS:
			return "4";
		case SermonRitualType.SERMON_RENOUNCE_FOOD:
			return "5";
		case SermonRitualType.SERMON_THREAT_DISSENTERS:
			return "6";
		case SermonRitualType.SERMON_DENOUNCE_NONBELEIVERS:
			return "7";
		default:
			return "";
		}
	}

	public static List<Follower> GetAffectedFollowers(SermonRitualType type)
	{
		return new List<Follower>();
	}

	public static List<string> GetReactions(SermonRitualType type)
	{
		switch (type)
		{
		case SermonRitualType.SERMON_OF_ENLIGHTENMENT:
			return new List<string> { "Reactions/react-enlightened1", "Reactions/react-enlightened2" };
		case SermonRitualType.SERMON_RENOUNCE_FOOD:
			return new List<string> { "Reactions/react-fasting1", "Reactions/react-fasting2" };
		case SermonRitualType.SERMON_LOVE_THY_NEIGHBOUR:
			return new List<string> { "Reactions/react-love1", "Reactions/react-love2" };
		case SermonRitualType.SERMON_THREAT_DISSENTERS:
			return new List<string> { "Reactions/react-worried1", "Reactions/react-worried2" };
		case SermonRitualType.SERMON_PURGE_SICKNESS:
			return new List<string> { "Sermons/sermon-heal" };
		case SermonRitualType.SERMON_DENOUNCE_NONBELEIVERS:
			return new List<string> { "Reactions/react-determined1", "Reactions/react-determined2" };
		default:
			return new List<string>();
		}
	}

	public static Action<Follower> PerFollowerCallbacks(SermonRitualType type)
	{
		switch (type)
		{
		case SermonRitualType.SERMON_RENOUNCE_FOOD:
			return delegate(Follower f)
			{
				f.Brain.Stats.Starvation = 0f;
				f.Brain.Stats.Satiation = 100f;
			};
		case SermonRitualType.SERMON_LOVE_THY_NEIGHBOUR:
			return delegate(Follower f)
			{
				f.Brain.Stats.GuaranteedGoodInteractionsUntil = DataManager.Instance.CurrentDayIndex + 2;
			};
		case SermonRitualType.SERMON_THREAT_DISSENTERS:
			return delegate(Follower f)
			{
				f.Brain.Stats.Reeducation = 0f;
			};
		case SermonRitualType.SERMON_PURGE_SICKNESS:
			return delegate(Follower f)
			{
				f.Brain.Stats.Illness = 0f;
			};
		case SermonRitualType.SERMON_DENOUNCE_NONBELEIVERS:
			return delegate(Follower f)
			{
				f.Brain.Stats.IncreasedDevotionOutputUntil = DataManager.Instance.CurrentDayIndex + 2;
			};
		default:
			return null;
		}
	}

	public static Action EffectCallbacks(SermonRitualType type)
	{
		if (type == SermonRitualType.RITUAL_REBIRTH)
		{
			return Rebirth;
		}
		return null;
	}

	public static void ApplySermonRitualBonus()
	{
		foreach (Follower item in FollowerManager.FollowersAtLocation(FollowerLocation.Church))
		{
			Follower follower = item;
		}
	}

	public static Action FinishCallbacks(SermonRitualType type)
	{
		switch (type)
		{
		case SermonRitualType.RITUAL_CULT_ACCESSION_I:
			return Ascension1;
		case SermonRitualType.RITUAL_CULT_ACCESSION_II:
			return Ascension2;
		default:
			return null;
		}
	}

	private static void Ascension1()
	{
		DataManager.Instance.CurrentCultLevel = DataManager.CultLevel.Two;
		UIAbilityUnlock.Play(UIAbilityUnlock.Ability.CultLevel1);
	}

	private static void Ascension2()
	{
		DataManager.Instance.CurrentCultLevel = DataManager.CultLevel.Three;
		UIAbilityUnlock.Play(UIAbilityUnlock.Ability.CultLevel2);
	}

	private static void Rebirth()
	{
		FollowerManager.ReviveFollower(CurrentSelectedFollowerInfo.ID, FollowerLocation.Church, ChurchFollowerManager.Instance.RitualCenterPosition.position);
		CurrentSelectedFollowerInfo = null;
		foreach (StructureBrain item in StructureManager.StructuresAtLocation(FollowerLocation.Base))
		{
			if ((item.Data.Type == StructureBrain.TYPES.GRAVE || item.Data.Type == StructureBrain.TYPES.BODY_PIT || item.Data.Type == StructureBrain.TYPES.GRAVE2) && item.Data.FollowerID == CurrentSelectedFollowerInfo.ID)
			{
				item.Data.FollowerID = -1;
			}
		}
	}

	public static bool CheckAvailabilityRequirementsMet(SermonRitualType type)
	{
		bool flag = TimeManager.GetSermonRitualCooldownRemaining(type) <= 0 && Inventory.Souls >= CostDevotion(type);
		switch (type)
		{
		case SermonRitualType.RITUAL_SACRIFICE_FOLLOWER:
			flag &= DataManager.Instance.Followers.Count >= 1;
			break;
		case SermonRitualType.RITUAL_CULT_ACCESSION_I:
			flag &= DataManager.Instance.CurrentCultLevel == DataManager.CultLevel.One && DataManager.Instance.Souls > 1 && DataManager.Instance.Followers.Count >= 1;
			break;
		case SermonRitualType.RITUAL_CULT_ACCESSION_II:
			flag &= DataManager.Instance.CurrentCultLevel == DataManager.CultLevel.Two;
			break;
		case SermonRitualType.RITUAL_REBIRTH:
			flag &= DataManager.Instance.Followers_Dead.Count >= 1;
			break;
		}
		return flag;
	}

	public static bool CheckUnlocked(SermonRitualType type)
	{
		return DataManager.Instance.UnlockedSermonsAndRituals.Contains(type);
	}
}
