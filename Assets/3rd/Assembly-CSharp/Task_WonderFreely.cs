using System;
using UnityEngine;

public class Task_WonderFreely : Task
{
	private float TargetAngle;

	private GameObject Player;

	private float BowingDelay;

	private Worshipper w;

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		t.UsePathing = false;
		t.OnCollision = (Action)Delegate.Combine(t.OnCollision, new Action(OnCollision));
		SimpleInventory component = t.GetComponent<SimpleInventory>();
		if (component != null)
		{
			component.DropItem();
		}
		Timer = UnityEngine.Random.Range(1, 3);
		Type = Task_Type.NONE;
		Player = GameObject.FindWithTag("Player");
		w = t.GetComponent<Worshipper>();
	}

	public override void TaskUpdate()
	{
		if (Player != null && w != null && (BowingDelay += Time.deltaTime) > 2f && state.CURRENT_STATE != StateMachine.State.TimedAction && Vector3.Distance(t.transform.position, Player.transform.position) < 2f)
		{
			w.TimedAnimation("bowed-down", 3f, w.BackToIdle);
			t.state.facingAngle = Utils.GetAngle(t.transform.position, Player.transform.position);
			BowingDelay = 0f;
		}
		base.TaskUpdate();
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			if ((Timer -= Time.deltaTime) < 0f)
			{
				Timer = UnityEngine.Random.Range(0.5f, 2f);
				TargetAngle = UnityEngine.Random.Range(0, 360);
				t.UsePathing = false;
				state.CURRENT_STATE = StateMachine.State.Moving;
			}
			break;
		case StateMachine.State.Moving:
			state.facingAngle += Mathf.Atan2(Mathf.Sin((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f))) * 57.29578f / (10f * GameManager.DeltaTime);
			if ((Timer -= Time.deltaTime) < 0f)
			{
				Timer = UnityEngine.Random.Range(3, 7);
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			break;
		}
	}

	private void OnCollision()
	{
		TargetAngle = state.facingAngle + 90f;
	}

	public override void ClearTask()
	{
		TaskDoer taskDoer = t;
		taskDoer.OnCollision = (Action)Delegate.Remove(taskDoer.OnCollision, new Action(OnCollision));
		t.UsePathing = true;
		base.ClearTask();
	}
}
