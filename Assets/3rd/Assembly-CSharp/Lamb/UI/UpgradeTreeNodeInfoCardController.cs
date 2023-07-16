using UnityEngine.UI;

namespace Lamb.UI
{
	public class UpgradeTreeNodeInfoCardController : UIInfoCardController<UpgradeTreeNodeInfoCard, UpgradeTreeNode>
	{
		protected override bool IsSelectionValid(Selectable selectable, out UpgradeTreeNode showParam)
		{
			if (selectable.TryGetComponent<UpgradeTreeNode>(out showParam))
			{
				return true;
			}
			return false;
		}
	}
}
