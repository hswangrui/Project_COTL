using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CritterSpawner : BaseMonoBehaviour
{
	public Vector2Int ButterfliesToSpawn;

	public GameObject ButterflyPrefab;

	public Vector2Int BeesToSpawn;

	public GameObject BeesPrefab;

	public Vector2Int BirdsToSpawn;

	public GameObject BirdsPrefab;

	public Vector2Int SpidersToSpawn;

	public GameObject SpiderPrefab;

	public Vector2Int FireFliesToSpawn;

	public GameObject FireFliesPrefab;

	public AssetReferenceGameObject halloweenGhost;

	private bool SpawnedDayCritters;

	public PolygonCollider2D Polygon;

	public Transform SpawnParent;

	private void Start()
	{
		OnNewPhaseStarted();
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
		ButterflyPrefab.CreatePool(ButterfliesToSpawn.y);
		BeesPrefab.CreatePool(BeesToSpawn.y);
		BirdsPrefab.CreatePool(BirdsToSpawn.y);
		SpiderPrefab.CreatePool(SpidersToSpawn.y);
		FireFliesPrefab.CreatePool(FireFliesToSpawn.y);
	}

	private void OnDestroy()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
	}

	private void OnNewPhaseStarted()
	{
		if (TimeManager.CurrentPhase == DayPhase.Night)
		{
			SpawnNightCritters();
		}
		else
		{
			if (!SpawnedDayCritters)
			{
				SpawnDayCritters();
			}
			SpawnBirds();
		}
		SpawnHalloweenGhosts();
	}

	private void SpawnDayCritters()
	{
		SpawnedDayCritters = true;
		int num = -1;
		int num2 = UnityEngine.Random.Range(ButterfliesToSpawn.x, ButterfliesToSpawn.y) - CritterBee.ButterFlys.Count;
		while (++num < num2)
		{
			Vector3 vector = RandomPointInCollider();
			ButterflyPrefab.Spawn(SpawnParent, vector, Quaternion.identity).GetComponent<CritterBee>().Setup(vector);
		}
		num = -1;
		num2 = UnityEngine.Random.Range(BeesToSpawn.x, BeesToSpawn.y) - CritterBee.Bees.Count;
		while (++num < num2)
		{
			Vector3 vector2 = RandomPointInCollider();
			BeesPrefab.Spawn(SpawnParent, vector2, Quaternion.identity).GetComponent<CritterBee>().Setup(vector2);
		}
	}

	private void SpawnBirds()
	{
		int num = -1;
		int num2 = UnityEngine.Random.Range(BirdsToSpawn.x, BirdsToSpawn.y) - CritterBaseBird.Birds.Count;
		while (++num < num2)
		{
			BirdsPrefab.Spawn(SpawnParent, RandomPointInCollider(), Quaternion.identity);
		}
	}

	private void SpawnNightCritters()
	{
		SpawnedDayCritters = false;
		int num = -1;
		int num2 = UnityEngine.Random.Range(SpidersToSpawn.x, SpidersToSpawn.y) - CritterSpider.Spiders.Count;
		while (++num < num2)
		{
			SpiderPrefab.Spawn(SpawnParent, RandomPointInCollider(), Quaternion.identity);
		}
		num = -1;
		num2 = UnityEngine.Random.Range(FireFliesToSpawn.x, FireFliesToSpawn.y) - CritterBee.FireFlys.Count;
		while (++num < num2)
		{
			Vector3 vector = RandomPointInCollider();
			FireFliesPrefab.Spawn(SpawnParent, vector, Quaternion.identity).GetComponent<CritterBee>().Setup(vector);
		}
	}

	private void SpawnHalloweenGhosts()
	{
		if (!FollowerBrainStats.IsBloodMoon || halloweenGhost == null || BaseLocationManager.Instance == null)
		{
			return;
		}
		int b = 4 - Interaction_HalloweenGhost.HalloweenGhosts.Count;
		int num = Mathf.Min(DataManager.Instance.Followers_Dead_IDs.Count, b);
		SeasonalEventManager.GetSeasonalEventData(SeasonalEventType.Halloween);
		List<FollowerInfo> list = new List<FollowerInfo>(DataManager.Instance.Followers_Dead);
		foreach (Interaction_HalloweenGhost halloweenGhost in Interaction_HalloweenGhost.HalloweenGhosts)
		{
			if (list.Contains(halloweenGhost.FollowerInfo))
			{
				list.Remove(halloweenGhost.FollowerInfo);
			}
		}
		for (int i = 0; i < num; i++)
		{
			if (list.Count > 0)
			{
				FollowerInfo info = list[UnityEngine.Random.Range(0, list.Count)];
				list.Remove(info);
				AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(this.halloweenGhost, RandomPointInCollider(), Quaternion.identity, SpawnParent);
				asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
				{
					obj.Result.GetComponent<Interaction_HalloweenGhost>().Configure(info);
				};
				continue;
			}
			break;
		}
	}

	private Vector3 RandomPointInCollider()
	{
		float num = Polygon.bounds.center.x - Polygon.bounds.extents.x;
		float num2 = Polygon.bounds.center.y - Polygon.bounds.extents.y;
		int num3 = 500;
		while (--num3 > 0)
		{
			Vector3 vector = new Vector3(num + UnityEngine.Random.Range(0f, Polygon.bounds.extents.x * 2f), num2 + UnityEngine.Random.Range(0f, Polygon.bounds.extents.y * 2f));
			if (Polygon.OverlapPoint(vector))
			{
				return vector;
			}
		}
		Debug.Log("Failed to find result vector zero");
		return Vector3.zero;
	}
}
