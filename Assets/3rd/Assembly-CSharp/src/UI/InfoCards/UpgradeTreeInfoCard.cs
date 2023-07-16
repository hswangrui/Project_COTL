using I2.Loc;
using Lamb.UI;
using Lamb.UI.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace src.UI.InfoCards
{
	public class UpgradeTreeInfoCard : UIInfoCardBase<UpgradeTreeNode>
	{
		[Header("Copy")]
		[SerializeField]
		private TextMeshProUGUI _headerText;

		[SerializeField]
		private TextMeshProUGUI _descriptionText;

		[SerializeField]
		private TextMeshProUGUI _requirementsText;

		[Header("Upgrade Category")]
		[SerializeField]
		private TextMeshProUGUI _categoryText;

		[SerializeField]
		private UpgradeCategoryIconMapping _categoryIconMapping;

		[Header("Costs")]
		[Header("Graphics")]
		[SerializeField]
		private UpgradeTypeMapping _upgradeTypeMapping;

		[SerializeField]
		private Image _icon;

		public override void Configure(UpgradeTreeNode node)
		{
			UpgradeSystem.Type upgrade = node.Upgrade;
			_icon.sprite = _upgradeTypeMapping.GetItem(upgrade).Sprite;
			_headerText.text = UpgradeSystem.GetLocalizedName(upgrade);
			_descriptionText.text = UpgradeSystem.GetLocalizedDescription(upgrade);
			if (_upgradeTypeMapping != null)
			{
				UpgradeTypeMapping.SpriteItem item = _upgradeTypeMapping.GetItem(node.Upgrade);
				if (_categoryText != null && _categoryIconMapping != null)
				{
					_categoryText.text = UpgradeCategoryIconMapping.GetIcon(item.Category);
					_categoryText.color = _categoryIconMapping.GetColor(item.Category);
				}
			}
			if (node.State == UpgradeTreeNode.NodeState.Locked)
			{
				if (DataManager.Instance.CurrentUpgradeTreeTier <= node.NodeTier || !node.TierConfig.RequiresCentralTier || node.TierConfig.CentralNode != node.Upgrade || node.TreeConfig.NumUnlockedUpgrades() < node.TreeConfig.NumRequiredNodesForTier(node.NodeTier))
				{
					_requirementsText.text = string.Format(LocalizationManager.GetTranslation("UI/UpgradeTree/RequiredTier"), (int)(node.NodeTier + 1)).Colour(Color.red);
				}
				else if (node.RequiresBuiltStructure != 0)
				{
					_requirementsText.text = string.Format(LocalizationManager.GetTranslation("UI/UpgradeTree/Requires"), StructuresData.LocalizedName(node.RequiresBuiltStructure)).Colour(Color.red);
				}
				else
				{
					_requirementsText.gameObject.SetActive(false);
				}
			}
			else if (node.State == UpgradeTreeNode.NodeState.Unavailable)
			{
				string text = "";
				UpgradeTreeNode[] prerequisiteNodes = node.PrerequisiteNodes;
				foreach (UpgradeTreeNode upgradeTreeNode in prerequisiteNodes)
				{
					text = text + "\n" + UpgradeSystem.GetLocalizedName(upgradeTreeNode.Upgrade);
				}
				_requirementsText.text = string.Format(LocalizationManager.GetTranslation("UI/UpgradeTree/Requires"), text).Colour(Color.red);
			}
			else
			{
				_requirementsText.gameObject.SetActive(false);
			}
		}
	}
}
