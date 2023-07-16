using System;
using System.Collections;
using FMODUnity;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyJuicedWormMiniboss : UnitObject
{
	public SkeletonAnimation Spine;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string idleAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string movingAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string enragedAttackAnticipationAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string enragedMovingAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string enragedFinishedAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string chargeAttackAnticipationAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string chargeAttackingAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string chargeAttackImpactAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string dashAttackAnticipationAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string dashAttackImpactAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string shootAttackAnticipationAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string shootAttackAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string spawnEnemiesAnimation;

	[Space]
	[SerializeField]
	private Vector2 timeBetweenAttacks;

	[SerializeField]
	private Vector2 timeBetweenEnemySpawns;

	[SerializeField]
	private float enragedAnticipation;

	[SerializeField]
	private float enragedPost;

	[SerializeField]
	private float enragedDuration;

	[SerializeField]
	private float enragedMaxSpeed;

	[SerializeField]
	private GameObject grenadeBullet;

	[SerializeField]
	private GameObject shootPosition;

	[SerializeField]
	private Vector2 amountOfShots;

	[SerializeField]
	private Vector2 delayBetweenShots;

	[SerializeField]
	private Vector2 shootDistance;

	[SerializeField]
	private float gravSpeed;

	[SerializeField]
	private Vector2 amountOfRounds;

	[SerializeField]
	private float targetedShootingAnticipation;

	[SerializeField]
	private float targetedShootingPost;

	[SerializeField]
	private float minDistanceToTargetShooting;

	[SerializeField]
	private float chargeAnticipation;

	[SerializeField]
	private float chargeMaxSpeed;

	[SerializeField]
	private float chargePost;

	[SerializeField]
	private float minDistanceToStartCharging;

	[SerializeField]
	private float dashAnticipation;

	[SerializeField]
	private float dashDuration;

	[SerializeField]
	private float dashPost;

	[SerializeField]
	private float dashForce;

	[SerializeField]
	private float maxDistanceToDash;

	[SerializeField]
	private Vector2 spawnAmount;

	[SerializeField]
	private int maxEnemies;

	[SerializeField]
	private float spawnEnemiesAnticipation;

	[SerializeField]
	private float spawnEnemiesPost;

	[SerializeField]
	private Vector2 delayBetweenSpawns;

	[SerializeField]
	private AssetReferenceGameObject[] spawnables;

	[SerializeField]
	public float chargeAttackWeight;

	[SerializeField]
	public float targetedShootAttackWeight;

	[SerializeField]
	public float enragedAttackWeight;

	[SerializeField]
	public float dashAttackWeight;

	[SerializeField]
	public float spawnEnemiesWeight;

	[Space]
	[SerializeField]
	private ColliderEvents damageCollider;

	[EventRef]
	public string GetHitVO = string.Empty;

	private float randomDirection;

	private Vector2 distanceRange = new Vector2(3f, 4f);

	private Vector2 idleWaitRange = new Vector2(0.2f, 1.5f);

	private bool moving = true;

	private float originalMaxSpeed;

	private Coroutine currentAttack;

	private SimpleSpineFlash[] simpleSpineFlashes;

	private float roamTime;

	private float spawnTime;

	private float targetTime;

	private bool canBeKnockedBack = true;

	private bool targeting;

	public override void Awake()
	{
		base.Awake();
		originalMaxSpeed = maxSpeed;
		simpleSpineFlashes = GetComponentsInChildren<SimpleSpineFlash>();
		damageCollider.OnTriggerEnterEvent += OnDamagedEnemy;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		damageCollider.OnTriggerEnterEvent -= OnDamagedEnemy;
	}

	public override void OnEnable()
	{
		base.OnEnable();
		currentAttack = null;
	}

	public override void Update()
	{
		roamTime -= Time.deltaTime * Spine.timeScale;
		spawnTime -= Time.deltaTime * Spine.timeScale;
		targetTime -= Time.deltaTime * Spine.timeScale;
		if (targetTime <= 0f)
		{
			targetTime = UnityEngine.Random.Range(0f, 5f);
			targeting = !targeting;
		}
		GetNewTargetPosition();
		if (moving)
		{
			if (UsePathing)
			{
				if (pathToFollow == null)
				{
					speed += (0f - speed) / 20f * GameManager.DeltaTime;
					move();
					return;
				}
				if (currentWaypoint >= pathToFollow.Count)
				{
					speed += (0f - speed) / 20f * GameManager.DeltaTime;
					move();
					return;
				}
			}
			if (state.CURRENT_STATE == StateMachine.State.Moving || state.CURRENT_STATE == StateMachine.State.Fleeing)
			{
				speed += (maxSpeed * SpeedMultiplier - speed) / 35f * GameManager.DeltaTime;
				if (UsePathing)
				{
					state.facingAngle = Mathf.LerpAngle(state.facingAngle, Utils.GetAngle(base.transform.position, pathToFollow[currentWaypoint]), Time.deltaTime * 2f);
					if (Vector2.Distance(base.transform.position, pathToFollow[currentWaypoint]) <= StoppingDistance)
					{
						currentWaypoint++;
						if (currentWaypoint == pathToFollow.Count)
						{
							state.CURRENT_STATE = StateMachine.State.Idle;
							System.Action endOfPath = EndOfPath;
							if (endOfPath != null)
							{
								endOfPath();
							}
							pathToFollow = null;
						}
					}
				}
			}
			else
			{
				speed += (0f - speed) / 20f * GameManager.DeltaTime;
			}
		}
		move();
		if (currentAttack == null && GetClosestTarget() != null && roamTime <= 0f)
		{
			if (Vector3.Distance(base.transform.position, GetClosestTarget().transform.position) <= maxDistanceToDash && UnityEngine.Random.value <= dashAttackWeight)
			{
				DashAttack();
			}
			else if (Health.team2.Count - 1 < maxEnemies && spawnTime <= 0f && (Health.team2.Count - 1 <= 2 || UnityEngine.Random.value <= spawnEnemiesWeight))
			{
				SpawnEnemies();
			}
			else if (Vector3.Distance(base.transform.position, GetClosestTarget().transform.position) >= minDistanceToTargetShooting && UnityEngine.Random.value <= targetedShootAttackWeight)
			{
				TargetedShoot();
			}
			else if (Vector3.Distance(base.transform.position, GetClosestTarget().transform.position) >= minDistanceToStartCharging && UnityEngine.Random.value <= chargeAttackWeight)
			{
				ChargeAttack();
			}
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		if (!string.IsNullOrEmpty(GetHitVO))
		{
			AudioManager.Instance.PlayOneShot(GetHitVO, base.transform.position);
		}
		if (canBeKnockedBack)
		{
			DoKnockBack(Attacker, 0.25f, 0.5f);
		}
		SimpleSpineFlash[] array = simpleSpineFlashes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].FlashFillRed();
		}
	}

	public void GetNewTargetPosition()
	{
		if (AstarPath.active == null)
		{
			return;
		}
		if (targeting & (GetClosestTarget() != null))
		{
			givePath(GetClosestTarget().transform.position);
			return;
		}
		float num = 100f;
		while ((num -= 1f) > 0f)
		{
			float num2 = UnityEngine.Random.Range(distanceRange.x, distanceRange.y);
			randomDirection += (float)UnityEngine.Random.Range(-45, 45) * ((float)Math.PI / 180f);
			float radius = 0.1f;
			Vector3 vector = base.transform.position + new Vector3(num2 * Mathf.Cos(randomDirection), num2 * Mathf.Sin(randomDirection));
			if (Physics2D.CircleCast(base.transform.position, radius, Vector3.Normalize(vector - base.transform.position), num2 * 0.5f, layerToCheck).collider != null)
			{
				randomDirection += 0.17453292f;
				continue;
			}
			givePath(vector);
			break;
		}
	}

	private void DisableMovement()
	{
		speed = 0f;
		ClearPaths();
		moving = false;
	}

	public void EnragedAttack()
	{
		currentAttack = StartCoroutine(EnragedAttackIE());
	}

	private IEnumerator EnragedAttackIE()
	{
		DisableMovement();
		SimpleSpineFlash[] array = simpleSpineFlashes;
		foreach (SimpleSpineFlash obj in array)
		{
			obj.Spine.AnimationState.SetAnimation(0, enragedAttackAnticipationAnimation, false);
			obj.Spine.AnimationState.AddAnimation(0, enragedMovingAnimation, true, 0f);
		}
		Spine.AnimationState.SetAnimation(0, enragedAttackAnticipationAnimation, false);
		Spine.AnimationState.AddAnimation(0, enragedMovingAnimation, true, 0f);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < enragedAnticipation))
			{
				break;
			}
			yield return null;
		}
		moving = true;
		maxSpeed = enragedMaxSpeed;
		speed = maxSpeed;
		canBeKnockedBack = false;
		Coroutine shootCoroutine = null;
		idleWaitRange /= 2f;
		distanceRange /= 2f;
		float t = 0f;
		while (t < enragedDuration)
		{
			t += Time.deltaTime;
			speed = maxSpeed;
			if (shootCoroutine == null)
			{
				shootCoroutine = StartCoroutine(ShootIE(false, delegate
				{
					shootCoroutine = null;
				}));
			}
			yield return null;
		}
		maxSpeed = originalMaxSpeed;
		DisableMovement();
		canBeKnockedBack = true;
		idleWaitRange *= 2f;
		distanceRange *= 2f;
		array = simpleSpineFlashes;
		foreach (SimpleSpineFlash obj2 in array)
		{
			obj2.Spine.AnimationState.SetAnimation(0, enragedFinishedAnimation, false);
			obj2.Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		}
		Spine.AnimationState.SetAnimation(0, enragedFinishedAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < enragedPost))
			{
				break;
			}
			yield return null;
		}
		moving = true;
		currentAttack = null;
		roamTime = UnityEngine.Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);
	}

	public void TargetedShoot()
	{
		currentAttack = StartCoroutine(TargetedShootIE());
	}

	private IEnumerator TargetedShootIE()
	{
		DisableMovement();
		int randomAmount = (int)UnityEngine.Random.Range(amountOfRounds.x, amountOfRounds.y + 1f);
		float time;
		for (int i = 0; i < randomAmount; i++)
		{
			SimpleSpineFlash[] array = simpleSpineFlashes;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].Spine.AnimationState.SetAnimation(0, shootAttackAnticipationAnimation, true);
			}
			Spine.AnimationState.SetAnimation(0, shootAttackAnticipationAnimation, true);
			time = 0f;
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * Spine.timeScale);
				if (!(num < targetedShootingAnticipation))
				{
					break;
				}
				yield return null;
			}
			array = simpleSpineFlashes;
			foreach (SimpleSpineFlash obj in array)
			{
				obj.Spine.AnimationState.SetAnimation(0, shootAttackAnimation, false);
				obj.Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
			}
			Spine.AnimationState.SetAnimation(0, shootAttackAnimation, false);
			Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
			yield return StartCoroutine(ShootIE(true, null));
		}
		time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < targetedShootingPost))
			{
				break;
			}
			yield return null;
		}
		moving = true;
		currentAttack = null;
		roamTime = UnityEngine.Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);
	}

	public void Shoot()
	{
		StartCoroutine(ShootIE(false, null));
	}

	private IEnumerator ShootIE(bool shootAtTarget, System.Action callback)
	{
		int shotsToFire = (int)UnityEngine.Random.Range(amountOfShots.x, amountOfShots.y + 1f);
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= shotsToFire)
			{
				break;
			}
			AudioManager.Instance.PlayOneShot("event:/enemy/spit_gross_projectile", base.transform.position);
			Vector3 position = shootPosition.transform.position;
			position.z = 0f;
			float num2 = UnityEngine.Random.Range(shootDistance.x, shootDistance.y);
			float angle = UnityEngine.Random.Range(0f, 360f);
			if (shootAtTarget && GetClosestTarget() != null)
			{
				num2 = Vector3.Distance(base.transform.position, GetClosestTarget().transform.position) / 1.5f + UnityEngine.Random.Range(-2f, 2f);
				angle = Mathf.Repeat(Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position) + UnityEngine.Random.Range(-20f, 20f), 360f);
			}
			ObjectPool.Spawn(grenadeBullet, position).GetComponent<GrenadeBullet>().Play(-2f, angle, num2, gravSpeed);
			float dur = UnityEngine.Random.Range(delayBetweenShots.x, delayBetweenShots.y);
			float time = 0f;
			while (true)
			{
				float num3;
				time = (num3 = time + Time.deltaTime * Spine.timeScale);
				if (!(num3 < dur))
				{
					break;
				}
				yield return null;
			}
		}
		if (callback != null)
		{
			callback();
		}
	}

	private void ChargeAttack()
	{
		currentAttack = StartCoroutine(ChargeAttackIE());
	}

	private IEnumerator ChargeAttackIE()
	{
		DisableMovement();
		SimpleSpineFlash[] array = simpleSpineFlashes;
		foreach (SimpleSpineFlash obj in array)
		{
			obj.Spine.AnimationState.SetAnimation(0, chargeAttackAnticipationAnimation, false);
			obj.Spine.AnimationState.AddAnimation(0, chargeAttackingAnimation, true, 0f);
		}
		Spine.AnimationState.SetAnimation(0, chargeAttackAnticipationAnimation, false);
		Spine.AnimationState.AddAnimation(0, chargeAttackingAnimation, true, 0f);
		float progress = 0f;
		while (true)
		{
			float num;
			progress = (num = progress + Time.deltaTime * Spine.timeScale);
			if (!(num < chargeAnticipation))
			{
				break;
			}
			array = simpleSpineFlashes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].FlashWhite(progress / chargeAnticipation);
			}
			yield return null;
		}
		array = simpleSpineFlashes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].FlashWhite(false);
		}
		Health closestTarget = GetClosestTarget();
		if (closestTarget == null)
		{
			currentAttack = null;
			yield break;
		}
		Vector3 dir = (closestTarget.transform.position - base.transform.position).normalized;
		float angle = Utils.GetAngle(Vector3.zero, dir);
		state.facingAngle = angle;
		state.LookAngle = angle;
		maxSpeed = chargeMaxSpeed;
		speed = maxSpeed;
		moving = false;
		canBeKnockedBack = false;
		damageCollider.SetActive(true);
		while (!(Physics2D.Raycast(base.transform.position, dir, 1f, layerToCheck).collider != null))
		{
			move();
			yield return null;
		}
		damageCollider.SetActive(false);
		DisableMovement();
		canBeKnockedBack = true;
		AudioManager.Instance.PlayOneShot("event:/enemy/patrol_worm/patrol_worm_land", base.transform.position);
		array = simpleSpineFlashes;
		foreach (SimpleSpineFlash obj2 in array)
		{
			obj2.Spine.AnimationState.SetAnimation(0, chargeAttackImpactAnimation, false);
			obj2.Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		}
		Spine.AnimationState.SetAnimation(0, chargeAttackImpactAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < chargePost))
			{
				break;
			}
			yield return null;
		}
		maxSpeed = originalMaxSpeed;
		moving = true;
		currentAttack = null;
		roamTime = UnityEngine.Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);
	}

	public void SpawnEnemies()
	{
		currentAttack = StartCoroutine(SpawnEnemiesIE());
	}

	private IEnumerator SpawnEnemiesIE()
	{
		DisableMovement();
		SimpleSpineFlash[] array = simpleSpineFlashes;
		foreach (SimpleSpineFlash obj2 in array)
		{
			obj2.Spine.AnimationState.SetAnimation(0, spawnEnemiesAnimation, false);
			obj2.Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		}
		Spine.AnimationState.SetAnimation(0, spawnEnemiesAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		float time3 = 0f;
		while (true)
		{
			float num;
			time3 = (num = time3 + Time.deltaTime * Spine.timeScale);
			if (!(num < spawnEnemiesAnticipation))
			{
				break;
			}
			yield return null;
		}
		int amount = (int)UnityEngine.Random.Range(spawnAmount.x, spawnAmount.y + 1f);
		for (int i = 0; i < amount; i++)
		{
			Vector3 position = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(2.5f, 5f);
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(spawnables[UnityEngine.Random.Range(0, spawnables.Length)], position, Quaternion.identity, base.transform.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				UnitObject component = obj.Result.GetComponent<UnitObject>();
				component.CanHaveModifier = false;
				component.RemoveModifier();
			};
			float dur = UnityEngine.Random.Range(delayBetweenSpawns.x, delayBetweenSpawns.y);
			time3 = 0f;
			while (true)
			{
				float num;
				time3 = (num = time3 + Time.deltaTime * Spine.timeScale);
				if (!(num < dur))
				{
					break;
				}
				yield return null;
			}
		}
		time3 = 0f;
		while (true)
		{
			float num;
			time3 = (num = time3 + Time.deltaTime * Spine.timeScale);
			if (!(num < spawnEnemiesPost))
			{
				break;
			}
			yield return null;
		}
		moving = true;
		currentAttack = null;
		roamTime = UnityEngine.Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);
		spawnTime = UnityEngine.Random.Range(timeBetweenEnemySpawns.x, timeBetweenEnemySpawns.y);
	}

	private void DashAttack()
	{
		currentAttack = StartCoroutine(DashAttackIE());
	}

	protected virtual IEnumerator DashAttackIE()
	{
		DisableMovement();
		SimpleSpineFlash[] array = simpleSpineFlashes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Spine.AnimationState.SetAnimation(0, dashAttackAnticipationAnimation, true);
		}
		Spine.AnimationState.SetAnimation(0, dashAttackAnticipationAnimation, true);
		AudioManager.Instance.PlayOneShot("event:/enemy/chaser/chaser_charge", base.transform.position);
		Health closestTarget = GetClosestTarget();
		if (closestTarget == null)
		{
			currentAttack = null;
			yield break;
		}
		if (closestTarget != null)
		{
			state.LookAngle = Utils.GetAngle(base.transform.position, closestTarget.transform.position);
			state.facingAngle = state.LookAngle;
		}
		float progress = 0f;
		while (true)
		{
			float num;
			progress = (num = progress + Time.deltaTime * Spine.timeScale);
			if (!(num < dashAnticipation))
			{
				break;
			}
			array = simpleSpineFlashes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].FlashWhite(progress / dashAnticipation);
			}
			yield return null;
		}
		array = simpleSpineFlashes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].FlashWhite(false);
		}
		closestTarget = GetClosestTarget();
		if (closestTarget == null)
		{
			closestTarget = health;
		}
		state.LookAngle = Utils.GetAngle(base.transform.position, closestTarget.transform.position);
		state.facingAngle = state.LookAngle;
		DoKnockBack(closestTarget.gameObject, dashForce * -1f, 1f);
		AudioManager.Instance.PlayOneShot("event:/enemy/chaser/chaser_attack", base.transform.position);
		array = simpleSpineFlashes;
		foreach (SimpleSpineFlash obj in array)
		{
			obj.Spine.AnimationState.SetAnimation(0, dashAttackImpactAnimation, false);
			obj.Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		}
		Spine.AnimationState.SetAnimation(0, dashAttackImpactAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		StartCoroutine(EnabledDamageCollider(dashDuration / 1.5f));
		canBeKnockedBack = false;
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < dashDuration))
			{
				break;
			}
			yield return null;
		}
		canBeKnockedBack = true;
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < dashPost))
			{
				break;
			}
			yield return null;
		}
		DisableForces = false;
		moving = true;
		currentAttack = null;
		roamTime = UnityEngine.Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);
	}

	private IEnumerator EnabledDamageCollider(float duration)
	{
		damageCollider.SetActive(true);
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
		damageCollider.SetActive(false);
	}

	private void OnDamagedEnemy(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && component != health && component.team != health.team)
		{
			component.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.8f));
		}
	}
}
