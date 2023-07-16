using System;
using Lamb.UI;
using MMTools;
using src.UINavigator;
using Unify;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SessionHandler : MonoBehaviour
{
	public static bool DLC_Checked;

	public static bool HasSessionStarted { get; private set; }

	public void Start()
	{
		SessionManager.OnSessionStart = (SessionManager.SessionEventDelegate)Delegate.Combine(SessionManager.OnSessionStart, new SessionManager.SessionEventDelegate(OnSessionStart));
		SessionManager.OnSessionEnd = (SessionManager.SessionEventDelegate)Delegate.Combine(SessionManager.OnSessionEnd, new SessionManager.SessionEventDelegate(OnSessionEnd));
	}

	public void OnDestroy()
	{
		SessionManager.OnSessionStart = (SessionManager.SessionEventDelegate)Delegate.Remove(SessionManager.OnSessionStart, new SessionManager.SessionEventDelegate(OnSessionStart));
		SessionManager.OnSessionEnd = (SessionManager.SessionEventDelegate)Delegate.Remove(SessionManager.OnSessionEnd, new SessionManager.SessionEventDelegate(OnSessionEnd));
	}

	public void OnSessionStart(Guid sessionGuid, User sessionUser)
	{
		HasSessionStarted = true;
		InputManager.General.ResetBindings();
		base.gameObject.AddComponent<AchievementsWrapper>();
	}

	public void OnSessionEnd(Guid sessionGuid, User sessionUser)
	{
		HasSessionStarted = false;
		Time.timeScale = 1f;
		GameManager.InMenu = false;
		PhotoModeManager.PhotoModeActive = false;
		PhotoModeManager.TakeScreenShotActive = false;
		MMTransition.ForceStopMMTransition();
		MMVideoPlayer.ForceStopVideo();
		MMTransition.ResumePlay();
		MonoSingleton<UINavigatorNew>.Instance.Clear();
		UIMenuBase.ActiveMenus.Clear();
		SimulationManager.Pause();
		FollowerManager.Reset();
		StructureManager.Reset();
		DeviceLightingManager.Reset();
		UIDynamicNotificationCenter.Reset();
		UnityEngine.Object.Destroy(base.gameObject.GetComponent<AchievementsWrapper>());
		//TwitchManager.Abort();
	}

	public void ResetToTitle()
	{
		Addressables.LoadSceneAsync("Assets/Scenes/Main Menu.unity");
	}

	public void UnlockTestAchievement()
	{
		Achievement achievementId = Achievements.Instance.Lookup("test");
		AchievementsWrapper.UnlockAchievement(achievementId);
	}

	public void Update()
	{
		if (PhotoModeManager.PhotoModeActive)
		{
			Time.timeScale = 0f;
		}
	}
}
