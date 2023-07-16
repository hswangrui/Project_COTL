using UnityEngine;

public class BlackSoulUpdater : MonoBehaviour
{
	private static BlackSoulUpdater instance;

	private static GameObject blacksoulupdateGO;

	public static BlackSoulUpdater Instance
	{
		get
		{
			if (instance == null)
			{
				blacksoulupdateGO = new GameObject("blacksoulupdate");
				instance = blacksoulupdateGO.AddComponent<BlackSoulUpdater>();
			}
			return instance;
		}
	}
}
