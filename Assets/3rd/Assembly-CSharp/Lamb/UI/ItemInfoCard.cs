using I2.Loc;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class ItemInfoCard : UIInfoCardBase<InventoryItem.ITEM_TYPE>
	{
		[SerializeField]
		private GenericInventoryItem _inventoryIcon;

		[SerializeField]
		private TextMeshProUGUI _itemHeader;

		[SerializeField]
		private TextMeshProUGUI _itemLore;

		[SerializeField]
		private TextMeshProUGUI _itemDescription;

		public override void Configure(InventoryItem.ITEM_TYPE config)
		{
			_inventoryIcon.Configure(config, false);
			_itemHeader.text = LocalizationManager.GetTranslation(string.Format("Inventory/{0}", config));
			_itemLore.text = LocalizationManager.GetTranslation(string.Format("Inventory/{0}/Lore", config));
			_itemDescription.text = LocalizationManager.GetTranslation(string.Format("Inventory/{0}/Description", config));
		}
	}
}
