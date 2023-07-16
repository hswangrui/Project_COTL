using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Threading;
using Unify.Automation;
using UnityEngine;

namespace Unify
{
	public class UnifyManager
	{
		public enum Platform
		{
			None = 0,
			Standalone = 1,
			XboxOne = 2,
			PS4 = 3,
			Switch = 4,
			GameCore = 5,
			PS5 = 6,
			GameCoreConsole = 7
		}

		public delegate void PlatformDetailsChanged();

		public delegate void UserControllerConnected(int playerNo, User user, bool connected);

		public delegate void PauseDelegate(bool paused);

		public delegate void IntentDelegate(UnifyManager manager, LaunchIntent intent);

		public static UnifyManager instance;

		public static bool isPaused = false;

		public static bool isSuspending = false;

		public static bool hasResumed = false;

		private static bool disableUserControllerConnectedEvent = false;

		public static bool debugWriteFail = false;

		public static bool debugReadFail = false;

		public static bool debugDenyNetwork = false;

		public static Platform platform;

		public static SystemLanguage language = SystemLanguage.Unknown;

		public PlatformDetailsChanged OnPlatformDetailsChanged;

		public UserControllerConnected OnUserControllerConnected;

		public Client AutomationClient;

		private ConcurrentQueue<Tuple<string, string>> commandQueue = new ConcurrentQueue<Tuple<string, string>>();

		public PauseDelegate OnPaused;

		public IntentDelegate OnIntentNotification;

		public static UnifyManager Instance => instance;

		public bool DisableUserControllerConnectedEvent
		{
			get
			{
				return disableUserControllerConnectedEvent;
			}
			set
			{
				disableUserControllerConnectedEvent = value;
			}
		}

		public LaunchIntent PendingLaunchIntent { get; set; }

		public static bool SplashScreenSkipFlag => false;

		public UnifyManager()
		{
			Logger.Log("UNIFY: Constructor");
			UserHelper.Init();
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
			SetPlatformFromDevice();
			LifecycleHelper.Init();
			AutomationClient = null;
			try
			{
				Logger.Log("Try to find Unify Automation.WebSocketClient...");
				Type type = Type.GetType("Unify.Automation.WebSocketClient, UnifyAutomationPlugin");
				if (type != null)
				{
					Logger.Log("Found");
					AutomationClient = (Client)Activator.CreateInstance(type);
				}
				else
				{
					Logger.Log("Not Found");
				}
			}
			catch (Exception)
			{
				Logger.Log("Exception");
			}
			if (AutomationClient == null)
			{
				AutomationClient = new Client();
			}
			Logger.Log("UNIFY: Constructor, returns.");
		}

		public void SetPlatformFromDevice()
		{
			Platform p = Platform.Standalone;
			SystemLanguage systemLanguage = Application.systemLanguage;
			SetPlatform(p, systemLanguage);
		}

		public void SetPlatform(Platform p, SystemLanguage l)
		{
			if (platform != p || language != l)
			{
				platform = p;
				language = l;
				Logger.Log("UNIFY: SetPlatform: " + platform.ToString() + ", " + language);
				if (OnPlatformDetailsChanged != null)
				{
					OnPlatformDetailsChanged();
				}
			}
		}

		public void SetLanguage(SystemLanguage l)
		{
			Logger.Log("UNIFY: Set Language directly to: " + l);
			SetPlatform(platform, l);
		}

		public static UnifyManager Create()
		{
			if (instance == null)
			{
				instance = new UnifyManager();
			}
			return instance;
		}

		public static UnifyManager Get()
		{
			return instance;
		}

		public void Update()
		{
			if (!isSuspending && hasResumed)
			{
				hasResumed = false;
			}
			if (!disableUserControllerConnectedEvent)
			{
				int num = 0;
				User[] allPlayers = UserHelper.GetAllPlayers();
				foreach (User user in allPlayers)
				{
					if (user != null)
					{
						bool flag = user.gamePadId.IsConnected();
						if (user.gamePadId.connected != flag)
						{
							Logger.Log("UNIFY: Controller connected: " + flag.ToString() + ", player: " + num);
							OnUserControllerConnected?.Invoke(num, user, flag);
						}
						user.gamePadId.connected = flag;
					}
					num++;
				}
			}
			ProcessWebClientCommand();
		}

		public void UpdateAllPlayers()
		{
		}

		public void Destroy()
		{
			UserHelper.Instance.Destroy();
			LifecycleHelper.Instance.Destroy();
		}

		public void Pause(bool paused)
		{
			if (paused != isPaused)
			{
				isPaused = paused;
				if (OnPaused != null)
				{
					OnPaused(isPaused);
				}
				if (!isPaused)
				{
					hasResumed = true;
				}
			}
		}

		public void SetLaunchIntent(User user, string action)
		{
			PendingLaunchIntent = new LaunchIntent(user, action);
			if (OnIntentNotification != null)
			{
				UnifyComponent.Instance.MainThreadEnqueue(delegate
				{
					Logger.Log($"UNIFY: OnIntentNotification callback for user({user}): {action}");
					OnIntentNotification(this, PendingLaunchIntent);
				});
			}
		}

		public System.Version GetLibraryVersion()
		{
			return Assembly.GetExecutingAssembly().GetName().Version;
		}

		public string GetLibrarySemanticVersion()
		{
			return UnifyLibraryVersion.SemanticVersion;
		}

		public void QueueWebClientCommand(string command, string data)
		{
			Tuple<string, string> item = new Tuple<string, string>(command, data);
			commandQueue.Enqueue(item);
		}

		private void ProcessWebClientCommand()
		{
			if (commandQueue.Count > 0)
			{
				Tuple<string, string> result = new Tuple<string, string>("", "");
				commandQueue.TryDequeue(out result);
				string item = result.Item1;
				string item2 = result.Item2;
				AutomationClient.OnCommandRecieved(item, item2);
			}
		}
	}
}
