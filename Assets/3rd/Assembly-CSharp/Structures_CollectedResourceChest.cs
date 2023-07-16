using System;

public class Structures_CollectedResourceChest : StructureBrain
{
	public void AddItem(InventoryItem.ITEM_TYPE ItemType, int Quantity)
	{
		foreach (InventoryItem item in Data.Inventory)
		{
			if (item.type == (int)ItemType)
			{
				item.quantity += Quantity;
				return;
			}
		}
		Data.Inventory.Add(new InventoryItem(ItemType, Quantity));
		Action onItemDeposited = OnItemDeposited;
		if (onItemDeposited != null)
		{
			onItemDeposited();
		}
	}
}
