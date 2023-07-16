using System;
using I2.Loc;
using UnityEngine;

[Serializable]
public struct MetaData
{
	public string CultName;

	public int FollowerCount;

	public int StructureCount;

	public int DeathCount;

	public int Day;

	public int Difficulty;

	public float PlayTime;

	public bool Dungeon1Completed;

	public bool Dungeon1NGPCompleted;

	public bool Dungeon2Completed;

	public bool Dungeon2NGPCompleted;

	public bool Dungeon3Completed;

	public bool Dungeon3NGPCompleted;

	public bool Dungeon4Completed;

	public bool Dungeon4NGPCompleted;

	public bool GameBeaten;

	public bool SandboxBeaten;

	public bool DeathCatRecruited;

	public int PercentageCompleted;

	public bool Permadeath;

	public string Version;

	public static MetaData Default(DataManager dataManager)
	{
		MetaData result = default(MetaData);
		result.CultName = dataManager.CultName;
		result.FollowerCount = dataManager.Followers.Count;
		result.StructureCount = StructureManager.GetTotalHomesCount();
		result.DeathCount = dataManager.Followers_Dead.Count;
		result.Day = dataManager.CurrentDayIndex;
		result.GameBeaten = dataManager.DeathCatBeaten;
		result.SandboxBeaten = dataManager.CompletedSandbox;
		result.PlayTime = dataManager.TimeInGame;
		result.Dungeon1Completed = dataManager.DungeonCompleted(FollowerLocation.Dungeon1_1);
		result.Dungeon2Completed = dataManager.DungeonCompleted(FollowerLocation.Dungeon1_2);
		result.Dungeon3Completed = dataManager.DungeonCompleted(FollowerLocation.Dungeon1_3);
		result.Dungeon4Completed = dataManager.DungeonCompleted(FollowerLocation.Dungeon1_4);
		result.Dungeon1NGPCompleted = dataManager.DungeonCompleted(FollowerLocation.Dungeon1_1, true);
		result.Dungeon2NGPCompleted = dataManager.DungeonCompleted(FollowerLocation.Dungeon1_2, true);
		result.Dungeon3NGPCompleted = dataManager.DungeonCompleted(FollowerLocation.Dungeon1_3, true);
		result.Dungeon4NGPCompleted = dataManager.DungeonCompleted(FollowerLocation.Dungeon1_4, true);
		result.DeathCatRecruited = dataManager.HasDeathCatFollower();
		result.Difficulty = DifficultyManager.AllAvailableDifficulties().IndexOf(DifficultyManager.Difficulty.Medium);
		result.PercentageCompleted = 0;
		result.Permadeath = false;
		result.Version = Application.version;
		return result;
	}

	public override string ToString()
	{
		return "";
	//	return string.Format("( {0} | {1} x {2} )", string.Format(ScriptLocalization.UI.DayNumber, Day), FollowerCount, "<sprite name=\"icon_Followers\">");
	}
}
