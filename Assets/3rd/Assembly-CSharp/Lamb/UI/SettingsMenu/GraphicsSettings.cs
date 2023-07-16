using System;
using System.Collections.Generic;
using Steamworks;
using Unify;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.SettingsMenu
{
	public class GraphicsSettings : UISubmenuBase
	{
		[SerializeField]
		private MMScrollRect _scrollRect;

		[Header("General Settings")]
		[SerializeField]
		private MMHorizontalSelector _graphicsPresetSelector;

		[SerializeField]
		private MMDropdown _resolutionDropdown;

		[SerializeField]
		private MMHorizontalSelector _fullScreenModeSelector;

		[SerializeField]
		private MMSelectable_HorizontalSelector _targetFpsSelectable;

		[SerializeField]
		private MMHorizontalSelector _targetFpsSelector;

		[SerializeField]
		private MMToggle _vSyncSwitch;

		[SerializeField]
		private MMHorizontalSelector _lightingQuality;

		[SerializeField]
		private MMHorizontalSelector _environmentDetail;

		[SerializeField]
		private MMToggle _shadowsToggle;

		[SerializeField]
		private MMToggle _bloomSwitch;

		[SerializeField]
		private MMToggle _chromaticAberrationSwitch;

		[SerializeField]
		private MMToggle _vignetteSwitch;

		[SerializeField]
		private MMToggle _depthOfFieldToggle;

		[SerializeField]
		private MMToggle _antiAliasing;

		[Header("Disable on Consoles")]
		public List<GameObject> InactiveOnConsole;

		public Selectable ConsoleDefaultSelectable;

		private List<string> _lowHigh = new List<string> { "UI/Settings/Graphics/QualitySettingsOption/Low", "UI/Settings/Graphics/QualitySettingsOption/High" };

		private List<string> _lowMediumHigh = new List<string> { "UI/Settings/Graphics/QualitySettingsOption/Low", "UI/Settings/Graphics/QualitySettingsOption/Medium", "UI/Settings/Graphics/QualitySettingsOption/High" };

		private List<string> _lowMediumHighUltraCustom = new List<string> { "UI/Settings/Graphics/QualitySettingsOption/Low", "UI/Settings/Graphics/QualitySettingsOption/Medium", "UI/Settings/Graphics/QualitySettingsOption/High", "UI/Settings/Graphics/QualitySettingsOption/Ultra", "UI/Settings/Graphics/QualitySettingsOption/Custom" };

		private List<string> _framerateSettings = new List<string> { "30", "60", "<sprite name=\"icon_Infinity\">" };

		private List<string> _fullscreenModes = new List<string> { "UI/Settings/Graphics/FullScreenMode/Windowed", "UI/Settings/Graphics/FullScreenMode/BorderlessWindowed", "UI/Settings/Graphics/FullScreenMode/FullScreen" };

		public override void Awake()
		{
			base.Awake();
			UnifyManager.Platform platform = UnifyManager.platform;
			if (platform != UnifyManager.Platform.Standalone && platform != 0)
			{
				for (int i = 0; i < InactiveOnConsole.Count; i++)
				{
					InactiveOnConsole[i].SetActive(false);
				}
			}
			UIMenuBase parent = _parent;
			parent.OnHide = (Action)Delegate.Combine(parent.OnHide, new Action(OnHideStarted));
		}

		private void Start()
		{
			if (SettingsManager.Settings != null)
			{
				_graphicsPresetSelector.LocalizeContent = true;
				_graphicsPresetSelector.PrefillContent(_lowMediumHighUltraCustom);
				List<string> list = new List<string>();
				Vector2Int[] availableResolutions = ScreenUtilities.GetAvailableResolutions();
				foreach (Vector2 vector in availableResolutions)
				{
					list.Add(vector.ToResolutionString());
				}
				_resolutionDropdown.PrefillContent(list);
				_fullScreenModeSelector.LocalizeContent = true;
				_fullScreenModeSelector.PrefillContent(_fullscreenModes);
				_targetFpsSelector.PrefillContent(_framerateSettings);
				_lightingQuality.LocalizeContent = true;
				_lightingQuality.PrefillContent(_lowMediumHigh);
				_environmentDetail.LocalizeContent = true;
				_environmentDetail.PrefillContent(_lowHigh);
				Configure(SettingsManager.Settings.Graphics);
				MMHorizontalSelector graphicsPresetSelector = _graphicsPresetSelector;
				graphicsPresetSelector.OnSelectionChanged = (Action<int>)Delegate.Combine(graphicsPresetSelector.OnSelectionChanged, new Action<int>(OnPresetValueChanged));
				MMHorizontalSelector fullScreenModeSelector = _fullScreenModeSelector;
				fullScreenModeSelector.OnSelectionChanged = (Action<int>)Delegate.Combine(fullScreenModeSelector.OnSelectionChanged, new Action<int>(OnFullscreenModeSelectorValueChanged));
				MMDropdown resolutionDropdown = _resolutionDropdown;
				resolutionDropdown.OnValueChanged = (Action<int>)Delegate.Combine(resolutionDropdown.OnValueChanged, new Action<int>(OnResolutionSelectorValueChanged));
				MMHorizontalSelector targetFpsSelector = _targetFpsSelector;
				targetFpsSelector.OnSelectionChanged = (Action<int>)Delegate.Combine(targetFpsSelector.OnSelectionChanged, new Action<int>(OnTargetFramerateValueChanged));
				MMToggle vSyncSwitch = _vSyncSwitch;
				vSyncSwitch.OnValueChanged = (Action<bool>)Delegate.Combine(vSyncSwitch.OnValueChanged, new Action<bool>(OnVSyncSwitchValueChanged));
				MMHorizontalSelector lightingQuality = _lightingQuality;
				lightingQuality.OnSelectionChanged = (Action<int>)Delegate.Combine(lightingQuality.OnSelectionChanged, new Action<int>(OnLightingQualityValueChanged));
				MMHorizontalSelector environmentDetail = _environmentDetail;
				environmentDetail.OnSelectionChanged = (Action<int>)Delegate.Combine(environmentDetail.OnSelectionChanged, new Action<int>(OnEnvironmentDetailValueChanged));
				MMToggle shadowsToggle = _shadowsToggle;
				shadowsToggle.OnValueChanged = (Action<bool>)Delegate.Combine(shadowsToggle.OnValueChanged, new Action<bool>(OnShadowsToggleChanged));
				MMToggle bloomSwitch = _bloomSwitch;
				bloomSwitch.OnValueChanged = (Action<bool>)Delegate.Combine(bloomSwitch.OnValueChanged, new Action<bool>(OnBloomSwitchValueChanged));
				MMToggle chromaticAberrationSwitch = _chromaticAberrationSwitch;
				chromaticAberrationSwitch.OnValueChanged = (Action<bool>)Delegate.Combine(chromaticAberrationSwitch.OnValueChanged, new Action<bool>(OnChromaticAberrationSwitchValueChanged));
				MMToggle vignetteSwitch = _vignetteSwitch;
				vignetteSwitch.OnValueChanged = (Action<bool>)Delegate.Combine(vignetteSwitch.OnValueChanged, new Action<bool>(OnVignetteSwitchValueChanged));
				MMToggle depthOfFieldToggle = _depthOfFieldToggle;
				depthOfFieldToggle.OnValueChanged = (Action<bool>)Delegate.Combine(depthOfFieldToggle.OnValueChanged, new Action<bool>(OnDepthOfFieldToggleValueChanged));
				MMToggle antiAliasing = _antiAliasing;
				antiAliasing.OnValueChanged = (Action<bool>)Delegate.Combine(antiAliasing.OnValueChanged, new Action<bool>(OnAntiAliasingToggleValueChanged));
			}
		}

		public void Configure(SettingsData.GraphicsSettings graphicsSettings)
		{
			_graphicsPresetSelector.ContentIndex = graphicsSettings.GraphicsPreset;
			_fullScreenModeSelector.ContentIndex = graphicsSettings.FullscreenMode;
			_resolutionDropdown.ContentIndex = graphicsSettings.Resolution;
			_targetFpsSelector.ContentIndex = graphicsSettings.TargetFrameRate;
			_vSyncSwitch.Value = graphicsSettings.VSync;
			_lightingQuality.ContentIndex = graphicsSettings.LightingQuality;
			_environmentDetail.ContentIndex = graphicsSettings.EnvironmentDetail;
			_shadowsToggle.Value = graphicsSettings.Shadows;
			_bloomSwitch.Value = graphicsSettings.Bloom;
			_chromaticAberrationSwitch.Value = graphicsSettings.ChromaticAberration;
			_vignetteSwitch.Value = graphicsSettings.Vignette;
			_depthOfFieldToggle.Value = graphicsSettings.DepthOfField;
			_antiAliasing.Value = graphicsSettings.AntiAliasing;
		}

		public void Reset()
		{
			SettingsManager.Settings.Graphics = new SettingsData.GraphicsSettings
			{
				Resolution = ScreenUtilities.GetDefaultResolution()
			};
			if (SteamUtils.IsSteamRunningOnSteamDeck())
			{
				SettingsManager.Settings.Accessibility.TextScale = 1.25f;
			}
		}

		protected override void OnShowStarted()
		{
			_scrollRect.normalizedPosition = Vector2.one;
			_targetFpsSelectable.Interactable = !_vSyncSwitch.Value;
			_scrollRect.enabled = true;
		}

		protected override void OnHideStarted()
		{
			_scrollRect.enabled = false;
		}

		private void OnPresetValueChanged(int index)
		{
			SettingsManager.Settings.Graphics.GraphicsPreset = index;
			switch (index)
			{
			default:
				return;
			case 0:
				SetToPreset(GraphicsSettingsUtilities.LowPreset);
				break;
			case 1:
				SetToPreset(GraphicsSettingsUtilities.MediumPreset);
				break;
			case 2:
				SetToPreset(GraphicsSettingsUtilities.HighPreset);
				break;
			case 3:
				SetToPreset(GraphicsSettingsUtilities.UltraPreset);
				break;
			}
			Configure(SettingsManager.Settings.Graphics);
			GraphicsSettingsUtilities.SetLightingQuality(SettingsManager.Settings.Graphics.LightingQuality);
			GraphicsSettingsUtilities.SetEnvironmentDetail(SettingsManager.Settings.Graphics.EnvironmentDetail);
			GraphicsSettingsUtilities.UpdateShadows(SettingsManager.Settings.Graphics.Shadows);
			GraphicsSettingsUtilities.UpdatePostProcessing();
		}

		private void SetToPreset(GraphicsSettingsUtilities.GraphicsPresetValues preset)
		{
			SettingsManager.Settings.Graphics.LightingQuality = preset.LightingQuality;
			SettingsManager.Settings.Graphics.EnvironmentDetail = preset.EnvironmentDetail;
			SettingsManager.Settings.Graphics.Shadows = preset.Shadows;
			SettingsManager.Settings.Graphics.Bloom = preset.Bloom;
			SettingsManager.Settings.Graphics.Vignette = preset.Vignette;
			SettingsManager.Settings.Graphics.ChromaticAberration = preset.ChromaticAberration;
			SettingsManager.Settings.Graphics.DepthOfField = preset.DepthOfField;
			SettingsManager.Settings.Graphics.AntiAliasing = preset.AntiAliasing;
		}

		private void SetToCustomPreset()
		{
			_graphicsPresetSelector.ContentIndex = (SettingsManager.Settings.Graphics.GraphicsPreset = 4);
		}

		private void OnFullscreenModeSelectorValueChanged(int value)
		{
			SettingsManager.Settings.Graphics.FullscreenMode = value;
			Screen.fullScreenMode = ScreenUtilities.GetFullScreenMode();
			Debug.Log(string.Format("GraphicsSettings - FullScreen Mode value changed to {0}", Screen.fullScreenMode).Colour(Color.yellow));
		}

		private void OnResolutionSelectorValueChanged(int index)
		{
			SettingsManager.Settings.Graphics.Resolution = index;
			ScreenUtilities.ApplyScreenSettings();
			Debug.Log(string.Format("GraphicsSettings - Resolution value changed to {0}", index).Colour(Color.yellow));
		}

		private void OnVSyncSwitchValueChanged(bool value)
		{
			SettingsManager.Settings.Graphics.VSync = value;
			ScreenUtilities.ApplyVSyncSettings();
			_targetFpsSelectable.Interactable = !_vSyncSwitch.Value;
			Debug.Log(string.Format("GraphicsSettings - VSync value changed to {0}", value).Colour(Color.yellow));
		}

		private void OnTargetFramerateValueChanged(int index)
		{
			SettingsManager.Settings.Graphics.TargetFrameRate = index;
			GraphicsSettingsUtilities.SetTargetFramerate(index);
			Debug.Log(string.Format("GraphicsSettings - Target FPS value changed to {0}", index).Colour(Color.yellow));
		}

		private void OnLightingQualityValueChanged(int index)
		{
			Debug.Log(string.Format("GraphicsSettings - Lighting Quality changed to {0}", index).Colour(Color.yellow));
			SetToCustomPreset();
			SettingsManager.Settings.Graphics.LightingQuality = index;
			GraphicsSettingsUtilities.SetLightingQuality(index);
		}

		private void OnEnvironmentDetailValueChanged(int index)
		{
			Debug.Log(string.Format("GraphicsSettings - Environment Detail changed to {0}", index).Colour(Color.yellow));
			SetToCustomPreset();
			SettingsManager.Settings.Graphics.EnvironmentDetail = index;
			GraphicsSettingsUtilities.SetEnvironmentDetail(index);
		}

		private void OnShadowsToggleChanged(bool value)
		{
			Debug.Log(string.Format("GraphicsSettings - Shadow toggle value changed to {0}", value).Colour(Color.yellow));
			SetToCustomPreset();
			SettingsManager.Settings.Graphics.Shadows = value;
			GraphicsSettingsUtilities.UpdateShadows(value);
		}

		private void OnBloomSwitchValueChanged(bool value)
		{
			Debug.Log(string.Format("GraphicsSettings - Bloom value changed to {0}", value).Colour(Color.yellow));
			SetToCustomPreset();
			SettingsManager.Settings.Graphics.Bloom = value;
			GraphicsSettingsUtilities.UpdatePostProcessing();
		}

		private void OnChromaticAberrationSwitchValueChanged(bool value)
		{
			Debug.Log(string.Format("GraphicsSettings - Chromatic Aberration value changed to {0}", value).Colour(Color.yellow));
			SetToCustomPreset();
			SettingsManager.Settings.Graphics.ChromaticAberration = value;
			GraphicsSettingsUtilities.UpdatePostProcessing();
		}

		private void OnVignetteSwitchValueChanged(bool value)
		{
			Debug.Log(string.Format("GraphicsSettings - Vignette value changed to {0}", value).Colour(Color.yellow));
			SetToCustomPreset();
			SettingsManager.Settings.Graphics.Vignette = value;
			GraphicsSettingsUtilities.UpdatePostProcessing();
		}

		private void OnDepthOfFieldToggleValueChanged(bool value)
		{
			Debug.Log(string.Format("GraphicsSettings - Depth of Field value changed to {0}", value).Colour(Color.yellow));
			SetToCustomPreset();
			SettingsManager.Settings.Graphics.DepthOfField = value;
			GraphicsSettingsUtilities.UpdatePostProcessing();
		}

		private void OnAntiAliasingToggleValueChanged(bool value)
		{
			Debug.Log(string.Format("GraphicsSettings - Depth of Field value changed to {0}", value).Colour(Color.yellow));
			SetToCustomPreset();
			SettingsManager.Settings.Graphics.AntiAliasing = value;
			GraphicsSettingsUtilities.UpdatePostProcessing();
		}
	}
}
