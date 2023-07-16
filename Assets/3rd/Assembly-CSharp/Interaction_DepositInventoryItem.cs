using System.Collections.Generic;
using UnityEngine;

public class Interaction_DepositInventoryItem : Interaction
{
	private Structure structure;

	private SimpleInventory PlayerInventory;

	public List<InventoryItem.ITEM_TYPE> AllowedItemTypes = new List<InventoryItem.ITEM_TYPE>();

	public void Start()
	{
		structure = GetComponent<Structure>();
	}

	public override void GetLabel()
	{
		base.Label = "";
		if (PlayerInventory == null)
		{
			GameObject gameObject = GameObject.FindWithTag("Player");
			if (gameObject != null)
			{
				PlayerInventory = gameObject.GetComponent<SimpleInventory>();
			}
		}
	}

	public override void OnInteract(StateMachine state)
	{
		SimpleInventory component = state.gameObject.GetComponent<SimpleInventory>();
		if (component != null && AllowedItemTypes.Contains(component.GetItemType()))
		{
			InventoryItem inventoryItem = new InventoryItem();
			inventoryItem.Init((int)component.GetItemType(), 1);
			structure.DepositInventory(inventoryItem);
			component.RemoveItem();
		}
		base.OnInteract(state);
	}
}
