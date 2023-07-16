using System;
using DG.Tweening;
using Lamb.UI;
using Rewired;
using src.UI;
using UnityEngine;
using UnityEngine.UI;

public class UITakePhotoOverlayController : UIMenuBase
{
	[SerializeField]
	private MMSlider focusSlider;

	[SerializeField]
	private MMSlider tiltSlider;

	[SerializeField]
	private Image photoFlash;

	[SerializeField]
	private RawImage takenPhotoPreview;

	[SerializeField]
	private Image takenPhotoPreviewOutline;

	[SerializeField]
	private UI_Transitions cameraIcon;

	[SerializeField]
	private Image fadeInImage;

	[SerializeField]
	private Image iconForCameraSnap;

	[SerializeField]
	private Image eyeIcon;

	[SerializeField]
	private PhotoGalleryAlert _galleryAlert;

	[SerializeField]
	private GameObject _controls;

	[Header("Controls")]
	[SerializeField]
	private GameObject _tiltConsole;

	[SerializeField]
	private GameObject _tiltPC;

	[SerializeField]
	private Image transformObjectX;

	[SerializeField]
	private Image transformObjectY;

	[SerializeField]
	private Image transformObjectZ;

	private Vector3 _takenPhotoPreviewStartPos;

	private bool _takenPhoto;

	public MMSlider FocusSlider
	{
		get
		{
			return focusSlider;
		}
	}

	public MMSlider TiltSlider
	{
		get
		{
			return tiltSlider;
		}
	}

	public Image TransformObjectX
	{
		get
		{
			return transformObjectX;
		}
	}

	public Image TransformObjectY
	{
		get
		{
			return transformObjectY;
		}
	}

	public Image TransformObjectZ
	{
		get
		{
			return transformObjectZ;
		}
	}

