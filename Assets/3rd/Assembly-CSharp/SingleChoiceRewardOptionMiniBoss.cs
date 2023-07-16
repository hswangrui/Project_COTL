using System.Collections.Generic;
using UnityEngine;

public class SingleChoiceRewardOptionMiniBoss : SingleChoiceRewardOption
{
	public override List<BuyEntry> GetOptions()
	{
		return new List<BuyEntry>
		{
			new BuyEntry
			{
				itemToBuy = InventoryItem.ITEM_TYPE.BERRY,
				quantity = (15 + AdditionalPerDungeon()) * BeatenBishopMultiplier(),
				SingleQuantityItem = false
			},
			new BuyEntry
			{
				itemToBuy = InventoryItem.ITEM_TYPE.SEED,
				quantity = 7 + AdditionalPerDungeon(),
				SingleQuantityItem = false
			},
			new BuyEntry
			{
				itemToBuy = InventoryItem.ITEM_TYPE.LOG,
				quantity = (15 + AdditionalPerDungeon()) * BeatenBishopMultiplier(),
				SingleQuantityItem = false
			},
			new BuyEntry
			{
				itemToBuy = InventoryItem.ITEM_TYPE.STONE,
				quantity = (7 + AdditionalPerDungeon()) * BeatenBishopMultiplier(),
				SingleQuantityItem = false
			},
			new BuyEntry
			{
				itemToBuy = InventoryItem.ITEM_TYPE.GOLD_NUGGET,
				quantity = Random.Range(20, 30) + AdditionalPerDungeon(),
				SingleQuantityItem = false
			},
			new BuyEntry
			{
				itemToBuy = ((Random.value < 0.75f) ? DataManager.AllGifts[Random.Range(0, DataManager.AllGifts.Count)] : DataManager.AllNecklaces[Random.Range(0, 5)]),
				quantity = 1,
				GroupID = 1,
				SingleQuantityItem = true
			},
			new BuyEntry
			{
				itemToBuy = ((Random.value < 0.5f) ? InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN : InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION),
				quantity = 1,
				GroupID = 1,
				SingleQuantityItem = true
			}
		};
	}

	private int AdditionalPerDungeon()
	{
		switch (DataManager.Instance.BossesCompleted.Count)
		{
		case 1:
			return 2;
		case 2:
			return 4;
		case 3:
			return 6;
		case 4:
			return 8;
		case 5:
			return 10;
		default:
			return 0;
		}
	}

	private int BeatenBishopMultiplier()
	{
		if (!DataManager.Instance.DungeonCompleted(PlayerFarming.Location))
		{
			return 1;
		}
		return 2;
	}
}
