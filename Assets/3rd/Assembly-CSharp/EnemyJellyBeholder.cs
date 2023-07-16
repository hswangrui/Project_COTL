using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyJellyBeholder : EnemyChaser
{
	[Serializable]
	public class RoundsOfEnemies
	{
		public bool DisplayGizmo;

		public float beholderHealthThresholdToTriggerRound = 1f;

		public List<EnemyRounds.EnemyAndPosition> Round = new List<EnemyRounds.EnemyAndPosition>();

		public float ComeBackDelay = 30f;
	}

	private enum BeholderState
	{
		Idle = 1,
		Moving = 2,
		ChargingUpSpawn = 3,
		Spawning = 4,
		Hiding = 5,
		ChargingUpAttack = 6,
		Attacking = 8,
		Shooting = 9
	}

	public enum AttackType
	{
		Melee,
		TargetedShot,
		RingShot,
		PatternShot
	}

	[SerializeField]
	private bool chase;

	[SerializeField]
	private bool patrol;

	[SerializeField]
	private bool flee;

	[SerializeField]
	private float TurningArc = 90f;

	[SerializeField]
	private Vector2 DistanceRange = new Vector2(1f, 3f);

	[SerializeField]
	private List<Vector3> patrolRoute = new List<Vector3>();

	[SerializeField]
	private float fleeCheckIntervalTime;

	[SerializeField]
	private float wallCheckDistance;

	[SerializeField]
	private float distanceToFlee;

	[SerializeField]
	public SkeletonAnimation effectsSpine;

	public ParticleSystem summonParticles;

	public SkeletonAnimation Spine;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string idleAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string anticipationAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string flyInAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string flyOutAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string attackAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string summonAnticipationAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string summonAnimation;

	[Space]
	[SerializeField]
	private GameObject shadow;

	private float fleeTimestamp;

	private float repathTimestamp;

	protected float initialSpawnTimestamp;

	private float randomDirection;

	private int patrolIndex;

	private Vector3 startPosition;

	protected float distanceToTarget = float.MaxValue;

	private float repathTimeInterval = 2f;

	[SerializeField]
	private List<RoundsOfEnemies> enemyRounds = new List<RoundsOfEnemies>();

	private List<Health> spawnedEnemies = new List<Health>();

	private int deathCount;

	private int currentRoundIndex;

	private bool isEnemiesSpawned;

	[SerializeField]
	private BeholderState beholderState = BeholderState.Idle;

	[SerializeField]
	private float spawningChargeUpTime;

	[SerializeField]
	private float hidingExitTime;

	[SerializeField]
	private float minTimeBetweenSpawns;

	private float lastSpawnTimestamp;

	[SerializeField]
	private bool killSpawnablesOnDeath;

	[SerializeField]
	private SpriteRenderer aiming;

	[SerializeField]
	private AttackType[] attackPattern;

	[SerializeField]
	private bool randomPattern;

	private int attackPatternIndex;

	[SerializeField]
	private bool attackOnHit = true;

	[SerializeField]
	private float attackDistance;

	[SerializeField]
	private float attackChargeDur;

	[SerializeField]
	private float attackCooldown;

	private bool canAttack;

	private float flashTickTimer;

	[SerializeField]
	private float attackDuration = 1f;

	[SerializeField]
	private float damageDuration = 0.2f;

	[SerializeField]
	private float attackForceModifier = 1f;

	[SerializeField]
	private int numAttacks = 1;

	[SerializeField]
	private float timeBetweenMultiAttacks = 0.5f;

	[SerializeField]
	private GameObject ringshotArrowPrefab;

	[SerializeField]
	private bool ringshotArrowIsBomb;

	[SerializeField]
	private bool ringshotArrowIsPoison;

	[SerializeField]
	private int numRingShots;

	[SerializeField]
	private float ringShotAngleArc = 360f;

	[SerializeField]
	private float ringShotDelay = 0.05f;

	[SerializeField]
	private GameObject targetedArrowPrefab;

	[SerializeField]
	private bool targetedArrowIsBomb;

	[SerializeField]
	private bool targetedArrowIsPoison;

	[SerializeField]
	private int numTargetedShots;

	[SerializeField]
	private float targetedShotDelay = 0.05f;

	[SerializeField]
	private float arrowSpeed = 9f;

	private const float bombDuration = 1f;

	private const float minBombRange = 1f;

	private float attackTimer;

	private Coroutine spawnRoutine;

	private ProjectilePattern projectilePattern;

	[EventRef]
	public string AttackVO = string.Empty;

	[EventRef]
	public string WarningVO = string.Empty;

	[EventRef]
	public string EnemySpawnSfx = string.Empty;

	[EventRef]
	public string ShootSFX = "event:/enemy/shoot_magicenergy";

	[EventRef]
	public string ShootSpikeSfx = "event:/enemy/shoot_arrowspike";

	[EventRef]
	public string SummonSfx = "event:/enemy/summon";

	[EventRef]
	public string SummonedSfx = "event:/enemy/summoned";

	private Dictionary<AssetReferenceGameObject, AsyncOperationHandle<GameObject>> loadedAddressableAssets = new Dictionary<AssetReferenceGameObject, AsyncOperationHandle<GameObject>>();

	private Vector3 v3Shake = Vector3.zero;

	private Vector3 v3ShakeSpeed = Vector3.zero;

	public override void Awake()
	{
		Spine.ForceVisible = true;
		base.Awake();
		spine = Spine;
		MiniBossController componentInParent = GetComponentInParent<MiniBossController>();
		string text = componentInParent.name;
		if (GameManager.Layer2)
		{
			text += "_P2";
		}
		if (componentInParent != null && DataManager.Instance.CheckKilledBosses(text) && Spine.AnimationState != null)
		{
			string skin = "Dungeon1_Beaten";
			switch (componentInParent.name)
			{
			case "Boss Beholder 1":
				skin = (GameManager.Layer2 ? "Dungeon1_2_Beaten" : "Dungeon1_Beaten");
				break;
			case "Boss Beholder 2":
				skin = (GameManager.Layer2 ? "Dungeon2_2_Beaten" : "Dungeon2_Beaten");
				break;
			case "Boss Beholder 3":
				skin = (GameManager.Layer2 ? "Dungeon3_2_Beaten" : "Dungeon3_Beaten");
				break;
			case "Boss Beholder 4":
				skin = (GameManager.Layer2 ? "Dungeon4_2_Beaten" : "Dungeon4_Beaten");
				break;
			}
			Spine.Skeleton.SetSkin(skin);
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (loadedAddressableAssets == null)
		{
			return;
		}
		foreach (AsyncOperationHandle<GameObject> value in loadedAddressableAssets.Values)
		{
			Addressables.Release((AsyncOperationHandle)value);
		}
		loadedAddressableAssets.Clear();
	}

	protected override void Start()
	{
		base.Start();
		startPosition = base.transform.position;
		if (patrolRoute.Count > 0)
		{
			patrolRoute.Insert(0, Vector3.zero);
		}
		if ((bool)gm)
		{
			health.enabled = false;
		}
		aiming.gameObject.SetActive(false);
		projectilePattern = GetComponent<ProjectilePattern>();
		if (projectilePattern != null && projectilePattern.BulletPrefab != null && projectilePattern.Waves != null && projectilePattern.Waves.Length != 0)
		{
			int num = 0;
			for (int i = 0; i < projectilePattern.Waves.Length; i++)
			{
				num += projectilePattern.Waves[i].Bullets;
			}
			ObjectPool.CreatePool(projectilePattern.BulletPrefab, num, true);
		}
		if (ringshotArrowPrefab != null && numRingShots > 0)
		{
			ObjectPool.CreatePool(ringshotArrowPrefab, numRingShots, true);
		}
		StartCoroutine(PreloadEnemyAssets());
		Health.team2.RemoveAll((Health unit) => unit == null);
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (beholderState != BeholderState.Hiding)
		{
			beholderState = BeholderState.Idle;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		canAttack = true;
	}

	public override void Update()
	{
		base.Update();
		if (gm.CurrentTime > initialSpawnTimestamp && !health.enabled)
		{
			health.enabled = true;
			canAttack = true;
		}
		if (IsTimeToComeBack())
		{
			StartCoroutine(EnemiesDefeated());
		}
		if (Time.deltaTime > 0f)
		{
			v3ShakeSpeed += (Vector3.zero - v3Shake) * 0.4f / Time.deltaTime;
			v3Shake += (v3ShakeSpeed *= 0.7f) * Time.deltaTime;
			Spine.transform.localPosition = v3Shake;
		}
		if ((bool)targetObject)
		{
			float num = Vector3.Distance(targetObject.transform.position, base.transform.position);
			inRange = num < (float)VisionRange;
		}
		if (ShouldStartSpawnEnemies())
		{
			SpawnEnemies();
		}
		if (attackPattern.Length == 0)
		{
			return;
		}
		if (beholderState == BeholderState.ChargingUpAttack)
		{
			attackTimer += Time.deltaTime * Spine.timeScale;
			float num2 = attackTimer / attackChargeDur;
			simpleSpineFlash.FlashWhite(num2 * 0.75f);
			if (attackPattern[attackPatternIndex] == AttackType.Melee || (attackPattern[attackPatternIndex] == AttackType.TargetedShot && !targetedArrowIsBomb) || attackPattern[attackPatternIndex] == AttackType.PatternShot)
			{
				aiming.gameObject.SetActive(true);
				if (flashTickTimer >= 0.12f && BiomeConstants.Instance.IsFlashLightsActive)
				{
					aiming.color = ((aiming.color == Color.red) ? Color.white : Color.red);
					flashTickTimer = 0f;
				}
				flashTickTimer += Time.deltaTime;
			}
			else
			{
				aiming.gameObject.SetActive(false);
			}
			if ((num2 < 0.5f || attackPattern[attackPatternIndex] == AttackType.TargetedShot || attackPattern[attackPatternIndex] == AttackType.PatternShot) && targetObject != null)
			{
				LookAtAngle(Utils.GetAngle(base.transform.position, targetObject.transform.position));
			}
			aiming.transform.eulerAngles = new Vector3(0f, 0f, state.LookAngle);
			if (num2 >= 1f)
			{
				aiming.gameObject.SetActive(false);
				if (attackPattern[attackPatternIndex] == AttackType.Melee)
				{
					MeleeAttack();
				}
				else if (attackPattern[attackPatternIndex] == AttackType.RingShot)
				{
					RingShotAttack();
				}
				else if (attackPattern[attackPatternIndex] == AttackType.TargetedShot)
				{
					TargetedShotAttack();
				}
				else if (attackPattern[attackPatternIndex] == AttackType.PatternShot)
				{
					PatternShotAttack();
				}
			}
		}
		if (canAttack && (bool)targetObject && Vector3.Distance(base.transform.position, targetObject.transform.position) < attackDistance)
		{
			ChargeAttack();
		}
	}

	private bool IsTimeToComeBack()
	{
		if (currentRoundIndex < enemyRounds.Count && isEnemiesSpawned && spawnedEnemies.Count <= 0)
		{
			return true;
		}
		return false;
	}

	private bool ShouldStartSpawnEnemies()
	{
		if (beholderState != BeholderState.Idle && beholderState != BeholderState.Moving)
		{
			return false;
		}
		if (gm.TimeSince(lastSpawnTimestamp) < minTimeBetweenSpawns)
		{
			return false;
		}
		if (currentRoundIndex >= enemyRounds.Count)
		{
			return false;
		}
		if (health.HP <= health.totalHP * enemyRounds[currentRoundIndex].beholderHealthThresholdToTriggerRound)
		{
			return true;
		}
		return false;
	}

	private void ChargeAttack()
	{
		if (beholderState == BeholderState.Idle || beholderState == BeholderState.Moving)
		{
			canAttack = false;
			attackTimer = 0f;
			beholderState = BeholderState.ChargingUpAttack;
			AudioManager.Instance.PlayOneShot("event:/enemy/chaser/chaser_charge", base.gameObject);
			Spine.AnimationState.SetAnimation(0, anticipationAnimation, true);
			if (targetObject != null)
			{
				LookAtAngle(Utils.GetAngle(base.transform.position, targetObject.transform.position));
			}
		}
	}

	private void SpawnEnemies()
	{
		spawnRoutine = StartCoroutine(SpawnDelay());
	}

	private IEnumerator PreloadEnemyAssets()
	{
		foreach (RoundsOfEnemies enemyRound in enemyRounds)
		{
			foreach (EnemyRounds.EnemyAndPosition enemyAndPosition in enemyRound.Round)
			{
				bool isLoaded = false;
				if (loadedAddressableAssets.ContainsKey(enemyAndPosition.EnemyTarget))
				{
					isLoaded = true;
				}
				else
				{
					AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(enemyAndPosition.EnemyTarget);
					asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
					{
						loadedAddressableAssets.Add(enemyAndPosition.EnemyTarget, obj);
						ObjectPool.Spawn(obj.Result, base.transform.parent, enemyAndPosition.Position, Quaternion.identity).Recycle();
						isLoaded = true;
					};
				}
				yield return new WaitUntil(() => isLoaded);
				isLoaded = false;
			}
		}
	}

	private IEnumerator SpawnDelay()
	{
		beholderState = BeholderState.ChargingUpSpawn;
		state.CURRENT_STATE = StateMachine.State.Idle;
		float time2 = 0f;
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
		ClearPaths();
		summonParticles.Play();
		Spine.AnimationState.SetAnimation(0, summonAnticipationAnimation, true);
		effectsSpine.AnimationState.SetAnimation(0, "anticipate-summon", true);
		AudioManager.Instance.PlayOneShot(SummonSfx, base.gameObject);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < spawningChargeUpTime))
			{
				break;
			}
			yield return null;
		}
		health.invincible = true;
		shadow.gameObject.SetActive(false);
		GameManager.GetInstance().RemoveFromCamera(base.gameObject);
		if (Interaction_Chest.Instance != null)
		{
			GameManager.GetInstance().AddToCamera(Interaction_Chest.Instance.gameObject);
		}
		beholderState = BeholderState.Spawning;
		Spine.AnimationState.SetAnimation(0, summonAnimation, false);
		effectsSpine.AnimationState.SetAnimation(0, "summon", false);
		Spine.AnimationState.AddAnimation(0, flyOutAnimation, false, 0f);
		effectsSpine.AnimationState.AddAnimation(0, "fly-out", false, 0f);
		AudioManager.Instance.PlayOneShot(SummonedSfx, base.gameObject);
		if (enemyRounds == null || enemyRounds.Count <= 0)
		{
			yield break;
		}
		deathCount = 0;
		foreach (EnemyRounds.EnemyAndPosition e in enemyRounds[currentRoundIndex].Round)
		{
			GameObject prefab = null;
			AsyncOperationHandle<GameObject> value;
			if (loadedAddressableAssets.TryGetValue(e.EnemyTarget, out value))
			{
				prefab = value.Result;
			}
			else
			{
				Debug.LogError(string.Concat(e.EnemyTarget, " asset is not loaded! Check PreloadEnemyAssets() method."));
			}
			GameObject gameObject = ObjectPool.Spawn(prefab, base.transform.parent, e.Position, Quaternion.identity);
			Health component = gameObject.GetComponent<Health>();
			component.gameObject.SetActive(false);
			component.OnDie += OnSpawnedDie;
			if (component.GetComponent<ShowHPBar>() == null)
			{
				component.gameObject.AddComponent<ShowHPBar>().zOffset = 2f;
			}
			spawnedEnemies.Add(component);
			EnemySpawner.CreateWithAndInitInstantiatedEnemy(e.Position, base.transform.parent, gameObject);
			time2 = 0f;
			while (true)
			{
				float num;
				time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num < e.Delay))
				{
					break;
				}
				yield return null;
			}
		}
		beholderState = BeholderState.Hiding;
		health.untouchable = true;
		isEnemiesSpawned = true;
	}

	private IEnumerator EnemiesDefeated()
	{
		deathCount = 0;
		currentRoundIndex++;
		isEnemiesSpawned = false;
		base.transform.position = Vector3.zero;
		Spine.AnimationState.SetAnimation(0, flyInAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		shadow.gameObject.SetActive(true);
		health.untouchable = false;
		health.invincible = false;
		GameManager.GetInstance().AddToCamera(base.gameObject);
		if (Interaction_Chest.Instance != null)
		{
			GameManager.GetInstance().RemoveFromCamera(Interaction_Chest.Instance.gameObject);
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		beholderState = BeholderState.Idle;
		canAttack = false;
		spawnRoutine = null;
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < attackCooldown))
			{
				break;
			}
			yield return null;
		}
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
		canAttack = true;
	}

	private void OnSpawnedDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (Victim == null)
		{
			return;
		}
		Victim.OnDie -= OnSpawnedDie;
		spawnedEnemies.Remove(Victim);
		SpawnEnemyOnDeath component = Victim.GetComponent<SpawnEnemyOnDeath>();
		if ((bool)component)
		{
			if (component.SpawnedEnemies != null && component.SpawnedEnemies.Length != 0)
			{
				UnitObject[] array = component.SpawnedEnemies;
				foreach (UnitObject enemy in array)
				{
					EnemySplit(enemy);
				}
			}
			else
			{
				component.OnEnemySpawned += EnemySplit;
				component.OnEnemyDespawned += EnemySplit;
			}
		}
		deathCount++;
		Debug.Log("Death count: " + deathCount);
	}

	private void EnemySplit(UnitObject enemy)
	{
		if (enemy != null)
		{
			enemy.health.OnDie += OnSpawnedDie;
			spawnedEnemies.Add(enemy.health);
			enemyRounds[currentRoundIndex].Round.Add(new EnemyRounds.EnemyAndPosition(enemy.health, Vector3.zero, 0f));
		}
		else if (enemyRounds[currentRoundIndex].Round.Contains(null))
		{
			enemyRounds[currentRoundIndex].Round.Remove(null);
		}
		else
		{
			enemyRounds[currentRoundIndex].Round.Add(null);
		}
	}

	private void MeleeAttack()
	{
		if (beholderState != BeholderState.Attacking)
		{
			attackTimer = 0f;
			beholderState = BeholderState.Attacking;
			canAttack = false;
			ClearPaths();
			StartCoroutine(MeleeAttackIE());
			attackPatternIndex = (attackPatternIndex + 1) % attackPattern.Length;
			if (randomPattern)
			{
				attackPatternIndex = UnityEngine.Random.Range(0, attackPattern.Length);
			}
		}
	}

	private IEnumerator MeleeAttackIE()
	{
		float flashMeleeTickTimer = 0f;
		float time;
		for (int i = 0; i < numAttacks; i++)
		{
			Spine.AnimationState.SetAnimation(0, attackAnimation, false);
			Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
			AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/attack", base.gameObject);
			yield return new WaitForEndOfFrame();
			StartCoroutine(TurnOnDamageColliderForDuration(damageDuration));
			simpleSpineFlash.FlashWhite(false);
			DisableForces = true;
			Vector2 force = new Vector2(2500f * Mathf.Cos(state.LookAngle * ((float)Math.PI / 180f)), 2500f * Mathf.Sin(state.LookAngle * ((float)Math.PI / 180f))) * attackForceModifier;
			rb.AddForce(force);
			time = 0f;
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * Spine.timeScale);
				if (!(num < attackDuration))
				{
					break;
				}
				yield return null;
			}
			DisableForces = false;
			if (i >= numAttacks - 1)
			{
				continue;
			}
			AudioManager.Instance.PlayOneShot("event:/enemy/chaser/chaser_charge", base.gameObject);
			Spine.AnimationState.SetAnimation(0, anticipationAnimation, true);
			float t = 0f;
			while (t < timeBetweenMultiAttacks)
			{
				t += Time.deltaTime;
				flashMeleeTickTimer += Time.deltaTime;
				simpleSpineFlash.FlashWhite(t / timeBetweenMultiAttacks * 0.75f);
				aiming.gameObject.SetActive(true);
				if (flashMeleeTickTimer >= 0.12f && BiomeConstants.Instance.IsFlashLightsActive)
				{
					aiming.color = ((aiming.color == Color.red) ? Color.white : Color.red);
					flashMeleeTickTimer = 0f;
				}
				if (t / timeBetweenMultiAttacks < 0.5f)
				{
					LookAtAngle(Utils.GetAngle(base.transform.position, targetObject.transform.position));
				}
				aiming.transform.eulerAngles = new Vector3(0f, 0f, state.LookAngle);
				yield return null;
			}
			aiming.gameObject.SetActive(false);
		}
		beholderState = BeholderState.Idle;
		state.CURRENT_STATE = StateMachine.State.Idle;
		time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < attackCooldown))
			{
				break;
			}
			yield return null;
		}
		canAttack = true;
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

	private void RingShotAttack()
	{
		if (beholderState != BeholderState.Shooting)
		{
			ClearPaths();
			StartCoroutine(RingShotAttackIE());
			attackPatternIndex = (attackPatternIndex + 1) % attackPattern.Length;
			if (randomPattern)
			{
				attackPatternIndex = UnityEngine.Random.Range(0, attackPattern.Length);
			}
		}
	}

	private IEnumerator RingShotAttackIE()
	{
		state.CURRENT_STATE = StateMachine.State.Idle;
		beholderState = BeholderState.Shooting;
		canAttack = false;
		int i = numRingShots;
		float aimingAngle = state.LookAngle;
		if (targetObject != null)
		{
			aimingAngle = Utils.GetAngle(base.transform.position, targetObject.transform.position) - ringShotAngleArc / 2f - ringShotAngleArc * 0.2f;
		}
		aiming.gameObject.SetActive(false);
		Spine.AnimationState.SetAnimation(0, attackAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		while (true)
		{
			int num = i - 1;
			i = num;
			if (num < 0)
			{
				break;
			}
			simpleSpineFlash.FlashWhite(false);
			CameraManager.shakeCamera(0.2f, aimingAngle);
			if (ringshotArrowIsBomb)
			{
				Vector3 vector = base.transform.position + (Vector3)Utils.DegreeToVector2(aimingAngle) * UnityEngine.Random.Range(1f, 6f);
				GameObject gameObject = UnityEngine.Object.Instantiate(ringshotArrowPrefab, vector, Quaternion.identity, base.transform.parent);
				if (ringshotArrowIsPoison)
				{
					gameObject.GetComponent<PoisonBomb>().Play(base.transform.position, 1f);
					AudioManager.Instance.PlayOneShot("event:/boss/spider/bomb_shoot", base.transform.position);
				}
				else
				{
					MortarBomb component = gameObject.GetComponent<MortarBomb>();
					if (Vector2.Distance(base.transform.position, vector) < 1f)
					{
						Vector2 vector2 = base.transform.position + (vector - base.transform.position).normalized * 1f;
						component.transform.position = (Vector3)AstarPath.active.GetNearest(vector2).node.position;
					}
					else
					{
						component.transform.position = (Vector3)AstarPath.active.GetNearest(vector).node.position;
					}
					component.Play(base.transform.position + new Vector3(0f, 0f, -1.5f), 1f, Health.Team.Team2);
					AudioManager.Instance.PlayOneShot("event:/boss/spider/bomb_shoot", base.transform.position);
				}
			}
			else
			{
				Projectile component2 = ObjectPool.Spawn(ringshotArrowPrefab, base.transform.parent).GetComponent<Projectile>();
				component2.transform.position = base.transform.position + new Vector3(0f, 0f, -0.5f) + (Vector3)Utils.DegreeToVector2(aimingAngle) * 0.66f;
				component2.Angle = aimingAngle;
				component2.team = health.team;
				component2.Speed = arrowSpeed * 0.666f;
				component2.Owner = health;
				AudioManager.Instance.PlayOneShot(ShootSFX, base.transform.position);
			}
			aimingAngle += ringShotAngleArc / (float)Mathf.Max(numRingShots, 0);
			float Progress = 0f;
			while (Progress < ringShotDelay)
			{
				Progress += Time.deltaTime * Spine.timeScale;
				yield return null;
			}
		}
		simpleSpineFlash.FlashWhite(false);
		beholderState = BeholderState.Idle;
		state.CURRENT_STATE = StateMachine.State.Idle;
		float time = 0f;
		while (true)
		{
			float num2;
			time = (num2 = time + Time.deltaTime * Spine.timeScale);
			if (!(num2 < attackCooldown))
			{
				break;
			}
			yield return null;
		}
		canAttack = true;
	}

	private void TargetedShotAttack()
	{
		if (beholderState != BeholderState.Shooting)
		{
			ClearPaths();
			StartCoroutine(TargetedShotAttackIE());
			attackPatternIndex = (attackPatternIndex + 1) % attackPattern.Length;
			if (randomPattern)
			{
				attackPatternIndex = UnityEngine.Random.Range(0, attackPattern.Length);
			}
		}
	}

	private IEnumerator TargetedShotAttackIE()
	{
		state.CURRENT_STATE = StateMachine.State.Idle;
		beholderState = BeholderState.Shooting;
		canAttack = false;
		int i = numTargetedShots;
		float flashTargetShotTickTimer = 0f;
		float aimingAngle = state.LookAngle;
		if (!targetedArrowIsBomb)
		{
			aiming.gameObject.SetActive(true);
		}
		if (targetObject != null)
		{
			aimingAngle = Utils.GetAngle(base.transform.position, targetObject.transform.position);
			LookAtAngle(Utils.GetAngle(base.transform.position, targetObject.transform.position));
		}
		aiming.transform.eulerAngles = new Vector3(0f, 0f, state.LookAngle);
		while (true)
		{
			int num = i - 1;
			i = num;
			if (num < 0)
			{
				break;
			}
			simpleSpineFlash.FlashWhite(false);
			CameraManager.shakeCamera(0.2f, aimingAngle);
			if (targetedArrowIsBomb)
			{
				Vector3 vector = targetObject.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 2f;
				GameObject gameObject = ObjectPool.Spawn(targetedArrowPrefab, base.transform.parent, vector, Quaternion.identity);
				AudioManager.Instance.PlayOneShot("event:/boss/spider/bomb_shoot", base.transform.position);
				if (targetedArrowIsPoison)
				{
					gameObject.GetComponent<PoisonBomb>().Play(base.transform.position, 1f);
				}
				else
				{
					MortarBomb component = gameObject.GetComponent<MortarBomb>();
					if (Vector2.Distance(base.transform.position, vector) < 1f)
					{
						component.transform.position = base.transform.position + (vector - base.transform.position).normalized * 1f;
					}
					else
					{
						component.transform.position = vector;
					}
					component.Play(base.transform.position + new Vector3(0f, 0f, -1.5f), 1f, Health.Team.Team2);
				}
			}
			else
			{
				if (numTargetedShots > 1)
				{
					aimingAngle += UnityEngine.Random.Range(-8f, 8f);
				}
				Projectile component2 = ObjectPool.Spawn(targetedArrowPrefab, base.transform.parent).GetComponent<Projectile>();
				component2.transform.position = base.transform.position + new Vector3(0f, 0f, -0.5f) + (Vector3)Utils.DegreeToVector2(aimingAngle);
				component2.Angle = aimingAngle;
				component2.team = health.team;
				component2.Speed = arrowSpeed;
				component2.Owner = health;
				AudioManager.Instance.PlayOneShot(ShootSFX, base.transform.position);
			}
			Spine.AnimationState.SetAnimation(0, attackAnimation, false);
			Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
			float Progress = 0f;
			if (i == 0)
			{
				aiming.gameObject.SetActive(false);
			}
			while (true)
			{
				float num2;
				Progress = (num2 = Progress + Time.deltaTime * Spine.timeScale);
				if (!(num2 < targetedShotDelay))
				{
					break;
				}
				if (targetObject != null)
				{
					aimingAngle = Utils.GetAngle(base.transform.position, targetObject.transform.position);
					LookAtAngle(Utils.GetAngle(base.transform.position, targetObject.transform.position));
				}
				if (flashTargetShotTickTimer >= 0.12f && BiomeConstants.Instance.IsFlashLightsActive)
				{
					aiming.color = ((aiming.color == Color.red) ? Color.white : Color.red);
					flashTargetShotTickTimer = 0f;
				}
				aiming.transform.eulerAngles = new Vector3(0f, 0f, state.LookAngle);
				flashTargetShotTickTimer += Time.deltaTime;
				yield return null;
			}
		}
		simpleSpineFlash.FlashWhite(false);
		beholderState = BeholderState.Idle;
		state.CURRENT_STATE = StateMachine.State.Idle;
		float time = 0f;
		while (true)
		{
			float num2;
			time = (num2 = time + Time.deltaTime * Spine.timeScale);
			if (!(num2 < attackCooldown))
			{
				break;
			}
			yield return null;
		}
		canAttack = true;
	}

	private void PatternShotAttack()
	{
		if (beholderState != BeholderState.Shooting)
		{
			ClearPaths();
			StartCoroutine(PatternShotAttackIE());
			attackPatternIndex = (attackPatternIndex + 1) % attackPattern.Length;
			if (randomPattern)
			{
				attackPatternIndex = UnityEngine.Random.Range(0, attackPattern.Length);
			}
		}
	}

	private IEnumerator PatternShotAttackIE()
	{
		state.CURRENT_STATE = StateMachine.State.Idle;
		beholderState = BeholderState.Shooting;
		canAttack = false;
		float direction = state.LookAngle;
		if (targetObject != null)
		{
			direction = Utils.GetAngle(base.transform.position, targetObject.transform.position);
			LookAtAngle(Utils.GetAngle(base.transform.position, targetObject.transform.position));
		}
		simpleSpineFlash.FlashWhite(false);
		CameraManager.shakeCamera(0.2f, direction);
		if ((bool)projectilePattern)
		{
			projectilePattern.Shoot();
		}
		AudioManager.Instance.PlayOneShot(ShootSFX, base.transform.position);
		Spine.AnimationState.SetAnimation(0, attackAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		aiming.gameObject.SetActive(false);
		simpleSpineFlash.FlashWhite(false);
		beholderState = BeholderState.Idle;
		state.CURRENT_STATE = StateMachine.State.Idle;
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < attackCooldown))
			{
				break;
			}
			yield return null;
		}
		canAttack = true;
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		if (attackOnHit && AttackType == Health.AttackTypes.Melee)
		{
			ChargeAttack();
		}
		else
		{
			base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
			simpleSpineFlash.FlashWhite(false);
			simpleSpineFlash.FlashFillRed();
			AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/gethit", base.transform.position);
		}
		v3ShakeSpeed = base.transform.position - Attacker.transform.position;
		v3ShakeSpeed = v3ShakeSpeed.normalized * 20f;
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		if (killSpawnablesOnDeath)
		{
			foreach (Health spawnedEnemy in spawnedEnemies)
			{
				if (spawnedEnemy != null)
				{
					spawnedEnemy.DealDamage(spawnedEnemy.totalHP, base.gameObject, base.transform.position, false, Health.AttackTypes.Heavy, true);
				}
			}
			foreach (EnemySpider enemySpider in EnemySpider.EnemySpiders)
			{
				if ((bool)enemySpider)
				{
					enemySpider.health.DealDamage(enemySpider.health.totalHP, base.gameObject, base.transform.position, false, Health.AttackTypes.Heavy, true);
				}
			}
		}
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/death", base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/enemy/enemy_death_large", base.transform.position);
	}

	protected override void UpdateMoving()
	{
		if (chase)
		{
			base.UpdateMoving();
		}
		else if (patrol && (state.CURRENT_STATE == StateMachine.State.Idle || gm.CurrentTime > repathTimestamp) && (beholderState == BeholderState.Idle || beholderState == BeholderState.Moving))
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
			repathTimestamp = gm.CurrentTime + repathTimeInterval;
		}
		else if (flee && (bool)gm && gm.CurrentTime > fleeTimestamp)
		{
			fleeTimestamp = gm.CurrentTime + fleeCheckIntervalTime;
			if (Vector3.Distance(base.transform.position, targetObject.transform.position) > distanceToFlee)
			{
				GetRandomTargetPosition();
				return;
			}
			ClearPaths();
			Flee();
		}
	}

	private void Flee()
	{
		float num = 100f;
		while ((num -= 1f) > 0f)
		{
			float f = (float)UnityEngine.Random.Range(0, 360) * ((float)Math.PI / 180f);
			float num2 = UnityEngine.Random.Range(4, 7);
			Vector3 vector = targetObject.transform.position + new Vector3(num2 * Mathf.Cos(f), num2 * Mathf.Sin(f));
			Vector3 vector2 = Vector3.Normalize(vector - targetObject.transform.position);
			RaycastHit2D raycastHit2D = Physics2D.CircleCast(targetObject.transform.position, 0.5f, vector2, num2, layerToCheck);
			if (raycastHit2D.collider != null)
			{
				if (Vector3.Distance(targetObject.transform.position, raycastHit2D.centroid) > 3f)
				{
					givePath(vector);
				}
			}
			else
			{
				givePath(vector);
			}
		}
	}

	private IEnumerator DelayedDestroy()
	{
		yield return new WaitForSeconds(1f);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void GetRandomTargetPosition()
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

	private void OnDrawGizmos()
	{
		if (patrol)
		{
			if (!Application.isPlaying)
			{
				int num = -1;
				while (++num < patrolRoute.Count)
				{
					if (num == patrolRoute.Count - 1 || num == 0)
					{
						Utils.DrawLine(base.transform.position, base.transform.position + patrolRoute[num], Color.yellow);
					}
					if (num > 0)
					{
						Utils.DrawLine(base.transform.position + patrolRoute[num - 1], base.transform.position + patrolRoute[num], Color.yellow);
					}
					Utils.DrawCircleXY(base.transform.position + patrolRoute[num], 0.2f, Color.yellow);
				}
			}
			else
			{
				int num2 = -1;
				while (++num2 < patrolRoute.Count)
				{
					if (num2 == patrolRoute.Count - 1 || num2 == 0)
					{
						Utils.DrawLine(startPosition, startPosition + patrolRoute[num2], Color.yellow);
					}
					if (num2 > 0)
					{
						Utils.DrawLine(startPosition + patrolRoute[num2 - 1], startPosition + patrolRoute[num2], Color.yellow);
					}
					Utils.DrawCircleXY(startPosition + patrolRoute[num2], 0.2f, Color.yellow);
				}
			}
		}
		Gradient gradient = new Gradient();
		GradientColorKey[] array = new GradientColorKey[2];
		GradientAlphaKey[] array2 = new GradientAlphaKey[2];
		array = new GradientColorKey[2];
		array[0].color = Color.red;
		array[0].time = 0f;
		array[1].color = Color.blue;
		array[1].time = 1f;
		array2 = new GradientAlphaKey[2];
		array2[0].alpha = 1f;
		array2[0].time = 0f;
		array2[1].alpha = 1f;
		array2[1].time = 1f;
		gradient.SetKeys(array, array2);
		int num3 = -1;
		while (++num3 < enemyRounds.Count)
		{
			RoundsOfEnemies roundsOfEnemies = enemyRounds[num3];
			if (!roundsOfEnemies.DisplayGizmo)
			{
				continue;
			}
			foreach (EnemyRounds.EnemyAndPosition item in roundsOfEnemies.Round)
			{
				Utils.DrawCircleXY(item.Position, 0.2f, gradient.Evaluate((float)num3 / (float)enemyRounds.Count));
			}
		}
	}
}
