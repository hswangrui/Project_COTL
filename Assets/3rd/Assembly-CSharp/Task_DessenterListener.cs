using System;
using UnityEngine;

public class Task_DessenterListener : Task
{
	public Worshipper w;

	private GameObject GoToObject;

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		Type = Task_Type.DISSENTER_LISTENER;
		w = t.GetComponent<Worshipper>();
		state.CURRENT_STATE = StateMachine.State.Idle;
		float num = UnityEngine.Random.Range(2, 3);
		GoToObject = new GameObject();
		float f = Utils.GetAngle(TargetObject.transform.position, t.transform.position) * ((float)Math.PI / 180f);
		GoToObject.transform.position = TargetObject.transform.position + new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
		w.GoToAndStop(GoToObject, ContinueListen, TargetObject, false);
	}

	private void ContinueListen()
	{
		w.state.facingAngle = Utils.GetAngle(w.transform.position, TargetObject.transform.position);
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		w.SetAnimation("Dissenters/dissenter-listening", true);
	}

	public override void ClearTask()
	{
		UnityEngine.Object.Destroy(GoToObject);
		base.ClearTask();
	}
}
