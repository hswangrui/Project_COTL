using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Data.ReadWrite;
using I2.Loc;
using Lamb.UI;
using Spine.Unity;
using src.Extensions;
using src.Managers;
using src.UI;
using src.UINavigator;
using UnityEngine;

public class PhotoModeCamera : MonoBehaviour
{
	public enum PhotoReadWriteResult
	{
		None,
		Success,
		Error
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass34_0
	{
		public PhotoReadWriteResult result;

		public PhotoModeCamera _003C_003E4__this;

		public Texture2D screenshot;

		internal void _003CTakeScreenshotRoutine_003Eg__WriteError_007C0(MMReadWriteError error)
		{
			result = PhotoReadWriteResult.Error;
		}

		internal void _003CTakeScreenshotRoutine_003Eg__WriteSuccess_007C1()
		{
			result = PhotoReadWriteResult.Success;
		}

		internal void _003CTakeScreenshotRoutine_003Eb__2()
		{
			_003C_003E4__this.IsErrorShown = false;
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			UnityEngine.Object.Destroy(screenshot);
			screenshot = null;
		}
	}

	public static PhotoModeCamera Instance;

	private float minHeight = -1f;

	private const float maxHeight = -11f;

	private Quaternion restoreCameraRotationOnDisable;

	private Vector3 restoreCameraPositionOnDisable;

	[SerializeField]
	private CameraFollowTarget cameraFollowerTarget;

	[SerializeField]
	private float maxMoveSpeed;

	[SerializeField]
	private float acceleration;

	[SerializeField]
	private float deceleration;

	private UITakePhotoOverlayController takePhotoOverlay;

	private List<KeyValuePair<SkeletonAnimation, bool>> spines = new List<KeyValuePair<SkeletonAnimation, bool>>();

	private Vector3 speed;

	private HideUI hideUI;

	private RaycastHit hit;

	private bool isErrorShown;

	public Camera Camera { get; private set; }

	public bool IsErrorShown
	{
		get
		{
			return isErrorShown;
		}
		set
		{
			StartCoroutine(SetErrorShownFlagNextFrame(value));
		}
	}

	private IEnumerator SetErrorShownFlagNextFrame(bool value)
	{
		yield return new WaitForEndOfFrame();
		isErrorShown = value;
	}

	private void Awake()
	{
		Instance = this;
		Camera = GetComponent<Camera>();
	}

	private void Start()
	{
		PhotoModeManager.OnPhotoModeEnabled += OnPhotoModeEnabled;
		PhotoModeManager.OnPhotoModeDisabled += OnPhotoModeDisabled;
	}

	private void OnDestroy()
	{
		Instance = null;
		PhotoModeManager.OnPhotoModeEnabled -= OnPhotoModeEnabled;
		PhotoModeManager.OnPhotoModeDisabled -= OnPhotoModeDisabled;
	}

	private void OnPhotoModeEnabled()
	{
		restoreCameraRotationOnDisable = base.transform.rotation;
		restoreCameraPositionOnDisable = base.transform.position;
		AudioManager.Instance.PauseActiveLoops();
		cameraFollowerTarget.enabled = false;
		hideUI = base.gameObject.AddComponent<HideUI>();
		BiomeConstants.Instance.DepthOfFieldTween(0.15f, 8.7f, 26f, 1f, 200f);
		if (Interactor.CurrentInteraction != null)
		{
			Interactor.CurrentInteraction.EndIndicateHighlighted();
		}
		StartCoroutine(FrameDelay(delegate
		{
			takePhotoOverlay = MonoSingleton<UIManager>.Instance.TakePhotoOverlayTemplate.Instantiate();
			takePhotoOverlay.gameObject.SetActive(true);
			takePhotoOverlay.Show();
			UITakePhotoOverlayController uITakePhotoOverlayController = takePhotoOverlay;
			uITakePhotoOverlayController.OnHidden = (Action)Delegate.Combine(uITakePhotoOverlayController.OnHidden, (Action)delegate
			{
				UnityEngine.Object.Destroy(takePhotoOverlay.gameObject);
				takePhotoOverlay = null;
				PhotoModeManager.DisablePhotoMode();
			});
		}));
		SkeletonAnimation[] array = UnityEngine.Object.FindObjectsOfType<SkeletonAnimation>();
		foreach (SkeletonAnimation skeletonAnimation in array)
		{
			spines.Add(new KeyValuePair<SkeletonAnimation, bool>(skeletonAnimation, skeletonAnimation.UseDeltaTime));
			skeletonAnimation.UseDeltaTime = true;
		}
	}

