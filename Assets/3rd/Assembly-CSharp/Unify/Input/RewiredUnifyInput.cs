using System;
using Rewired;
using Rewired.Integration.UnityUI;
using UnityEngine;

namespace Unify.Input
{
	internal class RewiredUnifyInput : MonoBehaviour
	{
		[Tooltip("Reference to the RewiredUIInputModule used by the game for menus etc. so only active users controllers are allowed.  DO NOT set this to the instance on an Unify owner EventSystems as they must remain enabled at all times.")]
		public RewiredStandaloneInputModule RewiredUIInputModule;

		public bool AllowDisableUIInput;

		public string VertAxis;

		public string HorzAxis;

		public string SubmitAction;

		public string CancelAction;

		public string QuitAction;

		private RewiredInputManager rewiredInputManager;

		private int currentPlayerId = -2;

		private int[] currentPlayerIds = new int[1] { -1 };

		private int[] noPlayerIds = new int[0];

		private bool enableInputOnAllButtonsUp;

		public static RewiredUnifyInput Instance { get; private set; }

		public void Awake()
		{
			Debug.Log("RewiredUnifyInput: Awake");
			Instance = this;
			rewiredInputManager = new RewiredInputManager();
			InputManager.Init(rewiredInputManager);
		}

		public void Start()
		{
			SessionManager.OnSessionEnd = (SessionManager.SessionEventDelegate)Delegate.Combine(SessionManager.OnSessionEnd, new SessionManager.SessionEventDelegate(OnSessionEnd));
			if (RewiredUIInputModule != null)
			{
				RewiredUIInputModule.UseAllRewiredGamePlayers = false;
			}
		}

		public void OnDestroy()
		{
			SessionManager.OnSessionEnd = (SessionManager.SessionEventDelegate)Delegate.Remove(SessionManager.OnSessionEnd, new SessionManager.SessionEventDelegate(OnSessionEnd));
		}

		public void OnSessionEnd(Guid sessionGuid, User sessionUser)
		{
			if (RewiredUIInputModule != null)
			{
				currentPlayerId = -1;
				RewiredUIInputModule.UseAllRewiredGamePlayers = false;
				RewiredUIInputModule.RewiredPlayerIds = noPlayerIds;
			}
		}

		public void ResetCurrentPlayerID()
		{
			currentPlayerId = -2;
		}

		public void Update()
		{
			if (RewiredUIInputModule != null)
			{
				if (!AllowDisableUIInput || InputManager.InputEnabled)
				{
					RewiredUIInputModule.UseAllRewiredGamePlayers = true;
					RewiredUIInputModule.RewiredPlayerIds = null;
				}
				else
				{
					RewiredUIInputModule.UseAllRewiredGamePlayers = false;
					RewiredUIInputModule.RewiredPlayerIds = noPlayerIds;
				}
			}
			if (!enableInputOnAllButtonsUp)
			{
				return;
			}
			bool flag = false;
			foreach (Player player in ReInput.players.Players)
			{
				if (player.GetAnyButtonDown())
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				enableInputOnAllButtonsUp = false;
				InputManager.InputEnabled = true;
			}
		}

		public void DisableInput()
		{
			InputManager.InputEnabled = false;
		}

		public void EnableInput()
		{
			enableInputOnAllButtonsUp = true;
		}
	}
}
