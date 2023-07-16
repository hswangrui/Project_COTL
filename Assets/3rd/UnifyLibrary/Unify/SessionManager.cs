using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using Unify.Automation;
using Unify.Input;
using UnityEngine;

namespace Unify
{
	public class SessionManager : MonoBehaviour
	{
		public delegate void SessionEventDelegate(Guid sessionGuid, User sessionUser);

		public delegate void SessionSaveResult(string key, bool success);

		public enum SessionState
		{
			None = 0,
			Loading = 1,
			LoadingFailed = 2,
			SaveFailed = 3,
			Error = 4,
			SaveSuccess = 5,
			DeleteCorrupt = 6,
			Started = 7,
			InProgress = 8,
			Ending = 9,
			Ended = 10
		}

		public static SessionEventDelegate OnSessionLoading;

		public static SessionEventDelegate OnSessionStart;

		public static SessionEventDelegate OnSessionLoadError;

		public static SessionEventDelegate OnSessionEnd;

		public static SessionEventDelegate OnSessionContinue;

		public static SessionSaveResult OnSessionSaveResult;

		public float delayBetweenSaves = 1f;

		public string[] syncSaveFiles = new string[1] { "save_data" };

		private static HashSet<string> toSync = new HashSet<string>();

		private static User sessionOwner;

		private static Guid sessionGuid = Guid.Empty;

		private static bool busyDataSync = false;

		private static bool? invokeSaveResult = null;

		private static string saveResultKey = null;

		private static List<string> loadErrorList = new List<string>();

		private static float lastSaveTime = 0f;

		private static SessionState state;

		public static SessionManager instance;

		public static bool SteamManagerInitialized = false;

		protected Callback<UserAchievementStored_t> m_UserAchievementStored;

		private CGameID m_GameID;

		private int SteamManagerInitializedCount;

		private int sessionEndTimeout;

		public SessionManager Instance => instance;

		public SessionState State => state;

		public bool HasStarted { get; private set; }

		public void Start()
		{
			Logger.Log("UNIFY:SESSION: Start");
			UserHelper.RegisterPlayer(0, null, GamePad.None);
			UserHelper.OnPlayerUserChanged = (UserHelper.PlayerUserChangedDelegate)Delegate.Combine(UserHelper.OnPlayerUserChanged, new UserHelper.PlayerUserChangedDelegate(OnUserChanged));
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			instance = this;
			UserHelper.Clear();
			sessionOwner = null;
			sessionGuid = Guid.Empty;
			state = SessionState.None;

			//StartCoroutine("WaitForSteamInit");
		}

		private IEnumerator WaitForSteamInit()
		{
			while (!SteamManagerInitialized)
			{
				try
				{
					m_GameID = new CGameID(SteamUtils.GetAppID());
					m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
					SteamManagerInitialized = true;
					Debug.Log("UNIFY:Steam: steamworks initialized");
				}
				catch
				{
					SteamManagerInitializedCount++;
					Debug.Log("UNIFY:Steam: waiting for steamworks to initialize.");
				}
				if (!SteamManagerInitialized)
				{
					yield return new WaitForSeconds(1f);
				}
				SteamManagerInitializedCount++;
				if (SteamManagerInitializedCount > 10)
				{
					Debug.LogError("UNIFY:Steam: cannot initialize steamworks.");
					Application.Quit();
				}
			}
		}

		private void OnAchievementStored(UserAchievementStored_t param)
		{
			if (SteamManagerInitialized && (ulong)m_GameID == param.m_nGameID)
			{
				if (param.m_nMaxProgress == 0)
				{
					Debug.Log("Achievement '" + param.m_rgchAchievementName + "' unlocked!");
					return;
				}
				Debug.Log("Achievement '" + param.m_rgchAchievementName + "' progress callback, (" + param.m_nCurProgress + "," + param.m_nMaxProgress + ")");
			}
		}

		public void OnDestroy()
		{
			Logger.Log("SESSIONMANAGER: Destroy");
			UserHelper.OnPlayerUserChanged = (UserHelper.PlayerUserChangedDelegate)Delegate.Remove(UserHelper.OnPlayerUserChanged, new UserHelper.PlayerUserChangedDelegate(OnUserChanged));
		}

		public void OnUserChanged(int playerNo, User was, User now)
		{
			Logger.Log("SESSIONMANAGER: OnUserChanged");
			if (sessionOwner == null && now != null)
			{
				Logger.Log("SESSIONMGR: OnUserChanged: New user signed in.");
				StartSession(now);
			}
			else if (playerNo == 0 && now != sessionOwner)
			{
				Logger.Log("SESSIONMGR: OnUserChanged: Main user has signed out.");
				EndSession();
			}
		}

