using I2.Loc;
using Rewired;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class BindingPrompt : MonoBehaviour
	{
		[Header("UI")]
		[SerializeField]
		private MMButtonPrompt _buttonPrompt;

		[SerializeField]
		private TextMeshProUGUI _text;

		[Header("Binding")]
		[SerializeField]
		private Platform _platform;

		[SerializeField]
		[ActionIdProperty(typeof(GamepadTemplate))]
		private int _button;

		[SerializeField]
		private KeyboardKeyCode _keyboardKeyCode;

		[SerializeField]
		private bool _predefinedAction;

		[SerializeField]
		[TermsPopup("")]
		private string[] _terms;

		public Platform Platform
		{
			set
			{
				if (_platform != value)
				{
					_platform = value;
					_buttonPrompt.Platform = _platform;
				}
			}
		}

		public int Button
		{
			get
			{
				return _button;
			}
		}

		public KeyboardKeyCode KeyboardKeyCode
		{
			get
			{
				return _keyboardKeyCode;
			}
		}

		private void OnValidate()
		{
			if (!(_buttonPrompt == null) && !(_text == null))
			{
				if (_buttonPrompt.Platform != _platform)
				{
					_buttonPrompt.Platform = _platform;
				}
				if (_buttonPrompt.KeyboardKeyCode != _keyboardKeyCode)
				{
					_buttonPrompt.KeyboardKeyCode = _keyboardKeyCode;
				}
				if (_buttonPrompt.Button != _button)
				{
					_buttonPrompt.Button = _button;
				}
			}
		}

		public void Clear()
		{
			_text.text = string.Empty;
		}

		public void TryAddAction(ControllerTemplateElementTarget elementTarget, ActionElementMap actionElementMap)
		{
			if (!_predefinedAction && elementTarget.element.id == _button)
			{
				AddAction(ControlMappings.LocForAction(actionElementMap.actionId));
			}
		}

		private void AddAction(string loc)
		{
			if (!string.IsNullOrWhiteSpace(loc))
			{
				if (string.IsNullOrEmpty(_text.text))
				{
					_text.text = loc;
				}
				else
				{
					_text.text = _text.text + " / " + loc;
				}
			}
		}

		public void FinalizeBinding()
		{
			if (_predefinedAction)
			{
				string[] terms = _terms;
				foreach (string term in terms)
				{
					AddAction(LocalizationManager.GetTranslation(term));
				}
			}
			else if (string.IsNullOrEmpty(_text.text))
			{
				_text.text = "--";
			}
		}
	}
}
