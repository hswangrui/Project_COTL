using UnityEngine;

public class Task_WorshipLeader : Task
{
	private bool GivenSoul;

	private Worshipper w;

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		state.CURRENT_STATE = StateMachine.State.Preach;
		state.facingAngle = Utils.GetAngle(t.transform.position, TargetObject.transform.position);
		w = t.GetComponent<Worshipper>();
	}

	public override void TaskUpdate()
	{
		if (state.CURRENT_STATE != StateMachine.State.Preach)
		{
			state.CURRENT_STATE = StateMachine.State.Preach;
		}
		if (w.simpleAnimator.CurrentAnimation() != "bowed-down")
		{
			w.SetAnimation("bowed-down", true);
		}
		if (!GivenSoul)
		{
			if ((Timer += Time.deltaTime) > 1f)
			{
				GivenSoul = true;
				SoulCustomTarget.Create(TargetObject, t.transform.position, Color.black, null);
				Timer = 0f;
			}
		}
		else if ((Timer += Time.deltaTime) > 1.5f)
		{
			w.EndWorship();
		}
	}
}
