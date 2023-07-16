using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class FontImageNames
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct FollowerCommand
	{
		public const string GiveWorkerCommand = "\uf82c";

		public const string ChangeRole = "\uf021";

		public const string GiveItem = "\uf4bd";

		public const string Talk = "\uf075";

		public const string MakeDemand = "\uf0a1";

		public const string BedRest = "\ue074";

		public const string Murder = "\uf6f5";

		public const string ExtortMoney = "<sprite name=\"icon_Bribe\">";

		public const string Dance = "\uf587";

		public const string DemandDevotion = "<sprite name=\"icon_SoulWhite\">";

		public const string Gift = "\uf06b";

		public const string Imprison = "\uf4b7";

		public const string SendToHospital = "\ue3b2";

		public const string CutTrees = "<sprite name=\"icon_ChopWood\">";

		public const string ForageBerries = "<sprite name=\"icon_berries\">";

		public const string ClearWeeds = "<sprite name=\"icon_grass\">";

		public const string ClearRubble = "<sprite name=\"icon_MineStone\">";

		public const string Undertaker = "<sprite name=\"icon_Undertaker\">";

		public const string TellMeYourProblems = "\uf4ad";

		public const string DemandLoyalty = "''u21";

		public const string Punish = "\uf0e3";

		public const string BeNice = "\uf118";

		public const string Romance = "\uf004";

		public const string PetDog = "\uf6d3";

		public const string WakeUp = "\uf34e";

		public const string LevelUp = "\uf357";

		public const string EatSomething = "\uf623";

		public const string Sleep = "\uf880";

		public const string WorshipAtShrine = "\uf684";

		public const string Build = "\uf6e3";

		public const string NoAvailablePrisons = "\uf4b7";

		public const string Cook = "\uf86b";

		public const string NextPage = "<sprite name=\"img_SwirleyRight\">";

		public const string Meal = "<sprite name=\"icon_Meal\">";

		public const string MealGrass = "<sprite name=\"icon_MealGrass\">";

		public const string MealPoop = "<sprite name=\"icon_MealPoop\">";

		public const string MealGoodFish = "<sprite name=\"icon_MealGoodFish\">";

		public const string MealFollowerMeat = "<sprite name=\"icon_MealFollowerMeat\">";

		public const string MealGreat = "<sprite name=\"icon_MealGreat\">";

		public const string MealMushrooms = "<sprite name=\"icon_MealMushrooms\">";

		public const string MealMeat = "<sprite name=\"icon_MealGood\">";

		public const string MealGreatFish = "<sprite name=\"icon_MealGreatFish\">";

		public const string MealBadFish = "<sprite name=\"icon_MealBadFish\">";

		public const string MealBerries = "<sprite name=\"icon_MealBerries\">";

		public const string MealDeadly = "<sprite name=\"icon_MealDeadly\">";

		public const string MealMediumVeg = "<sprite name=\"icon_MealMediumVeg\">";

		public const string MealBadMeat = "<sprite name=\"icon_MealBadMeat\">";

		public const string MealGreatMeat = "<sprite name=\"icon_MealGreatMeat\">";

		public const string MealMixedLow = "<sprite name=\"icon_MealBadMixed\">";

		public const string MealMixedMedium = "<sprite name=\"icon_MealMediumMixed\">";

		public const string MealMixedHigh = "<sprite name=\"icon_MealGreatMixed\">";

		public const string AreYouSure = "''u3f";

		public const string AreYouSureYes = "<sprite name=\"icon_ThumbsUp\">";

		public const string AreYouSureNo = "<sprite name=\"icon_ThumbsDown\">";

		public const string Study = "\uf02d";

		public const string Intimidate = "\uf556";

		public const string Bribe = "<sprite name=\"icon_Bribe\">";

		public const string Ascend = "\uf102";

		public const string Surveillance = "\ue32e";

		public const string Farmer = "\uf864";

		public const string Janitor = "<sprite name=\"icon_Janitor\">";

		public const string Bless = "\ue05d";

		public const string Reeducate = "\uf19d";

		public const string Refiner = "<sprite name=\"icon_RefineResources\">";

		public const string ViewTraits = "\uf06e";

		public const string GodTear = "<sprite name=\"icon_GodTear\">";
	}

	public const string Seed = "<sprite name=\"icon_seed\">";

	public const string Spirit = "<sprite name=\"icon_spirits\">";

	public const string BlackSoul = "<sprite name=\"icon_blackSoul\">";

	public const string Meat = "<sprite name=\"icon_meat\">";

	public const string Wood = "<sprite name=\"icon_wood\">";

	public const string Stone = "<sprite name=\"icon_stone\">";

	public const string Heart = "<sprite name=\"icon_heart\">";

	public const string Heart_Half = "<sprite name=\"icon_heart_half\">";

	public const string Blue_Heart = "<sprite name=\"icon_blueheart\">";

	public const string Blue_Heart_Half = "<sprite name=\"icon_blueheart_half\">";

	public const string Time_Token = "<sprite name=\"icon_timetoken\">";

	public const string Flowers = "<sprite name=\"icon_flowers\">";

	public const string StainedGlass = "<sprite name=\"icon_stainedglass\">";

	public const string Bones = "<sprite name=\"icon_bones\">";

	public const string BlackGold = "<sprite name=\"icon_blackgold\">";

	public const string Brambles = "<sprite name=\"icon_brambles\">";

	public const string MonsterHeart = "<sprite name=\"icon_monsterHeart\">";

	public const string Berries = "<sprite name=\"icon_berries\">";

	public const string Fish = "<sprite name=\"icon_Fish\">";

	public const string FishSmall = "<sprite name=\"icon_FishSmall\">";

	public const string FishBig = "<sprite name=\"icon_FishBig\">";

	public const string FishBlowfish = "<sprite name=\"icon_FishBlowfish\">";

	public const string FishLobster = "<sprite name=\"icon_FishLobster\">";

	public const string FishSwordfish = "<sprite name=\"icon_FishSwordfish\">";

	public const string FishCrab = "<sprite name=\"icon_FishCrab\">";

	public const string FishSquid = "<sprite name=\"icon_FishSquid\">";

	public const string FishOctopus = "<sprite name=\"icon_FishOctopus\">";

	public const string Mushroom = "<sprite name=\"icon_Mushroom\">";

	public const string Mushroom_Big = "<sprite name=\"icon_Mushroom\">";

	public const string Key = "<sprite name=\"icon_key\">";

	public const string Poop = "<sprite name=\"icon_Poop\">";

	public const string FlowerRed = "<sprite name=\"icon_FlowerRed\">";

	public const string FlowerWhite = "<sprite name=\"icon_FlowerWhite\">";

	public const string FlowerPurple = "<sprite name=\"icon_FlowerPurple\">";

	public const string GoldNugget = "<sprite name=\"icon_GoldNugget\">";

	public const string Rope = "<sprite name=\"icon_Rope\">";

	public const string LogRefined = "<sprite name=\"icon_LogRefined\">";

	public const string StoneRefined = "<sprite name=\"icon_StoneRefined\">";

	public const string GoldRefined = "<sprite name=\"icon_GoldRefined\">";

	public const string Crystal = "<sprite name=\"icon_Crystal\">";

	public const string SpiderWeb = "<sprite name=\"icon_SpiderWeb\">";

	public const string BeholderEye = "<sprite name=\"icon_BeholderEye\">";

	public const string MeatMorsel = "<sprite name=\"icon_MeatMorsel\">";

	public const string DoctrineStone = "<sprite name=\"icon_DoctrineStone\">";

	public const string CrystalDoctrineStone = "<sprite name=\"icon_CrystalDoctrineStone\">";

	public const string Shell = "<sprite name=\"icon_Shell\">";

	public const string Infinity = "<sprite name=\"icon_Infinity\">";

	public const string Ammo = "<sprite name=\"icon_UI_Curse\">";

	public const string ChallengeGold = "<sprite name=\"icon_GodTearShard\">";

	public const string GodTear = "<sprite name=\"icon_GodTear\">";

	public const string Faith = "<sprite name=\"icon_Faith\">";

	public const string Sickness = "<sprite name=\"icon_Sickness\">";

	public const string Hunger = "<sprite name=\"icon_Hunger\">";

	public const string Sleep = "<sprite name=\"icon_Sleep\">";

	public const string ThumbsUp = "<sprite name=\"icon_ThumbsUp\">";

	public const string ThumbsDown = "<sprite name=\"icon_ThumbsDown\">";

	public const string House = "<sprite name=\"icon_House\">";

	public const string Married = "<sprite name=\"icon_Married\">";

	public const string NewIcon = "<sprite name=\"icon_NewIcon\">";

	public const string Dead = "<sprite name=\"icon_Dead\">";

	public const string UI_Curse = "<sprite name=\"icon_UI_Curse\">";

	public const string Janitor = "<sprite name=\"icon_Janitor\">";

	public const string MineStone = "<sprite name=\"icon_MineStone\">";

	public const string ChopWood = "<sprite name=\"icon_ChopWood\">";

	public const string RefineResources = "<sprite name=\"icon_RefineResources\">";

	public const string Bribe = "<sprite name=\"icon_Bribe\">";

	public const string Undertaker = "<sprite name=\"icon_Undertaker\">";

	public const string FaithUp = "<sprite name=\"icon_FaithUp\">";

	public const string FaithDoubleUp = "<sprite name=\"icon_FaithDoubleUp\">";

	public const string FaithDown = "<sprite name=\"icon_FaithDown\">";

	public const string FaithDoubleDown = "<sprite name=\"icon_FaithDoubleDown\">";

	public const string Grass = "<sprite name=\"icon_grass\">";

	public const string Grass2 = "<sprite name=\"icon_grass2\">";

	public const string Grass3 = "<sprite name=\"icon_grass3\">";

	public const string Grass4 = "<sprite name=\"icon_grass4\">";

	public const string Grass5 = "<sprite name=\"icon_grass5\">";

	public const string Meal = "<sprite name=\"icon_Meal\">";

	public const string MealGrass = "<sprite name=\"icon_MealGrass\">";

	public const string MealPoop = "<sprite name=\"icon_MealPoop\">";

	public const string MealGood = "<sprite name=\"icon_MealGood\">";

	public const string MealGoodFish = "<sprite name=\"icon_MealGoodFish\">";

	public const string MealFollowerMeat = "<sprite name=\"icon_MealFollowerMeat\">";

	public const string MealGreat = "<sprite name=\"icon_MealGreat\">";

	public const string MealMeat = "<sprite name=\"icon_MealMeat\">";

	public const string MealMushrooms = "<sprite name=\"icon_MealMushrooms\">";

	public const string MealBadFish = "<sprite name=\"icon_MealBadFish\">";

	public const string MealGreatFish = "<sprite name=\"icon_MealGreatFish\">";

	public const string MealBerries = "<sprite name=\"icon_MealBerries\">";

	public const string MealDeadly = "<sprite name=\"icon_MealDeadly\">";

	public const string MealBadMixed = "<sprite name=\"icon_MealBadMixed\">";

	public const string MealMediumMixed = "<sprite name=\"icon_MealMediumMixed\">";

	public const string MealGreatMixed = "<sprite name=\"icon_MealGreatMixed\">";

	public const string MealMediumVeg = "<sprite name=\"icon_MealMediumVeg\">";

	public const string MealBadMeat = "<sprite name=\"icon_MealBadMeat\">";

	public const string MealGreatMeat = "<sprite name=\"icon_MealGreatMeat\">";

	public const string GoodTrait = "<sprite name=\"icon_GoodTrait\">";

	public const string BadTrait = "<sprite name=\"icon_BadTrait\">";

	public const string Trait_NaturallySkeptical = "<sprite name=\"icon_Trait_NaturallySkeptical\">";

	public const string Trait_NaturallyObedient = "<sprite name=\"icon_Trait_NaturallyObedient\">";

	public const string Trait_DesensitisedToDeath = "<sprite name=\"icon_Trait_DesensitisedToDeath\">";

	public const string Trait_FearOfDeath = "<sprite name=\"icon_Trait_FearOfDeath\">";

	public const string Trait_Cannibal = "<sprite name=\"icon_Trait_Cannibal\">";

	public const string Trait_Disciplinarian = "<sprite name=\"icon_Trait_Disciplinarian\">";

	public const string Trait_SacrificeEnthusiast = "<sprite name=\"icon_Trait_SacrificeEnthusiast\">";

	public const string Trait_AgainstSacrifice = "<sprite name=\"icon_Trait_AgainstSacrifice\">";

	public const string Trait_Faithful = "<sprite name=\"icon_Trait_Faithful\">";

	public const string Trait_Faithless = "<sprite name=\"icon_Trait_Faithless\">";

	public const string Trait_Libertarian = "<sprite name=\"icon_Trait_Libertarian\">";

	public const string Trait_Sickly = "<sprite name=\"icon_Trait_Sickly\">";

	public const string Trait_IronStomach = "<sprite name=\"icon_Trait_IronStomach\">";

	public const string Trait_Zealous = "<sprite name=\"icon_Trait_Zealous\">";

	public const string Trait_Materialistic = "<sprite name=\"icon_Trait_Materialistic\">";

	public const string Trait_Ascetic = "<sprite name=\"icon_Trait_Ascetic\">";

	public const string Trait_Cruel = "<sprite name=\"icon_Trait_Cruel\">";

	public const string Trait_Compassionate = "<sprite name=\"icon_Trait_Compassionate\">";

	public const string Trait_FearOfSickPeople = "<sprite name=\"icon_Trait_FearOfSickPeople\">";

	public const string Trait_LoveOfSickPeople = "<sprite name=\"icon_Trait_LoveOfSickPeople\">";

	public const string Trait_Germophobe = "<sprite name=\"icon_Trait_Germophobe\">";

	public const string Trait_Coprophiliac = "<sprite name=\"icon_Trait_Coprophiliac\">";

	public const string Trait_RespectElders = "<sprite name=\"icon_Trait_RespectElders\">";

	public const string Trait_OldDieYoung = "<sprite name=\"icon_Trait_OldDieYoung\">";

	public const string Trait_MushroomEncouraged = "<sprite name=\"icon_Trait_MushroomEncouraged\">";

	public const string Trait_MushroomBanned = "<sprite name=\"icon_Trait_MushroomBanned\">";

	public const string Trait_ConstructionEnthusiast = "<sprite name=\"icon_Trait_ConstructionEnthusiast\">";

	public const string Trait_SermonEnthusiast = "<sprite name=\"icon_Trait_SermonEnthusiast\">";

	public const string Trait_Industrious = "<sprite name=\"icon_Trait_Industrious\">";

	public const string Trait_Lazy = "<sprite name=\"icon_Trait_Lazy\">";

	public const string Trait_GrassEater = "<sprite name=\"icon_Trait_GrassEater\">";

	public const string Trait_FalseIdols = "<sprite name=\"icon_Trait_FalseIdols\">";

	public const string Trait_Immortal = "<sprite name=\"icon_Trait_Immortal\">";

	public const string Trait_DontStarve = "<sprite name=\"icon_Trait_DontStarve\">";

	public const string DemonShooty = "<sprite name=\"icon_Demon_Shooty\">";

	public const string DemonChomp = "<sprite name=\"icon_Demon_Chomp\">";

	public const string DemonExploder = "<sprite name=\"icon_Demon_Exploder\">";

	public const string DemonArrows = "<sprite name=\"icon_Demon_Arrows\">";

	public const string DemonHearts = "<sprite name=\"icon_Demon_Hearts\">";

	public const string DemonCollector = "<sprite name=\"icon_Demon_Collector\">";

	public const string Pumpkin = "<sprite name=\"icon_Pumpkin\">";

	public const string SeedPumpkin = "<sprite name=\"icon_SeedPumpkin\">";

	public const string SeedFlowerRed = "<sprite name=\"icon_SeedFlowerRed\">";

	public const string SeedFlowerWhite = "<sprite name=\"icon_SeedFlowerWhite\">";

	public const string SeedMushroom = "<sprite name=\"icon_SeedMushroom\">";

	public const string SeedTree = "<sprite name=\"icon_SeedTree\">";

	public const string Beetroot = "<sprite name=\"icon_Beetroot\">";

	public const string SeedBeetroot = "<sprite name=\"icon_SeedBeetroot\">";

	public const string Cauliflower = "<sprite name=\"icon_Cauliflower\">";

	public const string SeedCauliflower = "<sprite name=\"icon_SeedCauliflower\">";

	public const string FollowerMeat = "<sprite name=\"icon_FollowerMeat\">";

	public const string FollowerMeatRotten = "<sprite name=\"icon_FollowerMeatRotten\">";

	public const string Necklace_1 = "<sprite name=\"icon_Necklace1\">";

	public const string Necklace_2 = "<sprite name=\"icon_Necklace2\">";

	public const string Necklace_3 = "<sprite name=\"icon_Necklace3\">";

	public const string Necklace_4 = "<sprite name=\"icon_Necklace4\">";

	public const string Necklace_5 = "<sprite name=\"icon_Necklace5\">";

	public const string Necklace_Dark = "<sprite name=\"icon_Necklace_Dark\">";

	public const string Necklace_Demonic = "<sprite name=\"icon_Necklace_Demonic\">";

	public const string Necklace_Gold_Skull = "<sprite name=\"icon_Necklace_Gold_Skull\">";

	public const string Necklace_Light = "<sprite name=\"icon_Necklace_Light\">";

	public const string Necklace_Missionary = "<sprite name=\"icon_Necklace_Missionary\">";

	public const string Necklace_Loyalty = "<sprite name=\"icon_Necklace_Loyalty\">";

	public const string Gift_Small = "<sprite name=\"icon_GiftSmall\">";

	public const string Gift_Medium = "<sprite name=\"icon_GiftMedium\">";

	public const string RemoveNecklace = "";

	public const string HeartHalfBlue_Blue = "<sprite name=\"icon_HalfBlueHeart_BlueHighlight\">";

	public const string HeartBlue_Blue = "<sprite name=\"icon_BlueHeart_BlueHighlight\">";

	public const string HeartHalfRedBlue = "<sprite name=\"icon_HalfRedHeart_BlueHighlight\">";

	public const string HeartRed_Blue = "<sprite name=\"icon_RedHeart_BlueHighlight\">";

	public const string HeartHalfRed_Red = "<sprite name=\"icon_HalfRedHeart_RedHighlight\">";

	public const string HeartRed_Red = "<sprite name=\"icon_RedHeart_RedHighlight\">";

	public const string HeartHalfBlue_Red = "<sprite name=\"icon_HalfBlueHeart_RedHighlight\">";

	public const string HeartBlue_Red = "<sprite name=\"icon_BlueHeart_RedHighlight\">";

	public const string HeartHalfBlue = "<sprite name=\"icon_HalfBlueHeart\">";

	public const string HeartBlue = "<sprite name=\"icon_BlueHeart\">";

	public const string HeartHalfRed = "<sprite name=\"icon_HalfRedHeart\">";

	public const string HeartRed = "<sprite name=\"icon_RedHeart\">";

	public const string UIHeartEmpty = "<sprite name=\"icon_UIHeartEmpty\">";

	public const string UIHeart = "<sprite name=\"icon_UIHeart\">";

	public const string UIHeartHalf = "<sprite name=\"icon_UIHeartHalf\">";

	public const string UIHeartBlue = "<sprite name=\"icon_UIHeartBlue\">";

	public const string UIHeartBlueHalf = "<sprite name=\"icon_UIHeartBlueHalf\">";

	public const string UIHeartBlack = "<sprite name=\"icon_UIHeartBlack\">";

	public const string UIHeartHalfEmpty = "<sprite name=\"icon_UIHeartHalfFull\">";

	public const string UIHeartHalfFull = "<sprite name=\"icon_UIHeartHalfEmpty\">";

	public const string UIHeartSpiritEmpty = "<sprite name=\"icon_UIHeartSpiritEmpty\">";

	public const string UIHeartSpirit = "<sprite name=\"icon_UIHeartSpirit\">";

	public const string UIHeartHalfSpirit = "<sprite name=\"icon_UIHeartHalfSpirit\">";

	public const string Followers_Utopianist = "<sprite name=\"icon_Utopianist\">";

	public const string Followers_Fundementalist = "<sprite name=\"icon_Fundamentalist\">";

	public const string Followers_Misfit = "<sprite name=\"icon_Misfit\" > ";

	public const string Followers = "<sprite name=\"icon_Followers\">";

	public const string LeftSwirly = "<sprite name=\"img_SwirleyLeft\">";

	public const string RightSwirly = "<sprite name=\"img_SwirleyRight\">";

	public const string TwitchIcon = "<sprite name=\"icon_TwitchIcon\">";

	public static readonly Color FundamentalistColor = new Color(1f, 0.8235294f, 0.003921569f);

	public static readonly Color UtopianistColor = new Color(0f, 0.8078431f, 0.5960785f);

	public static readonly Color MisfitColor = new Color((float)Math.E * 105f / 302f, 0f, 0.1098039f);

	public static readonly Color TextColor = new Color(0.9960784f, 0.9411765f, 0.827451f);

	public static readonly Color TextRedColor = new Color(0.9960784f, 0f, 0f);

	public const string SoulWhite = "<sprite name=\"icon_SoulWhite\">";

	public const string LogsWhite = "<sprite name=\"icon_LogsWhite\">";

	public const string StonesWhite = "<sprite name=\"icon_StonesWhite\">";

	public const string SeedsWhite = "<sprite name=\"icon_SeedsWhite\">";

	public const string CoinsWhite = "<sprite name=\"icon_MoneyWhite\">";

	public const string PoopWhite = "<sprite name=\"icon_PoopWhite\">";

	public const string MealsWhite = "<sprite name=\"icon_MealsWhite\">";

	public const string GrassWhite = "<sprite name=\"icon_GrassWhite\">";

	public const string IngredientsWhite = "<sprite name=\"icon_IngredientsWhite\">";

	public static string GetIconByFaction(Villager_Info.Faction faction)
	{
		switch (faction)
		{
		case Villager_Info.Faction.Fundamentalist:
			return "<sprite name=\"icon_Fundamentalist\">";
		case Villager_Info.Faction.Utopianist:
			return "<sprite name=\"icon_Utopianist\">";
		case Villager_Info.Faction.Misfit:
			return "<sprite name=\"icon_Misfit\" > ";
		default:
			return "";
		}
	}

	public static string GetIconWhiteByType(InventoryItem.ITEM_TYPE type)
	{
		switch (type)
		{
		case InventoryItem.ITEM_TYPE.SOUL:
			return "<sprite name=\"icon_SoulWhite\">";
		case InventoryItem.ITEM_TYPE.LOG:
			return "<sprite name=\"icon_LogsWhite\">";
		case InventoryItem.ITEM_TYPE.STONE:
			return "<sprite name=\"icon_StonesWhite\">";
		case InventoryItem.ITEM_TYPE.SEEDS:
			return "<sprite name=\"icon_SeedsWhite\">";
		case InventoryItem.ITEM_TYPE.BLACK_GOLD:
			return "<sprite name=\"icon_MoneyWhite\">";
		case InventoryItem.ITEM_TYPE.POOP:
			return "<sprite name=\"icon_PoopWhite\">";
		case InventoryItem.ITEM_TYPE.MEALS:
			return "<sprite name=\"icon_MealsWhite\">";
		case InventoryItem.ITEM_TYPE.GRASS:
			return "<sprite name=\"icon_GrassWhite\">";
		case InventoryItem.ITEM_TYPE.INGREDIENTS:
			return "<sprite name=\"icon_IngredientsWhite\">";
		default:
			return "";
		}
	}

	public static string GetIconByType(InventoryItem.ITEM_TYPE Type)
	{
		switch (Type)
		{
		case InventoryItem.ITEM_TYPE.SEED:
			return "<sprite name=\"icon_seed\">";
		case InventoryItem.ITEM_TYPE.GRASS:
			return "<sprite name=\"icon_grass\">";
		case InventoryItem.ITEM_TYPE.LOG:
			return "<sprite name=\"icon_wood\">";
		case InventoryItem.ITEM_TYPE.BERRY:
			return "<sprite name=\"icon_berries\">";
		case InventoryItem.ITEM_TYPE.BLACK_GOLD:
			return "<sprite name=\"icon_blackgold\">";
		case InventoryItem.ITEM_TYPE.GOD_TEAR_FRAGMENT:
			return "<sprite name=\"icon_GodTearShard\">";
		case InventoryItem.ITEM_TYPE.BLUE_HEART:
			return "<sprite name=\"icon_blueheart\">";
		case InventoryItem.ITEM_TYPE.BONE:
			return "<sprite name=\"icon_bones\">";
		case InventoryItem.ITEM_TYPE.FLOWERS:
			return "<sprite name=\"icon_flowers\">";
		case InventoryItem.ITEM_TYPE.HALF_BLUE_HEART:
			return "<sprite name=\"icon_blueheart_half\">";
		case InventoryItem.ITEM_TYPE.HALF_HEART:
			return "<sprite name=\"icon_heart_half\">";
		case InventoryItem.ITEM_TYPE.MEAT:
			return "<sprite name=\"icon_meat\">";
		case InventoryItem.ITEM_TYPE.MEAT_MORSEL:
			return "<sprite name=\"icon_MeatMorsel\">";
		case InventoryItem.ITEM_TYPE.MONSTER_HEART:
			return "<sprite name=\"icon_monsterHeart\">";
		case InventoryItem.ITEM_TYPE.RED_HEART:
			return "<sprite name=\"icon_heart\">";
		case InventoryItem.ITEM_TYPE.SOUL:
			return "<sprite name=\"icon_spirits\">";
		case InventoryItem.ITEM_TYPE.BLACK_SOUL:
			return "<sprite name=\"icon_blackSoul\">";
		case InventoryItem.ITEM_TYPE.STAINED_GLASS:
			return "<sprite name=\"icon_stainedglass\">";
		case InventoryItem.ITEM_TYPE.STONE:
			return "<sprite name=\"icon_stone\">";
		case InventoryItem.ITEM_TYPE.TIME_TOKEN:
			return "<sprite name=\"icon_timetoken\">";
		case InventoryItem.ITEM_TYPE.VINES:
			return "<sprite name=\"icon_brambles\">";
		case InventoryItem.ITEM_TYPE.FISH:
			return "<sprite name=\"icon_Fish\">";
		case InventoryItem.ITEM_TYPE.MUSHROOM_SMALL:
			return "<sprite name=\"icon_Mushroom\">";
		case InventoryItem.ITEM_TYPE.MUSHROOM_BIG:
			return "<sprite name=\"icon_Mushroom\">";
		case InventoryItem.ITEM_TYPE.FISH_BIG:
			return "<sprite name=\"icon_FishBig\">";
		case InventoryItem.ITEM_TYPE.FISH_SMALL:
			return "<sprite name=\"icon_FishSmall\">";
		case InventoryItem.ITEM_TYPE.FISH_BLOWFISH:
			return "<sprite name=\"icon_FishBlowfish\">";
		case InventoryItem.ITEM_TYPE.FISH_CRAB:
			return "<sprite name=\"icon_FishCrab\">";
		case InventoryItem.ITEM_TYPE.FISH_LOBSTER:
			return "<sprite name=\"icon_FishLobster\">";
		case InventoryItem.ITEM_TYPE.FISH_OCTOPUS:
			return "<sprite name=\"icon_FishOctopus\">";
		case InventoryItem.ITEM_TYPE.FISH_SQUID:
			return "<sprite name=\"icon_FishSquid\">";
		case InventoryItem.ITEM_TYPE.FISH_SWORDFISH:
			return "<sprite name=\"icon_FishSwordfish\">";
		case InventoryItem.ITEM_TYPE.MEAL:
			return "<sprite name=\"icon_Meal\">";
		case InventoryItem.ITEM_TYPE.POOP:
			return "<sprite name=\"icon_Poop\">";
		case InventoryItem.ITEM_TYPE.Necklace_1:
			return "<sprite name=\"icon_Necklace1\">";
		case InventoryItem.ITEM_TYPE.Necklace_2:
			return "<sprite name=\"icon_Necklace2\">";
		case InventoryItem.ITEM_TYPE.Necklace_3:
			return "<sprite name=\"icon_Necklace3\">";
		case InventoryItem.ITEM_TYPE.Necklace_4:
			return "<sprite name=\"icon_Necklace4\">";
		case InventoryItem.ITEM_TYPE.Necklace_5:
			return "<sprite name=\"icon_Necklace5\">";
		case InventoryItem.ITEM_TYPE.Necklace_Dark:
			return "<sprite name=\"icon_Necklace_Dark\">";
		case InventoryItem.ITEM_TYPE.Necklace_Demonic:
			return "<sprite name=\"icon_Necklace_Demonic\">";
		case InventoryItem.ITEM_TYPE.Necklace_Loyalty:
			return "<sprite name=\"icon_Necklace_Loyalty\">";
		case InventoryItem.ITEM_TYPE.Necklace_Gold_Skull:
			return "<sprite name=\"icon_Necklace_Gold_Skull\">";
		case InventoryItem.ITEM_TYPE.Necklace_Light:
			return "<sprite name=\"icon_Necklace_Light\">";
		case InventoryItem.ITEM_TYPE.Necklace_Missionary:
			return "<sprite name=\"icon_Necklace_Missionary\">";
		case InventoryItem.ITEM_TYPE.GIFT_SMALL:
			return "<sprite name=\"icon_GiftSmall\">";
		case InventoryItem.ITEM_TYPE.GIFT_MEDIUM:
			return "<sprite name=\"icon_GiftMedium\">";
		case InventoryItem.ITEM_TYPE.PUMPKIN:
			return "<sprite name=\"icon_Pumpkin\">";
		case InventoryItem.ITEM_TYPE.SEED_PUMPKIN:
			return "<sprite name=\"icon_SeedPumpkin\">";
		case InventoryItem.ITEM_TYPE.FLOWER_RED:
			return "<sprite name=\"icon_FlowerRed\">";
		case InventoryItem.ITEM_TYPE.FLOWER_WHITE:
			return "<sprite name=\"icon_FlowerWhite\">";
		case InventoryItem.ITEM_TYPE.FLOWER_PURPLE:
			return "<sprite name=\"icon_FlowerPurple\">";
		case InventoryItem.ITEM_TYPE.FOLLOWER_MEAT:
			return "<sprite name=\"icon_FollowerMeat\">";
		case InventoryItem.ITEM_TYPE.FOLLOWER_MEAT_ROTTEN:
			return "<sprite name=\"icon_FollowerMeatRotten\">";
		case InventoryItem.ITEM_TYPE.SEED_MUSHROOM:
			return "<sprite name=\"icon_SeedMushroom\">";
		case InventoryItem.ITEM_TYPE.SEED_FLOWER_RED:
			return "<sprite name=\"icon_SeedFlowerRed\">";
		case InventoryItem.ITEM_TYPE.SEED_TREE:
			return "<sprite name=\"icon_SeedTree\">";
		case InventoryItem.ITEM_TYPE.FOLLOWERS:
			return "<sprite name=\"icon_Followers\">";
		case InventoryItem.ITEM_TYPE.ROPE:
			return "<sprite name=\"icon_Rope\">";
		case InventoryItem.ITEM_TYPE.LOG_REFINED:
			return "<sprite name=\"icon_LogRefined\">";
		case InventoryItem.ITEM_TYPE.STONE_REFINED:
			return "<sprite name=\"icon_StoneRefined\">";
		case InventoryItem.ITEM_TYPE.GOLD_REFINED:
			return "<sprite name=\"icon_GoldRefined\">";
		case InventoryItem.ITEM_TYPE.GOLD_NUGGET:
			return "<sprite name=\"icon_GoldNugget\">";
		case InventoryItem.ITEM_TYPE.MEAL_GREAT:
			return "<sprite name=\"icon_MealGreat\">";
		case InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH:
			return "<sprite name=\"icon_MealGoodFish\">";
		case InventoryItem.ITEM_TYPE.MEAL_GRASS:
			return "<sprite name=\"icon_MealGrass\">";
		case InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT:
			return "<sprite name=\"icon_MealFollowerMeat\">";
		case InventoryItem.ITEM_TYPE.MEAL_MEAT:
			return "<sprite name=\"icon_MealMeat\">";
		case InventoryItem.ITEM_TYPE.MEAL_POOP:
			return "<sprite name=\"icon_MealPoop\">";
		case InventoryItem.ITEM_TYPE.MEAL_BERRIES:
			return "<sprite name=\"icon_MealBerries\">";
		case InventoryItem.ITEM_TYPE.MEAL_DEADLY:
			return "<sprite name=\"icon_MealDeadly\">";
		case InventoryItem.ITEM_TYPE.MEAL_BAD_MIXED:
			return "<sprite name=\"icon_MealBadMixed\">";
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_MIXED:
			return "<sprite name=\"icon_MealMediumMixed\">";
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MIXED:
			return "<sprite name=\"icon_MealGreatMixed\">";
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG:
			return "<sprite name=\"icon_MealMediumVeg\">";
		case InventoryItem.ITEM_TYPE.MEAL_MUSHROOMS:
			return "<sprite name=\"icon_MealMushrooms\">";
		case InventoryItem.ITEM_TYPE.MEAL_BAD_MEAT:
			return "<sprite name=\"icon_MealBadMeat\">";
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT:
			return "<sprite name=\"icon_MealGreatMeat\">";
		case InventoryItem.ITEM_TYPE.CRYSTAL:
			return "<sprite name=\"icon_Crystal\">";
		case InventoryItem.ITEM_TYPE.SPIDER_WEB:
			return "<sprite name=\"icon_SpiderWeb\">";
		case InventoryItem.ITEM_TYPE.BEETROOT:
			return "<sprite name=\"icon_Beetroot\">";
		case InventoryItem.ITEM_TYPE.SEED_BEETROOT:
			return "<sprite name=\"icon_SeedBeetroot\">";
		case InventoryItem.ITEM_TYPE.CAULIFLOWER:
			return "<sprite name=\"icon_Cauliflower\">";
		case InventoryItem.ITEM_TYPE.SEED_CAULIFLOWER:
			return "<sprite name=\"icon_SeedCauliflower\">";
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH:
			return "<sprite name=\"icon_MealGreatFish\">";
		case InventoryItem.ITEM_TYPE.MEAL_BAD_FISH:
			return "<sprite name=\"icon_MealBadFish\">";
		case InventoryItem.ITEM_TYPE.BEHOLDER_EYE:
			return "<sprite name=\"icon_BeholderEye\">";
		case InventoryItem.ITEM_TYPE.TALISMAN:
			return "<sprite name=\"icon_key\">";
		case InventoryItem.ITEM_TYPE.DOCTRINE_STONE:
			return "<sprite name=\"icon_DoctrineStone\">";
		case InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE:
			return "<sprite name=\"icon_CrystalDoctrineStone\">";
		case InventoryItem.ITEM_TYPE.SHELL:
			return "<sprite name=\"icon_Shell\">";
		case InventoryItem.ITEM_TYPE.GOD_TEAR:
			return "<sprite name=\"icon_GodTear\">";
		default:
			return "";
		}
	}

	public static string IconForRole(FollowerRole followerRole)
	{
		switch (followerRole)
		{
		case FollowerRole.Forager:
		case FollowerRole.Berries:
			return IconForCommand(FollowerCommands.ForageBerries);
		case FollowerRole.Builder:
			return IconForCommand(FollowerCommands.Build);
		case FollowerRole.Chef:
			return IconForCommand(FollowerCommands.Cook_2);
		case FollowerRole.Farmer:
			return IconForCommand(FollowerCommands.Farmer_2);
		case FollowerRole.Janitor:
			return IconForCommand(FollowerCommands.Janitor_2);
		case FollowerRole.Lumberjack:
			return IconForCommand(FollowerCommands.CutTrees);
		case FollowerRole.Monk:
			return IconForCommand(FollowerCommands.Study);
		case FollowerRole.Refiner:
			return IconForCommand(FollowerCommands.Refiner_2);
		case FollowerRole.StoneMiner:
			return IconForCommand(FollowerCommands.ClearRubble);
		case FollowerRole.Worshipper:
			return IconForCommand(FollowerCommands.WorshipAtShrine);
		case FollowerRole.Undertaker:
			return IconForCommand(FollowerCommands.Undertaker);
		default:
			return "";
		}
	}

	public static string IconForCommand(FollowerCommands followerCommands)
	{
		switch (followerCommands)
		{
		case FollowerCommands.GiveWorkerCommand_2:
			return "\uf82c";
		case FollowerCommands.ChangeRole:
			return "\uf021";
		case FollowerCommands.GiveItem:
			return "\uf4bd";
		case FollowerCommands.Talk:
			return "\uf075";
		case FollowerCommands.MakeDemand:
			return "\uf0a1";
		case FollowerCommands.BedRest:
			return "\ue074";
		case FollowerCommands.Murder:
			return "\uf6f5";
		case FollowerCommands.ExtortMoney:
			return "<sprite name=\"icon_Bribe\">";
		case FollowerCommands.Dance:
			return "\uf587";
		case FollowerCommands.DemandDevotion:
			return "<sprite name=\"icon_SoulWhite\">";
		case FollowerCommands.Gift:
			return "\uf06b";
		case FollowerCommands.Imprison:
			return "\uf4b7";
		case FollowerCommands.SendToHospital:
			return "\ue3b2";
		case FollowerCommands.CutTrees:
			return "<sprite name=\"icon_ChopWood\">";
		case FollowerCommands.ForageBerries:
			return "<sprite name=\"icon_berries\">";
		case FollowerCommands.ClearWeeds:
			return "<sprite name=\"icon_grass\">";
		case FollowerCommands.ClearRubble:
			return "<sprite name=\"icon_MineStone\">";
		case FollowerCommands.TellMeYourProblems:
			return "\uf4ad";
		case FollowerCommands.DemandLoyalty:
			return "''u21";
		case FollowerCommands.Punish:
			return "\uf0e3";
		case FollowerCommands.BeNice:
			return "\uf118";
		case FollowerCommands.Romance:
			return "\uf004";
		case FollowerCommands.PetDog:
			return "\uf6d3";
		case FollowerCommands.WakeUp:
			return "\uf34e";
		case FollowerCommands.LevelUp:
			return "\uf357";
		case FollowerCommands.EatSomething:
			return "\uf623";
		case FollowerCommands.Sleep:
			return "\uf880";
		case FollowerCommands.WorshipAtShrine:
			return "\uf684";
		case FollowerCommands.Build:
			return "\uf6e3";
		case FollowerCommands.NextPage:
			return "<sprite name=\"img_SwirleyRight\">";
		case FollowerCommands.NoAvailablePrisons:
			return "\uf4b7";
		case FollowerCommands.Cook_2:
			return "\uf86b";
		case FollowerCommands.Meal:
			return "<sprite name=\"icon_Meal\">";
		case FollowerCommands.MealGrass:
			return "<sprite name=\"icon_MealGrass\">";
		case FollowerCommands.MealPoop:
			return "<sprite name=\"icon_MealPoop\">";
		case FollowerCommands.MealGoodFish:
			return "<sprite name=\"icon_MealGoodFish\">";
		case FollowerCommands.MealFollowerMeat:
			return "<sprite name=\"icon_MealFollowerMeat\">";
		case FollowerCommands.MealGreat:
			return "<sprite name=\"icon_MealGreat\">";
		case FollowerCommands.MealMushrooms:
			return "<sprite name=\"icon_MealMushrooms\">";
		case FollowerCommands.MealMeat:
			return "<sprite name=\"icon_MealGood\">";
		case FollowerCommands.MealGreatFish:
			return "<sprite name=\"icon_MealGreatFish\">";
		case FollowerCommands.MealBadFish:
			return "<sprite name=\"icon_MealBadFish\">";
		case FollowerCommands.MealBerries:
			return "<sprite name=\"icon_MealBerries\">";
		case FollowerCommands.MealDeadly:
			return "<sprite name=\"icon_MealDeadly\">";
		case FollowerCommands.MealMediumVeg:
			return "<sprite name=\"icon_MealMediumVeg\">";
		case FollowerCommands.MealMeatLow:
			return "<sprite name=\"icon_MealBadMeat\">";
		case FollowerCommands.MealMeatHigh:
			return "<sprite name=\"icon_MealGreatMeat\">";
		case FollowerCommands.MealMixedLow:
			return "<sprite name=\"icon_MealBadMixed\">";
		case FollowerCommands.MealMixedMedium:
			return "<sprite name=\"icon_MealMediumMixed\">";
		case FollowerCommands.MealMixedHigh:
			return "<sprite name=\"icon_MealGreatMixed\">";
		case FollowerCommands.AreYouSure:
			return "''u3f";
		case FollowerCommands.AreYouSureYes:
			return "<sprite name=\"icon_ThumbsUp\">";
		case FollowerCommands.AreYouSureNo:
			return "<sprite name=\"icon_ThumbsDown\">";
		case FollowerCommands.Study:
			return "\uf02d";
		case FollowerCommands.Intimidate:
			return "\uf556";
		case FollowerCommands.Bribe:
			return "<sprite name=\"icon_Bribe\">";
		case FollowerCommands.Ascend:
			return "\uf102";
		case FollowerCommands.Surveillance:
			return "\ue32e";
		case FollowerCommands.Farmer_2:
			return "\uf864";
		case FollowerCommands.Bless:
			return "\ue05d";
		case FollowerCommands.TaxEnforcer:
			return "<sprite name=\"icon_blackgold\">";
		case FollowerCommands.CollectTax:
			return "<sprite name=\"icon_blackgold\">";
		case FollowerCommands.FaithEnforcer:
			return "<sprite name=\"icon_Faith\">";
		case FollowerCommands.Gift_Small:
			return "<sprite name=\"icon_GiftSmall\">";
		case FollowerCommands.Gift_Medium:
			return "<sprite name=\"icon_GiftMedium\">";
		case FollowerCommands.Gift_Necklace1:
			return "<sprite name=\"icon_Necklace1\">";
		case FollowerCommands.Gift_Necklace2:
			return "<sprite name=\"icon_Necklace2\">";
		case FollowerCommands.Gift_Necklace3:
			return "<sprite name=\"icon_Necklace3\">";
		case FollowerCommands.Gift_Necklace4:
			return "<sprite name=\"icon_Necklace4\">";
		case FollowerCommands.Gift_Necklace5:
			return "<sprite name=\"icon_Necklace5\">";
		case FollowerCommands.Gift_Necklace_Dark:
			return "<sprite name=\"icon_Necklace_Dark\">";
		case FollowerCommands.Gift_Necklace_Demonic:
			return "<sprite name=\"icon_Necklace_Demonic\">";
		case FollowerCommands.Gift_Necklace_Loyalty:
			return "<sprite name=\"icon_Necklace_Loyalty\">";
		case FollowerCommands.Gift_Necklace_Light:
			return "<sprite name=\"icon_Necklace_Light\">";
		case FollowerCommands.Gift_Necklace_Missionary:
			return "<sprite name=\"icon_Necklace_Missionary\">";
		case FollowerCommands.Gift_Necklace_Gold_Skull:
			return "<sprite name=\"icon_Necklace_Gold_Skull\">";
		case FollowerCommands.RemoveNecklace:
			return "";
		case FollowerCommands.Undertaker:
			return "<sprite name=\"icon_Undertaker\">";
		case FollowerCommands.Janitor_2:
			return "<sprite name=\"icon_Janitor\">";
		case FollowerCommands.Reeducate:
			return "\uf19d";
		case FollowerCommands.Refiner_2:
			return "<sprite name=\"icon_RefineResources\">";
		case FollowerCommands.ViewTraits:
			return "\uf06e";
		case FollowerCommands.GiveLeaderItem:
			return "\uf4bd";
		default:
			throw new ArgumentOutOfRangeException("followerCommands", followerCommands, null);
		}
	}
}