		public void StartSession(User user)
		{
			state = SessionState.Loading;
			sessionOwner = user;
			sessionGuid = Guid.NewGuid();
			OnSessionLoading?.Invoke(sessionGuid, sessionOwner);
			Logger.Log("UNIFY:SESSION: StartSession for user: " + user.nickName);
			if (Achievements.Instance != null)
			{
				Achievements.Instance.GetCount();
				_ = 0;
			}
			Activities.Init();
			SaveData.Clear();
			toSync.Clear();
			string[] array = syncSaveFiles;
			foreach (string item in array)
			{
				toSync.Add(item);
			}
		}

		public void EndSession()
		{
			Logger.Log("UNIFY:SESSION: EndSession: " + sessionGuid);
			state = SessionState.Ending;
		}

		public void ContinueAfterLoadingFailure()
		{
			Logger.Log("UNIFY:SESSION: ContinueAfterLoadingFailure");
			foreach (string loadError in loadErrorList)
			{
				Logger.Log("UNIFY:SESSION: deleting corrupt file: " + loadError);
				SaveData.Delete(loadError, force: true);
			}
			loadErrorList.Clear();
			state = SessionState.DeleteCorrupt;
		}

		public void CancelAfterLoadingFailure()
		{
			Logger.Log("UNIFY:SESSION: CancelAfterLoadingFailure");
			loadErrorList.Clear();
			EndSession();
		}

		public void ContinueAfterSaveFailure()
		{
			Logger.Log("UNIFY:SESSION: ContinueAfterSaveFailure");
			saveResultKey = null;
			OnSessionContinue?.Invoke(sessionGuid, sessionOwner);
			state = SessionState.InProgress;
		}

		public void RetryAfterSaveFailure()
		{
			Logger.Log("UNIFY:SESSION: RetryAfterLoadingFailure");
			OnSessionContinue?.Invoke(sessionGuid, sessionOwner);
			SaveData.Retry(saveResultKey);
			saveResultKey = null;
			state = SessionState.InProgress;
		}

		public static User GetSessionOwner()
		{
			return sessionOwner;
		}

		public static Guid GetSessionGuid()
		{
			return sessionGuid;
		}

		public void UnlockAchievement(Achievement achievement, uint percentComplete = 100u)
		{
			Logger.Log("SESSIONMANAGER: UnlockAchievement: " + achievement);
			if (achievement == Achievement.None)
			{
				Logger.Log("SESSIONMANAGER: UnlockAchievement: ignoring invalid achievement.");
			}
			else if (sessionOwner != null)
			{
				if (SteamManagerInitialized && percentComplete >= 100)
				{
					SteamUserStats.SetAchievement(achievement.steamId);
					SteamUserStats.StoreStats();
				}
				UnifyManager.Instance.AutomationClient.SendGameEvent("AchievementUpdated=" + achievement.id + "," + percentComplete);
			}
			else
			{
				Logger.Log("SESSIONMANAGER: UnlockAchievement, user is null");
			}
		}

