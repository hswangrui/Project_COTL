namespace Unify.Input
{
	public abstract class GamePad
	{
		public enum VibrationType
		{
			General = 0,
			Left = 1,
			Right = 2,
			Trigger = 3,
			LeftTrigger = 4,
			RightTrigger = 5,
			All = 6
		}

		public static GamePad None = new GamePadNone();

		public int joystickId;

		public bool connected;

		protected bool vibrationEnabled;

		public virtual bool IsConnected()
		{
			return false;
		}

		public virtual bool GetButtonDown(InputManager.Actions action)
		{
			return false;
		}

		public virtual bool GetButtonPressed(InputManager.Actions action)
		{
			return false;
		}

		public virtual bool GetButtonUp(InputManager.Actions action)
		{
			return false;
		}

		public virtual void EnableVibration(bool enabled)
		{
		}

		public virtual void Vibrate(VibrationType type, float motorLevel, float duration)
		{
		}

		public virtual int GetJoystickId()
		{
			return joystickId;
		}

		public static bool operator ==(GamePad a, GamePad b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(GamePad a, GamePad b)
		{
			return !a.Equals(b);
		}

		public virtual bool Equals(GamePad b)
		{
			return joystickId == b.joystickId;
		}

		public override string ToString()
		{
			if (this == None)
			{
				return "GamePad.None";
			}
			return "";
		}
	}
}
