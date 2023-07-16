using System;
using UnityEngine;

public class Critter : UnitObject
{
	public float DangerDistance = 3f;

	private float Timer;

	private float TargetAngle;

	public float WalkSpeed = 0.02f;

	public float RunSpeed = 0.07f;

	private float IgnorePlayer;

	public bool FleeNearEnemies = true;

	public bool EatGrass;

	public bool WonderAround = true;

	private void Start()
	{
		Timer = UnityEngine.Random.Range(1, 5);
	}

	public override void Update()
	{
		base.Update();
		WonderFreely();
	}

	private void WonderFreely()
	{
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			UsePathing = false;
			if (WonderAround)
			{
				if ((Timer -= Time.deltaTime) < 0f)
				{
					Timer = UnityEngine.Random.Range(1, 5);
					TargetAngle = UnityEngine.Random.Range(0, 360);
					state.CURRENT_STATE = StateMachine.State.Moving;
				}
				else
				{
					LookForDanger();
				}
			}
			if (EatGrass && (Timer -= Time.deltaTime) < 0f)
			{
				TargetAngle = UnityEngine.Random.Range(0, 360);
				Timer = UnityEngine.Random.Range(4, 6);
				state.CURRENT_STATE = StateMachine.State.CustomAction0;
			}
			break;
		case StateMachine.State.CustomAction0:
			if ((Timer -= Time.deltaTime) < 0f)
			{
				Timer = UnityEngine.Random.Range(1, 3);
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			else
			{
				LookForDanger();
			}
			break;
		case StateMachine.State.Moving:
			state.facingAngle += Mathf.Atan2(Mathf.Sin((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f))) * 57.29578f / (12f * GameManager.DeltaTime);
			if ((Timer -= Time.deltaTime) < 0f)
			{
				if (EatGrass)
				{
					Timer = UnityEngine.Random.Range(4, 6);
					state.CURRENT_STATE = StateMachine.State.CustomAction0;
				}
				else
				{
					Timer = UnityEngine.Random.Range(1, 5);
					state.CURRENT_STATE = StateMachine.State.Idle;
				}
			}
			else
			{
				LookForDanger();
			}
			break;
		case StateMachine.State.Fleeing:
			state.facingAngle += Mathf.Atan2(Mathf.Sin((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f))) * 57.29578f / (15f * GameManager.DeltaTime);
			if (TargetEnemy == null || Vector3.Distance(base.transform.position, TargetEnemy.transform.position) > 5f)
			{
				maxSpeed = WalkSpeed;
				Timer = UnityEngine.Random.Range(1, 5);
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			else if (Vector3.Distance(base.transform.position, TargetEnemy.transform.position) <= 3f && (IgnorePlayer -= Time.deltaTime) < 0f)
			{
				TargetAngle = Utils.GetAngle(TargetEnemy.transform.position, base.transform.position);
			}
			break;
		}
	}

	private void LookForDanger()
	{
		if (!FleeNearEnemies)
		{
			return;
		}
		foreach (Health allUnit in Health.allUnits)
		{
			if (allUnit.team == Health.Team.PlayerTeam && Vector2.Distance(base.transform.position, allUnit.gameObject.transform.position) < DangerDistance && allUnit.team != 0 && !allUnit.untouchable)
			{
				TargetEnemy = allUnit;
				TargetAngle = Utils.GetAngle(allUnit.transform.position, base.transform.position);
				maxSpeed = RunSpeed;
				state.CURRENT_STATE = StateMachine.State.Fleeing;
			}
		}
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		TargetAngle = state.facingAngle + 90f;
		IgnorePlayer = 2f;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
	}
}
