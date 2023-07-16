using System;
using UnityEngine;

namespace Unify
{
	public class RequestAuthorizer
	{
		public delegate void AuthorizedCallback(bool authorized);

		public const bool SUPRESS_ERRORS = true;

		public virtual bool AuthorizeRequest(WWWForm form, bool silent = false)
		{
			Logger.Log("REQ: Default allow all request implementation.");
			return true;
		}

		public virtual bool AuthorizeRequest(ref string token, bool silent = false)
		{
			Logger.Log("REQ: Default allow all request implementation.");
			token = (UnifyManager.debugDenyNetwork ? null : "TEST_TOKEN");
			if (!UnifyManager.debugDenyNetwork)
			{
				return true;
			}
			return false;
		}

		public virtual void Cancel()
		{
			Logger.Log("REQ: Default cancel.");
		}

		public virtual void ConnectToNetwork(User user, Action<User, bool> callback)
		{
			if (user == null)
			{
				user = UserHelper.GetPlayer(0);
			}
			Logger.Log("REQ: ConnectToNetwork user.id: " + user.id + " user.nickName: " + user.nickName);
			callback(user, arg2: true);
		}

		public virtual void Disconnect()
		{
			Logger.Log("REQ: ConnectToNetwork.");
		}

		public virtual string OnlineUserID()
		{
			Logger.Log("REQ: OnlineUserID. ");
			return "";
		}
	}
}
