using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DG.Tweening;
using I2.Loc;
using src.Extensions;
using src.UINavigator;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class UIItemSelectorOverlayController : UIMenuBase
	{
		[StructLayout(LayoutKind.Auto)]
		[CompilerGenerated]
		private struct _003C_003Ec__DisplayClass44_0
		{
			public UIItemSelectorOverlayController _003C_003E4__this;

			public GenericInventoryItem item;
		}

		public Action<InventoryItem.ITEM_TYPE> OnItemChosen;

		public Func<InventoryItem.ITEM_TYPE, TraderTrackerItems> CostProvider;

		[Header("Item Selector")]
		[SerializeField]
		private RectTransform _itemSelectorRectTransform;

		[SerializeField]
		private CanvasGroup _itemSelectorCanvasGroup;

		[SerializeField]
		private RectTransform _itemContent;

		[SerializeField]
		private GameObject _buttonPrompt;

		[SerializeField]
		private TextMeshProUGUI _buttonPromptText;

		[SerializeField]
		private TextMeshProUGUI _coinsText;

		[SerializeField]
		private GameObject _coinsContainer;

		[SerializeField]
		private GameObject _backPrompt;

		[SerializeField]
		private GameObject _backText;

		[Header("Templates")]
		[SerializeField]
		private GenericInventoryItem _inventoryItemTemplate;

		private ItemSelector.Category _category;

		private ItemSelector.Params _params;

		private List<GenericInventoryItem> _inventoryItems = new List<GenericInventoryItem>();

		private string _contextString;

		private bool _didCancel;

		private List<InventoryItem> _customInventory;

		private string _addtionalText;

		private List<InventoryItem.ITEM_TYPE> _items
		{
			get
			{
				return _category.TrackedItems;
			}
		}

		private Vector2 _offset
		{
			get
			{
				return _params.Offset;
			}
		}

		private ItemSelector.Context _context
		{
			get
			{
				return _params.Context;
			}
		}

		private bool _hideOnSelection
		{
			get
			{
				return _params.HideOnSelection;
			}
		}

		private bool _showQuantity
		{
			get
			{
				return !_params.HideQuantity;
			}
		}

		private bool _showEmpty
		{
			get
			{
				return _params.ShowEmpty;
			}
		}

		private InventoryItem.ITEM_TYPE _mostRecentItem
		{
			get
			{
				return _category.MostRecentItem;
			}
		}

		private bool _preventCancellation
		{
			get
			{
				return _params.PreventCancellation;
			}
		}

		private bool _showCoins
		{
			get
			{
				return _params.ShowCoins;
			}
		}

		public override void Awake()
		{
			base.Awake();
			_itemSelectorCanvasGroup.alpha = 0f;
		}

		public void Show(List<InventoryItem> inventoryItems, ItemSelector.Params parameters, bool instant = false)
		{
			_customInventory = inventoryItems;
			List<InventoryItem.ITEM_TYPE> list = new List<InventoryItem.ITEM_TYPE>();
			foreach (InventoryItem item in _customInventory)
			{
				list.Add((InventoryItem.ITEM_TYPE)item.type);
			}
			Show(list, parameters, instant);
		}

		public void Show(List<InventoryItem.ITEM_TYPE> items, ItemSelector.Params parameters, bool instant = false)
		{
			_params = parameters;
			_category = DataManager.Instance.GetItemSelectorCategory(parameters.Key);
			foreach (InventoryItem.ITEM_TYPE item in items)
			{
				if (!_category.TrackedItems.Contains(item) && (!parameters.RequiresDiscovery || GetItemQuantity(item) != 0))
				{
					_category.TrackedItems.Add(item);
				}
			}
			_contextString = LocalizationManager.GetTranslation(string.Format("UI/ItemSelector/Context/{0}", _context));
			Show(instant);
			if (!_hideOnSelection)
			{
				StartCoroutine(WaitUntilRelease());
			}
		}

		private IEnumerator WaitUntilRelease()
		{
			while (!InputManager.UI.GetAcceptButtonUp())
			{
				yield return null;
			}
			MonoSingleton<UINavigatorNew>.Instance.AllowAcceptHold = true;
		}

		protected override void OnShowStarted()
		{
			_itemSelectorRectTransform.localScale = Vector2.one * 0.75f;
			_itemSelectorRectTransform.anchoredPosition += _offset;
			_backPrompt.SetActive(!_preventCancellation);
			_backText.SetActive(!_preventCancellation);
			if (_items.Count > 0 && _inventoryItems.Count == 0)
			{
				int index = 0;
				foreach (InventoryItem.ITEM_TYPE item in _items)
				{
					if (_showEmpty || GetItemQuantity(item) != 0)
					{
						GenericInventoryItem inventoryItemInstance = GameObjectExtensions.Instantiate(_inventoryItemTemplate, _itemContent);
						if (_customInventory != null)
						{
							inventoryItemInstance.Configure(_customInventory[_items.IndexOf(item)], _showQuantity);
						}
						else
						{
							inventoryItemInstance.Configure(item, _showQuantity);
						}
						inventoryItemInstance.Button.onClick.AddListener(delegate
						{
							OnItemClicked(inventoryItemInstance);
						});
						MMButton button = inventoryItemInstance.Button;
						button.OnSelected = (Action)Delegate.Combine(button.OnSelected, (Action)delegate
						{
							OnItemSelected(inventoryItemInstance);
						});
						_inventoryItems.Add(inventoryItemInstance);
						if (item == _category.MostRecentItem)
						{
							index = _inventoryItems.Count - 1;
						}
					}
				}
				if (_inventoryItems.Count > 0)
				{
					OverrideDefault(_inventoryItems[index].Button);
					ActivateNavigation();
				}
			}
			if (_inventoryItems.Count == 0)
			{
				_buttonPromptText.text = "-";
				_buttonPrompt.SetActive(false);
			}
		}

		private int GetItemQuantity(InventoryItem.ITEM_TYPE itemType)
		{
			if (_customInventory != null)
			{
				foreach (InventoryItem item in _customInventory)
				{
					if (item.type == (int)itemType)
					{
						return item.quantity;
					}
				}
			}
			return Inventory.GetItemQuantity(itemType);
		}

		protected override IEnumerator DoShowAnimation()
		{
			_itemSelectorRectTransform.DOScale(Vector2.one * 1.15f, 0.1f).SetEase(Ease.OutSine).SetUpdate(true);
			_itemSelectorCanvasGroup.DOFade(1f, 0.1f).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.1f);
			_itemSelectorRectTransform.DOScale(Vector2.one, 0.125f).SetEase(Ease.InSine).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.125f);
		}

		protected override IEnumerator DoHideAnimation()
		{
			_itemSelectorRectTransform.DOKill();
			_itemSelectorCanvasGroup.DOKill();
			_itemSelectorRectTransform.DOScale(Vector2.one * 1.15f, 0.1f).SetEase(Ease.OutSine).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.1f);
			_itemSelectorRectTransform.DOScale(Vector2.one * 0.75f, 0.125f).SetEase(Ease.InSine).SetUpdate(true);
			_itemSelectorCanvasGroup.DOFade(0f, 0.1f).SetDelay(0.1f).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.125f);
		}

		private void OnItemClicked(GenericInventoryItem item)
		{
			_003C_003Ec__DisplayClass44_0 _003C_003Ec__DisplayClass44_ = default(_003C_003Ec__DisplayClass44_0);
			_003C_003Ec__DisplayClass44_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass44_.item = item;
			if (_context == ItemSelector.Context.SetLabel)
			{
				_003COnItemClicked_003Eg__Choose_007C44_0(ref _003C_003Ec__DisplayClass44_);
				return;
			}
			if (GetItemQuantity(_003C_003Ec__DisplayClass44_.item.Type) > 0)
			{
				if (_context != ItemSelector.Context.Buy)
				{
					_003COnItemClicked_003Eg__Choose_007C44_0(ref _003C_003Ec__DisplayClass44_);
					return;
				}
				Func<InventoryItem.ITEM_TYPE, TraderTrackerItems> costProvider = CostProvider;
				TraderTrackerItems traderTrackerItems = ((costProvider != null) ? costProvider(_003C_003Ec__DisplayClass44_.item.Type) : null);
				if (traderTrackerItems != null && Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD) >= traderTrackerItems.SellPriceActual)
				{
					_003COnItemClicked_003Eg__Choose_007C44_0(ref _003C_003Ec__DisplayClass44_);
					return;
				}
			}
			_003C_003Ec__DisplayClass44_.item.Shake();
			AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback");
		}

		private void OnItemSelected(GenericInventoryItem item)
		{
			_category.MostRecentItem = item.Type;
			RefreshContextText();
		}

		private void RefreshContextText()
		{
			if (_context == ItemSelector.Context.Sell || _context == ItemSelector.Context.Buy)
			{
				Func<InventoryItem.ITEM_TYPE, TraderTrackerItems> costProvider = CostProvider;
				TraderTrackerItems traderTrackerItems = ((costProvider != null) ? costProvider(_mostRecentItem) : null);
				if (traderTrackerItems == null)
				{
					return;
				}
				if (_context == ItemSelector.Context.Buy)
				{
					if (traderTrackerItems.SellOffset > 0)
					{
						float num = (float)traderTrackerItems.SellPrice / (float)traderTrackerItems.SellPriceActual;
						num *= 100f;
						_addtionalText = " <color=red>+ " + Math.Round(num, 0) + "%</color> ";
					}
					_buttonPromptText.text = string.Format(_contextString, InventoryItem.LocalizedName(_mostRecentItem) ?? "", CostFormatter.FormatCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, traderTrackerItems.SellPriceActual)) + _addtionalText;
				}
				else
				{
					_buttonPromptText.text = string.Format(_contextString, InventoryItem.LocalizedName(_mostRecentItem), CostFormatter.FormatCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, traderTrackerItems.BuyPriceActual, true, true)) + _addtionalText;
				}
			}
			else
			{
				_buttonPromptText.text = string.Format(_contextString, InventoryItem.LocalizedName(_mostRecentItem)) + _addtionalText;
			}
		}

		private void Update()
		{
			if (_showCoins)
			{
				_coinsContainer.SetActive(true);
				_coinsText.text = "<sprite name=\"icon_blackgold\"> " + Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD);
			}
			else
			{
				_coinsContainer.SetActive(false);
				_coinsText.text = "";
			}
		}

		public void UpdateQuantities()
		{
			foreach (GenericInventoryItem inventoryItem in _inventoryItems)
			{
				inventoryItem.UpdateQuantity();
			}
			RefreshContextText();
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable && !_preventCancellation)
			{
				Hide();
				_didCancel = true;
			}
		}

		protected override void OnHideStarted()
		{
			MonoSingleton<UINavigatorNew>.Instance.AllowAcceptHold = false;
		}

		protected override void OnHideCompleted()
		{
			if (_didCancel)
			{
				Action onCancel = OnCancel;
				if (onCancel != null)
				{
					onCancel();
				}
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		[CompilerGenerated]
		private void _003COnItemClicked_003Eg__Choose_007C44_0(ref _003C_003Ec__DisplayClass44_0 P_0)
		{
			Action<InventoryItem.ITEM_TYPE> onItemChosen = OnItemChosen;
			if (onItemChosen != null)
			{
				onItemChosen(P_0.item.Type);
			}
			if (_hideOnSelection)
			{
				Hide();
			}
			else
			{
				UpdateQuantities();
			}
		}
	}
}
