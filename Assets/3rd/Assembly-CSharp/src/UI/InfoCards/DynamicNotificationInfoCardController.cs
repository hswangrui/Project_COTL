using Lamb.UI;
using UnityEngine.UI;

namespace src.UI.InfoCards
{
	public class DynamicNotificationInfoCardController : UIInfoCardController<DynamicNotificationInfoCard, DynamicNotificationData>
	{
		protected override bool IsSelectionValid(Selectable selectable, out DynamicNotificationData showParam)
		{
			showParam = null;
			NotificationDynamicGeneric component;
			if (selectable.TryGetComponent<NotificationDynamicGeneric>(out component))
			{
				showParam = component.Data;
				return true;
			}
			return false;
		}
	}
}
