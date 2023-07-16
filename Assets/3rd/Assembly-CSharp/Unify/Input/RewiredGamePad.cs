using Rewired;

namespace Unify.Input
{
	public class RewiredGamePad : GamePad
	{
		private int playerId = -1;

		public int PlayerId
		{
			get
			{
				return playerId;
			}
		}

		public static int PlayerToJoystick(int player)
		{
			return player + 1;
		}

		public static int JoystickToPlayer(int joystick)
		{
			return joystick - 1;
		}

		public RewiredGamePad(int player)
		{
			playerId = player;
			joystickId = PlayerToJoystick(playerId);
			connected = false;
			vibrationEnabled = true;
		}

		private bool AxisCheck(InputManager.Actions action)
		{
			Player player = ReInput.players.GetPlayer(playerId);
			switch (action)
			{
			case InputManager.Actions.Up:
				return player.GetAxis(RewiredUnifyInput.Instance.VertAxis) > 0.5f;
			case InputManager.Actions.Down:
				return player.GetAxis(RewiredUnifyInput.Instance.VertAxis) < -0.5f;
			case InputManager.Actions.Right:
				return player.GetAxis(RewiredUnifyInput.Instance.HorzAxis) > 0.5f;
			case InputManager.Actions.Left:
				return player.GetAxis(RewiredUnifyInput.Instance.HorzAxis) < -0.5f;
			default:
				return false;
			}
		}

		private static string ActionToString(InputManager.Actions action)
		{
			string result = "";
			switch (action)
			{
			case InputManager.Actions.Submit:
				result = RewiredUnifyInput.Instance.SubmitAction;
				break;
			case InputManager.Actions.Cancel:
				result = RewiredUnifyInput.Instance.CancelAction;
				break;
			case InputManager.Actions.Menu:
				result = RewiredUnifyInput.Instance.QuitAction;
				break;
			}
			return result;
		}

		public override bool GetButtonDown(InputManager.Actions action)
		{
			string actionName = ActionToString(action);
			return ReInput.players.GetPlayer(playerId).GetButtonDown(actionName);
		}

		public override bool GetButtonPressed(InputManager.Actions action)
		{
			if (AxisCheck(action))
			{
				return true;
			}
			string actionName = ActionToString(action);
			return ReInput.players.GetPlayer(playerId).GetButton(actionName);
		}

		public override bool GetButtonUp(InputManager.Actions action)
		{
			string actionName = ActionToString(action);
			return ReInput.players.GetPlayer(playerId).GetButtonUp(actionName);
		}

		public override bool IsConnected()
		{
			int num = 0;
			if (playerId < 0)
			{
				return false;
			}
			if (ReInput.players.GetPlayer(playerId) != null)
			{
				num += ReInput.players.GetPlayer(playerId).controllers.Joysticks.Count;
			}
			if ((UnifyManager.platform == UnifyManager.Platform.Standalone || UnifyManager.platform == UnifyManager.Platform.None) && ReInput.players.GetPlayer(playerId).controllers.hasKeyboard)
			{
				num++;
			}
			return num > 0;
		}

		public override void Vibrate(VibrationType type, float motorLevel, float duration)
		{
			if (vibrationEnabled)
			{
				Player player = ReInput.players.GetPlayer(playerId);
				if (player != null)
				{
					player.SetVibration(0, motorLevel, duration);
				}
			}
		}

		public override string ToString()
		{
			return "GamePad.Rewired, id: " + joystickId;
		}
	}
}
