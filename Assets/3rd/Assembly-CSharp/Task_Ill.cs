using Spine;
using UnityEngine;

public class Task_Ill : Task
{
	public Worshipper w;

	private bool Chundered;

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		Type = Task_Type.ILL;
		w = t.GetComponent<Worshipper>();
		w.maxSpeed = 0.02f;
		state.CURRENT_STATE = StateMachine.State.Idle;
		w.simpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "Sick/idle-sick");
		w.simpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Sick/walk-sick");
		w.Spine.AnimationState.Event += HandleEvent;
	}

	private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
	{
	}

	public override void TaskUpdate()
	{
		if (state.CURRENT_STATE == StateMachine.State.Idle)
		{
			if (!Chundered && Random.Range(0f, 1f) < 0.001f)
			{
				w.TimedAnimation("Sick/chunder", 3.5f, w.BackToIdle);
				Chundered = true;
			}
			else if ((Timer -= Time.deltaTime) < 0f)
			{
				Timer = Random.Range(4f, 6f);
				Chundered = false;
				t.givePath(TownCentre.Instance.RandomPositionInTownCentre());
			}
		}
	}

	public override void ClearTask()
	{
		w.Spine.AnimationState.Event -= HandleEvent;
		w.maxSpeed = 0.04f;
		t.ClearPaths();
		base.ClearTask();
		w.simpleAnimator.ResetAnimationsToDefaults();
	}
}
