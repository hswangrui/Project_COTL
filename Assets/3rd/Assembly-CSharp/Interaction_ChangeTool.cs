public class Interaction_ChangeTool : Interaction
{
	public InventoryWeapon.ITEM_TYPE Tool;

	private InventoryWeapon ToolItem;

	private void Start()
	{
		ToolItem = new InventoryWeapon(Tool, 1);
	}

	public override void GetLabel()
	{
		int cURRENT_WEAPON = Inventory.CURRENT_WEAPON;
		InventoryWeapon.ITEM_TYPE tool = Tool;
	}

	public override void OnInteract(StateMachine state)
	{
		if (Inventory.CURRENT_WEAPON != (int)Tool)
		{
			Inventory.CURRENT_WEAPON = (int)Tool;
		}
		else
		{
			Inventory.CURRENT_WEAPON = 0;
		}
	}
}
