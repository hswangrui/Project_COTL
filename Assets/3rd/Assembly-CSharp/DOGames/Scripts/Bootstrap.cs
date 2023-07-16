using UnityEngine;

namespace DOGames.Scripts
{
	internal class Bootstrap
	{
		private static bool initialised;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Startup()
		{
			if (!initialised)
			{
				Debug.Log("DOGames:Bootstrap.Start()");
				Object @object = Resources.Load("Prefabs/Debug Canvas");
				if (@object == null)
				{
					Debug.LogError("DOGames:Bootstrap.Start, unabled to load prefab.");
				}
				Object.Instantiate(@object);
				initialised = true;
			}
		}
	}
}
