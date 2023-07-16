using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class FollowerPet : MonoBehaviour
{
	public enum FollowerPetType
	{
		Spider
	}

	public static List<FollowerPet> FollowerPets = new List<FollowerPet>();

	private Follower follower;

	public Follower Follower
	{
		get
		{
			return follower;
		}
	}

	public static void Create(FollowerPetType petType, Follower target)
	{
		string key = "";
		if (petType == FollowerPetType.Spider)
		{
			key = "Assets/Prefabs/Spider Pet.prefab";
		}
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(key, target.transform.parent);
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
		{
			obj.Result.transform.position = target.transform.position;
			obj.Result.GetComponent<FollowerPet>().follower = target;
			if (obj.Result.GetComponent<CritterSpider>() != null)
			{
				obj.Result.GetComponent<CritterSpider>().TargetHost = target.gameObject;
			}
		};
	}

	private void Awake()
	{
		FollowerPets.Add(this);
	}

	private void OnDestroy()
	{
		FollowerPets.Remove(this);
	}
}
