using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GraphicsSettingsUtilities
{
	public struct GraphicsPresetValues
	{
		public int EnvironmentDetail;

		public int LightingQuality;

		public bool Shadows;

		public bool Bloom;

		public bool ChromaticAberration;

		public bool Vignette;

		public bool DepthOfField;

		public bool AntiAliasing;
	}

	private const string kShadowsOnKeyword = "_SHADOWS_ON";

	public static Action OnEnvironmentSettingsChanged;

	public static Action OnDarkModeSettingsChanged;

	public static GraphicsPresetValues LowPreset = new GraphicsPresetValues
	{
		EnvironmentDetail = 0,
		LightingQuality = 0,
		Shadows = false,
		Bloom = false,
		Vignette = false,
		ChromaticAberration = false,
		DepthOfField = false,
		AntiAliasing = false
	};

	public static readonly GraphicsPresetValues MediumPreset = new GraphicsPresetValues
	{
		EnvironmentDetail = 1,
		LightingQuality = 1,
		Shadows = true,
		Bloom = false,
		Vignette = false,
		ChromaticAberration = false,
		DepthOfField = false,
		AntiAliasing = false
	};

	public static readonly GraphicsPresetValues HighPreset = new GraphicsPresetValues
	{
		EnvironmentDetail = 1,
		LightingQuality = 2,
		Shadows = true,
		Bloom = true,
		Vignette = true,
		ChromaticAberration = true,
		DepthOfField = true,
		AntiAliasing = true
	};

	public static readonly GraphicsPresetValues UltraPreset = new GraphicsPresetValues
	{
		EnvironmentDetail = 1,
		LightingQuality = 2,
		Shadows = true,
		Bloom = true,
		Vignette = true,
		ChromaticAberration = true,
		DepthOfField = true,
		AntiAliasing = true
	};

	public static void SetLightingQuality(int index)
	{
		switch (index)
		{
		case 0:
			QualitySettings.pixelLightCount = 0;
			break;
		case 1:
			QualitySettings.pixelLightCount = 2;
			break;
		case 2:
			QualitySettings.pixelLightCount = 5;
			break;
		}
	}

	public static void SetEnvironmentDetail(int index)
	{
		Action onEnvironmentSettingsChanged = OnEnvironmentSettingsChanged;
		if (onEnvironmentSettingsChanged != null)
		{
			onEnvironmentSettingsChanged();
		}
	}

	public static void SetDarkMode()
	{
		Action onDarkModeSettingsChanged = OnDarkModeSettingsChanged;
		if (onDarkModeSettingsChanged != null)
		{
			onDarkModeSettingsChanged();
		}
	}

	public static void UpdateShadows(bool value)
	{
		if (!value)
		{
			QualitySettings.shadows = ShadowQuality.Disable;
			QualitySettings.shadowResolution = ShadowResolution.Low;
			QualitySettings.shadowDistance = 0f;
			Shader.DisableKeyword("_SHADOWS_ON");
		}
		else
		{
			QualitySettings.shadows = ShadowQuality.All;
			QualitySettings.shadowResolution = ShadowResolution.Low;
			QualitySettings.shadowDistance = 50f;
			Shader.EnableKeyword("_SHADOWS_ON");
		}
	}

	public static void UpdatePostProcessing()
	{
		if (BiomeConstants.Instance != null)
		{
			BiomeConstants.Instance.updatePostProcessing();
		}
	}

	public static void SetTargetFramerate(int index)
	{
		int targetFrameRate = 0;
		switch (index)
		{
		case 0:
			targetFrameRate = 30;
			break;
		case 1:
			targetFrameRate = 60;
			break;
		case 2:
			targetFrameRate = 300;
			break;
		}
		Application.targetFrameRate = targetFrameRate;
	}

	public static PostProcessLayer.Antialiasing AntiAliasingModeFromBool(bool b)
	{
		if (!b)
		{
			return PostProcessLayer.Antialiasing.None;
		}
		return PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
	}
}
