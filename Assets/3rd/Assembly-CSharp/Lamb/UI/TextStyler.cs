using System;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class TextStyler : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _text;

		private FontStyles _fontStyles;

		private void Awake()
		{
			if(_text)
				_fontStyles = _text.fontStyle;
			else
			{
				Debug.LogWarning("_fontStyles null!");
			}
			if (SettingsManager.Settings != null)
			{
				UpdateTextStyling(SettingsManager.Settings.Accessibility.RemoveTextStyling);
			}
		}

		private void OnEnable()
		{
			if (SettingsManager.Settings != null)
			{
				UpdateTextStyling(SettingsManager.Settings.Accessibility.RemoveTextStyling);
			}
			AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
			instance.OnRemoveTextStylingChanged = (Action<bool>)Delegate.Combine(instance.OnRemoveTextStylingChanged, new Action<bool>(OnRemoveTextStylingValueChanged));
		}

		private void OnDisable()
		{
			AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
			instance.OnRemoveTextStylingChanged = (Action<bool>)Delegate.Remove(instance.OnRemoveTextStylingChanged, new Action<bool>(OnRemoveTextStylingValueChanged));
		}

		private void OnRemoveTextStylingValueChanged(bool value)
		{
			UpdateTextStyling(value);
		}

		private void UpdateTextStyling(bool value)
		{
			if (!value)
			{
				_text.fontStyle = _fontStyles;
			}
			else
			{
				_text.fontStyle = FontStyles.Normal;
			}
			_text.fontMaterial.SetInt("unity_GUIZTestMode", 8);
		}
	}
}