	private void OnPhotoModeDisabled()
	{
		BiomeConstants.Instance.DepthOfFieldTween(0f, 8.7f, 26f, 1f, 200f);
		cameraFollowerTarget.enabled = true;
		base.transform.eulerAngles = new Vector3(-45f, 0f, 0f);
		hideUI.ShowUI();
		UnityEngine.Object.Destroy(hideUI);
		hideUI = null;
		if (Interactor.CurrentInteraction != null)
		{
			Interactor.CurrentInteraction.IndicateHighlighted();
		}
		foreach (KeyValuePair<SkeletonAnimation, bool> spine in spines)
		{
			spine.Key.UseDeltaTime = spine.Value;
		}
		base.transform.rotation = restoreCameraRotationOnDisable;
		base.transform.position = restoreCameraPositionOnDisable;
		AudioManager.Instance.ResumeActiveLoops();
	}

	private void OnFocusSliderChanged(float focusAxis)
	{
		focusAxis *= Time.unscaledDeltaTime * takePhotoOverlay.FocusSlider.maxValue;
		takePhotoOverlay.FocusSlider.value += focusAxis;
		BiomeConstants.Instance.DepthOfFieldTween(0f, Mathf.Lerp(0f, 8.7f, 1f - takePhotoOverlay.FocusSlider.value / 100f), 26f, 1f, 200f);
	}

	private void OnTiltSliderChanged(float tiltAxis, bool mouse)
	{
		if (!mouse)
		{
			tiltAxis *= Time.unscaledDeltaTime * takePhotoOverlay.TiltSlider.maxValue;
		}
		takePhotoOverlay.TiltSlider.value += tiltAxis;
		base.transform.eulerAngles = new Vector3(Mathf.Lerp(-70f, -20f, 1f - takePhotoOverlay.TiltSlider.value / 100f), 0f, 0f);
	}

