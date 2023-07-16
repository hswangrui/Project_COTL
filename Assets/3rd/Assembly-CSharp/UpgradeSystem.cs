using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Unify;
using UnityEngine;
using UnityEngine.U2D;

public class UpgradeSystem : BaseMonoBehaviour
{
	public enum UpgradeTypes
	{
		BlackSouls,
		Devotion,
		Both
	}

	public enum Category
	{
		NONE,
		CULT,
		ECONOMY,
		FOLLOWERS,
		COMBAT,
		FAITH,
		ASTHETIC,
		DEATH,
		POOP,
		FARMING,
		SLEEP,
		ILLNESS,
		P_HEALTH,
		P_STRENGTH,
		P_WEAPON,
		P_CURSE,
		P_FERVOR
	}

	public enum Type
	{
		Combat_ExtraHeart1,
		Combat_ExtraHeart2,
		Combat_Swords2,
		Combat_Arrows,
		Combat_Arrows2,
		Combat_Shrine,
		Combat_Dash2,
		Combat_MoreBlackSouls,
		Combat_EnemiesDropXP,
		Followers_Shelter,
		Followers_Bathrooms,
		Followers_Clothing,
		Followers_FoodBasics,
		Followers_FoodAutomation,
		Followers_Compost,
		Followers_Reproduction,
		Followers_Chef,
		Followers_AdvancedFoods,
		Followers_SeedSilo,
		Cult_RitualCircle,
		Cult_ConfessionBooth,
		Cult_Punishment,
		Cult_SacrificialStone,
		Cult_ReusingCorpses,
		Cult_RaiseFromDead,
		Cult_Brainwashing,
		Cult_Propaganda,
		Cult_Punishment2,
		Economy_Foraging,
		Economy_Foraging2,
		Economy_FishingRod,
		Economy_MineII,
		Economy_Mine,
		Economy_Lumberyard,
		Economy_Refinery,
		Economy_Refinery_2,
		Economy_LumberyardII,
		Combat_RedHeartShrine,
		Combat_BlueHeartShrine,
		Combat_BlackHeartShrine,
		Combat_TarotCardShrine,
		Combat_DamageShrine,
		Building_Temple,
		Building_Farms,
		Building_AdvancedFarming,
		Building_Prison,
		Building_Outhouse,
		Building_LumberMine,
		Building_Burial,
		Building_Apothecary,
		Building_ConfessionBooth,
		Building_Beds,
		Building_FollowerFarming,
		Building_NaturalBurial,
		Ability_Eat,
		Building_BetterBeds,
		Building_BetterShrine,
		Building_Graves,
		Ability_UpgradeHeal,
		Ability_Resurrection,
		Ritual_Sacrifice,
		Ritual_Reindoctrinate,
		Ritual_ConsumeFollower,
		Ritual_Placeholder2,
		Building_Temple2,
		Ritual_HeartsOfTheFaithful1,
		Ritual_HeartsOfTheFaithful2,
		Ritual_HeartsOfTheFaithful3,
		Ritual_HeartsOfTheFaithful4,
		Ritual_HeartsOfTheFaithful5,
		Ritual_HeartsOfTheFaithful6,
		Building_Kitchen,
		Shrine_II,
		Shrine_Flame,
		Shrine_OfferingStatue,
		Shrine_III,
		Shrine_PassiveShrines,
		Shrine_PassiveShrinesII,
		Shrine_FlameIII,
		Shrine_IV,
		Shrine_PassiveShrinesIII,
		Shrine_PassiveShrinesFlames,
		Shrine_FlameII,
		Temple_III,
		Temple_IV,
		Temple_MonksUpgrade,
		Temple_SermonEfficiency,
		Temple_SermonEfficiencyII,
		Temple_SuperSermon,
		Temple_CheaperRituals,
		Temple_CheaperRitualsII,
		Temple_FasterCoolDowns,
		Temple_FasterCoolDownsII,
		Temple_DonationBoxII,
		UseForSomethingElse1,
		UseForSomethingElse2,
		UseForSomethingElse3,
		UseForSomethingElse4,
		Ritual_UnlockWeapon,
		Ritual_UnlockCurse,
		Ritual_FasterBuilding,
		Ritual_Enlightenment,
		Ritual_WorkThroughNight,
		Ritual_Holiday,
		Ritual_AlmsToPoor,
		Ritual_DonationRitual,
		Ritual_Fast,
		Ritual_Feast,
		Ritual_HarvestRitual,
		Ritual_FishingRitual,
		Ritual_Ressurect,
		Ritual_Funeral,
		Ritual_Fightpit,
		Ritual_Wedding,
		Ritual_AssignFaithEnforcer,
		Ritual_AssignTaxCollector,
		Building_PropagandaSpeakers,
		Building_Missionary,
		Building_Beds3,
		Building_FeastTable,
		Building_SiloSeed,
		Building_SiloFertiliser,
		Building_Surveillance,
		Building_FishingHut2,
		Building_Outhouse2,
		Building_Scarecrow2,
		Building_HarvestTotem2,
		Building_DancingFirepit,
		Temple_DonationBox,
		Ability_BlackHeart,
		Depreciated3,
		Depreciated4,
		Depreciated5,
		Depreciated6,
		Building_BodyPit,
		Building_HarvestTotem,
		Building_HealingBay,
		Building_HealingBay2,
		Building_FoodStorage,
		Building_Decorations1,
		Building_Decorations2,
		Building_ShrinesOfNature,
		Ability_TeleportHome,
		Building_FoodStorage2,
		Building_JanitorStation,
		Building_DemonSummoner,
		Ritual_Brainwashing,
		Ritual_Blank,
		Ritual_HeartsOfTheFaithful7,
		Ritual_HeartsOfTheFaithful8,
		Ritual_HeartsOfTheFaithful9,
		Ritual_HeartsOfTheFaithful10,
		Ritual_HeartsOfTheFaithful11,
		Ritual_HeartsOfTheFaithful12,
		Ritual_Ascend,
		Building_DemonSummoner_2,
		Building_DemonSummoner_3,
		PUpgrade_Heart_1,
		PUpgrade_Heart_2,
		PUpgrade_Heart_3,
		PUpgrade_Heart_4,
		PUpgrade_Heart_5,
		PUpgrade_Heart_6,
		PUpgrade_Sword_0,
		PUpgrade_Sword_1,
		PUpgrade_Sword_2,
		PUpgrade_Sword_3,
		PUpgrade_Axe_0,
		PUpgrade_Axe_1,
		PUpgrade_Axe_2,
		PUpgrade_Axe_3,
		PUpgrade_Dagger_0,
		PUpgrade_Dagger_1,
		PUpgrade_Dagger_2,
		PUpgrade_Dagger_3,
		PUpgrade_Gauntlets_0,
		PUpgrade_Gauntlets_1,
		PUpgrade_Gauntlets_2,
		PUpgrade_Gauntlets_3,
		PUpgrade_Hammer_0,
		PUpgrade_Hammer_1,
		PUpgrade_Hammer_2,
		PUpgrade_Hammer_3,
		PUpgrade_Fireball_0,
		PUpgrade_Fireball_1,
		PUpgrade_Fireball_2,
		PUpgrade_Fireball_3,
		PUpgrade_EnemyBlast_0,
		PUpgrade_EnemyBlast_1,
		PUpgrade_EnemyBlast_2,
		PUpgrade_EnemyBlast_3,
		PUpgrade_ProjectileAOE_0,
		PUpgrade_ProjectileAOE_1,
		PUpgrade_ProjectileAOE_2,
		PUpgrade_ProjectileAOE_3,
		PUpgrade_Tentacles_0,
		PUpgrade_Tentacles_1,
		PUpgrade_Tentacles_2,
		PUpgrade_Tentacles_3,
		PUpgrade_Vortex_0,
		PUpgrade_Vortex_1,
		PUpgrade_Vortex_2,
		PUpgrade_Vortex_3,
		PUpgrade_MegaSlash_0,
		PUpgrade_MegaSlash_1,
		PUpgrade_MegaSlash_2,
		PUpgrade_MegaSlash_3,
		PUpgrade_Ammo_1,
		PUpgrade_Ammo_2,
		PUpgrade_Ammo_3,
		Building_MissionaryII,
		Building_MissionaryIII,
		Building_KitchenII,
		Building_FarmStationII,
		PUpgrade_WeaponCritHit,
		PUpgrade_WeaponPoison,
		PUpgrade_WeaponFervor,
		PUpgrade_WeaponNecromancy,
		PUpgrade_WeaponHeal,
		PUpgrade_WeaponGodly,
		PUpgrade_CursePack1,
		PUpgrade_CursePack2,
		PUpgrade_CursePack3,
		PUpgrade_StartingWeapon_1,
		PUpgrade_StartingWeapon_2,
		PUpgrade_StartingWeapon_3,
		PUpgrade_StartingWeapon_4,
		PUpgrade_StartingWeapon_5,
		PUpgrade_StartingWeapon_6,
		Ritual_FirePit,
		PUpgrade_CursePack4,
		PUpgrade_CursePack5,
		Ritual_Halloween,
		Relics_Blessed_1,
		Relic_Pack1,
		Relics_Dammed_1,
		Relic_Pack2,
		Building_Morgue_1,
		Building_Morgue_2,
		Building_Morgue_3,
		PUpgrade_HeavyAttacks,
		PUpgrade_HA_Sword,
		PUpgrade_HA_Axe,
		PUpgrade_HA_Dagger,
		PUpgrade_HA_Hammer,
		PUpgrade_HA_Gauntlets,
		Building_Crypt_1,
		Building_Crypt_2,
		Building_Crypt_3,
		Building_Shared_House,
		Relic_Pack_Default,
		PUpgrade_ResummonWeapon,
		Ritual_CrystalDoctrine,
		Count
	}

