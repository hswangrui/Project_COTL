using System;
using MMBiomeGeneration;
using UnityEngine;

public class DifficultyManager
{
	[Serializable]
	public enum Difficulty
	{
		Easy = 0,
		Medium = 50,
		Hard = 100,
		ExtraHard = 101
	}

	private static DifficultyData currentDifficultyData;

	private static DifficultyData[] difficulties = new DifficultyData[0];

	public static Difficulty PrimaryDifficulty { get; private set; } = Difficulty.Medium;


	public static Difficulty SecondaryDifficulty { get; private set; } = Difficulty.Medium;


	public static bool AssistModeEnabled
	{
		get
		{
			return true;
		}
	}

	public static DifficultyData CurrentDifficultyData
	{
		get
		{
			if (currentDifficultyData == null)
			{
				LoadCurrentDifficulty();
			}
			return currentDifficultyData;
		}
	}

	public static void LoadCurrentDifficulty()
	{
		if (difficulties.Length == 0)
		{
			difficulties = Resources.LoadAll<DifficultyData>("Data/Difficulty Data");
		}
		Difficulty secondaryDifficulty = DetermineCurrentDifficulty();
		currentDifficultyData = GetDifficultyData(PrimaryDifficulty, secondaryDifficulty);
		SecondaryDifficulty = secondaryDifficulty;
	}

	public static void ForceDifficulty(int difficulty)
	{
		ForceDifficulty(AllAvailableDifficulties()[Mathf.Clamp(difficulty, 0, AllAvailableDifficulties().Length - 1)]);
	}

	public static void ForceDifficulty(Difficulty difficulty)
	{
		Debug.Log(string.Format("Force Difficulty - {0}", difficulty).Colour(Color.yellow));
		PrimaryDifficulty = difficulty;
		LoadCurrentDifficulty();
		bool flag = !GameManager.IsDungeon(PlayerFarming.Location) || (BiomeGenerator.Instance != null && BiomeGenerator.Instance.CurrentRoom != null && BiomeGenerator.Instance.RoomEntrance != null && BiomeGenerator.Instance.RoomEntrance == BiomeGenerator.Instance.CurrentRoom && DataManager.Instance.PlayerDamageReceivedThisRun <= 0f && DataManager.Instance.PlayerDamageDealtThisRun <= 0f);
		if ((bool)PlayerFarming.Instance && flag)
		{
			if (GameManager.IsDungeon(PlayerFarming.Location) && DataManager.Instance.PlayerFleece == 2)
			{
				DataManager.Instance.RedHeartsTemporarilyRemoved = 0;
				DataManager.Instance.RedHeartsTemporarilyRemoved += (DataManager.Instance.PLAYER_STARTING_HEALTH + DataManager.Instance.PLAYER_HEARTS_LEVEL) / 2;
			}
			DataManager.Instance.PLAYER_TOTAL_HEALTH = (float)(DataManager.Instance.PLAYER_STARTING_HEALTH + DataManager.Instance.PLAYER_HEARTS_LEVEL + DataManager.Instance.PLAYER_HEALTH_MODIFIED - DataManager.Instance.RedHeartsTemporarilyRemoved) * PlayerFleeceManager.GetHealthMultiplier();
			DataManager.Instance.PLAYER_STARTING_HEALTH_CACHED = DataManager.Instance.PLAYER_STARTING_HEALTH;
			DataManager.Instance.PLAYER_HEALTH = DataManager.Instance.PLAYER_TOTAL_HEALTH;
			PlayerFarming.Instance.GetComponent<HealthPlayer>().InitHP();
		}
	}

