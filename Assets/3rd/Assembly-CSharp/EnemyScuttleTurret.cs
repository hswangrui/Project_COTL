using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class EnemyScuttleTurret : UnitObject
{
	public SkeletonAnimation spine;

	[SerializeField]
	private SkeletonAnimation Warning;

	public SimpleSpineFlash SimpleSpineFlash;

	private GameObject TargetObject;

	private float RandomDirection;

	public float acceleration = 5f;

	public float turningSpeed = 1f;

	public float knockbackModifier = 0.75f;

	public bool isMiniBoss;

	public GameObject Arrow;

	private bool Anticipating;

	private bool Shooting;

	private float ShootDelay = float.MaxValue;

	public int ShotsToFire = 3;

	public float ShootDelayTime = 1f;

	public float anticipateDuration = 1f;

	public float TimeBetweenShooting = 1f;

	public SpriteRenderer Aiming;

	public float AngleArc = 90f;

	public GameObject SecondaryArrow;

	public int SecondaryShotsToFire = 3;

	public float SecondaryShootDelayTime = 1f;

	public float SecondaryTimeBetweenShooting = 1f;

	public float SecondaryAngleArc = 90f;

	public int numSecondaryAttacks;

	public int secondaryAttackCounter;

	public bool LimitTo45Degrees;

	public float KnockbackDuration = 0.5f;

	public float KnockbackForce = 1f;

	private float Angle;

	private Vector3 Force;

	public Vector2 DistanceRange = new Vector2(1f, 3f);

	public Vector2 IdleWaitRange = new Vector2(1f, 3f);

	private float IdleWait;

	private bool ShownWarning;

	private Health EnemyHealth;

	private int DetectEnemyRange = 8;

	public float CircleCastRadius = 0.5f;

	public float CircleCastOffset = 1f;

	[SerializeField]
	private bool hasMegaAttack;

	[SerializeField]
	private float anticipationTime;

	[SerializeField]
	private Vector2 timeBetweenMegaAttacks;

	[SerializeField]
	private ProjectilePattern projectilePattern;

	private float lastMegaAttackTime;

	public bool ShowDebug;

	public List<Vector3> Points = new List<Vector3>();

	public List<Vector3> PointsLink = new List<Vector3>();

	public List<Vector3> EndPoints = new List<Vector3>();

	public List<Vector3> EndPointsLink = new List<Vector3>();

	public override void OnEnable()
	{
		base.OnEnable();
		SeperateObject = true;
		RandomDirection = (float)UnityEngine.Random.Range(0, 360) * ((float)Math.PI / 180f);
		state.facingAngle = RandomDirection * 57.29578f;
		Aiming.gameObject.SetActive(false);
		Shooting = false;
		StartCoroutine(ActiveRoutine());
		secondaryAttackCounter = numSecondaryAttacks;
		speed = 0f;
		if (GameManager.GetInstance() != null)
		{
			lastMegaAttackTime = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(timeBetweenMegaAttacks.x, timeBetweenMegaAttacks.y);
		}
	}

	public override void OnDisable()
	{
		SimpleSpineFlash.FlashWhite(false);
		base.OnDisable();
		ClearPaths();
		StopAllCoroutines();
	}

	private IEnumerator ShowWarning()
	{
		Warning.gameObject.SetActive(true);
		yield return Warning.YieldForAnimation("warn");
		Warning.gameObject.SetActive(false);
	}

	public override void Update()
	{
		if (UsePathing)
		{
			if (pathToFollow == null)
			{
				speed += (0f - speed) / (4f * acceleration) * GameManager.DeltaTime;
				move();
				return;
			}
			if (currentWaypoint >= pathToFollow.Count)
			{
				speed += (0f - speed) / (4f * acceleration) * GameManager.DeltaTime;
				move();
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
		move();
	}

	private IEnumerator ActiveRoutine()
	{
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * spine.timeScale);
			if (!(num < 0.2f))
			{
				break;
			}
			yield return null;
		}
		if (spine != null)
		{
			spine.AnimationState.SetAnimation(0, "animation", true);
		}
		while (true)
		{
			if (state.CURRENT_STATE == StateMachine.State.Idle)
			{
				IdleWait -= Time.deltaTime;
				float num2 = 0f;
			}
			GetNewTargetPosition();
			if (GameManager.RoomActive && ShootDelay == float.MaxValue)
			{
				ShootDelay = ((numSecondaryAttacks > 0 && secondaryAttackCounter > 0) ? SecondaryShootDelayTime : ShootDelayTime);
			}
			if (TargetObject == null)
			{
				if (Time.frameCount % 10 == 0)
				{
					GetNewTarget();
				}
			}
			else if (!Shooting && GameManager.RoomActive)
			{
				ShootDelay -= Time.deltaTime;
				if (GameManager.GetInstance().CurrentTime > lastMegaAttackTime && ShootDelay <= anticipateDuration && !Anticipating && hasMegaAttack)
				{
					StartCoroutine(MegaAttackIE());
				}
				else
				{
					if (!Anticipating && ShootDelay <= anticipateDuration && (secondaryAttackCounter < numSecondaryAttacks || numSecondaryAttacks == 0))
					{
						Anticipating = true;
						if (isMiniBoss)
						{
							AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/warning", base.gameObject);
						}
						else
						{
							AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish/warning", base.gameObject);
						}
						spine.AnimationState.SetAnimation(0, "anticipate", true);
					}
					if (Anticipating && ShootDelay <= anticipateDuration && ShootDelay > 0f)
					{
						SimpleSpineFlash.FlashWhite(1f - Mathf.Clamp01(ShootDelay / anticipateDuration));
					}
					if (ShootDelay <= 0f)
					{
						SimpleSpineFlash.FlashWhite(false);
						Anticipating = false;
						StartCoroutine(ShootArrowRoutine());
					}
				}
			}
			yield return null;
		}
	}

	protected override void FixedUpdate()
	{
		if (spine.timeScale != 0.0001f)
		{
			base.FixedUpdate();
		}
	}

	private IEnumerator ShootArrowRoutine()
	{
		if (isMiniBoss)
		{
			AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/attack", base.gameObject);
		}
		else
		{
			AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish/attack", base.gameObject);
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		IdleWait = 1f;
		Shooting = true;
		bool secondaryAttack = false;
		if (numSecondaryAttacks > 0 && UnityEngine.Random.Range(0, 2) == 0)
		{
			secondaryAttack = true;
			secondaryAttackCounter = 1;
		}
		else
		{
			secondaryAttackCounter = 0;
		}
		int _shotsToFire = (secondaryAttack ? SecondaryShotsToFire : ShotsToFire);
		float _angleArc = (secondaryAttack ? SecondaryAngleArc : AngleArc);
		float _timeBetweenShooting = (secondaryAttack ? SecondaryTimeBetweenShooting : TimeBetweenShooting);
		int i = _shotsToFire;
		float aimingAngle = state.LookAngle;
		if (TargetObject != null)
		{
			aimingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
		}
		if (LimitTo45Degrees)
		{
			aimingAngle = Mathf.Round(aimingAngle / 45f) * 45f;
		}
		float KnockbackAngle = Mathf.Repeat(aimingAngle + 180f, 360f) * ((float)Math.PI / 180f);
		if (TargetObject != null)
		{
			KnockbackAngle = Utils.GetAngle(TargetObject.transform.position, base.transform.position) * ((float)Math.PI / 180f);
		}
		aimingAngle -= _angleArc / 2f;
		while (true)
		{
			int num = i - 1;
			i = num;
			if (num < 0)
			{
				break;
			}
			if (!secondaryAttack && _timeBetweenShooting > 0.1f)
			{
				Aiming.gameObject.SetActive(true);
			}
			float Progress = 0f;
			while (true)
			{
				float num2;
				Progress = (num2 = Progress + Time.deltaTime);
				if (!(num2 < _timeBetweenShooting / spine.timeScale))
				{
					break;
				}
				if (!secondaryAttack)
				{
					SimpleSpineFlash.FlashWhite(Mathf.Clamp01(Progress / _timeBetweenShooting) * 0.75f);
				}
				Aiming.transform.eulerAngles = new Vector3(0f, 0f, aimingAngle);
				if (Time.frameCount % 5 == 0)
				{
					Aiming.color = ((Aiming.color == Color.red) ? Color.white : Color.red);
				}
				yield return null;
			}
			Aiming.gameObject.SetActive(false);
			SimpleSpineFlash.FlashWhite(false);
			CameraManager.shakeCamera(0.2f, aimingAngle);
			Projectile component = ObjectPool.Spawn(secondaryAttack ? SecondaryArrow : Arrow, base.transform.parent).GetComponent<Projectile>();
			AudioManager.Instance.PlayOneShot("event:/enemy/shoot_magicenergy", base.transform.position);
			component.transform.position = base.transform.position;
			component.Angle = aimingAngle;
			component.team = health.team;
			component.Speed = 6f;
			component.Owner = health;
			spine.AnimationState.SetAnimation(0, "attack-shoot", false);
			spine.AnimationState.AddAnimation(0, "animation", true, 0f);
			aimingAngle += _angleArc / (float)Mathf.Max(_shotsToFire, 0);
		}
		TargetObject = null;
		Shooting = false;
		ShootDelay = ((numSecondaryAttacks > 0 && secondaryAttackCounter > 0) ? SecondaryShootDelayTime : ShootDelayTime);
		DoKnockBack(KnockbackAngle, KnockbackForce, KnockbackDuration);
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		if (isMiniBoss)
		{
			AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/gethit", base.transform.position);
		}
		else
		{
			AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish/gethit", base.transform.position);
		}
		Anticipating = false;
		StartCoroutine(HurtRoutine(Attacker));
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		if (isMiniBoss)
		{
			AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/death", base.transform.position);
		}
		else
		{
			AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish/death", base.transform.position);
		}
	}

	private IEnumerator HurtRoutine(GameObject Attacker)
	{
		ClearPaths();
		state.CURRENT_STATE = StateMachine.State.KnockBack;
		DisableForces = true;
		Angle = Utils.GetAngle(Attacker.transform.position, base.transform.position);
		Force = new Vector2(25f * Mathf.Cos(Angle * ((float)Math.PI / 180f)), 25f * Mathf.Sin(Angle * ((float)Math.PI / 180f)));
		rb.velocity = Force;
		SimpleSpineFlash.FlashFillRed();
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * spine.timeScale);
			if (!(num < 0.5f))
			{
				break;
			}
			yield return null;
		}
		DisableForces = false;
		IdleWait = 0f;
		state.CURRENT_STATE = StateMachine.State.Idle;
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
			float num2 = UnityEngine.Random.Range(DistanceRange.x, DistanceRange.y);
			RandomDirection += (float)UnityEngine.Random.Range(-45, 45) * ((float)Math.PI / 180f);
			float radius = 0.1f;
			Vector3 vector = base.transform.position + new Vector3(num2 * Mathf.Cos(RandomDirection), num2 * Mathf.Sin(RandomDirection));
			RaycastHit2D raycastHit2D = Physics2D.CircleCast(base.transform.position, radius, Vector3.Normalize(vector - base.transform.position), num2 * 0.5f, layerToCheck);
			if (raycastHit2D.collider != null)
			{
				if (ShowDebug)
				{
					Points.Add(new Vector3(raycastHit2D.centroid.x, raycastHit2D.centroid.y) + Vector3.Normalize(base.transform.position - vector) * CircleCastOffset);
					PointsLink.Add(new Vector3(base.transform.position.x, base.transform.position.y));
				}
				RandomDirection += 0.17453292f;
				continue;
			}
			if (ShowDebug)
			{
				EndPoints.Add(new Vector3(vector.x, vector.y));
				EndPointsLink.Add(new Vector3(base.transform.position.x, base.transform.position.y));
			}
			IdleWait = UnityEngine.Random.Range(IdleWaitRange.x, IdleWaitRange.y);
			givePath(vector);
			break;
		}
	}

	public void GetNewTarget()
	{
		Health closestTarget = GetClosestTarget();
		if (!(closestTarget != null))
		{
			return;
		}
		if (ShownWarning)
		{
			if (isMiniBoss)
			{
				AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/warning", base.gameObject);
			}
			else
			{
				AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish/warning", base.gameObject);
			}
			StartCoroutine(ShowWarning());
			ShownWarning = true;
		}
		TargetObject = closestTarget.gameObject;
		EnemyHealth = closestTarget;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if ((int)layerToCheck == ((int)layerToCheck | (1 << collision.gameObject.layer)))
		{
			if (speed < maxSpeed)
			{
				speed *= 1.2f;
			}
			state.CURRENT_STATE = StateMachine.State.Idle;
			IdleWait = UnityEngine.Random.Range(IdleWaitRange.x, IdleWaitRange.y);
			state.facingAngle = Utils.GetAngle((Vector2)base.transform.position, (Vector2)base.transform.position + Vector2.Reflect(Utils.DegreeToVector2(state.facingAngle), collision.contacts[0].normal));
		}
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, VisionRange, Color.yellow);
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

	private IEnumerator MegaAttackIE()
	{
		state.CURRENT_STATE = StateMachine.State.Idle;
		IdleWait = 1f;
		Shooting = true;
		float s = maxSpeed;
		maxSpeed = 0f;
		ClearPaths();
		if (isMiniBoss)
		{
			AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/warning", base.gameObject);
		}
		else
		{
			AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish/warning", base.gameObject);
		}
		spine.AnimationState.SetAnimation(0, "anticipate", true);
		float t = 0f;
		while (t < anticipationTime)
		{
			t += Time.deltaTime * spine.timeScale;
			SimpleSpineFlash.FlashWhite(t / anticipationTime * 0.75f);
			yield return null;
		}
		SimpleSpineFlash.FlashWhite(false);
		if (isMiniBoss)
		{
			AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/attack", base.gameObject);
		}
		else
		{
			AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish/attack", base.gameObject);
		}
		projectilePattern.Shoot();
		for (int i = 0; i < projectilePattern.Waves.Length; i++)
		{
			spine.AnimationState.SetAnimation(0, "attack-shoot", false);
			float time = 0f;
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * spine.timeScale);
				if (!(num < projectilePattern.Waves[i].FinishDelay))
				{
					break;
				}
				yield return null;
			}
		}
		spine.AnimationState.AddAnimation(0, "animation", true, 0f);
		lastMegaAttackTime = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(timeBetweenMegaAttacks.x, timeBetweenMegaAttacks.y);
		ShootDelay = 2f;
		maxSpeed = s;
		Shooting = false;
	}
}
