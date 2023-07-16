public class SimpleInventoryWorshipper : SimpleInventory
{
	public SimpleSpineAnimator SpineAnimator;

	public override InventoryItem.ITEM_TYPE Item
	{
		get
		{
			return inventoryitem;
		}
		set
		{
			InventoryItem.ITEM_TYPE iTEM_TYPE = inventoryitem;
			inventoryitem = value;
			ItemImage.SetImage(Item);
			if (value != iTEM_TYPE)
			{
				if (inventoryitem == InventoryItem.ITEM_TYPE.NONE)
				{
					SpineAnimator.ChangeStateAnimation(StateMachine.State.Idle, "idle");
					SpineAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run");
				}
				else
				{
					SpineAnimator.ChangeStateAnimation(StateMachine.State.Idle, "idle-item");
					SpineAnimator.ChangeStateAnimation(StateMachine.State.Moving, "walk-item");
				}
			}
		}
	}
}
