using UnityEngine;

[CreateAssetMenu(menuName = "COTL/Difficulty Data")]
public class DifficultyData : ScriptableObject
{
	public DifficultyManager.Difficulty PrimaryDifficulty;

	public DifficultyManager.Difficulty SecondaryDifficulty;

	[Header("Healing")]
	public float HealthDropsMultiplier = 1f;

	public float ChanceOfNegatingDeath;

	public float PlayerDamageMultiplier = 1f;

	public float InvincibleTimeMultiplier = 1f;

	public float EnemyHealthMultiplier = 1f;

	public int EnemyRoundsScoreOffset;

	public float LuckMultiplier = 1f;

	public float DungeonRoomMultiplier = 1f;

	public int DeathPeneltyPercentage = 30;

	public int EscapedPeneltyPercentage = 30;

	public float DripMultiplier = 1f;

	public float HungerDepletionMultiplier = 1f;

	public float IllnessDepletionMultiplier = 1f;

	public float DissenterDepletionMultiplier = 1f;

	public float TimeBetweenDissentingMultiplier = 1f;

	public float TimeBetweenDeathMultiplier = 1f;

	public float TimeBetweenIllnessMultiplier = 1f;

	public float TimeBetweenOldAgeMultiplier = 1f;
}
