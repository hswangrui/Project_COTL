using System;
using UnityEngine;

public class Structures_FoodStorage : StructureBrain
{
	public Action OnFoodWithdrawn;

	public int Capacity
	{
		get
		{
			return 10;
		}
	}

	public Structures_FoodStorage(int level)
	{
	}

	public int GetUnreservedItemCount()
	{
		int num = 0;
		foreach (InventoryItem item in Data.Inventory)
		{
			Debug.Log(string.Concat("item: ", (InventoryItem.ITEM_TYPE)item.type, "   ", item.quantity, "   ", item.UnreservedQuantity));
			num += item.UnreservedQuantity;
		}
		return num;
	}

	public bool TryClaimFoodReservation(out InventoryItem.ITEM_TYPE itemType)
	{
		foreach (InventoryItem item in Data.Inventory)
		{
			Debug.Log((InventoryItem.ITEM_TYPE)item.type);
			if (item.UnreservedQuantity > 0)
			{
				item.QuantityReserved++;
				itemType = (InventoryItem.ITEM_TYPE)item.type;
				return true;
			}
		}
		itemType = InventoryItem.ITEM_TYPE.NONE;
		return false;
	}

	public bool TryClaimFoodReservation(InventoryItem.ITEM_TYPE itemType)
	{
		foreach (InventoryItem item in Data.Inventory)
		{
			if (item.type == (int)itemType && item.UnreservedQuantity > 0)
			{
				item.QuantityReserved++;
				itemType = (InventoryItem.ITEM_TYPE)item.type;
				return true;
			}
		}
		return false;
	}

	public void ReleaseFoodReservation(InventoryItem.ITEM_TYPE itemType)
	{
		bool flag = false;
		foreach (InventoryItem item in Data.Inventory)
		{
			if (item.type == (int)itemType && item.QuantityReserved > 0)
			{
				item.QuantityReserved--;
				flag = true;
				break;
			}
		}
	}

	public bool TryEatReservedFood(InventoryItem.ITEM_TYPE itemType)
	{
		for (int num = Data.Inventory.Count - 1; num >= 0; num--)
		{
			if (Data.Inventory[num].type == (int)itemType && Data.Inventory[num].quantity > 0 && Data.Inventory[num].QuantityReserved > 0)
			{
				Data.Inventory[num].QuantityReserved--;
				Data.Inventory[num].quantity--;
				if (Data.Inventory[num].quantity == 0)
				{
					Data.Inventory.RemoveAt(num);
				}
				Action onFoodWithdrawn = OnFoodWithdrawn;
				if (onFoodWithdrawn != null)
				{
					onFoodWithdrawn();
				}
				return true;
			}
		}
		return false;
	}
}
