using UnityEngine;

internal class Task_DoCooking : Task
{
	private Structure Kitchen;

	private SimpleInventory T_Inventory;

	public WorkPlace workplace;

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		Kitchen = TargetObject.GetComponent<Structure>();
		T_Inventory = t.GetComponent<SimpleInventory>();
	}

	public override void TaskUpdate()
	{
		base.TaskUpdate();
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			Timer = 0f;
			state.CURRENT_STATE = StateMachine.State.CustomAction0;
			Kitchen.RemoveInventoryByType(InventoryItem.ITEM_TYPE.WHEAT);
			Kitchen.RemoveInventoryByType(InventoryItem.ITEM_TYPE.WHEAT);
			Kitchen.RemoveInventoryByType(InventoryItem.ITEM_TYPE.WHEAT);
			break;
		case StateMachine.State.CustomAction0:
			if ((Timer += Time.deltaTime) > 10f)
			{
				T_Inventory.GiveItem(InventoryItem.ITEM_TYPE.MEAT);
				state.CURRENT_STATE = StateMachine.State.Idle;
				if (ParentTask != null)
				{
					ParentTask.ClearCurrentTask();
				}
			}
			else if (!workplace.HasPower())
			{
				InventoryItem inventoryItem = new InventoryItem();
				inventoryItem.Init(7, 1);
				Kitchen.DepositInventory(inventoryItem);
				inventoryItem = new InventoryItem();
				inventoryItem.Init(7, 1);
				Kitchen.DepositInventory(inventoryItem);
				inventoryItem = new InventoryItem();
				inventoryItem.Init(7, 1);
				Kitchen.DepositInventory(inventoryItem);
				state.CURRENT_STATE = StateMachine.State.Idle;
				if (ParentTask != null)
				{
					ParentTask.ClearCurrentTask();
				}
			}
			break;
		}
	}
}
