using System.Collections.Generic;
using UnityEngine;

public class Interaction_FixBridge : Interaction
{
	public BaseBridge baseBridge;

	private Structure structure;

	private SimpleInventory PlayerInventory;

	public List<InventoryItem.ITEM_TYPE> AllowedItemTypes = new List<InventoryItem.ITEM_TYPE>();

	private void Start()
	{
		structure = GetComponent<Structure>();
	}

	public override void GetLabel()
	{
		if (structure.Inventory.Count < 3)
		{
			HoldToInteract = false;
			Interactable = false;
			if (PlayerInventory == null)
			{
				GameObject gameObject = GameObject.FindWithTag("Player");
				if (gameObject != null)
				{
					PlayerInventory = gameObject.GetComponent<SimpleInventory>();
				}
			}
			else if (AllowedItemTypes.Contains(PlayerInventory.GetItemType()))
			{
				Interactable = true;
			}
		}
		else
		{
			Interactable = true;
			HoldToInteract = true;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (base.Label == "Fix Bridge")
		{
			base.OnInteract(state);
			return;
		}
		SimpleInventory component = state.gameObject.GetComponent<SimpleInventory>();
		if (component != null && AllowedItemTypes.Contains(component.GetItemType()))
		{
			InventoryItem inventoryItem = new InventoryItem();
			inventoryItem.Init((int)component.GetItemType(), 1);
			structure.DepositInventory(inventoryItem);
			component.RemoveItem();
		}
	}
}
