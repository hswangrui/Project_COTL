using Lamb.UI;
using Steamworks;
using UnityEngine;

namespace UI.Keyboards
{
	public class SteamDeckKeyboard : KeyboardBase, IKeyboardDelegate
	{
		private MMInputField _inputField;

		private Callback<FloatingGamepadTextInputDismissed_t> _floatingTextInputDismissedCallback;

		public SteamDeckKeyboard(IKeyboardDelegate keyboardDelegate, MMInputField inputField)
		{
			_inputField = inputField;
			_keyboardDelegate = keyboardDelegate;
			_inputField.ActivateInputField();
			SteamUtils.ShowFloatingGamepadTextInput(EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeSingleLine, 0, 0, Screen.width, Screen.height / 2);
			_floatingTextInputDismissedCallback = Callback<FloatingGamepadTextInputDismissed_t>.Create(OnFloatingTextInputDismissed);
		}

		public void KeyboardDismissed(string result)
		{
			_keyboardDelegate.KeyboardDismissed(result);
		}

		private void OnFloatingTextInputDismissed(FloatingGamepadTextInputDismissed_t callback)
		{
			_inputField.DeactivateInputField();
			if (_floatingTextInputDismissedCallback != null)
			{
				_floatingTextInputDismissedCallback.Dispose();
				_floatingTextInputDismissedCallback = null;
			}
		}
	}
}
