using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class EnemyExploder : EnemyChaser
{
	[SerializeField]
	private float lockOnDistance;

	[SerializeField]
	private float lockOnAngle;

	[SerializeField]
	private LayerMask lockOnMask;

	[SerializeField]
	private GameObject explosionFXprefab;

	[SerializeField]
	protected bool canExplode = true;

	[SerializeField]
	private float explodeTargetDistance;

	[SerializeField]
	private float explodeTime;

	[SerializeField]
	private float explosionPlayerDamage;

	[SerializeField]
	private float explosionEnemyDamage;

	[SerializeField]
	private float explosionRadius;

	[SerializeField]
	private float knockExplodeDelay;

	[SerializeField]
	private bool hittableWhileExploding;

	[SerializeField]
	private bool moveWhileExploding;

	[SerializeField]
	private bool flashWhite;

	[SerializeField]
	private bool flashRed;

	[SerializeField]
	private bool explodingOnStart;

	[SerializeField]
	private float startExplodeDelay;

	[SerializeField]
	private bool explodeWhenMeleed = true;

	[SerializeField]
	private bool restartExplodeOnHit = true;

	[SerializeField]
	public bool chase;

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

	[Space]
	[SerializeField]
	private ColliderEvents enemyCollider;

	public SkeletonAnimation Spine;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string idleAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string anticipationAnimation;

	protected bool isHit;

	protected bool isExploding;

	private GameObject hitInitiator;

	private bool exploded;

	private float explodeTimer;

	private float enemyExplodeDistance = 0.5f;

	private float flashTime = 0.15f;

	private float flashSpeed = 3f;

	private float startExplodeTimer = -1f;

	private float fleeTimestamp;

	private float repathTimestamp;

	protected float initialSpawnTimestamp;

	private float randomDirection;

	private int patrolIndex;

	private Vector3 startPosition;

	protected float distanceToTarget = float.MaxValue;

	private float repathTimeInterval = 2f;

	private float spawnExplodeDelay = 0.35f;

	private float ExplodeCountDown;

	public float ExplodeCountDownTarget = 0.75f;

	private static bool isRunning;

	private Collider2D[] colliders = new Collider2D[20];

	private static List<EnemyExploder> EnemyExploders = new List<EnemyExploder>();

	public bool CheckClosest;

	public System.Action OnExplode;

	public bool DelayingDestroy;

	protected override float timeStopMultiplier
	{
		get
		{
			return 0f;
		}
	}

	protected override void Start()
	{
		base.Start();
		startPosition = base.transform.position;
		if (patrolRoute.Count > 0)
		{
			patrolRoute.Insert(0, Vector3.zero);
		}
		if (!explodingOnStart && (bool)gm)
		{
			health.enabled = false;
			initialSpawnTimestamp = gm.CurrentTime + spawnExplodeDelay;
		}
		if ((bool)enemyCollider)
		{
			enemyCollider.OnTriggerEnterEvent += OnTriggerEnterEvent;
		}
		spine = Spine;
		health.OnPoisonedHit += OnHit;
	}

	public override void Update()
	{
		base.Update();
		if (gm.CurrentTime > initialSpawnTimestamp)
		{
			health.enabled = true;
		}
		if (targetObject != null)
		{
			distanceToTarget = Mathf.Sqrt(MagnitudeFindDistanceBetween(base.transform.position, targetObject.transform.position));
			if (canExplode && !isExploding && distanceToTarget < explodeTargetDistance && targetObject.state != null && targetObject.state.CURRENT_STATE != StateMachine.State.Dodging)
			{
				if ((ExplodeCountDown -= Time.deltaTime * Spine.timeScale) < 0f)
				{
					WithinDistanceOfTarget();
				}
			}
			else
			{
				ExplodeCountDown = ExplodeCountDownTarget;
			}
		}
		if (isExploding && !DelayingDestroy)
		{
			explodeTimer += Time.deltaTime * Spine.timeScale;
			if (explodeTimer > flashTime)
			{
				float num = explodeTimer / explodeTime;
				if (flashWhite)
				{
					simpleSpineFlash.FlashWhite(num);
				}
				else if (flashRed)
				{
					float amt = Mathf.PingPong(num * flashSpeed, 0.75f);
					simpleSpineFlash.FlashRed(amt);
				}
				if (num > 1f)
				{
					Explode();
				}
			}
		}
		if (inRange && explodingOnStart && !isExploding && gm != null)
		{
			if (startExplodeTimer == -1f)
			{
				startExplodeTimer = gm.CurrentTime + startExplodeDelay;
			}
			else if (gm.CurrentTime > startExplodeTimer)
			{
				ExplodeCharge();
			}
		}
	}

	protected virtual void WithinDistanceOfTarget()
	{
		ExplodeCharge();
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		if (!isHit && restartExplodeOnHit && canExplode)
		{
			health.ClearFreezeTime();
			isHit = true;
			isExploding = false;
			hitInitiator = Attacker;
			ExplodeCharge();
		}
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		if (!exploded)
		{
			health.ClearFreezeTime();
			isHit = true;
			KnockTowardsEnemy(Attacker, AttackType);
		}
	}

	protected virtual void KnockTowardsEnemy(GameObject attacker, Health.AttackTypes attackType)
	{
		StartCoroutine(NotExplodingHittable(attacker, attackType));
	}

	private IEnumerator NotExplodingHittable(GameObject Attacker, Health.AttackTypes AttackType)
	{
		if (isRunning)
		{
			StopCoroutine(NotExplodingHittable(Attacker, AttackType));
		}
		isRunning = true;
		if (!isExploding || hittableWhileExploding)
		{
			Collider2D[] colliders = Physics2D.OverlapCircleAll(base.transform.position, lockOnDistance, lockOnMask);
			yield return null;
			Vector3 from = new Vector3(InputManager.Gameplay.GetHorizontalAxis(), InputManager.Gameplay.GetVerticalAxis(), 0f);
			Collider2D collider2D = null;
			float num = 0f;
			float num2 = 0f;
			simpleSpineFlash.FlashFillRed();
			if (colliders.Length != 0)
			{
				Collider2D[] array = colliders;
				foreach (Collider2D collider2D2 in array)
				{
					if (collider2D2 == null || collider2D2.gameObject == null)
					{
						continue;
					}
					Vector3 normalized = (base.transform.position - collider2D2.transform.position).normalized;
					float num3 = Mathf.Abs(Vector3.Angle(from, normalized) - 180f);
					bool flag = num3 < lockOnAngle;
					if (collider2D == null || Mathf.Abs(num3) < num2)
					{
						Health component = collider2D2.GetComponent<Health>();
						if (flag && (bool)component && component.team == Health.Team.Team2 && collider2D2.gameObject != base.gameObject && component.HP >= num)
						{
							num = component.HP;
							collider2D = collider2D2;
							num2 = num3;
						}
					}
				}
			}
			Collider2D collider2D3 = collider2D;
			if (collider2D3 != null && collider2D3.gameObject != base.gameObject && MagnitudeFindDistanceBetween(collider2D3.transform.position, base.transform.position) < lockOnDistance * lockOnDistance)
			{
				float angle = Utils.GetAngle(base.transform.position, collider2D3.transform.position) * ((float)Math.PI / 180f);
				DoKnockBack(angle, knockbackMultiplier, 1f, false);
				targetObject = collider2D3.GetComponent<Health>();
			}
			else if (Attacker != null)
			{
				DoKnockBack(Attacker, knockbackMultiplier, 1f, false);
			}
			if (!isExploding)
			{
				explodeTime = (knockExplodeDelay + explodeTime / 2f) / Spine.timeScale;
			}
			if (explodeWhenMeleed || (!explodeWhenMeleed && AttackType != 0))
			{
				ExplodeCharge();
			}
			else if (base.gameObject.activeInHierarchy && !DelayingDestroy)
			{
				StartCoroutine(DelayedDestroy());
				Spine.gameObject.SetActive(false);
			}
			ClearPaths();
		}
		isRunning = false;
	}

	protected void ExplodeCharge()
	{
		if (GameManager.GetInstance().CurrentTime > initialSpawnTimestamp)
		{
			explodeTimer = 0f;
			isExploding = true;
			if (Spine != null && Spine.AnimationState != null)
			{
				Spine.AnimationState.SetAnimation(0, anticipationAnimation, true);
			}
		}
	}

	private bool GetClosest()
	{
		if (targetObject == null)
		{
			return false;
		}
		EnemyExploder enemyExploder = null;
		float num = float.MaxValue;
		foreach (EnemyExploder enemyExploder2 in EnemyExploders)
		{
			float num2 = Vector3.Distance(enemyExploder2.transform.position, targetObject.transform.position);
			if (num2 < num)
			{
				num = num2;
				enemyExploder = enemyExploder2;
			}
		}
		return enemyExploder == this;
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (CheckClosest)
		{
			EnemyExploders.Add(this);
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		EnemyExploders.Remove(this);
	}

	protected override void UpdateMoving()
	{
		if ((isExploding && (!moveWhileExploding || targetObject.team != Health.Team.PlayerTeam)) || isHit)
		{
			return;
		}
		if (chase)
		{
			if (!CheckClosest)
			{
				base.UpdateMoving();
			}
			if (!CheckClosest)
			{
				return;
			}
			if (GetClosest())
			{
				base.UpdateMoving();
			}
			else if (state.CURRENT_STATE == StateMachine.State.Idle || gm.CurrentTime > repathTimestamp / Spine.timeScale)
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
		}
		else if (patrol && (state.CURRENT_STATE == StateMachine.State.Idle || gm.CurrentTime > repathTimestamp / Spine.timeScale))
		{
			if (patrolRoute.Count == 0)
			{
				GetRandomTargetPosition();
			}
			else if (pathToFollow == null)
			{
				patrolIndex = ++patrolIndex % patrolRoute.Count;
				givePath(startPosition + patrolRoute[patrolIndex]);
				float angle2 = Utils.GetAngle(base.transform.position, startPosition + patrolRoute[patrolIndex]);
				LookAtAngle(angle2);
			}
			repathTimestamp = gm.CurrentTime + repathTimeInterval;
		}
		else if (flee && (bool)gm && gm.CurrentTime > fleeTimestamp / Spine.timeScale)
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

	protected void Explode()
	{
		if (canExplode && !DelayingDestroy)
		{
			if (explosionFXprefab != null)
			{
				Explosion.CreateExplosionCustomFX(base.transform.position, isHit ? Health.Team.PlayerTeam : Health.Team.KillAll, health, explosionRadius, explosionFXprefab, explosionPlayerDamage, explosionEnemyDamage);
			}
			else
			{
				Explosion.CreateExplosion(base.transform.position, isHit ? Health.Team.PlayerTeam : Health.Team.KillAll, health, explosionRadius, explosionPlayerDamage, explosionEnemyDamage);
			}
			AudioManager.Instance.PlayOneShot("event:/explosion/explosion", base.transform.position);
			System.Action onExplode = OnExplode;
			if (onExplode != null)
			{
				onExplode();
			}
			exploded = true;
			GameObject attacker = ((hitInitiator != null) ? hitInitiator : base.gameObject);
			health.DealDamage(health.totalHP, attacker, base.transform.position, false, Health.AttackTypes.Projectile, false, Health.AttackFlags.DoesntChargeRelics);
			if (base.gameObject.activeInHierarchy && !DelayingDestroy)
			{
				StartCoroutine(DelayedDestroy());
				Spine.gameObject.SetActive(false);
			}
		}
	}

	private IEnumerator DelayedDestroy()
	{
		DelayingDestroy = true;
		float time = 0f;
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
		if ((bool)base.gameObject)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
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

	protected virtual void OnTriggerEnterEvent(Collider2D collision)
	{
		if ((int)lockOnMask == ((int)lockOnMask | (1 << collision.gameObject.layer)) && isHit && collision.gameObject != base.gameObject)
		{
			Explode();
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if ((int)lockOnMask == ((int)lockOnMask | (1 << collision.collider.gameObject.layer)) && isHit && collision.gameObject != base.gameObject)
		{
			Explode();
		}
	}

	private float MagnitudeFindDistanceBetween(Vector3 a, Vector3 b)
	{
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		float num3 = a.z - b.z;
		return num * num + num2 * num2 + num3 * num3;
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, explodeTargetDistance, Color.red);
		if (!patrol)
		{
			return;
		}
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
			return;
		}
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
