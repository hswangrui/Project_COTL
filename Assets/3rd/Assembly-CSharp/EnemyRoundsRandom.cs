using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyRoundsRandom : EnemyRoundsBase
{
	[Serializable]
	private struct SpawnPoints
	{
		public Vector3[] Positions;

		public bool Debug;
	}

	[Serializable]
	public struct EnemySet
	{
		public AssetReferenceGameObject[] EnemyList;

		public int Score;
	}

	[Serializable]
	private struct Round
	{
		public int TargetScore;

		public int MinEnemyScore;

		public int MaxEnemyScore;

		public int MinEnemies;

		public int MaxEnemies;
	}

	[SerializeField]
	private List<UnitObject> startingEnemies;

	[Space]
	[SerializeField]
	private Round[] rounds;

	[SerializeField]
	private SpawnPoints[] spawnPointSets = new SpawnPoints[0];

	[SerializeField]
	public EnemySet[] enemySets;

	private List<UnitObject> currentSpawnedEnemies = new List<UnitObject>();

	private bool beganRounds;

	private int roundIndex = -1;

	private bool allEnemiesSpawned = true;

	private List<AsyncOperationHandle<GameObject>> loadedAddressableAssets = new List<AsyncOperationHandle<GameObject>>();

	public override int TotalRounds
	{
		get
		{
			return rounds.Length + ((startingEnemies.Count > 0) ? 1 : 0);
		}
	}

	public override int CurrentRound
	{
		get
		{
			return roundIndex + 1;
		}
	}

	private void OnDestroy()
	{
		if (loadedAddressableAssets == null)
		{
			return;
		}
		foreach (AsyncOperationHandle<GameObject> loadedAddressableAsset in loadedAddressableAssets)
		{
			Addressables.Release((AsyncOperationHandle)loadedAddressableAsset);
		}
		loadedAddressableAssets.Clear();
	}

	private void Update()
	{
		if (!beganRounds && AllEnemiesDead(startingEnemies))
		{
			beganRounds = true;
		}
		if (!combatBegan)
		{
			return;
		}
		if (beganRounds && roundIndex < rounds.Length - 1)
		{
			if (AllEnemiesDead(currentSpawnedEnemies))
			{
				currentSpawnedEnemies.Clear();
				roundIndex++;
				RoundStarted(roundIndex + ((startingEnemies.Count > 0) ? 1 : 0), TotalRounds);
				SpawnEnemies(GetEnemyRound(rounds[roundIndex]));
			}
		}
		else if (roundIndex >= rounds.Length - 1 && !base.Completed && AllEnemiesDead(currentSpawnedEnemies))
		{
			UIEnemyRoundsHUD.Hide();
			Action action = actionCallback;
			if (action != null)
			{
				action();
			}
			base.Completed = true;
		}
	}

	public override void AddEnemyToRound(Health e)
	{
		if (currentSpawnedEnemies != null && e != null && !currentSpawnedEnemies.Contains(e.GetComponent<UnitObject>()))
		{
			currentSpawnedEnemies.Add(e.GetComponent<UnitObject>());
			Interaction_Chest instance = Interaction_Chest.Instance;
			if ((object)instance != null)
			{
				instance.AddEnemy(e);
			}
			base.AddEnemyToRound(e);
		}
	}

	private void SpawnEnemies(List<AssetReferenceGameObject> enemies)
	{
		allEnemiesSpawned = false;
		int targetAmount = enemies.Count;
		int count = 0;
		for (int i = 0; i < enemies.Count; i++)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(enemies[i]);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				loadedAddressableAssets.Add(obj);
				if (obj.Result != null)
				{
					UnitObject component = UnityEngine.Object.Instantiate(obj.Result, base.transform.parent).GetComponent<UnitObject>();
					component.RemoveModifier();
					component.CanHaveModifier = false;
					component.gameObject.SetActive(false);
					AddEnemyToRound(component.GetComponent<Health>());
				}
				count++;
				allEnemiesSpawned = count >= targetAmount;
				if (allEnemiesSpawned)
				{
					StartCoroutine(SetEnemyPositions(currentSpawnedEnemies));
				}
			};
		}
	}

	private IEnumerator SetEnemyPositions(List<UnitObject> enemies)
	{
		SpawnPoints[] array = spawnPointSets;
		for (int j = 0; j < array.Length; j++)
		{
			SpawnPoints spawnPoints2 = array[j];
			if (enemies.Count != spawnPoints2.Positions.Length)
			{
				continue;
			}
			List<Vector3> spawnPoints = spawnPoints2.Positions.ToList();
			for (int i = 0; i < enemies.Count; i++)
			{
				Vector3 vector = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
				spawnPoints.Remove(vector);
				enemies[i].transform.position = vector;
				enemies[i].gameObject.SetActive(true);
				EnemySpawner.CreateWithAndInitInstantiatedEnemy(enemies[i].transform.position, base.transform.parent, enemies[i].gameObject);
				if (base.SpawnDelay != 0f)
				{
					yield return new WaitForSeconds(base.SpawnDelay);
				}
			}
			yield break;
		}
		Debug.LogWarning("POSITION SET FOR '" + enemies.Count + "' DOESN'T EXIST");
	}

	private bool AllEnemiesDead(List<UnitObject> enemies)
	{
		bool flag = true;
		foreach (UnitObject enemy in enemies)
		{
			if (enemy != null)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			return allEnemiesSpawned;
		}
		return false;
	}

	private List<AssetReferenceGameObject> GetEnemyRound(Round round)
	{
		List<AssetReferenceGameObject> list = new List<AssetReferenceGameObject>();
		List<EnemySet> enemySetBetweenScore = GetEnemySetBetweenScore(round.MinEnemyScore, round.MaxEnemyScore);
		int num = Mathf.Clamp(round.TargetScore + DifficultyManager.GetEnemyRoundsScoreOffset(), 2, int.MaxValue);
		int num2 = 0;
		int num3 = 0;
		while (num2 != num && num3++ < 200)
		{
			EnemySet enemySet = enemySetBetweenScore[UnityEngine.Random.Range(0, enemySetBetweenScore.Count)];
			list.Add(enemySet.EnemyList[UnityEngine.Random.Range(0, enemySet.EnemyList.Length)]);
			num2 += enemySet.Score;
			if (num2 > num || list.Count > round.MaxEnemies || (num2 == num && list.Count < Mathf.Max(round.MinEnemies, spawnPointSets[0].Positions.Length)))
			{
				list.Clear();
				num2 = 0;
			}
		}
		if (list.Count < spawnPointSets[0].Positions.Length)
		{
			list.Clear();
			for (int i = 0; i < spawnPointSets[0].Positions.Length; i++)
			{
				list.Add(enemySetBetweenScore[UnityEngine.Random.Range(0, enemySetBetweenScore.Count)].EnemyList[0]);
			}
		}
		return list;
	}

	private List<EnemySet> GetEnemySetBetweenScore(int minEnemyScore, int maxEnemyScore)
	{
		List<EnemySet> list = new List<EnemySet>();
		EnemySet[] array = enemySets;
		for (int i = 0; i < array.Length; i++)
		{
			EnemySet item = array[i];
			if (item.Score >= minEnemyScore && (maxEnemyScore == 0 || item.Score <= maxEnemyScore))
			{
				list.Add(item);
			}
		}
		return list;
	}

	private void OnDrawGizmosSelected()
	{
		SpawnPoints[] array = spawnPointSets;
		for (int i = 0; i < array.Length; i++)
		{
			SpawnPoints spawnPoints = array[i];
			if (spawnPoints.Debug)
			{
				Vector3[] positions = spawnPoints.Positions;
				for (int j = 0; j < positions.Length; j++)
				{
					Utils.DrawCircleXY(positions[j], 0.5f, Color.blue);
				}
			}
		}
	}
}
