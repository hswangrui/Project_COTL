using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unify
{
	public class SessionEvents : MonoBehaviour
	{
		public UnityEvent onLoading;

		public UnityEvent onStart;

		public UnityEvent onEnd;

		public UnityEvent onLoadFail;

		public UnityEvent onSaveFail;

		public UnityEvent onContinue;

		private string failedKey;

		public void Start()
		{
			Logger.Log("UNIFY:SESSIONEVENTS: Start");
			SessionManager.OnSessionLoading = (SessionManager.SessionEventDelegate)Delegate.Combine(SessionManager.OnSessionLoading, (SessionManager.SessionEventDelegate)delegate
			{
				Logger.Log("UNIFY:SESSIONEVENTS: OnSessionLoading Invoke");
				onLoading?.Invoke();
			});
			SessionManager.OnSessionStart = (SessionManager.SessionEventDelegate)Delegate.Combine(SessionManager.OnSessionStart, (SessionManager.SessionEventDelegate)delegate
			{
				Logger.Log("UNIFY:SESSIONEVENTS: OnSessionStart Invoke");
				onStart?.Invoke();
			});
			SessionManager.OnSessionLoadError = (SessionManager.SessionEventDelegate)Delegate.Combine(SessionManager.OnSessionLoadError, (SessionManager.SessionEventDelegate)delegate
			{
				Logger.Log("UNIFY:SESSIONEVENTS: OnSessionLoadError Invoke");
				onLoadFail?.Invoke();
			});
			SessionManager.OnSessionEnd = (SessionManager.SessionEventDelegate)Delegate.Combine(SessionManager.OnSessionEnd, (SessionManager.SessionEventDelegate)delegate
			{
				Logger.Log("UNIFY:SESSIONEVENTS: OnSessionEnd Invoke");
				onEnd?.Invoke();
			});
			SessionManager.OnSessionSaveResult = (SessionManager.SessionSaveResult)Delegate.Combine(SessionManager.OnSessionSaveResult, (SessionManager.SessionSaveResult)delegate(string key, bool result)
			{
				Logger.Log("UNIFY:SESSIONEVENTS: OnSessionSaveResult: " + key + ", result: " + result);
				if (!result)
				{
					onSaveFail?.Invoke();
					failedKey = key;
				}
			});
			SessionManager.OnSessionContinue = (SessionManager.SessionEventDelegate)Delegate.Combine(SessionManager.OnSessionContinue, (SessionManager.SessionEventDelegate)delegate
			{
				Logger.Log("UNIFY:SESSIONEVENTS: OnSessionContinue");
				onContinue?.Invoke();
			});
		}

		public void ContinueAfterLoadingFailure()
		{
			SessionManager.instance.ContinueAfterLoadingFailure();
		}

		public void CancelAfterLoadingFailure()
		{
			SessionManager.instance.CancelAfterLoadingFailure();
		}

		public void ContinueAfterSaveFailure()
		{
			SessionManager.instance.ContinueAfterSaveFailure();
		}

		public void RetryAfterSaveFailure()
		{
			SessionManager.instance.RetryAfterSaveFailure();
		}
	}
}
