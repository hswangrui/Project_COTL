using DG.Tweening;
using UnityEngine;

namespace Lamb.UI
{
	public class FinalizeRecipeButton : BaseMonoBehaviour
	{
		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private MMButton _button;

		[HideInInspector]
		public InventoryItem.ITEM_TYPE Recipe;

		private Vector2 _origin;

		public MMButton Button
		{
			get
			{
				return _button;
			}
		}

		private void Awake()
		{
			_origin = _rectTransform.anchoredPosition;
		}

		public void Shake()
		{
			UIManager.PlayAudio("event:/ui/negative_feedback");
			_rectTransform.DOKill();
			_rectTransform.anchoredPosition = _origin;
			_rectTransform.DOShakePosition(1f, new Vector3(10f, 0f)).SetUpdate(true);
		}
	}
}
