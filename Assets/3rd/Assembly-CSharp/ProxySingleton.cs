using UnityEngine;

public abstract class ProxySingleton<T> : MonoSingleton<T> where T : MonoBehaviour
{
	public override void Awake()
	{
		T component;
		if (TryGetComponent<T>(out component))
		{
			if ((Object)MonoSingleton<T>.Instance != (Object)component)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				Object.DontDestroyOnLoad(this);
			}
		}
	}
}
