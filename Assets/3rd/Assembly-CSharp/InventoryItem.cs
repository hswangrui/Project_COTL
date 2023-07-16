using System;
using System.Collections.Generic;
using I2.Loc;
using MMBiomeGeneration;
using MMRoomGeneration;
using UnityEngine;

public class InventoryItem
{
	public enum ITEM_CATEGORIES
	{
		NONE,
		LOGS,
		STONES,
		SEEDS,
		INGREDIENTS,
		MEALS,
		SOULS,
		POOP,
		COINS,
		GRASS
	}

	public enum ITEM_TYPE
	{
		NONE,
		LOG,
		STONE,
		ROCK2,
		ROCK3,
		SEED_SWORD,
		MEAT,
		WHEAT,
		SEED,
		BONE,
		SOUL,
		VINES,
		RED_HEART,
		HALF_HEART,
		BLUE_HEART,
		HALF_BLUE_HEART,
		TIME_TOKEN,
		GENERIC,
		STAINED_GLASS,
		FLOWERS,
		BLACK_GOLD,
		BERRY,
		MONSTER_HEART,
		BLUE_PRINT,
		WEAPON_CARD,
		CURSE_CARD,
		TRINKET_CARD,
		SOUL_FRAGMENT,
		FISH,
		MUSHROOM_SMALL,
		BLACK_SOUL,
		MUSHROOM_BIG,
		MEAL,
		FISH_SMALL,
		FISH_BIG,
		GRASS,
		THORNS,
		KEY_PIECE,
		KEY,
		POOP,
		FOUND_ITEM_DECORATION,
		FOUND_ITEM_WEAPON,
		FOUND_ITEM_CURSE,
		GIFT_SMALL,
		GIFT_MEDIUM,
		Necklace_1,
		Necklace_2,
		Necklace_3,
		Necklace_4,
		Necklace_5,
		PUMPKIN,
		SEED_PUMPKIN,
		FOUND_ITEM_FOLLOWERSKIN,
		BLACK_HEART,
		PERMANENT_HALF_HEART,
		FLOWER_RED,
		FLOWER_WHITE,
		MEAL_GRASS,
		MEAL_MEAT,
		MEAL_GREAT,
		MEAL_GOOD_FISH,
		MEAT_ROTTEN,
		FOLLOWER_MEAT,
		FOLLOWER_MEAT_ROTTEN,
		MEAL_ROTTEN,
		MEAL_FOLLOWER_MEAT,
		SEEDS,
		MEALS,
		INGREDIENTS,
		MEAL_POOP,
		SEED_MUSHROOM,
		SEED_FLOWER_WHITE,
		SEED_FLOWER_RED,
		GRASS2,
		GRASS3,
		GRASS4,
		GRASS5,
		FLOWER_PURPLE,
		SEED_TREE,
		MAP,
		MEAL_MUSHROOMS,
		LOG_REFINED,
		STONE_REFINED,
		GOLD_NUGGET,
		ROPE,
		FOLLOWERS,
		GOLD_REFINED,
		BLOOD_STONE,
		TRINKET_CARD_UNLOCKED,
		CRYSTAL,
		SPIDER_WEB,
		FISH_CRAB,
		FISH_LOBSTER,
		FISH_OCTOPUS,
		FISH_SQUID,
		FISH_SWORDFISH,
		FISH_BLOWFISH,
		BEETROOT,
		SEED_BEETROOT,
		MEAL_GREAT_FISH,
		MEAL_BAD_FISH,
		BEHOLDER_EYE,
		CAULIFLOWER,
		SEED_CAULIFLOWER,
		DISCIPLE_POINTS,
		MEAT_MORSEL,
		MEAL_BERRIES,
		MEAL_MEDIUM_VEG,
		MEAL_BAD_MIXED,
		MEAL_MEDIUM_MIXED,
		MEAL_GREAT_MIXED,
		MEAL_DEADLY,
		MEAL_BAD_MEAT,
		MEAL_GREAT_MEAT,
		TALISMAN,
		MEAL_BURNED,
		DOCTRINE_STONE,
		SHELL,
		RELIC,
		GOD_TEAR,
		CRYSTAL_DOCTRINE_FRAGMENT,
		CRYSTAL_DOCTRINE_STONE,
		Necklace_Loyalty,
		Necklace_Demonic,
		Necklace_Dark,
		Necklace_Light,
		Necklace_Missionary,
		Necklace_Gold_Skull,
		GOD_TEAR_FRAGMENT
	}

	public int type;

	public int quantity = 1;

	public int QuantityReserved;

