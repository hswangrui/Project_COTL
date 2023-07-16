using System.Collections;
using Spine.Unity;
using UnityEngine;

public class EnemyHopperAOE : EnemyHopper
{
	[SerializeField]
	private bool shoot;

	[SerializeField]
	private GameObject bulletPrefab;

	[SerializeField]
	private float shootAnticipation;

	[SerializeField]
	private float bulletSpeed;

	[SerializeField]
	private float bulletSpeedRandomness;

	[SerializeField]
	private Vector2 timeBetweenBullets;

	[SerializeField]
	private int amountToShoot;

	[SerializeField]
	private float shootRadius;

	[SerializeField]
	private float shootChance = 0.3f;

	[SerializeField]
	private float shootAgainChance = 0.3f;

	[SerializeField]
	private bool doRageHop;

	[SerializeField]
	private float rageHopChance;

	[SerializeField]
	private float rageHopBulletSpeed;

	[SerializeField]
	private float rageHopAnticipation;

	[SerializeField]
	private int rageHopBulletAmount;

	[SerializeField]
	private Vector2 rageHopAmount;

	private bool shooting;

	public ParticleSystem aoeParticles;

	private bool shootAgain;

	protected override void UpdateStateIdle()
	{
		speed = 0f;
		if (gm.TimeSince(idleTimestamp) >= (idleDur - signPostParryWindow) * Spine[0].timeScale)
		{
			canBeParried = true;
		}
		float idleDur2 = idleDur;
		float signPostDur2 = signPostDur;
		if (!(gm.TimeSince(idleTimestamp) >= idleDur * Spine[0].timeScale) || shooting)
		{
			return;
		}
		if (targetObject == null && GetClosestTarget() != null)
		{
			targetObject = GetClosestTarget().gameObject;
		}
		TargetIsVisible();
		if (canLayEggs && gm.TimeSince(lastLaidEggTimestamp - 0.5f) >= minTimeBetweenEggs * Spine[0].timeScale && EnemyHopper.EnemyHoppers.Count < EnemyHopper.maxHoppersPerRoom && EnemyEgg.EnemyEggs.Count < EnemyHopper.maxEggsPerRoom)
		{
			alwaysTargetPlayer = false;
			isFleeing = true;
		}
		if (targetObject != null)
		{
			if (Vector3.Distance(base.transform.position, targetObject.transform.position) < 2f)
			{
				TargetAngle = GetFleeAngle();
			}
			else if (Random.Range(0f, 1f) > 0.4f)
			{
				TargetAngle = GetAngleToTarget();
			}
			else
			{
				TargetAngle = GetRandomFacingAngle();
			}
		}
		state.LookAngle = TargetAngle;
		state.facingAngle = TargetAngle;
		foreach (SkeletonAnimation item in Spine)
		{
			item.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
		}
		if (shootAgain || Random.Range(0f, 1f) < shootChance)
		{
			Shoot();
			return;
		}
		if (Random.Range(0f, 1f) < rageHopChance && doRageHop)
		{
			RageHop();
			return;
		}
		state.CURRENT_STATE = StateMachine.State.Moving;
		idleDur = signPostParryWindow;
	}

	protected override void UpdateStateMoving()
	{
		if (!_playedVO)
		{
			AudioManager.Instance.PlayOneShot(WarningVO, base.gameObject);
			_playedVO = true;
		}
		speed = hopSpeedCurve.Evaluate(gm.TimeSince(hoppingTimestamp) / hoppingDur) * (hopMoveSpeed / Spine[0].timeScale);
		Spine[0].transform.localPosition = -Vector3.forward * hopZCurve.Evaluate(gm.TimeSince(hoppingTimestamp) / hoppingDur) * hopZHeight * Spine[0].timeScale;
		if (gm.TimeSince(hoppingTimestamp) / (hoppingDur * Spine[0].timeScale) > 0.1f && gm.TimeSince(hoppingTimestamp) / (hoppingDur * Spine[0].timeScale) < 0.9f)
		{
			health.enabled = false;
		}
		else if (!health.enabled)
		{
			health.enabled = true;
		}
		canBeParried = false;
		SimpleSpineFlash.FlashWhite(1f - Mathf.Clamp01(gm.TimeSince(hoppingTimestamp) / (attackingDur * 0.5f * Spine[0].timeScale)));
		if (gm.TimeSince(hoppingTimestamp) >= hoppingDur / Spine[0].timeScale)
		{
			speed = 0f;
			DoAttack();
			_playedVO = false;
			if (ShouldStartCharging())
			{
				state.CURRENT_STATE = StateMachine.State.Charging;
				idleDur = signPostParryWindow;
			}
			else
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
				idleDur = signPostParryWindow;
			}
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		shooting = false;
	}

