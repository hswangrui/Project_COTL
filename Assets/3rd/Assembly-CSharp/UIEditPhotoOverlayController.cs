using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Data.ReadWrite;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using src.Extensions;
using src.UI;
using src.UINavigator;
using UnityEngine;
using UnityEngine.UI;

public class UIEditPhotoOverlayController : UIMenuBase
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass34_0
	{
		public PhotoModeCamera.PhotoReadWriteResult result;

		public Texture2D screenshot;

		public UIEditPhotoOverlayController _003C_003E4__this;

		internal void _003CSaveNewPhotoIE_003Eg__WriteError_007C0(MMReadWriteError writeError)
		{
			result = PhotoModeCamera.PhotoReadWriteResult.Error;
		}

		internal void _003CSaveNewPhotoIE_003Eg__WriteSuccess_007C1()
		{
			result = PhotoModeCamera.PhotoReadWriteResult.Success;
		}

		internal void _003CSaveNewPhotoIE_003Eb__2()
		{
			PhotoModeCamera.Instance.IsErrorShown = false;
			UnityEngine.Object.Destroy(screenshot);
			screenshot = null;
			_003C_003E4__this.OnCancelButtonInput();
			PhotoModeManager.TakeScreenShotActive = false;
		}
	}

	public Action<PhotoModeManager.PhotoData> OnNewPhotoCreated;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private RawImage image;

	[SerializeField]
	private Transform stickerContent;

	[SerializeField]
	private StickerItem stickerItemTemplate;

	[SerializeField]
	private Image photoFlash;

	[SerializeField]
	private RawImage takenPhotoPreview;

	[SerializeField]
	private Image takenPhotoPreviewOutline;

	[SerializeField]
	private RectTransform _standardPrompts;

	[SerializeField]
	private RectTransform _editStickersPrompts;

	[SerializeField]
	private float movementSpeed;

	[SerializeField]
	private float rotateSpeed;

	[SerializeField]
	private float scaleSpeed;

	[SerializeField]
	private RectTransform PhotoIcon;

	[SerializeField]
	private GameObject controls;

	[SerializeField]
	private GameObject backControlPrompt;

	private List<StickerItem> stickerItems = new List<StickerItem>();

	private PhotoModeManager.PhotoData photoData;

	private StickerItem currentSticker;

	private float cachedScale;

	private bool stickerEnteredPhotoZone;

	private bool reselecting;

	private Tween tween;

	private List<StickerItem> placedStickers = new List<StickerItem>();

	private StickerItem CacheCurrentSticker;

	private bool inputsDisabled;

	private bool _placementToggle;

	private Texture2D FullResScreenshot;

	public void Show(PhotoModeManager.PhotoData photoData)
	{
		this.photoData = photoData;
		image.texture = photoData.PhotoTexture;
		Show();
		_standardPrompts.transform.DOKill();
		_editStickersPrompts.transform.DOKill();
		_editStickersPrompts.transform.localPosition = new Vector3(0f, -200f, 0f);
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		_standardPrompts.DOKill();
		_standardPrompts.anchoredPosition = new Vector3(0f, -200f, 0f);
		_standardPrompts.DOAnchorPos(Vector3.zero, 1f).SetUpdate(true).SetEase(Ease.OutCirc);
		PhotoIcon.anchoredPosition = new Vector3(0f, 200f, 0f);
		PhotoIcon.DOKill();
		PhotoIcon.DOAnchorPos(Vector3.zero, 1f).SetUpdate(true).SetEase(Ease.OutCirc);
		if (CheatConsole.HidingUI)
		{
			controls.gameObject.SetActive(false);
			backControlPrompt.gameObject.SetActive(false);
		}
		StickerData[] allStickers = PhotoModeManager.GetAllStickers();
		foreach (StickerData stickerData in allStickers)
		{
			StickerItem stickerItem = UnityEngine.Object.Instantiate(stickerItemTemplate, stickerContent);
			stickerItems.Add(stickerItem);
			stickerItem.Button.onClick.AddListener(delegate
			{
				if (InputManager.PhotoMode.GetPlaceStickerButtonDown())
				{
					_placementToggle = true;
				}
				SpawnSticker(stickerData);
				MonoSingleton<UINavigatorNew>.Instance.Clear();
			});
			stickerItem.Configure(stickerData, this);
		}
		OverrideDefault(stickerItems[0].Button);
		ActivateNavigation();
	}

	private void Update()
	{
		if (inputsDisabled || PhotoModeCamera.Instance.IsErrorShown || PhotoModeManager.TakeScreenShotActive)
		{
			return;
		}
		if (currentSticker != null)
		{
			if (!_placementToggle)
			{
				if (InputManager.PhotoMode.GetPlaceStickerButtonDown())
				{
					if (((RectTransform)image.transform).rect.Contains(image.transform.InverseTransformPoint(currentSticker.transform.position)))
					{
						PlaceSticker(currentSticker);
					}
					if (currentSticker == null)
					{
						return;
					}
				}
			}
			else if (InputManager.PhotoMode.GetPlaceStickerButtonUp())
			{
				_placementToggle = false;
			}
			if (InputManager.General.InputIsController())
			{
				Vector2 vector = new Vector2(InputManager.Gameplay.GetHorizontalAxis(), InputManager.Gameplay.GetVerticalAxis());
				currentSticker.transform.position += (Vector3)vector * movementSpeed * Time.unscaledDeltaTime;
			}
			else
			{
				currentSticker.transform.position = InputManager.General.GetMousePosition();
			}
			Vector3 localScale = currentSticker.transform.localScale;
			if (Mathf.Abs(InputManager.PhotoMode.GetStickerScaleAxis()) > 0f)
			{
				currentSticker.transform.DOComplete();
				localScale += Vector3.one * scaleSpeed * 3f * Time.unscaledDeltaTime * InputManager.PhotoMode.GetStickerScaleAxis();
				localScale.x = Mathf.Clamp(localScale.x, currentSticker.StickerData.MinScale, currentSticker.StickerData.MaxScale);
				localScale.y = Mathf.Clamp(localScale.y, currentSticker.StickerData.MinScale, currentSticker.StickerData.MaxScale);
				currentSticker.transform.localScale = localScale;
				cachedScale = localScale.x;
			}
			currentSticker.transform.Rotate(0f, 0f, rotateSpeed * 2f * movementSpeed * Time.unscaledDeltaTime * InputManager.PhotoMode.GetStickerRotateAxis());
			if (InputManager.PhotoMode.GetUndoButtonDown())
			{
				UndoStickerPlacement();
			}
			if (InputManager.PhotoMode.GetFlipStickerButtonDown())
			{
				currentSticker.Flip();
			}
			if (!InputManager.General.InputIsController())
			{
				bool flag = ((RectTransform)image.transform).rect.Contains(image.transform.InverseTransformPoint(currentSticker.transform.position));
				if (flag || !stickerEnteredPhotoZone)
				{
					stickerEnteredPhotoZone = flag;
					if (currentSticker.transform.localScale.x <= 0f && tween != null)
					{
						currentSticker.transform.DOKill();
						tween = currentSticker.transform.DOScale(cachedScale, 0.15f).SetEase(Ease.OutBack).SetUpdate(true);
						DisableStickers();
						reselecting = false;
					}
				}
				else if (stickerEnteredPhotoZone && currentSticker.transform.localScale.x > 0f && (tween == null || !tween.IsPlaying()))
				{
					currentSticker.transform.DOKill();
					tween = currentSticker.transform.DOScale(0f, 0.15f).SetEase(Ease.InBack).SetUpdate(true);
					EnableStickers();
					reselecting = true;
				}
			}
		}
		else
		{
			if (InputManager.PhotoMode.GetSaveButtonDown())
			{
				SaveNewPhoto();
			}
			if (InputManager.PhotoMode.GetClearStickersButtonDown())
			{
				ClearStickers();
			}
		}
		if (CacheCurrentSticker != currentSticker)
		{
			CacheCurrentSticker = currentSticker;
			if (currentSticker == null)
			{
				_standardPrompts.DOKill();
				_editStickersPrompts.DOKill();
				_standardPrompts.DOAnchorPos(Vector3.zero, 1f).SetUpdate(true).SetEase(Ease.OutCirc);
				_editStickersPrompts.DOAnchorPos(new Vector3(0f, -200f, 0f), 1f).SetUpdate(true).SetEase(Ease.OutCirc);
			}
			else
			{
				_standardPrompts.DOKill();
				_editStickersPrompts.DOKill();
				_standardPrompts.DOAnchorPos(new Vector3(0f, -200f, 0f), 1f).SetUpdate(true).SetEase(Ease.OutCirc);
				_editStickersPrompts.DOAnchorPos(Vector3.zero, 1f).SetUpdate(true).SetEase(Ease.OutCirc);
			}
		}
	}

	public override void OnCancelButtonInput()
	{
		if (!inputsDisabled && !PhotoModeManager.TakeScreenShotActive)
		{
			base.OnCancelButtonInput();
			if (currentSticker != null)
			{
				CancelSticker();
			}
			else if (currentSticker == null && _canvasGroup.interactable)
			{
				Hide();
			}
		}
	}

	private void ClearStickers()
	{
		for (int num = image.transform.childCount - 1; num >= 0; num--)
		{
			GameObject gameObject = image.transform.GetChild(num).gameObject;
			if (currentSticker == null || gameObject != currentSticker.gameObject)
			{
				gameObject.gameObject.SetActive(false);
			}
		}
	}

	public void SaveNewPhoto()
	{
		PhotoModeManager.TakeScreenShotActive = true;
		StartCoroutine(SaveNewPhotoIE());
	}

	private IEnumerator SaveNewPhotoIE()
	{
		_003C_003Ec__DisplayClass34_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass34_0();
		CS_0024_003C_003E8__locals0._003C_003E4__this = this;
		AudioManager.Instance.PlayOneShot("event:/Stings/generic_positive");
		MMVibrate.Haptic(MMVibrate.HapticTypes.Success);
		Transform parent = image.transform.parent;
		Vector3 s = image.transform.localScale;
		image.transform.SetParent(_canvas.transform);
		image.transform.SetAsLastSibling();
		image.transform.localScale = Vector3.one;
		image.transform.localPosition = Vector3.zero;
		canvas.worldCamera = PhotoModeCamera.Instance.Camera;
		canvas.renderMode = RenderMode.ScreenSpaceCamera;
		canvas.gameObject.layer = LayerMask.NameToLayer("Default");
		canvas.planeDistance = 1f;
		BiomeConstants.Instance.ppv.enabled = false;
		PhotoModeCamera.Instance.Camera.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));
		StickerItem stickerItem = currentSticker;
		if ((object)stickerItem != null)
		{
			stickerItem.gameObject.SetActive(false);
		}
		RenderTexture originalCameraTargetTexture = PhotoModeCamera.Instance.Camera.targetTexture;
		RenderTexture originalActiveRenderTexture = RenderTexture.active;
		RenderTexture rt = RenderTexture.GetTemporary(PhotoModeManager.ResolutionX, PhotoModeManager.ResolutionY, 0);
		PhotoModeCamera.Instance.Camera.targetTexture = rt;
		CS_0024_003C_003E8__locals0.screenshot = new Texture2D(PhotoModeManager.ResolutionX, PhotoModeManager.ResolutionY, TextureFormat.RGB24, false);
		yield return new WaitForEndOfFrame();
		PhotoModeCamera.Instance.Camera.Render();
		yield return new WaitForEndOfFrame();
		RenderTexture.active = rt;
		CS_0024_003C_003E8__locals0.screenshot.ReadPixels(new Rect(0f, 0f, PhotoModeManager.ResolutionX, PhotoModeManager.ResolutionY), 0, 0);
		CS_0024_003C_003E8__locals0.screenshot.Apply();
		yield return new WaitForEndOfFrame();
		PhotoModeCamera.Instance.Camera.targetTexture = originalCameraTargetTexture;
		RenderTexture.active = originalActiveRenderTexture;
		RenderTexture.ReleaseTemporary(rt);
		yield return new WaitForEndOfFrame();
		image.transform.SetParent(parent);
		image.transform.localScale = s;
		image.transform.localPosition = Vector3.zero;
		canvas.worldCamera = null;
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.gameObject.layer = LayerMask.NameToLayer("UI");
		canvas.planeDistance = 100f;
		PhotoModeCamera.Instance.Camera.cullingMask |= 1 << LayerMask.NameToLayer("UI");
		BiomeConstants.Instance.ppv.enabled = true;
		StickerItem stickerItem2 = currentSticker;
		if ((object)stickerItem2 != null)
		{
			stickerItem2.gameObject.SetActive(true);
		}
		yield return null;
		string fileName = GetScreenShotName();
		CS_0024_003C_003E8__locals0.result = PhotoModeCamera.PhotoReadWriteResult.None;
		COTLImageReadWriter imageReadWriter = PhotoModeManager.ImageReadWriter;
		imageReadWriter.OnWriteError = (Action<MMReadWriteError>)Delegate.Combine(imageReadWriter.OnWriteError, new Action<MMReadWriteError>(CS_0024_003C_003E8__locals0._003CSaveNewPhotoIE_003Eg__WriteError_007C0));
		COTLImageReadWriter imageReadWriter2 = PhotoModeManager.ImageReadWriter;
		imageReadWriter2.OnWriteCompleted = (Action)Delegate.Combine(imageReadWriter2.OnWriteCompleted, new Action(CS_0024_003C_003E8__locals0._003CSaveNewPhotoIE_003Eg__WriteSuccess_007C1));
		PhotoModeManager.ImageReadWriter.Write(CS_0024_003C_003E8__locals0.screenshot, fileName);
		Action<string> onPhotoSaved = PhotoModeManager.OnPhotoSaved;
		if (onPhotoSaved != null)
		{
			onPhotoSaved(fileName);
		}
		CS_0024_003C_003E8__locals0.screenshot.name = fileName;
		while (CS_0024_003C_003E8__locals0.result == PhotoModeCamera.PhotoReadWriteResult.None)
		{
			yield return null;
		}
		COTLImageReadWriter imageReadWriter3 = PhotoModeManager.ImageReadWriter;
		imageReadWriter3.OnWriteError = (Action<MMReadWriteError>)Delegate.Remove(imageReadWriter3.OnWriteError, new Action<MMReadWriteError>(CS_0024_003C_003E8__locals0._003CSaveNewPhotoIE_003Eg__WriteError_007C0));
		COTLImageReadWriter imageReadWriter4 = PhotoModeManager.ImageReadWriter;
		imageReadWriter4.OnWriteCompleted = (Action)Delegate.Remove(imageReadWriter4.OnWriteCompleted, new Action(CS_0024_003C_003E8__locals0._003CSaveNewPhotoIE_003Eg__WriteSuccess_007C1));
		if (CS_0024_003C_003E8__locals0.result == PhotoModeCamera.PhotoReadWriteResult.Error)
		{
			PhotoModeCamera.Instance.IsErrorShown = true;
			PhotoModeManager.ImageReadWriter.Delete(fileName);
			UIMenuConfirmationWindow uIMenuConfirmationWindow = Push(MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate);
			uIMenuConfirmationWindow.Configure(ScriptLocalization.UI_PhotoMode_Error_Write.Heading, ScriptLocalization.UI_PhotoMode_Error_Write.Heading, true);
			uIMenuConfirmationWindow.OnHide = (Action)Delegate.Combine(uIMenuConfirmationWindow.OnHide, (Action)delegate
			{
				PhotoModeCamera.Instance.IsErrorShown = false;
				UnityEngine.Object.Destroy(CS_0024_003C_003E8__locals0.screenshot);
				CS_0024_003C_003E8__locals0.screenshot = null;
				CS_0024_003C_003E8__locals0._003C_003E4__this.OnCancelButtonInput();
				PhotoModeManager.TakeScreenShotActive = false;
			});
			yield return uIMenuConfirmationWindow.YieldUntilHidden();
		}
		else
		{
			PhotoModeManager.PhotoData obj = new PhotoModeManager.PhotoData
			{
				PhotoName = fileName,
				PhotoTexture = CS_0024_003C_003E8__locals0.screenshot
			};
			Action<PhotoModeManager.PhotoData> onNewPhotoCreated = OnNewPhotoCreated;
			if (onNewPhotoCreated != null)
			{
				onNewPhotoCreated(obj);
			}
			OnPhotoSaved(CS_0024_003C_003E8__locals0.screenshot);
		}
		PhotoModeManager.TakeScreenShotActive = false;
	}

	private void OnPhotoSaved(Texture2D photo)
	{
		photoFlash.DOKill();
		photoFlash.color = new Color(photoFlash.color.r, photoFlash.color.g, photoFlash.color.b, 0f);
		photoFlash.DOFade(1f, 0.1f).OnComplete(delegate
		{
			photoFlash.DOFade(0f, 0.5f).SetUpdate(true);
		}).SetUpdate(true);
		takenPhotoPreviewOutline.DOKill();
		takenPhotoPreview.DOKill();
		takenPhotoPreview.texture = photo;
		takenPhotoPreview.color = new Color(takenPhotoPreview.color.r, takenPhotoPreview.color.g, takenPhotoPreview.color.b, 0f);
		Sequence sequence = DOTween.Sequence();
		sequence.Append(takenPhotoPreviewOutline.DOFade(1f, 0.25f).SetUpdate(true));
		sequence.Join(takenPhotoPreview.DOFade(1f, 0.25f).SetUpdate(true));
		sequence.Append(takenPhotoPreviewOutline.DOFade(0f, 1f).SetUpdate(true).SetDelay(2f));
		sequence.Join(takenPhotoPreview.DOFade(0f, 1f).SetUpdate(true).SetDelay(2f));
		sequence.OnComplete(delegate
		{
			PhotoModeManager.TakeScreenShotActive = false;
		});
		sequence.SetUpdate(true);
		sequence.Play();
	}

	private string GetScreenShotName()
	{
		int num = 0;
		string text;
		do
		{
			num++;
			text = photoData.PhotoName + string.Format("_{0}", num);
		}
		while (PhotoModeManager.ImageReadWriter.FileExists(text));
		return text;
	}

	public void SpawnSticker(StickerData stickerData)
	{
		if (reselecting)
		{
			CancelSticker();
		}
		StickerItem stickerItem = UnityEngine.Object.Instantiate((stickerData.UsePrefab && stickerData.Prefab != null) ? stickerData.Prefab : stickerItemTemplate, image.transform, image.transform);
		stickerItem.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f).SetUpdate(true);
		stickerItem.Configure(stickerData, this);
		UnityEngine.Object.Destroy(stickerItem.Button);
		stickerItem.GetComponent<Image>().raycastTarget = false;
		if (!InputManager.General.MouseInputActive)
		{
			stickerItem.transform.position = ((currentSticker != null) ? currentSticker.transform.position : image.transform.position);
		}
		if (currentSticker != null)
		{
			currentSticker.transform.DOComplete();
			stickerItem.transform.rotation = currentSticker.transform.rotation;
			stickerItem.transform.localScale = currentSticker.transform.localScale;
			stickerItem.Transform.localScale = currentSticker.Transform.localScale;
			stickerItem.Image.transform.localScale = currentSticker.Image.transform.localScale;
		}
		else
		{
			stickerItem.transform.localScale = Vector3.one * 1.5f;
			cachedScale = 1.5f;
		}
		stickerEnteredPhotoZone = false;
		reselecting = false;
		tween = null;
		currentSticker = stickerItem;
		DisableStickers();
		AudioManager.Instance.PlayOneShot("event:/ui/photo_mode/sticker_place");
		MMVibrate.Haptic(MMVibrate.HapticTypes.Selection);
	}

	private void PlaceSticker(StickerItem stickerItem)
	{
		SpawnSticker(stickerItem.StickerData);
		placedStickers.Add(stickerItem);
		stickerItem.OnStickerPlaced();
	}

	public void CancelSticker()
	{
		AudioManager.Instance.PlayOneShot("event:/ui/go_back");
		MMVibrate.Haptic(MMVibrate.HapticTypes.LightImpact);
		EnableStickers();
		foreach (StickerItem stickerItem in stickerItems)
		{
			if (stickerItem.StickerData == currentSticker.StickerData)
			{
				OverrideDefault(stickerItem.Button);
				ActivateNavigation();
				break;
			}
		}
		currentSticker.gameObject.SetActive(false);
		currentSticker = null;
		cachedScale = 1.5f;
		stickerEnteredPhotoZone = false;
		reselecting = false;
	}

	private void UndoStickerPlacement()
	{
		if (placedStickers.Count > 0)
		{
			StickerItem stickerItem = placedStickers[placedStickers.Count - 1];
			placedStickers.RemoveAt(placedStickers.Count - 1);
			stickerItem.gameObject.SetActive(false);
		}
	}

	private void EnableStickers()
	{
		foreach (StickerItem stickerItem in stickerItems)
		{
			stickerItem.Button.Interactable = true;
		}
	}

	private void DisableStickers()
	{
		foreach (StickerItem stickerItem in stickerItems)
		{
			stickerItem.Button.Interactable = false;
			stickerItem.DeSelectSticker();
		}
	}

	public void DisableAllInputs()
	{
		DisableStickers();
		inputsDisabled = true;
	}

	public void EnableAllInputs()
	{
		EnableStickers();
		inputsDisabled = false;
	}

	protected override void OnHideStarted()
	{
		base.OnHideStarted();
		UIManager.PlayAudio("event:/ui/close_menu");
	}

	protected override void OnHideCompleted()
	{
		image.texture = null;
		if (FullResScreenshot != null)
		{
			UnityEngine.Object.Destroy(FullResScreenshot);
		}
		FullResScreenshot = null;
		base.OnHideCompleted();
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