	private void Update()
	{
		if ((PhotoModeManager.PhotoModeActive || PhotoModeManager.TakeScreenShotActive) && !isErrorShown)
		{
			Time.timeScale = 0f;
		}
		if (!PhotoModeManager.PhotoModeActive || isErrorShown || PhotoModeManager.TakeScreenShotActive || PhotoModeManager.CurrentPhotoState != PhotoModeManager.PhotoState.TakePhoto || PhotoModeManager.TakeScreenShotActive || takePhotoOverlay == null)
		{
			return;
		}
		Vector2 vector = new Vector2(InputManager.Gameplay.GetHorizontalAxis(), InputManager.Gameplay.GetVerticalAxis());
		if (Mathf.Abs(vector.x) > 0f)
		{
			speed.x = Mathf.Lerp(speed.x, vector.x * maxMoveSpeed, acceleration * Time.unscaledDeltaTime);
			takePhotoOverlay.TransformObjectX.color = StaticColors.GreenColor;
		}
		else
		{
			speed.x = Mathf.Lerp(speed.x, 0f, deceleration * Time.unscaledDeltaTime);
			takePhotoOverlay.TransformObjectX.color = StaticColors.GreyColor;
		}
		if (Physics.Raycast(base.transform.position + Vector3.back * 20f, Vector3.forward, out hit, float.PositiveInfinity))
		{
			minHeight = hit.point.z - 1f;
		}
		else
		{
			minHeight = -1f;
		}
		if (base.transform.position.z > minHeight)
		{
			base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, minHeight);
		}
		if (Mathf.Abs(vector.y) > 0f)
		{
			speed.y = Mathf.Lerp(speed.y, vector.y * maxMoveSpeed, acceleration * Time.unscaledDeltaTime);
			takePhotoOverlay.TransformObjectY.color = StaticColors.GreenColor;
		}
		else
		{
			speed.y = Mathf.Lerp(speed.y, 0f, deceleration * Time.unscaledDeltaTime);
			takePhotoOverlay.TransformObjectY.color = StaticColors.GreyColor;
		}
		if (InputManager.PhotoMode.GetCameraHeightAxis() < 0f && base.transform.position.z + speed.z * Time.unscaledDeltaTime < minHeight)
		{
			speed.z = Mathf.Lerp(speed.z, 1f * maxMoveSpeed, acceleration * Time.unscaledDeltaTime);
			takePhotoOverlay.TransformObjectZ.color = StaticColors.GreenColor;
		}
		else if ((InputManager.General.InputIsController() ? (InputManager.PhotoMode.GetCameraHeightAxis() > 0f) : InputManager.UI.GetAcceptButtonHeld()) && base.transform.position.z + speed.z * Time.unscaledDeltaTime > -11f)
		{
			speed.z = Mathf.Lerp(speed.z, -1f * maxMoveSpeed, acceleration * Time.unscaledDeltaTime);
			takePhotoOverlay.TransformObjectZ.color = StaticColors.GreenColor;
		}
		else
		{
			speed.z = Mathf.Lerp(speed.z, 0f, deceleration * Time.unscaledDeltaTime);
			takePhotoOverlay.TransformObjectZ.color = StaticColors.GreyColor;
		}
		if (InputManager.General.InputIsController())
		{
			OnTiltSliderChanged(InputManager.PhotoMode.GetCameraTiltAxis(), false);
			OnFocusSliderChanged(InputManager.PhotoMode.GetFocusAxis());
		}
		else
		{
			OnFocusSliderChanged(InputManager.PhotoMode.GetFocusAxis());
			if (InputManager.General.MouseInputActive)
			{
				if (InputManager.Gameplay.GetInteract2ButtonHeld())
				{
					OnTiltSliderChanged(InputManager.PhotoMode.GetCameraTiltAxis(), true);
				}
			}
			else
			{
				OnTiltSliderChanged(InputManager.PhotoMode.GetCameraTiltAxis(), false);
			}
		}
		if (InputManager.PhotoMode.GetTakePhotoButtonDown())
		{
			TakeScreenshot();
		}
		base.transform.position += speed * Time.unscaledDeltaTime;
	}

	private IEnumerator FrameDelay(Action callback)
	{
		yield return new WaitForEndOfFrame();
		if (callback != null)
		{
			callback();
		}
	}

	private void TakeScreenshot()
	{
		PhotoModeManager.TakeScreenShotActive = true;
		StartCoroutine(TakeScreenshotRoutine());
	}

