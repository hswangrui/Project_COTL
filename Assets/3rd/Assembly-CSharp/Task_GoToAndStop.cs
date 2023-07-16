using System;
using UnityEngine;

public class Task_GoToAndStop : Task
{
	public bool DoCallback = true;

	public bool ClearCurrentTaskAfterGoToAndStop = true;

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		t.UsePathing = true;
	}

	public override void TaskUpdate()
	{
		if (TargetObject == null)
		{
			EndOfPath();
			return;
		}
		base.TaskUpdate();
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			if (Vector3.Distance(TargetObject.transform.position, t.transform.position) > t.StoppingDistance)
			{
				Timer = 0f;
				PathToPosition(TargetObject.transform.position);
				TaskDoer taskDoer = t;
				taskDoer.EndOfPath = (Action)Delegate.Combine(taskDoer.EndOfPath, new Action(EndOfPath));
			}
			else
			{
				t.transform.position = Vector3.Lerp(t.transform.position, TargetObject.transform.position, 2f * Time.deltaTime);
				EndOfPath();
			}
			break;
		case StateMachine.State.Moving:
			if ((Timer += Time.deltaTime) > 1f)
			{
				Timer = 0f;
				PathToPosition(TargetObject.transform.position);
			}
			break;
		}
	}

	private void EndOfPath()
	{
		Worshipper component = t.GetComponent<Worshipper>();
		if (component != null && DoCallback)
		{
			component.EndGoToAndStop();
		}
		TaskDoer taskDoer = t;
		taskDoer.EndOfPath = (Action)Delegate.Remove(taskDoer.EndOfPath, new Action(EndOfPath));
	}

	public override void ClearTask()
	{
		TaskDoer taskDoer = t;
		taskDoer.EndOfPath = (Action)Delegate.Remove(taskDoer.EndOfPath, new Action(EndOfPath));
		if (ClearCurrentTaskAfterGoToAndStop)
		{
			base.ClearTask();
		}
	}
}
