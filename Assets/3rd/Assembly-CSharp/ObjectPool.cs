using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public sealed class ObjectPool : BaseMonoBehaviour
{
	public enum StartupPoolMode
	{
		Awake,
		Start,
		CallManually
	}

	[Serializable]
	public class StartupPool
	{
		public int size;

		public GameObject prefab;
	}

	private static ObjectPool _instance;

	private static List<GameObject> tempList = new List<GameObject>();

	private Dictionary<GameObject, List<GameObject>> pooledObjects = new Dictionary<GameObject, List<GameObject>>();

	private Dictionary<GameObject, GameObject> spawnedObjects = new Dictionary<GameObject, GameObject>();

	public StartupPoolMode startupPoolMode;

	public StartupPool[] startupPools;

	private bool startupPoolsCreated;

	private Dictionary<string, AsyncOperationHandle<GameObject>> loadedAddressables = new Dictionary<string, AsyncOperationHandle<GameObject>>();

	public static ObjectPool instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject obj = new GameObject("ObjectPool");
				obj.transform.localPosition = Vector3.zero;
				obj.transform.localRotation = Quaternion.identity;
				obj.transform.localScale = Vector3.one;
				obj.AddComponent<ObjectPool>();
				UnityEngine.Object.DontDestroyOnLoad(obj);
			}
			return _instance;
		}
	}

	private void Awake()
	{
		if (_instance != null)
		{
			Debug.Log("OBJECT POOL: Dupe Awake");
			UnityEngine.Object.DestroyImmediate(this);
			return;
		}
		Debug.Log("OBJECT POOL: Awake");
		_instance = this;
		if (startupPoolMode == StartupPoolMode.Awake)
		{
			CreateStartupPools();
		}
	}

	private void Start()
	{
		if (startupPoolMode == StartupPoolMode.Start)
		{
			CreateStartupPools();
		}
	}

	private void OnDestroy()
	{
	}

	public static void CreateStartupPools()
	{
		Debug.Log("OBJECT POOL CreateStartupPools");
		if (instance.startupPoolsCreated)
		{
			return;
		}
		instance.startupPoolsCreated = true;
		StartupPool[] array = instance.startupPools;
		if (array != null && array.Length != 0)
		{
			for (int i = 0; i < array.Length; i++)
			{
				CreatePool(array[i].prefab, array[i].size);
			}
		}
	}

	public static IEnumerator PoolReset()
	{
		Debug.Log("OBJECT POOL PoolReset");
		foreach (KeyValuePair<GameObject, List<GameObject>> pooledObject in instance.pooledObjects)
		{
			DestroyPooled(pooledObject.Key);
			Debug.Log("OBJECT POOL Clearing Pool:" + pooledObject.Key.name);
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForEndOfFrame();
		instance.startupPoolsCreated = false;
		yield return new WaitForEndOfFrame();
		CreateStartupPools();
	}

	public static void CreatePool<T>(T prefab, int initialPoolSize, bool updateExisting = false) where T : Component
	{
		CreatePool(prefab.gameObject, initialPoolSize, updateExisting);
	}

	public static void CreatePool(GameObject prefab, int initialPoolSize, bool updateExisting = false)
	{
		Debug.Log(string.Format("OBJECT POOL CreatePool: {0} : initialPoolSize({1}) : updateExisting({2})", prefab.name, initialPoolSize, updateExisting));
		if (!(prefab != null) || !(!instance.pooledObjects.ContainsKey(prefab) || updateExisting))
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		if (updateExisting && instance.pooledObjects.ContainsKey(prefab))
		{
			list = instance.pooledObjects[prefab];
		}
		if (initialPoolSize > 0)
		{
			bool activeSelf = prefab.activeSelf;
			Transform parent = instance.transform;
			while (list.Count < initialPoolSize)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
				gameObject.transform.SetParent(parent);
				gameObject.SetActive(false);
				list.Add(gameObject);
			}
		}
		if (updateExisting && instance.pooledObjects.ContainsKey(prefab))
		{
			instance.pooledObjects[prefab] = list;
		}
		else
		{
			instance.pooledObjects.Add(prefab, list);
		}
	}

	public static T Spawn<T>(T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
	{
		return Spawn(prefab.gameObject, parent, position, rotation).GetComponent<T>();
	}

	public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
	{
		return Spawn(prefab.gameObject, null, position, rotation).GetComponent<T>();
	}

	public static T Spawn<T>(T prefab, Transform parent, Vector3 position) where T : Component
	{
		return Spawn(prefab.gameObject, parent, position, Quaternion.identity).GetComponent<T>();
	}

	public static T Spawn<T>(T prefab, Vector3 position) where T : Component
	{
		return Spawn(prefab.gameObject, null, position, Quaternion.identity).GetComponent<T>();
	}

	public static T Spawn<T>(T prefab, Transform parent) where T : Component
	{
		return Spawn(prefab.gameObject, parent, Vector3.zero, Quaternion.identity).GetComponent<T>();
	}

	public static T Spawn<T>(T prefab) where T : Component
	{
		return Spawn(prefab.gameObject, null, Vector3.zero, Quaternion.identity).GetComponent<T>();
	}

	public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
	{
		if (prefab == null)
		{
			return null;
		}
		List<GameObject> value;
		GameObject gameObject;
		if (instance.pooledObjects.TryGetValue(prefab, out value))
		{
			gameObject = null;
			if (value.Count > 0)
			{
				for (int i = 0; i < value.Count; i++)
				{
					if (value[i] != null)
					{
						gameObject = value[i];
						break;
					}
				}
				if (gameObject != null)
				{
					Transform obj = gameObject.transform;
					obj.SetParent(parent);
					obj.localPosition = position;
					obj.localRotation = rotation;
					gameObject.SetActive(true);
					value.Remove(gameObject);
					instance.pooledObjects[prefab] = value;
					instance.spawnedObjects.Add(gameObject, prefab);
					return gameObject;
				}
			}
			gameObject = UnityEngine.Object.Instantiate(prefab);
			Transform obj2 = gameObject.transform;
			obj2.SetParent(parent);
			obj2.localPosition = position;
			obj2.localRotation = rotation;
			instance.spawnedObjects.Add(gameObject, prefab);
			return gameObject;
		}
		gameObject = UnityEngine.Object.Instantiate(prefab);
		Transform component = gameObject.GetComponent<Transform>();
		component.SetParent(parent);
		component.localPosition = position;
		component.localRotation = rotation;
		instance.pooledObjects.Add(prefab, new List<GameObject>());
		instance.spawnedObjects.Add(gameObject, prefab);
		return gameObject;
	}

	public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position)
	{
		return Spawn(prefab, parent, position, Quaternion.identity);
	}

	public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		return Spawn(prefab, null, position, rotation);
	}

	public static GameObject Spawn(GameObject prefab, Transform parent)
	{
		return Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
	}

	public static GameObject Spawn(GameObject prefab, Vector3 position)
	{
		return Spawn(prefab, null, position, Quaternion.identity);
	}

	public static GameObject Spawn(GameObject prefab)
	{
		return Spawn(prefab, null, Vector3.zero, Quaternion.identity);
	}

	public static void Spawn(string path, Vector3 position, Quaternion rotation, Transform parent, Action<GameObject> callback)
	{
		if (!string.IsNullOrEmpty(path))
		{
			GameManager.GetInstance().StartCoroutine(SpawnIE(path, position, rotation, parent, callback));
		}
	}

	private static IEnumerator SpawnIE(string path, Vector3 position, Quaternion rotation, Transform parent, Action<GameObject> callback)
	{
		if (instance.loadedAddressables.ContainsKey(path))
		{
			while (instance.loadedAddressables[path].Result == null)
			{
				yield return null;
			}
			if (instance.loadedAddressables[path].Status != AsyncOperationStatus.Failed)
			{
				GameObject obj2 = Spawn(instance.loadedAddressables[path].Result, parent, position, rotation);
				Action<GameObject> action = callback;
				if (action != null)
				{
					action(obj2);
				}
			}
			else
			{
				Debug.Log(("OBJECT POOL, failed addressable: " + path).Colour(Color.red));
			}
			yield break;
		}
		AsyncOperationHandle<GameObject> value = Addressables.LoadAssetAsync<GameObject>(path);
		instance.loadedAddressables.Add(path, value);
		value.Completed += delegate(AsyncOperationHandle<GameObject> obj)
		{
			instance.loadedAddressables[path] = obj;
			Action<GameObject> action2 = callback;
			if (action2 != null)
			{
				action2(Spawn(obj.Result, parent, position, rotation));
			}
		};
	}

	public static void Recycle<T>(T obj) where T : Component
	{
		if ((UnityEngine.Object)obj != (UnityEngine.Object)null && obj.gameObject != null)
		{
			Recycle(obj.gameObject);
		}
	}

	public static void Recycle(GameObject obj)
	{
		GameObject value;
		if (instance.spawnedObjects.TryGetValue(obj, out value))
		{
			Recycle(obj, value);
		}
		else
		{
			UnityEngine.Object.Destroy(obj);
		}
	}

	private static void Recycle(GameObject obj, GameObject prefab)
	{
		List<GameObject> list = new List<GameObject>();
		if (instance.pooledObjects.ContainsKey(prefab))
		{
			list = instance.pooledObjects[prefab];
			list.Add(obj);
			instance.spawnedObjects.Remove(obj);
			instance.pooledObjects[prefab] = list;
			obj.transform.SetParent(instance.transform);
			obj.SetActive(false);
			IPoolListener component;
			if (obj.TryGetComponent<IPoolListener>(out component))
			{
				component.OnRecycled();
			}
		}
	}

	public static void RecycleAll<T>(T prefab) where T : Component
	{
		RecycleAll(prefab.gameObject);
	}

	public static void RecycleAll(GameObject prefab)
	{
		foreach (KeyValuePair<GameObject, GameObject> spawnedObject in instance.spawnedObjects)
		{
			if (spawnedObject.Value == prefab)
			{
				tempList.Add(spawnedObject.Key);
			}
		}
		for (int i = 0; i < tempList.Count; i++)
		{
			Recycle(tempList[i]);
		}
		tempList.Clear();
	}

	public static void RecycleAll()
	{
		tempList.AddRange(instance.spawnedObjects.Keys);
		for (int i = 0; i < tempList.Count; i++)
		{
			if (tempList[i] != null && tempList[i].gameObject != null)
			{
				Recycle(tempList[i]);
			}
		}
		tempList.Clear();
		Debug.Log("OBJECT POOL RecycleAll Remaining spawnedObjects Count " + instance.spawnedObjects.Count);
		instance.spawnedObjects.Clear();
	}

	public static bool IsSpawned(GameObject obj)
	{
		if (instance != null && instance.spawnedObjects != null)
		{
			return instance.spawnedObjects.ContainsKey(obj);
		}
		return false;
	}

	public static int CountPooled<T>(T prefab) where T : Component
	{
		return CountPooled(prefab.gameObject);
	}

	public static int CountPooled(GameObject prefab)
	{
		List<GameObject> value;
		if (instance.pooledObjects.TryGetValue(prefab, out value))
		{
			return value.Count;
		}
		return 0;
	}

	public static int CountSpawned<T>(T prefab) where T : Component
	{
		return CountSpawned(prefab.gameObject);
	}

	public static int CountSpawned(GameObject prefab)
	{
		int num = 0;
		foreach (GameObject value in instance.spawnedObjects.Values)
		{
			if (prefab == value)
			{
				num++;
			}
		}
		return num;
	}

	public static int CountAllSpawned()
	{
		if (instance == null || instance.spawnedObjects == null)
		{
			return 0;
		}
		return instance.spawnedObjects.Count;
	}

	public static int CountAllPooled()
	{
		if (instance == null || instance.pooledObjects == null)
		{
			return 0;
		}
		int num = 0;
		foreach (List<GameObject> value in instance.pooledObjects.Values)
		{
			num += value.Count;
		}
		return num;
	}

	public static List<GameObject> GetPooled(GameObject prefab, List<GameObject> list, bool appendList)
	{
		if (list == null)
		{
			list = new List<GameObject>();
		}
		if (!appendList)
		{
			list.Clear();
		}
		List<GameObject> value;
		if (instance.pooledObjects.TryGetValue(prefab, out value))
		{
			list.AddRange(value);
		}
		return list;
	}

	public static List<T> GetPooled<T>(T prefab, List<T> list, bool appendList) where T : Component
	{
		if (list == null)
		{
			list = new List<T>();
		}
		if (!appendList)
		{
			list.Clear();
		}
		List<GameObject> value;
		if (instance.pooledObjects.TryGetValue(prefab.gameObject, out value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				list.Add(value[i].GetComponent<T>());
			}
		}
		return list;
	}

	public static List<GameObject> GetSpawned(GameObject prefab, List<GameObject> list, bool appendList)
	{
		if (list == null)
		{
			list = new List<GameObject>();
		}
		if (!appendList)
		{
			list.Clear();
		}
		foreach (KeyValuePair<GameObject, GameObject> spawnedObject in instance.spawnedObjects)
		{
			if (spawnedObject.Value == prefab)
			{
				list.Add(spawnedObject.Key);
			}
		}
		return list;
	}

	public static List<T> GetSpawned<T>(T prefab, List<T> list, bool appendList) where T : Component
	{
		if (list == null)
		{
			list = new List<T>();
		}
		if (!appendList)
		{
			list.Clear();
		}
		GameObject gameObject = prefab.gameObject;
		foreach (KeyValuePair<GameObject, GameObject> spawnedObject in instance.spawnedObjects)
		{
			if (spawnedObject.Value == gameObject)
			{
				list.Add(spawnedObject.Key.GetComponent<T>());
			}
		}
		return list;
	}

	public static void DestroyPooled(GameObject prefab)
	{
		List<GameObject> value;
		if (instance.pooledObjects.TryGetValue(prefab, out value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				UnityEngine.Object.Destroy(value[i]);
			}
			value.Clear();
		}
	}

	public static void DestroyPooled<T>(T prefab) where T : Component
	{
		DestroyPooled(prefab.gameObject);
	}

	public static void DestroyAll(GameObject prefab)
	{
		RecycleAll(prefab);
		DestroyPooled(prefab);
	}

	public static void DestroyAll<T>(T prefab) where T : Component
	{
		DestroyAll(prefab.gameObject);
	}

	public static void DestroyAll()
	{
		Debug.Log("OBJECT POOL DestroyAll");
		RecycleAll();
		instance.StopAllCoroutines();
		for (int num = instance.transform.childCount - 1; num >= 0; num--)
		{
			if (instance.transform.GetChild(num) != null)
			{
				UnityEngine.Object.Destroy(instance.transform.GetChild(num).gameObject);
			}
		}
		Debug.Log("OBJECT POOL Release Addressables" + instance.loadedAddressables.Count);
		foreach (AsyncOperationHandle<GameObject> value in instance.loadedAddressables.Values)
		{
			if (value.Result != null)
			{
				Addressables.Release(value);
			}
		}
		instance.loadedAddressables.Clear();
		instance.pooledObjects.Clear();
		tempList.Clear();
		Projectile.CleanCache();
		EnemySpawner.Unload();
	}
}
