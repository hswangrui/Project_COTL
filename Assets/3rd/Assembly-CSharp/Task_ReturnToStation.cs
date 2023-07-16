using UnityEngine;

internal class Task_ReturnToStation : Task
{
	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
	}

	public override void TaskUpdate()
	{
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
			PathToPosition(TargetObject.gameObject.transform.position);
			break;
		case StateMachine.State.Moving:
			if (Vector3.Distance(t.transform.position, TargetObject.transform.position) <= Farm.FarmTileSize)
			{
				if (ParentTask != null)
				{
					ParentTask.ClearCurrentTask();
				}
			}
			else if ((Timer += Time.deltaTime) > 1f)
			{
				Timer = 0f;
				PathToPosition(TargetObject.gameObject.transform.position);
			}
			break;
		}
	}
}
