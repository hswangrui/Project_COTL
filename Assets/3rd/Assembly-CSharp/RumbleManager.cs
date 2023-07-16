using UnityEngine;

public class RumbleManager : BaseMonoBehaviour
{
	private static RumbleManager _instance;

	public static RumbleManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = (Object.Instantiate(Resources.Load("MMVibrate/RumbleManager")) as GameObject).GetComponent<RumbleManager>();
			}
			return _instance;
		}
	}

	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		_instance = this;
		if (base.transform.parent != null)
		{
			base.transform.SetParent(null);
		}
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public void Rumble()
	{
		MMVibrate.Haptic(MMVibrate.HapticTypes.SoftImpact, false, true, this);
	}
}
