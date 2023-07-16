using System;
using System.Collections.Generic;
using I2.Loc;
using Unify;
using UnityEngine;
using UnityEngine.U2D;

public class DoctrineUpgradeSystem : BaseMonoBehaviour
{
	public enum DoctrineType
	{
		None,
		Category_Afterlife,
		Category_Food,
		Category_LawAndOrder,
		Category_Possession,
		Category_WorkAndWorship,
		WorkWorship_Inspire,
		WorkWorship_Intimidate,
		WorkWorship_FasterBuilding,
		WorkWorship_Enlightenment,
		WorkWorship_FaithfulTrait,
		WorkWorship_GoodWorkerTrait,
		WorkWorship_WorkThroughNightRitual,
		WorkWorship_HolidayRitual,
		Possessions_ExtortTithes,
		Possessions_Bribe,
		Possessions_MoreFaithFromHomes,
		Possessions_MoreFaithFromRituals,
		Possessions_TraitMaterialistic,
		Possessions_TraitFalseIdols,
		Possessions_AlmsToPoorRitual,
		Possessions_DonationRitual,
		Sustenance_Fast,
		Sustenance_Feast,
		Sustenance_TraitMushroomEncouraged,
		Sustenance_TraitMushroomBanned,
		Sustenance_TraitCannibal,
		Sustenance_TraitGrassEater,
		Sustenance_TraitHarvestRitual,
		Sustenance_TraitFishingRitual,
		DeathSacrifice_TraitSacrificeEnthusiast,
		DeathSacrifice_TraitDesensitisedToDeath,
		DeathSacrifice_RessurectionRitual,
		DeathSacrifice_Funeral,
		DeathSacrifice_TraitRespectElders,
		DeathSacrifice_TraitOldDieYoung,
		DeathSacrifice_BuildingReturnToEarth,
		DeathSacrifice_BuildingGoodGraves,
		LawOrder_MurderFollower,
		LawOrder_AscendFollower,
		LawOrder_FightPitRitual,
		LawOrder_JudgementRitual,
		LawOrder_AssignFaithEnforcerRitual,
		LawOrder_AssignTaxCollectorRitual,
		LawOrder_TraitDisciplinarian,
		LawOrder_TraitLibertarian,
		Special_Brainwashed,
		Special_Sacrifice,
		Special_Consume,
		Special_ReadMind,
		Special_Bonfire,
		Special_Halloween,
		Count
	}

	public enum DoctrineCategory
	{
		None,
		Trait,
		FollowerAction,
		Ritual
	}

	public enum DoctrineUnlockType
	{
		None,
		Ritual,
		FollowerAbility,
		Building,
		Trait
	}

	public const int kMaxSermonLevel = 4;

	public static Action<DoctrineType> OnDoctrineUnlocked;

	public static bool GiveInstantCheat;

	public static Action OnAbilityPointDelta;

	public const int BribeCost = 3;

	public static List<DoctrineType> UnlockedUpgrades
	{
		get
		{
			return DataManager.Instance.DoctrineUnlockedUpgrades;
		}
		set
		{
			DataManager.Instance.DoctrineUnlockedUpgrades = value;
		}
	}

	public static bool TryGetStillDoctrineStone()
	{
		return (0 + (4 - GetLevelBySermon(SermonCategory.Afterlife)) + (4 - GetLevelBySermon(SermonCategory.Food)) + (4 - GetLevelBySermon(SermonCategory.Possession)) + (4 - GetLevelBySermon(SermonCategory.WorkAndWorship)) + (4 - GetLevelBySermon(SermonCategory.LawAndOrder)) - DataManager.Instance.CompletedDoctrineStones) * 3 - DataManager.Instance.DoctrineCurrentCount > 0;
	}

	public static bool TrySermonsStillAvailable()
	{
		if (DataManager.Instance.dungeonRun < 2 && !DataManager.Instance.ForceDoctrineStones)
		{
			return false;
		}
		if (GetLevelBySermon(SermonCategory.Afterlife) < 4)
		{
			return true;
		}
		if (GetLevelBySermon(SermonCategory.Food) < 4)
		{
			return true;
		}
		if (GetLevelBySermon(SermonCategory.Possession) < 4)
		{
			return true;
		}
		if (GetLevelBySermon(SermonCategory.WorkAndWorship) < 4)
		{
			return true;
		}
		if (GetLevelBySermon(SermonCategory.LawAndOrder) < 4)
		{
			return true;
		}
		if (GetLevelBySermon(SermonCategory.Special) < 1)
		{
			return true;
		}
		return false;
	}

	public static int TotalRemainingDoctrinesAvailable()
	{
		return 40 - GetUnlockedDoctrinesForCategory(SermonCategory.Afterlife).Count - GetUnlockedDoctrinesForCategory(SermonCategory.Food).Count - GetUnlockedDoctrinesForCategory(SermonCategory.Possession).Count - GetUnlockedDoctrinesForCategory(SermonCategory.WorkAndWorship).Count - GetUnlockedDoctrinesForCategory(SermonCategory.Special).Count;
	}

