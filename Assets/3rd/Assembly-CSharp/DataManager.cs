using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.DeathScreen;
using Map;
using Newtonsoft.Json;
using src.Data;
using Unify;
using UnityEngine;

[XmlInclude(typeof(ObjectivesData))]
[XmlInclude(typeof(Objectives_Custom))]
[XmlInclude(typeof(Objectives_CollectItem))]
[XmlInclude(typeof(Objectives_BuildStructure))]
[XmlInclude(typeof(Objectives_RecruitFollower))]
[XmlInclude(typeof(Objectives_DepositFood))]
[XmlInclude(typeof(Objectives_KillEnemies))]
[XmlInclude(typeof(Objectives_NoDodge))]
[XmlInclude(typeof(Objectives_NoCurses))]
[XmlInclude(typeof(Objectives_NoDamage))]
[XmlInclude(typeof(Objectives_NoHealing))]
[XmlInclude(typeof(Objectives_BedRest))]
[XmlInclude(typeof(Objectives_RemoveStructure))]
[XmlInclude(typeof(Objectives_ShootDummy))]
[XmlInclude(typeof(Objectives_UseRelic))]
[XmlInclude(typeof(Objectives_TalkToFollower))]
[XmlInclude(typeof(Objectives_CookMeal))]
[XmlInclude(typeof(Objectives_PlaceStructure))]
[XmlInclude(typeof(Objectives_PerformRitual))]
[XmlInclude(typeof(Objectives_UnlockUpgrade))]
[XmlInclude(typeof(Objectives_EatMeal))]
[XmlInclude(typeof(Objectives_RecruitCursedFollower))]
[XmlInclude(typeof(Objectives_FindFollower))]
[XmlInclude(typeof(ObjectivesDataFinalized))]
[XmlInclude(typeof(Objectives_Custom.FinalizedData_Custom))]
[XmlInclude(typeof(Objectives_CollectItem.FinalizedData_CollectItem))]
[XmlInclude(typeof(Objectives_BuildStructure.FinalizedData_BuildStructure))]
[XmlInclude(typeof(Objectives_RecruitFollower.FinalizedData_RecruitFollower))]
[XmlInclude(typeof(Objectives_DepositFood.FinalizedData_DepositFood))]
[XmlInclude(typeof(Objectives_KillEnemies.FinalizedData_KillEnemies))]
[XmlInclude(typeof(Objectives_NoDodge.FinalizedData_NoDodge))]
[XmlInclude(typeof(Objectives_NoCurses.FinalizedData_NoCurses))]
[XmlInclude(typeof(Objectives_NoDamage.FinalizedData_NoDamage))]
[XmlInclude(typeof(Objectives_NoHealing.FinalizedData_NoHealing))]
[XmlInclude(typeof(Objectives_BedRest.FinalizedData_BedRest))]
[XmlInclude(typeof(Objectives_RemoveStructure.FinalizedData_RemoveStructure))]
[XmlInclude(typeof(Objectives_ShootDummy.FinalizedData_ShootDummy))]
[XmlInclude(typeof(Objectives_UseRelic.FinalizedData_UseRelic))]
[XmlInclude(typeof(Objectives_TalkToFollower.FinalizedData_TalkToFollower))]
[XmlInclude(typeof(Objectives_CookMeal.FinalizedData_CookMeal))]
[XmlInclude(typeof(Objectives_PlaceStructure.FinalizedData_PlaceStructure))]
[XmlInclude(typeof(Objectives_PerformRitual.FinalizedData_PerformRitual))]
[XmlInclude(typeof(Objectives_UnlockUpgrade.FinalizedData_UnlockUpgrade))]
[XmlInclude(typeof(Objectives_EatMeal.FinalizedData_EatMeal))]
[XmlInclude(typeof(Objectives_RecruitCursedFollower.FinalizedData_RecruitCursedFollower))]
[XmlInclude(typeof(Objectives_FindFollower.FinalizedData_FindFollower))]
[XmlInclude(typeof(FinalizedNotification))]
[XmlInclude(typeof(FinalizedFaithNotification))]
[XmlInclude(typeof(FinalizedFollowerNotification))]
[XmlInclude(typeof(FinalizedRelationshipNotification))]
[XmlInclude(typeof(FinalizedItemNotification))]
public class DataManager
{
	public enum OnboardingPhase
	{
		Off,
		Indoctrinate,
		Shrine,
		Devotion,
		IndoctrinateBerriesAllowed,
		Done
	}

	public enum Chains
	{
		Chain1,
		Chain2,
		Chain3
	}

	[Serializable]
	public class LocationAndLayer
	{
		public FollowerLocation Location;

		public int Layer;

		public LocationAndLayer()
		{
		}

		public LocationAndLayer(FollowerLocation Location, int Layer)
		{
			this.Location = Location;
			this.Layer = Layer;
		}

		public static bool Contains(FollowerLocation Location, int Layer, List<LocationAndLayer> List)
		{
			foreach (LocationAndLayer item in List)
			{
				if (item.Location == Location && item.Layer == Layer)
				{
					return true;
				}
			}
			return false;
		}

		public static LocationAndLayer ContainsLocation(FollowerLocation Location, List<LocationAndLayer> List)
		{
			foreach (LocationAndLayer item in List)
			{
				if (item.Location == Location)
				{
					return item;
				}
			}
			return null;
		}
	}

	public enum CultLevel
	{
		One,
		Two,
		Three,
		Four
	}

	public class LocationSeedsData
	{
		public FollowerLocation Location;

		public int Seed;
	}

	public class LocationWeather
	{
		public FollowerLocation Location;

		public WeatherSystemController.WeatherType WeatherType;

		public WeatherSystemController.WeatherStrength WeatherStrength;

		public float StartingTime;

		public int Duration;
	}

	[Serializable]
	public struct DungeonCompletedFleeces
	{
		public FollowerLocation Location;

		public List<int> Fleeces;
	}

	public enum Variables
	{
		Rat_Tutorial_Bell,
		Goat_First_Meeting,
		Goat_Guardian_Door_Open,
		Key_Shrine_1,
		Key_Shrine_2,
		Key_Shrine_3,
		InTutorial,
		Tutorial_Rooms_Completed,
		Tutorial_Enable_Store_Resources,
		Tutorial_Completed,
		Create_Tutorial_Rooms,
		RatauExplainsFollowers,
		RatauExplainsDemo,
		SpokenToDeathNPC,
		RatauExplainsTeleporter,
		SozoIntro,
		SozoQuestComplete,
		TarotIntro,
		HasTarotBuilding,
		ForestOfferingRoomCompleted,
		KnucklebonesIntroCompleted,
		ForestChallengeRoom1Completed,
		ForestRescueWorshipper,
		RatauExplainsBiome0,
		RatauExplainsBiome0Boss,
		RatauExplainsBiome1,
		GetFirstFollower,
		RatauExplainBuilding,
		PromoteFollowerExplained,
		FoxMeeting_0,
		FirstMonsterHeart,
		Lighthouse_FirstConvo,
		Lighthouse_LitFirstConvo,
		Lighthouse_FireOutAgain,
		BirdConvo,
		RatOutpostIntro,
		RatExplainDungeon,
		RatExplainTemple,
		HorseTown_PaidRespectToHorse,
		HorseTown_JoinCult,
		HorseTown_OpenedChest,
		Tutorial_Indoctrinate_Begun,
		Tutorial_Indoctrinate_Completed,
		Dungeon1Story1,
		Dungeon1Story2,
		ShowFaith,
		EnabledSpells,
		EnabledHealing,
		FirstFollowerRescue,
		SherpaFirstConvo,
		UnlockedHubShore,
		ResourceRoom1Revealed,
		DecorationRoomFirstConvo,
		FirstTarot,
		Knucklebones_Opponent_Ratau_Won,
		Knucklebones_Opponent_0,
		Knucklebones_Opponent_0_FirstConvoRataus,
		Knucklebones_Opponent_0_Won,
		Knucklebones_Opponent_1_FirstConvoRataus,
		Knucklebones_Opponent_1,
		Knucklebones_Opponent_1_Won,
		Knucklebones_Opponent_2_FirstConvoRataus,
		Knucklebones_Opponent_2,
		Knucklebones_Opponent_2_Won,
		CultLeader1_LastRun,
		CultLeader1_StoryPosition,
		CultLeader2_LastRun,
		CultLeader2_StoryPosition,
		CultLeader3_LastRun,
		CultLeader3_StoryPosition,
		CultLeader4_LastRun,
		CultLeader4_StoryPosition,
		HaroConversationCompleted,
		PlayerHasFollowers,
		DungeonLayer1,
		DungeonLayer2,
		DungeonLayer3,
		DungeonLayer4,
		DungeonLayer5,
		HasBuiltShrine1,
		ShoreFishFirstConvo,
		ShoreFishFished,
		RatauShowShrineShop,
		RatauKilled,
		RatauReadLetter,
		BeatenDungeon1,
		BeatenDungeon2,
		BeatenDungeon3,
		BeatenDungeon4,
		BeatenDeathCat,
		CanFindTarotCards,
		DungeonKeyRoomCompleted1,
		DungeonKeyRoomCompleted2,
		DungeonKeyRoomCompleted3,
		DungeonKeyRoomCompleted4,
		ShoreTarotShotConvo1,
		ShoreTarotShotConvo2,
		ChefShopDoublePrices,
		FollowerShopUses,
		MidasIntro,
		MidasBankUnlocked,
		MidasBankIntro,
		ShoreFlowerShopConvo1,
		MidasDevotionIntro,
		MidasSacrificeIntro,
		EncounteredHealingRoom,
		GivenFreeDungeonFollower,
		BeatenFirstMiniBoss,
		RatooGivenHeart,
		RatooMentionedWrongHeart,
		RatauFoundSkin,
		DiedLastRun,
		RatauToGiveCurseNextRun,
		MidasFoundSkin,
		MinimumRandomRoomsEncountered,
		Tutorial_First_Indoctoring,
		FirstDoctrineStone,
		ForneusLore,
		HaroIntroduceDoctrines,
		SozoBeforeDeath,
		SozoDead,
		FirstDungeon1RescueRoom,
		FirstDungeon2RescueRoom,
		FirstDungeon3RescueRoom,
		FirstDungeon4RescueRoom,
		Lighthouse_QuestGiven,
		Lighthouse_QuestComplete,
		FirstTimeWeaponMarketplace,
		FirstTimeSpiderMarketplace,
		FirstTimeSeedMarketplace,
		ShowHaroDoctrineStoneRoom,
		SozoFoundDecoration,
		RatauGiftMediumCollected,
		SozoFlowerShopConvo1,
		SozoTarotShopConvo1,
		MidasStatue,
		ShowLoyaltyBars,
		CanBuildShrine,
		SandboxModeEnabled,
		OnboardedRelics,
		CanUnlockRelics,
		BeatenOneDungeons,
		BeatenTwoDungeons,
		BeatenThreeDungeons,
		BeatenFourDungeons,
		OnboardedMysticShop,
		OnboardedLayer2,
		BeatenWitnessDungeon1,
		BeatenWitnessDungeon2,
		BeatenWitnessDungeon3,
		BeatenWitnessDungeon4,
		FoundRelicAtHubShore,
		FoundRelicInFish,
		GivenRelicFishRiddle,
		GivenRelicLighthouseRiddle,
		CanFindLeaderRelic,
		PlimboSpecialEncountered,
		KlunkoSpecialEncountered,
		ShowSpecialHaroRoom,
		ShowSpecialMidasRoom,
		ShowSpecialPlimboRoom,
		ShowSpecialKlunkoRoom,
		ShowSpecialLeaderRoom,
		ShowSpecialFishermanRoom,
		ShowSpecialSozoRoom,
		ShowSpecialLighthouseKeeperRoom,
		LighthouseKeeperSpecialEncountered,
		SozoSpecialEncountered,
		FishermanSpecialEncountered,
		BaalAndAymSpecialEncounterd,
		ShowSpecialBaalAndAymRoom,
		MysticKeeperBeatenLeshy,
		MysticKeeperBeatenHeket,
		MysticKeeperBeatenKallamar,
		MysticKeeperBeatenShamura,
		MysticKeeperBeatenAll,
		SeedMarketPlacePostGame,
		HelobPostGame,
		MysticKeeperFirstPurchase,
		ForceMarketplaceCat,
		Total
	}

	public class QuestHistoryData
	{
		public int QuestIndex;

		public float QuestTimestamp;

		public float QuestCooldownDuration;

		public bool IsStory;
	}

	public enum DecorationType
	{
		None,
		Dungeon1,
		Mushroom,
		Crystal,
		Spider,
		All,
		Path,
		DLC,
		Special_Events
	}

	public delegate void ChangeToolAction(int PrevTool, int NewTool);

	public class EnemyData
	{
		public Enemy EnemyType;

		public int AmountKilled = 1;
	}

	[NonSerialized]
	[XmlIgnore]
	public MetaData MetaData;

	public bool AllowSaving = true;

	public bool PauseGameTime;

	public bool GameOverEnabled;

	public bool DisplayGameOverWarning;

	public bool InGameOver;

	public bool GameOver;

	public bool DifficultyChosen;

	public bool DifficultyReminded;

	public int playerDeaths;

	public int playerDeathsInARow;

	public int playerDeathsInARowFightingLeader;

	public int dungeonRun = -1;

	public float dungeonRunDuration;

	public List<NodeType> dungeonVisitedRooms = new List<NodeType> { NodeType.FirstFloor };

	public List<FollowerLocation> dungeonLocationsVisited = new List<FollowerLocation>();

	public List<int> FollowersRecruitedInNodes = new List<int>();

	public int FollowersRecruitedThisNode;

	public float TimeInGame;

	public int KillsInGame;

	public int dungeonRunXPOrbs;

	public int ChestRewardCount = -1;

	public bool BaseGoopDoorLocked;

	public string BaseGoopDoorLoc = "";

	public int STATS_FollowersStarvedToDeath;

	public int STATS_Murders;

	public int STATS_Sacrifices;

	public int STATS_NaturalDeaths;

	public int PlayerKillsOnRun;

	public int PlayerStartingBlackSouls;

	public bool GivenFollowerHearts;

	public bool EnabledSpells = true;

	public bool ForceDoctrineStones = true;

	public int DoctrineStoneTotalCount;

	public bool BuildShrineEnabled = true;

	public bool EnabledHealing = true;

	public bool EnabledSword = true;

	public bool BonesEnabled = true;

	public bool XPEnabled = true;

	public bool ShownDodgeTutorial = true;

	public bool ShownInventoryTutorial = true;

	public int ShownDodgeTutorialCount;

	public bool HadInitialDeathCatConversation = true;

	public bool PlayerHasBeenGivenHearts = true;

	public int TotalFirefliesCaught;

	public int TotalSquirrelsCaught;

	public int TotalBirdsCaught;

	public int TotalBodiesHarvested;

	public OnboardingPhase CurrentOnboardingPhase;

	public bool firstRecruit;

	public int MissionariesCompleted;

	public int PlayerFleece;

	public List<int> UnlockedFleeces = new List<int>();

	public bool PostGameFleecesOnboarded;

	public List<ThoughtData> Thoughts = new List<ThoughtData>();

	public bool CanReadMinds = true;

	public bool HappinessEnabled;

	public bool TeachingsEnabled;

	public bool SchedulingEnabled;

	public bool PrayerEnabled;

	public bool PrayerOrdered;

	public bool HasBuiltCookingFire;

	public bool HasBuiltFarmPlot;

	public bool HasBuiltTemple1;

	public bool HasBuiltTemple2;

	public bool HasBuiltTemple3;

	public bool HasBuiltTemple4;

	public bool HasBuiltShrine1 = true;

	public bool HasBuiltShrine2;

	public bool HasBuiltShrine3;

	public bool HasBuiltShrine4;

	public bool PerformedMushroomRitual;

	public bool BuiltMushroomDecoration;

	public bool HasBuiltSurveillance;

	public int TempleDevotionBoxCoinCount;

	public bool CanBuildShrine;

	public int WokeUpEveryoneDay = -1;

	public bool DiedLastRun;

	public UIDeathScreenOverlayController.Results LastRunResults = UIDeathScreenOverlayController.Results.None;

	public float LastFollowerToStarveToDeath;

	public float LastFollowerToStartStarving;

	public float LastFollowerToStartDissenting;

	public float LastFollowerToReachOldAge;

	public float LastFollowerToBecomeIll;

	public float LastFollowerToBecomeIllFromSleepingNearIllFollower;

	public float LastFollowerToPassOut;

	public int LastFollowerPurchasedFromSpider = -1;

	public float TimeSinceFaithHitEmpty = -1f;

	public float TimeSinceLastCrisisOfFaithQuest;

	public int JudgementAmount;

	public float HungerBarCount;

	public float IllnessBarCount;

	public float IllnessBarDynamicMax;

	public float StaticFaith = 55f;

	public float CultFaith;

	public float LastBrainwashed = float.MinValue;

	public float LastHolidayDeclared = float.MinValue;

