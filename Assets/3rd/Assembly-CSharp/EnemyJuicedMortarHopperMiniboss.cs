using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMODUnity;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyJuicedMortarHopperMiniboss : UnitObject
{
	public SkeletonAnimation Spine;

	[SerializeField]
	private SimpleSpineFlash simpleSpineFlash;

	[SerializeField]
	private SpriteRenderer shadow;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string idleAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string shootAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string shootLongerAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string shootNoAnticipationAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string shootLongerNoAnticipationAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string poisonSplashAnimation;

	[SerializeField]
	private GameObject smallMortarPrefab;

	[SerializeField]
	private int shotsToFireCross;

	[SerializeField]
	private int shotsToFireLine = 4;

	[SerializeField]
	private int shotsToFireAroundMiniboss = 6;

	[SerializeField]
	private float shotsAroundMinibossDistance;

	[SerializeField]
	private float timeBetweenShots;

	[SerializeField]
	private float shootAnticipation;

	[Space]
	[SerializeField]
	private float minBombRange;

	[SerializeField]
	private float maxBombRange;

	[SerializeField]
	private float bombDuration;

	[SerializeField]
	private AnimationCurve hopSpeedCurve;

	[SerializeField]
	private AnimationCurve hopZCurve;

	[SerializeField]
	private float hopZHeight;

	[SerializeField]
	private float hoppingDur = 0.4f;

	[SerializeField]
	private float attackRange;

	[SerializeField]
	private Vector2 timeBetweenJumps;

	[SerializeField]
	private ParticleSystem aoeParticles;

	[SerializeField]
	private float rageHopAnticipation;

	[SerializeField]
	private float rageHopBulletSpeed;

	[SerializeField]
	private float rageHopBulletDeceleration;

	[SerializeField]
	private int rageHopBulletAmount;

	[SerializeField]
	private Vector2 rageHopAmount;

	[SerializeField]
	private Vector2 bigRotationAmountOfShots;

	[SerializeField]
	private float bigRotationTimeBetween;

	[SerializeField]
	private float poisonRadius;

	[SerializeField]
	private float poisonAnticipation;

	[SerializeField]
	private Vector2 poisonAmount;

	[SerializeField]
	private float megaHopDuration;

	[SerializeField]
	private int megaHopProjectiles;

	[SerializeField]
	private int megaHopProjectilesSpeed;

	[SerializeField]
	public AssetReference enemyToSpawn;

	[SerializeField]
	private int maxEnemies = 3;

	[SerializeField]
	public Vector2 enemiesToSpawn;

	[SerializeField]
	public Vector2 timeBetweenEnemySpawn;

	[SerializeField]
	private float rageHopChance;

	[SerializeField]
	private float poisonSplashChance;

	[SerializeField]
	private float shootMortarChance;

	[SerializeField]
	private float moveChance;

	[SerializeField]
	private float megaHopChance;

	[Space]
	[SerializeField]
	private Vector2 timeBetweenAttacks;

	[SerializeField]
	private ColliderEvents damageColliderEvents;

	[EventRef]
	[SerializeField]
	private string attackVO = string.Empty;

	[EventRef]
	public string OnLandSoundPath = string.Empty;

	[EventRef]
	public string OnJumpSoundPath = string.Empty;

	private Coroutine currentAttack;

	private float idleTime;

	private float spawnTime;

	private bool phase2;

	private int attackIndex = -1;

	private float hoppingTimestamp;

	private bool hasCollidedWithObstacle;

	private float zFallSpeed = 15f;

	private CircleCollider2D collider;

	private void Start()
	{
		collider = GetComponent<CircleCollider2D>();
		damageColliderEvents.SetActive(false);
		damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
		shadow.enabled = false;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		damageColliderEvents.OnTriggerEnterEvent -= OnDamageTriggerEnter;
	}

	public override void OnEnable()
	{
		base.OnEnable();
		currentAttack = null;
		attackIndex = -1;
		health.invincible = false;
		StateMachine stateMachine = state;
		stateMachine.OnStateChange = (StateMachine.StateChange)Delegate.Combine(stateMachine.OnStateChange, new StateMachine.StateChange(OnStateChange));
	}

	public override void OnDisable()
	{
		base.OnDisable();
		StateMachine stateMachine = state;
		stateMachine.OnStateChange = (StateMachine.StateChange)Delegate.Remove(stateMachine.OnStateChange, new StateMachine.StateChange(OnStateChange));
	}

	public override void Update()
	{
		base.Update();
		idleTime -= Time.deltaTime * Spine.timeScale;
		spawnTime -= Time.deltaTime * Spine.timeScale;
		if (state.CURRENT_STATE != StateMachine.State.Moving)
		{
			if (currentAttack == null)
			{
				Spine.transform.localPosition = Vector3.Lerp(Spine.transform.localPosition, Vector3.zero, Time.deltaTime * zFallSpeed);
			}
		}
		else
		{
			UpdateStateMoving();
			move();
		}
		if (state.CURRENT_STATE != StateMachine.State.Moving && currentAttack == null && GetClosestTarget() != null && idleTime <= 0f)
		{
			if (GetClosestTarget() != null)
			{
				state.facingAngle = Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position);
			}
			if (!phase2 && health.HP < health.totalHP / 2f)
			{
				phase2 = true;
				timeBetweenAttacks /= 1.5f;
				bombDuration /= 1.5f;
				maxEnemies = (int)((float)maxEnemies * 1.5f);
			}
			if (UnityEngine.Random.value <= moveChance && phase2)
			{
				float value = UnityEngine.Random.value;
				float lookAngle = ((value < 0.33f) ? GetAngleToTarget() : ((!(value < 0.66f)) ? GetFleeAngle() : GetRandomFacingAngle()));
				state.facingAngle = (state.LookAngle = lookAngle);
				state.CURRENT_STATE = StateMachine.State.Moving;
			}
			else if (UnityEngine.Random.value <= shootMortarChance && attackIndex != 0)
			{
				ShootMortars();
				attackIndex = 0;
			}
			else if (UnityEngine.Random.value <= poisonSplashChance && GetClosestTarget() != null && Vector3.Distance(base.transform.position, GetClosestTarget().transform.position) < 3f && TrapPoison.ActivePoison.Count <= 0 && attackIndex != 1)
			{
				PoisonSplash();
				attackIndex = 1;
			}
			else if (UnityEngine.Random.value <= rageHopChance && attackIndex != 2 && !phase2)
			{
				RageHop();
				attackIndex = 2;
			}
			else if (UnityEngine.Random.value <= megaHopChance && attackIndex != 3 && phase2)
			{
				MegaHop();
				attackIndex = 3;
			}
		}
		if (!(spawnTime <= 0f) || Health.team2.Count - 1 >= maxEnemies)
		{
			return;
		}
		int num = (int)UnityEngine.Random.Range(enemiesToSpawn.x, enemiesToSpawn.y + 1f);
		for (int i = 0; i < num; i++)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(enemyToSpawn, UnityEngine.Random.insideUnitCircle * 4.5f, Quaternion.identity, base.transform.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				EnemySpawner.CreateWithAndInitInstantiatedEnemy(obj.Result.transform.position, base.transform.parent, obj.Result);
			};
		}
		spawnTime = UnityEngine.Random.Range(timeBetweenEnemySpawn.x, timeBetweenEnemySpawn.y);
	}

	private void ShootMortars()
	{
		if (UnityEngine.Random.value < 0.5f || phase2)
		{
			LineShootTargeted();
		}
		else
		{
			BigRotationShot();
		}
	}

	private void RageHop()
	{
		currentAttack = StartCoroutine(RageHopIE());
	}

	private IEnumerator RageHopIE()
	{
		float t = 0f;
		while (t < rageHopAnticipation)
		{
			t += Time.deltaTime * Spine.timeScale;
			simpleSpineFlash.FlashWhite(t / rageHopAnticipation * 0.75f);
			yield return null;
		}
		simpleSpineFlash.FlashWhite(false);
		state.CURRENT_STATE = StateMachine.State.Aiming;
		float a = 0f;
		for (int i = 0; (float)i < UnityEngine.Random.Range(rageHopAmount.x, rageHopAmount.y + 1f); i++)
		{
			Spine.AnimationState.SetAnimation(0, "jumpcombined", false);
			Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
			float time2 = 0f;
			float num;
			while (true)
			{
				time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num < 0.5f))
				{
					break;
				}
				yield return null;
			}
			CameraManager.instance.ShakeCameraForDuration(1f, 1f, 0.25f);
			BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position + Vector3.back * 0.5f);
			int amount = rageHopBulletAmount;
			Health owner = health;
			Vector3 position = base.transform.position;
			float num2 = rageHopBulletSpeed;
			a = (num = a + 45f);
			Projectile.CreateProjectiles(amount, owner, position, num2, 1f, num, false, delegate(List<Projectile> projectiles)
			{
				StartCoroutine(SetProjectilePulse(projectiles));
			});
			time2 = 0f;
			while (true)
			{
				time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num < 0.4f))
				{
					break;
				}
				yield return null;
			}
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		currentAttack = null;
		idleTime = UnityEngine.Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);
	}

	private IEnumerator SetProjectilePulse(List<Projectile> projectiles)
	{
		yield return new WaitForEndOfFrame();
		foreach (Projectile projectile in projectiles)
		{
			projectile.Deceleration = rageHopBulletDeceleration;
			projectile.PulseMove = true;
		}
	}

	private void CrossShoot()
	{
		currentAttack = StartCoroutine(CrossShootIE());
	}

	private IEnumerator CrossShootIE()
	{
		Spine.AnimationState.SetAnimation(0, shootNoAnticipationAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		float time2 = 0f;
		while (time2 < shootAnticipation)
		{
			time2 += Time.deltaTime * Spine.timeScale;
			simpleSpineFlash.FlashWhite(time2 / shootAnticipation * 0.75f);
			yield return null;
		}
		AudioManager.Instance.PlayOneShot(attackVO, base.gameObject);
		simpleSpineFlash.FlashWhite(false);
		float distance = 1f;
		for (int i = 0; i < shotsToFireCross; i++)
		{
			float num = 0f;
			for (int j = 0; j < 4; j++)
			{
				Vector3 position = base.transform.position + (Vector3)Utils.DegreeToVector2(num) * distance;
				UnityEngine.Object.Instantiate(smallMortarPrefab, position, Quaternion.identity, base.transform.parent).GetComponent<MortarBomb>().Play(base.transform.position + new Vector3(0f, 0f, -1.5f), bombDuration, Health.Team.Team2);
				num += 90f;
			}
			distance += 2f;
			time2 = 0f;
			while (true)
			{
				float num2;
				time2 = (num2 = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num2 < timeBetweenShots * 0.75f))
				{
					break;
				}
				yield return null;
			}
		}
		currentAttack = null;
		idleTime = UnityEngine.Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);
	}

	private IEnumerator LineShootIE(bool anticipate, float aimAngle, int shotsToFireLine)
	{
		if (anticipate)
		{
			Spine.AnimationState.SetAnimation(0, shootLongerAnimation, false);
			Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
			float time2 = 0f;
			while (time2 < shootAnticipation)
			{
				time2 += Time.deltaTime * Spine.timeScale;
				simpleSpineFlash.FlashWhite(time2 / shootAnticipation * 0.75f);
				yield return null;
			}
			AudioManager.Instance.PlayOneShot(attackVO, base.gameObject);
			simpleSpineFlash.FlashWhite(false);
		}
		float distance = 1f;
		for (int i = 0; i < shotsToFireLine; i++)
		{
			Vector3 position = base.transform.position + (Vector3)Utils.DegreeToVector2(aimAngle) * distance;
			UnityEngine.Object.Instantiate(smallMortarPrefab, position, Quaternion.identity, base.transform.parent).GetComponent<MortarBomb>().Play(base.transform.position + new Vector3(0f, 0f, -1.5f), bombDuration, Health.Team.Team2);
			distance += 2f;
			float time2 = 0f;
			while (true)
			{
				float num;
				time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num < timeBetweenShots * 0.75f))
				{
					break;
				}
				yield return null;
			}
		}
		idleTime = UnityEngine.Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);
	}

	private void LineShootTargeted()
	{
		currentAttack = StartCoroutine(LineShootTargetedIE());
	}

	private IEnumerator LineShootTargetedIE()
	{
		yield return StartCoroutine(LineShootIE(true, (GetClosestTarget() != null) ? Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position) : UnityEngine.Random.Range(0f, 360f), shotsToFireLine));
		currentAttack = null;
	}

	private void BigRotationShot()
	{
		currentAttack = StartCoroutine(BigRotationShotIE());
	}

	private IEnumerator BigRotationShotIE()
	{
		Spine.AnimationState.SetAnimation(0, shootLongerAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		float time2 = 0f;
		while (time2 < shootAnticipation)
		{
			time2 += Time.deltaTime * Spine.timeScale;
			simpleSpineFlash.FlashWhite(time2 / shootAnticipation * 0.75f);
			yield return null;
		}
		AudioManager.Instance.PlayOneShot(attackVO, base.gameObject);
		simpleSpineFlash.FlashWhite(false);
		float offset = 0f;
		int shotsToFire = (int)UnityEngine.Random.Range(bigRotationAmountOfShots.x, bigRotationAmountOfShots.y + 1f);
		for (int i = 0; i < shotsToFire; i++)
		{
			float num = offset;
			if (i != 0)
			{
				Spine.AnimationState.SetAnimation(0, shootLongerNoAnticipationAnimation, false);
				Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
			}
			for (int j = 0; j < 4; j++)
			{
				StartCoroutine(LineShootIE(false, num, 5));
				num = Mathf.Repeat(num + 90f, 360f);
			}
			offset += 30f;
			time2 = 0f;
			while (true)
			{
				float num2;
				time2 = (num2 = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num2 < bigRotationTimeBetween))
				{
					break;
				}
				yield return null;
			}
		}
		currentAttack = null;
		idleTime = UnityEngine.Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);
	}

	private void PoisonSplash()
	{
		currentAttack = StartCoroutine(PoisonSplashIE());
	}

	private IEnumerator PoisonSplashIE()
	{
		Spine.AnimationState.SetAnimation(0, poisonSplashAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		float time2 = 0f;
		while (time2 < poisonAnticipation)
		{
			time2 += Time.deltaTime * Spine.timeScale;
			simpleSpineFlash.FlashWhite(time2 / poisonAnticipation * 0.75f);
			yield return null;
		}
		AudioManager.Instance.PlayOneShot(attackVO, base.gameObject);
		Vector3 position = base.transform.position;
		simpleSpineFlash.FlashWhite(false);
		int amount = (int)UnityEngine.Random.Range(poisonAmount.x, poisonAmount.y + 1f);
		for (int i = 0; i < amount; i++)
		{
			float num = UnityEngine.Random.Range(0f, poisonRadius);
			TrapPoison.CreatePoison(base.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * num, 1, 0f, base.transform.parent);
			time2 = 0f;
			while (true)
			{
				float num2;
				time2 = (num2 = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num2 < 0.05f))
				{
					break;
				}
				yield return null;
			}
		}
		currentAttack = null;
		idleTime = UnityEngine.Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);
	}

	private void MegaHop()
	{
		currentAttack = StartCoroutine(MegaHopIE());
	}

	private IEnumerator MegaHopIE()
	{
		health.invincible = true;
		Spine.AnimationState.SetAnimation(0, "jump-start", false);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.6f))
			{
				break;
			}
			yield return null;
		}
		Spine.transform.DOLocalMoveZ(-20f, 1f).SetEase(Ease.OutSine);
		shadow.transform.DOScale(3f, 1f);
		shadow.enabled = true;
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 1f))
			{
				break;
			}
			yield return null;
		}
		Vector3 position2 = base.transform.position;
		Vector3 position = GetClosestTarget().transform.position;
		base.transform.DOMove(position, megaHopDuration).SetEase(Ease.OutSine);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < megaHopDuration))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.PlayOneShot(attackVO, base.gameObject);
		shadow.transform.DOScale(5f, 1f);
		Spine.transform.DOLocalMoveZ(0f, 0.5f).SetEase(Ease.Linear);
		state.facingAngle = (state.LookAngle = Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position));
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.5f))
			{
				break;
			}
			yield return null;
		}
		Spine.AnimationState.SetAnimation(0, "jump-end-big", false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		AudioManager.Instance.PlayOneShot(OnLandSoundPath, base.gameObject);
		AudioManager.Instance.PlayOneShot(attackVO, base.gameObject);
		StartCoroutine(TurnOnDamageColliderForDuration(0.1f));
		aoeParticles.Play();
		shadow.enabled = false;
		health.invincible = false;
		BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position + Vector3.back * 0.5f);
		CameraManager.shakeCamera(2f);
		Projectile.CreateProjectiles(megaHopProjectiles, health, base.transform.position, megaHopProjectilesSpeed);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 1f))
			{
				break;
			}
			yield return null;
		}
		currentAttack = null;
		idleTime = UnityEngine.Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);
	}

	protected virtual void UpdateStateMoving()
	{
		speed = hopSpeedCurve.Evaluate((hoppingTimestamp - Time.time) / hoppingDur);
		if (hasCollidedWithObstacle || TargetIsInAttackRange())
		{
			speed *= 0.5f;
		}
		Spine.transform.localPosition = -Vector3.forward * hopZCurve.Evaluate((hoppingTimestamp - Time.time) / hoppingDur) * hopZHeight;
		if (Time.time >= hoppingTimestamp)
		{
			speed = 0f;
			state.CURRENT_STATE = StateMachine.State.Idle;
			AudioManager.Instance.PlayOneShot(OnLandSoundPath, base.gameObject);
			AudioManager.Instance.PlayOneShot(attackVO, base.gameObject);
			StartCoroutine(TurnOnDamageColliderForDuration(0.1f));
			aoeParticles.Play();
			BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position + Vector3.back * 0.5f);
			CameraManager.shakeCamera(1f);
		}
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		if (state.CURRENT_STATE == StateMachine.State.Moving && collider.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
		{
			hasCollidedWithObstacle = true;
		}
	}

	private bool TargetIsInAttackRange()
	{
		if (!GameManager.RoomActive)
		{
			return false;
		}
		if (GetClosestTarget() == null)
		{
			return false;
		}
		return Vector3.Distance(GetClosestTarget().transform.position, base.transform.position) <= attackRange * 0.75f;
	}

	protected virtual void OnStateChange(StateMachine.State newState, StateMachine.State prevState)
	{
		switch (newState)
		{
		case StateMachine.State.Idle:
			if (newState != prevState)
			{
				idleTime = UnityEngine.Random.Range(timeBetweenJumps.x, timeBetweenJumps.y);
				if (prevState == StateMachine.State.Moving)
				{
					Spine.AnimationState.SetAnimation(0, "jump-end", false);
				}
				Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
				if (!string.IsNullOrEmpty(OnLandSoundPath))
				{
					AudioManager.Instance.PlayOneShot(OnLandSoundPath, base.transform.position);
				}
				simpleSpineFlash.FlashWhite(false);
			}
			break;
		case StateMachine.State.Moving:
			hasCollidedWithObstacle = false;
			hoppingTimestamp = Time.time + hoppingDur;
			simpleSpineFlash.FlashWhite(false);
			Spine.AnimationState.SetAnimation(0, "jump", false);
			if (!string.IsNullOrEmpty(OnJumpSoundPath))
			{
				AudioManager.Instance.PlayOneShot(OnJumpSoundPath, base.transform.position);
			}
			break;
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		simpleSpineFlash.FlashFillRed();
	}

	private float GetAngleToTarget()
	{
		if (GetClosestTarget() == null)
		{
			return GetRandomFacingAngle();
		}
		float num = Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position);
		if (collider == null)
		{
			return num;
		}
		float num2 = 32f;
		for (int i = 0; (float)i < num2; i++)
		{
			if (!Physics2D.CircleCast(base.transform.position, collider.radius, new Vector2(Mathf.Cos(num * ((float)Math.PI / 180f)), Mathf.Sin(num * ((float)Math.PI / 180f))), attackRange * 0.5f, layerToCheck))
			{
				break;
			}
			num += 360f / (num2 + 1f);
		}
		return num;
	}

	private float GetRandomFacingAngle()
	{
		float num = UnityEngine.Random.Range(0, 360);
		if (collider == null)
		{
			return num;
		}
		float num2 = 16f;
		for (int i = 0; (float)i < num2; i++)
		{
			if (!Physics2D.CircleCast(base.transform.position, collider.radius, new Vector2(Mathf.Cos(num * ((float)Math.PI / 180f)), Mathf.Sin(num * ((float)Math.PI / 180f))), attackRange * 0.5f, layerToCheck))
			{
				break;
			}
			num += 360f / (num2 + 1f);
		}
		return num;
	}

	private float GetFleeAngle()
	{
		if (GetClosestTarget() == null)
		{
			return GetRandomFacingAngle();
		}
		float num = 100f;
		while ((num -= 1f) > 0f)
		{
			float f = (float)UnityEngine.Random.Range(0, 360) * ((float)Math.PI / 180f);
			float num2 = UnityEngine.Random.Range(4, 7);
			Vector3 vector = GetClosestTarget().transform.position + new Vector3(num2 * Mathf.Cos(f), num2 * Mathf.Sin(f));
			Vector3 vector2 = Vector3.Normalize(vector - GetClosestTarget().transform.position);
			RaycastHit2D raycastHit2D = Physics2D.CircleCast(GetClosestTarget().transform.position, 1.5f, vector2, num2, layerToCheck);
			if (raycastHit2D.collider != null)
			{
				if (Vector3.Distance(GetClosestTarget().transform.position, raycastHit2D.centroid) > 3f)
				{
					return Utils.GetAngle(base.transform.position, vector);
				}
				continue;
			}
			return Utils.GetAngle(base.transform.position, vector);
		}
		return GetRandomFacingAngle();
	}

	private IEnumerator TurnOnDamageColliderForDuration(float duration)
	{
		damageColliderEvents.SetActive(true);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < duration))
			{
				break;
			}
			yield return null;
		}
		damageColliderEvents.SetActive(false);
	}

	private void OnDamageTriggerEnter(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && (component.team != health.team || health.team == Health.Team.PlayerTeam))
		{
			component.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.7f));
		}
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		for (int num = Health.team2.Count - 1; num >= 0; num--)
		{
			if (Health.team2[num] != null && Health.team2[num] != health)
			{
				Health.team2[num].enabled = true;
				Health.team2[num].invincible = false;
				Health.team2[num].untouchable = false;
				Health.team2[num].DealDamage(Health.team2[num].totalHP, base.gameObject, base.transform.position, false, Health.AttackTypes.Heavy);
			}
		}
	}
}
