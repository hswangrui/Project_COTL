using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using TMPro;
using UnityEngine;

namespace src.UI.InfoCards
{
	public class RecipeInfoCard : UIInfoCardBase<InventoryItem.ITEM_TYPE>
	{
		[SerializeField]
		private RecipeItem _recipeItem;

		[SerializeField]
		private TextMeshProUGUI _itemHeader;

		[SerializeField]
		private TextMeshProUGUI _itemDescription;

		[SerializeField]
		private TextMeshProUGUI _mealEffects;

		[SerializeField]
		private GameObject[] _starFills;

		[SerializeField]
		private TextMeshProUGUI[] _ingredients;

		[SerializeField]
		private HungerRecipeController _hungerRecipeController;

		[SerializeField]
		private TextMeshProUGUI _hungerDelta;

		[SerializeField]
		private TextMeshProUGUI _categoryIcon;

		[SerializeField]
		private TextMeshProUGUI _followerCount;

		public HungerRecipeController HungerRecipeController
		{
			get
			{
				return _hungerRecipeController;
			}
		}

		public override void Configure(InventoryItem.ITEM_TYPE config)
		{
			_recipeItem.Configure(config, false);
			_itemHeader.text = LocalizationManager.GetTranslation(string.Format("CookingData/{0}/Name", config));
			_itemDescription.text = LocalizationManager.GetTranslation(string.Format("CookingData/{0}/Description", config));
			CookingData.MealEffect[] mealEffects = CookingData.GetMealEffects(config);
			if (mealEffects.Length == 0)
			{
				_mealEffects.gameObject.SetActive(false);
			}
			else
			{
				_mealEffects.text = "";
				_mealEffects.gameObject.SetActive(true);
				CookingData.MealEffect[] array = mealEffects;
				foreach (CookingData.MealEffect mealEffect in array)
				{
					_mealEffects.text += CookingData.GetEffectDescription(mealEffect, config);
					_mealEffects.text += "\n";
				}
			}
			ThoughtData thoughtData = CookingData.ThoughtDataForMeal(config);
			if (thoughtData.Modifier != 0f)
			{
				bool flag = thoughtData.Modifier > 0f;
				_mealEffects.text += "\n";
				TextMeshProUGUI mealEffects2 = _mealEffects;
				mealEffects2.text = mealEffects2.text + (flag ? "<sprite name=\"icon_FaithUp\">" : "<sprite name=\"icon_FaithDown\">") + ((flag ? "+" : "-") + Mathf.Abs(thoughtData.Modifier)).Colour(flag ? StaticColors.GreenColor : StaticColors.RedColor) + " " + ScriptLocalization.UI_PrisonMenu.Faith;
			}
			int satationLevel = CookingData.GetSatationLevel(config);
			for (int j = 0; j < _starFills.Length; j++)
			{
				_starFills[j].SetActive(satationLevel >= j + 1);
			}
			_ingredients[0].text = "";
			foreach (List<InventoryItem> item in CookingData.GetRecipe(config))
			{
				foreach (InventoryItem item2 in item)
				{
					StructuresData.ItemCost itemCost = new StructuresData.ItemCost((InventoryItem.ITEM_TYPE)item2.type, item2.quantity);
					_ingredients[0].text += itemCost.ToStringShowQuantity();
					_ingredients[0].text += "    ";
					_ingredients[0].gameObject.SetActive(true);
				}
				_ingredients[0].text += "\n \n";
			}
			float num = Mathf.Round(_hungerRecipeController.GetDelta(config));
			if (Mathf.Abs(num) > 0f)
			{
				Color colour = ((num > 0f) ? StaticColors.GreenColor : StaticColors.RedColor);
				_hungerDelta.text = ((num > 0f) ? "<sprite name=\"icon_FaithUp\">" : "<sprite name=\"icon_FaithDown\">") + Mathf.Abs(num).ToString().Colour(colour);
			}
			else
			{
				_hungerDelta.text = string.Empty;
			}
			_categoryIcon.text = "";
			_followerCount.text = "<sprite name=\"icon_Followers\">" + DataManager.Instance.Followers.Count;
		}
	}
}
