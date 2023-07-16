using System;
using UnityEngine;

namespace Unify
{
	public class Presence : MonoBehaviour
	{
		public string presenceString;

		public void Start()
		{
			SetPresence();
			SessionManager.OnSessionStart = (SessionManager.SessionEventDelegate)Delegate.Combine(SessionManager.OnSessionStart, (SessionManager.SessionEventDelegate)delegate
			{
				SetPresence();
			});
		}

		public void SetPresence()
		{
			Logger.Log("UNIFY:PRESENCE: Set Presence:" + presenceString);
			User sessionOwner = SessionManager.GetSessionOwner();
			if (sessionOwner != null && sessionOwner.IsSignedIn())
			{
				UserHelper.Instance.SetPresence(sessionOwner, presenceString);
			}
			else
			{
				Logger.Log("UNIFY:PRESENCE: No action, User not signed in.");
			}
		}
	}
}