	public float LastWorkThroughTheNight = float.MinValue;

	public float LastConstruction = float.MinValue;

	public float LastEnlightenment = float.MinValue;

	public float LastFastDeclared = float.MinValue;

	public float LastFeastDeclared = float.MinValue;

	public float LastFishingDeclared = float.MinValue;

	public float LastHalloween = float.MinValue;

	public float LastArcherShot = float.MinValue;

	public float LastSimpleGuardianAttacked = float.MinValue;

	public float LastSimpleGuardianRingProjectiles = float.MinValue;

	public int LastDayTreesAtBase = -1;

	public int PreviousSermonDayIndex = -1;

	public SermonCategory PreviousSermonCategory;

	public int ShrineLevel;

	public bool GivenSermonQuest;

	public bool GivenFaithOfFlockQuest;

	public bool PrayedAtMassiveMonsterShrine;

	public string TwitchSecretKey;

	public string ChannelID;

	public string ChannelName;

	public bool TwitchSentFollowers;

	public TwitchSettings TwitchSettings = new TwitchSettings();

	public int TwitchTotemsCompleted;

	public float TwitchNextHHEvent = -1f;

	public List<string> TwitchFollowerViewerIDs = new List<string>();

	public List<string> TwitchFollowerIDs = new List<string>();

	public bool OnboardingFinished;

	public string SaveUniqueID = "";

	public List<string> enemiesEncountered = new List<string>();

	public bool Chain1;

	public bool Chain2;

	public bool Chain3;

	public int DoorRoomChainProgress = -1;

	public int DoorRoomDoorsProgress = -1;

	public int Dungeon1_Layer = 1;

	public int Dungeon2_Layer = 1;

	public int Dungeon3_Layer = 1;

	public int Dungeon4_Layer = 1;

	public bool First_Dungeon_Resurrecting = true;

	public bool PermadeDeathActive;

	public int SpidersCaught;

	public bool PhotoCameraLookedAtGallery;

	public bool PhotoUsedCamera;

	public List<MiniBossController.MiniBossData> MiniBossData = new List<MiniBossController.MiniBossData>();

	public List<LocationAndLayer> CachePreviousRun = new List<LocationAndLayer>();

	public List<FollowerLocation> DiscoveredLocations = new List<FollowerLocation>();

	public List<FollowerLocation> VisitedLocations = new List<FollowerLocation>();

	public List<FollowerLocation> NewLocationFaithReward = new List<FollowerLocation>();

	public List<FollowerLocation> DissentingFolllowerRooms = new List<FollowerLocation>();

	[NonSerialized]
	[XmlIgnore]
	public FollowerLocation CurrentLocation = FollowerLocation.Base;

	public List<LocationAndLayer> OpenedDungeonDoors = new List<LocationAndLayer>();

	public List<string> KeyPiecesFromLocation = new List<string>();

	public List<FollowerLocation> UsedFollowerDispensers = new List<FollowerLocation>();

	public List<FollowerLocation> UnlockedBossTempleDoor = new List<FollowerLocation>();

	public List<FollowerLocation> UnlockedDungeonDoor = new List<FollowerLocation>();

	public List<FollowerLocation> BossesCompleted = new List<FollowerLocation>();

	public List<FollowerLocation> BossesEncountered = new List<FollowerLocation>();

	public List<FollowerLocation> DoorRoomBossLocksDestroyed = new List<FollowerLocation>();

	public List<FollowerLocation> SignPostsRead = new List<FollowerLocation>();

	public bool ShrineDoor;

	public bool BaseDoorEast;

	public bool BaseDoorNorthEast;

	public bool BaseDoorNorthWest;

	public bool BossForest;

	public bool ForestTempleDoor;

	public List<int> CompletedQuestFollowerIDs = new List<int>();

	public CultLevel CurrentCultLevel;

	public List<UnlockManager.UnlockType> MechanicsUnlocked = new List<UnlockManager.UnlockType>();

	public List<SermonsAndRituals.SermonRitualType> UnlockedSermonsAndRituals = new List<SermonsAndRituals.SermonRitualType>();

	public List<StructureBrain.TYPES> UnlockedStructures = new List<StructureBrain.TYPES>();

	public List<StructureBrain.TYPES> HistoryOfStructures = new List<StructureBrain.TYPES>();

	public Dictionary<StructureBrain.TYPES, int> DayPreviosulyUsedStructures = new Dictionary<StructureBrain.TYPES, int>();

	public bool NewBuildings;

	public List<TutorialTopic> RevealedTutorialTopics = new List<TutorialTopic>();

	public List<StructuresData.ResearchObject> CurrentResearch = new List<StructuresData.ResearchObject>();

	public UpgradeTreeNode.TreeTier CurrentUpgradeTreeTier;

	public UpgradeTreeNode.TreeTier CurrentPlayerUpgradeTreeTier;

	public UpgradeSystem.Type MostRecentTreeUpgrade = UpgradeSystem.Type.Building_Temple;

	public UpgradeSystem.Type MostRecentPlayerTreeUpgrade = UpgradeSystem.Type.PUpgrade_Heart_1;

	public List<UpgradeSystem.Type> UnlockedUpgrades = new List<UpgradeSystem.Type>();

	public List<DoctrineUpgradeSystem.DoctrineType> DoctrineUnlockedUpgrades = new List<DoctrineUpgradeSystem.DoctrineType>();

	public List<UpgradeSystem.UpgradeCoolDown> UpgradeCoolDowns = new List<UpgradeSystem.UpgradeCoolDown>();

	public List<FollowerTrait.TraitType> CultTraits = new List<FollowerTrait.TraitType>();

	public List<string> WeaponUnlockedUpgrades = new List<string>();

	public string CultName;

	public string MysticKeeperName = "???";

	public int PlayerTriedToEnterMysticDimensionCount;

	public bool DungeonBossFight;

	public List<LocationSeedsData> LocationSeeds = new List<LocationSeedsData>();

	public List<LocationWeather> LocationsWeather = new List<LocationWeather>();

	public LocationWeather GlobalWeatherOverride = new LocationWeather();

	public float TempleStudyXP;

	public int UnlockededASermon;

	public int CurrentDayIndex = 1;

	public int CurrentPhaseIndex;

	public float CurrentGameTime;

	public int[] LastUsedSermonRitualDayIndex = new int[0];

	public int[] ScheduledActivityIndexes = new int[5] { 0, 0, 0, 0, 2 };

	public int OverrideScheduledActivity = -1;

	public int[] InstantActivityIndexes = new int[1] { 8 };

	public bool PlayerEaten;

	public ResurrectionType ResurrectionType;

	public bool FirstTimeResurrecting = true;

	public bool FirstTimeFertilizing = true;

	public bool FirstTimeChop;

	public bool FirstTimeMine;

	public List<DungeonCompletedFleeces> DungeonsCompletedWithFleeces = new List<DungeonCompletedFleeces>();

	public StructureBrain.Categories currentCategory;

	public float TimeSinceLastComplaint;

	public float TimeSinceLastQuest;

	public int DessentingFollowerChoiceQuestionIndex;

	public int HaroConversationIndex;

	public int SpecialHaroConversationIndex;

	public List<FollowerLocation> HaroSpecialEncounteredLocations = new List<FollowerLocation>();

	public List<FollowerLocation> LeaderSpecialEncounteredLocations = new List<FollowerLocation>();

	public bool HaroConversationCompleted;

	public bool RatauKilled;

	public bool RatauReadLetter;

	public FollowerLocation CurrentFoxLocation = FollowerLocation.None;

	public int CurrentFoxEncounter;

	public List<FollowerLocation> FoxIntroductions = new List<FollowerLocation>();

	public List<FollowerLocation> FoxCompleted = new List<FollowerLocation>();

	public int PlimboStoryProgress;

	public int RatooFishingProgress;

	public bool RatooFishing_FISH_CRAB;

	public bool RatooFishing_FISH_LOBSTER;

	public bool RatooFishing_FISH_OCTOPUS;

	public bool RatooFishing_FISH_SQUID;

	public bool PlayerHasFollowers;

	public bool ShowSpecialHaroRoom;

	public bool ShowSpecialMidasRoom;

	public bool ShowSpecialPlimboRoom;

	public bool ShowSpecialKlunkoRoom;

	public bool ShowSpecialLeaderRoom;

	public bool ShowSpecialFishermanRoom;

	public bool ShowSpecialSozoRoom;

	public bool ShowSpecialBaalAndAymRoom;

	public bool ShowSpecialLighthouseKeeperRoom;

	public int FishCaughtTotal;

	public bool RatooGivenHeart;

	public bool RatooMentionedWrongHeart;

	public bool ShownInitialTempleDoorSeal;

	public bool FirstFollowerSpawnInteraction = true;

	public List<int> DecorationTypesBuilt = new List<int>();

	public List<TarotCards.Card> WeaponSelectionPositions = new List<TarotCards.Card>();

	public bool ShowCultFaith = true;

	public bool ShowCultIllness = true;

	public bool ShowCultHunger = true;

	public bool ShowLoyaltyBars = true;

	public bool SandboxModeEnabled;

	public bool IntroDoor1;

	public bool FirstDoctrineStone;

	public bool ShowHaroDoctrineStoneRoom;

	public bool HaroIntroduceDoctrines;

	public bool RatExplainDungeon = true;

	public bool RatauToGiveCurseNextRun;

	public int SozoStoryProgress = -1;

	public bool MidasBankUnlocked;

	public bool MidasBankIntro;

	public bool MidasSacrificeIntro;

	public bool MidasIntro;

	public bool MidasDevotionIntro;

	public bool MidasStatue;

	public float MidasDevotionCost = 1f;

	public int MidasDevotionLastUsed;

	public int MidasFollowerStatueCount;

	public bool RatauShowShrineShop;

	public bool DecorationRoomFirstConvo;

	public bool FirstTarot;

	public bool Tutorial_Night;

	public bool Tutorial_ReturnToDungeon;

	public bool FirstTimeInDungeon;

	public bool AllowBuilding = true;

	public bool CookedFirstFood = true;

	public bool Dungeon1Story1;

	public bool Dungeon1Story2;

	public bool FirstFollowerRescue;

	public bool FirstDungeon1RescueRoom;

	public bool FirstDungeon2RescueRoom;

	public bool FirstDungeon3RescueRoom;

	public bool FirstDungeon4RescueRoom;

	public bool SherpaFirstConvo;

	public bool ResourceRoom1Revealed;

	public bool EncounteredHealingRoom;

	public bool MinimumRandomRoomsEncountered;

	public int MinimumRandomRoomsEncounteredAmount;

	public bool ForneusLore;

	public bool SozoBeforeDeath;

	public bool SozoDead;

	public bool FirstTimeWeaponMarketplace;

	public bool FirstTimeSpiderMarketplace;

	public bool FirstTimeSeedMarketplace;

	public bool ShowFirstDoctrineStone = true;

	public bool RatauGiftMediumCollected;

	public bool CompletedLighthouseCrystalQuest;

	public bool CameFromDeathCatFight;

	public bool OldFollowerSpoken;

	public bool CanUnlockRelics;

	public bool FoundRelicAtHubShore;

	public bool FoundRelicInFish;

	public bool GivenRelicFishRiddle;

	public bool GivenRelicLighthouseRiddle;

	public bool ForceMarketplaceCat;

	public int CultLeader1_LastRun = -1;

	public int CultLeader1_StoryPosition;

	public int CultLeader2_LastRun = -1;

	public int CultLeader2_StoryPosition;

	public int CultLeader3_LastRun = -1;

	public int CultLeader3_StoryPosition;

	public int CultLeader4_LastRun = -1;

	public int CultLeader4_StoryPosition;

	public int DeathCatConversationLastRun = -999;

	public int DeathCatStory;

	public int DeathCatDead;

	public int DeathCatWon;

	public bool DeathCatBoss1;

	public bool DeathCatBoss2;

	public bool DeathCatBoss3;

	public bool DeathCatBoss4;

	public bool DeathCatRatauKilled;

	public bool DungeonKeyRoomCompleted1;

	public bool DungeonKeyRoomCompleted2;

	public bool DungeonKeyRoomCompleted3;

	public bool DungeonKeyRoomCompleted4;

	public bool RatOutpostIntro;

	public bool FirstMonsterHeart;

	public bool Rat_Tutorial_Bell;

	public bool Goat_First_Meeting;

	public bool Goat_Guardian_Door_Open;

	public bool Key_Shrine_1;

	public bool Key_Shrine_2;

	public bool Key_Shrine_3;

	public bool InTutorial = true;

	public bool UnlockBaseTeleporter = true;

	public bool Tutorial_First_Indoctoring;

	public bool Tutorial_Second_Enter_Base = true;

	public bool Tutorial_Rooms_Completed;

	public bool Tutorial_Enable_Store_Resources;

	public bool Tutorial_Completed;

	public bool Tutorial_Mission_Board;

	public bool Create_Tutorial_Rooms;

	public bool RatauExplainsFollowers;

	public bool RatauExplainsDemo;

	public bool RatauExplainsBiome0;

	public bool RatauExplainsBiome1;

	public bool RatauExplainsBiome0Boss;

	public bool RatauExplainsTeleporter;

	public bool SozoIntro;

	public bool SozoDecorationQuestActive;

	public bool SozoQuestComplete;

	public bool CollectedMenticide;

	public bool TarotIntro;

	public bool HasTarotBuilding = true;

	public bool ForestOfferingRoomCompleted;

	public bool KnucklebonesIntroCompleted;

	public bool KnucklebonesFirstGameRatauStart = true;

	public bool ForestChallengeRoom1Completed;

	public bool ForestRescueWorshipper;

	public bool GetFirstFollower;

	public bool BeatenFirstMiniBoss;

	public bool RatauExplainBuilding;

	public bool PromoteFollowerExplained;

	public bool HasMadeFirstOffering;

	public bool BirdConvo;

	public bool UnlockedHubShore;

	public bool GivenFollowerGift;

	public bool FinalBossSlowWalk = true;

	public int HadNecklaceOnRun;

	public bool ShownDungeon1FinalLeaderEncounter;

	public bool ShownDungeon2FinalLeaderEncounter;

	public bool ShownDungeon3FinalLeaderEncounter;

	public bool ShownDungeon4FinalLeaderEncounter;

	public bool HaroOnbardedHarderDungeon1;

	public bool HaroOnbardedHarderDungeon2;

	public bool HaroOnbardedHarderDungeon3;

	public bool HaroOnbardedHarderDungeon4;

	public bool HaroOnbardedHarderDungeon1_PostGame;

	public bool HaroOnbardedHarderDungeon2_PostGame;

	public bool HaroOnbardedHarderDungeon3_PostGame;

	public bool HaroOnbardedHarderDungeon4_PostGame;

	public bool RevealOfferingChest;

	public bool OnboardedOfferingChest;

	public bool OnboardedHomeless = true;

	public bool OnboardedHomelessAtNight;

	public bool OnboardedEndlessMode;

	public bool OnboardedDeadFollower;

	public bool OnboardedBuildingHouse;

	public bool OnboardedMakingMoreFood;

	public bool OnboardedCleaningBase;

	public bool OnboardedOldFollower;

	public bool OnboardedSickFollower;

	public bool OnboardedStarvingFollower;

	public bool OnboardedDissenter;

	public bool OnboardedFaithOfFlock;

	public bool OnboardedRaiseFaith;

	public bool OnboardedResourceYard;

	public bool OnboardedCrisisOfFaith;

	public bool OnboardedHalloween;

	public bool OnboardedSermon;

	public bool OnboardedBuildFarm;

	public bool OnboardedRefinery;

	public bool OnboardedCultName;

	public bool OnboardedZombie;

	public bool OnboardedLoyalty;

	public bool OnboardedGodTear;

	public bool OnboardedMysticShop;

	public bool ForeshadowedMysticShop;

	public bool OnboardedLayer2;

	public bool OnboardedRelics;

	public bool HasMetChefShop;

	public int CurrentOnboardingFollowerID = -1;

	public int CurrentOnboardingFollowerType = -1;

	public string CurrentOnboardingFollowerTerm;

	public bool HasPerformedRitual;

	public bool DeathCatBaalAndAymSecret;

	public bool ShamuraBaalAndAymSecret;

	public bool CanFindLeaderRelic;

	public List<FollowerLocation> SecretItemsGivenToFollower = new List<FollowerLocation>();

	public bool BeatenWitnessDungeon1;

	public bool BeatenWitnessDungeon2;

	public bool BeatenWitnessDungeon3;

	public bool BeatenWitnessDungeon4;

	public bool MysticKeeperBeatenLeshy;

	public bool MysticKeeperBeatenHeket;

	public bool MysticKeeperBeatenKallamar;

	public bool MysticKeeperBeatenShamura;

	public bool MysticKeeperBeatenAll;

	public bool MysticKeeperFirstPurchase;

	public bool BeatenPostGame;

	public int GivenLoyaltyQuestDay = -1;

	public int LastDaySincePlayerUpgrade = -1;

