using System.Collections;
using UnityEngine;

public class Task_Imprisoned : Task
{
	public Worshipper w;

	public Prison Prison;

	public Task_Imprisoned()
	{
		Type = Task_Type.IMPRISONED;
	}

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		w = t.GetComponent<Worshipper>();
		Prison = TargetObject.GetComponent<Prison>();
		w.simpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "picked-up-hate");
		w.transform.position = Prison.PrisonerLocation.position;
		w.StartCoroutine(ArriveAtPrisonRoutine());
	}

	private IEnumerator ArriveAtPrisonRoutine()
	{
		state.CURRENT_STATE = StateMachine.State.Moving;
		yield return null;
		state.CURRENT_STATE = StateMachine.State.Idle;
		w.wim.v_i.Starve = 0f;
	}

	public override void ClearTask()
	{
		base.ClearTask();
		w.simpleAnimator.ResetAnimationsToDefaults();
	}
}