		private void ProcessState()
		{
			switch (state)
			{
			case SessionState.Loading:
				if (toSync.Count > 0 || busyDataSync)
				{
					if (!busyDataSync)
					{
						ProcessLoadSync();
					}
				}
				else if (loadErrorList.Count > 0)
				{
					state = SessionState.LoadingFailed;
				}
				else
				{
					state = SessionState.Started;
				}
				break;
			case SessionState.LoadingFailed:
				Logger.Log(string.Concat("UNIFY:SESSION: LoadingFailed: ", sessionGuid, ", for user: ", sessionOwner.nickName));
				OnSessionLoadError?.Invoke(sessionGuid, sessionOwner);
				UnifyManager.Instance.AutomationClient.SendEvent(Client.Event.SESSION_LOADFAIL);
				state = SessionState.Error;
				break;
			case SessionState.DeleteCorrupt:
				if (!ProcessDeleteSync())
				{
					OnSessionContinue?.Invoke(sessionGuid, sessionOwner);
					state = SessionState.Started;
				}
				break;
			case SessionState.SaveFailed:
				OnSessionSaveResult?.Invoke(saveResultKey, success: false);
				state = SessionState.Error;
				UnifyManager.Instance.AutomationClient.SendEvent(Client.Event.SESSION_SAVEFAIL);
				break;
			case SessionState.Started:
				Logger.Log(string.Concat("UNIFY:SESSION: New Session Started: ", sessionGuid, ", for user: ", sessionOwner.nickName));
				HasStarted = true;
				OnSessionStart?.Invoke(sessionGuid, sessionOwner);
				UnifyManager.Instance.AutomationClient.SendEvent(Client.Event.SESSION_START);
				state = SessionState.InProgress;
				break;
			case SessionState.SaveSuccess:
				OnSessionSaveResult?.Invoke(saveResultKey, success: true);
				saveResultKey = null;
				state = SessionState.InProgress;
				break;
			case SessionState.Ending:
				Logger.Log(string.Concat("UNIFY:SESSION: Session Ended: ", sessionGuid, ", for user: ", sessionOwner.nickName));
				sessionGuid = Guid.Empty;
				sessionOwner = null;
				SaveData.Clear();
				HasStarted = false;
				UserHelper.DisengageAllPlayers();
				state = SessionState.Ended;
				sessionEndTimeout = 60;
				break;
			case SessionState.Ended:
				sessionEndTimeout--;
				if (busyDataSync)
				{
					if (sessionEndTimeout > 0)
					{
						Logger.Log("SessionManager: Waiting for pending saves to complete.");
						break;
					}
					Logger.Log("SessionManager: Timed out waiting for pending saves to complete.");
				}
				busyDataSync = false;
				UnifyManager.Instance.AutomationClient.SendEvent(Client.Event.SESSION_END);
				OnSessionEnd?.Invoke(sessionGuid, sessionOwner);
				state = SessionState.None;
				break;
			case SessionState.Error:
			case SessionState.InProgress:
				break;
			}
		}

		public void Update()
		{
			ProcessState();
			if (invokeSaveResult.HasValue)
			{
				OnSessionSaveResult?.Invoke(saveResultKey, invokeSaveResult.Value);
				invokeSaveResult = null;
			}
			if (sessionOwner != null && state >= SessionState.InProgress && !busyDataSync && !ProcessDeleteSync())
			{
				ProcessSaveSync();
			}
		}

		private void ProcessLoadSync()
		{
			Logger.Log("UNIFY:SAVEDATA: ProcessLoadSync - " + toSync.Count);
			string key = toSync.First();
			toSync.Remove(key);
			Logger.Log("UNIFY:SESSION:Loading data for key: " + key);
			busyDataSync = true;
			sessionOwner.OpenStorage(delegate(UserStorage storage)
			{
				if (storage == null)
				{
					CancelSave();
					EndSession();
				}
				else
				{
					Logger.Log("UNIFY:SESSION: Opened user storage, now loading: " + key);
					storage.readKey(key, OnLoadSuccess, OnLoadFail);
				}
			});
		}

		private bool ProcessDeleteSync()
		{
			User user = sessionOwner;
			if (busyDataSync)
			{
				return true;
			}
			if (SaveData.HasKeyToDelete)
			{
				string key = SaveData.PopKeyToDelete();
				busyDataSync = true;
				Logger.Log("SESSIONMANAGER:SAVEDATA: Need to delete: " + key);
				user.OpenStorage(delegate(UserStorage storage)
				{
					if (storage == null)
					{
						Logger.LogError("SESSIONMANAGER: Could not open storage for a delete");
						busyDataSync = false;
					}
					else
					{
						storage.deleteKey(key, delegate
						{
							Logger.Log("SESSIONMANAGER: deleted save file: " + key);
							user.CloseStorage();
							busyDataSync = false;
						}, delegate
						{
							Logger.LogError("SESSIONMANAGER: could not delete save file: " + key);
							user.CloseStorage();
							busyDataSync = false;
						});
					}
				});
				return true;
			}
			return false;
		}

		private bool ProcessSaveSync()
		{
			User user = sessionOwner;
			if (SaveData.HasKeyToSave && Time.realtimeSinceStartup - lastSaveTime > delayBetweenSaves)
			{
				saveResultKey = SaveData.PopKeyToSave();
				Logger.Log("SESSIONMANAGER:SAVEDATA: Need to save changed data: " + saveResultKey);
				if (user != null)
				{
					busyDataSync = true;
					user.OpenStorage(delegate(UserStorage storage)
					{
						if (storage == null)
						{
							CancelSave();
						}
						else
						{
							Logger.Log("SESSIONMANAGER: Opened user storage for save");
							storage.writeKeyValue(saveResultKey, SaveData.GetBytes(saveResultKey), OnSaveSuccess, OnSaveFail);
						}
					});
				}
				else
				{
					Logger.LogError("SESSIONMANAGER: Unable to save, no user for main player");
				}
				lastSaveTime = Time.realtimeSinceStartup;
				return true;
			}
			return false;
		}