	public int MealsCooked;

	public int TalismanPiecesReceivedFromMysticShop;

	public bool MysticShopUsed;

	public int CrystalDoctrinesReceivedFromMysticShop;

	public InventoryItem.ITEM_TYPE PreviousMysticShopItem;

	public bool OnboardedCrystalDoctrine;

	public bool Dungeon1_1_Key;

	public bool Dungeon1_2_Key;

	public bool Dungeon1_3_Key;

	public bool Dungeon1_4_Key;

	public bool Dungeon2_1_Key;

	public bool Dungeon2_2_Key;

	public bool Dungeon2_3_Key;

	public bool Dungeon2_4_Key;

	public bool Dungeon3_1_Key;

	public bool Dungeon3_2_Key;

	public bool Dungeon3_3_Key;

	public bool Dungeon3_4_Key;

	public bool HadFirstTempleKey;

	public int CurrentKeyPieces;

	public bool GivenFreeDungeonFollower;

	public bool GivenFreeDungeonGold;

	public bool FoxMeeting_0;

	public bool GaveFollowerToFox;

	public bool Ritual_0;

	public bool Ritual_1;

	public bool Lighthouse_FirstConvo;

	public bool Lighthouse_LitFirstConvo;

	public bool Lighthouse_FireOutAgain;

	public bool Lighthouse_QuestGiven;

	public bool Lighthouse_QuestComplete;

	public int LighthouseFuel;

	public bool Lighthouse_Lit;

	public bool ShoreFishFirstConvo;

	public bool ShoreFishFished;

	public bool ShoreTarotShotConvo1;

	public bool ShoreTarotShotConvo2;

	public bool ShoreFlowerShopConvo1;

	public bool SozoFlowerShopConvo1;

	public bool SozoTarotShopConvo1;

	public bool RatauFoundSkin;

	public bool MidasFoundSkin;

	public bool SozoFoundDecoration;

	public int MidasTotalGoldStolen;

	public int MidasSpecialEncounter;

	public List<FollowerLocation> MidasSpecialEncounteredLocations = new List<FollowerLocation>();

	public bool MidasBeaten;

	public bool PlimboSpecialEncountered;

	public bool KlunkoSpecialEncountered;

	public bool FishermanSpecialEncountered;

	public bool BaalAndAymSpecialEncounterd;

	public bool LighthouseKeeperSpecialEncountered;

	public bool SozoSpecialEncountered;

	public float OpenedDoorTimestamp;

	public bool SeedMarketPlacePostGame;

	public bool HelobPostGame;

	public bool HorseTown_PaidRespectToHorse;

	public bool HorseTown_JoinCult;

	public bool HorseTown_OpenedChest;

	public bool BlackSoulsEnabled = true;

	public bool PlacedRubble;

	public bool DefeatedExecutioner;

	public int WorldMapCurrentSelection;

	public bool HasBaalSkin;

	public bool HasReturnedBaal;

	public bool HasAymSkin;

	public bool HasReturnedAym;

	public bool HasReturnedBoth;

	public int RedHeartsTemporarilyRemoved;

	public bool ShownKnuckleboneTutorial;

	public bool Knucklebones_Opponent_Ratau_Won;

	public int ShopKeeperChefState;

	public int ShopKeeperChefEnragedDay;

	public bool Knucklebones_Opponent_0;

	public bool Knucklebones_Opponent_0_FirstConvoRataus;

	public bool Knucklebones_Opponent_0_Won;

	public bool Knucklebones_Opponent_1;

	public bool Knucklebones_Opponent_1_FirstConvoRataus;

	public bool Knucklebones_Opponent_1_Won;

	public bool Knucklebones_Opponent_2;

	public bool Knucklebones_Opponent_2_FirstConvoRataus;

	public bool Knucklebones_Opponent_2_Won;

	public bool DungeonLayer1;

	public bool DungeonLayer2;

	public bool DungeonLayer3;

	public bool DungeonLayer4;

	public bool DungeonLayer5;

	public bool BeatenDungeon1;

	public bool BeatenDungeon2;

	public bool BeatenDungeon3;

	public bool BeatenDungeon4;

	public bool BeatenDeathCat;

	public bool BeatenLeshyLayer2;

	public bool BeatenHeketLayer2;

	public bool BeatenKallamarLayer2;

	public bool BeatenShamuraLayer2;

	public bool BeatenOneDungeons;

	public bool BeatenTwoDungeons;

	public bool BeatenThreeDungeons;

	public bool BeatenFourDungeons;

	public int Dungeon1GodTears;

	public int Dungeon2GodTears;

	public int Dungeon3GodTears;

	public int Dungeon4GodTears;

	public int DungeonRunsSinceBeatingFirstDungeon;

	public string PreviousMiniBoss;

	public List<DungeonSandboxManager.ProgressionSnapshot> SandboxProgression = new List<DungeonSandboxManager.ProgressionSnapshot>();

	public bool OnboardedBossRush;

	public bool CompletedSandbox;

	public bool CanFindTarotCards = true;

	public float LuckMultiplier = 1f;

	public bool NextMissionarySuccessful;

	public float EnemyModifiersChanceMultiplier = 1f;

	public float EnemyHealthMultiplier = 1f;

	public float ProjectileMoveSpeedMultiplier = 1f;

	public float DodgeDistanceMultiplier = 1f;

	public float CurseFeverMultiplier = 1f;

	public bool SpawnPoisonOnAttack;

	public bool EnemiesInNextRoomHaveModifiers;

	public Vector2 CurrentRoomCoordinates;

	public const float SpecialAttackDamageMultiplierBaseConst = 1.25f;

	public float SpecialAttackDamageMultiplier = 1.25f;

	public bool NextChestGold;

	public bool SpecialAttacksDisabled;

	public float BossHealthMultiplier = 1f;

	public int ResurrectRitualCount;

	public bool EncounteredGambleRoom;

	public List<int> LeaderFollowersRecruited = new List<int>();

	public int SwordLevel;

	public int DaggerLevel;

	public int AxeLevel;

	public int HammerLevel;

	public int GauntletLevel;

	public int FireballLevel;

	public int EnemyBlastLevel;

	public int MegaSlashLevel;

	public int ProjectileAOELevel;

	public int TentaclesLevel;

	public int VortexLevel;

	public float LastFollowerQuestGivenTime;

	public bool DLC_Pre_Purchase;

	public bool DLC_Cultist_Pack;

	public bool DLC_Heretic_Pack;

	public bool DLC_Plush_Bonus;

	public bool PAX_DLC;

	public bool Twitch_Drop_1;

	public bool Twitch_Drop_2;

	public bool Twitch_Drop_3;

	public bool Twitch_Drop_4;

	public bool Twitch_Drop_5;

	public bool Twitch_Drop_6;

	public bool Twitch_Drop_7;

	public bool ForceDammedRelic;

	public bool ForceBlessedRelic;

	public bool FirstRelic = true;

	public bool EndlessModeOnCooldown;

	public bool DeathCatBeaten;

	public bool HasEncounteredTarot;

	public List<InventoryItem.ITEM_TYPE> RecentRecipes = new List<InventoryItem.ITEM_TYPE>();

	public List<InventoryItem.ITEM_TYPE> RecipesDiscovered = new List<InventoryItem.ITEM_TYPE>();

	private float playerDamageDealt;

	public int PlayerScaleModifier = 1;

	public bool ChefShopDoublePrices;

	public int FollowerShopUses;

	public int sacrificesCompleted;

	public List<InventoryItem.ITEM_TYPE> FoundItems = new List<InventoryItem.ITEM_TYPE>();

	public bool TakenBossDamage;

	public int PoopMealsCreated;

	public bool PrayedAtCrownShrine;

	public bool ShellsGifted_0;

	public bool ShellsGifted_1;

	public bool ShellsGifted_2;

	public bool ShellsGifted_3;

	public bool ShellsGifted_4;

	public int DateLastScreenshot = -1;

	public float PlayerDamageDealtThisRun;

	public float PlayerDamageReceivedThisRun;

	private float playerDamageReceived;

	public bool Options_ScreenShake = true;

	public static System.Random RandomSeed = new System.Random(UnityEngine.Random.Range(int.MinValue, int.MaxValue));

	public static bool UseDataManagerSeed = false;

	public bool PlayerIsASpirit;

	public bool BridgeFixed;

	public bool BuildingTome;

	public bool BeenToDungeon;

	public int FollowerID;

	public int ObjectiveGroupID;

	public List<FollowerInfo> Followers = new List<FollowerInfo>();

	public List<FollowerInfo> Followers_Recruit = new List<FollowerInfo>();

	public List<FollowerInfo> Followers_Dead = new List<FollowerInfo>();

	public List<int> Followers_Dead_IDs = new List<int>();

	public int StructureID;

	public List<StructuresData> BaseStructures = new List<StructuresData>();

	public List<StructuresData> HubStructures = new List<StructuresData>();

	public List<StructuresData> HubShoreStructures = new List<StructuresData>();

	public List<StructuresData> Hub1_MainStructures = new List<StructuresData>();

	public List<StructuresData> Hub1_BerriesStructures = new List<StructuresData>();

	public List<StructuresData> Hub1_ForestStructures = new List<StructuresData>();

	public List<StructuresData> Hub1_RatauInsideStructures = new List<StructuresData>();

	public List<StructuresData> Hub1_RatauOutsideStructures = new List<StructuresData>();

	public List<StructuresData> Hub1_SozoStructures = new List<StructuresData>();

	public List<StructuresData> Hub1_SwampStructures = new List<StructuresData>();

	public List<StructuresData> Dungeon_Logs1Structures = new List<StructuresData>();

	public List<StructuresData> Dungeon_Logs2Structures = new List<StructuresData>();

	public List<StructuresData> Dungeon_Logs3Structures = new List<StructuresData>();

	public List<StructuresData> Dungeon_Food1Structures = new List<StructuresData>();

	public List<StructuresData> Dungeon_Food2Structures = new List<StructuresData>();

	public List<StructuresData> Dungeon_Food3Structures = new List<StructuresData>();

	public List<StructuresData> Dungeon_Stone1Structures = new List<StructuresData>();

	public List<StructuresData> Dungeon_Stone2Structures = new List<StructuresData>();

	public List<StructuresData> Dungeon_Stone3Structures = new List<StructuresData>();

	public List<int> Followers_Imprisoned_IDs = new List<int>();

	public List<int> Followers_Elderly_IDs = new List<int>();

	public List<int> Followers_OnMissionary_IDs = new List<int>();

	public List<int> Followers_Transitioning_IDs = new List<int>();

	public List<int> Followers_Demons_IDs = new List<int>();

	public List<int> Followers_Demons_Types = new List<int>();

	public List<SeasonalEventType> ActiveSeasonalEvents = new List<SeasonalEventType>();

	public List<StructureBrain.TYPES> RevealedStructures = new List<StructureBrain.TYPES>();

	public List<DayObject> DayList = new List<DayObject>();

	public DayObject CurrentDay;

	public List<string> TrackedObjectiveGroupIDs = new List<string>();

	public List<ObjectivesData> Objectives = new List<ObjectivesData>();

	public List<ObjectivesData> CompletedObjectives = new List<ObjectivesData>();

	public List<ObjectivesData> FailedObjectives = new List<ObjectivesData>();

	public List<ObjectivesData> DungeonObjectives = new List<ObjectivesData>();

	public List<StoryData> StoryObjectives = new List<StoryData>();

	public List<ObjectivesDataFinalized> CompletedObjectivesHistory = new List<ObjectivesDataFinalized>();

	public List<ObjectivesDataFinalized> FailedObjectivesHistory = new List<ObjectivesDataFinalized>();

	public List<QuestHistoryData> CompletedQuestsHistorys = new List<QuestHistoryData>();

	public InventoryItem.ITEM_TYPE SimpleInventoryItem;

	public List<InventoryItem> items = new List<InventoryItem>();

	public int IngredientsCapacityLevel;

	public static List<int> IngredientsCapacity = new List<int> { 150, 50, 100 };

	public int FoodCapacityLevel;

	public static List<int> FoodCapacity = new List<int> { 150, 50, 100 };

	public int LogCapacityLevel;

	public static List<int> LogCapacity = new List<int> { 150, 50, 100 };

	public int StoneCapacityLevel;

	public static List<int> StoneCapacity = new List<int> { 150, 50, 100 };

	[NonSerialized]
	[XmlIgnore]
	public static Action<string> OnSkinUnlocked;

	public List<string> FollowerSkinsUnlocked = new List<string> { "Cat", "Dog", "Pig", "Deer", "Fox" };

	[NonSerialized]
	[XmlIgnore]
	public List<string> FollowerSkinsBlacklist = new List<string>
	{
		"Axolotl", "Starfish", "Fish", "Shrew", "Poop", "Crab", "Snail", "MassiveMonster", "Nightwolf", "Butterfly",
		"Squirrel", "CultLeader 1", "CultLeader 2", "CultLeader 3", "CultLeader 4", "Boss Baal", "Boss Aym", "Shrimp", "Koala", "Owl",
		"Bee", "Tapir", "Turtle", "Monkey", "Narwal", "Cthulhu", "Webber", "TwitchMouse", "TwitchCat", "TwitchPoggers",
		"TwitchDog", "TwitchDogAlt", "Lion", "Penguin", "Kiwi", "Pelican", "DeerSkull", "BatDemon", "Crow", "Moose",
		"Gorilla", "Mosquito", "Goldfish", "Possum", "StarBunny"
	};

	[NonSerialized]
	[XmlIgnore]
	public List<string> DLCSkins = new List<string>
	{
		"TwitchPoggers", "TwitchDog", "TwitchDogAlt", "Lion", "Penguin", "Kiwi", "Pelican", "TwitchMouse", "TwitchCat", "Cthulhu",
		"Bee", "Tapir", "Turtle", "Monkey", "Narwal", "Moose", "Gorilla", "Mosquito", "Goldfish", "Possum"
	};

	[NonSerialized]
	[XmlIgnore]
	public string[] SpecialEventSkins = new string[4] { "DeerSkull", "BatDemon", "Crow", "StarBunny" };

	[NonSerialized]
	[XmlIgnore]
	public static string[] MysticShopKeeperSkins = new string[10] { "Penguin", "Lion", "Shrimp", "Koala", "Owl", "TwitchPoggers", "TwitchDog", "TwitchDogAlt", "TwitchCat", "TwitchMouse" };

	public int MysticRewardCount;

	public List<StructureEffect> StructureEffects = new List<StructureEffect>();

	public List<string> KilledBosses = new List<string>();

	public static List<InventoryItem.ITEM_TYPE> AllNecklaces = new List<InventoryItem.ITEM_TYPE>
	{
		InventoryItem.ITEM_TYPE.Necklace_1,
		InventoryItem.ITEM_TYPE.Necklace_2,
		InventoryItem.ITEM_TYPE.Necklace_3,
		InventoryItem.ITEM_TYPE.Necklace_4,
		InventoryItem.ITEM_TYPE.Necklace_5,
		InventoryItem.ITEM_TYPE.Necklace_Missionary,
		InventoryItem.ITEM_TYPE.Necklace_Light,
		InventoryItem.ITEM_TYPE.Necklace_Loyalty,
		InventoryItem.ITEM_TYPE.Necklace_Demonic,
		InventoryItem.ITEM_TYPE.Necklace_Dark,
		InventoryItem.ITEM_TYPE.Necklace_Gold_Skull
	};

	public static List<InventoryItem.ITEM_TYPE> AllGifts = new List<InventoryItem.ITEM_TYPE>
	{
		InventoryItem.ITEM_TYPE.GIFT_SMALL,
		InventoryItem.ITEM_TYPE.GIFT_MEDIUM
	};

	public static Action<EquipmentType> OnWeaponUnlocked;

	public List<EquipmentType> WeaponPool = new List<EquipmentType>();

	public EquipmentType CurrentWeapon = EquipmentType.None;

	public int CurrentWeaponLevel;

	public int CurrentRunWeaponLevel;

	public EquipmentType ForcedStartingWeapon = EquipmentType.None;

	public EquipmentType CurrentCurse = EquipmentType.None;

	public int CurrentCurseLevel;

	public int CurrentRunCurseLevel;

	public EquipmentType ForcedStartingCurse = EquipmentType.None;

	public RelicType CurrentRelic;

	public List<RelicType> SpawnedRelicsThisRun = new List<RelicType>();

	public float RelicChargeAmount;

	public const int WEAPON_LEVEL_DIVIDER = 3;

	public static List<float> WeaponDurabilityChance = new List<float> { 80f, 90f, 98f };

	public static List<float> WeaponDurabilityLevels = new List<float> { 75f, 50f, 85f, 60f, 100f };

	public static Action<TarotCards.Card> OnCurseUnlocked;

	public List<EquipmentType> CursePool = new List<EquipmentType>();