	public delegate void UnlockEvent(Type upgradeType);

	[Serializable]
	public class UpgradeCoolDown
	{
		public Type Type;

		public float TotalElapsedGameTime;

		public float Duration;
	}

	public static UpgradeTypes UpgradeType = UpgradeTypes.Both;

	public static Action OnAbilityPointDelta;

	public static Action OnBuildingUnlocked;

	public static Action OnDisciplePointDelta;

	public static Action<Type> OnAbilityUnlocked;

	public static Action<Type> OnAbilityLocked;

	private static List<Type> UnlocksToReveal = new List<Type>();

	public static Action OnCoolDownAdded;

	public static readonly Type PrimaryRitual1 = Type.Ritual_HeartsOfTheFaithful1;

	public static readonly Type[] SecondaryRituals = new Type[3]
	{
		Type.Ritual_FirePit,
		Type.Ritual_Sacrifice,
		Type.Ritual_Brainwashing
	};

	public static readonly Type[] SecondaryRitualPairs = new Type[16]
	{
		Type.Ritual_WorkThroughNight,
		Type.Ritual_Holiday,
		Type.Ritual_FasterBuilding,
		Type.Ritual_Enlightenment,
		Type.Ritual_AlmsToPoor,
		Type.Ritual_DonationRitual,
		Type.Ritual_Fast,
		Type.Ritual_Feast,
		Type.Ritual_HarvestRitual,
		Type.Ritual_FishingRitual,
		Type.Ritual_Funeral,
		Type.Ritual_Ressurect,
		Type.Ritual_AssignFaithEnforcer,
		Type.Ritual_AssignTaxCollector,
		Type.Ritual_Fightpit,
		Type.Ritual_Wedding
	};

	public static readonly Type[] SingleRituals = new Type[1] { Type.Ritual_Ascend };

	public static readonly Type[] SpecialRituals = new Type[1] { Type.Ritual_Halloween };

	public static List<Type> UnlockedUpgrades
	{
		get
		{
			return DataManager.Instance.UnlockedUpgrades;
		}
		set
		{
			DataManager.Instance.UnlockedUpgrades = value;
		}
	}

	public static int AbilityPoints
	{
		get
		{
			return DataManager.Instance.AbilityPoints;
		}
		set
		{
			if (value > DataManager.Instance.AbilityPoints)
			{
				NotificationCentreScreen.Play(NotificationCentre.NotificationType.NewUpgradePoint);
			}
			DataManager.Instance.AbilityPoints = value;
			Action onAbilityPointDelta = OnAbilityPointDelta;
			if (onAbilityPointDelta != null)
			{
				onAbilityPointDelta();
			}
		}
	}

	public static int DisciplePoints
	{
		get
		{
			return DataManager.Instance.DiscipleAbilityPoints;
		}
		set
		{
			DataManager.Instance.DiscipleAbilityPoints = value;
			Action onDisciplePointDelta = OnDisciplePointDelta;
			if (onDisciplePointDelta != null)
			{
				onDisciplePointDelta();
			}
		}
	}

	public static float Foraging
	{
		get
		{
			List<Structures_ForagingShrine> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_ForagingShrine>(FollowerLocation.Base);
			if (allStructuresOfType.Count > 0)
			{
				foreach (Structures_ForagingShrine item in allStructuresOfType)
				{
					if (item.Data.FullyFueled)
					{
						return 0.75f;
					}
				}
			}
			return 0f;
		}
	}

	public static float Chopping
	{
		get
		{
			List<Structures_ChoppingShrine> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_ChoppingShrine>(FollowerLocation.Base);
			if (allStructuresOfType.Count > 0)
			{
				foreach (Structures_ChoppingShrine item in allStructuresOfType)
				{
					if (item.Data.FullyFueled)
					{
						return 1.5f;
					}
				}
			}
			return 0f;
		}
	}

	public static float Mining
	{
		get
		{
			List<Structures_MiningShrine> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_MiningShrine>(FollowerLocation.Base);
			if (allStructuresOfType.Count > 0)
			{
				foreach (Structures_MiningShrine item in allStructuresOfType)
				{
					if (item.Data.FullyFueled)
					{
						return 7.5f;
					}
				}
			}
			return 0f;
		}
	}

	public static int GetForageIncreaseModifier
	{
		get
		{
			if (!GetUnlocked(Type.Economy_Foraging2))
			{
				return 0;
			}
			return 1;
		}
	}

	public static float GetPriceModifier
	{
		get
		{
			return 0.8f;
		}
	}

	public static event UnlockEvent OnUpgradeUnlocked;

	public static string GetLocalizedName(Type Type)
	{
		switch (Type)
		{
		case Type.Ritual_FirePit:
			return ScriptLocalization.UpgradeSystem_Building_DancingFirepit.Name;
		case Type.Ritual_HeartsOfTheFaithful1:
			return ScriptLocalization.DoctrineUpgradeSystem.DeclareDoctrine;
		default:
			return LocalizationManager.GetTranslation(string.Format("UpgradeSystem/{0}/Name", Type));
		}
	}

	public static string GetUnlockLocalizedName(Type Type)
	{
		StructureBrain.TYPES structureTypeFromUpgrade = GetStructureTypeFromUpgrade(Type);
		return ScriptLocalization.UpgradeSystem.Unlock + " <color=yellow>" + StructuresData.GetLocalizedNameStatic(structureTypeFromUpgrade) + "</color>";
	}

