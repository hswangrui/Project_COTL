using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class UpgradeInfoCard : UIInfoCardBase<UpgradeSystem.Type>
	{
		[Header("Copy")]
		[SerializeField]
		private TextMeshProUGUI _headerText;

		[SerializeField]
		private TextMeshProUGUI _descriptionText;

		[Header("Costs")]
		[SerializeField]
		private GameObject _costHeader;

		[SerializeField]
		private TextMeshProUGUI _costText;

		[Header("Graphics")]
		[SerializeField]
		private Image _icon;

		[SerializeField]
		private SpriteAtlas _atlas;

		public override void Configure(UpgradeSystem.Type config)
		{
			_icon.sprite = _atlas.GetSprite(config.ToString());
			_headerText.text = UpgradeSystem.GetLocalizedName(config);
			_descriptionText.text = UpgradeSystem.GetLocalizedDescription(config);
			if (UpgradeSystem.GetUnlocked(config))
			{
				_costHeader.SetActive(false);
				_costText.text = "";
			}
			else
			{
				_costText.text = StructuresData.ItemCost.GetCostStringWithQuantity(UpgradeSystem.GetCost(config));
			}
		}
	}
}
