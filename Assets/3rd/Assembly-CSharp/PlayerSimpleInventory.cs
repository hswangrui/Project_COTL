public class PlayerSimpleInventory : SimpleInventory
{
	public override InventoryItem.ITEM_TYPE Item
	{
		get
		{
			return DataManager.Instance.SimpleInventoryItem;
		}
		set
		{
			inventoryitem = value;
			DataManager.Instance.SimpleInventoryItem = inventoryitem;
		}
	}

	private void Start()
	{
		ItemImage.SetImage(Item);
	}

	public void DepositItem()
	{
		Inventory.AddItem((int)Item, 1);
		RemoveItem();
	}
}
