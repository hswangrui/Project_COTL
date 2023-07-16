using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RewardsItem
{
	public enum ChestRewards
	{
		NONE,
		FOOD,
		SEEDS,
		GOLD,
		MUSHROOM,
		BIOME_RESOURCE,
		FOLLOWER_NECKLASE,
		FOLLOWER_GIFT,
		FOLLOWER_SKIN,
		PLAYER_WEAPON,
		PLAYER_CURSE,
		BASE_DECORATION,
		HEART,
		TAROT,
		GOLD_NUGGETS,
		GOLD_BAR,
		SPIDERS,
		DOCTRINE_STONE
	}

	private static RewardsItem instance;

	public ChestRewards chestReward;

	public float ChanceToSpawn;

	[Range(0f, 100f)]
	public float probabilityChance;

	[HideInInspector]
	public float SpawnNumber;

	private List<ChestRewards> GoodReward = new List<ChestRewards>
	{
		ChestRewards.FOLLOWER_GIFT,
		ChestRewards.FOLLOWER_NECKLASE,
		ChestRewards.TAROT,
		ChestRewards.FOLLOWER_SKIN,
		ChestRewards.BASE_DECORATION,
		ChestRewards.GOLD
	};

	public static RewardsItem Instance
	{
		get
		{
			if (instance == null)
			{
				new RewardsItem();
			}
			return instance;
		}
		set
		{
			instance = value;
		}
	}

	public RewardsItem()
	{
		instance = this;
	}

	public void OnEnable()
	{
		Instance = this;
	}

	public void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public InventoryItem.ITEM_TYPE ReturnItemType(ChestRewards chestRewardType)
	{
		switch (chestRewardType)
		{
		case ChestRewards.DOCTRINE_STONE:
			return InventoryItem.ITEM_TYPE.DOCTRINE_STONE;
		case ChestRewards.BASE_DECORATION:
			if (DataManager.GetDecorationsAvailableCategory(PlayerFarming.Location))
			{
				return InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION;
			}
			return InventoryItem.ITEM_TYPE.NONE;
		case ChestRewards.BIOME_RESOURCE:
			return GetRandomBiomeResource();
		case ChestRewards.TAROT:
			return InventoryItem.ITEM_TYPE.TRINKET_CARD;
		case ChestRewards.GOLD_NUGGETS:
			return InventoryItem.ITEM_TYPE.GOLD_NUGGET;
		case ChestRewards.FOLLOWER_GIFT:
			if (PlayerFarming.Location == FollowerLocation.IntroDungeon)
			{
				return InventoryItem.ITEM_TYPE.BLACK_GOLD;
			}
			return DataManager.AllGifts[UnityEngine.Random.Range(0, DataManager.AllGifts.Count)];
		case ChestRewards.FOLLOWER_NECKLASE:
			return DataManager.AllNecklaces[UnityEngine.Random.Range(0, 5)];
		case ChestRewards.FOLLOWER_SKIN:
			if (DataManager.CheckIfThereAreSkinsAvailable())
			{
				return InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN;
			}
			return InventoryItem.ITEM_TYPE.NONE;
		case ChestRewards.GOLD:
			return InventoryItem.ITEM_TYPE.BLACK_GOLD;
		case ChestRewards.HEART:
			if ((double)UnityEngine.Random.value < 0.5)
			{
				if (!(UnityEngine.Random.value < 0.5f))
				{
					return InventoryItem.ITEM_TYPE.HALF_HEART;
				}
				return InventoryItem.ITEM_TYPE.RED_HEART;
			}
			if (!(UnityEngine.Random.value < 0.5f))
			{
				return InventoryItem.ITEM_TYPE.HALF_BLUE_HEART;
			}
			return InventoryItem.ITEM_TYPE.BLUE_HEART;
		case ChestRewards.SEEDS:
			switch (PlayerFarming.Location)
			{
			case FollowerLocation.Dungeon1_1:
				return InventoryItem.ITEM_TYPE.SEED;
			case FollowerLocation.Dungeon1_2:
				return InventoryItem.ITEM_TYPE.SEED_PUMPKIN;
			case FollowerLocation.Dungeon1_3:
				return InventoryItem.ITEM_TYPE.SEED_CAULIFLOWER;
			case FollowerLocation.Dungeon1_4:
				return InventoryItem.ITEM_TYPE.SEED_BEETROOT;
			default:
				return InventoryItem.ITEM_TYPE.NONE;
			}
		case ChestRewards.GOLD_BAR:
			return InventoryItem.ITEM_TYPE.GOLD_REFINED;
		default:
			Debug.Log("Uh Oh no reward for you :(");
			return InventoryItem.ITEM_TYPE.NONE;
		}
	}

	private InventoryItem.ITEM_TYPE GetRandomBiomeResource()
	{
		List<InventoryItem.ITEM_TYPE> list = new List<InventoryItem.ITEM_TYPE>();
		if (PlayerFarming.Location >= FollowerLocation.Dungeon1_1)
		{
			list.Add(InventoryItem.ITEM_TYPE.LOG);
			list.Add(InventoryItem.ITEM_TYPE.BERRY);
		}
		if (PlayerFarming.Location >= FollowerLocation.Dungeon1_2)
		{
			list.Add(InventoryItem.ITEM_TYPE.STONE);
			list.Add(InventoryItem.ITEM_TYPE.PUMPKIN);
		}
		if (PlayerFarming.Location >= FollowerLocation.Dungeon1_3)
		{
			list.Add(InventoryItem.ITEM_TYPE.CAULIFLOWER);
		}
		if (PlayerFarming.Location >= FollowerLocation.Dungeon1_4)
		{
			list.Add(InventoryItem.ITEM_TYPE.BEETROOT);
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public bool IsBiomeResource(InventoryItem.ITEM_TYPE item)
	{
		if (item != InventoryItem.ITEM_TYPE.LOG && item != InventoryItem.ITEM_TYPE.STONE && item != InventoryItem.ITEM_TYPE.GOLD_REFINED && item != InventoryItem.ITEM_TYPE.LOG_REFINED)
		{
			return item == InventoryItem.ITEM_TYPE.STONE_REFINED;
		}
		return true;
	}

	public InventoryItem.ITEM_TYPE GetRandomChestReward()
	{
		return ReturnItemType((ChestRewards)UnityEngine.Random.Range(0, Enum.GetNames(typeof(ChestRewards)).Length));
	}

	public ChestRewards GetWeapon()
	{
		if (UnityEngine.Random.Range(0, 2) == 0)
		{
			return ChestRewards.PLAYER_CURSE;
		}
		return ChestRewards.PLAYER_WEAPON;
	}

	public ChestRewards GetGoodReward(bool includeTarot = true, bool IsBoss = false)
	{
		if (PlayerFleeceManager.FleecePreventTarotCards())
		{
			includeTarot = false;
		}
		if (DungeonSandboxManager.Active)
		{
			if (UnityEngine.Random.value < 0.5f)
			{
				return ChestRewards.HEART;
			}
			return ChestRewards.TAROT;
		}
		if (DataManager.Instance.HadNecklaceOnRun > 0)
		{
			GoodReward.Remove(ChestRewards.FOLLOWER_NECKLASE);
		}
		bool flag = false;
		while (!flag)
		{
			ChestRewards chestRewards = GoodReward[UnityEngine.Random.Range(0, GoodReward.Count)];
			switch (chestRewards)
			{
			case ChestRewards.FOLLOWER_GIFT:
				if (IsBoss)
				{
					flag = true;
					if (PlayerFarming.Location == FollowerLocation.IntroDungeon)
					{
						return ChestRewards.GOLD;
					}
					return ChestRewards.FOLLOWER_GIFT;
				}
				break;
			case ChestRewards.GOLD:
				if (!IsBoss)
				{
					return ChestRewards.GOLD;
				}
				break;
			case ChestRewards.GOLD_BAR:
				return ChestRewards.GOLD_BAR;
			case ChestRewards.GOLD_NUGGETS:
				return ChestRewards.GOLD_NUGGETS;
			case ChestRewards.FOLLOWER_NECKLASE:
				DataManager.Instance.HadNecklaceOnRun++;
				return ChestRewards.FOLLOWER_NECKLASE;
			case ChestRewards.TAROT:
				if (includeTarot)
				{
					return ChestRewards.TAROT;
				}
				flag = false;
				break;
			case ChestRewards.FOLLOWER_SKIN:
				if (IsBoss)
				{
					if (DataManager.CheckIfThereAreSkinsAvailable())
					{
						return ChestRewards.FOLLOWER_SKIN;
					}
					Debug.Log("NO available Skins");
					return ChestRewards.GOLD;
				}
				break;
			case ChestRewards.BASE_DECORATION:
				if (DataManager.GetDecorationsAvailableCategory(PlayerFarming.Location))
				{
					return ChestRewards.BASE_DECORATION;
				}
				Debug.Log("NO available decorations");
				return ChestRewards.GOLD;
			default:
				flag = true;
				return chestRewards;
			}
		}
		return ChestRewards.NONE;
	}
}
