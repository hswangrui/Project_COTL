using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	public static T Instance
	{
		get
		{
			if ((Object)_instance == (Object)null)
			{
				_instance = Object.FindObjectOfType<T>();
			}
			return _instance;
		}
	}

	public virtual void Awake()
	{
		if (Instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			Object.DontDestroyOnLoad(this);
		}
	}

	public virtual void Start()
	{
	}
}