	public static List<TarotCards.Card> TarotCardBlacklist = new List<TarotCards.Card>
	{
		TarotCards.Card.Count,
		TarotCards.Card.Vortex,
		TarotCards.Card.Tripleshot,
		TarotCards.Card.MegaSlash,
		TarotCards.Card.Tentacles,
		TarotCards.Card.Fireball,
		TarotCards.Card.EnemyBlast,
		TarotCards.Card.ProjectileAOE,
		TarotCards.Card.Sword,
		TarotCards.Card.Dagger,
		TarotCards.Card.Axe,
		TarotCards.Card.Blunderbuss,
		TarotCards.Card.Hammer,
		TarotCards.Card.InvincibleWhileHealing
	};

	public static List<TarotCards.Card> AllTrinkets = new List<TarotCards.Card>
	{
		TarotCards.Card.Hearts1,
		TarotCards.Card.Hearts2,
		TarotCards.Card.Hearts3,
		TarotCards.Card.Moon,
		TarotCards.Card.NaturesGift,
		TarotCards.Card.Lovers1,
		TarotCards.Card.Lovers2,
		TarotCards.Card.Sun,
		TarotCards.Card.EyeOfWeakness,
		TarotCards.Card.Telescope,
		TarotCards.Card.HandsOfRage,
		TarotCards.Card.Skull,
		TarotCards.Card.DiseasedHeart,
		TarotCards.Card.Spider,
		TarotCards.Card.AttackRate,
		TarotCards.Card.IncreasedDamage,
		TarotCards.Card.MovementSpeed,
		TarotCards.Card.IncreaseBlackSoulsDrop,
		TarotCards.Card.HealChance,
		TarotCards.Card.NegateDamageChance,
		TarotCards.Card.BombOnRoll,
		TarotCards.Card.GoopOnDamaged,
		TarotCards.Card.GoopOnRoll,
		TarotCards.Card.PoisonImmune,
		TarotCards.Card.DamageOnRoll,
		TarotCards.Card.AmmoEfficient,
		TarotCards.Card.BlackSoulAutoRecharge,
		TarotCards.Card.BlackSoulOnDamage,
		TarotCards.Card.NeptunesCurse,
		TarotCards.Card.HealTwiceAmount,
		TarotCards.Card.DeathsDoor,
		TarotCards.Card.TheDeal,
		TarotCards.Card.RabbitFoot,
		TarotCards.Card.Potion,
		TarotCards.Card.GiftFromBelow,
		TarotCards.Card.Arrows,
		TarotCards.Card.ImmuneToTraps,
		TarotCards.Card.BombOnDamaged,
		TarotCards.Card.WalkThroughBlocks,
		TarotCards.Card.InvincibilityPerRoom,
		TarotCards.Card.TentacleOnDamaged,
		TarotCards.Card.MoreRelics,
		TarotCards.Card.DecreaseRelicCharge
	};

	public List<TarotCards.TarotCard> PlayerRunTrinkets = new List<TarotCards.TarotCard>();

	public List<TarotCards.Card> PlayerFoundTrinkets = new List<TarotCards.Card>();

	public List<CrownAbilities> CrownAbilitiesUnlocked = new List<CrownAbilities>();

	public List<RelicType> PlayerFoundRelics = new List<RelicType>();

	public static List<BluePrint.BluePrintType> AllBluePrints = new List<BluePrint.BluePrintType>
	{
		BluePrint.BluePrintType.TREE,
		BluePrint.BluePrintType.STONE,
		BluePrint.BluePrintType.PATH_DIRT
	};

	public List<BluePrint> PlayerBluePrints = new List<BluePrint>();

	public List<InventoryItem.ITEM_TYPE> FishCaught = new List<InventoryItem.ITEM_TYPE>();

	public List<MissionManager.Mission> ActiveMissions = new List<MissionManager.Mission>();

	public List<MissionManager.Mission> AvailableMissions = new List<MissionManager.Mission>();

	public float NewMissionDayTimestamp = -1f;

	public int LastGoldenMissionDay = -1;

	public bool MissionShrineUnlocked;

	public List<BuyEntry> ItemsForSale = new List<BuyEntry>();

	public List<ShopLocationTracker> Shops = new List<ShopLocationTracker>();

	public int LastDayUsedFollowerShop = -1;

	public FollowerInfo FollowerForSale;

	public MidasDonation midasDonation;

	public int LastDayUsedBank = -1;

	public JellyFishInvestment Investment;

	public List<TraderTracker> Traders = new List<TraderTracker>();

	public List<ShrineUsageInfo> ShrineTimerInfo = new List<ShrineUsageInfo>();

	public static List<int> RedHeartShrineCosts = new List<int> { 50, 250, 500, 1000 };

	public int RedHeartShrineLevel;

	public int ShrineHeart;

	public int ShrineCurses;

	public int ShrineVoodo;

	public int ShrineAstrology;

	public List<ItemSelector.Category> ItemSelectorCategories = new List<ItemSelector.Category>();

	public List<InventoryItem> itemsDungeon = new List<InventoryItem>();

	public float DUNGEON_TIME;

	public float PLAYER_RUN_DAMAGE_LEVEL;

	public int PLAYER_HEARTS_LEVEL;

	public int PLAYER_DAMAGE_LEVEL;

	public float PLAYER_HEALTH = 6f;

	public float PLAYER_TOTAL_HEALTH = 6f;

	public float PLAYER_BLUE_HEARTS;

	public float PLAYER_BLACK_HEARTS;

	public float PLAYER_SPIRIT_HEARTS;

	public float PLAYER_SPIRIT_TOTAL_HEARTS;

	public float PLAYER_SPECIAL_CHARGE;

	public float PLAYER_SPECIAL_AMMO;

	public float PLAYER_SPECIAL_CHARGE_TARGET = 10f;

	public int PLAYER_ARROW_AMMO = 3;

	public int PLAYER_ARROW_TOTAL_AMMO = 3;

	public int PLAYER_SPIRIT_AMMO;

	public int PLAYER_SPIRIT_TOTAL_AMMO;

	public int PLAYER_REDEAL_TOKEN;

	public int PLAYER_REDEAL_TOKEN_TOTAL;

	public int PLAYER_HEALTH_MODIFIED;

	public int PLAYER_STARTING_HEALTH_CACHED = -1;

	public int Souls;

	public int BlackSouls;

	public int BlackSoulTarget;

	public int FollowerTokens;

	public float DiscipleXP;

	public int DiscipleLevel;

	public int DiscipleAbilityPoints;

	public static List<float> TargetDiscipleXP = new List<float> { 3f, 2f, 3f, 4f };

	public int XP;

	public int Level;

	public const float XPMultiplier = 1.3f;

	public const float XPMultiplierLvlII = 1.5f;

	public const float XPMultiplierLvlIII = 2.5f;

	public const float XPMultiplierLvlIV = 3f;

	public static List<int> TargetXP = new List<int>
	{
		1,
		Mathf.FloorToInt(13f),
		Mathf.FloorToInt(29.9f),
		Mathf.FloorToInt(45.5f),
		Mathf.FloorToInt(59.8f),
		Mathf.FloorToInt(65f),
		Mathf.FloorToInt(68.899994f),
		Mathf.FloorToInt(93f),
		Mathf.FloorToInt(93f),
		Mathf.FloorToInt(106.5f),
		Mathf.FloorToInt(106.5f),
		Mathf.FloorToInt(200f),
		Mathf.FloorToInt(200f),
		Mathf.FloorToInt(195f),
		Mathf.FloorToInt(215f),
		Mathf.FloorToInt(215f),
		Mathf.FloorToInt(215f),
		Mathf.FloorToInt(258f),
		Mathf.FloorToInt(258f),
		Mathf.FloorToInt(285f),
		Mathf.FloorToInt(285f),
		Mathf.FloorToInt(315f),
		Mathf.FloorToInt(345f),
		Mathf.FloorToInt(339f),
		Mathf.FloorToInt(363f),
		Mathf.FloorToInt(363f),
		Mathf.FloorToInt(363f),
		Mathf.FloorToInt(363f),
		Mathf.FloorToInt(390f),
		Mathf.FloorToInt(390f),
		Mathf.FloorToInt(390f),
		Mathf.FloorToInt(390f),
		Mathf.FloorToInt(417f),
		Mathf.FloorToInt(441f),
		Mathf.FloorToInt(441f),
		Mathf.FloorToInt(441f),
		Mathf.FloorToInt(441f),
		Mathf.FloorToInt(465f),
		Mathf.FloorToInt(465f),
		Mathf.FloorToInt(465f),
		Mathf.FloorToInt(465f)
	};

	public int AbilityPoints;

	public int WeaponAbilityPoints = 100;

	public int CurrentChallengeModeXP;

	public int CurrentChallengeModeLevel;

	public static List<int> ChallengeModeTargetXP = new List<int> { 50, 65, 100, 125, 150, 200, 250, 300 };

	public float Doctrine_PlayerUpgrade_XP;

	public int Doctrine_PlayerUpgrade_Level;

	public float Doctrine_Special_XP;

	public int Doctrine_Special_Level;

	public float Doctrine_WorkWorship_XP;

	public int Doctrine_WorkWorship_Level;

	public float Doctrine_Possessions_XP;

	public int Doctrine_Possessions_Level;

	public float Doctrine_Food_XP;

	public int Doctrine_Food_Level;

	public float Doctrine_Afterlife_XP;

	public int Doctrine_Afterlife_Level;

	public float Doctrine_LawAndOrder_XP;

	public int Doctrine_LawAndOrder_Level;

	public int CompletedDoctrineStones;

	public int DoctrineCurrentCount;

	public int DoctrineTargetCount;

	public int FRUIT_LOW_MEALS_COOKED;

	public int VEGETABLE_LOW_MEALS_COOKED;

	public int VEGETABLE_MEDIUM_MEALS_COOKED;

	public int VEGETABLE_HIGH_MEALS_COOKED;

	public int FISH_LOW_MEALS_COOKED;

	public int FISH_MEDIUM_MEALS_COOKED;

	public int FISH_HIGH_MEALS_COOKED;

	public int MEAT_LOW_COOKED;

	public int MEAT_MEDIUM_COOKED;

	public int MEAT_HIGH_COOKED;

	public int MIXED_LOW_COOKED;

	public int MIXED_MEDIUM_COOKED;

	public int MIXED_HIGH_COOKED;

	public int POOP_MEALS_COOKED;

	public int GRASS_MEALS_COOKED;

	public int FOLLOWER_MEAT_MEALS_COOKED;

	public int DEADLY_MEALS_COOKED;

	public static List<float> PlayerUpgradeTargetXP = new List<float>
	{
		0.3f, 0.4f, 1.1f, 1.2f, 1.5f, 2f, 2.6f, 3.5f, 5f, 6f,
		7f, 8f, 9f, 10f
	};

	public static float DoctrineMultiplier = 4f;

	public static List<float> DoctrineTargetXP = new List<float>
	{
		0.5f,
		0.6f * (DoctrineMultiplier / 2f),
		1f * (DoctrineMultiplier / 1.5f),
		1.5f * DoctrineMultiplier,
		3f * DoctrineMultiplier,
		6f * DoctrineMultiplier,
		9f * DoctrineMultiplier,
		12f * DoctrineMultiplier,
		15f * DoctrineMultiplier,
		18f * DoctrineMultiplier
	};

	private int currentweapon;

	private static DataManager instance = null;

	public List<EnemyData> EnemiesKilled = new List<EnemyData>();

	public Alerts Alerts = new Alerts();

	public List<FinalizedNotification> NotificationHistory = new List<FinalizedNotification>();

	public static List<StructureBrain.TYPES> CultistDLCStructures = new List<StructureBrain.TYPES>
	{
		StructureBrain.TYPES.TILE_FLOWERS,
		StructureBrain.TYPES.DECORATION_FLOWERPOTWALL,
		StructureBrain.TYPES.DECORATION_LEAFYLAMPPOST,
		StructureBrain.TYPES.DECORATION_FLOWERVASE,
		StructureBrain.TYPES.DECORATION_WATERINGCAN,
		StructureBrain.TYPES.DECORATION_FLOWER_CART_SMALL,
		StructureBrain.TYPES.DECORATION_WEEPINGSHRINE
	};

	public static List<string> CultistDLCSkins = new List<string> { "Bee", "Tapir", "Turtle", "Monkey", "Narwal" };

	public static List<StructureBrain.TYPES> HereticDLCStructures = new List<StructureBrain.TYPES>
	{
		StructureBrain.TYPES.TILE_OLDFAITH,
		StructureBrain.TYPES.DECORATION_OLDFAITH_CRYSTAL,
		StructureBrain.TYPES.DECORATION_OLDFAITH_FLAG,
		StructureBrain.TYPES.DECORATION_OLDFAITH_FOUNTAIN,
		StructureBrain.TYPES.DECORATION_OLDFAITH_IRONMAIDEN,
		StructureBrain.TYPES.DECORATION_OLDFAITH_SHRINE,
		StructureBrain.TYPES.DECORATION_OLDFAITH_TORCH,
		StructureBrain.TYPES.DECORATION_OLDFAITH_WALL
	};

	public static List<string> HereticDLCSkins = new List<string> { "Moose", "Gorilla", "Mosquito", "Goldfish", "Possum" };

	public float PlayerDamageDealt
	{
		get
		{
			return playerDamageDealt;
		}
		set
		{
			playerDamageDealt = value;
			DifficultyManager.LoadCurrentDifficulty();
		}
	}

	public float PlayerDamageReceived
	{
		get
		{
			return playerDamageReceived;
		}
		set
		{
			playerDamageReceived = value;
			DifficultyManager.LoadCurrentDifficulty();
		}
	}

	public List<InventoryItem> Food
	{
		get
		{
			List<InventoryItem> list = new List<InventoryItem>();
			foreach (InventoryItem item in Inventory.items)
			{
				if (item.type == 6 || item.type == 21)
				{
					list.Add(item);
				}
			}
			return list;
		}
	}