	private static List<ITEM_TYPE> ItemsThatCanBeGivenToFollower = new List<ITEM_TYPE>
	{
		ITEM_TYPE.GIFT_MEDIUM,
		ITEM_TYPE.GIFT_SMALL,
		ITEM_TYPE.Necklace_1,
		ITEM_TYPE.Necklace_2,
		ITEM_TYPE.Necklace_3,
		ITEM_TYPE.Necklace_4,
		ITEM_TYPE.Necklace_5,
		ITEM_TYPE.Necklace_Dark,
		ITEM_TYPE.Necklace_Demonic,
		ITEM_TYPE.Necklace_Loyalty,
		ITEM_TYPE.Necklace_Light,
		ITEM_TYPE.Necklace_Missionary,
		ITEM_TYPE.Necklace_Gold_Skull
	};

	public int UnreservedQuantity
	{
		get
		{
			return quantity - QuantityReserved;
		}
	}

	public static List<ITEM_TYPE> AllSeeds
	{
		get
		{
			return new List<ITEM_TYPE>
			{
				ITEM_TYPE.SEED,
				ITEM_TYPE.SEED_PUMPKIN,
				ITEM_TYPE.SEED_MUSHROOM,
				ITEM_TYPE.SEED_FLOWER_RED,
				ITEM_TYPE.SEED_BEETROOT,
				ITEM_TYPE.SEED_CAULIFLOWER
			};
		}
	}

	public static List<ITEM_TYPE> AllPlantables
	{
		get
		{
			return new List<ITEM_TYPE>
			{
				ITEM_TYPE.BERRY,
				ITEM_TYPE.PUMPKIN,
				ITEM_TYPE.MUSHROOM_SMALL,
				ITEM_TYPE.FLOWER_RED,
				ITEM_TYPE.BEETROOT,
				ITEM_TYPE.CAULIFLOWER
			};
		}
	}

	public static List<ITEM_TYPE> AllFoods
	{
		get
		{
			return new List<ITEM_TYPE>
			{
				ITEM_TYPE.BERRY,
				ITEM_TYPE.PUMPKIN,
				ITEM_TYPE.BEETROOT,
				ITEM_TYPE.CAULIFLOWER,
				ITEM_TYPE.FISH,
				ITEM_TYPE.FISH_BIG,
				ITEM_TYPE.FISH_BLOWFISH,
				ITEM_TYPE.FISH_CRAB,
				ITEM_TYPE.FISH_LOBSTER,
				ITEM_TYPE.FISH_OCTOPUS,
				ITEM_TYPE.FISH_SMALL,
				ITEM_TYPE.FISH_SQUID,
				ITEM_TYPE.FISH_SWORDFISH,
				ITEM_TYPE.POOP,
				ITEM_TYPE.GRASS,
				ITEM_TYPE.FOLLOWER_MEAT,
				ITEM_TYPE.MEAT,
				ITEM_TYPE.MEAT_MORSEL
			};
		}
	}

	public static List<ITEM_TYPE> AllBurnableFuel
	{
		get
		{
			return new List<ITEM_TYPE>
			{
				ITEM_TYPE.LOG,
				ITEM_TYPE.GRASS
			};
		}
	}

	public InventoryItem()
	{
		type = 0;
		quantity = 1;
	}

	public InventoryItem(ITEM_TYPE Type)
	{
		type = (int)Type;
		quantity = 1;
	}

	public InventoryItem(ITEM_TYPE Type, int Quantity)
	{
		type = (int)Type;
		quantity = Quantity;
	}

	public void Init(int type, int quantity)
	{
		this.type = type;
		this.quantity = quantity;
	}

	public static void Spawn(GameObject ItemToSpawn, int quantity, Vector3 position, float StartSpeed = -1f)
	{
		if (!(ItemToSpawn == null))
		{
			GameObject gameObject = ItemToSpawn.Spawn((RoomManager.Instance != null) ? RoomManager.Instance.CurrentRoomPrefab.transform : GameObject.FindGameObjectWithTag("Unit Layer").transform);
			gameObject.transform.position = position;
			gameObject.transform.eulerAngles = Vector3.zero;
			PickUp component = gameObject.GetComponent<PickUp>();
			if (component != null && StartSpeed != -1f)
			{
				component.Speed = StartSpeed;
			}
		}
	}

	public static BlackSoul SpawnBlackSoul(int quantity, Vector3 position, bool giveXP = false, bool simulated = false)
	{
		if (!DataManager.Instance.EnabledSpells || PlayerFarming.Location == FollowerLocation.IntroDungeon || PlayerFleeceManager.FleeceSwapsCurseForRelic())
		{
			return null;
		}
		int num = quantity;
		UnityEngine.Random.Range(0, 360);
		BlackSoul blackSoul = null;
		Transform transform = null;
		BiomeGenerator instance = BiomeGenerator.Instance;
		transform = (((((object)instance != null) ? instance.CurrentRoom : null) == null) ? GameObject.FindGameObjectWithTag("Unit Layer").transform : BiomeGenerator.Instance.CurrentRoom.GameObject.transform);
		while (--num >= 0)
		{
			if (BlackSoul.BlackSouls.Count >= 20 && SettingsManager.Settings.Graphics.EnvironmentDetail == 0)
			{
				PlayerFarming.LeftoverSouls += 1f;
				continue;
			}
			blackSoul = BiomeConstants.Instance.SpawnBlackSouls(position, transform, 360f / (float)quantity * (float)num, simulated);
			blackSoul.GiveXP = giveXP;
		}
		return blackSoul;
	}