	public override void Awake()
	{
		base.Awake();
		PhotoModeManager.OnPhotoTaken += OnPhotoTaken;
		GeneralInputSource general = InputManager.General;
		general.OnActiveControllerChanged = (Action<Controller>)Delegate.Combine(general.OnActiveControllerChanged, new Action<Controller>(OnActiveControllerChanged));
		OnActiveControllerChanged(InputManager.General.GetLastActiveController());
		PhotoModeManager.OnPhotoSaved = (Action<string>)Delegate.Combine(PhotoModeManager.OnPhotoSaved, new Action<string>(OnPhotoSaved));
		if (PhotoModeManager.ImageReadWriter.GetFiles().Length == 0)
		{
			DataManager.Instance.Alerts.GalleryAlerts.ClearAll();
		}
		if (CheatConsole.HidingUI)
		{
			_controls.gameObject.SetActive(false);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		PhotoModeManager.OnPhotoTaken -= OnPhotoTaken;
		PhotoModeManager.OnPhotoSaved = (Action<string>)Delegate.Remove(PhotoModeManager.OnPhotoSaved, new Action<string>(OnPhotoSaved));
		GeneralInputSource general = InputManager.General;
		general.OnActiveControllerChanged = (Action<Controller>)Delegate.Remove(general.OnActiveControllerChanged, new Action<Controller>(OnActiveControllerChanged));
		MonoSingleton<UIManager>.Instance.PhotoInformationBoxTemplate.DestroyAll();
		ReleaseTexture();
	}

	private void OnActiveControllerChanged(Controller controller)
	{
		_tiltConsole.SetActive(InputManager.General.InputIsController(controller));
		_tiltPC.SetActive(!InputManager.General.InputIsController(controller));
	}

	private void OnPhotoSaved(string photo)
	{
		_galleryAlert.ResetAlert();
		_galleryAlert.ConfigureSingle();
	}

	protected override void OnShowCompleted()
	{
		base.OnShowCompleted();
		SetActiveStateForMenu(true);
		_takenPhotoPreviewStartPos = takenPhotoPreview.transform.position;
		iconForCameraSnap.color = StaticColors.GreyColor;
		Vector3 position = cameraIcon.transform.position;
		cameraIcon.transform.position = position + new Vector3(0f, 200f, 0f);
		cameraIcon.transform.DOKill();
		cameraIcon.transform.DOMove(position, 1f).SetUpdate(true).SetEase(Ease.OutBack);
		fadeInImage.gameObject.SetActive(true);
		fadeInImage.color = Color.black;
		fadeInImage.DOKill();
		fadeInImage.DOColor(new Color(0f, 0f, 0f, 0f), 1f).SetUpdate(true);
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
	}

	private void OnPhotoTaken(Texture2D photo)
	{
		_takenPhoto = true;
		DataManager.Instance.PhotoUsedCamera = true;
		iconForCameraSnap.color = StaticColors.GreenColor;
		iconForCameraSnap.DOKill();
		iconForCameraSnap.DOColor(StaticColors.GreyColor, 1f).SetDelay(1f).SetUpdate(true);
		photoFlash.DOKill();
		photoFlash.color = new Color(photoFlash.color.r, photoFlash.color.g, photoFlash.color.b, 0f);
		if (SettingsManager.Settings != null && SettingsManager.Settings.Accessibility != null && SettingsManager.Settings.Accessibility.DarkMode)
		{
			photoFlash.color = new Color(0f, 0f, 0f, 0f);
		}
		photoFlash.DOFade(1f, 0.1f).OnComplete(delegate
		{
			photoFlash.DOFade(0f, 0.5f).SetUpdate(true);
		}).SetUpdate(true);
		takenPhotoPreviewOutline.DOKill();
		takenPhotoPreview.DOKill();
		takenPhotoPreview.texture = photo;
		takenPhotoPreview.transform.position = _takenPhotoPreviewStartPos + new Vector3(0f, 200f, 0f);
		takenPhotoPreview.rectTransform.DOMove(_takenPhotoPreviewStartPos, 2f).SetUpdate(true).SetEase(Ease.OutCirc)
			.SetDelay(0.25f);
		takenPhotoPreview.color = new Color(takenPhotoPreview.color.r, takenPhotoPreview.color.g, takenPhotoPreview.color.b, 0f);
		Sequence sequence = DOTween.Sequence();
		sequence.Append(takenPhotoPreviewOutline.DOFade(1f, 0.25f).SetUpdate(true));
		sequence.Join(takenPhotoPreview.DOFade(1f, 0.25f).SetUpdate(true));
		sequence.Append(takenPhotoPreviewOutline.DOFade(0f, 1f).SetUpdate(true).SetDelay(2f));
		sequence.Join(takenPhotoPreview.DOFade(0f, 1f).SetUpdate(true).SetDelay(2f));
		sequence.OnComplete(delegate
		{
			RemovePhotoFromMemory(photo);
		});
		sequence.SetUpdate(true);
		sequence.Play();
		eyeIcon.transform.localScale = new Vector3(1f, 0f, 1f);
		eyeIcon.transform.DOScale(Vector3.one, 0.25f).SetUpdate(true);
	}

	private void RemovePhotoFromMemory(Texture2D photo)
	{
		ReleaseTexture();
		if (photo != null)
		{
			UnityEngine.Object.Destroy(photo);
		}
		PhotoModeManager.TakeScreenShotActive = false;
	}

	private void ReleaseTexture()
	{
		if (takenPhotoPreview != null && takenPhotoPreview.texture != null)
		{
			UnityEngine.Object.Destroy(takenPhotoPreview.texture);
			takenPhotoPreview.texture = null;
		}
	}

	public override void OnCancelButtonInput()
	{
		if (!PhotoModeManager.TakeScreenShotActive && _canvasGroup.interactable)
		{
			Hide();
		}
	}

	private void Update()
	{
		Time.timeScale = 0f;
		if (PhotoModeManager.CurrentPhotoState == PhotoModeManager.PhotoState.TakePhoto && !PhotoModeManager.TakeScreenShotActive && InputManager.PhotoMode.GetGalleryFolderButtonDown())
		{
			OpenGallery();
		}
	}

	private void OpenGallery()
	{
		DataManager.Instance.PhotoCameraLookedAtGallery = true;
		PhotoModeManager.CurrentPhotoState = PhotoModeManager.PhotoState.Gallery;
		UIPhotoGalleryMenuController gallery = Push(MonoSingleton<UIManager>.Instance.PhotoGalleryMenuTemplate);
		UIPhotoGalleryMenuController uIPhotoGalleryMenuController = gallery;
		uIPhotoGalleryMenuController.OnHidden = (Action)Delegate.Combine(uIPhotoGalleryMenuController.OnHidden, (Action)delegate
		{
			gallery = null;
			PhotoModeManager.CurrentPhotoState = PhotoModeManager.PhotoState.TakePhoto;
		});
	}
}
