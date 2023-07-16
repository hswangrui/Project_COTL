using System;
using System.Collections;
using System.Collections.Generic;
using CotL.Projectiles;
using DG.Tweening;
using I2.Loc;
using Spine;
using Spine.Unity;
using src.Data;
using src.Managers;
using Unify;
using UnityEngine;

public class EnemyDeathCatBoss : UnitObject
{
	[Serializable]
	private struct Cloneshot
	{
		public DeathCatClone.AttackType AttackType;

		public int CloneIndex;

		public Vector3 Position;

		public bool Moving;
	}

	[Serializable]
	private struct CloneWave
	{
		public Cloneshot[] CloneShots;

		public float StartDelay;

		public float EndDelay;
	}

	public enum AttackType
	{
		None,
		LineVertical,
		RingsMulti,
		Pattern2,
		LinesMulti,
		SidePattern1,
		RockFall,
		TargetedBombs,
		Melee,
		ProjectilePattern1,
		TrapPattern0,
		TrapPattern1,
		TrapPattern2
	}

	public static EnemyDeathCatBoss Instance;

	public SkeletonAnimation Spine;

	public SkeletonAnimation BaseFormSpine;

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
	private string handSlamAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string summonAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string spawnAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string dieNoHeartAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string dieHeartAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string deadNoHeartAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string deadHeartAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string handStartAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string handLoopAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string handEndAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string meleeAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string hurtAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string downStartAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string downIdleAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string downAttackAnticipateAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string downAttackAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string downEndAnimation;

	[SerializeField]
	private GameObject trapPrefab;

	[SerializeField]
	private float projectilePatternAnticipation;

	[SerializeField]
	private ProjectilePatternBase projectilePattern1;

	[SerializeField]
	private ProjectilePatternBase projectilePattern2;

	[SerializeField]
	private ProjectilePatternBase projectilePattern3;

	[SerializeField]
	private ProjectilePatternBase projectilePattern4;

	[SerializeField]
	private ProjectilePatternBase leftSidePattern1;

	[SerializeField]
	private ProjectilePatternBase rightSidePattern1;

	[SerializeField]
	private Vector2 poisonAmount;

	[SerializeField]
	private float poisonDelayBetween;

	[SerializeField]
	private PoisonBomb poisonBombPrefab;

	[Space]
	[SerializeField]
	private float projectilePatternLineDuration;

	[SerializeField]
	private float projectilePatternLineSpeed;

	[SerializeField]
	private float projectilePatternLineAcceleration;

	[SerializeField]
	private ProjectileLine projectilePatternLine;

	[SerializeField]
	private float projectilePatternLineVerticalAnticipation;

	[SerializeField]
	private float projectilePatternLineVerticalSpeed;

	[SerializeField]
	private ProjectileLine projectilePatternVertical;

	[Space]
	[SerializeField]
	private float projectileRingBigDuration;

	[SerializeField]
	private float projectileRingBigSpeed;

	[SerializeField]
	private float projectileRingBigAcceleration;

	[SerializeField]
	private float projectileRingBigLifetime;

	[SerializeField]
	private ProjectileCircle projectileRingBig;

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

	[SerializeField]
	private ColliderEvents meleeCollider;

	[SerializeField]
	private TrapRockFall rockFallPrefab;

	[SerializeField]
	private float rockFallDuration;

	[SerializeField]
	private Vector2 rockFallAmount;

	[SerializeField]
	private Vector2 rockFallDelay;

	[SerializeField]
	private MortarBomb bombPrefab;

	[SerializeField]
	private float bombMoveDuration;

	[SerializeField]
	private Vector2 bombAmount;

	[SerializeField]
	private Vector2 bombDelayBetween;

	[SerializeField]
	private DeathCatClone clonePrefab;

	[SerializeField]
	private CloneWave[] cloneAttack1;

	[SerializeField]
	private CloneWave[] cloneAttack2;

	[SerializeField]
	private CloneWave[] cloneAttack3;

	[SerializeField]
	private CloneWave[] cloneAttack4;

	[SerializeField]
	private CloneWave[] cloneAttack5;

	[SerializeField]
	private EnemyRoundsBase enemyRounds1;

	[SerializeField]
	private EnemyRoundsBase enemyRounds2;

	[SerializeField]
	private EnemyRoundsBase enemyRounds3;

	[SerializeField]
	private EnemyRoundsBase enemyRounds4;

	[SerializeField]
	private GameObject cameraBone;

	[SerializeField]
	private GameObject cinematicBone;

	[SerializeField]
	private GameObject middleBone1;

	[SerializeField]
	private GameObject middleBone2;

	[SerializeField]
	private GameObject enemyDeathCatEyesManager;

	[SerializeField]
	private Collider2D blockingCollider;

	private List<DeathCatClone> currentDeadClones = new List<DeathCatClone>();

	private List<GameObject> spawnedEnemies = new List<GameObject>();

	private Coroutine currentMainAttackRoutine;

	private float attackTimestamp;

	private AttackType previousAttackType;

	public GameObject CameraBone
	{
		get
		{
			return cameraBone;
		}
	}

