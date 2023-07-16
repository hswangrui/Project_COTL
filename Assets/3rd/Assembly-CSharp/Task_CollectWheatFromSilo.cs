using UnityEngine;

internal class Task_CollectWheatFromSilo : Task
{
	private PickUp pickUp;

	private Structure TargetSilo;

	private SimpleInventory T_Inventory;

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		T_Inventory = t.GetComponent<SimpleInventory>();
		T_Inventory.DropItem();
	}

	public override void TaskUpdate()
	{
		base.TaskUpdate();
		if (T_Inventory.GetItemType() != InventoryItem.ITEM_TYPE.WHEAT)
		{
			if (TargetSilo == null)
			{
				GetTargetStructure();
				if (TargetSilo == null && ParentTask != null)
				{
					ParentTask.ClearCurrentTask();
				}
				return;
			}
			switch (state.CURRENT_STATE)
			{
			case StateMachine.State.Idle:
				Timer = 0f;
				PathToPosition(TargetSilo.gameObject.transform.position);
				break;
			case StateMachine.State.Moving:
				if (Vector3.Distance(t.transform.position, TargetSilo.transform.position) <= 1f)
				{
					Timer = 0f;
					state.CURRENT_STATE = StateMachine.State.CustomAction0;
				}
				else if ((Timer += Time.deltaTime) > 1f)
				{
					Timer = 0f;
					PathToPosition(TargetSilo.gameObject.transform.position);
				}
				break;
			case StateMachine.State.CustomAction0:
				if ((Timer += Time.deltaTime) > 0.5f)
				{
					if (TargetSilo.HasInventoryType(InventoryItem.ITEM_TYPE.WHEAT))
					{
						TargetSilo.RemoveInventoryByType(InventoryItem.ITEM_TYPE.WHEAT);
						T_Inventory.GiveItem(InventoryItem.ITEM_TYPE.WHEAT);
						TargetSilo = null;
						state.CURRENT_STATE = StateMachine.State.Idle;
					}
					else if (ParentTask != null)
					{
						ParentTask.ClearCurrentTask();
					}
				}
				break;
			}
			return;
		}
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			Timer = 0f;
			PathToPosition(TargetObject.gameObject.transform.position);
			break;
		case StateMachine.State.Moving:
			if (Vector3.Distance(t.transform.position, TargetObject.transform.position) <= 1f)
			{
				Timer = 0f;
				state.CURRENT_STATE = StateMachine.State.CustomAction0;
			}
			else if ((Timer += Time.deltaTime) > 1f)
			{
				Timer = 0f;
				PathToPosition(TargetObject.gameObject.transform.position);
			}
			break;
		case StateMachine.State.CustomAction0:
			if ((Timer += Time.deltaTime) > 0.5f)
			{
				Structure component = TargetObject.GetComponent<Structure>();
				InventoryItem inventoryItem = new InventoryItem();
				inventoryItem.Init(7, 1);
				if (component != null)
				{
					component.Inventory.Add(inventoryItem);
				}
				T_Inventory.RemoveItem();
				state.CURRENT_STATE = StateMachine.State.Idle;
				if (ParentTask != null)
				{
					ParentTask.ClearCurrentTask();
				}
			}
			break;
		}
	}

	private void GetTargetStructure()
	{
		Structure structure = null;
		float num = float.MaxValue;
		foreach (Structure structure2 in Structure.Structures)
		{
			if (structure2.HasInventoryType(InventoryItem.ITEM_TYPE.WHEAT) && structure2.Type == StructureBrain.TYPES.WHEAT_SILO)
			{
				float num2 = Vector3.Distance(structure2.gameObject.transform.position, t.transform.position);
				if (num2 < num)
				{
					structure = structure2;
					num = num2;
				}
			}
		}
		if (structure != null)
		{
			TargetSilo = structure;
		}
	}

	public override void ClearTask()
	{
		TargetObject = null;
		if (pickUp != null)
		{
			pickUp.Reserved = null;
		}
		pickUp = null;
	}
}
