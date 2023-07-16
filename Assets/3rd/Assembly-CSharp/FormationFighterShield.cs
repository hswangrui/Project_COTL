using System;
using UnityEngine;

public class FormationFighterShield : FormationFighter
{
	private float VulnerableTimer;

	private Task_Combat_Shield ShieldTask;

	private UnitObject AttackerUO;

	private void Start()
	{
		health.OnHitEarly += OnHitEarly;
	}

	public override void SetTask()
	{
		AddNewTask(Task_Type.SHIELD, false);
		ShieldTask = CurrentTask as Task_Combat_Shield;
		ShieldTask.Init(DetectEnemyRange, AttackRange, LoseEnemyRange, PreAttackDuration, PostAttackDuration, DefendingDuration, this);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		health.OnHitEarly -= OnHitEarly;
		if (ShieldTask != null)
		{
			ShieldTask.ClearTarget();
		}
	}

	private void OnHitEarly(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		health.DamageModifier = 1f;
		if (state.CURRENT_STATE != StateMachine.State.Defending && (state.CURRENT_STATE == StateMachine.State.Defending || state.CURRENT_STATE == StateMachine.State.Vulnerable || state.CURRENT_STATE == StateMachine.State.RecoverFromAttack || state.CURRENT_STATE == StateMachine.State.RecoverFromCounterAttack))
		{
			return;
		}
		health.DamageModifier = 0f;
		GameManager.GetInstance().HitStop();
		CameraManager.shakeCamera(0.5f, Utils.GetAngle(AttackLocation, base.transform.position));
		if (Attacker != null)
		{
			AttackerUO = Attacker.GetComponent<UnitObject>();
			if (AttackerUO != null)
			{
				AttackerUO.knockBackVX = 0.7f * Mathf.Cos(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
				AttackerUO.knockBackVY = 0.7f * Mathf.Sin(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
			}
		}
		if (state.CURRENT_STATE != StateMachine.State.Defending)
		{
			state.CURRENT_STATE = StateMachine.State.Defending;
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (state.CURRENT_STATE != StateMachine.State.Defending)
		{
			base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
			if (state.CURRENT_STATE == StateMachine.State.RecoverFromAttack)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
		}
	}

	public override void SpecialStates()
	{
		if (state.CURRENT_STATE == StateMachine.State.Defending && (DefendTimer += Time.deltaTime) >= DefendingDuration)
		{
			ShieldTask.DoAttack(AttackRange, StateMachine.State.RecoverFromCounterAttack);
			DefendTimer = 0f;
		}
		if (state.CURRENT_STATE == StateMachine.State.Vulnerable && (VulnerableTimer += Time.deltaTime) >= 1f)
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
			VulnerableTimer = 0f;
		}
	}
}
