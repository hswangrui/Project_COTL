using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using Spine;
using Spine.Unity;
using UnityEngine;
using WebSocketSharp;

public class EnemyWormTurret : UnitObject
{
	public SimpleSpineFlash SimpleSpineFlash;

	public SkeletonAnimation Spine;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string IdleAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string AppearAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string DisapearAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string HiddenAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string ShootAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string AnticipationShootAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string AnticipationSpikeAnimation;

	[SerializeField]
	private GameObject lighting;

	public GameObject TrailPrefab;

	public List<GameObject> Trails = new List<GameObject>();

	public float DelayBetweenTrails = 0.2f;

	private float TrailsTimer;

	[SerializeField]
	private bool repositionOnHit = true;

	[SerializeField]
	private bool spawns;

	[SerializeField]
	private Vector2 spawnAmount;

	[SerializeField]
	private Vector2 spawnDelay;

	[SerializeField]
	private int maxActiveSpawns;

	[SerializeField]
	private UnitObject spawnable;

	[SerializeField]
	private bool canSpikeCircle;

	[SerializeField]
	private float spikeAnticipation;

	[SerializeField]
	private int circleSpikeAmount = 4;

	[SerializeField]
	private float circleDelayBetweenSpikes = 0.2f;

	[SerializeField]
	private float circleDistanceBetweenSpikes = 0.2f;

	[SerializeField]
	private int circleSpikeDistance = 10;

	[EventRef]
	public string AttackVO = string.Empty;

	[EventRef]
	public string DeathVO = string.Empty;

	[EventRef]
	public string GetHitVO = string.Empty;

	[EventRef]
	public string WarningVO = string.Empty;

	[EventRef]
	public string ShootSFX = "event:/enemy/shoot_magicenergy";

	[EventRef]
	public string ShootSpikeSfx = "event:/enemy/shoot_arrowspike";

	private ShowHPBar ShowHPBar;

	private GameObject targetObject;

	private float spawnTimestamp = 5f;

	private float spawnRadius = 5f;

	private int spawnedAmount;

	private bool targeted;

	private bool active;

	private List<UnitObject> spawnedEnemies = new List<UnitObject>();

	private List<GameObject> spawnedSpikes = new List<GameObject>();

	private Coroutine currentRoutine;

	private float initialDelay;

	private GameObject t;

	private Vector3 previousSpawnPosition;

	private bool ShootingSpikeCircle;

	public bool Shoots = true;

	public GameObject Prefab;

	public float ShootDelay = 0.25f;

	public Vector2 DelayBetweenShots = new Vector2(0.1f, 0.3f);

	public float NumberOfShotsToFire = 5f;

	public float DistanceFromPlayerToFire = 5f;

	public float GravSpeed = -15f;

	public float AnticipationTime;

	public float Arc = 360f;

	public float shootCooldown = 0.5f;

	public Vector2 RandomArcOffset = new Vector2(0f, 0f);

	public Vector2 ShootDistanceRange = new Vector2(2f, 3f);

	public Vector3 ShootOffset;

	public bool BulletsTargetPlayer = true;

	private bool anticipating;

	private float anticipatingTimer;

	private float anticipationTime;

	private GameObject g;

	private GrenadeBullet GrenadeBullet;

	private float CacheShootDirectionCache;

	private float Angle;

	private Vector3 Force;

	public bool DiveAboveGround;

	public float ArcHeight = 5f;

	private EventInstance loopingSoundInstance;

	[Range(0f, 1f)]
	public float ChanceToPathTowardsPlayer = 0.8f;

	public float TurningArc = 90f;

	public Vector2 DistanceRange = new Vector2(1f, 3f);

	private bool PathingToPlayer;

	private float RandomDirection;

	public float DistanceToPathTowardsPlayer = 6f;

	public float MinimumPlayerDistance = 3f;

	public float MoveSpeed = 5f;

	public float MoveDelay;

	public Vector2 MaxMoveDistance;

	private float IdleWait = 0.5f;

	public Vector3 TargetPosition;

	public float CircleCastRadius = 0.5f;

	public float CircleCastOffset = 1f;

	public bool ShowDebug;

	public List<Vector3> Points = new List<Vector3>();

	public List<Vector3> PointsLink = new List<Vector3>();

	public List<Vector3> EndPoints = new List<Vector3>();

	public List<Vector3> EndPointsLink = new List<Vector3>();

