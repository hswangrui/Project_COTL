using I2.Loc;
using Lamb.UI;
using TMPro;
using UnityEngine;

namespace src.UI.InfoCards
{
	public class CrownAbilityInfoCard : UIInfoCardBase<UpgradeSystem.Type>
	{
		[SerializeField]
		private bool _ignoreLockedState;

		[SerializeField]
		private bool _showCost;

		[SerializeField]
		private TextMeshProUGUI _headerText;

		[SerializeField]
		private TextMeshProUGUI _descriptionText;

		[SerializeField]
		private CrownAbilityItem _crownAbilityItem;

		[SerializeField]
		private GameObject _costHeader;

		[SerializeField]
		private GameObject _costContainer;

		[SerializeField]
		private TextMeshProUGUI _costText;

		public GameObject _redOutline;

		public override void Configure(UpgradeSystem.Type type)
		{
			_redOutline.SetActive(false);
			_crownAbilityItem.Configure(type);
			_costText.text = StructuresData.ItemCost.GetCostStringWithQuantity(UpgradeSystem.GetCost(type));
			if (UpgradeSystem.GetUnlocked(type) || _ignoreLockedState)
			{
				_headerText.text = UpgradeSystem.GetLocalizedName(type);
				_descriptionText.text = UpgradeSystem.GetLocalizedDescription(type);
				_costHeader.SetActive(_ignoreLockedState && _showCost && !UpgradeSystem.GetUnlocked(type));
				_costContainer.SetActive(_ignoreLockedState && _showCost && !UpgradeSystem.GetUnlocked(type));
				return;
			}
			_headerText.text = LocalizationManager.GetTranslation("UI/PauseScreen/Player/AbilityLocked");
			_descriptionText.text = LocalizationManager.GetTranslation("UI/PauseScreen/Player/AbilityLocked/Description");
			_costText.text = StructuresData.ItemCost.GetCostStringWithQuantity(UpgradeSystem.GetCost(type));
			if (_showCost)
			{
				_costHeader.SetActive(true);
				_costContainer.SetActive(true);
			}
		}
	}
}
