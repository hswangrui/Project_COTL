namespace Lamb.UI
{
	public class HistoricalNotificationItem : HistoricalNotificationBase<FinalizedItemNotification>
	{
		protected override void ConfigureImpl(FinalizedItemNotification finalizedNotification)
		{
		}

		protected override string GetLocalizedDescription(FinalizedItemNotification finalizedNotification)
		{
			float num = finalizedNotification.ItemDelta;
			InventoryItem.ITEM_TYPE itemType = finalizedNotification.ItemType;
			return FontImageNames.GetIconByType(itemType) + " " + ((num > 0f) ? "+" : "") + num + InventoryItem.LocalizedName(itemType) + " <color=#6E6666>" + Inventory.GetItemQuantity((int)num) + "</color>";
		}
	}
}
