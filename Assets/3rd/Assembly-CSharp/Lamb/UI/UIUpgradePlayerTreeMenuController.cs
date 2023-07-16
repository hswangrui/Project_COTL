using UnityEngine.UI;

namespace Lamb.UI
{
	public class UIUpgradePlayerTreeMenuController : UIUpgradeTreeMenuBase<UIPlayerUpgradeUnlockOverlayController>
	{
		protected override Selectable GetDefaultSelectable()
		{
			foreach (UpgradeTreeNode treeNode in _treeNodes)
			{
				if (treeNode.Upgrade == DataManager.Instance.MostRecentPlayerTreeUpgrade)
				{
					return treeNode.Button;
				}
			}
			return null;
		}

		protected override int UpgradePoints()
		{
			return UpgradeSystem.DisciplePoints;
		}

		protected override string GetPointsText()
		{
			return "";
		}

		protected override void DoUnlock(UpgradeSystem.Type upgrade)
		{
			UpgradeSystem.UnlockAbility(upgrade);
			DataManager.Instance.MostRecentPlayerTreeUpgrade = upgrade;
			DataManager.Instance.LastDaySincePlayerUpgrade = TimeManager.CurrentDay;
			_didUpgraded = true;
		}

		protected override void DoRelease()
		{
		}

		protected override UpgradeTreeNode.TreeTier TreeTier()
		{
			return DataManager.Instance.CurrentPlayerUpgradeTreeTier;
		}

		protected override void UpdateTier(UpgradeTreeNode.TreeTier tier)
		{
			DataManager.Instance.CurrentPlayerUpgradeTreeTier = tier;
		}

		public override void OnCancelButtonInput()
		{
		}

		public override void Awake()
		{
			disableBackPrompt = base.gameObject.transform.Find("UpgradeTreeMenuContainer/Control Prompts/Back Prompt").gameObject;
			disableBackPrompt.SetActive(false);
			base.Awake();
		}
	}
}
