using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyJellySpikerMiniBoss : UnitObject
{
	[Space]
	[SerializeField]
	private ColliderEvents damageColliderEvents;

	public SkeletonAnimation Spine;

	[SerializeField]
	private SimpleSpineFlash simpleSpineFlash;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string idleAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string closeAnticipationAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string distanceAnticipationAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string spikeAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string chargeAttackAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string chargeAttackEndAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string slamAnimation;

	[SerializeField]
	private float chargeAnticipation;

	[SerializeField]
	private float chargeSpeed;

	[SerializeField]
	private float chargeMinDistance;

	[SerializeField]
	private float chargeCooldown;

	[SerializeField]
	private ProjectilePattern hitWallProjectilePattern;

	[SerializeField]
	private float spikeAnticipation;

	[SerializeField]
	private float spikeColliderDuration;

	[SerializeField]
	private float spikeCooldown;

	[SerializeField]
	private float TurningArc = 90f;

	[SerializeField]
	private Vector2 DistanceRange = new Vector2(1f, 3f);

	[SerializeField]
	private List<Vector3> patrolRoute = new List<Vector3>();

	[SerializeField]
	private AssetReferenceGameObject spawnable;

	[SerializeField]
	private GameObject trailPrefab;

	[SerializeField]
	private int spikeAmount = 30;

	[SerializeField]
	private float delayBetweenSpikes = 0.05f;

	[SerializeField]
	private float distanceBetweenSpikes = 0.5f;

	[SerializeField]
	private float spikeScale = 1f;

	[SerializeField]
	private bool tripleBeams;

	[Space]
	[SerializeField]
	private bool spawnLongSpikes;

	[SerializeField]
	private int spikeRingAmount = 10;

	[SerializeField]
	private float delayBetweenSpikesRing = 0.1f;

	[SerializeField]
	private float distanceBetweenSpikesRing = 0.5f;

	[SerializeField]
	private int spikeRingLongAmount = 10;

	[Space]
	[SerializeField]
	private float knockback;

	[SerializeField]
	private SpriteRenderer aiming;

	private bool cooldown;

	private bool attacking;

	private bool anticipating;

	private bool chargeAttacking;

	private bool shortAttacking;

	private bool aboutToChargeAttacking;

	private float anticipationTimer;

	private float anticipationDuration;

	private float randomDirection;

	private float repathTimestamp;

	private int patrolIndex;

	private float repathTimeInterval = 2f;

	private float flashTickTimer;

	private Vector3 startPosition;

	private EventInstance LoopedSound;

	private int phase;

	private List<GameObject> spawnedSpikes = new List<GameObject>();

	private void Start()
	{
		aiming.gameObject.SetActive(false);
		startPosition = base.transform.position;
		damageColliderEvents.OnTriggerEnterEvent += OnTriggerEnterEvent;
		SpawnEnemies();
	}

	private void SpawnEnemies()
	{
		if (spawnable == null)
		{
			return;
		}
		List<Vector3> list = new List<Vector3>(2)
		{
			new Vector3(-2.5f, 0f, 0f),
			new Vector3(2.5f, 0f, 0f)
		};
		for (int i = 0; i < 2; i++)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(spawnable, list[i], Quaternion.identity, base.transform.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				obj.Result.gameObject.SetActive(false);
				EnemySpawner.CreateWithAndInitInstantiatedEnemy(obj.Result.transform.position, base.transform.parent, obj.Result);
			};
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		attacking = false;
		anticipating = false;
		cooldown = false;
		chargeAttacking = false;
	}

	public override void Update()
	{
		base.Update();
		float num = (PlayerFarming.Instance ? Vector3.Distance(PlayerFarming.Instance.transform.position, base.transform.position) : float.MaxValue);
		if (!attacking && !anticipating && !cooldown)
		{
			if (!chargeAttacking && num > chargeMinDistance && num < (float)VisionRange)
			{
				switch (UnityEngine.Random.Range(0, spawnLongSpikes ? 3 : 2))
				{
				case 0:
					StartCoroutine(BeginChargeAttackIE());
					break;
				case 1:
					StartCoroutine(SpawnSpikesInLineIE(spikeAmount, delayBetweenSpikes, distanceBetweenSpikes));
					break;
				default:
					StartCoroutine(SpawnSpikesInCircleLongIE(8, delayBetweenSpikes));
					break;
				}
			}
			else if (num < chargeMinDistance)
			{
				switch (UnityEngine.Random.Range(0, spawnLongSpikes ? 3 : 2))
				{
				case 0:
					StartCoroutine(SpikeAttackIE());
					break;
				case 1:
					StartCoroutine(SpawnSpikesInCircleIE(spikeRingAmount, delayBetweenSpikesRing, distanceBetweenSpikesRing));
					break;
				default:
					StartCoroutine(SpawnSpikesInCircleLongIE(8, delayBetweenSpikes));
					break;
				}
			}
		}
		aiming.gameObject.SetActive(anticipating && (aboutToChargeAttacking || shortAttacking));
		if (anticipating && PlayerFarming.Instance != null)
		{
			anticipationTimer += Time.deltaTime;
			flashTickTimer += Time.deltaTime;
			float num2 = anticipationTimer / anticipationDuration;
			simpleSpineFlash.FlashWhite(num2 * 0.75f);
			aiming.transform.eulerAngles = new Vector3(0f, 0f, Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position));
			if (flashTickTimer >= 0.12f && BiomeConstants.Instance.IsFlashLightsActive)
			{
				aiming.color = ((aiming.color == Color.red) ? Color.white : Color.red);
				flashTickTimer = 0f;
			}
		}
		if (chargeAttacking)
		{
			speed = chargeSpeed;
			maxSpeed = chargeSpeed;
			move();
		}
		else
		{
			GameManager instance = GameManager.GetInstance();
			if ((((object)instance != null) ? new float?(instance.CurrentTime) : null) > repathTimestamp)
			{
				UpdateMovement();
			}
			else
			{
				maxSpeed = 0.015f;
			}
		}
		if (health != null)
		{
			if (phase == 0 && health.HP < health.totalHP * 0.66f)
			{
				phase = 1;
				SpawnEnemies();
			}
			else if (phase == 1 && health.HP < health.totalHP * 0.33f)
			{
				phase = 2;
				SpawnEnemies();
			}
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		simpleSpineFlash.FlashFillRed();
		if (knockback != 0f && !chargeAttacking)
		{
			DoKnockBack(Attacker, knockback, 0.5f);
		}
	}

	private void UpdateMovement()
	{
		if (patrolRoute.Count == 0)
		{
			GetRandomTargetPosition();
		}
		else if (pathToFollow == null)
		{
			patrolIndex = ++patrolIndex % patrolRoute.Count;
			givePath(startPosition + patrolRoute[patrolIndex]);
			float angle = Utils.GetAngle(base.transform.position, startPosition + patrolRoute[patrolIndex]);
			LookAtAngle(angle);
		}
		repathTimestamp = GameManager.GetInstance().CurrentTime + repathTimeInterval;
	}

	private void GetRandomTargetPosition()
	{
		float num = 100f;
		while ((num -= 1f) > 0f)
		{
			float num2 = UnityEngine.Random.Range(DistanceRange.x, DistanceRange.y);
			randomDirection += UnityEngine.Random.Range(0f - TurningArc, TurningArc) * ((float)Math.PI / 180f);
			float radius = 0.2f;
			Vector3 vector = base.transform.position + new Vector3(num2 * Mathf.Cos(randomDirection), num2 * Mathf.Sin(randomDirection));
			if (Physics2D.CircleCast(base.transform.position, radius, Vector3.Normalize(vector - base.transform.position), num2, layerToCheck).collider != null)
			{
				randomDirection = 180f - randomDirection;
				continue;
			}
			float angle = Utils.GetAngle(base.transform.position, vector);
			givePath(vector);
			LookAtAngle(angle);
			break;
		}
	}

	private IEnumerator BeginChargeAttackIE()
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/warning", base.gameObject);
		LoopedSound = AudioManager.Instance.CreateLoop("event:/enemy/jellyfish_miniboss/jellyfish_miniboss_charge", base.gameObject, true);
		Spine.AnimationState.SetAnimation(0, distanceAnticipationAnimation, false);
		aboutToChargeAttacking = true;
		anticipating = true;
		anticipationTimer = 0f;
		anticipationDuration = chargeAnticipation;
		yield return new WaitForSeconds(chargeAnticipation);
		AudioManager.Instance.StopLoop(LoopedSound);
		anticipating = false;
		aboutToChargeAttacking = false;
		ChargeAtTarget();
		yield return new WaitForEndOfFrame();
		simpleSpineFlash.FlashWhite(false);
	}

	private void ChargeAtTarget()
	{
		attacking = true;
		chargeAttacking = true;
		ClearPaths();
		damageColliderEvents.gameObject.SetActive(true);
		AudioManager.Instance.PlayOneShot("event:/enemy/spike_trap/spike_trap_trigger", base.gameObject);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/attack", base.gameObject);
		Spine.AnimationState.SetAnimation(0, chargeAttackAnimation, true);
		LookAtTarget();
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (chargeAttacking && (layerToCheck.value & (1 << collision.gameObject.layer)) > 0)
		{
			AttackEnd();
		}
	}

	private void AttackEnd()
	{
		CameraManager.instance.ShakeCameraForDuration(0.5f, 1.5f, 0.5f);
		MMVibrate.Haptic(MMVibrate.HapticTypes.LightImpact);
		AudioManager.Instance.PlayOneShot("event:/enemy/impact_squishy", base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/gethit", base.gameObject);
		if (hitWallProjectilePattern != null)
		{
			hitWallProjectilePattern.Shoot();
		}
		if (chargeAttacking)
		{
			chargeAttacking = false;
			attacking = false;
			ClearPaths();
			Spine.AnimationState.SetAnimation(0, chargeAttackEndAnimation, false);
			Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
			damageColliderEvents.gameObject.SetActive(false);
			speed = 0f;
			cooldown = true;
			StartCoroutine(CoolingDown(chargeCooldown));
		}
	}

	private IEnumerator SpikeAttackIE()
	{
		Spine.AnimationState.SetAnimation(0, distanceAnticipationAnimation, false);
		shortAttacking = true;
		anticipating = true;
		anticipationTimer = 0f;
		anticipationDuration = spikeAnticipation;
		yield return new WaitForSeconds(anticipationDuration);
		DoKnockBack(Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position) * ((float)Math.PI / 180f), 1.5f, 1.5f);
		anticipating = false;
		attacking = true;
		yield return new WaitForEndOfFrame();
		simpleSpineFlash.FlashWhite(false);
		SpikeAttack();
	}

	private void SpikeAttack()
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/spike_trap/spike_trap_trigger", base.gameObject);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/attack", base.gameObject);
		Spine.AnimationState.SetAnimation(0, spikeAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		ClearPaths();
		StartCoroutine(TurnOnDamageColliderForDuration(spikeColliderDuration));
		cooldown = true;
		attacking = false;
		shortAttacking = false;
		StartCoroutine(CoolingDown(spikeCooldown));
	}

	private IEnumerator CoolingDown(float duration)
	{
		yield return new WaitForSeconds(duration);
		cooldown = false;
	}

	private void LookAtTarget()
	{
		float angle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
		LookAtAngle(angle);
	}

	private void LookAtAngle(float angle)
	{
		state.facingAngle = angle;
		state.LookAngle = angle;
	}

	private void OnTriggerEnterEvent(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && component.team != health.team)
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

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		if (!base.transform.parent.GetComponent<MiniBossController>())
		{
			return;
		}
		for (int i = 0; i < Health.team2.Count; i++)
		{
			if (Health.team2[i] != null && Health.team2[i] != health)
			{
				Health.team2[i].invincible = false;
				Health.team2[i].enabled = true;
				Health.team2[i].DealDamage(Health.team2[i].totalHP, base.gameObject, base.transform.position);
			}
		}
	}

	private GameObject GetSpawnSpike()
	{
		GameObject gameObject = null;
		if (spawnedSpikes.Count > 0)
		{
			foreach (GameObject spawnedSpike in spawnedSpikes)
			{
				if (!spawnedSpike.activeSelf)
				{
					gameObject = spawnedSpike;
					gameObject.transform.position = base.transform.position;
					gameObject.SetActive(true);
					break;
				}
			}
		}
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate(trailPrefab, base.transform.position, Quaternion.identity, base.transform.parent);
			spawnedSpikes.Add(gameObject);
			ColliderEvents componentInChildren = gameObject.GetComponentInChildren<ColliderEvents>();
			if ((bool)componentInChildren)
			{
				componentInChildren.OnTriggerEnterEvent += OnDamageTriggerEnter;
			}
		}
		return gameObject;
	}

	protected virtual void OnDamageTriggerEnter(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && (component.team != health.team || health.team == Health.Team.PlayerTeam))
		{
			component.DealDamage(1f, component.gameObject, component.transform.position);
		}
	}

	private IEnumerator SpawnSpikesInLineIE(int amount, float delayBetweenSpikes, float distanceBetweenSpikes)
	{
		attacking = true;
		anticipating = true;
		anticipationTimer = 0f;
		anticipationDuration = spikeAnticipation;
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/warning", base.gameObject);
		LoopedSound = AudioManager.Instance.CreateLoop("event:/enemy/jellyfish_miniboss/jellyfish_miniboss_charge", base.gameObject, true);
		Spine.AnimationState.SetAnimation(0, closeAnticipationAnimation, false);
		yield return new WaitForSeconds(chargeAnticipation);
		AudioManager.Instance.StopLoop(LoopedSound);
		anticipating = false;
		CameraManager.instance.ShakeCameraForDuration(0.1f, 0.1f, 0.1f, false);
		yield return new WaitForEndOfFrame();
		simpleSpineFlash.FlashWhite(false);
		Spine.AnimationState.SetAnimation(0, spikeAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/enemy/spike_trap/spike_trap_trigger", base.gameObject);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/attack", base.gameObject);
		Vector3 normalized = (PlayerFarming.Instance.transform.position - base.transform.position).normalized;
		if (tripleBeams)
		{
			StartCoroutine(ShootSpikesInDirectionIE(amount, Quaternion.Euler(0f, 0f, -15f) * normalized, delayBetweenSpikes, distanceBetweenSpikes));
			StartCoroutine(ShootSpikesInDirectionIE(amount, Quaternion.Euler(0f, 0f, 15f) * normalized, delayBetweenSpikes, distanceBetweenSpikes));
		}
		yield return StartCoroutine(ShootSpikesInDirectionIE(amount, normalized, delayBetweenSpikes, distanceBetweenSpikes));
		yield return new WaitForSeconds(1f);
		attacking = false;
	}

	private IEnumerator SpawnSpikesInCircleIE(int amount, float delayBetweenSpikes, float distanceBetweenSpikes)
	{
		attacking = true;
		anticipating = true;
		anticipationTimer = 0f;
		anticipationDuration = spikeAnticipation;
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/warning", base.gameObject);
		LoopedSound = AudioManager.Instance.CreateLoop("event:/enemy/jellyfish_miniboss/jellyfish_miniboss_charge", base.gameObject, true);
		Spine.AnimationState.SetAnimation(0, closeAnticipationAnimation, false);
		yield return new WaitForSeconds(anticipationDuration);
		AudioManager.Instance.StopLoop(LoopedSound);
		anticipating = false;
		CameraManager.instance.ShakeCameraForDuration(0.5f, 1.5f, 0.5f);
		yield return new WaitForEndOfFrame();
		simpleSpineFlash.FlashWhite(false);
		Spine.AnimationState.SetAnimation(0, spikeAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/enemy/spike_trap/spike_trap_trigger", base.gameObject);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/attack", base.gameObject);
		int num = UnityEngine.Random.Range(0, 360);
		for (int i = 0; i < amount; i++)
		{
			StartCoroutine(ShootSpikesInDirectionIE(direction: new Vector3(Mathf.Cos((float)num * ((float)Math.PI / 180f)), Mathf.Sin((float)num * ((float)Math.PI / 180f)), 0f), amount: spikeRingAmount, delayBetweenSpikes: delayBetweenSpikesRing, distanceBetweenSpikes: distanceBetweenSpikesRing));
			num = (int)Mathf.Repeat(num + 360 / amount, 360f);
		}
		yield return new WaitForSeconds(spikeAnticipation + delayBetweenSpikes + 1f);
		attacking = false;
	}

	private IEnumerator SpawnSpikesInCircleLongIE(int amount, float delayBetweenSpikes)
	{
		attacking = true;
		anticipating = true;
		anticipationTimer = 0f;
		anticipationDuration = spikeAnticipation;
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/warning", base.gameObject);
		LoopedSound = AudioManager.Instance.CreateLoop("event:/enemy/jellyfish_miniboss/jellyfish_miniboss_charge", base.gameObject, true);
		Spine.AnimationState.SetAnimation(0, slamAnimation, false);
		bool waiting = true;
		Spine.AnimationState.Event += delegate(TrackEntry trackEntry, global::Spine.Event e)
		{
			if (e.Data.Name == "slam-impact")
			{
				waiting = false;
			}
		};
		while (waiting || Spine.timeScale == 0.0001f)
		{
			yield return null;
		}
		AudioManager.Instance.StopLoop(LoopedSound);
		anticipating = false;
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, 0.1f, false);
		yield return new WaitForEndOfFrame();
		simpleSpineFlash.FlashWhite(false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/enemy/spike_trap/spike_trap_trigger", base.gameObject);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/attack", base.gameObject);
		int num = UnityEngine.Random.Range(0, 360);
		for (int i = 0; i < amount; i++)
		{
			StartCoroutine(ShootSpikesInDirectionIE(direction: new Vector3(Mathf.Cos((float)num * ((float)Math.PI / 180f)), Mathf.Sin((float)num * ((float)Math.PI / 180f)), 0f), amount: spikeRingLongAmount, delayBetweenSpikes: delayBetweenSpikes, distanceBetweenSpikes: distanceBetweenSpikes));
			num = (int)Mathf.Repeat(num + 360 / amount, 360f);
		}
		yield return new WaitForSeconds(spikeAnticipation + delayBetweenSpikes + 1f);
		attacking = false;
	}

	public IEnumerator ShootSpikesInDirectionIE(int amount, Vector3 direction, float delayBetweenSpikes, float distanceBetweenSpikes)
	{
		Vector3 position = base.transform.position;
		for (int i = 0; i < amount; i++)
		{
			GameObject spawnSpike = GetSpawnSpike();
			spawnSpike.transform.localScale = Vector3.one * spikeScale;
			spawnSpike.transform.position = position;
			position += direction * distanceBetweenSpikes;
			while (PlayerRelic.TimeFrozen)
			{
				yield return null;
			}
			yield return new WaitForSeconds(delayBetweenSpikes);
		}
	}
}
