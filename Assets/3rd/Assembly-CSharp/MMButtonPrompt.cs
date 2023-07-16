using System;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MMButtonPrompt : MonoBehaviour
{
	[Serializable]
	private enum KeyboardOrMouse
	{
		Keyboard,
		Mouse
	}

	[Header("Components")]
	[SerializeField]
	private TextMeshProUGUI _text;

	[SerializeField]
	private TextMeshProUGUI _iconText;

	[SerializeField]
	private Image _icon;

	[Header("Settings")]
	[SerializeField]
	private Platform _platform;

	[SerializeField]
	[ActionIdProperty(typeof(GamepadTemplate))]
	private int _button;

	[SerializeField]
	private KeyboardOrMouse _keyboardOrMouse;

	[SerializeField]
	private MouseInputElement _mouseInputElement;

	[SerializeField]
	private KeyboardKeyCode _keyboardKeyCode;

	[SerializeField]
	private ControlMappings _controlMappings;

	public Platform Platform
	{
		get
		{
			return _platform;
		}
		set
		{
			if (_platform != value)
			{
				_platform = value;
				UpdatePrompt();
			}
		}
	}

	public int Button
	{
		get
		{
			return _button;
		}
		set
		{
			if (_button != value)
			{
				_button = value;
				UpdatePrompt();
			}
		}
	}

	public KeyboardKeyCode KeyboardKeyCode
	{
		get
		{
			return _keyboardKeyCode;
		}
		set
		{
			if (_keyboardKeyCode != value)
			{
				_keyboardKeyCode = value;
				UpdatePrompt();
			}
		}
	}

	private void UpdatePrompt()
	{
		if (_iconText == null || _icon == null || _text == null)
		{
			return;
		}
		if (_platform != 0)
		{
			_iconText.font = _controlMappings.GetFontForPlatform(_platform);
			if (_platform != Platform.PC)
			{
				_iconText.fontSize = 42f;
				_iconText.enableAutoSizing = false;
				_iconText.fontStyle = FontStyles.Normal;
				_icon.gameObject.SetActive(false);
				_text.gameObject.SetActive(false);
				_iconText.gameObject.SetActive(true);
				_iconText.verticalAlignment = VerticalAlignmentOptions.Geometry;
				_iconText.text = ControlMappings.GetControllerCodeFromID(_button);
				return;
			}
			bool isSpecialCharacter = false;
			string text;
			if (_keyboardOrMouse == KeyboardOrMouse.Keyboard)
			{
				text = ControlMappings.GetKeyboardCode(_keyboardKeyCode, out isSpecialCharacter);
			}
			else
			{
				text = ControlMappings.GetMouseCode(_mouseInputElement, Pole.Positive);
				isSpecialCharacter = true;
			}
			if (!isSpecialCharacter)
			{
				_text.text = text;
				_text.gameObject.SetActive(true);
				_text.fontSize = 30f;
				_text.fontSizeMin = 10f;
				_text.fontSizeMax = 30f;
				_text.enableAutoSizing = true;
				_iconText.gameObject.SetActive(false);
				_iconText.fontStyle = FontStyles.Normal;
				_icon.gameObject.SetActive(true);
			}
			else
			{
				_iconText.font = _controlMappings.GetFontForPlatform(Platform.PC);
				_iconText.text = text;
				_iconText.fontSize = 70f;
				_iconText.fontStyle = FontStyles.Bold;
				_iconText.enableAutoSizing = false;
				_iconText.gameObject.SetActive(true);
				_iconText.verticalAlignment = VerticalAlignmentOptions.Geometry;
				_text.gameObject.SetActive(false);
				_icon.gameObject.SetActive(false);
			}
		}
		else
		{
			_iconText.text = "";
			_text.text = "";
			_icon.gameObject.SetActive(false);
		}
	}
}
