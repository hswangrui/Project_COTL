using I2.Loc;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class UIUpgradeUnlockOverlayController : UIUpgradeUnlockOverlayControllerBase
	{
		[SerializeField]
		private TextMeshProUGUI _requirementsText;

		protected override void OnShowStarted()
		{
			if (_node.State != UpgradeTreeNode.NodeState.Unlocked && !IsAvailable())
			{
				_requirementsText.gameObject.SetActive(true);
				_requirementsText.text = string.Format(ScriptLocalization.UI_UpgradeTree.Requires, string.Join(" ", "1x", ScriptLocalization.UI.AbilityPoints)).Replace(":", string.Empty);
			}
			else
			{
				_requirementsText.gameObject.SetActive(false);
			}
			base.OnShowStarted();
		}

		protected override bool IsAvailable()
		{
			return UpgradeSystem.AbilityPoints > 0;
		}
	}
}
