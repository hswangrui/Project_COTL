using System;
using System.Collections;
using System.Collections.Generic;
using CotL.Projectiles;
using DG.Tweening;
using FMOD.Studio;
using I2.Loc;
using MMTools;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DeathCatClone : UnitObject
{
	public enum AttackType
	{
		None = 0,
		Projectiles_Rings = 1,
		Projectiles_Swirls = 2,
		Projectiles_Bouncers = 3,
		Projectiles_Circles = 4,
		Projectiles_Cross = 5,
		Projectiles_Pulse = 6,
		Projectiles_TripleRings = 7,
		Projectiles_CirclesFast = 8,
		Traps_Targeted = 50,
		Traps_Pattern0 = 51,
		Traps_Pattern1 = 52,
		Traps_Pattern2 = 53,
		Traps_Scattered = 54,
		Teleport_Attack0 = 100,
		Teleport_Attack1 = 101,
		Teleport_Attack2 = 102
	}

	public SkeletonAnimation Spine;

	public SimpleSpineFlash SimpleSpineFlash;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string idleAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string appearAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string disappearAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string shootAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string transformAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string hurtAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string hurtLoopAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string stopHurtAnimation;

	public SkeletonAnimation GroundCracksSpine;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "GroundCracksSpine")]
	private string crackingAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "GroundCracksSpine")]
	private string crackedAnimation;

	public SkeletonAnimation Chain4Spine;

	public SkeletonAnimation Chain5Spine;

	public SkeletonAnimation Chain6Spine;

	public SkeletonAnimation Chain7Spine;

	public SkeletonAnimation Chain8Spine;

	public SkeletonAnimation Chain9Spine;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Chain4Spine")]
	private string breakAnimation1;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Chain4Spine")]
	private string breakAnimation2;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Chain4Spine")]
	private string breakAnimation3;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Chain4Spine")]
	private string brokenAnimation;

	[SerializeField]
	private GameObject Chain5;

	[SerializeField]
	private GameObject Chain7;

	[SerializeField]
	private GameObject Chain9;

	[Space]
	[SerializeField]
	private ColliderEvents distortionObject;

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
	private float projectilePatternCrossDuration;

	[SerializeField]
	private ProjectileCross projectilePatternCross;

	[Space]
	[SerializeField]
	private ProjectilePatternBase swirls;

	[SerializeField]
	private ProjectilePatternBase bouncers;

	[SerializeField]
	private ProjectilePatternBase circles;

	[SerializeField]
	private ProjectilePatternBase circlesFast;

	[SerializeField]
	private ProjectilePatternBase pulse;

	[SerializeField]
	private GameObject trapPrefab;

	public List<TrapProjectileCross> crossTraps;

	[SerializeField]
	private Vector2 distanceRange = new Vector2(1f, 3f);

	[SerializeField]
	private Vector2 idleWaitRange = new Vector2(1f, 3f);

	[SerializeField]
	private float acceleration = 5f;

	[SerializeField]
	private float turningSpeed = 1f;

	[SerializeField]
	private bool manualAttacking;

	private Vector2 timeBetweenAttacks;

	[SerializeField]
	private EnemyDeathCatBoss deathCatBoss;

	[SerializeField]
	private int maxEnemies;

	[SerializeField]
	private Vector2 timeBetweenEnemies;

	[SerializeField]
	private AssetReferenceGameObject[] spawningEnemiesList;

	[SerializeField]
	private bool SpawnEnemies;

	private GameObject targetObject;

	private float randomDirection;

	private LayerMask bounceMask;

	private float attackTimestamp;

	private float spawnEnemyTimestamp;

	private Coroutine currentAttackRoutine;

	private bool isMoving = true;

	private bool isSpawning = true;

	private int countSinceTeleport = -1;

	private int phase = 1;

	private bool initialised;

	private ProjectileCross projectileCross;

	private List<AsyncOperationHandle<GameObject>> loadedAddressableAssets = new List<AsyncOperationHandle<GameObject>>();

	private EventInstance groundCrackSoundInstance;

	private int count;

	private float IdleWait;

	private int DetectEnemyRange = 8;

	public bool IsFake { get; private set; }

	public override void OnDestroy()
	{
		base.OnDestroy();
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

	public override void Awake()
	{
		base.Awake();
		InitializeTraps();
		InitializeProjectilePatternRings();
		InitializeProjectileCross();
	}

	private void Start()
	{
		health.BlackSoulOnHit = true;
		timeBetweenAttacks = new Vector2(1f, 2f);
		bounceMask = (int)bounceMask | (1 << LayerMask.NameToLayer("Island"));
		bounceMask = (int)bounceMask | (1 << LayerMask.NameToLayer("Obstacles"));
		bounceMask = (int)bounceMask | (1 << LayerMask.NameToLayer("Units"));
		attackTimestamp = GameManager.GetInstance().CurrentTime + 1f;
		spawnEnemyTimestamp = GameManager.GetInstance().CurrentTime + 0.5f;
		Spine.AnimationState.SetAnimation(0, shootAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		GetComponent<Health>().invincible = false;
		if ((bool)distortionObject)
		{
			distortionObject.OnTriggerEnterEvent += DamageEnemies;
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (!initialised)
		{
			SeperateObject = true;
			randomDirection = (float)UnityEngine.Random.Range(0, 360) * ((float)Math.PI / 180f);
			state.facingAngle = randomDirection * 57.29578f;
			StartCoroutine(ActiveRoutine());
			speed = 0f;
			initialised = true;
			UIBossHUD.Play(health, ScriptLocalization.NAMES.DeathNPC);
			currentAttackRoutine = null;
			AudioManager.Instance.SetMusicRoomID(3, "deathcat_room_id");
		}
		else
		{
			GameManager.GetInstance().StartCoroutine(FrameDelay(delegate
			{
				currentAttackRoutine = null;
				AudioManager.Instance.SetMusicRoomID(3, "deathcat_room_id");
				StartCoroutine(ActiveRoutine());
			}));
		}
	}

	private IEnumerator FrameDelay(System.Action callback)
	{
		yield return new WaitForEndOfFrame();
		if (callback != null)
		{
			callback();
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		ClearPaths();
		StopAllCoroutines();
	}

	public void Attack(AttackType attackType)
	{
		switch (attackType)
		{
		case AttackType.Projectiles_Rings:
			ShootProjectileRings();
			break;
		case AttackType.Projectiles_Swirls:
			ShootProjectileSwirls();
			break;
		case AttackType.Projectiles_Bouncers:
			ShootProjectileBouncers();
			break;
		case AttackType.Projectiles_Circles:
			ShootProjectileCircles();
			break;
		case AttackType.Projectiles_Cross:
			ShootProjectileCross();
			break;
		case AttackType.Projectiles_Pulse:
			ShootProjectilePulse();
			break;
		case AttackType.Projectiles_TripleRings:
			ShootProjectileRingsTriple();
			break;
		case AttackType.Projectiles_CirclesFast:
			ShootProjectileCirclesFast();
			break;
		case AttackType.Traps_Targeted:
			TrapPatternChasePlayer();
			break;
		case AttackType.Traps_Pattern0:
			TrapPattern0();
			break;
		case AttackType.Traps_Pattern1:
			TrapPattern1();
			break;
		case AttackType.Traps_Pattern2:
			TrapPattern2();
			break;
		case AttackType.Traps_Scattered:
			TrapScattered();
			break;
		case AttackType.Teleport_Attack0:
			TeleportAttack0();
			break;
		case AttackType.Teleport_Attack1:
			TeleportAttack1();
			break;
		case AttackType.Teleport_Attack2:
			TeleportAttack2();
			break;
		}
	}

	private void Reveal()
	{
		StartCoroutine(RevealIE());
	}

	public IEnumerator RevealIE()
	{
		if (Spine.AnimationState == null)
		{
			yield return new WaitForEndOfFrame();
		}
		Spine.AnimationState.SetAnimation(0, appearAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(1f);
	}

	public void Hide()
	{
		StartCoroutine(HideIE());
	}

	private IEnumerator HideIE()
	{
		GameManager.GetInstance().RemoveFromCamera(base.gameObject);
		Spine.AnimationState.SetAnimation(0, disappearAnimation, false);
		yield return new WaitForSeconds(1f);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		AudioManager.Instance.SetMusicRoomID(0, "deathcat_room_id");
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		StopAllCoroutines();
		base.enabled = false;
		GameManager.GetInstance().StartCoroutine(DieIE());
		if ((bool)projectileCross)
		{
			GameManager.GetInstance().StartCoroutine(projectileCross.DisableProjectiles());
		}
		PlayerFarming.Instance.playerWeapon.DoSlowMo(false);
		UIBossHUD.Hide();
	}

	private IEnumerator DieIE()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 14f);
		UIBossHUD.Instance.ForceHealthAmount(0f);
		Chain5.SetActive(false);
		Chain7.SetActive(false);
		Chain9.SetActive(false);
		groundCrackSoundInstance = AudioManager.Instance.CreateLoop("event:/boss/deathcat/first_death", base.gameObject, true);
		GroundCracksSpine.AnimationState.SetAnimation(0, crackingAnimation, false);
		GroundCracksSpine.AnimationState.AddAnimation(0, crackedAnimation, true, 0f);
		Chain4Spine.AnimationState.SetAnimation(0, breakAnimation1, false);
		Chain6Spine.AnimationState.SetAnimation(0, breakAnimation2, false);
		Chain8Spine.AnimationState.SetAnimation(0, breakAnimation3, false);
		Chain4Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		Chain6Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		Chain8Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		yield return StartCoroutine(DamagedRoutineIE(false));
		DeathCatController.Instance.conversation3.Play();
		CameraManager.instance.Stopshake();
		MMVibrate.StopRumble();
		yield return new WaitForEndOfFrame();
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 14f);
		yield return StartCoroutine(TeleportOutIE());
		base.gameObject.SetActive(false);
		DeathCatController.Instance.DeathCatBigTransform();
	}

	public void SetFake()
	{
		IsFake = true;
		health.HP = 3f;
	}

	public void SetReal()
	{
		IsFake = false;
		health.HP = health.totalHP;
	}

	private void InitializeProjectilePatternRings()
	{
		int num = 9;
		if (projectilePatternRings is ProjectileCirclePattern)
		{
			ProjectileCirclePattern projectileCirclePattern = (ProjectileCirclePattern)projectilePatternRings;
			if (projectileCirclePattern.ProjectilePrefab != null)
			{
				ObjectPool.CreatePool(projectileCirclePattern.ProjectilePrefab, projectileCirclePattern.BaseProjectilesCount * num);
			}
		}
		ObjectPool.CreatePool(projectilePatternRings, num);
	}

	private void ShootProjectileRings()
	{
		currentAttackRoutine = StartCoroutine(ShootProjectileRingsIE());
	}

	private IEnumerator ShootProjectileRingsIE()
	{
		Spine.AnimationState.SetAnimation(0, shootAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(0.5f);
		for (int i = 0; i <= UnityEngine.Random.Range(3, 6); i++)
		{
			Projectile component = ObjectPool.Spawn(projectilePatternRings, base.transform.parent).GetComponent<Projectile>();
			component.transform.position = base.transform.position;
			component.Angle = GetAngleToPlayer();
			component.health = health;
			component.team = Health.Team.Team2;
			component.Speed = projectilePatternRingsSpeed;
			component.Acceleration = projectilePatternRingsAcceleration;
			component.GetComponent<ProjectileCircleBase>().InitDelayed(PlayerFarming.Instance.gameObject, projectilePatternRingsRadius * 2f, 0f);
			yield return new WaitForSeconds(0.5f);
		}
		yield return new WaitForSeconds(0.5f);
		currentAttackRoutine = null;
	}

	private void InitializeProjectileCross()
	{
		projectileCross = UnityEngine.Object.Instantiate(projectilePatternCross, base.transform.parent);
		projectileCross.gameObject.SetActive(false);
	}

	private void ShootProjectileCross()
	{
		currentAttackRoutine = StartCoroutine(ShootProjectileCrossIE());
	}

	private IEnumerator ShootProjectileCrossIE()
	{
		isMoving = false;
		yield return new WaitForSeconds(0.5f);
		Spine.AnimationState.SetAnimation(0, shootAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		projectileCross.gameObject.SetActive(true);
		projectileCross.InitDelayed();
		projectileCross.transform.position = base.transform.position;
		projectileCross.Projectile.health = health;
		projectileCross.Projectile.team = Health.Team.Team2;
		float duration = projectilePatternCrossDuration;
		float t = 0f;
		float timer = 0f;
		while (t < duration)
		{
			t += Time.deltaTime;
			timer += Time.deltaTime;
			if (timer > 2f)
			{
				timer = 0f;
				Attack(AttackType.Projectiles_Circles);
			}
			yield return null;
		}
		yield return StartCoroutine(projectileCross.DisableProjectiles());
		isMoving = true;
		currentAttackRoutine = null;
	}

	private void ShootProjectileSwirls()
	{
		Spine.AnimationState.SetAnimation(0, shootAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		currentAttackRoutine = StartCoroutine(ShootProjectileSwirlsIE());
	}

	private IEnumerator ShootProjectileSwirlsIE()
	{
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(swirls.ShootIE());
		yield return new WaitForSeconds(1f);
		currentAttackRoutine = null;
	}

	private void ShootProjectileBouncers()
	{
		Spine.AnimationState.SetAnimation(0, shootAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		currentAttackRoutine = StartCoroutine(ShootProjectileBouncersIE());
	}

	private IEnumerator ShootProjectileBouncersIE()
	{
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(bouncers.ShootIE());
		currentAttackRoutine = null;
	}

	private void ShootProjectileCircles()
	{
		Spine.AnimationState.SetAnimation(0, shootAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		StartCoroutine(ShootProjectileCirclesIE());
	}

	private IEnumerator ShootProjectileCirclesIE()
	{
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(circles.ShootIE());
		yield return new WaitForSeconds(3f);
	}

	private void ShootProjectileCirclesFast()
	{
		Spine.AnimationState.SetAnimation(0, shootAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		currentAttackRoutine = StartCoroutine(ShootProjectileCirclesFastIE());
	}

	private IEnumerator ShootProjectileCirclesFastIE()
	{
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(circlesFast.ShootIE());
		yield return new WaitForSeconds(5f);
		currentAttackRoutine = null;
	}

	private void ShootProjectilePulse()
	{
		Spine.AnimationState.SetAnimation(0, shootAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		currentAttackRoutine = StartCoroutine(ShootProjectilePulseIE());
	}

	private IEnumerator ShootProjectilePulseIE()
	{
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(pulse.ShootIE());
		currentAttackRoutine = null;
	}

	private float GetAngleToPlayer()
	{
		return Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
	}

	private void ShootProjectileRingsTriple()
	{
		currentAttackRoutine = StartCoroutine(ShootProjectileRingsTripleIE());
	}

	private IEnumerator ShootProjectileRingsTripleIE()
	{
		Spine.AnimationState.SetAnimation(0, shootAnimation, true);
		yield return new WaitForSeconds(0.5f);
		for (int i = 0; i < UnityEngine.Random.Range(5, 9); i++)
		{
			float t = (float)i / 2f;
			float angleToPlayer = GetAngleToPlayer();
			Vector3 vector = (Vector3)Utils.DegreeToVector2(angleToPlayer);
			Vector3 vector2 = Vector3.Lerp(Utils.DegreeToVector2(angleToPlayer - 90f), Utils.DegreeToVector2(angleToPlayer + 90f), t) * 1.25f;
			Projectile component = ObjectPool.Spawn(projectilePatternRings, base.transform.parent).GetComponent<Projectile>();
			component.transform.position = base.transform.position;
			component.Angle = angleToPlayer;
			component.health = health;
			component.team = Health.Team.Team2;
			component.Speed = projectilePatternRingsSpeed;
			component.Acceleration = projectilePatternRingsAcceleration;
			float shootDelay = (float)i * 0.5f;
			component.GetComponent<ProjectileCircleBase>().InitDelayed(PlayerFarming.Instance.gameObject, projectilePatternRingsRadius, shootDelay, delegate
			{
				AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/grenade_mass_launch", base.gameObject);
			});
		}
		yield return new WaitForSeconds(projectilePatternRingsDuration + 3f);
		Spine.AnimationState.SetAnimation(0, idleAnimation, true);
		currentAttackRoutine = null;
	}

	public void TrapPatternChasePlayer()
	{
		currentAttackRoutine = StartCoroutine(TrapPatternChasePlayerIE());
	}

	private void InitializeTraps()
	{
		ObjectPool.CreatePool(trapPrefab, 40);
	}

	private IEnumerator TrapPatternChasePlayerIE()
	{
		Spine.AnimationState.SetAnimation(0, shootAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		Vector3 Position = Vector3.zero;
		int i = -1;
		float Dist = 1f;
		state.facingAngle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
		float facingAngle = state.facingAngle;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= 20)
			{
				break;
			}
			GameObject obj = ObjectPool.Spawn(trapPrefab);
			float angle = Utils.GetAngle(base.transform.position + Position, PlayerFarming.Instance.transform.position);
			Position += new Vector3(Dist * Mathf.Cos(angle * ((float)Math.PI / 180f)), Dist * Mathf.Sin(angle * ((float)Math.PI / 180f)));
			obj.transform.position = base.transform.position + Position;
			CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
			yield return new WaitForSeconds(0.2f);
		}
		yield return new WaitForSeconds(1f);
		currentAttackRoutine = null;
	}

	public void TrapPattern0()
	{
		currentAttackRoutine = StartCoroutine(TrapPattern0IE());
	}

	private IEnumerator TrapPattern0IE()
	{
		Spine.AnimationState.SetAnimation(0, shootAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		state.facingAngle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
		int r = UnityEngine.Random.Range(3, 6);
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= 10)
			{
				break;
			}
			int num2 = -1;
			while (++num2 < r)
			{
				GameObject obj = ObjectPool.Spawn(trapPrefab);
				float f = (state.facingAngle + (float)(90 * num2)) * ((float)Math.PI / 180f);
				float num3 = i * 2;
				Vector3 vector = new Vector3(num3 * Mathf.Cos(f), num3 * Mathf.Sin(f));
				obj.transform.position = base.transform.position + vector;
			}
			CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
			yield return new WaitForSeconds(0.2f);
		}
		yield return new WaitForSeconds(1f);
		currentAttackRoutine = null;
	}

	public void TrapPattern1()
	{
		currentAttackRoutine = StartCoroutine(TrapPattern1IE());
	}

	private IEnumerator TrapPattern1IE()
	{
		isMoving = false;
		Spine.AnimationState.SetAnimation(0, shootAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		state.facingAngle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= 10)
			{
				break;
			}
			GameObject obj = ObjectPool.Spawn(trapPrefab);
			float f = (state.facingAngle + (float)(36 * i)) * ((float)Math.PI / 180f);
			float num2 = 3f;
			Vector3 vector = new Vector3(num2 * Mathf.Cos(f), num2 * Mathf.Sin(f));
			obj.transform.position = base.transform.position + vector;
			CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
			yield return new WaitForSeconds(0.02f);
		}
		yield return new WaitForSeconds(0.5f);
		isMoving = true;
		currentAttackRoutine = null;
	}

	public void TrapPattern2()
	{
		currentAttackRoutine = StartCoroutine(TrapPattern2IE());
	}

	private IEnumerator TrapPattern2IE()
	{
		isMoving = false;
		Spine.AnimationState.SetAnimation(0, shootAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		state.facingAngle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
		float startingAngle = state.facingAngle;
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= 10)
			{
				break;
			}
			int num2 = -1;
			while (++num2 < UnityEngine.Random.Range(4, 7))
			{
				GameObject obj = ObjectPool.Spawn(trapPrefab);
				float num3 = (startingAngle + (float)(90 * num2)) * ((float)Math.PI / 180f);
				float num4 = i * 2;
				num3 += (float)i * 0.1f;
				Vector3 vector = new Vector3(num4 * Mathf.Cos(num3), num4 * Mathf.Sin(num3));
				obj.transform.position = base.transform.position + vector;
			}
			CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(1f);
		isMoving = true;
		currentAttackRoutine = null;
	}

	public void TrapScattered()
	{
		currentAttackRoutine = StartCoroutine(TrapScatteredIE());
	}

	private IEnumerator TrapScatteredIE(int c = 0)
	{
		isMoving = false;
		Spine.AnimationState.SetAnimation(0, shootAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		state.facingAngle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
		List<Vector3> spawnedTrapsPositions = new List<Vector3>();
		int rand = UnityEngine.Random.Range(30, 40);
		for (int i = 0; i < UnityEngine.Random.Range(30, 40); i++)
		{
			Vector3 vector = Vector3.zero;
			int num = 0;
			while (num++ < 30)
			{
				vector = new Vector3(UnityEngine.Random.Range(-11f, 11f), UnityEngine.Random.Range(-7f, 7f), 0f);
				bool flag = false;
				foreach (Vector3 item in spawnedTrapsPositions)
				{
					if (Vector3.Distance(item, vector) < 1.5f)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					break;
				}
			}
			if (rand == i)
			{
				vector = PlayerFarming.Instance.transform.position;
			}
			spawnedTrapsPositions.Add(vector);
			ObjectPool.Spawn(trapPrefab).transform.position = vector;
			yield return new WaitForSeconds(0.05f);
		}
		if (c < 3 && UnityEngine.Random.Range(0, 2) == 0)
		{
			currentAttackRoutine = StartCoroutine(TrapScatteredIE(c + 1));
			yield break;
		}
		yield return new WaitForSeconds(1f);
		isMoving = true;
		currentAttackRoutine = null;
	}

	public void TeleportAttack0()
	{
		currentAttackRoutine = StartCoroutine(TeleportAttack0IE());
	}

	private IEnumerator TeleportAttack0IE()
	{
		yield return StartCoroutine(TeleportToPositionIE(GetPositionAwayFromPlayer()));
		isMoving = false;
		Spine.AnimationState.SetAnimation(0, shootAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(0.25f);
		int j = -1;
		while (true)
		{
			int num = j + 1;
			j = num;
			if (num >= UnityEngine.Random.Range(2, 4))
			{
				break;
			}
			state.facingAngle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
			float Angle = state.facingAngle * ((float)Math.PI / 180f);
			int i = -1;
			while (true)
			{
				num = i + 1;
				i = num;
				if (num >= 15)
				{
					break;
				}
				GameObject obj = ObjectPool.Spawn(trapPrefab);
				float num2 = i * 2;
				Vector3 vector = new Vector3(num2 * Mathf.Cos(Angle), num2 * Mathf.Sin(Angle));
				obj.transform.position = base.transform.position + vector;
				CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
				yield return new WaitForSeconds(0.05f);
			}
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(0.5f);
		isMoving = true;
		currentAttackRoutine = null;
	}

	public void TeleportAttack1()
	{
		currentAttackRoutine = StartCoroutine(TeleportAttack1IE());
	}

	private IEnumerator TeleportAttack1IE()
	{
		yield return StartCoroutine(TeleportToPositionIE(GetPositionAwayFromPlayer()));
		isMoving = false;
		yield return currentAttackRoutine = StartCoroutine(ShootProjectileRingsIE());
		isMoving = true;
		currentAttackRoutine = null;
	}

	public void TeleportAttack2()
	{
		currentAttackRoutine = StartCoroutine(TeleportAttack12E());
	}

	private IEnumerator TeleportAttack12E()
	{
		yield return StartCoroutine(TeleportToPositionIE(GetPositionAwayFromPlayer()));
		isMoving = false;
		yield return currentAttackRoutine = StartCoroutine(ShootProjectileRingsTripleIE());
		isMoving = true;
		currentAttackRoutine = null;
	}

	public void TeleportRandomly()
	{
		currentAttackRoutine = StartCoroutine(TeleportRandomlyIE());
	}

	private IEnumerator TeleportRandomlyIE(float endDelay = 1f)
	{
		yield return StartCoroutine(TeleportToPositionIE(GetRandomPosition(), endDelay));
		currentAttackRoutine = null;
	}

	private IEnumerator TeleportToPositionIE(Vector3 position, float endDelay = 0f)
	{
		yield return StartCoroutine(TeleportOutIE());
		base.transform.position = position;
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(TeleportInIE());
		yield return new WaitForSeconds(endDelay);
	}

	private IEnumerator TeleportOutIE()
	{
		Spine.AnimationState.SetAnimation(0, disappearAnimation, false);
		health.invincible = true;
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/teleport_away", base.gameObject);
		yield return new WaitForSeconds(0.5f);
	}

	private IEnumerator TeleportInIE()
	{
		Spine.AnimationState.SetAnimation(0, appearAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/teleport_return", base.gameObject);
		yield return new WaitForSeconds(0.5f);
		health.invincible = false;
	}

	private Vector3 GetRandomPosition()
	{
		Vector3 vector;
		do
		{
			vector = UnityEngine.Random.insideUnitCircle * 7f;
		}
		while (!(Vector3.Distance(base.transform.position, vector) > 3f));
		return vector;
	}

	private Vector3 GetPositionAwayFromPlayer()
	{
		List<RaycastHit2D> list = new List<RaycastHit2D>();
		list.Add(Physics2D.Raycast(base.transform.position, new Vector2(1f, 1f), 100f, layerToCheck));
		list.Add(Physics2D.Raycast(base.transform.position, new Vector2(-1f, 1f), 100f, layerToCheck));
		list.Add(Physics2D.Raycast(base.transform.position, new Vector2(-0.5f, -0.5f), 100f, layerToCheck));
		list.Add(Physics2D.Raycast(base.transform.position, new Vector2(0.5f, -0.5f), 100f, layerToCheck));
		RaycastHit2D item = list[0];
		for (int i = 1; i < list.Count; i++)
		{
			if (PlayerFarming.Instance != null)
			{
				float num = Vector3.Distance(PlayerFarming.Instance.transform.position, list[i].point);
				float num2 = Vector3.Distance(PlayerFarming.Instance.transform.position, item.point);
				if (num > num2 && Vector3.Distance(base.transform.position, list[i].point) > 3.5f)
				{
					item = list[i];
				}
			}
		}
		if (Vector3.Distance(base.transform.position, item.point) <= 3.5f)
		{
			list.Remove(item);
			item = list[UnityEngine.Random.Range(0, list.Count)];
		}
		return item.point + ((Vector2)base.transform.position - item.point).normalized * 3f;
	}

	public override void Update()
	{
		if (UsePathing)
		{
			if (pathToFollow == null)
			{
				speed += (0f - speed) / (4f * acceleration) * GameManager.DeltaTime;
				if (isMoving)
				{
					move();
				}
				return;
			}
			if (currentWaypoint >= pathToFollow.Count)
			{
				speed += (0f - speed) / (4f * acceleration) * GameManager.DeltaTime;
				if (isMoving)
				{
					move();
				}
				return;
			}
		}
		if (state.CURRENT_STATE == StateMachine.State.Moving || state.CURRENT_STATE == StateMachine.State.Fleeing)
		{
			speed += (maxSpeed * SpeedMultiplier - speed) / (7f * acceleration) * GameManager.DeltaTime;
			if (UsePathing)
			{
				state.facingAngle = Mathf.LerpAngle(state.facingAngle, Utils.GetAngle(base.transform.position, pathToFollow[currentWaypoint]), Time.deltaTime * turningSpeed);
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
			speed += (0f - speed) / (4f * acceleration) * GameManager.DeltaTime;
		}
		DisableForces = !isMoving;
		move();
		if (currentAttackRoutine == null && attackTimestamp == -1f)
		{
			attackTimestamp = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);
			if (phase > 1)
			{
				attackTimestamp -= UnityEngine.Random.Range(0.5f, 1.5f);
			}
		}
		else if (currentAttackRoutine != null)
		{
			attackTimestamp = -1f;
		}
		if (manualAttacking && GameManager.GetInstance().CurrentTime > attackTimestamp && attackTimestamp != -1f && currentAttackRoutine == null)
		{
			RandomAttack();
		}
		if (SpawnEnemies && GameManager.GetInstance().CurrentTime > spawnEnemyTimestamp && Health.team2.Count < maxEnemies + 1 && isSpawning)
		{
			SpawnRandomEnemy();
			spawnEnemyTimestamp = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(timeBetweenEnemies.x, timeBetweenEnemies.y);
		}
		if (phase == 1 && health.HP / health.totalHP < 0.5f && currentAttackRoutine == null)
		{
			phase = 2;
			DamagedRoutine(true);
			maxEnemies = Mathf.RoundToInt((float)maxEnemies * 1.25f);
		}
	}

	private void SpawnRandomEnemy()
	{
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(spawningEnemiesList[UnityEngine.Random.Range(0, spawningEnemiesList.Length)]);
		UnitObject enemy;
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
		{
			loadedAddressableAssets.Add(obj);
			enemy = ObjectPool.Spawn(obj.Result, base.transform.parent, GetRandomPosition(), Quaternion.identity).GetComponent<UnitObject>();
			enemy.gameObject.SetActive(false);
			EnemySpawner.CreateWithAndInitInstantiatedEnemy(enemy.transform.position, base.transform.parent, enemy.gameObject);
			enemy.GetComponent<UnitObject>().CanHaveModifier = false;
			enemy.GetComponent<UnitObject>().RemoveModifier();
			DropLootOnDeath component = enemy.GetComponent<DropLootOnDeath>();
			if ((bool)component)
			{
				component.GiveXP = false;
			}
		};
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		SimpleSpineFlash.FlashFillRed();
	}

	private void RandomAttack()
	{
		int num = UnityEngine.Random.Range(0, 2);
		if (count <= 1)
		{
			count++;
			switch (num)
			{
			case 0:
				Attack(AttackType.Teleport_Attack0);
				break;
			case 1:
				if (phase == 1)
				{
					Attack(AttackType.Teleport_Attack1);
				}
				else
				{
					Attack(AttackType.Teleport_Attack2);
				}
				break;
			}
			return;
		}
		if (count == 2)
		{
			count++;
			TeleportRandomly();
			return;
		}
		count = 0;
		switch (num)
		{
		case 0:
			if (UnityEngine.Random.Range(0, 2) == 0)
			{
				Attack(AttackType.Traps_Scattered);
			}
			else if (phase == 1)
			{
				Attack(AttackType.Projectiles_Swirls);
			}
			else
			{
				Attack(AttackType.Projectiles_CirclesFast);
			}
			break;
		case 1:
			if (UnityEngine.Random.Range(0, 2) == 0)
			{
				Attack(AttackType.Traps_Pattern1);
			}
			else
			{
				Attack(AttackType.Traps_Pattern2);
			}
			break;
		}
	}

	private void DamagedRoutine(bool teleportAfter)
	{
		currentAttackRoutine = StartCoroutine(DamagedRoutineIE(teleportAfter));
	}

	private IEnumerator DamagedRoutineIE(bool teleportAfter)
	{
		isSpawning = false;
		isMoving = false;
		Spine.AnimationState.SetAnimation(0, hurtAnimation, false);
		Spine.AnimationState.AddAnimation(0, hurtLoopAnimation, true, 0f);
		distortionObject.SetActive(true);
		distortionObject.transform.DOScale(25f, 3.5f).SetEase(Ease.Linear).OnComplete(delegate
		{
			distortionObject.transform.localScale = Vector3.zero;
			distortionObject.SetActive(false);
		});
		yield return new WaitForSeconds(1f);
		Chain5Spine.AnimationState.SetAnimation(0, breakAnimation1, false);
		Chain7Spine.AnimationState.SetAnimation(0, breakAnimation2, false);
		Chain9Spine.AnimationState.SetAnimation(0, breakAnimation3, false);
		Chain5Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		Chain7Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		Chain9Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		if (!teleportAfter)
		{
			MMVibrate.RumbleContinuous(0.5f, 0.75f);
			CameraManager.instance.ShakeCameraForDuration(0.4f, 0.6f, float.MaxValue);
		}
		yield return new WaitForSeconds(2f);
		FaithAmmo.Reload();
		Spine.AnimationState.SetAnimation(0, stopHurtAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		currentAttackRoutine = null;
		if (teleportAfter)
		{
			currentAttackRoutine = StartCoroutine(TeleportToPositionIE(Vector3.zero));
			yield return currentAttackRoutine;
			Attack(AttackType.Projectiles_Cross);
			isSpawning = true;
		}
	}

	private void DamageEnemies(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && component.team != Health.Team.PlayerTeam && component != health)
		{
			component.ImpactOnHit = false;
			component.ScreenshakeOnDie = false;
			component.ScreenshakeOnHit = false;
			component.invincible = false;
			component.untouchable = false;
			component.DealDamage(component.totalHP, base.gameObject, base.transform.position, false, Health.AttackTypes.Poison);
		}
	}

	private IEnumerator ActiveRoutine()
	{
		yield return new WaitForSeconds(0.2f);
		Debug.Log("ActiveRoutine");
		while (true)
		{
			if (!GameManager.GetInstance().CamFollowTarget.Contains(base.gameObject) && PlayerFarming.Instance != null && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.Dieing && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.GameOver)
			{
				GameManager.GetInstance().AddToCamera(base.gameObject);
				GameManager.GetInstance().CamFollowTarget.MinZoom = 9f;
				GameManager.GetInstance().CamFollowTarget.MaxZoom = 18f;
				continue;
			}
			if (state.CURRENT_STATE == StateMachine.State.Idle)
			{
				IdleWait -= Time.deltaTime;
				float num = 0f;
			}
			GetNewTargetPosition();
			if (targetObject == null && Time.frameCount % 10 == 0)
			{
				GetNewTarget();
			}
			yield return null;
		}
	}

	public void GetNewTargetPosition()
	{
		if (AstarPath.active == null)
		{
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
			IdleWait = UnityEngine.Random.Range(idleWaitRange.x, idleWaitRange.y);
			givePath(vector);
			break;
		}
	}

	public void GetNewTarget()
	{
		Health health = null;
		float num = float.MaxValue;
		foreach (Health allUnit in Health.allUnits)
		{
			if (allUnit.team != base.health.team && !allUnit.InanimateObject && allUnit.team != 0 && (base.health.team != Health.Team.PlayerTeam || (base.health.team == Health.Team.PlayerTeam && allUnit.team != Health.Team.DangerousAnimals)) && Vector2.Distance(base.transform.position, allUnit.gameObject.transform.position) < (float)DetectEnemyRange && CheckLineOfSight(allUnit.gameObject.transform.position, Vector2.Distance(allUnit.gameObject.transform.position, base.transform.position)))
			{
				float num2 = Vector3.Distance(base.transform.position, allUnit.gameObject.transform.position);
				if (num2 < num)
				{
					health = allUnit;
					num = num2;
				}
			}
		}
		if (health != null)
		{
			targetObject = health.gameObject;
		}
		else
		{
			targetObject = PlayerFarming.Instance.gameObject;
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if ((int)layerToCheck == ((int)bounceMask | (1 << collision.gameObject.layer)))
		{
			if (speed < maxSpeed)
			{
				speed *= 1.2f;
			}
			state.CURRENT_STATE = StateMachine.State.Idle;
			IdleWait = UnityEngine.Random.Range(idleWaitRange.x, idleWaitRange.y);
			state.facingAngle = Utils.GetAngle((Vector2)base.transform.position, (Vector2)base.transform.position + Vector2.Reflect(Utils.DegreeToVector2(state.facingAngle), collision.contacts[0].normal));
		}
	}
}
