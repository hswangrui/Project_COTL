using Lamb.UI.Rituals;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class RitualInfoCardController : UIInfoCardController<RitualInfoCard, UpgradeSystem.Type>
	{
		protected override bool IsSelectionValid(Selectable selectable, out UpgradeSystem.Type showParam)
		{
			showParam = UpgradeSystem.Type.Combat_ExtraHeart1;
			RitualItem component;
			NotificationDynamicRitual component2;
			if (selectable.TryGetComponent<RitualItem>(out component))
			{
				if (!component.Locked)
				{
					showParam = component.RitualType;
					return true;
				}
			}
			else if (selectable.TryGetComponent<NotificationDynamicRitual>(out component2))
			{
				showParam = component2.Ritual;
				return true;
			}
			return false;
		}

		protected override UpgradeSystem.Type DefaultShowParam()
		{
			return UpgradeSystem.Type.Ritual_Blank;
		}
	}
}
