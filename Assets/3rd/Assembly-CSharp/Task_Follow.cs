using System;
using UnityEngine;

public class Task_Follow : Task
{
	private float MaxRange = 2f;

	private float FollowDistance = 1f;

	private float posAngle;

	private float RepathTimer;

	public Task_Follow()
	{
		Type = Task_Type.FOLLOW;
	}

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		state.CURRENT_STATE = StateMachine.State.Idle;
	}

	public override void TaskUpdate()
	{
		if (TargetObject != null)
		{
			posAngle = 90 * t.Position;
			TargetV3 = AstarPath.active.GetNearest(TargetObject.transform.position + new Vector3(1f * Mathf.Cos(posAngle * ((float)Math.PI / 180f)), 1f * Mathf.Sin(posAngle * ((float)Math.PI / 180f)), 0f)).position;
		}
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			if (TargetObject != null && state.CURRENT_STATE == StateMachine.State.Idle && Vector3.Distance(t.transform.position, TargetV3) > MaxRange)
			{
				Timer = 0f;
				PathToPosition(TargetV3);
			}
			break;
		case StateMachine.State.Moving:
			if (TargetObject == null)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			else if (Vector3.Distance(t.transform.position, TargetV3) < 0.5f)
			{
				t.ClearPaths();
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			else if ((RepathTimer += Time.deltaTime) > 1f)
			{
				t.ClearPaths();
				if (!t.IsPathPossible(t.transform.position, TargetV3) && t.OnGround(TargetV3 + Vector3.back * 10f))
				{
					t.transform.position = TargetV3;
					RepathTimer = 0f;
				}
				else
				{
					RepathTimer = 0f;
				}
				PathToPosition(TargetV3);
			}
			break;
		}
	}
}
