using System;
using Unify.Input;
using UnityEngine;

namespace Unify
{
	public class ControllerSwitch : MonoBehaviour
	{
		public int player;

		public bool invert;

		public GameObject[] showWhenConnected = new GameObject[1];

		private bool connected = true;

		private UnifyManager unifyManager;

		public void Start()
		{
			unifyManager = UnifyManager.Create();
			UnifyManager obj = unifyManager;
			obj.OnUserControllerConnected = (UnifyManager.UserControllerConnected)Delegate.Combine(obj.OnUserControllerConnected, new UnifyManager.UserControllerConnected(OnUserControllerConnected));
			UserHelper.OnPlayerGamePadChanged = (UserHelper.PlayerGamePadChangedDelegate)Delegate.Combine(UserHelper.OnPlayerGamePadChanged, new UserHelper.PlayerGamePadChangedDelegate(OnPlayerGamePadChanged));
			UserHelper.OnPlayerUserChanged = (UserHelper.PlayerUserChangedDelegate)Delegate.Combine(UserHelper.OnPlayerUserChanged, new UserHelper.PlayerUserChangedDelegate(OnPlayerUserChanged));
		}

		public void OnDestroy()
		{
			if (unifyManager != null)
			{
				UnifyManager obj = unifyManager;
				obj.OnUserControllerConnected = (UnifyManager.UserControllerConnected)Delegate.Remove(obj.OnUserControllerConnected, new UnifyManager.UserControllerConnected(OnUserControllerConnected));
				UserHelper.OnPlayerGamePadChanged = (UserHelper.PlayerGamePadChangedDelegate)Delegate.Remove(UserHelper.OnPlayerGamePadChanged, new UserHelper.PlayerGamePadChangedDelegate(OnPlayerGamePadChanged));
				UserHelper.OnPlayerUserChanged = (UserHelper.PlayerUserChangedDelegate)Delegate.Remove(UserHelper.OnPlayerUserChanged, new UserHelper.PlayerUserChangedDelegate(OnPlayerUserChanged));
			}
		}

		public void OnUserControllerConnected(int playerNo, User user, bool connected)
		{
			if (player == playerNo)
			{
				Debug.Log("OnUserControllerConnected: " + playerNo + " connected? " + connected.ToString());
				this.connected = connected;
				Process();
			}
		}

		public void OnPlayerGamePadChanged(int playerNo, User user)
		{
			if (player == playerNo && user.gamePadId == GamePad.None)
			{
				Debug.Log("OnPlayerGamePadChanged: " + playerNo + " GamePad.None");
				connected = false;
				Process();
			}
		}

		public void OnPlayerUserChanged(int playerNo, User was, User now)
		{
			if (player == playerNo && now == null)
			{
				Debug.Log("OnPlayerUserChanged: " + playerNo + " logged out");
				connected = true;
				Process();
			}
		}

		private void Process()
		{
			if (showWhenConnected == null)
			{
				return;
			}
			GameObject[] array = showWhenConnected;
			foreach (GameObject gameObject in array)
			{
				if (gameObject != null)
				{
					gameObject.SetActive(invert ^ connected);
				}
			}
		}
	}
}