	public static float GetXPBySermon(SermonCategory sermonCategory)
	{
		switch (sermonCategory)
		{
		case SermonCategory.Afterlife:
			return DataManager.Instance.Doctrine_Afterlife_XP;
		case SermonCategory.Food:
			return DataManager.Instance.Doctrine_Food_XP;
		case SermonCategory.LawAndOrder:
			return DataManager.Instance.Doctrine_LawAndOrder_XP;
		case SermonCategory.Possession:
			return DataManager.Instance.Doctrine_Possessions_XP;
		case SermonCategory.WorkAndWorship:
			return DataManager.Instance.Doctrine_WorkWorship_XP;
		case SermonCategory.PlayerUpgrade:
			return DataManager.Instance.Doctrine_PlayerUpgrade_XP;
		default:
			return 0f;
		}
	}

	public static float GetXPBySermonNormalised(SermonCategory sermonCategory)
	{
		switch (sermonCategory)
		{
		case SermonCategory.Afterlife:
			return DataManager.Instance.Doctrine_Afterlife_XP / GetXPTargetBySermon(sermonCategory);
		case SermonCategory.Food:
			return DataManager.Instance.Doctrine_Food_XP / GetXPTargetBySermon(sermonCategory);
		case SermonCategory.LawAndOrder:
			return DataManager.Instance.Doctrine_LawAndOrder_XP / GetXPTargetBySermon(sermonCategory);
		case SermonCategory.Possession:
			return DataManager.Instance.Doctrine_Possessions_XP / GetXPTargetBySermon(sermonCategory);
		case SermonCategory.WorkAndWorship:
			return DataManager.Instance.Doctrine_WorkWorship_XP / GetXPTargetBySermon(sermonCategory);
		case SermonCategory.Special:
			return DataManager.Instance.Doctrine_Special_XP / GetXPTargetBySermon(sermonCategory);
		case SermonCategory.PlayerUpgrade:
			return DataManager.Instance.Doctrine_PlayerUpgrade_XP / GetXPTargetBySermon(sermonCategory);
		default:
			return 0f;
		}
	}

	public static float GetXPTargetBySermon(SermonCategory sermonCategory)
	{
		switch (sermonCategory)
		{
		case SermonCategory.Afterlife:
			return DataManager.DoctrineTargetXP[Mathf.Min(GetLevelBySermon(sermonCategory), DataManager.DoctrineTargetXP.Count - 1)];
		case SermonCategory.Food:
			return DataManager.DoctrineTargetXP[Mathf.Min(GetLevelBySermon(sermonCategory), DataManager.DoctrineTargetXP.Count - 1)];
		case SermonCategory.LawAndOrder:
			return DataManager.DoctrineTargetXP[Mathf.Min(GetLevelBySermon(sermonCategory), DataManager.DoctrineTargetXP.Count - 1)];
		case SermonCategory.Possession:
			return DataManager.DoctrineTargetXP[Mathf.Min(GetLevelBySermon(sermonCategory), DataManager.DoctrineTargetXP.Count - 1)];
		case SermonCategory.WorkAndWorship:
			return DataManager.DoctrineTargetXP[Mathf.Min(GetLevelBySermon(sermonCategory), DataManager.DoctrineTargetXP.Count - 1)];
		case SermonCategory.PlayerUpgrade:
			return DataManager.PlayerUpgradeTargetXP[Mathf.Min(GetLevelBySermon(sermonCategory), DataManager.PlayerUpgradeTargetXP.Count - 1)];
		case SermonCategory.Special:
			return (float)Ritual.FollowersAvailableToAttendSermon() * 0.1f;
		default:
			return 0f;
		}
	}

	public static void SetXPBySermon(SermonCategory sermonCategory, float Value)
	{
		switch (sermonCategory)
		{
		case SermonCategory.Afterlife:
			DataManager.Instance.Doctrine_Afterlife_XP = Value;
			break;
		case SermonCategory.Food:
			DataManager.Instance.Doctrine_Food_XP = Value;
			break;
		case SermonCategory.LawAndOrder:
			DataManager.Instance.Doctrine_LawAndOrder_XP = Value;
			break;
		case SermonCategory.Possession:
			DataManager.Instance.Doctrine_Possessions_XP = Value;
			break;
		case SermonCategory.WorkAndWorship:
			DataManager.Instance.Doctrine_WorkWorship_XP = Value;
			break;
		case SermonCategory.PlayerUpgrade:
			DataManager.Instance.Doctrine_PlayerUpgrade_XP = Value;
			break;
		case SermonCategory.Special:
			DataManager.Instance.Doctrine_Special_XP = Value;
			break;
		}
	}

	public static int GetLevelBySermon(SermonCategory sermonCategory)
	{
		switch (sermonCategory)
		{
		case SermonCategory.Afterlife:
			return DataManager.Instance.Doctrine_Afterlife_Level;
		case SermonCategory.Food:
			return DataManager.Instance.Doctrine_Food_Level;
		case SermonCategory.LawAndOrder:
			return DataManager.Instance.Doctrine_LawAndOrder_Level;
		case SermonCategory.Possession:
			return DataManager.Instance.Doctrine_Possessions_Level;
		case SermonCategory.WorkAndWorship:
			return DataManager.Instance.Doctrine_WorkWorship_Level;
		case SermonCategory.PlayerUpgrade:
			return DataManager.Instance.Doctrine_PlayerUpgrade_Level;
		case SermonCategory.Special:
			return DataManager.Instance.Doctrine_Special_Level;
		default:
			return 0;
		}
	}

