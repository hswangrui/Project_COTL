using UnityEngine.UI;

namespace Lamb.UI
{
	public class ItemInfoCardController : UIInfoCardController<ItemInfoCard, InventoryItem.ITEM_TYPE>
	{
		protected override bool IsSelectionValid(Selectable selectable, out InventoryItem.ITEM_TYPE showParam)
		{
			showParam = InventoryItem.ITEM_TYPE.NONE;
			GenericInventoryItem component;
			if (selectable.TryGetComponent<GenericInventoryItem>(out component))
			{
				showParam = component.Type;
				return true;
			}
			return false;
		}
	}
}
