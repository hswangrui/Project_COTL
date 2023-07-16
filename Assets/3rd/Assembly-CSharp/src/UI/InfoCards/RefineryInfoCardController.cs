using Lamb.UI;
using UnityEngine.UI;

namespace src.UI.InfoCards
{
	public class RefineryInfoCardController : UIInfoCardController<RefineryInfoCard, InventoryItem.ITEM_TYPE>
	{
		protected override bool IsSelectionValid(Selectable selectable, out InventoryItem.ITEM_TYPE showParam)
		{
			showParam = InventoryItem.ITEM_TYPE.NONE;
			RefineryItem component;
			if (selectable.TryGetComponent<RefineryItem>(out component))
			{
				showParam = component.Type;
				return true;
			}
			return false;
		}
	}
}
