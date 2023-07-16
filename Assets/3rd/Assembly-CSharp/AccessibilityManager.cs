using System;
using System.Collections.Generic;
using src;
using UnityEngine;

public class AccessibilityManager : Singleton<AccessibilityManager>
{
	public const float kTextScaleMin = 1f;

	public const float KTextScaleMax = 1.25f;

	private const string kDitheringFadeDistance = "_GlobalDitherIntensity";

	public const float kMinDitheringFadeDistance = 0.75f;

	public const float kMaxDitheringFadeDistance = 1.75f;

	public const float kWorldTimeScaleMin = 0.25f;

	public const float kWorldTimeScaleMax = 3f;

	public Action OnTextScaleChanged;

	public Action OnColorblindModeChanged;

	public Action<bool> OnRomanNumeralsChanged;

	public Action<bool> OnHighContrastTextChanged;

	public Action<bool> OnHoldActionToggleChanged;

	public Action<bool> OnAutoCookChanged;

	public Action<float> OnWorldTimeScaleChanged;

	public Action<bool> OnStopTimeInCrusadeChanged;

	public Action<bool> OnBuildModeFilterChanged;

	public Action<bool> OnRemoveTextStylingChanged;

	public Action<bool> OnDyslexicFontValueChanged;

	public int ColorblindMode { get; private set; }

	public List<string> AllColorblindModes
	{
		get
		{
			return new List<string> { "UI/Settings/Graphics/ColorBlindModes/none", "UI/Settings/Graphics/ColorBlindModes/Deuteranopia", "UI/Settings/Graphics/ColorBlindModes/Protanopia", "UI/Settings/Graphics/ColorBlindModes/Tritanopia", "UI/Settings/Graphics/ColorBlindModes/Gray L Red", "UI/Settings/Graphics/ColorBlindModes/Gray M Green", "UI/Settings/Graphics/ColorBlindModes/Gray S Blue", "UI/Settings/Graphics/ColorBlindModes/Doge" };
		}
	}

	public void UpdateTextScale(float scale)
	{
		SettingsManager.Settings.Accessibility.TextScale = scale;
		Action onTextScaleChanged = OnTextScaleChanged;
		if (onTextScaleChanged != null)
		{
			onTextScaleChanged();
		}
	}

	public void DispatchAll()
	{
		Action onTextScaleChanged = OnTextScaleChanged;
		if (onTextScaleChanged != null)
		{
			onTextScaleChanged();
		}
	}

	public void SetHighContrastText(bool value)
	{
		Action<bool> onHighContrastTextChanged = OnHighContrastTextChanged;
		if (onHighContrastTextChanged != null)
		{
			onHighContrastTextChanged(value);
		}
	}

	public void SetColorblindMode(int mode)
	{
		ColorblindMode = mode;
		Action onColorblindModeChanged = OnColorblindModeChanged;
		if (onColorblindModeChanged != null)
		{
			onColorblindModeChanged();
		}
	}

	public static void UpdateTextStyling()
	{
		Singleton<AccessibilityManager>.Instance.UpdateTextScale(SettingsManager.Settings.Accessibility.TextScale);
		TextAnimatorSettings.Animated = SettingsManager.Settings.Accessibility.AnimatedText;
	}

	public static void UpdateDitheringFadeDistance(float ditheringFadeDistance)
	{
		Shader.SetGlobalFloat("_GlobalDitherIntensity", ditheringFadeDistance);
	}

	public void UpdateHoldActionsToggle(bool value)
	{
		Action<bool> onHoldActionToggleChanged = OnHoldActionToggleChanged;
		if (onHoldActionToggleChanged != null)
		{
			onHoldActionToggleChanged(value);
		}
		SettingsManager.Settings.Accessibility.HoldActions = value;
	}

	public void UpdateAutoCook(bool value)
	{
		Action<bool> onAutoCookChanged = OnAutoCookChanged;
		if (onAutoCookChanged != null)
		{
			onAutoCookChanged(value);
		}
		SettingsManager.Settings.Accessibility.AutoCook = value;
	}

	public void UpdateRomanNumerals(bool value)
	{
		SettingsManager.Settings.Accessibility.RomanNumerals = value;
		Action<bool> onRomanNumeralsChanged = OnRomanNumeralsChanged;
		if (onRomanNumeralsChanged != null)
		{
			onRomanNumeralsChanged(value);
		}
	}

	public void UpdateWorldTimeScale(float value)
	{
		SettingsManager.Settings.Accessibility.WorldTimeScale = value;
		Action<float> onWorldTimeScaleChanged = OnWorldTimeScaleChanged;
		if (onWorldTimeScaleChanged != null)
		{
			onWorldTimeScaleChanged(value);
		}
	}

	public void UpdateStopTimeInCrusades(bool value)
	{
		SettingsManager.Settings.Accessibility.StopTimeInCrusade = value;
		Action<bool> onStopTimeInCrusadeChanged = OnStopTimeInCrusadeChanged;
		if (onStopTimeInCrusadeChanged != null)
		{
			onStopTimeInCrusadeChanged(value);
		}
	}

	public void UpdateBuildModeFilter(bool value)
	{
		SettingsManager.Settings.Accessibility.ShowBuildModeFilter = value;
		Action<bool> onBuildModeFilterChanged = OnBuildModeFilterChanged;
		if (onBuildModeFilterChanged != null)
		{
			onBuildModeFilterChanged(value);
		}
	}

	public void UpdateRemoveTextStyling(bool value)
	{
		SettingsManager.Settings.Accessibility.RemoveTextStyling = value;
		Action<bool> onRemoveTextStylingChanged = OnRemoveTextStylingChanged;
		if (onRemoveTextStylingChanged != null)
		{
			onRemoveTextStylingChanged(value);
		}
	}

	public void UpdateDyslexicFontSetting(bool value)
	{
		SettingsManager.Settings.Accessibility.DyslexicFont = value;
		Action<bool> onDyslexicFontValueChanged = OnDyslexicFontValueChanged;
		if (onDyslexicFontValueChanged != null)
		{
			onDyslexicFontValueChanged(value);
		}
	}

	public void UpdateLightingEffectsSetting(bool value)
	{
		SettingsManager.Settings.Accessibility.RemoveLightingEffects = value;
		if (LightingManager.Instance != null && !DeathCatRoomMarker.IsDeathCatRoom)
		{
			float transitionDurationMultiplier = LightingManager.Instance.transitionDurationMultiplier;
			LightingManager.Instance.transitionDurationMultiplier = 0f;
			LightingManager.Instance.UpdateLighting(true);
			LightingManager.Instance.transitionDurationMultiplier = transitionDurationMultiplier;
		}
	}
}
