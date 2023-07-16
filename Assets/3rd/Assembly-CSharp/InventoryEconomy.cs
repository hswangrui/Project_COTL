using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryEconomy : BaseMonoBehaviour
{
	public List<InventoryItem.ITEM_TYPE> InventoryDisplay = new List<InventoryItem.ITEM_TYPE>
	{
		InventoryItem.ITEM_TYPE.SOUL,
		InventoryItem.ITEM_TYPE.SEEDS,
		InventoryItem.ITEM_TYPE.INGREDIENTS,
		InventoryItem.ITEM_TYPE.MEALS,
		InventoryItem.ITEM_TYPE.LOG,
		InventoryItem.ITEM_TYPE.STONE,
		InventoryItem.ITEM_TYPE.GRASS,
		InventoryItem.ITEM_TYPE.POOP,
		InventoryItem.ITEM_TYPE.BLACK_GOLD
	};

	public List<InventoryEconomyItem> EconomyItems = new List<InventoryEconomyItem>();

	public GameObject IconPrefab;

	public Transform Container;

	public UI_Transitions UITransitions;

	private bool populatedList;

	private bool FoundOne;

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
		ManualPopulate(Inventory.items);
		foreach (InventoryItem.ITEM_TYPE item in InventoryDisplay)
		{
			if (Inventory.GetItemQuantity((int)item) > 0)
			{
				FoundOne = true;
				break;
			}
		}
		if (!FoundOne)
		{
			UITransitions.hideBar();
			StartCoroutine(CheckResources());
		}
	}

	private void PopulateFromInventory()
	{
		ManualUpdateResource();
	}

	public void ManualPopulate(List<InventoryItem> Items)
	{
		foreach (InventoryItem.ITEM_TYPE item in InventoryDisplay)
		{
			GameObject obj = UnityEngine.Object.Instantiate(IconPrefab, Container);
			obj.SetActive(true);
			InventoryEconomyItem component = obj.GetComponent<InventoryEconomyItem>();
			component.Init(item);
			EconomyItems.Add(component);
		}
		IconPrefab.SetActive(false);
		populatedList = true;
	}

	public void ManualUpdateResource()
	{
		foreach (InventoryEconomyItem economyItem in EconomyItems)
		{
			economyItem.UpdateResources();
		}
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

	private IEnumerator CheckResources()
	{
		while (UITransitions.Hidden)
		{
			yield return new WaitForSeconds(1f);
			foreach (InventoryItem.ITEM_TYPE item in InventoryDisplay)
			{
				if (Inventory.GetItemQuantity((int)item) > 0)
				{
					UITransitions.StartCoroutine(UITransitions.MoveBarIn());
					break;
				}
			}
		}
	}
}
