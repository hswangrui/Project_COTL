using Lamb.UI;
using UnityEngine.UI;

namespace src.UI.InfoCards
{
	public class FleeceInfoCardController : UIInfoCardController<FleeceInfoCard, int>
	{
		protected override bool IsSelectionValid(Selectable selectable, out int showParam)
		{
			showParam = 0;
			FleeceItem component;
			if (selectable.TryGetComponent<FleeceItem>(out component))
			{
				showParam = DataManager.Instance.PlayerFleece;
				return true;
			}
			FleeceItemBuyable component2;
			if (selectable.TryGetComponent<FleeceItemBuyable>(out component2))
			{
				showParam = component2.Fleece;
				return true;
			}
			return false;
		}

		protected override int DefaultShowParam()
		{
			return -1;
		}
	}
}
