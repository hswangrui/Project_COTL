using UnityEngine.UI;

namespace Lamb.UI
{
	public class IngredientInfoCardController : UIInfoCardController<IngredientInfoCard, InventoryItem.ITEM_TYPE>
	{
		protected override bool IsSelectionValid(Selectable selectable, out InventoryItem.ITEM_TYPE showParam)
		{
			showParam = InventoryItem.ITEM_TYPE.NONE;
			IngredientItem component;
			if (selectable.TryGetComponent<IngredientItem>(out component))
			{
				showParam = component.Type;
				return true;
			}
			return false;
		}
	}
}
