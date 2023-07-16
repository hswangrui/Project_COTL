using System;
using System.Collections;
using UnityEngine;

public class EnemyChaserDasher : EnemyChaser
{
	[SerializeField]
	private float attackDistance;

	[SerializeField]
	private float dashChargeDur;

	[SerializeField]
	private float dashStrength;

	[SerializeField]
	private float dashCooldown;

	[SerializeField]
	private float damageDuration = 0.2f;

	private new ColliderEvents damageColliderEvents;

	private float attackTimer;

	private bool attacking;

	public override void Awake()
	{
		base.Awake();
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEvents.SetActive(false);
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		Charge();
	}

	private void Charge()
	{
		if (state.CURRENT_STATE != StateMachine.State.Charging)
		{
			attackTimer = 0f;
			state.CURRENT_STATE = StateMachine.State.Charging;
		}
	}

	public override void Update()
	{
		base.Update();
		if (attacking)
		{
			return;
		}
		if (state.CURRENT_STATE == StateMachine.State.Charging)
		{
			attackTimer += Time.deltaTime;
			float num = attackTimer / dashChargeDur;
			simpleSpineFlash.FlashWhite(num * 0.75f);
			if (num > 1f)
			{
				Attack();
			}
		}
		if ((bool)targetObject && Vector3.Distance(base.transform.position, targetObject.transform.position) < attackDistance)
		{
			Charge();
		}
	}

	protected override void UpdateMoving()
	{
		if (state.CURRENT_STATE != StateMachine.State.Charging && !attacking)
		{
			base.UpdateMoving();
		}
	}

	private void Attack()
	{
		if (!attacking)
		{
			attackTimer = 0f;
			attacking = true;
			simpleSpineFlash.FlashWhite(false);
			ClearPaths();
			float angle = Utils.GetAngle(base.transform.position, targetObject.transform.position) * ((float)Math.PI / 180f);
			DoKnockBack(angle, dashStrength, dashCooldown);
			StartCoroutine(TurnOnDamageColliderForDuration(damageDuration));
			StartCoroutine(AttackCooldownIE());
		}
	}

	private IEnumerator AttackCooldownIE()
	{
		yield return new WaitForEndOfFrame();
		simpleSpineFlash.FlashWhite(false);
		yield return new WaitForSeconds(dashCooldown);
		attacking = false;
		state.CURRENT_STATE = StateMachine.State.Idle;
	}

	private new void OnDamageTriggerEnter(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && (component.team != health.team || health.team == Health.Team.PlayerTeam))
		{
			component.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.7f));
		}
	}

	private IEnumerator TurnOnDamageColliderForDuration(float duration)
	{
		damageColliderEvents.SetActive(true);
		yield return new WaitForSeconds(duration);
		damageColliderEvents.SetActive(false);
	}
}
