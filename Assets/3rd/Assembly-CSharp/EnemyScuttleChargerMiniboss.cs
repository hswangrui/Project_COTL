using System.Collections;
using Spine.Unity;
using UnityEngine;

public class EnemyScuttleChargerMiniboss : EnemyScuttleCharger
{
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string IntroAnimation;

	public float turningSpeedWhileCharging = 10f;

	protected bool IsTargetingPlayer;

	public GameObject Arrow;

	private float ShootDelay;

	public int ShotsToFire = 3;

	public float TimeBetweenShoots = 1f;

	public override void OnEnable()
	{
		base.OnEnable();
		if (StartHidden == StartingStates.Intro)
		{
			StartCoroutine(IntroRoutine());
		}
	}

	private IEnumerator IntroRoutine()
	{
		yield return new WaitForEndOfFrame();
		health.enabled = false;
		Spine.AnimationState.SetAnimation(0, IntroAnimation, false);
		Spine.AnimationState.AddAnimation(0, IdleAnimation, true, 0f);
		AttackDelay = 0f;
		yield return new WaitForSeconds(1.5f);
		StartCoroutine(ActiveRoutine());
	}

	protected override IEnumerator AttackRoutine()
	{
		Attacking = true;
		GetNewTarget();
		Spine.AnimationState.SetAnimation(0, SignPostAttackAnimation, LoopSignPostAttackAnimation);
		state.CURRENT_STATE = StateMachine.State.SignPostAttack;
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < SignPostAttackDuration))
			{
				break;
			}
			if (TargetObject != null)
			{
				state.LookAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
			}
			SimpleSpineFlash[] simpleSpineFlashes = SimpleSpineFlashes;
			for (int i = 0; i < simpleSpineFlashes.Length; i++)
			{
				simpleSpineFlashes[i].FlashWhite(Progress / SignPostAttackDuration);
			}
			state.facingAngle = state.LookAngle;
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
		Spine.AnimationState.SetAnimation(0, AttackAnimation, false);
		Spine.AnimationState.AddAnimation(0, IdleAnimation, true, 0f);
		float timer = 0f;
		damageColliderEvents.SetActive(true);
		IsCharging = true;
		DisableKnockback = true;
		IsTargetingPlayer = true;
		while (timer < AttackDuration)
		{
			if (!IsCharging)
			{
				SimpleSpineFlash[] simpleSpineFlashes = SimpleSpineFlashes;
				for (int i = 0; i < simpleSpineFlashes.Length; i++)
				{
					simpleSpineFlashes[i].FlashWhite(0f);
				}
				yield break;
			}
			if (IsTargetingPlayer)
			{
				TurnTowardsTarget();
			}
			maxSpeed = Mathf.Lerp(maxSpeed, ChargeSpeed, Time.deltaTime * ChargeAcceleration);
			speed = maxSpeed;
			if (timer > AttackDuration * 0.9f)
			{
				damageColliderEvents.SetActive(false);
				SimpleSpineFlash[] simpleSpineFlashes = SimpleSpineFlashes;
				for (int i = 0; i < simpleSpineFlashes.Length; i++)
				{
					simpleSpineFlashes[i].FlashWhite(0f);
				}
			}
			else
			{
				SimpleSpineFlash[] simpleSpineFlashes = SimpleSpineFlashes;
				for (int i = 0; i < simpleSpineFlashes.Length; i++)
				{
					simpleSpineFlashes[i].FlashWhite(0.5f);
				}
			}
			timer += Time.deltaTime;
			yield return null;
		}
		EndCharge();
	}

	protected override void ChargeIntoWall()
	{
		base.ChargeIntoWall();
		CameraManager.shakeCamera(1f, state.LookAngle);
		StartCoroutine(ShootArrowRoutine());
	}

	private void TurnTowardsTarget()
	{
		if (!(TargetObject == null))
		{
			float num = turningSpeedWhileCharging;
			if (Mathf.Abs(state.LookAngle - Utils.GetAngle(base.transform.position, TargetObject.transform.position)) > 180f)
			{
				num /= 2f;
			}
			state.LookAngle = Mathf.LerpAngle(state.LookAngle, Utils.GetAngle(base.transform.position, TargetObject.transform.position), Time.deltaTime * num);
			state.facingAngle = state.LookAngle;
		}
	}

	protected override void OnDamageTriggerEnter(Collider2D collider)
	{
		EnemyHealth = collider.GetComponent<Health>();
		if (EnemyHealth != null && (EnemyHealth.team != health.team || health.team == Health.Team.PlayerTeam) && EnemyHealth.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, EnemyHealth.transform.position, 0.7f)))
		{
			IsTargetingPlayer = false;
		}
	}

	private IEnumerator ShootArrowRoutine()
	{
		int i = ShotsToFire;
		float angle = 0f;
		while (true)
		{
			int num = i - 1;
			i = num;
			if (num < 0)
			{
				break;
			}
			float Progress = 0f;
			while (true)
			{
				float num2;
				Progress = (num2 = Progress + Time.deltaTime);
				if (!(num2 < TimeBetweenShoots))
				{
					break;
				}
				yield return null;
			}
			Projectile component = ObjectPool.Spawn(Arrow, base.transform.parent).GetComponent<Projectile>();
			component.transform.position = base.transform.position;
			component.Angle = angle;
			component.team = health.team;
			component.Speed = 6f;
			component.Owner = health;
			angle += 360f / ((float)ShotsToFire + 1f);
		}
	}
}
