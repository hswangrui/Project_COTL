using UnityEngine;

namespace Unify.Input
{
	public abstract class InputManager
	{
		public enum Actions
		{
			Up = 0,
			Down = 1,
			Left = 2,
			Right = 3,
			Submit = 4,
			Cancel = 5,
			Menu = 6
		}

		private static InputManager instance;

		protected static bool inputEnabled = true;

		public static InputManager Instance => instance;

		public static bool InputEnabled
		{
			get
			{
				return inputEnabled;
			}
			set
			{
				inputEnabled = value;
				Debug.Log("UnifyInput: InputEnabled: " + value);
			}
		}

		public static void Init(InputManager concreteInstance)
		{
			Logger.Log("InputManager:Init");
			instance = concreteInstance;
		}

		public virtual GamePad GetGamePad(int id)
		{
			return GamePad.None;
		}

		public virtual GamePad GetGamePadWithAction(string action)
		{
			return GamePad.None;
		}

		public virtual GamePad GetGamePadWithAnyAction()
		{
			return GamePad.None;
		}
	}
}