	public GameObject CinematicBone
	{
		get
		{
			return cinematicBone;
		}
	}

	public List<DeathCatClone> CurrentActiveClones { get; private set; } = new List<DeathCatClone>();


	public bool CanAttack { get; set; }

	public override void Awake()
	{
		base.Awake();
		InitializeProjectilePatternRings();
		InitializeMortarStrikes();
		InitializeTraps();
		InitializeProjectileLines();
	}

	private void Start()
	{
		enemyRounds1.SpawnDelay = 0.5f;
		enemyRounds2.SpawnDelay = 0.5f;
		enemyRounds3.SpawnDelay = 0.5f;
		enemyRounds4.SpawnDelay = 0.5f;
		attackTimestamp = GameManager.GetInstance().CurrentTime + 10f;
		Spine.AnimationState.Event += AnimationState_Event;
		meleeCollider.OnTriggerEnterEvent += MeleeColliderHit;
		meleeCollider.gameObject.SetActive(false);
		GetComponent<Health>().invincible = true;
		enemyDeathCatEyesManager.SetActive(true);
		foreach (EnemyDeathCatEye eye in enemyDeathCatEyesManager.GetComponent<EnemyDeathCatEyesManager>().Eyes)
		{
			eye.enabled = true;
		}
		Collider2D[] components = GetComponents<Collider2D>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = false;
		}
	}

	private void MeleeColliderHit(Collider2D collider)
	{
		if (collider.tag == "Player")
		{
			PlayerFarming.Instance.health.DealDamage(1f, base.gameObject, PlayerFarming.Instance.transform.position);
		}
	}

	private void AnimationState_Event(TrackEntry trackEntry, global::Spine.Event e)
	{
		if (e.Data.Name == "slam")
		{
			CameraManager.instance.ShakeCameraForDuration(2f, 2f, 0.5f);
			MMVibrate.Rumble(1f, 2f, 0.5f, this);
		}
		else if (e.Data.Name == "Deathcat SweepAttack")
		{
			meleeCollider.SetActive(true);
			StartCoroutine(DelayCallback(0.1f, delegate
			{
				meleeCollider.SetActive(false);
			}));
		}
	}

	private IEnumerator DelayCallback(float delay, System.Action callback)
	{
		yield return new WaitForSeconds(delay);
		if (callback != null)
		{
			callback();
		}
	}

	public override void Update()
	{
		base.Update();
		if (health.HP <= 0f)
		{
			return;
		}
		if (currentMainAttackRoutine == null && attackTimestamp == -1f)
		{
			attackTimestamp = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(1.5f, 2f);
		}
		if (currentMainAttackRoutine == null && CanAttack)
		{
			if (GameManager.GetInstance().CurrentTime > attackTimestamp && attackTimestamp != -1f)
			{
				SecondaryAttack();
			}
			else if (attackTimestamp == -1f)
			{
				attackTimestamp = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(1.5f, 2f);
			}
		}
		else
		{
			attackTimestamp = -1f;
		}
	}

	public void BeginPhase1()
	{
		UIBossHUD.Play(health, ScriptLocalization.NAMES.DeathNPC);
	}

	public void MainAttack(System.Action callback)
	{
		attackTimestamp = -1f;
		AttackType attackType = previousAttackType;
		while (attackType == previousAttackType)
		{
			switch (UnityEngine.Random.Range((DataManager.Instance.PlayerFleece == 9) ? 1 : 0, 4))
			{
			case 0:
				attackType = AttackType.LinesMulti;
				break;
			case 1:
				attackType = AttackType.RingsMulti;
				break;
			case 2:
				attackType = AttackType.Pattern2;
				break;
			case 3:
				attackType = AttackType.SidePattern1;
				break;
			}
		}
		Attack(attackType);
		StartCoroutine(WaitForAttackToFinish(callback));
	}

	private IEnumerator WaitForAttackToFinish(System.Action callback)
	{
		while (currentMainAttackRoutine != null)
		{
			yield return null;
		}
		if (callback != null)
		{
			callback();
		}
	}

	public void SecondaryAttack()
	{
		attackTimestamp = -1f;
		AttackType attackType = previousAttackType;
		while (attackType == previousAttackType)
		{
			switch (UnityEngine.Random.Range(0, 3))
			{
			case 0:
				attackType = AttackType.ProjectilePattern1;
				break;
			case 1:
				attackType = AttackType.TrapPattern0;
				break;
			case 2:
				attackType = AttackType.TrapPattern1;
				break;
			}
		}
		if (EnemyDeathCatEyesManager.Instance.Eyes.Count == 1 && UnityEngine.Random.Range(0, 100) <= 60)
		{
			attackType = previousAttackType;
			while (attackType == previousAttackType)
			{
				switch (UnityEngine.Random.Range((DataManager.Instance.PlayerFleece == 9) ? 1 : 0, 4))
				{
				case 0:
					attackType = AttackType.LinesMulti;
					break;
				case 1:
					attackType = AttackType.RingsMulti;
					break;
				case 2:
					attackType = AttackType.Pattern2;
					break;
				case 3:
					attackType = AttackType.SidePattern1;
					break;
				}
			}
		}
		Attack(attackType);
	}

	private void Attack(AttackType attackType)
	{
		previousAttackType = attackType;
		switch (attackType)
		{
		case AttackType.LinesMulti:
		{
			Coroutine coroutine = StartCoroutine(ShootProjectileLinesMultiIE());
			currentMainAttackRoutine = coroutine;
			break;
		}
		case AttackType.RingsMulti:
		{
			Coroutine coroutine = StartCoroutine(ShootProjectileRingsMultiIE());
			currentMainAttackRoutine = coroutine;
			break;
		}
		case AttackType.Pattern2:
		{
			Coroutine coroutine = StartCoroutine(ProjectilePattern2IE());
			currentMainAttackRoutine = coroutine;
			break;
		}
		case AttackType.SidePattern1:
		{
			Coroutine coroutine = StartCoroutine(ProjectileSidePattern1IE());
			currentMainAttackRoutine = coroutine;
			break;
		}
		case AttackType.Melee:
		{
			Coroutine coroutine = StartCoroutine(MeleeAttackIE());
			currentMainAttackRoutine = coroutine;
			break;
		}
		case AttackType.LineVertical:
			currentMainAttackRoutine = StartCoroutine(ShootProjectileLineIE(true));
			break;
		case AttackType.RockFall:
			currentMainAttackRoutine = StartCoroutine(ScatterRockFallIE());
			break;
		case AttackType.TargetedBombs:
			currentMainAttackRoutine = StartCoroutine(TargetedBombsIE());
			break;
		case AttackType.ProjectilePattern1:
			currentMainAttackRoutine = StartCoroutine(ProjectilePattern1IE());
			break;
		case AttackType.TrapPattern0:
			currentMainAttackRoutine = StartCoroutine(TrapPattern0IE(0f));
			break;
		case AttackType.TrapPattern1:
			currentMainAttackRoutine = StartCoroutine(TrapPattern1IE(0f));
			break;
		}
	}

	public void MeleeAttack()
	{
		currentMainAttackRoutine = StartCoroutine(ProjectilePattern1IE());
	}

	private IEnumerator MeleeAttackIE()
	{
		Spine.AnimationState.SetAnimation(0, meleeAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(5f);
		currentMainAttackRoutine = null;
	}

	public void EnemyRounds1()
	{
		GameManager.GetInstance().StartCoroutine(EnemyRounds1IE());
	}

	private IEnumerator EnemyRounds1IE()
	{
		yield return new WaitForSeconds(0.5f);
		enemyRounds1.gameObject.SetActive(true);
		bool finished = false;
		enemyRounds1.BeginCombat(false, delegate
		{
			finished = true;
		});
		while (!finished)
		{
			yield return null;
		}
	}

	public void EnemyRounds2()
	{
		StartCoroutine(EnemyRounds2IE());
	}

	private IEnumerator EnemyRounds2IE()
	{
		Spine.AnimationState.SetAnimation(0, spawnAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(0.5f);
		enemyRounds2.gameObject.SetActive(true);
		bool finished = false;
		enemyRounds2.BeginCombat(false, delegate
		{
			finished = true;
		});
		while (!finished)
		{
			yield return null;
		}
	}

	public void EnemyRounds3()
	{
		StartCoroutine(EnemyRounds2IE());
	}

	private IEnumerator EnemyRounds3IE()
	{
		Spine.AnimationState.SetAnimation(0, spawnAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(0.5f);
		enemyRounds3.gameObject.SetActive(true);
		bool finished = false;
		enemyRounds3.BeginCombat(false, delegate
		{
			finished = true;
		});
		while (!finished)
		{
			yield return null;
		}
	}

	public void EnemyRounds4()
	{
		StartCoroutine(EnemyRounds4IE());
	}

	private IEnumerator EnemyRounds4IE()
	{
		Spine.AnimationState.SetAnimation(0, spawnAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(0.5f);
		enemyRounds4.gameObject.SetActive(true);
		bool finished = false;
		enemyRounds4.BeginCombat(false, delegate
		{
			finished = true;
		});
		while (!finished)
		{
			yield return null;
		}
	}

	private void InitializeTraps()
	{
		ObjectPool.CreatePool(trapPrefab, 40);
	}

	public void TrapPattern0()
	{
		StartCoroutine(TrapPattern0IE(0f));
	}

	private IEnumerator TrapPattern0IE(float delay)
	{
		yield return new WaitForSeconds(delay);
		Spine.AnimationState.SetAnimation(0, summonAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(0.5f);
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= 10)
			{
				break;
			}
			for (int j = 0; j < 2; j++)
			{
				GameObject obj = ObjectPool.Spawn(trapPrefab);
				TrapSpikesSpawnOthers component = obj.GetComponent<TrapSpikesSpawnOthers>();
				component.Spine.Skeleton.SetSkin("White");
				component.OverrideColor = Color.black;
				float num2 = i * 2;
				float t = (float)j / 1f;
				float x = Mathf.Lerp(-5.5f, 5.5f, t);
				Vector3 vector = Vector3.down * num2;
				vector.x = x;
				vector.x += UnityEngine.Random.Range(-0.5f, 0.5f);
				obj.transform.position = base.transform.position + vector;
			}
			CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
			yield return new WaitForSeconds(0.2f);
		}
		yield return new WaitForSeconds(5f);
		currentMainAttackRoutine = null;
	}

	public void TrapPattern1()
	{
		StartCoroutine(TrapPattern1IE(0f));
	}

	private IEnumerator TrapPattern1IE(float delay)
	{
		yield return new WaitForSeconds(delay);
		Spine.AnimationState.SetAnimation(0, summonAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(0.5f);
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= 13)
			{
				break;
			}
			for (int j = 0; j < 2; j++)
			{
				GameObject obj = ObjectPool.Spawn(trapPrefab);
				TrapSpikesSpawnOthers component = obj.GetComponent<TrapSpikesSpawnOthers>();
				component.Spine.Skeleton.SetSkin("White");
				component.OverrideColor = Color.black;
				float num2 = i * 2;
				float t = (float)j / 1f;
				float x = Mathf.Lerp(-12f, 12f, t);
				float y = Mathf.Lerp(-10f, -2f, t);
				Vector3 vector = new Vector2(x, y);
				Vector3 vector2 = ((j == 0) ? Vector3.right : Vector3.left) * num2;
				vector2 += vector;
				vector2.y += UnityEngine.Random.Range(-0.5f, 0.5f);
				obj.transform.position = base.transform.position + vector2;
			}
			CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
			yield return new WaitForSeconds(0.2f);
		}
		yield return new WaitForSeconds(5f);
		currentMainAttackRoutine = null;
	}

	public void TrapPattern2()
	{
		StartCoroutine(TrapPattern2IE());
	}

	private IEnumerator TrapPattern2IE()
	{
		Spine.AnimationState.SetAnimation(0, summonAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(0.5f);
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
			while (++num2 < 4)
			{
				GameObject obj = ObjectPool.Spawn(trapPrefab);
				TrapSpikesSpawnOthers component = obj.GetComponent<TrapSpikesSpawnOthers>();
				component.Spine.Skeleton.SetSkin("White");
				component.OverrideColor = Color.black;
				float num3 = 90 * num2;
				float num4 = i * 2;
				float num5 = UnityEngine.Random.Range(-0.5f, 0.5f);
				Vector3 vector = Utils.DegreeToVector2(num3 + 90f) * num5;
				Vector3 vector2 = new Vector3(num4 * Mathf.Cos(num3 * ((float)Math.PI / 180f)), num4 * Mathf.Sin(num3 * ((float)Math.PI / 180f)));
				obj.transform.position = vector2 + vector;
			}
			CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
			yield return new WaitForSeconds(0.2f);
		}
		yield return new WaitForSeconds(5f);
		currentMainAttackRoutine = null;
	}

	public void TrapPattern3()
	{
		StartCoroutine(TrapPattern3IE());
	}

	private IEnumerator TrapPattern3IE()
	{
		Spine.AnimationState.SetAnimation(0, summonAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(0.5f);
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= 13)
			{
				break;
			}
			for (int j = 0; j < 8; j++)
			{
				GameObject obj = ObjectPool.Spawn(trapPrefab);
				TrapSpikesSpawnOthers component = obj.GetComponent<TrapSpikesSpawnOthers>();
				component.Spine.Skeleton.SetSkin("White");
				component.OverrideColor = Color.black;
				float num2 = i * 2;
				float t = (float)j / 7f;
				float num3 = ((j % 2 != 0) ? 1 : 0);
				float x = Mathf.Lerp(-12f, 12f, num3);
				float y = Mathf.Lerp(-12f, 0f, t);
				Vector3 vector = new Vector2(x, y);
				Vector3 vector2 = ((num3 == 0f) ? Vector3.right : Vector3.left) * num2;
				vector2 += vector;
				vector2.y += UnityEngine.Random.Range(-0.5f, 0.5f);
				obj.transform.position = base.transform.position + vector2;
			}
			CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
			yield return new WaitForSeconds(0.2f);
		}
		yield return new WaitForSeconds(5f);
		currentMainAttackRoutine = null;
	}

	public void TrapPattern4()
	{
		StartCoroutine(TrapPattern4IE());
	}

	private IEnumerator TrapPattern4IE()
	{
		Spine.AnimationState.SetAnimation(0, summonAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(0.5f);
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= 10)
			{
				break;
			}
			for (int j = 0; j < 10; j++)
			{
				GameObject obj = ObjectPool.Spawn(trapPrefab);
				TrapSpikesSpawnOthers component = obj.GetComponent<TrapSpikesSpawnOthers>();
				component.Spine.Skeleton.SetSkin("White");
				component.OverrideColor = Color.black;
				float num2 = i * 2;
				float t = (float)j / 9f;
				float x = Mathf.Lerp(-12f, 12f, t);
				Vector3 vector = Vector3.down * num2;
				vector.x = x;
				vector.x += UnityEngine.Random.Range(-0.5f, 0.5f);
				obj.transform.position = base.transform.position + vector;
			}
			CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
			yield return new WaitForSeconds(0.2f);
		}
		yield return new WaitForSeconds(5f);
		currentMainAttackRoutine = null;
	}

	public void TrapPattern5()
	{
		StartCoroutine(TrapPattern5IE());
	}

	private IEnumerator TrapPattern5IE()
	{
		Spine.AnimationState.SetAnimation(0, summonAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(TrapPatternTargeted(new Vector3(-12f, 7f)));
		StartCoroutine(TrapPatternTargeted(new Vector3(12f, 7f)));
		StartCoroutine(TrapPatternTargeted(new Vector3(-12f, -7f)));
		yield return StartCoroutine(TrapPatternTargeted(new Vector3(12f, -7f)));
		yield return new WaitForSeconds(5f);
		currentMainAttackRoutine = null;
	}

	private IEnumerator TrapPatternTargeted(Vector3 startingPosition)
	{
		Vector3 Position = Vector3.zero;
		int i = -1;
		float Dist = 1f;
		state.facingAngle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
		float facingAngle = state.facingAngle;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num < 10)
			{
				GameObject obj = ObjectPool.Spawn(trapPrefab);
				obj.GetComponent<TrapSpikesSpawnOthers>();
				float angle = Utils.GetAngle(startingPosition + Position, PlayerFarming.Instance.transform.position);
				Position += new Vector3(Dist * Mathf.Cos(angle * ((float)Math.PI / 180f)), Dist * Mathf.Sin(angle * ((float)Math.PI / 180f)));
				obj.transform.position = startingPosition + Position;
				CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
				yield return new WaitForSeconds(0.2f);
				continue;
			}
			break;
		}
	}

	public void ProjectilePattern1()
	{
		currentMainAttackRoutine = StartCoroutine(ProjectilePattern1IE());
	}

	private IEnumerator ProjectilePattern1IE()
	{
		Spine.AnimationState.SetAnimation(0, handSlamAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(projectilePatternAnticipation);
		yield return StartCoroutine(projectilePattern1.ShootIE());
		yield return new WaitForSeconds(5f);
		currentMainAttackRoutine = null;
	}

	public void ProjectilePattern2()
	{
		currentMainAttackRoutine = StartCoroutine(ProjectilePattern2IE());
	}

	private IEnumerator ProjectilePattern2IE()
	{
		Spine.AnimationState.SetAnimation(0, handSlamAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(projectilePatternAnticipation);
		yield return StartCoroutine(projectilePattern2.ShootIE());
		yield return new WaitForSeconds(6f);
		currentMainAttackRoutine = null;
	}

	public void ProjectilePattern3()
	{
		currentMainAttackRoutine = StartCoroutine(ProjectilePattern3IE());
	}

	private IEnumerator ProjectilePattern3IE()
	{
		Spine.AnimationState.SetAnimation(0, handSlamAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(projectilePatternAnticipation);
		yield return StartCoroutine(projectilePattern3.ShootIE());
		yield return new WaitForSeconds(5f);
		currentMainAttackRoutine = null;
	}

	public void ProjectilePattern4()
	{
		currentMainAttackRoutine = StartCoroutine(ProjectilePattern4IE());
	}

	private IEnumerator ProjectilePattern4IE()
	{
		Spine.AnimationState.SetAnimation(0, handSlamAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(projectilePatternAnticipation);
		yield return StartCoroutine(projectilePattern4.ShootIE());
		yield return new WaitForSeconds(5f);
		currentMainAttackRoutine = null;
	}

	public void ProjectileSidePattern1()
	{
		currentMainAttackRoutine = StartCoroutine(ProjectileSidePattern1IE());
	}

	private IEnumerator ProjectileSidePattern1IE()
	{
		Spine.AnimationState.SetAnimation(0, handStartAnimation, false);
		Spine.AnimationState.AddAnimation(0, handLoopAnimation, true, 0f);
		yield return new WaitForSeconds(projectilePatternAnticipation);
		StartCoroutine(leftSidePattern1.ShootIE());
		StartCoroutine(rightSidePattern1.ShootIE());
		yield return new WaitForSeconds(4f);
		Spine.AnimationState.SetAnimation(0, handEndAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(1f);
		currentMainAttackRoutine = null;
	}

	private void InitializeProjectileLines()
	{
		ObjectPool.CreatePool(projectilePatternVertical, 4);
		ObjectPool.CreatePool(projectilePatternLine, 2);
	}

	private void ShootProjectileLine()
	{
		currentMainAttackRoutine = StartCoroutine(ShootProjectileLineIE(true));
	}

	private IEnumerator ShootProjectileLineIE(bool clearCoroutines)
	{
		Spine.AnimationState.SetAnimation(0, handSlamAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(projectilePatternAnticipation);
		int num = ((UnityEngine.Random.value > 0.5f) ? 1 : (-1));
		Projectile component = ObjectPool.Spawn(projectilePatternLine, base.transform.parent).GetComponent<Projectile>();
		component.transform.position = ((num == 1) ? (base.transform.position + Vector3.down * 2f) : (base.transform.position + Vector3.down * 11.5f));
		component.Angle = 0f;
		component.health = health;
		component.team = Health.Team.Team2;
		component.Speed = projectilePatternLineSpeed * (float)num;
		component.Acceleration = projectilePatternLineAcceleration;
		component.GetComponent<ProjectileLine>().InitDelayed(PlayerFarming.Instance.gameObject, 0f, 270f);
		yield return new WaitForSeconds(projectilePatternLineDuration);
		yield return new WaitForSeconds(1f);
		if (clearCoroutines)
		{
			currentMainAttackRoutine = null;
		}
	}

	private void ShootProjectileLinesVeritcal()
	{
		currentMainAttackRoutine = StartCoroutine(ShootProjectileLinesMultiIE());
	}

	private IEnumerator ShootProjectileLinesMultiIE()
	{
		for (int t = 0; t < 2; t++)
		{
			Spine.AnimationState.SetAnimation(0, handSlamAnimation, false);
			Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
			yield return new WaitForSeconds(projectilePatternAnticipation);
			for (int i = 0; i < 2; i++)
			{
				Projectile component = ObjectPool.Spawn(projectilePatternVertical, base.transform.parent).GetComponent<Projectile>();
				component.transform.position = new Vector3((i == 0) ? (-10) : 10, 0f, 0f);
				component.Angle = 0f;
				component.health = health;
				component.team = Health.Team.Team2;
				component.Speed = projectilePatternLineVerticalSpeed;
				component.Acceleration = 0f;
				component.GetComponent<ProjectileLine>().InitDelayed(PlayerFarming.Instance.gameObject, 0f, (i != 0) ? 180 : 0);
			}
			StartCoroutine(ShootProjectileLineIE(false));
			yield return new WaitForSeconds(projectilePatternLineVerticalAnticipation);
		}
		yield return new WaitForSeconds(projectilePatternLineDuration);
		yield return new WaitForSeconds(2.5f);
		currentMainAttackRoutine = null;
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

	private void ShootProjectileRingsMulti()
	{
		currentMainAttackRoutine = StartCoroutine(ShootProjectileRingsMultiIE());
	}

	private IEnumerator ShootProjectileRingsMultiIE()
	{
		Spine.AnimationState.SetAnimation(0, handSlamAnimation, true);
		yield return new WaitForSeconds(0.5f);
		List<float> list = new List<float> { 1f, 3f, 2f };
		List<float> list2 = new List<float> { 2f, 3f, 1f };
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				Vector3 position = new Vector3((j == 0) ? (-10) : 10, 3f * (float)(i - 1), 0f);
				float num = (float)i / 2f;
				float angleToPlayer = GetAngleToPlayer();
				Vector3 vector = (Vector3)Utils.DegreeToVector2(angleToPlayer);
				Projectile component = ObjectPool.Spawn(projectilePatternRings, base.transform.parent).GetComponent<Projectile>();
				component.transform.position = position;
				component.Angle = angleToPlayer;
				component.health = health;
				component.team = Health.Team.Team2;
				component.Speed = projectilePatternRingsSpeed;
				component.Acceleration = projectilePatternRingsAcceleration;
				float shootDelay = ((j == 0) ? list[i] : list2[i]);
				component.GetComponent<ProjectileCircleBase>().InitDelayed(PlayerFarming.Instance.gameObject, projectilePatternRingsRadius, shootDelay, delegate
				{
					AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/grenade_mass_launch", base.gameObject);
				});
			}
		}
		yield return new WaitForSeconds(projectilePatternRingsDuration);
		Spine.AnimationState.SetAnimation(0, idleAnimation, true);
		currentMainAttackRoutine = null;
	}

	public void EyeDestroyed()
	{
		if (EnemyDeathCatEyesManager.Instance.Eyes.Count == 0)
		{
			StopAllCoroutines();
		}
		StartCoroutine(HurtIE());
	}

	private IEnumerator HurtIE()
	{
		CanAttack = false;
		EnemyDeathCatEyesManager.Instance.HideAllEyes(0f);
		EnemyDeathCatEyesManager.Instance.Active = false;
		if (EnemyDeathCatEyesManager.Instance.Eyes.Count > 0)
		{
			Spine.AnimationState.SetAnimation(0, hurtAnimation, false);
			Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
			yield return new WaitForSeconds(3f);
			StartCoroutine(WaitForCurrentAttackToFinish(delegate
			{
				MainAttack(delegate
				{
					StartCoroutine(DelayCallback(UnityEngine.Random.Range(1.5f, 2.5f), delegate
					{
						CanAttack = true;
					}));
					EnemyDeathCatEyesManager.Instance.Active = true;
				});
			}));
			yield break;
		}
		CanAttack = false;
		Projectile.ClearProjectiles();
		projectilePattern1.StopAllCoroutines();
		projectilePattern2.StopAllCoroutines();
		projectilePattern3.StopAllCoroutines();
		projectilePattern4.StopAllCoroutines();
		leftSidePattern1.StopAllCoroutines();
		rightSidePattern1.StopAllCoroutines();
		GameManager.GetInstance().CamFollowTarget.MinZoom = 8f;
		GameManager.GetInstance().CamFollowTarget.MaxZoom = 14f;
		GameManager.GetInstance().RemoveFromCamera(middleBone1);
		GameManager.GetInstance().RemoveFromCamera(middleBone2);
		GameManager.GetInstance().AddToCamera(base.gameObject);
		Spine.AnimationState.SetAnimation(0, downStartAnimation, false);
		Spine.AnimationState.AddAnimation(0, downIdleAnimation, true, 0f);
		Spine.transform.DOLocalMoveX(0f, 2f);
		blockingCollider.isTrigger = false;
		blockingCollider.gameObject.SetActive(true);
		yield return new WaitForSeconds(2f);
		Collider2D[] components = GetComponents<Collider2D>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = true;
		}
		Instance.health.invincible = false;
	}

	private IEnumerator WaitForCurrentAttackToFinish(System.Action callback)
	{
		while (currentMainAttackRoutine != null)
		{
			yield return null;
		}
		if (callback != null)
		{
			callback();
		}
	}

	private void ScatterRockFall()
	{
		currentMainAttackRoutine = StartCoroutine(ScatterRockFallIE());
	}

	private IEnumerator ScatterRockFallIE()
	{
		Spine.AnimationState.SetAnimation(0, handSlamAnimation, true);
		yield return new WaitForSeconds(0.5f);
		new List<float> { 1f, 3f, 2f };
		new List<float> { 2f, 3f, 1f };
		int amount = (int)UnityEngine.Random.Range(rockFallAmount.x, rockFallAmount.y);
		int randomTargetPlayerNumber = UnityEngine.Random.Range(0, amount);
		float delay = 0f;
		for (int i = 0; i < amount; i++)
		{
			Vector3 position = UnityEngine.Random.insideUnitCircle * 7f;
			if (i == randomTargetPlayerNumber)
			{
				position = PlayerFarming.Instance.transform.position;
			}
			UnityEngine.Object.Instantiate(rockFallPrefab, position, Quaternion.identity, base.transform.parent).GetComponent<TrapRockFall>().Drop(false);
			if (i != amount - 1)
			{
				float num = UnityEngine.Random.Range(rockFallDelay.x, rockFallDelay.y);
				delay += num;
				yield return new WaitForSeconds(num);
			}
		}
		yield return new WaitForSeconds(rockFallDuration);
		Spine.AnimationState.SetAnimation(0, idleAnimation, true);
		currentMainAttackRoutine = null;
	}

	private void InitializeMortarStrikes()
	{
		List<MortarBomb> list = new List<MortarBomb>();
		for (int i = 0; (float)i < bombAmount.y; i++)
		{
			MortarBomb mortarBomb = ObjectPool.Spawn(bombPrefab, base.transform.parent);
			mortarBomb.destroyOnFinish = false;
			list.Add(mortarBomb);
		}
		for (int j = 0; j < list.Count; j++)
		{
			list[j].gameObject.Recycle();
		}
	}

	private void TargetedBombs()
	{
		currentMainAttackRoutine = StartCoroutine(TargetedBombsIE());
	}

	private IEnumerator TargetedBombsIE()
	{
		Spine.AnimationState.SetAnimation(0, handSlamAnimation, true);
		yield return new WaitForSeconds(0.5f);
		int amount = (int)UnityEngine.Random.Range(bombAmount.x, bombAmount.y);
		int dir = 1;
		for (int i = 0; i < amount; i++)
		{
			yield return StartCoroutine(ShootMortarTarget(dir));
			dir *= -1;
			yield return new WaitForSeconds(UnityEngine.Random.Range(bombDelayBetween.x, bombDelayBetween.y));
		}
		Spine.AnimationState.SetAnimation(0, idleAnimation, true);
		currentMainAttackRoutine = null;
	}

	private IEnumerator ShootMortarTarget(float direction)
	{
		Vector3 position = new Vector3(15f * direction, UnityEngine.Random.Range(-4f, 4f), 2f);
		MortarBomb mortarBomb = ObjectPool.Spawn(bombPrefab, base.transform.parent, (Vector3)AstarPath.active.GetNearest(PlayerFarming.Instance.transform.position).node.position, Quaternion.identity);
		mortarBomb.destroyOnFinish = false;
		mortarBomb.Play(position, bombMoveDuration, Health.Team.Team2);
		yield return new WaitForSeconds(bombMoveDuration);
		AudioManager.Instance.PlayOneShot("event:/boss/frog/mortar_explode");
	}

	private float GetAngleToPlayer()
	{
		return Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		AudioManager.Instance.SetMusicRoomID(5, "deathcat_room_id");
		StopAllCoroutines();
		Projectile.ClearProjectiles();
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("KILL_BOSS_5"));
		foreach (GameObject spawnedEnemy in spawnedEnemies)
		{
			if (spawnedEnemy != null)
			{
				spawnedEnemy.gameObject.SetActive(false);
			}
		}
		foreach (DeathCatClone currentActiveClone in CurrentActiveClones)
		{
			if (currentActiveClone != null && currentActiveClone != this && currentActiveClone.IsFake)
			{
				currentActiveClone.health.DealDamage(currentActiveClone.health.totalHP, base.gameObject, base.transform.position);
			}
		}
		StopAllCoroutines();
		GameManager.GetInstance().StartCoroutine(Die());
		UIBossHUD.Hide();
		GetComponent<Collider2D>().enabled = false;
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.FreeDeathCat);
		DataManager.Instance.DeathCatBeaten = true;
		PersistenceManager.PersistentData.GameCompletionSnapshots.Add(new PersistentData.GameCompletionSnapshot
		{
			Permadeath = DataManager.Instance.PermadeDeathActive,
			Difficulty = (DifficultyManager.Difficulty)DataManager.Instance.MetaData.Difficulty
		});
		PlayerFarming.Instance.playerWeapon.DoSlowMo(false);
	}

	public override void OnEnable()
	{
		base.OnEnable();
		Instance = this;
		currentMainAttackRoutine = null;
		attackTimestamp = -1f;
		GameManager.GetInstance().StartCoroutine(AddToCamera());
	}

	public override void OnDisable()
	{
		base.OnDisable();
		Instance = null;
	}

	private IEnumerator AddToCamera()
	{
		yield return new WaitForSeconds(0.25f);
		while (!GameManager.GetInstance().CamFollowTarget.Contains(middleBone1))
		{
			if (PlayerFarming.Instance != null && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.Dieing && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.GameOver)
			{
				GameManager.GetInstance().RemoveFromCamera(base.gameObject);
				GameManager.GetInstance().AddToCamera(middleBone1);
				GameManager.GetInstance().AddToCamera(middleBone2);
				GameManager.GetInstance().CamFollowTarget.MinZoom = 9f;
				GameManager.GetInstance().CamFollowTarget.MaxZoom = 18f;
			}
			yield return null;
		}
	}

	private IEnumerator Die()
	{
		DeathCatController.Instance.DroppingFervour = false;
		ClearPaths();
		speed = 0f;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(CinematicBone, 12f);
		yield return new WaitForEndOfFrame();
		state.CURRENT_STATE = StateMachine.State.Dieing;
		if (!DataManager.Instance.BossesCompleted.Contains(PlayerFarming.Location))
		{
			Spine.AnimationState.SetAnimation(0, dieHeartAnimation, false);
			Spine.AnimationState.AddAnimation(0, deadHeartAnimation, true, 0f);
		}
		else
		{
			Spine.AnimationState.SetAnimation(0, dieNoHeartAnimation, false);
			Spine.AnimationState.AddAnimation(0, deadNoHeartAnimation, true, 0f);
		}
		yield return new WaitForSeconds(6.6f);
		for (int i = 0; i < 20; i++)
		{
			BiomeConstants.Instance.EmitBloodSplatterGroundParticles(base.transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * 3f), Vector3.zero, Color.red);
		}
		DeathCatController.Instance.DeathCatKilled();
		base.gameObject.SetActive(false);
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		SimpleSpineFlash.FlashFillRed();
	}

	private void MonsterHeart_OnHeartTaken()
	{
	}

	private Vector3 GetRandomPosition(Vector3 fromPosition, float minDistance)
	{
		float x;
		float y;
		do
		{
			x = UnityEngine.Random.Range(-12f, 12f);
			y = UnityEngine.Random.Range(-6.5f, 6.5f);
		}
		while (!(Vector3.Distance(fromPosition, new Vector3(x, y, 0f)) > minDistance));
		return new Vector3(x, y, 0f);
	}

	private void OnDrawGizmosSelected()
	{
		CloneWave[] array = cloneAttack1;
		for (int i = 0; i < array.Length; i++)
		{
			Cloneshot[] cloneShots = array[i].CloneShots;
			for (int j = 0; j < cloneShots.Length; j++)
			{
				Utils.DrawCircleXY(cloneShots[j].Position, 0.5f, Color.green);
			}
		}
		array = cloneAttack2;
		for (int i = 0; i < array.Length; i++)
		{
			Cloneshot[] cloneShots = array[i].CloneShots;
			for (int j = 0; j < cloneShots.Length; j++)
			{
				Utils.DrawCircleXY(cloneShots[j].Position, 0.5f, Color.green);
			}
		}
		array = cloneAttack3;
		for (int i = 0; i < array.Length; i++)
		{
			Cloneshot[] cloneShots = array[i].CloneShots;
			for (int j = 0; j < cloneShots.Length; j++)
			{
				Utils.DrawCircleXY(cloneShots[j].Position, 0.5f, Color.green);
			}
		}
		array = cloneAttack4;
		for (int i = 0; i < array.Length; i++)
		{
			Cloneshot[] cloneShots = array[i].CloneShots;
			for (int j = 0; j < cloneShots.Length; j++)
			{
				Utils.DrawCircleXY(cloneShots[j].Position, 0.5f, Color.green);
			}
		}
		array = cloneAttack5;
		for (int i = 0; i < array.Length; i++)
		{
			Cloneshot[] cloneShots = array[i].CloneShots;
			for (int j = 0; j < cloneShots.Length; j++)
			{
				Utils.DrawCircleXY(cloneShots[j].Position, 0.5f, Color.green);
			}
		}
	}
}
