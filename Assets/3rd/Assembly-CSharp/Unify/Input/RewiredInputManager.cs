using Rewired;

namespace Unify.Input
{
	public class RewiredInputManager : InputManager
	{
		public static Player MainPlayer
		{
			get
			{
				if (InputManager.inputEnabled)
				{
					GamePad playerGamePad = UserHelper.GetPlayerGamePad(0);
					int num = ((playerGamePad == GamePad.None) ? (-1) : ((RewiredGamePad)playerGamePad).PlayerId);
					if (num < 0)
					{
						return null;
					}
					return ReInput.players.GetPlayer(num);
				}
				return null;
			}
		}

		public static Player MainPlayerRaw
		{
			get
			{
				GamePad playerGamePad = UserHelper.GetPlayerGamePad(0);
				int num = ((playerGamePad == GamePad.None) ? (-1) : ((RewiredGamePad)playerGamePad).PlayerId);
				if (num < 0)
				{
					return null;
				}
				return ReInput.players.GetPlayer(num);
			}
		}

		public override GamePad GetGamePad(int joystickId)
		{
			return new RewiredGamePad(RewiredGamePad.JoystickToPlayer(joystickId));
		}

		public override GamePad GetGamePadWithAction(string action)
		{
			if (ReInput.isReady)
			{
				foreach (Player player in ReInput.players.GetPlayers())
				{
					if (player.GetButtonDown(action))
					{
						return new RewiredGamePad(player.id);
					}
				}
			}
			return GamePad.None;
		}

		public override GamePad GetGamePadWithAnyAction()
		{
			if (ReInput.isReady)
			{
				foreach (Player player in ReInput.players.GetPlayers())
				{
					if (player.GetAnyButtonDown())
					{
						return new RewiredGamePad(player.id);
					}
				}
			}
			return GamePad.None;
		}
	}
}
