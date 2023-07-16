using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using UnityEngine;

public static class MissionManager
{
	public enum MissionType
	{
		Bounty
	}

	public class Mission
	{
		public MissionType MissionType;

		public int Difficulty;

		public int ID;

		public float ExpiryTimestamp;

		public BuyEntry[] Rewards;

		public bool GoldenMission;

		public StructureBrain.TYPES Decoration;

		public string FollowerSkin;
	}

	public const int MaxAvailableMissionsAllowed = 3;

	public const int DaysTillNewMission = 1;

	public const int MinExpiryDays = 3;

	public const int MaxExpiryDays = 5;

	private static List<InventoryItem.ITEM_TYPE> possibleRewards = new List<InventoryItem.ITEM_TYPE>
	{
		InventoryItem.ITEM_TYPE.STONE,
		InventoryItem.ITEM_TYPE.LOG,
		InventoryItem.ITEM_TYPE.BERRY,
		InventoryItem.ITEM_TYPE.BLACK_GOLD,
		InventoryItem.ITEM_TYPE.FISH
	};

	private static List<InventoryItem.ITEM_TYPE> possibleGoldRewards = new List<InventoryItem.ITEM_TYPE>
	{
		InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION,
		InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN
	};

	public static void AddMission(MissionType missionType, int difficulty, bool goldenMission = false)
	{
		Mission mission = new Mission
		{
			MissionType = missionType,
			Difficulty = difficulty,
			Rewards = GetRewards(goldenMission ? 3 : difficulty),
			GoldenMission = goldenMission,
			ExpiryTimestamp = TimeManager.TotalElapsedGameTime + (float)(Random.Range(3, 6) * 1200)
		};
		if (goldenMission)
		{
			List<BuyEntry> list = mission.Rewards.ToList();
			list.Add(GetGoldenReward(mission));
			mission.Rewards = list.ToArray();
			mission.Difficulty = 3;
		}
		DataManager.Instance.AvailableMissions.Add(mission);
	}

	private static StructuresData GetStructureData()
	{
		List<Structures_MissionShrine> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_MissionShrine>();
		if (allStructuresOfType.Count > 0)
		{
			return allStructuresOfType[0].Data;
		}
		return null;
	}

	private static BuyEntry[] GetRewards(int difficulty)
	{
		int num = Random.Range(1 + Mathf.FloorToInt(difficulty / 2), difficulty);
		List<BuyEntry> list = new List<BuyEntry>();
		for (int i = 0; i < num; i++)
		{
			int num2 = 0;
			while (++num2 < 20)
			{
				BuyEntry randomReward = GetRandomReward();
				if (!list.Contains(randomReward))
				{
					randomReward.quantity = 5 * difficulty;
					list.Add(randomReward);
					break;
				}
			}
		}
		return list.ToArray();
	}

	public static void RemoveMission(Mission mission)
	{
		DataManager.Instance.ActiveMissions.Remove(mission);
	}

	private static BuyEntry GetGoldenReward(Mission mission)
	{
		int num = 0;
		while (++num < 20)
		{
			InventoryItem.ITEM_TYPE iTEM_TYPE = possibleGoldRewards[Random.Range(0, possibleGoldRewards.Count)];
			if (iTEM_TYPE == InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION && DataManager.CheckAvailableDecorations())
			{
				mission.Decoration = DataManager.GetRandomLockedDecoration();
				return new BuyEntry(iTEM_TYPE, InventoryItem.ITEM_TYPE.NONE, 0, 0);
			}
			if (iTEM_TYPE == InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN && DataManager.CheckIfThereAreSkinsAvailable())
			{
				mission.FollowerSkin = DataManager.GetRandomLockedSkin();
				return new BuyEntry(iTEM_TYPE, InventoryItem.ITEM_TYPE.NONE, 0, 0);
			}
		}
		return GetRewards(3)[0];
	}

	private static BuyEntry GetRandomReward()
	{
		return new BuyEntry(possibleRewards[Random.Range(0, possibleRewards.Count)], InventoryItem.ITEM_TYPE.NONE, 0);
	}

	public static string GetMissionName(Mission mission)
	{
		return LocalizationManager.GetTermTranslation(string.Format("Interactions/Missions/{0}", mission.MissionType));
	}

	public static string GetExpiryFormatted(Mission mission)
	{
		int num = Mathf.FloorToInt((mission.ExpiryTimestamp - TimeManager.TotalElapsedGameTime) / 1200f);
		if (num > 0)
		{
			return string.Format(LocalizationManager.GetTranslation("UI/Generic/Days"), num + 1);
		}
		return "< 1 " + LocalizationManager.GetTranslation("UI/Generic/Days");
	}
}
