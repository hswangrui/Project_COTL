using System.Collections.Generic;
using UnityEngine;

public class Task_Bat : Task
{
	private Health health;

	private Health EnemyHealth;

	public bool CannotLoseTarget;

	private float DetectEnemyRange;

	private float AttackRange;

	private float LoseEnemyRange;

	private float PreAttackDuration;

	private float PostAttackDuration;

	private float MinimumRange;

	private float DefaultMinimumRange;

	private float AttackDelay;

	private MeshRenderer meshRenderer;

	private MaterialPropertyBlock block;

	private int fillAlpha;

	private int fillColor;

	private float FillAlpha;

	private GameObject TrailRenderer;

	public ColliderEvents damageColliderEvents;

	private List<Collider2D> collider2DList;

	private float AttackAngle;

	private float AttackSpeed = 20f;

	private float AttackVel;

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		state.CURRENT_STATE = StateMachine.State.Idle;
		health = t.gameObject.GetComponent<Health>();
		health.OnDie += OnDie;
		health.OnHit += OnHit;
		meshRenderer = t.GetComponentInChildren<MeshRenderer>();
		block = new MaterialPropertyBlock();
		meshRenderer.SetPropertyBlock(block);
		fillAlpha = Shader.PropertyToID("_FillAlpha");
		fillColor = Shader.PropertyToID("_FillColor");
		block.SetFloat(fillAlpha, FillAlpha = 0f);
		block.SetColor(fillColor, Color.white);
		meshRenderer.SetPropertyBlock(block);
	}

	private void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		FillAlpha = 0f;
		block.SetFloat(fillAlpha, FillAlpha);
		meshRenderer.SetPropertyBlock(block);
	}

	public void Init(float DetectEnemyRange, float AttackRange, float LoseEnemyRange, float PreAttackDuration, float PostAttackDuration, float MinimumRange, GameObject TrailRenderer, ColliderEvents damageColliderEvents)
	{
		this.DetectEnemyRange = DetectEnemyRange;
		this.AttackRange = AttackRange;
		this.LoseEnemyRange = LoseEnemyRange;
		this.PreAttackDuration = PreAttackDuration;
		this.PostAttackDuration = PostAttackDuration;
		DefaultMinimumRange = (this.MinimumRange = MinimumRange);
		this.TrailRenderer = TrailRenderer;
		this.damageColliderEvents = damageColliderEvents;
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
		}
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
		AttackDelay -= Time.deltaTime;
		if (TargetObject != null)
		{
			switch (state.CURRENT_STATE)
			{
			case StateMachine.State.Idle:
			{
				damageColliderEvents.SetActive(false);
				float num = Vector2.Distance(TargetPosition(), t.transform.position);
				if (AttackDelay < 0f)
				{
					if (num < AttackRange)
					{
						Timer = 0f;
						state.facingAngle = Utils.GetAngle(t.transform.position, TargetObject.transform.position);
						state.CURRENT_STATE = StateMachine.State.SignPostAttack;
					}
					else
					{
						Timer = 0f;
						PathToPosition(TargetPosition());
						MinimumRange = DefaultMinimumRange + (float)Random.Range(-2, 2);
					}
				}
				break;
			}
			case StateMachine.State.Moving:
			{
				damageColliderEvents.SetActive(false);
				float num = Vector2.Distance(TargetPosition(), t.transform.position);
				if ((num > LoseEnemyRange && !CannotLoseTarget) || num < MinimumRange)
				{
					state.CURRENT_STATE = StateMachine.State.Idle;
					if (!CannotLoseTarget)
					{
						ClearTarget();
					}
				}
				else if ((Timer += Time.deltaTime) > 1f)
				{
					Timer = 0f;
					if (!CannotLoseTarget)
					{
						ClearTarget();
						GetNewTarget();
					}
					PathToPosition(TargetPosition());
				}
				break;
			}
			case StateMachine.State.SignPostAttack:
				damageColliderEvents.SetActive(false);
				Timer += Time.deltaTime;
				if (Timer > PreAttackDuration / 2f)
				{
					state.facingAngle = Utils.GetAngle(t.transform.position, TargetObject.transform.position);
					if (t.speed > -2f)
					{
						t.speed -= 0.01f;
					}
				}
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
				if (AttackVel > 0f)
				{
					AttackVel -= 0.1f * GameManager.DeltaTime;
				}
				t.speed = AttackVel * Time.deltaTime;
				damageColliderEvents.SetActive(true);
				if (FillAlpha > 0f)
				{
					FillAlpha -= 3f * Time.deltaTime;
					block.SetFloat(fillAlpha, FillAlpha);
					meshRenderer.SetPropertyBlock(block);
				}
				if ((Timer += Time.deltaTime) > 0.4f)
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
			damageColliderEvents.SetActive(false);
			GetNewTarget();
			if (TargetObject == null)
			{
				ClearTask();
			}
		}
	}

	public override void DoAttack(float AttackRange, StateMachine.State NextState = StateMachine.State.RecoverFromAttack)
	{
		block.SetFloat(fillAlpha, FillAlpha = 1f);
		if (meshRenderer.isVisible)
		{
			CameraManager.shakeCamera(0.3f, Utils.GetAngle(t.transform.position, TargetObject.transform.position));
		}
		meshRenderer.SetPropertyBlock(block);
		Timer = 0f;
		state.CURRENT_STATE = NextState;
		AttackAngle = Utils.GetAngle(t.transform.position, TargetObject.transform.position);
		AttackDelay = 2f;
		AttackVel = AttackSpeed;
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
		health.OnHit -= OnHit;
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent -= OnDamageTriggerEnter;
		}
	}

	public override void ClearTask()
	{
		health.OnDie -= OnDie;
		health.OnHit -= OnHit;
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

	private void OnDamageTriggerEnter(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && component.team != health.team)
		{
			component.DealDamage(1f, t.gameObject, Vector3.Lerp(t.transform.position, component.transform.position, 0.7f));
		}
	}

	public void OnCollisionEnter2D(Collision2D collision)
	{
		if (state.CURRENT_STATE == StateMachine.State.RecoverFromAttack && (collision.gameObject.layer == LayerMask.NameToLayer("Obstacles") || collision.gameObject.layer == LayerMask.NameToLayer("Island")))
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
			t.speed = 0f;
			block.SetFloat(fillAlpha, FillAlpha = 0f);
			meshRenderer.SetPropertyBlock(block);
		}
	}
}
