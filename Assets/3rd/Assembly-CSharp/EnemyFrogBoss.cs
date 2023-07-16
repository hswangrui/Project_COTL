using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Spine;
using Spine.Unity;
using Unify;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyFrogBoss : UnitObject
{
	private const float PHASE3_SPAWN_MULTIPLAYER = 1.6f;

	public SkeletonAnimation Spine;

	[SerializeField]
	private SimpleSpineFlash simpleSpineFlash;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string idleAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string hopAnticipationAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string hopAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string hopEndAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string mortarStrikeAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string mortarValleyAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string eggSpawnAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string burpAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string bounceAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string enragedAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string tongueAttackAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string tongueEndAnimation;

	[SerializeField]
	private CircleCollider2D physicsCollider;

	[SerializeField]
	private float hopAnticipation;

	[SerializeField]
	private float hopDuration;

	[SerializeField]
	private float hopSpeed;

	[SerializeField]
	private float hopZHeight;

	[SerializeField]
	private AnimationCurve hopSpeedCurve;

	[SerializeField]
	private AnimationCurve hopZCurve;

	[SerializeField]
	private ColliderEvents damageColliderEvents;

	[SerializeField]
	private LayerMask unitMask;

	[SerializeField]
	private GameObject shadow;

	[SerializeField]
	private MortarBomb randomMortarPrefab;

	[SerializeField]
	private float randomMortarDuration;

	[SerializeField]
	private float randomMortarAnticipation;

	[SerializeField]
	private Vector2 randomMortarsToSpawn;

	[SerializeField]
	private Vector2 randomMortarDistance;

	[SerializeField]
	private Vector2 randomMortarDelayBetweenShots;

	[SerializeField]
	private MortarBomb targetedMortarPrefab;

	[SerializeField]
	private float targetedMortarDuration;

	[SerializeField]
	private float targetedMortarAnticipation;

	[SerializeField]
	private Vector2 targetedMortarsToSpawn;

	[SerializeField]
	private Vector2 targetedMortarDelayBetweenShots;

	[SerializeField]
	private GameObject eggPrefab;

	[SerializeField]
	private Vector2 eggsToSpawn;

	[SerializeField]
	private float eggSpawnDelay;

	[SerializeField]
	private float eggsMultipleAnticipation;

	[SerializeField]
	private Vector2 eggsMultipleToSpawn;

	[SerializeField]
	private Vector2 eggsMultipleSpawnDelay;

	[SerializeField]
	private Vector2 eggsKnockback;

	[SerializeField]
	private ParticleSystem aoeParticles;

	[SerializeField]
	private float aoeDuration;

	[SerializeField]
	private Vector2 projectilesToSpawn;

	[SerializeField]
	private Vector2 projectileDelayBetweenSpawn;

	[SerializeField]
	private GameObject burpPosition;

	[SerializeField]
	private float burpAnticipation;

	[SerializeField]
	private GameObject projectilePrefab;

	[SerializeField]
	private Vector2 bounces;

	[SerializeField]
	private float bounceAnticipation;

	[SerializeField]
	private Vector2 timeBetweenBounce;

	[SerializeField]
	private Vector3 sitPosition = new Vector3(0f, 7.75f, -0.2f);

	[SerializeField]
	private float tongueSpitDelay = 0.25f;

	[SerializeField]
	private FrogBossTongue tonguePrefab;

	[SerializeField]
	private GameObject tonguePosition;

	[SerializeField]
	private float tongueWhipAnticipation = 1f;

	[SerializeField]
	private float tongueWhipDuration = 0.5f;

	[SerializeField]
	private float tongueRetrieveDelay = 2f;

	[SerializeField]
	private float tongueRetrieveDuration = 0.1f;

	[SerializeField]
	private Vector2 tongueWhipDelay;

	[SerializeField]
	private Vector2 tongueWhipAmount;

	[SerializeField]
	private float tongueScatterRadius = 10f;

	[SerializeField]
	private float tongueScatterPostAnticipation;

	[SerializeField]
	private float tongueScatterPreAnticipation;

	[SerializeField]
	private float tongueScatterWhipDuration;

	[SerializeField]
	private Vector2 tongueScatterDelay;

	[SerializeField]
	private Vector2 tongueScatterAmount;

	[SerializeField]
	private float enragedDuration = 2f;

	[SerializeField]
	[Range(0f, 1f)]
	private float p2HealthThreshold = 0.6f;

	[SerializeField]
	[Range(0f, 1f)]
	private float p3HealthThreshold = 0.3f;

	[SerializeField]
	private AssetReferenceGameObject miniBossFrogTarget;

	[SerializeField]
	private AssetReferenceGameObject miniBossOther;

	[SerializeField]
	private float miniBossSpawningDelay = 0.5f;

	[SerializeField]
	private int miniBossFrogsSpawnAmount;

	[SerializeField]
	private float spawnForce = 0.75f;

	[SerializeField]
	private Vector3[] miniBossSpawnPositions = new Vector3[0];

	[SerializeField]
	private int maxEnemies;

	[SerializeField]
	private AssetReferenceGameObject enemyHopperTarget;

	[SerializeField]
	private AssetReferenceGameObject[] enemyRare;

	[SerializeField]
	private Vector2 spawnDelay;

	[SerializeField]
	private Vector3[] spawnPositions = new Vector3[0];

	[Space]
	[SerializeField]
	private Renderer distortionObject;

	[SerializeField]
	private Interaction_MonsterHeart interaction_MonsterHeart;

	[SerializeField]
	private GameObject playerBlocker;

	[SerializeField]
	private GameObject followerToSpawn;

	private GameObject cameraTarget;

	private bool attacking;

	private bool anticipating;

	private bool hasCollidedWithObstacle;

	private bool queuePhaseIncrement;

	private float anticipationTimer;

	private float anticipationDuration;

	private float targetAngle;

	private float startingHealth;

	private int currentPhaseNumber;

	private Coroutine currentPhaseRoutine;

	private Coroutine currentAttackRoutine;

	private IEnumerator damageColliderRoutine;

	private Collider2D collider;

	private ShowHPBar hpBar;

	private int enemiesAlive;

	private List<UnitObject> spawnedEnemies = new List<UnitObject>();

	private List<FrogBossTongue> tongues = new List<FrogBossTongue>();

	private List<Projectile> projectiles = new List<Projectile>();

	private Projectile baseProjectile;

	private bool isDead;

	private bool facePlayer;

	private bool usingTongue;

	private bool active;

	private float spawnTimestamp;

	private int miniBossesKilled;

	private int previousAttackIndex;

	private bool juicedForm;

	public GameObject TonguePosition
	{
		get
		{
			return tonguePosition;
		}
	}

	public override void Awake()
	{
		base.Awake();
		cameraTarget = GetComponentInParent<MiniBossController>().cameraTarget;
	}

	private void Start()
	{
		damageColliderEvents.OnTriggerEnterEvent += OnTriggerEnterEvent;
		Spine.AnimationState.Event += AnimationEvent;
		collider = GetComponent<Collider2D>();
		hpBar = GetComponent<ShowHPBar>();
		startingHealth = health.HP;
		if (DataManager.Instance.playerDeathsInARowFightingLeader >= 2)
		{
			maxEnemies -= 3;
		}
		facePlayer = true;
		juicedForm = GameManager.Layer2;
		if (juicedForm)
		{
			maxEnemies = (int)((float)maxEnemies * 1.33f);
			health.totalHP *= 1.5f;
			health.HP = health.totalHP;
			miniBossFrogsSpawnAmount = 4;
			randomMortarDuration /= 1.5f;
		}
		health.SlowMoOnkill = false;
		InitializeMortarStrikes();
		InitializeBurpingProjectiles();
	}

	public override void Update()
	{
		if (!usingTongue)
		{
			base.Update();
		}
		if (anticipating)
		{
			anticipationTimer += Time.deltaTime * Spine.timeScale;
			if (anticipationTimer / anticipationDuration > 1f)
			{
				anticipating = false;
				anticipationTimer = 0f;
			}
		}
		if (state.CURRENT_STATE == StateMachine.State.Moving && hasCollidedWithObstacle)
		{
			speed *= 0.5f;
		}
		if (queuePhaseIncrement)
		{
			queuePhaseIncrement = false;
			IncrementPhase();
		}
		if (currentPhaseNumber > 0)
		{
			GameManager instance = GameManager.GetInstance();
			if ((((object)instance != null) ? new float?(instance.CurrentTime) : null) > spawnTimestamp && !isDead)
			{
				SpawnEnemy();
			}
		}
		if ((bool)PlayerFarming.Instance && facePlayer && !isDead)
		{
			LookAt(GetAngleToTarget());
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		simpleSpineFlash.FlashFillRed(0.25f);
		AudioManager.Instance.PlayOneShot("event:/boss/frog/get_hit");
		Vector3 position = (AttackLocation + Attacker.transform.position) / 2f;
		BiomeConstants.Instance.EmitHitVFX(position, Quaternion.identity.z, "HitFX_Weak");
		float num = health.HP / startingHealth;
		if ((currentPhaseNumber == 1 && num <= p2HealthThreshold) || (currentPhaseNumber == 2 && num <= p3HealthThreshold))
		{
			IncrementPhase();
		}
	}

	private void IncrementPhase()
	{
		if (attacking)
		{
			queuePhaseIncrement = true;
			return;
		}
		if (currentAttackRoutine != null)
		{
			StopCoroutine(currentAttackRoutine);
		}
		if (currentPhaseRoutine != null)
		{
			StopCoroutine(currentPhaseRoutine);
		}
		currentPhaseNumber++;
		usingTongue = false;
		anticipating = false;
		if (currentPhaseNumber == 2)
		{
			BeginPhase2();
		}
		else if (currentPhaseNumber >= 3)
		{
			BeginPhase3();
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (!active)
		{
			return;
		}
		StopCoroutine(currentAttackRoutine);
		StopCoroutine(currentPhaseRoutine);
		usingTongue = false;
		foreach (FrogBossTongue tongue in tongues)
		{
			tongue.gameObject.SetActive(false);
		}
		StartCoroutine(DelayAddCamera());
		if (currentPhaseNumber == 1)
		{
			currentPhaseRoutine = StartCoroutine(Phase1IE(false));
		}
		else if (currentPhaseNumber == 2)
		{
			currentPhaseRoutine = StartCoroutine(Phase2IE(false));
		}
		else if (currentPhaseNumber == 3 && miniBossesKilled >= miniBossFrogsSpawnAmount)
		{
			currentPhaseRoutine = StartCoroutine(Phase3IE(false));
		}
	}

	private IEnumerator DelayAddCamera()
	{
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().AddToCamera(cameraTarget);
		GameManager.GetInstance().CamFollowTarget.MinZoom = 9f;
		GameManager.GetInstance().CamFollowTarget.MaxZoom = 18f;
	}

	public void BeginPhase1()
	{
		simpleSpineFlash.SetFacing = SimpleSpineFlash.SetFacingMode.None;
		GameManager.GetInstance().AddToCamera(cameraTarget);
		GameManager.GetInstance().CamFollowTarget.MinZoom = 9f;
		GameManager.GetInstance().CamFollowTarget.MaxZoom = 18f;
		currentPhaseNumber = 1;
		currentPhaseRoutine = StartCoroutine(Phase1IE(true));
	}

	private IEnumerator Phase1IE(bool firstLoop)
	{
		active = true;
		while (PlayerFarming.Instance == null)
		{
			yield return null;
		}
		bool flag = firstLoop;
		for (int i = 0; i < 3; i++)
		{
			int num = previousAttackIndex;
			while (previousAttackIndex == num)
			{
				num = UnityEngine.Random.Range(0, 3);
			}
			previousAttackIndex = num;
			switch (num)
			{
			case 0:
				if (juicedForm && UnityEngine.Random.value > 0.5f)
				{
					yield return currentAttackRoutine = StartCoroutine(HopToPositionIE(PlayerFarming.Instance.transform.position));
				}
				else
				{
					yield return currentAttackRoutine = StartCoroutine(HopIE(hopAnticipation, GetAngleToTarget()));
				}
				yield return currentAttackRoutine = StartCoroutine(BurpProjectilesIE());
				break;
			case 1:
				yield return currentAttackRoutine = StartCoroutine(HopToPositionIE(sitPosition));
				yield return currentAttackRoutine = StartCoroutine(TongueRapidAttackIE());
				break;
			case 2:
				yield return currentAttackRoutine = StartCoroutine(HopToPositionIE(PlayerFarming.Instance.transform.position));
				yield return currentAttackRoutine = StartCoroutine(MortarStrikeTargetedIE());
				yield return new WaitForSeconds(1f);
				break;
			}
		}
		currentPhaseRoutine = StartCoroutine(Phase1IE(false));
	}

	private void BeginPhase2()
	{
		currentPhaseRoutine = StartCoroutine(Phase2IE(true));
	}

	private IEnumerator Phase2IE(bool firstLoop)
	{
		while (PlayerFarming.Instance == null)
		{
			yield return null;
		}
		if (firstLoop)
		{
			yield return EnragedIE();
		}
		yield return new WaitForEndOfFrame();
		for (int i = 0; i < 3; i++)
		{
			int num = previousAttackIndex;
			while (previousAttackIndex == num)
			{
				num = UnityEngine.Random.Range(0, 3);
			}
			previousAttackIndex = num;
			switch (num)
			{
			case 0:
				yield return currentAttackRoutine = StartCoroutine(HopToPositionIE(PlayerFarming.Instance.transform.position));
				yield return currentAttackRoutine = StartCoroutine(MortarStrikeRandomIE());
				break;
			case 1:
				if (juicedForm && UnityEngine.Random.value > 0.5f)
				{
					yield return currentAttackRoutine = StartCoroutine(HopToPositionIE(PlayerFarming.Instance.transform.position));
				}
				else
				{
					yield return currentAttackRoutine = StartCoroutine(HopIE(hopAnticipation, GetAngleToTarget()));
				}
				yield return currentAttackRoutine = StartCoroutine(BurpProjectilesIE(1.3f));
				break;
			case 2:
				yield return currentAttackRoutine = StartCoroutine(HopToPositionIE(sitPosition));
				yield return currentAttackRoutine = StartCoroutine(TongueRapidAttackIE());
				break;
			}
		}
		currentPhaseRoutine = StartCoroutine(Phase2IE(false));
	}

	private void BeginPhase3()
	{
		currentPhaseRoutine = StartCoroutine(Phase3IE(true));
	}

	private IEnumerator Phase3IE(bool firstLoop)
	{
		while (PlayerFarming.Instance == null)
		{
			yield return null;
		}
		if (firstLoop)
		{
			yield return StartCoroutine(HopToPositionIE(Vector3.zero));
			StartCoroutine(SpawnMiniBossesIE());
			yield return StartCoroutine(EnragedIE());
			yield return StartCoroutine(HopUpIE(hopAnticipation));
			yield break;
		}
		for (int i = 0; i < 3; i++)
		{
			int num = previousAttackIndex;
			while (previousAttackIndex == num)
			{
				num = UnityEngine.Random.Range(0, 3);
			}
			previousAttackIndex = num;
			switch (num)
			{
			case 0:
				if (juicedForm && UnityEngine.Random.value > 0.5f)
				{
					yield return currentAttackRoutine = StartCoroutine(HopToPositionIE(PlayerFarming.Instance.transform.position));
				}
				else
				{
					yield return currentAttackRoutine = StartCoroutine(HopIE(hopAnticipation, GetAngleToTarget()));
				}
				yield return currentAttackRoutine = StartCoroutine(MortarStrikeTargetedIE());
				break;
			case 1:
				yield return StartCoroutine(HopToPositionIE(sitPosition));
				yield return currentAttackRoutine = StartCoroutine(TongueScatterAttackIE());
				break;
			case 2:
				if (juicedForm && UnityEngine.Random.value > 0.5f)
				{
					yield return currentAttackRoutine = StartCoroutine(HopToPositionIE(PlayerFarming.Instance.transform.position));
				}
				else
				{
					yield return currentAttackRoutine = StartCoroutine(HopIE(hopAnticipation, GetAngleToTarget()));
				}
				yield return currentAttackRoutine = StartCoroutine(BurpProjectilesIE(1.6f));
				break;
			}
		}
		currentPhaseRoutine = StartCoroutine(Phase3IE(false));
	}

	private void MortarStrikeRandom()
	{
		StartCoroutine(MortarStrikeRandomIE());
	}

	private void InitializeMortarStrikes()
	{
		List<MortarBomb> list = new List<MortarBomb>();
		for (int i = 0; (float)i < targetedMortarsToSpawn.y; i++)
		{
			MortarBomb mortarBomb = ObjectPool.Spawn(targetedMortarPrefab, base.transform.parent);
			mortarBomb.destroyOnFinish = false;
			list.Add(mortarBomb);
		}
		for (int j = 0; (float)j < randomMortarsToSpawn.y; j++)
		{
			MortarBomb mortarBomb2 = ObjectPool.Spawn(randomMortarPrefab, base.transform.parent);
			mortarBomb2.destroyOnFinish = false;
			list.Add(mortarBomb2);
		}
		for (int k = 0; k < list.Count; k++)
		{
			list[k].gameObject.Recycle();
		}
	}

	private IEnumerator MortarStrikeRandomIE()
	{
		anticipating = true;
		anticipationDuration = randomMortarAnticipation;
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < anticipationDuration))
			{
				break;
			}
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		attacking = true;
		facePlayer = false;
		Spine.AnimationState.SetAnimation(0, mortarValleyAnimation, false);
		time2 = 0f;
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
		int shotsToFire = (int)UnityEngine.Random.Range(randomMortarsToSpawn.x, randomMortarsToSpawn.y);
		float aimingAngle = UnityEngine.Random.Range(0f, 360f);
		for (int i = 0; i < shotsToFire; i++)
		{
			Vector3 targetPosition = base.transform.position + (Vector3)Utils.DegreeToVector2(aimingAngle) * UnityEngine.Random.Range(randomMortarDistance.x, randomMortarDistance.y);
			StartCoroutine(ShootMortarPosition(targetPosition));
			aimingAngle += (float)(360 / shotsToFire);
			float dur = UnityEngine.Random.Range(randomMortarDelayBetweenShots.x, randomMortarDelayBetweenShots.y);
			time2 = 0f;
			while (true)
			{
				float num;
				time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num < dur))
				{
					break;
				}
				yield return null;
			}
		}
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		attacking = false;
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
		facePlayer = true;
	}

	private void MortarStrikeTargeted()
	{
		StartCoroutine(MortarStrikeTargetedIE());
	}

	private IEnumerator MortarStrikeTargetedIE()
	{
		anticipating = true;
		anticipationDuration = targetedMortarAnticipation;
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < anticipationDuration))
			{
				break;
			}
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		attacking = true;
		int shotsToFire = (int)UnityEngine.Random.Range(targetedMortarsToSpawn.x, targetedMortarsToSpawn.y);
		for (int i = 0; i < shotsToFire; i++)
		{
			Spine.AnimationState.SetAnimation(0, mortarStrikeAnimation, false);
			AudioManager.Instance.PlayOneShot("event:/boss/frog/mortar_spit");
			float dur = UnityEngine.Random.Range(targetedMortarDelayBetweenShots.x, targetedMortarDelayBetweenShots.y);
			time2 = 0f;
			while (true)
			{
				float num;
				time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num < dur))
				{
					break;
				}
				yield return null;
			}
		}
		Spine.AnimationState.SetAnimation(0, idleAnimation, false);
		attacking = false;
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (num < 0.5f)
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator ShootMortarTarget()
	{
		MortarBomb mortarBomb = ObjectPool.Spawn(targetedMortarPrefab, base.transform.parent, (Vector3)AstarPath.active.GetNearest(PlayerFarming.Instance.transform.position).node.position, Quaternion.identity);
		mortarBomb.destroyOnFinish = false;
		float num = Mathf.Clamp(Vector3.Distance(base.transform.position, burpPosition.transform.position) / 3f, 1f, float.MaxValue);
		mortarBomb.Play(burpPosition.transform.position, targetedMortarDuration * num, Health.Team.Team2);
		AudioManager.Instance.PlayOneShot("event:/boss/frog/mortar_spawn");
		yield return new WaitForSeconds(targetedMortarDuration * num);
		AudioManager.Instance.PlayOneShot("event:/boss/frog/mortar_explode");
	}

	private IEnumerator ShootMortarPosition(Vector3 targetPosition)
	{
		MortarBomb mortarBomb = ObjectPool.Spawn(randomMortarPrefab, base.transform.parent, (Vector3)AstarPath.active.GetNearest(targetPosition).node.position, Quaternion.identity);
		mortarBomb.destroyOnFinish = false;
		mortarBomb.Play(burpPosition.transform.position, randomMortarDuration, Health.Team.Team2);
		AudioManager.Instance.PlayOneShot("event:/boss/frog/mortar_spawn");
		yield return new WaitForSeconds(randomMortarDuration);
		AudioManager.Instance.PlayOneShot("event:/boss/frog/mortar_explode");
	}

	private void SpawnEggs()
	{
		StartCoroutine(SpawnEggsIE());
	}

	private void SpawnEggsCircle()
	{
		StartCoroutine(SpawnEggsCircleIE(1f));
	}

	private void SpawnEggsForward()
	{
		StartCoroutine(SpawnEggsForward(1f, 1.3f));
	}

	private IEnumerator SpawnEggsIE()
	{
		int eggCount = (int)UnityEngine.Random.Range(eggsToSpawn.x, eggsToSpawn.y + 1f);
		yield return StartCoroutine(HopIE(0f, GetAngleToTarget()));
		for (int i = 0; i < eggCount; i++)
		{
			Spine.AnimationState.SetAnimation(0, eggSpawnAnimation, false);
			yield return new WaitForSeconds(eggSpawnDelay);
			SpawnEgg(base.transform.position, 0.75f, UnityEngine.Random.Range(0, 360));
			yield return new WaitForSeconds(0.25f);
			yield return StartCoroutine(HopIE(0f, GetAngleToTarget()));
		}
		yield return new WaitForSeconds(1f);
	}

	private IEnumerator SpawnEggsCircleIE(float spawnMultiplier)
	{
		anticipating = true;
		anticipationDuration = eggsMultipleAnticipation;
		yield return new WaitForSeconds(anticipationDuration);
		yield return new WaitForEndOfFrame();
		attacking = true;
		Spine.AnimationState.SetAnimation(0, eggSpawnAnimation, false);
		int eggCount = (int)(UnityEngine.Random.Range(eggsMultipleToSpawn.x, eggsMultipleToSpawn.y + 1f) * spawnMultiplier);
		float aimingAngle = UnityEngine.Random.Range(0f, 360f);
		for (int i = 0; i < eggCount; i++)
		{
			SpawnEgg(base.transform.position, UnityEngine.Random.Range(eggsKnockback.x, eggsKnockback.y), aimingAngle);
			aimingAngle += (float)(360 / eggCount);
			yield return new WaitForSeconds(UnityEngine.Random.Range(eggsMultipleSpawnDelay.x, eggsMultipleSpawnDelay.y));
		}
		yield return new WaitForSeconds(1f);
		attacking = false;
	}

	private IEnumerator SpawnEggsForward(float spawnMultiplier, float knockMultiplier)
	{
		anticipating = true;
		anticipationDuration = eggsMultipleAnticipation;
		yield return new WaitForSeconds(anticipationDuration);
		yield return new WaitForEndOfFrame();
		attacking = true;
		Spine.AnimationState.SetAnimation(0, eggSpawnAnimation, false);
		int eggCount = (int)(UnityEngine.Random.Range(eggsMultipleToSpawn.x, eggsMultipleToSpawn.y + 1f) * spawnMultiplier);
		float aimingAngle = UnityEngine.Random.Range(-1f, 1f);
		for (int i = 0; i < eggCount; i++)
		{
			SpawnEgg(base.transform.position, UnityEngine.Random.Range(eggsKnockback.x, eggsKnockback.y) * knockMultiplier, aimingAngle + 80f);
			aimingAngle = Mathf.Repeat(aimingAngle + 1f / (float)eggCount, 2f) - 1f;
			yield return new WaitForSeconds(UnityEngine.Random.Range(eggsMultipleSpawnDelay.x, eggsMultipleSpawnDelay.y));
		}
		yield return new WaitForSeconds(1f);
		attacking = false;
	}

	private void SpawnEgg(Vector3 position, float knockback = 0f, float angle = 0f)
	{
		UnitObject component = UnityEngine.Object.Instantiate(eggPrefab, position, Quaternion.identity, (base.transform.parent != null) ? base.transform.parent : null).GetComponent<UnitObject>();
		if (knockback != 0f)
		{
			component.DoKnockBack(angle, knockback, 1f);
		}
	}

	private void Hop()
	{
		StartCoroutine(HopIE(hopAnticipation, GetAngleToTarget()));
	}

	private void HopToBack()
	{
		StartCoroutine(HopToPositionIE(sitPosition));
	}

	private void HopAOE()
	{
		StartCoroutine(HopIE(hopAnticipation, GetAngleToTarget()));
	}

	private void HopUp()
	{
		StartCoroutine(HopUpIE(hopAnticipation));
	}

	private void HopDown()
	{
		StartCoroutine(HopDownIE());
	}

	private IEnumerator HopIE(float anticipation, float angle)
	{
		anticipating = true;
		anticipationDuration = anticipation;
		facePlayer = false;
		targetAngle = angle;
		LookAt(angle);
		Spine.AnimationState.SetAnimation(0, hopAnticipationAnimation, false);
		playerBlocker.SetActive(false);
		float time2 = 0f;
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
		Spine.AnimationState.SetAnimation(0, hopAnimation, false);
		AudioManager.Instance.PlayOneShot("event:/boss/frog/jump");
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.1f))
			{
				break;
			}
			yield return null;
		}
		hpBar.Hide();
		Physics2D.IgnoreCollision(collider, PlayerFarming.Instance.circleCollider2D, true);
		float hopStartTime = GameManager.GetInstance().CurrentTime;
		float t = 0f;
		while (t < hopDuration)
		{
			speed = hopSpeedCurve.Evaluate(GameManager.GetInstance().TimeSince(hopStartTime) / hopDuration) * hopSpeed;
			Spine.transform.localPosition = -Vector3.forward * hopZCurve.Evaluate(GameManager.GetInstance().TimeSince(hopStartTime) / hopDuration) * hopZHeight;
			t += Time.deltaTime * Spine.timeScale;
			yield return null;
		}
		Spine.transform.localPosition = Vector3.zero;
		Physics2D.IgnoreCollision(collider, PlayerFarming.Instance.circleCollider2D, false);
		Spine.AnimationState.SetAnimation(0, hopEndAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/boss/frog/land");
		DoAOE();
		facePlayer = true;
		playerBlocker.SetActive(true);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (num < 1f)
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator HopToPositionIE(Vector3 position)
	{
		Spine.AnimationState.SetAnimation(0, hopAnticipationAnimation, false);
		playerBlocker.SetActive(false);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.5f))
			{
				break;
			}
			yield return null;
		}
		Physics2D.IgnoreCollision(collider, PlayerFarming.Instance.circleCollider2D, true);
		Spine.AnimationState.SetAnimation(0, hopAnimation, false);
		AudioManager.Instance.PlayOneShot("event:/boss/frog/jump");
		time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.1f))
			{
				break;
			}
			yield return null;
		}
		Vector3 fromPosition = base.transform.position;
		LookAt(Utils.GetAngle(base.transform.position, position));
		hpBar.Hide();
		float hopStartTime = GameManager.GetInstance().CurrentTime;
		float t = 0f;
		while (t < hopDuration)
		{
			float num2 = GameManager.GetInstance().TimeSince(hopStartTime) / hopDuration;
			base.transform.position = Vector3.Lerp(fromPosition, position, num2);
			Spine.transform.localPosition = -Vector3.forward * hopZCurve.Evaluate(num2) * hopZHeight;
			t += Time.deltaTime * Spine.timeScale;
			yield return null;
		}
		Spine.transform.localPosition = Vector3.zero;
		Spine.AnimationState.SetAnimation(0, hopEndAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/boss/frog/land");
		DoAOE();
		playerBlocker.SetActive(true);
		Physics2D.IgnoreCollision(collider, PlayerFarming.Instance.circleCollider2D, false);
		time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (num < 0.5f)
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator HopUpIE(float anticipation)
	{
		anticipating = true;
		anticipationDuration = anticipation;
		Spine.AnimationState.SetAnimation(0, hopAnticipationAnimation, false);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < anticipationDuration))
			{
				break;
			}
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		Spine.AnimationState.SetAnimation(0, hopAnimation, false);
		playerBlocker.SetActive(false);
		AudioManager.Instance.PlayOneShot("event:/boss/frog/jump");
		GameManager.GetInstance().RemoveFromCamera(cameraTarget);
		Physics2D.IgnoreCollision(collider, PlayerFarming.Instance.circleCollider2D, true);
		health.invincible = true;
		shadow.SetActive(false);
		hpBar.Hide();
		float t = 0f;
		while (t < hopDuration)
		{
			Spine.transform.localPosition += -(Vector3.forward * 5f) * hopSpeed;
			t += Time.deltaTime * Spine.timeScale;
			yield return null;
		}
	}

	private IEnumerator HopDownIE()
	{
		Physics2D.IgnoreCollision(collider, PlayerFarming.Instance.circleCollider2D, false);
		health.invincible = false;
		shadow.SetActive(true);
		float t = 0f;
		while (t < hopDuration)
		{
			Spine.transform.localPosition -= -Vector3.forward * hopSpeed * 2f;
			t += Time.deltaTime * Spine.timeScale;
			yield return null;
		}
		GameManager.GetInstance().AddToCamera(cameraTarget);
		Spine.transform.localPosition = Vector3.zero;
		Spine.AnimationState.SetAnimation(0, hopEndAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/boss/frog/land");
		DoAOE();
		playerBlocker.SetActive(true);
	}

	private void DoAOE()
	{
		if (damageColliderRoutine != null)
		{
			StopCoroutine(damageColliderRoutine);
		}
		damageColliderRoutine = TurnOnDamageColliderForDuration(damageColliderEvents.gameObject, aoeDuration);
		StartCoroutine(damageColliderRoutine);
		if (aoeParticles != null)
		{
			aoeParticles.Play();
		}
		float num = 3f;
		Collider2D[] array = Physics2D.OverlapCircleAll(collider.transform.position, num);
		for (int i = 0; i < array.Length; i++)
		{
			UnitObject component = array[i].GetComponent<UnitObject>();
			if ((bool)component && component.health.team == Health.Team.Team2 && component != this)
			{
				component.DoKnockBack(collider.gameObject, Mathf.Clamp(num - Vector3.Distance(base.transform.position, component.transform.position), 0.1f, 3f) / 2f, 0.5f);
			}
		}
		distortionObject.gameObject.SetActive(true);
		distortionObject.transform.localScale = Vector3.one;
		distortionObject.material.SetFloat("_FishEyeIntensity", 0.1f);
		distortionObject.transform.DOScale(2f, 0.5f).SetEase(Ease.Linear).OnComplete(delegate
		{
			distortionObject.gameObject.SetActive(false);
		});
		float v = 1f;
		DOTween.To(() => v, delegate(float x)
		{
			v = x;
		}, 0f, 0.5f).SetEase(Ease.OutQuint).OnUpdate(delegate
		{
			distortionObject.material.SetFloat("_FishEyeIntensity", v);
		});
		BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position + Vector3.back * 0.5f);
		CameraManager.instance.ShakeCameraForDuration(1f, 1f, 0.2f);
	}

	private void BurpProjectiles()
	{
		StartCoroutine(BurpProjectilesIE());
	}

	private void InitializeBurpingProjectiles()
	{
		baseProjectile = projectilePrefab.GetComponent<Projectile>();
		ObjectPool.CreatePool(projectilePrefab, (int)(projectilesToSpawn.y * 1.6f));
	}

	private IEnumerator BurpProjectilesIE(float spawnMultiplier = 1f)
	{
		anticipating = true;
		anticipationDuration = burpAnticipation;
		Spine.AnimationState.SetAnimation(0, burpAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < anticipationDuration))
			{
				break;
			}
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		LookAt(GetAngleToTarget());
		int amountToSpawn = (int)(UnityEngine.Random.Range(projectilesToSpawn.x, projectilesToSpawn.y) * spawnMultiplier);
		for (int i = 0; i < amountToSpawn; i++)
		{
			Projectile component = ObjectPool.Spawn(projectilePrefab, base.transform.parent).GetComponent<Projectile>();
			component.UseDelay = true;
			component.CollideOnlyTarget = true;
			component.transform.position = new Vector3(burpPosition.transform.position.x, burpPosition.transform.position.y, 0f);
			component.GetComponentInChildren<SkeletonAnimation>().transform.localPosition = new Vector3(0f, 0f, burpPosition.transform.position.z);
			component.ModifyingZ = true;
			component.Angle = UnityEngine.Random.Range(-90f, 0f);
			component.team = health.team;
			component.Speed = baseProjectile.Speed + UnityEngine.Random.Range(-0.5f, 0.5f);
			component.turningSpeed = baseProjectile.turningSpeed + UnityEngine.Random.Range(-0.1f, 0.1f);
			component.angleNoiseFrequency = baseProjectile.angleNoiseFrequency + UnityEngine.Random.Range(-0.1f, 0.1f);
			component.LifeTime = baseProjectile.LifeTime + UnityEngine.Random.Range(0f, 0.3f);
			component.Owner = health;
			component.InvincibleTime = 1f;
			component.SetTarget(PlayerFarming.Health);
			yield return new WaitForSeconds(UnityEngine.Random.Range(projectileDelayBetweenSpawn.x, projectileDelayBetweenSpawn.y));
		}
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (num < 1f)
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	private void BounceAOE()
	{
		StartCoroutine(BounceAOEIE());
	}

	private IEnumerator BounceAOEIE()
	{
		anticipating = true;
		anticipationDuration = bounceAnticipation;
		yield return new WaitForSeconds(anticipationDuration);
		yield return new WaitForEndOfFrame();
		LookAt(GetAngleToTarget());
		int bouncesCount = (int)UnityEngine.Random.Range(bounces.x, bounces.y);
		for (int i = 0; i < bouncesCount; i++)
		{
			attacking = true;
			Spine.AnimationState.SetAnimation(0, bounceAnimation, false);
			yield return new WaitForSeconds(0.4f);
			DoAOE();
			attacking = false;
			yield return new WaitForSeconds(UnityEngine.Random.Range(timeBetweenBounce.x, timeBetweenBounce.y));
		}
	}

	private void TongueRapidAttack()
	{
		StartCoroutine(TongueRapidAttackIE());
	}

	private void TongueScatterAttack()
	{
		StartCoroutine(TongueScatterAttackIE());
	}

	private IEnumerator TongueRapidAttackIE()
	{
		usingTongue = true;
		int tongueAttackAmount = (int)UnityEngine.Random.Range(tongueWhipAmount.x, tongueWhipAmount.y);
		float time2;
		for (int i = 0; i < tongueAttackAmount; i++)
		{
			anticipating = true;
			anticipationDuration = tongueWhipAnticipation + tongueSpitDelay;
			Spine.AnimationState.SetAnimation(0, tongueAttackAnimation, false);
			time2 = 0f;
			while (true)
			{
				float num;
				time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num < anticipationDuration))
				{
					break;
				}
				yield return null;
			}
			yield return new WaitForEndOfFrame();
			facePlayer = false;
			attacking = true;
			AudioManager.Instance.PlayOneShot("event:/boss/frog/tongue_attack");
			Vector3 position = PlayerFarming.Instance.transform.position;
			StartCoroutine(GetTongue().SpitTongueIE(position, tongueSpitDelay, tongueWhipDuration, tongueRetrieveDelay, tongueRetrieveDuration));
			time2 = 0f;
			while (true)
			{
				float num;
				time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num < tongueWhipDuration + tongueRetrieveDelay + 0.5f))
				{
					break;
				}
				yield return null;
			}
			AudioManager.Instance.PlayOneShot("event:/boss/frog/tongue_return");
			Spine.AnimationState.SetAnimation(0, tongueEndAnimation, false);
			attacking = false;
		}
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
		facePlayer = true;
		usingTongue = false;
	}

	private IEnumerator TongueScatterAttackIE()
	{
		usingTongue = true;
		anticipating = true;
		anticipationDuration = tongueWhipAnticipation + tongueSpitDelay;
		attacking = true;
		facePlayer = false;
		int amount = (int)UnityEngine.Random.Range(tongueScatterAmount.x, tongueScatterAmount.y);
		int randomTargetPlayerNumber = UnityEngine.Random.Range(0, amount);
		float delay = 0f;
		AudioManager.Instance.PlayOneShot("event:/boss/frog/tongue_attack");
		float time;
		for (int i = 0; i < amount; i++)
		{
			Vector3 targetPosition = UnityEngine.Random.insideUnitCircle * tongueScatterRadius;
			if (i == randomTargetPlayerNumber)
			{
				targetPosition = PlayerFarming.Instance.transform.position;
			}
			StartCoroutine(GetTongue().SpitTongueIE(targetPosition, tongueScatterWhipDuration, tongueWhipDuration, tongueRetrieveDelay - delay + 0.5f, tongueRetrieveDuration));
			if (i == amount - 1)
			{
				continue;
			}
			float d = UnityEngine.Random.Range(tongueScatterDelay.x, tongueScatterDelay.y);
			delay += d;
			time = 0f;
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * Spine.timeScale);
				if (!(num < d))
				{
					break;
				}
				yield return null;
			}
		}
		time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < tongueScatterPreAnticipation))
			{
				break;
			}
			yield return null;
		}
		Spine.AnimationState.SetAnimation(0, tongueAttackAnimation, false);
		time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < tongueWhipDuration + tongueRetrieveDelay + 0.4f))
			{
				break;
			}
			yield return null;
		}
		time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < tongueScatterPostAnticipation))
			{
				break;
			}
			yield return null;
		}
		Spine.AnimationState.SetAnimation(0, tongueEndAnimation, false);
		time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < 1f))
			{
				break;
			}
			yield return null;
		}
		attacking = false;
		facePlayer = true;
		usingTongue = false;
	}

	private FrogBossTongue GetTongue()
	{
		foreach (FrogBossTongue tongue in tongues)
		{
			if (!tongue.gameObject.activeSelf)
			{
				return tongue;
			}
		}
		FrogBossTongue frogBossTongue = UnityEngine.Object.Instantiate(tonguePrefab, tonguePosition.transform.position, Quaternion.identity, base.transform);
		frogBossTongue.transform.localPosition = Vector3.zero;
		tongues.Add(frogBossTongue);
		return frogBossTongue;
	}

	private void SpawnMiniBosses()
	{
		StartCoroutine(SpawnMiniBossesIE());
	}

	private IEnumerator SpawnMiniBossesIE()
	{
		facePlayer = false;
		Spine.AnimationState.SetAnimation(0, burpAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < miniBossSpawningDelay))
			{
				break;
			}
			yield return null;
		}
		for (int i = 0; i < miniBossFrogsSpawnAmount; i++)
		{
			AssetReferenceGameObject key = miniBossFrogTarget;
			if (i > 1)
			{
				key = miniBossOther;
			}
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(key, miniBossSpawnPositions[i], Quaternion.identity, base.transform.parent.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				UnitObject component = obj.Result.GetComponent<UnitObject>();
				component.gameObject.SetActive(false);
				EnemySpawner.CreateWithAndInitInstantiatedEnemy(component.transform.position, base.transform.parent, component.gameObject);
				ShowHPBar showHPBar = component.GetComponent<ShowHPBar>();
				if (showHPBar == null)
				{
					showHPBar = component.gameObject.AddComponent<ShowHPBar>();
				}
				showHPBar.zOffset = 2f;
				component.GetComponent<Health>().OnDie += MiniBossKilled;
				spawnedEnemies.Add(component);
			};
		}
		facePlayer = true;
	}

	private void MiniBossKilled(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		miniBossesKilled++;
		if (miniBossesKilled >= miniBossFrogsSpawnAmount)
		{
			StartCoroutine(AllMiniBossesKilled());
		}
	}

	private IEnumerator AllMiniBossesKilled()
	{
		yield return StartCoroutine(HopDownIE());
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < 1.5f))
			{
				break;
			}
			yield return null;
		}
		StartCoroutine(Phase3IE(false));
	}

	private void AnimationEvent(TrackEntry trackEntry, global::Spine.Event e)
	{
		if (e.Data.Name == "mortar")
		{
			StartCoroutine(ShootMortarTarget());
		}
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		RoomLockController.RoomCompleted(true, false);
		PlayerFarming.Instance.health.invincible = true;
		PlayerFarming.Instance.playerWeapon.DoSlowMo(false);
		Spine.transform.localPosition = Vector3.zero;
		AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("KILL_BOSS_2"));
		Spine.transform.localPosition = Vector3.zero;
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, 0f);
		damageColliderEvents.gameObject.SetActive(false);
		for (int num = spawnedEnemies.Count - 1; num >= 0; num--)
		{
			if (spawnedEnemies[num] != null)
			{
				spawnedEnemies[num].health.enabled = true;
				spawnedEnemies[num].health.DealDamage(spawnedEnemies[num].health.totalHP, base.gameObject, base.transform.position, false, Health.AttackTypes.Heavy);
			}
		}
		foreach (FrogBossTongue tongue in tongues)
		{
			tongue.gameObject.SetActive(false);
		}
		GameManager.GetInstance().CamFollowTarget.MinZoom = 11f;
		GameManager.GetInstance().CamFollowTarget.MaxZoom = 13f;
		AudioManager.Instance.PlayOneShot("event:/boss/frog/death");
		isDead = true;
		StopAllCoroutines();
		StartCoroutine(Die());
	}

	public IEnumerator EnragedIE()
	{
		yield return new WaitForEndOfFrame();
		facePlayer = false;
		Spine.AnimationState.SetAnimation(0, enragedAnimation, false);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < enragedDuration))
			{
				break;
			}
			yield return null;
		}
		CameraManager.instance.ShakeCameraForDuration(3.5f, 4f, 1f);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 1.5f))
			{
				break;
			}
			yield return null;
		}
		facePlayer = true;
	}

	private float GetRandomAngle()
	{
		float num = UnityEngine.Random.Range(0, 360);
		float num2 = 32f;
		for (int i = 0; (float)i < num2; i++)
		{
			Vector3 vector = new Vector2(Mathf.Cos(num * ((float)Math.PI / 180f)), Mathf.Sin(num * ((float)Math.PI / 180f))).normalized;
			if (!Physics2D.Raycast(base.transform.position - vector, vector, physicsCollider.radius * 3f, layerToCheck))
			{
				break;
			}
			num = Mathf.Repeat(num + 360f / (num2 + 1f), 360f);
		}
		return num;
	}

	private float GetAngleToTarget()
	{
		if (!(PlayerFarming.Instance != null))
		{
			return 0f;
		}
		return Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
	}

	private void LookAt(float angle)
	{
		state.LookAngle = angle;
		state.facingAngle = angle;
	}

	private void OnTriggerEnterEvent(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && component.team != health.team)
		{
			component.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.7f));
		}
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		if (state.CURRENT_STATE == StateMachine.State.Moving && collider.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
		{
			Debug.Log("I have hopped into an obstacle", base.gameObject);
			hasCollidedWithObstacle = true;
			targetAngle += ((targetAngle > 180f) ? (-180f) : 180f);
			LookAt(targetAngle);
		}
	}

	private IEnumerator TurnOnDamageColliderForDuration(GameObject collider, float duration)
	{
		collider.SetActive(true);
		yield return new WaitForSeconds(duration);
		collider.SetActive(false);
	}

	private void SpawnEnemy()
	{
		if (enemiesAlive >= maxEnemies)
		{
			return;
		}
		enemiesAlive++;
		AssetReferenceGameObject key = enemyHopperTarget;
		if (juicedForm && UnityEngine.Random.value < 0.2f)
		{
			key = enemyRare[UnityEngine.Random.Range(0, enemyRare.Length)];
		}
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(key, spawnPositions[UnityEngine.Random.Range(0, spawnPositions.Length)], Quaternion.identity, base.transform.parent);
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
		{
			UnitObject component = obj.Result.GetComponent<UnitObject>();
			component.GetComponent<EnemyHopper>().alwaysTargetPlayer = true;
			component.GetComponent<Collider2D>().isTrigger = true;
			component.GetComponent<Health>().OnDie += Enemy_OnDie;
			spawnedEnemies.Add(component);
			DropLootOnDeath component2 = component.GetComponent<DropLootOnDeath>();
			if ((bool)component2)
			{
				component2.GiveXP = false;
			}
		};
		spawnTimestamp = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(spawnDelay.x, spawnDelay.y);
	}

	private void Enemy_OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		enemiesAlive--;
	}

	private IEnumerator Die()
	{
		ClearPaths();
		speed = 0f;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(cameraTarget, 12f);
		anticipating = false;
		playerBlocker.SetActive(false);
		rb.velocity = Vector3.zero;
		rb.isKinematic = true;
		rb.simulated = false;
		rb.bodyType = RigidbodyType2D.Static;
		if (base.transform.position.x > 11f)
		{
			base.transform.position = new Vector3(11f, base.transform.position.y, 0f);
		}
		if (base.transform.position.x < -11f)
		{
			base.transform.position = new Vector3(-11f, base.transform.position.y, 0f);
		}
		if (base.transform.position.y > 7f)
		{
			base.transform.position = new Vector3(base.transform.position.x, 7f, 0f);
		}
		if (base.transform.position.y < -7f)
		{
			base.transform.position = new Vector3(base.transform.position.x, -7f, 0f);
		}
		yield return new WaitForEndOfFrame();
		simpleSpineFlash.StopAllCoroutines();
		simpleSpineFlash.SetColor(new Color(0f, 0f, 0f, 0f));
		state.CURRENT_STATE = StateMachine.State.Dieing;
		bool beatenLayer2 = DataManager.Instance.BeatenHeketLayer2;
		if (!DataManager.Instance.BossesCompleted.Contains(PlayerFarming.Location) && !DungeonSandboxManager.Active)
		{
			Spine.AnimationState.SetAnimation(0, "die", false);
			Spine.AnimationState.AddAnimation(0, "dead", true, 0f);
		}
		else
		{
			if (juicedForm && !DataManager.Instance.BeatenHeketLayer2 && !DungeonSandboxManager.Active)
			{
				Spine.AnimationState.SetAnimation(0, "die-follower", false);
				yield return new WaitForSeconds(3.33f);
				Spine.gameObject.SetActive(false);
				PlayerReturnToBase.Disabled = true;
				GameObject Follower = UnityEngine.Object.Instantiate(followerToSpawn, base.transform.position, Quaternion.identity, base.transform.parent);
				Follower.GetComponent<Interaction_FollowerSpawn>().Play("CultLeader 2", ScriptLocalization.NAMES_CultLeaders.Dungeon2, true, Thought.BecomeStarving);
				DataManager.SetFollowerSkinUnlocked("CultLeader 2");
				DataManager.Instance.BeatenHeketLayer2 = true;
				ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.NewGamePlus2);
				while (Follower != null)
				{
					yield return null;
				}
				GameManager.GetInstance().OnConversationEnd();
				Interaction_Chest instance = Interaction_Chest.Instance;
				if ((object)instance != null)
				{
					instance.RevealBossReward(InventoryItem.ITEM_TYPE.GOD_TEAR);
				}
				yield break;
			}
			Spine.AnimationState.SetAnimation(0, "die-noheart", false);
			Spine.AnimationState.AddAnimation(0, "dead-noheart", true, 0f);
			if (!DungeonSandboxManager.Active)
			{
				RoomLockController.RoomCompleted();
			}
		}
		yield return new WaitForSeconds(3.2f);
		GameManager.GetInstance().OnConversationEnd();
		if (!DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_2))
		{
			interaction_MonsterHeart.ObjectiveToComplete = Objectives.CustomQuestTypes.BishopsOfTheOldFaith2;
		}
		interaction_MonsterHeart.Play((!beatenLayer2 && GameManager.Layer2) ? InventoryItem.ITEM_TYPE.GOD_TEAR : InventoryItem.ITEM_TYPE.NONE);
	}

	private void OnDrawGizmosSelected()
	{
		Vector3[] array = spawnPositions;
		for (int i = 0; i < array.Length; i++)
		{
			Utils.DrawCircleXY(array[i], 0.5f, Color.blue);
		}
		array = miniBossSpawnPositions;
		for (int i = 0; i < array.Length; i++)
		{
			Utils.DrawCircleXY(array[i], 0.5f, Color.green);
		}
	}
}