	public static ITEM_CATEGORIES GetItemCategory(ITEM_TYPE type)
	{
		switch (type)
		{
		case ITEM_TYPE.LOG:
			return ITEM_CATEGORIES.LOGS;
		case ITEM_TYPE.POOP:
			return ITEM_CATEGORIES.POOP;
		case ITEM_TYPE.SOUL:
			return ITEM_CATEGORIES.SOULS;
		case ITEM_TYPE.BLACK_GOLD:
			return ITEM_CATEGORIES.COINS;
		case ITEM_TYPE.GRASS:
			return ITEM_CATEGORIES.GRASS;
		default:
			return ITEM_CATEGORIES.NONE;
		}
	}

	public static ITEM_TYPE GetSeedType(ITEM_TYPE type)
	{
		switch (type)
		{
		case ITEM_TYPE.BERRY:
			return ITEM_TYPE.SEED;
		case ITEM_TYPE.FLOWER_RED:
			return ITEM_TYPE.SEED_FLOWER_RED;
		case ITEM_TYPE.FLOWER_WHITE:
			return ITEM_TYPE.SEED_FLOWER_WHITE;
		case ITEM_TYPE.MUSHROOM_SMALL:
			return ITEM_TYPE.SEED_MUSHROOM;
		case ITEM_TYPE.PUMPKIN:
			return ITEM_TYPE.SEED_PUMPKIN;
		case ITEM_TYPE.BEETROOT:
			return ITEM_TYPE.SEED_BEETROOT;
		case ITEM_TYPE.CAULIFLOWER:
			return ITEM_TYPE.SEED_CAULIFLOWER;
		case ITEM_TYPE.LOG:
			return ITEM_TYPE.SEED_TREE;
		default:
			return ITEM_TYPE.NONE;
		}
	}