	private void Start()
	{
		initialDelay = UnityEngine.Random.Range(0f, 2f);
		CreatePool(circleSpikeAmount * circleSpikeDistance * 2);
	}

	public override void OnEnable()
	{
		base.OnEnable();
		DisableForces = true;
		ShowHPBar = GetComponent<ShowHPBar>();
		StartCoroutine(CreateSpineEventListener());
		if (active)
		{
			if (currentRoutine != null)
			{
				StopCoroutine(currentRoutine);
			}
			currentRoutine = StartCoroutine(MoveRoutine());
		}
		active = true;
	}

	private IEnumerator CreateSpineEventListener()
	{
		yield return new WaitForSeconds(0.1f);
		Spine.AnimationState.Event += Shoot;
	}

	private void Shoot(TrackEntry trackEntry, global::Spine.Event e)
	{
		if (e.Data.Name == "shoot" && Shoots)
		{
			if (currentRoutine != null)
			{
				StopCoroutine(currentRoutine);
			}
			currentRoutine = StartCoroutine(ShootRoutine());
		}
	}

	public void SpawnTrails()
	{
		if (!((TrailsTimer += Time.deltaTime) > DelayBetweenTrails) || !(Vector3.Distance(base.transform.position, previousSpawnPosition) > 0.1f))
		{
			return;
		}
		TrailsTimer = 0f;
		t = null;
		if (Trails.Count > 0)
		{
			foreach (GameObject trail in Trails)
			{
				if (!trail.activeSelf)
				{
					t = trail;
					t.transform.position = base.transform.position;
					t.SetActive(true);
					break;
				}
			}
		}
		if (t == null)
		{
			t = UnityEngine.Object.Instantiate(TrailPrefab, base.transform.position, Quaternion.identity, base.transform.parent);
			Trails.Add(t);
			ColliderEvents componentInChildren = t.GetComponentInChildren<ColliderEvents>();
			if ((bool)componentInChildren)
			{
				componentInChildren.OnTriggerEnterEvent += OnDamageTriggerEnter;
			}
		}
		previousSpawnPosition = t.transform.position;
	}

