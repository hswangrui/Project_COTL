using System;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.SettingsMenu
{
	public class AccessibilitySettings : UISubmenuBase
	{
		[Header("Graphics")]
		[SerializeField]
		private Slider _screenShakeSlider;

		[SerializeField]
		private MMToggle _reduceCameraMotion;

		[SerializeField]
		private Slider _textScaleSlider;

		[SerializeField]
		private MMToggle _animatedTextToggle;

		[SerializeField]
		private MMToggle _flashingLightsToggle;

		[SerializeField]
		private Slider _ditherFadeDistance;

		[SerializeField]
		private MMToggle _holdActions;

		[SerializeField]
		private MMSelectable_Toggle _dyslexicToggle;

		[SerializeField]
		private MMToggle _dyslexicFontToggle;

		[SerializeField]
		private MMToggle _romanNumerals;

		[SerializeField]
		private MMToggle _highConstrastFonts;

		[SerializeField]
		private MMToggle _showBuildModeFilter;

		[SerializeField]
		private MMToggle _removeTextStyling;

		[SerializeField]
		private MMToggle _removeLightingEffects;

		[SerializeField]
		private MMToggle _darkModeToggle;

		[Header("Gameplay Modifiers")]
		[SerializeField]
		private MMToggle _autoCook;

		[SerializeField]
		private MMToggle _autoFish;

		[SerializeField]
		private MMSelectable_Toggle _stopTimeInCrusdeToggle;

		private void Start()
		{
			_textScaleSlider.minValue = 100f;
			_textScaleSlider.maxValue = 125f;
			_ditherFadeDistance.minValue = 75f;
			_ditherFadeDistance.maxValue = 175f;
			Configure(SettingsManager.Settings.Accessibility);
			_screenShakeSlider.onValueChanged.AddListener(OnScreenshakeSensitivityChanged);
			MMToggle reduceCameraMotion = _reduceCameraMotion;
			reduceCameraMotion.OnValueChanged = (Action<bool>)Delegate.Combine(reduceCameraMotion.OnValueChanged, new Action<bool>(OnReduceCameraMotionToggled));
			_textScaleSlider.onValueChanged.AddListener(OnTextScaleSliderValueChanged);
			MMToggle animatedTextToggle = _animatedTextToggle;
			animatedTextToggle.OnValueChanged = (Action<bool>)Delegate.Combine(animatedTextToggle.OnValueChanged, new Action<bool>(OnAnimatedTextValueChanged));
			MMToggle flashingLightsToggle = _flashingLightsToggle;
			flashingLightsToggle.OnValueChanged = (Action<bool>)Delegate.Combine(flashingLightsToggle.OnValueChanged, new Action<bool>(OnFlashingLightsValueChanged));
			_ditherFadeDistance.onValueChanged.AddListener(OnDitherFadeDistanceValueChanged);
			MMToggle holdActions = _holdActions;
			holdActions.OnValueChanged = (Action<bool>)Delegate.Combine(holdActions.OnValueChanged, new Action<bool>(OnHoldActionsValueChanged));
			MMToggle autoCook = _autoCook;
			autoCook.OnValueChanged = (Action<bool>)Delegate.Combine(autoCook.OnValueChanged, new Action<bool>(OnAutoCookValueChanged));
			MMToggle autoFish = _autoFish;
			autoFish.OnValueChanged = (Action<bool>)Delegate.Combine(autoFish.OnValueChanged, new Action<bool>(OnAutoFishValueChanged));
			MMToggle dyslexicFontToggle = _dyslexicFontToggle;
			dyslexicFontToggle.OnValueChanged = (Action<bool>)Delegate.Combine(dyslexicFontToggle.OnValueChanged, new Action<bool>(OnDyslexicFontValueChanged));
			MMToggle romanNumerals = _romanNumerals;
			romanNumerals.OnValueChanged = (Action<bool>)Delegate.Combine(romanNumerals.OnValueChanged, new Action<bool>(OnRomanNumeralsValueChanged));
			MMToggle toggle = _stopTimeInCrusdeToggle.Toggle;
			toggle.OnValueChanged = (Action<bool>)Delegate.Combine(toggle.OnValueChanged, new Action<bool>(OnStopTimeInCrusadeValueChanged));
			MMToggle highConstrastFonts = _highConstrastFonts;
			highConstrastFonts.OnValueChanged = (Action<bool>)Delegate.Combine(highConstrastFonts.OnValueChanged, new Action<bool>(OnHighContrastFontsValueChanged));
			MMToggle showBuildModeFilter = _showBuildModeFilter;
			showBuildModeFilter.OnValueChanged = (Action<bool>)Delegate.Combine(showBuildModeFilter.OnValueChanged, new Action<bool>(OnBuildModeFilterValueChanged));
			MMToggle removeTextStyling = _removeTextStyling;
			removeTextStyling.OnValueChanged = (Action<bool>)Delegate.Combine(removeTextStyling.OnValueChanged, new Action<bool>(OnRemoveTextStylingValueChanged));
			MMToggle removeLightingEffects = _removeLightingEffects;
			removeLightingEffects.OnValueChanged = (Action<bool>)Delegate.Combine(removeLightingEffects.OnValueChanged, new Action<bool>(OnRemoveLightingEffectsValueChanges));
			MMToggle darkModeToggle = _darkModeToggle;
			darkModeToggle.OnValueChanged = (Action<bool>)Delegate.Combine(darkModeToggle.OnValueChanged, new Action<bool>(OnDarkModeToggleValueChanged));
		}

		protected override void SetActiveStateForMenu(GameObject target, bool state)
		{
			base.SetActiveStateForMenu(target, state);
			_dyslexicToggle.Interactable = SettingsManager.Settings.Game.Language == "English";
		}

		public void Configure(SettingsData.AccessibilitySettings accessibilitySettings)
		{
			_screenShakeSlider.value = accessibilitySettings.ScreenShake * 100f;
			_reduceCameraMotion.Value = accessibilitySettings.ReduceCameraMotion;
			_textScaleSlider.value = accessibilitySettings.TextScale * 100f;
			_animatedTextToggle.Value = accessibilitySettings.AnimatedText;
			_flashingLightsToggle.Value = accessibilitySettings.FlashingLights;
			_ditherFadeDistance.value = accessibilitySettings.DitherFadeDistance * 100f;
			_holdActions.Value = accessibilitySettings.HoldActions;
			_autoCook.Value = accessibilitySettings.AutoCook;
			_autoFish.Value = accessibilitySettings.AutoFish;
			_dyslexicFontToggle.Value = accessibilitySettings.DyslexicFont;
			_romanNumerals.Value = accessibilitySettings.RomanNumerals;
			_stopTimeInCrusdeToggle.Toggle.Value = accessibilitySettings.StopTimeInCrusade;
			_highConstrastFonts.Value = accessibilitySettings.HighContrastText;
			_showBuildModeFilter.Value = accessibilitySettings.ShowBuildModeFilter;
			_removeTextStyling.Value = accessibilitySettings.RemoveTextStyling;
			_removeLightingEffects.Value = accessibilitySettings.RemoveLightingEffects;
			_darkModeToggle.Value = accessibilitySettings.DarkMode;
		}

		public void Reset()
		{
			SettingsManager.Settings.Accessibility = new SettingsData.AccessibilitySettings();
		}

		private void OnHighContrastFontsValueChanged(bool value)
		{
			SettingsManager.Settings.Accessibility.HighContrastText = value;
			Singleton<AccessibilityManager>.Instance.SetHighContrastText(value);
			Debug.Log(string.Format("AccessibilitySettings - Change HighContrastText to {0}", value));
		}

		private void OnScreenshakeSensitivityChanged(float screenshakeIntensity)
		{
			screenshakeIntensity /= 100f;
			SettingsManager.Settings.Accessibility.ScreenShake = screenshakeIntensity;
			Debug.Log(string.Format("AccessibilitySettings - Change screenshake intensity to {0}", screenshakeIntensity));
		}

		private void OnReduceCameraMotionToggled(bool value)
		{
			SettingsManager.Settings.Accessibility.ReduceCameraMotion = value;
			if (CameraFollowTarget.Instance != null)
			{
				CameraFollowTarget.Instance.CamWobbleSettings = 1 - value.ToInt();
			}
			Debug.Log(string.Format("AccessibilitySettings - Change Reduce Camera motion to {0}", value));
		}

		private void OnTextScaleSliderValueChanged(float value)
		{
			value /= 100f;
			Singleton<AccessibilityManager>.Instance.UpdateTextScale(value);
			Debug.Log(string.Format("AccessibilitySettings - Text Scale changed to {0}", value));
		}

		private void OnAnimatedTextValueChanged(bool value)
		{
			Debug.Log(string.Format("AccessibilitySettings - Animated text value changed to {0}", value).Colour(Color.yellow));
			SettingsManager.Settings.Accessibility.AnimatedText = value;
			AccessibilityManager.UpdateTextStyling();
		}

		private void OnFlashingLightsValueChanged(bool value)
		{
			Debug.Log(string.Format("AccessibilitySettings - Flashing lights value changed to {0}", value).Colour(Color.yellow));
			SettingsManager.Settings.Accessibility.FlashingLights = value;
		}

		private void OnDitherFadeDistanceValueChanged(float value)
		{
			value /= 100f;
			Debug.Log(string.Format("AccessibilitySettings - Dithering Fade value changed to {0}", value).Colour(Color.yellow));
			SettingsManager.Settings.Accessibility.DitherFadeDistance = value;
			AccessibilityManager.UpdateDitheringFadeDistance(value);
		}

		private void OnHoldActionsValueChanged(bool value)
		{
			Debug.Log(string.Format("AccessibilitySettings - Hold Actions value changed to {0}", value).Colour(Color.yellow));
			Singleton<AccessibilityManager>.Instance.UpdateHoldActionsToggle(value);
		}

		private void OnAutoCookValueChanged(bool value)
		{
			Debug.Log(string.Format("AccessibilitySettings - Auto Cook value changed to {0}", value).Colour(Color.yellow));
			Singleton<AccessibilityManager>.Instance.UpdateAutoCook(value);
		}

		private void OnAutoFishValueChanged(bool value)
		{
			Debug.Log(string.Format("AccessibilitySettings - Auto Fish value changed to {0}", value).Colour(Color.yellow));
			SettingsManager.Settings.Accessibility.AutoFish = value;
		}

		private void OnDyslexicFontValueChanged(bool value)
		{
			Debug.Log(string.Format("AccessibilitySettings - Dyslexic Font Value changed to {0}", value).Colour(Color.yellow));
			Singleton<AccessibilityManager>.Instance.UpdateDyslexicFontSetting(value);
			LocalizationManager.LocalizeAll(true);
		}

		private void OnRomanNumeralsValueChanged(bool value)
		{
			Debug.Log(string.Format("AccessibilitySettings - Roman Numerals Value changed to {0}", value).Colour(Color.yellow));
			Singleton<AccessibilityManager>.Instance.UpdateRomanNumerals(value);
		}

		private void OnWorldTimeScaleChanged(float value)
		{
			value /= 100f;
			Debug.Log(string.Format("AccessibilitySettings - World Time Scale Value changed to {0}", value).Colour(Color.yellow));
			Singleton<AccessibilityManager>.Instance.UpdateWorldTimeScale(value);
		}

		private void OnStopTimeInCrusadeValueChanged(bool value)
		{
			Debug.Log(string.Format("AccessibilitySettings - Pause Time In Crusade Value changed to {0}", value).Colour(Color.yellow));
			Singleton<AccessibilityManager>.Instance.UpdateStopTimeInCrusades(value);
		}

		private void OnBuildModeFilterValueChanged(bool value)
		{
			Debug.Log(string.Format("AccessibilitySettings - Show Build Mode filter value changed to {0}", value).Colour(Color.yellow));
			Singleton<AccessibilityManager>.Instance.UpdateBuildModeFilter(value);
		}

		private void OnRemoveTextStylingValueChanged(bool value)
		{
			Debug.Log(string.Format("AccessibilitySettings - Remove Text Styling value changed to {0}", value).Colour(Color.yellow));
			Singleton<AccessibilityManager>.Instance.UpdateRemoveTextStyling(value);
		}

		private void OnRemoveLightingEffectsValueChanges(bool value)
		{
			Debug.Log(string.Format("AccessibilitySettings - Remove Text Styling value changed to {0}", value).Colour(Color.yellow));
			Singleton<AccessibilityManager>.Instance.UpdateLightingEffectsSetting(value);
		}

		private void OnDarkModeToggleValueChanged(bool value)
		{
			SettingsManager.Settings.Accessibility.DarkMode = value;
			GraphicsSettingsUtilities.SetDarkMode();
		}
	}
}
