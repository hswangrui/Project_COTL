using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StickerItem : MonoBehaviour
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private MMButton button;

	[SerializeField]
	private Image _outline;

	protected UIEditPhotoOverlayController _editOverlay;

	[SerializeField]
	private RectTransform _transform;

	public Image Image
	{
		get
		{
			return image;
		}
	}

	public MMButton Button
	{
		get
		{
			return button;
		}
	}

	public StickerData StickerData { get; private set; }

	public RectTransform Transform
	{
		get
		{
			return _transform;
		}
	}

	public void Configure(StickerData stickerData, UIEditPhotoOverlayController editOverlay)
	{
		StickerData = stickerData;
		_editOverlay = editOverlay;
		image.sprite = stickerData.Sticker;
		_outline.sprite = stickerData.Sticker;
		_outline.gameObject.SetActive(false);
		MMButton mMButton = button;
		mMButton.OnSelected = (Action)Delegate.Combine(mMButton.OnSelected, new Action(SelectSticker));
		MMButton mMButton2 = button;
		mMButton2.OnDeselected = (Action)Delegate.Combine(mMButton2.OnDeselected, new Action(DeSelectSticker));
		base.transform.localScale = Vector3.one * 0.8f;
	}

	private void OnDisable()
	{
		if (button != null)
		{
			MMButton mMButton = button;
			mMButton.OnSelected = (Action)Delegate.Remove(mMButton.OnSelected, new Action(SelectSticker));
			MMButton mMButton2 = button;
			mMButton2.OnDeselected = (Action)Delegate.Remove(mMButton2.OnDeselected, new Action(DeSelectSticker));
		}
	}

	public void SelectSticker()
	{
		_outline.gameObject.SetActive(true);
		base.transform.DOKill();
		base.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.InSine).SetUpdate(true);
	}

	public void DeSelectSticker()
	{
		_outline.gameObject.SetActive(false);
		base.transform.DOKill();
		base.transform.DOScale(Vector3.one * 0.8f, 0.1f).SetEase(Ease.OutSine).SetUpdate(true);
	}

	public virtual void Flip()
	{
		Vector3 localScale = Transform.localScale;
		localScale.x *= -1f;
		Transform.localScale = localScale;
	}

	public virtual void OnStickerPlaced()
	{
	}
}
