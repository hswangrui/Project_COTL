using Lamb.UI;
using Steamworks;
using UnityEngine;

namespace UI.Keyboards
{
	public class MMOnScreenKeyboard : MonoBehaviour
	{
		private KeyboardBase _keyboard;

		public void Show(IKeyboardDelegate keyboardDelegate, MMInputField inputField)
		{
			if (SteamUtils.IsSteamRunningOnSteamDeck())
			{
				_keyboard = new SteamBigPictureKeyboard(keyboardDelegate, inputField);
			}
			else if (SteamUtils.IsSteamInBigPictureMode())
			{
				_keyboard = new SteamBigPictureKeyboard(keyboardDelegate, inputField);
			}
		}

		public static bool RequiresOnScreenKeyboard()
		{
			if (!SteamUtils.IsSteamRunningOnSteamDeck())
			{
				return SteamUtils.IsSteamInBigPictureMode();
			}
			return true;
		}

		public void Update()
		{
			if (_keyboard != null)
			{
				_keyboard.Update();
			}
		}

		private void OnDestroy()
		{
			_keyboard = null;
		}
	}
}
