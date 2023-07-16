using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Steamworks;
using UnityEngine;

public class ScreenUtilities
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass4_0
	{
		public Vector2Int[] availableResolutions;

		public Vector2Int resolution;
	}

	public static Vector2Int[] GetAvailableResolutions()
	{
		List<Vector2Int> list = new List<Vector2Int>();
		Resolution[] resolutions = Screen.resolutions;
		for (int i = 0; i < resolutions.Length; i++)
		{
			Resolution resolution = resolutions[i];
			Vector2Int item = new Vector2Int(resolution.width, resolution.height);
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	public static int GetDefaultResolution(Vector2Int[] availableResolutions)
	{
		if (SteamAPI.Init() && SteamUtils.IsSteamRunningOnSteamDeck())
		{
			Vector2Int item = new Vector2Int(1280, 800);
			if (availableResolutions.Contains(item))
			{
				return availableResolutions.IndexOf(item);
			}
		}
		Vector2Int[] array = new Vector2Int[4]
		{
			new Vector2Int(1920, 1080),
			new Vector2Int(1920, 1200),
			new Vector2Int(1600, 900),
			new Vector2Int(1366, 768)
		};
		for (int i = 0; i < array.Length; i++)
		{
			if (availableResolutions.Contains(array[i]))
			{
				return availableResolutions.IndexOf(array[i]);
			}
		}
		return availableResolutions.Length / 2;
	}

	public static int GetDefaultResolution()
	{
		return GetDefaultResolution(GetAvailableResolutions());
	}

	public static FullScreenMode GetFullScreenMode()
	{
		switch (SettingsManager.Settings.Graphics.FullscreenMode)
		{
		case 0:
			return FullScreenMode.Windowed;
		case 1:
			return FullScreenMode.MaximizedWindow;
		default:
			return FullScreenMode.ExclusiveFullScreen;
		}
	}

	public static void ApplyScreenSettings()
	{
		if (SettingsManager.Settings == null)
		{
			return;
		}
		_003C_003Ec__DisplayClass4_0 _003C_003Ec__DisplayClass4_ = default(_003C_003Ec__DisplayClass4_0);
		_003C_003Ec__DisplayClass4_.availableResolutions = GetAvailableResolutions();
		if (SettingsManager.Settings.Graphics.Resolution != -1)
		{
			try
			{
				_003C_003Ec__DisplayClass4_.resolution = _003C_003Ec__DisplayClass4_.availableResolutions[SettingsManager.Settings.Graphics.Resolution];
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Something went wrong when trying to read and set the resolution, so we are going to reset to the default values.");
				Debug.LogWarning(ex.Message);
				_003CApplyScreenSettings_003Eg__DefaultFallback_007C4_0(ref _003C_003Ec__DisplayClass4_);
			}
		}
		else
		{
			_003CApplyScreenSettings_003Eg__DefaultFallback_007C4_0(ref _003C_003Ec__DisplayClass4_);
		}
		Vector2Int vector2Int = new Vector2Int(Screen.width, Screen.height);
		if (vector2Int.x != _003C_003Ec__DisplayClass4_.resolution.x || vector2Int.y != _003C_003Ec__DisplayClass4_.resolution.y || Screen.fullScreenMode != GetFullScreenMode())
		{
			Screen.SetResolution(_003C_003Ec__DisplayClass4_.resolution.x, _003C_003Ec__DisplayClass4_.resolution.y, GetFullScreenMode());
		}
		ApplyVSyncSettings();
	}

	public static void ApplyVSyncSettings()
	{
		QualitySettings.vSyncCount = SettingsManager.Settings.Graphics.VSync.ToInt();
	}

	[CompilerGenerated]
	private static void _003CApplyScreenSettings_003Eg__DefaultFallback_007C4_0(ref _003C_003Ec__DisplayClass4_0 P_0)
	{
		SettingsManager.Settings.Graphics.Resolution = GetDefaultResolution(P_0.availableResolutions);
		P_0.resolution = P_0.availableResolutions[SettingsManager.Settings.Graphics.Resolution];
	}
}
