using UnityEngine;

public class Task_Combat_Shield : Task_Combat
{
	public float ChargeRange = 5f;

	public float ChargeDuration = 0.3f;

	public float ChargeDelay;

	private float HitTimer;

	private float DefendTimer;

	public Task_Combat_Shield()
	{
		Type = Task_Type.SHIELD;
	}

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		state.CURRENT_STATE = StateMachine.State.Idle;
		health = t.gameObject.GetComponent<Health>();
		health.OnDie += OnDie;
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		ClearTarget();
		health.OnDie -= OnDie;
	}

	public override void TaskUpdate()
	{
		thisChecked = 0;
		ChargeDelay -= Time.deltaTime;
		if (TargetObject != null)
		{
			switch (state.CURRENT_STATE)
			{
			case StateMachine.State.Defending:
				if ((DefendTimer += Time.deltaTime) >= DefendingDuration)
				{
					DoCounterAttack();
					DefendTimer = 0f;
				}
				break;
			case StateMachine.State.HitLeft:
			case StateMachine.State.HitRight:
				if ((HitTimer += Time.deltaTime) >= 0.4f)
				{
					DoCounterAttack();
					Timer = 0f;
					HitTimer = 0f;
				}
				break;
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
				if (Vector2.Distance(t.transform.position, TargetObject.transform.position) > LoseEnemyRange)
				{
					state.CURRENT_STATE = StateMachine.State.Idle;
					ClearTarget();
				}
				else if (ChargeDelay < 0f && Vector2.Distance(t.transform.position, TargetObject.transform.position) < ChargeRange)
				{
					state.facingAngle = Utils.GetAngle(t.transform.position, TargetObject.transform.position);
					Timer = 0f;
					state.CURRENT_STATE = StateMachine.State.Charging;
				}
				else if ((Timer += Time.deltaTime) > 1f)
				{
					Timer = 0f;
					ClearTarget();
					GetNewTarget();
					SortAttackPosition();
					PathToPosition(TargetPosition());
				}
				break;
			case StateMachine.State.Charging:
				if ((Timer += Time.deltaTime) > 0.2f)
				{
					t.speed = 0.2f;
					if (Vector2.Distance(TargetPosition(), t.transform.position) < AttackRange)
					{
						Timer = 0f;
						state.facingAngle = Utils.GetAngle(t.transform.position, TargetObject.transform.position);
						DoAttack(AttackRange);
						ChargeDelay = 5f;
					}
					else if (Timer > ChargeDuration + 0.5f)
					{
						state.CURRENT_STATE = StateMachine.State.Vulnerable;
						ChargeDelay = 5f;
					}
				}
				else
				{
					t.speed = 0f;
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
					state.CURRENT_STATE = StateMachine.State.Vulnerable;
					if (TargetObject == null)
					{
						ClearTarget();
					}
				}
				break;
			case StateMachine.State.RecoverFromCounterAttack:
				if ((Timer += Time.deltaTime) > 1f)
				{
					Timer = 0f;
					state.CURRENT_STATE = StateMachine.State.Vulnerable;
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

	private void DoCounterAttack()
	{
		if (TargetObject == null)
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
			return;
		}
		state.facingAngle = Utils.GetAngle(t.transform.position, TargetObject.transform.position);
		if (Vector2.Distance(TargetPosition(), t.transform.position) < AttackRange)
		{
			state.CURRENT_STATE = StateMachine.State.SignPostAttack;
		}
		else
		{
			state.CURRENT_STATE = StateMachine.State.Charging;
		}
	}

	public override void ClearTask()
	{
		ClearTarget();
		base.ClearTask();
	}

	private static int SortByY(Vector2 p1, Vector2 p2)
	{
		return p1.y.CompareTo(p2.y);
	}
}
