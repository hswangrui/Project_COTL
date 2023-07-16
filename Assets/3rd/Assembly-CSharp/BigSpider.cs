using System;
using UnityEngine;

public class BigSpider : UnitObject
{
	private float Timer;

	private float TargetAngle;

	public float WalkSpeed = 0.02f;

	public float RunSpeed = 0.07f;

	private float IgnorePlayer;

	public MonsterHive MonsterDen;

	private Animator animator;

	private Worshipper worshipper;

	private Worshipper TargetWorshipper;

	private bool Stealing;

	private void Start()
	{
		Timer = UnityEngine.Random.Range(1, 5);
		animator = GetComponentInChildren<Animator>();
	}

	public override void Update()
	{
		base.Update();
		if (!Stealing)
		{
			WonderFreely();
		}
		else if (MonsterDen != null)
		{
			StealPickUp();
		}
		else
		{
			Stealing = false;
		}
	}

	private void WonderFreely()
	{
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			UsePathing = false;
			animator.speed = 1f;
			if ((Timer -= Time.deltaTime) < 0f)
			{
				if (MonsterDen != null && worshipper == null)
				{
					foreach (Worshipper worshipper in Worshipper.worshippers)
					{
						if (!worshipper.BeingCarried && Vector3.Distance(worshipper.transform.position, base.transform.position) < 6f)
						{
							TargetWorshipper = worshipper;
							Stealing = true;
							return;
						}
					}
				}
				animator.SetTrigger("WALK");
				Timer = UnityEngine.Random.Range(1, 5);
				TargetAngle = UnityEngine.Random.Range(0, 360);
				state.CURRENT_STATE = StateMachine.State.Moving;
			}
			else
			{
				LookForDanger();
			}
			break;
		case StateMachine.State.Moving:
			animator.speed = 1f;
			state.facingAngle += Mathf.Atan2(Mathf.Sin((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f))) * 57.29578f / (15f * GameManager.DeltaTime);
			if ((Timer -= Time.deltaTime) < 0f)
			{
				animator.SetTrigger("IDLE");
				Timer = UnityEngine.Random.Range(1, 5);
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			else
			{
				LookForDanger();
			}
			break;
		case StateMachine.State.Fleeing:
			animator.speed = 1.5f;
			state.facingAngle += Mathf.Atan2(Mathf.Sin((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f))) * 57.29578f / (15f * GameManager.DeltaTime);
			if (TargetEnemy == null || Vector3.Distance(base.transform.position, TargetEnemy.transform.position) > 5f)
			{
				animator.SetTrigger("IDLE");
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

	private void StealPickUp()
	{
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
		{
			UsePathing = true;
			animator.speed = 1f;
			Vector3 vector = (Vector3)AstarPath.active.GetNearest(TargetWorshipper.transform.position, UnitObject.constraint).node.position;
			givePath(vector);
			animator.SetTrigger("WALK");
			break;
		}
		case StateMachine.State.Moving:
			animator.speed = 1f;
			maxSpeed = RunSpeed;
			if (TargetWorshipper == null)
			{
				animator.SetTrigger("IDLE");
				state.CURRENT_STATE = StateMachine.State.Idle;
				Stealing = false;
			}
			else if (Vector3.Distance(base.transform.position, TargetWorshipper.transform.position) < 0.5f)
			{
				worshipper = TargetWorshipper;
				worshipper.PickUp();
				ClearPaths();
				UsePathing = true;
				givePath(MonsterDen.HoomanTrap.transform.position);
				EndOfPath = (System.Action)Delegate.Combine(EndOfPath, new System.Action(TrapHooman));
				animator.SetTrigger("WALK");
				AudioManager.Instance.PlayOneShot("event:/enemy/vocals/spider_small/warning", base.gameObject);
				state.CURRENT_STATE = StateMachine.State.Fleeing;
			}
			else if ((Timer += Time.deltaTime) > 1f)
			{
				Timer = 0f;
				Vector3 vector = (Vector3)AstarPath.active.GetNearest(TargetWorshipper.transform.position, UnitObject.constraint).node.position;
				givePath(vector);
			}
			break;
		case StateMachine.State.Fleeing:
			if (worshipper != null)
			{
				worshipper.transform.position = base.transform.position + new Vector3(0f, 0f, -0.3f);
			}
			animator.speed = 1.7f;
			maxSpeed = RunSpeed;
			break;
		case StateMachine.State.CustomAction0:
			if ((Timer += Time.deltaTime) > 0.1f)
			{
				givePath(MonsterDen.Den.transform.position);
				EndOfPath = (System.Action)Delegate.Combine(EndOfPath, new System.Action(HideInDen));
				AudioManager.Instance.PlayOneShot("event:/enemy/vocals/spider_small/warning", base.gameObject);
				state.CURRENT_STATE = StateMachine.State.Fleeing;
				animator.SetTrigger("WALK");
			}
			break;
		}
	}

	private void TrapHooman()
	{
		MonsterDen.HoomanTrap.GetComponent<ScaleBounce>().SquishMe(0.2f, -0.2f);
		MonsterDen.worshipper = worshipper;
		worshipper.CapturedByBigSpider();
		worshipper = null;
		EndOfPath = (System.Action)Delegate.Remove(EndOfPath, new System.Action(TrapHooman));
		Timer = 0f;
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		animator.SetTrigger("IDLE");
	}

	private void HideInDen()
	{
		MonsterDen.Den.GetComponent<ScaleBounce>().SquishMe(0.2f, -0.2f);
		Stealing = false;
		UnityEngine.Object.Destroy(base.gameObject);
		EndOfPath = (System.Action)Delegate.Remove(EndOfPath, new System.Action(HideInDen));
	}

	private void LookForDanger()
	{
		foreach (Health allUnit in Health.allUnits)
		{
			if (allUnit.team == Health.Team.PlayerTeam && Vector2.Distance(base.transform.position, allUnit.gameObject.transform.position) < 2.5f && allUnit.team != 0 && !allUnit.untouchable)
			{
				TargetEnemy = allUnit;
				TargetAngle = Utils.GetAngle(allUnit.transform.position, base.transform.position);
				maxSpeed = RunSpeed;
				if (state.CURRENT_STATE == StateMachine.State.Idle)
				{
					animator.SetTrigger("WALK");
				}
				AudioManager.Instance.PlayOneShot("event:/enemy/vocals/spider_small/warning", base.gameObject);
				state.CURRENT_STATE = StateMachine.State.Fleeing;
			}
		}
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		TargetAngle = state.facingAngle + 90f;
		IgnorePlayer = 2f;
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		EndOfPath = (System.Action)Delegate.Remove(EndOfPath, new System.Action(TrapHooman));
		EndOfPath = (System.Action)Delegate.Remove(EndOfPath, new System.Action(HideInDen));
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		if (worshipper != null)
		{
			worshipper.DropMe();
		}
	}
}
