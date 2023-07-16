using UnityEngine;

public class SingleChoiceRewardHeartShop : SingleChoiceRewardOption
{
	private void OnEnable()
	{
		if (DungeonSandboxManager.Active)
		{
			itemOptions.Clear();
			itemOptions.Add(new BuyEntry(InventoryItem.ITEM_TYPE.RED_HEART, InventoryItem.ITEM_TYPE.BLACK_GOLD, 1));
		}
		AllowDecorationAndSkin = true;
		switch (itemOptions[0].itemToBuy)
		{
		case InventoryItem.ITEM_TYPE.DOCTRINE_STONE:
			if (!DoctrineUpgradeSystem.TrySermonsStillAvailable() || !DoctrineUpgradeSystem.TryGetStillDoctrineStone())
			{
				if (Random.value < 0.5f)
				{
					itemOptions[0].itemToBuy = InventoryItem.ITEM_TYPE.GIFT_SMALL;
				}
				else
				{
					itemOptions[0].itemToBuy = InventoryItem.ITEM_TYPE.GIFT_MEDIUM;
				}
				itemOptions[0].quantity = 1;
			}
			break;
		case InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION:
			if (!DataManager.CheckAvailableDecorations())
			{
				if (Random.value > 0.75f)
				{
					itemOptions[0].itemToBuy = InventoryItem.ITEM_TYPE.RED_HEART;
				}
				else if (Random.value > 0.5f)
				{
					itemOptions[0].itemToBuy = InventoryItem.ITEM_TYPE.HALF_HEART;
				}
				else if (Random.value > 0.25f)
				{
					itemOptions[0].itemToBuy = InventoryItem.ITEM_TYPE.BLUE_HEART;
				}
				else
				{
					itemOptions[0].itemToBuy = InventoryItem.ITEM_TYPE.HALF_BLUE_HEART;
				}
				itemOptions[0].quantity = 1;
			}
			break;
		case InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN:
			if (!DataManager.CheckIfThereAreSkinsAvailable())
			{
				if (Random.value > 0.8f)
				{
					itemOptions[0].itemToBuy = InventoryItem.ITEM_TYPE.GOLD_REFINED;
					itemOptions[0].quantity = 5;
				}
				else if (Random.value > 0.4f)
				{
					itemOptions[0].itemToBuy = InventoryItem.ITEM_TYPE.BLACK_GOLD;
					itemOptions[0].quantity = 15;
				}
				else
				{
					itemOptions[0].itemToBuy = InventoryItem.ITEM_TYPE.GOLD_NUGGET;
					itemOptions[0].quantity = 25;
				}
			}
			break;
		case InventoryItem.ITEM_TYPE.RED_HEART:
		{
			float value = Random.value;
			if (value > 0.75f)
			{
				itemOptions[0].itemToBuy = InventoryItem.ITEM_TYPE.RED_HEART;
			}
			else if (value > 0.5f)
			{
				itemOptions[0].itemToBuy = InventoryItem.ITEM_TYPE.HALF_HEART;
			}
			else if (value > 0.25f)
			{
				itemOptions[0].itemToBuy = InventoryItem.ITEM_TYPE.BLUE_HEART;
			}
			else
			{
				itemOptions[0].itemToBuy = InventoryItem.ITEM_TYPE.HALF_BLUE_HEART;
			}
			break;
		}
		}
		if (itemOptions.Count <= 0 || itemOptions[0].itemToBuy == InventoryItem.ITEM_TYPE.NONE)
		{
			base.gameObject.SetActive(false);
		}
	}
}
