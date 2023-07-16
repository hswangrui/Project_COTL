using System;
using System.Collections.Generic;
using Lamb.UI;
using src.Extensions;
using src.UINavigator;
using UnityEngine;

namespace src.UI
{
	public class RecipesMenu : BaseMonoBehaviour
	{
		public const int kMaxRecipes = 12;

		public Action<InventoryItem.ITEM_TYPE> OnRecipeChosen;

		[SerializeField]
		private RectTransform _contentContainer;

		[SerializeField]
		private RecipeItem _recipeItemTemplate;

		[SerializeField]
		private GameObject _noRecipesText;

		private StructuresData _kitchenData;

		private List<RecipeItem> _items = new List<RecipeItem>();

		public void Configure(StructuresData kitchenData)
		{
			_kitchenData = kitchenData;
			if (_items.Count == 0)
			{
				List<InventoryItem.ITEM_TYPE> list = new List<InventoryItem.ITEM_TYPE>();
				InventoryItem.ITEM_TYPE[] allMeals = CookingData.GetAllMeals();
				foreach (InventoryItem.ITEM_TYPE iTEM_TYPE in allMeals)
				{
					if ((iTEM_TYPE != InventoryItem.ITEM_TYPE.MEAL_POOP || DataManager.Instance.RecipesDiscovered.Contains(iTEM_TYPE)) && (iTEM_TYPE != InventoryItem.ITEM_TYPE.MEAL_GRASS || DataManager.Instance.RecipesDiscovered.Contains(iTEM_TYPE)))
					{
						if (CookingData.CanMakeMeal(iTEM_TYPE))
						{
							CookingData.TryDiscoverRecipe(iTEM_TYPE);
						}
						list.Add(iTEM_TYPE);
					}
				}
				list.Sort((InventoryItem.ITEM_TYPE a, InventoryItem.ITEM_TYPE b) => CookingData.HasRecipeDiscovered(b).CompareTo(CookingData.HasRecipeDiscovered(a)));
				foreach (InventoryItem.ITEM_TYPE item in list)
				{
					RecipeItem recipeItem = GameObjectExtensions.Instantiate(_recipeItemTemplate, _contentContainer);
					recipeItem.Configure(item);
					recipeItem.OnRecipeChosen = (Action<RecipeItem>)Delegate.Combine(recipeItem.OnRecipeChosen, new Action<RecipeItem>(OnRecipeClicked));
					_items.Add(recipeItem);
				}
			}
			else
			{
				UpdateQuantities();
			}
			_noRecipesText.SetActive(_items.Count == 0);
		}

		private void OnRecipeClicked(RecipeItem item)
		{
			if (_kitchenData.QueuedMeals.Count < RecipeLimit())
			{
				Action<InventoryItem.ITEM_TYPE> onRecipeChosen = OnRecipeChosen;
				if (onRecipeChosen != null)
				{
					onRecipeChosen(item.Type);
				}
			}
		}

		public void UpdateQuantities()
		{
			foreach (RecipeItem item in _items)
			{
				item.UpdateQuantity();
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

		private int RecipeLimit()
		{
			return 12 + ((_kitchenData.Type == StructureBrain.TYPES.KITCHEN_II) ? 5 : 0);
		}

		public int ReadilyAvailableMeals()
		{
			int num = 0;
			foreach (RecipeItem item in _items)
			{
				num += item.Quantity;
			}
			return num;
		}
	}
}