		public void FlushSaveWithNotify(string file, SessionSaveResult callback)
		{
			SessionSaveResult ourCallback = null;
			if (!SaveData.IsDirty(file))
			{
				Logger.LogError("UNIFY: WaitForSaveResult, key is not dirty: " + file);
				return;
			}
			SaveData.FlushKey(file);
			OnSessionSaveResult = (SessionSaveResult)Delegate.Combine(OnSessionSaveResult, ourCallback = delegate(string key, bool success)
			{
				if (key == file)
				{
					callback(key, success);
					OnSessionSaveResult = (SessionSaveResult)Delegate.Remove(OnSessionSaveResult, ourCallback);
				}
				else
				{
					Logger.LogWarning("FlushSaveWithNotify: Recieved call back for different key: " + key);
				}
			});
		}

		private string encodeSaveData(Dictionary<string, string> dict)
		{
			string text = "";
			foreach (KeyValuePair<string, string> item in dict)
			{
				text = text + item.Key + "=" + item.Value + ";";
			}
			Logger.Log("Encoded: " + text);
			return text;
		}

		private void decodeSaveData(Dictionary<string, string> dict, string data)
		{
			string[] array = data.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('=');
				if (array2.Length == 2)
				{
					dict[array2[0]] = array2[1];
				}
			}
		}

		public void CancelSave()
		{
			if (sessionOwner != null)
			{
				sessionOwner.CloseStorage();
			}
			busyDataSync = false;
		}

		public void RetrySave(string key)
		{
			SaveData.Retry(key);
		}

		public void OnLoadSuccess(string key, byte[] data, string result)
		{
			if (state != SessionState.Loading)
			{
				CancelSave();
				return;
			}
			Logger.Log("SESSIONMANAGER: LOAD SUCCESS");
			SaveData._Store(key, data);
			CancelSave();
		}

		public void OnLoadFail(string key, byte[] data, string result)
		{
			if (state != SessionState.Loading)
			{
				CancelSave();
				return;
			}
			Logger.Log("SESSIONMANAGER: LOAD FAIL");
			if (result == "error:notfound")
			{
				Logger.Log("SESSIONMANAGER: save not found.");
				CancelSave();
				return;
			}
			if (result != "error:toomanyops")
			{
				loadErrorList.Add(key);
			}
			CancelSave();
		}

		public void OnSaveSuccess(string key, byte[] data, string result)
		{
			if (state < SessionState.InProgress)
			{
				CancelSave();
				return;
			}
			Logger.Log("SESSIONMANAGER: SAVE SUCCESS");
			CancelSave();
			state = SessionState.SaveSuccess;
		}

		public void OnSaveFail(string key, byte[] data, string result)
		{
			if (state < SessionState.InProgress)
			{
				CancelSave();
				return;
			}
			Logger.Log("SESSIONMANAGER: SAVE FAIL");
			CancelSave();
			state = SessionState.SaveFailed;
		}

		public IEnumerator AwaitSaveResultCoroutine(string file, Action<bool> resultCallback)
		{
			SessionSaveResult ourCallback = null;
			bool complete = false;
			bool success = false;
			OnSessionSaveResult = (SessionSaveResult)Delegate.Combine(OnSessionSaveResult, ourCallback = delegate(string key, bool resultSuccess)
			{
				if (key == file)
				{
					Debug.Log("Unify SessionManger.AwaitSaveResultCoroutine() key: " + key + "resultSuccess: " + resultSuccess);
					complete = true;
					success = resultSuccess;
					OnSessionSaveResult = (SessionSaveResult)Delegate.Remove(OnSessionSaveResult, ourCallback);
				}
				else
				{
					Logger.LogWarning("UNIFY:SaveData.FlushCoroutine: Ignoring callback for different key: " + key);
				}
			});
			if (!SaveData.IsDirty(file))
			{
				Logger.LogError("UNIFY:SaveData.Flush, key is not dirty: " + file);
				resultCallback(obj: false);
			}
			while (!complete && !UnifyManager.isSuspending)
			{
				yield return null;
			}
			resultCallback(success);
		}
	}
}
