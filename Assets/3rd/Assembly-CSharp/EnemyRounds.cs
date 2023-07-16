using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyRounds : EnemyRoundsBase
{
	[Serializable]
	public class RoundsOfEnemies
	{
		public bool DisplayGizmo;

		public List<EnemyAndPosition> Round = new List<EnemyAndPosition>();
	}

	[Serializable]
	public class EnemyAndPosition
	{
		public AssetReferenceGameObject EnemyTarget;

		public Vector3 Position;

		public float Delay = 1f;

		public Health Enemy { get; set; }

		public EnemyAndPosition(Health e, Vector3 p, float d)
		{
			Enemy = e;
			Position = p;
			Delay = d;
		}
	}

	public List<Health> EnemyList = new List<Health>();

	public Gradient GizmoColorGradient;

	public List<RoundsOfEnemies> Rounds = new List<RoundsOfEnemies>();

	public UnityEvent Callback;

	private int currentRound = -1;

	private bool allEnemiesSpawned = true;

	private List<AsyncOperationHandle<GameObject>> loadedAddressableAssets = new List<AsyncOperationHandle<GameObject>>();

	public override int TotalRounds
	{
		get
		{
			if (Rounds.Count + EnemyList.Count <= 0)
			{
				return 0;
			}
			return 1;
		}
	}

	public override int CurrentRound
	{
		get
		{
			return currentRound + 1;
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

	public override void BeginCombat(bool showRoundsUI, Action ActionCallback)
	{
		base.BeginCombat(showRoundsUI, ActionCallback);
		StartCoroutine(SpawnNewEnemies());
	}

	private void OnEnable()
	{
		if (combatBegan)
		{
			BeginCombat(showRoundsUI, actionCallback);
		}
	}

	private IEnumerator SpawnNewEnemies()
	{
		EnemyRoundsBase.Instance = this;
		if (currentRound == -1)
		{
			bool allEnemiesKilled;
			do
			{
				allEnemiesKilled = true;
				foreach (Health enemy in EnemyList)
				{
					if (enemy != null)
					{
						allEnemiesKilled = false;
					}
				}
				yield return null;
			}
			while (!allEnemiesKilled);
		}
		RoundStarted(1, TotalRounds);
		Debug.Log("AA");
		if (Rounds != null && Rounds.Count > 0)
		{
			while (currentRound < Rounds.Count)
			{
				if (currentRound != -1)
				{
					bool allEnemiesKilled;
					do
					{
						allEnemiesKilled = true;
						foreach (EnemyAndPosition item in Rounds[currentRound].Round)
						{
							if (item.Enemy != null)
							{
								allEnemiesKilled = false;
							}
						}
						yield return null;
					}
					while (!allEnemiesKilled || !allEnemiesSpawned);
				}
				currentRound++;
				RoundStarted(currentRound + ((EnemyList.Count > 0) ? 1 : 0), TotalRounds);
				if (currentRound >= Rounds.Count)
				{
					continue;
				}
				allEnemiesSpawned = false;
				int count = 0;
				for (int i = 0; i < Rounds[currentRound].Round.Count; i++)
				{
					AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(Rounds[currentRound].Round[i].EnemyTarget);
					GameObject g;
					asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
					{
						loadedAddressableAssets.Add(obj);
						g = ObjectPool.Spawn(obj.Result, base.transform.parent);
						g.transform.position = base.transform.position + Rounds[currentRound].Round[i].Position;
						g.GetComponent<UnitObject>().RemoveModifier();
						g.GetComponent<UnitObject>().CanHaveModifier = false;
						g.SetActive(false);
						EnemySpawner.CreateWithAndInitInstantiatedEnemy(base.transform.position + Rounds[currentRound].Round[i].Position, base.transform.parent, g);
						Rounds[currentRound].Round[i].Enemy = g.GetComponent<Health>();
						int num = count;
						count = num + 1;
						allEnemiesSpawned = count >= Rounds[currentRound].Round.Count;
					};
					yield return new WaitForSeconds(Rounds[currentRound].Round[i].Delay);
				}
			}
		}
		Debug.Log("DO CALLBACKS!");
		UnityEvent callback = Callback;
		if (callback != null)
		{
			callback.Invoke();
		}
		Action action = actionCallback;
		if (action != null)
		{
			action();
		}
		UIEnemyRoundsHUD.Hide();
		AudioManager.Instance.SetMusicCombatState(false);
		Debug.Log("Chest Reveal!");
		Interaction_Chest instance = Interaction_Chest.Instance;
		if ((object)instance != null)
		{
			instance.Reveal();
		}
		EnemyRoundsBase.Instance = null;
	}

	public override void AddEnemyToRound(Health e)
	{
		foreach (EnemyAndPosition item in Rounds[currentRound].Round)
		{
			if (item.Enemy == e)
			{
				return;
			}
		}
		Rounds[currentRound].Round.Add(new EnemyAndPosition(e, Vector3.zero, 0f));
		base.AddEnemyToRound(e);
	}

	private void OnDrawGizmos()
	{
		foreach (Health enemy in EnemyList)
		{
			if (enemy != null)
			{
				Utils.DrawLine(base.transform.position, enemy.transform.position, Color.yellow);
			}
		}
		int num = -1;
		while (++num < Rounds.Count)
		{
			RoundsOfEnemies roundsOfEnemies = Rounds[num];
			if (!roundsOfEnemies.DisplayGizmo)
			{
				continue;
			}
			foreach (EnemyAndPosition item in roundsOfEnemies.Round)
			{
				Utils.DrawCircleXY(base.transform.position + item.Position, 0.2f, GizmoColorGradient.Evaluate((float)num / ((float)Rounds.Count - 1f)));
			}
		}
	}
}
