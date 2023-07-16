using Unify;
using UnityEngine;

public class PlayerHealthTracker : MonoBehaviour
{
	public Health bossToTrack;

	[SerializeField]
	private Enemy enemyType;

	private bool isOnPlayerHitAssigned;

	private bool failed;

	private void Start()
	{
		failed = false;
	}

	private void OnEnable()
	{
		bossToTrack.OnDieCallback.AddListener(OnDie);
		TryAssignOnPlayerHit();
	}

	private void OnDisable()
	{
		bossToTrack.OnDieCallback.RemoveListener(OnDie);
		if (PlayerFarming.Instance != null)
		{
			PlayerFarming.Instance.health.OnHitCallback.RemoveListener(OnPlayerHit);
		}
		else
		{
			Debug.Log("Player Farming Null!");
		}
	}

	private void Update()
	{
		if (!isOnPlayerHitAssigned)
		{
			TryAssignOnPlayerHit();
		}
	}

	private void OnPlayerHit()
	{
		if (!failed)
		{
			Debug.Log("Achievement failed to get no damage");
		}
		failed = true;
	}

	private void TryAssignOnPlayerHit()
	{
		if (PlayerFarming.Instance != null)
		{
			PlayerFarming.Instance.health.OnHitCallback.AddListener(OnPlayerHit);
			isOnPlayerHitAssigned = true;
		}
		else
		{
			Debug.Log("Player Farming Null!");
			isOnPlayerHitAssigned = false;
		}
	}

	private void OnDie()
	{
		if (!failed)
		{
			Debug.Log("Achievement unlocked killed no damage boss: " + enemyType);
			switch (enemyType)
			{
			case Enemy.WormBoss:
				AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("KILL_BOSS_1_NODAMAGE"));
				break;
			case Enemy.FrogBoss:
				AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("KILL_BOSS_2_NODAMAGE"));
				break;
			case Enemy.JellyBoss:
				AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("KILL_BOSS_3_NODAMAGE"));
				break;
			case Enemy.SpiderBoss:
				AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("KILL_BOSS_4_NODAMAGE"));
				break;
			}
		}
	}
}
