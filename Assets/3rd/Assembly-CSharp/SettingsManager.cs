using System;
using I2.Loc;
using Steamworks;
using Unify;
using UnityEngine;
using WebSocketSharp;

public class SettingsManager : Singleton<SettingsManager>
{
	public const string kSettingsFilename = "settings.json";

	private SettingsData _settings;

	private COTLDataReadWriter<SettingsData> _readWriter = new COTLDataReadWriter<SettingsData>();

	public static SettingsData Settings
	{
		get
		{
			return Singleton<SettingsManager>.Instance._settings;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	public static void LoadSettingsManager()
	{
		if (UnifyManager.platform == UnifyManager.Platform.Standalone || UnifyManager.platform == UnifyManager.Platform.None)
		{
			Singleton<SettingsManager>.Instance.LoadAndApply();
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void LoadFonts()
	{
		LocalizationManager.SetupFonts();
	}

	public SettingsManager()
	{
		COTLDataReadWriter<SettingsData> readWriter = _readWriter;
		readWriter.OnReadCompleted = (Action<SettingsData>)Delegate.Combine(readWriter.OnReadCompleted, (Action<SettingsData>)delegate(SettingsData data)
		{
			_settings = data;
		});
		COTLDataReadWriter<SettingsData> readWriter2 = _readWriter;
		readWriter2.OnCreateDefault = (Action)Delegate.Combine(readWriter2.OnCreateDefault, (Action)delegate
		{
			MakeDefaultFile();
		});
		COTLDataReadWriter<SettingsData> readWriter3 = _readWriter;
		readWriter3.OnReadError = (Action<MMReadWriteError>)Delegate.Combine(readWriter3.OnReadError, (Action<MMReadWriteError>)delegate
		{
			MakeDefaultFile();
		});
	}

	private void MakeDefaultFile()
	{
		_settings = new SettingsData();
		_readWriter.Write(_settings, "settings.json");
	}

	public void LoadAndApply(bool forceReload = false)
	{
		if (_settings == null || forceReload)
		{
			Load();
			ApplySettings();
		}
	}

	private void Load()
	{
		_readWriter.Read("settings.json");
	}

	public void SaveSettings()
	{
		_readWriter.Write(_settings, "settings.json");
	}

	public void ApplySettings()
	{
		Debug.Log("SettingsManager - Apply Settings".Colour(Color.yellow));
		if (_settings.Game.Language.IsNullOrEmpty())
		{
			_settings.Game.Language = LanguageUtilities.GetDefaultLanguage();
		}
		if (_settings.Graphics.Resolution == -1)
		{
			_settings.Graphics.Resolution = ScreenUtilities.GetDefaultResolution();
			if (SteamAPI.Init() && SteamUtils.IsSteamRunningOnSteamDeck())
			{
				_settings.Accessibility.TextScale = 1.25f;
				_settings.Graphics.FullscreenMode = 1;
			}
		}
		LocalizationManager.CurrentLanguage = Settings.Game.Language;
		if (CameraFollowTarget.Instance != null)
		{
			CameraFollowTarget.Instance.CamWobbleSettings = 1 - Settings.Accessibility.ReduceCameraMotion.ToInt();
		}
		ScreenUtilities.ApplyScreenSettings();
		GraphicsSettingsUtilities.SetTargetFramerate(Settings.Graphics.TargetFrameRate);
		GraphicsSettingsUtilities.UpdatePostProcessing();
		GraphicsSettingsUtilities.SetLightingQuality(Settings.Graphics.LightingQuality);
		GraphicsSettingsUtilities.UpdateShadows(Settings.Graphics.Shadows);
		AudioManager.Instance.SetMasterBusVolume(Settings.Audio.MasterVolume);
		AudioManager.Instance.SetMusicBusVolume(Settings.Audio.MusicVolume);
		AudioManager.Instance.SetSFXBusVolume(Settings.Audio.SFXVolume);
		AudioManager.Instance.SetVOBusVolume(Settings.Audio.VOVolume);
		MMVibrate.SetHapticsIntensity(Settings.Game.RumbleIntensity);
		AccessibilityManager.UpdateTextStyling();
		Singleton<AccessibilityManager>.Instance.DispatchAll();
		AccessibilityManager.UpdateDitheringFadeDistance(Settings.Accessibility.DitherFadeDistance);
		Singleton<AccessibilityManager>.Instance.SetHighContrastText(Settings.Accessibility.HighContrastText);
		LocalizationManager.LocalizeAll(true);
		ControlSettingsUtilities.UpdateGamepadPrompts();
		Application.runInBackground = ScreenUtilities.GetFullScreenMode() != FullScreenMode.ExclusiveFullScreen;
	}
}
