using UnityEngine;

public class Task_Archer : Task
{
	private Health health;

	private Health EnemyHealth;

	private float DetectEnemyRange;

	private float AttackRange;

	private float LoseEnemyRange;

	private float PreAttackDuration;

	private float PostAttackDuration;

	private float MinimumRange;

	private float DefaultMinimumRange;

	private GameObject Arrow;

	public Task_Archer()
	{
		Type = Task_Type.ARCHER;
	}

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		state.CURRENT_STATE = StateMachine.State.Idle;
		health = t.gameObject.GetComponent<Health>();
		health.OnDie += OnDie;
	}

	public void Init(float DetectEnemyRange, float AttackRange, float LoseEnemyRange, float PreAttackDuration, float PostAttackDuration, float MinimumRange, GameObject Arrow)
	{
		this.DetectEnemyRange = DetectEnemyRange;
		this.AttackRange = AttackRange;
		this.LoseEnemyRange = LoseEnemyRange;
		this.PreAttackDuration = PreAttackDuration;
		this.PostAttackDuration = PostAttackDuration;
		DefaultMinimumRange = (this.MinimumRange = MinimumRange);
		this.Arrow = Arrow;
	}

	public Vector3 TargetPosition()
	{
		if (TargetObject == null)
		{
			return Vector3.zero;
		}
		return TargetObject.transform.position;
	}

	public override void TaskUpdate()
	{
		if (TargetObject != null)
		{
			switch (state.CURRENT_STATE)
			{
			case StateMachine.State.Idle:
			{
				float num = Vector2.Distance(TargetPosition(), t.transform.position);
				if (num < AttackRange)
				{
					Timer = 0f;
					state.facingAngle = Utils.GetAngle(t.transform.position, TargetObject.transform.position);
					state.CURRENT_STATE = StateMachine.State.SignPostAttack;
				}
				else if (t.IsPathPossible(t.transform.position, TargetPosition()))
				{
					Timer = 0f;
					PathToPosition(TargetPosition());
					MinimumRange = DefaultMinimumRange + (float)Random.Range(-2, 2);
				}
				break;
			}
			case StateMachine.State.Moving:
			{
				float num = Vector2.Distance(TargetPosition(), t.transform.position);
				if (num > LoseEnemyRange || num < MinimumRange || !t.IsPathPossible(t.transform.position, TargetPosition()))
				{
					state.CURRENT_STATE = StateMachine.State.Idle;
					ClearTarget();
				}
				else if ((Timer += Time.deltaTime) > 1f)
				{
					Timer = 0f;
					ClearTarget();
					GetNewTarget();
					PathToPosition(TargetPosition());
				}
				break;
			}
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
			case StateMachine.State.Attacking:
			case StateMachine.State.Defending:
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

	public override void DoAttack(float AttackRange, StateMachine.State NextState = StateMachine.State.RecoverFromAttack)
	{
		Timer = 0f;
		state.CURRENT_STATE = NextState;
		Projectile component = ObjectPool.Spawn(Arrow, t.transform.parent).GetComponent<Projectile>();
		component.transform.position = t.transform.position;
		component.Angle = state.facingAngle;
		component.team = health.team;
		component.Owner = health;
	}

	public void GetNewTarget()
	{
		Health health = null;
		float num = float.MaxValue;
		foreach (Health allUnit in Health.allUnits)
		{
			if (allUnit.team != this.health.team && !allUnit.InanimateObject && allUnit.team != 0 && (this.health.team != Health.Team.PlayerTeam || (this.health.team == Health.Team.PlayerTeam && allUnit.team != Health.Team.DangerousAnimals)) && Vector2.Distance(t.transform.position, allUnit.gameObject.transform.position) < DetectEnemyRange && t.CheckLineOfSight(allUnit.gameObject.transform.position, Vector2.Distance(allUnit.gameObject.transform.position, t.transform.position)))
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
			TargetObject = health.gameObject;
			EnemyHealth = health;
			EnemyHealth.attackers.Add(t.gameObject);
		}
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		ClearTarget();
		health.OnDie -= OnDie;
	}

	public override void ClearTask()
	{
		ClearTarget();
		base.ClearTask();
	}

	public void ClearTarget()
	{
		if (EnemyHealth != null)
		{
			EnemyHealth.attackers.Remove(t.gameObject);
		}
		TargetObject = null;
		EnemyHealth = null;
		Timer = 0f;
		state.CURRENT_STATE = StateMachine.State.Idle;
	}
}
