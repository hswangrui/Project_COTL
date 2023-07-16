using Lamb.UI;
using UnityEngine.UI;

namespace src.UI.InfoCards
{
	public class CrownAbilityInfoCardController : UIInfoCardController<CrownAbilityInfoCard, UpgradeSystem.Type>
	{
		protected override bool IsSelectionValid(Selectable selectable, out UpgradeSystem.Type showParam)
		{
			showParam = UpgradeSystem.Type.Combat_ExtraHeart1;
			CrownAbilityItem component;
			if (selectable.TryGetComponent<CrownAbilityItem>(out component))
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
