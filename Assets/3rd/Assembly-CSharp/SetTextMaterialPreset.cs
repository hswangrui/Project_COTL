using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SetTextMaterialPreset : MonoBehaviour
{
	[SerializeField]
	private Material materialPreset;

	private TextMeshProUGUI _text;

	private void Awake()
	{
		_text = GetComponent<TextMeshProUGUI>();
	}

	private void OnEnable()
	{
		AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
		instance.OnDyslexicFontValueChanged = (Action<bool>)Delegate.Combine(instance.OnDyslexicFontValueChanged, new Action<bool>(OnDyslexicFontSettingChanged));
		if (!SettingsManager.Settings.Accessibility.DyslexicFont)
		{
			UpdateMaterial();
		}
	}

	private void UpdateMaterial()
	{
		if (materialPreset != null)
		{
			_text.fontSharedMaterial = materialPreset;
		}
	}

	private void OnDisable()
	{
		AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
		instance.OnDyslexicFontValueChanged = (Action<bool>)Delegate.Remove(instance.OnDyslexicFontValueChanged, new Action<bool>(OnDyslexicFontSettingChanged));
	}

	private void OnDyslexicFontSettingChanged(bool state)
	{
		UpdateMaterial();
	}
}