	public static string GetLocalizedDescription(Type Type)
	{
		switch (Type)
		{
		case Type.PUpgrade_CursePack1:
		case Type.PUpgrade_CursePack2:
		case Type.PUpgrade_CursePack3:
		case Type.PUpgrade_CursePack4:
		case Type.PUpgrade_CursePack5:
			return string.Format(ScriptLocalization.UpgradeSystem_PUpgrade_CursePacks.Description, 3);
		case Type.Building_FollowerFarming:
		{
			string text2 = "<color=#FFD201>" + StructuresData.GetLocalizedNameStatic(StructureBrain.TYPES.FARM_STATION) + ": </color>" + StructuresData.LocalizedDescription(StructureBrain.TYPES.FARM_STATION);
			text2 += "<br><br>";
			return text2 + "<color=#FFD201>" + StructuresData.GetLocalizedNameStatic(StructureBrain.TYPES.SILO_SEED) + ": </color>" + StructuresData.LocalizedDescription(StructureBrain.TYPES.SILO_SEED);
		}
		case Type.Shrine_Flame:
		{
			string text = "<color=#FFD201>" + LocalizationManager.GetTranslation("UpgradeSystem/Shrine_Flame_Building/Name") + ": </color>" + LocalizationManager.GetTranslation("UpgradeSystem/Shrine_Flame_Building/Description");
			text += "<br><br>";
			return text + "<color=#FFD201>" + LocalizationManager.GetTranslation("UpgradeSystem/Shrine_PassiveShrinesFlames/Name") + ": </color>" + LocalizationManager.GetTranslation("UpgradeSystem/Shrine_PassiveShrinesFlames/Description");
		}
		case Type.Ritual_FirePit:
			return ScriptLocalization.FollowerInteractions_GiveQuest.UseFirePit;
		default:
			if (Type.ToString().Contains("PUpgrade_"))
			{
				return LocalizationManager.GetTranslation(string.Format("UpgradeSystem/{0}/Description", Type));
			}
			if (Type.ToString().Contains("Ritual_"))
			{
				return LocalizationManager.GetTranslation(string.Format("UpgradeSystem/{0}/Description", Type));
			}
			if (Type == Type.Temple_CheaperRituals)
			{
				Type = Type.Temple_CheaperRitualsII;
			}
			if (Type == Type.Temple_FasterCoolDowns)
			{
				Type = Type.Temple_FasterCoolDownsII;
			}
			switch (Type)
			{
			case Type.Ability_Eat:
			case Type.Ability_Resurrection:
			case Type.Shrine_Flame:
			case Type.Shrine_FlameIII:
			case Type.Shrine_PassiveShrinesFlames:
			case Type.Shrine_FlameII:
			case Type.Temple_MonksUpgrade:
			case Type.Temple_SermonEfficiency:
			case Type.Temple_SermonEfficiencyII:
			case Type.Temple_CheaperRituals:
			case Type.Temple_CheaperRitualsII:
			case Type.Temple_FasterCoolDowns:
			case Type.Temple_FasterCoolDownsII:
			case Type.Temple_DonationBoxII:
			case Type.Temple_DonationBox:
			case Type.Ability_BlackHeart:
			case Type.Building_Decorations1:
			case Type.Building_Decorations2:
			case Type.Building_ShrinesOfNature:
			case Type.Ability_TeleportHome:
			case Type.Relics_Blessed_1:
			case Type.Relic_Pack1:
			case Type.Relics_Dammed_1:
			case Type.Relic_Pack2:
			case Type.Relic_Pack_Default:
				return LocalizationManager.GetTranslation(string.Format("UpgradeSystem/{0}/Description", Type));
			default:
				Debug.Log("D " + Type);
				return StructuresData.LocalizedDescription(GetStructureTypeFromUpgrade(Type));
			}
		}
	}

	public static StructureBrain.TYPES GetStructureTypeFromUpgrade(Type Type)
	{
		switch (Type)
		{
		case Type.Building_Beds:
			return StructureBrain.TYPES.BED;
		case Type.Building_BetterBeds:
			return StructureBrain.TYPES.BED_2;
		case Type.Building_Beds3:
			return StructureBrain.TYPES.BED_3;
		case Type.Shrine_II:
			return StructureBrain.TYPES.SHRINE_II;
		case Type.Shrine_III:
			return StructureBrain.TYPES.SHRINE_III;
		case Type.Shrine_IV:
			return StructureBrain.TYPES.SHRINE_IV;
		case Type.Building_Farms:
			return StructureBrain.TYPES.FARM_PLOT;
		case Type.Followers_Compost:
			return StructureBrain.TYPES.COMPOST_BIN;
		case Type.Building_Graves:
			return StructureBrain.TYPES.GRAVE;
		case Type.Economy_Lumberyard:
			return StructureBrain.TYPES.LUMBERJACK_STATION;
		case Type.Economy_LumberyardII:
			return StructureBrain.TYPES.LUMBERJACK_STATION_2;
		case Type.Economy_Mine:
			return StructureBrain.TYPES.BLOODSTONE_MINE;
		case Type.Economy_MineII:
			return StructureBrain.TYPES.BLOODSTONE_MINE_2;
		case Type.Building_PropagandaSpeakers:
			return StructureBrain.TYPES.PROPAGANDA_SPEAKER;
		case Type.Building_Missionary:
			return StructureBrain.TYPES.MISSIONARY;
		case Type.Building_MissionaryII:
			return StructureBrain.TYPES.MISSIONARY_II;
		case Type.Building_MissionaryIII:
			return StructureBrain.TYPES.MISSIONARY_III;
		case Type.Building_DemonSummoner:
			return StructureBrain.TYPES.DEMON_SUMMONER;
		case Type.Building_DemonSummoner_2:
			return StructureBrain.TYPES.DEMON_SUMMONER_2;
		case Type.Building_DemonSummoner_3:
			return StructureBrain.TYPES.DEMON_SUMMONER_3;
		case Type.Building_FeastTable:
			return StructureBrain.TYPES.FEAST_TABLE;
		case Type.Building_SiloSeed:
			return StructureBrain.TYPES.SILO_SEED;
		case Type.Building_SiloFertiliser:
			return StructureBrain.TYPES.SILO_FERTILISER;
		case Type.Building_Surveillance:
			return StructureBrain.TYPES.SURVEILLANCE;
		case Type.Building_FishingHut2:
			return StructureBrain.TYPES.FISHING_HUT_2;
		case Type.Building_Outhouse:
			return StructureBrain.TYPES.OUTHOUSE;
		case Type.Building_Outhouse2:
			return StructureBrain.TYPES.OUTHOUSE_2;
		case Type.Building_Scarecrow2:
			return StructureBrain.TYPES.SCARECROW_2;
		case Type.Building_HarvestTotem2:
			return StructureBrain.TYPES.HARVEST_TOTEM_2;
		case Type.Building_DancingFirepit:
			return StructureBrain.TYPES.DANCING_FIREPIT;
		case Type.Building_HealingBay:
			return StructureBrain.TYPES.HEALING_BAY;
		case Type.Building_HealingBay2:
			return StructureBrain.TYPES.HEALING_BAY_2;
		case Type.Economy_Refinery:
			return StructureBrain.TYPES.REFINERY;
		case Type.Economy_Refinery_2:
			return StructureBrain.TYPES.REFINERY_2;
		case Type.Building_FollowerFarming:
			return StructureBrain.TYPES.FARM_STATION;
		case Type.Building_AdvancedFarming:
			return StructureBrain.TYPES.SCARECROW;
		case Type.Building_HarvestTotem:
			return StructureBrain.TYPES.HARVEST_TOTEM;
		case Type.Building_FoodStorage:
			return StructureBrain.TYPES.FOOD_STORAGE;
		case Type.Building_FoodStorage2:
			return StructureBrain.TYPES.FOOD_STORAGE_2;
		case Type.Building_JanitorStation:
			return StructureBrain.TYPES.JANITOR_STATION;
		case Type.Building_ConfessionBooth:
			return StructureBrain.TYPES.CONFESSION_BOOTH;
		case Type.Building_Prison:
			return StructureBrain.TYPES.PRISON;
		case Type.Building_BodyPit:
			return StructureBrain.TYPES.BODY_PIT;
		case Type.Shrine_PassiveShrines:
			return StructureBrain.TYPES.SHRINE_PASSIVE;
		case Type.Shrine_PassiveShrinesII:
			return StructureBrain.TYPES.SHRINE_PASSIVE_II;
		case Type.Shrine_PassiveShrinesIII:
			return StructureBrain.TYPES.SHRINE_PASSIVE_III;
		case Type.Shrine_OfferingStatue:
			return StructureBrain.TYPES.OFFERING_STATUE;
		case Type.Building_NaturalBurial:
			return StructureBrain.TYPES.COMPOST_BIN_DEAD_BODY;
		case Type.Building_FarmStationII:
			return StructureBrain.TYPES.FARM_STATION_II;
		case Type.Building_Temple:
			return StructureBrain.TYPES.TEMPLE;
		case Type.Building_Temple2:
			return StructureBrain.TYPES.TEMPLE_II;
		case Type.Temple_III:
			return StructureBrain.TYPES.TEMPLE_III;
		case Type.Temple_IV:
			return StructureBrain.TYPES.TEMPLE_IV;
		case Type.Building_Shared_House:
			return StructureBrain.TYPES.SHARED_HOUSE;
		case Type.Building_Crypt_1:
			return StructureBrain.TYPES.CRYPT_1;
		case Type.Building_Crypt_2:
			return StructureBrain.TYPES.CRYPT_2;
		case Type.Building_Crypt_3:
			return StructureBrain.TYPES.CRYPT_3;
		case Type.Building_Morgue_1:
			return StructureBrain.TYPES.MORGUE_1;
		case Type.Building_Morgue_2:
			return StructureBrain.TYPES.MORGUE_2;
		case Type.Building_Kitchen:
			return StructureBrain.TYPES.KITCHEN;
		default:
			return StructureBrain.TYPES.SHRINE;
		}
	}

