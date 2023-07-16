using System;
using System.Collections;
using System.Collections.Generic;
using MMBiomeGeneration;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyBatBurp : EnemyBat
{
	public int ShotsToFire = 10;

	public float DetectEnemyRange = 10f;

	public GameObject projectilePrefab;

	protected float LookAngle;

	protected float lastBurpedFliesTimestamp;

	protected float minTimeBetweenBurps = 7f;

	protected float chargingTimestamp;

	public float chargingDuration = 0.5f;

	[SerializeField]
	private bool projectileTrail;

	[SerializeField]
	private GameObject bulletPrefab;

	[SerializeField]
	private LayerMask wallMask;

	[SerializeField]
	private float projectileSpeed;

	[SerializeField]
	private float projectileAcceleration;

	[SerializeField]
	private float projectileMoveSpeed;

	[SerializeField]
	private float distanceBetweenProjectiles;

	[SerializeField]
	private float timeBetweenProjectileTrails;

	[SerializeField]
	private bool spawning;

	[SerializeField]
	private float timeBetweenSpawning;

	[SerializeField]
	private Vector2 spawningAmount;

	[SerializeField]
	private int maxEnemies;

	[SerializeField]
	private float spawnForce;

	[SerializeField]
	private AssetReferenceGameObject[] spawnables;

	[SerializeField]
	private bool flyAcrossScreen;

	[SerializeField]
	private float flyAcrossScreenSpeed;

	[SerializeField]
	private Vector2 flyAcrossScreenAmount;

	[SerializeField]
	private float timeBetweenFlyAcrossScreenAmount;

	public ParticleSystem flyParticles;

	private float lastProjectileTrailTime;

	private float lastSurroundingFliesTime;

	private float lastFlyAcrossScreenTime;

	private bool fleeing;

	protected List<Projectile> activeProjectiles = new List<Projectile>();

	public override void OnEnable()
	{
		base.OnEnable();
		health.invincible = false;
	}

	protected override IEnumerator ActiveRoutine()
	{
		yield return new WaitForEndOfFrame();
		while (true)
		{
			float num = turningSpeed;
			if (!ChasingPlayer)
			{
				state.LookAngle = state.facingAngle;
				if (GameManager.RoomActive && GetClosestTarget() != null && Vector3.Distance(base.transform.position, GetClosestTarget().transform.position) < noticePlayerDistance)
				{
					if (!NoticedPlayer)
					{
						if (!string.IsNullOrEmpty(WarningVO))
						{
							AudioManager.Instance.PlayOneShot(WarningVO, base.gameObject);
						}
						warningIcon.AnimationState.SetAnimation(0, "warn-start", false);
						warningIcon.AnimationState.AddAnimation(0, "warn-stop", false, 2f);
						NoticedPlayer = true;
					}
					maxSpeed = ChaseSpeed;
					ChasingPlayer = true;
				}
			}
			else
			{
				if (!fleeing)
				{
					if (GetClosestTarget() == null || Vector3.Distance(base.transform.position, GetClosestTarget().transform.position) > 12f)
					{
						TargetPosition = StartingPosition.Value;
						maxSpeed = IdleSpeed;
						ChasingPlayer = false;
					}
					else
					{
						TargetPosition = GetClosestTarget().transform.position;
					}
				}
				state.LookAngle = Utils.GetAngle(base.transform.position, TargetPosition);
				if ((AttackCoolDown -= Time.deltaTime) < 0f)
				{
					if (ShouldStartCharging() && UnityEngine.Random.value > 0.5f)
					{
						StartCoroutine(ChargingRoutine());
						yield break;
					}
					if (ShouldTrail() && UnityEngine.Random.value > 0.5f)
					{
						StartCoroutine(ProjectileTrailRoutine());
						yield break;
					}
					if (ShouldAttack() && UnityEngine.Random.value > 0.5f)
					{
						CurrentAttackNum = 0;
						StartCoroutine(AttackRoutine());
						yield break;
					}
					if (ShouldSpawnEnemiess() && UnityEngine.Random.value > 0.5f)
					{
						CurrentAttackNum = 0;
						StartCoroutine(SpawnEnemuesIE());
						yield break;
					}
					if (ShouldFlyAcrossScreenAttack() && UnityEngine.Random.value > 0.5f)
					{
						break;
					}
				}
			}
			Angle = Mathf.LerpAngle(Angle, Utils.GetAngle(base.transform.position, TargetPosition), Time.deltaTime * num);
			if (GameManager.GetInstance() != null && angleNoiseAmplitude > 0f && angleNoiseFrequency > 0f && Vector3.Distance(TargetPosition, base.transform.position) < MaximumRange)
			{
				Angle += (-0.5f + Mathf.PerlinNoise(GameManager.GetInstance().TimeSince(timestamp) * angleNoiseFrequency, 0f)) * angleNoiseAmplitude * (float)RanDirection;
			}
			if (!useAcceleration)
			{
				speed = maxSpeed * SpeedMultiplier;
			}
			state.facingAngle = Angle;
			yield return null;
		}
		CurrentAttackNum = 0;
		StartCoroutine(FlyAcrossScreenAttackIE());
	}

	protected override IEnumerator ChargingRoutine()
	{
		if (GameManager.GetInstance() == null)
		{
			yield break;
		}
		if (Spine.AnimationState != null)
		{
			Spine.AnimationState.SetAnimation(0, "burpcharge", true);
			if (!string.IsNullOrEmpty(WarningVO))
			{
				AudioManager.Instance.PlayOneShot(WarningVO, base.gameObject);
			}
		}
		chargingTimestamp = GameManager.GetInstance().CurrentTime;
		while (GameManager.GetInstance().TimeSince(chargingTimestamp) < chargingDuration)
		{
			SimpleSpineFlash.FlashMeWhite();
			speed = Mathf.Lerp(speed, IdleSpeed, Time.deltaTime * 10f);
			Angle = Utils.GetAngle(base.transform.position, TargetPosition);
			yield return null;
		}
		SimpleSpineFlash.FlashWhite(false);
		if (Spine.AnimationState != null)
		{
			Spine.AnimationState.SetAnimation(0, "burp", true);
			Spine.AnimationState.AddAnimation(0, IdleAnimation, true, 0f);
		}
		yield return StartCoroutine(ShootProjectileRoutine());
		AttackCoolDown = UnityEngine.Random.Range(AttackCoolDownDuration.x, AttackCoolDownDuration.y);
		StartCoroutine(ActiveRoutine());
	}

	private IEnumerator ShootProjectileRoutine()
	{
		speed = IdleSpeed;
		lastBurpedFliesTimestamp = GameManager.GetInstance().CurrentTime;
		AttackCoolDown = UnityEngine.Random.Range(AttackCoolDownDuration.x, AttackCoolDownDuration.y);
		CameraManager.shakeCamera(0.2f, LookAngle);
		List<float> shootAngles = new List<float>(ShotsToFire);
		for (int j = 0; j < ShotsToFire; j++)
		{
			shootAngles.Add(360f / (float)ShotsToFire * (float)j);
		}
		shootAngles.Shuffle();
		float initAngle = UnityEngine.Random.Range(0f, 360f);
		for (int i = 0; i < shootAngles.Count; i++)
		{
			Projectile component = UnityEngine.Object.Instantiate(projectilePrefab, base.transform.parent).GetComponent<Projectile>();
			component.UseDelay = true;
			component.CollideOnlyTarget = true;
			component.transform.position = base.transform.position;
			component.Angle = initAngle + shootAngles[i];
			component.team = health.team;
			component.Speed += UnityEngine.Random.Range(-0.5f, 0.5f);
			component.turningSpeed += UnityEngine.Random.Range(-0.1f, 0.1f);
			component.angleNoiseFrequency += UnityEngine.Random.Range(-0.1f, 0.1f);
			component.LifeTime += UnityEngine.Random.Range(0f, 0.3f);
			component.Owner = health;
			component.SetTarget(PlayerFarming.Health);
			activeProjectiles.Add(component);
			yield return new WaitForSeconds(0.03f);
		}
		yield return new WaitForSeconds(0.3f);
	}

	private bool ShouldTrail()
	{
		if (GameManager.GetInstance().TimeSince(lastProjectileTrailTime) >= timeBetweenProjectileTrails)
		{
			return projectileTrail;
		}
		return false;
	}

	private IEnumerator ProjectileTrailRoutine()
	{
		Attacking = true;
		fleeing = true;
		maxSpeed = projectileMoveSpeed * SpeedMultiplier;
		TargetPosition = GetPositionAwayFromPlayer();
		lastProjectileTrailTime = GameManager.GetInstance().CurrentTime;
		float t = 0f;
		while (t < 2f && Vector3.Distance(base.transform.position, TargetPosition) > 1f)
		{
			t += Time.deltaTime;
			state.facingAngle = (state.LookAngle = (Angle = Utils.GetAngle(base.transform.position, TargetPosition)));
			yield return null;
		}
		KnockbackForceModifier = 0f;
		TargetPosition *= -1f;
		state.facingAngle = (state.LookAngle = (Angle = Utils.GetAngle(base.transform.position, TargetPosition)));
		if (!string.IsNullOrEmpty(WarningVO))
		{
			AudioManager.Instance.PlayOneShot(WarningVO, base.gameObject);
		}
		Spine.AnimationState.SetAnimation(0, "attackcharge", false);
		Spine.AnimationState.AddAnimation(0, "attack", true, 0f);
		Spine.AnimationState.AddAnimation(0, "Fly", true, 0f);
		maxSpeed = 0f;
		t = 0f;
		while (t < 1.1f)
		{
			t += Time.deltaTime;
			SimpleSpineFlash.FlashWhite(t / 1.1f * 0.75f);
			yield return null;
		}
		Spine.timeScale = 0.5f;
		maxSpeed = projectileMoveSpeed * 2f * SpeedMultiplier;
		SimpleSpineFlash.FlashWhite(false);
		Vector2 previousSpawnPosition = base.transform.position;
		t = 0f;
		while (t < 2f && Vector3.Distance(base.transform.position, TargetPosition) > 1f)
		{
			t += Time.deltaTime;
			state.facingAngle = (Angle = Utils.GetAngle(base.transform.position, TargetPosition));
			if (Vector3.Distance(base.transform.position, previousSpawnPosition) > distanceBetweenProjectiles)
			{
				for (int i = 0; i < 2; i++)
				{
					Projectile component = ObjectPool.Spawn(bulletPrefab, base.transform.parent).GetComponent<Projectile>();
					component.transform.position = base.transform.position;
					component.Angle = Mathf.Repeat(state.facingAngle + (float)(45 * ((i != 0) ? 1 : (-1))), 360f);
					component.team = health.team;
					component.Speed = projectileSpeed;
					component.Acceleration = projectileAcceleration;
					component.LifeTime = 4f + UnityEngine.Random.Range(0f, 0.3f);
					component.Owner = health;
				}
				AudioManager.Instance.PlayOneShot("event:/boss/spider/bomb_shoot", base.transform.position);
				previousSpawnPosition = base.transform.position;
			}
			state.facingAngle = (state.LookAngle = (Angle = Utils.GetAngle(base.transform.position, TargetPosition)));
			yield return null;
		}
		Spine.timeScale = 1f;
		maxSpeed = IdleSpeed * SpeedMultiplier;
		KnockbackForceModifier = 1f;
		Attacking = false;
		AttackCoolDown = UnityEngine.Random.Range(AttackCoolDownDuration.x, AttackCoolDownDuration.y);
		StartCoroutine(ActiveRoutine());
	}

	private bool ShouldSpawnEnemiess()
	{
		if (GameManager.GetInstance().TimeSince(lastSurroundingFliesTime) >= timeBetweenSpawning && spawning)
		{
			return Health.team2.Count - 1 < maxEnemies;
		}
		return false;
	}

	public void SpawnEnemies()
	{
		StartCoroutine(SpawnEnemuesIE());
	}

	private IEnumerator SpawnEnemuesIE()
	{
		if (GameManager.GetInstance() == null)
		{
			yield break;
		}
		lastSurroundingFliesTime = GameManager.GetInstance().CurrentTime;
		if (Spine.AnimationState != null)
		{
			Spine.AnimationState.SetAnimation(0, "burpcharge", true);
			if (!string.IsNullOrEmpty(WarningVO))
			{
				AudioManager.Instance.PlayOneShot(WarningVO, base.gameObject);
			}
		}
		chargingTimestamp = GameManager.GetInstance().CurrentTime;
		while (GameManager.GetInstance().TimeSince(chargingTimestamp) < chargingDuration)
		{
			SimpleSpineFlash.FlashMeWhite();
			speed = Mathf.Lerp(speed, IdleSpeed, Time.deltaTime * 10f);
			Angle = Utils.GetAngle(base.transform.position, TargetPosition);
			yield return null;
		}
		SimpleSpineFlash.FlashWhite(false);
		if (Spine.AnimationState != null)
		{
			Spine.AnimationState.SetAnimation(0, "burp", true);
			Spine.AnimationState.AddAnimation(0, IdleAnimation, true, 0f);
		}
		int num = (int)UnityEngine.Random.Range(spawningAmount.x, spawningAmount.y + 1f);
		for (int i = 0; i < num; i++)
		{
			Vector3 position = base.transform.position;
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(spawnables[UnityEngine.Random.Range(0, spawnables.Length)], position, Quaternion.identity, base.transform.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				UnitObject component = obj.Result.GetComponent<UnitObject>();
				component.CanHaveModifier = false;
				component.RemoveModifier();
				component.DoKnockBack(UnityEngine.Random.Range(0f, 360f), spawnForce, 0.75f);
			};
		}
		yield return new WaitForSeconds(1f);
		AttackCoolDown = UnityEngine.Random.Range(AttackCoolDownDuration.x, AttackCoolDownDuration.y);
		StartCoroutine(ActiveRoutine());
	}

	private bool ShouldFlyAcrossScreenAttack()
	{
		if (GameManager.GetInstance().TimeSince(lastFlyAcrossScreenTime) >= timeBetweenFlyAcrossScreenAmount)
		{
			return flyAcrossScreen;
		}
		return false;
	}

	private void FlyAcrossScreenAttack()
	{
		StartCoroutine(FlyAcrossScreenAttackIE());
	}

	private IEnumerator FlyAcrossScreenAttackIE()
	{
		Attacking = true;
		fleeing = true;
		KnockbackForceModifier = 0f;
		health.ApplyStasisImmunity();
		maxSpeed = projectileMoveSpeed * SpeedMultiplier;
		lastFlyAcrossScreenTime = GameManager.GetInstance().CurrentTime;
		bool right = UnityEngine.Random.value > 0.5f;
		if (!string.IsNullOrEmpty(WarningVO))
		{
			AudioManager.Instance.PlayOneShot(WarningVO, base.gameObject);
		}
		Angle = Utils.GetAngle(Vector3.zero, right ? Vector3.right : Vector3.left);
		state.facingAngle = (state.LookAngle = Angle);
		Spine.AnimationState.SetAnimation(0, "attackcharge", false);
		Spine.AnimationState.AddAnimation(0, "flyattack", true, 0f);
		GameManager.GetInstance().RemoveFromCamera(base.gameObject);
		if ((bool)BiomeGenerator.Instance)
		{
			GameManager.GetInstance().AddToCamera(BiomeGenerator.Instance.CurrentRoom.generateRoom.gameObject);
		}
		maxSpeed = 0f;
		float t2 = 0f;
		while (t2 < 1.1f)
		{
			t2 += Time.deltaTime;
			SimpleSpineFlash.FlashWhite(t2 / 1.1f * 0.75f);
			yield return null;
		}
		SimpleSpineFlash.FlashWhite(false);
		health.invincible = true;
		maxSpeed = flyAcrossScreenSpeed * SpeedMultiplier;
		damageColliderEvents.gameObject.SetActive(true);
		TargetPosition = (right ? new Vector3(15f, base.transform.position.y, base.transform.position.z) : new Vector3(-15f, base.transform.position.y, base.transform.position.z));
		t2 = 0f;
		while (t2 < 4f && Vector3.Distance(base.transform.position, TargetPosition) > 1f)
		{
			t2 += Time.deltaTime;
			Angle = Utils.GetAngle(base.transform.position, TargetPosition);
			state.facingAngle = (state.LookAngle = Angle);
			yield return null;
		}
		int amount = (int)UnityEngine.Random.Range(flyAcrossScreenAmount.x, flyAcrossScreenAmount.y + 1f) + 1;
		for (int i = 0; i < amount; i++)
		{
			Vector3 position = (right ? new Vector3(-15f, PlayerFarming.Instance.transform.position.y, base.transform.position.z) : new Vector3(15f, PlayerFarming.Instance.transform.position.y, base.transform.position.z));
			TargetPosition = (right ? new Vector3(15f, PlayerFarming.Instance.transform.position.y, base.transform.position.z) : new Vector3(-15f, PlayerFarming.Instance.transform.position.y, base.transform.position.z));
			base.transform.position = position;
			if (i == amount - 1)
			{
				TargetPosition = new Vector3(PlayerFarming.Instance.transform.position.x, PlayerFarming.Instance.transform.position.y, base.transform.position.z);
			}
			t2 = 0f;
			while (t2 < 4f && Vector3.Distance(base.transform.position, TargetPosition) > 1f)
			{
				t2 += Time.deltaTime;
				Angle = Utils.GetAngle(base.transform.position, TargetPosition);
				state.facingAngle = (state.LookAngle = Angle);
				yield return null;
			}
		}
		damageColliderEvents.gameObject.SetActive(false);
		if (!string.IsNullOrEmpty(WarningVO))
		{
			AudioManager.Instance.PlayOneShot(WarningVO, base.gameObject);
		}
		Spine.AnimationState.SetAnimation(0, "flyattack-stop", false);
		Spine.AnimationState.AddAnimation(0, "Fly", true, 0f);
		health.ClearStasisImmunity();
		health.invincible = false;
		t2 = 0f;
		while (t2 < 0.2f)
		{
			speed = Mathf.Lerp(maxSpeed, 0f, t2 / 0.2f);
			t2 += Time.deltaTime;
			yield return null;
		}
		if ((bool)BiomeGenerator.Instance)
		{
			GameManager.GetInstance().RemoveFromCamera(BiomeGenerator.Instance.CurrentRoom.generateRoom.gameObject);
		}
		GameManager.GetInstance().AddToCamera(base.gameObject);
		KnockbackForceModifier = 1f;
		Attacking = false;
		fleeing = false;
		t2 = 0f;
		while (t2 < 3f)
		{
			speed = 0f;
			maxSpeed = 0f;
			t2 += Time.deltaTime;
			yield return null;
		}
		maxSpeed = IdleSpeed * SpeedMultiplier;
		StartCoroutine(ActiveRoutine());
	}

	private Vector3 GetPositionAwayFromPlayer()
	{
		List<RaycastHit2D> list = new List<RaycastHit2D>();
		list.Add(Physics2D.Raycast(base.transform.position, new Vector2(1f, 1f), 100f, wallMask));
		list.Add(Physics2D.Raycast(base.transform.position, new Vector2(-1f, 1f), 100f, wallMask));
		list.Add(Physics2D.Raycast(base.transform.position, new Vector2(1f, -1f), 100f, wallMask));
		list.Add(Physics2D.Raycast(base.transform.position, new Vector2(-1f, -1f), 100f, wallMask));
		RaycastHit2D raycastHit2D = list[0];
		for (int i = 1; i < list.Count; i++)
		{
			float num = Vector3.Distance(PlayerFarming.Instance.transform.position, list[i].point);
			float num2 = Vector3.Distance(PlayerFarming.Instance.transform.position, raycastHit2D.point);
			if (num > num2 && Vector3.Distance(base.transform.position, list[i].point) > 2f)
			{
				raycastHit2D = list[i];
			}
		}
		return raycastHit2D.point + ((Vector2)base.transform.position - raycastHit2D.point).normalized;
	}

	protected override bool ShouldStartCharging()
	{
		if (GameManager.GetInstance().TimeSince(lastBurpedFliesTimestamp) >= minTimeBetweenBurps)
		{
			return IsPlayerNearby();
		}
		return false;
	}

	protected override IEnumerator ApplyForceRoutine(GameObject Attacker)
	{
		DisableForces = true;
		Angle = Utils.GetAngle(Attacker.transform.position, base.transform.position) * ((float)Math.PI / 180f);
		Force = new Vector2(500f * Mathf.Cos(Angle), 500f * Mathf.Sin(Angle));
		rb.AddForce(Force);
		yield return new WaitForSeconds(0.2f);
		DisableForces = false;
	}

	private bool IsPlayerNearby()
	{
		foreach (Health allUnit in Health.allUnits)
		{
			if (allUnit.team != health.team && !allUnit.InanimateObject && allUnit.team != 0 && (health.team != Health.Team.PlayerTeam || (health.team == Health.Team.PlayerTeam && allUnit.team != Health.Team.DangerousAnimals)) && Vector2.Distance(base.transform.position, allUnit.gameObject.transform.position) < DetectEnemyRange)
			{
				return true;
			}
		}
		return false;
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		for (int i = 0; i < activeProjectiles.Count; i++)
		{
			if (activeProjectiles[i] != null && activeProjectiles[i].gameObject.activeSelf)
			{
				activeProjectiles[i].DestroyWithVFX();
			}
		}
		activeProjectiles.Clear();
	}
}
