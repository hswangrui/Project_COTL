using System;
using UnityEngine;

public class Task_DefenceTower : Task
{
	private WorkPlace workplace;

	private WorshipperInfoManager wim;

	private Health health;

	private Health EnemyTarget;

	private float DetectEnemyRange = 5f;

	public Task_DefenceTower()
	{
		Type = Task_Type.DEFENCE_TOWER;
	}

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		wim = t.GetComponent<WorshipperInfoManager>();
		workplace = WorkPlace.GetWorkPlaceByID(wim.v_i.WorkPlace);
		health = t.GetComponent<Health>();
	}

	public override void TaskUpdate()
	{
		base.TaskUpdate();
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			if (Vector3.Distance(workplace.Positions[wim.v_i.WorkPlaceSlot].transform.position, t.transform.position) > 0.5f)
			{
				Timer = 0f;
				PathToPosition(workplace.Positions[wim.v_i.WorkPlaceSlot].transform.position);
				TaskDoer taskDoer = t;
				taskDoer.EndOfPath = (Action)Delegate.Combine(taskDoer.EndOfPath, new Action(ArriveAtWorkPlace));
			}
			else
			{
				state.CURRENT_STATE = StateMachine.State.CustomAction0;
			}
			break;
		case StateMachine.State.CustomAction0:
			Defend();
			if (Vector3.Distance(workplace.Positions[wim.v_i.WorkPlaceSlot].transform.position, t.transform.position) > 0.5f)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			break;
		}
	}

	private void Defend()
	{
		if (EnemyTarget == null)
		{
			GetTarget();
		}
		else if ((Timer -= Time.deltaTime) < 0f)
		{
			Projectile component = ObjectPool.Spawn(Resources.Load("Prefabs/Weapons/Arrow") as GameObject, t.transform.parent).GetComponent<Projectile>();
			component.transform.position = t.transform.position + new Vector3(0.5f * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)), 0.5f * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)), -0.5f);
			component.Angle = state.facingAngle;
			component.team = health.team;
			component.Damage = 0.6f;
			component.Owner = health;
			state.CURRENT_STATE = StateMachine.State.Idle;
			EnemyTarget = null;
		}
		else
		{
			state.facingAngle = Utils.GetAngle(t.transform.position, EnemyTarget.transform.position);
		}
	}

	private void GetTarget()
	{
		Health health = null;
		float num = float.MaxValue;
		foreach (Health allUnit in Health.allUnits)
		{
			if (allUnit.team != this.health.team && allUnit.team != 0 && allUnit.attackers.Count < 4 && Vector2.Distance(t.transform.position, allUnit.gameObject.transform.position) < DetectEnemyRange && t.CheckLineOfSight(allUnit.gameObject.transform.position, Vector2.Distance(allUnit.gameObject.transform.position, t.transform.position)))
			{
				float num2 = Vector3.Distance(t.transform.position, allUnit.gameObject.transform.position);
				if (num2 < num)
				{
					health = allUnit;
					num = num2;
				}
			}
		}
		if (health != null)
		{
			health.attackers.Add(t.gameObject);
			TargetObject = health.gameObject;
			EnemyTarget = health;
			Timer = 2f;
		}
	}

	private void ArriveAtWorkPlace()
	{
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		TaskDoer taskDoer = t;
		taskDoer.EndOfPath = (Action)Delegate.Remove(taskDoer.EndOfPath, new Action(ArriveAtWorkPlace));
	}

	public override void ClearTask()
	{
		workplace.EndJob(t, wim.v_i.WorkPlaceSlot);
		base.ClearTask();
		TaskDoer taskDoer = t;
		taskDoer.EndOfPath = (Action)Delegate.Remove(taskDoer.EndOfPath, new Action(ArriveAtWorkPlace));
	}
}
