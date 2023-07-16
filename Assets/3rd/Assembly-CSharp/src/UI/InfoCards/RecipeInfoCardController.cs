using Lamb.UI;
using UnityEngine.UI;

namespace src.UI.InfoCards
{
	public class RecipeInfoCardController : UIInfoCardController<RecipeInfoCard, InventoryItem.ITEM_TYPE>
	{
		protected override bool IsSelectionValid(Selectable selectable, out InventoryItem.ITEM_TYPE showParam)
		{
			showParam = InventoryItem.ITEM_TYPE.NONE;
			RecipeItem component;
			if (selectable.TryGetComponent<RecipeItem>(out component) && CookingData.HasRecipeDiscovered(component.Type))
			{
				showParam = component.Type;
				return true;
			}
			FinalizeRecipeButton component2;
			if (selectable.TryGetComponent<FinalizeRecipeButton>(out component2))
			{
				showParam = component2.Recipe;
				return showParam != InventoryItem.ITEM_TYPE.NONE;
			}
			return false;
		}
	}
}