	public static string GetLocalizedActivated(Type Type)
	{
		return LocalizationManager.GetTranslation(string.Format("UpgradeSystem/{0}/Activated", Type));
	}

	public static bool GetUnlocked(Type Type)
	{
		return UnlockedUpgrades.Contains(Type);
	}

	public static Sprite GetIcon(Type Type)
	{
		SpriteAtlas spriteAtlas = Resources.Load<SpriteAtlas>("Atlases/AbilityIcons");
		string text = Type.ToString();
		return spriteAtlas.GetSprite(text);
	}

	public static bool IsRitualActive(Type type)
	{
		switch (type)
		{
		case Type.Ritual_Holiday:
			return FollowerBrainStats.IsHoliday;
		case Type.Ritual_FishingRitual:
			return FollowerBrainStats.IsFishing;
		case Type.Ritual_FasterBuilding:
			return FollowerBrainStats.IsConstruction;
		case Type.Ritual_Enlightenment:
			return FollowerBrainStats.IsEnlightened;
		case Type.Ritual_WorkThroughNight:
			return FollowerBrainStats.IsWorkThroughTheNight;
		case Type.Ritual_Brainwashing:
			return FollowerBrainStats.BrainWashed;
		case Type.Ritual_Fast:
			return FollowerBrainStats.Fasting;
		default:
			return false;
		}
	}