	protected override bool ShouldStartCharging()
	{
		if (base.ShouldStartCharging())
		{
			return state.CURRENT_STATE != StateMachine.State.Aiming;
		}
		return false;
	}

	private void Shoot()
	{
		StartCoroutine(ShootIE());
	}

	private IEnumerator ShootIE()
	{
		shooting = true;
		yield return new WaitForSeconds(0.5f);
		state.CURRENT_STATE = StateMachine.State.Aiming;
		foreach (SkeletonAnimation item in Spine)
		{
			if (item.AnimationState != null)
			{
				item.AnimationState.SetAnimation(0, "burp", false);
				item.AnimationState.AddAnimation(0, "idle", true, 0f);
			}
		}
		float t = 0f;
		while (t < shootAnticipation)
		{
			t += Time.deltaTime;
			SimpleSpineFlash.FlashWhite(t / shootAnticipation * 0.75f);
			yield return null;
		}
		SimpleSpineFlash.FlashWhite(false);
		for (int i = 0; i < amountToShoot; i++)
		{
			Vector3 vector = Random.insideUnitCircle * shootRadius;
			Projectile component = ObjectPool.Spawn(bulletPrefab, base.transform.parent).GetComponent<Projectile>();
			component.transform.position = base.transform.position;
			component.Angle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position + vector);
			component.team = health.team;
			component.Speed = bulletSpeed + Random.Range(0f - bulletSpeedRandomness, bulletSpeedRandomness);
			component.LifeTime = 4f + Random.Range(0f, 0.3f);
			component.Owner = health;
			if (timeBetweenBullets != Vector2.zero)
			{
				yield return new WaitForSeconds(Random.Range(timeBetweenBullets.x, timeBetweenBullets.y));
			}
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		shooting = false;
		if (Random.Range(0f, 1f) < shootAgainChance && !shootAgain)
		{
			shootAgain = true;
			idleDur = 0.5f;
		}
		else
		{
			shootAgain = false;
			idleDur = signPostParryWindow;
		}
	}

	private void RageHop()
	{
		StartCoroutine(RageHopIE());
	}

	private IEnumerator RageHopIE()
	{
		shooting = true;
		float t = 0f;
		while (t < rageHopAnticipation)
		{
			t += Time.deltaTime;
			SimpleSpineFlash.FlashWhite(t / rageHopAnticipation * 0.75f);
			yield return null;
		}
		SimpleSpineFlash.FlashWhite(false);
		foreach (SkeletonAnimation item in Spine)
		{
			item.AnimationState.SetAnimation(0, "angry", false);
		}
		yield return new WaitForSeconds(rageHopAnticipation);
		float a = 0f;
		for (int i = 0; (float)i < Random.Range(rageHopAmount.x, rageHopAmount.y + 1f); i++)
		{
			foreach (SkeletonAnimation item2 in Spine)
			{
				if (item2.AnimationState != null)
				{
					item2.AnimationState.SetAnimation(0, "jumpcombined", false);
					item2.AnimationState.AddAnimation(0, "idle", true, 0f);
				}
			}
			yield return new WaitForSeconds(0.5f);
			CameraManager.instance.ShakeCameraForDuration(1f, 2f, 0.25f);
			BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position + Vector3.back * 0.5f);
			int amount = rageHopBulletAmount;
			Health owner = health;
			Vector3 position = base.transform.position;
			float num = rageHopBulletSpeed;
			float angleOffset;
			a = (angleOffset = a + 45f);
			Projectile.CreateProjectiles(amount, owner, position, num, 1f, angleOffset);
			yield return new WaitForSeconds(0.4f);
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		shooting = false;
	}

	protected override void DoAttack()
	{
		base.DoAttack();
		if (aoeParticles != null)
		{
			aoeParticles.Play();
		}
		BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position + Vector3.back * 0.5f);
		CameraManager.shakeCamera(2f);
	}
}
