using System.Collections.Generic;

namespace src.Extensions
{
	public static class InventoryExtensions
	{
		public static List<InventoryItem.ITEM_TYPE> TrimEmpty(this List<InventoryItem.ITEM_TYPE> list)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (Inventory.GetItemQuantity(list[num]) <= 0)
				{
					list.RemoveAt(num);
				}
			}
			return list;
		}
	}
}
