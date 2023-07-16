using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unify
{
	public class PauseEvents : MonoBehaviour
	{
		public bool muteAudio;

		public bool freezeAudio;

		public bool freezeTime;

		public bool global;

		public UnityEvent onPause;

		private float previousTimeScale = 1f;

		public GameObject[] showWhenPaused = new GameObject[1];

		private static GameObject globalInstance;

		public void Awake()
		{
			Logger.Log("UNIFY: PAUSEMANAGER: Awake");
			UnifyManager unifyManager = UnifyManager.Create();
			unifyManager.OnPlatformDetailsChanged = (UnifyManager.PlatformDetailsChanged)Delegate.Combine(unifyManager.OnPlatformDetailsChanged, new UnifyManager.PlatformDetailsChanged(OnPlatformDetailsChanged));
			if (global)
			{
				if (globalInstance != null && globalInstance != base.gameObject)
				{
					UnityEngine.Object.DestroyImmediate(base.gameObject);
					return;
				}
				Logger.Log("UNIFY: PauseEvents, global set.");
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
				globalInstance = base.gameObject;
			}
		}

		public void OnEnable()
		{
			Logger.Log("UNIFY: PAUSEMANAGER: OnEnable");
			UnifyManager unifyManager = UnifyManager.Create();
			unifyManager.OnPaused = (UnifyManager.PauseDelegate)Delegate.Combine(unifyManager.OnPaused, new UnifyManager.PauseDelegate(OnPaused));
		}

		public void OnDisable()
		{
			Logger.Log("UNIFY: PAUSEMANAGER: OnDisable");
			UnifyManager unifyManager = UnifyManager.Create();
			unifyManager.OnPaused = (UnifyManager.PauseDelegate)Delegate.Remove(unifyManager.OnPaused, new UnifyManager.PauseDelegate(OnPaused));
		}

		public void OnPlatformDetailsChanged()
		{
			Logger.Log("UNIFY: PAUSEMANAGER: Platform details changed, unpause.");
			OnPaused(paused: false);
		}

		public void OnDestroy()
		{
			Logger.Log("UNIFY: PAUSEMANAGER: OnDestroy");
			UnifyManager unifyManager = UnifyManager.Create();
			unifyManager.OnPlatformDetailsChanged = (UnifyManager.PlatformDetailsChanged)Delegate.Remove(unifyManager.OnPlatformDetailsChanged, new UnifyManager.PlatformDetailsChanged(OnPlatformDetailsChanged));
		}

		public void OnPaused(bool paused)
		{
			Logger.Log("UNIFY: PAUSEMANAGER: OnPaused: " + paused);
			if (muteAudio)
			{
				AudioListener.volume = (paused ? 0f : 1f);
			}
			if (freezeAudio)
			{
				AudioListener.pause = paused;
			}
			if (freezeTime)
			{
				if (paused)
				{
					previousTimeScale = Time.timeScale;
				}
				Time.timeScale = (paused ? 0f : previousTimeScale);
			}
			if (paused)
			{
				Logger.Log("UNIFY: PAUSEMANAGER: Invoke OnPaused");
				if (onPause != null)
				{
					onPause.Invoke();
				}
			}
			GameObject[] array = showWhenPaused;
			foreach (GameObject gameObject in array)
			{
				if (gameObject != null)
				{
					gameObject.SetActive(paused);
				}
			}
		}
	}
}
