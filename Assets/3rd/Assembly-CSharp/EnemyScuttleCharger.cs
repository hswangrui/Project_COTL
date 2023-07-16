using System;
using System.Collections;
using UnityEngine;

public class EnemyScuttleCharger : EnemyScuttleSwiper
{
	public float IdleSpeed = 0.1f;

	public float ChargeSpeed = 0.1f;

	public float ChargeAcceleration = 0.1f;

	public float StunnedDuration = 1f;

	protected bool IsCharging;

	public float KnockBackModifier = 1f;

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (IsCharging && ((int)layerToCheck & (1 << collision.gameObject.layer)) != 0)
		{
			ChargeIntoWall();
		}
	}

	protected override IEnumerator AttackRoutine()
	{
		Attacking = true;
		ClearPaths();
		speed = 0f;
		Spine.AnimationState.SetAnimation(0, SignPostAttackAnimation, LoopSignPostAttackAnimation);
		state.CURRENT_STATE = StateMachine.State.SignPostAttack;
		float Progress = 0f;
		SimpleSpineFlash[] simpleSpineFlashes;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * Spine.timeScale);
			if (!(num < SignPostAttackDuration))
			{
				break;
			}
			if (TargetObject != null && Progress < SignPostAttackDuration - 0.2f)
			{
				state.LookAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
			}
			simpleSpineFlashes = SimpleSpineFlashes;
			for (int i = 0; i < simpleSpineFlashes.Length; i++)
			{
				simpleSpineFlashes[i].FlashWhite(Progress / SignPostAttackDuration);
			}
			state.facingAngle = state.LookAngle;
			yield return null;
		}
		simpleSpineFlashes = SimpleSpineFlashes;
		for (int i = 0; i < simpleSpineFlashes.Length; i++)
		{
			simpleSpineFlashes[i].FlashWhite(false);
		}
		state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
		Spine.AnimationState.SetAnimation(0, AttackAnimation, false);
		Spine.AnimationState.AddAnimation(0, IdleAnimation, true, 0f);
		float timer = 0f;
		damageColliderEvents.SetActive(true);
		DisableKnockback = true;
		IsCharging = true;
		while (timer < AttackDuration)
		{
			if (!IsCharging)
			{
				simpleSpineFlashes = SimpleSpineFlashes;
				for (int i = 0; i < simpleSpineFlashes.Length; i++)
				{
					simpleSpineFlashes[i].FlashWhite(0f);
				}
				yield break;
			}
			maxSpeed = Mathf.Lerp(maxSpeed, ChargeSpeed, Time.deltaTime * ChargeAcceleration);
			speed = maxSpeed;
			if (timer > AttackDuration * 0.9f)
			{
				damageColliderEvents.SetActive(false);
				simpleSpineFlashes = SimpleSpineFlashes;
				for (int i = 0; i < simpleSpineFlashes.Length; i++)
				{
					simpleSpineFlashes[i].FlashWhite(0f);
				}
			}
			else
			{
				simpleSpineFlashes = SimpleSpineFlashes;
				for (int i = 0; i < simpleSpineFlashes.Length; i++)
				{
					simpleSpineFlashes[i].FlashWhite(0.8f);
				}
			}
			timer += Time.deltaTime;
			yield return null;
		}
		EndCharge();
	}

	protected override bool ShouldAttack()
	{
		if (IsStunned)
		{
			return false;
		}
		Vector3 vector = Camera.main.WorldToViewportPoint(base.transform.position);
		if (!(vector.x > 0f) || !(vector.x < 1f) || !(vector.y > 0f) || !(vector.y < 1f))
		{
			return false;
		}
		return base.ShouldAttack();
	}

	protected virtual void ChargeIntoWall()
	{
		CameraManager.shakeCamera(0.2f, state.LookAngle);
		if (!IsStunned)
		{
			StartCoroutine(StunnedRoutine());
		}
		EndCharge();
	}

	protected void EndCharge()
	{
		IsCharging = false;
		DisableKnockback = false;
		damageColliderEvents.SetActive(false);
		maxSpeed = IdleSpeed;
		state.CURRENT_STATE = StateMachine.State.Idle;
		IdleWait = StunnedDuration;
		AttackDelay = AttackDelayTime;
		TargetObject = null;
		Attacking = false;
	}

	protected void Stop()
	{
	}

	protected IEnumerator StunnedRoutine()
	{
		IsStunned = true;
		StartCoroutine(ApplyForceRoutine(base.transform.position + new Vector3(Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)), Mathf.Sin(state.facingAngle) * ((float)Math.PI / 180f))));
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < StunnedDuration))
			{
				break;
			}
			yield return null;
		}
		IsStunned = false;
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		IsStunned = false;
		if (!DisableKnockback && AttackType != Health.AttackTypes.NoKnockBack)
		{
			DoKnockBack(Attacker, KnockBackModifier, 0.5f);
		}
	}
}
