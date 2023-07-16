using System;
using System.Collections.Generic;
using UnityEngine;

public class Task_Combat : Task
{
	public float DetectEnemyRange = 5f;

	public float AttackRange = 0.5f;

	public float LoseEnemyRange = 7f;

	public float PreAttackDuration = 0.5f;

	public float PostAttackDuration = 1f;

	[HideInInspector]
	public int thisChecked;

	[HideInInspector]
	public int AttackPosition;

	[HideInInspector]
	public Health EnemyHealth;

	[HideInInspector]
	public Health health;

	[HideInInspector]
	public float DefendingDuration;

	public bool CannotLoseTarget;

	private float posAngle;

	public Task_Combat()
	{
		Type = Task_Type.COMBAT;
	}

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		state.CURRENT_STATE = StateMachine.State.Idle;
		health = t.gameObject.GetComponent<Health>();
		health.OnDie += OnDie;
	}

	public void Init(float DetectEnemyRange, float AttackRange, float LoseEnemyRange, float PreAttackDuration, float PostAttackDuration, float DefendingDuration, TaskDoer doer)
	{
		this.DetectEnemyRange = DetectEnemyRange;
		this.AttackRange = AttackRange;
		this.LoseEnemyRange = LoseEnemyRange;
		this.PreAttackDuration = PreAttackDuration;
		this.PostAttackDuration = PostAttackDuration;
		this.DefendingDuration = DefendingDuration;
		t = doer;
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		ClearTarget();
		health.OnDie -= OnDie;
	}

	public override void TaskUpdate()
	{
		thisChecked = 0;
		if (TargetObject != null)
		{
			switch (state.CURRENT_STATE)
			{
			case StateMachine.State.Idle:
				if (Vector2.Distance(TargetPosition(), t.transform.position) < AttackRange)
				{
					Timer = 0f;
					state.facingAngle = Utils.GetAngle(t.transform.position, TargetObject.transform.position);
					state.CURRENT_STATE = StateMachine.State.SignPostAttack;
				}
				else
				{
					Timer = 0f;
					SortAttackPosition();
					PathToPosition(TargetPosition());
				}
				break;
			case StateMachine.State.Moving:
				if (Vector2.Distance(t.transform.position, TargetObject.transform.position) > LoseEnemyRange && !CannotLoseTarget)
				{
					state.CURRENT_STATE = StateMachine.State.Idle;
					ClearTarget();
				}
				else if ((Timer += Time.deltaTime) > 0.5f)
				{
					Timer = 0f;
					if (!CannotLoseTarget)
					{
						ClearTarget();
						GetNewTarget();
					}
					SortAttackPosition();
					PathToPosition(TargetPosition());
				}
				break;
			case StateMachine.State.SignPostAttack:
				if ((Timer += Time.deltaTime) > PreAttackDuration)
				{
					Timer = 0f;
					if (TargetObject == null)
					{
						state.CURRENT_STATE = StateMachine.State.Idle;
					}
					else
					{
						DoAttack(AttackRange);
					}
				}
				break;
			case StateMachine.State.RecoverFromAttack:
				if ((Timer += Time.deltaTime) > PostAttackDuration)
				{
					Timer = 0f;
					state.CURRENT_STATE = StateMachine.State.Idle;
					if (TargetObject == null)
					{
						ClearTarget();
					}
				}
				break;
			case StateMachine.State.SignPostCounterAttack:
				if ((Timer += Time.deltaTime) > PreAttackDuration)
				{
					Timer = 0f;
					if (TargetObject == null)
					{
						state.CURRENT_STATE = StateMachine.State.Idle;
						break;
					}
					CameraManager.shakeCamera(0.3f, state.facingAngle);
					DoAttack(AttackRange, StateMachine.State.RecoverFromCounterAttack);
				}
				break;
			case StateMachine.State.RecoverFromCounterAttack:
				if ((Timer += Time.deltaTime) > PostAttackDuration)
				{
					Timer = 0f;
					state.CURRENT_STATE = StateMachine.State.Idle;
					if (TargetObject == null)
					{
						ClearTarget();
					}
				}
				break;
			}
		}
		else
		{
			GetNewTarget();
			if (TargetObject == null)
			{
				ClearTask();
			}
		}
	}

	public override void ClearTask()
	{
		ClearTarget();
		base.ClearTask();
	}

	public void ClearTarget()
	{
		if (TargetObject != null)
		{
			if (TargetObject.GetComponent<Health>().AttackPositions[AttackPosition] != null)
			{
				TargetObject.GetComponent<Health>().AttackPositions[AttackPosition] = null;
			}
			TargetObject.GetComponent<Health>().attackers.Remove(t.gameObject);
		}
		TargetObject = null;
		Timer = 0f;
		state.CURRENT_STATE = StateMachine.State.Idle;
	}

	public Vector3 TargetPosition()
	{
		if (TargetObject == null)
		{
			return t.transform.position;
		}
		posAngle = 90 * AttackPosition;
		return TargetObject.transform.position + new Vector3(1f * Mathf.Cos(posAngle * ((float)Math.PI / 180f)), 1f * Mathf.Sin(posAngle * ((float)Math.PI / 180f)), 0f);
	}

	public void GetNewTarget()
	{
		Health health = null;
		float num = float.MaxValue;
		foreach (Health allUnit in Health.allUnits)
		{
			if (allUnit.team != this.health.team && !allUnit.InanimateObject && allUnit.team != 0 && Vector2.Distance(t.transform.position, allUnit.gameObject.transform.position) < DetectEnemyRange && t.CheckLineOfSight(allUnit.gameObject.transform.position, Vector2.Distance(allUnit.gameObject.transform.position, t.transform.position)))
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
			EnemyHealth = health;
			SortAttackPosition();
		}
	}

	public void SortAttackPosition()
	{
		if (TargetObject == null)
		{
			return;
		}
		thisChecked++;
		if (EnemyHealth.AttackPositions[AttackPosition] != null && EnemyHealth.AttackPositions[AttackPosition] == this)
		{
			EnemyHealth.AttackPositions[AttackPosition] = null;
		}
		List<Vector2> list = new List<Vector2>();
		for (int i = 0; i < 4; i++)
		{
			float num = 90 * i;
			Vector2 b = TargetObject.gameObject.transform.position + new Vector3(1f * Mathf.Cos(num * ((float)Math.PI / 180f)), 1f * Mathf.Sin(num * ((float)Math.PI / 180f)), 0f);
			float y = Vector2.Distance(t.transform.position, b);
			Vector2 item = new Vector2(i, y);
			list.Add(item);
		}
		list.Sort(SortByY);
		for (int j = 0; j < 4; j++)
		{
			if (EnemyHealth.AttackPositions[(int)list[j].x] == null)
			{
				AttackPosition = (int)list[j].x;
				EnemyHealth.AttackPositions[(int)list[j].x] = this;
				break;
			}
			if (EnemyHealth.AttackPositions[(int)list[j].x].thisChecked < 4)
			{
				Task_Combat task_Combat = EnemyHealth.AttackPositions[(int)list[j].x];
				float num2 = 90 * j;
				Vector3 vector = TargetObject.transform.position + new Vector3(1f * Mathf.Cos(num2 * ((float)Math.PI / 180f)), 1f * Mathf.Sin(num2 * ((float)Math.PI / 180f)), 0f);
				if (task_Combat != null && task_Combat.t != null && Vector2.Distance(task_Combat.t.transform.position, vector) > Vector2.Distance(t.transform.position, vector))
				{
					Task_Combat obj = EnemyHealth.AttackPositions[(int)list[j].x];
					AttackPosition = (int)list[j].x;
					EnemyHealth.AttackPositions[(int)list[j].x] = this;
					obj.SortAttackPosition();
					break;
				}
			}
		}
	}

	private static int SortByY(Vector2 p1, Vector2 p2)
	{
		return p1.y.CompareTo(p2.y);
	}
}
