using System;
using System.Collections;
using System.Collections.Generic;
using CotL.Projectiles;
using FMODUnity;
using I2.Loc;
using MMTools;
using UnityEngine;

public class EnemyGuardian2 : UnitObject
{
	public AudioClip CombatMusic;

	public Interaction_MonsterHeart interaction_MonsterHeart;

	public BreakTempleChain BreakTempleChain;

	public Goat_GuardianDoor Goat_GuardianDoor;

	private SimpleSpineAnimator simpleSpineAnimator;

	private SimpleSpineEventListener simpleSpineEventListener;

	private Vector3 CentreOfLevel = Vector3.zero;

	public GameObject CenterOfLevelObject;

	private GameObject TargetObject;

	public ColliderEvents damageColliderEvents;

	private Health EnemyHealth;

	public GameObject[] EnemyToSpawn;

	public Transform CameraBone;

	public ParticleSystem Particles;

	private Collider2D HealthCollider;

	[SerializeField]
	private ProjectileCircleBase projectilePatternRings;

	[SerializeField]
	private float projectilePatternRingsSpeed;

	[SerializeField]
	private float projectilePatternRingsAcceleration;

	[SerializeField]
	private float projectilePatternRingsRadius;

	[SerializeField]
	private float projectilePatternRingsTimeBetween;

	[SerializeField]
	private float projectilePatternShootDelay;

	[SerializeField]
	private Vector2 projectilePatternRingsAmount;

	[SerializeField]
	private float projectilePatternScattedSpeed;

	[SerializeField]
	private float projectilePatternScattedAcceleration;

	[SerializeField]
	private float projectileScatteredRadius;

	[SerializeField]
	private Vector2 projectilePatternScatteredAmount;

	[SerializeField]
	private Vector2 projectilePatternScatteredDelay;

	[TermsPopup("")]
	public string DisplayName;

	[EventRef]
	public string attackSoundPath = string.Empty;

	[EventRef]
	public string onHitSoundPath = string.Empty;

	private bool active;

	private int enemiesAlive;

	private const int maxEnemies = 13;

	public DeadBodySliding deadBodySliding;

	public GameObject Trap;

	private int TrapPattern;

	private int TargetEnemies = 3;

	private int NumWaves = 2;

	private List<GameObject> enemies = new List<GameObject>();

	public LineRenderer lineRenderer;

	private Vector3 TargetPosition
	{
		get
		{
			return TargetObject.transform.position;
		}
	}

	public void Activate()
	{
		StartCoroutine(ActivateIE());
	}

	public override void Awake()
	{
		base.Awake();
		InitializeTraps();
		InitializeProjectilePatternRings();
	}

