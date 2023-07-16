using UnityEngine.UI;

namespace Lamb.UI
{
	public class UpgradeInfoCardController : UIInfoCardController<UpgradeInfoCard, UpgradeSystem.Type>
	{
		protected override bool IsSelectionValid(Selectable selectable, out UpgradeSystem.Type showParam)
		{
			showParam = UpgradeSystem.Type.Combat_ExtraHeart1;
			UpgradeShopItem component;
			if (selectable.TryGetComponent<UpgradeShopItem>(out component))
			{
				showParam = component.Type;
				return true;
			}
			CrownAbilityItemBuyable component2;
			if (selectable.TryGetComponent<CrownAbilityItemBuyable>(out component2))
			{
				showParam = component2.Type;
				return true;
			}
			return false;
		}
	}
}
