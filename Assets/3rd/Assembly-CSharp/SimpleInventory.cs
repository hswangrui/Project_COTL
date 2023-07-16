using System.Collections.Generic;

public class SimpleInventory : BaseMonoBehaviour
{
	public InventoryItemDisplay ItemImage;

	public InventoryItem.ITEM_TYPE inventoryitem;

	private static List<SimpleInventory> simpleInventories = new List<SimpleInventory>();

	private static int _TotalItemsActive;

	public virtual InventoryItem.ITEM_TYPE Item
	{
		get
		{
			return inventoryitem;
		}
		set
		{
			inventoryitem = value;
			ItemImage.SetImage(Item);
		}
	}

	public static int TotalItemsActive
	{
		get
		{
			_TotalItemsActive = 0;
			foreach (SimpleInventory simpleInventory in simpleInventories)
			{
				if (simpleInventory.Item != 0)
				{
					_TotalItemsActive++;
				}
			}
			return _TotalItemsActive;
		}
	}

	private void OnEnable()
	{
		simpleInventories.Add(this);
	}

	private void OnDisable()
	{
		simpleInventories.Remove(this);
	}

	private void Start()
	{
		Item = InventoryItem.ITEM_TYPE.NONE;
		ItemImage.SetImage(Item);
	}

	public InventoryItem.ITEM_TYPE GetItemType()
	{
		return Item;
	}

	public void GiveItem(InventoryItem.ITEM_TYPE ItemType)
	{
		DropItem();
		Item = ItemType;
		ItemImage.SetImage(Item);
	}

	public void RemoveItem()
	{
		Item = InventoryItem.ITEM_TYPE.NONE;
		ItemImage.SetImage(Item);
	}

	public void DropItem()
	{
		if (Item != 0)
		{
			InventoryItem.Spawn(Item, 1, ItemImage.transform.position);
			RemoveItem();
		}
	}
}