	private IEnumerator ActivateIE()
	{
		health.BlackSoulOnHit = true;
		if ((bool)DeathCatController.Instance)
		{
			DeathCatController.Instance.conversation1.Play();
		}
		yield return new WaitForEndOfFrame();
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		TargetObject = GameObject.FindWithTag("Player");
		HUD_DisplayName.Play(LocalizationManager.GetTranslation(DisplayName), 2, HUD_DisplayName.Positions.Centre, HUD_DisplayName.textBlendMode.DungeonFinal);
		CentreOfLevel = CenterOfLevelObject.transform.position;
		simpleSpineAnimator = GetComponentInChildren<SimpleSpineAnimator>();
		simpleSpineEventListener = GetComponent<SimpleSpineEventListener>();
		simpleSpineEventListener.OnSpineEvent += OnSpineEvent;
		Particles.Stop();
		HealthCollider = GetComponent<Collider2D>();
		HealthCollider.enabled = false;
		health.invincible = true;
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEvents.SetActive(false);
		}
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(CameraBone.gameObject, 6f);
		simpleSpineAnimator.Animate("intro2", 0, false);
		simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		AudioManager.Instance.SetMusicRoomID(2, "deathcat_room_id");
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (active)
		{
			health.invincible = false;
			HealthCollider.enabled = true;
			StartCoroutine(SpawnTraps());
			UIBossHUD.Play(health, LocalizationManager.GetTranslation(DisplayName));
			AudioManager.Instance.SetMusicRoomID(2, "deathcat_room_id");
		}
	}

	private void OnSpineEvent(string EventName)
	{
		switch (EventName)
		{
		case "Intro Complete":
			Debug.Log("INTRO COMPLETE?");
			health.invincible = false;
			HealthCollider.enabled = true;
			if (UnityEngine.Random.Range(0, 2) == 0)
			{
				StartCoroutine(ShootRingsScattered());
			}
			else
			{
				StartCoroutine(ShootRingsTargeted());
			}
			GameManager.GetInstance().OnConversationEnd();
			active = true;
			UIBossHUD.Play(health, LocalizationManager.GetTranslation(DisplayName));
			GameManager.GetInstance().AddToCamera(base.gameObject);
			break;
		case "Start Particles":
			CameraManager.instance.ShakeCameraForDuration(0.1f, 0.4f, 2.5f);
			Particles.Play();
			break;
		case "Stop Particles":
			Particles.Stop();
			break;
		case "Invincible Off":
			break;
		}
	}

	private IEnumerator Die()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(CameraBone.gameObject, 7f);
		state.CURRENT_STATE = StateMachine.State.Dead;
		yield return new WaitForEndOfFrame();
		simpleSpineAnimator.Animate("dead", 0, false);
		yield return new WaitForSeconds(4f);
		if (UnityEngine.Random.value < 0.33f)
		{
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLUE_HEART, 1, base.transform.position + Vector3.back);
		}
		else
		{
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.RED_HEART, 1, base.transform.position + Vector3.back);
		}
		BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position);
		DeathCatController.Instance.DeathCatCloneTransform();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (!string.IsNullOrEmpty(onHitSoundPath))
		{
			AudioManager.Instance.PlayOneShot(onHitSoundPath, base.transform.position);
		}
		CameraManager.shakeCamera(0.5f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		simpleSpineAnimator.FlashFillRed();
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		AudioManager.Instance.SetMusicRoomID(0, "deathcat_room_id");
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		simpleSpineAnimator.FlashWhite(false);
		StopAllCoroutines();
		StartCoroutine(Die());
		HealthCollider.enabled = false;
		GameManager.GetInstance().RemoveFromCamera(base.gameObject);
		UIBossHUD.Hide();
		foreach (GameObject enemy in enemies)
		{
			if ((bool)enemy)
			{
				Health component = enemy.GetComponent<Health>();
				component.enabled = true;
				component.invincible = false;
				component.DealDamage(float.MaxValue, component.gameObject, component.transform.position);
			}
		}
	}

	private IEnumerator DoHurt()
	{
		simpleSpineAnimator.Animate("Coughing", 0, true);
		yield return new WaitForSeconds(2f);
		simpleSpineAnimator.Animate("idle", 0, true);
		GameManager.GetInstance().RemoveFromCamera(CameraBone.gameObject);
		StartCoroutine(TeleportAway());
	}

	private IEnumerator Teleport(Vector3 Destination)
	{
		simpleSpineAnimator.Animate("dash", 0, false);
		simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		yield return new WaitForSeconds(0.15f);
		CameraManager.shakeCamera(0.5f, Utils.GetAngle(base.transform.position, Vector3.zero));
		BiomeConstants.Instance.SpawnInWhite.Spawn().transform.position = base.transform.position + Vector3.down * 2f;
		base.transform.position = Destination;
		BiomeConstants.Instance.SpawnInWhite.Spawn().transform.position = base.transform.position + Vector3.down * 2f;
		state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
	}

	private IEnumerator TeleportAway()
	{
		float f = (Utils.GetAngle(TargetObject.transform.position, CentreOfLevel) + (float)UnityEngine.Random.Range(-90, 90)) * ((float)Math.PI / 180f);
		float num = 4f;
		Vector3 destination = CentreOfLevel + new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
		yield return StartCoroutine(Teleport(destination));
		StartCoroutine(SpawnTraps());
	}

	private IEnumerator SpawnTraps(int c = 0)
	{
		simpleSpineAnimator.Animate("summon-fast", 0, false);
		simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
		yield return new WaitForSeconds(0.66f);
		for (int i = 0; i < UnityEngine.Random.Range(3, 6); i++)
		{
			yield return new WaitForSeconds(0.5f);
			StartCoroutine(TrapPattern0());
		}
		yield return new WaitForSeconds(2f);
		if (c == 0 && UnityEngine.Random.Range(0, 2) == 0)
		{
			StartCoroutine(SpawnTraps(1));
		}
		else
		{
			StartCoroutine(TeleportAndShoot());
		}
	}

	private void InitializeProjectilePatternRings()
	{
		int num = Mathf.Max((int)projectilePatternScatteredAmount.y, (int)projectilePatternRingsAmount.y);
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

	private IEnumerator ShootRingsTargeted()
	{
		yield return StartCoroutine(Teleport(CentreOfLevel));
		simpleSpineAnimator.Animate("floating-start", 0, true);
		simpleSpineAnimator.AddAnimate("floating-spin", 0, true, 0f);
		yield return new WaitForSeconds(0.5f);
		yield return new WaitForSeconds(0.5f);
		for (int i = 0; (float)i < UnityEngine.Random.Range(projectilePatternRingsAmount.x, projectilePatternRingsAmount.y); i++)
		{
			Projectile component = ObjectPool.Spawn(projectilePatternRings, base.transform.parent).GetComponent<Projectile>();
			component.transform.position = base.transform.position;
			component.Angle = GetAngleToPlayer();
			component.health = health;
			component.team = Health.Team.Team2;
			component.Speed = projectilePatternRingsSpeed;
			component.Acceleration = projectilePatternRingsAcceleration;
			component.GetComponent<ProjectileCircleBase>().InitDelayed(PlayerFarming.Instance.gameObject, projectilePatternRingsRadius, projectilePatternShootDelay, delegate
			{
				AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/grenade_mass_launch", base.gameObject);
			});
			yield return new WaitForSeconds(projectilePatternRingsTimeBetween);
		}
		simpleSpineAnimator.Animate("floating-stop", 0, false);
		simpleSpineAnimator.AddAnimate("Coughing", 0, true, 0f);
		yield return new WaitForSeconds(0.5f);
		yield return new WaitForSeconds(0.5f);
		yield return new WaitForSeconds(1.5f);
		StartCoroutine(TeleportAway());
	}

	private IEnumerator ShootRingsScattered()
	{
		yield return StartCoroutine(Teleport(CentreOfLevel));
		simpleSpineAnimator.Animate("floating-start", 0, true);
		simpleSpineAnimator.AddAnimate("floating-spin", 0, true, 0f);
		yield return new WaitForSeconds(0.5f);
		yield return new WaitForSeconds(0.5f);
		for (int i = 0; (float)i < UnityEngine.Random.Range(projectilePatternScatteredAmount.x, projectilePatternScatteredAmount.y); i++)
		{
			Projectile component = ObjectPool.Spawn(projectilePatternRings, base.transform.parent).GetComponent<Projectile>();
			component.transform.position = base.transform.position;
			component.Angle = UnityEngine.Random.Range(0f, 360f);
			component.health = health;
			component.team = Health.Team.Team2;
			component.Speed = projectilePatternScattedSpeed;
			component.Deceleration = UnityEngine.Random.Range(2f, 7f);
			component.GetComponent<ProjectileCircleBase>().InitDelayed((i % 4 == 0) ? PlayerFarming.Instance.gameObject : null, projectileScatteredRadius, 0f, delegate
			{
				AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/grenade_mass_launch", base.gameObject);
			});
			yield return new WaitForSeconds(UnityEngine.Random.Range(projectilePatternScatteredDelay.x, projectilePatternScatteredDelay.y));
		}
		yield return new WaitForSeconds(projectilePatternScatteredDelay.y + 0.5f);
		simpleSpineAnimator.Animate("floating-stop", 0, false);
		simpleSpineAnimator.AddAnimate("Coughing", 0, true, 0f);
		yield return new WaitForSeconds(0.5f);
		yield return new WaitForSeconds(0.5f);
		yield return new WaitForSeconds(1.5f);
		StartCoroutine(TeleportAway());
	}

	private float GetAngleToPlayer()
	{
		if (!(PlayerFarming.Instance != null))
		{
			return 0f;
		}
		return Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
	}

	private IEnumerator TeleportAndShoot()
	{
		int NumAttacks = 2;
		bool Loop = true;
		while (Loop)
		{
			float num = UnityEngine.Random.Range(0, 360);
			float num2 = 6f;
			int num3 = 0;
			while (num3++ < 32 && (bool)Physics2D.Raycast(base.transform.position, Utils.DegreeToVector2(num), num2, layerToCheck))
			{
				num += Mathf.Repeat(UnityEngine.Random.Range(0, 360), 360f);
			}
			num *= (float)Math.PI / 180f;
			Vector3 destination = base.transform.position + new Vector3(num2 * Mathf.Cos(num), num2 * Mathf.Sin(num));
			yield return StartCoroutine(Teleport(destination));
			yield return new WaitForSeconds(0.5f);
			simpleSpineAnimator.Animate("summon-fast", 0, false);
			simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
			yield return StartCoroutine(TrapPatternChasePlayer(UnityEngine.Random.Range(8, 15)));
			yield return new WaitForSeconds(1f);
			int num4 = NumAttacks - 1;
			NumAttacks = num4;
			if (num4 <= 0)
			{
				Loop = false;
			}
			yield return null;
		}
		if (UnityEngine.Random.Range(0, 2) == 0)
		{
			StartCoroutine(ShootRingsScattered());
		}
		else
		{
			StartCoroutine(ShootRingsTargeted());
		}
	}

	private IEnumerator SpawnEnemies()
	{
		yield return StartCoroutine(Teleport(CentreOfLevel));
		yield return new WaitForSeconds(1f);
		simpleSpineAnimator.Animate("floating-start", 0, true);
		simpleSpineAnimator.AddAnimate("floating-spin", 0, true, 0f);
		yield return new WaitForSeconds(1f);
		bool Loop = true;
		int WaveCount = NumWaves;
		while (Loop)
		{
			int Count = -1;
			int num;
			while (true)
			{
				num = Count + 1;
				Count = num;
				if (num >= TargetEnemies || enemiesAlive >= 13)
				{
					break;
				}
				int num2 = 5;
				Vector3 vector = base.transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * num2);
				GameObject gameObject = EnemySpawner.Create(vector, base.transform.parent, EnemyToSpawn[UnityEngine.Random.Range(0, EnemyToSpawn.Length)]);
				enemies.Add(gameObject);
				gameObject.GetComponent<UnitObject>().CanHaveModifier = false;
				gameObject.GetComponent<UnitObject>().RemoveModifier();
				gameObject.GetComponent<Health>().OnDie += EnemyDied;
				enemiesAlive++;
				CameraManager.shakeCamera(0.3f, Utils.GetAngle(base.transform.position, base.transform.position + vector));
				yield return new WaitForSeconds(0.2f);
			}
			yield return new WaitForSeconds(0.5f);
			GameManager.GetInstance().RemoveFromCamera(base.gameObject);
			simpleSpineAnimator.Animate("floating", 0, true);
			yield return new WaitForSeconds(3f);
			num = WaveCount - 1;
			WaveCount = num;
			if (num <= 0)
			{
				Loop = false;
			}
			else
			{
				simpleSpineAnimator.Animate("floating-spin", 0, true);
			}
		}
		if (NumWaves < 3)
		{
			NumWaves++;
		}
		if (TargetEnemies < 6)
		{
			TargetEnemies++;
		}
		float BatTimer = 0f;
		while (true)
		{
			float num3;
			BatTimer = (num3 = BatTimer + Time.deltaTime);
			if (!(num3 < 5f) || FormationFighter.fighters.Count <= 0)
			{
				break;
			}
			yield return null;
		}
		simpleSpineAnimator.Animate("floating-stop", 0, false);
		simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		yield return new WaitForSeconds(1f);
		yield return new WaitForSeconds(2f);
		StartCoroutine(TeleportAway());
	}

	private void EnemyDied(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		enemiesAlive--;
	}

	private void InitializeTraps()
	{
		ObjectPool.CreatePool(Trap, 40);
	}

	private IEnumerator TrapPattern0()
	{
		state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
		float Angle = state.facingAngle * ((float)Math.PI / 180f);
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num < 10)
			{
				GameObject obj = ObjectPool.Spawn(Trap);
				float num2 = i * 2;
				Vector3 vector = new Vector3(num2 * Mathf.Cos(Angle), num2 * Mathf.Sin(Angle));
				obj.transform.position = base.transform.position + vector;
				CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
				yield return new WaitForSeconds(0.1f);
				continue;
			}
			break;
		}
	}

	private IEnumerator TrapPatternChasePlayer(int NumToSpawn)
	{
		Vector3 Position = Vector3.zero;
		int i = -1;
		float Dist = 1f;
		state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
		float facingAngle = state.facingAngle;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num < NumToSpawn)
			{
				GameObject obj = ObjectPool.Spawn(Trap);
				float angle = Utils.GetAngle(base.transform.position + Position, TargetObject.transform.position);
				Position += new Vector3(Dist * Mathf.Cos(angle * ((float)Math.PI / 180f)), Dist * Mathf.Sin(angle * ((float)Math.PI / 180f)));
				obj.transform.position = base.transform.position + Position;
				CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
				yield return new WaitForSeconds(0.15f);
				continue;
			}
			break;
		}
	}

	private IEnumerator ShowLineRenderer(Vector3 Destination)
	{
		float Progress = 1f;
		Color c = Color.white;
		Gradient gradient = new Gradient();
		while (true)
		{
			float num;
			Progress = (num = Progress - Time.deltaTime);
			if (num >= 0f)
			{
				if (Progress < 0f)
				{
					Progress = 0f;
				}
				gradient.SetKeys(new GradientColorKey[2]
				{
					new GradientColorKey(c, 0f),
					new GradientColorKey(c, 1f)
				}, new GradientAlphaKey[2]
				{
					new GradientAlphaKey(Progress, 0f),
					new GradientAlphaKey(Progress, 1f)
				});
				yield return null;
				continue;
			}
			break;
		}
	}

	private void OnDamageTriggerEnter(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && component.team != health.team)
		{
			component.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.7f));
		}
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(CentreOfLevel, 0.4f, Color.blue);
	}
}
