using System;
using System.Collections.Generic;

[Serializable]
public class ShopLocationTracker
{
	public string shopKeeperName = "";

	public int lastDayUsed = 1;

	public FollowerLocation location = FollowerLocation.None;

	public List<BuyEntry> itemsForSale = new List<BuyEntry>();

	public ShopLocationTracker()
	{
	}

	public ShopLocationTracker(FollowerLocation Location)
	{
		location = Location;
	}

	public ShopLocationTracker(FollowerLocation Location, int LastDayUsed, List<BuyEntry> ItemsForSale, string ShopKeeperName)
	{
		shopKeeperName = ShopKeeperName;
		lastDayUsed = LastDayUsed;
		location = Location;
		itemsForSale = ItemsForSale;
	}
}
