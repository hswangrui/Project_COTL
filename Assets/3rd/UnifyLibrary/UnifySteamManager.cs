using System;
using System.Text;
using Steamworks;
using UnityEngine;

[DisallowMultipleComponent]
public class UnifySteamManager : MonoBehaviour
{
	private static uint AppId;

	protected static UnifySteamManager s_instance;

	protected static bool s_EverInitialized;

	protected bool m_bInitialized;

	private static bool m_InitializedExternal;

	protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

	public static int AppID
	{
		set
		{
			AppId = (uint)value;
		}
	}

	public static UnifySteamManager Instance => s_instance;

	public static bool Initialized
	{
		get
		{
			if (Instance == null)
			{
				return m_InitializedExternal;
			}
			return Instance.m_bInitialized;
		}
	}

	public static bool InitializedExternal
	{
		set
		{
			m_InitializedExternal = value;
		}
	}

	protected static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	protected virtual void Awake()
	{
		if (s_instance != null)
		{
			if (s_instance == this)
			{
				UnityEngine.Object.Destroy(this);
			}
			return;
		}
		s_instance = this;
		if (s_EverInitialized)
		{
			throw new Exception("Tried to Initialize the SteamAPI twice in one session!");
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (!Packsize.Test())
		{
			Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
		}
		if (!DllCheck.Test())
		{
			Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
		}
		try
		{
			if (AppId == 0)
			{
				if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
				{
					Application.Quit();
					return;
				}
			}
			else if (SteamAPI.RestartAppIfNecessary(new AppId_t(AppId)))
			{
				Application.Quit();
				return;
			}
		}
		catch (DllNotFoundException ex)
		{
			Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + ex, this);
			Application.Quit();
			return;
		}
		m_bInitialized = SteamAPI.Init();
		if (!m_bInitialized)
		{
			Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
			return;
		}
		Debug.Log("[Steamworks.NET] SteamAPI_Init() Succsess.", this);
		s_EverInitialized = true;
	}

	protected virtual void OnEnable()
	{
		if (s_instance == null)
		{
			s_instance = this;
		}
		if (m_bInitialized && m_SteamAPIWarningMessageHook == null)
		{
			m_SteamAPIWarningMessageHook = SteamAPIDebugTextHook;
			SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
		}
	}

	protected virtual void OnDestroy()
	{
		if (!(s_instance != this))
		{
			s_instance = null;
			if (m_bInitialized)
			{
				SteamAPI.Shutdown();
			}
		}
	}

	protected virtual void Update()
	{
		if (m_bInitialized)
		{
			SteamAPI.RunCallbacks();
		}
	}
}
