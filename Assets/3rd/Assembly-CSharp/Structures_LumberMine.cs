public class Structures_LumberMine : StructureBrain
{
	public int RemainingLumber
	{
		get
		{
			InventoryItem inventoryItem = FindLogItem();
			if (inventoryItem != null)
			{
				return inventoryItem.quantity;
			}
			return 0;
		}
	}

	private InventoryItem FindLogItem()
	{
		InventoryItem result = null;
		foreach (InventoryItem item in Data.Inventory)
		{
			if (item.type == 1)
			{
				result = item;
				break;
			}
		}
		return result;
	}

	public bool TryTakeLumber()
	{
		bool result = false;
		InventoryItem inventoryItem = FindLogItem();
		if (inventoryItem.quantity > 0)
		{
			inventoryItem.quantity--;
			result = true;
		}
		return result;
	}
}
