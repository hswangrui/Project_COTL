using System.IO;
using Steamworks;
using UnityEngine;

public class InitUnifyGlobal : MonoBehaviour
{
	private static GameObject instance;

	public static MemoryStream _outputMemoryStream;

	public static MemoryStream _serializedMemoryStream;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Startup()
	{
		if (!(instance == null))
		{
			return;
		}
		Debug.Log("Unify: Startup");
		if (SteamManager.Initialized)
		{
			Debug.Log("Steam user -> " + SteamFriends.GetPersonaName());
		}
		else
		{
			Debug.LogError("Steam client might be not running!");
		}
		_outputMemoryStream = new NotClosingMemoryStream(new MemoryStream(1048576));
		_serializedMemoryStream = new NotClosingMemoryStream(new MemoryStream(16777216));
		Object @object = Resources.Load("Prefabs/Unify Global");
		if (@object != null)
		{
			instance = Object.Instantiate(@object) as GameObject;
			if (instance != null)
			{
				Object.DontDestroyOnLoad(instance);
			}
			else
			{
				Debug.LogError("Unify: Could not instantiate Unify prefab.");
			}
		}
		else
		{
			Debug.LogError("Unify: Unable to find game specific Unify prefab.");
		}
	}
}