	public static PickUp Spawn(ITEM_TYPE type, int quantity, Vector3 position, float StartSpeed = 4f, Action<PickUp> result = null)
	{
		if (type == ITEM_TYPE.NONE)
		{
			return null;
		}
		if (DungeonSandboxManager.Active && type != ITEM_TYPE.BLACK_GOLD && type != ITEM_TYPE.RELIC && type != ITEM_TYPE.HALF_HEART && type != ITEM_TYPE.HALF_BLUE_HEART && type != ITEM_TYPE.RED_HEART && type != ITEM_TYPE.BLUE_HEART && type != ITEM_TYPE.TRINKET_CARD && type != ITEM_TYPE.GOD_TEAR)
		{
			return null;
		}
		if (PlayerFarming.Location == FollowerLocation.IntroDungeon && type != ITEM_TYPE.BLACK_GOLD)
		{
			return null;
		}
		if (!DataManager.Instance.ShowLoyaltyBars && (type == ITEM_TYPE.GIFT_SMALL || type == ITEM_TYPE.GIFT_MEDIUM))
		{
			type = ITEM_TYPE.BLACK_GOLD;
		}
		bool flag = false;
		string text = "";
		switch (type)
		{
		case ITEM_TYPE.LOG:
			text = "Log";
			break;
		case ITEM_TYPE.THORNS:
			text = "Thorns";
			break;
		case ITEM_TYPE.STONE:
			text = "Rock1";
			break;
		case ITEM_TYPE.BLOOD_STONE:
			text = "Bloodstone";
			break;
		case ITEM_TYPE.ROCK2:
			text = "Rock2";
			break;
		case ITEM_TYPE.ROCK3:
			text = "Rock3";
			break;
		case ITEM_TYPE.SEED_SWORD:
			text = "Seed - Sword";
			break;
		case ITEM_TYPE.MEAT:
			text = "Meat";
			break;
		case ITEM_TYPE.WHEAT:
			text = "Wheat";
			break;
		case ITEM_TYPE.SEED:
			text = "Seed";
			break;
		case ITEM_TYPE.BONE:
			if (!DataManager.Instance.BonesEnabled)
			{
				return null;
			}
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.DestroyBodiesForBones);
			text = "VileBones";
			break;
		case ITEM_TYPE.GRASS:
			text = "Grass";
			break;
		case ITEM_TYPE.GRASS2:
			text = "Grass 2";
			break;
		case ITEM_TYPE.GRASS3:
			text = "Grass 3";
			break;
		case ITEM_TYPE.GRASS4:
			text = "Grass 4";
			break;
		case ITEM_TYPE.GRASS5:
			text = "Grass 5";
			break;
		case ITEM_TYPE.SOUL:
			text = "Soul";
			break;
		case ITEM_TYPE.VINES:
			text = "GildedVine";
			break;
		case ITEM_TYPE.BLUE_HEART:
			if (PlayerFleeceManager.FleecePreventsHealthPickups())
			{
				return null;
			}
			text = "Blue Heart";
			break;
		case ITEM_TYPE.RED_HEART:
			if (PlayerFleeceManager.FleecePreventsHealthPickups())
			{
				return null;
			}
			text = "Red Heart";
			break;
		case ITEM_TYPE.HALF_HEART:
			if (PlayerFleeceManager.FleecePreventsHealthPickups(true))
			{
				return null;
			}
			text = "Half Heart";
			break;
		case ITEM_TYPE.HALF_BLUE_HEART:
			if (PlayerFleeceManager.FleecePreventsHealthPickups())
			{
				return null;
			}
			text = "Half Blue Heart";
			break;
		case ITEM_TYPE.TIME_TOKEN:
			text = "Time Token";
			break;
		case ITEM_TYPE.GENERIC:
			text = "Generic Pick Up";
			break;
		case ITEM_TYPE.STAINED_GLASS:
			text = "StainedGlass";
			break;
		case ITEM_TYPE.FLOWERS:
			text = "SacredFlower";
			break;
		case ITEM_TYPE.BLACK_GOLD:
			text = ((!DungeonSandboxManager.Active) ? "BlackGold" : "ChallengeGold");
			break;
		case ITEM_TYPE.BERRY:
			text = "Berries";
			break;
		case ITEM_TYPE.PUMPKIN:
			text = "Pumpkin";
			break;
		case ITEM_TYPE.SEED_PUMPKIN:
			text = "Seed Pumpkin";
			break;
		case ITEM_TYPE.SOUL_FRAGMENT:
			text = "SoulFragment";
			break;
		case ITEM_TYPE.FISH:
			text = "Fish";
			break;
		case ITEM_TYPE.FISH_SMALL:
			text = "Fish Small";
			break;
		case ITEM_TYPE.FISH_BIG:
			text = "Fish Big";
			break;
		case ITEM_TYPE.FISH_CRAB:
			text = "Fish Crab";
			break;
		case ITEM_TYPE.FISH_LOBSTER:
			text = "Fish Lobster";
			break;
		case ITEM_TYPE.FISH_OCTOPUS:
			text = "Fish Octopus";
			break;
		case ITEM_TYPE.FISH_SQUID:
			text = "Fish Squid";
			break;
		case ITEM_TYPE.FISH_SWORDFISH:
			text = "Fish Swordfish";
			break;
		case ITEM_TYPE.FISH_BLOWFISH:
			text = "Fish Blowfish";
			break;
		case ITEM_TYPE.BEHOLDER_EYE:
			text = "Beholder Eye";
			break;
		case ITEM_TYPE.MUSHROOM_SMALL:
			text = "Mushroom Small";
			break;
		case ITEM_TYPE.MUSHROOM_BIG:
			text = "Mushroom Big";
			break;
		case ITEM_TYPE.BLACK_SOUL:
			text = "Black Soul";
			break;
		case ITEM_TYPE.MEAL:
			text = "Assets/Prefabs/Structures/Other/Meal.prefab";
			flag = true;
			break;
		case ITEM_TYPE.MEAL_BURNED:
			text = "Assets/Prefabs/Structures/Other/Meal Burned.prefab";
			flag = true;
			break;
		case ITEM_TYPE.MEAL_GRASS:
			text = "Assets/Prefabs/Structures/Other/Meal Grass.prefab";
			flag = true;
			break;
		case ITEM_TYPE.MEAL_MEAT:
			text = "Assets/Prefabs/Structures/Other/Meal Good.prefab";
			flag = true;
			break;
		case ITEM_TYPE.MEAL_GOOD_FISH:
			text = "Assets/Prefabs/Structures/Other/Meal Good Fish.prefab";
			flag = true;
			break;
		case ITEM_TYPE.MEAL_GREAT_FISH:
			text = "Assets/Prefabs/Structures/Other/Meal Great Fish.prefab";
			flag = true;
			break;
		case ITEM_TYPE.MEAL_BAD_FISH:
			text = "Assets/Prefabs/Structures/Other/Meal Bad Fish.prefab";
			flag = true;
			break;
		case ITEM_TYPE.MEAL_FOLLOWER_MEAT:
			text = "Assets/Prefabs/Structures/Other/Meal Follower Meat.prefab";
			flag = true;
			break;
		case ITEM_TYPE.MEAL_GREAT:
			text = "Assets/Prefabs/Structures/Other/Meal Great.prefab";
			flag = true;
			break;
		case ITEM_TYPE.MEAL_POOP:
			text = "Assets/Prefabs/Structures/Other/Meal Poop.prefab";
			flag = true;
			break;
		case ITEM_TYPE.MEAL_MUSHROOMS:
			text = "Assets/Prefabs/Structures/Other/Meal Mushrooms.prefab";
			flag = true;
			break;
		case ITEM_TYPE.MEAL_BAD_MIXED:
			text = "Assets/Prefabs/Structures/Other/Meal Bad Mixed.prefab";
			flag = true;
			break;
		case ITEM_TYPE.MEAL_MEDIUM_MIXED:
			text = "Assets/Prefabs/Structures/Other/Meal Medium Mixed.prefab";
			flag = true;
			break;
		case ITEM_TYPE.MEAL_GREAT_MIXED:
			text = "Assets/Prefabs/Structures/Other/Meal Great Mixed.prefab";
			flag = true;
			break;
		case ITEM_TYPE.MEAL_BERRIES:
			text = "Assets/Prefabs/Structures/Other/Meal Berries.prefab";
			flag = true;
			break;
		case ITEM_TYPE.MEAL_DEADLY:
			text = "Assets/Prefabs/Structures/Other/Meal Deadly.prefab";
			flag = true;
			break;
		case ITEM_TYPE.MEAL_MEDIUM_VEG:
			text = "Assets/Prefabs/Structures/Other/Meal Medium Veg.prefab";
			flag = true;
			break;
		case ITEM_TYPE.MEAL_BAD_MEAT:
			text = "Assets/Prefabs/Structures/Other/Meal Bad Meat.prefab";
			flag = true;
			break;
		case ITEM_TYPE.MEAL_GREAT_MEAT:
			text = "Assets/Prefabs/Structures/Other/Meal Great Meat.prefab";
			flag = true;
			break;
		case ITEM_TYPE.MEAT_ROTTEN:
			text = "Meat Rotten";
			break;
		case ITEM_TYPE.FOLLOWER_MEAT:
			text = "Follower Meat";
			break;
		case ITEM_TYPE.FOLLOWER_MEAT_ROTTEN:
			text = "Follower Meat";
			break;
		case ITEM_TYPE.MONSTER_HEART:
			text = "Monster Heart";
			break;
		case ITEM_TYPE.KEY_PIECE:
			text = "Key Piece";
			break;
		case ITEM_TYPE.POOP:
			text = "Poop";
			break;
		case ITEM_TYPE.FOUND_ITEM_DECORATION:
			text = "FoundItem";
			break;
		case ITEM_TYPE.FOUND_ITEM_WEAPON:
			text = "FoundItemWeapon";
			break;
		case ITEM_TYPE.FOUND_ITEM_CURSE:
			text = "FoundItemCurse";
			break;
		case ITEM_TYPE.MAP:
			text = "FoundItemMap";
			break;
		case ITEM_TYPE.GIFT_SMALL:
			text = "Gift Small";
			break;
		case ITEM_TYPE.GIFT_MEDIUM:
			text = "Gift Medium";
			break;
		case ITEM_TYPE.Necklace_1:
			text = "Necklace 1";
			break;
		case ITEM_TYPE.Necklace_2:
			text = "Necklace 2";
			break;
		case ITEM_TYPE.Necklace_3:
			text = "Necklace 3";
			break;
		case ITEM_TYPE.Necklace_4:
			text = "Necklace 4";
			break;
		case ITEM_TYPE.Necklace_5:
			text = "Necklace 5";
			break;
		case ITEM_TYPE.Necklace_Missionary:
			text = "Necklace Missionary";
			break;
		case ITEM_TYPE.Necklace_Light:
			text = "Necklace Light";
			break;
		case ITEM_TYPE.Necklace_Loyalty:
			text = "Necklace Loyalty";
			break;
		case ITEM_TYPE.Necklace_Gold_Skull:
			text = "Necklace Gold Skull";
			break;
		case ITEM_TYPE.Necklace_Demonic:
			text = "Necklace Demonic";
			break;
		case ITEM_TYPE.Necklace_Dark:
			text = "Necklace Dark";
			break;
		case ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN:
			text = "FoundItemSkin";
			break;
		case ITEM_TYPE.BLACK_HEART:
			if (PlayerFleeceManager.FleecePreventsHealthPickups())
			{
				return null;
			}
			text = "Black Heart";
			break;
		case ITEM_TYPE.TRINKET_CARD:
			text = "TarotCard";
			break;
		case ITEM_TYPE.PERMANENT_HALF_HEART:
			if (PlayerFleeceManager.FleecePreventsHealthPickups())
			{
				return null;
			}
			text = "Permanent Half Heart";
			break;
		case ITEM_TYPE.FLOWER_RED:
			text = "Flower_red";
			break;
		case ITEM_TYPE.FLOWER_WHITE:
			text = "Flower_White";
			break;
		case ITEM_TYPE.SEED_MUSHROOM:
			text = "Seed Mushroom";
			break;
		case ITEM_TYPE.SEED_FLOWER_WHITE:
			text = "Seed White Flower";
			break;
		case ITEM_TYPE.SEED_FLOWER_RED:
			text = "Seed Red Flower";
			break;
		case ITEM_TYPE.FLOWER_PURPLE:
			text = "Flower_Purple";
			break;
		case ITEM_TYPE.SEED_TREE:
			text = "Seed_tree";
			break;
		case ITEM_TYPE.LOG_REFINED:
			text = "Log Refined";
			break;
		case ITEM_TYPE.STONE_REFINED:
			text = "Stone Refined";
			break;
		case ITEM_TYPE.GOLD_NUGGET:
			text = "Gold Nugget";
			break;
		case ITEM_TYPE.ROPE:
			text = "Rope";
			break;
		case ITEM_TYPE.GOLD_REFINED:
			text = "Gold Refined";
			break;
		case ITEM_TYPE.TRINKET_CARD_UNLOCKED:
			text = "TarotCardUnlocked";
			break;
		case ITEM_TYPE.CRYSTAL:
			text = "Crystal";
			break;
		case ITEM_TYPE.SPIDER_WEB:
			text = "Spider Web";
			break;
		case ITEM_TYPE.BEETROOT:
			text = "Beetroot";
			break;
		case ITEM_TYPE.SEED_BEETROOT:
			text = "Seed Beetroot";
			break;
		case ITEM_TYPE.CAULIFLOWER:
			text = "Cauliflower";
			break;
		case ITEM_TYPE.SEED_CAULIFLOWER:
			text = "Seed Cauliflower";
			break;
		case ITEM_TYPE.MEAT_MORSEL:
			text = "Meat Morsel";
			break;
		case ITEM_TYPE.DOCTRINE_STONE:
			if (!DoctrineUpgradeSystem.TrySermonsStillAvailable() || !DoctrineUpgradeSystem.TryGetStillDoctrineStone())
			{
				return null;
			}
			text = "Doctrine Stone Piece";
			break;
		case ITEM_TYPE.SHELL:
			text = "Shell";
			break;
		case ITEM_TYPE.RELIC:
			text = "Relic";
			break;
		case ITEM_TYPE.GOD_TEAR:
			text = "God Tear";
			break;
		default:
			Debug.Log("failed to get: " + type);
			break;
		}
		GameObject gameObject = GameObject.FindGameObjectWithTag("Unit Layer");
		Transform transform = (((object)gameObject != null) ? gameObject.transform : null);
		PickUp p = null;
		while (--quantity >= 0)
		{
			BiomeGenerator instance = BiomeGenerator.Instance;
			if ((((object)instance != null) ? instance.CurrentRoom : null) != null)
			{
				transform = BiomeGenerator.Instance.CurrentRoom.GameObject.transform;
			}
			if (transform == null && GenerateRoom.Instance != null)
			{
				transform = GenerateRoom.Instance.transform;
			}
			if (transform == null)
			{
				break;
			}
			if (flag)
			{
				ObjectPool.Spawn(text, position, Quaternion.identity, transform, delegate(GameObject obj)
				{
					p = obj.GetComponent<PickUp>();
					if (p != null)
					{
						p.Speed = StartSpeed;
					}
					Action<PickUp> action = result;
					if (action != null)
					{
						action(p);
					}
				});
			}
			else
			{
				GameObject gameObject2 = (Resources.Load("Prefabs/Resources/" + text) as GameObject).Spawn(transform);
				gameObject2.transform.position = position;
				gameObject2.transform.eulerAngles = Vector3.zero;
				p = gameObject2.GetComponent<PickUp>();
				if (p != null)
				{
					p.Speed = StartSpeed;
				}
			}
		}
		return p;
	}

	public static bool IsGift(ITEM_TYPE type)
	{
		if (DataManager.AllGifts.Contains(type))
		{
			return true;
		}
		return false;
	}

	public static bool IsGiftOrNecklace(ITEM_TYPE type)
	{
		if (DataManager.AllGifts.Contains(type) || DataManager.AllNecklaces.Contains(type))
		{
			return true;
		}
		return false;
	}

	public static string Name(ITEM_TYPE Type)
	{
		return LocalizedName(Type);
	}

	public static string LocalizedName(ITEM_TYPE Type)
	{
		return LocalizationManager.GetTranslation(string.Format("Inventory/{0}", Type));
	}

	public static string LocalizedLore(ITEM_TYPE Type)
	{
		return LocalizationManager.GetTranslation(string.Format("Inventory/{0}/Lore", Type));
	}

	public static string LocalizedDescription(ITEM_TYPE Type)
	{
		return LocalizationManager.GetTranslation(string.Format("Inventory/{0}/Description", Type));
	}

	public static string Lore(ITEM_TYPE Type)
	{
		return LocalizedDescription(Type);
	}

	public static string Description(ITEM_TYPE Type)
	{
		switch (Type)
		{
		case ITEM_TYPE.GRASS:
			return "A simple material used for building structures.";
		case ITEM_TYPE.LOG:
			return "A simple material used for building structures.";
		case ITEM_TYPE.BLUE_HEART:
			return "Additional HP that is not permenant, once lost they cannot be replenished.";
		case ITEM_TYPE.BONE:
			return "Bone";
		case ITEM_TYPE.VINES:
			return "Vines";
		case ITEM_TYPE.HALF_BLUE_HEART:
			return "Half Blue Heart";
		case ITEM_TYPE.HALF_HEART:
			return "Half Heart";
		case ITEM_TYPE.MEAT:
			return "Meat";
		case ITEM_TYPE.RED_HEART:
			return "Heart";
		case ITEM_TYPE.STONE:
			return "Stone";
		case ITEM_TYPE.BLOOD_STONE:
			return "Blood Stone";
		case ITEM_TYPE.TIME_TOKEN:
			return "Time Token";
		case ITEM_TYPE.WHEAT:
			return "Wheat";
		case ITEM_TYPE.MUSHROOM_SMALL:
			return "Mushroom Sample";
		case ITEM_TYPE.MUSHROOM_BIG:
			return "Metricide Mushroom";
		case ITEM_TYPE.MONSTER_HEART:
			return "The heart of a vile monstrosity, slain by your hand.";
		case ITEM_TYPE.BLUE_PRINT:
			return "Blueprint";
		case ITEM_TYPE.POOP:
			return "Rich in... nutrients...";
		default:
			Debug.Log(string.Concat(Type, " description not set"));
			return "Not Set";
		}
	}

	public static int FuelWeight(int type)
	{
		return FuelWeight((ITEM_TYPE)type);
	}

	public static int FuelWeight(ITEM_TYPE type)
	{
		switch (type)
		{
		case ITEM_TYPE.LOG:
			return 13;
		case ITEM_TYPE.GRASS:
			return 3;
		case ITEM_TYPE.GOLD_REFINED:
			return 15;
		case ITEM_TYPE.MEAT:
		case ITEM_TYPE.BERRY:
		case ITEM_TYPE.FISH:
		case ITEM_TYPE.MUSHROOM_SMALL:
		case ITEM_TYPE.MUSHROOM_BIG:
		case ITEM_TYPE.FISH_SMALL:
		case ITEM_TYPE.FISH_BIG:
		case ITEM_TYPE.PUMPKIN:
		case ITEM_TYPE.FOLLOWER_MEAT:
		case ITEM_TYPE.FOLLOWER_MEAT_ROTTEN:
		case ITEM_TYPE.FISH_CRAB:
		case ITEM_TYPE.FISH_LOBSTER:
		case ITEM_TYPE.FISH_OCTOPUS:
		case ITEM_TYPE.FISH_SQUID:
		case ITEM_TYPE.FISH_SWORDFISH:
		case ITEM_TYPE.FISH_BLOWFISH:
		case ITEM_TYPE.BEETROOT:
		case ITEM_TYPE.CAULIFLOWER:
			return 1;
		default:
			return 1;
		}
	}

	public static int FoodSatitation(ITEM_TYPE Type)
	{
		return CookingData.GetSatationAmount(Type);
	}

	public static bool IsFish(ITEM_TYPE Type)
	{
		if (Type == ITEM_TYPE.FISH || (uint)(Type - 33) <= 1u || (uint)(Type - 91) <= 5u)
		{
			return true;
		}
		return false;
	}

	public static bool IsFood(ITEM_TYPE Type)
	{
		if (IsFish(Type))
		{
			return true;
		}
		switch (Type)
		{
		case ITEM_TYPE.MEAT:
		case ITEM_TYPE.BERRY:
		case ITEM_TYPE.PUMPKIN:
		case ITEM_TYPE.BEETROOT:
		case ITEM_TYPE.CAULIFLOWER:
		case ITEM_TYPE.MEAT_MORSEL:
			return true;
		default:
			return false;
		}
	}

	public static bool IsBigFish(ITEM_TYPE Type)
	{
		if (Type == ITEM_TYPE.FISH_BIG || (uint)(Type - 92) <= 1u || Type == ITEM_TYPE.FISH_SWORDFISH)
		{
			return true;
		}
		return false;
	}

	public static bool IsHeart(ITEM_TYPE Type)
	{
		if ((uint)(Type - 12) <= 3u || (uint)(Type - 53) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool HasGifts()
	{
		foreach (ITEM_TYPE item in ItemsThatCanBeGivenToFollower)
		{
			if (Inventory.GetItemByType((int)item) != null)
			{
				return true;
			}
		}
		return false;
	}

	public static bool CanBeGivenToFollower(ITEM_TYPE Type)
	{
		return ItemsThatCanBeGivenToFollower.Contains(Type);
	}

	public static Action<Follower, ITEM_TYPE, Action> GiveToFollowerCallbacks(ITEM_TYPE Type)
	{
		switch (Type)
		{
		case ITEM_TYPE.MUSHROOM_BIG:
			return OnFeedBigMushroom;
		case ITEM_TYPE.MEAT:
		case ITEM_TYPE.BERRY:
		case ITEM_TYPE.FISH:
		case ITEM_TYPE.FISH_SMALL:
		case ITEM_TYPE.FISH_BIG:
			return OnFeedFood;
		case ITEM_TYPE.GIFT_SMALL:
		case ITEM_TYPE.GIFT_MEDIUM:
			return OnGetGift;
		case ITEM_TYPE.Necklace_1:
		case ITEM_TYPE.Necklace_2:
		case ITEM_TYPE.Necklace_3:
		case ITEM_TYPE.Necklace_4:
		case ITEM_TYPE.Necklace_5:
		case ITEM_TYPE.Necklace_Loyalty:
		case ITEM_TYPE.Necklace_Demonic:
		case ITEM_TYPE.Necklace_Dark:
		case ITEM_TYPE.Necklace_Light:
		case ITEM_TYPE.Necklace_Missionary:
		case ITEM_TYPE.Necklace_Gold_Skull:
			return OnGetNecklace;
		default:
			return null;
		}
	}

	private static void OnGetNecklace(Follower follower, ITEM_TYPE ItemType, Action Callback)
	{
		Debug.Log("Spawning necklace");
		if (follower.Brain.Info.Necklace != 0)
		{
			Spawn(follower.Brain.Info.Necklace, 1, follower.transform.position);
		}
		follower.Brain.Info.Necklace = ItemType;
		follower.SetOutfit(FollowerOutfitType.Follower, false);
		follower.Brain.AddThought(Thought.ReceivedNecklace);
		if (ItemType == ITEM_TYPE.Necklace_Gold_Skull && !follower.Brain.HasTrait(FollowerTrait.TraitType.Immortal))
		{
			follower.Brain.AddTrait(FollowerTrait.TraitType.Immortal);
		}
		if (ItemType == ITEM_TYPE.Necklace_5)
		{
			follower.Brain.Stats.Rest = 100f;
		}
		follower.TimedAnimation("Reactions/react-enlightened1", 2f, delegate
		{
			Action action = Callback;
			if (action != null)
			{
				action();
			}
		}, false, false);
		follower.AddBodyAnimation("idle", true, 0f);
	}

	private static void OnGetGift(Follower follower, ITEM_TYPE Item, Action Callback)
	{
		follower.Brain.AddThought(Thought.ReceivedGift);
		follower.TimedAnimation("Reactions/react-enlightened1", 2f, delegate
		{
			Action action = Callback;
			if (action != null)
			{
				action();
			}
		}, false, false);
		follower.AddBodyAnimation("idle", true, 0f);
	}

	private static void OnFeedFood(Follower follower, ITEM_TYPE Item, Action Callback)
	{
		follower.TimedAnimation("Food/food_eat", 2f, delegate
		{
			follower.Brain.Stats.Satiation += FoodSatitation(Item);
			follower.Brain.Stats.TargetBathroom = 30f;
			Action action = Callback;
			if (action != null)
			{
				action();
			}
		}, true, false);
		switch (Item)
		{
		case ITEM_TYPE.MEAL:
			follower.Brain.AddThought(Thought.AteMeal);
			break;
		case ITEM_TYPE.MEAL_MEAT:
			follower.Brain.AddThought(Thought.AteGoodMeal);
			break;
		case ITEM_TYPE.MEAL_GOOD_FISH:
			follower.Brain.AddThought(Thought.AteGoodMealFish);
			break;
		case ITEM_TYPE.MEAL_GRASS:
			follower.Brain.AddThought(Thought.AteSpecialMealBad);
			break;
		case ITEM_TYPE.MEAL_GREAT:
			follower.Brain.AddThought(Thought.AteSpecialMealGood);
			break;
		default:
			follower.Brain.AddThought(Thought.AteRawFood);
			break;
		}
	}

	private static void OnFeedBigMushroom(Follower follower, ITEM_TYPE InventoryItem, Action Callback)
	{
		follower.TimedAnimation("Food/food_eat", 2f, delegate
		{
			follower.Brain.Stats.Brainwash(follower.Brain);
			if (UnityEngine.Random.Range(0f, 1f) <= 0.2f)
			{
				follower.Brain.Stats.Illness = 100f;
			}
			Action action = Callback;
			if (action != null)
			{
				action();
			}
		}, true, false);
	}

	public static string CapacityString(ITEM_TYPE type, int minimum)
	{
		int itemQuantity = Inventory.GetItemQuantity(type);
		string text = string.Format("{0} {1}/{2}", FontImageNames.GetIconByType(type), itemQuantity, minimum);
		if (itemQuantity < minimum)
		{
			text = text.Colour(StaticColors.RedColor);
		}
		return text;
	}

	public static ITEM_TYPE GetInventoryItemTypeOf(GameObject pickupObject)
	{
		PickUp component = pickupObject.GetComponent<PickUp>();
		if (component != null)
		{
			return component.type;
		}
		return ITEM_TYPE.NONE;
	}
}
