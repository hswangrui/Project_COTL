using System.Collections;
using UnityEngine;

public class Task_DanceAtFirePit : Task
{
	private Worshipper Worshipper;

	private GameObject Position;

	private Coroutine cDanceRoutine;

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		Worshipper = t.GetComponent<Worshipper>();
		Type = Task_Type.DANCE_AT_FIREPIT;
	}

	public void GivePosition(GameObject Position)
	{
		this.Position = Position;
		Worshipper.GoToAndStop(Position, DoDance, TargetObject, false);
	}

	private void DoDance()
	{
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		Worshipper.SetAnimation("dance", true);
		cDanceRoutine = Worshipper.StartCoroutine(DanceRoutine());
	}

	private IEnumerator DanceRoutine()
	{
		while (true)
		{
			Worshipper.wim.v_i.IncreaseCultFaith(1f);
			yield return null;
		}
	}

	public void Cheer()
	{
		Debug.Log("Cheer!");
		if (cDanceRoutine != null)
		{
			Worshipper.StopCoroutine(cDanceRoutine);
		}
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		Worshipper.SetAnimation("cheer", true);
	}

	public void EndDance()
	{
		state.CURRENT_STATE = StateMachine.State.Idle;
		ClearTask();
	}

	public override void ClearTask()
	{
		if (Position != null)
		{
			Object.Destroy(Position);
		}
		if (cDanceRoutine != null)
		{
			Worshipper.StopCoroutine(cDanceRoutine);
		}
		base.ClearTask();
	}
}
