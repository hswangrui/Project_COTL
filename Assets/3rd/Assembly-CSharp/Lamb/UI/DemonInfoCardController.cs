using UnityEngine.UI;

namespace Lamb.UI
{
	public class DemonInfoCardController : UIInfoCardController<DemonInfoCard, FollowerInfo>
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
