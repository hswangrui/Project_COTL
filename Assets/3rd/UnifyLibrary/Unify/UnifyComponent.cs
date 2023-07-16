using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unify
{
	public class UnifyComponent : MonoBehaviour
	{
		public bool dontDestroy;

		private static UnifyComponent instance;

		private static GameObject unifyGameObject;

		private UnifyManager unifyManager;

		private Queue<Action> mainThreadQueue = new Queue<Action>();

		[Header("Debug Settings")]
		public bool debugWriteFail;

		public bool debugReadFail;

		public bool debugDenyNetwork;

		[Space]
		public bool EnableEmulation;

		public UnifyManager.Platform emulatePlatform;

		public SystemLanguage emulateLanguage = SystemLanguage.Unknown;

		[Header("Logging")]
		public bool LogInfo = true;

		public bool LogWarning = true;

		public bool LogError = true;

		[Header("Intent")]
		public string IntentTest;

		public static UnifyComponent Instance => instance;

		public void Start()
		{
			if (unifyGameObject != null && unifyGameObject != base.gameObject)
			{
				UnityEngine.Object.DestroyImmediate(base.gameObject);
				return;
			}
			unifyGameObject = base.gameObject;
			instance = this;
			ApplySettings();
			ApplyDebugSettings();
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		public void Awake()
		{
			Logger.Log("UNIFY: Awake");
			unifyManager = UnifyManager.Create();
			if (dontDestroy)
			{
				Logger.Log("UNIFY: Don't destroy on load set.");
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			}
			Logger.Log("UNIFY: Awake, returns.");
			EnableEmulation = false;
			debugReadFail = false;
			debugWriteFail = false;
		}

		public void ApplySettings()
		{
			if (unifyManager != null)
			{
				Logger.EnabledCategories = (LogInfo ? Logger.Category.INFO : Logger.Category.NONE) | (LogWarning ? Logger.Category.WARNING : Logger.Category.NONE) | (LogError ? Logger.Category.ERROR : Logger.Category.NONE);
			}
		}

		public void ApplyDebugSettings()
		{
			if (unifyManager == null || !Application.isEditor)
			{
				return;
			}
			if (EnableEmulation)
			{
				if (UnifyManager.platform != emulatePlatform || UnifyManager.language != emulateLanguage)
				{
					Logger.Log("UNIFY: Platform/Language emulation changed");
					unifyManager.SetPlatform(emulatePlatform, emulateLanguage);
				}
			}
			else
			{
				unifyManager.SetPlatformFromDevice();
			}
			UnifyManager.debugReadFail = debugReadFail;
			UnifyManager.debugWriteFail = debugWriteFail;
			UnifyManager.debugDenyNetwork = debugDenyNetwork;
		}

		public void Update()
		{
			UserHelper.Instance.Update();
			lock (mainThreadQueue)
			{
				while (mainThreadQueue.Count > 0)
				{
					mainThreadQueue.Dequeue()();
				}
			}
			unifyManager.Update();
		}

		public void OnApplicationFocus(bool hasFocus)
		{
			if (unifyManager != null)
			{
				unifyManager.Pause(!hasFocus);
			}
		}

		public void OnApplicationPause(bool pauseStatus)
		{
			if (unifyManager != null)
			{
				unifyManager.Pause(pauseStatus);
			}
		}

		private static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			string text = scene.name;
			UnifyManager.Instance.AutomationClient.SendGameEvent("SceneLoaded=" + text);
		}

		public void MainThreadEnqueue(IEnumerator action)
		{
			lock (mainThreadQueue)
			{
				mainThreadQueue.Enqueue(delegate
				{
					StartCoroutine(action);
				});
			}
		}

		public void MainThreadEnqueue(Action action)
		{
			MainThreadEnqueue(ActionWrapper(action));
		}

		public Task MainThreadEnqueueAsync(Action action)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			MainThreadEnqueue(ActionWrapper(delegate
			{
				try
				{
					action();
					tcs.TrySetResult(result: true);
				}
				catch (Exception exception)
				{
					tcs.TrySetException(exception);
				}
			}));
			return tcs.Task;
		}

		private IEnumerator ActionWrapper(Action action)
		{
			action();
			yield return null;
		}
	}
}