	public static bool UnlockAbility(Type Type)
	{
		Debug.Log("UnlockAbilit " + Type);
		if (!UnlockedUpgrades.Contains(Type))
		{
			if (Type == Type.PUpgrade_WeaponGodly && !DungeonSandboxManager.Active)
			{
				AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("ALL_WEAPONS_UNLOCKED"));
			}
			else if (Type == Type.PUpgrade_CursePack3 && !DungeonSandboxManager.Active)
			{
				AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("ALL_CURSES_UNLOCKED"));
			}
			UnlockedUpgrades.Add(Type);
			UnlocksToReveal.Add(Type);
			Action<Type> onAbilityUnlocked = OnAbilityUnlocked;
			if (onAbilityUnlocked != null)
			{
				onAbilityUnlocked(Type);
			}
			return true;
		}
		return false;
	}

	public static void LockAbility(Type type)
	{
		UnlockedUpgrades.Remove(type);
		Action<Type> onAbilityLocked = OnAbilityLocked;
		if (onAbilityLocked != null)
		{
			onAbilityLocked(type);
		}
		ObjectiveManager.CheckObjectives(Objectives.TYPES.PERFORM_RITUAL);
	}

	public static IEnumerator ListOfUnlocksRoutine()
	{
		List<Type> list = new List<Type>(UnlocksToReveal);
		foreach (Type item in list)
		{
			if (DungeonSandboxManager.Active)
			{
				GameManager.GetInstance().StartCoroutine(OnUnlockAbility(item));
			}
			else
			{
				yield return GameManager.GetInstance().StartCoroutine(OnUnlockAbility(item));
			}
		}
		UnlocksToReveal.Clear();
	}

	public static IEnumerator OnUnlockAbility(Type Type)
	{
		if (!DungeonSandboxManager.Active)
		{
			yield return null;
		}
		Debug.Log("Type unlocked: " + Type);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.CollectDivineInspiration);
		switch (Type)
		{
		case Type.Shrine_Flame:
			UnlockedUpgrades.Add(Type.Shrine_PassiveShrinesFlames);
			break;
		case Type.Combat_ExtraHeart1:
		case Type.Combat_ExtraHeart2:
		case Type.Ritual_HeartsOfTheFaithful1:
		case Type.Ritual_HeartsOfTheFaithful2:
		case Type.Ritual_HeartsOfTheFaithful3:
		case Type.Ritual_HeartsOfTheFaithful4:
		case Type.Ritual_HeartsOfTheFaithful5:
		case Type.Ritual_HeartsOfTheFaithful6:
		case Type.Ritual_HeartsOfTheFaithful7:
		case Type.Ritual_HeartsOfTheFaithful8:
		case Type.Ritual_HeartsOfTheFaithful9:
		case Type.Ritual_HeartsOfTheFaithful10:
		case Type.Ritual_HeartsOfTheFaithful11:
		case Type.Ritual_HeartsOfTheFaithful12:
			Debug.Log("UNLOCK : " + Type);
			break;
		case Type.Building_Decorations1:
			DataManager.Instance.UnlockedStructures.Add(StructureBrain.TYPES.DECORATION_WALL_TWIGS);
			DataManager.Instance.UnlockedStructures.Add(StructureBrain.TYPES.DECORATION_WALL_GRASS);
			DataManager.Instance.UnlockedStructures.Add(StructureBrain.TYPES.DECORATION_TREE);
			DataManager.Instance.UnlockedStructures.Add(StructureBrain.TYPES.DECORATION_SMALL_STONE_CANDLE);
			DataManager.Instance.UnlockedStructures.Add(StructureBrain.TYPES.DECORATION_CANDLE_BARREL);
			DataManager.Instance.UnlockedStructures.Add(StructureBrain.TYPES.DECORATION_STONE);
			DataManager.Instance.UnlockedStructures.Add(StructureBrain.TYPES.DECORATION_FLAG_CROWN);
			break;
		case Type.Building_Decorations2:
			DataManager.Instance.UnlockedStructures.Add(StructureBrain.TYPES.DECORATION_BONE_ARCH);
			DataManager.Instance.UnlockedStructures.Add(StructureBrain.TYPES.DECORATION_BONE_BARREL);
			DataManager.Instance.UnlockedStructures.Add(StructureBrain.TYPES.DECORATION_BONE_CANDLE);
			DataManager.Instance.UnlockedStructures.Add(StructureBrain.TYPES.DECORATION_BONE_FLAG);
			DataManager.Instance.UnlockedStructures.Add(StructureBrain.TYPES.DECORATION_BONE_LANTERN);
			DataManager.Instance.UnlockedStructures.Add(StructureBrain.TYPES.DECORATION_BONE_PILLAR);
			DataManager.Instance.UnlockedStructures.Add(StructureBrain.TYPES.DECORATION_BONE_SCULPTURE);
			DataManager.Instance.UnlockedStructures.Add(StructureBrain.TYPES.DECORATION_WALL_BONE);
			break;
		case Type.Building_Temple:
		{
			DataManager.Instance.NewBuildings = true;
			Action onBuildingUnlocked2 = OnBuildingUnlocked;
			if (onBuildingUnlocked2 != null)
			{
				onBuildingUnlocked2();
			}
			Debug.Log("AA!");
			break;
		}
		case Type.Building_Beds:
		{
			DataManager.Instance.OnboardedHomelessAtNight = true;
			DataManager.Instance.NewBuildings = true;
			Action onBuildingUnlocked3 = OnBuildingUnlocked;
			if (onBuildingUnlocked3 != null)
			{
				onBuildingUnlocked3();
			}
			break;
		}
		case Type.Economy_Refinery:
		case Type.Building_Farms:
		case Type.Building_AdvancedFarming:
		case Type.Building_Prison:
		case Type.Building_Outhouse:
		case Type.Building_LumberMine:
		case Type.Building_Burial:
		case Type.Building_Apothecary:
		case Type.Building_ConfessionBooth:
		case Type.Building_FollowerFarming:
		case Type.Building_NaturalBurial:
		case Type.Building_BetterBeds:
		case Type.Building_BetterShrine:
		case Type.Building_Graves:
		case Type.Shrine_OfferingStatue:
		case Type.Shrine_PassiveShrines:
		case Type.Shrine_PassiveShrinesII:
		case Type.Shrine_PassiveShrinesIII:
		case Type.Building_FeastTable:
		case Type.Building_Scarecrow2:
		{
			DataManager.Instance.NewBuildings = true;
			Action onBuildingUnlocked = OnBuildingUnlocked;
			if (onBuildingUnlocked != null)
			{
				onBuildingUnlocked();
			}
			break;
		}
		case Type.PUpgrade_Heart_1:
		case Type.PUpgrade_Heart_2:
		case Type.PUpgrade_Heart_3:
		case Type.PUpgrade_Heart_4:
		case Type.PUpgrade_Heart_5:
		case Type.PUpgrade_Heart_6:
		{
			HealthPlayer healthPlayer = UnityEngine.Object.FindObjectOfType<HealthPlayer>();
			healthPlayer.totalHP += 1f;
			healthPlayer.HP = healthPlayer.totalHP;
			DataManager.Instance.PLAYER_HEARTS_LEVEL++;
			break;
		}
		case Type.PUpgrade_Gauntlets_0:
			DataManager.Instance.AddWeapon(EquipmentType.Gauntlet);
			DataManager.Instance.ForcedStartingWeapon = EquipmentType.Gauntlet;
			if (GetUnlocked(Type.PUpgrade_WeaponFervor))
			{
				DataManager.Instance.AddWeapon(EquipmentType.Gauntlet_Fervour);
			}
			if (GetUnlocked(Type.PUpgrade_WeaponNecromancy))
			{
				DataManager.Instance.AddWeapon(EquipmentType.Gauntlet_Nercomancy);
			}
			if (GetUnlocked(Type.PUpgrade_WeaponHeal))
			{
				DataManager.Instance.AddWeapon(EquipmentType.Gauntlet_Healing);
			}
			break;
		case Type.PUpgrade_Hammer_0:
			DataManager.Instance.AddWeapon(EquipmentType.Hammer);
			DataManager.Instance.ForcedStartingWeapon = EquipmentType.Hammer;
			if (GetUnlocked(Type.PUpgrade_WeaponFervor))
			{
				DataManager.Instance.AddWeapon(EquipmentType.Hammer_Fervour);
			}
			if (GetUnlocked(Type.PUpgrade_WeaponNecromancy))
			{
				DataManager.Instance.AddWeapon(EquipmentType.Hammer_Nercomancy);
			}
			if (GetUnlocked(Type.PUpgrade_WeaponHeal))
			{
				DataManager.Instance.AddWeapon(EquipmentType.Hammer_Healing);
			}
			break;
		case Type.PUpgrade_Tentacles_0:
			DataManager.Instance.AddCurse(EquipmentType.Tentacles);
			DataManager.Instance.ForcedStartingCurse = EquipmentType.Tentacles;
			if (GetUnlocked(Type.PUpgrade_CursePack3))
			{
				DataManager.Instance.AddCurse(EquipmentType.Tentacles_Circular);
			}
			break;
		case Type.PUpgrade_MegaSlash_0:
			DataManager.Instance.AddCurse(EquipmentType.MegaSlash);
			DataManager.Instance.ForcedStartingCurse = EquipmentType.MegaSlash;
			if (GetUnlocked(Type.PUpgrade_CursePack2))
			{
				DataManager.Instance.AddCurse(EquipmentType.MegaSlash_Necromancy);
			}
			break;
		case Type.PUpgrade_WeaponCritHit:
			DataManager.Instance.AddWeapon(EquipmentType.Axe_Critical);
			DataManager.Instance.AddWeapon(EquipmentType.Sword_Critical);
			DataManager.Instance.AddWeapon(EquipmentType.Dagger_Critical);
			DataManager.Instance.AddWeapon(EquipmentType.Gauntlet_Critical);
			DataManager.Instance.AddWeapon(EquipmentType.Hammer_Critical);
			DataManager.Instance.ForcedStartingWeapon = ((!(UnityEngine.Random.value > 0.5f)) ? EquipmentType.Axe_Critical : ((UnityEngine.Random.value > 0.5f) ? EquipmentType.Sword_Critical : EquipmentType.Dagger_Critical));
			break;
		case Type.PUpgrade_WeaponFervor:
			DataManager.Instance.AddWeapon(EquipmentType.Axe_Fervour);
			DataManager.Instance.AddWeapon(EquipmentType.Sword_Fervour);
			DataManager.Instance.AddWeapon(EquipmentType.Dagger_Fervour);
			DataManager.Instance.AddWeapon(EquipmentType.Hammer_Fervour);
			DataManager.Instance.AddWeapon(EquipmentType.Gauntlet_Fervour);
			DataManager.Instance.ForcedStartingWeapon = ((!(UnityEngine.Random.value > 0.5f)) ? EquipmentType.Axe_Fervour : ((UnityEngine.Random.value > 0.5f) ? EquipmentType.Sword_Fervour : EquipmentType.Dagger_Fervour));
			break;
		case Type.PUpgrade_WeaponGodly:
			DataManager.Instance.AddWeapon(EquipmentType.Axe_Godly);
			DataManager.Instance.AddWeapon(EquipmentType.Sword_Godly);
			DataManager.Instance.AddWeapon(EquipmentType.Dagger_Godly);
			DataManager.Instance.AddWeapon(EquipmentType.Hammer_Godly);
			DataManager.Instance.AddWeapon(EquipmentType.Gauntlet_Godly);
			DataManager.Instance.ForcedStartingWeapon = ((!(UnityEngine.Random.value > 0.5f)) ? EquipmentType.Axe_Godly : ((UnityEngine.Random.value > 0.5f) ? EquipmentType.Sword_Godly : EquipmentType.Dagger_Godly));
			break;
		case Type.PUpgrade_WeaponHeal:
			DataManager.Instance.AddWeapon(EquipmentType.Axe_Healing);
			DataManager.Instance.AddWeapon(EquipmentType.Sword_Healing);
			DataManager.Instance.AddWeapon(EquipmentType.Dagger_Healing);
			DataManager.Instance.AddWeapon(EquipmentType.Hammer_Healing);
			DataManager.Instance.AddWeapon(EquipmentType.Gauntlet_Healing);
			DataManager.Instance.ForcedStartingWeapon = ((!(UnityEngine.Random.value > 0.5f)) ? EquipmentType.Axe_Healing : ((UnityEngine.Random.value > 0.5f) ? EquipmentType.Sword_Healing : EquipmentType.Dagger_Healing));
			break;
		case Type.PUpgrade_WeaponNecromancy:
			DataManager.Instance.AddWeapon(EquipmentType.Axe_Nercomancy);
			DataManager.Instance.AddWeapon(EquipmentType.Sword_Nercomancy);
			DataManager.Instance.AddWeapon(EquipmentType.Dagger_Nercomancy);
			DataManager.Instance.AddWeapon(EquipmentType.Hammer_Nercomancy);
			DataManager.Instance.AddWeapon(EquipmentType.Gauntlet_Nercomancy);
			DataManager.Instance.ForcedStartingWeapon = ((!(UnityEngine.Random.value > 0.5f)) ? EquipmentType.Axe_Nercomancy : ((UnityEngine.Random.value > 0.5f) ? EquipmentType.Sword_Nercomancy : EquipmentType.Dagger_Nercomancy));
			break;
		case Type.PUpgrade_WeaponPoison:
			DataManager.Instance.AddWeapon(EquipmentType.Axe_Poison);
			DataManager.Instance.AddWeapon(EquipmentType.Sword_Poison);
			DataManager.Instance.AddWeapon(EquipmentType.Dagger_Poison);
			DataManager.Instance.AddWeapon(EquipmentType.Hammer_Poison);
			DataManager.Instance.AddWeapon(EquipmentType.Gauntlet_Poison);
			DataManager.Instance.ForcedStartingWeapon = ((!(UnityEngine.Random.value > 0.5f)) ? EquipmentType.Axe_Poison : ((UnityEngine.Random.value > 0.5f) ? EquipmentType.Sword_Poison : EquipmentType.Dagger_Poison));
			break;
		case Type.PUpgrade_CursePack1:
			DataManager.Instance.AddCurse(EquipmentType.EnemyBlast_Ice);
			DataManager.Instance.AddCurse(EquipmentType.MegaSlash_Ice);
			DataManager.Instance.AddCurse(EquipmentType.Tentacles_Ice);
			DataManager.Instance.ForcedStartingCurse = ((!(UnityEngine.Random.value > 0.5f)) ? EquipmentType.EnemyBlast_Ice : ((UnityEngine.Random.value > 0.5f) ? EquipmentType.Tentacles_Ice : EquipmentType.MegaSlash_Ice));
			break;
		case Type.PUpgrade_CursePack2:
			DataManager.Instance.AddCurse(EquipmentType.Tentacles_Circular);
			DataManager.Instance.AddCurse(EquipmentType.Fireball_Swarm);
			DataManager.Instance.AddCurse(EquipmentType.ProjectileAOE_ExplosiveImpact);
			DataManager.Instance.ForcedStartingCurse = ((!(UnityEngine.Random.value > 0.5f)) ? EquipmentType.ProjectileAOE_ExplosiveImpact : ((UnityEngine.Random.value > 0.5f) ? EquipmentType.Tentacles_Circular : EquipmentType.Fireball_Swarm));
			break;
		case Type.PUpgrade_CursePack3:
			DataManager.Instance.AddCurse(EquipmentType.Fireball_Charm);
			DataManager.Instance.AddCurse(EquipmentType.MegaSlash_Charm);
			DataManager.Instance.AddCurse(EquipmentType.ProjectileAOE_Charm);
			DataManager.Instance.ForcedStartingCurse = ((!(UnityEngine.Random.value > 0.5f)) ? EquipmentType.ProjectileAOE_Charm : ((UnityEngine.Random.value > 0.5f) ? EquipmentType.Fireball_Charm : EquipmentType.MegaSlash_Charm));
			break;
		case Type.PUpgrade_CursePack4:
			DataManager.Instance.AddCurse(EquipmentType.MegaSlash_Necromancy);
			DataManager.Instance.AddCurse(EquipmentType.Tentacles_Necromancy);
			DataManager.Instance.AddCurse(EquipmentType.EnemyBlast_Poison);
			DataManager.Instance.ForcedStartingCurse = ((!(UnityEngine.Random.value > 0.5f)) ? EquipmentType.EnemyBlast_Poison : ((UnityEngine.Random.value > 0.5f) ? EquipmentType.MegaSlash_Necromancy : EquipmentType.Tentacles_Necromancy));
			break;
		case Type.PUpgrade_CursePack5:
			DataManager.Instance.AddCurse(EquipmentType.ProjectileAOE_GoopTrail);
			DataManager.Instance.AddCurse(EquipmentType.EnemyBlast_DeflectsProjectiles);
			DataManager.Instance.AddCurse(EquipmentType.Fireball_Triple);
			DataManager.Instance.ForcedStartingCurse = ((!(UnityEngine.Random.value > 0.5f)) ? EquipmentType.Fireball_Triple : ((UnityEngine.Random.value > 0.5f) ? EquipmentType.ProjectileAOE_GoopTrail : EquipmentType.EnemyBlast_DeflectsProjectiles));
			break;
		case Type.Relics_Blessed_1:
			if (!DataManager.Instance.ForceDammedRelic)
			{
				DataManager.Instance.ForceBlessedRelic = true;
			}
			EquipmentManager.UnlockRelics(Type);
			break;
		case Type.Relics_Dammed_1:
			if (!DataManager.Instance.ForceBlessedRelic)
			{
				DataManager.Instance.ForceDammedRelic = true;
			}
			EquipmentManager.UnlockRelics(Type);
			break;
		case Type.Relic_Pack1:
		case Type.Relic_Pack2:
		case Type.Relic_Pack_Default:
			EquipmentManager.UnlockRelics(Type);
			break;
		}
		UnlockEvent onUpgradeUnlocked = UpgradeSystem.OnUpgradeUnlocked;
		if (onUpgradeUnlocked != null)
		{
			onUpgradeUnlocked(Type);
		}
	}

	private static void UnlockUpgradeWeapon(TarotCards.Card Weapon)
	{
		if (false)
		{
			UpgradeWeapon(Weapon);
		}
		else
		{
			UnlockWeapon(Weapon);
		}
	}

	private static void UpgradeWeapon(TarotCards.Card Weapon)
	{
	}

	private static void UnlockWeapon(TarotCards.Card Weapon)
	{
	}

	private static void UnlockUpgradeCurse(TarotCards.Card Curse)
	{
	}

	private static void UpgradeCurse(TarotCards.Card Curse)
	{
	}

	private static void UnlockCurse(TarotCards.Card Curse)
	{
	}

	public static float GetRitualFaithChange(Type Type)
	{
		switch (Type)
		{
		case Type.Ritual_Sacrifice:
			if (DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.SacrificeEnthusiast))
			{
				return FollowerThoughts.GetData(Thought.Cult_Sacrifice_Trait).Modifier;
			}
			return FollowerThoughts.GetData(Thought.Cult_Sacrifice).Modifier;
		case Type.Ritual_ConsumeFollower:
			return FollowerThoughts.GetData(Thought.Cult_ConsumeFollower).Modifier;
		case Type.Ritual_Ascend:
			return FollowerThoughts.GetData(Thought.Cult_Ascend).Modifier;
		case Type.Ritual_Brainwashing:
			if (DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.MushroomEncouraged))
			{
				return FollowerThoughts.GetData(Thought.Cult_MushroomEncouraged_Trait).Modifier;
			}
			return FollowerThoughts.GetData(Thought.Brainwashed).Modifier;
		case Type.Ritual_Enlightenment:
			return FollowerThoughts.GetData(Thought.Cult_Enlightenment).Modifier;
		case Type.Ritual_Fast:
			return FollowerThoughts.GetData(Thought.Cult_Fast).Modifier;
		case Type.Ritual_Feast:
			return FollowerThoughts.GetData(Thought.Cult_Feast).Modifier;
		case Type.Ritual_FirePit:
			return FollowerThoughts.GetData(Thought.DancePit).Modifier;
		case Type.Ritual_Fightpit:
			return FollowerThoughts.GetData(Thought.Cult_FightPit).Modifier;
		case Type.Ritual_Funeral:
			return FollowerThoughts.GetData(Thought.Cult_Funeral).Modifier;
		case Type.Ritual_Holiday:
			return FollowerThoughts.GetData(Thought.Cult_Holiday).Modifier;
		case Type.Ritual_Ressurect:
			return FollowerThoughts.GetData(Thought.Cult_Ressurection).Modifier;
		case Type.Ritual_Wedding:
			return FollowerThoughts.GetData(Thought.Cult_Wedding).Modifier;
		case Type.Ritual_DonationRitual:
			return FollowerThoughts.GetData(Thought.Cult_DonationRitual).Modifier;
		case Type.Ritual_FasterBuilding:
			return FollowerThoughts.GetData(Thought.Cult_FasterBuilding).Modifier;
		case Type.Ritual_FishingRitual:
			return FollowerThoughts.GetData(Thought.Cult_FishingRitual).Modifier;
		case Type.Ritual_HarvestRitual:
			return FollowerThoughts.GetData(Thought.Cult_HarvestRitual).Modifier;
		case Type.Ritual_AlmsToPoor:
			return FollowerThoughts.GetData(Thought.Cult_AlmsToPoor).Modifier;
		case Type.Ritual_WorkThroughNight:
			return FollowerThoughts.GetData(Thought.Cult_WorkThroughNight).Modifier;
		case Type.Ritual_AssignFaithEnforcer:
			return FollowerThoughts.GetData(Thought.Cult_FaithEnforcer).Modifier;
		case Type.Ritual_AssignTaxCollector:
			return FollowerThoughts.GetData(Thought.Cult_TaxEnforcer).Modifier;
		default:
			return 0f;
		}
	}

	public static FollowerTrait.TraitType GetRitualTrait(Type Type)
	{
		switch (Type)
		{
		case Type.Ritual_Sacrifice:
			return FollowerTrait.TraitType.SacrificeEnthusiast;
		case Type.Ritual_Brainwashing:
			return FollowerTrait.TraitType.MushroomEncouraged;
		default:
			return FollowerTrait.TraitType.None;
		}
	}

	public static List<StructuresData.ItemCost> GetCost(Type Type)
	{
		switch (Type)
		{
		case Type.Ritual_CrystalDoctrine:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE, 1)
			};
		case Type.Ritual_HeartsOfTheFaithful1:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.DOCTRINE_STONE, 1)
			};
		case Type.Ritual_HeartsOfTheFaithful2:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.DISCIPLE_POINTS, 1)
			};
		case Type.Ritual_HeartsOfTheFaithful3:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.DISCIPLE_POINTS, 1)
			};
		case Type.Ritual_HeartsOfTheFaithful4:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.DISCIPLE_POINTS, 1)
			};
		case Type.Ritual_HeartsOfTheFaithful5:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.DISCIPLE_POINTS, 1)
			};
		case Type.Ritual_HeartsOfTheFaithful6:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.DISCIPLE_POINTS, 1)
			};
		case Type.Ritual_HeartsOfTheFaithful7:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.FOLLOWERS, 19),
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 40)
			};
		case Type.Ritual_HeartsOfTheFaithful8:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.FOLLOWERS, 21),
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 45)
			};
		case Type.Ritual_HeartsOfTheFaithful9:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.FOLLOWERS, 22),
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 50)
			};
		case Type.Ritual_HeartsOfTheFaithful10:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.FOLLOWERS, 23),
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 55)
			};
		case Type.Ritual_HeartsOfTheFaithful11:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.FOLLOWERS, 24),
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 60)
			};
		case Type.Ritual_HeartsOfTheFaithful12:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.FOLLOWERS, 25),
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 65)
			};
		case Type.Ritual_UnlockCurse:
			return new List<StructuresData.ItemCost>();
		case Type.Ritual_UnlockWeapon:
			return new List<StructuresData.ItemCost>();
		case Type.Ritual_FirePit:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.LOG, 10),
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 25)
			});
		case Type.Ritual_Sacrifice:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 75),
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.FOLLOWERS, 1)
			});
		case Type.Ritual_Ascend:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 75),
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.FOLLOWERS, 1)
			});
		case Type.Ritual_ConsumeFollower:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 75)
			});
		case Type.Ritual_FasterBuilding:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 75)
			});
		case Type.Ritual_Enlightenment:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 75)
			});
		case Type.Ritual_Fast:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 75)
			});
		case Type.Ritual_Feast:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 75)
			});
		case Type.Ritual_WorkThroughNight:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 75)
			});
		case Type.Ritual_Holiday:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 75)
			});
		case Type.Ritual_DonationRitual:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 125)
			});
		case Type.Ritual_AlmsToPoor:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 50)
			});
		case Type.Ritual_FishingRitual:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 75)
			});
		case Type.Ritual_HarvestRitual:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 75)
			});
		case Type.Ritual_Fightpit:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.FOLLOWERS, 2),
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 75)
			});
		case Type.Ritual_Wedding:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 75)
			});
		case Type.Ritual_Funeral:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 75),
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.FLOWER_RED, 5)
			});
		case Type.Ritual_Ressurect:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 150)
			});
		case Type.Ritual_Brainwashing:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, 25)
			});
		case Type.Ritual_AssignFaithEnforcer:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 75)
			});
		case Type.Ritual_AssignTaxCollector:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BONE, 75)
			});
		case Type.Ritual_Halloween:
			return ApplyRitualDiscount(new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.PUMPKIN, 40)
			});
		case Type.Economy_FishingRod:
			return new List<StructuresData.ItemCost>();
		case Type.Ability_UpgradeHeal:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.MONSTER_HEART, 1)
			};
		case Type.Ability_Eat:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.MONSTER_HEART, 1)
			};
		case Type.Ability_TeleportHome:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.MONSTER_HEART, 1)
			};
		case Type.Ability_Resurrection:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.MONSTER_HEART, 1)
			};
		case Type.Ability_BlackHeart:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.MONSTER_HEART, 1)
			};
		case Type.Temple_DonationBox:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 10)
			};
		case Type.Depreciated3:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 5)
			};
		case Type.Depreciated4:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 20),
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 5)
			};
		case Type.Depreciated5:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 10),
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 5)
			};
		case Type.Depreciated6:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 15)
			};
		default:
			return new List<StructuresData.ItemCost>();
		}
	}

	public static bool UserCanAffordUpgrade(Type type)
	{
		if (CheatConsole.BuildingsFree)
		{
			return true;
		}
		List<StructuresData.ItemCost> cost = GetCost(type);
		for (int i = 0; i < cost.Count; i++)
		{
			if (Inventory.GetItemQuantity((int)cost[i].CostItem) < cost[i].CostValue)
			{
				return false;
			}
		}
		return true;
	}

	public static bool CanAffordRatauAbility()
	{
		if (!UserCanAffordUpgrade(Type.Ability_Eat))
		{
			return false;
		}
		if (!UserCanAffordUpgrade(Type.Ability_BlackHeart))
		{
			return false;
		}
		if (!UserCanAffordUpgrade(Type.Ability_Resurrection))
		{
			return false;
		}
		if (!UserCanAffordUpgrade(Type.Ability_TeleportHome))
		{
			return false;
		}
		return true;
	}

	public static bool CanAffordDoctrine()
	{
		if (GetCoolDownNormalised(Type.Ritual_HeartsOfTheFaithful1) > 0f)
		{
			return false;
		}
		if (GetCoolDownNormalised(Type.Ritual_CrystalDoctrine) <= 0f && DoctrineUpgradeSystem.GetAllRemainingDoctrines().Count > 0 && Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE) > 0)
		{
			return true;
		}
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.DOCTRINE_STONE) < 1)
		{
			return false;
		}
		if (!DoctrineUpgradeSystem.TrySermonsStillAvailable())
		{
			return false;
		}
		if (!DataManager.Instance.PostGameFleecesOnboarded)
		{
			bool deathCatBeaten = DataManager.Instance.DeathCatBeaten;
			return true;
		}
		return true;
	}

	private static List<StructuresData.ItemCost> ApplyRitualDiscount(List<StructuresData.ItemCost> result)
	{
		if (GetUnlocked(Type.Temple_CheaperRituals))
		{
			foreach (StructuresData.ItemCost item in result)
			{
				if (item.CostItem != InventoryItem.ITEM_TYPE.FOLLOWERS)
				{
					item.CostValue = Mathf.Max(1, Mathf.FloorToInt((float)item.CostValue * 0.5f));
				}
			}
		}
		return result;
	}

	public static bool PlayerHasRequiredBuildings(Type Type)
	{
		List<StructureBrain.TYPES> requiredBuilding = GetRequiredBuilding(Type);
		if (requiredBuilding == null)
		{
			return true;
		}
		foreach (StructureBrain.TYPES item in requiredBuilding)
		{
			if (!DataManager.Instance.HistoryOfStructures.Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	public static List<StructureBrain.TYPES> GetRequiredBuilding(Type Type)
	{
		switch (Type)
		{
		case Type.Shrine_II:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.SHRINE,
				StructureBrain.TYPES.REFINERY
			};
		case Type.Shrine_III:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.SHRINE_II,
				StructureBrain.TYPES.REFINERY
			};
		case Type.Shrine_IV:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.SHRINE_III,
				StructureBrain.TYPES.REFINERY
			};
		case Type.Building_Temple2:
			return new List<StructureBrain.TYPES> { StructureBrain.TYPES.TEMPLE };
		case Type.Temple_III:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.TEMPLE_II,
				StructureBrain.TYPES.REFINERY
			};
		case Type.Temple_IV:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.TEMPLE_III,
				StructureBrain.TYPES.REFINERY
			};
		case Type.Building_Kitchen:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.COOKING_FIRE,
				StructureBrain.TYPES.REFINERY
			};
		case Type.Building_Outhouse2:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.OUTHOUSE,
				StructureBrain.TYPES.REFINERY
			};
		case Type.Building_HarvestTotem2:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.HARVEST_TOTEM,
				StructureBrain.TYPES.REFINERY
			};
		case Type.Building_Scarecrow2:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.SCARECROW,
				StructureBrain.TYPES.REFINERY
			};
		case Type.Economy_LumberyardII:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.LUMBERJACK_STATION,
				StructureBrain.TYPES.REFINERY
			};
		case Type.Economy_MineII:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.BLOODSTONE_MINE,
				StructureBrain.TYPES.REFINERY
			};
		case Type.Building_HealingBay2:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.HEALING_BAY,
				StructureBrain.TYPES.REFINERY
			};
		case Type.Shrine_PassiveShrines:
			return new List<StructureBrain.TYPES> { StructureBrain.TYPES.REFINERY };
		case Type.Shrine_PassiveShrinesII:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.REFINERY,
				StructureBrain.TYPES.SHRINE_PASSIVE
			};
		case Type.Shrine_PassiveShrinesIII:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.REFINERY,
				StructureBrain.TYPES.SHRINE_PASSIVE_II
			};
		case Type.Building_BetterBeds:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.BED,
				StructureBrain.TYPES.REFINERY
			};
		case Type.Building_Beds3:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.BED_2,
				StructureBrain.TYPES.REFINERY
			};
		case Type.Building_FoodStorage2:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.BED_2,
				StructureBrain.TYPES.REFINERY
			};
		case Type.Followers_Compost:
		case Type.Economy_Refinery_2:
		case Type.Building_ConfessionBooth:
		case Type.Building_Graves:
		case Type.Shrine_OfferingStatue:
		case Type.Building_SiloSeed:
		case Type.Building_SiloFertiliser:
		case Type.Building_DancingFirepit:
		case Type.Building_HarvestTotem:
		case Type.Building_FoodStorage:
			return new List<StructureBrain.TYPES> { StructureBrain.TYPES.REFINERY };
		default:
			return null;
		}
	}

	public static void AddCooldown(Type type, float duration)
	{
		if (GetUnlocked(Type.Temple_FasterCoolDowns))
		{
			duration *= 0.5f;
		}
		DataManager.Instance.UpgradeCoolDowns.Add(new UpgradeCoolDown
		{
			TotalElapsedGameTime = TimeManager.TotalElapsedGameTime,
			Type = type,
			Duration = duration
		});
		Action onCoolDownAdded = OnCoolDownAdded;
		if (onCoolDownAdded != null)
		{
			onCoolDownAdded();
		}
	}

	public static void ClearAllCoolDowns()
	{
		for (int num = DataManager.Instance.UpgradeCoolDowns.Count - 1; num >= 0; num--)
		{
			if (!IsRitualActive(DataManager.Instance.UpgradeCoolDowns[num].Type))
			{
				DataManager.Instance.UpgradeCoolDowns.RemoveAt(num);
			}
		}
	}

	public static float GetCoolDownNormalised(Type Type)
	{
		foreach (UpgradeCoolDown upgradeCoolDown in DataManager.Instance.UpgradeCoolDowns)
		{
			if (upgradeCoolDown.Type == Type)
			{
				if (TimeManager.TotalElapsedGameTime >= upgradeCoolDown.TotalElapsedGameTime + upgradeCoolDown.Duration)
				{
					DataManager.Instance.UpgradeCoolDowns.Remove(upgradeCoolDown);
					return 0f;
				}
				return 1f - (TimeManager.TotalElapsedGameTime - upgradeCoolDown.TotalElapsedGameTime) / upgradeCoolDown.Duration;
			}
		}
		return 0f;
	}

	public static Type[] AllRituals()
	{
		List<Type> list = new List<Type>();
		list.AddRange(SecondaryRituals);
		list.AddRange(SecondaryRitualPairs);
		list.AddRange(SingleRituals);
		list.AddRange(SpecialRituals);
		return list.ToArray();
	}

	public static bool IsSpecialRitual(Type type)
	{
		return SpecialRituals.Contains(type);
	}

	public static bool IsUpgradeMaxed(Type upgrade)
	{
		switch (upgrade)
		{
		case Type.Ritual_UnlockCurse:
			return true;
		case Type.Ritual_UnlockWeapon:
			return true;
		case Type.Ritual_HeartsOfTheFaithful1:
		case Type.Ritual_HeartsOfTheFaithful2:
		case Type.Ritual_HeartsOfTheFaithful3:
		case Type.Ritual_HeartsOfTheFaithful4:
		case Type.Ritual_HeartsOfTheFaithful5:
		case Type.Ritual_HeartsOfTheFaithful6:
		case Type.Ritual_HeartsOfTheFaithful7:
		case Type.Ritual_HeartsOfTheFaithful8:
		case Type.Ritual_HeartsOfTheFaithful9:
		case Type.Ritual_HeartsOfTheFaithful10:
		case Type.Ritual_HeartsOfTheFaithful11:
		case Type.Ritual_HeartsOfTheFaithful12:
			return DataManager.Instance.PLAYER_HEARTS_LEVEL + DataManager.Instance.PLAYER_DAMAGE_LEVEL >= 12;
		default:
			return false;
		}
	}
}
