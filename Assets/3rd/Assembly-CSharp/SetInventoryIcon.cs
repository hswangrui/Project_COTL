public class SetInventoryIcon : BaseMonoBehaviour
{
	public Inventory_Icon inventoryIcon;

	public InventoryItem.ITEM_TYPE Item;

	private void Start()
	{
		InventoryItem itemByType = Inventory.GetItemByType((int)Item);
		if (itemByType == null)
		{
			inventoryIcon.SetImage((int)Item, 0);
		}
		else
		{
			inventoryIcon.SetItem(itemByType);
		}
	}
}