	private static Difficulty DetermineCurrentDifficulty()
	{
		if (!AssistModeEnabled)
		{
			return Difficulty.Medium;
		}
		Difficulty result = Difficulty.Medium;
		float playerSkillValue = PlayerSkillManager.GetPlayerSkillValue();
		float playerTotal = PlayerSkillManager.GetPlayerTotal();
		if (playerSkillValue < 10f && playerTotal > 100f)
		{
			result = Difficulty.Easy;
		}
		if (playerSkillValue > 50f && playerTotal > 500f)
		{
			result = Difficulty.Hard;
		}
		return result;
	}

	private static DifficultyData GetDifficultyData(Difficulty primaryDifficulty, Difficulty secondaryDifficulty)
	{
		DifficultyData[] array = difficulties;
		foreach (DifficultyData difficultyData in array)
		{
			if (difficultyData.PrimaryDifficulty == primaryDifficulty && difficultyData.SecondaryDifficulty == secondaryDifficulty)
			{
				return difficultyData;
			}
		}
		return difficulties[0];
	}

	public static float GetHealthDropsMultiplier()
	{
		return CurrentDifficultyData.HealthDropsMultiplier;
	}

	public static float GetDungeonRoomsMultiplier()
	{
		return CurrentDifficultyData.DungeonRoomMultiplier;
	}

	public static float GetChanceOfNegatingDeath()
	{
		return CurrentDifficultyData.ChanceOfNegatingDeath;
	}

	public static float GetPlayerDamageMultiplier()
	{
		return CurrentDifficultyData.PlayerDamageMultiplier;
	}

	public static float GetInvincibleTimeMultiplier()
	{
		return CurrentDifficultyData.InvincibleTimeMultiplier;
	}

	public static float GetEnemyHealthMultiplier()
	{
		return CurrentDifficultyData.EnemyHealthMultiplier;
	}

	public static float GetLuckMultiplier()
	{
		return CurrentDifficultyData.LuckMultiplier;
	}

	public static float GetTimeBetweenDissentingMultiplier()
	{
		return CurrentDifficultyData.TimeBetweenDissentingMultiplier;
	}

	public static int GetDeathPeneltyPercentage()
	{
		return CurrentDifficultyData.DeathPeneltyPercentage;
	}

	public static float GetDripMultiplier()
	{
		return CurrentDifficultyData.DripMultiplier;
	}

	public static int GetEnemyRoundsScoreOffset()
	{
		return CurrentDifficultyData.EnemyRoundsScoreOffset;
	}

	public static int GetEscapedPeneltyPercentage()
	{
		return CurrentDifficultyData.EscapedPeneltyPercentage;
	}

	public static float GetTimeBetweenDeathMultiplier()
	{
		return CurrentDifficultyData.TimeBetweenDeathMultiplier;
	}

	public static float GetTimeBetweenIllnessMultiplier()
	{
		return CurrentDifficultyData.TimeBetweenIllnessMultiplier;
	}

	public static float GetTimeBetweenOldAgeMultiplier()
	{
		return CurrentDifficultyData.TimeBetweenOldAgeMultiplier;
	}

	public static float GetHungerDepletionMultiplier()
	{
		return CurrentDifficultyData.HungerDepletionMultiplier;
	}

	public static float GetIllnessDepletionMultiplier()
	{
		return CurrentDifficultyData.IllnessDepletionMultiplier;
	}

	public static float GetDissenterDepletionMultiplier()
	{
		return CurrentDifficultyData.DissenterDepletionMultiplier;
	}

	public static Difficulty[] AllAvailableDifficulties()
	{
		return new Difficulty[4]
		{
			Difficulty.Easy,
			Difficulty.Medium,
			Difficulty.Hard,
			Difficulty.ExtraHard
		};
	}

	public static string[] GetDifficultyLocalisation()
	{
		Difficulty[] array = AllAvailableDifficulties();
		string[] array2 = new string[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = GetDifficultyLocalisation(array[i]);
		}
		return array2;
	}

	private static string GetDifficultyLocalisation(Difficulty difficulty)
	{
		return string.Format("UI/Settings/Game/Difficulty/{0}", difficulty);
	}
}
