using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using Lamb.UI.BuildMenu;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public sealed class StructuresData
{
	[Serializable]
	public struct PathData
	{
		public Vector2Int TilePosition;

		public Vector3 WorldPosition;

		public int PathID;
	}

	public enum Phase
	{
		Hidden,
		Available,
		Built
	}

	public enum ResearchState
	{
		Unresearched,
		Researching,
		Researched
	}

	[Serializable]
	public class ResearchObject
	{
		public StructureBrain.TYPES Type;

		public float Progress;

		public float TargetProgress
		{
			get
			{
				StructureBrain.TYPES type = Type;
				if (type == StructureBrain.TYPES.FARM_PLOT)
				{
					return 480f;
				}
				return 240f;
			}
		}

		public ResearchObject()
		{
		}

		public ResearchObject(StructureBrain.TYPES Type)
		{
			this.Type = Type;
		}

		public static float GetResearchTimeInDays(StructureBrain.TYPES Type)
		{
			return new ResearchObject(Type).TargetProgress / 480f;
		}
	}

	public class ItemCost : IComparable<ItemCost>
	{
		public InventoryItem.ITEM_TYPE CostItem;

		public int CostValue;

		public ItemCost(InventoryItem.ITEM_TYPE CostItem, int CostValue)
		{
			this.CostItem = CostItem;
			this.CostValue = CostValue;
		}

		public int CompareTo(ItemCost other)
		{
			if (other == null)
			{
				return 1;
			}
			if (CostItem == InventoryItem.ITEM_TYPE.FOLLOWERS)
			{
				return 0;
			}
			return 1;
		}

		public override string ToString()
		{
			return CostFormatter.FormatCost(this, false);
		}

		public string ToStringShowQuantity()
		{
			return CostFormatter.FormatCost(this);
		}

		public static string GetCostString(params ItemCost[] itemCosts)
		{
			return CostFormatter.FormatCosts(itemCosts, false);
		}

		public static string GetCostStringWithQuantity(params ItemCost[] itemCosts)
		{
			return CostFormatter.FormatCosts(itemCosts);
		}

		public static string GetCostStringWithQuantity(List<ItemCost> itemCosts)
		{
			return GetCostStringWithQuantity(itemCosts.ToArray());
		}

		public static string GetCostString(List<ItemCost> itemCosts)
		{
			return GetCostString(itemCosts.ToArray());
		}

		public bool CanAfford()
		{
			return global::Inventory.GetItemQuantity((int)CostItem) >= CostValue;
		}
	}

	public enum Availabilty
	{
		Available,
		Locked,
		Hidden
	}

	public StructureBrain.TYPES Type;

	public int VariantIndex;

	public string PrefabPath;

	public bool RemoveOnDie = true;

	public float ProgressTarget = 10f;

	public bool WorkIsRequiredForProgress = true;

	public bool IsUpgrade;

	public bool IsUpgradeDestroyPrevious = true;

	public bool IgnoreGrid;

	public bool IsBuildingProject;

	public bool IsCollapsed;

	public StructureBrain.TYPES UpgradeFromType;

	public StructureBrain.TYPES RequiresType;

	public int TILE_WIDTH = 1;

	public int TILE_HEIGHT = 1;

	public bool CanBeMoved = true;

	public bool CanBeRecycled = true;

	public bool IsObstruction;

	public bool DoesNotOccupyGrid;

	public bool isDeletable = true;

	public Vector2Int LootCountToDropRange;

	public Vector2Int CropLootCountToDropRange;

	public List<InventoryItem.ITEM_TYPE> MultipleLootToDrop;

	public List<int> MultipleLootToDropChance;

	public InventoryItem.ITEM_TYPE LootToDrop;

	public int LootCountToDrop;

	public int ID;

	public FollowerLocation Location = FollowerLocation.None;

	public bool DontLoadMe;

	public bool Destroyed;

	public int GridX;

	public int GridY;

	public Vector2Int Bounds = new Vector2Int(1, 1);

	public List<InventoryItem> Inventory = new List<InventoryItem>();

	public float Progress;

	public float PowerRequirement;

	public Vector3 Position;

	public Vector3 Offset;

	public float OffsetMax;

	public bool Repaired;

	public Vector2Int GridTilePosition = NullPosition;

	public Vector3Int PlacementRegionPosition;

	public static readonly Vector2Int NullPosition = new Vector2Int(-2147483647, -2147483647);

	public int Age;

	public bool Exhausted;

	public int UpgradeLevel;

	public List<PathData> pathData = new List<PathData>();

	public int Direction = 1;

	public Villager_Info v_i;

	public int SoulCount;

	public int Level;

	public StructureBrain.TYPES ToBuildType;

	public Phase CurrentPhase;

	public bool Purchased;

	private List<PlacementRegion.TileGridTile> grid = new List<PlacementRegion.TileGridTile>();

	[JsonIgnore]
	public Dictionary<Vector2Int, PlacementRegion.TileGridTile> GridTileLookup = new Dictionary<Vector2Int, PlacementRegion.TileGridTile>();

	public int FollowerID = -1;

	public List<int> MultipleFollowerIDs = new List<int>();

	public List<int> FollowersClaimedSlots = new List<int>();

	public int BedpanCount;

	public bool HasFood;

	public float FollowerImprisonedTimestamp;

	public float FollowerImprisonedFaith;

	public bool GivenGift;

	public int Dir = 1;

	public bool BodyWrapped;

	public bool BeenInMorgueAlready;

	public bool Prioritised;

	public bool PrioritisedAsBuildingObstruction;

	public bool WeedsAndRubblePlaced;

	public bool Rotten;

	public bool Burned;

	public bool Eaten;

	public int GatheringEndPhase = -1;

	public bool IsSapling;

	public float GrowthStage;

	public bool CanRegrow = true;

	public bool BenefitedFromFertilizer;

	public int RemainingHarvests;

	public string Animation = "";

	public float StartingScale;

	public bool Picked;

	public bool Watered;

	public int WateredCount;

	public bool HasBird;

	public int TotalPoops;

	public InventoryItem.ITEM_TYPE SignPostItem;

	public bool GivenHealth;

	public int WeedType = -1;

	public float LastPrayTime = -1f;

	public int Fuel;

	public int MaxFuel = 50;

	public bool FullyFueled;

	public int FuelDepletionDayTimestamp = -1;

	public bool onlyDepleteWhenFullyFueled;

	public List<InventoryItem.ITEM_TYPE> QueuedResources = new List<InventoryItem.ITEM_TYPE>();

	public List<Interaction_Kitchen.QueuedMeal> QueuedMeals = new List<Interaction_Kitchen.QueuedMeal>();

	public Interaction_Kitchen.QueuedMeal CurrentCookingMeal;

	public float WeaponUpgradePointProgress;

	public float WeaponUpgradePointDuration;

	public WeaponUpgradeSystem.WeaponType CurrentUnlockingWeaponType;

	public WeaponUpgradeSystem.WeaponUpgradeType CurrentUnlockingUpgradeType;

	public static Action OnResearchBegin;

	public static readonly List<StructureBrain.TYPES> AllStructures = new List<StructureBrain.TYPES>
	{
		StructureBrain.TYPES.BED,
		StructureBrain.TYPES.COOKING_FIRE,
		StructureBrain.TYPES.HARVEST_TOTEM,
		StructureBrain.TYPES.SCARECROW,
		StructureBrain.TYPES.COLLECTED_RESOURCES_CHEST,
		StructureBrain.TYPES.DANCING_FIREPIT,
		StructureBrain.TYPES.BODY_PIT,
		StructureBrain.TYPES.BLACKSMITH,
		StructureBrain.TYPES.OUTHOUSE,
		StructureBrain.TYPES.COMPOST_BIN,
		StructureBrain.TYPES.BED_2,
		StructureBrain.TYPES.GRAVE,
		StructureBrain.TYPES.FARM_STATION,
		StructureBrain.TYPES.FARM_STATION_II,
		StructureBrain.TYPES.FARM_PLOT,
		StructureBrain.TYPES.FOOD_STORAGE,
		StructureBrain.TYPES.FOOD_STORAGE_2,
		StructureBrain.TYPES.JANITOR_STATION,
		StructureBrain.TYPES.LUMBERJACK_STATION,
		StructureBrain.TYPES.LUMBERJACK_STATION_2,
		StructureBrain.TYPES.BLOODSTONE_MINE,
		StructureBrain.TYPES.BLOODSTONE_MINE_2,
		StructureBrain.TYPES.PROPAGANDA_SPEAKER,
		StructureBrain.TYPES.MISSIONARY,
		StructureBrain.TYPES.MISSIONARY_II,
		StructureBrain.TYPES.MISSIONARY_III,
		StructureBrain.TYPES.DEMON_SUMMONER,
		StructureBrain.TYPES.DEMON_SUMMONER_2,
		StructureBrain.TYPES.DEMON_SUMMONER_3,
		StructureBrain.TYPES.FISHING_HUT,
		StructureBrain.TYPES.PRISON,
		StructureBrain.TYPES.HEALING_BAY,
		StructureBrain.TYPES.HEALING_BAY_2,
		StructureBrain.TYPES.TAROT_BUILDING,
		StructureBrain.TYPES.BED_3,
		StructureBrain.TYPES.SHARED_HOUSE,
		StructureBrain.TYPES.SILO_SEED,
		StructureBrain.TYPES.SILO_FERTILISER,
		StructureBrain.TYPES.SURVEILLANCE,
		StructureBrain.TYPES.FISHING_HUT_2,
		StructureBrain.TYPES.OUTHOUSE_2,
		StructureBrain.TYPES.SCARECROW_2,
		StructureBrain.TYPES.HARVEST_TOTEM_2,
		StructureBrain.TYPES.CHOPPING_SHRINE,
		StructureBrain.TYPES.MINING_SHRINE,
		StructureBrain.TYPES.FORAGING_SHRINE,
		StructureBrain.TYPES.KITCHEN,
		StructureBrain.TYPES.MORGUE_1,
		StructureBrain.TYPES.MORGUE_2,
		StructureBrain.TYPES.CRYPT_1,
		StructureBrain.TYPES.CRYPT_2,
		StructureBrain.TYPES.CRYPT_3,
		StructureBrain.TYPES.CONFESSION_BOOTH,
		StructureBrain.TYPES.TEMPLE,
		StructureBrain.TYPES.TEMPLE_II,
		StructureBrain.TYPES.TEMPLE_III,
		StructureBrain.TYPES.TEMPLE_IV,
		StructureBrain.TYPES.SHRINE_PASSIVE,
		StructureBrain.TYPES.OFFERING_STATUE,
		StructureBrain.TYPES.SHRINE,
		StructureBrain.TYPES.SHRINE_II,
		StructureBrain.TYPES.SHRINE_III,
		StructureBrain.TYPES.SHRINE_IV,
		StructureBrain.TYPES.PLANK_PATH,
		StructureBrain.TYPES.TILE_PATH,
		StructureBrain.TYPES.TILE_HAY,
		StructureBrain.TYPES.TILE_BLOOD,
		StructureBrain.TYPES.TILE_ROCKS,
		StructureBrain.TYPES.TILE_WATER,
		StructureBrain.TYPES.TILE_BRICKS,
		StructureBrain.TYPES.TILE_PLANKS,
		StructureBrain.TYPES.TILE_FLOWERS,
		StructureBrain.TYPES.TILE_REDGRASS,
		StructureBrain.TYPES.TILE_SPOOKYPLANKS,
		StructureBrain.TYPES.TILE_GOLD,
		StructureBrain.TYPES.TILE_MOSAIC,
		StructureBrain.TYPES.TILE_FLOWERSROCKY,
		StructureBrain.TYPES.DECORATION_STONE,
		StructureBrain.TYPES.DECORATION_TREE,
		StructureBrain.TYPES.DECORATION_TORCH,
		StructureBrain.TYPES.DECORATION_FLOWER_BOX_1,
		StructureBrain.TYPES.DECORATION_SMALL_STONE_CANDLE,
		StructureBrain.TYPES.DECORATION_FLAG_CROWN,
		StructureBrain.TYPES.DECORATION_FLAG_SCRIPTURE,
		StructureBrain.TYPES.DECORATION_WALL_TWIGS,
		StructureBrain.TYPES.DECORATION_LAMB_FLAG_STATUE,
		StructureBrain.TYPES.FARM_PLOT_SIGN,
		StructureBrain.TYPES.DECORATION_BARROW,
		StructureBrain.TYPES.DECORATION_BELL_STATUE,
		StructureBrain.TYPES.DECORATION_BONE_ARCH,
		StructureBrain.TYPES.DECORATION_BONE_BARREL,
		StructureBrain.TYPES.DECORATION_BONE_CANDLE,
		StructureBrain.TYPES.DECORATION_BONE_FLAG,
		StructureBrain.TYPES.DECORATION_BONE_LANTERN,
		StructureBrain.TYPES.DECORATION_BONE_PILLAR,
		StructureBrain.TYPES.DECORATION_BONE_SCULPTURE,
		StructureBrain.TYPES.DECORATION_CANDLE_BARREL,
		StructureBrain.TYPES.DECORATION_CRYSTAL_LAMP,
		StructureBrain.TYPES.DECORATION_CRYSTAL_LIGHT,
		StructureBrain.TYPES.DECORATION_CRYSTAL_ROCK,
		StructureBrain.TYPES.DECORATION_CRYSTAL_STATUE,
		StructureBrain.TYPES.DECORATION_CRYSTAL_TREE,
		StructureBrain.TYPES.DECORATION_CRYSTAL_WINDOW,
		StructureBrain.TYPES.DECORATION_FLOWER_ARCH,
		StructureBrain.TYPES.DECORATION_FOUNTAIN,
		StructureBrain.TYPES.DECORATION_POST_BOX,
		StructureBrain.TYPES.DECORATION_PUMPKIN_PILE,
		StructureBrain.TYPES.DECORATION_PUMPKIN_STOOL,
		StructureBrain.TYPES.DECORATION_STONE_CANDLE,
		StructureBrain.TYPES.DECORATION_STONE_FLAG,
		StructureBrain.TYPES.DECORATION_STONE_MUSHROOM,
		StructureBrain.TYPES.DECORATION_TORCH_BIG,
		StructureBrain.TYPES.DECORATION_TWIG_LAMP,
		StructureBrain.TYPES.DECORATION_WALL_BONE,
		StructureBrain.TYPES.DECORATION_WALL_GRASS,
		StructureBrain.TYPES.DECORATION_WALL_STONE,
		StructureBrain.TYPES.DECORATION_WREATH_STICK,
		StructureBrain.TYPES.DECORATION_STUMP_LAMB_STATUE,
		StructureBrain.TYPES.DECORATION_BELL_SMALL,
		StructureBrain.TYPES.DECORATION_BONE_SKULL_BIG,
		StructureBrain.TYPES.DECORATION_BONE_SKULL_PILE,
		StructureBrain.TYPES.DECORATION_FLAG_CRYSTAL,
		StructureBrain.TYPES.DECORATION_FLAG_MUSHROOM,
		StructureBrain.TYPES.DECORATION_FLOWER_BOTTLE,
		StructureBrain.TYPES.DECORATION_FLOWER_CART,
		StructureBrain.TYPES.DECORATION_HAY_BALE,
		StructureBrain.TYPES.DECORATION_HAY_PILE,
		StructureBrain.TYPES.DECORATION_LEAFY_FLOWER_SCULPTURE,
		StructureBrain.TYPES.DECORATION_LEAFY_LANTERN,
		StructureBrain.TYPES.DECORATION_LEAFY_SCULPTURE,
		StructureBrain.TYPES.DECORATION_MUSHROOM_1,
		StructureBrain.TYPES.DECORATION_MUSHROOM_2,
		StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_1,
		StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_2,
		StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_LARGE,
		StructureBrain.TYPES.DECORATION_MUSHROOM_SCULPTURE,
		StructureBrain.TYPES.DECORATION_SPIDER_LANTERN,
		StructureBrain.TYPES.DECORATION_SPIDER_PILLAR,
		StructureBrain.TYPES.DECORATION_SPIDER_SCULPTURE,
		StructureBrain.TYPES.DECORATION_SPIDER_TORCH,
		StructureBrain.TYPES.DECORATION_SPIDER_WEB_CROWN_SCULPTURE,
		StructureBrain.TYPES.DECORATION_STONE_CANDLE_LAMP,
		StructureBrain.TYPES.DECORATION_STONE_HENGE,
		StructureBrain.TYPES.DECORATION_WALL_SPIDER,
		StructureBrain.TYPES.DECORATION_POND,
		StructureBrain.TYPES.DECORATION_MONSTERSHRINE,
		StructureBrain.TYPES.DECORATION_FLOWERPOTWALL,
		StructureBrain.TYPES.DECORATION_LEAFYLAMPPOST,
		StructureBrain.TYPES.DECORATION_FLOWERVASE,
		StructureBrain.TYPES.DECORATION_WATERINGCAN,
		StructureBrain.TYPES.DECORATION_FLOWER_CART_SMALL,
		StructureBrain.TYPES.DECORATION_WEEPINGSHRINE,
		StructureBrain.TYPES.DECORATION_PLUSH,
		StructureBrain.TYPES.DECORATION_TWITCH_FLAG_CROWN,
		StructureBrain.TYPES.DECORATION_TWITCH_MUSHROOM_BAG,
		StructureBrain.TYPES.DECORATION_TWITCH_ROSE_BUSH,
		StructureBrain.TYPES.DECORATION_TWITCH_STONE_FLAG,
		StructureBrain.TYPES.DECORATION_TWITCH_STONE_STATUE,
		StructureBrain.TYPES.DECORATION_TWITCH_WOODEN_GUARDIAN,
		StructureBrain.TYPES.SHRINE_BLUEHEART,
		StructureBrain.TYPES.SHRINE_BLACKHEART,
		StructureBrain.TYPES.SHRINE_REDHEART,
		StructureBrain.TYPES.SHRINE_TAROT,
		StructureBrain.TYPES.SHRINE_DAMAGE,
		StructureBrain.TYPES.FARM_PLOT_SOZO,
		StructureBrain.TYPES.DECORATION_BOSS_TROPHY_1,
		StructureBrain.TYPES.DECORATION_BOSS_TROPHY_2,
		StructureBrain.TYPES.DECORATION_BOSS_TROPHY_3,
		StructureBrain.TYPES.DECORATION_BOSS_TROPHY_4,
		StructureBrain.TYPES.DECORATION_BOSS_TROPHY_5,
		StructureBrain.TYPES.DECORATION_HALLOWEEN_CANDLE,
		StructureBrain.TYPES.DECORATION_HALLOWEEN_PUMPKIN,
		StructureBrain.TYPES.DECORATION_HALLOWEEN_TREE,
		StructureBrain.TYPES.DECORATION_HALLOWEEN_SKULL,
		StructureBrain.TYPES.DECORATION_OLDFAITH_CRYSTAL,
		StructureBrain.TYPES.DECORATION_OLDFAITH_FLAG,
		StructureBrain.TYPES.DECORATION_OLDFAITH_FOUNTAIN,
		StructureBrain.TYPES.DECORATION_OLDFAITH_IRONMAIDEN,
		StructureBrain.TYPES.DECORATION_OLDFAITH_SHRINE,
		StructureBrain.TYPES.DECORATION_OLDFAITH_TORCH,
		StructureBrain.TYPES.DECORATION_OLDFAITH_WALL,
		StructureBrain.TYPES.TILE_OLDFAITH,
		StructureBrain.TYPES.DECORATION_VIDEO
	};

	private static readonly List<StructureBrain.TYPES> HiddenStructuresUntilUnlocked = new List<StructureBrain.TYPES>
	{
		StructureBrain.TYPES.GRAVE,
		StructureBrain.TYPES.COMPOST_BIN_DEAD_BODY,
		StructureBrain.TYPES.DECORATION_MONSTERSHRINE,
		StructureBrain.TYPES.DECORATION_PLUSH,
		StructureBrain.TYPES.DECORATION_MUSHROOM_1,
		StructureBrain.TYPES.DECORATION_MUSHROOM_SCULPTURE,
		StructureBrain.TYPES.TILE_FLOWERS,
		StructureBrain.TYPES.DECORATION_FLOWERPOTWALL,
		StructureBrain.TYPES.DECORATION_LEAFYLAMPPOST,
		StructureBrain.TYPES.DECORATION_FLOWERVASE,
		StructureBrain.TYPES.DECORATION_WATERINGCAN,
		StructureBrain.TYPES.DECORATION_FLOWER_CART_SMALL,
		StructureBrain.TYPES.DECORATION_WEEPINGSHRINE,
		StructureBrain.TYPES.DECORATION_OLDFAITH_CRYSTAL,
		StructureBrain.TYPES.DECORATION_OLDFAITH_FLAG,
		StructureBrain.TYPES.DECORATION_OLDFAITH_FOUNTAIN,
		StructureBrain.TYPES.DECORATION_OLDFAITH_IRONMAIDEN,
		StructureBrain.TYPES.DECORATION_OLDFAITH_SHRINE,
		StructureBrain.TYPES.DECORATION_OLDFAITH_TORCH,
		StructureBrain.TYPES.DECORATION_OLDFAITH_WALL,
		StructureBrain.TYPES.TILE_OLDFAITH,
		StructureBrain.TYPES.DECORATION_BOSS_TROPHY_5,
		StructureBrain.TYPES.DECORATION_VIDEO,
		StructureBrain.TYPES.DECORATION_TWITCH_FLAG_CROWN,
		StructureBrain.TYPES.DECORATION_TWITCH_MUSHROOM_BAG,
		StructureBrain.TYPES.DECORATION_TWITCH_ROSE_BUSH,
		StructureBrain.TYPES.DECORATION_TWITCH_STONE_FLAG,
		StructureBrain.TYPES.DECORATION_TWITCH_STONE_STATUE,
		StructureBrain.TYPES.DECORATION_TWITCH_WOODEN_GUARDIAN,
		StructureBrain.TYPES.DECORATION_HALLOWEEN_CANDLE,
		StructureBrain.TYPES.DECORATION_HALLOWEEN_PUMPKIN,
		StructureBrain.TYPES.DECORATION_HALLOWEEN_TREE,
		StructureBrain.TYPES.DECORATION_HALLOWEEN_SKULL
	};

	private const float GoldModifier = 2f;

	public bool IsDeletable
	{
		get
		{
			if ((Type == StructureBrain.TYPES.BODY_PIT || Type == StructureBrain.TYPES.GRAVE || Type == StructureBrain.TYPES.CRYPT_1 || Type == StructureBrain.TYPES.CRYPT_2 || Type == StructureBrain.TYPES.CRYPT_3 || Type == StructureBrain.TYPES.MORGUE_1 || Type == StructureBrain.TYPES.MORGUE_2) && (FollowerID != -1 || MultipleFollowerIDs.Count > 0))
			{
				return false;
			}
			return isDeletable;
		}
	}

	[JsonIgnore]
	public List<PlacementRegion.TileGridTile> Grid
	{
		get
		{
			return grid;
		}
	}

	public bool IsGatheringActive
	{
		get
		{
			if (GatheringEndPhase != -1)
			{
				return TimeManager.CurrentPhase != (DayPhase)GatheringEndPhase;
			}
			return false;
		}
	}

	public bool IsFull
	{
		get
		{
			return Inventory.Count >= Structures_Outhouse.Capacity(Type);
		}
	}

	public bool WeaponUpgradingInProgress
	{
		get
		{
			if (WeaponUpgradePointDuration > 0f)
			{
				return WeaponUpgradePointProgress < WeaponUpgradePointDuration;
			}
			return false;
		}
	}

	public bool WeaponUpgradingCompleted
	{
		get
		{
			if (WeaponUpgradePointProgress > 0f)
			{
				return WeaponUpgradePointProgress >= WeaponUpgradePointDuration;
			}
			return false;
		}
	}

	public void SetPathData(Vector2Int tilePos, Vector3 worldPosition, int pathID)
	{
		for (int num = pathData.Count - 1; num >= 0; num--)
		{
			if (pathData[num].TilePosition == tilePos)
			{
				pathData.RemoveAt(num);
			}
		}
		pathData.Add(new PathData
		{
			TilePosition = tilePos,
			WorldPosition = worldPosition,
			PathID = pathID
		});
	}

	public void CreateStructure(FollowerLocation location, Vector3 position, Vector2Int bounds)
	{
		ID = ++DataManager.Instance.StructureID;
		Location = location;
		GridX = (int)position.x;
		GridY = (int)position.y;
		Position = position;
		Bounds = bounds;
		Offset = new Vector3(UnityEngine.Random.Range(0f - OffsetMax, OffsetMax), UnityEngine.Random.Range(0f - OffsetMax, OffsetMax));
	}

	public static string GetLocalizedNameStatic(StructureBrain.TYPES Type)
	{
		return LocalizationManager.GetTranslation(string.Format("Structures/{0}", Type));
	}

	public static string LocalizedName(StructureBrain.TYPES Type)
	{
		return LocalizationManager.GetTranslation(string.Format("Structures/{0}", Type));
	}

	public static string LocalizedDescription(StructureBrain.TYPES Type)
	{
		switch (Type)
		{
		case StructureBrain.TYPES.MISSIONARY:
		case StructureBrain.TYPES.MISSIONARY_II:
		case StructureBrain.TYPES.MISSIONARY_III:
		{
			int num = 1;
			switch (Type)
			{
			case StructureBrain.TYPES.MISSIONARY_II:
				num = 2;
				break;
			case StructureBrain.TYPES.MISSIONARY_III:
				num = 3;
				break;
			}
			string text2 = " <br><br><sprite name=\"icon_wood\"> <sprite name=\"icon_stone\"> <sprite name=\"icon_blackgold\"> <sprite name=\"icon_meat\">";
			if (Type == StructureBrain.TYPES.MISSIONARY_II || Type == StructureBrain.TYPES.MISSIONARY_III)
			{
				text2 += " <sprite name=\"icon_bones\"> <sprite name=\"icon_Followers\"> <sprite name=\"icon_seed\">";
			}
			if (Type == StructureBrain.TYPES.MISSIONARY_III)
			{
				text2 += " <sprite name=\"icon_LogRefined\"> <sprite name=\"icon_StoneRefined\">";
			}
			string text3 = num + "x " + ScriptLocalization.Inventory.FOLLOWERS + " <sprite name=\"icon_Followers\">";
			return LocalizationManager.GetTranslation(string.Format("Structures/{0}/Description", Type)) + " <br><color=#FFD201>" + text3 + "</color>" + text2;
		}
		case StructureBrain.TYPES.LUMBERJACK_STATION_2:
		case StructureBrain.TYPES.BLOODSTONE_MINE_2:
		{
			string text5 = ((Type == StructureBrain.TYPES.LUMBERJACK_STATION_2) ? "<sprite name=\"icon_wood\">" : "<sprite name=\"icon_stone\">") + " <sprite name=\"icon_FaithDoubleUp\">";
			return LocalizationManager.GetTranslation(string.Format("Structures/{0}/Description", Type)) + "<br><br>" + text5;
		}
		case StructureBrain.TYPES.REFINERY_2:
		{
			string text4 = "<sprite name=\"icon_GoldRefined\"><sprite name=\"icon_LogRefined\"><sprite name=\"icon_StoneRefined\"> <sprite name=\"icon_FaithDoubleUp\">";
			return LocalizationManager.GetTranslation(string.Format("Structures/{0}/Description", Type)) + "<br><br>" + text4;
		}
		case StructureBrain.TYPES.DECORATION_MUSHROOM_SCULPTURE:
			if (DataManager.Instance.SozoDecorationQuestActive)
			{
				string text = ScriptLocalization.Objectives_GroupTitles.VisitSozo.Colour(Color.yellow) + ": " + string.Format(ScriptLocalization.Objectives.BuildStructure, LocalizedName(Type));
				return LocalizationManager.GetTranslation(string.Format("Structures/{0}/Description", Type)) + "<br><br>" + text;
			}
			break;
		}
		switch (Type)
		{
		case StructureBrain.TYPES.CRYPT_1:
		case StructureBrain.TYPES.CRYPT_2:
		case StructureBrain.TYPES.CRYPT_3:
		{
			string text7 = Structures_Crypt.GetCapacity(Type) + "x " + ScriptLocalization.Inventory.FOLLOWERS + " <sprite name=\"icon_Followers\">";
			return LocalizationManager.GetTranslation(string.Format("Structures/{0}/Description", Type)) + "<br><br><color=#FFD201>" + text7 + "</color>";
		}
		case StructureBrain.TYPES.MORGUE_1:
		case StructureBrain.TYPES.MORGUE_2:
		{
			string text6 = Structures_Morgue.GetCapacity(Type) + "x " + ScriptLocalization.Inventory.FOLLOWERS + " <sprite name=\"icon_Followers\">";
			return LocalizationManager.GetTranslation(string.Format("Structures/{0}/Description", Type)) + "<br><br><color=#FFD201>" + text6 + "</color>";
		}
		default:
			Debug.Log("Type: " + Type);
			return LocalizationManager.GetTranslation(string.Format("Structures/{0}/Description", Type));
		}
	}

	public static string LocalizedPros(StructureBrain.TYPES Type)
	{
		return LocalizationManager.GetTranslation(string.Format("Structures/{0}/Pros", Type));
	}

	public static string LocalizedCons(StructureBrain.TYPES Type)
	{
		return LocalizationManager.GetTranslation(string.Format("Structures/{0}/Cons", Type));
	}

	public string GetLocalizedName()
	{
		return LocalizationManager.GetTranslation(string.Format("Structures/{0}", Type));
	}

	public string GetLocalizedDescription()
	{
		return LocalizationManager.GetTranslation(string.Format("Structures/{0}/Description", Type));
	}

	public string GetLocalizedLore()
	{
		return LocalizationManager.GetTranslation("Structures/" + Type.ToString() + "/Lore");
	}

	public string GetLocalizedName(bool plural, bool withArticle, bool definite)
	{
		string text = "Structures/" + Type.ToString() + (plural ? "/Plural" : "") + ((!withArticle) ? "" : (definite ? "/Definite" : "/Indefinite"));
		Debug.Log(text);
		return LocalizationManager.GetTranslation(text);
	}

	public static StructuresData GetInfoByType(StructureBrain.TYPES Type, int variantIndex)
	{
		StructuresData structuresData = null;
		switch (Type)
		{
		case StructureBrain.TYPES.DANCING_FIREPIT:
		case StructureBrain.TYPES.TEMPLE_BASE:
		case StructureBrain.TYPES.TEMPLE_BASE_EXTENSION1:
		case StructureBrain.TYPES.TEMPLE_BASE_EXTENSION2:
		case StructureBrain.TYPES.SHRINE_BASE:
		case StructureBrain.TYPES.FEAST_TABLE:
		case StructureBrain.TYPES.FISHING_SPOT:
			structuresData = new StructuresData
			{
				DontLoadMe = true,
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.SHRINE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Base Shrine",
				CanBeMoved = false,
				CanBeRecycled = false,
				IgnoreGrid = true,
				isDeletable = false,
				UpgradeFromType = StructureBrain.TYPES.SHRINE_BASE
			};
			break;
		case StructureBrain.TYPES.SHRINE_II:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Base Shrine II",
				CanBeMoved = false,
				CanBeRecycled = false,
				IgnoreGrid = true,
				isDeletable = false
			};
			break;
		case StructureBrain.TYPES.SHRINE_III:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Base Shrine III",
				CanBeMoved = false,
				CanBeRecycled = false,
				IgnoreGrid = true,
				isDeletable = false
			};
			break;
		case StructureBrain.TYPES.SHRINE_IV:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Base Shrine IV",
				CanBeMoved = false,
				CanBeRecycled = false,
				IgnoreGrid = true,
				isDeletable = false
			};
			break;
		case StructureBrain.TYPES.TEMPLE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Temple",
				isDeletable = false
			};
			break;
		case StructureBrain.TYPES.TEMPLE_II:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Temple 2",
				isDeletable = false
			};
			break;
		case StructureBrain.TYPES.TEMPLE_III:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Temple 3",
				isDeletable = false
			};
			break;
		case StructureBrain.TYPES.TEMPLE_IV:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Temple 4",
				isDeletable = false
			};
			break;
		case StructureBrain.TYPES.TEMPLE_EXTENSION1:
			structuresData = new StructuresData
			{
				IsUpgradeDestroyPrevious = false,
				PrefabPath = "Prefabs/Structures/Buildings/Building Temple Extension 1",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.TEMPLE_EXTENSION2:
			structuresData = new StructuresData
			{
				IsUpgradeDestroyPrevious = false,
				PrefabPath = "Prefabs/Structures/Buildings/Building Temple Extension 2",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.BUILD_PLOT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Build Plot",
				TILE_WIDTH = 3,
				TILE_HEIGHT = 3
			};
			break;
		case StructureBrain.TYPES.COLLECTED_RESOURCES_CHEST:
			structuresData = new StructuresData
			{
				DontLoadMe = true,
				PrefabPath = "Prefabs/Structures/Buildings/Collected Resource Chest"
			};
			break;
		case StructureBrain.TYPES.PROPAGANDA_SPEAKER:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Propaganda Speakers"
			};
			break;
		case StructureBrain.TYPES.MISSIONARY:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Missionary"
			};
			break;
		case StructureBrain.TYPES.MISSIONARY_II:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Missionary 2"
			};
			break;
		case StructureBrain.TYPES.MISSIONARY_III:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Missionary 3"
			};
			break;
		case StructureBrain.TYPES.DEMON_SUMMONER:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Demon Summoner"
			};
			break;
		case StructureBrain.TYPES.DEMON_SUMMONER_2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Demon Summoner 2"
			};
			break;
		case StructureBrain.TYPES.DEMON_SUMMONER_3:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Demon Summoner 3"
			};
			break;
		case StructureBrain.TYPES.SILO_SEED:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Silo Seed"
			};
			break;
		case StructureBrain.TYPES.SILO_FERTILISER:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Silo Fertiliser"
			};
			break;
		case StructureBrain.TYPES.SURVEILLANCE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Surveillance Tower"
			};
			break;
		case StructureBrain.TYPES.FISHING_HUT_2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Fishing Hut 2"
			};
			break;
		case StructureBrain.TYPES.OUTHOUSE_2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Outhouse 2"
			};
			break;
		case StructureBrain.TYPES.SCARECROW_2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Farm Scarecrow 2"
			};
			break;
		case StructureBrain.TYPES.HARVEST_TOTEM_2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Farm Harvest Totem 2"
			};
			break;
		case StructureBrain.TYPES.BLACKSMITH:
			structuresData = new StructuresData
			{
				TILE_WIDTH = 4,
				TILE_HEIGHT = 4,
				PrefabPath = "Prefabs/Structures/Buildings/Building Blacksmith"
			};
			break;
		case StructureBrain.TYPES.SHRINE_PASSIVE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Shrine Passive"
			};
			break;
		case StructureBrain.TYPES.SHRINE_PASSIVE_II:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Shrine Passive II"
			};
			break;
		case StructureBrain.TYPES.SHRINE_PASSIVE_III:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Shrine Passive III"
			};
			break;
		case StructureBrain.TYPES.OFFERING_STATUE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Offering Statue"
			};
			break;
		case StructureBrain.TYPES.REFINERY:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Refinery"
			};
			break;
		case StructureBrain.TYPES.REFINERY_2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Refinery 2"
			};
			break;
		case StructureBrain.TYPES.BUILDER:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Builder"
			};
			break;
		case StructureBrain.TYPES.COOKED_FOOD_SILO:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Cooked Food Silo"
			};
			break;
		case StructureBrain.TYPES.PORTAL:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/PORTAL",
				TILE_WIDTH = 5,
				TILE_HEIGHT = 5
			};
			break;
		case StructureBrain.TYPES.FARM_STATION:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Farm House"
			};
			break;
		case StructureBrain.TYPES.FARM_STATION_II:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Farm House 2"
			};
			break;
		case StructureBrain.TYPES.FARM_PLOT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Farm Plot"
			};
			break;
		case StructureBrain.TYPES.JANITOR_STATION:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Janitor Station"
			};
			break;
		case StructureBrain.TYPES.COMPOST_BIN:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Compost Bin"
			};
			break;
		case StructureBrain.TYPES.COMPOST_BIN_DEAD_BODY:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Compost Bin Dead Body"
			};
			break;
		case StructureBrain.TYPES.LUMBERJACK_STATION:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Lumberjack",
				LootToDrop = InventoryItem.ITEM_TYPE.LOG
			};
			break;
		case StructureBrain.TYPES.LUMBERJACK_STATION_2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Lumberjack Lvl2",
				LootToDrop = InventoryItem.ITEM_TYPE.LOG
			};
			break;
		case StructureBrain.TYPES.KITCHEN:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Follower Kitchen"
			};
			break;
		case StructureBrain.TYPES.MORGUE_1:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Morgue 1"
			};
			break;
		case StructureBrain.TYPES.MORGUE_2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Morgue 2",
				IsUpgrade = true,
				UpgradeFromType = StructureBrain.TYPES.MORGUE_1
			};
			break;
		case StructureBrain.TYPES.CRYPT_1:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Crypt 1"
			};
			break;
		case StructureBrain.TYPES.CRYPT_2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Crypt 2",
				IsUpgrade = true,
				UpgradeFromType = StructureBrain.TYPES.CRYPT_1
			};
			break;
		case StructureBrain.TYPES.CRYPT_3:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Crypt 3",
				IsUpgrade = true,
				UpgradeFromType = StructureBrain.TYPES.CRYPT_2
			};
			break;
		case StructureBrain.TYPES.SACRIFICIAL_TEMPLE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/SacrificalTemple"
			};
			break;
		case StructureBrain.TYPES.TAVERN:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Tavern"
			};
			break;
		case StructureBrain.TYPES.BED:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building_House"
			};
			break;
		case StructureBrain.TYPES.BED_1_COLLAPSED:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building_House_Collapsed"
			};
			break;
		case StructureBrain.TYPES.BED_2_COLLAPSED:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building_House_2_Collapsed"
			};
			break;
		case StructureBrain.TYPES.WHEAT_SILO:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Wheat Silo"
			};
			break;
		case StructureBrain.TYPES.WOOD_STORE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/WoodStore"
			};
			break;
		case StructureBrain.TYPES.CROP:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Crops/Crop",
				OffsetMax = 0.1f
			};
			break;
		case StructureBrain.TYPES.NIGHTMARE_MACHINE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Nightmare Machine"
			};
			break;
		case StructureBrain.TYPES.DEFENCE_TOWER:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Defence Tower"
			};
			break;
		case StructureBrain.TYPES.FOLLOWER_RECRUIT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Recruit/Recruit",
				v_i = Villager_Info.NewCharacter()
			};
			break;
		case StructureBrain.TYPES.HEALING_BATH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Healing Bath"
			};
			break;
		case StructureBrain.TYPES.FIRE_PIT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Fire Pit"
			};
			break;
		case StructureBrain.TYPES.BLOODMOON_OFFERING:
			structuresData = new StructuresData
			{
				DontLoadMe = true,
				CanBeMoved = false,
				CanBeRecycled = false
			};
			break;
		case StructureBrain.TYPES.BARRACKS:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Barracks"
			};
			break;
		case StructureBrain.TYPES.ASTROLOGIST:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Astrologist"
			};
			break;
		case StructureBrain.TYPES.STORAGE_PIT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/StoragePit"
			};
			break;
		case StructureBrain.TYPES.BUILD_SITE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building_BuildSite"
			};
			break;
		case StructureBrain.TYPES.BUILDSITE_BUILDINGPROJECT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building_BuildSiteBuildingProject",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MISSION_SHRINE:
			structuresData = new StructuresData
			{
				DontLoadMe = true,
				PrefabPath = "Prefabs/Structures/Buildings/Mission Shrine"
			};
			break;
		case StructureBrain.TYPES.ALTAR:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Altar"
			};
			break;
		case StructureBrain.TYPES.PLACEMENT_REGION:
			structuresData = new StructuresData
			{
				DontLoadMe = true,
				DoesNotOccupyGrid = true
			};
			break;
		case StructureBrain.TYPES.DECORATION_TREE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Tree 1"
			};
			break;
		case StructureBrain.TYPES.DECORATION_BARROW:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Barrow"
			};
			break;
		case StructureBrain.TYPES.DECORATION_BELL_STATUE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Bell Statue"
			};
			break;
		case StructureBrain.TYPES.DECORATION_BONE_ARCH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Bone Arch"
			};
			break;
		case StructureBrain.TYPES.DECORATION_BONE_BARREL:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Bone Barrel"
			};
			break;
		case StructureBrain.TYPES.DECORATION_BONE_CANDLE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Bone Candle"
			};
			break;
		case StructureBrain.TYPES.DECORATION_BONE_FLAG:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Bone Flag Crown"
			};
			break;
		case StructureBrain.TYPES.DECORATION_BONE_LANTERN:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Bone Lantern"
			};
			break;
		case StructureBrain.TYPES.DECORATION_BONE_PILLAR:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Bone Pillar"
			};
			break;
		case StructureBrain.TYPES.DECORATION_BONE_SCULPTURE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Bone Sculpture"
			};
			break;
		case StructureBrain.TYPES.DECORATION_CANDLE_BARREL:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Candle Barrel"
			};
			break;
		case StructureBrain.TYPES.DECORATION_CRYSTAL_LAMP:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Crystal Lamp"
			};
			break;
		case StructureBrain.TYPES.DECORATION_CRYSTAL_LIGHT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Crystal Light"
			};
			break;
		case StructureBrain.TYPES.DECORATION_CRYSTAL_ROCK:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Crystal Rock"
			};
			break;
		case StructureBrain.TYPES.DECORATION_CRYSTAL_STATUE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Crystal Statue"
			};
			break;
		case StructureBrain.TYPES.DECORATION_CRYSTAL_TREE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Crystal Tree"
			};
			break;
		case StructureBrain.TYPES.DECORATION_CRYSTAL_WINDOW:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Crystal Window"
			};
			break;
		case StructureBrain.TYPES.DECORATION_FLOWER_ARCH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Flower Arch"
			};
			break;
		case StructureBrain.TYPES.DECORATION_FOUNTAIN:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Fountain"
			};
			break;
		case StructureBrain.TYPES.DECORATION_POST_BOX:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Post Box"
			};
			break;
		case StructureBrain.TYPES.DECORATION_PUMPKIN_PILE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Pumpkin Pile"
			};
			break;
		case StructureBrain.TYPES.DECORATION_PUMPKIN_STOOL:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Pumpkin Stool"
			};
			break;
		case StructureBrain.TYPES.DECORATION_STONE_CANDLE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Stone Candle"
			};
			break;
		case StructureBrain.TYPES.DECORATION_STONE_FLAG:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Stone Flag"
			};
			break;
		case StructureBrain.TYPES.DECORATION_STONE_MUSHROOM:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Stone Mushroom"
			};
			break;
		case StructureBrain.TYPES.DECORATION_TORCH_BIG:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Torch Big"
			};
			break;
		case StructureBrain.TYPES.DECORATION_TWIG_LAMP:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Twig Lamp"
			};
			break;
		case StructureBrain.TYPES.DECORATION_WALL_BONE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Wall Bone"
			};
			break;
		case StructureBrain.TYPES.DECORATION_WALL_GRASS:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Wall Grass"
			};
			break;
		case StructureBrain.TYPES.DECORATION_WALL_STONE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Wall Stone"
			};
			break;
		case StructureBrain.TYPES.DECORATION_WREATH_STICK:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Wreath Stick"
			};
			break;
		case StructureBrain.TYPES.DECORATION_STUMP_LAMB_STATUE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Stump Lamb Statue"
			};
			break;
		case StructureBrain.TYPES.DECORATION_STONE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Stone 1"
			};
			break;
		case StructureBrain.TYPES.DECORATION_BELL_SMALL:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Bell Statue Small"
			};
			break;
		case StructureBrain.TYPES.DECORATION_BONE_SKULL_BIG:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Bone Skull Big"
			};
			break;
		case StructureBrain.TYPES.DECORATION_BONE_SKULL_PILE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Bone Skull Pile"
			};
			break;
		case StructureBrain.TYPES.DECORATION_FLAG_CRYSTAL:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Flag Crystal"
			};
			break;
		case StructureBrain.TYPES.DECORATION_FLAG_MUSHROOM:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Flag Mushroom"
			};
			break;
		case StructureBrain.TYPES.DECORATION_FLOWER_BOTTLE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Flower Bottle"
			};
			break;
		case StructureBrain.TYPES.DECORATION_FLOWER_CART:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Flower Cart"
			};
			break;
		case StructureBrain.TYPES.DECORATION_HAY_BALE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Hay Bale"
			};
			break;
		case StructureBrain.TYPES.DECORATION_HAY_PILE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Hay Pile"
			};
			break;
		case StructureBrain.TYPES.DECORATION_LEAFY_FLOWER_SCULPTURE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Leafy Flower Sculpture"
			};
			break;
		case StructureBrain.TYPES.DECORATION_LEAFY_LANTERN:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Leafy Lantern"
			};
			break;
		case StructureBrain.TYPES.DECORATION_LEAFY_SCULPTURE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Leafy Sculpture"
			};
			break;
		case StructureBrain.TYPES.DECORATION_MUSHROOM_1:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Mushroom 1"
			};
			break;
		case StructureBrain.TYPES.DECORATION_MUSHROOM_2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Mushroom 2"
			};
			break;
		case StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_1:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Mushroom Candle 1"
			};
			break;
		case StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Mushroom Candle 2"
			};
			break;
		case StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_LARGE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Mushroom Candle Large"
			};
			break;
		case StructureBrain.TYPES.DECORATION_MUSHROOM_SCULPTURE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Mushroom Sculpture"
			};
			break;
		case StructureBrain.TYPES.DECORATION_SPIDER_LANTERN:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Spider Lantern"
			};
			break;
		case StructureBrain.TYPES.DECORATION_SPIDER_PILLAR:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Spider Pillar"
			};
			break;
		case StructureBrain.TYPES.DECORATION_SPIDER_SCULPTURE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Spider Sculpture"
			};
			break;
		case StructureBrain.TYPES.DECORATION_SPIDER_TORCH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Spider Torch"
			};
			break;
		case StructureBrain.TYPES.DECORATION_SPIDER_WEB_CROWN_SCULPTURE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Spider Web Crown Sculpture"
			};
			break;
		case StructureBrain.TYPES.DECORATION_STONE_CANDLE_LAMP:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Stone Candle Lamp"
			};
			break;
		case StructureBrain.TYPES.DECORATION_STONE_HENGE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Stone Henge"
			};
			break;
		case StructureBrain.TYPES.DECORATION_WALL_SPIDER:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Wall Spider"
			};
			break;
		case StructureBrain.TYPES.DECORATION_POND:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Pond"
			};
			break;
		case StructureBrain.TYPES.DECORATION_MONSTERSHRINE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration MassiveMonster Shrine"
			};
			break;
		case StructureBrain.TYPES.DECORATION_PLUSH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Plush"
			};
			break;
		case StructureBrain.TYPES.DECORATION_TWITCH_FLAG_CROWN:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Twitch Flag Crown"
			};
			break;
		case StructureBrain.TYPES.DECORATION_TWITCH_MUSHROOM_BAG:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Twitch Mushroom Bag"
			};
			break;
		case StructureBrain.TYPES.DECORATION_TWITCH_ROSE_BUSH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Twitch Rose Bush"
			};
			break;
		case StructureBrain.TYPES.DECORATION_TWITCH_STONE_FLAG:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Twitch Stone Flag"
			};
			break;
		case StructureBrain.TYPES.DECORATION_TWITCH_STONE_STATUE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Twitch Stone Statue"
			};
			break;
		case StructureBrain.TYPES.DECORATION_TWITCH_WOODEN_GUARDIAN:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Twitch Wooden Guardian"
			};
			break;
		case StructureBrain.TYPES.DECORATION_FLOWERPOTWALL:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Wall Flower Pots"
			};
			break;
		case StructureBrain.TYPES.DECORATION_LEAFYLAMPPOST:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Leafy Lamp Post"
			};
			break;
		case StructureBrain.TYPES.DECORATION_FLOWERVASE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Flower Vase"
			};
			break;
		case StructureBrain.TYPES.DECORATION_WATERINGCAN:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Flower Watering Can"
			};
			break;
		case StructureBrain.TYPES.DECORATION_FLOWER_CART_SMALL:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Flower Cart Small"
			};
			break;
		case StructureBrain.TYPES.DECORATION_WEEPINGSHRINE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Flower Weeping Shrine"
			};
			break;
		case StructureBrain.TYPES.DECORATION_OLDFAITH_CRYSTAL:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration OldFaith Crystal"
			};
			break;
		case StructureBrain.TYPES.DECORATION_OLDFAITH_FLAG:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration OldFaith Flag"
			};
			break;
		case StructureBrain.TYPES.DECORATION_OLDFAITH_FOUNTAIN:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration OldFaith Fountain"
			};
			break;
		case StructureBrain.TYPES.DECORATION_OLDFAITH_IRONMAIDEN:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration OldFaith IronMaiden"
			};
			break;
		case StructureBrain.TYPES.DECORATION_OLDFAITH_SHRINE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration OldFaith Shrine"
			};
			break;
		case StructureBrain.TYPES.DECORATION_OLDFAITH_TORCH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration OldFaith Torch"
			};
			break;
		case StructureBrain.TYPES.DECORATION_OLDFAITH_WALL:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Wall OldFaith"
			};
			break;
		case StructureBrain.TYPES.TILE_OLDFAITH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Path OldFaith"
			};
			break;
		case StructureBrain.TYPES.REPAIRABLE_HEARTS:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Repairable Heart Statue",
				Repaired = true
			};
			break;
		case StructureBrain.TYPES.REPAIRABLE_ASTROLOGY:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Repairable Astrology Statue",
				Repaired = true
			};
			break;
		case StructureBrain.TYPES.REPAIRABLE_VOODOO:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Repairable Voodoo Statue",
				Repaired = true
			};
			break;
		case StructureBrain.TYPES.REPAIRABLE_CURSES:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Repairable Curse Statue",
				Repaired = true
			};
			break;
		case StructureBrain.TYPES.BED_2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building_House2"
			};
			break;
		case StructureBrain.TYPES.BED_3:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building_House3"
			};
			break;
		case StructureBrain.TYPES.SHARED_HOUSE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Shared House"
			};
			break;
		case StructureBrain.TYPES.SLEEPING_BAG:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building_SleepingBag"
			};
			break;
		case StructureBrain.TYPES.SHRINE_FUNDAMENTALIST:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building_Fundamentalist Shrine"
			};
			break;
		case StructureBrain.TYPES.SHRINE_MISFIT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building_Misfit Shrine"
			};
			break;
		case StructureBrain.TYPES.SHRINE_UTOPIANIST:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building_Utopianist Shrine"
			};
			break;
		case StructureBrain.TYPES.DEAD_WORSHIPPER:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Dead Worshipper",
				IgnoreGrid = true,
				CanBeMoved = false,
				isDeletable = false
			};
			break;
		case StructureBrain.TYPES.GRAVE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Grave"
			};
			break;
		case StructureBrain.TYPES.VOMIT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Vomit",
				IgnoreGrid = true,
				CanBeMoved = false,
				isDeletable = false
			};
			break;
		case StructureBrain.TYPES.RUBBLE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Rubble",
				LootToDrop = InventoryItem.ITEM_TYPE.STONE
			};
			break;
		case StructureBrain.TYPES.RUBBLE_BIG:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Rubble Big",
				TILE_WIDTH = 5,
				TILE_HEIGHT = 5,
				LootToDrop = InventoryItem.ITEM_TYPE.STONE
			};
			break;
		case StructureBrain.TYPES.WATER_SMALL:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Water_3x3",
				TILE_WIDTH = 3,
				TILE_HEIGHT = 3
			};
			break;
		case StructureBrain.TYPES.WATER_MEDIUM:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Water_3x4",
				TILE_WIDTH = 4,
				TILE_HEIGHT = 3
			};
			break;
		case StructureBrain.TYPES.WATER_BIG:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Water_4x4",
				TILE_WIDTH = 4,
				TILE_HEIGHT = 4
			};
			break;
		case StructureBrain.TYPES.WEEDS:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Weeds",
				IsObstruction = true,
				CanBeMoved = false,
				isDeletable = false
			};
			break;
		case StructureBrain.TYPES.DEMOLISH_STRUCTURE:
			structuresData = new StructuresData();
			break;
		case StructureBrain.TYPES.MOVE_STRUCTURE:
			structuresData = new StructuresData();
			break;
		case StructureBrain.TYPES.FARM_PLOT_SOZO:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Farm Plot - Sozo",
				RequiresType = StructureBrain.TYPES.FARM_STATION
			};
			break;
		case StructureBrain.TYPES.RATAU_SHRINE:
			structuresData = new StructuresData
			{
				DontLoadMe = true
			};
			break;
		case StructureBrain.TYPES.MEAL:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MEAL_BURNED:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal Burned",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MEAL_GRASS:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal Grass",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MEAL_MEAT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal Good",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MEAL_GOOD_FISH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal Good Fish",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MEAL_GREAT_FISH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal Great Fish",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MEAL_BAD_FISH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal Bad Fish",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MEAL_FOLLOWER_MEAT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal Follower Meat",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MEAL_GREAT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal Great",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MEAL_MUSHROOMS:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal Mushrooms",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MEAL_POOP:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal Poop",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MEAL_ROTTEN:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal Rotten",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MEAL_BERRIES:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal Berries",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MEAL_DEADLY:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal Deadly",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MEAL_BAD_MIXED:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal Bad Mixed",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MEAL_MEDIUM_MIXED:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal Medium Mixed",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MEAL_GREAT_MIXED:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal Great Mixed",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MEAL_MEDIUM_VEG:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal Medium Veg",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MEAL_GREAT_MEAT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal Great Meat",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.MEAL_BAD_MEAT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Meal Bad Meat",
				IgnoreGrid = true
			};
			break;
		case StructureBrain.TYPES.BODY_PIT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building BodyPit"
			};
			break;
		case StructureBrain.TYPES.TAROT_BUILDING:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Tarot Building"
			};
			break;
		case StructureBrain.TYPES.CULT_UPGRADE1:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Cult Upgrade 1 Building"
			};
			break;
		case StructureBrain.TYPES.CULT_UPGRADE2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Cult Upgrade 2 Building"
			};
			break;
		case StructureBrain.TYPES.PLANK_PATH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Path Plank"
			};
			break;
		case StructureBrain.TYPES.TILE_PATH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Path Tile"
			};
			break;
		case StructureBrain.TYPES.TILE_FLOWERS:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Path Flowers"
			};
			break;
		case StructureBrain.TYPES.TILE_HAY:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Path Hay"
			};
			break;
		case StructureBrain.TYPES.TILE_BLOOD:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Path Blood"
			};
			break;
		case StructureBrain.TYPES.TILE_ROCKS:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Path Rocks"
			};
			break;
		case StructureBrain.TYPES.TILE_WATER:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Path Water"
			};
			break;
		case StructureBrain.TYPES.TILE_BRICKS:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Path Bricks"
			};
			break;
		case StructureBrain.TYPES.TILE_PLANKS:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Path Planks"
			};
			break;
		case StructureBrain.TYPES.TILE_REDGRASS:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Path RedGrass"
			};
			break;
		case StructureBrain.TYPES.TILE_SPOOKYPLANKS:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Path SpookyPlanks"
			};
			break;
		case StructureBrain.TYPES.TILE_GOLD:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Path Gold"
			};
			break;
		case StructureBrain.TYPES.TILE_FLOWERSROCKY:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Path FloweryRocky"
			};
			break;
		case StructureBrain.TYPES.TILE_MOSAIC:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Path Mosaic"
			};
			break;
		case StructureBrain.TYPES.PRISON:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building_Prison"
			};
			break;
		case StructureBrain.TYPES.HEALING_BAY:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Healing Bay"
			};
			break;
		case StructureBrain.TYPES.HEALING_BAY_2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Healing Bay 2"
			};
			break;
		case StructureBrain.TYPES.GRAVE2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building BodyPit"
			};
			break;
		case StructureBrain.TYPES.RESEARCH_1:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Research 1"
			};
			break;
		case StructureBrain.TYPES.RESEARCH_2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Research 1"
			};
			break;
		case StructureBrain.TYPES.ONE:
			structuresData = new StructuresData();
			break;
		case StructureBrain.TYPES.TWO:
			structuresData = new StructuresData();
			break;
		case StructureBrain.TYPES.THREE:
			structuresData = new StructuresData();
			break;
		case StructureBrain.TYPES.COOKING_FIRE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Kitchen"
			};
			break;
		case StructureBrain.TYPES.APOTHECARY:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Apothecary"
			};
			break;
		case StructureBrain.TYPES.SCARECROW:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Farm Scarecrow"
			};
			break;
		case StructureBrain.TYPES.HARVEST_TOTEM:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Farm Harvest Totem"
			};
			break;
		case StructureBrain.TYPES.ALCHEMY_CAULDRON:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Alchemy Cauldron"
			};
			break;
		case StructureBrain.TYPES.FOOD_STORAGE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Food Storage"
			};
			break;
		case StructureBrain.TYPES.FOOD_STORAGE_2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Food Storage Lvl2"
			};
			break;
		case StructureBrain.TYPES.SACRIFICIAL_TEMPLE_2:
			structuresData = new StructuresData();
			break;
		case StructureBrain.TYPES.MATING_TENT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Mating Tent"
			};
			break;
		case StructureBrain.TYPES.BLOODSTONE_MINE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building BloodstoneMine",
				LootToDrop = InventoryItem.ITEM_TYPE.STONE
			};
			break;
		case StructureBrain.TYPES.BLOODSTONE_MINE_2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building BloodstoneMine Lvl2",
				LootToDrop = InventoryItem.ITEM_TYPE.STONE
			};
			break;
		case StructureBrain.TYPES.CONFESSION_BOOTH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Confession Booth"
			};
			break;
		case StructureBrain.TYPES.DRUM_CIRCLE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Drum Circle"
			};
			break;
		case StructureBrain.TYPES.ENEMY_TRAP:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building EnemyTrap"
			};
			break;
		case StructureBrain.TYPES.FISHING_HUT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Fishing Hut"
			};
			break;
		case StructureBrain.TYPES.GHOST_CIRCLE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Ghost Circle"
			};
			break;
		case StructureBrain.TYPES.HIPPY_TENT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Hippy Tent"
			};
			break;
		case StructureBrain.TYPES.HUNTERS_HUT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Hunters Hut"
			};
			break;
		case StructureBrain.TYPES.KNUCKLEBONES_ARENA:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Knucklebones"
			};
			break;
		case StructureBrain.TYPES.MEDITATION_MAT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Meditation Mat"
			};
			break;
		case StructureBrain.TYPES.SCARIFICATIONIST:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Scarificationist"
			};
			break;
		case StructureBrain.TYPES.SECURITY_TURRET:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Security Turret"
			};
			break;
		case StructureBrain.TYPES.SECURITY_TURRET_2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Security Turret Lvl 2"
			};
			break;
		case StructureBrain.TYPES.WITCH_DOCTOR:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Witch Doctor"
			};
			break;
		case StructureBrain.TYPES.MAYPOLE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Maypole"
			};
			break;
		case StructureBrain.TYPES.FLOWER_GARDEN:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Flower Garden"
			};
			break;
		case StructureBrain.TYPES.RESOURCE:
			structuresData = (new StructuresData[3]
			{
				new StructuresData
				{
					PrefabPath = "Prefabs/Resources/Log Refined"
				},
				new StructuresData
				{
					PrefabPath = "Prefabs/Resources/Stone Refined"
				},
				new StructuresData
				{
					PrefabPath = "Prefabs/Resources/Rope"
				}
			})[variantIndex];
			break;
		case StructureBrain.TYPES.TREE:
			structuresData = (new StructuresData[4]
			{
				new StructuresData
				{
					PrefabPath = "Prefabs/Structures/Other/Tree1",
					DontLoadMe = true,
					ProgressTarget = 7f,
					LootToDrop = InventoryItem.ITEM_TYPE.LOG,
					LootCountToDrop = 5
				},
				new StructuresData
				{
					PrefabPath = "Prefabs/Structures/Other/Tree2",
					DontLoadMe = true,
					ProgressTarget = 5f,
					LootToDrop = InventoryItem.ITEM_TYPE.LOG,
					LootCountToDrop = 3
				},
				new StructuresData
				{
					PrefabPath = "Prefabs/Structures/Other/Tree3",
					DontLoadMe = true,
					ProgressTarget = 100f,
					LootToDrop = InventoryItem.ITEM_TYPE.LOG,
					LootCountToDrop = 15
				},
				new StructuresData
				{
					PrefabPath = "Prefabs/Structures/Other/Tree4",
					DontLoadMe = true,
					ProgressTarget = 3f,
					LootToDrop = InventoryItem.ITEM_TYPE.LOG,
					LootCountToDrop = 1
				}
			})[variantIndex];
			break;
		case StructureBrain.TYPES.TREE_HITTABLE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Tree Hittable"
			};
			break;
		case StructureBrain.TYPES.STONE_HITTABLE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Stone Hittable"
			};
			break;
		case StructureBrain.TYPES.BONES_HITTABLE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Bones Hittable"
			};
			break;
		case StructureBrain.TYPES.POOP_HITTABLE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/Poop Hittable"
			};
			break;
		case StructureBrain.TYPES.BERRY_BUSH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/BerryBush",
				DontLoadMe = true,
				ProgressTarget = 10f,
				MultipleLootToDrop = new List<InventoryItem.ITEM_TYPE>
				{
					InventoryItem.ITEM_TYPE.BERRY,
					InventoryItem.ITEM_TYPE.SEED
				},
				MultipleLootToDropChance = new List<int> { 85, 15 },
				LootCountToDropRange = new Vector2Int(3, 4),
				CropLootCountToDropRange = new Vector2Int(3, 4)
			};
			break;
		case StructureBrain.TYPES.RED_FLOWER_BUSH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/RedFlowerBush",
				DontLoadMe = true,
				ProgressTarget = 10f,
				MultipleLootToDrop = new List<InventoryItem.ITEM_TYPE>
				{
					InventoryItem.ITEM_TYPE.FLOWER_RED,
					InventoryItem.ITEM_TYPE.SEED_FLOWER_RED
				},
				MultipleLootToDropChance = new List<int> { 85, 15 },
				LootCountToDropRange = new Vector2Int(3, 4),
				CropLootCountToDropRange = new Vector2Int(3, 4)
			};
			break;
		case StructureBrain.TYPES.WHITE_FLOWER_BUSH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/WhiteFlowerBush",
				DontLoadMe = true,
				ProgressTarget = 10f,
				MultipleLootToDrop = new List<InventoryItem.ITEM_TYPE>
				{
					InventoryItem.ITEM_TYPE.FLOWER_WHITE,
					InventoryItem.ITEM_TYPE.SEED_FLOWER_WHITE
				},
				MultipleLootToDropChance = new List<int> { 85, 15 },
				LootCountToDropRange = new Vector2Int(3, 4),
				CropLootCountToDropRange = new Vector2Int(3, 4)
			};
			break;
		case StructureBrain.TYPES.PUMPKIN_BUSH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/PumpkinPatch",
				DontLoadMe = true,
				ProgressTarget = 10f,
				MultipleLootToDrop = new List<InventoryItem.ITEM_TYPE>
				{
					InventoryItem.ITEM_TYPE.PUMPKIN,
					InventoryItem.ITEM_TYPE.SEED_PUMPKIN
				},
				MultipleLootToDropChance = new List<int> { 85, 15 },
				LootCountToDropRange = new Vector2Int(3, 4),
				CropLootCountToDropRange = new Vector2Int(3, 4)
			};
			break;
		case StructureBrain.TYPES.MUSHROOM_BUSH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/MushroomPatch",
				DontLoadMe = true,
				ProgressTarget = 10f,
				MultipleLootToDrop = new List<InventoryItem.ITEM_TYPE>
				{
					InventoryItem.ITEM_TYPE.MUSHROOM_SMALL,
					InventoryItem.ITEM_TYPE.SEED_MUSHROOM
				},
				MultipleLootToDropChance = new List<int> { 85, 15 },
				LootCountToDropRange = new Vector2Int(3, 4),
				CropLootCountToDropRange = new Vector2Int(3, 4)
			};
			break;
		case StructureBrain.TYPES.BEETROOT_BUSH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/BeetrootBush",
				DontLoadMe = true,
				ProgressTarget = 10f,
				MultipleLootToDrop = new List<InventoryItem.ITEM_TYPE>
				{
					InventoryItem.ITEM_TYPE.BEETROOT,
					InventoryItem.ITEM_TYPE.SEED_BEETROOT
				},
				MultipleLootToDropChance = new List<int> { 85, 15 },
				LootCountToDropRange = new Vector2Int(3, 4),
				CropLootCountToDropRange = new Vector2Int(3, 4)
			};
			break;
		case StructureBrain.TYPES.CAULIFLOWER_BUSH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Other/CauliflowerBush",
				DontLoadMe = true,
				ProgressTarget = 10f,
				MultipleLootToDrop = new List<InventoryItem.ITEM_TYPE>
				{
					InventoryItem.ITEM_TYPE.CAULIFLOWER,
					InventoryItem.ITEM_TYPE.SEED_CAULIFLOWER
				},
				MultipleLootToDropChance = new List<int> { 85, 15 },
				LootCountToDropRange = new Vector2Int(3, 4),
				CropLootCountToDropRange = new Vector2Int(3, 4)
			};
			break;
		case StructureBrain.TYPES.BLOOD_STONE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Bloodstone",
				LootToDrop = InventoryItem.ITEM_TYPE.BLOOD_STONE,
				DontLoadMe = true
			};
			break;
		case StructureBrain.TYPES.GOLD_ORE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Gold Ore",
				LootToDrop = InventoryItem.ITEM_TYPE.GOLD_NUGGET,
				DontLoadMe = true
			};
			break;
		case StructureBrain.TYPES.OUTHOUSE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Outhouse"
			};
			break;
		case StructureBrain.TYPES.POOP:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Poop",
				IgnoreGrid = true,
				CanBeMoved = false,
				isDeletable = false
			};
			break;
		case StructureBrain.TYPES.OUTPOST_SHRINE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Outpost Shrine"
			};
			break;
		case StructureBrain.TYPES.LUMBER_MINE:
		{
			InventoryItem inventoryItem = new InventoryItem(InventoryItem.ITEM_TYPE.LOG);
			inventoryItem.quantity = 200;
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Lumber Mine",
				Inventory = new List<InventoryItem> { inventoryItem }
			};
			break;
		}
		case StructureBrain.TYPES.DECORATION_TORCH:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/BaseTorch"
			};
			break;
		case StructureBrain.TYPES.SACRIFICIAL_STONE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Sacrificial Stone",
				TILE_WIDTH = 5,
				TILE_HEIGHT = 5
			};
			break;
		case StructureBrain.TYPES.DECORATION_FLOWER_BOX_1:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Flower Box 1"
			};
			break;
		case StructureBrain.TYPES.DECORATION_SMALL_STONE_CANDLE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Small Stone Candle"
			};
			break;
		case StructureBrain.TYPES.DECORATION_FLAG_CROWN:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Flag Crown"
			};
			break;
		case StructureBrain.TYPES.DECORATION_FLAG_SCRIPTURE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Flag Scripture"
			};
			break;
		case StructureBrain.TYPES.DECORATION_WALL_TWIGS:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Wall Twigs"
			};
			break;
		case StructureBrain.TYPES.DECORATION_LAMB_FLAG_STATUE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Lamb Flag Statue"
			};
			break;
		case StructureBrain.TYPES.SHRINE_REDHEART:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/RedHeartShrine"
			};
			break;
		case StructureBrain.TYPES.SHRINE_BLUEHEART:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/BlueHeartShrine"
			};
			break;
		case StructureBrain.TYPES.SHRINE_BLACKHEART:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/BlackHeartShrine"
			};
			break;
		case StructureBrain.TYPES.SHRINE_DAMAGE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/DamageShrine"
			};
			break;
		case StructureBrain.TYPES.SHRINE_TAROT:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Base Shrine II"
			};
			break;
		case StructureBrain.TYPES.FARM_PLOT_SIGN:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Farm Plot Signpost"
			};
			break;
		case StructureBrain.TYPES.CHOPPING_SHRINE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Chopping Shrine"
			};
			break;
		case StructureBrain.TYPES.MINING_SHRINE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Mining Shrine"
			};
			break;
		case StructureBrain.TYPES.FORAGING_SHRINE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Building Foraging Shrine"
			};
			break;
		case StructureBrain.TYPES.EDIT_BUILDINGS:
			structuresData = new StructuresData();
			break;
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_1:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Crown Shrine_0"
			};
			break;
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_2:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Crown Shrine_1"
			};
			break;
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_3:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Crown Shrine_2"
			};
			break;
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_4:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Crown Shrine_3"
			};
			break;
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_5:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Deathcat Shrine"
			};
			break;
		case StructureBrain.TYPES.DECORATION_VIDEO:
			structuresData = new StructuresData
			{
				PrefabPath = "Assets/Prefabs/Placement Objects/Placement Object VideoDecoration"
			};
			break;
		case StructureBrain.TYPES.DECORATION_HALLOWEEN_CANDLE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Halloween Candle"
			};
			break;
		case StructureBrain.TYPES.DECORATION_HALLOWEEN_SKULL:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Halloween Skull"
			};
			break;
		case StructureBrain.TYPES.DECORATION_HALLOWEEN_PUMPKIN:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Halloween Pumpkin"
			};
			break;
		case StructureBrain.TYPES.DECORATION_HALLOWEEN_TREE:
			structuresData = new StructuresData
			{
				PrefabPath = "Prefabs/Structures/Buildings/Decoration Halloween Tree"
			};
			break;
		}
		if (structuresData != null)
		{
			structuresData.Type = Type;
			structuresData.VariantIndex = variantIndex;
			structuresData.IsUpgrade = IsUpgradeStructure(structuresData.Type);
			structuresData.UpgradeFromType = GetUpgradePrerequisite(structuresData.Type);
		}
		return structuresData;
	}

	public static List<ItemCost> GetResearchCostList(StructureBrain.TYPES Type, TypeAndPlacementObjects.Tier Tier)
	{
		switch (Type)
		{
		case StructureBrain.TYPES.ONE:
			return new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 15),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_SOUL, 150)
			};
		case StructureBrain.TYPES.TWO:
			return new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 15),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_SOUL, 300),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 3)
			};
		case StructureBrain.TYPES.THREE:
			return new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 60),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_SOUL, 450),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 20)
			};
		default:
			switch (Tier)
			{
			case TypeAndPlacementObjects.Tier.One:
				return new List<ItemCost>
				{
					new ItemCost(InventoryItem.ITEM_TYPE.BLACK_SOUL, 50)
				};
			case TypeAndPlacementObjects.Tier.Two:
				return new List<ItemCost>
				{
					new ItemCost(InventoryItem.ITEM_TYPE.BLACK_SOUL, 150)
				};
			case TypeAndPlacementObjects.Tier.Three:
				return new List<ItemCost>
				{
					new ItemCost(InventoryItem.ITEM_TYPE.BLACK_SOUL, 300)
				};
			default:
				return new List<ItemCost>();
			}
		}
	}

	public static int GetResearchCost(StructureBrain.TYPES Type)
	{
		if (Type == StructureBrain.TYPES.FARM_PLOT)
		{
			return 10;
		}
		return 5;
	}

	public static ResearchState GetResearchStateByType(StructureBrain.TYPES Types)
	{
		if (GetUnlocked(Types))
		{
			return ResearchState.Researched;
		}
		if (ResearchExists(Types))
		{
			return ResearchState.Researching;
		}
		return ResearchState.Unresearched;
	}

	public static bool HasTemple()
	{
		if (!DataManager.Instance.HasBuiltTemple1)
		{
			return false;
		}
		if (StructureManager.IsBuilding(StructureBrain.TYPES.TEMPLE))
		{
			return false;
		}
		return true;
	}

	public static bool RequiresTempleToBuild(StructureBrain.TYPES type)
	{
		if (type == StructureBrain.TYPES.SHRINE || type == StructureBrain.TYPES.COOKING_FIRE || type == StructureBrain.TYPES.TEMPLE)
		{
			return false;
		}
		return true;
	}

	public static bool GetUnlocked(StructureBrain.TYPES Types)
	{
		if (CheatConsole.AllBuildingsUnlocked)
		{
			return true;
		}
		switch (Types)
		{
		case StructureBrain.TYPES.COOKING_FIRE:
			return true;
		case StructureBrain.TYPES.SHRINE:
			return DataManager.Instance.CanBuildShrine;
		case StructureBrain.TYPES.SHRINE_II:
		case StructureBrain.TYPES.TEMPLE_II:
		case StructureBrain.TYPES.SHRINE_III:
		case StructureBrain.TYPES.SHRINE_IV:
		case StructureBrain.TYPES.TEMPLE_III:
		case StructureBrain.TYPES.TEMPLE_IV:
			return false;
		case StructureBrain.TYPES.TEMPLE:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Temple);
		case StructureBrain.TYPES.FARM_PLOT:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Farms);
		case StructureBrain.TYPES.BED:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Beds);
		case StructureBrain.TYPES.COMPOST_BIN:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Followers_Compost);
		case StructureBrain.TYPES.COMPOST_BIN_DEAD_BODY:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_NaturalBurial);
		case StructureBrain.TYPES.GRAVE:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Graves);
		case StructureBrain.TYPES.LUMBERJACK_STATION:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Economy_Lumberyard);
		case StructureBrain.TYPES.LUMBERJACK_STATION_2:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Economy_LumberyardII);
		case StructureBrain.TYPES.BLOODSTONE_MINE:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Economy_Mine);
		case StructureBrain.TYPES.BLOODSTONE_MINE_2:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Economy_MineII);
		case StructureBrain.TYPES.PROPAGANDA_SPEAKER:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_PropagandaSpeakers);
		case StructureBrain.TYPES.MISSIONARY:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Missionary);
		case StructureBrain.TYPES.MISSIONARY_II:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_MissionaryII);
		case StructureBrain.TYPES.MISSIONARY_III:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_MissionaryIII);
		case StructureBrain.TYPES.DEMON_SUMMONER:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_DemonSummoner);
		case StructureBrain.TYPES.DEMON_SUMMONER_2:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_DemonSummoner_2);
		case StructureBrain.TYPES.DEMON_SUMMONER_3:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_DemonSummoner_3);
		case StructureBrain.TYPES.SILO_FERTILISER:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_SiloFertiliser);
		case StructureBrain.TYPES.SURVEILLANCE:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Surveillance);
		case StructureBrain.TYPES.FISHING_HUT_2:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_FishingHut2);
		case StructureBrain.TYPES.OUTHOUSE_2:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Outhouse2);
		case StructureBrain.TYPES.SCARECROW_2:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Scarecrow2);
		case StructureBrain.TYPES.HARVEST_TOTEM_2:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_HarvestTotem2);
		case StructureBrain.TYPES.DANCING_FIREPIT:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_DancingFirepit);
		case StructureBrain.TYPES.HEALING_BAY:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_HealingBay);
		case StructureBrain.TYPES.HEALING_BAY_2:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_HealingBay2);
		case StructureBrain.TYPES.REFINERY:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Economy_Refinery);
		case StructureBrain.TYPES.REFINERY_2:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Economy_Refinery_2);
		case StructureBrain.TYPES.CHOPPING_SHRINE:
		case StructureBrain.TYPES.MINING_SHRINE:
		case StructureBrain.TYPES.FORAGING_SHRINE:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_ShrinesOfNature);
		case StructureBrain.TYPES.FARM_STATION:
		case StructureBrain.TYPES.SILO_SEED:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_FollowerFarming);
		case StructureBrain.TYPES.FARM_STATION_II:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_FarmStationII);
		case StructureBrain.TYPES.SCARECROW:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_AdvancedFarming);
		case StructureBrain.TYPES.HARVEST_TOTEM:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_HarvestTotem);
		case StructureBrain.TYPES.FOOD_STORAGE:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_FoodStorage);
		case StructureBrain.TYPES.FOOD_STORAGE_2:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_FoodStorage2);
		case StructureBrain.TYPES.JANITOR_STATION:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_JanitorStation);
		case StructureBrain.TYPES.BED_2:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_BetterBeds);
		case StructureBrain.TYPES.BED_3:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Beds3);
		case StructureBrain.TYPES.SHARED_HOUSE:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Shared_House);
		case StructureBrain.TYPES.OUTHOUSE:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Outhouse);
		case StructureBrain.TYPES.CONFESSION_BOOTH:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_ConfessionBooth);
		case StructureBrain.TYPES.PRISON:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Prison);
		case StructureBrain.TYPES.BODY_PIT:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_BodyPit);
		case StructureBrain.TYPES.SHRINE_PASSIVE:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Shrine_PassiveShrines);
		case StructureBrain.TYPES.SHRINE_PASSIVE_II:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Shrine_PassiveShrinesII);
		case StructureBrain.TYPES.SHRINE_PASSIVE_III:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Shrine_PassiveShrinesIII);
		case StructureBrain.TYPES.OFFERING_STATUE:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Shrine_OfferingStatue);
		case StructureBrain.TYPES.KITCHEN:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Kitchen);
		case StructureBrain.TYPES.MORGUE_1:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Morgue_1);
		case StructureBrain.TYPES.MORGUE_2:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Morgue_2);
		case StructureBrain.TYPES.CRYPT_1:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Crypt_1);
		case StructureBrain.TYPES.CRYPT_2:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Crypt_2);
		case StructureBrain.TYPES.CRYPT_3:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Crypt_3);
		case StructureBrain.TYPES.DECORATION_TREE:
		case StructureBrain.TYPES.DECORATION_STONE:
		case StructureBrain.TYPES.DECORATION_SMALL_STONE_CANDLE:
		case StructureBrain.TYPES.DECORATION_FLAG_CROWN:
		case StructureBrain.TYPES.DECORATION_WALL_TWIGS:
		case StructureBrain.TYPES.DECORATION_CANDLE_BARREL:
		case StructureBrain.TYPES.DECORATION_WALL_GRASS:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Decorations1);
		case StructureBrain.TYPES.DECORATION_BONE_ARCH:
		case StructureBrain.TYPES.DECORATION_BONE_BARREL:
		case StructureBrain.TYPES.DECORATION_BONE_CANDLE:
		case StructureBrain.TYPES.DECORATION_BONE_FLAG:
		case StructureBrain.TYPES.DECORATION_BONE_LANTERN:
		case StructureBrain.TYPES.DECORATION_BONE_PILLAR:
		case StructureBrain.TYPES.DECORATION_BONE_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_WALL_BONE:
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Decorations2);
		default:
			return DataManager.Instance.UnlockedStructures.Contains(Types);
		}
	}

	private static bool IsTempleBuiltOrBeingBuilt()
	{
		int num = Structure.CountStructuresOfType(StructureBrain.TYPES.TEMPLE) + Structure.CountStructuresOfType(StructureBrain.TYPES.TEMPLE_II) + Structure.CountStructuresOfType(StructureBrain.TYPES.TEMPLE_III) + Structure.CountStructuresOfType(StructureBrain.TYPES.TEMPLE_IV);
		foreach (Structures_BuildSite item in StructureManager.GetAllStructuresOfType<Structures_BuildSite>())
		{
			if (item.Data.ToBuildType == StructureBrain.TYPES.TEMPLE || item.Data.ToBuildType == StructureBrain.TYPES.TEMPLE_II || item.Data.ToBuildType == StructureBrain.TYPES.TEMPLE_III || item.Data.ToBuildType == StructureBrain.TYPES.TEMPLE_IV)
			{
				num++;
			}
		}
		return num > 0;
	}

	public static void CompleteResearch(StructureBrain.TYPES Types)
	{
		if (!GetUnlocked(Types))
		{
			DataManager.Instance.UnlockedStructures.Add(Types);
		}
		NotificationCentre.Instance.PlayGenericNotification(NotificationCentre.NotificationType.ResearchComplete);
		int num = -1;
		while (++num < DataManager.Instance.CurrentResearch.Count)
		{
			if (DataManager.Instance.CurrentResearch[num].Type == Types)
			{
				DataManager.Instance.CurrentResearch.RemoveAt(num);
			}
		}
	}

	public static void BeginResearch(StructureBrain.TYPES Types)
	{
		if (!ResearchExists(Types))
		{
			DataManager.Instance.CurrentResearch.Add(new ResearchObject(Types));
			Action onResearchBegin = OnResearchBegin;
			if (onResearchBegin != null)
			{
				onResearchBegin();
			}
		}
	}

	public static bool ResearchExists(StructureBrain.TYPES Types)
	{
		foreach (ResearchObject item in DataManager.Instance.CurrentResearch)
		{
			if (item.Type == Types)
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetAnyResearchExists()
	{
		return DataManager.Instance.CurrentResearch.Count > 0;
	}

	public static float ResearchProgressByType(StructureBrain.TYPES Types)
	{
		foreach (ResearchObject item in DataManager.Instance.CurrentResearch)
		{
			if (item.Type == Types)
			{
				return item.Progress / item.TargetProgress;
			}
		}
		return 0f;
	}

	public static float CurrentResearchProgress()
	{
		if (DataManager.Instance.CurrentResearch.Count > 0)
		{
			return DataManager.Instance.CurrentResearch[0].Progress / DataManager.Instance.CurrentResearch[0].TargetProgress;
		}
		return 0.5f;
	}

	public static float GetResearchTime(StructureBrain.TYPES Types)
	{
		return ResearchObject.GetResearchTimeInDays(Types);
	}

	public static StructureBrain.TYPES GetMealStructureType(InventoryItem.ITEM_TYPE mealType)
	{
		StructureBrain.TYPES result;
		Enum.TryParse<StructureBrain.TYPES>(mealType.ToString(), true, out result);
		return result;
	}

	public static InventoryItem.ITEM_TYPE GetMealType(StructureBrain.TYPES structureType)
	{
		InventoryItem.ITEM_TYPE result;
		Enum.TryParse<InventoryItem.ITEM_TYPE>(structureType.ToString(), true, out result);
		return result;
	}

	public static int BuildDurationGameMinutes(StructureBrain.TYPES Type)
	{
		switch (Type)
		{
		case StructureBrain.TYPES.DECORATION_SHRUB:
			return 15;
		case StructureBrain.TYPES.FARM_PLOT:
			return 30;
		case StructureBrain.TYPES.FARM_PLOT_SIGN:
			return 60;
		case StructureBrain.TYPES.BED:
			return 30;
		case StructureBrain.TYPES.GRAVE:
			return 30;
		case StructureBrain.TYPES.BODY_PIT:
			return 30;
		case StructureBrain.TYPES.REPAIRABLE_HEARTS:
			return 600;
		case StructureBrain.TYPES.REPAIRABLE_CURSES:
			return 600;
		case StructureBrain.TYPES.PLANK_PATH:
			return 10;
		case StructureBrain.TYPES.TILE_PATH:
			return 10;
		case StructureBrain.TYPES.TEMPLE:
			return 120;
		case StructureBrain.TYPES.TEMPLE_EXTENSION1:
			return 6000;
		case StructureBrain.TYPES.TEMPLE_EXTENSION2:
			return 9000;
		case StructureBrain.TYPES.SHRINE_BLUEHEART:
			return 600;
		case StructureBrain.TYPES.SHRINE_BLACKHEART:
			return 600;
		case StructureBrain.TYPES.SHRINE_REDHEART:
			return 600;
		case StructureBrain.TYPES.SHRINE_TAROT:
			return 600;
		case StructureBrain.TYPES.SHRINE_DAMAGE:
			return 600;
		case StructureBrain.TYPES.SHRINE:
			return 30;
		case StructureBrain.TYPES.COOKING_FIRE:
			return 20;
		case StructureBrain.TYPES.SHRINE_II:
			return 180;
		case StructureBrain.TYPES.SHRINE_III:
			return 180;
		case StructureBrain.TYPES.SHRINE_IV:
			return 180;
		case StructureBrain.TYPES.TEMPLE_II:
			return 300;
		case StructureBrain.TYPES.TEMPLE_III:
			return 300;
		case StructureBrain.TYPES.TEMPLE_IV:
			return 300;
		case StructureBrain.TYPES.DECORATION_TREE:
		case StructureBrain.TYPES.DECORATION_STONE:
		case StructureBrain.TYPES.DECORATION_SMALL_STONE_CANDLE:
		case StructureBrain.TYPES.DECORATION_FLAG_CROWN:
		case StructureBrain.TYPES.DECORATION_WALL_TWIGS:
		case StructureBrain.TYPES.DECORATION_CANDLE_BARREL:
		case StructureBrain.TYPES.DECORATION_WALL_GRASS:
			return 30;
		default:
			if (GetCategory(Type) == StructureBrain.Categories.AESTHETIC)
			{
				return 120;
			}
			return 300;
		}
	}

	public static List<StructuresData> GetStructuresList(List<StructureBrain.TYPES> filterTypeList, bool useWhitelist)
	{
		List<StructuresData> list = new List<StructuresData>();
		foreach (StructureBrain.TYPES allStructure in AllStructures)
		{
			if ((useWhitelist && filterTypeList.Contains(allStructure)) || (!useWhitelist && !filterTypeList.Contains(allStructure)))
			{
				list.Add(GetInfoByType(allStructure, 0));
			}
		}
		return list;
	}

	public static bool CategoryHasUnrevealed(StructureBrain.Categories Category)
	{
		foreach (StructureBrain.TYPES allStructure in AllStructures)
		{
			if (!HasRevealed(allStructure) && GetCategory(allStructure) == Category && GetOldAvailability(allStructure) == Availabilty.Available)
			{
				return true;
			}
		}
		return false;
	}

	public static bool CategoryHasAvailable(StructureBrain.Categories Category)
	{
		foreach (StructureBrain.TYPES allStructure in AllStructures)
		{
			if (GetCategory(allStructure) == Category && (GetOldAvailability(allStructure) == Availabilty.Available || GetOldAvailability(allStructure) == Availabilty.Locked))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CategoryHasResearchedBuilding(StructureBrain.Categories Category)
	{
		foreach (StructureBrain.TYPES allStructure in AllStructures)
		{
			if (GetCategory(allStructure) == Category && GetUnlocked(allStructure))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsUpgradeStructure(StructureBrain.TYPES type)
	{
		if (type == StructureBrain.TYPES.SHRINE && DataManager.Instance.HasBuiltShrine1)
		{
			return false;
		}
		return GetUpgradePrerequisite(type) != StructureBrain.TYPES.NONE;
	}

	public static List<StructureBrain.TYPES> GetUpgradePath(StructureBrain.TYPES type)
	{
		switch (type)
		{
		case StructureBrain.TYPES.SHRINE:
		case StructureBrain.TYPES.SHRINE_II:
		case StructureBrain.TYPES.SHRINE_III:
		case StructureBrain.TYPES.SHRINE_IV:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.SHRINE,
				StructureBrain.TYPES.SHRINE_II,
				StructureBrain.TYPES.SHRINE_III,
				StructureBrain.TYPES.SHRINE_IV
			};
		case StructureBrain.TYPES.TEMPLE:
		case StructureBrain.TYPES.TEMPLE_II:
		case StructureBrain.TYPES.TEMPLE_III:
		case StructureBrain.TYPES.TEMPLE_IV:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.TEMPLE,
				StructureBrain.TYPES.TEMPLE_II,
				StructureBrain.TYPES.TEMPLE_III,
				StructureBrain.TYPES.TEMPLE_IV
			};
		case StructureBrain.TYPES.TEMPLE_EXTENSION1:
		case StructureBrain.TYPES.TEMPLE_EXTENSION2:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.TEMPLE_BASE_EXTENSION1,
				StructureBrain.TYPES.TEMPLE_BASE_EXTENSION2
			};
		case StructureBrain.TYPES.MISSIONARY:
		case StructureBrain.TYPES.MISSIONARY_II:
		case StructureBrain.TYPES.MISSIONARY_III:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.MISSIONARY,
				StructureBrain.TYPES.MISSIONARY_II,
				StructureBrain.TYPES.MISSIONARY_III
			};
		case StructureBrain.TYPES.DEMON_SUMMONER:
		case StructureBrain.TYPES.DEMON_SUMMONER_2:
		case StructureBrain.TYPES.DEMON_SUMMONER_3:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.DEMON_SUMMONER,
				StructureBrain.TYPES.DEMON_SUMMONER_2,
				StructureBrain.TYPES.DEMON_SUMMONER_3
			};
		case StructureBrain.TYPES.OUTHOUSE:
		case StructureBrain.TYPES.OUTHOUSE_2:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.OUTHOUSE,
				StructureBrain.TYPES.OUTHOUSE_2
			};
		case StructureBrain.TYPES.SCARECROW:
		case StructureBrain.TYPES.SCARECROW_2:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.SCARECROW,
				StructureBrain.TYPES.SCARECROW_2
			};
		case StructureBrain.TYPES.HARVEST_TOTEM:
		case StructureBrain.TYPES.HARVEST_TOTEM_2:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.HARVEST_TOTEM,
				StructureBrain.TYPES.HARVEST_TOTEM_2
			};
		case StructureBrain.TYPES.SHRINE_PASSIVE:
		case StructureBrain.TYPES.SHRINE_PASSIVE_II:
		case StructureBrain.TYPES.SHRINE_PASSIVE_III:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.SHRINE_PASSIVE,
				StructureBrain.TYPES.SHRINE_PASSIVE_II,
				StructureBrain.TYPES.SHRINE_PASSIVE_III
			};
		case StructureBrain.TYPES.REFINERY:
		case StructureBrain.TYPES.REFINERY_2:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.REFINERY,
				StructureBrain.TYPES.REFINERY_2
			};
		case StructureBrain.TYPES.FARM_STATION:
		case StructureBrain.TYPES.FARM_STATION_II:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.FARM_STATION,
				StructureBrain.TYPES.FARM_STATION_II
			};
		case StructureBrain.TYPES.LUMBERJACK_STATION:
		case StructureBrain.TYPES.LUMBERJACK_STATION_2:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.LUMBERJACK_STATION,
				StructureBrain.TYPES.LUMBERJACK_STATION_2
			};
		case StructureBrain.TYPES.COOKING_FIRE:
			return new List<StructureBrain.TYPES> { StructureBrain.TYPES.COOKING_FIRE };
		case StructureBrain.TYPES.BED:
		case StructureBrain.TYPES.BED_2:
		case StructureBrain.TYPES.BED_3:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.BED,
				StructureBrain.TYPES.BED_2,
				StructureBrain.TYPES.BED_3
			};
		case StructureBrain.TYPES.GRAVE:
		case StructureBrain.TYPES.BODY_PIT:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.GRAVE,
				StructureBrain.TYPES.BODY_PIT
			};
		case StructureBrain.TYPES.HEALING_BAY:
		case StructureBrain.TYPES.HEALING_BAY_2:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.HEALING_BAY,
				StructureBrain.TYPES.HEALING_BAY_2
			};
		case StructureBrain.TYPES.FOOD_STORAGE:
		case StructureBrain.TYPES.FOOD_STORAGE_2:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.FOOD_STORAGE,
				StructureBrain.TYPES.FOOD_STORAGE_2
			};
		case StructureBrain.TYPES.BLOODSTONE_MINE:
		case StructureBrain.TYPES.BLOODSTONE_MINE_2:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.BLOODSTONE_MINE,
				StructureBrain.TYPES.BLOODSTONE_MINE_2
			};
		case StructureBrain.TYPES.MORGUE_1:
		case StructureBrain.TYPES.MORGUE_2:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.MORGUE_1,
				StructureBrain.TYPES.MORGUE_2
			};
		case StructureBrain.TYPES.CRYPT_1:
		case StructureBrain.TYPES.CRYPT_2:
		case StructureBrain.TYPES.CRYPT_3:
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.CRYPT_1,
				StructureBrain.TYPES.CRYPT_2,
				StructureBrain.TYPES.CRYPT_3
			};
		default:
			return null;
		}
	}

	public static StructureBrain.TYPES GetUpgradePrerequisite(StructureBrain.TYPES type)
	{
		switch (type)
		{
		case StructureBrain.TYPES.SHRINE:
			return StructureBrain.TYPES.SHRINE_BASE;
		case StructureBrain.TYPES.SHRINE_II:
			return StructureBrain.TYPES.SHRINE;
		case StructureBrain.TYPES.SHRINE_III:
			return StructureBrain.TYPES.SHRINE_II;
		case StructureBrain.TYPES.SHRINE_IV:
			return StructureBrain.TYPES.SHRINE_III;
		case StructureBrain.TYPES.TEMPLE_II:
			return StructureBrain.TYPES.TEMPLE;
		case StructureBrain.TYPES.TEMPLE_III:
			return StructureBrain.TYPES.TEMPLE_II;
		case StructureBrain.TYPES.TEMPLE_IV:
			return StructureBrain.TYPES.TEMPLE_III;
		case StructureBrain.TYPES.TEMPLE_EXTENSION1:
			return StructureBrain.TYPES.TEMPLE_BASE_EXTENSION1;
		case StructureBrain.TYPES.TEMPLE_EXTENSION2:
			return StructureBrain.TYPES.TEMPLE_BASE_EXTENSION2;
		case StructureBrain.TYPES.MISSIONARY_II:
			return StructureBrain.TYPES.MISSIONARY;
		case StructureBrain.TYPES.MISSIONARY_III:
			return StructureBrain.TYPES.MISSIONARY_II;
		case StructureBrain.TYPES.DEMON_SUMMONER_2:
			return StructureBrain.TYPES.DEMON_SUMMONER;
		case StructureBrain.TYPES.DEMON_SUMMONER_3:
			return StructureBrain.TYPES.DEMON_SUMMONER_2;
		case StructureBrain.TYPES.OUTHOUSE_2:
			return StructureBrain.TYPES.OUTHOUSE;
		case StructureBrain.TYPES.SCARECROW_2:
			return StructureBrain.TYPES.SCARECROW;
		case StructureBrain.TYPES.HARVEST_TOTEM_2:
			return StructureBrain.TYPES.HARVEST_TOTEM;
		case StructureBrain.TYPES.SHRINE_PASSIVE_II:
			return StructureBrain.TYPES.SHRINE_PASSIVE;
		case StructureBrain.TYPES.SHRINE_PASSIVE_III:
			return StructureBrain.TYPES.SHRINE_PASSIVE_II;
		case StructureBrain.TYPES.REFINERY_2:
			return StructureBrain.TYPES.REFINERY;
		case StructureBrain.TYPES.FARM_STATION_II:
			return StructureBrain.TYPES.FARM_STATION;
		case StructureBrain.TYPES.LUMBERJACK_STATION_2:
			return StructureBrain.TYPES.LUMBERJACK_STATION;
		case StructureBrain.TYPES.BED_2:
			return StructureBrain.TYPES.BED;
		case StructureBrain.TYPES.BED_3:
			return StructureBrain.TYPES.BED_2;
		case StructureBrain.TYPES.SHRINE_FUNDAMENTALIST:
			return StructureBrain.TYPES.SHRINE;
		case StructureBrain.TYPES.SHRINE_MISFIT:
			return StructureBrain.TYPES.SHRINE;
		case StructureBrain.TYPES.SHRINE_UTOPIANIST:
			return StructureBrain.TYPES.SHRINE;
		case StructureBrain.TYPES.GRAVE:
			return StructureBrain.TYPES.BODY_PIT;
		case StructureBrain.TYPES.CULT_UPGRADE2:
			return StructureBrain.TYPES.CULT_UPGRADE1;
		case StructureBrain.TYPES.HEALING_BAY_2:
			return StructureBrain.TYPES.HEALING_BAY;
		case StructureBrain.TYPES.RESEARCH_2:
			return StructureBrain.TYPES.CULT_UPGRADE1;
		case StructureBrain.TYPES.FOOD_STORAGE_2:
			return StructureBrain.TYPES.FOOD_STORAGE;
		case StructureBrain.TYPES.BLOODSTONE_MINE_2:
			return StructureBrain.TYPES.BLOODSTONE_MINE;
		case StructureBrain.TYPES.MORGUE_2:
			return StructureBrain.TYPES.MORGUE_1;
		case StructureBrain.TYPES.CRYPT_2:
			return StructureBrain.TYPES.CRYPT_1;
		case StructureBrain.TYPES.CRYPT_3:
			return StructureBrain.TYPES.CRYPT_2;
		default:
			return StructureBrain.TYPES.NONE;
		}
	}

	public static bool GetBuildOnlyOne(StructureBrain.TYPES Type)
	{
		switch (Type)
		{
		case StructureBrain.TYPES.SHRINE:
		case StructureBrain.TYPES.FARM_PLOT_SOZO:
		case StructureBrain.TYPES.COOKING_FIRE:
		case StructureBrain.TYPES.CONFESSION_BOOTH:
		case StructureBrain.TYPES.TEMPLE:
		case StructureBrain.TYPES.TEMPLE_EXTENSION1:
		case StructureBrain.TYPES.TEMPLE_EXTENSION2:
		case StructureBrain.TYPES.SHRINE_BLUEHEART:
		case StructureBrain.TYPES.SHRINE_REDHEART:
		case StructureBrain.TYPES.SHRINE_BLACKHEART:
		case StructureBrain.TYPES.SHRINE_TAROT:
		case StructureBrain.TYPES.SHRINE_DAMAGE:
		case StructureBrain.TYPES.SHRINE_II:
		case StructureBrain.TYPES.HEALING_BAY:
		case StructureBrain.TYPES.TEMPLE_II:
		case StructureBrain.TYPES.SHRINE_III:
		case StructureBrain.TYPES.SHRINE_IV:
		case StructureBrain.TYPES.TEMPLE_III:
		case StructureBrain.TYPES.TEMPLE_IV:
		case StructureBrain.TYPES.HEALING_BAY_2:
		case StructureBrain.TYPES.MISSIONARY:
		case StructureBrain.TYPES.SURVEILLANCE:
		case StructureBrain.TYPES.CHOPPING_SHRINE:
		case StructureBrain.TYPES.MINING_SHRINE:
		case StructureBrain.TYPES.FORAGING_SHRINE:
		case StructureBrain.TYPES.DEMON_SUMMONER:
		case StructureBrain.TYPES.DEMON_SUMMONER_2:
		case StructureBrain.TYPES.DEMON_SUMMONER_3:
		case StructureBrain.TYPES.MISSIONARY_II:
		case StructureBrain.TYPES.MISSIONARY_III:
		case StructureBrain.TYPES.DECORATION_MONSTERSHRINE:
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_1:
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_2:
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_3:
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_4:
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_5:
		case StructureBrain.TYPES.DECORATION_VIDEO:
		case StructureBrain.TYPES.DECORATION_PLUSH:
		case StructureBrain.TYPES.MORGUE_1:
		case StructureBrain.TYPES.MORGUE_2:
			return true;
		default:
			return false;
		}
	}

	public static DataManager.CultLevel GetRequiredLevel(StructureBrain.TYPES Type)
	{
		switch (Type)
		{
		case StructureBrain.TYPES.BED:
		case StructureBrain.TYPES.BLACKSMITH:
		case StructureBrain.TYPES.SHRINE:
		case StructureBrain.TYPES.FARM_PLOT_SOZO:
		case StructureBrain.TYPES.BODY_PIT:
		case StructureBrain.TYPES.TAROT_BUILDING:
		case StructureBrain.TYPES.CULT_UPGRADE1:
		case StructureBrain.TYPES.DANCING_FIREPIT:
		case StructureBrain.TYPES.PLANK_PATH:
		case StructureBrain.TYPES.PRISON:
		case StructureBrain.TYPES.LUMBERJACK_STATION:
		case StructureBrain.TYPES.COOKING_FIRE:
		case StructureBrain.TYPES.ALCHEMY_CAULDRON:
		case StructureBrain.TYPES.FOOD_STORAGE:
		case StructureBrain.TYPES.FISHING_HUT:
		case StructureBrain.TYPES.OUTHOUSE:
		case StructureBrain.TYPES.SHRINE_II:
		case StructureBrain.TYPES.COLLECTED_RESOURCES_CHEST:
		case StructureBrain.TYPES.HEALING_BAY:
		case StructureBrain.TYPES.SCARECROW:
		case StructureBrain.TYPES.HARVEST_TOTEM:
		case StructureBrain.TYPES.SHRINE_PASSIVE:
		case StructureBrain.TYPES.OFFERING_STATUE:
		case StructureBrain.TYPES.TILE_PATH:
		case StructureBrain.TYPES.CHOPPING_SHRINE:
		case StructureBrain.TYPES.MINING_SHRINE:
		case StructureBrain.TYPES.FORAGING_SHRINE:
		case StructureBrain.TYPES.MORGUE_1:
		case StructureBrain.TYPES.CRYPT_1:
			return DataManager.CultLevel.One;
		case StructureBrain.TYPES.FARM_STATION:
		case StructureBrain.TYPES.KITCHEN:
		case StructureBrain.TYPES.DECORATION_TREE:
		case StructureBrain.TYPES.DECORATION_STONE:
		case StructureBrain.TYPES.REPAIRABLE_HEARTS:
		case StructureBrain.TYPES.REPAIRABLE_CURSES:
		case StructureBrain.TYPES.BED_2:
		case StructureBrain.TYPES.FARM_PLOT:
		case StructureBrain.TYPES.GRAVE:
		case StructureBrain.TYPES.CULT_UPGRADE2:
		case StructureBrain.TYPES.SHRINE_III:
		case StructureBrain.TYPES.HEALING_BAY_2:
		case StructureBrain.TYPES.JANITOR_STATION:
		case StructureBrain.TYPES.FARM_STATION_II:
		case StructureBrain.TYPES.MORGUE_2:
		case StructureBrain.TYPES.CRYPT_2:
			return DataManager.CultLevel.Two;
		default:
			return DataManager.CultLevel.One;
		}
	}

	public static string GetBuildSfx(StructureBrain.TYPES Type)
	{
		switch (Type)
		{
		case StructureBrain.TYPES.BED:
		case StructureBrain.TYPES.BED_2:
		case StructureBrain.TYPES.BED_3:
		case StructureBrain.TYPES.DECORATION_FLAG_CROWN:
		case StructureBrain.TYPES.DECORATION_FLAG_SCRIPTURE:
		case StructureBrain.TYPES.DECORATION_WALL_GRASS:
		case StructureBrain.TYPES.DECORATION_FLAG_CRYSTAL:
		case StructureBrain.TYPES.DECORATION_FLAG_MUSHROOM:
		case StructureBrain.TYPES.DECORATION_SPIDER_PILLAR:
		case StructureBrain.TYPES.DECORATION_SPIDER_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_SPIDER_TORCH:
		case StructureBrain.TYPES.DECORATION_WALL_SPIDER:
		case StructureBrain.TYPES.DECORATION_PLUSH:
		case StructureBrain.TYPES.DECORATION_TWITCH_FLAG_CROWN:
		case StructureBrain.TYPES.DECORATION_TWITCH_MUSHROOM_BAG:
		case StructureBrain.TYPES.SHARED_HOUSE:
			return "event:/building/finished_fabric";
		case StructureBrain.TYPES.FARM_PLOT:
		case StructureBrain.TYPES.FARM_PLOT_SOZO:
		case StructureBrain.TYPES.DECORATION_HAY_BALE:
		case StructureBrain.TYPES.DECORATION_HAY_PILE:
			return "event:/building/finished_farmplot";
		case StructureBrain.TYPES.GRAVE:
		case StructureBrain.TYPES.BODY_PIT:
			return "event:/building/finished_grave";
		case StructureBrain.TYPES.BLACKSMITH:
		case StructureBrain.TYPES.SHRINE:
		case StructureBrain.TYPES.DECORATION_STONE:
		case StructureBrain.TYPES.DECORATION_SMALL_STONE_CANDLE:
		case StructureBrain.TYPES.DECORATION_LAMB_FLAG_STATUE:
		case StructureBrain.TYPES.SHRINE_BLUEHEART:
		case StructureBrain.TYPES.SHRINE_REDHEART:
		case StructureBrain.TYPES.SHRINE_BLACKHEART:
		case StructureBrain.TYPES.SHRINE_TAROT:
		case StructureBrain.TYPES.SHRINE_DAMAGE:
		case StructureBrain.TYPES.SHRINE_II:
		case StructureBrain.TYPES.SHRINE_PASSIVE:
		case StructureBrain.TYPES.SHRINE_III:
		case StructureBrain.TYPES.SHRINE_IV:
		case StructureBrain.TYPES.OFFERING_STATUE:
		case StructureBrain.TYPES.SHRINE_PASSIVE_II:
		case StructureBrain.TYPES.SHRINE_PASSIVE_III:
		case StructureBrain.TYPES.PROPAGANDA_SPEAKER:
		case StructureBrain.TYPES.DECORATION_FOUNTAIN:
		case StructureBrain.TYPES.DECORATION_STONE_CANDLE:
		case StructureBrain.TYPES.DECORATION_STONE_FLAG:
		case StructureBrain.TYPES.DECORATION_STONE_MUSHROOM:
		case StructureBrain.TYPES.DECORATION_WALL_STONE:
		case StructureBrain.TYPES.DECORATION_STONE_CANDLE_LAMP:
		case StructureBrain.TYPES.DECORATION_STONE_HENGE:
		case StructureBrain.TYPES.DECORATION_POND:
		case StructureBrain.TYPES.DECORATION_MONSTERSHRINE:
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_1:
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_2:
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_3:
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_4:
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_5:
		case StructureBrain.TYPES.DECORATION_VIDEO:
		case StructureBrain.TYPES.DECORATION_TWITCH_STONE_FLAG:
		case StructureBrain.TYPES.DECORATION_TWITCH_STONE_STATUE:
		case StructureBrain.TYPES.DECORATION_TWITCH_WOODEN_GUARDIAN:
		case StructureBrain.TYPES.DECORATION_HALLOWEEN_SKULL:
		case StructureBrain.TYPES.DECORATION_HALLOWEEN_CANDLE:
		case StructureBrain.TYPES.MORGUE_1:
		case StructureBrain.TYPES.MORGUE_2:
		case StructureBrain.TYPES.CRYPT_1:
		case StructureBrain.TYPES.CRYPT_2:
		case StructureBrain.TYPES.CRYPT_3:
		case StructureBrain.TYPES.DECORATION_OLDFAITH_CRYSTAL:
		case StructureBrain.TYPES.DECORATION_OLDFAITH_FLAG:
		case StructureBrain.TYPES.DECORATION_OLDFAITH_FOUNTAIN:
		case StructureBrain.TYPES.DECORATION_OLDFAITH_IRONMAIDEN:
		case StructureBrain.TYPES.DECORATION_OLDFAITH_SHRINE:
		case StructureBrain.TYPES.DECORATION_OLDFAITH_TORCH:
		case StructureBrain.TYPES.DECORATION_OLDFAITH_WALL:
		case StructureBrain.TYPES.TILE_OLDFAITH:
			return "event:/building/finished_stone";
		case StructureBrain.TYPES.FARM_STATION:
		case StructureBrain.TYPES.KITCHEN:
		case StructureBrain.TYPES.DECORATION_TREE:
		case StructureBrain.TYPES.PLANK_PATH:
		case StructureBrain.TYPES.PRISON:
		case StructureBrain.TYPES.LUMBERJACK_STATION:
		case StructureBrain.TYPES.COOKING_FIRE:
		case StructureBrain.TYPES.FOOD_STORAGE:
		case StructureBrain.TYPES.FOOD_STORAGE_2:
		case StructureBrain.TYPES.FISHING_HUT:
		case StructureBrain.TYPES.OUTHOUSE:
		case StructureBrain.TYPES.DECORATION_TORCH:
		case StructureBrain.TYPES.DECORATION_FLOWER_BOX_1:
		case StructureBrain.TYPES.DECORATION_FLOWER_BOX_2:
		case StructureBrain.TYPES.DECORATION_WALL_TWIGS:
		case StructureBrain.TYPES.COLLECTED_RESOURCES_CHEST:
		case StructureBrain.TYPES.HEALING_BAY:
		case StructureBrain.TYPES.SCARECROW:
		case StructureBrain.TYPES.HARVEST_TOTEM:
		case StructureBrain.TYPES.FARM_PLOT_SIGN:
		case StructureBrain.TYPES.TILE_PATH:
		case StructureBrain.TYPES.HEALING_BAY_2:
		case StructureBrain.TYPES.SILO_SEED:
		case StructureBrain.TYPES.SILO_FERTILISER:
		case StructureBrain.TYPES.SURVEILLANCE:
		case StructureBrain.TYPES.FISHING_HUT_2:
		case StructureBrain.TYPES.OUTHOUSE_2:
		case StructureBrain.TYPES.SCARECROW_2:
		case StructureBrain.TYPES.HARVEST_TOTEM_2:
		case StructureBrain.TYPES.DECORATION_BARROW:
		case StructureBrain.TYPES.DECORATION_BELL_STATUE:
		case StructureBrain.TYPES.DECORATION_BONE_ARCH:
		case StructureBrain.TYPES.DECORATION_BONE_BARREL:
		case StructureBrain.TYPES.DECORATION_BONE_CANDLE:
		case StructureBrain.TYPES.DECORATION_BONE_FLAG:
		case StructureBrain.TYPES.DECORATION_BONE_LANTERN:
		case StructureBrain.TYPES.DECORATION_BONE_PILLAR:
		case StructureBrain.TYPES.DECORATION_BONE_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_CANDLE_BARREL:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_LAMP:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_LIGHT:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_ROCK:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_STATUE:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_TREE:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_WINDOW:
		case StructureBrain.TYPES.DECORATION_FLOWER_ARCH:
		case StructureBrain.TYPES.DECORATION_POST_BOX:
		case StructureBrain.TYPES.DECORATION_PUMPKIN_PILE:
		case StructureBrain.TYPES.DECORATION_PUMPKIN_STOOL:
		case StructureBrain.TYPES.DECORATION_TORCH_BIG:
		case StructureBrain.TYPES.DECORATION_TWIG_LAMP:
		case StructureBrain.TYPES.DECORATION_WALL_BONE:
		case StructureBrain.TYPES.DECORATION_WREATH_STICK:
		case StructureBrain.TYPES.DECORATION_STUMP_LAMB_STATUE:
		case StructureBrain.TYPES.JANITOR_STATION:
		case StructureBrain.TYPES.DECORATION_BELL_SMALL:
		case StructureBrain.TYPES.DECORATION_BONE_SKULL_BIG:
		case StructureBrain.TYPES.DECORATION_BONE_SKULL_PILE:
		case StructureBrain.TYPES.DECORATION_FLOWER_BOTTLE:
		case StructureBrain.TYPES.DECORATION_FLOWER_CART:
		case StructureBrain.TYPES.DECORATION_LEAFY_FLOWER_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_LEAFY_LANTERN:
		case StructureBrain.TYPES.DECORATION_LEAFY_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_1:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_2:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_1:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_2:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_LARGE:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_SPIDER_LANTERN:
		case StructureBrain.TYPES.DECORATION_SPIDER_WEB_CROWN_SCULPTURE:
		case StructureBrain.TYPES.FARM_STATION_II:
		case StructureBrain.TYPES.DECORATION_FLOWERPOTWALL:
		case StructureBrain.TYPES.DECORATION_LEAFYLAMPPOST:
		case StructureBrain.TYPES.DECORATION_FLOWERVASE:
		case StructureBrain.TYPES.DECORATION_WATERINGCAN:
		case StructureBrain.TYPES.DECORATION_FLOWER_CART_SMALL:
		case StructureBrain.TYPES.DECORATION_WEEPINGSHRINE:
		case StructureBrain.TYPES.DECORATION_TWITCH_ROSE_BUSH:
		case StructureBrain.TYPES.DECORATION_HALLOWEEN_PUMPKIN:
		case StructureBrain.TYPES.DECORATION_HALLOWEEN_TREE:
			return "event:/building/finished_wood";
		default:
			return "event:/building/finished_wood";
		}
	}

	public static bool GetRequiresUnlock(StructureBrain.TYPES Type)
	{
		return false;
	}

	public static UIBuildMenuController.Category CategoryForType(StructureBrain.TYPES type)
	{
		switch (type)
		{
		case StructureBrain.TYPES.SHRINE:
		case StructureBrain.TYPES.DANCING_FIREPIT:
		case StructureBrain.TYPES.CONFESSION_BOOTH:
		case StructureBrain.TYPES.TEMPLE:
		case StructureBrain.TYPES.SHRINE_II:
		case StructureBrain.TYPES.TEMPLE_II:
		case StructureBrain.TYPES.SHRINE_PASSIVE:
		case StructureBrain.TYPES.SHRINE_III:
		case StructureBrain.TYPES.SHRINE_IV:
		case StructureBrain.TYPES.OFFERING_STATUE:
		case StructureBrain.TYPES.SHRINE_PASSIVE_II:
		case StructureBrain.TYPES.SHRINE_PASSIVE_III:
		case StructureBrain.TYPES.TEMPLE_III:
		case StructureBrain.TYPES.TEMPLE_IV:
		case StructureBrain.TYPES.MISSIONARY:
		case StructureBrain.TYPES.MISSIONARY_II:
		case StructureBrain.TYPES.MISSIONARY_III:
			return UIBuildMenuController.Category.Faith;
		case StructureBrain.TYPES.DECORATION_TREE:
		case StructureBrain.TYPES.DECORATION_STONE:
		case StructureBrain.TYPES.PLANK_PATH:
		case StructureBrain.TYPES.DECORATION_TORCH:
		case StructureBrain.TYPES.DECORATION_FLOWER_BOX_1:
		case StructureBrain.TYPES.DECORATION_SMALL_STONE_CANDLE:
		case StructureBrain.TYPES.DECORATION_FLAG_CROWN:
		case StructureBrain.TYPES.DECORATION_FLAG_SCRIPTURE:
		case StructureBrain.TYPES.DECORATION_WALL_TWIGS:
		case StructureBrain.TYPES.DECORATION_LAMB_FLAG_STATUE:
		case StructureBrain.TYPES.FARM_PLOT_SIGN:
		case StructureBrain.TYPES.TILE_PATH:
		case StructureBrain.TYPES.DECORATION_BARROW:
		case StructureBrain.TYPES.DECORATION_BELL_STATUE:
		case StructureBrain.TYPES.DECORATION_BONE_ARCH:
		case StructureBrain.TYPES.DECORATION_BONE_BARREL:
		case StructureBrain.TYPES.DECORATION_BONE_CANDLE:
		case StructureBrain.TYPES.DECORATION_BONE_FLAG:
		case StructureBrain.TYPES.DECORATION_BONE_LANTERN:
		case StructureBrain.TYPES.DECORATION_BONE_PILLAR:
		case StructureBrain.TYPES.DECORATION_BONE_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_CANDLE_BARREL:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_LAMP:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_LIGHT:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_ROCK:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_STATUE:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_TREE:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_WINDOW:
		case StructureBrain.TYPES.DECORATION_FLOWER_ARCH:
		case StructureBrain.TYPES.DECORATION_FOUNTAIN:
		case StructureBrain.TYPES.DECORATION_POST_BOX:
		case StructureBrain.TYPES.DECORATION_PUMPKIN_PILE:
		case StructureBrain.TYPES.DECORATION_PUMPKIN_STOOL:
		case StructureBrain.TYPES.DECORATION_STONE_CANDLE:
		case StructureBrain.TYPES.DECORATION_STONE_FLAG:
		case StructureBrain.TYPES.DECORATION_STONE_MUSHROOM:
		case StructureBrain.TYPES.DECORATION_TORCH_BIG:
		case StructureBrain.TYPES.DECORATION_TWIG_LAMP:
		case StructureBrain.TYPES.DECORATION_WALL_BONE:
		case StructureBrain.TYPES.DECORATION_WALL_STONE:
		case StructureBrain.TYPES.DECORATION_WALL_GRASS:
		case StructureBrain.TYPES.DECORATION_WREATH_STICK:
		case StructureBrain.TYPES.DECORATION_STUMP_LAMB_STATUE:
		case StructureBrain.TYPES.DECORATION_BELL_SMALL:
		case StructureBrain.TYPES.DECORATION_BONE_SKULL_BIG:
		case StructureBrain.TYPES.DECORATION_BONE_SKULL_PILE:
		case StructureBrain.TYPES.DECORATION_FLAG_CRYSTAL:
		case StructureBrain.TYPES.DECORATION_FLAG_MUSHROOM:
		case StructureBrain.TYPES.DECORATION_FLOWER_BOTTLE:
		case StructureBrain.TYPES.DECORATION_FLOWER_CART:
		case StructureBrain.TYPES.DECORATION_HAY_BALE:
		case StructureBrain.TYPES.DECORATION_HAY_PILE:
		case StructureBrain.TYPES.DECORATION_LEAFY_FLOWER_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_LEAFY_LANTERN:
		case StructureBrain.TYPES.DECORATION_LEAFY_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_1:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_2:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_1:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_2:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_LARGE:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_SPIDER_LANTERN:
		case StructureBrain.TYPES.DECORATION_SPIDER_PILLAR:
		case StructureBrain.TYPES.DECORATION_SPIDER_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_SPIDER_TORCH:
		case StructureBrain.TYPES.DECORATION_SPIDER_WEB_CROWN_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_STONE_CANDLE_LAMP:
		case StructureBrain.TYPES.DECORATION_STONE_HENGE:
		case StructureBrain.TYPES.DECORATION_WALL_SPIDER:
		case StructureBrain.TYPES.DECORATION_POND:
		case StructureBrain.TYPES.TILE_FLOWERS:
		case StructureBrain.TYPES.TILE_HAY:
		case StructureBrain.TYPES.TILE_PLANKS:
		case StructureBrain.TYPES.TILE_SPOOKYPLANKS:
		case StructureBrain.TYPES.TILE_REDGRASS:
		case StructureBrain.TYPES.TILE_ROCKS:
		case StructureBrain.TYPES.TILE_BRICKS:
		case StructureBrain.TYPES.TILE_BLOOD:
		case StructureBrain.TYPES.TILE_WATER:
		case StructureBrain.TYPES.TILE_GOLD:
		case StructureBrain.TYPES.TILE_MOSAIC:
		case StructureBrain.TYPES.TILE_FLOWERSROCKY:
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
			return UIBuildMenuController.Category.Aesthetic;
		default:
			return UIBuildMenuController.Category.Follower;
		}
	}

	public static StructureBrain.Categories GetCategory(StructureBrain.TYPES Type)
	{
		switch (Type)
		{
		case StructureBrain.TYPES.COOKING_FIRE:
			return StructureBrain.Categories.FOOD;
		case StructureBrain.TYPES.KITCHEN:
			return StructureBrain.Categories.FOOD;
		case StructureBrain.TYPES.HARVEST_TOTEM:
			return StructureBrain.Categories.FOOD;
		case StructureBrain.TYPES.SCARECROW:
			return StructureBrain.Categories.FOOD;
		case StructureBrain.TYPES.FOOD_STORAGE:
			return StructureBrain.Categories.FOOD;
		case StructureBrain.TYPES.FOOD_STORAGE_2:
			return StructureBrain.Categories.FOOD;
		case StructureBrain.TYPES.COMPOST_BIN:
			return StructureBrain.Categories.FOOD;
		case StructureBrain.TYPES.FARM_PLOT_SOZO:
			return StructureBrain.Categories.FOOD;
		case StructureBrain.TYPES.FARM_STATION:
			return StructureBrain.Categories.FOOD;
		case StructureBrain.TYPES.FARM_STATION_II:
			return StructureBrain.Categories.FOOD;
		case StructureBrain.TYPES.FARM_PLOT:
			return StructureBrain.Categories.FOOD;
		case StructureBrain.TYPES.REFINERY:
			return StructureBrain.Categories.FOOD;
		case StructureBrain.TYPES.REFINERY_2:
			return StructureBrain.Categories.FOOD;
		case StructureBrain.TYPES.BED:
			return StructureBrain.Categories.FOLLOWERS;
		case StructureBrain.TYPES.BODY_PIT:
			return StructureBrain.Categories.FOLLOWERS;
		case StructureBrain.TYPES.OUTHOUSE:
			return StructureBrain.Categories.FOLLOWERS;
		case StructureBrain.TYPES.BED_2:
			return StructureBrain.Categories.FOLLOWERS;
		case StructureBrain.TYPES.GRAVE:
			return StructureBrain.Categories.FOLLOWERS;
		case StructureBrain.TYPES.HEALING_BAY:
			return StructureBrain.Categories.FOLLOWERS;
		case StructureBrain.TYPES.HEALING_BAY_2:
			return StructureBrain.Categories.FOLLOWERS;
		case StructureBrain.TYPES.JANITOR_STATION:
			return StructureBrain.Categories.FOLLOWERS;
		case StructureBrain.TYPES.DECORATION_TREE:
		case StructureBrain.TYPES.DECORATION_STONE:
		case StructureBrain.TYPES.PLANK_PATH:
		case StructureBrain.TYPES.DECORATION_TORCH:
		case StructureBrain.TYPES.DECORATION_FLOWER_BOX_1:
		case StructureBrain.TYPES.DECORATION_SMALL_STONE_CANDLE:
		case StructureBrain.TYPES.DECORATION_FLAG_CROWN:
		case StructureBrain.TYPES.DECORATION_FLAG_SCRIPTURE:
		case StructureBrain.TYPES.DECORATION_WALL_TWIGS:
		case StructureBrain.TYPES.DECORATION_LAMB_FLAG_STATUE:
		case StructureBrain.TYPES.FARM_PLOT_SIGN:
		case StructureBrain.TYPES.TILE_PATH:
		case StructureBrain.TYPES.DECORATION_BARROW:
		case StructureBrain.TYPES.DECORATION_BELL_STATUE:
		case StructureBrain.TYPES.DECORATION_BONE_ARCH:
		case StructureBrain.TYPES.DECORATION_BONE_BARREL:
		case StructureBrain.TYPES.DECORATION_BONE_CANDLE:
		case StructureBrain.TYPES.DECORATION_BONE_FLAG:
		case StructureBrain.TYPES.DECORATION_BONE_LANTERN:
		case StructureBrain.TYPES.DECORATION_BONE_PILLAR:
		case StructureBrain.TYPES.DECORATION_BONE_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_CANDLE_BARREL:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_LAMP:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_LIGHT:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_ROCK:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_STATUE:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_TREE:
		case StructureBrain.TYPES.DECORATION_CRYSTAL_WINDOW:
		case StructureBrain.TYPES.DECORATION_FLOWER_ARCH:
		case StructureBrain.TYPES.DECORATION_FOUNTAIN:
		case StructureBrain.TYPES.DECORATION_POST_BOX:
		case StructureBrain.TYPES.DECORATION_PUMPKIN_PILE:
		case StructureBrain.TYPES.DECORATION_PUMPKIN_STOOL:
		case StructureBrain.TYPES.DECORATION_STONE_CANDLE:
		case StructureBrain.TYPES.DECORATION_STONE_FLAG:
		case StructureBrain.TYPES.DECORATION_STONE_MUSHROOM:
		case StructureBrain.TYPES.DECORATION_TORCH_BIG:
		case StructureBrain.TYPES.DECORATION_TWIG_LAMP:
		case StructureBrain.TYPES.DECORATION_WALL_BONE:
		case StructureBrain.TYPES.DECORATION_WALL_STONE:
		case StructureBrain.TYPES.DECORATION_WALL_GRASS:
		case StructureBrain.TYPES.DECORATION_WREATH_STICK:
		case StructureBrain.TYPES.DECORATION_STUMP_LAMB_STATUE:
		case StructureBrain.TYPES.DECORATION_BELL_SMALL:
		case StructureBrain.TYPES.DECORATION_BONE_SKULL_BIG:
		case StructureBrain.TYPES.DECORATION_BONE_SKULL_PILE:
		case StructureBrain.TYPES.DECORATION_FLAG_CRYSTAL:
		case StructureBrain.TYPES.DECORATION_FLAG_MUSHROOM:
		case StructureBrain.TYPES.DECORATION_FLOWER_BOTTLE:
		case StructureBrain.TYPES.DECORATION_FLOWER_CART:
		case StructureBrain.TYPES.DECORATION_HAY_BALE:
		case StructureBrain.TYPES.DECORATION_HAY_PILE:
		case StructureBrain.TYPES.DECORATION_LEAFY_FLOWER_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_LEAFY_LANTERN:
		case StructureBrain.TYPES.DECORATION_LEAFY_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_1:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_2:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_1:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_2:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_LARGE:
		case StructureBrain.TYPES.DECORATION_MUSHROOM_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_SPIDER_LANTERN:
		case StructureBrain.TYPES.DECORATION_SPIDER_PILLAR:
		case StructureBrain.TYPES.DECORATION_SPIDER_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_SPIDER_TORCH:
		case StructureBrain.TYPES.DECORATION_SPIDER_WEB_CROWN_SCULPTURE:
		case StructureBrain.TYPES.DECORATION_STONE_CANDLE_LAMP:
		case StructureBrain.TYPES.DECORATION_STONE_HENGE:
		case StructureBrain.TYPES.DECORATION_WALL_SPIDER:
		case StructureBrain.TYPES.DECORATION_POND:
		case StructureBrain.TYPES.TILE_FLOWERS:
		case StructureBrain.TYPES.TILE_HAY:
		case StructureBrain.TYPES.TILE_PLANKS:
		case StructureBrain.TYPES.TILE_SPOOKYPLANKS:
		case StructureBrain.TYPES.TILE_REDGRASS:
		case StructureBrain.TYPES.TILE_ROCKS:
		case StructureBrain.TYPES.TILE_BRICKS:
		case StructureBrain.TYPES.TILE_BLOOD:
		case StructureBrain.TYPES.TILE_WATER:
		case StructureBrain.TYPES.TILE_GOLD:
		case StructureBrain.TYPES.TILE_MOSAIC:
		case StructureBrain.TYPES.TILE_FLOWERSROCKY:
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
			return StructureBrain.Categories.AESTHETIC;
		case StructureBrain.TYPES.SHRINE:
		case StructureBrain.TYPES.DANCING_FIREPIT:
		case StructureBrain.TYPES.PRISON:
		case StructureBrain.TYPES.CONFESSION_BOOTH:
		case StructureBrain.TYPES.TEMPLE_BASE:
		case StructureBrain.TYPES.TEMPLE:
		case StructureBrain.TYPES.TEMPLE_EXTENSION1:
		case StructureBrain.TYPES.TEMPLE_EXTENSION2:
		case StructureBrain.TYPES.TEMPLE_BASE_EXTENSION1:
		case StructureBrain.TYPES.TEMPLE_BASE_EXTENSION2:
		case StructureBrain.TYPES.SHRINE_II:
		case StructureBrain.TYPES.TEMPLE_II:
		case StructureBrain.TYPES.SHRINE_III:
		case StructureBrain.TYPES.SHRINE_IV:
		case StructureBrain.TYPES.TEMPLE_III:
		case StructureBrain.TYPES.TEMPLE_IV:
		case StructureBrain.TYPES.CHOPPING_SHRINE:
		case StructureBrain.TYPES.MINING_SHRINE:
		case StructureBrain.TYPES.FORAGING_SHRINE:
			return StructureBrain.Categories.FAITH;
		case StructureBrain.TYPES.SHRINE_BLUEHEART:
		case StructureBrain.TYPES.SHRINE_REDHEART:
		case StructureBrain.TYPES.SHRINE_BLACKHEART:
		case StructureBrain.TYPES.SHRINE_TAROT:
		case StructureBrain.TYPES.SHRINE_DAMAGE:
			return StructureBrain.Categories.COMBAT;
		case StructureBrain.TYPES.LUMBERJACK_STATION:
		case StructureBrain.TYPES.FISHING_HUT:
		case StructureBrain.TYPES.COLLECTED_RESOURCES_CHEST:
			return StructureBrain.Categories.ECONOMY;
		default:
			return StructureBrain.Categories.TECH;
		}
	}

	public static void SetRevealed(StructureBrain.TYPES Type)
	{
		if (!DataManager.Instance.RevealedStructures.Contains(Type))
		{
			DataManager.Instance.RevealedStructures.Add(Type);
		}
	}

	public static bool HasRevealed(StructureBrain.TYPES Type)
	{
		return DataManager.Instance.RevealedStructures.Contains(Type);
	}

	public static Availabilty GetOldAvailability(StructureBrain.TYPES Type)
	{
		Availabilty availabilty = Availabilty.Hidden;
		availabilty = ((!GetUnlocked(Type)) ? Availabilty.Locked : Availabilty.Available);
		if (Type == StructureBrain.TYPES.SACRIFICIAL_TEMPLE)
		{
			availabilty = Availabilty.Hidden;
		}
		if (GetBuildOnlyOne(Type) && (Structure.CountStructuresOfType(Type) > 0 || BuildSitePlot.StructureOfTypeUnderConstruction(Type) || BuildSitePlotProject.StructureOfTypeUnderConstruction(Type)))
		{
			availabilty = Availabilty.Hidden;
		}
		if (Type == StructureBrain.TYPES.FARM_PLOT_SOZO && !DataManager.Instance.SozoQuestComplete)
		{
			availabilty = Availabilty.Hidden;
		}
		return availabilty;
	}

	public static bool HiddenUntilUnlocked(StructureBrain.TYPES structure)
	{
		return HiddenStructuresUntilUnlocked.Contains(structure);
	}

	private static int GetModifiedGold(int Cost)
	{
		return Mathf.CeilToInt((float)Cost * 2f);
	}

	public static List<ItemCost> GetCost(StructureBrain.TYPES Type)
	{
		List<ItemCost> list = new List<ItemCost>();
		switch (Type)
		{
		case StructureBrain.TYPES.COOKING_FIRE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 2)
			};
			break;
		case StructureBrain.TYPES.BED:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 5)
			};
			break;
		case StructureBrain.TYPES.BODY_PIT:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 5)
			};
			break;
		case StructureBrain.TYPES.MISSIONARY:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 20),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 20)
			};
			break;
		case StructureBrain.TYPES.FARM_PLOT:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1)
			};
			break;
		case StructureBrain.TYPES.LUMBERJACK_STATION:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 15),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 15)
			};
			break;
		case StructureBrain.TYPES.BLOODSTONE_MINE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 15)
			};
			break;
		case StructureBrain.TYPES.FARM_STATION:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 20),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10)
			};
			break;
		case StructureBrain.TYPES.SILO_SEED:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10)
			};
			break;
		case StructureBrain.TYPES.DEMON_SUMMONER:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10)
			};
			break;
		case StructureBrain.TYPES.SCARECROW:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 25),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 15)
			};
			break;
		case StructureBrain.TYPES.BED_2:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 3),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 5)
			};
			break;
		case StructureBrain.TYPES.PRISON:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 25),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10)
			};
			break;
		case StructureBrain.TYPES.OFFERING_STATUE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 20),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 15)
			};
			break;
		case StructureBrain.TYPES.SHRINE_PASSIVE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10)
			};
			break;
		case StructureBrain.TYPES.DANCING_FIREPIT:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 30),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 5)
			};
			break;
		case StructureBrain.TYPES.MORGUE_1:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 30),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 15)
			};
			break;
		case StructureBrain.TYPES.CRYPT_1:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 20),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 15)
			};
			break;
		case StructureBrain.TYPES.PROPAGANDA_SPEAKER:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 2),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 5)
			};
			break;
		case StructureBrain.TYPES.CONFESSION_BOOTH:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 3),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 5)
			};
			break;
		case StructureBrain.TYPES.HEALING_BAY:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.FLOWER_RED, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10)
			};
			break;
		case StructureBrain.TYPES.OUTHOUSE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10)
			};
			break;
		case StructureBrain.TYPES.MISSIONARY_II:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 40)
			};
			break;
		case StructureBrain.TYPES.DEMON_SUMMONER_2:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 1),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 3),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 2)
			};
			break;
		case StructureBrain.TYPES.SILO_FERTILISER:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10)
			};
			break;
		case StructureBrain.TYPES.FOOD_STORAGE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 3),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10)
			};
			break;
		case StructureBrain.TYPES.HARVEST_TOTEM:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 2),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10)
			};
			break;
		case StructureBrain.TYPES.KITCHEN:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 5)
			};
			break;
		case StructureBrain.TYPES.MORGUE_2:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 3)
			};
			break;
		case StructureBrain.TYPES.CRYPT_2:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 6),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 2)
			};
			break;
		case StructureBrain.TYPES.BED_3:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 2),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 1),
				new ItemCost(InventoryItem.ITEM_TYPE.POOP, 3)
			};
			break;
		case StructureBrain.TYPES.SHARED_HOUSE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 4),
				new ItemCost(InventoryItem.ITEM_TYPE.POOP, 10)
			};
			break;
		case StructureBrain.TYPES.JANITOR_STATION:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 3)
			};
			break;
		case StructureBrain.TYPES.REFINERY_2:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 3),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 3)
			};
			break;
		case StructureBrain.TYPES.FARM_STATION_II:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 5)
			};
			break;
		case StructureBrain.TYPES.COMPOST_BIN:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.SPIDER_WEB, 10)
			};
			break;
		case StructureBrain.TYPES.KITCHEN_II:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 25),
				new ItemCost(InventoryItem.ITEM_TYPE.CRYSTAL, 10)
			};
			break;
		case StructureBrain.TYPES.SCARECROW_2:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 5)
			};
			break;
		case StructureBrain.TYPES.HEALING_BAY_2:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.FLOWER_RED, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 5)
			};
			break;
		case StructureBrain.TYPES.OUTHOUSE_2:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 7),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.CRYSTAL, 10)
			};
			break;
		case StructureBrain.TYPES.LUMBERJACK_STATION_2:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 3),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 1),
				new ItemCost(InventoryItem.ITEM_TYPE.CRYSTAL, 5)
			};
			break;
		case StructureBrain.TYPES.BLOODSTONE_MINE_2:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 2),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 1),
				new ItemCost(InventoryItem.ITEM_TYPE.SPIDER_WEB, 10)
			};
			break;
		case StructureBrain.TYPES.MISSIONARY_III:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 20),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.CRYSTAL, 20)
			};
			break;
		case StructureBrain.TYPES.DEMON_SUMMONER_3:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 3),
				new ItemCost(InventoryItem.ITEM_TYPE.SPIDER_WEB, 15)
			};
			break;
		case StructureBrain.TYPES.FOOD_STORAGE_2:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 20),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 15)
			};
			break;
		case StructureBrain.TYPES.HARVEST_TOTEM_2:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 5)
			};
			break;
		case StructureBrain.TYPES.CRYPT_3:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 16),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 8)
			};
			break;
		case StructureBrain.TYPES.REFINERY:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10)
			};
			break;
		case StructureBrain.TYPES.GRAVE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 3),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10)
			};
			break;
		case StructureBrain.TYPES.COMPOST_BIN_DEAD_BODY:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 3)
			};
			break;
		case StructureBrain.TYPES.SURVEILLANCE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 15)
			};
			break;
		case StructureBrain.TYPES.SHRINE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 15)
			};
			break;
		case StructureBrain.TYPES.SHRINE_II:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 25)
			};
			break;
		case StructureBrain.TYPES.SHRINE_III:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 15),
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 15),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 30)
			};
			break;
		case StructureBrain.TYPES.SHRINE_IV:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 15)
			};
			break;
		case StructureBrain.TYPES.TEMPLE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 15),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 5)
			};
			break;
		case StructureBrain.TYPES.TEMPLE_II:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 20),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 25)
			};
			break;
		case StructureBrain.TYPES.TEMPLE_III:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 10)
			};
			break;
		case StructureBrain.TYPES.TEMPLE_IV:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 20),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 25)
			};
			break;
		case StructureBrain.TYPES.SHRINE_PASSIVE_II:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 15)
			};
			break;
		case StructureBrain.TYPES.SHRINE_PASSIVE_III:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 5)
			};
			break;
		case StructureBrain.TYPES.MINING_SHRINE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 5)
			};
			break;
		case StructureBrain.TYPES.CHOPPING_SHRINE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 25),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 5)
			};
			break;
		case StructureBrain.TYPES.FORAGING_SHRINE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 5)
			};
			break;
		case StructureBrain.TYPES.PLANK_PATH:
			list = new List<ItemCost>();
			break;
		case StructureBrain.TYPES.TILE_PATH:
			list = new List<ItemCost>();
			break;
		case StructureBrain.TYPES.DECORATION_STONE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 1)
			};
			break;
		case StructureBrain.TYPES.DECORATION_TREE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 3)
			};
			break;
		case StructureBrain.TYPES.DECORATION_TORCH:
			return new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 2)
			};
		case StructureBrain.TYPES.DECORATION_FLOWER_BOX_1:
			return new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.FLOWER_RED, 2)
			};
		case StructureBrain.TYPES.DECORATION_SMALL_STONE_CANDLE:
			return new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 2)
			};
		case StructureBrain.TYPES.DECORATION_FLAG_CROWN:
			return new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 1)
			};
		case StructureBrain.TYPES.DECORATION_FLAG_SCRIPTURE:
			return new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 1)
			};
		case StructureBrain.TYPES.DECORATION_WALL_TWIGS:
			return new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 1)
			};
		case StructureBrain.TYPES.DECORATION_LAMB_FLAG_STATUE:
			return new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 5)
			};
		case StructureBrain.TYPES.FARM_PLOT_SIGN:
			return new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 2)
			};
		case StructureBrain.TYPES.COLLECTED_RESOURCES_CHEST:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 10)
			};
			break;
		case StructureBrain.TYPES.FISHING_HUT:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 20),
				new ItemCost(InventoryItem.ITEM_TYPE.FISH, 2)
			};
			break;
		case StructureBrain.TYPES.FISHING_HUT_2:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.FISH, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.LOG_REFINED, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 10)
			};
			break;
		case StructureBrain.TYPES.FARM_PLOT_SOZO:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, 5)
			};
			break;
		case StructureBrain.TYPES.DECORATION_BARROW:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 3),
				new ItemCost(InventoryItem.ITEM_TYPE.FLOWER_RED, 1)
			};
			break;
		case StructureBrain.TYPES.DECORATION_BELL_STATUE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 4)
			};
			break;
		case StructureBrain.TYPES.DECORATION_BONE_ARCH:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.BONE, 4)
			};
			break;
		case StructureBrain.TYPES.DECORATION_BONE_BARREL:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 1),
				new ItemCost(InventoryItem.ITEM_TYPE.BONE, 1)
			};
			break;
		case StructureBrain.TYPES.DECORATION_BONE_CANDLE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 1)
			};
			break;
		case StructureBrain.TYPES.DECORATION_BONE_FLAG:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.BONE, 3)
			};
			break;
		case StructureBrain.TYPES.DECORATION_BONE_LANTERN:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.BONE, 3)
			};
			break;
		case StructureBrain.TYPES.DECORATION_BONE_PILLAR:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.BONE, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_BONE_SCULPTURE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.BONE, 5)
			};
			break;
		case StructureBrain.TYPES.DECORATION_CANDLE_BARREL:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 1)
			};
			break;
		case StructureBrain.TYPES.DECORATION_CRYSTAL_LAMP:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.CRYSTAL, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_CRYSTAL_LIGHT:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.CRYSTAL, 3)
			};
			break;
		case StructureBrain.TYPES.DECORATION_CRYSTAL_ROCK:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.CRYSTAL, 3)
			};
			break;
		case StructureBrain.TYPES.DECORATION_CRYSTAL_STATUE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 2),
				new ItemCost(InventoryItem.ITEM_TYPE.CRYSTAL, 5)
			};
			break;
		case StructureBrain.TYPES.DECORATION_CRYSTAL_TREE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 3),
				new ItemCost(InventoryItem.ITEM_TYPE.CRYSTAL, 3)
			};
			break;
		case StructureBrain.TYPES.DECORATION_CRYSTAL_WINDOW:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.CRYSTAL, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_FLOWER_ARCH:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 10)
			};
			break;
		case StructureBrain.TYPES.DECORATION_FOUNTAIN:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 5)
			};
			break;
		case StructureBrain.TYPES.DECORATION_POST_BOX:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 1)
			};
			break;
		case StructureBrain.TYPES.DECORATION_PUMPKIN_PILE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.PUMPKIN, 3)
			};
			break;
		case StructureBrain.TYPES.DECORATION_PUMPKIN_STOOL:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 1),
				new ItemCost(InventoryItem.ITEM_TYPE.PUMPKIN, 1)
			};
			break;
		case StructureBrain.TYPES.DECORATION_STONE_CANDLE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 1)
			};
			break;
		case StructureBrain.TYPES.DECORATION_STONE_FLAG:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_STONE_MUSHROOM:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 3)
			};
			break;
		case StructureBrain.TYPES.DECORATION_TORCH_BIG:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 2),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_TWIG_LAMP:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_WALL_BONE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.BONE, 1)
			};
			break;
		case StructureBrain.TYPES.DECORATION_WALL_GRASS:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 1)
			};
			break;
		case StructureBrain.TYPES.DECORATION_WALL_STONE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 1)
			};
			break;
		case StructureBrain.TYPES.DECORATION_WREATH_STICK:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 4)
			};
			break;
		case StructureBrain.TYPES.DECORATION_STUMP_LAMB_STATUE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 3)
			};
			break;
		case StructureBrain.TYPES.DECORATION_BELL_SMALL:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_BONE_SKULL_BIG:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.BONE, 5)
			};
			break;
		case StructureBrain.TYPES.DECORATION_BONE_SKULL_PILE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.BONE, 5)
			};
			break;
		case StructureBrain.TYPES.DECORATION_FLAG_CRYSTAL:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.CRYSTAL, 3)
			};
			break;
		case StructureBrain.TYPES.DECORATION_FLAG_MUSHROOM:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 2),
				new ItemCost(InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, 1)
			};
			break;
		case StructureBrain.TYPES.DECORATION_FLOWER_BOTTLE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.FLOWER_RED, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_FLOWER_CART:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 3),
				new ItemCost(InventoryItem.ITEM_TYPE.FLOWER_RED, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_HAY_BALE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_HAY_PILE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_LEAFY_FLOWER_SCULPTURE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 5)
			};
			break;
		case StructureBrain.TYPES.DECORATION_LEAFY_LANTERN:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 5)
			};
			break;
		case StructureBrain.TYPES.DECORATION_LEAFY_SCULPTURE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 5)
			};
			break;
		case StructureBrain.TYPES.DECORATION_MUSHROOM_1:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_MUSHROOM_2:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_1:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_2:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_MUSHROOM_CANDLE_LARGE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, 5)
			};
			break;
		case StructureBrain.TYPES.DECORATION_MUSHROOM_SCULPTURE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, 35),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 10)
			};
			break;
		case StructureBrain.TYPES.DECORATION_SPIDER_LANTERN:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.SPIDER_WEB, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_SPIDER_PILLAR:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.SPIDER_WEB, 8)
			};
			break;
		case StructureBrain.TYPES.DECORATION_SPIDER_SCULPTURE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.SPIDER_WEB, 10)
			};
			break;
		case StructureBrain.TYPES.DECORATION_SPIDER_TORCH:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.SPIDER_WEB, 3)
			};
			break;
		case StructureBrain.TYPES.DECORATION_SPIDER_WEB_CROWN_SCULPTURE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.SPIDER_WEB, 3)
			};
			break;
		case StructureBrain.TYPES.DECORATION_STONE_CANDLE_LAMP:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_STONE_HENGE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_WALL_SPIDER:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.SPIDER_WEB, 3)
			};
			break;
		case StructureBrain.TYPES.DECORATION_POND:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_MONSTERSHRINE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 5)
			};
			break;
		case StructureBrain.TYPES.DECORATION_FLOWERPOTWALL:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 2),
				new ItemCost(InventoryItem.ITEM_TYPE.FLOWER_RED, 1)
			};
			break;
		case StructureBrain.TYPES.DECORATION_LEAFYLAMPPOST:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 2),
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 3)
			};
			break;
		case StructureBrain.TYPES.DECORATION_FLOWERVASE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.FLOWER_RED, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_WATERINGCAN:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 1),
				new ItemCost(InventoryItem.ITEM_TYPE.FLOWER_RED, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_FLOWER_CART_SMALL:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 1),
				new ItemCost(InventoryItem.ITEM_TYPE.FLOWER_RED, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_WEEPINGSHRINE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_PLUSH:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 20)
			};
			break;
		case StructureBrain.TYPES.DECORATION_TWITCH_FLAG_CROWN:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 3)
			};
			break;
		case StructureBrain.TYPES.DECORATION_TWITCH_MUSHROOM_BAG:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_TWITCH_ROSE_BUSH:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 5)
			};
			break;
		case StructureBrain.TYPES.DECORATION_TWITCH_STONE_FLAG:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 3)
			};
			break;
		case StructureBrain.TYPES.DECORATION_TWITCH_STONE_STATUE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 3)
			};
			break;
		case StructureBrain.TYPES.DECORATION_TWITCH_WOODEN_GUARDIAN:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 3)
			};
			break;
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_1:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.FLOWER_RED, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 3),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_2:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_3:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.CRYSTAL, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_4:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.SPIDER_WEB, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_BOSS_TROPHY_5:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 10)
			};
			break;
		case StructureBrain.TYPES.DECORATION_VIDEO:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 10),
				new ItemCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, 10)
			};
			break;
		case StructureBrain.TYPES.DECORATION_HALLOWEEN_CANDLE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 3),
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 1)
			};
			break;
		case StructureBrain.TYPES.DECORATION_HALLOWEEN_PUMPKIN:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.PUMPKIN, 3),
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_HALLOWEEN_TREE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 10)
			};
			break;
		case StructureBrain.TYPES.DECORATION_HALLOWEEN_SKULL:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.BONE, 2),
				new ItemCost(InventoryItem.ITEM_TYPE.FLOWER_RED, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_OLDFAITH_CRYSTAL:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE_REFINED, 1),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_OLDFAITH_FLAG:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1),
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_OLDFAITH_FOUNTAIN:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 5),
				new ItemCost(InventoryItem.ITEM_TYPE.FOLLOWER_MEAT, 1)
			};
			break;
		case StructureBrain.TYPES.DECORATION_OLDFAITH_IRONMAIDEN:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.LOG, 3),
				new ItemCost(InventoryItem.ITEM_TYPE.FOLLOWER_MEAT, 1)
			};
			break;
		case StructureBrain.TYPES.DECORATION_OLDFAITH_SHRINE:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 2),
				new ItemCost(InventoryItem.ITEM_TYPE.FLOWER_RED, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_OLDFAITH_TORCH:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 1),
				new ItemCost(InventoryItem.ITEM_TYPE.GRASS, 2)
			};
			break;
		case StructureBrain.TYPES.DECORATION_OLDFAITH_WALL:
			list = new List<ItemCost>
			{
				new ItemCost(InventoryItem.ITEM_TYPE.STONE, 1)
			};
			break;
		default:
			return new List<ItemCost>();
		}
		if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Economy_MineII))
		{
			foreach (ItemCost item in list)
			{
				item.CostValue = Mathf.CeilToInt((float)item.CostValue * UpgradeSystem.GetPriceModifier);
			}
		}
		foreach (ItemCost item2 in list)
		{
			if (item2.CostItem == InventoryItem.ITEM_TYPE.BLACK_GOLD || item2.CostItem == InventoryItem.ITEM_TYPE.GOLD_REFINED)
			{
				item2.CostValue = GetModifiedGold(item2.CostValue);
			}
		}
		return list;
	}

	public static string GetCostText(StructureBrain.TYPES type, bool includeCurrent)
	{
		string text = "";
		List<ItemCost> cost = GetCost(type);
		for (int i = 0; i < cost.Count; i++)
		{
			int itemQuantity = global::Inventory.GetItemQuantity((int)cost[i].CostItem);
			int costValue = cost[i].CostValue;
			if (includeCurrent)
			{
				text += ((costValue > itemQuantity) ? "<color=#ff0000>" : "<color=#00ff00>");
			}
			text += FontImageNames.GetIconByType(cost[i].CostItem);
			text = text + " x" + costValue;
			if (includeCurrent)
			{
				text = text + "   (" + itemQuantity + ")</color>";
			}
			text += "\n";
		}
		return text;
	}

	public static bool CanAfford(StructureBrain.TYPES type)
	{
		List<ItemCost> cost = GetCost(type);
		for (int i = 0; i < cost.Count; i++)
		{
			if (global::Inventory.GetItemQuantity((int)cost[i].CostItem) < cost[i].CostValue)
			{
				return false;
			}
		}
		return true;
	}

	public static bool CreateBuildSite(StructureBrain.TYPES Type)
	{
		if ((uint)(Type - 61) <= 1u || Type == StructureBrain.TYPES.RUBBLE_BIG)
		{
			return false;
		}
		return true;
	}

	public static bool CanBeFlipped(StructureBrain.TYPES type)
	{
		if (type == StructureBrain.TYPES.SHRINE || type == StructureBrain.TYPES.SHRINE_PASSIVE || (uint)(type - 154) <= 1u)
		{
			return false;
		}
		return true;
	}

	public void UpdateDictionaryLookup()
	{
		foreach (PlacementRegion.TileGridTile item in Grid)
		{
			if (!GridTileLookup.ContainsKey(item.Position))
			{
				GridTileLookup.Add(item.Position, item);
			}
		}
		List<PlacementRegion.TileGridTile> list = GridTileLookup.Values.ToList();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (!Grid.Contains(list[num]))
			{
				GridTileLookup.Remove(list[num].Position);
			}
		}
	}
}
