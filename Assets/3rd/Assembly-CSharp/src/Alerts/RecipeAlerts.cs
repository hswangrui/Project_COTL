using System;

namespace src.Alerts
{
	public class RecipeAlerts : AlertCategory<InventoryItem.ITEM_TYPE>
	{
		public RecipeAlerts()
		{
			CookingData.OnRecipeDiscovered = (Action<InventoryItem.ITEM_TYPE>)Delegate.Combine(CookingData.OnRecipeDiscovered, new Action<InventoryItem.ITEM_TYPE>(OnRecipeDiscovered));
		}

		~RecipeAlerts()
		{
			CookingData.OnRecipeDiscovered = (Action<InventoryItem.ITEM_TYPE>)Delegate.Remove(CookingData.OnRecipeDiscovered, new Action<InventoryItem.ITEM_TYPE>(OnRecipeDiscovered));
		}

		private void OnRecipeDiscovered(InventoryItem.ITEM_TYPE recipe)
		{
			AddOnce(recipe);
		}
	}
}
