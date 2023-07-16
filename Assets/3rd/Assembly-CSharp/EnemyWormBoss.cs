using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using I2.Loc;
using Spine;
using Spine.Unity;
using Unify;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyWormBoss : UnitObject
{
	public SkeletonAnimation Spine;

	[SerializeField]
	private SimpleSpineFlash simpleSpineFlash;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string idleAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string hitAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string jumpAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string diveAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string popInAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string popOutAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string spikeAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string shootAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string summonAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string headSmashAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string enragedAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string dieAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string deadAnimation;

	[Space]
	[SerializeField]
	private GameObject cameraTarget;

	[SerializeField]
	private Interaction_MonsterHeart interaction_MonsterHeart;

	[SerializeField]
	private ColliderEvents damageCollider;

	[SerializeField]
	private int spikeAmount = 4;

	[SerializeField]
	private GameObject spikePrefab;

	[SerializeField]
	private float spikeAnticipation;

	[SerializeField]
	private float directionalDelayBetweenSpikes = 0.2f;

	[SerializeField]
	private float directionalDistanceBetweenSpikes = 0.2f;

	[SerializeField]
	private int circleSpikeAmount = 4;

	[SerializeField]
	private float circleDelayBetweenSpikes = 0.2f;

	[SerializeField]
	private float circleDistanceBetweenSpikes = 0.2f;

	[SerializeField]
	private float inAirDuration = 2f;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("0.75 means will stop targeting the player in the last 25% of in air time")]
	private float targetPercentage = 0.7f;

	[SerializeField]
	private float popOutDelay = 2f;

	[SerializeField]
	private float targetLerpSpeed = 10f;

	[SerializeField]
	private GameObject warningObject;

	[SerializeField]
	private GameObject groundImpactVFX;

	[SerializeField]
	private GameObject bulletPrefab;

	[SerializeField]
	private float shootAnticipation = 1f;

	[SerializeField]
	private Vector2 delayBetweenShots = new Vector2(0.1f, 0.3f);

	[SerializeField]
	public float numberOfShotsToFire = 45f;

	[SerializeField]
	private Vector2 gravSpeed;

	[SerializeField]
	private float arc;

	[SerializeField]
	private Vector2 randomArcOffset = new Vector2(0f, 0f);

	[SerializeField]
	private Vector2 shootDistanceRange = new Vector2(2f, 3f);

	[SerializeField]
	private GameObject ShootBone;

	[SerializeField]
	private bool bulletsTargetPlayer = true;

	[SerializeField]
	private float moveSpeed = 2.5f;

	[SerializeField]
	private float delayBetweenTrailSpike = 0.2f;

	[SerializeField]
	private GameObject trailSpikePrefab;

	[SerializeField]
	private float headSmashAnticipation = 2f;

	[SerializeField]
	private float zSpacing;

	[SerializeField]
	private Vector3[] headSmashPositions = new Vector3[3];

	[SerializeField]
	private Vector2 p1SpawnAmount;

	[SerializeField]
	private float p1SpawnAnticipation;

	[SerializeField]
	private Vector2 p1DelayBetweenSpawns;

	[SerializeField]
	private AssetReferenceGameObject[] p1SpawnablesList;

	[SerializeField]
	[Range(0f, 1f)]
	private float p2HealthThreshold = 0.6f;

	[SerializeField]
	[Range(0f, 1f)]
	private float p3HealthThreshold = 0.3f;

	[SerializeField]
	private float p3MoveSpeed = 5f;

	[Space]
	[SerializeField]
	private Vector2 p3SpawnAmount;

	[SerializeField]
	private float p3SpawnAnticipation;

	[SerializeField]
	private float enragedDuration = 6f;

	[SerializeField]
	private GameObject[] deathParticlePrefabs;

	[SerializeField]
	private GameObject followerToSpawn;

	private bool targetingPlayer;

	private bool anticipating;

	private bool phaseChangeBlocked;

	private float anticipationTimer;

	private float anticipationDuration;

	private float spawnRadius = 7f;

	private float trailTimer;

	private int currentPhaseNumber = 1;

	private float startingHealth;

	private float ogSpacing;

	private bool queuePhaseIncrement;

	private bool active;

	private bool isDead;

	private List<GameObject> trailSpikes = new List<GameObject>();

	private List<GameObject> spawnedSpikes = new List<GameObject>();

	private List<UnitObject> spawnedEnemies = new List<UnitObject>();

	private int startingTrailSpikes = 9;

	private int startingSpawnedSpikes = 90;

	private Coroutine currentPhaseRoutine;

	private Coroutine currentAttackRoutine;

	private int previousAttackIndex;

	private bool juicedForm;

	private EventInstance loopingSoundInstance;

	private List<AsyncOperationHandle<GameObject>> loadedAddressableAssets = new List<AsyncOperationHandle<GameObject>>();

	public GameObject CameraTarget
	{
		get
		{
			return cameraTarget;
		}
	}

	private void Start()
	{
		damageCollider.gameObject.SetActive(false);
		damageCollider.OnTriggerEnterEvent += OnDamageTriggerEnter;
		Spine.AnimationState.Event += HandleEvent;
		startingHealth = health.HP;
		ogSpacing = Spine.zSpacing;
		if (DataManager.Instance.playerDeathsInARowFightingLeader >= 2)
		{
			p1SpawnAmount -= Vector2.one;
			p3SpawnAmount -= Vector2.one * 2f;
		}
		juicedForm = GameManager.Layer2;
		if (juicedForm)
		{
			moveSpeed *= 1.5f;
			circleDelayBetweenSpikes *= 0.75f;
			directionalDelayBetweenSpikes *= 0.75f;
			p1SpawnAmount *= 1.5f;
			p3SpawnAmount *= 1.5f;
			numberOfShotsToFire *= 1.33f;
			health.totalHP *= 1.5f;
			health.HP = health.totalHP;
		}
		health.SlowMoOnkill = false;
		InitializeGranadeBullets();
		InitializeTrailSpikes();
		StartCoroutine(InitializeSpawnedSpikesIE());
	}

	private void HandleEvent(TrackEntry trackEntry, global::Spine.Event e)
	{
		string text = e.Data.Name;
		switch (text)
		{
		default:
		{
			bool flag = text == "spikeAttack";
			break;
		}
		case "diveDown":
			AudioManager.Instance.PlayOneShot("event:/boss/worm/dive_down", AudioManager.Instance.Listener);
			break;
		case "diveUp":
			AudioManager.Instance.PlayOneShot("event:/boss/worm/dive_up", AudioManager.Instance.Listener);
			break;
		case "pushThroughGround":
			AudioManager.Instance.PlayOneShot("event:/boss/worm/push_through_ground", AudioManager.Instance.Listener);
			break;
		case "spawnMiniboss":
			break;
		}
	}

	public override void Update()
	{
		base.Update();
		if (anticipating)
		{
			anticipationTimer += Time.deltaTime * Spine.timeScale;
			if (anticipationTimer / anticipationDuration > 1f)
			{
				anticipating = false;
				anticipationTimer = 0f;
			}
		}
		if (queuePhaseIncrement)
		{
			queuePhaseIncrement = false;
			IncrementPhase();
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		AudioManager.Instance.PlayOneShot("event:/boss/worm/get_hit", AudioManager.Instance.Listener);
		Vector3 position = (AttackLocation + Attacker.transform.position) / 2f;
		BiomeConstants.Instance.EmitHitVFX(position, Quaternion.identity.z, "HitFX_Weak");
		simpleSpineFlash.FlashFillRed(0.25f);
		float num = health.HP / startingHealth;
		if ((currentPhaseNumber == 1 && num <= p2HealthThreshold) || (currentPhaseNumber == 2 && num <= p3HealthThreshold))
		{
			IncrementPhase();
		}
	}

	private void IncrementPhase()
	{
		if (phaseChangeBlocked)
		{
			queuePhaseIncrement = true;
			return;
		}
		StopCoroutine(currentAttackRoutine);
		StopCoroutine(currentPhaseRoutine);
		currentPhaseNumber++;
		foreach (GameObject spawnedSpike in spawnedSpikes)
		{
			spawnedSpike.SetActive(false);
		}
		anticipating = false;
		health.invincible = false;
		if (currentPhaseNumber == 2)
		{
			BeginPhase2();
		}
		else if (currentPhaseNumber == 3)
		{
			BeginPhase3();
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		AudioManager.Instance.StopLoop(loopingSoundInstance);
		Spine.AnimationState.Event -= HandleEvent;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		AudioManager.Instance.StopLoop(loopingSoundInstance);
		Spine.AnimationState.Event -= HandleEvent;
		if (loadedAddressableAssets == null)
		{
			return;
		}
		foreach (AsyncOperationHandle<GameObject> loadedAddressableAsset in loadedAddressableAssets)
		{
			Addressables.Release((AsyncOperationHandle)loadedAddressableAsset);
		}
		loadedAddressableAssets.Clear();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (active)
		{
			if (currentAttackRoutine != null)
			{
				StopCoroutine(currentAttackRoutine);
			}
			if (currentPhaseRoutine != null)
			{
				StopCoroutine(currentPhaseRoutine);
			}
			warningObject.SetActive(false);
			StartCoroutine(DelayAddCamera());
			if (currentPhaseNumber == 1)
			{
				currentPhaseRoutine = StartCoroutine(Phase1IE(false));
			}
			else if (currentPhaseNumber == 2)
			{
				currentPhaseRoutine = StartCoroutine(Phase2IE(false));
			}
			else if (currentPhaseNumber == 3)
			{
				currentPhaseRoutine = StartCoroutine(Phase3IE(false));
			}
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
		GameManager.GetInstance().AddToCamera(cameraTarget);
		GameManager.GetInstance().CamFollowTarget.MinZoom = 9f;
		GameManager.GetInstance().CamFollowTarget.MaxZoom = 18f;
		currentPhaseRoutine = StartCoroutine(Phase1IE(true));
	}

	private IEnumerator Phase1IE(bool firstLoop)
	{
		active = true;
		if (firstLoop)
		{
			yield return StartCoroutine(SpawnIE(p1SpawnablesList, (int)UnityEngine.Random.Range(p1SpawnAmount.x, p1SpawnAmount.y + 1f), p1SpawnAnticipation, p1DelayBetweenSpawns));
		}
		for (int i = 0; i < 3; i++)
		{
			yield return currentAttackRoutine = StartCoroutine(TunnelMoveIE(moveSpeed, GetRandomPosition(7f), true));
			if (i == 2)
			{
				if (juicedForm && UnityEngine.Random.value > 0.5f && GetClosestTarget() != null)
				{
					yield return currentAttackRoutine = StartCoroutine(ShootTargetedSpikesInDirectionIE(directionalDelayBetweenSpikes / 3f, directionalDistanceBetweenSpikes));
				}
				else
				{
					yield return currentAttackRoutine = StartCoroutine(SpawnSpikesInDirectionsIE(spikeAmount, directionalDelayBetweenSpikes, directionalDistanceBetweenSpikes));
				}
			}
			else
			{
				yield return currentAttackRoutine = StartCoroutine(ShootIE());
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
		if (firstLoop)
		{
			yield return currentAttackRoutine = StartCoroutine(TunnelMoveIE(p3MoveSpeed, GetRandomPosition(7f), false));
			StartCoroutine(EnragedIE());
			yield return new WaitForSeconds(2f);
			yield return StartCoroutine(SpawnIE(p1SpawnablesList, (int)UnityEngine.Random.Range(p1SpawnAmount.x, p1SpawnAmount.y + 1f), p1SpawnAnticipation, p1DelayBetweenSpawns, false));
			yield return new WaitForSeconds(2f);
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
				yield return currentAttackRoutine = StartCoroutine(JumpDiveIE());
				break;
			case 1:
				yield return currentAttackRoutine = StartCoroutine(TunnelMoveIE(moveSpeed, GetRandomPosition(7f), true));
				if (juicedForm && UnityEngine.Random.value > 0.5f && GetClosestTarget() != null)
				{
					yield return currentAttackRoutine = StartCoroutine(ShootTargetedSpikesInDirectionIE(directionalDelayBetweenSpikes / 3f, directionalDistanceBetweenSpikes));
				}
				else
				{
					yield return currentAttackRoutine = StartCoroutine(SpawnSpikesInDirectionsIE(circleSpikeAmount, circleDelayBetweenSpikes, circleDistanceBetweenSpikes));
				}
				break;
			case 2:
				yield return currentAttackRoutine = StartCoroutine(TunnelMoveIE(moveSpeed, GetRandomPosition(7f), true));
				yield return currentAttackRoutine = StartCoroutine(ShootIE());
				break;
			}
			yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 2f));
		}
		currentPhaseRoutine = StartCoroutine(Phase2IE(false));
	}

	private void BeginPhase3()
	{
		currentPhaseRoutine = StartCoroutine(Phase3IE(true));
	}

	private IEnumerator Phase3IE(bool firstLoop)
	{
		if (firstLoop)
		{
			yield return currentAttackRoutine = StartCoroutine(TunnelMoveIE(p3MoveSpeed, Vector3.zero, false));
			StartCoroutine(EnragedIE());
			yield return new WaitForSeconds(2f);
			yield return StartCoroutine(SpawnIE(p1SpawnablesList, (int)UnityEngine.Random.Range(p3SpawnAmount.x, p3SpawnAmount.y + 1f), p3SpawnAnticipation, p1DelayBetweenSpawns, false));
			yield return new WaitForSeconds(2f);
		}
		for (int i = 1; i < 3; i++)
		{
			if (i % 2 == 0)
			{
				if (juicedForm && UnityEngine.Random.value > 0.5f && GetClosestTarget() != null)
				{
					yield return currentAttackRoutine = StartCoroutine(ShootTargetedSpikesInDirectionIE(directionalDelayBetweenSpikes / 3f, directionalDistanceBetweenSpikes));
				}
				else
				{
					yield return currentAttackRoutine = StartCoroutine(TunnelMoveIE(p3MoveSpeed, GetRandomPosition(7f), true));
				}
			}
			else
			{
				yield return currentAttackRoutine = StartCoroutine(HeadSmashIE());
			}
		}
		if (UnityEngine.Random.Range(0, 2) == 0)
		{
			yield return currentAttackRoutine = StartCoroutine(SpawnSpikesInDirectionsIE(circleSpikeAmount, circleDelayBetweenSpikes, circleDistanceBetweenSpikes));
		}
		else
		{
			yield return currentAttackRoutine = StartCoroutine(ShootIE());
		}
		currentPhaseRoutine = StartCoroutine(Phase3IE(false));
	}

	private void JumpDive()
	{
		StartCoroutine(JumpDiveIE());
	}

	private IEnumerator JumpDiveIE()
	{
		Spine.ForceVisible = true;
		phaseChangeBlocked = true;
		yield return new WaitForEndOfFrame();
		Spine.AnimationState.SetAnimation(0, jumpAnimation, false);
		CameraManager.instance.ShakeCameraForDuration(1f, 1.2f, 0.2f);
		float time2 = 0f;
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
		health.invincible = true;
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.75f))
			{
				break;
			}
			yield return null;
		}
		warningObject.SetActive(true);
		targetingPlayer = true;
		base.transform.position = PlayerFarming.Instance.transform.position;
		float dur = inAirDuration * targetPercentage;
		float t = 0f;
		while (t < dur)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, PlayerFarming.Instance.transform.position, targetLerpSpeed * Time.deltaTime);
			t += Time.deltaTime * Spine.timeScale;
			yield return null;
		}
		warningObject.transform.localPosition = Vector3.zero;
		targetingPlayer = false;
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < inAirDuration - t))
			{
				break;
			}
			yield return null;
		}
		Spine.AnimationState.SetAnimation(0, diveAnimation, false);
		warningObject.SetActive(false);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.3f))
			{
				break;
			}
			yield return null;
		}
		damageCollider.gameObject.SetActive(true);
		CameraManager.instance.ShakeCameraForDuration(2f, 2.2f, 0.2f);
		if (juicedForm)
		{
			StartCoroutine(SpawnSpikesInDirectionsIE(circleSpikeAmount, circleDelayBetweenSpikes / 2f, circleDistanceBetweenSpikes, false));
		}
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
		if (groundImpactVFX != null)
		{
			ParticleSystem component = groundImpactVFX.GetComponent<ParticleSystem>();
			if (component != null)
			{
				component.Play();
			}
		}
		damageCollider.gameObject.SetActive(false);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < popOutDelay))
			{
				break;
			}
			yield return null;
		}
		health.invincible = false;
		Spine.AnimationState.SetAnimation(0, popInAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
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
		phaseChangeBlocked = false;
		Spine.ForceVisible = false;
	}

	private void SpawnSpikesInDirections()
	{
		StartCoroutine(SpawnSpikesInDirectionsIE(spikeAmount, directionalDelayBetweenSpikes, directionalDistanceBetweenSpikes));
	}

	private void SpawnSpikesInCircle()
	{
		StartCoroutine(SpawnSpikesInDirectionsIE(circleSpikeAmount, circleDelayBetweenSpikes, circleDistanceBetweenSpikes));
	}

	private IEnumerator InitializeSpawnedSpikesIE()
	{
		int instantiatePerFrame = 3;
		for (int i = 0; i < startingSpawnedSpikes; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(spikePrefab, base.transform.position, Quaternion.identity, base.transform.parent);
			gameObject.SetActive(false);
			spawnedSpikes.Add(gameObject);
			ColliderEvents componentInChildren = gameObject.GetComponentInChildren<ColliderEvents>();
			if ((bool)componentInChildren)
			{
				componentInChildren.OnTriggerEnterEvent += OnDamageTriggerEnter;
			}
			if (i % instantiatePerFrame == 0)
			{
				yield return null;
			}
		}
	}

	private IEnumerator SpawnSpikesInDirectionsIE(int amount, float delayBetweenSpikes, float distanceBetweenSpikes, bool anticipate = true)
	{
		phaseChangeBlocked = true;
		yield return new WaitForEndOfFrame();
		int num = UnityEngine.Random.Range(0, 360);
		for (int i = 0; i < amount; i++)
		{
			Vector3 direction = new Vector3(Mathf.Cos((float)num * ((float)Math.PI / 180f)), Mathf.Sin((float)num * ((float)Math.PI / 180f)), 0f);
			StartCoroutine(ShootSpikesInDirectionIE(direction, delayBetweenSpikes, distanceBetweenSpikes, anticipate));
			num = (int)Mathf.Repeat(num + 360 / amount, 360f);
		}
		AudioManager.Instance.PlayOneShot("event:/boss/worm/spike_attack", AudioManager.Instance.Listener);
		float time = 0f;
		while (true)
		{
			float num2;
			time = (num2 = time + Time.deltaTime * Spine.timeScale);
			if (!(num2 < spikeAnticipation + delayBetweenSpikes + 1f))
			{
				break;
			}
			yield return null;
		}
		phaseChangeBlocked = false;
	}

	public IEnumerator ShootSpikesInDirectionIE(Vector3 direction, float delayBetweenSpikes, float distanceBetweenSpikes, bool anticipate = true)
	{
		if (anticipate)
		{
			Spine.AnimationState.SetAnimation(0, spikeAnimation, false);
			Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
			anticipating = true;
			anticipationDuration = spikeAnticipation;
			float time2 = 0f;
			while (true)
			{
				float num;
				time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num < spikeAnticipation))
				{
					break;
				}
				yield return null;
			}
			CameraManager.instance.ShakeCameraForDuration(0.4f, 0.6f, 0.5f);
			yield return new WaitForEndOfFrame();
		}
		Vector3 position = base.transform.position;
		for (int i = 0; i < 30; i++)
		{
			GetSpawnSpike().transform.position = position;
			position += direction * distanceBetweenSpikes;
			if ((bool)Physics2D.Raycast(position, direction, 1f, layerToCheck))
			{
				break;
			}
			float time2 = 0f;
			while (true)
			{
				float num;
				time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num < delayBetweenSpikes))
				{
					break;
				}
				yield return null;
			}
		}
	}

	public IEnumerator ShootTargetedSpikesInDirectionIE(float delayBetweenSpikes, float distanceBetweenSpikes)
	{
		Spine.AnimationState.SetAnimation(0, spikeAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		anticipating = true;
		anticipationDuration = spikeAnticipation;
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < spikeAnticipation))
			{
				break;
			}
			yield return null;
		}
		CameraManager.instance.ShakeCameraForDuration(0.4f, 0.6f, 0.5f);
		yield return new WaitForEndOfFrame();
		Vector3 normalized = (GetClosestTarget().transform.position - base.transform.position).normalized;
		Vector3 direction = Quaternion.Euler(0f, 0f, -25f) * normalized;
		Vector3 direction2 = Quaternion.Euler(0f, 0f, 25f) * normalized;
		StartCoroutine(ShootSpikesInDirectionIE(direction, delayBetweenSpikes, distanceBetweenSpikes, false));
		StartCoroutine(ShootSpikesInDirectionIE(direction2, delayBetweenSpikes, distanceBetweenSpikes, false));
		yield return StartCoroutine(ShootSpikesInDirectionIE(normalized, delayBetweenSpikes, distanceBetweenSpikes, false));
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
			gameObject = UnityEngine.Object.Instantiate(spikePrefab, base.transform.position, Quaternion.identity, base.transform.parent);
			spawnedSpikes.Add(gameObject);
			ColliderEvents componentInChildren = gameObject.GetComponentInChildren<ColliderEvents>();
			if ((bool)componentInChildren)
			{
				componentInChildren.OnTriggerEnterEvent += OnDamageTriggerEnter;
			}
		}
		return gameObject;
	}

	private void Shoot()
	{
		StartCoroutine(ShootIE());
	}

	private void InitializeGranadeBullets()
	{
		ObjectPool.CreatePool(bulletPrefab, (int)numberOfShotsToFire);
	}

	private IEnumerator ShootIE()
	{
		AudioManager.Instance.PlayOneShot("event:/boss/worm/spit_projectiles", AudioManager.Instance.Listener);
		anticipating = true;
		anticipationDuration = shootAnticipation;
		yield return new WaitForEndOfFrame();
		Spine.AnimationState.SetAnimation(0, shootAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < shootAnticipation))
			{
				break;
			}
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		phaseChangeBlocked = true;
		float angle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
		CameraManager.instance.ShakeCameraForDuration(1f, 1.2f, 0.4f);
		int num2 = -1;
		while ((float)(++num2) < numberOfShotsToFire)
		{
			AudioManager.Instance.PlayOneShot("event:/enemy/spit_gross_projectile", AudioManager.Instance.Listener);
			StartCoroutine(BulletDelay(angle, num2, UnityEngine.Random.Range(delayBetweenShots.x, delayBetweenShots.y)));
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
		phaseChangeBlocked = false;
	}

	private IEnumerator BulletDelay(float shootAngle, int index, float delay)
	{
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < delay))
			{
				break;
			}
			yield return null;
		}
		Vector3 position = ShootBone.transform.position;
		ObjectPool.Spawn(bulletPrefab, position, Quaternion.identity).GetComponent<GrenadeBullet>().Play(-6f, shootAngle - arc / 2f + arc / numberOfShotsToFire * (float)index + UnityEngine.Random.Range(randomArcOffset.x, randomArcOffset.y), UnityEngine.Random.Range(shootDistanceRange.x, shootDistanceRange.y), UnityEngine.Random.Range(gravSpeed.x, gravSpeed.y));
	}

	private void SpawnP1()
	{
		StartCoroutine(SpawnIE(p1SpawnablesList, (int)UnityEngine.Random.Range(p1SpawnAmount.x, p1SpawnAmount.y + 1f), p1SpawnAnticipation, p1DelayBetweenSpawns));
	}

	private void SpawnP3()
	{
		StartCoroutine(SpawnIE(p1SpawnablesList, (int)UnityEngine.Random.Range(p3SpawnAmount.x, p3SpawnAmount.y + 1f), p3SpawnAnticipation, p1DelayBetweenSpawns));
	}

	private IEnumerator SpawnIE(AssetReferenceGameObject[] spawnables, int amount, float anticipationTime, Vector2 delayBetweenSpawns, bool playAnimations = true)
	{
		phaseChangeBlocked = true;
		AudioManager.Instance.PlayOneShot("event:/boss/worm/spike_attack", AudioManager.Instance.Listener);
		yield return new WaitForEndOfFrame();
		float time2;
		if (playAnimations)
		{
			Spine.AnimationState.SetAnimation(0, summonAnimation, false);
			Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
			time2 = 0f;
			while (true)
			{
				float num;
				time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num < anticipationTime))
				{
					break;
				}
				yield return null;
			}
			yield return new WaitForEndOfFrame();
			CameraManager.instance.ShakeCameraForDuration(1f, 1.5f, 0.5f);
		}
		for (int i = 0; i < amount; i++)
		{
			Vector3 spawnPosition = UnityEngine.Random.insideUnitCircle * spawnRadius;
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(spawnables[UnityEngine.Random.Range(0, spawnables.Length)]);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				loadedAddressableAssets.Add(obj);
				UnitObject component = EnemySpawner.Create(spawnPosition, base.transform.parent, obj.Result).GetComponent<UnitObject>();
				component.CanHaveModifier = false;
				component.RemoveModifier();
				spawnedEnemies.Add(component);
				DropLootOnDeath component2 = component.GetComponent<DropLootOnDeath>();
				if ((bool)component2)
				{
					component2.GiveXP = false;
				}
			};
			time2 = 0f;
			float dur = UnityEngine.Random.Range(delayBetweenSpawns.x, delayBetweenSpawns.y);
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
		phaseChangeBlocked = false;
	}

	private IEnumerator SpawnIE(UnitObject[] spawnables, int amount, Vector3[] spawnPositions, float anticipationTime)
	{
		phaseChangeBlocked = true;
		AudioManager.Instance.PlayOneShot("event:/boss/worm/spike_attack", AudioManager.Instance.Listener);
		Spine.AnimationState.SetAnimation(0, summonAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < anticipationTime))
			{
				break;
			}
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		CameraManager.instance.ShakeCameraForDuration(1f, 1.5f, 0.5f);
		for (int i = 0; i < amount; i++)
		{
			if (!isDead)
			{
				Vector3 position = UnityEngine.Random.insideUnitCircle * spawnRadius;
				UnitObject unitObject = UnityEngine.Object.Instantiate(spawnables[UnityEngine.Random.Range(0, spawnables.Length)], position, Quaternion.identity, base.transform.parent);
				unitObject.CanHaveModifier = false;
				unitObject.RemoveModifier();
				unitObject.transform.position = spawnPositions[i];
				spawnedEnemies.Add(unitObject);
				DropLootOnDeath component = unitObject.GetComponent<DropLootOnDeath>();
				if ((bool)component)
				{
					component.GiveXP = false;
				}
			}
		}
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
		phaseChangeBlocked = false;
	}

	private void TunnelMove()
	{
		currentAttackRoutine = StartCoroutine(TunnelMoveIE(moveSpeed, GetRandomPosition(7f), true));
	}

	private void InitializeTrailSpikes()
	{
		for (int i = 0; i < startingTrailSpikes; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(trailSpikePrefab, base.transform.position, Quaternion.identity, base.transform.parent);
			gameObject.SetActive(false);
			trailSpikes.Add(gameObject);
			ColliderEvents componentInChildren = gameObject.GetComponentInChildren<ColliderEvents>();
			if ((bool)componentInChildren)
			{
				componentInChildren.OnTriggerEnterEvent += OnDamageTriggerEnter;
			}
		}
	}

	private IEnumerator TunnelMoveIE(float moveSpeed, Vector3 position, bool popOut)
	{
		phaseChangeBlocked = true;
		yield return new WaitForEndOfFrame();
		Spine.AnimationState.SetAnimation(0, popOutAnimation, false);
		AudioManager.Instance.PlayOneShot("event:/enemy/tunnel_worm/tunnel_worm_burst_out_of_ground", base.gameObject);
		float time2 = 0f;
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
		health.invincible = true;
		Spine.gameObject.SetActive(false);
		AudioManager.Instance.PlayOneShot("event:/enemy/tunnel_worm/tunnel_worm_disappear_underground", base.gameObject);
		float Progress = 0f;
		float Duration2 = 11f / 30f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * Spine.timeScale);
			if (!(num < Duration2))
			{
				break;
			}
			SpawnTrailSpikes();
			yield return null;
		}
		loopingSoundInstance = AudioManager.Instance.CreateLoop("event:/enemy/tunnel_worm/tunnel_worm_underground_loop", base.gameObject, true);
		Vector3 startPosition = base.transform.position;
		Progress = 0f;
		Duration2 = Vector3.Distance(startPosition, position) / moveSpeed;
		CameraManager.instance.ShakeCameraForDuration(0.2f, 0.3f, Duration2);
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * Spine.timeScale);
			if (!(num < Duration2))
			{
				break;
			}
			base.transform.position = Vector3.Lerp(startPosition, position, Mathf.SmoothStep(0f, 1f, Progress / Duration2));
			SpawnTrailSpikes();
			yield return null;
		}
		base.transform.position = position;
		health.invincible = false;
		Spine.gameObject.SetActive(true);
		AudioManager.Instance.StopLoop(loopingSoundInstance);
		AudioManager.Instance.PlayOneShot("event:/enemy/tunnel_worm/tunnel_worm_burst_out_of_ground", base.gameObject);
		yield return null;
		simpleSpineFlash.FlashWhite(0f);
		if (popOut)
		{
			Spine.AnimationState.SetAnimation(0, popInAnimation, false);
			Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
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
			phaseChangeBlocked = false;
		}
		else
		{
			phaseChangeBlocked = false;
		}
	}

	private void SpawnTrailSpikes()
	{
		if ((trailTimer += Time.deltaTime) > delayBetweenTrailSpike)
		{
			trailTimer = 0f;
			GetTrailSpike();
		}
	}

	private GameObject GetTrailSpike()
	{
		GameObject gameObject = null;
		if (trailSpikes.Count > 0)
		{
			foreach (GameObject trailSpike in trailSpikes)
			{
				if (!trailSpike.activeSelf)
				{
					gameObject = trailSpike;
					gameObject.transform.position = base.transform.position;
					gameObject.SetActive(true);
					break;
				}
			}
		}
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate(trailSpikePrefab, base.transform.position, Quaternion.identity, base.transform.parent);
			trailSpikes.Add(gameObject);
			ColliderEvents componentInChildren = gameObject.GetComponentInChildren<ColliderEvents>();
			if ((bool)componentInChildren)
			{
				componentInChildren.OnTriggerEnterEvent += OnDamageTriggerEnter;
			}
		}
		return gameObject;
	}

	private void HeadSmash()
	{
		StartCoroutine(HeadSmashIE());
	}

	private IEnumerator HeadSmashIE()
	{
		anticipating = true;
		anticipationDuration = headSmashAnticipation;
		yield return new WaitForSeconds(headSmashAnticipation);
		yield return new WaitForEndOfFrame();
		phaseChangeBlocked = true;
		AudioManager.Instance.PlayOneShot("event:/boss/worm/slam_attack", base.gameObject);
		Spine.AnimationState.SetAnimation(0, headSmashAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		Spine.zSpacing = zSpacing;
		yield return new WaitForSeconds(0.5f);
		CameraManager.instance.ShakeCameraForDuration(1.3f, 1.5f, 0.3f);
		yield return StartCoroutine(EnableDamageCollider(0.1f, headSmashPositions[0]));
		yield return new WaitForSeconds(0.5f);
		CameraManager.instance.ShakeCameraForDuration(1.3f, 1.5f, 0.3f);
		yield return StartCoroutine(EnableDamageCollider(0.1f, headSmashPositions[1]));
		yield return new WaitForSeconds(0.8f);
		CameraManager.instance.ShakeCameraForDuration(1.6f, 1.8f, 0.3f);
		yield return StartCoroutine(EnableDamageCollider(0.1f, headSmashPositions[2]));
		DOTween.To(() => Spine.zSpacing, delegate(float x)
		{
			Spine.zSpacing = x;
		}, ogSpacing, 0.25f);
		yield return new WaitForSeconds(1f);
		phaseChangeBlocked = false;
	}

	private IEnumerator EnableDamageCollider(float time, Vector3 position)
	{
		damageCollider.transform.localPosition = position;
		damageCollider.gameObject.SetActive(true);
		yield return new WaitForSeconds(time);
		damageCollider.gameObject.SetActive(false);
	}

	public IEnumerator EnragedIE()
	{
		AudioManager.Instance.PlayOneShot("event:/boss/worm/roar", AudioManager.Instance.Listener);
		yield return new WaitForEndOfFrame();
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
		CameraManager.instance.ShakeCameraForDuration(2f, 2.5f, 1f);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (num < 1.5f)
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	private Vector3 GetRandomPosition(float radius)
	{
		if (juicedForm && UnityEngine.Random.value > 0.5f && GetClosestTarget() != null)
		{
			return GetClosestTarget().transform.position;
		}
		return UnityEngine.Random.insideUnitCircle * radius;
	}

	protected virtual void OnDamageTriggerEnter(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && component.team != health.team)
		{
			component.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.7f));
		}
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		RoomLockController.RoomCompleted(true, false);
		PlayerFarming.Instance.health.invincible = true;
		PlayerFarming.Instance.playerWeapon.DoSlowMo(false);
		AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("KILL_BOSS_1"));
		damageCollider.gameObject.SetActive(false);
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, 0f);
		foreach (UnitObject spawnedEnemy in spawnedEnemies)
		{
			if (spawnedEnemy != null)
			{
				spawnedEnemy.health.enabled = true;
				spawnedEnemy.health.invincible = false;
				spawnedEnemy.health.untouchable = false;
				spawnedEnemy.health.DealDamage(spawnedEnemy.health.totalHP, base.gameObject, base.transform.position, false, Health.AttackTypes.Heavy);
			}
		}
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
		isDead = true;
		GameManager.GetInstance().CamFollowTarget.MinZoom = 11f;
		GameManager.GetInstance().CamFollowTarget.MaxZoom = 13f;
		StopCoroutine(currentAttackRoutine);
		StopCoroutine(currentPhaseRoutine);
		StartCoroutine(Die());
	}

	private IEnumerator Die()
	{
		GameObject[] array = deathParticlePrefabs;
		for (int i = 0; i < array.Length; i++)
		{
			UnityEngine.Object.Instantiate(array[i], base.transform.position, Quaternion.identity, base.transform.parent);
		}
		ClearPaths();
		speed = 0f;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(cameraTarget, 12f);
		anticipating = false;
		Spine.zSpacing = ogSpacing;
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
		AudioManager.Instance.PlayOneShot("event:/boss/worm/death", AudioManager.Instance.Listener);
		bool beatenLayer2 = DataManager.Instance.BeatenLeshyLayer2;
		float time2;
		if (!DataManager.Instance.BossesCompleted.Contains(PlayerFarming.Location) && !DungeonSandboxManager.Active)
		{
			Spine.AnimationState.SetAnimation(0, "die", false);
			Spine.AnimationState.AddAnimation(0, "dead", true, 0f);
		}
		else
		{
			if (juicedForm && !DataManager.Instance.BeatenLeshyLayer2 && !DungeonSandboxManager.Active)
			{
				Spine.AnimationState.SetAnimation(0, "die-follower", false);
				time2 = 0f;
				while (true)
				{
					float num;
					time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
					if (!(num < 4.5f))
					{
						break;
					}
					yield return null;
				}
				Spine.gameObject.SetActive(false);
				PlayerReturnToBase.Disabled = true;
				GameObject Follower = UnityEngine.Object.Instantiate(followerToSpawn, base.transform.position, Quaternion.identity, base.transform.parent);
				Follower.GetComponent<Interaction_FollowerSpawn>().Play("CultLeader 1", ScriptLocalization.NAMES_CultLeaders.Dungeon1);
				DataManager.SetFollowerSkinUnlocked("CultLeader 1");
				DataManager.Instance.BeatenLeshyLayer2 = true;
				ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.NewGamePlus1);
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
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 4.2f))
			{
				break;
			}
			yield return null;
		}
		for (int j = 0; j < 20; j++)
		{
			BiomeConstants.Instance.EmitBloodSplatterGroundParticles(base.transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * 3f), Vector3.zero, Color.red);
		}
		GameManager.GetInstance().OnConversationEnd();
		if (!DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_1))
		{
			interaction_MonsterHeart.ObjectiveToComplete = Objectives.CustomQuestTypes.BishopsOfTheOldFaith1;
		}
		interaction_MonsterHeart.Play((!beatenLayer2 && GameManager.Layer2) ? InventoryItem.ITEM_TYPE.GOD_TEAR : InventoryItem.ITEM_TYPE.NONE);
	}

	private void OnDrawGizmosSelected()
	{
		Vector3[] array = headSmashPositions;
		foreach (Vector3 position in array)
		{
			Utils.DrawCircleXY(base.transform.InverseTransformPoint(position), 0.5f, Color.yellow);
		}
	}
}
