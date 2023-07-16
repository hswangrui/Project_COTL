using System;
using System.Collections.Generic;
using DG.Tweening;
using src.Extensions;
using src.UINavigator;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class RecipeQueue : BaseMonoBehaviour
	{
		public Func<IMMSelectable> RequestSelectable;

		public Action<InventoryItem.ITEM_TYPE, int> OnRecipeRemoved;

		[SerializeField]
		private RectTransform _contentContainer;

		[SerializeField]
		private RecipeItem _inventoryItemTemplate;

		[SerializeField]
		private TextMeshProUGUI _count;

		[SerializeField]
		private GameObject[] _vacantSlots;

		private StructuresData _kitchenData;

		private List<RecipeItem> _items = new List<RecipeItem>();

		public void Configure(StructuresData kitchenData)
		{
			_kitchenData = kitchenData;
			for (int i = 0; i < _vacantSlots.Length; i++)
			{
				_vacantSlots[i].SetActive(i < RecipeLimit());
			}
			foreach (Interaction_Kitchen.QueuedMeal queuedMeal in kitchenData.QueuedMeals)
			{
				AddRecipe(queuedMeal.MealType);
			}
			UpdateCount();
		}

		public void AddRecipe(InventoryItem.ITEM_TYPE recipe)
		{
			MakeRecipe(recipe);
			UpdateCount();
		}

		private RecipeItem MakeRecipe(InventoryItem.ITEM_TYPE recipe)
		{
			RecipeItem newItem = GameObjectExtensions.Instantiate(_inventoryItemTemplate, _contentContainer);
			newItem.Button.onClick.AddListener(delegate
			{
				RemoveRecipe(newItem);
			});
			Vector3 localScale = newItem.RectTransform.localScale;
			newItem.RectTransform.localScale = Vector3.one * 1.2f;
			newItem.RectTransform.DOScale(localScale, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
			newItem.Configure(recipe, false, true);
			_vacantSlots[_items.Count].SetActive(false);
			_items.Add(newItem);
			newItem.transform.SetSiblingIndex(_items.Count - 1);
			return newItem;
		}

		private void RemoveRecipe(RecipeItem item)
		{
			Action<InventoryItem.ITEM_TYPE, int> onRecipeRemoved = OnRecipeRemoved;
			if (onRecipeRemoved != null)
			{
				onRecipeRemoved(item.Type, _items.IndexOf(item));
			}
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
			else if (_items.Count - 1 > 0)
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(_items[_items.Count - 2].Button);
			}
			else
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(RequestSelectable());
			}
			_items.Remove(item);
			UnityEngine.Object.Destroy(item.gameObject);
			_vacantSlots[_items.Count].SetActive(true);
			UpdateCount();
		}

		private void UpdateCount()
		{
			Color colour = ((_kitchenData.QueuedMeals.Count > 0) ? StaticColors.GreenColor : StaticColors.RedColor);
			_count.text = string.Format("{0}/{1}", _kitchenData.QueuedMeals.Count, RecipeLimit()).Colour(colour);
		}

		private int RecipeLimit()
		{
			return 12 + ((_kitchenData.Type == StructureBrain.TYPES.KITCHEN_II) ? 5 : 0);
		}
	}
}
