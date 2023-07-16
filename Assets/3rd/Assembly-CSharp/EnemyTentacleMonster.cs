using System;
using System.Collections;
using System.Collections.Generic;
using CotL.Projectiles;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using FMOD.Studio;
using I2.Loc;
using Spine.Unity;
using Unify;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyTentacleMonster : UnitObject
{
	public SkeletonAnimation spine;

	public SimpleSpineFlash simpleSpineFlash;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string idleAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string shootProjectilesAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string holyHandGrenadeAnticipationAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string holyHandGrenadeAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string sweepingSpawnAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string randomSpawnAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string teleportAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string daggerAttackAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string swordAnticipationAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string swordLoopAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string swordAttackAnimation;

	[Space]
	[SerializeField]
	private CircleCollider2D physicsCollider;

	[SerializeField]
	private float teleportMinDistanceToPlayer;

	[SerializeField]
	private float lungeSpeed;

	[SerializeField]
	private float daggerOverrideMinRadius;

	[SerializeField]
	private float daggerOverrideMaxRadius;

	[SerializeField]
	private float daggerOverrideIntervals;

	[SerializeField]
	private ColliderEvents daggerCollider;

	[Space]
	[SerializeField]
	private float swordAttackWithinRadius;

	[SerializeField]
	private float swordChaseSpeed;

	[SerializeField]
	private float swordMaxChaseDuration;

	[SerializeField]
	private float swordRetargetPlayerInterval;

	[SerializeField]
	private ColliderEvents swordCollider;

	[SerializeField]
	private float projectilePatternCircleAnticipation;

	[SerializeField]
	private float projectilePatternCircleDuration;

	[SerializeField]
	private ProjectilePattern projectilePatternCircle;

	[Space]
	[SerializeField]
	private float projectilePatternSnakeAnticipation;

	[SerializeField]
	private float projectilePatternSnakeDuration;

	[SerializeField]
	private ProjectilePatternBeam projectilePatternSnake;

	[Space]
	[SerializeField]
	private float projectilePatternScatterAnticipation;

	[SerializeField]
	private float projectilePatternScatterDuration;

	[SerializeField]
	private ProjectilePattern projectilePatternScatter;

	[Space]
	[SerializeField]
	private float projectilePatternRingsAnticipation;

	[SerializeField]
	private float projectilePatternRingsDuration;

	[SerializeField]
	private float projectilePatternRingsSpeed;

	[SerializeField]
	private float projectilePatternRingsAcceleration;

	[SerializeField]
	private float projectilePatternRingsRadius;

	[SerializeField]
	private ProjectileCircleBase projectilePatternRings;

	[Space]
	[SerializeField]
	private float projectilePatternRandomAnticipation;

	[SerializeField]
	private float projectilePatternRandomDuration;

	[SerializeField]
	private ProjectilePattern projectilePatternRandom;

	[Space]
	[SerializeField]
	private float projectilePatternBeamAnticipation;

	[SerializeField]
	private float projectilePatternBeamDuration;

	[SerializeField]
	private ProjectilePatternBeam projectilePatternBeam;

	[SerializeField]
	private float hhgAnticipation;

	[SerializeField]
	private float hhgMinDistance;

	[SerializeField]
	private Vector2 hhgSpawnAmount;

	[SerializeField]
	private Vector2 hhgSpawnOffset;

	[SerializeField]
	private Vector2 hhgSpawnDelay;

	[SerializeField]
	private AssetReferenceGameObject[] hhgEnemiesList;

	[SerializeField]
	private float ssAnticipation;

	[SerializeField]
	private float ssDistanceBetween;

	[SerializeField]
	private float ssDelayBetween;

	[SerializeField]
	private float ssForce;

	[SerializeField]
	private AnimationCurve ssArc;

	[SerializeField]
	private Vector2 ssSpawnAmount;

	[SerializeField]
	private AssetReferenceGameObject[] ssEnemiesList;

	[SerializeField]
	private float rsAnticipation;

	[SerializeField]
	private Vector2 rsSpawnAmount;

	[SerializeField]
	private AssetReferenceGameObject[] rsEnemiesList;

	[SerializeField]
	private float enragedDuration = 2f;

	[SerializeField]
	[Range(0f, 1f)]
	private float p2HealthThreshold = 0.6f;

	[Space]
	[SerializeField]
	private Interaction_MonsterHeart interaction_MonsterHeart;

	[SerializeField]
	private GameObject blockingCollider;

	[SerializeField]
	private GameObject followerToSpawn;

	private List<GameObject> spawnedEnemies = new List<GameObject>();

	private Coroutine currentAttackRoutine;

	private EventInstance grenadeLoopingSoundInstance;

	private bool facePlayer = true;

	private bool isDead;

	private bool queuePhaseIncrement;

	private bool activated;

	private bool attacking;

	private bool m = true;

	private int currentPhaseNumber;

	private float startingHealth;

	private float repathTimestamp;

	private bool recentlyTeleported;

	private bool juicedForm;

	private bool moving
	{
		get
		{
			return m;
		}
		set
		{
			m = value;
			if (!m)
			{
				ClearPaths();
			}
		}
	}

	public override void Awake()
	{
		base.Awake();
		daggerCollider.OnTriggerEnterEvent += CombatAttack_OnTriggerEnterEvent;
		swordCollider.OnTriggerEnterEvent += CombatAttack_OnTriggerEnterEvent;
		juicedForm = GameManager.Layer2;
		if (juicedForm)
		{
			health.totalHP *= 1.5f;
			health.HP = health.totalHP;
			rsSpawnAmount *= 1.25f;
			for (int i = 0; i < projectilePatternCircle.Waves.Length; i++)
			{
				projectilePatternCircle.Waves[i].Speed *= 1.5f;
				projectilePatternCircle.Waves[i].FinishDelay /= 1.5f;
			}
			for (int j = 0; j < projectilePatternSnake.BulletWaves.Length; j++)
			{
				projectilePatternSnake.BulletWaves[j].Speed *= 1.5f;
				projectilePatternSnake.BulletWaves[j].DelayBetweenBullets /= 1.5f;
			}
			for (int k = 0; k < projectilePatternScatter.Waves.Length; k++)
			{
				projectilePatternScatter.Waves[k].Speed *= 1.5f;
				projectilePatternScatter.Waves[k].FinishDelay /= 1.5f;
			}
			for (int l = 0; l < projectilePatternRandom.Waves.Length; l++)
			{
				projectilePatternRandom.Waves[l].Speed *= 1.5f;
				projectilePatternRandom.Waves[l].FinishDelay /= 1.5f;
			}
			for (int m = 0; m < projectilePatternBeam.BulletWaves.Length; m++)
			{
				projectilePatternBeam.BulletWaves[m].Speed *= 1.5f;
				projectilePatternBeam.BulletWaves[m].DelayBetweenBullets /= 1.5f;
			}
			projectilePatternRingsSpeed *= 1.5f;
		}
		InitializeProjectilePatternRings();
	}

	private void Start()
	{
		startingHealth = health.HP;
		health.SlowMoOnkill = false;
		ProjectilePatternBase.OnProjectileSpawned += OnProjectileSpawned;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		AudioManager.Instance.StopLoop(grenadeLoopingSoundInstance);
		ProjectilePatternBase.OnProjectileSpawned -= OnProjectileSpawned;
	}

	public override void Update()
	{
		base.Update();
		if (PlayerFarming.Instance == null)
		{
			return;
		}
		if (queuePhaseIncrement && currentAttackRoutine == null)
		{
			queuePhaseIncrement = false;
			IncrementPhase();
		}
		if (facePlayer && activated && !isDead)
		{
			LookAt(GetAngleToTarget());
		}
		if (activated && !isDead && moving)
		{
			GameManager instance = GameManager.GetInstance();
			if ((((object)instance != null) ? new float?(instance.CurrentTime) : null) > repathTimestamp)
			{
				if (attacking)
				{
					if (Vector3.Distance(PlayerFarming.Instance.transform.position, base.transform.position) < 2f)
					{
						givePath(PlayerFarming.Instance.transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * 2f));
					}
					else
					{
						givePath(PlayerFarming.Instance.transform.position + Vector3.up);
					}
				}
				else
				{
					givePath(GetPositionAwayFromPlayer());
				}
				repathTimestamp = GameManager.GetInstance().CurrentTime + swordRetargetPlayerInterval;
			}
		}
		if (!isDead)
		{
			if (activated && currentPhaseNumber == 1)
			{
				UpdatePhase1();
			}
			else if (activated && currentPhaseNumber >= 2)
			{
				UpdatePhase2();
			}
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (activated)
		{
			StartCoroutine(DelayAddCamera());
		}
	}

	private IEnumerator DelayAddCamera()
	{
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().AddToCamera(base.gameObject);
		GameManager.GetInstance().CamFollowTarget.MinZoom = 9f;
		GameManager.GetInstance().CamFollowTarget.MaxZoom = 18f;
	}

	private bool CanSpawnEnemies()
	{
		return spawnedEnemies.Count < 15;
	}

	public void BeginPhase1()
	{
		GameManager.GetInstance().AddToCamera(base.gameObject);
		GameManager.GetInstance().CamFollowTarget.MinZoom = 9f;
		GameManager.GetInstance().CamFollowTarget.MaxZoom = 18f;
		StartCoroutine(DelayCallback(1f, delegate
		{
			activated = true;
			currentPhaseNumber = 1;
			currentAttackRoutine = StartCoroutine(RandomSpawnIE());
		}));
	}

	private IEnumerator DelayCallback(float delay, System.Action callback)
	{
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * spine.timeScale);
			if (!(num < delay))
			{
				break;
			}
			yield return null;
		}
		if (callback != null)
		{
			callback();
		}
	}

	private void UpdatePhase1()
	{
		float num = Vector3.Distance(PlayerFarming.Instance.transform.position, base.transform.position);
		if (currentAttackRoutine != null)
		{
			return;
		}
		if (UnityEngine.Random.Range(0, 3) == 0 && !recentlyTeleported)
		{
			recentlyTeleported = true;
			if (num < daggerOverrideMinRadius)
			{
				currentAttackRoutine = StartCoroutine(DaggerAttackIE());
				return;
			}
			if (num < daggerOverrideMinRadius * 2f)
			{
				currentAttackRoutine = StartCoroutine(TeleportAwayFromPlayerIE(0.25f));
				return;
			}
		}
		recentlyTeleported = false;
		int num2 = UnityEngine.Random.Range(0, 5);
		if (num2 < 2 && CanSpawnEnemies())
		{
			if (UnityEngine.Random.Range(0, 2) == 0 && spawnedEnemies.Count < 3)
			{
				currentAttackRoutine = StartCoroutine(RandomSpawnIE());
			}
			else
			{
				currentAttackRoutine = StartCoroutine(SweepingSpawnIE());
			}
			return;
		}
		if (num2 == 2 && num > 4f)
		{
			currentAttackRoutine = StartCoroutine(SwordAttackIE());
			return;
		}
		switch (UnityEngine.Random.Range(0, 4))
		{
		case 0:
			currentAttackRoutine = StartCoroutine(ShootProjectileBeamIE());
			break;
		case 1:
			currentAttackRoutine = StartCoroutine(ShootProjectileRandomIE());
			break;
		default:
			currentAttackRoutine = StartCoroutine(ShootProjectileRingsIE());
			break;
		}
	}

	public void BeginPhase2()
	{
		currentAttackRoutine = StartCoroutine(Phase2IE());
	}

	private IEnumerator Phase2IE()
	{
		yield return StartCoroutine(EnragedIE());
		maxSpeed *= 2f;
		yield return StartCoroutine(ShootProjectileCircleIE());
		currentAttackRoutine = null;
	}

	private void UpdatePhase2()
	{
		float num = Vector3.Distance(PlayerFarming.Instance.transform.position, base.transform.position);
		if (currentAttackRoutine != null)
		{
			return;
		}
		if (UnityEngine.Random.Range(0, 3) == 0 && !recentlyTeleported)
		{
			recentlyTeleported = true;
			if (num < daggerOverrideMinRadius)
			{
				currentAttackRoutine = StartCoroutine(DaggerAttackIE());
				return;
			}
			if (num < daggerOverrideMinRadius * 2f)
			{
				currentAttackRoutine = StartCoroutine(TeleportAwayFromPlayerIE(0.25f));
				return;
			}
		}
		recentlyTeleported = false;
		int num2 = UnityEngine.Random.Range(0, 5);
		if (num2 < 2 && CanSpawnEnemies())
		{
			num2 = UnityEngine.Random.Range(0, 3);
			if (num2 == 0 && spawnedEnemies.Count < 5)
			{
				currentAttackRoutine = StartCoroutine(RandomSpawnIE());
			}
			else if (num2 == 1)
			{
				currentAttackRoutine = StartCoroutine(HolyHandGrenadeIE());
			}
			else
			{
				currentAttackRoutine = StartCoroutine(SweepingSpawnIE());
			}
			return;
		}
		if (num2 == 2)
		{
			if (num > 4f)
			{
				currentAttackRoutine = StartCoroutine(SwordAttackIE());
			}
			else
			{
				currentAttackRoutine = StartCoroutine(DaggerRapidAttackIE());
			}
			return;
		}
		switch (UnityEngine.Random.Range(0, 3))
		{
		case 0:
			currentAttackRoutine = StartCoroutine(ShootProjectileCircleIE());
			break;
		case 1:
			currentAttackRoutine = StartCoroutine(ShootProjectileRandomIE());
			break;
		default:
			currentAttackRoutine = StartCoroutine(ShootProjectileRingsTripleIE());
			break;
		}
	}

	private void IncrementPhase()
	{
		if (currentAttackRoutine != null)
		{
			queuePhaseIncrement = true;
			return;
		}
		if (currentAttackRoutine != null)
		{
			StopCoroutine(currentAttackRoutine);
		}
		currentPhaseNumber++;
		if (currentPhaseNumber == 2)
		{
			BeginPhase2();
		}
	}

	public void HolyHandGrenade()
	{
		StartCoroutine(HolyHandGrenadeIE());
	}

	private IEnumerator HolyHandGrenadeIE()
	{
		spine.AnimationState.SetAnimation(0, holyHandGrenadeAnticipationAnimation, false);
		spine.AnimationState.AddAnimation(0, holyHandGrenadeAnimation, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/grenade_start", base.gameObject);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < hhgAnticipation))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.StopLoop(grenadeLoopingSoundInstance);
		grenadeLoopingSoundInstance = AudioManager.Instance.CreateLoop("event:/boss/jellyfish/grenade_loop", base.gameObject, true);
		int spawnAmount = (int)UnityEngine.Random.Range(hhgSpawnAmount.x, hhgSpawnAmount.y + 1f);
		for (int i = 0; i < spawnAmount; i++)
		{
			Vector3 position = GetPosition(PlayerFarming.Instance.transform.position, PlayerFarming.Instance.state.LookAngle + UnityEngine.Random.Range(hhgSpawnOffset.x, hhgSpawnOffset.y), hhgMinDistance);
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(hhgEnemiesList[UnityEngine.Random.Range(0, hhgEnemiesList.Length)], position, Quaternion.identity, base.transform.parent.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				GameObject spawnedEnemy = obj.Result;
				UnitObject component = spawnedEnemy.GetComponent<UnitObject>();
				component.CanHaveModifier = false;
				spawnedEnemies.Add(spawnedEnemy);
				component.gameObject.SetActive(false);
				EnemySpawner.CreateWithAndInitInstantiatedEnemy(component.transform.position, base.transform.parent, component.gameObject);
				component.GetComponent<Health>().OnDie += delegate
				{
					spawnedEnemies.Remove(spawnedEnemy);
				};
			};
			float dur = UnityEngine.Random.Range(hhgSpawnDelay.x, hhgSpawnDelay.y);
			time2 = 0f;
			while (true)
			{
				float num;
				time2 = (num = time2 + Time.deltaTime * spine.timeScale);
				if (!(num < dur))
				{
					break;
				}
				yield return null;
			}
		}
		spine.AnimationState.SetAnimation(0, idleAnimation, true);
		AudioManager.Instance.StopLoop(grenadeLoopingSoundInstance);
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/grenade_end", base.gameObject);
		float dura = UnityEngine.Random.Range(0.5f, 1.25f);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < dura))
			{
				break;
			}
			yield return null;
		}
		currentAttackRoutine = null;
	}

	public void SweepingSpawn()
	{
		StartCoroutine(SweepingSpawnIE());
	}

	private IEnumerator SweepingSpawnIE()
	{
		spine.AnimationState.SetAnimation(0, sweepingSpawnAnimation, false);
		spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/staff", base.gameObject);
		float time3 = 0f;
		while (true)
		{
			float num;
			time3 = (num = time3 + Time.deltaTime * spine.timeScale);
			if (!(num < ssAnticipation))
			{
				break;
			}
			yield return null;
		}
		int spawnAmount = (int)UnityEngine.Random.Range(ssSpawnAmount.x, ssSpawnAmount.y + 1f);
		Mathf.RoundToInt(spawnAmount / 2);
		float angle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position) * ((float)Math.PI / 180f);
		for (int i = 0; i < spawnAmount; i++)
		{
			float norm = (float)i / (float)spawnAmount;
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(ssEnemiesList[UnityEngine.Random.Range(0, ssEnemiesList.Length)], base.transform.position, Quaternion.identity, base.transform.parent.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				GameObject spawnedEnemy = obj.Result;
				UnitObject component = spawnedEnemy.GetComponent<UnitObject>();
				component.CanHaveModifier = false;
				spawnedEnemies.Add(spawnedEnemy);
				component.GetComponent<Health>().OnDie += delegate
				{
					spawnedEnemies.Remove(spawnedEnemy);
				};
				DropLootOnDeath component2 = component.GetComponent<DropLootOnDeath>();
				if ((bool)component2)
				{
					component2.GiveXP = false;
				}
				component.GetComponent<UnitObject>().DoKnockBack(angle, ssForce * ssArc.Evaluate(norm), 1f);
				if (component is EnemyJellyCharger)
				{
					((EnemyJellyCharger)component).AllowMultipleChargers = true;
					((EnemyJellyCharger)component).VisionRange = int.MaxValue;
				}
			};
			angle += ssDistanceBetween;
			if (ssDelayBetween == 0f)
			{
				continue;
			}
			time3 = 0f;
			while (true)
			{
				float num;
				time3 = (num = time3 + Time.deltaTime * spine.timeScale);
				if (!(num < ssDelayBetween))
				{
					break;
				}
				yield return null;
			}
		}
		float dur = UnityEngine.Random.Range(0.5f, 1.25f);
		time3 = 0f;
		while (true)
		{
			float num;
			time3 = (num = time3 + Time.deltaTime * spine.timeScale);
			if (!(num < dur))
			{
				break;
			}
			yield return null;
		}
		currentAttackRoutine = null;
	}

	public void RandomSpawn()
	{
		StartCoroutine(RandomSpawnIE());
	}

	private IEnumerator RandomSpawnIE()
	{
		moving = false;
		spine.AnimationState.SetAnimation(0, randomSpawnAnimation, false);
		spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/staff", base.gameObject);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * spine.timeScale);
			if (!(num < rsAnticipation))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/staff_magic", base.gameObject);
		int num2 = (int)UnityEngine.Random.Range(rsSpawnAmount.x, rsSpawnAmount.y + 1f);
		for (int i = 0; i < num2; i++)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(rsEnemiesList[UnityEngine.Random.Range(0, rsEnemiesList.Length)], GetRandomPosition(Vector3.zero, 0f, 3f, 8f), Quaternion.identity, base.transform.parent.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				GameObject spawnedEnemy = obj.Result;
				UnitObject component = spawnedEnemy.GetComponent<UnitObject>();
				component.CanHaveModifier = false;
				spawnedEnemies.Add(spawnedEnemy);
				component.gameObject.SetActive(false);
				EnemySpawner.CreateWithAndInitInstantiatedEnemy(component.transform.position, base.transform.parent.parent, component.gameObject);
				DropLootOnDeath component2 = component.GetComponent<DropLootOnDeath>();
				if ((bool)component2)
				{
					component2.GiveXP = false;
				}
				component.GetComponent<Health>().OnDie += delegate
				{
					spawnedEnemies.Remove(spawnedEnemy);
				};
			};
		}
		float dur = UnityEngine.Random.Range(0.5f, 1.25f);
		time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * spine.timeScale);
			if (!(num < dur))
			{
				break;
			}
			yield return null;
		}
		moving = true;
		currentAttackRoutine = null;
	}

	private void OnProjectileSpawned()
	{
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/grenade_spawn", base.gameObject);
	}

	private void ShootProjectileCircle()
	{
		StartCoroutine(ShootProjectileCircleIE());
	}

	private IEnumerator ShootProjectileCircleIE()
	{
		if (Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) < 4f)
		{
			yield return StartCoroutine(TeleportAwayFromPlayerIE(1f, false));
		}
		moving = false;
		spine.AnimationState.SetAnimation(0, shootProjectilesAnimation, true);
		AudioManager.Instance.StopLoop(grenadeLoopingSoundInstance);
		grenadeLoopingSoundInstance = AudioManager.Instance.CreateLoop("event:/boss/jellyfish/grenade_loop", base.gameObject, true);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < projectilePatternCircleAnticipation))
			{
				break;
			}
			yield return null;
		}
		projectilePatternCircle.Shoot();
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < projectilePatternCircleDuration))
			{
				break;
			}
			yield return null;
		}
		spine.AnimationState.SetAnimation(0, idleAnimation, true);
		AudioManager.Instance.StopLoop(grenadeLoopingSoundInstance);
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/grenade_end", base.gameObject);
		float dur = UnityEngine.Random.Range(0.5f, 1.25f);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < dur))
			{
				break;
			}
			yield return null;
		}
		moving = true;
		currentAttackRoutine = null;
	}

	private void ShootProjectileSnake()
	{
		StartCoroutine(ShootProjectileSnakeIE());
	}

	private IEnumerator ShootProjectileSnakeIE()
	{
		if (Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) < 4f)
		{
			yield return StartCoroutine(TeleportAwayFromPlayerIE(1f, false));
		}
		spine.AnimationState.SetAnimation(0, shootProjectilesAnimation, true);
		AudioManager.Instance.StopLoop(grenadeLoopingSoundInstance);
		grenadeLoopingSoundInstance = AudioManager.Instance.CreateLoop("event:/boss/jellyfish/grenade_loop", base.gameObject, true);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < projectilePatternSnakeAnticipation))
			{
				break;
			}
			yield return null;
		}
		projectilePatternSnake.Shoot();
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < projectilePatternSnakeDuration))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.StopLoop(grenadeLoopingSoundInstance);
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/grenade_end", base.gameObject);
		spine.AnimationState.SetAnimation(0, idleAnimation, true);
		currentAttackRoutine = null;
	}

	private void ShootProjectileRandom()
	{
		StartCoroutine(ShootProjectileRandomIE());
	}

	private IEnumerator ShootProjectileRandomIE()
	{
		if (Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) < 4f)
		{
			yield return StartCoroutine(TeleportAwayFromPlayerIE(1f, false));
		}
		spine.AnimationState.SetAnimation(0, shootProjectilesAnimation, true);
		AudioManager.Instance.StopLoop(grenadeLoopingSoundInstance);
		grenadeLoopingSoundInstance = AudioManager.Instance.CreateLoop("event:/boss/jellyfish/grenade_loop", base.gameObject, true);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < projectilePatternRandomAnticipation))
			{
				break;
			}
			yield return null;
		}
		projectilePatternRandom.Shoot();
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < projectilePatternRandomDuration))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.StopLoop(grenadeLoopingSoundInstance);
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/grenade_end", base.gameObject);
		spine.AnimationState.SetAnimation(0, idleAnimation, true);
		float dur = UnityEngine.Random.Range(0.5f, 1.25f);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < dur))
			{
				break;
			}
			yield return null;
		}
		currentAttackRoutine = null;
	}

	private void ShootProjectileScatter()
	{
		StartCoroutine(ShootProjectileScatterIE());
	}

	private IEnumerator ShootProjectileScatterIE()
	{
		if (Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) < 4f)
		{
			yield return StartCoroutine(TeleportAwayFromPlayerIE(1f, false));
		}
		spine.AnimationState.SetAnimation(0, shootProjectilesAnimation, true);
		AudioManager.Instance.StopLoop(grenadeLoopingSoundInstance);
		grenadeLoopingSoundInstance = AudioManager.Instance.CreateLoop("event:/boss/jellyfish/grenade_loop", base.gameObject, true);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < projectilePatternScatterAnticipation))
			{
				break;
			}
			yield return null;
		}
		projectilePatternScatter.Shoot();
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < projectilePatternScatterDuration))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.StopLoop(grenadeLoopingSoundInstance);
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/grenade_end", base.gameObject);
		spine.AnimationState.SetAnimation(0, idleAnimation, true);
		currentAttackRoutine = null;
	}

	private void ShootProjectileBeam()
	{
		StartCoroutine(ShootProjectileBeamIE());
	}

	private IEnumerator ShootProjectileBeamIE()
	{
		if (Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) < 4f)
		{
			yield return StartCoroutine(TeleportAwayFromPlayerIE(1f, false));
		}
		spine.AnimationState.SetAnimation(0, shootProjectilesAnimation, true);
		AudioManager.Instance.StopLoop(grenadeLoopingSoundInstance);
		grenadeLoopingSoundInstance = AudioManager.Instance.CreateLoop("event:/boss/jellyfish/grenade_loop", base.gameObject, true);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < projectilePatternBeamAnticipation))
			{
				break;
			}
			yield return null;
		}
		projectilePatternBeam.Shoot();
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < projectilePatternBeamDuration))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.StopLoop(grenadeLoopingSoundInstance);
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/grenade_end", base.gameObject);
		spine.AnimationState.SetAnimation(0, idleAnimation, true);
		float dur = UnityEngine.Random.Range(0.5f, 1.25f);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < dur))
			{
				break;
			}
			yield return null;
		}
		currentAttackRoutine = null;
	}

	private void ShootProjectileRings()
	{
		StartCoroutine(ShootProjectileRingsIE());
	}

	private void InitializeProjectilePatternRings()
	{
		if (projectilePatternRings is ProjectileCirclePattern)
		{
			ProjectileCirclePattern projectileCirclePattern = (ProjectileCirclePattern)projectilePatternRings;
			if (projectileCirclePattern.ProjectilePrefab != null)
			{
				ObjectPool.CreatePool(projectileCirclePattern.ProjectilePrefab, projectileCirclePattern.BaseProjectilesCount * 3);
			}
		}
		ObjectPool.CreatePool(projectilePatternRings, 3);
	}

	private IEnumerator ShootProjectileRingsIE()
	{
		if (Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) < 4f)
		{
			yield return StartCoroutine(TeleportAwayFromPlayerIE(1f, false));
		}
		moving = false;
		spine.AnimationState.SetAnimation(0, shootProjectilesAnimation, true);
		AudioManager.Instance.StopLoop(grenadeLoopingSoundInstance);
		grenadeLoopingSoundInstance = AudioManager.Instance.CreateLoop("event:/boss/jellyfish/grenade_loop", base.gameObject, true);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < projectilePatternRingsAnticipation))
			{
				break;
			}
			yield return null;
		}
		Projectile component = ObjectPool.Spawn(projectilePatternRings, base.transform.parent).GetComponent<Projectile>();
		component.transform.position = base.transform.position + (Vector3)(Utils.DegreeToVector2(GetAngleToPlayer()) * projectilePatternRingsRadius * 2f);
		component.Angle = GetAngleToTarget();
		component.health = health;
		component.team = Health.Team.Team2;
		component.Speed = projectilePatternRingsSpeed;
		component.Acceleration = projectilePatternRingsAcceleration;
		component.GetComponent<ProjectileCircleBase>().InitDelayed(PlayerFarming.Instance.gameObject, projectilePatternRingsRadius, 0f, delegate
		{
			AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/grenade_mass_launch", base.gameObject);
		});
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < projectilePatternRingsDuration))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.StopLoop(grenadeLoopingSoundInstance);
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/grenade_end", base.gameObject);
		spine.AnimationState.SetAnimation(0, idleAnimation, true);
		float dur = UnityEngine.Random.Range(0.5f, 1.25f);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < dur))
			{
				break;
			}
			yield return null;
		}
		moving = true;
		currentAttackRoutine = null;
	}

	public void ShootProjectileRingsTriple()
	{
		StartCoroutine(ShootProjectileRingsTripleIE());
	}

	private IEnumerator ShootProjectileRingsTripleIE()
	{
		if (Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) < 4f)
		{
			yield return StartCoroutine(TeleportAwayFromPlayerIE(1f, false));
		}
		moving = false;
		spine.AnimationState.SetAnimation(0, shootProjectilesAnimation, true);
		AudioManager.Instance.StopLoop(grenadeLoopingSoundInstance);
		grenadeLoopingSoundInstance = AudioManager.Instance.CreateLoop("event:/boss/jellyfish/grenade_loop", base.gameObject, true);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * spine.timeScale);
			if (!(num < projectilePatternRingsAnticipation))
			{
				break;
			}
			yield return null;
		}
		List<float> list = new List<float> { 0f, 0.5f, 1f };
		for (int i = 0; i < 3; i++)
		{
			float t = (float)i / 2f;
			float angleToPlayer = GetAngleToPlayer();
			Vector3 vector = Utils.DegreeToVector2(angleToPlayer);
			Vector3 vector2 = Vector3.Lerp(Utils.DegreeToVector2(angleToPlayer - 90f), Utils.DegreeToVector2(angleToPlayer + 90f), t) * 1.25f;
			Projectile component = ObjectPool.Spawn(projectilePatternRings, base.transform.parent).GetComponent<Projectile>();
			component.transform.position = base.transform.position + (vector + vector2) * projectilePatternRingsRadius * 2f;
			component.Angle = angleToPlayer;
			component.health = health;
			component.team = Health.Team.Team2;
			component.Speed = projectilePatternRingsSpeed;
			component.Acceleration = projectilePatternRingsAcceleration;
			float num2 = list[UnityEngine.Random.Range(0, list.Count)];
			list.Remove(num2);
			component.GetComponent<ProjectileCircleBase>().InitDelayed(PlayerFarming.Instance.gameObject, projectilePatternRingsRadius, num2, delegate
			{
				AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/grenade_mass_launch", base.gameObject);
			});
		}
		time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * spine.timeScale);
			if (!(num < projectilePatternRingsDuration))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.StopLoop(grenadeLoopingSoundInstance);
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/grenade_end", base.gameObject);
		spine.AnimationState.SetAnimation(0, idleAnimation, true);
		float dur = UnityEngine.Random.Range(0.5f, 1.25f);
		time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * spine.timeScale);
			if (!(num < dur))
			{
				break;
			}
			yield return null;
		}
		moving = true;
		currentAttackRoutine = null;
	}

	public void DaggerAttack()
	{
		StartCoroutine(DaggerAttackIE());
	}

	private IEnumerator DaggerAttackIE(bool finishAttack = true)
	{
		attacking = true;
		spine.AnimationState.SetAnimation(0, daggerAttackAnimation, false);
		spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/dagger_attack", base.gameObject);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < 0.4f))
			{
				break;
			}
			yield return null;
		}
		float ogMaxSpeed = maxSpeed;
		maxSpeed = lungeSpeed;
		speed = lungeSpeed;
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < 0.2f))
			{
				break;
			}
			yield return null;
		}
		maxSpeed = ogMaxSpeed;
		speed = maxSpeed / 2f;
		StartCoroutine(EnableCollider(daggerCollider.gameObject, 0.25f));
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < 1.5f))
			{
				break;
			}
			yield return null;
		}
		if (finishAttack)
		{
			currentAttackRoutine = null;
		}
		attacking = false;
	}

	private IEnumerator DaggerRapidAttackIE()
	{
		for (int i = 0; i < UnityEngine.Random.Range(2, 5); i++)
		{
			yield return StartCoroutine(DaggerAttackIE(false));
		}
		float dur = UnityEngine.Random.Range(0.5f, 1f);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * spine.timeScale);
			if (!(num < dur))
			{
				break;
			}
			yield return null;
		}
		currentAttackRoutine = null;
	}

	public void SwordAttack()
	{
		StartCoroutine(SwordAttackIE());
	}

	private IEnumerator SwordAttackIE()
	{
		attacking = true;
		spine.AnimationState.SetAnimation(0, swordAnticipationAnimation, false);
		spine.AnimationState.AddAnimation(0, swordLoopAnimation, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/sword_charge", base.gameObject);
		float timeStamp = GameManager.GetInstance().CurrentTime + swordMaxChaseDuration;
		repathTimestamp = 0f;
		float ogMaxSpeed = maxSpeed;
		DisableForces = false;
		TweenerCore<float, float, FloatOptions> tween = DOTween.To(() => maxSpeed, delegate(float x)
		{
			maxSpeed = x;
		}, swordChaseSpeed, 2f);
		float t = 0f;
		while ((Vector3.Distance(PlayerFarming.Instance.transform.position, base.transform.position) > swordAttackWithinRadius && GameManager.GetInstance().CurrentTime < timeStamp) || t < 1f)
		{
			t += Time.deltaTime * spine.timeScale;
			yield return null;
		}
		spine.AnimationState.SetAnimation(0, swordAttackAnimation, false);
		spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/sword_attack", base.gameObject);
		speed = lungeSpeed;
		float time3 = 0f;
		while (true)
		{
			float num;
			time3 = (num = time3 + Time.deltaTime * spine.timeScale);
			if (!(num < 0.1f))
			{
				break;
			}
			yield return null;
		}
		StartCoroutine(EnableCollider(swordCollider.gameObject, 0.15f));
		time3 = 0f;
		while (true)
		{
			float num;
			time3 = (num = time3 + Time.deltaTime * spine.timeScale);
			if (!(num < 0.1f))
			{
				break;
			}
			yield return null;
		}
		tween.Kill();
		maxSpeed = ogMaxSpeed;
		speed = maxSpeed / 2f;
		time3 = 0f;
		while (true)
		{
			float num;
			time3 = (num = time3 + Time.deltaTime * spine.timeScale);
			if (!(num < 1f))
			{
				break;
			}
			yield return null;
		}
		attacking = false;
		currentAttackRoutine = null;
	}

	private IEnumerator EnableCollider(GameObject collider, float duration)
	{
		collider.SetActive(true);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * spine.timeScale);
			if (!(num < duration))
			{
				break;
			}
			yield return null;
		}
		collider.SetActive(false);
	}

	private void CombatAttack_OnTriggerEnterEvent(Collider2D collider)
	{
		if (collider.tag == "Player")
		{
			PlayerFarming.Instance.health.DealDamage(1f, base.gameObject, base.transform.position);
		}
	}

	public void TeleportAwayFromPlayer()
	{
		StartCoroutine(TeleportAwayFromPlayerIE(0.5f));
	}

	public void TeleportNearPlayer()
	{
		StartCoroutine(TeleportNearPlayerIE(0f));
	}

	public void TeleportToPostion(Vector3 position)
	{
		StartCoroutine(TeleportToPositionIE(position));
	}

	private IEnumerator TeleportAwayFromPlayerIE(float endDelay = 1f, bool nullifyRoutine = true)
	{
		moving = false;
		yield return StartCoroutine(TeleportToPositionIE(GetPositionAwayFromPlayer(), endDelay));
		if (nullifyRoutine)
		{
			moving = true;
			currentAttackRoutine = null;
		}
	}

	private IEnumerator TeleportNearPlayerIE(float endDelay = 1f)
	{
		moving = false;
		yield return StartCoroutine(TeleportOutIE());
		Vector3 position = PlayerFarming.Instance.transform.position;
		float num = UnityEngine.Random.Range(0, 360);
		int num2 = 0;
		while (num2++ < 32)
		{
			if ((bool)Physics2D.Raycast(position, Utils.DegreeToVector2(num), teleportMinDistanceToPlayer, layerToCheck))
			{
				num = Mathf.Repeat(num + 11.25f, 360f);
				continue;
			}
			position += (Vector3)Utils.DegreeToVector2(num) * teleportMinDistanceToPlayer;
			break;
		}
		base.transform.position = position;
		yield return StartCoroutine(TeleportInIE());
		float time = 0f;
		while (true)
		{
			float num3;
			time = (num3 = time + Time.deltaTime * spine.timeScale);
			if (!(num3 < endDelay))
			{
				break;
			}
			yield return null;
		}
		moving = true;
	}

	private IEnumerator TeleportToPositionIE(Vector3 position, float endDelay = 0f)
	{
		yield return StartCoroutine(TeleportOutIE());
		base.transform.position = position;
		yield return StartCoroutine(TeleportInIE());
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * spine.timeScale);
			if (num < endDelay)
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator TeleportOutIE()
	{
		spine.AnimationState.SetAnimation(0, teleportAnimation, false);
		spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/teleport_away", base.gameObject);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * spine.timeScale);
			if (!(num < 0.5f))
			{
				break;
			}
			yield return null;
		}
		physicsCollider.enabled = false;
	}

	private IEnumerator TeleportInIE()
	{
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/teleport_return", base.gameObject);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * spine.timeScale);
			if (!(num < 0.5f))
			{
				break;
			}
			yield return null;
		}
		physicsCollider.enabled = true;
	}

	private IEnumerator EnragedIE()
	{
		spine.AnimationState.SetAnimation(0, "roar", false);
		spine.AnimationState.AddAnimation(0, "animation", true, 0f);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < 0.7f))
			{
				break;
			}
			yield return null;
		}
		CameraManager.instance.ShakeCameraForDuration(1f, 1.5f, 1.3f);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (num < 2.4f)
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		simpleSpineFlash.FlashFillRed(0.25f);
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/gethit");
		Vector3 position = (AttackLocation + Attacker.transform.position) / 2f;
		BiomeConstants.Instance.EmitHitVFX(position, Quaternion.identity.z, "HitFX_Weak");
		float num = health.HP / startingHealth;
		if (currentPhaseNumber == 1 && num <= p2HealthThreshold)
		{
			IncrementPhase();
		}
	}

	private void LookAt(float angle)
	{
		state.LookAngle = angle;
		state.facingAngle = angle;
	}

	private float GetAngleToTarget()
	{
		float num = GetAngleToPlayer();
		if (physicsCollider == null)
		{
			return num;
		}
		float num2 = 32f;
		for (int i = 0; (float)i < num2; i++)
		{
			if (!Physics2D.CircleCast(base.transform.position, physicsCollider.radius, new Vector2(Mathf.Cos(num * ((float)Math.PI / 180f)), Mathf.Sin(num * ((float)Math.PI / 180f))), 2.5f, layerToCheck))
			{
				break;
			}
			num += 360f / (num2 + 1f);
		}
		return num;
	}

	private float GetAngleToPlayer()
	{
		if (!(PlayerFarming.Instance != null))
		{
			return 0f;
		}
		return Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
	}

	private Vector3 GetPositionAwayFromPlayer()
	{
		float x = PlayerFarming.Instance.transform.position.x;
		float y = PlayerFarming.Instance.transform.position.y;
		while (Vector3.Distance(new Vector3(x, y), PlayerFarming.Instance.transform.position) < 4f)
		{
			x = UnityEngine.Random.Range(-10f, 10f);
			y = UnityEngine.Random.Range(-5.5f, 5.5f);
		}
		return new Vector3(x, y, 0f);
	}

	private Vector3 GetRandomPosition(Vector3 startingPosition, float radius, float minDist, float maxDist)
	{
		float num = UnityEngine.Random.Range(0, 360);
		Vector3 vector = startingPosition;
		int num2 = 0;
		while (num2++ < 32)
		{
			float num3 = UnityEngine.Random.Range(minDist, maxDist);
			if ((bool)Physics2D.Raycast(vector, Utils.DegreeToVector2(num), num3 - radius, layerToCheck))
			{
				num = Mathf.Repeat(num + (float)UnityEngine.Random.Range(0, 360), 360f);
				continue;
			}
			vector += (Vector3)Utils.DegreeToVector2(num) * (num3 - radius);
			break;
		}
		return vector;
	}

	private Vector3 GetPosition(Vector3 startingPosition, float angle, float distance)
	{
		float num = angle;
		Vector3 vector = startingPosition;
		int num2 = 0;
		while (num2++ < 32)
		{
			if ((bool)Physics2D.Raycast(vector, Utils.DegreeToVector2(num), distance, layerToCheck))
			{
				num = Mathf.Repeat(num + (float)UnityEngine.Random.Range(0, 360), 360f);
				continue;
			}
			vector += (Vector3)Utils.DegreeToVector2(num) * distance;
			break;
		}
		return vector;
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		AudioManager.Instance.StopLoop(grenadeLoopingSoundInstance);
		RoomLockController.RoomCompleted(true, false);
		PlayerFarming.Instance.health.invincible = true;
		PlayerFarming.Instance.playerWeapon.DoSlowMo(false);
		AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("KILL_BOSS_3"));
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, 0f);
		daggerCollider.gameObject.SetActive(false);
		swordCollider.gameObject.SetActive(false);
		for (int num = spawnedEnemies.Count - 1; num >= 0; num--)
		{
			if (spawnedEnemies[num] != null)
			{
				Health component = spawnedEnemies[num].GetComponent<Health>();
				if (component != null)
				{
					component.enabled = true;
					component.DealDamage(component.totalHP, base.gameObject, base.transform.position, false, Health.AttackTypes.Heavy);
				}
			}
		}
		for (int num2 = Health.team2.Count - 1; num2 >= 0; num2--)
		{
			if (Health.team2[num2] != null && Health.team2[num2] != health)
			{
				Health.team2[num2].enabled = true;
				Health.team2[num2].invincible = false;
				Health.team2[num2].untouchable = false;
				Health.team2[num2].DealDamage(Health.team2[num2].totalHP, base.gameObject, base.transform.position, false, Health.AttackTypes.Heavy);
			}
		}
		GameManager.GetInstance().CamFollowTarget.MinZoom = 11f;
		GameManager.GetInstance().CamFollowTarget.MaxZoom = 13f;
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/death");
		isDead = true;
		moving = false;
		StopAllCoroutines();
		StartCoroutine(Die());
	}

	private IEnumerator Die()
	{
		ClearPaths();
		speed = 0f;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 12f);
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/death", base.gameObject);
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
		bool beatenLayer2 = DataManager.Instance.BeatenKallamarLayer2;
		float time2;
		if (!DataManager.Instance.BossesCompleted.Contains(PlayerFarming.Location) && !DungeonSandboxManager.Active)
		{
			spine.AnimationState.SetAnimation(0, "die", false);
			spine.AnimationState.AddAnimation(0, "dead", true, 0f);
		}
		else
		{
			if (juicedForm && !DataManager.Instance.BeatenKallamarLayer2 && !DungeonSandboxManager.Active)
			{
				spine.AnimationState.SetAnimation(0, "die-follower", false);
				time2 = 0f;
				while (true)
				{
					float num;
					time2 = (num = time2 + Time.deltaTime * spine.timeScale);
					if (!(num < 3.83f))
					{
						break;
					}
					yield return null;
				}
				spine.gameObject.SetActive(false);
				PlayerReturnToBase.Disabled = true;
				GameObject Follower = UnityEngine.Object.Instantiate(followerToSpawn, base.transform.position, Quaternion.identity, base.transform.parent);
				Follower.GetComponent<Interaction_FollowerSpawn>().Play("CultLeader 3", ScriptLocalization.NAMES_CultLeaders.Dungeon3, true, Thought.Ill);
				DataManager.SetFollowerSkinUnlocked("CultLeader 3");
				DataManager.Instance.BeatenKallamarLayer2 = true;
				ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.NewGamePlus3);
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
			spine.AnimationState.SetAnimation(0, "die-noheart", false);
			spine.AnimationState.AddAnimation(0, "dead-noheart", true, 0f);
			if (!DungeonSandboxManager.Active)
			{
				RoomLockController.RoomCompleted();
			}
		}
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * spine.timeScale);
			if (!(num < 4.5f))
			{
				break;
			}
			yield return null;
		}
		GameManager.GetInstance().OnConversationEnd();
		blockingCollider.SetActive(false);
		rb.isKinematic = true;
		if (!DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_3))
		{
			interaction_MonsterHeart.ObjectiveToComplete = Objectives.CustomQuestTypes.BishopsOfTheOldFaith3;
		}
		interaction_MonsterHeart.Play((!beatenLayer2 && GameManager.Layer2) ? InventoryItem.ITEM_TYPE.GOD_TEAR : InventoryItem.ITEM_TYPE.NONE);
		SimulationManager.UnPause();
	}

	public override void OnDisable()
	{
		base.OnDisable();
		AudioManager.Instance.StopLoop(grenadeLoopingSoundInstance);
		currentAttackRoutine = null;
		moving = true;
	}
}
