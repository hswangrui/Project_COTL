using System.Collections.Generic;

public class HUDDungeonInventory : BaseMonoBehaviour
{
	public List<HUD_InventoryIcon> Icons = new List<HUD_InventoryIcon>();

	private void OnEnable()
	{
		PopulateInventory();
		Inventory.OnItemAddedToDungeonInventory += ItemAddedToInventory;
	}

	private void OnDisable()
	{
		Inventory.OnItemAddedToDungeonInventory -= ItemAddedToInventory;
	}

	private void ItemAddedToInventory(InventoryItem.ITEM_TYPE ItemType)
	{
		PopulateInventory();
	}

	private void PopulateInventory()
	{
		int num = -1;
		while (++num < Icons.Count)
		{
			if (num > Inventory.itemsDungeon.Count - 1)
			{
				Icons[num].gameObject.SetActive(false);
				continue;
			}
			Icons[num].gameObject.SetActive(true);
			Icons[num].Init(Inventory.itemsDungeon[num]);
		}
	}
}