	public static int StartingEquipmentLevel
	{
		get
		{
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_StartingWeapon_6))
			{
				return 18;
			}
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_StartingWeapon_5))
			{
				return 15;
			}
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_StartingWeapon_4))
			{
				return 12;
			}
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_StartingWeapon_3))
			{
				return 9;
			}
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_StartingWeapon_2))
			{
				return 6;
			}
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_StartingWeapon_1))
			{
				return 3;
			}
			return 0;
		}
	}

	public int PLAYER_STARTING_HEALTH
	{
		get
		{
			switch (DifficultyManager.PrimaryDifficulty)
			{
			case DifficultyManager.Difficulty.Easy:
				return 8;
			case DifficultyManager.Difficulty.Hard:
				return 4;
			case DifficultyManager.Difficulty.ExtraHard:
				return 2;
			default:
				return 6;
			}
		}
	}

	private static float AllUnlockedMultiplier
	{
		get
		{
			if (GameManager.HasUnlockAvailable())
			{
				return 1f;
			}
			return 3f;
		}
	}

	[JsonIgnore]
	public int CurrentChallengeModeTargetXP
	{
		get
		{
			return ChallengeModeTargetXP[Mathf.Min(CurrentChallengeModeLevel, ChallengeModeTargetXP.Count - 1)];
		}
	}

	public int CURRENT_WEAPON
	{
		get
		{
			return currentweapon;
		}
		set
		{
			if (DataManager.OnChangeTool != null)
			{
				DataManager.OnChangeTool(currentweapon, value);
			}
			currentweapon = value;
		}
	}

	public static DataManager Instance
	{
		get
		{
			if (instance == null)
			{
				new DataManager();
			}
			return instance;
		}
		set
		{
			instance = value;
		}
	}

	public static event ChangeToolAction OnChangeTool;

	public static bool TwitchTotemRewardAvailable()
	{
		if (GetAvailableTwitchTotemDecorations().Count <= 0)
		{
			return GetAvailableTwitchTotemSkins().Count > 0;
		}
		return true;
	}

	public static List<StructureBrain.TYPES> GetAvailableTwitchTotemDecorations()
	{
		List<StructureBrain.TYPES> list = new List<StructureBrain.TYPES>(6)
		{
			StructureBrain.TYPES.DECORATION_TWITCH_FLAG_CROWN,
			StructureBrain.TYPES.DECORATION_TWITCH_MUSHROOM_BAG,
			StructureBrain.TYPES.DECORATION_TWITCH_ROSE_BUSH,
			StructureBrain.TYPES.DECORATION_TWITCH_STONE_FLAG,
			StructureBrain.TYPES.DECORATION_TWITCH_STONE_STATUE,
			StructureBrain.TYPES.DECORATION_TWITCH_WOODEN_GUARDIAN
		};
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (StructuresData.GetUnlocked(list[num]))
			{
				list.RemoveAt(num);
			}
		}
		return list;
	}

	public static List<string> GetAvailableTwitchTotemSkins()
	{
		List<string> list = new List<string>(2) { "TwitchCat", "TwitchMouse" };
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (GetFollowerSkinUnlocked(list[num]))
			{
				list.RemoveAt(num);
			}
		}
		return list;
	}

	public void AddEncounteredEnemy(string enemy)
	{
		for (int i = 0; i < enemy.Length; i++)
		{
			if (enemy[i] == '(')
			{
				enemy = enemy.Remove(i - 1, enemy.Length - i + 1);
				break;
			}
		}
		if (!enemiesEncountered.Contains(enemy))
		{
			enemiesEncountered.Add(enemy);
		}
	}

	public bool HasEncounteredEnemy(string enemy)
	{
		for (int i = 0; i < enemy.Length; i++)
		{
			if (enemy[i] == '(')
			{
				enemy = enemy.Remove(i - 1, enemy.Length - i + 1);
				break;
			}
		}
		return enemiesEncountered.Contains(enemy);
	}

	public MiniBossController.MiniBossData GetMiniBossData(Enemy enemyType)
	{
		foreach (MiniBossController.MiniBossData miniBossDatum in MiniBossData)
		{
			if (miniBossDatum.EnemyType == enemyType)
			{
				return miniBossDatum;
			}
		}
		return null;
	}

	public bool DiscoverLocation(FollowerLocation location)
	{
		Alerts.Locations.Add(location);
		if (!DiscoveredLocations.Contains(location))
		{
			DiscoveredLocations.Add(location);
			if (DiscoveredLocations.Contains(FollowerLocation.Hub1_RatauOutside) && DiscoveredLocations.Contains(FollowerLocation.Hub1_Sozo) && DiscoveredLocations.Contains(FollowerLocation.HubShore) && DiscoveredLocations.Contains(FollowerLocation.Dungeon_Decoration_Shop1) && DiscoveredLocations.Contains(FollowerLocation.Dungeon_Location_4))
			{
				AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("FIND_ALL_LOCATIONS"));
			}
			return true;
		}
		return false;
	}

	public static bool HasKeyPieceFromLocation(FollowerLocation Location, int Layer)
	{
		return Instance.KeyPiecesFromLocation.Contains(string.Concat(Location, "_", Layer));
	}

	public static void SaveKeyPieceFromLocation(FollowerLocation Location, int Layer)
	{
		if (!HasKeyPieceFromLocation(Location, Layer))
		{
			Instance.KeyPiecesFromLocation.Add(string.Concat(Location, "_", Layer));
		}
	}

	public bool TryRevealTutorialTopic(TutorialTopic topic)
	{
		if (!RevealedTutorialTopics.Contains(topic) && !CheatConsole.HidingUI)
		{
			RevealedTutorialTopics.Add(topic);
			Alerts.Tutorial.AddOnce(topic);
			return true;
		}
		return false;
	}

	public void SetTutorialVariables()
	{
		Instance.AllowSaving = false;
		Instance.EnabledHealing = false;
		Instance.BuildShrineEnabled = false;
		instance.CookedFirstFood = false;
		instance.XPEnabled = false;
		Instance.InTutorial = false;
		Instance.Tutorial_Second_Enter_Base = false;
		Instance.AllowBuilding = false;
		Instance.ShowLoyaltyBars = false;
		Instance.RatExplainDungeon = false;
		Instance.ShowCultFaith = false;
		Instance.ShowCultHunger = false;
		Instance.ShowCultIllness = false;
		Instance.UnlockBaseTeleporter = false;
		Instance.BonesEnabled = false;
		instance.PauseGameTime = true;
		instance.ShownDodgeTutorial = false;
		instance.ShownInventoryTutorial = false;
		instance.HasEncounteredTarot = false;
		Instance.CurrentGameTime = 244f;
		Instance.HasBuiltShrine1 = false;
		Instance.OnboardedHomeless = false;
		Instance.ForceDoctrineStones = false;
		instance.HadInitialDeathCatConversation = false;
		instance.PlayerHasBeenGivenHearts = false;
		instance.BaseGoopDoorLocked = true;
		instance.FirstRelic = true;
		Instance.PLAYER_TOTAL_HEALTH = Instance.PLAYER_STARTING_HEALTH;
		instance.PLAYER_STARTING_HEALTH_CACHED = Instance.PLAYER_STARTING_HEALTH;
		instance.PLAYER_HEALTH = Instance.PLAYER_STARTING_HEALTH;
		instance.SaveUniqueID = UnityEngine.Random.Range(int.MinValue, int.MaxValue).ToString().GetStableHashCode()
			.ToString();
		GameManager.CurrentDungeonLayer = 1;
		if (!CheatConsole.IN_DEMO)
		{
			Instance.CanReadMinds = false;
			Instance.EnabledSpells = false;
		}
	}

	public bool GetVariable(Variables variable)
	{
		return (bool)typeof(DataManager).GetField(variable.ToString()).GetValue(Instance);
	}

	public bool GetVariable(string variablestring)
	{
		return (bool)typeof(DataManager).GetField(variablestring).GetValue(Instance);
	}

	public int GetVariableInt(string variablestring)
	{
		return (int)typeof(DataManager).GetField(variablestring).GetValue(Instance);
	}

	public int GetVariableInt(Variables variable)
	{
		return (int)typeof(DataManager).GetField(variable.ToString()).GetValue(Instance);
	}

	public void SetVariableInt(Variables variable, int Value)
	{
		typeof(DataManager).GetField(variable.ToString()).SetValue(Instance, Value);
	}

	public void SetVariable(Variables variable, bool Toggle)
	{
		typeof(DataManager).GetField(variable.ToString()).SetValue(Instance, Toggle);
	}

	public void SetVariable(string variablestring, bool Toggle)
	{
		typeof(DataManager).GetField(variablestring).SetValue(Instance, Toggle);
	}

	public static void SetNewRun(FollowerLocation location = FollowerLocation.None)
	{
		instance.EnabledSword = true;
		instance.dungeonRunDuration = Time.time;
		instance.dungeonVisitedRooms = new List<NodeType> { NodeType.FirstFloor };
		instance.dungeonLocationsVisited.Add(PlayerFarming.Location);
		instance.FollowersRecruitedInNodes = new List<int>();
		instance.FollowersRecruitedThisNode = 0;
		instance.PlayerKillsOnRun = 0;
		instance.PlayerStartingBlackSouls = instance.BlackSouls;
		instance.dungeonRunXPOrbs = 0;
		instance.dungeonRun++;
		instance.ShrineTimerInfo.Clear();
		if (instance.BossesCompleted.Count >= 1)
		{
			instance.DungeonRunsSinceBeatingFirstDungeon++;
		}
		HealthPlayer.ResetHealthData = true;
		Debug.Log("Increase dungeon run! " + instance.dungeonRun);
		instance.GivenFollowerHearts = false;
		RandomSeed = new System.Random(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
		UseDataManagerSeed = true;
		instance.PlayerHasFollowers = instance.Followers.Count > 0;
		instance.PlayerDamageReceivedThisRun = 0f;
		instance.PlayerDamageDealtThisRun = 0f;
		instance.SpecialAttackDamageMultiplier = 1.25f;
		instance.NextChestGold = false;
		instance.SpecialAttacksDisabled = false;
		instance.BossHealthMultiplier = 1f;
		instance.HadNecklaceOnRun = 0;
		instance.CanFindLeaderRelic = false;
		DungeonModifier.SetActiveModifier(null);
		instance.CurrentWeapon = EquipmentType.None;
		Instance.CurrentWeaponLevel = 0;
		Instance.CurrentRunWeaponLevel = 0;
		instance.CurrentCurse = EquipmentType.None;
		Instance.CurrentCurseLevel = 0;
		Instance.CurrentRunCurseLevel = 0;
		instance.CurrentRelic = RelicType.None;
		Instance.RelicChargeAmount = 0f;
		if (!Instance.OnboardedRelics)
		{
			Instance.FirstRelic = true;
		}
		if (LocalizationManager.GetTermData(string.Format("Conversation_NPC/Haro/Conversation_{0}/Line{1}", Instance.HaroConversationIndex, 1)) != null)
		{
			instance.HaroConversationCompleted = false;
		}
		else
		{
			instance.HaroConversationCompleted = true;
		}
		instance.DungeonLayer1 = GameManager.CurrentDungeonLayer == 1;
		instance.DungeonLayer2 = GameManager.CurrentDungeonLayer == 2;
		instance.DungeonLayer3 = GameManager.CurrentDungeonLayer == 3;
		instance.DungeonLayer4 = GameManager.CurrentDungeonLayer == 4;
		instance.DungeonLayer5 = GameManager.CurrentDungeonLayer == 5;
		instance.BeatenDungeon1 = instance.DungeonCompleted(FollowerLocation.Dungeon1_1);
		instance.BeatenDungeon2 = instance.DungeonCompleted(FollowerLocation.Dungeon1_2);
		instance.BeatenDungeon3 = instance.DungeonCompleted(FollowerLocation.Dungeon1_3);
		instance.BeatenDungeon4 = instance.DungeonCompleted(FollowerLocation.Dungeon1_4);
		instance.BeatenOneDungeons = instance.BossesCompleted.Count >= 1;
		instance.BeatenTwoDungeons = instance.BossesCompleted.Count >= 2;
		instance.BeatenThreeDungeons = instance.BossesCompleted.Count >= 3;
		instance.BeatenFourDungeons = instance.BossesCompleted.Count >= 4;
		instance.BeatenDeathCat = instance.DeathCatBeaten;
		instance.BeatenWitnessDungeon1 = Instance.CheckKilledBosses("Boss Beholder 1");
		instance.BeatenWitnessDungeon2 = Instance.CheckKilledBosses("Boss Beholder 2");
		instance.BeatenWitnessDungeon3 = Instance.CheckKilledBosses("Boss Beholder 3");
		instance.BeatenWitnessDungeon4 = Instance.CheckKilledBosses("Boss Beholder 4");
		instance.ForceMarketplaceCat = instance.Followers_Demons_Types.Contains(6) || instance.Followers_Demons_Types.Contains(7);
		instance.CanUnlockRelics = instance.BeatenOneDungeons && (instance.DungeonRunsSinceBeatingFirstDungeon >= 3 || instance.BeatenTwoDungeons);
		instance.ShowSpecialMidasRoom = (!instance.MidasSpecialEncounteredLocations.Contains(location) && GetGodTearNotches(location) + 1 >= 1 && location == FollowerLocation.Dungeon1_1) || (!instance.MidasSpecialEncounteredLocations.Contains(location) && GetGodTearNotches(location) + 1 >= 1 && location == FollowerLocation.Dungeon1_3) || (!instance.MidasSpecialEncounteredLocations.Contains(location) && GetGodTearNotches(location) + 1 >= 3 && location == FollowerLocation.Dungeon1_4);
		instance.ShowSpecialLeaderRoom = (!instance.LeaderSpecialEncounteredLocations.Contains(location) && GetGodTearNotches(location) + 1 >= 2 && location == FollowerLocation.Dungeon1_1) || (!instance.LeaderSpecialEncounteredLocations.Contains(location) && GetGodTearNotches(location) + 1 >= 1 && location == FollowerLocation.Dungeon1_2) || (!instance.LeaderSpecialEncounteredLocations.Contains(location) && GetGodTearNotches(location) + 1 >= 3 && location == FollowerLocation.Dungeon1_3) || (!instance.LeaderSpecialEncounteredLocations.Contains(location) && GetGodTearNotches(location) + 1 >= 1 && location == FollowerLocation.Dungeon1_4);
		instance.ShowSpecialKlunkoRoom = ((GetGodTearNotches(location) + 1 >= 4) & !instance.KlunkoSpecialEncountered) && location == FollowerLocation.Dungeon1_1;
		instance.ShowSpecialPlimboRoom = ((GetGodTearNotches(location) + 1 >= 2) & !instance.PlimboSpecialEncountered) && location == FollowerLocation.Dungeon1_3;
		instance.ShowSpecialFishermanRoom = ((GetGodTearNotches(location) + 1 >= 4) & !instance.FishermanSpecialEncountered) && location == FollowerLocation.Dungeon1_3;
		instance.ShowSpecialLighthouseKeeperRoom = ((GetGodTearNotches(location) + 1 >= 4) & !instance.LighthouseKeeperSpecialEncountered) && location == FollowerLocation.Dungeon1_2;
		instance.ShowSpecialSozoRoom = ((GetGodTearNotches(location) + 1 >= 3) & !instance.LighthouseKeeperSpecialEncountered) && location == FollowerLocation.Dungeon1_2;
		instance.ShowSpecialBaalAndAymRoom = instance.HasReturnedBoth && !instance.BaalAndAymSpecialEncounterd;
		instance.MinimumRandomRoomsEncountered = instance.MinimumRandomRoomsEncounteredAmount > 3;
		instance.CanFindTarotCards = !PlayerFleeceManager.FleecePreventTarotCards();
		PlayerFleeceManager.ResetDamageModifier();
	}

	public static void ResetRunData()
	{
		TrinketManager.RemoveAllTrinkets();
		Instance.PlayerEaten = false;
		instance.PLAYER_REDEAL_TOKEN = instance.PLAYER_REDEAL_TOKEN_TOTAL;
		instance.PLAYER_RUN_DAMAGE_LEVEL = 0f;
		ResurrectOnHud.ResurrectionType = ResurrectionType.None;
		FaithAmmo.Reload();
		Instance.FailedObjectives.Clear();
		instance.HadNecklaceOnRun = 0;
		HealthPlayer.ResetHealthData = true;
		Instance.PLAYER_TOTAL_HEALTH = Instance.PLAYER_STARTING_HEALTH + Instance.PLAYER_HEARTS_LEVEL + instance.PLAYER_HEALTH_MODIFIED;
		Instance.PLAYER_STARTING_HEALTH_CACHED = Instance.PLAYER_STARTING_HEALTH;
		Instance.RedHeartsTemporarilyRemoved = 0;
		Instance.PlayerScaleModifier = 1;
		HUD_Timer.TimerRunning = false;
		HUD_Timer.Timer = 0f;
		Instance.PLAYER_HEALTH = Instance.PLAYER_TOTAL_HEALTH;
		Instance.PLAYER_SPIRIT_HEARTS = (Instance.PLAYER_SPIRIT_TOTAL_HEARTS = 0f);
		Instance.PLAYER_BLUE_HEARTS = 0f;
		instance.PLAYER_BLACK_HEARTS = 0f;
		DungeonLayerStatue.ShownDungeonLayer = false;
		instance.EnemyModifiersChanceMultiplier = 1f;
		instance.EnemyHealthMultiplier = 1f;
		instance.CurseFeverMultiplier = 1f;
		instance.ProjectileMoveSpeedMultiplier = 1f;
		instance.DodgeDistanceMultiplier = 1f;
		instance.SpawnPoisonOnAttack = false;
		instance.EnemiesInNextRoomHaveModifiers = false;
		instance.CurrentRoomCoordinates = Vector2.zero;
		DungeonModifier.SetActiveModifier(null);
		for (int num = Instance.Objectives.Count - 1; num >= 0; num--)
		{
			if (Instance.DungeonObjectives.Contains(Instance.Objectives[num]))
			{
				Instance.Objectives.Remove(Instance.Objectives[num]);
			}
		}
		for (int num2 = Instance.CompletedObjectives.Count - 1; num2 >= 0; num2--)
		{
			if (Instance.DungeonObjectives.Contains(Instance.CompletedObjectives[num2]))
			{
				Instance.CompletedObjectives.Remove(Instance.CompletedObjectives[num2]);
			}
		}
		Instance.DungeonObjectives.Clear();
		instance.Alerts.RunTarotCardAlerts.ClearAll();
		Instance.CurrentWeaponLevel = 1;
		Instance.CurrentWeapon = EquipmentType.None;
		Instance.CurrentCurseLevel = 1;
		Instance.CurrentCurse = EquipmentType.None;
		Instance.RelicChargeAmount = 0f;
		Instance.CurrentRelic = RelicType.None;
		instance.SpawnedRelicsThisRun.Clear();
	}

	public void ReplaceDeprication(GameManager root)
	{
		foreach (FollowerInfo item in Followers_Dead)
		{
			if (!Followers_Dead_IDs.Contains(item.ID))
			{
				Followers_Dead_IDs.Add(item.ID);
			}
		}
		if (string.IsNullOrEmpty(instance.SaveUniqueID))
		{
			instance.SaveUniqueID = UnityEngine.Random.Range(int.MinValue, int.MaxValue).ToString().GetStableHashCode()
				.ToString();
		}
		if (DeathCatBeaten && !StructuresData.GetUnlocked(StructureBrain.TYPES.DECORATION_BOSS_TROPHY_5) && !LeaderFollowersRecruited.Contains(FollowerManager.DeathCatID))
		{
			LeaderFollowersRecruited.Add(FollowerManager.DeathCatID);
		}
		instance.PlayerScaleModifier = 1;
		for (int num = Alerts.CharacterSkinAlerts.Total - 1; num >= 0; num--)
		{
			if (WorshipperData.Instance.GetCharacters(Alerts.CharacterSkinAlerts._alerts[num]).Invariant)
			{
				Alerts.CharacterSkinAlerts._alerts.RemoveAt(num);
			}
		}
	}

	public void AddToCompletedQuestHistory(ObjectivesDataFinalized finalizedData)
	{
		CompletedObjectivesHistory.Insert(0, finalizedData);
		ObjectiveManager.MaintainObjectiveHistoryList(ref CompletedObjectivesHistory);
	}

	public void AddToFailedQuestHistory(ObjectivesDataFinalized finalizedData)
	{
		FailedObjectivesHistory.Insert(0, finalizedData);
		ObjectiveManager.MaintainObjectiveHistoryList(ref FailedObjectivesHistory);
	}

	public static WorshipperData.SkinAndData GetSkinAndDataFromString(string FollowerSkin)
	{
		foreach (WorshipperData.SkinAndData character in WorshipperData.Instance.Characters)
		{
			if (character.Skin[0].Skin == FollowerSkin)
			{
				return character;
			}
		}
		return null;
	}

	public static bool CheckAvailableDecorations()
	{
		foreach (StructureBrain.TYPES allStructure in StructuresData.AllStructures)
		{
			if (StructuresData.GetCategory(allStructure) == StructureBrain.Categories.AESTHETIC && !StructuresData.GetUnlocked(allStructure) && !OnDecorationBlacklist(allStructure))
			{
				return true;
			}
		}
		return false;
	}

	public static List<StructureBrain.TYPES> GetAvailableDecorations(bool excludeBlackList = true)
	{
		List<StructureBrain.TYPES> list = new List<StructureBrain.TYPES>();
		foreach (StructureBrain.TYPES allStructure in StructuresData.AllStructures)
		{
			if (StructuresData.GetCategory(allStructure) == StructureBrain.Categories.AESTHETIC && !StructuresData.GetUnlocked(allStructure) && (!excludeBlackList || (excludeBlackList && !OnDecorationBlacklist(allStructure))))
			{
				list.Add(allStructure);
			}
		}
		return list;
	}

	public static bool OnDecorationBlacklist(StructureBrain.TYPES type)
	{
		switch (type)
		{
		case StructureBrain.TYPES.DECORATION_TREE:
		case StructureBrain.TYPES.DECORATION_STONE:
		case StructureBrain.TYPES.PLANK_PATH:
		case StructureBrain.TYPES.DECORATION_LAMB_STATUE:
		case StructureBrain.TYPES.DECORATION_TORCH:
		case StructureBrain.TYPES.DECORATION_FLOWER_BOX_2:
		case StructureBrain.TYPES.DECORATION_FLOWER_BOX_3:
		case StructureBrain.TYPES.DECORATION_SMALL_STONE_CANDLE:
		case StructureBrain.TYPES.DECORATION_FLAG_CROWN:
		case StructureBrain.TYPES.DECORATION_FLAG_SCRIPTURE:
		case StructureBrain.TYPES.DECORATION_WALL_TWIGS:
		case StructureBrain.TYPES.DECORATION_SHRUB:
		case StructureBrain.TYPES.DECORATION_BELL_STATUE:
		case StructureBrain.TYPES.DECORATION_BONE_ARCH:
		case StructureBrain.TYPES.DECORATION_BONE_BARREL:
		case StructureBrain.TYPES.DECORATION_BONE_CANDLE:
		case StructureBrain.TYPES.DECORATION_BONE_FLAG:
		case StructureBrain.TYPES.DECORATION_BONE_LANTERN:
		case StructureBrain.TYPES.DECORATION_BONE_PILLAR:
		case StructureBrain.TYPES.DECORATION_BONE_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_CANDLE_BARREL:
		case StructureBrain.TYPES.DECORATION_POST_BOX:
		case StructureBrain.TYPES.DECORATION_WALL_BONE:
		case StructureBrain.TYPES.DECORATION_WALL_GRASS:
		case StructureBrain.TYPES.DECORATION_FLOWER_BOTTLE:
		case StructureBrain.TYPES.DECORATION_FLOWER_CART:
		case StructureBrain.TYPES.DECORATION_LEAFY_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_1:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_2:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_2:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_LARGE:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_SPIDER_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_STONE_CANDLE_LAMP:
		case StructureBrain.TYPES.TILE_FLOWERS:
		case StructureBrain.TYPES.TILE_SPOOKYPLANKS:
		case StructureBrain.TYPES.TILE_REDGRASS:
		case StructureBrain.TYPES.TILE_WATER:
		case StructureBrain.TYPES.DECORATION_MONSTERSHRINE:
		case StructureBrain.TYPES.DECORATION_FLOWERPOTWALL:
		case StructureBrain.TYPES.DECORATION_LEAFYLAMPPOST:
		case StructureBrain.TYPES.DECORATION_FLOWERVASE:
		case StructureBrain.TYPES.DECORATION_WATERINGCAN:
		case StructureBrain.TYPES.DECORATION_FLOWER_CART_SMALL:
		case StructureBrain.TYPES.DECORATION_WEEPINGSHRINE:
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_1:
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_2:
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_3:
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_4:
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_5:
		case StructureBrain.TYPES.DECORATION_VIDEO:
		case StructureBrain.TYPES.DECORATION_PLUSH:
		case StructureBrain.TYPES.DECORATION_TWITCH_FLAG_CROWN:
		case StructureBrain.TYPES.DECORATION_TWITCH_MUSHROOM_BAG:
		case StructureBrain.TYPES.DECORATION_TWITCH_ROSE_BUSH:
		case StructureBrain.TYPES.DECORATION_TWITCH_STONE_FLAG:
		case StructureBrain.TYPES.DECORATION_TWITCH_STONE_STATUE:
		case StructureBrain.TYPES.DECORATION_TWITCH_WOODEN_GUARDIAN:
		case StructureBrain.TYPES.DECORATION_HALLOWEEN_PUMPKIN:
		case StructureBrain.TYPES.DECORATION_HALLOWEEN_SKULL:
		case StructureBrain.TYPES.DECORATION_HALLOWEEN_CANDLE:
		case StructureBrain.TYPES.DECORATION_HALLOWEEN_TREE:
		case StructureBrain.TYPES.DECORATION_OLDFAITH_CRYSTAL:
		case StructureBrain.TYPES.DECORATION_OLDFAITH_FLAG:
		case StructureBrain.TYPES.DECORATION_OLDFAITH_FOUNTAIN:
		case StructureBrain.TYPES.DECORATION_OLDFAITH_IRONMAIDEN:
		case StructureBrain.TYPES.DECORATION_OLDFAITH_SHRINE:
		case StructureBrain.TYPES.DECORATION_OLDFAITH_TORCH:
		case StructureBrain.TYPES.DECORATION_OLDFAITH_WALL:
		case StructureBrain.TYPES.TILE_OLDFAITH:
			return true;
		default:
			return false;
		}
	}

	public static bool GetDecorationsAvailableCategory(FollowerLocation location)
	{
		DecorationType decorationType;
		switch (location)
		{
		case FollowerLocation.HubShore:
		case FollowerLocation.Dungeon1_1:
		case FollowerLocation.Boss_1:
			decorationType = DecorationType.Dungeon1;
			break;
		case FollowerLocation.Dungeon1_2:
		case FollowerLocation.Boss_2:
		case FollowerLocation.Sozo:
			decorationType = DecorationType.Mushroom;
			break;
		case FollowerLocation.Dungeon1_3:
		case FollowerLocation.Boss_3:
		case FollowerLocation.Dungeon_Decoration_Shop1:
			decorationType = DecorationType.Crystal;
			break;
		case FollowerLocation.Dungeon1_4:
		case FollowerLocation.Boss_4:
		case FollowerLocation.Dungeon_Location_3:
			decorationType = DecorationType.Spider;
			break;
		default:
			Debug.Log("Couldnt find player farming location: " + location);
			decorationType = DecorationType.All;
			break;
		}
		List<StructureBrain.TYPES> list = new List<StructureBrain.TYPES>();
		List<StructureBrain.TYPES> decorationListFromCategory = instance.GetDecorationListFromCategory(DecorationType.All);
		foreach (StructureBrain.TYPES availableDecoration in GetAvailableDecorations())
		{
			if ((GetDecorationType(availableDecoration) == decorationType || GetDecorationType(availableDecoration) == DecorationType.Path) && !StructuresData.GetUnlocked(availableDecoration))
			{
				list.Add(availableDecoration);
			}
		}
		foreach (StructureBrain.TYPES item in decorationListFromCategory)
		{
			if (!StructuresData.GetUnlocked(item))
			{
				list.Add(item);
			}
		}
		if (list.Count > 0)
		{
			return true;
		}
		Debug.Log("Couldnt find any unlocked decorations");
		return false;
	}

	public List<StructureBrain.TYPES> GetDecorationListFromLocation(FollowerLocation location)
	{
		switch (location)
		{
		case FollowerLocation.HubShore:
		case FollowerLocation.Dungeon1_1:
		case FollowerLocation.Boss_1:
			return Instance.GetDecorationListFromCategory(DecorationType.Dungeon1);
		case FollowerLocation.Dungeon1_2:
		case FollowerLocation.Boss_2:
		case FollowerLocation.Sozo:
			return Instance.GetDecorationListFromCategory(DecorationType.Mushroom);
		case FollowerLocation.Dungeon1_3:
		case FollowerLocation.Boss_3:
		case FollowerLocation.Dungeon_Decoration_Shop1:
			return Instance.GetDecorationListFromCategory(DecorationType.Crystal);
		case FollowerLocation.Dungeon1_4:
		case FollowerLocation.Boss_4:
		case FollowerLocation.Dungeon_Location_4:
			return Instance.GetDecorationListFromCategory(DecorationType.Spider);
		default:
			Debug.Log("Couldnt find player farming location: " + location);
			return Instance.GetDecorationListFromCategory(DecorationType.All);
		}
	}

	public List<StructureBrain.TYPES> GetDecorationListFromCategory(DecorationType category)
	{
		List<StructureBrain.TYPES> availableDecorations = GetAvailableDecorations();
		List<StructureBrain.TYPES> list = new List<StructureBrain.TYPES>();
		foreach (StructureBrain.TYPES item in availableDecorations)
		{
			if (GetDecorationType(item) == category || GetDecorationType(item) == DecorationType.Path)
			{
				list.Add(item);
			}
		}
		if (list.Count > 0)
		{
			return list;
		}
		return new List<StructureBrain.TYPES>();
	}

	public static DecorationCost GetDecorationTypeCost(DecorationType type)
	{
		DecorationCost decorationCost = new DecorationCost();
		switch (type)
		{
		case DecorationType.All:
			decorationCost.costAmount = 25;
			decorationCost.costType = InventoryItem.ITEM_TYPE.BLACK_GOLD;
			return decorationCost;
		case DecorationType.Dungeon1:
			decorationCost.costAmount = 25;
			decorationCost.costType = InventoryItem.ITEM_TYPE.BLACK_GOLD;
			return decorationCost;
		case DecorationType.Mushroom:
			decorationCost.costAmount = 50;
			decorationCost.costType = InventoryItem.ITEM_TYPE.BLACK_GOLD;
			return decorationCost;
		case DecorationType.Crystal:
			decorationCost.costAmount = 100;
			decorationCost.costType = InventoryItem.ITEM_TYPE.BLACK_GOLD;
			return decorationCost;
		case DecorationType.Spider:
			decorationCost.costAmount = 2;
			decorationCost.costType = InventoryItem.ITEM_TYPE.GOLD_REFINED;
			return decorationCost;
		default:
			decorationCost.costAmount = 50;
			decorationCost.costType = InventoryItem.ITEM_TYPE.BLACK_GOLD;
			return decorationCost;
		}
	}

	public static string GetDecorationLocation(StructureBrain.TYPES type)
	{
		switch (GetDecorationType(type))
		{
		case DecorationType.Dungeon1:
			return ScriptLocalization.NAMES_Places.Dungeon1_1;
		case DecorationType.Crystal:
			return ScriptLocalization.NAMES_Places.Dungeon1_3;
		case DecorationType.All:
			return "";
		case DecorationType.Path:
			return ScriptLocalization.UI.Paths;
		case DecorationType.Mushroom:
			return ScriptLocalization.NAMES_Places.Dungeon1_2;
		case DecorationType.Spider:
			return ScriptLocalization.NAMES_Places.Dungeon1_4;
		default:
			return "";
		}
	}

	public static DecorationType GetDecorationType(StructureBrain.TYPES type)
	{
		if (DecorationsForType(DecorationType.All).Contains(type))
		{
			return DecorationType.All;
		}
		if (DecorationsForType(DecorationType.Dungeon1).Contains(type))
		{
			return DecorationType.Dungeon1;
		}
		if (DecorationsForType(DecorationType.Mushroom).Contains(type))
		{
			return DecorationType.Mushroom;
		}
		if (DecorationsForType(DecorationType.Crystal).Contains(type))
		{
			return DecorationType.Crystal;
		}
		if (DecorationsForType(DecorationType.Spider).Contains(type))
		{
			return DecorationType.Spider;
		}
		if (DecorationsForType(DecorationType.Path).Contains(type))
		{
			return DecorationType.Path;
		}
		return DecorationType.None;
	}

	public static List<StructureBrain.TYPES> DecorationsForType(DecorationType decorationType)
	{
		switch (decorationType)
		{
		case DecorationType.Dungeon1:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.FARM_PLOT_SIGN,
				StructureBrain.TYPES.DECORATION_BARROW,
				StructureBrain.TYPES.DECORATION_WALL_STONE,
				StructureBrain.TYPES.DECORATION_TORCH_BIG,
				StructureBrain.TYPES.DECORATION_HAY_BALE,
				StructureBrain.TYPES.DECORATION_HAY_PILE,
				StructureBrain.TYPES.DECORATION_STONE_FLAG,
				StructureBrain.TYPES.DECORATION_BOSS_TROPHY_1
			};
		case DecorationType.Mushroom:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.DECORATION_MUSHROOM_1,
				StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_1,
				StructureBrain.TYPES.DECORATION_MUSHROOM_SCULPTURE,
				StructureBrain.TYPES.DECORATION_FLAG_MUSHROOM,
				StructureBrain.TYPES.DECORATION_STONE_MUSHROOM,
				StructureBrain.TYPES.DECORATION_PUMPKIN_PILE,
				StructureBrain.TYPES.DECORATION_PUMPKIN_STOOL,
				StructureBrain.TYPES.DECORATION_BOSS_TROPHY_2
			};
		case DecorationType.Crystal:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.DECORATION_CRYSTAL_LAMP,
				StructureBrain.TYPES.DECORATION_CRYSTAL_LIGHT,
				StructureBrain.TYPES.DECORATION_CRYSTAL_ROCK,
				StructureBrain.TYPES.DECORATION_CRYSTAL_STATUE,
				StructureBrain.TYPES.DECORATION_CRYSTAL_WINDOW,
				StructureBrain.TYPES.DECORATION_FLAG_CRYSTAL,
				StructureBrain.TYPES.DECORATION_BOSS_TROPHY_3,
				StructureBrain.TYPES.DECORATION_CRYSTAL_TREE
			};
		case DecorationType.Spider:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.DECORATION_WALL_SPIDER,
				StructureBrain.TYPES.DECORATION_BONE_SKULL_BIG,
				StructureBrain.TYPES.DECORATION_BONE_SKULL_PILE,
				StructureBrain.TYPES.DECORATION_SPIDER_LANTERN,
				StructureBrain.TYPES.DECORATION_SPIDER_PILLAR,
				StructureBrain.TYPES.DECORATION_SPIDER_TORCH,
				StructureBrain.TYPES.DECORATION_SPIDER_WEB_CROWN_SCULPTURE,
				StructureBrain.TYPES.DECORATION_BOSS_TROPHY_4
			};
		case DecorationType.Path:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.TILE_PATH,
				StructureBrain.TYPES.TILE_HAY,
				StructureBrain.TYPES.TILE_BLOOD,
				StructureBrain.TYPES.TILE_ROCKS,
				StructureBrain.TYPES.TILE_BRICKS,
				StructureBrain.TYPES.TILE_PLANKS,
				StructureBrain.TYPES.TILE_GOLD,
				StructureBrain.TYPES.TILE_MOSAIC,
				StructureBrain.TYPES.TILE_FLOWERSROCKY
			};
		case DecorationType.All:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.DECORATION_TWIG_LAMP,
				StructureBrain.TYPES.DECORATION_FOUNTAIN,
				StructureBrain.TYPES.DECORATION_WREATH_STICK,
				StructureBrain.TYPES.DECORATION_STUMP_LAMB_STATUE,
				StructureBrain.TYPES.DECORATION_STONE_HENGE,
				StructureBrain.TYPES.DECORATION_POND,
				StructureBrain.TYPES.DECORATION_LEAFY_FLOWER_SCULPTURE,
				StructureBrain.TYPES.DECORATION_LEAFY_LANTERN,
				StructureBrain.TYPES.DECORATION_LAMB_FLAG_STATUE,
				StructureBrain.TYPES.DECORATION_STONE_CANDLE,
				StructureBrain.TYPES.DECORATION_CANDLE_BARREL,
				StructureBrain.TYPES.DECORATION_FLOWER_ARCH,
				StructureBrain.TYPES.DECORATION_SMALL_STONE_CANDLE,
				StructureBrain.TYPES.DECORATION_FLOWER_BOX_1,
				StructureBrain.TYPES.DECORATION_BONE_ARCH,
				StructureBrain.TYPES.DECORATION_BONE_BARREL,
				StructureBrain.TYPES.DECORATION_BONE_CANDLE,
				StructureBrain.TYPES.DECORATION_BONE_FLAG,
				StructureBrain.TYPES.DECORATION_BONE_LANTERN,
				StructureBrain.TYPES.DECORATION_BONE_PILLAR,
				StructureBrain.TYPES.DECORATION_BONE_SCULPTURE,
				StructureBrain.TYPES.DECORATION_WALL_TWIGS,
				StructureBrain.TYPES.DECORATION_WALL_GRASS,
				StructureBrain.TYPES.DECORATION_TREE,
				StructureBrain.TYPES.DECORATION_STONE,
				StructureBrain.TYPES.DECORATION_FLAG_CROWN,
				StructureBrain.TYPES.DECORATION_WALL_BONE,
				StructureBrain.TYPES.DECORATION_MONSTERSHRINE,
				StructureBrain.TYPES.DECORATION_BOSS_TROPHY_5,
				StructureBrain.TYPES.DECORATION_VIDEO
			};
		case DecorationType.DLC:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.DECORATION_FLOWERPOTWALL,
				StructureBrain.TYPES.DECORATION_LEAFYLAMPPOST,
				StructureBrain.TYPES.DECORATION_FLOWERVASE,
				StructureBrain.TYPES.DECORATION_WATERINGCAN,
				StructureBrain.TYPES.DECORATION_FLOWER_CART_SMALL,
				StructureBrain.TYPES.DECORATION_WEEPINGSHRINE,
				StructureBrain.TYPES.TILE_FLOWERS,
				StructureBrain.TYPES.DECORATION_OLDFAITH_CRYSTAL,
				StructureBrain.TYPES.DECORATION_OLDFAITH_FLAG,
				StructureBrain.TYPES.DECORATION_OLDFAITH_FOUNTAIN,
				StructureBrain.TYPES.DECORATION_OLDFAITH_IRONMAIDEN,
				StructureBrain.TYPES.DECORATION_OLDFAITH_SHRINE,
				StructureBrain.TYPES.DECORATION_OLDFAITH_TORCH,
				StructureBrain.TYPES.DECORATION_OLDFAITH_WALL,
				StructureBrain.TYPES.TILE_OLDFAITH,
				StructureBrain.TYPES.DECORATION_TWITCH_FLAG_CROWN,
				StructureBrain.TYPES.DECORATION_TWITCH_MUSHROOM_BAG,
				StructureBrain.TYPES.DECORATION_TWITCH_ROSE_BUSH,
				StructureBrain.TYPES.DECORATION_TWITCH_STONE_FLAG,
				StructureBrain.TYPES.DECORATION_TWITCH_STONE_STATUE,
				StructureBrain.TYPES.DECORATION_TWITCH_WOODEN_GUARDIAN,
				StructureBrain.TYPES.DECORATION_PLUSH
			};
		case DecorationType.Special_Events:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.DECORATION_HALLOWEEN_CANDLE,
				StructureBrain.TYPES.DECORATION_HALLOWEEN_SKULL,
				StructureBrain.TYPES.DECORATION_HALLOWEEN_TREE,
				StructureBrain.TYPES.DECORATION_HALLOWEEN_PUMPKIN
			};
		default:
			return null;
		}
	}

	public static StructureBrain.TYPES GetRandomLockedDecoration()
	{
		List<StructureBrain.TYPES> list = new List<StructureBrain.TYPES>();
		foreach (StructureBrain.TYPES allStructure in StructuresData.AllStructures)
		{
			if (StructuresData.GetCategory(allStructure) == StructureBrain.Categories.AESTHETIC && !StructuresData.GetUnlocked(allStructure) && !OnDecorationBlacklist(allStructure))
			{
				list.Add(allStructure);
			}
		}
		if (list.Count > 0)
		{
			return list[UnityEngine.Random.Range(0, list.Count - 1)];
		}
		return StructureBrain.TYPES.NONE;
	}

	public static bool CheckIfThereAreSkinsAvailable()
	{
		List<string> list = new List<string>();
		foreach (WorshipperData.SkinAndData item in WorshipperData.Instance.GetSkinsFromLocation(WorshipperData.DropLocation.Other))
		{
			if (!item.Skin[0].Skin.Contains("Boss") && !OnBlackList(item.Skin[0].Skin) && !GetFollowerSkinUnlocked(item.Skin[0].Skin))
			{
				list.Add(item.Skin[0].Skin);
			}
		}
		foreach (WorshipperData.SkinAndData item2 in WorshipperData.Instance.GetSkinsFromFollowerLocation(PlayerFarming.Location))
		{
			if (!item2.Skin[0].Skin.Contains("Boss") && !OnBlackList(item2.Skin[0].Skin) && !GetFollowerSkinUnlocked(item2.Skin[0].Skin))
			{
				list.Add(item2.Skin[0].Skin);
			}
		}
		if (list.Count > 0)
		{
			return true;
		}
		return false;
	}

	public static bool CheckIfThereAreSkinsAvailableAll()
	{
		List<string> list = new List<string>();
		foreach (WorshipperData.SkinAndData item in WorshipperData.Instance.GetSkinsAll())
		{
			if (!GetFollowerSkinUnlocked(item.Skin[0].Skin) && !list.Contains(item.Skin[0].Skin) && !instance.DLCSkins.Contains(item.Skin[0].Skin) && !instance.SpecialEventSkins.Contains(item.Skin[0].Skin))
			{
				list.Add(item.Skin[0].Skin);
			}
		}
		if (list.Count > 0)
		{
			return true;
		}
		return false;
	}

	public static List<string> AvailableSkins()
	{
		List<string> list = new List<string>();
		foreach (WorshipperData.SkinAndData character in WorshipperData.Instance.Characters)
		{
			if (!OnBlackList(character.Title))
			{
				list.Add(character.Title);
			}
		}
		return list;
	}

	public static bool OnBlackList(string skin)
	{
		if (skin.Contains("Boss"))
		{
			return true;
		}
		foreach (string item in Instance.FollowerSkinsBlacklist)
		{
			if (item == skin)
			{
				return true;
			}
		}
		return false;
	}

	public static string GetRandomUnlockedSkin()
	{
		List<string> list = new List<string>();
		foreach (string item in Instance.FollowerSkinsUnlocked)
		{
			if (!item.Contains("Boss"))
			{
				list.Add(item);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public static string GetRandomSkin()
	{
		List<string> list = new List<string>();
		foreach (WorshipperData.SkinAndData item in WorshipperData.Instance.GetSkinsFromFollowerLocation(PlayerFarming.Location))
		{
			if (!item.Skin[0].Skin.Contains("Boss") && !OnBlackList(item.Skin[0].Skin))
			{
				list.Add(item.Skin[0].Skin);
			}
		}
		if (list.Count == 0)
		{
			foreach (WorshipperData.SkinAndData item2 in WorshipperData.Instance.GetSkinsFromLocation(WorshipperData.DropLocation.Other))
			{
				if (!item2.Skin[0].Skin.Contains("Boss") && !OnBlackList(item2.Skin[0].Skin))
				{
					list.Add(item2.Skin[0].Skin);
				}
			}
		}
		if (list.Count > 0)
		{
			return list[UnityEngine.Random.Range(0, list.Count)];
		}
		Debug.Log("No Skins Found!");
		return "";
	}

	public static string GetRandomLockedSkin()
	{
		List<string> list = new List<string>();
		foreach (WorshipperData.SkinAndData item in WorshipperData.Instance.GetSkinsFromFollowerLocation(PlayerFarming.Location))
		{
			if (!item.Skin[0].Skin.Contains("Boss") && !OnBlackList(item.Skin[0].Skin) && !GetFollowerSkinUnlocked(item.Skin[0].Skin))
			{
				list.Add(item.Skin[0].Skin);
			}
		}
		if (list.Count == 0)
		{
			foreach (WorshipperData.SkinAndData item2 in WorshipperData.Instance.GetSkinsFromLocation(WorshipperData.DropLocation.Other))
			{
				if (!item2.Skin[0].Skin.Contains("Boss") && !OnBlackList(item2.Skin[0].Skin) && !GetFollowerSkinUnlocked(item2.Skin[0].Skin))
				{
					list.Add(item2.Skin[0].Skin);
				}
			}
		}
		if (list.Count > 0)
		{
			return list[UnityEngine.Random.Range(0, list.Count)];
		}
		Debug.Log("No Skins Found!");
		return "";
	}

	public static bool GetFollowerSkinUnlocked(string skinName)
	{
		if (Instance.FollowerSkinsUnlocked.Contains(skinName))
		{
			return true;
		}
		return false;
	}

	public static void SetFollowerSkinUnlocked(string skinName)
	{
		if (!skinName.Contains("Boss") && !skinName.Contains("CultLeader"))
		{
			skinName = skinName.StripNumbers();
		}
		if (!Instance.FollowerSkinsUnlocked.Contains(skinName))
		{
			Instance.FollowerSkinsUnlocked.Add(skinName);
			Action<string> onSkinUnlocked = OnSkinUnlocked;
			if (onSkinUnlocked != null)
			{
				onSkinUnlocked(skinName);
			}
			if (!CheckIfThereAreSkinsAvailableAll())
			{
				Debug.Log("Follower Skin Achievement Unlocked");
				AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("ALL_SKINS_UNLOCKED"));
			}
		}
	}

	public static void RemoveUnlockedFollowerSkin(string skinName)
	{
		if (Instance.FollowerSkinsUnlocked.Contains(skinName))
		{
			Instance.FollowerSkinsUnlocked.Remove(skinName);
		}
	}

	public static void UnlockAllDecorations()
	{
		foreach (StructureBrain.TYPES allStructure in StructuresData.AllStructures)
		{
			if (StructuresData.GetCategory(allStructure) == StructureBrain.Categories.AESTHETIC && !StructuresData.GetUnlocked(allStructure))
			{
				StructuresData.CompleteResearch(allStructure);
				StructuresData.SetRevealed(allStructure);
			}
		}
	}

	public void AddKilledBoss(string BossSkin)
	{
		if (!KilledBosses.Contains(BossSkin))
		{
			KilledBosses.Add(BossSkin);
		}
	}

	public bool CheckKilledBosses(string BossSkin)
	{
		return KilledBosses.Contains(BossSkin);
	}

	public void AddWeapon(EquipmentType weapon)
	{
		if (!WeaponPool.Contains(weapon))
		{
			WeaponPool.Add(weapon);
			Action<EquipmentType> onWeaponUnlocked = OnWeaponUnlocked;
			if (onWeaponUnlocked != null)
			{
				onWeaponUnlocked(weapon);
			}
		}
	}

	public EquipmentType GetRandomWeaponInPool()
	{
		if (GameManager.CurrentDungeonFloor <= 1 && Instance.dungeonRun >= 2 && !DungeonSandboxManager.Active)
		{
			if (!Instance.WeaponPool.Contains(EquipmentType.Axe))
			{
				PlayerWeapon.FirstTimeUsingWeapon = true;
				return EquipmentType.Axe;
			}
			if (!Instance.WeaponPool.Contains(EquipmentType.Dagger))
			{
				PlayerWeapon.FirstTimeUsingWeapon = true;
				return EquipmentType.Dagger;
			}
			if (!Instance.WeaponPool.Contains(EquipmentType.Hammer) && Instance.BossesCompleted.Count >= 2)
			{
				PlayerWeapon.FirstTimeUsingWeapon = true;
				return EquipmentType.Hammer;
			}
			if (!Instance.WeaponPool.Contains(EquipmentType.Gauntlet) && Instance.BossesCompleted.Count >= 1)
			{
				PlayerWeapon.FirstTimeUsingWeapon = true;
				return EquipmentType.Gauntlet;
			}
		}
		List<EquipmentType> list = new List<EquipmentType>(Instance.WeaponPool);
		if (list.Count <= 1)
		{
			return list[0];
		}
		if (list.Contains(instance.CurrentWeapon))
		{
			list.Remove(instance.CurrentWeapon);
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (EquipmentManager.GetWeaponData(list[num]) != null)
			{
				EquipmentType primaryEquipmentType = EquipmentManager.GetWeaponData(list[num]).PrimaryEquipmentType;
				if (!Instance.WeaponPool.Contains(primaryEquipmentType))
				{
					list.Remove(list[num]);
				}
				else if ((bool)EquipmentManager.GetWeaponData(list[num]).GetAttachment(AttachmentEffect.Fervour) && (DungeonSandboxManager.Active || (!GameManager.HasUnlockAvailable() && !Instance.DeathCatBeaten)))
				{
					list.Remove(list[num]);
				}
			}
			else
			{
				list.Remove(list[num]);
			}
		}
		float num2 = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			float weight = EquipmentManager.GetWeaponData(list[i]).Weight;
			if (float.IsPositiveInfinity(weight))
			{
				return list[i];
			}
			if (weight >= 0f && !float.IsNaN(weight))
			{
				num2 += weight;
			}
		}
		float value = UnityEngine.Random.value;
		float num3 = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			float weight = EquipmentManager.GetWeaponData(list[i]).Weight;
			if (!float.IsNaN(weight) && !(weight <= 0f))
			{
				num3 += weight / (float)list.Count;
				if (num3 >= value)
				{
					return list[i];
				}
			}
		}
		return GetRandomWeaponInPool();
	}

	public void AddCurse(EquipmentType curse)
	{
		if (!CursePool.Contains(curse))
		{
			CursePool.Add(curse);
		}
	}

	public EquipmentType GetRandomCurseInPool()
	{
		if (GameManager.CurrentDungeonFloor <= 1 && Instance.dungeonRun >= 2 && !DungeonSandboxManager.Active)
		{
			if (!Instance.CursePool.Contains(EquipmentType.Tentacles))
			{
				return EquipmentType.Tentacles;
			}
			if (!Instance.CursePool.Contains(EquipmentType.EnemyBlast))
			{
				return EquipmentType.EnemyBlast;
			}
			if (!Instance.CursePool.Contains(EquipmentType.ProjectileAOE))
			{
				return EquipmentType.ProjectileAOE;
			}
			if (!Instance.CursePool.Contains(EquipmentType.MegaSlash))
			{
				return EquipmentType.MegaSlash;
			}
		}
		List<EquipmentType> list = new List<EquipmentType>(Instance.CursePool);
		if (list.Count <= 1)
		{
			return list[0];
		}
		if (list.Contains(instance.CurrentCurse))
		{
			list.Remove(instance.CurrentCurse);
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (EquipmentManager.GetCurseData(list[num]) != null)
			{
				EquipmentType primaryEquipmentType = EquipmentManager.GetCurseData(list[num]).PrimaryEquipmentType;
				if (!Instance.CursePool.Contains(primaryEquipmentType))
				{
					list.Remove(list[num]);
				}
			}
			else
			{
				list.Remove(list[num]);
			}
		}
		float num2 = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			float weight = EquipmentManager.GetCurseData(list[i]).Weight;
			if (float.IsPositiveInfinity(weight))
			{
				return list[i];
			}
			if (weight >= 0f && !float.IsNaN(weight))
			{
				num2 += weight;
			}
		}
		float value = UnityEngine.Random.value;
		float num3 = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			float weight = EquipmentManager.GetCurseData(list[i]).Weight;
			if (float.IsNaN(weight) || weight <= 0f)
			{
				continue;
			}
			num3 += weight / (float)list.Count;
			if (num3 >= value)
			{
				EquipmentType equipmentType = list[i];
				if (PlayerFleeceManager.FleeceSwapsWeaponForCurse() && equipmentType == EquipmentType.ProjectileAOE_GoopTrail)
				{
					equipmentType = EquipmentType.Fireball;
				}
				return equipmentType;
			}
		}
		return GetRandomCurseInPool();
	}

	public static void DungeonCompletedWithFleece(FollowerLocation location, int fleece)
	{
		foreach (DungeonCompletedFleeces dungeonsCompletedWithFleece in Instance.DungeonsCompletedWithFleeces)
		{
			if (dungeonsCompletedWithFleece.Location == location)
			{
				if (!dungeonsCompletedWithFleece.Fleeces.Contains(fleece))
				{
					dungeonsCompletedWithFleece.Fleeces.Add(fleece);
				}
				return;
			}
		}
		instance.DungeonsCompletedWithFleeces.Add(new DungeonCompletedFleeces
		{
			Location = location,
			Fleeces = new List<int>(1) { fleece }
		});
	}

	public static bool UnlockTrinket(TarotCards.Card card)
	{
		if (!Instance.PlayerFoundTrinkets.Contains(card))
		{
			Debug.Log("Collected Tarots: " + Instance.PlayerFoundTrinkets.Count + "Total Tarot: " + AllTrinkets.Count);
			Instance.PlayerFoundTrinkets.Add(card);
			if (Instance.PlayerFoundTrinkets.Count >= AllTrinkets.Count)
			{
				AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("ALL_TAROTS_UNLOCKED"));
			}
			return true;
		}
		return false;
	}

	public static bool TrinketUnlocked(TarotCards.Card card)
	{
		if (Instance.PlayerFoundTrinkets.Contains(card))
		{
			return true;
		}
		return false;
	}

	public static int GetTrinketsUnlocked()
	{
		int num = 0;
		foreach (TarotCards.Card allTrinket in AllTrinkets)
		{
			if (TrinketUnlocked(allTrinket))
			{
				num++;
			}
		}
		return num;
	}

	public static float GetWeaponDamageMultiplier(int level)
	{
		return 0.13f + 0.07f * (float)level;
	}

	public static float GetWeaponAttackRateMultiplier(int level)
	{
		return 0.3f * (float)level;
	}

	public static void UnlockFullyUpgradedWeapon()
	{
	}

	public static void UnlockFullyUpgradedCurse()
	{
	}

	public static void IncreaseCurseLevel(TarotCards.Card curse)
	{
		switch (curse)
		{
		case TarotCards.Card.Fireball:
			instance.FireballLevel++;
			if (instance.FireballLevel >= 3)
			{
				UnlockFullyUpgradedCurse();
			}
			break;
		case TarotCards.Card.EnemyBlast:
			instance.EnemyBlastLevel++;
			if (instance.EnemyBlastLevel >= 3)
			{
				UnlockFullyUpgradedCurse();
			}
			break;
		case TarotCards.Card.Tentacles:
			instance.TentaclesLevel++;
			if (instance.TentaclesLevel >= 3)
			{
				UnlockFullyUpgradedCurse();
			}
			break;
		case TarotCards.Card.ProjectileAOE:
			instance.ProjectileAOELevel++;
			if (instance.ProjectileAOELevel >= 3)
			{
				UnlockFullyUpgradedCurse();
			}
			break;
		case TarotCards.Card.Vortex:
			instance.VortexLevel++;
			if (instance.VortexLevel >= 3)
			{
				UnlockFullyUpgradedCurse();
			}
			break;
		case TarotCards.Card.MegaSlash:
			instance.MegaSlashLevel++;
			if (instance.MegaSlashLevel >= 3)
			{
				UnlockFullyUpgradedCurse();
			}
			break;
		case TarotCards.Card.Tripleshot:
			break;
		}
	}

	public static string GetCurseUpgradeDescription(TarotCards.Card curse, int upgradeLevel)
	{
		return LocalizationManager.GetTranslation(string.Format("UpgradeSystem/{0}/Upgrade/{1}", curse.ToString(), upgradeLevel));
	}

	public bool HasBuiltDecoration(StructureBrain.TYPES structureType)
	{
		return DecorationTypesBuilt.Contains((int)structureType);
	}

	public void SetBuiltDecoration(StructureBrain.TYPES structureType)
	{
		if (!HasBuiltDecoration(structureType))
		{
			DecorationTypesBuilt.Add((int)structureType);
		}
	}

	public MissionManager.Mission GetMission(MissionManager.MissionType missionType, int id)
	{
		foreach (MissionManager.Mission activeMission in ActiveMissions)
		{
			if (activeMission.MissionType == missionType && activeMission.ID == id)
			{
				return activeMission;
			}
		}
		return null;
	}

	public bool ContainsMissionID(MissionManager.MissionType missionType, int id)
	{
		return GetMission(missionType, id) != null;
	}

	public void UpdateShop(ShopLocationTracker shop)
	{
		for (int i = 0; i < Shops.Count; i++)
		{
			if (Shops[i].location == shop.location)
			{
				Shops[i] = shop;
				break;
			}
		}
	}

	public bool CheckShopExists(FollowerLocation location, string ShopKeeperName)
	{
		foreach (ShopLocationTracker shop in Shops)
		{
			if (shop.location == location && shop.shopKeeperName == ShopKeeperName)
			{
				return true;
			}
		}
		return false;
	}

	public ShopLocationTracker GetShop(FollowerLocation location, string ShopKeeperName)
	{
		foreach (ShopLocationTracker shop in Shops)
		{
			if (shop.location == location && shop.shopKeeperName == ShopKeeperName)
			{
				return shop;
			}
		}
		return null;
	}

	public bool CheckInvestmentExist()
	{
		if (Investment != null)
		{
			return true;
		}
		return false;
	}

	public void CreateInvestment(JellyFishInvestment _Investment)
	{
		Investment = _Investment;
	}

	public TraderTracker ReturnTrader(FollowerLocation location)
	{
		foreach (TraderTracker trader in Traders)
		{
			if (trader.location == location)
			{
				return trader;
			}
		}
		Debug.Log("Couldn't find Trader");
		return null;
	}

	public void SetTrader(TraderTracker trader)
	{
		int num = 0;
		using (List<TraderTracker>.Enumerator enumerator = Traders.GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current.location != trader.location)
			{
				num++;
			}
		}
		Traders[num] = trader;
	}

	public ItemSelector.Category GetItemSelectorCategory(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return null;
		}
		foreach (ItemSelector.Category itemSelectorCategory in ItemSelectorCategories)
		{
			if (itemSelectorCategory.Key == key)
			{
				return itemSelectorCategory;
			}
		}
		ItemSelector.Category category = new ItemSelector.Category
		{
			Key = key
		};
		ItemSelectorCategories.Add(category);
		return category;
	}

	public static int GetTargetXP(int index)
	{
		return Mathf.FloorToInt((float)TargetXP[index] * AllUnlockedMultiplier);
	}

	public DataManager()
	{
		instance = this;
	}

	public static void ResetData()
	{
		instance = new DataManager();
		instance.PlayerBluePrints = new List<BluePrint>();
		instance.MetaData = MetaData.Default(instance);
	}

	public void AddEnemyKilled(Enemy enemy)
	{
		for (int i = 0; i < EnemiesKilled.Count; i++)
		{
			if (EnemiesKilled[i].EnemyType == enemy)
			{
				EnemiesKilled[i].AmountKilled++;
				return;
			}
		}
		EnemiesKilled.Add(new EnemyData
		{
			EnemyType = enemy
		});
	}

	public int GetEnemiesKilled(Enemy enemy)
	{
		for (int i = 0; i < EnemiesKilled.Count; i++)
		{
			if (EnemiesKilled[i].EnemyType == enemy)
			{
				return EnemiesKilled[i].AmountKilled;
			}
		}
		return 0;
	}

	public float GetLuckMultiplier()
	{
		float num = LuckMultiplier * DifficultyManager.GetLuckMultiplier() / 10f;
		return 1f - num;
	}

	public bool DungeonCompleted(FollowerLocation location, bool layer2 = false)
	{
		bool flag = BossesCompleted.Contains(location);
		if (layer2)
		{
			DungeonCompleted(location);
			switch (location)
			{
			case FollowerLocation.Dungeon1_2:
				if (flag)
				{
					return instance.BeatenHeketLayer2;
				}
				return false;
			case FollowerLocation.Dungeon1_3:
				if (flag)
				{
					return instance.BeatenKallamarLayer2;
				}
				return false;
			case FollowerLocation.Dungeon1_4:
				if (flag)
				{
					return instance.BeatenShamuraLayer2;
				}
				return false;
			default:
				if (flag)
				{
					return instance.BeatenLeshyLayer2;
				}
				return false;
			}
		}
		return flag;
	}

	public int GetDungeonLayer(FollowerLocation location)
	{
		switch (location)
		{
		case FollowerLocation.Dungeon1_1:
			return Dungeon1_Layer;
		case FollowerLocation.Dungeon1_2:
			return Dungeon2_Layer;
		case FollowerLocation.Dungeon1_3:
			return Dungeon3_Layer;
		case FollowerLocation.Dungeon1_4:
			return Dungeon4_Layer;
		default:
			return 0;
		}
	}

	public FollowerLocation GetCurrentDungeon()
	{
		if (!BossesCompleted.Contains(FollowerLocation.Dungeon1_1))
		{
			return FollowerLocation.Dungeon1_1;
		}
		if (!BossesCompleted.Contains(FollowerLocation.Dungeon1_2))
		{
			return FollowerLocation.Dungeon1_2;
		}
		if (!BossesCompleted.Contains(FollowerLocation.Dungeon1_3))
		{
			return FollowerLocation.Dungeon1_3;
		}
		if (!BossesCompleted.Contains(FollowerLocation.Dungeon1_4))
		{
			return FollowerLocation.Dungeon1_4;
		}
		return FollowerLocation.Dungeon1_1;
	}

	public int GetDungeonNumber(FollowerLocation location)
	{
		switch (location)
		{
		case FollowerLocation.Dungeon1_1:
			return 1;
		case FollowerLocation.Dungeon1_2:
			return 2;
		case FollowerLocation.Dungeon1_3:
			return 3;
		case FollowerLocation.Dungeon1_4:
		case FollowerLocation.Dungeon1_5:
			return 4;
		default:
			return 1;
		}
	}

	public void AddDungeonLayer(FollowerLocation location)
	{
		switch (location)
		{
		case FollowerLocation.Dungeon1_1:
			Dungeon1_Layer++;
			break;
		case FollowerLocation.Dungeon1_2:
			Dungeon2_Layer++;
			break;
		case FollowerLocation.Dungeon1_3:
			Dungeon3_Layer++;
			break;
		case FollowerLocation.Dungeon1_4:
			Dungeon4_Layer++;
			break;
		}
	}

	public void SetDungeonLayer(FollowerLocation location, int layer)
	{
		switch (location)
		{
		case FollowerLocation.Dungeon1_1:
			Dungeon1_Layer = layer;
			break;
		case FollowerLocation.Dungeon1_2:
			Dungeon2_Layer = layer;
			break;
		case FollowerLocation.Dungeon1_3:
			Dungeon3_Layer = layer;
			break;
		case FollowerLocation.Dungeon1_4:
			Dungeon4_Layer = layer;
			break;
		}
	}

	public void AddToNotificationHistory(FinalizedNotification finalizedNotification)
	{
		if (NotificationHistory.Count > 20)
		{
			NotificationHistory.RemoveAt(NotificationHistory.Count - 1);
		}
		NotificationHistory.Insert(0, finalizedNotification);
	}

	public static int GetGodTearNotches(FollowerLocation location)
	{
		switch (location)
		{
		case FollowerLocation.Dungeon1_1:
			return Instance.Dungeon1GodTears;
		case FollowerLocation.Dungeon1_2:
			return Instance.Dungeon2GodTears;
		case FollowerLocation.Dungeon1_3:
			return Instance.Dungeon3GodTears;
		case FollowerLocation.Dungeon1_4:
			return Instance.Dungeon4GodTears;
		default:
			return 0;
		}
	}

	public static int GetGodTearNotchesTotal()
	{
		return Instance.Dungeon1GodTears + Instance.Dungeon2GodTears + Instance.Dungeon3GodTears + Instance.Dungeon4GodTears;
	}

	public static bool ActivateCultistDLC()
	{
		if (!Instance.DLC_Cultist_Pack)
		{
			Instance.DLC_Cultist_Pack = true;
			for (int i = 0; i < CultistDLCSkins.Count; i++)
			{
				SetFollowerSkinUnlocked(CultistDLCSkins[i]);
			}
			for (int j = 0; j < CultistDLCStructures.Count; j++)
			{
				StructuresData.CompleteResearch(CultistDLCStructures[j]);
			}
			return true;
		}
		return false;
	}

	public static void DeactivateCultistDLC()
	{
		Instance.DLC_Cultist_Pack = false;
		for (int i = 0; i < CultistDLCSkins.Count; i++)
		{
			RemoveUnlockedFollowerSkin(CultistDLCSkins[i]);
		}
		for (int j = 0; j < CultistDLCStructures.Count; j++)
		{
			Instance.UnlockedStructures.Remove(CultistDLCStructures[j]);
		}
	}

	public static bool ActivateHereticDLC()
	{
		if (!Instance.DLC_Heretic_Pack)
		{
			Instance.DLC_Heretic_Pack = true;
			for (int i = 0; i < HereticDLCSkins.Count; i++)
			{
				SetFollowerSkinUnlocked(HereticDLCSkins[i]);
			}
			for (int j = 0; j < HereticDLCStructures.Count; j++)
			{
				StructuresData.CompleteResearch(HereticDLCStructures[j]);
			}
			if (!instance.UnlockedFleeces.Contains(999))
			{
				Instance.UnlockedFleeces.Add(999);
			}
			return true;
		}
		return false;
	}

	public static void DeactivateHereticDLC()
	{
		Instance.DLC_Heretic_Pack = false;
		for (int i = 0; i < HereticDLCSkins.Count; i++)
		{
			RemoveUnlockedFollowerSkin(HereticDLCSkins[i]);
		}
		for (int j = 0; j < HereticDLCStructures.Count; j++)
		{
			Instance.UnlockedStructures.Remove(HereticDLCStructures[j]);
		}
		if (instance.UnlockedFleeces.Contains(999))
		{
			Instance.UnlockedFleeces.Remove(999);
		}
		if (instance.PlayerFleece != 999)
		{
			return;
		}
		instance.PlayerFleece = 0;
		if (PlayerFarming.Instance != null)
		{
			SimpleSpineAnimator simpleSpineAnimator = PlayerFarming.Instance.simpleSpineAnimator;
			if ((object)simpleSpineAnimator != null)
			{
				simpleSpineAnimator.SetSkin("Lamb_0");
			}
		}
	}

	public static bool ActivatePrePurchaseDLC()
	{
		if (!Instance.DLC_Pre_Purchase)
		{
			Instance.DLC_Pre_Purchase = true;
			SetFollowerSkinUnlocked("Cthulhu");
			return true;
		}
		return false;
	}

	public static void DeactivatePrePurchaseDLC()
	{
		Instance.DLC_Pre_Purchase = false;
		RemoveUnlockedFollowerSkin("Cthulhu");
	}

	public static bool ActivatePlushBonusDLC()
	{
		if (!Instance.DLC_Plush_Bonus)
		{
			Instance.DLC_Plush_Bonus = true;
			StructuresData.CompleteResearch(StructureBrain.TYPES.DECORATION_PLUSH);
			return true;
		}
		return false;
	}

	public static bool ActivatePAXDLC()
	{
		if (!Instance.PAX_DLC)
		{
			Instance.PAX_DLC = true;
			SetFollowerSkinUnlocked("StarBunny");
			return true;
		}
		return false;
	}

	public static bool ActivateTwitchDrop1()
	{
		if (!Instance.Twitch_Drop_1)
		{
			Instance.Twitch_Drop_1 = true;
			SetFollowerSkinUnlocked("TwitchPoggers");
			return true;
		}
		return false;
	}

	public static bool ActivateTwitchDrop2()
	{
		if (!Instance.Twitch_Drop_2)
		{
			Instance.Twitch_Drop_2 = true;
			SetFollowerSkinUnlocked("TwitchDog");
			return true;
		}
		return false;
	}

	public static bool ActivateTwitchDrop3()
	{
		if (!Instance.Twitch_Drop_3)
		{
			Instance.Twitch_Drop_3 = true;
			SetFollowerSkinUnlocked("TwitchDogAlt");
			return true;
		}
		return false;
	}

	public static bool ActivateTwitchDrop4()
	{
		if (!Instance.Twitch_Drop_4)
		{
			Instance.Twitch_Drop_4 = true;
			SetFollowerSkinUnlocked("Lion");
			return true;
		}
		return false;
	}

	public static bool ActivateTwitchDrop5()
	{
		if (!Instance.Twitch_Drop_5)
		{
			Instance.Twitch_Drop_5 = true;
			SetFollowerSkinUnlocked("Penguin");
			return true;
		}
		return false;
	}

	public static bool ActivateTwitchDrop6()
	{
		if (!Instance.Twitch_Drop_6)
		{
			Instance.Twitch_Drop_6 = true;
			SetFollowerSkinUnlocked("Kiwi");
			return true;
		}
		return false;
	}

	public static bool ActivateTwitchDrop7()
	{
		if (!Instance.Twitch_Drop_7)
		{
			Instance.Twitch_Drop_7 = true;
			SetFollowerSkinUnlocked("Pelican");
			return true;
		}
		return false;
	}

	public static void UnlockRelic(RelicType relicType)
	{
		if (!instance.PlayerFoundRelics.Contains(relicType))
		{
			instance.PlayerFoundRelics.Add(relicType);
			if (instance.PlayerFoundRelics.Count >= EquipmentManager.RelicData.Length)
			{
				AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("ALL_RELICS_UNLOCKED"));
				Debug.Log("ACHIEVEMENT GOT : ALL_RELICS_UNLOCKED");
			}
		}
	}

	public bool HasDeathCatFollower()
	{
		foreach (FollowerInfo follower in Instance.Followers)
		{
			if (follower.SkinName == "Boss Death Cat")
			{
				return true;
			}
		}
		return false;
	}
}
