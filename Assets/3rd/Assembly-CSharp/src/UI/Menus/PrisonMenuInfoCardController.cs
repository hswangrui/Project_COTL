using Lamb.UI;
using UnityEngine.UI;

namespace src.UI.Menus
{
	public class PrisonMenuInfoCardController : UIInfoCardController<PrisonInfoCard, FollowerInfo>
	{
		protected override bool IsSelectionValid(Selectable selectable, out FollowerInfo showParam)
		{
			showParam = null;
			FollowerInformationBox component;
			if (selectable.TryGetComponent<FollowerInformationBox>(out component))
			{
				showParam = component.FollowerInfo;
				return true;
			}
			return false;
		}
	}
}