	private IEnumerator TakeScreenshotRoutine()
	{
		_003C_003Ec__DisplayClass34_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass34_0();
		CS_0024_003C_003E8__locals0._003C_003E4__this = this;
		Debug.Log("Begin TakeScreenshotRoutine");
		RenderTexture originalCameraTargetTexture = Camera.targetTexture;
		RenderTexture originalActiveRenderTexture = RenderTexture.active;
		RenderTexture rt = RenderTexture.GetTemporary(PhotoModeManager.ResolutionX, PhotoModeManager.ResolutionY, 0);
		Camera.targetTexture = rt;
		CS_0024_003C_003E8__locals0.screenshot = new Texture2D(PhotoModeManager.ResolutionX, PhotoModeManager.ResolutionY, TextureFormat.RGB24, false);
		yield return new WaitForEndOfFrame();
		Camera.Render();
		yield return new WaitForEndOfFrame();
		RenderTexture.active = rt;
		CS_0024_003C_003E8__locals0.screenshot.ReadPixels(new Rect(0f, 0f, PhotoModeManager.ResolutionX, PhotoModeManager.ResolutionY), 0, 0);
		yield return new WaitForEndOfFrame();
		Camera.targetTexture = originalCameraTargetTexture;
		RenderTexture.active = originalActiveRenderTexture;
		RenderTexture.ReleaseTemporary(rt);
		yield return new WaitForEndOfFrame();
		CS_0024_003C_003E8__locals0.result = PhotoReadWriteResult.None;
		COTLImageReadWriter imageReadWriter = PhotoModeManager.ImageReadWriter;
		imageReadWriter.OnWriteError = (Action<MMReadWriteError>)Delegate.Combine(imageReadWriter.OnWriteError, new Action<MMReadWriteError>(CS_0024_003C_003E8__locals0._003CTakeScreenshotRoutine_003Eg__WriteError_007C0));
		COTLImageReadWriter imageReadWriter2 = PhotoModeManager.ImageReadWriter;
		imageReadWriter2.OnWriteCompleted = (Action)Delegate.Combine(imageReadWriter2.OnWriteCompleted, new Action(CS_0024_003C_003E8__locals0._003CTakeScreenshotRoutine_003Eg__WriteSuccess_007C1));
		PhotoModeManager.PhotoData photoData = GetScreenShotData();
		CS_0024_003C_003E8__locals0.screenshot.name = photoData.PhotoName;
		CS_0024_003C_003E8__locals0.screenshot.Apply();
		PhotoModeManager.ImageReadWriter.Write(CS_0024_003C_003E8__locals0.screenshot, photoData.PhotoName);
		Action<string> onPhotoSaved = PhotoModeManager.OnPhotoSaved;
		if (onPhotoSaved != null)
		{
			onPhotoSaved(photoData.PhotoName);
		}
		while (CS_0024_003C_003E8__locals0.result == PhotoReadWriteResult.None)
		{
			yield return new WaitForEndOfFrame();
		}
		COTLImageReadWriter imageReadWriter3 = PhotoModeManager.ImageReadWriter;
		imageReadWriter3.OnWriteError = (Action<MMReadWriteError>)Delegate.Remove(imageReadWriter3.OnWriteError, new Action<MMReadWriteError>(CS_0024_003C_003E8__locals0._003CTakeScreenshotRoutine_003Eg__WriteError_007C0));
		COTLImageReadWriter imageReadWriter4 = PhotoModeManager.ImageReadWriter;
		imageReadWriter4.OnWriteCompleted = (Action)Delegate.Remove(imageReadWriter4.OnWriteCompleted, new Action(CS_0024_003C_003E8__locals0._003CTakeScreenshotRoutine_003Eg__WriteSuccess_007C1));
		yield return new WaitForEndOfFrame();
		if (CS_0024_003C_003E8__locals0.result == PhotoReadWriteResult.Error)
		{
			IsErrorShown = true;
			PhotoModeManager.ImageReadWriter.Delete(photoData.PhotoName);
			UIMenuConfirmationWindow uIMenuConfirmationWindow = UIMenuBase.ActiveMenus.LastElement().Push(MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate);
			uIMenuConfirmationWindow.Configure(ScriptLocalization.UI_PhotoMode_Error_Write.Heading, ScriptLocalization.UI_PhotoMode_Error_Write.Heading, true);
			uIMenuConfirmationWindow.OnHide = (Action)Delegate.Combine(uIMenuConfirmationWindow.OnHide, (Action)delegate
			{
				CS_0024_003C_003E8__locals0._003C_003E4__this.IsErrorShown = false;
				MonoSingleton<UINavigatorNew>.Instance.Clear();
				UnityEngine.Object.Destroy(CS_0024_003C_003E8__locals0.screenshot);
				CS_0024_003C_003E8__locals0.screenshot = null;
			});
			yield return uIMenuConfirmationWindow.YieldUntilHidden();
			PhotoModeManager.TakeScreenShotActive = false;
		}
		else
		{
			PhotoModeManager.PhotoTaken(CS_0024_003C_003E8__locals0.screenshot);
		}
	}

	private PhotoModeManager.PhotoData GetScreenShotData()
	{
		string input;
		do
		{
			PersistenceManager.PersistentData.PhotoModePictureIndex++;
			input = string.Format("Photo_{0}_{1}", DataManager.Instance.CultName, PersistenceManager.PersistentData.PhotoModePictureIndex.ToString());
			string str = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
			input = new Regex(string.Format("[{0}]", Regex.Escape(str))).Replace(input, "");
		}
		while (PhotoModeManager.ImageReadWriter.FileExists(input));
		return new PhotoModeManager.PhotoData
		{
			PhotoName = input,
			PhotoTexture = null
		};
	}
}
