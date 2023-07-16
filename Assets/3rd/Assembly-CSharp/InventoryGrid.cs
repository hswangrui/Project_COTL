using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryGrid : BaseMonoBehaviour
{
	public GameObject IconPrefab;

	public Transform Container;

	private void OnEnable()
	{
		Inventory.OnInventoryUpdated = (Inventory.InventoryUpdated)Delegate.Combine(Inventory.OnInventoryUpdated, new Inventory.InventoryUpdated(PopulateFromInventory));
	}

	private void OnDisable()
	{
		Inventory.OnInventoryUpdated = (Inventory.InventoryUpdated)Delegate.Remove(Inventory.OnInventoryUpdated, new Inventory.InventoryUpdated(PopulateFromInventory));
	}

	private void Start()
	{
		PopulateFromInventory();
	}

	private void PopulateFromInventory()
	{
		Populate(Inventory.items);
	}

	private void FakePopulate()
	{
		Populate(new List<InventoryItem>
		{
			new InventoryItem(InventoryItem.ITEM_TYPE.BERRY),
			new InventoryItem(InventoryItem.ITEM_TYPE.FISH),
			new InventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD),
			new InventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD),
			new InventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD),
			new InventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD),
			new InventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD),
			new InventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD),
			new InventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD),
			new InventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD),
			new InventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD),
			new InventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD),
			new InventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD),
			new InventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD),
			new InventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD),
			new InventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD),
			new InventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD)
		});
	}

	public void Populate(List<InventoryItem> Items)
	{
		int num = Container.childCount;
		while (--num >= 0)
		{
			UnityEngine.Object.Destroy(Container.GetChild(num).gameObject);
		}
		foreach (InventoryItem Item in Items)
		{
			GameObject obj = UnityEngine.Object.Instantiate(IconPrefab, Container);
			obj.SetActive(true);
			obj.GetComponent<HUD_InventoryIcon>().InitFromType((InventoryItem.ITEM_TYPE)Item.type);
		}
	}
}
