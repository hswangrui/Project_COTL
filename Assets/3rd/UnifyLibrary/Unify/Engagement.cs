using System;
using Unify.Automation;
using Unify.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace Unify
{
	public class Engagement : MonoBehaviour
	{
		public string engagementAction;

		public GameObject[] hideWhenEngaged;

		public GameObject[] showWhenEngaged;

		public UnityEvent onEngage;

		public int maxActivePlayers = 1;

		public int state;

		private const float DevicePairTimeout = 15f;

		private float currentDevicePairTimeout;

		public static bool GlobalAllowEngagement { get; set; } = true;


		public void Awake()
		{
			Logger.Log("UNIFY: Engagaement, Awake");
		}

		public void Start()
		{
			UserHelper.DisengageAllPlayers();
			SessionManager.OnSessionEnd = (SessionManager.SessionEventDelegate)Delegate.Combine(SessionManager.OnSessionEnd, (SessionManager.SessionEventDelegate)delegate
			{
				Logger.Log("UNIFY:Engagement: session end so reset state machine.");
				ResetEngagementState();
			});
			UnifyManager.Instance.AutomationClient.Connect();
		}

		public void OnDestroy()
		{
		}

		public void OnEnable()
		{
			ResetEngagementState();
		}

		private void EngageUser(User user)
		{
			Logger.Log("ENGAGEMENTREWIRED: Invoke engaged event.");
			if (onEngage != null)
			{
				onEngage.Invoke();
			}
			GameObject[] array;
			if (showWhenEngaged != null)
			{
				array = showWhenEngaged;
				foreach (GameObject gameObject in array)
				{
					if (gameObject != null)
					{
						gameObject.SetActive(value: true);
					}
				}
			}
			if (hideWhenEngaged == null)
			{
				return;
			}
			array = hideWhenEngaged;
			foreach (GameObject gameObject2 in array)
			{
				if (gameObject2 != null)
				{
					gameObject2.SetActive(value: false);
				}
			}
		}

		public void Update()
		{
			GamePad gamePad = null;
			if (!GlobalAllowEngagement || !SplashScreen.isFinished || ControllerReconnect.IsVisible || UnifyManager.isSuspending || UnifyManager.isPaused)
			{
				return;
			}
			gamePad = (string.IsNullOrEmpty(engagementAction) ? InputManager.Instance.GetGamePadWithAnyAction() : InputManager.Instance.GetGamePadWithAction(engagementAction));
			switch (state)
			{
			case 0:
				if (gamePad == GamePad.None)
				{
					state = 1;
					UnifyManager.Instance.AutomationClient.SendEvent(Client.Event.ENGAGE_READY);
				}
				break;
			case 1:
			{
				int nextOpenPlayerSlot = UserHelper.GetNextOpenPlayerSlot();
				if (nextOpenPlayerSlot >= 0 && nextOpenPlayerSlot < maxActivePlayers && gamePad != GamePad.None)
				{
					Logger.Log("ENGAGEMENTREWIRED: Button pressed on pad: " + gamePad);
					switch (UserHelper.EngagePlayer(nextOpenPlayerSlot, gamePad))
					{
					case UserHelper.EngagementResult.Engaged:
					{
						User player = UserHelper.GetPlayer(nextOpenPlayerSlot);
						EngageUser(player);
						ResetEngagementState();
						break;
					}
					case UserHelper.EngagementResult.RequiresSignin:
						Logger.Log("ENGAGEMENT: Engagement failed, no user found.");
						ResetEngagementState();
						break;
					default:
						ResetEngagementState();
						break;
					}
				}
				break;
			}
			}
		}

		private void ResetEngagementState()
		{
			state = 0;
		}
	}
}
