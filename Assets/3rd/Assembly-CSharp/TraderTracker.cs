using System;
using System.Collections.Generic;

[Serializable]
public class TraderTracker
{
	public FollowerLocation location = FollowerLocation.None;

	public List<TraderTrackerItems> itemsToTrade = new List<TraderTrackerItems>();

	public string traderName = "";

	public List<InventoryItem.ITEM_TYPE> itemsForSale = new List<InventoryItem.ITEM_TYPE>();

	public List<InventoryItem.ITEM_TYPE> blackList = new List<InventoryItem.ITEM_TYPE>();

	private bool inBlackList;

	public void GetItemsForSale()
	{
		itemsForSale.Clear();
		foreach (TraderTrackerItems item in itemsToTrade)
		{
			inBlackList = false;
			if (blackList.Count > 0)
			{
				foreach (InventoryItem.ITEM_TYPE black in blackList)
				{
					if (item.itemForTrade == black)
					{
						inBlackList = true;
					}
				}
			}
			if (!inBlackList)
			{
				itemsForSale.Add(item.itemForTrade);
			}
		}
	}
}