	public static void SetLevelBySermon(SermonCategory sermonCategory, int Value)
	{
		Debug.Log("SET LEVEL!");
		switch (sermonCategory)
		{
		case SermonCategory.Afterlife:
			DataManager.Instance.Doctrine_Afterlife_Level = Value;
			break;
		case SermonCategory.Food:
			DataManager.Instance.Doctrine_Food_Level = Value;
			break;
		case SermonCategory.LawAndOrder:
			DataManager.Instance.Doctrine_LawAndOrder_Level = Value;
			break;
		case SermonCategory.Possession:
			DataManager.Instance.Doctrine_Possessions_Level = Value;
			break;
		case SermonCategory.WorkAndWorship:
			DataManager.Instance.Doctrine_WorkWorship_Level = Value;
			break;
		case SermonCategory.PlayerUpgrade:
			DataManager.Instance.Doctrine_PlayerUpgrade_Level = Value;
			break;
		case SermonCategory.Special:
			DataManager.Instance.Doctrine_Special_Level = Value;
			break;
		}
		if (DataManager.Instance.Doctrine_Afterlife_Level == 4 && DataManager.Instance.Doctrine_Food_Level == 4 && DataManager.Instance.Doctrine_LawAndOrder_Level == 4 && DataManager.Instance.Doctrine_Possessions_Level == 4 && DataManager.Instance.Doctrine_WorkWorship_Level == 4)
		{
			AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("UPGRADE_ALL_SERMONS"));
		}
	}

	public static string GetSermonCategoryIcon(SermonCategory Category)
	{
		switch (Category)
		{
		case SermonCategory.Afterlife:
			return "\uf714";
		case SermonCategory.Food:
			return "\uf623";
		case SermonCategory.LawAndOrder:
			return "\uf0e3";
		case SermonCategory.Possession:
			return "\uf890";
		case SermonCategory.WorkAndWorship:
			return "\uf683";
		case SermonCategory.PlayerUpgrade:
			return "";
		case SermonCategory.Special:
			return "";
		default:
			return "Uh Oh something happened";
		}
	}

	public static string GetSermonCategoryLocalizedName(SermonCategory Category)
	{
		return LocalizationManager.GetTranslation(string.Format("DoctrineUpgradeSystem/{0}", Category));
	}

	public static string GetSermonCategoryLocalizedDescription(SermonCategory Category)
	{
		return LocalizationManager.GetTranslation(string.Format("DoctrineUpgradeSystem/{0}/Description", Category));
	}

	public static SermonCategory GetCategory(DoctrineType d)
	{
		switch (d)
		{
		case DoctrineType.WorkWorship_Inspire:
		case DoctrineType.WorkWorship_Intimidate:
		case DoctrineType.WorkWorship_FasterBuilding:
		case DoctrineType.WorkWorship_Enlightenment:
		case DoctrineType.WorkWorship_FaithfulTrait:
		case DoctrineType.WorkWorship_GoodWorkerTrait:
		case DoctrineType.WorkWorship_WorkThroughNightRitual:
		case DoctrineType.WorkWorship_HolidayRitual:
			return SermonCategory.WorkAndWorship;
		case DoctrineType.Possessions_ExtortTithes:
		case DoctrineType.Possessions_Bribe:
		case DoctrineType.Possessions_MoreFaithFromHomes:
		case DoctrineType.Possessions_MoreFaithFromRituals:
		case DoctrineType.Possessions_TraitMaterialistic:
		case DoctrineType.Possessions_TraitFalseIdols:
		case DoctrineType.Possessions_AlmsToPoorRitual:
		case DoctrineType.Possessions_DonationRitual:
			return SermonCategory.Possession;
		case DoctrineType.Sustenance_Fast:
		case DoctrineType.Sustenance_Feast:
		case DoctrineType.Sustenance_TraitMushroomEncouraged:
		case DoctrineType.Sustenance_TraitMushroomBanned:
		case DoctrineType.Sustenance_TraitCannibal:
		case DoctrineType.Sustenance_TraitGrassEater:
		case DoctrineType.Sustenance_TraitHarvestRitual:
		case DoctrineType.Sustenance_TraitFishingRitual:
			return SermonCategory.Food;
		case DoctrineType.DeathSacrifice_TraitSacrificeEnthusiast:
		case DoctrineType.DeathSacrifice_TraitDesensitisedToDeath:
		case DoctrineType.DeathSacrifice_RessurectionRitual:
		case DoctrineType.DeathSacrifice_Funeral:
		case DoctrineType.DeathSacrifice_TraitRespectElders:
		case DoctrineType.DeathSacrifice_TraitOldDieYoung:
		case DoctrineType.DeathSacrifice_BuildingReturnToEarth:
		case DoctrineType.DeathSacrifice_BuildingGoodGraves:
			return SermonCategory.Afterlife;
		case DoctrineType.LawOrder_MurderFollower:
		case DoctrineType.LawOrder_AscendFollower:
		case DoctrineType.LawOrder_FightPitRitual:
		case DoctrineType.LawOrder_JudgementRitual:
		case DoctrineType.LawOrder_AssignFaithEnforcerRitual:
		case DoctrineType.LawOrder_AssignTaxCollectorRitual:
		case DoctrineType.LawOrder_TraitDisciplinarian:
		case DoctrineType.LawOrder_TraitLibertarian:
			return SermonCategory.LawAndOrder;
		default:
			return SermonCategory.None;
		}
	}

	public static DoctrineCategory ShowDoctrineTutorialForType(DoctrineType Type)
	{
		switch (Type)
		{
		case DoctrineType.WorkWorship_FaithfulTrait:
		case DoctrineType.WorkWorship_GoodWorkerTrait:
		case DoctrineType.Possessions_MoreFaithFromHomes:
		case DoctrineType.Possessions_MoreFaithFromRituals:
		case DoctrineType.Possessions_TraitMaterialistic:
		case DoctrineType.Possessions_TraitFalseIdols:
		case DoctrineType.Sustenance_TraitMushroomEncouraged:
		case DoctrineType.Sustenance_TraitMushroomBanned:
		case DoctrineType.Sustenance_TraitCannibal:
		case DoctrineType.Sustenance_TraitGrassEater:
		case DoctrineType.DeathSacrifice_TraitSacrificeEnthusiast:
		case DoctrineType.DeathSacrifice_TraitDesensitisedToDeath:
		case DoctrineType.DeathSacrifice_TraitRespectElders:
		case DoctrineType.DeathSacrifice_TraitOldDieYoung:
		case DoctrineType.LawOrder_TraitDisciplinarian:
		case DoctrineType.LawOrder_TraitLibertarian:
			return DoctrineCategory.Trait;
		case DoctrineType.WorkWorship_Inspire:
		case DoctrineType.WorkWorship_Intimidate:
		case DoctrineType.Possessions_ExtortTithes:
		case DoctrineType.Possessions_Bribe:
		case DoctrineType.LawOrder_MurderFollower:
		case DoctrineType.LawOrder_AscendFollower:
		case DoctrineType.Special_ReadMind:
			return DoctrineCategory.FollowerAction;
		case DoctrineType.WorkWorship_FasterBuilding:
		case DoctrineType.WorkWorship_Enlightenment:
		case DoctrineType.WorkWorship_WorkThroughNightRitual:
		case DoctrineType.WorkWorship_HolidayRitual:
		case DoctrineType.Possessions_AlmsToPoorRitual:
		case DoctrineType.Possessions_DonationRitual:
		case DoctrineType.Sustenance_Fast:
		case DoctrineType.Sustenance_Feast:
		case DoctrineType.Sustenance_TraitHarvestRitual:
		case DoctrineType.Sustenance_TraitFishingRitual:
		case DoctrineType.DeathSacrifice_RessurectionRitual:
		case DoctrineType.DeathSacrifice_Funeral:
		case DoctrineType.LawOrder_FightPitRitual:
		case DoctrineType.LawOrder_JudgementRitual:
		case DoctrineType.LawOrder_AssignFaithEnforcerRitual:
		case DoctrineType.LawOrder_AssignTaxCollectorRitual:
		case DoctrineType.Special_Brainwashed:
		case DoctrineType.Special_Sacrifice:
		case DoctrineType.Special_Bonfire:
			return DoctrineCategory.Ritual;
		default:
			return DoctrineCategory.None;
		}
	}

	public static DoctrineType GetSermonReward(SermonCategory sermonCategory, int Level, bool FirstChoice)
	{
		switch (sermonCategory)
		{
		case SermonCategory.WorkAndWorship:
			switch (Level)
			{
			case 1:
				if (!FirstChoice)
				{
					return DoctrineType.WorkWorship_GoodWorkerTrait;
				}
				return DoctrineType.WorkWorship_FaithfulTrait;
			case 2:
				if (!FirstChoice)
				{
					return DoctrineType.WorkWorship_Intimidate;
				}
				return DoctrineType.WorkWorship_Inspire;
			case 3:
				if (!FirstChoice)
				{
					return DoctrineType.WorkWorship_Enlightenment;
				}
				return DoctrineType.WorkWorship_FasterBuilding;
			case 4:
				if (!FirstChoice)
				{
					return DoctrineType.WorkWorship_HolidayRitual;
				}
				return DoctrineType.WorkWorship_WorkThroughNightRitual;
			}
			break;
		case SermonCategory.Possession:
			switch (Level)
			{
			case 1:
				if (!FirstChoice)
				{
					return DoctrineType.Possessions_Bribe;
				}
				return DoctrineType.Possessions_ExtortTithes;
			case 2:
				if (!FirstChoice)
				{
					return DoctrineType.Possessions_TraitFalseIdols;
				}
				return DoctrineType.Possessions_TraitMaterialistic;
			case 3:
				if (!FirstChoice)
				{
					return DoctrineType.Possessions_DonationRitual;
				}
				return DoctrineType.Possessions_AlmsToPoorRitual;
			case 4:
				if (!FirstChoice)
				{
					return DoctrineType.Possessions_MoreFaithFromRituals;
				}
				return DoctrineType.Possessions_MoreFaithFromHomes;
			}
			break;
		case SermonCategory.Food:
			switch (Level)
			{
			case 1:
				if (!FirstChoice)
				{
					return DoctrineType.Sustenance_Feast;
				}
				return DoctrineType.Sustenance_Fast;
			case 2:
				if (!FirstChoice)
				{
					return DoctrineType.Sustenance_TraitGrassEater;
				}
				return DoctrineType.Sustenance_TraitCannibal;
			case 3:
				if (!FirstChoice)
				{
					return DoctrineType.Sustenance_TraitFishingRitual;
				}
				return DoctrineType.Sustenance_TraitHarvestRitual;
			case 4:
				if (!FirstChoice)
				{
					return DoctrineType.Sustenance_TraitMushroomBanned;
				}
				return DoctrineType.Sustenance_TraitMushroomEncouraged;
			}
			break;
		case SermonCategory.Afterlife:
			switch (Level)
			{
			case 1:
				if (!FirstChoice)
				{
					return DoctrineType.DeathSacrifice_TraitDesensitisedToDeath;
				}
				return DoctrineType.DeathSacrifice_TraitSacrificeEnthusiast;
			case 2:
				if (!FirstChoice)
				{
					return DoctrineType.DeathSacrifice_Funeral;
				}
				return DoctrineType.DeathSacrifice_RessurectionRitual;
			case 3:
				if (!FirstChoice)
				{
					return DoctrineType.DeathSacrifice_TraitOldDieYoung;
				}
				return DoctrineType.DeathSacrifice_TraitRespectElders;
			case 4:
				if (!FirstChoice)
				{
					return DoctrineType.DeathSacrifice_BuildingGoodGraves;
				}
				return DoctrineType.DeathSacrifice_BuildingReturnToEarth;
			}
			break;
		case SermonCategory.LawAndOrder:
			switch (Level)
			{
			case 1:
				if (!FirstChoice)
				{
					return DoctrineType.LawOrder_AscendFollower;
				}
				return DoctrineType.LawOrder_MurderFollower;
			case 2:
				if (!FirstChoice)
				{
					return DoctrineType.LawOrder_JudgementRitual;
				}
				return DoctrineType.LawOrder_FightPitRitual;
			case 3:
				if (!FirstChoice)
				{
					return DoctrineType.LawOrder_TraitLibertarian;
				}
				return DoctrineType.LawOrder_TraitDisciplinarian;
			case 4:
				if (!FirstChoice)
				{
					return DoctrineType.LawOrder_AssignTaxCollectorRitual;
				}
				return DoctrineType.LawOrder_AssignFaithEnforcerRitual;
			}
			break;
		case SermonCategory.Special:
			switch (Level)
			{
			case 1:
				return DoctrineType.Special_Bonfire;
			case 2:
				return DoctrineType.Special_ReadMind;
			case 3:
				return DoctrineType.Special_Sacrifice;
			case 4:
				return DoctrineType.Special_Brainwashed;
			}
			break;
		}
		if (Level >= 5 && GetRemainingDoctrines(sermonCategory).Count > 0)
		{
			if (!FirstChoice)
			{
				if (GetRemainingDoctrines(sermonCategory).Count <= 1)
				{
					return DoctrineType.None;
				}
				return GetRemainingDoctrines(sermonCategory)[1];
			}
			return GetRemainingDoctrines(sermonCategory)[0];
		}
		return DoctrineType.None;
	}

	public static List<DoctrineType> GetRemainingDoctrines(SermonCategory sermonCategory)
	{
		List<DoctrineType> list = new List<DoctrineType>();
		for (int i = 1; i < 5; i++)
		{
			DoctrineType sermonReward = GetSermonReward(sermonCategory, i, true);
			DoctrineType sermonReward2 = GetSermonReward(sermonCategory, i, false);
			if (UnlockedUpgrades.Contains(sermonReward) || UnlockedUpgrades.Contains(sermonReward2))
			{
				if (!UnlockedUpgrades.Contains(sermonReward))
				{
					list.Add(sermonReward);
				}
				if (!UnlockedUpgrades.Contains(sermonReward2))
				{
					list.Add(sermonReward2);
				}
			}
		}
		return list;
	}

	public static List<DoctrineType> GetAllRemainingDoctrines()
	{
		List<DoctrineType> list = new List<DoctrineType>();
		list.AddRange(GetRemainingDoctrines(SermonCategory.WorkAndWorship));
		list.AddRange(GetRemainingDoctrines(SermonCategory.Food));
		list.AddRange(GetRemainingDoctrines(SermonCategory.Afterlife));
		list.AddRange(GetRemainingDoctrines(SermonCategory.LawAndOrder));
		list.AddRange(GetRemainingDoctrines(SermonCategory.Possession));
		return list;
	}

	public static List<DoctrineType> GetUnlockedDoctrinesForCategory(SermonCategory sermonCategory)
	{
		List<DoctrineType> list = new List<DoctrineType>();
		foreach (DoctrineType doctrineUnlockedUpgrade in DataManager.Instance.DoctrineUnlockedUpgrades)
		{
			if (GetCategory(doctrineUnlockedUpgrade) == sermonCategory)
			{
				list.Add(doctrineUnlockedUpgrade);
			}
		}
		return list;
	}

	public static string GetLocalizedName(DoctrineType Type)
	{
		return LocalizationManager.GetTranslation(string.Format("DoctrineUpgradeSystem/{0}", Type));
	}

	public static string GetLocalizedDescription(DoctrineType Type)
	{
		if (Type == DoctrineType.Special_Bonfire)
		{
			return LocalizationManager.GetTranslation(string.Format("DoctrineUpgradeSystem/{0}/Description", Type)) + "<br><sprite name=\"icon_Faith\">" + (" +" + UpgradeSystem.GetRitualFaithChange(UpgradeSystem.Type.Ritual_FirePit)).Colour(StaticColors.GreenColor);
		}
		return LocalizationManager.GetTranslation(string.Format("DoctrineUpgradeSystem/{0}/Description", Type));
	}

	public static bool GetUnlocked(DoctrineType Type)
	{
		return UnlockedUpgrades.Contains(Type);
	}

	private static Sprite GetIcon(string icon)
	{
		return Resources.Load<SpriteAtlas>("Atlases/DoctrineAbilityIcons").GetSprite(icon);
	}

	public static Sprite GetIcon(DoctrineType type)
	{
		return GetIcon(type.ToString());
	}

	public static bool UnlockAbility(DoctrineType Type)
	{
		if (!UnlockedUpgrades.Contains(Type))
		{
			UnlockedUpgrades.Add(Type);
			DataManager.Instance.Alerts.Doctrine.Add(Type);
			OnUnlockAbility(Type);
			return true;
		}
		return false;
	}

	private static void OnUnlockAbility(DoctrineType Type)
	{
		Debug.Log("UNLOCKED!! " + Type);
		switch (Type)
		{
		case DoctrineType.WorkWorship_FaithfulTrait:
			FollowerTrait.AddCultTrait(FollowerTrait.TraitType.Faithful);
			break;
		case DoctrineType.WorkWorship_GoodWorkerTrait:
			FollowerTrait.AddCultTrait(FollowerTrait.TraitType.Industrious);
			break;
		case DoctrineType.Possessions_TraitMaterialistic:
			FollowerTrait.AddCultTrait(FollowerTrait.TraitType.Materialistic);
			break;
		case DoctrineType.Possessions_TraitFalseIdols:
			FollowerTrait.AddCultTrait(FollowerTrait.TraitType.FalseIdols);
			break;
		case DoctrineType.Possessions_MoreFaithFromHomes:
			FollowerTrait.AddCultTrait(FollowerTrait.TraitType.ConstructionEnthusiast);
			break;
		case DoctrineType.Possessions_MoreFaithFromRituals:
			FollowerTrait.AddCultTrait(FollowerTrait.TraitType.SermonEnthusiast);
			break;
		case DoctrineType.Sustenance_TraitCannibal:
			FollowerTrait.AddCultTrait(FollowerTrait.TraitType.Cannibal);
			JudgementMeter.ShowModify(-1);
			break;
		case DoctrineType.Sustenance_TraitGrassEater:
			FollowerTrait.AddCultTrait(FollowerTrait.TraitType.GrassEater);
			JudgementMeter.ShowModify(1);
			break;
		case DoctrineType.DeathSacrifice_TraitSacrificeEnthusiast:
			FollowerTrait.AddCultTrait(FollowerTrait.TraitType.SacrificeEnthusiast);
			break;
		case DoctrineType.DeathSacrifice_TraitDesensitisedToDeath:
			FollowerTrait.AddCultTrait(FollowerTrait.TraitType.DesensitisedToDeath);
			break;
		case DoctrineType.Sustenance_TraitMushroomEncouraged:
			FollowerTrait.AddCultTrait(FollowerTrait.TraitType.MushroomEncouraged);
			break;
		case DoctrineType.Sustenance_TraitMushroomBanned:
			FollowerTrait.AddCultTrait(FollowerTrait.TraitType.MushroomBanned);
			break;
		case DoctrineType.LawOrder_TraitDisciplinarian:
			FollowerTrait.AddCultTrait(FollowerTrait.TraitType.Disciplinarian);
			JudgementMeter.ShowModify(-1);
			break;
		case DoctrineType.LawOrder_TraitLibertarian:
			FollowerTrait.AddCultTrait(FollowerTrait.TraitType.Libertarian);
			JudgementMeter.ShowModify(1);
			break;
		case DoctrineType.DeathSacrifice_TraitRespectElders:
			FollowerTrait.AddCultTrait(FollowerTrait.TraitType.LoveElderly);
			JudgementMeter.ShowModify(1);
			break;
		case DoctrineType.DeathSacrifice_TraitOldDieYoung:
			FollowerTrait.AddCultTrait(FollowerTrait.TraitType.HateElderly);
			JudgementMeter.ShowModify(-1);
			break;
		case DoctrineType.Special_Sacrifice:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Sacrifice);
			break;
		case DoctrineType.Special_Bonfire:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_FirePit);
			break;
		case DoctrineType.Special_ReadMind:
			DataManager.Instance.CanReadMinds = true;
			break;
		case DoctrineType.Special_Brainwashed:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Brainwashing);
			break;
		case DoctrineType.Special_Consume:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_ConsumeFollower);
			break;
		case DoctrineType.LawOrder_FightPitRitual:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Fightpit);
			JudgementMeter.ShowModify(-1);
			break;
		case DoctrineType.LawOrder_JudgementRitual:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Wedding);
			JudgementMeter.ShowModify(1);
			break;
		case DoctrineType.WorkWorship_FasterBuilding:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_FasterBuilding);
			break;
		case DoctrineType.Sustenance_Fast:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Fast);
			JudgementMeter.ShowModify(-1);
			break;
		case DoctrineType.Sustenance_Feast:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Feast);
			JudgementMeter.ShowModify(1);
			break;
		case DoctrineType.WorkWorship_Enlightenment:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Enlightenment);
			break;
		case DoctrineType.WorkWorship_WorkThroughNightRitual:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_WorkThroughNight);
			break;
		case DoctrineType.WorkWorship_HolidayRitual:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Holiday);
			JudgementMeter.ShowModify(1);
			break;
		case DoctrineType.Sustenance_TraitFishingRitual:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_FishingRitual);
			break;
		case DoctrineType.Sustenance_TraitHarvestRitual:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_HarvestRitual);
			break;
		case DoctrineType.Possessions_AlmsToPoorRitual:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_AlmsToPoor);
			break;
		case DoctrineType.Possessions_DonationRitual:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_DonationRitual);
			break;
		case DoctrineType.DeathSacrifice_Funeral:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Funeral);
			JudgementMeter.ShowModify(1);
			break;
		case DoctrineType.DeathSacrifice_RessurectionRitual:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Ressurect);
			JudgementMeter.ShowModify(-1);
			break;
		case DoctrineType.LawOrder_AssignFaithEnforcerRitual:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_AssignFaithEnforcer);
			break;
		case DoctrineType.LawOrder_AssignTaxCollectorRitual:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_AssignTaxCollector);
			break;
		case DoctrineType.LawOrder_AscendFollower:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Ascend);
			JudgementMeter.ShowModify(1);
			break;
		case DoctrineType.DeathSacrifice_BuildingGoodGraves:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Building_Graves);
			JudgementMeter.ShowModify(1);
			break;
		case DoctrineType.DeathSacrifice_BuildingReturnToEarth:
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Building_NaturalBurial);
			JudgementMeter.ShowModify(-1);
			break;
		case DoctrineType.LawOrder_MurderFollower:
			JudgementMeter.ShowModify(-1);
			break;
		}
		Action<DoctrineType> onDoctrineUnlocked = OnDoctrineUnlocked;
		if (onDoctrineUnlocked != null)
		{
			onDoctrineUnlocked(Type);
		}
	}

	public static UpgradeSystem.Type RitualForDoctrineUpgrade(DoctrineType type)
	{
		switch (type)
		{
		case DoctrineType.Special_Sacrifice:
			return UpgradeSystem.Type.Ritual_Sacrifice;
		case DoctrineType.Special_Bonfire:
			return UpgradeSystem.Type.Ritual_FirePit;
		case DoctrineType.Special_Brainwashed:
			return UpgradeSystem.Type.Ritual_Brainwashing;
		case DoctrineType.Special_Consume:
			return UpgradeSystem.Type.Ritual_ConsumeFollower;
		case DoctrineType.LawOrder_FightPitRitual:
			return UpgradeSystem.Type.Ritual_Fightpit;
		case DoctrineType.LawOrder_JudgementRitual:
			return UpgradeSystem.Type.Ritual_Wedding;
		case DoctrineType.WorkWorship_FasterBuilding:
			return UpgradeSystem.Type.Ritual_FasterBuilding;
		case DoctrineType.Sustenance_Fast:
			return UpgradeSystem.Type.Ritual_Fast;
		case DoctrineType.Sustenance_Feast:
			return UpgradeSystem.Type.Ritual_Feast;
		case DoctrineType.WorkWorship_Enlightenment:
			return UpgradeSystem.Type.Ritual_Enlightenment;
		case DoctrineType.WorkWorship_WorkThroughNightRitual:
			return UpgradeSystem.Type.Ritual_WorkThroughNight;
		case DoctrineType.WorkWorship_HolidayRitual:
			return UpgradeSystem.Type.Ritual_Holiday;
		case DoctrineType.Sustenance_TraitFishingRitual:
			return UpgradeSystem.Type.Ritual_FishingRitual;
		case DoctrineType.Sustenance_TraitHarvestRitual:
			return UpgradeSystem.Type.Ritual_HarvestRitual;
		case DoctrineType.Possessions_AlmsToPoorRitual:
			return UpgradeSystem.Type.Ritual_AlmsToPoor;
		case DoctrineType.Possessions_DonationRitual:
			return UpgradeSystem.Type.Ritual_DonationRitual;
		case DoctrineType.DeathSacrifice_Funeral:
			return UpgradeSystem.Type.Ritual_Funeral;
		case DoctrineType.DeathSacrifice_RessurectionRitual:
			return UpgradeSystem.Type.Ritual_Ressurect;
		case DoctrineType.LawOrder_AssignFaithEnforcerRitual:
			return UpgradeSystem.Type.Ritual_AssignFaithEnforcer;
		case DoctrineType.LawOrder_AssignTaxCollectorRitual:
			return UpgradeSystem.Type.Ritual_AssignTaxCollector;
		case DoctrineType.LawOrder_AscendFollower:
			return UpgradeSystem.Type.Ritual_Ascend;
		default:
			return UpgradeSystem.Type.Count;
		}
	}

	public static string GetDoctrineUnlockString(DoctrineType Type)
	{
		switch (GetUnlockType(Type))
		{
		case DoctrineUnlockType.None:
			return "";
		case DoctrineUnlockType.Ritual:
			return ScriptLocalization.DoctrineUpgradeSystem.UnlockType_Ritual;
		case DoctrineUnlockType.FollowerAbility:
			return ScriptLocalization.DoctrineUpgradeSystem.UnlockType_FollowerAbility;
		case DoctrineUnlockType.Building:
			return ScriptLocalization.DoctrineUpgradeSystem.UnlockType_Building;
		case DoctrineUnlockType.Trait:
			return ScriptLocalization.DoctrineUpgradeSystem.UnlockType_Trait;
		default:
			return "";
		}
	}

	public static string GetDoctrineUnlockIcon(DoctrineType Type)
	{
		switch (GetUnlockType(Type))
		{
		case DoctrineUnlockType.None:
			return "";
		case DoctrineUnlockType.Ritual:
			return "\uf755";
		case DoctrineUnlockType.FollowerAbility:
			return "\uf683";
		case DoctrineUnlockType.Building:
			return "\uf6e3";
		case DoctrineUnlockType.Trait:
			return "\uf118";
		default:
			return "";
		}
	}

	private static DoctrineUnlockType GetUnlockType(DoctrineType Type)
	{
		switch (Type)
		{
		case DoctrineType.WorkWorship_FaithfulTrait:
		case DoctrineType.WorkWorship_GoodWorkerTrait:
		case DoctrineType.Possessions_TraitMaterialistic:
		case DoctrineType.Possessions_TraitFalseIdols:
		case DoctrineType.Sustenance_TraitMushroomEncouraged:
		case DoctrineType.Sustenance_TraitMushroomBanned:
		case DoctrineType.Sustenance_TraitCannibal:
		case DoctrineType.Sustenance_TraitGrassEater:
		case DoctrineType.DeathSacrifice_TraitSacrificeEnthusiast:
		case DoctrineType.DeathSacrifice_TraitDesensitisedToDeath:
		case DoctrineType.DeathSacrifice_TraitRespectElders:
		case DoctrineType.DeathSacrifice_TraitOldDieYoung:
		case DoctrineType.LawOrder_TraitDisciplinarian:
		case DoctrineType.LawOrder_TraitLibertarian:
			return DoctrineUnlockType.Trait;
		case DoctrineType.WorkWorship_FasterBuilding:
		case DoctrineType.WorkWorship_Enlightenment:
		case DoctrineType.WorkWorship_WorkThroughNightRitual:
		case DoctrineType.WorkWorship_HolidayRitual:
		case DoctrineType.Possessions_AlmsToPoorRitual:
		case DoctrineType.Possessions_DonationRitual:
		case DoctrineType.Sustenance_Fast:
		case DoctrineType.Sustenance_Feast:
		case DoctrineType.Sustenance_TraitHarvestRitual:
		case DoctrineType.Sustenance_TraitFishingRitual:
		case DoctrineType.DeathSacrifice_RessurectionRitual:
		case DoctrineType.DeathSacrifice_Funeral:
		case DoctrineType.LawOrder_FightPitRitual:
		case DoctrineType.LawOrder_JudgementRitual:
		case DoctrineType.LawOrder_AssignFaithEnforcerRitual:
		case DoctrineType.LawOrder_AssignTaxCollectorRitual:
		case DoctrineType.Special_Sacrifice:
		case DoctrineType.Special_Consume:
		case DoctrineType.Special_Bonfire:
			return DoctrineUnlockType.Ritual;
		case DoctrineType.WorkWorship_Inspire:
		case DoctrineType.WorkWorship_Intimidate:
		case DoctrineType.Special_ReadMind:
			return DoctrineUnlockType.FollowerAbility;
		case DoctrineType.DeathSacrifice_BuildingReturnToEarth:
		case DoctrineType.DeathSacrifice_BuildingGoodGraves:
			return DoctrineUnlockType.Building;
		default:
			Debug.Log(string.Concat("Uh oh ", Type, " Hasn't been set an unlock type"));
			return DoctrineUnlockType.None;
		}
	}
}
