using UnityEngine;

public class Task_ChopWood : Task
{
	private Tree TargetTree;

	public Task_ChopWood()
	{
		Type = Task_Type.CHOP_WOOD;
	}

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		state.CURRENT_STATE = StateMachine.State.Idle;
		FindClosestTree();
	}

	public override void ClearTask()
	{
		base.ClearTask();
		if (TargetTree != null)
		{
			TargetTree.TaskDoer = null;
		}
		TargetTree = null;
	}

	public override void TaskUpdate()
	{
		if (TargetObject != null)
		{
			switch (state.CURRENT_STATE)
			{
			case StateMachine.State.Idle:
				if (TargetTree.Dead)
				{
					ClearTarget();
				}
				else if (Vector3.Distance(t.transform.position, TargetObject.transform.position) > 1f)
				{
					if (state.CURRENT_STATE != StateMachine.State.Moving)
					{
						PathToTargetObject(t, 0f);
					}
				}
				else
				{
					state.facingAngle = Utils.GetAngle(t.transform.position, TargetObject.transform.position);
					state.CURRENT_STATE = StateMachine.State.SignPostAttack;
					Timer = 0f;
				}
				break;
			case StateMachine.State.Moving:
				if (TargetObject == null)
				{
					ClearTarget();
				}
				else if (TargetTree.Dead)
				{
					ClearTarget();
				}
				else if ((Timer += Time.deltaTime) > 1f)
				{
					Timer = 0f;
					PathToTargetObject(t, 0f);
				}
				break;
			case StateMachine.State.SignPostAttack:
				if ((Timer += Time.deltaTime) > 0.5f)
				{
					Timer = 0f;
					DoAttack(1f);
				}
				break;
			case StateMachine.State.RecoverFromAttack:
				if ((Timer += Time.deltaTime) > 0.5f)
				{
					Timer = 0f;
					if (TargetTree.Dead)
					{
						ClearTarget();
					}
					state.CURRENT_STATE = StateMachine.State.Idle;
				}
				break;
			case StateMachine.State.Attacking:
			case StateMachine.State.Defending:
				break;
			}
		}
		else if (TargetObject == null)
		{
			ClearTask();
		}
	}

	private void FindClosestTree()
	{
		GameObject targetObject = null;
		Tree targetTree = null;
		float num = float.MaxValue;
		foreach (Tree tree in Tree.Trees)
		{
			float num2 = Vector3.Distance(t.transform.position, tree.transform.position);
			if (num2 < num && !tree.Dead && tree.TaskDoer == null)
			{
				targetObject = tree.gameObject;
				targetTree = tree;
				num = num2;
			}
		}
		TargetObject = targetObject;
		TargetTree = targetTree;
		if (TargetTree != null)
		{
			TargetTree.TaskDoer = t.gameObject;
		}
	}

	private void ClearTarget()
	{
		Timer = 0f;
		TargetObject = null;
		if (TargetTree != null)
		{
			TargetTree.TaskDoer = null;
		}
		TargetTree = null;
		state.CURRENT_STATE = StateMachine.State.Idle;
	}
}
