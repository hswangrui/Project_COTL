using System;
using UnityEngine;

public class WildLifeDangerous : UnitObject
{
	private float LookAround;

	private float FacingAngle;

	private float Timer;

	private SpriteRenderer spriteRenderer;

	private new Health TargetEnemy;

	public GameObject AttackCircle;

	public GameObject VisionCone;

	private float AttackSpeed;

	public float LoseEnemyRange = 999f;

	private void Start()
	{
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		LookAround = 90f;
		AttackCircle.SetActive(false);
		VisionCone.SetActive(false);
		ChangeState(StateMachine.State.Idle);
	}

	public override void Update()
	{
		base.Update();
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			state.facingAngle = FacingAngle + 45f * Mathf.Cos(LookAround += 0.005f * GameManager.DeltaTime);
			speed += (0f - speed) / 7f;
			AttackSpeed += (0f - AttackSpeed) / 7f;
			LookOutForDanger();
			break;
		case StateMachine.State.RaiseAlarm:
			AttackSpeed += (0f - AttackSpeed) / 7f;
			if ((Timer += Time.deltaTime) > 1f)
			{
				ChangeState(StateMachine.State.Moving);
				givePath(TargetEnemy.transform.position);
			}
			break;
		case StateMachine.State.Moving:
			if (TargetEnemy == null)
			{
				ChangeState(StateMachine.State.Idle);
				return;
			}
			if (Vector2.Distance(base.transform.position, TargetEnemy.transform.position) > LoseEnemyRange)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
				TargetEnemy = null;
				return;
			}
			if ((Timer += Time.deltaTime) > 1f)
			{
				Timer = 0f;
				givePath(TargetEnemy.transform.position);
			}
			if (Vector2.Distance(base.transform.position, TargetEnemy.transform.position) < 5f)
			{
				ChangeState(StateMachine.State.SignPostAttack);
			}
			break;
		case StateMachine.State.SignPostAttack:
			if (AttackSpeed > -5f)
			{
				AttackSpeed -= 0.2f;
			}
			if ((Timer += Time.deltaTime) > 0.5f)
			{
				ChangeState(StateMachine.State.Attacking);
			}
			break;
		case StateMachine.State.Attacking:
			if (AttackSpeed < 10f)
			{
				AttackSpeed += 1f;
			}
			if ((Timer += Time.deltaTime) > 0.7f)
			{
				ChangeState(StateMachine.State.RecoverFromAttack);
			}
			break;
		case StateMachine.State.RecoverFromAttack:
			AttackSpeed += (0f - AttackSpeed) / 15f;
			if ((Timer += Time.deltaTime) > 1f)
			{
				if (TargetEnemy == null)
				{
					ChangeState(StateMachine.State.Idle);
					break;
				}
				ChangeState(StateMachine.State.Moving);
				givePath(TargetEnemy.transform.position);
			}
			break;
		}
		vx = AttackSpeed * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f));
		vy = AttackSpeed * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f));
	}

	private void ChangeState(StateMachine.State newState)
	{
		Timer = 0f;
		AttackCircle.SetActive(false);
		VisionCone.SetActive(false);
		switch (newState)
		{
		case StateMachine.State.Idle:
			VisionCone.SetActive(true);
			break;
		case StateMachine.State.RaiseAlarm:
			spriteRenderer.color = Color.white;
			if (TargetEnemy.gameObject.tag == "Player")
			{
				GameManager.GetInstance().AddToCamera(base.gameObject);
			}
			break;
		case StateMachine.State.Moving:
			spriteRenderer.color = Color.white;
			break;
		case StateMachine.State.SignPostAttack:
			AttackSpeed = 0f;
			spriteRenderer.color = Color.white;
			state.facingAngle = Utils.GetAngle(base.transform.position, TargetEnemy.transform.position);
			break;
		case StateMachine.State.Attacking:
			AttackSpeed = 0f;
			AttackCircle.SetActive(true);
			spriteRenderer.color = Color.white;
			break;
		case StateMachine.State.RecoverFromAttack:
			AttackCircle.SetActive(true);
			spriteRenderer.color = Color.white;
			break;
		}
		state.CURRENT_STATE = newState;
	}

	private void LookOutForDanger()
	{
		spriteRenderer.color = Color.white;
		foreach (Health allUnit in Health.allUnits)
		{
			if (allUnit.team != health.team && Vector2.Distance(base.transform.position, allUnit.gameObject.transform.position) < 5f && allUnit.team != 0 && !allUnit.untouchable)
			{
				spriteRenderer.color = Color.yellow;
				float angle = Utils.GetAngle(base.transform.position, allUnit.gameObject.transform.position);
				if (angle < state.facingAngle + 45f && angle > state.facingAngle - 45f && CheckLineOfSight(allUnit.gameObject.transform.position, 5f))
				{
					TargetEnemy = allUnit;
					ChangeState(StateMachine.State.RaiseAlarm);
				}
			}
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		base.OnHit(Attacker, AttackLocation, AttackType);
		if (state.CURRENT_STATE != 0)
		{
			return;
		}
		TargetEnemy = Attacker.GetComponent<Health>();
		if (TargetEnemy == null)
		{
			foreach (Health allUnit in Health.allUnits)
			{
				if (allUnit.team != health.team && !allUnit.untouchable && Vector2.Distance(base.transform.position, allUnit.gameObject.transform.position) < 15f)
				{
					TargetEnemy = allUnit;
				}
			}
		}
		ChangeState(StateMachine.State.RaiseAlarm);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
		{
			AttackSpeed *= -0.4f;
		}
	}
}
