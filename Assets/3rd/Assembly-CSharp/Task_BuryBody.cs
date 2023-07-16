using System.Collections;
using UnityEngine;

public class Task_BuryBody : Task
{
	private Worshipper Worshipper;

	private Grave Grave;

	public DeadWorshipper DeadWorshipper;

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		Worshipper = t.GetComponent<Worshipper>();
		Grave = TargetObject.GetComponent<Grave>();
		Type = Task_Type.BURY_BUILDING;
		Worshipper.GoToAndStop(DeadWorshipper.gameObject, PickUpBody, DeadWorshipper.gameObject, false);
	}

	private void PickUpBody()
	{
		t.StartCoroutine(PickUpBodyRoutine());
	}

	private IEnumerator PickUpBodyRoutine()
	{
		yield return new WaitForSeconds(0.5f);
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		yield return new WaitForSeconds(1f);
		DeadWorshipper.WrapBody();
		state.CURRENT_STATE = StateMachine.State.Idle;
		yield return new WaitForSeconds(0.5f);
		DeadWorshipper.HideBody();
		Worshipper.simpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run-corpse");
		Worshipper.GoToAndStop(TargetObject, BuryBody, TargetObject, false);
		while (Worshipper.GoToAndStopping)
		{
			DeadWorshipper.transform.position = t.transform.position;
			yield return null;
		}
	}

	private void BuryBody()
	{
		t.StartCoroutine(BuryBodyRoutine());
	}

	private IEnumerator BuryBodyRoutine()
	{
		DeadWorshipper.ShowBody();
		yield return new WaitForSeconds(0.5f);
		Worshipper.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		Worshipper.SetAnimation("dig", true);
		yield return new WaitForSeconds(5f);
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		yield return new WaitForSeconds(0.5f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		yield return new WaitForSeconds(0.5f);
		ClearTask();
	}

	public override void ClearTask()
	{
		Worshipper.simpleAnimator.ResetAnimationsToDefaults();
		bool flag = DeadWorshipper != null;
		base.ClearTask();
	}
}
