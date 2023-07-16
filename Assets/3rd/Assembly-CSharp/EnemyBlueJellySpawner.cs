using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyBlueJellySpawner : EnemyJellyCharger
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
	private bool teleportOnHit;

	[SerializeField]
	private float circleCastRadius = 0.5f;

	[SerializeField]
	private float teleportMoveDelay = 0.5f;

	[SerializeField]
	private float teleportCooldownDelay = 0.5f;

	[SerializeField]
	private float randomTeleportMinDelay;

	[SerializeField]
	private float randomTeleportMaxDelay;

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
	private bool isSpawnablesIncreasingDamageMultiplier = true;

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

	private float spawnTime = float.MaxValue;

	private float teleportTime = float.MaxValue;

	private bool spawning;

	private int spawnedAmount;

	private bool teleporting;

	private float attackTimer;

	private bool charging;

	private bool attacking;

	private Coroutine spawnRoutine;

	private ShowHPBar hpBar;

	private List<UnitObject> spawnedEnemies = new List<UnitObject>();

	private float TeleportOnHitDelay = 1f;

	private Coroutine cDelayTeleport;

	public bool ShowDebug;

	public List<Vector3> Points = new List<Vector3>();

	public List<Vector3> PointsLink = new List<Vector3>();

	public List<Vector3> EndPoints = new List<Vector3>();

	public List<Vector3> EndPointsLink = new List<Vector3>();

	protected override void Start()
	{
		base.Start();
		SetSpawnTime();
		SetTeleportTime();
		hpBar = GetComponent<ShowHPBar>();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		spawning = false;
		teleporting = false;
		charging = false;
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		simpleSpineFlash.FlashWhite(false);
		simpleSpineFlash.FlashFillRed();
		DoKnockBack(Attacker, 0.25f, 0.5f);
		if (TeleportOnHitDelay < 0f && cDelayTeleport == null)
		{
			TeleportOnHitDelay = 2f;
			cDelayTeleport = StartCoroutine(DelayTeleport());
		}
	}

	private IEnumerator DelayTeleport()
	{
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
		StartCoroutine(TeleportIE());
		cDelayTeleport = null;
	}

	public override void Update()
	{
		TeleportOnHitDelay -= Time.deltaTime * Spine.timeScale;
		if (!teleporting)
		{
			base.Update();
		}
		if (inRange && spawnTime == float.MaxValue)
		{
			SetSpawnTime();
		}
		if (inRange && teleportTime == float.MaxValue)
		{
			SetTeleportTime();
		}
		if (inRange && GameManager.RoomActive && (bool)targetObject && (bool)gm && gm.CurrentTime > spawnTime / Spine.timeScale)
		{
			SpawnEnemy();
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
			attackTimer += Time.deltaTime * Spine.timeScale;
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
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < spawnDuration))
			{
				break;
			}
			yield return null;
		}
		int num2 = UnityEngine.Random.Range(spawnAmountMin, spawnAmountMax + 1);
		float num3 = UnityEngine.Random.Range(0, 360);
		if (targetObject != null)
		{
			num3 = Utils.GetAngle(base.transform.position, targetObject.transform.position);
		}
		AudioManager.Instance.PlayOneShot("event:/enemy/chaser_boss/chaser_boss_egg_spawn", base.transform.position);
		for (int i = 0; i < num2; i++)
		{
			Vector3 direction;
			if (randomSpawnDirection)
			{
				direction = UnityEngine.Random.insideUnitCircle;
			}
			else
			{
				direction = new Vector3(Mathf.Cos(num3 * ((float)Math.PI / 180f)), Mathf.Sin(num3 * ((float)Math.PI / 180f)), 0f);
				num3 += (float)(360 / num2);
			}
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(spawnableEnemies[UnityEngine.Random.Range(0, spawnableEnemies.Length)], base.transform.position, Quaternion.identity, base.transform.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				EnemyExploder component = obj.Result.GetComponent<EnemyExploder>();
				component.givePath(component.transform.position + direction * 5f);
				component.health.OnDie += OnEnemyKilled;
				component.health.CanIncreaseDamageMultiplier = isSpawnablesIncreasingDamageMultiplier;
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
				if (base.transform != null)
				{
					float angle = Utils.GetAngle(base.transform.position, base.transform.position + direction) * ((float)Math.PI / 180f);
					component.DoKnockBack(angle, spawnSpitOutForce, 0.75f);
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
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < spawnCooldown))
			{
				break;
			}
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		Spine.AnimationState.SetAnimation(0, idleAnimation, true);
		SetSpawnTime();
		spawning = false;
		spawnRoutine = null;
	}

	private new void OnEnemyKilled(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		spawnedAmount--;
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		if (!killSpawnablesOnDeath)
		{
			return;
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

	private void SetTeleportTime()
	{
		if ((bool)gm)
		{
			teleportTime = gm.CurrentTime + UnityEngine.Random.Range(randomTeleportMinDelay, randomTeleportMaxDelay);
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
		ClearPaths();
		Spine.AnimationState.SetAnimation(0, teleportAnimation, false);
		yield return new WaitForEndOfFrame();
		if ((bool)hpBar)
		{
			hpBar.Hide();
		}
		AudioManager.Instance.PlayOneShot("event:/enemy/teleport_away", base.transform.position);
		float time2 = 0f;
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
		SetTeleportTime();
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
		attacking = false;
		state.CURRENT_STATE = StateMachine.State.Idle;
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
