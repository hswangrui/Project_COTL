using System;
using Data.ReadWrite;
using UnityEngine;

public static class PhotoModeManager
{
	public enum PhotoState
	{
		None,
		TakePhoto,
		Gallery,
		EditPhoto
	}

	public class PhotoData
	{
		public string PhotoName;

		public Texture2D PhotoTexture;
	}

	public delegate void PhotoEvent();

	public delegate void PhotoTakenEvent(Texture2D photo);

	public static bool PhotoModeActive = false;

	public static PhotoState CurrentPhotoState = PhotoState.None;

	public static Action<string> OnPhotoSaved;

	public static Action<string> OnPhotoDeleted;

	public static int ResolutionX = Screen.width;

	public static int ResolutionY = Screen.height;

	public static int ConsoleFileLimit = 14680064;

	public static int ConsolePhotoLimit = 10;

	public const string StickerPath = "Data/Sticker Data";

	public static COTLImageReadWriter ImageReadWriter = new COTLImageReadWriter();

	private static bool depthOfFieldSetting = false;

	public static bool TakeScreenShotActive = false;

	public static event PhotoEvent OnPhotoModeEnabled;

	public static event PhotoEvent OnPhotoModeDisabled;

	public static event PhotoTakenEvent OnPhotoTaken;

	public static void EnablePhotoMode()
	{
		depthOfFieldSetting = SettingsManager.Settings.Graphics.DepthOfField;
		PhotoModeActive = true;
		PhotoEvent onPhotoModeEnabled = PhotoModeManager.OnPhotoModeEnabled;
		if (onPhotoModeEnabled != null)
		{
			onPhotoModeEnabled();
		}
		Time.timeScale = 0f;
		GameManager.GetInstance().WaitForSecondsRealtime(0.1f, delegate
		{
			AudioManager.Instance.PlayOneShot("event:/ui/photo_mode/camera_focus");
			AudioManager.Instance.ToggleFilter(SoundParams.Filter, true);
			CurrentPhotoState = PhotoState.TakePhoto;
		});
		SettingsManager.Settings.Graphics.DepthOfField = true;
		GraphicsSettingsUtilities.UpdatePostProcessing();
	}

	public static void DisablePhotoMode()
	{
		AudioManager.Instance.ToggleFilter(SoundParams.Filter, false);
		PhotoModeActive = false;
		CurrentPhotoState = PhotoState.None;
		PhotoEvent onPhotoModeDisabled = PhotoModeManager.OnPhotoModeDisabled;
		if (onPhotoModeDisabled != null)
		{
			onPhotoModeDisabled();
		}
		Time.timeScale = 1f;
		SettingsManager.Settings.Graphics.DepthOfField = depthOfFieldSetting;
		GraphicsSettingsUtilities.UpdatePostProcessing();
		SettingsManager.Settings.Graphics.DepthOfField = depthOfFieldSetting;
		GraphicsSettingsUtilities.UpdatePostProcessing();
	}

	public static void PhotoTaken(Texture2D texture)
	{
		DeviceLightingManager.FlashColor(Color.white);
		AudioManager.Instance.PlayOneShot("event:/ui/photo_mode/camera_click");
		MMVibrate.Haptic(MMVibrate.HapticTypes.Success);
		PhotoTakenEvent onPhotoTaken = PhotoModeManager.OnPhotoTaken;
		if (onPhotoTaken != null)
		{
			onPhotoTaken(texture);
		}
	}

	public static void DeletePhoto(string filePath)
	{
		ImageReadWriter.Delete(filePath);
		Action<string> onPhotoDeleted = OnPhotoDeleted;
		if (onPhotoDeleted != null)
		{
			onPhotoDeleted(filePath);
		}
	}

	public static StickerData[] GetAllStickers()
	{
		return Resources.LoadAll<StickerData>("Data/Sticker Data");
	}
}
