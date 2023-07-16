using UnityEngine;

public class Task_DepositItem : Task
{
	private SimpleInventory T_Inventory;

	private Structure TargetStructure;

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		T_Inventory = t.GetComponent<SimpleInventory>();
		TargetStructure = TargetObject.GetComponent<Structure>();
	}

	public override void TaskUpdate()
	{
		base.TaskUpdate();
		if (TargetObject == null)
		{
			if (ParentTask != null)
			{
				ParentTask.ClearCurrentTask();
			}
			return;
		}
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			Timer = 0f;
			PathToPosition(TargetObject.transform.position);
			break;
		case StateMachine.State.Moving:
			if (Vector3.Distance(t.transform.position, TargetObject.transform.position) < 1f)
			{
				Timer = 0f;
				state.CURRENT_STATE = StateMachine.State.CustomAction0;
			}
			else if ((Timer += Time.deltaTime) > 1f)
			{
				Timer = 0f;
				PathToPosition(TargetObject.transform.position);
			}
			break;
		case StateMachine.State.CustomAction0:
			if ((Timer += Time.deltaTime) > 0.5f)
			{
				InventoryItem inventoryItem = new InventoryItem();
				inventoryItem.Init((int)T_Inventory.GetItemType(), 1);
				TargetStructure.DepositInventory(inventoryItem);
				T_Inventory.RemoveItem();
				state.CURRENT_STATE = StateMachine.State.Idle;
				TargetObject = null;
			}
			break;
		}
	}
}
