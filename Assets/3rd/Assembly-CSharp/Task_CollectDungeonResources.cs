using UnityEngine;

public class Task_CollectDungeonResources : Task
{
	private SimpleInventory T_Inventory;

	private PickUp pickUp;

	public Task_CollectDungeonResources()
	{
		Type = Task_Type.COLLECT_DUNGEON_RESOURCES;
		ClearOnComplete = true;
	}

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		pickUp = TargetObject.GetComponent<PickUp>();
		T_Inventory = t.GetComponent<SimpleInventory>();
	}

	public override void TaskUpdate()
	{
		base.TaskUpdate();
		if (TargetObject == null)
		{
			ClearTask();
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
		{
			Timer += Time.deltaTime;
			float num = 0.5f;
			break;
		}
		}
	}

	public override void ClearTask()
	{
		base.ClearTask();
		if (pickUp != null)
		{
			pickUp.Reserved = null;
		}
		pickUp = null;
	}
}
