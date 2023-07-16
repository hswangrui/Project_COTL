using System;
using System.Collections;
using DG.Tweening;
using Lamb.UI.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public abstract class UIInventoryItem : BaseMonoBehaviour
	{
		[SerializeField]
		protected Image _icon;

		[SerializeField]
		protected RectTransform _rectTransform;

		[SerializeField]
		protected CanvasGroup _canvasGroup;

		[SerializeField]
		protected InventoryIconMapping _iconMapping;

		[SerializeField]
		protected TextMeshProUGUI _amountText;

		[SerializeField]
		protected MMButton _button;

		[SerializeField]
		protected RectTransform _container;

		protected InventoryItem _item;

		protected int _quantity;

		protected bool _showQuantity;

		private Vector2 _containerOrigin;

		public MMButton Button
		{
			get
			{
				return _button;
			}
		}

		public InventoryItem.ITEM_TYPE Type
		{
			get
			{
				return (InventoryItem.ITEM_TYPE)_item.type;
			}
		}

		public int Quantity
		{
			get
			{
				return _quantity;
			}
		}

		public RectTransform RectTransform
		{
			get
			{
				return _rectTransform;
			}
		}

		public CanvasGroup CanvasGroup
		{
			get
			{
				return _canvasGroup;
			}
		}

		public virtual void Configure(InventoryItem.ITEM_TYPE type, bool showQuantity = true)
		{
			InventoryItem inventoryItem = Inventory.GetItemByType(type);
			if (inventoryItem == null)
			{
				inventoryItem = new InventoryItem(type, 0);
			}
			Configure(inventoryItem, showQuantity);
		}

		public virtual void Configure(InventoryItem item, bool showQuantity = true)
		{
			_item = item;
			_showQuantity = showQuantity;
			if (_amountText != null)
			{
				_amountText.gameObject.SetActive(_showQuantity);
			}
			_iconMapping.GetImage(Type, _icon);
			_containerOrigin = _container.anchoredPosition;
			UpdateQuantity();
		}

		public void FadeIn(float delay, Action andThen = null)
		{
			StopAllCoroutines();
			StartCoroutine(DoFade(delay, andThen));
		}

		private IEnumerator DoFade(float delay, Action andThen)
		{
			_canvasGroup.alpha = 0f;
			yield return new WaitForSecondsRealtime(delay);
			float progress = 0f;
			float duration = 0.2f;
			while (true)
			{
				float num;
				progress = (num = progress + Time.unscaledDeltaTime);
				if (!(num < duration))
				{
					break;
				}
				_canvasGroup.alpha = progress / duration;
				yield return null;
			}
			_canvasGroup.alpha = 1f;
			if (andThen != null)
			{
				andThen();
			}
		}

		public void Shake()
		{
			_container.transform.DOKill();
			_container.anchoredPosition = _containerOrigin;
			_container.transform.DOShakePosition(1f, new Vector3(10f, 0f)).SetUpdate(true);
		}

		public virtual void UpdateQuantity()
		{
			_quantity = _item.quantity;
			if (_showQuantity)
			{
				_icon.color = new Color((_quantity > 0) ? 1 : 0, 1f, 1f, 1f);
				_amountText.color = ((_quantity <= 0) ? StaticColors.RedColor : StaticColors.OffWhiteColor);
				_amountText.text = _quantity.ToString();
			}
			else
			{
				_amountText.text = "";
			}
		}
	}
}
