using System;
using DG.Tweening;
using src.UI;
using UnityEngine;
using UnityEngine.UI;

public class PhotoInformationBox : MonoBehaviour, IPoolListener
{
	[SerializeField]
	private RawImage image;

	[SerializeField]
	private MMButton button;

	[SerializeField]
	private PhotoGalleryAlert _alert;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	public Action<PhotoInformationBox> OnPhotoSelected;

	public Action<PhotoInformationBox> OnPhotoHovered;

	private bool _removeAlertOnHover;

	public MMButton Button
	{
		get
		{
			return button;
		}
	}

	public PhotoModeManager.PhotoData PhotoData { get; private set; }

	public void Configure(PhotoModeManager.PhotoData photoData, bool removeAlertOnHover)
	{
		PhotoData = photoData;
		image.texture = PhotoData.PhotoTexture;
		button.onClick.AddListener(PhotoSelected);
		MMButton mMButton = button;
		mMButton.OnSelected = (Action)Delegate.Combine(mMButton.OnSelected, new Action(PhotoHovered));
		MMButton mMButton2 = button;
		mMButton2.OnDeselected = (Action)Delegate.Combine(mMButton2.OnDeselected, new Action(OnPhotoDeselected));
		ColorBlock colors = button.Selectable.colors;
		colors.normalColor = new Color(1f, 1f, 1f, 0f);
		colors.highlightedColor = StaticColors.GreenColor;
		colors.selectedColor = StaticColors.GreenColor;
		colors.pressedColor = StaticColors.GreenColor;
		button.Selectable.colors = colors;
		_canvasGroup.alpha = 0f;
		_canvasGroup.DOFade(1f, 0.25f).SetUpdate(true);
		_removeAlertOnHover = removeAlertOnHover;
		_alert.Configure(photoData.PhotoName);
	}

	public void OnRecycled()
	{
		OnPhotoSelected = null;
		OnPhotoHovered = null;
		button.interactable = true;
		image.raycastTarget = true;
		button.onClick.RemoveAllListeners();
		MMButton mMButton = button;
		mMButton.OnSelected = (Action)Delegate.Remove(mMButton.OnSelected, new Action(PhotoHovered));
		MMButton mMButton2 = button;
		mMButton2.OnDeselected = (Action)Delegate.Remove(mMButton2.OnDeselected, new Action(OnPhotoDeselected));
		button.OnConfirmDenied = null;
		button.Confirmable = true;
		button.SetNormalTransitionState();
		UnityEngine.Object.Destroy(PhotoData.PhotoTexture);
		PhotoData.PhotoTexture = null;
		PhotoData = null;
		image.texture = null;
		_canvasGroup.alpha = 1f;
		_alert.ResetAlert();
	}

	private void OnPhotoDeselected()
	{
		if (!_removeAlertOnHover)
		{
			_alert.TryRemoveAlert();
		}
	}

	private void PhotoHovered()
	{
		Action<PhotoInformationBox> onPhotoHovered = OnPhotoHovered;
		if (onPhotoHovered != null)
		{
			onPhotoHovered(this);
		}
		if (_removeAlertOnHover)
		{
			_alert.TryRemoveAlert();
		}
	}

	private void PhotoSelected()
	{
		Action<PhotoInformationBox> onPhotoSelected = OnPhotoSelected;
		if (onPhotoSelected != null)
		{
			onPhotoSelected(this);
		}
	}
}
