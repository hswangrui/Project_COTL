using System;
using UnityEngine;
using UnityEngine.UI;

namespace Unify
{
	public class UserProfile : MonoBehaviour
	{
		public GameObject overlay;

		public Text profileName;

		public void Awake()
		{
			overlay.SetActive(false);
		}

		public void Start()
		{
			SessionManager.OnSessionStart = (SessionManager.SessionEventDelegate)Delegate.Combine(SessionManager.OnSessionStart, new SessionManager.SessionEventDelegate(OnSessionStart));
			SessionManager.OnSessionEnd = (SessionManager.SessionEventDelegate)Delegate.Combine(SessionManager.OnSessionEnd, new SessionManager.SessionEventDelegate(OnSessionEnd));
			if (SessionHandler.HasSessionStarted)
			{
				ShowUserProfile(true);
			}
		}

		public void OnDestroy()
		{
			SessionManager.OnSessionStart = (SessionManager.SessionEventDelegate)Delegate.Remove(SessionManager.OnSessionStart, new SessionManager.SessionEventDelegate(OnSessionStart));
			SessionManager.OnSessionEnd = (SessionManager.SessionEventDelegate)Delegate.Remove(SessionManager.OnSessionEnd, new SessionManager.SessionEventDelegate(OnSessionEnd));
		}

		public void OnSessionStart(Guid sessionGuid, User sessionUser)
		{
			ShowUserProfile(true);
		}

		public void OnSessionEnd(Guid sessionGuid, User sessionUser)
		{
			ShowUserProfile(false);
		}

		private void ShowUserProfile(bool show)
		{
			if (show)
			{
				User sessionOwner = SessionManager.GetSessionOwner();
				if (sessionOwner != null)
				{
					profileName.text = sessionOwner.nickName;
					overlay.SetActive(true);
					UserHelper.Instance.GetUserPicture(sessionOwner, 512, delegate(Texture2D texture)
					{
						if (texture != null)
						{
							Logger.Log("USERPROFILE: profile texture: " + texture.width + "x" + texture.height);
						}
						else
						{
							Logger.Log("USERPROFILE: on profile picture available");
						}
					});
				}
				else
				{
					overlay.SetActive(false);
				}
			}
			else
			{
				overlay.SetActive(false);
			}
		}
	}
}
