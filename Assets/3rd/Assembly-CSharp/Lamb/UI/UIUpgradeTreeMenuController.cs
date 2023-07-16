using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class UIUpgradeTreeMenuController : UIUpgradeTreeMenuBase<UIUpgradeUnlockOverlayController>
	{
		[SerializeField]
		private Image _divineInspirationBackground;

		protected override Selectable GetDefaultSelectable()
		{
			foreach (UpgradeTreeNode treeNode in _treeNodes)
			{
				if (treeNode.Upgrade == DataManager.Instance.MostRecentTreeUpgrade)
				{
					return treeNode.Button;
				}
			}
			return null;
		}

		protected override int UpgradePoints()
		{
			return UpgradeSystem.AbilityPoints;
		}

		protected override string GetPointsText()
		{
			return UpgradeSystem.AbilityPoints.ToString();
		}

		protected override void UpdatePointsText()
		{
			base.UpdatePointsText();
			if (UpgradePoints() <= 0)
			{
				_divineInspirationBackground.color = "#FD1D03".ColourFromHex();
				_pointsText.color = StaticColors.OffWhiteColor;
			}
		}

		protected override void DoRelease()
		{
		}

		protected override void DoUnlock(UpgradeSystem.Type upgrade)
		{
			UpgradeSystem.UnlockAbility(upgrade);
			UpgradeSystem.AbilityPoints--;
			DataManager.Instance.MostRecentTreeUpgrade = upgrade;
		}

		protected override UpgradeTreeNode.TreeTier TreeTier()
		{
			return DataManager.Instance.CurrentUpgradeTreeTier;
		}

		protected override void UpdateTier(UpgradeTreeNode.TreeTier tier)
		{
			DataManager.Instance.CurrentUpgradeTreeTier = tier;
		}

		protected override void OnUnlockAnimationCompleted()
		{
			base.OnUnlockAnimationCompleted();
			_cursor.LockPosition = false;
		}
	}
}
