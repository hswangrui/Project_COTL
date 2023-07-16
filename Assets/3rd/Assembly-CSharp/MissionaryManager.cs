using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public static class MissionaryManager
{
	public static int BaseChance_Follower = 65;

	public static int BaseChance_Wood = 75;

	public static int BaseChance_Stone = 75;

	public static int BaseChance_Gold = 70;

	public static int BaseChance_Food = 80;

	public static int BaseChance_Bones = 75;

	public static int BaseChance_RefinedMaterials = 70;

	public static int BaseChance_Seeds = 80;

	public static int RandomSeedSpread = 5;

	public static readonly IntRange DurationRange = new IntRange(1, 3);

	private const int kMeatIterations = 2;

	private static readonly IntRange FollowerRange = new IntRange(1, 1);

	private static readonly IntRange WoodRange = new IntRange(30, 42);

	private static readonly IntRange StoneRange = new IntRange(18, 26);

	private static readonly IntRange GoldRange = new IntRange(28, 52);

	private static readonly IntRange BoneRange = new IntRange(40, 70);

	private static readonly IntRange SeedRange = new IntRange(5, 8);

	private static readonly IntRange MeatRange = new IntRange(16, 24);

	private static readonly IntRange MorselRange = new IntRange(5, 9);

	private static readonly IntRange RefinedRange = new IntRange(3, 6);

	public static int GetDurationDeterministic(FollowerInfo info, InventoryItem.ITEM_TYPE type)
	{
		return DurationRange.Random((int)(info.ID + type + DataManager.Instance.CurrentDayIndex));
	}

	public static float GetBaseChanceMultiplier(InventoryItem.ITEM_TYPE type, FollowerInfo followerInfo)
	{
		float num = 0.9f;
		if (followerInfo.XPLevel == 1)
		{
			num += 0.15f;
		}
		else if (followerInfo.XPLevel <= 2)
		{
			num += 0.2f;
		}
		else if (followerInfo.XPLevel <= 4)
		{
			num += 0.25f;
		}
		else if (followerInfo.XPLevel > 4)
		{
			num += 0.3f;
		}
		if (FollowerBrain.GetOrCreateBrain(followerInfo).CurrentState.Type == FollowerStateType.Exhausted)
		{
			num -= 0.5f;
		}
		if (FollowerBrainStats.BrainWashed)
		{
			num += 0.2f;
		}
		return num;
	}

	public static float GetChance(InventoryItem.ITEM_TYPE type, FollowerInfo followerInfo, StructureBrain.TYPES missionaryType)
	{
		float baseChanceMultiplier = GetBaseChanceMultiplier(type, followerInfo);
		if (DataManager.Instance.NextMissionarySuccessful || followerInfo.Necklace == InventoryItem.ITEM_TYPE.Necklace_Missionary)
		{
			return 1f;
		}
		System.Random random = new System.Random((int)(followerInfo.ID + type));
		switch (type)
		{
		case InventoryItem.ITEM_TYPE.FOLLOWERS:
			return Mathf.Clamp((float)(BaseChance_Follower + random.Next(-RandomSeedSpread, RandomSeedSpread)) / 100f * baseChanceMultiplier, 0f, 0.95f);
		case InventoryItem.ITEM_TYPE.LOG:
			return Mathf.Clamp((float)(BaseChance_Wood + random.Next(-RandomSeedSpread, RandomSeedSpread)) / 100f * baseChanceMultiplier, 0f, 0.95f);
		case InventoryItem.ITEM_TYPE.STONE:
			return Mathf.Clamp((float)(BaseChance_Stone + random.Next(-RandomSeedSpread, RandomSeedSpread)) / 100f * baseChanceMultiplier, 0f, 0.95f);
		case InventoryItem.ITEM_TYPE.BLACK_GOLD:
			return Mathf.Clamp((float)(BaseChance_Gold + random.Next(-RandomSeedSpread, RandomSeedSpread)) / 100f * baseChanceMultiplier, 0f, 0.95f);
		case InventoryItem.ITEM_TYPE.MEAT:
			return Mathf.Clamp((float)(BaseChance_Food + random.Next(-RandomSeedSpread, RandomSeedSpread)) / 100f * baseChanceMultiplier, 0f, 0.95f);
		case InventoryItem.ITEM_TYPE.BONE:
			return Mathf.Clamp((float)(BaseChance_Bones + random.Next(-RandomSeedSpread, RandomSeedSpread)) / 100f * baseChanceMultiplier, 0f, 0.95f);
		case InventoryItem.ITEM_TYPE.SEED:
		case InventoryItem.ITEM_TYPE.SEED_PUMPKIN:
		case InventoryItem.ITEM_TYPE.SEED_BEETROOT:
		case InventoryItem.ITEM_TYPE.SEED_CAULIFLOWER:
			return Mathf.Clamp((float)(BaseChance_Seeds + random.Next(-RandomSeedSpread, RandomSeedSpread)) / 100f * baseChanceMultiplier, 0f, 0.95f);
		case InventoryItem.ITEM_TYPE.LOG_REFINED:
		case InventoryItem.ITEM_TYPE.STONE_REFINED:
			return Mathf.Clamp((float)(BaseChance_RefinedMaterials + random.Next(-RandomSeedSpread, RandomSeedSpread)) / 100f * baseChanceMultiplier, 0f, 0.95f);
		default:
			return 0f;
		}
	}

	public static InventoryItem[] GetReward(InventoryItem.ITEM_TYPE type, float chance, int followerID)
	{
		float num = UnityEngine.Random.Range(0f, 1f);
		foreach (ObjectivesData completedObjective in DataManager.Instance.CompletedObjectives)
		{
			if (completedObjective.Follower == followerID)
			{
				chance = float.MaxValue;
				break;
			}
		}
		if (chance >= num)
		{
			switch (type)
			{
			case InventoryItem.ITEM_TYPE.FOLLOWERS:
				return GetFollowerReward();
			case InventoryItem.ITEM_TYPE.LOG:
				return GetWoodReward();
			case InventoryItem.ITEM_TYPE.STONE:
				return GetStoneReward();
			case InventoryItem.ITEM_TYPE.BLACK_GOLD:
				return GetGoldReward();
			case InventoryItem.ITEM_TYPE.MEAT:
				return GetFoodReward();
			case InventoryItem.ITEM_TYPE.BONE:
				return GetBoneReward();
			case InventoryItem.ITEM_TYPE.SEED:
			case InventoryItem.ITEM_TYPE.SEED_PUMPKIN:
			case InventoryItem.ITEM_TYPE.SEEDS:
			case InventoryItem.ITEM_TYPE.SEED_BEETROOT:
			case InventoryItem.ITEM_TYPE.SEED_CAULIFLOWER:
				return new InventoryItem[1] { GetSeedsReward(type) };
			case InventoryItem.ITEM_TYPE.LOG_REFINED:
				return new InventoryItem[1] { GetRefinedReward(InventoryItem.ITEM_TYPE.LOG_REFINED) };
			case InventoryItem.ITEM_TYPE.STONE_REFINED:
				return new InventoryItem[1] { GetRefinedReward(InventoryItem.ITEM_TYPE.STONE_REFINED) };
			}
		}
		return new InventoryItem[0];
	}

	public static IntRange GetRewardRange(InventoryItem.ITEM_TYPE type)
	{
		switch (type)
		{
		case InventoryItem.ITEM_TYPE.FOLLOWERS:
			return FollowerRange;
		case InventoryItem.ITEM_TYPE.LOG:
			return WoodRange;
		case InventoryItem.ITEM_TYPE.STONE:
			return StoneRange;
		case InventoryItem.ITEM_TYPE.BLACK_GOLD:
			return GoldRange;
		case InventoryItem.ITEM_TYPE.MEAT:
			return new IntRange(Mathf.Min(MeatRange.Min, MorselRange.Min), Mathf.Max(MeatRange.Max, MeatRange.Max));
		case InventoryItem.ITEM_TYPE.BONE:
			return BoneRange;
		case InventoryItem.ITEM_TYPE.SEED:
		case InventoryItem.ITEM_TYPE.SEED_PUMPKIN:
		case InventoryItem.ITEM_TYPE.SEEDS:
		case InventoryItem.ITEM_TYPE.SEED_BEETROOT:
		case InventoryItem.ITEM_TYPE.SEED_CAULIFLOWER:
			return SeedRange;
		case InventoryItem.ITEM_TYPE.LOG_REFINED:
		case InventoryItem.ITEM_TYPE.STONE_REFINED:
			return RefinedRange;
		default:
			return new IntRange(0, 0);
		}
	}

	private static InventoryItem[] GetFollowerReward()
	{
		return new InventoryItem[1]
		{
			new InventoryItem(InventoryItem.ITEM_TYPE.FOLLOWERS, FollowerRange.Random())
		};
	}

	private static InventoryItem[] GetWoodReward()
	{
		return new InventoryItem[1]
		{
			new InventoryItem(InventoryItem.ITEM_TYPE.LOG, WoodRange.Random())
		};
	}

	private static InventoryItem[] GetStoneReward()
	{
		return new InventoryItem[1]
		{
			new InventoryItem(InventoryItem.ITEM_TYPE.STONE, StoneRange.Random())
		};
	}

	private static InventoryItem[] GetGoldReward()
	{
		return new InventoryItem[1]
		{
			new InventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD, GoldRange.Random())
		};
	}

	private static InventoryItem[] GetBoneReward()
	{
		return new InventoryItem[1]
		{
			new InventoryItem(InventoryItem.ITEM_TYPE.BONE, BoneRange.Random())
		};
	}

	private static InventoryItem[] GetFoodReward()
	{
		List<InventoryItem> list = new List<InventoryItem>();
		InventoryItem inventoryItem = new InventoryItem(InventoryItem.ITEM_TYPE.MEAT);
		InventoryItem inventoryItem2 = new InventoryItem(InventoryItem.ITEM_TYPE.MEAT_MORSEL);
		inventoryItem.quantity += MeatRange.Random();
		inventoryItem2.quantity += MorselRange.Random();
		list.Add(inventoryItem);
		list.Add(inventoryItem2);
		return list.ToArray();
	}

	public static InventoryItem GetSeedsReward(InventoryItem.ITEM_TYPE overrideType)
	{
		InventoryItem.ITEM_TYPE type = InventoryItem.ITEM_TYPE.SEED;
		int num = new System.Random(TimeManager.CurrentDay).Next(0, 4);
		if (num == 0 && DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_1))
		{
			type = InventoryItem.ITEM_TYPE.SEED_PUMPKIN;
		}
		else if (num == 1 && DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_2))
		{
			type = InventoryItem.ITEM_TYPE.SEED_CAULIFLOWER;
		}
		else if (num == 2 && DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_3))
		{
			type = InventoryItem.ITEM_TYPE.SEED_BEETROOT;
		}
		if (overrideType != InventoryItem.ITEM_TYPE.SEEDS)
		{
			type = overrideType;
		}
		return new InventoryItem(type, SeedRange.Random());
	}

	private static InventoryItem GetRefinedReward(InventoryItem.ITEM_TYPE type)
	{
		return new InventoryItem(type, RefinedRange.Random());
	}

	public static string GetExpiryFormatted(float timeStamp)
	{
		int num = Mathf.RoundToInt(timeStamp / 1200f);
		int num2 = Mathf.RoundToInt(TimeManager.TotalElapsedGameTime / 1200f);
		int num3 = Mathf.RoundToInt(num - num2);
		if (num3 > 1)
		{
			return string.Format(LocalizationManager.GetTranslation("UI/Generic/Days"), num3);
		}
		if (num3 == 1)
		{
			return "1 " + LocalizationManager.GetTranslation("UI/Day");
		}
		return "< 1 " + LocalizationManager.GetTranslation("UI/Day");
	}

	public static List<Follower> FollowersAvailableForMission()
	{
		List<Follower> list = new List<Follower>(FollowerManager.FollowersAtLocation(FollowerLocation.Base));
		for (int num = list.Count - 1; num >= 0; num--)
		{
			int iD = list[num].Brain.Info.ID;
			if (list[num].Brain.Info.CursedState != 0 || FollowerManager.FollowerLocked(iD))
			{
				list.RemoveAt(num);
			}
		}
		list.Sort((Follower a, Follower b) => b.Brain.Info.XPLevel.CompareTo(a.Brain.Info.XPLevel));
		return list;
	}
}
