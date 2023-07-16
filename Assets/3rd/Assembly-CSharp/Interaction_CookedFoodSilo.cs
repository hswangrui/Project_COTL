using I2.Loc;
using UnityEngine;

public class Interaction_CookedFoodSilo : Interaction
{
	private Structure structure;

	private SimpleInventory PlayerInventory;

	public string ItemLabel;

	public void Start()
	{
		UpdateLocalisation();
		structure = GetComponent<Structure>();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		ItemLabel = ScriptLocalization.Inventory.MEAT;
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
		else if (structure.Inventory.Count > 0)
		{
			base.Label = ItemLabel;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (PlayerInventory != null && structure.Inventory.Count > 0)
		{
			PlayerInventory.GiveItem((InventoryItem.ITEM_TYPE)structure.Inventory[0].type);
			structure.Inventory.RemoveAt(0);
		}
		base.OnInteract(state);
	}
}
