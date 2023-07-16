using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MMBiomeGeneration;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyJellySpawner : EnemyJellyCharger
{
	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string spawnAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string teleportAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string attackAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string shootAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string shootLongAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string shootLongAnticipateAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string teleportShootAnimation;

	[SerializeField]
	private bool teleportOnHit;

	[SerializeField]
	private float circleCastRadius = 0.5f;

	[SerializeField]
	private float teleportDelay = 0.5f;

	[SerializeField]
	private float teleportMoveDelay = 0.5f;

	[SerializeField]
	private float teleportCooldownDelay = 0.5f;

	[SerializeField]
	private AssetReferenceGameObject[] spawnableEnemies;

	[SerializeField]
	private float randomSpawnMinDelay;

	[SerializeField]
	private float randomSpawnMaxDelay;

	[SerializeField]
	private float spawnSpitOutForce;

	[SerializeField]
	private float spawnDuration;

	[SerializeField]
	private float spawnCooldown;

	[SerializeField]
	private int spawnAmountMin;

	[SerializeField]
	private int spawnAmountMax;

	[SerializeField]
	private float growSpeed;

	[SerializeField]
	private Ease growEase;

	[SerializeField]
	private int maxEnemiesActive;

	[Tooltip("will not spawn when target is within min radius")]
	[SerializeField]
	private float targetMinSpawnRadius;

	[SerializeField]
	private bool randomSpawnDirection = true;

	[SerializeField]
	private bool killSpawnablesOnDeath;

	[SerializeField]
	private bool canAttack;

	[SerializeField]
	private bool attackOnHit = true;

	[SerializeField]
	private float attackDistance;

	[SerializeField]
	private float attackChargeDur;

	[SerializeField]
	private float attackCooldown;

	[SerializeField]
	private float damageDuration = 0.2f;

	[SerializeField]
	private bool canShoot;

	[SerializeField]
	private ProjectilePattern projectilePattern;

	[SerializeField]
	private float projectileAnticipation;

	[SerializeField]
	private Vector2 timeBetweenProjectileShots;

	[Space]
	[SerializeField]
	private bool canShootSwirl;

	[SerializeField]
	private ProjectilePatternBase projectileSwirlPattern;

	[SerializeField]
	private float projectileSwirlAnticipation;

	[SerializeField]
	private Vector2 timeBetweenProjectileSwirlShots;

	[Space]
	[SerializeField]
	private bool canDropBombs;

	[SerializeField]
	private Vector2 bombsToDrop;

	[SerializeField]
	private Vector2 timeBetweenDroppingBombs;

	private float lastProjectileShootingTime;

	private float lastProjectileSwirlShootingTime;

	private float lastDropBombTime;

	private float lastBeamTime;

	private float spawnTime = float.MaxValue;

	private bool spawning;

	private int spawnedAmount;

	private bool teleporting;

	private float attackTimer;

	private bool charging;

	private bool attacking;

	private Coroutine spawnRoutine;

	private ShowHPBar hpBar;

	private List<UnitObject> spawnedEnemies = new List<UnitObject>();

	private Vector3 v3Shake = Vector3.zero;

	private Vector3 v3ShakeSpeed = Vector3.zero;

	public bool ShowDebug;

	public List<Vector3> Points = new List<Vector3>();

	public List<Vector3> PointsLink = new List<Vector3>();

	public List<Vector3> EndPoints = new List<Vector3>();

	public List<Vector3> EndPointsLink = new List<Vector3>();

	protected override void Start()
	{
		base.Start();
		SetSpawnTime();
		hpBar = GetComponent<ShowHPBar>();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		spawning = false;
		teleporting = false;
		charging = false;
		attacking = false;
		health.invincible = false;
		lastProjectileSwirlShootingTime = ((GameManager.GetInstance() != null) ? GameManager.GetInstance().CurrentTime : Time.time) + UnityEngine.Random.Range(timeBetweenProjectileSwirlShots.x, timeBetweenProjectileSwirlShots.y);
		lastDropBombTime = ((GameManager.GetInstance() != null) ? GameManager.GetInstance().CurrentTime : Time.time) + UnityEngine.Random.Range(timeBetweenDroppingBombs.x, timeBetweenDroppingBombs.y);
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		simpleSpineFlash.FlashWhite(false);
		simpleSpineFlash.FlashFillRed();
		if (teleportOnHit && !attacking && (AttackType == Health.AttackTypes.Melee || AttackType == Health.AttackTypes.Projectile))
		{
			StartCoroutine(TeleportIE());
			return;
		}
		if (canAttack && attackOnHit && AttackType == Health.AttackTypes.Melee)
		{
			ChargeAttack();
		}
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		v3ShakeSpeed = base.transform.position - Attacker.transform.position;
		v3ShakeSpeed = v3ShakeSpeed.normalized * 30f;
	}

	public override void Update()
	{
		if (!teleporting)
		{
			base.Update();
		}
		if (Time.deltaTime > 0f)
		{
			v3ShakeSpeed += (Vector3.zero - v3Shake) * 0.4f / Time.deltaTime;
			v3Shake += (v3ShakeSpeed *= 0.7f) * Time.deltaTime;
			Spine.transform.localPosition = v3Shake;
		}
		if (inRange && spawnTime == float.MaxValue)
		{
			SetSpawnTime();
		}
		if (inRange && GameManager.RoomActive && (bool)targetObject && (bool)gm && gm.CurrentTime > spawnTime && !attacking && !spawning && !teleporting)
		{
			SpawnEnemy();
		}
		if (GameManager.RoomActive && (bool)gm && gm.CurrentTime > lastProjectileShootingTime && canShoot && !attacking && !spawning && !teleporting && UnityEngine.Random.value < 0.3f)
		{
			ShootProjectile();
		}
		if (GameManager.RoomActive && (bool)gm && gm.CurrentTime > lastProjectileSwirlShootingTime && canShootSwirl && !attacking && !spawning && !teleporting && UnityEngine.Random.value < 0.3f)
		{
			ShootSwirl();
		}
		if (GameManager.RoomActive && (bool)gm && gm.CurrentTime > lastDropBombTime && canDropBombs && !attacking && !spawning && !teleporting && UnityEngine.Random.value < 0.3f)
		{
			SpawnTrapBombs();
		}
		if ((bool)targetObject)
		{
			float num = Vector3.Distance(targetObject.transform.position, base.transform.position);
			inRange = num < (float)VisionRange;
		}
		if (!canAttack)
		{
			return;
		}
		if (charging)
		{
			attackTimer += Time.deltaTime;
			float num2 = attackTimer / attackChargeDur;
			simpleSpineFlash.FlashWhite(num2);
			if (num2 > 1f)
			{
				Attack();
			}
		}
		if ((bool)targetObject && !charging && !attacking && Vector3.Distance(base.transform.position, targetObject.transform.position) < attackDistance)
		{
			ChargeAttack();
		}
	}

	protected override void UpdateMoving()
	{
		if (!attacking && !spawning && !teleporting)
		{
			base.UpdateMoving();
		}
	}

	private void ChargeAttack()
	{
		if (!attacking && !spawning)
		{
			charging = true;
			attackTimer = 0f;
			state.CURRENT_STATE = StateMachine.State.Charging;
			Spine.AnimationState.SetAnimation(0, anticipationAnimation, true);
			AudioManager.Instance.PlayOneShot("event:/enemy/chaser/chaser_charge", base.transform.position);
		}
	}

	private void SpawnEnemy()
	{
		if (!spawning && !charging && !attacking && !teleporting && spawnableEnemies.Length != 0 && spawnedAmount < maxEnemiesActive && (targetMinSpawnRadius == 0f || Vector3.Distance(base.transform.position, targetObject.transform.position) > targetMinSpawnRadius))
		{
			spawnRoutine = StartCoroutine(SpawnDelay());
		}
	}

	private IEnumerator SpawnDelay()
	{
		ClearPaths();
		spawning = true;
		Spine.AnimationState.SetAnimation(0, anticipationAnimation, true);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < spawnDuration))
			{
				break;
			}
			yield return null;
		}
		int num2 = UnityEngine.Random.Range(spawnAmountMin, spawnAmountMax + 1);
		UnityEngine.Random.Range(0, 360);
		AudioManager.Instance.PlayOneShot("event:/enemy/chaser_boss/chaser_boss_egg_spawn", base.transform.position);
		for (int i = 0; i < num2; i++)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(spawnableEnemies[UnityEngine.Random.Range(0, spawnableEnemies.Length)], GetRandomSpawnPosition(), Quaternion.identity, base.transform.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				EnemyExploder component = obj.Result.GetComponent<EnemyExploder>();
				component.gameObject.SetActive(false);
				EnemySpawner.CreateWithAndInitInstantiatedEnemy(obj.Result.transform.position, base.transform.parent, obj.Result);
				component.health.OnDie += OnEnemyKilled;
				EnemyRoundsBase instance = EnemyRoundsBase.Instance;
				if ((object)instance != null)
				{
					instance.AddEnemyToRound(component.GetComponent<Health>());
				}
				Interaction_Chest instance2 = Interaction_Chest.Instance;
				if ((object)instance2 != null)
				{
					instance2.AddEnemy(component.health);
				}
				if (growSpeed != 0f)
				{
					component.Spine.transform.localScale = Vector3.zero;
					component.Spine.transform.DOScale(1f, growSpeed).SetEase(growEase);
				}
				spawnedAmount++;
				DropLootOnDeath component2 = component.GetComponent<DropLootOnDeath>();
				if ((bool)component2)
				{
					component2.GiveXP = false;
				}
				spawnedEnemies.Add(component);
			};
		}
		Spine.AnimationState.SetAnimation(0, spawnAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < spawnCooldown))
			{
				break;
			}
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		SetSpawnTime();
		spawning = false;
		spawnRoutine = null;
	}

	private Vector3 GetRandomSpawnPosition()
	{
		return new Vector3(UnityEngine.Random.Range(-6.5f, 6.5f), UnityEngine.Random.Range(-3.5f, 3.5f), 0f);
	}

	private new void OnEnemyKilled(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		spawnedAmount--;
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		if (!killSpawnablesOnDeath || !GetComponentInParent<MiniBossController>())
		{
			return;
		}
		if (spawnRoutine != null)
		{
			StopCoroutine(spawnRoutine);
			spawning = false;
		}
		foreach (UnitObject spawnedEnemy in spawnedEnemies)
		{
			if (spawnedEnemy != null)
			{
				spawnedEnemy.enabled = true;
				spawnedEnemy.health.DealDamage(spawnedEnemy.health.totalHP, base.gameObject, base.transform.position, false, Health.AttackTypes.Heavy, true);
			}
		}
	}

	private void SetSpawnTime()
	{
		if ((bool)gm)
		{
			spawnTime = gm.CurrentTime + UnityEngine.Random.Range(randomSpawnMinDelay, randomSpawnMaxDelay);
		}
	}

	private IEnumerator TeleportIE()
	{
		if (teleporting)
		{
			yield break;
		}
		if (spawning && spawnRoutine != null)
		{
			StopCoroutine(spawnRoutine);
			spawning = false;
		}
		teleporting = true;
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < teleportDelay))
			{
				break;
			}
			yield return null;
		}
		ClearPaths();
		Spine.AnimationState.SetAnimation(0, teleportAnimation, false);
		yield return new WaitForEndOfFrame();
		if ((bool)hpBar)
		{
			hpBar.Hide();
		}
		AudioManager.Instance.PlayOneShot("event:/enemy/teleport_away", base.transform.position);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < teleportMoveDelay))
			{
				break;
			}
			yield return null;
		}
		float num2 = 100f;
		while ((num2 -= 1f) > 0f)
		{
			float f = (float)UnityEngine.Random.Range(0, 360) * ((float)Math.PI / 180f);
			float num3 = UnityEngine.Random.Range(4, 7);
			Vector3 vector = ((targetObject != null) ? targetObject.transform.position : base.transform.position);
			Vector3 vector2 = vector + new Vector3(num3 * Mathf.Cos(f), num3 * Mathf.Sin(f));
			RaycastHit2D raycastHit2D = Physics2D.CircleCast(vector, circleCastRadius, Vector3.Normalize(vector2 - vector), num3, layerToCheck);
			if (raycastHit2D.collider != null)
			{
				if (Vector3.Distance(vector, raycastHit2D.centroid) > 3f)
				{
					if (ShowDebug)
					{
						Points.Add(new Vector3(raycastHit2D.centroid.x, raycastHit2D.centroid.y));
						PointsLink.Add(new Vector3(base.transform.position.x, base.transform.position.y));
					}
					base.transform.position = (Vector3)raycastHit2D.centroid + Vector3.Normalize(vector - vector2) * circleCastRadius;
					break;
				}
				continue;
			}
			if (ShowDebug)
			{
				EndPoints.Add(new Vector3(vector2.x, vector2.y));
				EndPointsLink.Add(new Vector3(base.transform.position.x, base.transform.position.y));
			}
			base.transform.position = vector2;
			break;
		}
		if (targetObject != null)
		{
			state.facingAngle = Utils.GetAngle(base.transform.position, targetObject.transform.position);
		}
		AudioManager.Instance.PlayOneShot("event:/enemy/teleport_appear", base.transform.position);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < teleportCooldownDelay))
			{
				break;
			}
			yield return null;
		}
		Spine.AnimationState.SetAnimation(0, idleAnimation, true);
		teleporting = false;
	}

	private IEnumerator TeleportToPositionIE(Vector3 pos, string teleportAnim)
	{
		if (teleporting)
		{
			yield break;
		}
		if (spawning && spawnRoutine != null)
		{
			StopCoroutine(spawnRoutine);
			spawning = false;
		}
		teleporting = true;
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < teleportDelay))
			{
				break;
			}
			yield return null;
		}
		ClearPaths();
		Spine.AnimationState.SetAnimation(0, teleportAnim, false);
		yield return new WaitForEndOfFrame();
		if ((bool)hpBar)
		{
			hpBar.Hide();
		}
		AudioManager.Instance.PlayOneShot("event:/enemy/teleport_away", base.transform.position);
		bool waiting = true;
		Spine.AnimationState.Event += delegate(TrackEntry trackEntry, global::Spine.Event e)
		{
			if (e.Data.Name == "teleport")
			{
				waiting = false;
			}
		};
		while (waiting || Spine.timeScale == 0.0001f)
		{
			yield return null;
		}
		base.transform.position = pos;
		AudioManager.Instance.PlayOneShot("event:/enemy/teleport_appear", base.transform.position);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < teleportCooldownDelay))
			{
				break;
			}
			yield return null;
		}
		Spine.AnimationState.SetAnimation(0, idleAnimation, true);
		teleporting = false;
	}

	private void Attack()
	{
		if (!attacking)
		{
			attackTimer = 0f;
			attacking = true;
			charging = false;
			Spine.AnimationState.SetAnimation(0, attackAnimation, false);
			Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
			AudioManager.Instance.PlayOneShot("event:/enemy/chaser/chaser_attack", base.transform.position);
			ClearPaths();
			StartCoroutine(TurnOnDamageColliderForDuration(damageDuration));
			StartCoroutine(AttackCooldownIE());
		}
	}

	private IEnumerator AttackCooldownIE()
	{
		yield return new WaitForEndOfFrame();
		simpleSpineFlash.FlashWhite(false);
		yield return new WaitForSeconds(attackCooldown);
		attacking = false;
		state.CURRENT_STATE = StateMachine.State.Idle;
	}

	private IEnumerator TurnOnDamageColliderForDuration(float duration)
	{
		damageColliderEvents.SetActive(true);
		yield return new WaitForSeconds(duration);
		damageColliderEvents.SetActive(false);
	}

	private void ShootProjectile()
	{
		if (!spawning && !charging && !attacking && !teleporting)
		{
			StartCoroutine(ProjectilePattern());
		}
	}

	private IEnumerator ProjectilePattern()
	{
		attacking = true;
		ClearPaths();
		float t = 0f;
		while (t < projectileAnticipation / Spine.timeScale)
		{
			t += Time.deltaTime;
			simpleSpineFlash.FlashWhite(t / projectileAnticipation * 0.75f);
			yield return null;
		}
		simpleSpineFlash.FlashWhite(false);
		Spine.AnimationState.SetAnimation(0, shootAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return StartCoroutine(projectilePattern.ShootIE());
		lastProjectileShootingTime = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(timeBetweenProjectileShots.x, timeBetweenProjectileShots.y);
		attacking = false;
	}

	private void SpawnTrapBombs()
	{
		if (!spawning && !charging && !attacking && !teleporting)
		{
			StartCoroutine(DropTrapBombs());
		}
	}

	private IEnumerator DropTrapBombs()
	{
		attacking = true;
		ClearPaths();
		Spine.AnimationState.SetAnimation(0, teleportAnimation, false);
		yield return new WaitForEndOfFrame();
		if ((bool)hpBar)
		{
			hpBar.Hide();
		}
		AudioManager.Instance.PlayOneShot("event:/enemy/teleport_away", base.transform.position);
		yield return new WaitForSeconds(0.35f);
		Spine.timeScale = 0f;
		yield return new WaitForSeconds(0.5f);
		health.invincible = true;
		List<Vector3> spawnedPositions = new List<Vector3>();
		for (int i = 0; (float)i < UnityEngine.Random.Range(bombsToDrop.x, bombsToDrop.y + 1f); i++)
		{
			int num = 0;
			while (num++ < 100)
			{
				Vector3 randomPositionInIsland = BiomeGenerator.GetRandomPositionInIsland();
				bool flag = false;
				foreach (Vector3 item in spawnedPositions)
				{
					if (Vector3.Distance(item, randomPositionInIsland) < 2f)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Prefabs/Dungeon/Traps/Trap Bomb.prefab", randomPositionInIsland, Quaternion.identity, base.transform.parent);
					asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
					{
						obj.Result.transform.localScale = Vector3.zero;
						obj.Result.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
					};
					spawnedPositions.Add(randomPositionInIsland);
					break;
				}
			}
			yield return new WaitForSeconds(0.25f);
		}
		yield return new WaitForSeconds(1f);
		health.invincible = false;
		Spine.timeScale = 1f;
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(2f);
		lastDropBombTime = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(timeBetweenDroppingBombs.x, timeBetweenDroppingBombs.y);
		attacking = false;
	}

	private void ShootSwirl()
	{
		if (!spawning && !charging && !attacking && !teleporting)
		{
			StartCoroutine(ProjectileSwirlPattern());
		}
	}

	private IEnumerator ProjectileSwirlPattern()
	{
		attacking = true;
		ClearPaths();
		yield return StartCoroutine(TeleportToPositionIE(Vector3.zero, teleportShootAnimation));
		Spine.AnimationState.SetAnimation(0, shootLongAnticipateAnimation, true);
		AudioManager.Instance.PlayOneShot("event:/enemy/jellyfish_miniboss/jellyfish_miniboss_charge", base.transform.position);
		float t = 0f;
		while (t < projectileSwirlAnticipation / Spine.timeScale)
		{
			t += Time.deltaTime;
			simpleSpineFlash.FlashWhite(t / projectileSwirlAnticipation * 0.75f);
			yield return null;
		}
		simpleSpineFlash.FlashWhite(false);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/warning", base.transform.position);
		Spine.AnimationState.SetAnimation(0, shootLongAnimation, true);
		yield return StartCoroutine(projectileSwirlPattern.ShootIE());
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		yield return new WaitForSeconds(2f);
		lastProjectileSwirlShootingTime = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(timeBetweenProjectileSwirlShots.x, timeBetweenProjectileSwirlShots.y);
		attacking = false;
	}

	private void OnDrawGizmos()
	{
		int num = -1;
		while (++num < Points.Count)
		{
			Utils.DrawCircleXY(PointsLink[num], 0.5f, Color.blue);
			Utils.DrawCircleXY(Points[num], circleCastRadius, Color.blue);
			Utils.DrawLine(Points[num], PointsLink[num], Color.blue);
		}
		num = -1;
		while (++num < EndPoints.Count)
		{
			Utils.DrawCircleXY(EndPointsLink[num], 0.5f, Color.red);
			Utils.DrawCircleXY(EndPoints[num], circleCastRadius, Color.red);
			Utils.DrawLine(EndPointsLink[num], EndPoints[num], Color.red);
		}
	}
}
