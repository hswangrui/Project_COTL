using System;
using System.Collections.Generic;
using Unify;

[Serializable]
public class SettingsData
{
	[Serializable]
	public class GameSettings
	{
		public string Language = string.Empty;

		public float RumbleIntensity = 1f;

		public bool ShowTutorials = true;

		public int GamepadPrompts;

		public bool ShowFollowerNames = true;

		public GameSettings()
		{
		}

		public GameSettings(GameSettings gameSettings)
		{
			Language = gameSettings.Language;
			RumbleIntensity = gameSettings.RumbleIntensity;
			ShowTutorials = gameSettings.ShowTutorials;
			GamepadPrompts = gameSettings.GamepadPrompts;
			ShowFollowerNames = gameSettings.ShowFollowerNames;
		}
	}

	[Serializable]
	public class GraphicsSettings
	{
		public int GraphicsPreset = 3;

		public int FullscreenMode = 2;

		public int Resolution = -1;

		public int TargetFrameRate = 1;

		public bool VSync;

		public int LightingQuality = GraphicsSettingsUtilities.UltraPreset.LightingQuality;

		public int EnvironmentDetail = GraphicsSettingsUtilities.UltraPreset.EnvironmentDetail;

		public bool Shadows = GraphicsSettingsUtilities.UltraPreset.Shadows;

		public bool Bloom = GraphicsSettingsUtilities.UltraPreset.Bloom;

		public bool ChromaticAberration = GraphicsSettingsUtilities.UltraPreset.ChromaticAberration;

		public bool Vignette = GraphicsSettingsUtilities.UltraPreset.Vignette;

		public bool DepthOfField = GraphicsSettingsUtilities.UltraPreset.DepthOfField;

		public bool AntiAliasing = GraphicsSettingsUtilities.UltraPreset.AntiAliasing;

		public GraphicsSettings()
		{
			if (UnifyManager.platform == UnifyManager.Platform.Switch)
			{
				GraphicsPreset = 0;
				FullscreenMode = 2;
				TargetFrameRate = 1;
				LightingQuality = GraphicsSettingsUtilities.LowPreset.LightingQuality;
				EnvironmentDetail = GraphicsSettingsUtilities.LowPreset.EnvironmentDetail;
				Shadows = GraphicsSettingsUtilities.LowPreset.Shadows;
				VSync = true;
				Bloom = GraphicsSettingsUtilities.LowPreset.Bloom;
				Vignette = GraphicsSettingsUtilities.LowPreset.Vignette;
				ChromaticAberration = GraphicsSettingsUtilities.LowPreset.ChromaticAberration;
				DepthOfField = GraphicsSettingsUtilities.LowPreset.DepthOfField;
				AntiAliasing = GraphicsSettingsUtilities.LowPreset.AntiAliasing;
			}
		}

		public GraphicsSettings(GraphicsSettings graphicsSettings)
		{
			GraphicsPreset = graphicsSettings.GraphicsPreset;
			FullscreenMode = graphicsSettings.FullscreenMode;
			Resolution = graphicsSettings.Resolution;
			TargetFrameRate = graphicsSettings.TargetFrameRate;
			VSync = graphicsSettings.VSync;
			LightingQuality = graphicsSettings.LightingQuality;
			EnvironmentDetail = graphicsSettings.EnvironmentDetail;
			Shadows = graphicsSettings.Shadows;
			Bloom = graphicsSettings.Bloom;
			ChromaticAberration = graphicsSettings.ChromaticAberration;
			Vignette = graphicsSettings.Vignette;
			DepthOfField = graphicsSettings.DepthOfField;
		}
	}

	[Serializable]
	public class AccessibilitySettings
	{
		public bool DyslexicFont;

		public float TextScale = 1f;

		public bool AnimatedText = true;

		public bool FlashingLights = true;

		public float ScreenShake = 1f;

		public bool ReduceCameraMotion;

		public float DitherFadeDistance = 1f;

		public bool HoldActions = true;

		public bool AutoCook;

		public bool AutoFish;

		public bool RomanNumerals = true;

		public float WorldTimeScale = 1f;

		public bool StopTimeInCrusade;

		public bool HighContrastText;

		public bool ShowBuildModeFilter = true;

		public bool RemoveTextStyling;

		public bool RemoveLightingEffects;

		public bool DarkMode;

		public AccessibilitySettings()
		{
		}

		public AccessibilitySettings(AccessibilitySettings accessibilitySettings)
		{
			DyslexicFont = accessibilitySettings.DyslexicFont;
			TextScale = accessibilitySettings.TextScale;
			AnimatedText = accessibilitySettings.AnimatedText;
			FlashingLights = accessibilitySettings.FlashingLights;
			ScreenShake = accessibilitySettings.ScreenShake;
			ReduceCameraMotion = accessibilitySettings.ReduceCameraMotion;
			DitherFadeDistance = accessibilitySettings.DitherFadeDistance;
			RomanNumerals = accessibilitySettings.RomanNumerals;
			WorldTimeScale = accessibilitySettings.WorldTimeScale;
			StopTimeInCrusade = accessibilitySettings.StopTimeInCrusade;
			HighContrastText = accessibilitySettings.HighContrastText;
			ShowBuildModeFilter = accessibilitySettings.ShowBuildModeFilter;
			RemoveTextStyling = accessibilitySettings.RemoveTextStyling;
			RemoveLightingEffects = accessibilitySettings.RemoveLightingEffects;
		}
	}

	[Serializable]
	public class AudioSettings
	{
		public float MasterVolume = 1f;

		public float MusicVolume = 0.75f;

		public float SFXVolume = 0.75f;

		public float VOVolume = 0.75f;

		public AudioSettings()
		{
		}

		public AudioSettings(AudioSettings audioSettings)
		{
			MasterVolume = audioSettings.MasterVolume;
			MusicVolume = audioSettings.MusicVolume;
			SFXVolume = audioSettings.SFXVolume;
			VOVolume = audioSettings.VOVolume;
		}
	}

	[Serializable]
	public class ControlSettings
	{
		public List<Binding> KeyboardBindings = new List<Binding>();

		public List<UnboundBinding> KeyboardBindingsUnbound = new List<UnboundBinding>();

		public List<Binding> MouseBindings = new List<Binding>();

		public List<UnboundBinding> MouseBindingsUnbound = new List<UnboundBinding>();

		public int GamepadLayout;

		public List<Binding> GamepadBindings = new List<Binding>();

		public List<UnboundBinding> GamepadBindingsUnbound = new List<UnboundBinding>();

		public ControlSettings()
		{
		}

		public ControlSettings(ControlSettings controlSettings)
		{
			KeyboardBindings = new List<Binding>(controlSettings.KeyboardBindings);
			KeyboardBindingsUnbound = new List<UnboundBinding>(controlSettings.KeyboardBindingsUnbound);
			MouseBindings = new List<Binding>(controlSettings.MouseBindings);
			MouseBindingsUnbound = new List<UnboundBinding>(controlSettings.MouseBindingsUnbound);
			GamepadBindings = new List<Binding>(controlSettings.GamepadBindings);
			GamepadBindingsUnbound = new List<UnboundBinding>(controlSettings.GamepadBindingsUnbound);
			GamepadLayout = controlSettings.GamepadLayout;
		}
	}

	public GameSettings Game = new GameSettings();

	public GraphicsSettings Graphics = new GraphicsSettings();

	public AccessibilitySettings Accessibility = new AccessibilitySettings();

	public AudioSettings Audio = new AudioSettings();

	public ControlSettings Control = new ControlSettings();
}
