using UnityEngine;

public class GlobalCanvasReference : MonoBehaviour
{
	private static Transform instance;

	private static Canvas canvasInstance;

	private static bool isInitialized;

	public static Canvas CanvasInstance
	{
		get
		{
			return canvasInstance;
		}
	}

	public static Transform Instance
	{
		get
		{
			if (isInitialized)
			{
				return instance;
			}
			GameObject gameObject = GameObject.FindWithTag("Canvas");
			if ((bool)gameObject)
			{
				return gameObject.transform;
			}
			return GameObject.Find("Canvas").transform;
		}
	}

	private void Awake()
	{
		canvasInstance = base.transform.GetComponent<Canvas>();
		instance = base.transform;
		isInitialized = true;
	}

	private void OnDestroy()
	{
		canvasInstance = null;
		instance = null;
		isInitialized = false;
	}
}
