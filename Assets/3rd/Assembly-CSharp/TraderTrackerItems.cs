using System;
using UnityEngine;

[Serializable]
public class TraderTrackerItems
{
	public InventoryItem.ITEM_TYPE itemForTrade;

	public int SellPrice;

	public int BuyPrice;

	public int BuyOffsetPercent = 20;

	public int BuyOffset;

	public int SellOffset;

	public int LastDayChecked;

	public int BuyPriceActual
	{
		get
		{
			return (int)Mathf.Clamp((float)BuyPrice - (float)BuyPrice * ((float)BuyOffset / 100f), 1f, BuyPrice);
		}
	}

	public int SellPriceActual
	{
		get
		{
			return SellPrice + SellOffset;
		}
	}
}
