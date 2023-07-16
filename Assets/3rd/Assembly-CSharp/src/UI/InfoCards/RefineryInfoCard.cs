using I2.Loc;
using Lamb.UI;
using Lamb.UI.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace src.UI.InfoCards
{
	public class RefineryInfoCard : UIInfoCardBase<InventoryItem.ITEM_TYPE>
	{
		[SerializeField]
		private TextMeshProUGUI _headerText;

		[SerializeField]
		private TextMeshProUGUI _loreText;

		[SerializeField]
		private TextMeshProUGUI _descriptionText;

		[SerializeField]
		private Image _itemBefore;

		[SerializeField]
		private TextMeshProUGUI _itemBeforeQuantity;

		[SerializeField]
		private Image _itemAfter;

		[SerializeField]
		private TextMeshProUGUI _itemAfterQuantity;

		[SerializeField]
		private TextMeshProUGUI _costText;

		[SerializeField]
		private TextMeshProUGUI _acquisitionText;

		[SerializeField]
		private InventoryIconMapping _iconMapping;

		public override void Configure(InventoryItem.ITEM_TYPE config)
		{
			StructuresData.ItemCost itemCost = Structures_Refinery.GetCost(config)[0];
			StructuresData.ItemCost itemCost2 = new StructuresData.ItemCost(config, Structures_Refinery.GetAmount(config));
			_headerText.text = LocalizationManager.GetTranslation(string.Format("Inventory/{0}", config));
			_descriptionText.text = LocalizationManager.GetTranslation(string.Format("Inventory/{0}/Description", config));
			_itemBefore.sprite = _iconMapping.GetImage(itemCost.CostItem);
			_itemBeforeQuantity.text = itemCost.CostValue.ToString();
			_itemAfter.sprite = _iconMapping.GetImage(config);
			_itemAfterQuantity.text = Structures_Refinery.GetAmount(config).ToString();
			_costText.text = StructuresData.ItemCost.GetCostStringWithQuantity(itemCost);
			_acquisitionText.text = StructuresData.ItemCost.GetCostStringWithQuantity(itemCost2);
		}
	}
}
