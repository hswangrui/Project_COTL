using System;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(TextMeshPro))]
	public class MMTextMeshScaler : MonoBehaviour
	{
		private TextMeshPro _text;

		private float _originalFontSize;

		private float _originalFontSizeMin;

		private float _originalFontSizeMax;

		public void OnEnable()
		{
			AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
			instance.OnTextScaleChanged = (Action)Delegate.Combine(instance.OnTextScaleChanged, new Action(OnTextScaleChanged));
			if (_text == null)
			{
				_text = GetComponent<TextMeshPro>();
				_originalFontSize = _text.fontSize;
				_originalFontSizeMin = _text.fontSizeMin;
				_originalFontSizeMax = _text.fontSizeMax;
			}
			if (SettingsManager.Settings != null && _text != null)
			{
				OnTextScaleChanged();
			}
		}

		public void OnDisable()
		{
			if (Singleton<AccessibilityManager>.Instance != null)
			{
				AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
				instance.OnTextScaleChanged = (Action)Delegate.Remove(instance.OnTextScaleChanged, new Action(OnTextScaleChanged));
			}
		}

		private void OnTextScaleChanged()
		{
			_text.fontSize = _originalFontSize * SettingsManager.Settings.Accessibility.TextScale;
			_text.fontSizeMin = _originalFontSizeMin * SettingsManager.Settings.Accessibility.TextScale;
			_text.fontSizeMax = _originalFontSizeMax * SettingsManager.Settings.Accessibility.TextScale;
		}
	}
}