	protected virtual void OnDamageTriggerEnter(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && (component.team != health.team || health.team == Health.Team.PlayerTeam))
		{
			component.DealDamage(1f, component.gameObject, component.transform.position);
		}
	}

	public override void OnDestroy()
	{
		AudioManager.Instance.StopLoop(loopingSoundInstance);
		foreach (GameObject trail in Trails)
		{
			if (trail != null)
			{
				ColliderEvents componentInChildren = trail.GetComponentInChildren<ColliderEvents>();
				if ((bool)componentInChildren)
				{
					componentInChildren.OnTriggerEnterEvent -= OnDamageTriggerEnter;
				}
			}
		}
		base.OnDestroy();
	}

	private IEnumerator SpawnSpikesInDirectionsIE(int amount, float delayBetweenSpikes, float distanceBetweenSpikes)
	{
		float time2 = 0f;
		ShootingSpikeCircle = true;
		anticipating = true;
		anticipatingTimer = 0f;
		anticipationTime = spikeAnticipation;
		AudioManager.Instance.PlayOneShot("event:/enemy/tunnel_worm/tunnel_worm_screech");
		if (!AnticipationSpikeAnimation.IsNullOrEmpty())
		{
			yield return Spine.YieldForAnimation(AnticipationSpikeAnimation);
		}
		else
		{
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
		}
		AudioManager.Instance.PlayOneShot("event:/boss/worm/spike_attack");
		Spine.AnimationState.SetAnimation(0, ShootAnimation, false);
		Spine.AnimationState.AddAnimation(0, IdleAnimation, true, 0f);
		anticipating = false;
		CameraManager.instance.ShakeCameraForDuration(0.1f, 0.1f, 0.1f, false);
		yield return new WaitForEndOfFrame();
		SimpleSpineFlash.FlashWhite(false);
		int num2 = UnityEngine.Random.Range(0, 360);
		for (int i = 0; i < amount; i++)
		{
			Vector3 direction = new Vector3(Mathf.Cos((float)num2 * ((float)Math.PI / 180f)), Mathf.Sin((float)num2 * ((float)Math.PI / 180f)), 0f);
			StartCoroutine(ShootSpikesInDirectionIE(direction, delayBetweenSpikes, distanceBetweenSpikes));
			num2 = (int)Mathf.Repeat(num2 + 360 / amount, 360f);
		}
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < spikeAnticipation + delayBetweenSpikes + 1f))
			{
				break;
			}
			yield return null;
		}
		ShootingSpikeCircle = false;
	}

	public IEnumerator ShootSpikesInDirectionIE(Vector3 direction, float delayBetweenSpikes, float distanceBetweenSpikes)
	{
		Vector3 position = base.transform.position;
		for (int i = 0; i < circleSpikeDistance; i++)
		{
			GetSpawnSpike().transform.position = position;
			position += direction * distanceBetweenSpikes;
			float time = 0f;
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * Spine.timeScale);
				if (!(num < delayBetweenSpikes))
				{
					break;
				}
				yield return null;
			}
		}
	}

	private void CreatePool(int count)
	{
		for (int i = 0; i < count; i++)
		{
			t = UnityEngine.Object.Instantiate(TrailPrefab, base.transform.position, Quaternion.identity, base.transform.parent);
			t.gameObject.SetActive(false);
			spawnedSpikes.Add(t);
			ColliderEvents componentInChildren = t.GetComponentInChildren<ColliderEvents>();
			if ((bool)componentInChildren)
			{
				componentInChildren.OnTriggerEnterEvent += OnDamageTriggerEnter;
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
			gameObject = UnityEngine.Object.Instantiate(TrailPrefab, base.transform.position, Quaternion.identity, base.transform.parent);
			spawnedSpikes.Add(gameObject);
			ColliderEvents componentInChildren = gameObject.GetComponentInChildren<ColliderEvents>();
			if ((bool)componentInChildren)
			{
				componentInChildren.OnTriggerEnterEvent += OnDamageTriggerEnter;
			}
		}
		return gameObject;
	}

	private IEnumerator ShootRoutine()
	{
		CacheShootDirectionCache = ((GetClosestTarget() == null) ? 0f : Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position));
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.4f, 0.2f, false);
		float randomStartAngle = UnityEngine.Random.Range(0, 360);
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (!((float)num < NumberOfShotsToFire))
			{
				break;
			}
			if (!string.IsNullOrEmpty(ShootSFX))
			{
				AudioManager.Instance.PlayOneShot(ShootSFX, base.transform.position);
			}
			float num2 = (BulletsTargetPlayer ? (CacheShootDirectionCache - Arc / 2f + Arc / NumberOfShotsToFire * (float)i) : randomStartAngle);
			num2 += UnityEngine.Random.Range(RandomArcOffset.x, RandomArcOffset.y);
			GrenadeBullet = ObjectPool.Spawn(Prefab, base.transform.position + ShootOffset, Quaternion.identity).GetComponent<GrenadeBullet>();
			GrenadeBullet.Play(-1f, num2, UnityEngine.Random.Range(ShootDistanceRange.x, ShootDistanceRange.y), UnityEngine.Random.Range(GravSpeed - 2f, GravSpeed + 2f), health.team);
			randomStartAngle = Mathf.Repeat(randomStartAngle + 360f / NumberOfShotsToFire, 360f);
			if (!(DelayBetweenShots != Vector2.zero))
			{
				continue;
			}
			float time = 0f;
			float dur = UnityEngine.Random.Range(DelayBetweenShots.x, DelayBetweenShots.y);
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
	}

	public override void OnDisable()
	{
		AudioManager.Instance.StopLoop(loopingSoundInstance);
		Spine.AnimationState.Event -= Shoot;
		base.OnDisable();
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		if (!string.IsNullOrEmpty(GetHitVO))
		{
			AudioManager.Instance.PlayOneShot(GetHitVO, base.transform.position);
		}
		if (repositionOnHit && GetNewTargetPosition() && !ShootingSpikeCircle)
		{
			StopAllCoroutines();
			if (currentRoutine != null)
			{
				StopCoroutine(currentRoutine);
			}
			currentRoutine = StartCoroutine(MoveRoutine());
		}
		StartCoroutine(ApplyForceRoutine(Attacker));
		if (SimpleSpineFlash != null)
		{
			SimpleSpineFlash.FlashFillRed();
		}
		AudioManager.Instance.StopLoop(loopingSoundInstance);
	}

	private IEnumerator ApplyForceRoutine(GameObject Attacker)
	{
		Angle = Utils.GetAngle(Attacker.transform.position, base.transform.position) * ((float)Math.PI / 180f);
		Force = new Vector2(100f * Mathf.Cos(Angle), 100f * Mathf.Sin(Angle));
		rb.AddForce(Force);
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
		if (state.CURRENT_STATE == StateMachine.State.Idle)
		{
			IdleWait = 0f;
		}
	}

	private IEnumerator ActiveRoutine()
	{
		state.CURRENT_STATE = StateMachine.State.Idle;
		while (state.CURRENT_STATE != 0 || !((IdleWait -= Time.deltaTime) <= 0f) || !GetNewTargetPosition())
		{
			yield return null;
		}
		if (currentRoutine != null)
		{
			StopCoroutine(currentRoutine);
		}
		currentRoutine = StartCoroutine(DiveAboveGround ? DiveMoveRoutine() : MoveRoutine());
	}

	private IEnumerator DiveMoveRoutine()
	{
		CameraManager.shakeCamera(0.3f);
		Vector3 StartPosition = base.transform.position;
		float Progress = 0f;
		float Duration = Vector3.Distance(StartPosition, TargetPosition) / MoveSpeed;
		Vector3 Curve = StartPosition + (TargetPosition - StartPosition) / 2f + Vector3.back * ArcHeight;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * Spine.timeScale);
			if (!(num < Duration))
			{
				break;
			}
			Vector3 a = Vector3.Lerp(StartPosition, Curve, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			Vector3 b = Vector3.Lerp(Curve, TargetPosition, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			base.transform.position = Vector3.Lerp(a, b, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		TargetPosition.z = 0f;
		base.transform.position = TargetPosition;
		Spine.transform.localPosition = Vector3.zero;
		state.CURRENT_STATE = StateMachine.State.Idle;
		if (currentRoutine != null)
		{
			StopCoroutine(currentRoutine);
		}
		currentRoutine = StartCoroutine(ActiveRoutine());
	}

	private IEnumerator MoveRoutine()
	{
		Spine.AnimationState.AddAnimation(0, IdleAnimation, true, 0f);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < initialDelay))
			{
				break;
			}
			yield return null;
		}
		initialDelay = 0f;
		yield return new WaitForEndOfFrame();
		Spine.AnimationState.SetAnimation(0, DisapearAnimation, false);
		Spine.AnimationState.AddAnimation(0, HiddenAnimation, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/enemy/tunnel_worm/tunnel_worm_disappear_underground", base.gameObject);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.25f))
			{
				break;
			}
			yield return null;
		}
		health.invincible = true;
		previousSpawnPosition = Vector3.positiveInfinity;
		float Progress = 0f;
		float Duration = 11f / 30f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * Spine.timeScale);
			if (!(num < Duration))
			{
				break;
			}
			SpawnTrails();
			yield return null;
		}
		if ((bool)lighting)
		{
			lighting.SetActive(false);
		}
		ShowHPBar showHPBar = ShowHPBar;
		if ((object)showHPBar != null)
		{
			showHPBar.Hide();
		}
		state.CURRENT_STATE = StateMachine.State.Fleeing;
		if (!loopingSoundInstance.isValid())
		{
			loopingSoundInstance = AudioManager.Instance.CreateLoop("event:/enemy/tunnel_worm/tunnel_worm_underground_loop", base.gameObject, true);
		}
		Vector3 StartPosition = base.transform.position;
		StartPosition.z = 0f;
		TargetPosition.z = 0f;
		Progress = 0f;
		Duration = Vector3.Distance(StartPosition, TargetPosition) / MoveSpeed;
		previousSpawnPosition = Vector3.positiveInfinity;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * Spine.timeScale);
			if (!(num < Duration))
			{
				break;
			}
			base.transform.position = Vector3.Lerp(StartPosition, TargetPosition, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			SpawnTrails();
			yield return null;
		}
		base.transform.position = TargetPosition;
		health.invincible = false;
		AudioManager.Instance.StopLoop(loopingSoundInstance);
		AudioManager.Instance.PlayOneShot("event:/enemy/tunnel_worm/tunnel_worm_burst_out_of_ground", base.gameObject);
		Spine.AnimationState.SetAnimation(0, AppearAnimation, false);
		Spine.AnimationState.AddAnimation(0, IdleAnimation, true, 0f);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.55f))
			{
				break;
			}
			yield return null;
		}
		if ((bool)lighting)
		{
			lighting.SetActive(true);
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		if (GameManager.RoomActive && Shoots)
		{
			if (canSpikeCircle && UnityEngine.Random.Range(0f, 1f) < 0.5f)
			{
				yield return StartCoroutine(SpawnSpikesInDirectionsIE(circleSpikeAmount, circleDelayBetweenSpikes, circleDistanceBetweenSpikes));
				time2 = 0f;
				while (true)
				{
					float num;
					time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
					if (!(num < MoveDelay))
					{
						break;
					}
					yield return null;
				}
				if (GetNewTargetPosition())
				{
					if (currentRoutine != null)
					{
						StopCoroutine(currentRoutine);
					}
					currentRoutine = StartCoroutine(MoveRoutine());
				}
				else
				{
					if (currentRoutine != null)
					{
						StopCoroutine(currentRoutine);
					}
					currentRoutine = StartCoroutine(ActiveRoutine());
				}
			}
			else
			{
				StartCoroutine(ShootAtPlayerRoutine());
			}
			yield break;
		}
		if (canSpikeCircle)
		{
			yield return StartCoroutine(SpawnSpikesInDirectionsIE(circleSpikeAmount, circleDelayBetweenSpikes, circleDistanceBetweenSpikes));
		}
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < MoveDelay))
			{
				break;
			}
			yield return null;
		}
		if (GetNewTargetPosition())
		{
			if (currentRoutine != null)
			{
				StopCoroutine(currentRoutine);
			}
			currentRoutine = StartCoroutine(MoveRoutine());
		}
		else
		{
			if (currentRoutine != null)
			{
				StopCoroutine(currentRoutine);
			}
			currentRoutine = StartCoroutine(ActiveRoutine());
		}
	}

	public override void Update()
	{
		base.Update();
		if (targetObject == null)
		{
			targetObject = GameObject.FindWithTag("Player");
		}
		if (!targeted && (bool)targetObject && Vector3.Distance(base.transform.position, targetObject.transform.position) < (float)VisionRange)
		{
			targeted = true;
			if (currentRoutine != null)
			{
				StopCoroutine(currentRoutine);
			}
			currentRoutine = StartCoroutine(ActiveRoutine());
		}
		if (anticipating)
		{
			anticipatingTimer += Time.deltaTime;
			float num = anticipatingTimer / anticipationTime * 0.75f;
			SimpleSpineFlash.FlashWhite(num);
			if (num > 1f)
			{
				anticipating = false;
				anticipatingTimer = 0f;
			}
		}
		if (targeted && spawns)
		{
			GameManager instance = GameManager.GetInstance();
			if ((((object)instance != null) ? new float?(instance.CurrentTime) : null) > spawnTimestamp && spawnedAmount < maxActiveSpawns)
			{
				Spawn();
			}
		}
	}

	private void Spawn()
	{
		int num = (int)UnityEngine.Random.Range(spawnAmount.x, spawnAmount.y + 1f);
		for (int i = 0; i < num; i++)
		{
			if (spawnedAmount >= maxActiveSpawns)
			{
				break;
			}
			Vector3 position = UnityEngine.Random.insideUnitCircle * spawnRadius;
			UnitObject unitObject = UnityEngine.Object.Instantiate(spawnable, position, Quaternion.identity, base.transform.parent);
			unitObject.health.OnDie += SpawnedEnemyKilled;
			DropLootOnDeath component = unitObject.GetComponent<DropLootOnDeath>();
			if ((bool)component)
			{
				component.GiveXP = false;
			}
			spawnedEnemies.Add(unitObject);
			spawnedAmount++;
		}
		spawnTimestamp = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(spawnDelay.x, spawnDelay.y);
	}

	private void SpawnedEnemyKilled(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		spawnedAmount--;
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (!string.IsNullOrEmpty(DeathVO))
		{
			AudioManager.Instance.PlayOneShot(GetHitVO, base.transform.position);
		}
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		foreach (UnitObject spawnedEnemy in spawnedEnemies)
		{
			if (spawnedEnemy != null)
			{
				spawnedEnemy.health.enabled = true;
				spawnedEnemy.health.DealDamage(spawnedEnemy.health.totalHP, base.gameObject, spawnedEnemy.transform.position, false, Health.AttackTypes.Heavy);
			}
		}
		AudioManager.Instance.StopLoop(loopingSoundInstance);
		if (!health.DestroyOnDeath)
		{
			StartCoroutine(DelayedDestroy());
			base.gameObject.SetActive(false);
		}
	}

	private IEnumerator DelayedDestroy()
	{
		yield return new WaitForSeconds(1f);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private IEnumerator ShootAtPlayerRoutine()
	{
		anticipating = true;
		anticipatingTimer = 0f;
		anticipationTime = AnticipationTime;
		float time2;
		if (!AnticipationShootAnimation.IsNullOrEmpty())
		{
			yield return Spine.YieldForAnimation(AnticipationShootAnimation);
		}
		else
		{
			time2 = 0f;
			while (true)
			{
				float num;
				time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num < anticipationTime - ShootDelay))
				{
					break;
				}
				yield return null;
			}
		}
		Spine.AnimationState.SetAnimation(0, ShootAnimation, false);
		Spine.AnimationState.AddAnimation(0, IdleAnimation, true, 0f);
		if (AnticipationShootAnimation.IsNullOrEmpty())
		{
			time2 = 0f;
			while (true)
			{
				float num;
				time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num < ShootDelay))
				{
					break;
				}
				yield return null;
			}
		}
		anticipating = false;
		yield return new WaitForEndOfFrame();
		SimpleSpineFlash.FlashWhite(false);
		if (currentRoutine != null)
		{
			StopCoroutine(currentRoutine);
		}
		currentRoutine = StartCoroutine(ShootRoutine());
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < shootCooldown))
			{
				break;
			}
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		if (currentRoutine != null)
		{
			StopCoroutine(currentRoutine);
		}
		currentRoutine = StartCoroutine(ActiveRoutine());
	}

	private bool GetNewTargetPosition()
	{
		float num = 100f;
		if (GetClosestTarget() != null && ChanceToPathTowardsPlayer > 0f && UnityEngine.Random.value < ChanceToPathTowardsPlayer && Vector3.Distance(base.transform.position, GetClosestTarget().transform.position) < DistanceToPathTowardsPlayer)
		{
			PathingToPlayer = true;
			RandomDirection = Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position) * ((float)Math.PI / 180f);
		}
		if (ChanceToPathTowardsPlayer >= 1f && GameManager.RoomActive && GetClosestTarget() != null)
		{
			float num2 = Vector3.Distance(base.transform.position, GetClosestTarget().transform.position);
			if (num2 > MinimumPlayerDistance && num2 < DistanceToPathTowardsPlayer)
			{
				Vector3 normalized = (GetClosestTarget().transform.position - base.transform.position).normalized;
				float num3 = MaxMoveDistance.y;
				RaycastHit2D raycastHit2D;
				RaycastHit2D raycastHit2D2 = (raycastHit2D = Physics2D.Raycast(base.transform.position, normalized, 100f, layerToCheck));
				if (raycastHit2D2.collider != null)
				{
					num3 = Mathf.Min(num3, Vector3.Distance(base.transform.position, raycastHit2D.point));
				}
				TargetPosition = base.transform.position + normalized * UnityEngine.Random.Range(MaxMoveDistance.x, num3);
				return true;
			}
			PathingToPlayer = false;
		}
		while ((num -= 1f) > 0f)
		{
			float num4 = UnityEngine.Random.Range(DistanceRange.x, DistanceRange.y);
			if (!PathingToPlayer)
			{
				RandomDirection += UnityEngine.Random.Range(0f - TurningArc, TurningArc) * ((float)Math.PI / 180f);
			}
			PathingToPlayer = false;
			float radius = 0.2f;
			Vector3 vector = base.transform.position + new Vector3(num4 * Mathf.Cos(RandomDirection), num4 * Mathf.Sin(RandomDirection));
			if (Physics2D.CircleCast(base.transform.position, radius, Vector3.Normalize(vector - base.transform.position), num4, layerToCheck).collider != null)
			{
				RandomDirection = 180f - RandomDirection;
				continue;
			}
			TargetPosition = vector;
			return true;
		}
		return false;
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, VisionRange, Color.yellow);
		Utils.DrawCircleXY(base.transform.position + ShootOffset, 0.5f, Color.yellow);
		int num = -1;
		while (++num < Points.Count)
		{
			Utils.DrawCircleXY(PointsLink[num], 0.5f, Color.blue);
			Utils.DrawCircleXY(Points[num], CircleCastRadius, Color.blue);
			Utils.DrawLine(Points[num], PointsLink[num], Color.blue);
		}
		num = -1;
		while (++num < EndPoints.Count)
		{
			Utils.DrawCircleXY(EndPointsLink[num], 0.5f, Color.red);
			Utils.DrawCircleXY(EndPoints[num], CircleCastRadius, Color.red);
			Utils.DrawLine(EndPointsLink[num], EndPoints[num], Color.red);
		}
	}
}
