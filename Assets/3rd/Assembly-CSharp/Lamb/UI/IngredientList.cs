using System;
using System.Collections.Generic;
using src.Extensions;
using src.UINavigator;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class IngredientList : BaseMonoBehaviour
	{
		public Action<InventoryItem.ITEM_TYPE, int> OnIngredientRemoved;

		public Func<IMMSelectable> RequestSelectable;

		[SerializeField]
		private RectTransform _contentContainer;

		[SerializeField]
		private IngredientItem _inventoryItemTemplate;

		[SerializeField]
		private TextMeshProUGUI _count;

		[SerializeField]
		private GameObject[] _vacantSlots;

		private StructuresData _kitchenData;

		private List<IngredientItem> _items = new List<IngredientItem>();

		public void Configure(StructuresData kitchenData)
		{
			_kitchenData = kitchenData;
			foreach (InventoryItem item in kitchenData.Inventory)
			{
				AddIngredient((InventoryItem.ITEM_TYPE)item.type);
			}
			UpdateCount();
		}

		public void AddIngredient(InventoryItem.ITEM_TYPE ingredient)
		{
			MakeIngredient(ingredient);
			UpdateCount();
		}

		public void Clear()
		{
			foreach (IngredientItem item in _items)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			_items.Clear();
			UpdateCount();
		}

		private void RemoveIngredient(IngredientItem item)
		{
			IMMSelectable iMMSelectable = item.Button.FindSelectableOnRight() as IMMSelectable;
			IMMSelectable iMMSelectable2 = item.Button.FindSelectableOnLeft() as IMMSelectable;
			if (_items.IndexOf(item) < _items.Count - 1 && iMMSelectable != null)
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(iMMSelectable);
			}
			else if (_items.IndexOf(item) > 0 && iMMSelectable2 != null)
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(iMMSelectable2);
			}
			else
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(RequestSelectable());
			}
			Action<InventoryItem.ITEM_TYPE, int> onIngredientRemoved = OnIngredientRemoved;
			if (onIngredientRemoved != null)
			{
				onIngredientRemoved(item.Type, _items.IndexOf(item));
			}
			_items.Remove(item);
			UnityEngine.Object.Destroy(item.gameObject);
			_vacantSlots[_items.Count].SetActive(true);
			UpdateCount();
		}

		private IngredientItem MakeIngredient(InventoryItem.ITEM_TYPE ingredient)
		{
			IngredientItem newItem = GameObjectExtensions.Instantiate(_inventoryItemTemplate, _contentContainer);
			newItem.Configure(ingredient, true, false);
			newItem.Button.onClick.AddListener(delegate
			{
				RemoveIngredient(newItem);
			});
			_vacantSlots[_items.Count].SetActive(false);
			_items.Add(newItem);
			newItem.transform.SetSiblingIndex(_items.Count - 1);
			return newItem;
		}

		private void UpdateCount()
		{
			Color colour = ((_kitchenData.Inventory.Count >= 3) ? StaticColors.GreenColor : StaticColors.RedColor);
			_count.text = string.Format("{0}/{1}", _kitchenData.Inventory.Count, 3).Colour(colour);
		}

		public IMMSelectable ProvideFirstSelectable()
		{
			if (_items.Count > 0)
			{
				return _items[0].Button;
			}
			return null;
		}
	}
}
