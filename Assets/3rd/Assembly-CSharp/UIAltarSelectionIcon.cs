using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIAltarSelectionIcon : BaseMonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[FormerlySerializedAs("SelectedImage")]
	public Image _selectedImage;

	private void OnEnable()
	{
		_selectedImage.color = new Color(1f, 1f, 1f, 0f);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		_selectedImage.DOKill();
		_selectedImage.DOColor(new Color(1f, 1f, 1f, 0f), 0.4f).SetUpdate(true);
		base.transform.DOKill();
		base.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
	}

	public void OnSelect(BaseEventData eventData)
	{
		_selectedImage.DOKill();
		_selectedImage.DOColor(Color.white, 0.2f).SetUpdate(true);
		base.transform.DOKill();
		base.transform.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
	}
}
