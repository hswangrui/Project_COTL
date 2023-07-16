using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class StructureBrain
{
	public enum Categories
	{
		FOLLOWERS,
		FOOD,
		FAITH,
		DUNGEON,
		RESOURCES,
		TECH,
		AESTHETIC,
		SECURITY,
		WORK,
		LEISURE,
		SLEEP,
		WORSHIP,
		STUDY,
		ECONOMY,
		CULT,
		COMBAT
	}

	[Serializable]
	public enum TYPES
	{
		NONE,
		BUILDER,
		BED,
		PORTAL,
		SACRIFICIAL_TEMPLE,
		WOOD_STORE,
		BLACKSMITH,
		TAVERN,
		FARM_STATION,
		WHEAT_SILO,
		KITCHEN,
		COOKED_FOOD_SILO,
		CROP,
		NIGHTMARE_MACHINE,
		MONSTER_HIVE,
		DEFENCE_TOWER,
		TREE,
		BUSH,
		GRASS,
		FIRE,
		ROCK,
		FOLLOWER_RECRUIT,
		SEED_FLOWER,
		COTTON_PLANT,
		HEALING_BATH,
		FIRE_PIT,
		BUILD_PLOT,
		SHRINE,
		BARRACKS,
		ASTROLOGIST,
		STORAGE_PIT,
		BUILD_SITE,
		ALTAR,
		PLACEMENT_REGION,
		DECORATION_TREE,
		DECORATION_STONE,
		REPAIRABLE_HEARTS,
		REPAIRABLE_ASTROLOGY,
		REPAIRABLE_VOODOO,
		REPAIRABLE_CURSES,
		BED_2,
		BED_3,
		SHRINE_FUNDAMENTALIST,
		SHRINE_MISFIT,
		SHRINE_UTOPIANIST,
		FARM_PLOT,
		GRAVE,
		DEAD_WORSHIPPER,
		VOMIT,
		DEMOLISH_STRUCTURE,
		MOVE_STRUCTURE,
		FARM_PLOT_SOZO,
		MEAL,
		BODY_PIT,
		TAROT_BUILDING,
		CULT_UPGRADE1,
		DANCING_FIREPIT,
		CULT_UPGRADE2,
		PLANK_PATH,
		PRISON,
		GRAVE2,
		RUBBLE,
		WEEDS,
		LUMBERJACK_STATION,
		LUMBERJACK_STATION_2,
		RESEARCH_1,
		RESEARCH_2,
		ONE,
		TWO,
		THREE,
		SACRIFICIAL_TEMPLE_2,
		COOKING_FIRE,
		ALCHEMY_CAULDRON,
		FOOD_STORAGE,
		FOOD_STORAGE_2,
		MATING_TENT,
		BLOODSTONE_MINE,
		BLOODSTONE_MINE_2,
		CONFESSION_BOOTH,
		DRUM_CIRCLE,
		ENEMY_TRAP,
		FISHING_HUT,
		GHOST_CIRCLE,
		HIPPY_TENT,
		HUNTERS_HUT,
		KNUCKLEBONES_ARENA,
		MEDITATION_MAT,
		SCARIFICATIONIST,
		SECURITY_TURRET,
		SECURITY_TURRET_2,
		WITCH_DOCTOR,
		MAYPOLE,
		FLOWER_GARDEN,
		BERRY_BUSH,
		SLEEPING_BAG,
		BLOOD_STONE,
		OUTHOUSE,
		POOP,
		OUTPOST_SHRINE,
		LUMBER_MINE,
		COMPOST_BIN,
		BLOODMOON_OFFERING,
		DECORATION_LAMB_STATUE,
		DECORATION_TORCH,
		PUMPKIN_BUSH,
		SACRIFICIAL_STONE,
		DECORATION_FLOWER_BOX_1,
		DECORATION_FLOWER_BOX_2,
		DECORATION_FLOWER_BOX_3,
		DECORATION_SMALL_STONE_CANDLE,
		DECORATION_FLAG_CROWN,
		DECORATION_FLAG_SCRIPTURE,
		DECORATION_WALL_TWIGS,
		DECORATION_LAMB_FLAG_STATUE,
		TEMPLE_BASE,
		TEMPLE,
		TEMPLE_EXTENSION1,
		TEMPLE_EXTENSION2,
		BUILDSITE_BUILDINGPROJECT,
		TEMPLE_BASE_EXTENSION1,
		TEMPLE_BASE_EXTENSION2,
		SHRINE_BLUEHEART,
		SHRINE_REDHEART,
		SHRINE_BLACKHEART,
		SHRINE_TAROT,
		SHRINE_DAMAGE,
		SHRINE_II,
		COLLECTED_RESOURCES_CHEST,
		SHRINE_BASE,
		MEAL_MEAT,
		MEAL_GREAT,
		MEAL_GRASS,
		MEAL_GOOD_FISH,
		MEAL_FOLLOWER_MEAT,
		HEALING_BAY,
		APOTHECARY,
		SCARECROW,
		HARVEST_TOTEM,
		FARM_PLOT_SIGN,
		MUSHROOM_BUSH,
		RED_FLOWER_BUSH,
		WHITE_FLOWER_BUSH,
		DECORATION_SHRUB,
		MEAL_MUSHROOMS,
		TEMPLE_II,
		MEAL_POOP,
		MEAL_ROTTEN,
		MISSION_SHRINE,
		REFINERY,
		SHRINE_PASSIVE,
		RESOURCE,
		SHRINE_III,
		SHRINE_IV,
		OFFERING_STATUE,
		SHRINE_PASSIVE_II,
		SHRINE_PASSIVE_III,
		TEMPLE_III,
		TEMPLE_IV,
		TILE_PATH,
		RUBBLE_BIG,
		WATER_SMALL,
		WATER_MEDIUM,
		WATER_BIG,
		RATAU_SHRINE,
		GOLD_ORE,
		PROPAGANDA_SPEAKER,
		HEALING_BAY_2,
		MISSIONARY,
		FEAST_TABLE,
		SILO_SEED,
		SILO_FERTILISER,
		SURVEILLANCE,
		FISHING_HUT_2,
		OUTHOUSE_2,
		SCARECROW_2,
		HARVEST_TOTEM_2,
		REFINERY_2,
		TREE_HITTABLE,
		STONE_HITTABLE,
		BONES_HITTABLE,
		POOP_HITTABLE,
		DECORATION_BARROW,
		DECORATION_BELL_STATUE,
		DECORATION_BONE_ARCH,
		DECORATION_BONE_BARREL,
		DECORATION_BONE_CANDLE,
		DECORATION_BONE_FLAG,
		DECORATION_BONE_LANTERN,
		DECORATION_BONE_PILLAR,
		DECORATION_BONE_SCULPTURE,
		DECORATION_CANDLE_BARREL,
		DECORATION_CRYSTAL_LAMP,
		DECORATION_CRYSTAL_LIGHT,
		DECORATION_CRYSTAL_ROCK,
		DECORATION_CRYSTAL_STATUE,
		DECORATION_CRYSTAL_TREE,
		DECORATION_CRYSTAL_WINDOW,
		DECORATION_FLOWER_ARCH,
		DECORATION_FOUNTAIN,
		DECORATION_POST_BOX,
		DECORATION_PUMPKIN_PILE,
		DECORATION_PUMPKIN_STOOL,
		DECORATION_STONE_CANDLE,
		DECORATION_STONE_FLAG,
		DECORATION_STONE_MUSHROOM,
		DECORATION_TORCH_BIG,
		DECORATION_TWIG_LAMP,
		DECORATION_WALL_BONE,
		DECORATION_WALL_STONE,
		DECORATION_WALL_GRASS,
		DECORATION_WREATH_STICK,
		DECORATION_STUMP_LAMB_STATUE,
		CHOPPING_SHRINE,
		MINING_SHRINE,
		FORAGING_SHRINE,
		EDIT_BUILDINGS,
		JANITOR_STATION,
		DECORATION_BELL_SMALL,
		DECORATION_BONE_SKULL_BIG,
		DECORATION_BONE_SKULL_PILE,
		DECORATION_FLAG_CRYSTAL,
		DECORATION_FLAG_MUSHROOM,
		DECORATION_FLOWER_BOTTLE,
		DECORATION_FLOWER_CART,
		DECORATION_HAY_BALE,
		DECORATION_HAY_PILE,
		DECORATION_LEAFY_FLOWER_SCULPTURE,
		DECORATION_LEAFY_LANTERN,
		DECORATION_LEAFY_SCULPTURE,
		DECORATION_MUSHROOM_1,
		DECORATION_MUSHROOM_2,
		DECORATION_MUSHROOM_CANDLE_1,
		DECORATION_MUSHROOM_CANDLE_2,
		DECORATION_MUSHROOM_CANDLE_LARGE,
		DECORATION_MUSHROOM_SCULPTURE,
		DECORATION_SPIDER_LANTERN,
		DECORATION_SPIDER_PILLAR,
		DECORATION_SPIDER_SCULPTURE,
		DECORATION_SPIDER_TORCH,
		DECORATION_SPIDER_WEB_CROWN_SCULPTURE,
		DECORATION_STONE_CANDLE_LAMP,
		DECORATION_STONE_HENGE,
		DECORATION_WALL_SPIDER,
		DECORATION_POND,
		DEMON_SUMMONER,
		COMPOST_BIN_DEAD_BODY,
		MEAL_GREAT_FISH,
		MEAL_BAD_FISH,
		BEETROOT_BUSH,
		CAULIFLOWER_BUSH,
		BED_1_COLLAPSED,
		BED_2_COLLAPSED,
		DEMON_SUMMONER_2,
		DEMON_SUMMONER_3,
		MEAL_BERRIES,
		MEAL_MEDIUM_VEG,
		MEAL_BAD_MIXED,
		MEAL_MEDIUM_MIXED,
		MEAL_GREAT_MIXED,
		MEAL_DEADLY,
		MEAL_BAD_MEAT,
		MEAL_GREAT_MEAT,
		FISHING_SPOT,
		MISSIONARY_II,
		MISSIONARY_III,
		KITCHEN_II,
		FARM_STATION_II,
		MEAL_BURNED,
		TILE_FLOWERS,
		TILE_HAY,
		TILE_PLANKS,
		TILE_SPOOKYPLANKS,
		TILE_REDGRASS,
		TILE_ROCKS,
		TILE_BRICKS,
		TILE_BLOOD,
		TILE_WATER,
		TILE_GOLD,
		TILE_MOSAIC,
		TILE_FLOWERSROCKY,
		DECORATION_MONSTERSHRINE,
		DECORATION_FLOWERPOTWALL,
		DECORATION_LEAFYLAMPPOST,
		DECORATION_FLOWERVASE,
		DECORATION_WATERINGCAN,
		DECORATION_FLOWER_CART_SMALL,
		DECORATION_WEEPINGSHRINE,
		DECORATION_BOSS_TROPHY_1,
		DECORATION_BOSS_TROPHY_2,
		DECORATION_BOSS_TROPHY_3,
		DECORATION_BOSS_TROPHY_4,
		DECORATION_BOSS_TROPHY_5,
		DECORATION_VIDEO,
		DECORATION_PLUSH,
		DECORATION_TWITCH_FLAG_CROWN,
		DECORATION_TWITCH_MUSHROOM_BAG,
		DECORATION_TWITCH_ROSE_BUSH,
		DECORATION_TWITCH_STONE_FLAG,
		DECORATION_TWITCH_STONE_STATUE,
		DECORATION_TWITCH_WOODEN_GUARDIAN,
		DECORATION_HALLOWEEN_PUMPKIN,
		DECORATION_HALLOWEEN_SKULL,
		DECORATION_HALLOWEEN_CANDLE,
		DECORATION_HALLOWEEN_TREE,
		MORGUE_1,
		MORGUE_2,
		CRYPT_1,
		CRYPT_2,
		CRYPT_3,
		SHARED_HOUSE,
		DECORATION_OLDFAITH_CRYSTAL,
		DECORATION_OLDFAITH_FLAG,
		DECORATION_OLDFAITH_FOUNTAIN,
		DECORATION_OLDFAITH_IRONMAIDEN,
		DECORATION_OLDFAITH_SHRINE,
		DECORATION_OLDFAITH_TORCH,
		DECORATION_OLDFAITH_WALL,
		TILE_OLDFAITH
	}

	public static List<TYPES> DecorationsToAdmire = new List<TYPES>
	{
		TYPES.DECORATION_LAMB_STATUE,
		TYPES.DECORATION_FOUNTAIN,
		TYPES.DECORATION_CRYSTAL_TREE,
		TYPES.DECORATION_CRYSTAL_WINDOW,
		TYPES.DECORATION_FLOWER_ARCH,
		TYPES.DECORATION_CRYSTAL_ROCK,
		TYPES.DECORATION_FLAG_SCRIPTURE,
		TYPES.DECORATION_LAMB_FLAG_STATUE,
		TYPES.DECORATION_BELL_STATUE,
		TYPES.DECORATION_BONE_ARCH,
		TYPES.DECORATION_BONE_CANDLE,
		TYPES.DECORATION_BONE_FLAG,
		TYPES.DECORATION_BONE_SCULPTURE,
		TYPES.DECORATION_BONE_LANTERN,
		TYPES.DECORATION_CRYSTAL_STATUE,
		TYPES.DECORATION_FLOWER_ARCH,
		TYPES.DECORATION_FOUNTAIN,
		TYPES.DECORATION_BONE_SKULL_BIG,
		TYPES.DECORATION_BONE_SKULL_PILE,
		TYPES.DECORATION_LEAFY_FLOWER_SCULPTURE,
		TYPES.DECORATION_LEAFY_SCULPTURE,
		TYPES.DECORATION_MUSHROOM_CANDLE_LARGE,
		TYPES.DECORATION_MUSHROOM_SCULPTURE,
		TYPES.DECORATION_SPIDER_PILLAR,
		TYPES.DECORATION_SPIDER_SCULPTURE,
		TYPES.DECORATION_SPIDER_WEB_CROWN_SCULPTURE,
		TYPES.DECORATION_STONE_HENGE,
		TYPES.DECORATION_POND,
		TYPES.DECORATION_MONSTERSHRINE,
		TYPES.DECORATION_WEEPINGSHRINE,
		TYPES.DECORATION_BOSS_TROPHY_1,
		TYPES.DECORATION_BOSS_TROPHY_2,
		TYPES.DECORATION_BOSS_TROPHY_3,
		TYPES.DECORATION_BOSS_TROPHY_4,
		TYPES.DECORATION_BOSS_TROPHY_5,
		TYPES.DECORATION_PLUSH,
		TYPES.DECORATION_TWITCH_STONE_STATUE,
		TYPES.DECORATION_TWITCH_WOODEN_GUARDIAN,
		TYPES.DECORATION_HALLOWEEN_TREE,
		TYPES.DECORATION_HALLOWEEN_CANDLE,
		TYPES.DECORATION_OLDFAITH_FOUNTAIN,
		TYPES.DECORATION_OLDFAITH_CRYSTAL
	};

	public StructuresData Data;

	public bool ReservedForTask;

	public bool ReservedByPlayer;

	public bool ForceRemoved;

	public Action<float> OnFuelModified;

	public Action OnItemDeposited;

	private static Dictionary<int, StructureBrain> _brainsByID = new Dictionary<int, StructureBrain>();

	public virtual bool IsFull
	{
		get
		{
			return false;
		}
	}

	public static IEnumerable<StructureBrain> AllBrains
	{
		get
		{
			return _brainsByID.Values;
		}
	}

	public static Dictionary<int, StructureBrain> BrainsByID
	{
		get
		{
			return _brainsByID;
		}
	}

	public virtual void OnAdded()
	{
	}

	public virtual void OnRemoved()
	{
	}

	public virtual void Init(StructuresData data)
	{
		Data = data;
	}

	public void UpdateFuel(int amountToRemove = 5)
	{
		if (Data.Fuel > 0 && (!Data.onlyDepleteWhenFullyFueled || (Data.onlyDepleteWhenFullyFueled && Data.FullyFueled)))
		{
			Data.Fuel = Mathf.Clamp(Data.Fuel - amountToRemove, 0, Data.MaxFuel);
			if (Data.Fuel <= 0)
			{
				Data.FullyFueled = false;
			}
			Action<float> onFuelModified = OnFuelModified;
			if (onFuelModified != null)
			{
				onFuelModified((float)Data.Fuel / (float)Data.MaxFuel);
			}
		}
	}

	public void Remove()
	{
		RemoveFromGrid();
		StructureManager.RemoveStructure(this);
	}

	public void RemoveFromGrid(Vector2Int gridTilePosition)
	{
		Structures_PlacementRegion structures_PlacementRegion = FindPlacementRegion();
		if (structures_PlacementRegion != null)
		{
			structures_PlacementRegion.ClearStructureFromGrid(this);
		}
		else if (PlacementRegion.Instance != null && PlacementRegion.Instance.structureBrain != null)
		{
			PlacementRegion.Instance.structureBrain.ClearStructureFromGrid(this, gridTilePosition);
		}
	}

	public void RemoveFromGrid()
	{
		RemoveFromGrid(Data.GridTilePosition);
	}

	public void AddToGrid(Vector2Int gridTilePosition)
	{
		Structures_PlacementRegion structures_PlacementRegion = FindPlacementRegion();
		if (structures_PlacementRegion != null)
		{
			structures_PlacementRegion.AddStructureToGrid(Data);
		}
		else if (PlacementRegion.Instance != null)
		{
			PlacementRegion.Instance.structureBrain.AddStructureToGrid(Data, gridTilePosition);
		}
	}

	public void AddToGrid()
	{
		AddToGrid(Data.GridTilePosition);
	}

	public Structures_PlacementRegion FindPlacementRegion()
	{
		Structures_PlacementRegion result = null;
		foreach (StructureBrain item in StructureManager.StructuresAtLocation(Data.Location))
		{
			if (item.Data.Type == TYPES.PLACEMENT_REGION && Data.PlacementRegionPosition == new Vector3Int((int)item.Data.Position.x, (int)item.Data.Position.y, 0))
			{
				result = item as Structures_PlacementRegion;
				break;
			}
		}
		return result;
	}

	public static Structures_PlacementRegion FindPlacementRegion(StructuresData Data)
	{
		Structures_PlacementRegion result = null;
		foreach (StructureBrain item in StructureManager.StructuresAtLocation(Data.Location))
		{
			if (item.Data.Type == TYPES.PLACEMENT_REGION && Data.PlacementRegionPosition == new Vector3Int((int)item.Data.Position.x, (int)item.Data.Position.y, 0))
			{
				result = item as Structures_PlacementRegion;
				break;
			}
		}
		return result;
	}

	public void DepositItem(InventoryItem.ITEM_TYPE type, int quantity = 1)
	{
		InventoryItem inventoryItem = null;
		foreach (InventoryItem item in Data.Inventory)
		{
			if (item.type == (int)type)
			{
				inventoryItem = item;
				break;
			}
		}
		if (inventoryItem == null)
		{
			Data.Inventory.Add(new InventoryItem(type, quantity));
		}
		else
		{
			inventoryItem.quantity += quantity;
		}
		Action onItemDeposited = OnItemDeposited;
		if (onItemDeposited != null)
		{
			onItemDeposited();
		}
	}

	public void DepositItemUnstacked(InventoryItem.ITEM_TYPE type)
	{
		Data.Inventory.Add(new InventoryItem(type));
		Action onItemDeposited = OnItemDeposited;
		if (onItemDeposited != null)
		{
			onItemDeposited();
		}
	}

	public void RemoveItems(InventoryItem.ITEM_TYPE type, int quantity)
	{
		for (int num = Data.Inventory.Count - 1; num >= 0; num--)
		{
			if (Data.Inventory[num].type == (int)type)
			{
				int num2 = quantity;
				quantity = Mathf.Clamp(quantity - Data.Inventory[num].quantity, 0, quantity);
				Data.Inventory[num].quantity -= num2;
				if (Data.Inventory[num].quantity <= 0)
				{
					Data.Inventory.RemoveAt(num);
				}
			}
		}
	}

	public virtual void ToDebugString(StringBuilder sb)
	{
		sb.AppendLine(string.Format("{0}; ({1},{2}); {3}", Data.Location, Data.GridX, Data.GridY, Data.Position));
	}

	public static StructureBrain CreateBrain(StructuresData data)
	{
		StructureBrain structureBrain;
		switch (data.Type)
		{
		case TYPES.RUBBLE:
		case TYPES.BLOOD_STONE:
		case TYPES.GOLD_ORE:
			structureBrain = new Structures_Rubble(0);
			break;
		case TYPES.RUBBLE_BIG:
			structureBrain = new Structures_Rubble(1);
			break;
		case TYPES.WATER_SMALL:
		case TYPES.WATER_MEDIUM:
		case TYPES.WATER_BIG:
			structureBrain = new Structures_Water();
			break;
		case TYPES.WEEDS:
			structureBrain = new Structures_Weeds();
			break;
		case TYPES.GRAVE:
		case TYPES.BODY_PIT:
		case TYPES.GRAVE2:
			structureBrain = new Structures_Grave();
			break;
		case TYPES.RESEARCH_1:
			structureBrain = new Structures_Research1();
			break;
		case TYPES.RESEARCH_2:
			structureBrain = new Structures_Research2();
			break;
		case TYPES.COLLECTED_RESOURCES_CHEST:
			structureBrain = new Structures_CollectedResourceChest();
			break;
		case TYPES.MISSION_SHRINE:
			structureBrain = new Structures_MissionShrine();
			break;
		case TYPES.COOKING_FIRE:
			structureBrain = new Structures_CookingFire();
			break;
		case TYPES.KITCHEN:
			structureBrain = new Structures_Kitchen();
			break;
		case TYPES.MORGUE_1:
			structureBrain = new Structures_Morgue();
			break;
		case TYPES.MORGUE_2:
			structureBrain = new Structures_Morgue();
			break;
		case TYPES.CRYPT_1:
			structureBrain = new Structures_Crypt();
			break;
		case TYPES.CRYPT_2:
			structureBrain = new Structures_Crypt();
			break;
		case TYPES.CRYPT_3:
			structureBrain = new Structures_Crypt();
			break;
		case TYPES.DANCING_FIREPIT:
			structureBrain = new Structures_DancingFirePit();
			break;
		case TYPES.MEAL:
		case TYPES.MEAL_MEAT:
		case TYPES.MEAL_GREAT:
		case TYPES.MEAL_GRASS:
		case TYPES.MEAL_GOOD_FISH:
		case TYPES.MEAL_FOLLOWER_MEAT:
		case TYPES.MEAL_MUSHROOMS:
		case TYPES.MEAL_POOP:
		case TYPES.MEAL_GREAT_FISH:
		case TYPES.MEAL_BAD_FISH:
		case TYPES.MEAL_BERRIES:
		case TYPES.MEAL_MEDIUM_VEG:
		case TYPES.MEAL_BAD_MIXED:
		case TYPES.MEAL_MEDIUM_MIXED:
		case TYPES.MEAL_GREAT_MIXED:
		case TYPES.MEAL_DEADLY:
		case TYPES.MEAL_BAD_MEAT:
		case TYPES.MEAL_GREAT_MEAT:
		case TYPES.MEAL_BURNED:
			structureBrain = new Structures_Meal();
			break;
		case TYPES.DEAD_WORSHIPPER:
			structureBrain = new Structures_DeadWorshipper();
			break;
		case TYPES.PLACEMENT_REGION:
			structureBrain = new Structures_PlacementRegion();
			break;
		case TYPES.FISHING_SPOT:
			structureBrain = new Structures_FishingSpot();
			break;
		case TYPES.BED:
			structureBrain = new Structures_Bed();
			break;
		case TYPES.BED_2:
			structureBrain = new Structures_Bed2();
			break;
		case TYPES.BED_3:
			structureBrain = new Structures_Bed3();
			break;
		case TYPES.SHARED_HOUSE:
			structureBrain = new Structures_SharedHouse();
			break;
		case TYPES.DEMON_SUMMONER:
			structureBrain = new Structures_Demon_Summoner();
			break;
		case TYPES.DEMON_SUMMONER_2:
			structureBrain = new Structures_Demon_Summoner();
			break;
		case TYPES.DEMON_SUMMONER_3:
			structureBrain = new Structures_Demon_Summoner();
			break;
		case TYPES.SLEEPING_BAG:
			structureBrain = new Structures_SleepingBag();
			break;
		case TYPES.FARM_STATION:
			structureBrain = new Structures_FarmerStation();
			break;
		case TYPES.FARM_STATION_II:
			structureBrain = new Structures_FarmerStation();
			break;
		case TYPES.FARM_PLOT:
			structureBrain = new Structures_FarmerPlot();
			break;
		case TYPES.JANITOR_STATION:
			structureBrain = new Structures_JanitorStation();
			break;
		case TYPES.SCARECROW:
		case TYPES.SCARECROW_2:
			structureBrain = new Structures_Scarecrow();
			break;
		case TYPES.HARVEST_TOTEM:
		case TYPES.HARVEST_TOTEM_2:
			structureBrain = new Structures_HarvestTotem();
			break;
		case TYPES.PROPAGANDA_SPEAKER:
			structureBrain = new Structures_PropagandaSpeaker();
			break;
		case TYPES.MISSIONARY:
			structureBrain = new Structures_Missionary();
			break;
		case TYPES.MISSIONARY_II:
			structureBrain = new Structures_Missionary();
			break;
		case TYPES.MISSIONARY_III:
			structureBrain = new Structures_Missionary();
			break;
		case TYPES.HEALING_BAY:
			structureBrain = new Structures_HealingBay(0);
			break;
		case TYPES.HEALING_BAY_2:
			structureBrain = new Structures_HealingBay(1);
			break;
		case TYPES.FEAST_TABLE:
			structureBrain = new Structures_FeastTable();
			break;
		case TYPES.SILO_SEED:
			structureBrain = new Structures_SiloSeed();
			break;
		case TYPES.SILO_FERTILISER:
			structureBrain = new Structures_SiloFertiliser();
			break;
		case TYPES.SURVEILLANCE:
			structureBrain = new Structures_Surveillance();
			break;
		case TYPES.OFFERING_STATUE:
			structureBrain = new Structures_OfferingShrine();
			break;
		case TYPES.REFINERY:
		case TYPES.REFINERY_2:
			structureBrain = new Structures_Refinery();
			break;
		case TYPES.LUMBERJACK_STATION:
		case TYPES.LUMBERJACK_STATION_2:
			structureBrain = new Structures_LumberjackStation();
			break;
		case TYPES.BLOODSTONE_MINE:
		case TYPES.BLOODSTONE_MINE_2:
			structureBrain = new Structures_MinerStation();
			break;
		case TYPES.SHRINE:
		case TYPES.SHRINE_FUNDAMENTALIST:
		case TYPES.SHRINE_UTOPIANIST:
		case TYPES.OUTPOST_SHRINE:
		case TYPES.SHRINE_II:
		case TYPES.SHRINE_III:
		case TYPES.SHRINE_IV:
			structureBrain = new Structures_Shrine();
			break;
		case TYPES.SHRINE_PASSIVE:
		case TYPES.SHRINE_PASSIVE_II:
		case TYPES.SHRINE_PASSIVE_III:
			structureBrain = new Structures_Shrine_Passive();
			break;
		case TYPES.RATAU_SHRINE:
			structureBrain = new Structures_Shrine_Ratau();
			break;
		case TYPES.SHRINE_MISFIT:
			structureBrain = new Structures_Shrine_Misfit();
			break;
		case TYPES.BUILD_SITE:
			structureBrain = new Structures_BuildSite();
			break;
		case TYPES.BUILDSITE_BUILDINGPROJECT:
			structureBrain = new Structures_BuildSiteProject();
			break;
		case TYPES.ALTAR:
			structureBrain = new Structures_Altar();
			break;
		case TYPES.FISHING_HUT:
			structureBrain = new Structures_FishingHut();
			break;
		case TYPES.FISHING_HUT_2:
			structureBrain = new Structures_FishingHut2();
			break;
		case TYPES.TEMPLE:
		case TYPES.TEMPLE_II:
		case TYPES.TEMPLE_III:
		case TYPES.TEMPLE_IV:
			structureBrain = new Structures_Temple();
			break;
		case TYPES.FOOD_STORAGE:
			structureBrain = new Structures_FoodStorage(0);
			break;
		case TYPES.FOOD_STORAGE_2:
			structureBrain = new Structures_FoodStorage(1);
			break;
		case TYPES.TREE:
			structureBrain = new Structures_Tree();
			break;
		case TYPES.BERRY_BUSH:
			structureBrain = new Structures_BerryBush();
			break;
		case TYPES.PUMPKIN_BUSH:
			structureBrain = new Structures_BerryBush();
			break;
		case TYPES.RED_FLOWER_BUSH:
			structureBrain = new Structures_BerryBush();
			break;
		case TYPES.WHITE_FLOWER_BUSH:
			structureBrain = new Structures_BerryBush();
			break;
		case TYPES.MUSHROOM_BUSH:
			structureBrain = new Structures_BerryBush();
			break;
		case TYPES.BEETROOT_BUSH:
			structureBrain = new Structures_BerryBush();
			break;
		case TYPES.CAULIFLOWER_BUSH:
			structureBrain = new Structures_BerryBush();
			break;
		case TYPES.LUMBER_MINE:
			structureBrain = new Structures_LumberMine();
			break;
		case TYPES.OUTHOUSE:
			structureBrain = new Structures_Outhouse();
			break;
		case TYPES.OUTHOUSE_2:
			structureBrain = new Structures_Outhouse();
			break;
		case TYPES.COMPOST_BIN:
			structureBrain = new Structures_CompostBin();
			break;
		case TYPES.COMPOST_BIN_DEAD_BODY:
			structureBrain = new Structures_DeadBodyCompost();
			break;
		case TYPES.CHOPPING_SHRINE:
			structureBrain = new Structures_ChoppingShrine();
			break;
		case TYPES.MINING_SHRINE:
			structureBrain = new Structures_MiningShrine();
			break;
		case TYPES.FORAGING_SHRINE:
			structureBrain = new Structures_ForagingShrine();
			break;
		case TYPES.PRISON:
			structureBrain = new Structures_Prison();
			break;
		default:
			structureBrain = new StructureBrain();
			break;
		}
		ApplyConfigToData(data);
		structureBrain.Init(data);
		_brainsByID.Add(data.ID, structureBrain);
		StructureManager.StructuresAtLocation(data.Location).Add(structureBrain);
		return structureBrain;
	}

	public static void ApplyConfigToData(StructuresData data)
	{
		StructuresData infoByType = StructuresData.GetInfoByType(data.Type, data.VariantIndex);
		data.PrefabPath = infoByType.PrefabPath;
		data.RemoveOnDie = infoByType.RemoveOnDie;
		data.ProgressTarget = infoByType.ProgressTarget;
		data.WorkIsRequiredForProgress = infoByType.WorkIsRequiredForProgress;
		data.IsUpgrade = infoByType.IsUpgrade;
		data.UpgradeFromType = infoByType.UpgradeFromType;
		data.RequiresType = infoByType.RequiresType;
		data.TILE_WIDTH = infoByType.TILE_WIDTH;
		data.TILE_HEIGHT = infoByType.TILE_HEIGHT;
		data.CanBeMoved = infoByType.CanBeMoved;
		data.CanBeRecycled = infoByType.CanBeRecycled;
		data.IsObstruction = infoByType.IsObstruction;
		data.DoesNotOccupyGrid = infoByType.DoesNotOccupyGrid;
		data.LootToDrop = infoByType.LootToDrop;
		data.LootCountToDrop = infoByType.LootCountToDrop;
		data.IsUpgrade = infoByType.IsUpgrade;
		data.IsUpgradeDestroyPrevious = infoByType.IsUpgradeDestroyPrevious;
		data.IgnoreGrid = infoByType.IgnoreGrid;
		data.IsBuildingProject = infoByType.IsBuildingProject;
	}

	public static void RemoveBrain(StructureBrain brain)
	{
		_brainsByID.Remove(brain.Data.ID);
		brain.OnRemoved();
	}

	public static StructureBrain FindBrainByID(int ID)
	{
		StructureBrain value = null;
		_brainsByID.TryGetValue(ID, out value);
		return value;
	}

	public static StructureBrain GetOrCreateBrain(StructuresData data)
	{
		StructureBrain structureBrain = FindBrainByID(data.ID);
		if (structureBrain == null)
		{
			structureBrain = CreateBrain(data);
			structureBrain.OnAdded();
		}
		return structureBrain;
	}

	public static bool IsPath(TYPES type)
	{
		if (type == TYPES.PLANK_PATH || type == TYPES.TILE_PATH || type == TYPES.TILE_HAY || type == TYPES.TILE_BLOOD || type == TYPES.TILE_ROCKS || type == TYPES.TILE_WATER || type == TYPES.TILE_BRICKS || type == TYPES.TILE_PLANKS || type == TYPES.TILE_FLOWERS || type == TYPES.TILE_REDGRASS || type == TYPES.TILE_SPOOKYPLANKS || type == TYPES.TILE_GOLD || type == TYPES.TILE_MOSAIC || type == TYPES.TILE_FLOWERSROCKY || type == TYPES.TILE_OLDFAITH)
		{
			return true;
		}
		return false;
	}
}
