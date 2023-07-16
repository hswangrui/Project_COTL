namespace Lamb.UI.Alerts
{
	public class InventoryAlert : AlertBadge<InventoryItem.ITEM_TYPE>
	{
		protected override AlertCategory<InventoryItem.ITEM_TYPE> _source
		{
			get
			{
				return DataManager.Instance.Alerts.Inventory;
			}
		}
	}
}
