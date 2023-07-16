using System;
using Lamb.UI;
using Steamworks;

namespace UI.Keyboards
{
	public class SteamBigPictureKeyboard : KeyboardBase, IKeyboardDelegate
	{
		private Callback<GamepadTextInputDismissed_t> _textInputDismissedCallback;

		public SteamBigPictureKeyboard(IKeyboardDelegate keyboardDelegate, MMInputField inputField)
		{
			_keyboardDelegate = keyboardDelegate;
			SteamUtils.ShowGamepadTextInput(EGamepadTextInputMode.k_EGamepadTextInputModeNormal, EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine, string.Empty, Convert.ToUInt32(inputField.characterLimit), inputField.text);
			_textInputDismissedCallback = Callback<GamepadTextInputDismissed_t>.Create(OnGamepadTextInputDismissed);
		}

		public void KeyboardDismissed(string result)
		{
			_keyboardDelegate.KeyboardDismissed(result);
		}

		private void OnGamepadTextInputDismissed(GamepadTextInputDismissed_t callback)
		{
			if (callback.m_bSubmitted)
			{
				string pchText;
				SteamUtils.GetEnteredGamepadTextInput(out pchText, callback.m_unSubmittedText + 1);
				KeyboardDismissed(pchText);
				if (_textInputDismissedCallback != null)
				{
					_textInputDismissedCallback.Dispose();
					_textInputDismissedCallback = null;
				}
			}
		}
	}
}
