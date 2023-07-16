using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ara;
using FMOD.Studio;
using FMODUnity;
using MMBiomeGeneration;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Projectile : BaseMonoBehaviour
{
	public class CollisionEvent
	{
		public float UnscaledTimestamp;

		public Health TargetHealth;

		public CollisionEvent(float unscaledTimestamp, Health targetHealth)
		{
			UnscaledTimestamp = unscaledTimestamp;
			TargetHealth = targetHealth;
		}
	}

	public static List<Projectile> Projectiles = new List<Projectile>();

	public Transform ArrowImage;

	public Transform ArrowImageEmission;

	public bool IsProjectilesParent;

	public bool followDirection;

	public float Damage = 1f;

	public float Speed;

	public float SpeedMultiplier = 1f;

	public float Angle;

	public float LifeTime = 1f;

	public float Acceleration;

	public float Deceleration;

	public bool canParry = true;

	public bool destroyOnParry;

	public float timestamp;

	public float InvincibleTime;

	public float SinLength;

	public TrailRenderer LQ_Trail;

	public AraTrail Trail;

	public bool Explosive;

	public bool CanKnockBack = true;

	public bool ReturnToSenderOnKnockback;

	public bool ModifyingZ;

	public int AllowedBounces;

	public bool Destroyable = true;

	public bool DestroyOnWallHit = true;

	public float ScreenShakeMultiplier = 1f;

	public bool UseDelay;

	public bool CollideOnlyTarget;

	private float physicsTimer;

	private float radius;

	private int layerCollisionMask;

	public Health.Team _team = Health.Team.PlayerTeam;

	private float InitScale;

	private float Scale;

	private float ScaleSpeed;

	public List<Sprite> ChunksToSpawn;

	private List<Sprite> ValidParticleChunksToSpawn;

	private float Timer;

	public Health health;

	public Health Owner;

	public Health.AttackFlags AttackFlags;

	private int bouncesRemaining;

	private bool destroyed;

	private Rigidbody2D rb;

	private Vector3 startPos = Vector3.zero;

	[HideInInspector]
	public CollisionEvent collisionEventQueue;

	public bool SetTargetToClosest;

	public bool homeInOnTarget;

	public float turningSpeed = 1f;

	public float angleNoiseAmplitude;

	public float angleNoiseFrequency;

	public float AngleIncrement;

	public float DamageToNeutral = 10f;

	public float NeutralSplashRadius;

	public float HitFxScaleMultiplier = 1f;

	public bool Seperate;

	private Health targetObject;

	public bool PulseMove;

	[EventRef]
	public string OnSpawnSoundPath = string.Empty;

	[EventRef]
	public string LoopedSoundPath = string.Empty;

	private EventInstance LoopedSound;

	private float spawnTimestamp;

	private GameObject hitPrefab;

	private GameObject hitParticleObj;

	private Collider2D collider;

	private LayerMask bounceMask;

	private LayerMask unitMask;

	public static GameObject projectilePrefab;

	public Vector2 Seperator = Vector2.zero;

	private float SeperationRadius = 0.6f;

	[HideInInspector]
	private float SeparationCooldownTime;

	private float PhysicsDelay = 0.1f;

	private static GameObject loadedArrow;

	private static GameObject loadedPlayerArrow;

	private CircleCollider2D circleCollider2D;

	private bool initialized;

	private Coroutine recycleRoutine;

	private bool disableOnce;

	private float ZWave;

	public float ZWaveSize;

	public float ZWaveSpeed;

	private bool stoppedLoop;

	public static Dictionary<int, Projectile> ProjectileComponents = new Dictionary<int, Projectile>();

	public static Dictionary<int, Health> HealthComponents = new Dictionary<int, Health>();

	public bool IgnoreIsland;

	public SkeletonAnimation Spine { get; set; }

	public Health.Team team
	{
		get
		{
			return _team;
		}
		set
		{
			_team = value;
		}
	}

	public CircleCollider2D CircleCollider2D
	{
		get
		{
			return circleCollider2D;
		}
	}

	public bool KnockedBack { get; set; }

	public static void CreateProjectiles(int amount, Health owner, Vector3 position, bool explosive = false)
	{
		Transform parent = owner.transform.parent;
		if (projectilePrefab == null)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Enemies/Weapons/ArrowTurrets.prefab");
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				projectilePrefab = obj.Result;
				if (ControlUtilities.GetPlatformFromUnifyPlatform() == Platform.Switch)
				{
					SpriteRenderer component2 = projectilePrefab.GetComponent<Projectile>().ArrowImage.Find("Bullet_Core").GetComponent<SpriteRenderer>();
					if (component2 != null)
					{
						component2.material.SetFloat("_OPNOISEA", 0f);
						component2.material.SetFloat("_OPNOISEB", 0f);
					}
				}
				CreateProjectiles(amount, owner, position, explosive);
			};
			return;
		}
		float num = 0f;
		int num2 = -1;
		while (++num2 < amount)
		{
			Projectile component = ObjectPool.Spawn(projectilePrefab, parent).GetComponent<Projectile>();
			component.transform.position = position;
			component.Angle = num;
			component.team = Health.Team.Team2;
			component.Speed = 4f;
			component.Owner = owner;
			component.Explosive = explosive;
			num += 360f / (float)amount;
			if (owner.team != Health.Team.PlayerTeam)
			{
				component.Speed *= DataManager.Instance.ProjectileMoveSpeedMultiplier;
			}
		}
	}

	public static void CreatePlayerProjectiles(int amount, Health owner, Vector3 position, float speed = 4f, float damage = 1f, float angleOffset = 0f, bool explosive = false, Action<List<Projectile>> callback = null)
	{
		CreatePlayerProjectiles(amount, owner, position, "Assets/Prefabs/Enemies/Weapons/ArrowPlayer.prefab", damage, speed, angleOffset, explosive, callback);
	}

	public static void CreatePlayerProjectiles(int amount, Health owner, Vector3 position, string prefabPath, float speed = 4f, float damage = 1f, float angleOffset = 0f, bool explosive = false, Action<List<Projectile>> callback = null, Health.AttackFlags attackFlags = (Health.AttackFlags)0)
	{
		if (loadedPlayerArrow == null)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(prefabPath);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				loadedPlayerArrow = obj.Result;
				CreateProjectiles(amount, owner, position, speed, damage, angleOffset, explosive, callback);
			};
			return;
		}
		float num = angleOffset;
		int num2 = -1;
		List<Projectile> list = new List<Projectile>();
		while (++num2 < amount)
		{
			Projectile component = ObjectPool.Spawn(loadedPlayerArrow).GetComponent<Projectile>();
			component.transform.position = position;
			component.Angle = num;
			component.team = owner.team;
			component.Speed = speed;
			component.Owner = owner;
			component.Damage = damage;
			component.Explosive = explosive;
			component.AttackFlags = attackFlags;
			component.transform.parent = ((BiomeGenerator.Instance != null) ? BiomeGenerator.Instance.CurrentRoom.generateRoom.transform : null);
			num += Mathf.Repeat(360f / (float)amount, 360f);
			if (owner.team != Health.Team.PlayerTeam)
			{
				component.Speed *= DataManager.Instance.ProjectileMoveSpeedMultiplier;
			}
			list.Add(component);
		}
		Action<List<Projectile>> action = callback;
		if (action != null)
		{
			action(list);
		}
	}

	public static void CreateProjectiles(int amount, Health owner, Vector3 position, float speed = 4f, float damage = 1f, float angleOffset = 0f, bool explosive = false, Action<List<Projectile>> callback = null)
	{
		if (loadedArrow == null)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Enemies/Weapons/ArrowTurrets.prefab");
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				loadedArrow = obj.Result;
				CreateProjectiles(amount, owner, position, speed, damage, angleOffset, explosive, callback);
			};
			return;
		}
		float num = angleOffset;
		int num2 = -1;
		List<Projectile> list = new List<Projectile>();
		while (++num2 < amount)
		{
			Projectile component = ObjectPool.Spawn(loadedArrow).GetComponent<Projectile>();
			component.transform.position = position;
			component.transform.parent = owner.transform.parent;
			component.Angle = num;
			component.team = owner.team;
			component.Speed = speed;
			component.Owner = owner;
			component.Damage = damage;
			component.Explosive = explosive;
			num += Mathf.Repeat(360f / (float)amount, 360f);
			if (owner.team != Health.Team.PlayerTeam)
			{
				component.Speed *= DataManager.Instance.ProjectileMoveSpeedMultiplier;
			}
			list.Add(component);
		}
		Action<List<Projectile>> action = callback;
		if (action != null)
		{
			action(list);
		}
	}

	private void Awake()
	{
		collider = GetComponentInChildren<Collider2D>();
		circleCollider2D = GetComponent<CircleCollider2D>();
		Rigidbody2D component = GetComponent<Rigidbody2D>();
		radius = ((circleCollider2D != null) ? circleCollider2D.radius : 0.5f);
		if (circleCollider2D != null)
		{
			UnityEngine.Object.Destroy(circleCollider2D);
		}
		if (component != null)
		{
			UnityEngine.Object.Destroy(component);
		}
		layerCollisionMask = Physics2D.GetLayerCollisionMask(base.gameObject.layer);
	}

	private void Start()
	{
		Scale = InitScale * 2f;
		if ((bool)health)
		{
			health.team = team;
		}
		Spine = GetComponentInChildren<SkeletonAnimation>();
		startPos = base.transform.position;
		spawnTimestamp = GameManager.GetInstance().CurrentTime;
		bounceMask = (int)bounceMask | (1 << LayerMask.NameToLayer("Obstacles"));
		bounceMask = (int)bounceMask | (1 << LayerMask.NameToLayer("Island"));
		unitMask = (int)unitMask | (1 << LayerMask.NameToLayer("Obstacles"));
		unitMask = (int)unitMask | (1 << LayerMask.NameToLayer("Units"));
		PhysicsDelay += UnityEngine.Random.Range(0f, PhysicsDelay);
		initialized = true;
	}

	private void OnDisable()
	{
		AudioManager.Instance.StopLoop(LoopedSound);
		Acceleration = 0f;
		Deceleration = 0f;
		PulseMove = false;
		KnockedBack = false;
		if ((bool)health)
		{
			health.OnHit -= OnHit;
		}
		Projectiles.Remove(this);
		if (hitParticleObj != null)
		{
			hitParticleObj.Recycle();
		}
		if (initialized && !destroyed && (base.transform.parent == null || (base.transform.parent.GetComponent<Projectile>() == null && base.transform.parent.GetComponent<ObjectPool>() == null)))
		{
			GameManager instance = GameManager.GetInstance();
			if ((object)instance != null)
			{
				instance.StartCoroutine(RecycleIE(1f));
			}
		}
	}

	protected virtual void OnEnable()
	{
		if ((bool)health)
		{
			health.OnHit += OnHit;
		}
		if (!LoopedSound.isValid())
		{
			LoopedSound = AudioManager.Instance.CreateLoop(LoopedSoundPath, true);
		}
		if (!string.IsNullOrEmpty(OnSpawnSoundPath))
		{
			AudioManager.Instance.PlayOneShot(OnSpawnSoundPath, base.gameObject);
		}
		if (!Projectiles.Contains(this))
		{
			Projectiles.Add(this);
		}
		timestamp = GameManager.GetInstance().CurrentTime;
		Timer = 0f;
		destroyed = false;
		Explosive = false;
		Damage = 1f;
		if (recycleRoutine != null)
		{
			StopCoroutine(recycleRoutine);
			recycleRoutine = null;
		}
		bouncesRemaining = AllowedBounces;
		if ((bool)ArrowImage)
		{
			ArrowImage.gameObject.SetActive(true);
		}
		if ((bool)Trail)
		{
			Trail.Clear();
			Trail.enabled = true;
			Trail.emit = true;
		}
		if ((bool)collider)
		{
			collider.enabled = true;
		}
		if (SettingsManager.Settings.Graphics.EnvironmentDetail == GraphicsSettingsUtilities.LowPreset.EnvironmentDetail)
		{
			base.gameObject.layer = LayerMask.NameToLayer("ObstaclesAndPlayer");
		}
		else
		{
			base.gameObject.layer = LayerMask.NameToLayer("VFX");
		}
		ValidParticleChunksToSpawn = ChunksToSpawn.Where((Sprite x) => x != null).ToList();
	}

	private IEnumerator DelayTrailByFrame()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		LQ_Trail.emitting = true;
	}

	protected void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (!canParry || GameManager.GetInstance().CurrentTime < spawnTimestamp + InvincibleTime || !CanKnockBack || Attacker.gameObject != PlayerFarming.Instance.gameObject)
		{
			return;
		}
		KnockedBack = true;
		team = Health.Team.PlayerTeam;
		this.health.team = Health.Team.PlayerTeam;
		if (destroyOnParry)
		{
			if (AttackType == Health.AttackTypes.Melee && !destroyed)
			{
				EmitParticle();
			}
			AudioManager.Instance.PlayOneShot("event:/player/Curses/arrow_hit", base.transform.position);
			CameraManager.shakeCamera(0.1f);
			DestroyProjectile();
			return;
		}
		if (AttackType == Health.AttackTypes.Projectile)
		{
			CameraManager.shakeCamera(0.5f, Utils.GetAngle(base.transform.position, AttackLocation));
			Explosion.CreateExplosion(base.transform.position, Health.Team.PlayerTeam, this.health, 3f, -1f, -1f, false, 1f, AttackFlags);
			AudioManager.Instance.PlayOneShot("event:/player/Curses/arrow_hit", base.transform.position);
			DestroyProjectile();
			return;
		}
		GameManager.GetInstance().HitStop();
		CameraManager.shakeCamera(0.2f, UnityEngine.Random.Range(0, 360));
		if (!destroyed)
		{
			EmitParticle();
		}
		Angle += 180f;
		Speed = 15f;
		Damage *= 3f;
		LifeTime = 3f;
		if (Attacker == PlayerFarming.Instance.gameObject)
		{
			ForgiveCollisionWithPlayer();
		}
		if (Owner != null)
		{
			Angle = Utils.GetAngle(base.transform.position, Owner.transform.position);
			return;
		}
		Health health = null;
		float num = float.MaxValue;
		float angle = 0f;
		float num2 = 90f;
		foreach (Health allUnit in Health.allUnits)
		{
			float angle2 = Utils.GetAngle(base.transform.position, allUnit.transform.position);
			float num3 = Vector2.Distance(base.transform.position, allUnit.transform.position);
			if (allUnit != this.health && !allUnit.InanimateObject && allUnit.team == Health.Team.Team2 && num3 < 8f && num3 < num && Mathf.Abs(Angle - angle2) < 180f && angle2 > Angle - num2 && angle2 < Angle + num2)
			{
				health = allUnit;
				num = num3;
				angle = angle2;
			}
		}
		if (health != null)
		{
			Angle = angle;
		}
	}

	private void EmitParticle(float scaleMultiplier = 1f)
	{
		if (ArrowImage != null)
		{
			BiomeConstants.Instance.EmitHitFX_BlockedRedSmall(ArrowImage.position, Quaternion.identity, Vector3.one * scaleMultiplier * HitFxScaleMultiplier);
		}
	}

	public void Deflect()
	{
	}

	protected void FixedUpdate()
	{
		if (PlayerFarming.Instance == null || destroyed)
		{
			return;
		}
		if (PlayerFarming.Instance.health.HP <= 0f && !stoppedLoop)
		{
			AudioManager.Instance.StopLoop(LoopedSound);
			stoppedLoop = true;
		}
		ScaleSpeed += (InitScale - Scale) * 0.4f;
		Scale += (ScaleSpeed *= 0.6f);
		Speed += Acceleration * Time.fixedDeltaTime;
		Speed -= Deceleration * Time.fixedDeltaTime;
		if (PulseMove)
		{
			if (Speed <= 0f && Deceleration > 0f)
			{
				Acceleration = Deceleration;
				Deceleration = 0f;
			}
			else if (Acceleration != 0f && Speed >= Acceleration)
			{
				Deceleration = Acceleration;
				Acceleration = 0f;
			}
		}
		Seperator = Vector2.zero;
		if (Seperate && SeparationCooldownTime < Time.time)
		{
			foreach (Projectile projectile in Projectiles)
			{
				if (!(projectile == null) && !(projectile == this) && projectile.Seperate)
				{
					float num = Vector2.Distance(projectile.transform.position, base.transform.position);
					float angle = Utils.GetAngle(projectile.transform.position, base.transform.position);
					if (num < SeperationRadius)
					{
						float num2 = (SeperationRadius - num) / 2f * Mathf.Cos(angle * ((float)Math.PI / 180f)) * Time.unscaledDeltaTime * 60f;
						float num3 = (SeperationRadius - num) / 2f * Mathf.Sin(angle * ((float)Math.PI / 180f)) * Time.unscaledDeltaTime * 60f;
						Seperator.x += num2;
						Seperator.y += num3;
						projectile.Seperator.x -= num2;
						projectile.Seperator.y -= num3;
					}
				}
			}
			SeparationCooldownTime = Time.time + 0.5f;
		}
		if (homeInOnTarget && !KnockedBack)
		{
			if (targetObject == null && team != Health.Team.PlayerTeam)
			{
				GameObject gameObject = PlayerFarming.Instance.gameObject;
				if (gameObject != null)
				{
					targetObject = gameObject.GetComponent<Health>();
				}
			}
			TurnTowardsTarget();
		}
		Angle = Mathf.Repeat(Angle + AngleIncrement, 360f);
		float num4 = Angle;
		if (angleNoiseAmplitude > 0f && angleNoiseFrequency > 0f)
		{
			num4 += (-0.5f + Mathf.PerlinNoise(GameManager.GetInstance().TimeSince(timestamp) * angleNoiseFrequency, 0f)) * angleNoiseAmplitude;
		}
		float f = num4 * ((float)Math.PI / 180f);
		Vector2 vector = new Vector3(Speed * Mathf.Cos(f), Speed * Mathf.Sin(f), 0f) * Time.fixedDeltaTime;
		if (SinLength != 0f)
		{
			Vector2 vector3 = Utils.DegreeToVector2(num4 + 90f).normalized * Mathf.PingPong(Time.time, SinLength);
		}
		if (followDirection)
		{
			base.transform.rotation = Quaternion.Euler(0f, 0f, num4);
		}
		Vector2 vector2 = vector * SpeedMultiplier + Seperator;
		if (!PlayerRelic.TimeFrozen)
		{
			base.transform.position = base.transform.position + new Vector3(vector2.x, vector2.y, 0f);
		}
		if (ModifyingZ)
		{
			Spine.transform.localPosition = Vector3.Lerp(Spine.transform.localPosition, Vector3.zero, Time.fixedDeltaTime);
		}
		if (collisionEventQueue != null)
		{
			if (GameManager.GetInstance().UnscaledTimeSince(collisionEventQueue.UnscaledTimestamp) >= Health.DealDamageForgivenessWindow)
			{
				OnCollisionWithPlayer(collisionEventQueue.TargetHealth, true);
			}
		}
		else if (!PlayerRelic.TimeFrozen && (Timer += Time.fixedDeltaTime) > LifeTime && !destroyed && Destroyable)
		{
			EndOfLife();
		}
		if (ArrowImageEmission != null)
		{
			ArrowImageEmission.position = startPos;
		}
		if (IsProjectilesParent)
		{
			return;
		}
		if (CollideOnlyTarget)
		{
			if (!(targetObject == null) && targetObject.enabled && Vector2.Distance(targetObject.transform.position, base.transform.position) <= radius * 2f)
			{
				Collider2D component = targetObject.GetComponent<Collider2D>();
				if (component != null)
				{
					OnRayEnter2D(component);
				}
			}
		}
		else
		{
			if (UseDelay && (!UseDelay || !((physicsTimer -= Time.fixedDeltaTime) <= 0f)))
			{
				return;
			}
			physicsTimer = PhysicsDelay;
			Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position, radius, layerCollisionMask);
			if (array != null && array.Length != 0)
			{
				for (int i = 0; i < array.Length; i++)
				{
					OnRayEnter2D(array[i]);
				}
			}
		}
	}

	public virtual void EndOfLife()
	{
		AudioManager.Instance.StopLoop(LoopedSound);
		if (Explosive)
		{
			Explosion.CreateExplosion(base.transform.position, Health.Team.PlayerTeam, health, 4f, 1f, 1f, false, 1f, AttackFlags);
			AudioManager.Instance.PlayOneShot("event:/player/Curses/explosive_shot", base.transform.position);
		}
		else
		{
			AudioManager.Instance.PlayOneShot("event:/player/Curses/arrow_hitwall", base.transform.position);
			if (ArrowImage != null)
			{
				EmitParticle(0.5f);
			}
		}
		DestroyProjectile();
	}

	public void SetTarget(Health go)
	{
		if (!(go == null))
		{
			targetObject = go;
		}
	}

	public bool IsAttachedToProjectileTrap()
	{
		if (health != null)
		{
			return health.GetComponent<IProjectileTrap>() != null;
		}
		return false;
	}

	private void TurnTowardsTarget()
	{
		if (!(targetObject == null) && targetObject.enabled && !(MagnitudeFindDistanceBetween(base.transform.position, targetObject.transform.position) <= 0.25f))
		{
			float num = turningSpeed;
			if (Mathf.Abs(Angle - Utils.GetAngle(base.transform.position, targetObject.transform.position)) > 180f)
			{
				num /= 2f;
			}
			Angle = Mathf.LerpAngle(Angle, Utils.GetAngle(base.transform.position, targetObject.transform.position), Time.fixedDeltaTime * num);
		}
	}

	protected void OnCollisionWithPlayer(Health targetHealth, bool collideImmediately = false)
	{
		if (destroyed)
		{
			return;
		}
		if (!collideImmediately)
		{
			if (collisionEventQueue == null)
			{
				collisionEventQueue = new CollisionEvent(Time.unscaledTime, targetHealth);
			}
			return;
		}
		if (targetHealth != null && targetHealth.state.CURRENT_STATE != StateMachine.State.Dodging)
		{
			CameraManager.shakeCamera(0.5f, Utils.GetAngle(base.transform.position, targetHealth.transform.position));
			AudioManager.Instance.PlayOneShot("event:/player/Curses/arrow_hit", base.transform.position);
			DestroyProjectile();
		}
		if (collideImmediately)
		{
			collisionEventQueue = null;
		}
	}

	protected void ForgiveCollisionWithPlayer()
	{
		if (collisionEventQueue != null)
		{
			Debug.Log("Projectile collision forgiven!");
		}
		collisionEventQueue = null;
	}

	private T GetCachedComponent<T>(Dictionary<int, T> dictionary, int objectID, GameObject cachedCollider) where T : MonoBehaviour
	{
		T val = null;
		T value;
		if (dictionary.TryGetValue(objectID, out value))
		{
			val = value;
		}
		else
		{
			val = cachedCollider.GetComponent<T>();
			dictionary.Add(objectID, val);
		}
		return val;
	}

	public static void CleanCache()
	{
		ProjectileComponents.Clear();
		HealthComponents.Clear();
	}

	protected void OnRayEnter2D(Collider2D collider)
	{
		int instanceID = collider.gameObject.GetInstanceID();
		Projectile cachedComponent = GetCachedComponent(ProjectileComponents, instanceID, collider.gameObject);
		if (destroyed || collider == null || cachedComponent != null || destroyed)
		{
			return;
		}
		Health cachedComponent2 = GetCachedComponent(HealthComponents, instanceID, collider.gameObject);
		if (cachedComponent2 != null && cachedComponent2.enabled && cachedComponent2 != health && (cachedComponent2.team != team || cachedComponent2.IsCharmedEnemy) && !cachedComponent2.untouchable && !cachedComponent2.invincible && !cachedComponent2.IgnoreProjectiles && (cachedComponent2.state == null || cachedComponent2.state.CURRENT_STATE != StateMachine.State.Dodging))
		{
			if (((cachedComponent2.team == Health.Team.Neutral || (!(ArrowImage == null) && !ArrowImage.gameObject.activeSelf) || cachedComponent2.invincible) && (cachedComponent2.team != 0 || !(DamageToNeutral > 0f) || !(cachedComponent2.tag != "BreakableDecoration"))) || (cachedComponent2 == Owner && cachedComponent2.IsCharmed) || (cachedComponent2.isPlayer && TrinketManager.HasTrinket(TarotCards.Card.ImmuneToTraps) && IsAttachedToProjectileTrap()))
			{
				return;
			}
			bool flag = cachedComponent2.DealDamage((cachedComponent2.team == Health.Team.Neutral) ? DamageToNeutral : Damage, base.gameObject, base.transform.position, false, Health.AttackTypes.Projectile, false, AttackFlags);
			if (cachedComponent2.isPlayer)
			{
				OnCollisionWithPlayer(cachedComponent2);
				return;
			}
			if (!flag)
			{
				CameraManager.shakeCamera(0.2f * ScreenShakeMultiplier, Utils.GetAngle(base.transform.position, cachedComponent2.transform.position));
				SpawnChunks(collider.transform.position);
			}
			else
			{
				CameraManager.shakeCamera(0.5f * ScreenShakeMultiplier, Utils.GetAngle(base.transform.position, cachedComponent2.transform.position));
			}
			if (cachedComponent2.team == Health.Team.Neutral && NeutralSplashRadius > 0f)
			{
				Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position, NeutralSplashRadius, unitMask);
				for (int i = 0; i < array.Length; i++)
				{
					Health component = array[i].GetComponent<Health>();
					if ((bool)component && component.team == Health.Team.Neutral)
					{
						component.DealDamage(DamageToNeutral, base.gameObject, base.transform.position, false, Health.AttackTypes.Projectile, false, AttackFlags);
					}
				}
			}
			if (KnockedBack)
			{
				Explosion.CreateExplosion(base.transform.position, Health.Team.PlayerTeam, health, 3f, -1f, -1f, false, 1f, AttackFlags);
			}
			else if (Explosive)
			{
				Explosion.CreateExplosion(base.transform.position, Health.Team.PlayerTeam, health, 4f, 1f, 1f, false, 1f, AttackFlags);
			}
			if (!Explosive)
			{
				AudioManager.Instance.PlayOneShot("event:/player/Curses/arrow_hit", base.transform.position);
			}
			else
			{
				AudioManager.Instance.PlayOneShot("event:/player/Curses/explosive_shot", base.transform.position);
			}
			if ((bool)health && health.team == Health.Team.PlayerTeam && (bool)collider && collider.GetComponentInParent<Projectile>() != null)
			{
				Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position, NeutralSplashRadius);
				for (int i = 0; i < array.Length; i++)
				{
					Projectile componentInParent = array[i].GetComponentInParent<Projectile>();
					if ((bool)componentInParent)
					{
						componentInParent.DestroyProjectile();
					}
				}
			}
			DestroyProjectile();
		}
		else
		{
			if (IgnoreIsland || (collider.gameObject.layer != LayerMask.NameToLayer("Obstacles") && collider.gameObject.layer != LayerMask.NameToLayer("Island")))
			{
				return;
			}
			if (bouncesRemaining <= 0)
			{
				if (DestroyOnWallHit && !IsAttachedToProjectileTrap())
				{
					EmitParticle();
					CameraManager.shakeCamera(0.2f * ScreenShakeMultiplier, Utils.GetAngle(base.transform.position, collider.transform.position));
					SpawnChunks(collider.transform.position);
					AudioManager.Instance.PlayOneShot("event:/player/Curses/arrow_hitwall", base.transform.position);
					DestroyProjectile();
					if (Explosive)
					{
						Explosion.CreateExplosion(base.transform.position, Health.Team.PlayerTeam, health, 4f, 1f, 1f, false, 1f, AttackFlags);
					}
				}
			}
			else
			{
				Vector3 vector = Utils.DegreeToVector2(Angle);
				RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position, vector, 1f, bounceMask);
				if ((bool)raycastHit2D)
				{
					vector = Vector3.Reflect(vector, raycastHit2D.normal);
					Angle = Utils.GetAngle(Vector3.zero, vector);
					bouncesRemaining--;
				}
			}
		}
	}

	private float MagnitudeFindDistanceBetween(Vector3 a, Vector3 b)
	{
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		float num3 = a.z - b.z;
		return num * num + num2 * num2 + num3 * num3;
	}

	protected void SpawnChunks(Vector3 collisionPosition)
	{
		if (ValidParticleChunksToSpawn.Count != 0)
		{
			int num = -1;
			while (++num < ValidParticleChunksToSpawn.Count)
			{
				Particle_Chunk.AddNew(base.transform.position, Utils.GetAngle(collisionPosition, base.transform.position) + (float)UnityEngine.Random.Range(-20, 20), ValidParticleChunksToSpawn, num);
			}
		}
	}

	public void DestroyProjectile(bool forced = false)
	{
		if ((Destroyable || forced) && !destroyed)
		{
			AudioManager.Instance.StopLoop(LoopedSound);
			destroyed = true;
			float delay = 0f;
			if ((bool)Trail)
			{
				delay = Trail.time;
				Trail.emit = false;
			}
			if ((bool)collider)
			{
				collider.enabled = false;
			}
			if ((bool)ArrowImage)
			{
				recycleRoutine = StartCoroutine(RecycleIE(0f));
			}
			else
			{
				recycleRoutine = StartCoroutine(RecycleIE(delay));
			}
		}
	}

	private IEnumerator RecycleIE(float delay)
	{
		yield return new WaitForSeconds(delay);
		Projectiles.Remove(this);
		if (this != null)
		{
			base.gameObject.Recycle();
		}
		if (hitParticleObj != null)
		{
			hitParticleObj.Recycle();
		}
		recycleRoutine = null;
	}

	public virtual void DestroyWithVFX()
	{
		if (!destroyed)
		{
			EmitParticle();
			DestroyProjectile();
		}
	}

	public static void ClearProjectiles()
	{
		for (int i = 0; i < Projectiles.Count; i++)
		{
			if (Projectiles[i] != null)
			{
				Projectiles[i].DestroyProjectile(true);
			}
		}
	}
}
