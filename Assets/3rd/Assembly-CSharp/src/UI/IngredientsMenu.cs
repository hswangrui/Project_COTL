using System;
using System.Collections.Generic;
using Lamb.UI;
using src.Extensions;
using src.UINavigator;
using UnityEngine;

namespace src.UI
{
	public class IngredientsMenu : UISubmenuBase
	{
		public const int kMaxIngredients = 3;

		public Action<InventoryItem.ITEM_TYPE> OnIngredientChosen;

		[SerializeField]
		private RectTransform _contentContainer;

		[SerializeField]
		private IngredientItem _inventoryItemTemplate;

		[SerializeField]
		private GameObject _noIngredientsText;

		[SerializeField]
		private MMScrollRect _scrollRect;

		private StructuresData _kitchenData;

		private List<IngredientItem> _items = new List<IngredientItem>();

		public void Configure(StructuresData kitchenData)
		{
			_kitchenData = kitchenData;
		}

		protected override void OnShowStarted()
		{
			_scrollRect.enabled = false;
			if (_items.Count == 0)
			{
				List<InventoryItem> list = new List<InventoryItem>();
				InventoryItem.ITEM_TYPE[] allFoods = CookingData.GetAllFoods();
				foreach (InventoryItem.ITEM_TYPE iTEM_TYPE in allFoods)
				{
					if (Inventory.GetItemQuantity(iTEM_TYPE) > 0)
					{
						list.Add(Inventory.GetItemByType(iTEM_TYPE));
						continue;
					}
					foreach (InventoryItem item in _kitchenData.Inventory)
					{
						if (item.type == (int)iTEM_TYPE)
						{
							list.Add(new InventoryItem(iTEM_TYPE, 0));
							break;
						}
					}
				}
				foreach (InventoryItem item2 in list)
				{
					IngredientItem newItem = GameObjectExtensions.Instantiate(_inventoryItemTemplate, _contentContainer);
					newItem.Configure((InventoryItem.ITEM_TYPE)item2.type, false);
					newItem.Button.onClick.AddListener(delegate
					{
						OnIngredientClicked(newItem);
					});
					_items.Add(newItem);
				}
			}
			_scrollRect.normalizedPosition = Vector3.one;
			_scrollRect.enabled = true;
			_noIngredientsText.SetActive(_items.Count == 0);
		}

		private void OnIngredientClicked(IngredientItem item)
		{
			if (_kitchenData.Inventory.Count < 3 && Inventory.GetItemQuantity(item.Type) > 0)
			{
				Action<InventoryItem.ITEM_TYPE> onIngredientChosen = OnIngredientChosen;
				if (onIngredientChosen != null)
				{
					onIngredientChosen(item.Type);
				}
			}
			else
			{
				item.Shake();
			}
		}

		public IMMSelectable ProvideFirstSelectable()
		{
			if (_items.Count > 0)
			{
				return _items[0].Button;
			}
			return null;
		}

		public IMMSelectable ProvideSelectable()
		{
			return _items.LastElement().Button;
		}

		public void UpdateQuantities()
		{
			foreach (IngredientItem item in _items)
			{
				if (item.Quantity == 0 && Inventory.GetItemQuantity(item.Type) > 0)
				{
					item.Configure(Inventory.GetItemByType(item.Type));
				}
				else
				{
					item.UpdateQuantity();
				}
			}
		}
	}
}
