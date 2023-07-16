using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using MMBiomeGeneration;
using Spine.Unity;
using UnityEngine;

public class EnemyArcher : UnitObject
{
	private enum State
	{
		Idle,
		Shooting,
		Teleporting,
		CloseCombatAttack
	}

	private static float LastArcherShot = float.MinValue;

	public float GlobalShotDelay = 5f;

	public int ShotsToFire = 1;

	public float DelayBetweenShots = 0.2f;

	public float DelayReaiming = 0.5f;

	public GameObject Arrow;

	private GameObject TargetObject;

	public SkeletonAnimation Spine;

	public int DistanceFromTargetPosition = 3;

	public SimpleSpineFlash SimpleSpineFlash;

	public SpriteRenderer Aiming;

	public ColliderEvents damageColliderEvents;

	private bool canBeParried;

	private static float signPostParryWindow = 0.2f;

	private static float attackParryWindow = 0.15f;

	[EventRef]
	public string attackSoundPath = string.Empty;

	[EventRef]
	public string shootSoundPath = string.Empty;

	[EventRef]
	public string AttackVO = string.Empty;

	[EventRef]
	public string DeathVO = string.Empty;

	[EventRef]
	public string GetHitVO = string.Empty;

	[EventRef]
	public string WarningVO = string.Empty;

	private Coroutine cWaitForTarget;

	public float KnockbackMultiplier = 1f;

	private float ShootDelay;

	private float TeleportDelay;

	private float Angle;

	private float RandomChangeAngle = 3f;

	private Vector3 TargetPosition;

	public int MaintainDistance = 3;

	private float RepathTimer;

	private State MyState;

	private float CloseCombatCooldown;

	public float SignPostCloseCombatDelay = 1f;

	public float CircleCastRadius = 0.5f;

	public float CircleCastOffset = 1f;

	public int AcceptableMove = 2;

	public bool ShowDebug;

	public List<Vector3> Points = new List<Vector3>();

	public List<Vector3> PointsLink = new List<Vector3>();

	public List<Vector3> EndPoints = new List<Vector3>();

	public List<Vector3> EndPointsLink = new List<Vector3>();

	public override void Awake()
	{
		base.Awake();
		if (BiomeGenerator.Instance != null)
		{
			GetComponent<Health>().totalHP *= BiomeGenerator.Instance.HumanoidHealthMultiplier;
		}
	}

	private void Start()
	{
		SeperateObject = true;
		Aiming.gameObject.SetActive(false);
	}

	private void DoWaitForTarget()
	{
		if (cWaitForTarget != null)
		{
			StopCoroutine(cWaitForTarget);
		}
		cWaitForTarget = StartCoroutine(WaitForTarget());
	}

	public override void OnEnable()
	{
		base.OnEnable();
		DoWaitForTarget();
		health.OnHitEarly += OnHitEarly;
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEvents.SetActive(false);
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		Aiming.gameObject.SetActive(false);
		ClearPaths();
		StopAllCoroutines();
		DisableForces = false;
		SimpleSpineFlash.FlashWhite(false);
		health.OnHitEarly -= OnHitEarly;
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent -= OnDamageTriggerEnter;
		}
	}

	public override void BeAlarmed(GameObject TargetObject)
	{
		base.BeAlarmed(TargetObject);
		if (!string.IsNullOrEmpty(WarningVO))
		{
			AudioManager.Instance.PlayOneShot(WarningVO, base.transform.position);
		}
		this.TargetObject = TargetObject;
		DoWaitForTarget();
	}

	private void OnHitEarly(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (canBeParried && !FromBehind && AttackType == Health.AttackTypes.Melee)
		{
			health.WasJustParried = true;
			Aiming.gameObject.SetActive(false);
			SimpleSpineFlash.FlashWhite(false);
			StopAllCoroutines();
			DisableForces = false;
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (health.WasJustParried)
		{
			return;
		}
		Aiming.gameObject.SetActive(false);
		SimpleSpineFlash.FlashWhite(false);
		base.OnHit(Attacker, AttackLocation, AttackType);
		SimpleSpineFlash.FlashFillRed();
		TargetObject = null;
		if (!string.IsNullOrEmpty(GetHitVO))
		{
			AudioManager.Instance.PlayOneShot(GetHitVO, base.transform.position);
		}
		Spine.AnimationState.SetAnimation(1, "hurt-eyes", false);
		if (MyState != State.Shooting && AttackType != Health.AttackTypes.Heavy && (AttackType != Health.AttackTypes.Projectile || health.HasShield))
		{
			if (AttackLocation.x > base.transform.position.x && state.CURRENT_STATE != StateMachine.State.HitRight)
			{
				state.CURRENT_STATE = StateMachine.State.HitRight;
			}
			if (AttackLocation.x < base.transform.position.x && state.CURRENT_STATE != StateMachine.State.HitLeft)
			{
				state.CURRENT_STATE = StateMachine.State.HitLeft;
			}
			StopAllCoroutines();
			DisableForces = false;
			StartCoroutine(HurtRoutine());
		}
		if (AttackType == Health.AttackTypes.Projectile && !health.HasShield)
		{
			state.facingAngle = (state.LookAngle = Utils.GetAngle(base.transform.position, Attacker.transform.position));
			Spine.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
		}
		else
		{
			DoKnockBack(Utils.GetAngle(Attacker.transform.position, base.transform.position) * ((float)Math.PI / 180f), KnockbackMultiplier, 0.5f);
		}
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (!string.IsNullOrEmpty(DeathVO))
		{
			AudioManager.Instance.PlayOneShot(DeathVO, base.transform.position);
		}
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
	}

	private IEnumerator HurtRoutine()
	{
		if (damageColliderEvents != null)
		{
			damageColliderEvents.SetActive(false);
		}
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
		DoWaitForTarget();
		ShootDelay = 0f;
		if (TeleportDelay < 0f && UnityEngine.Random.Range(0f, 1f) < 0.5f)
		{
			Teleport();
		}
	}

	private IEnumerator WaitForTarget()
	{
		if (damageColliderEvents != null)
		{
			damageColliderEvents.SetActive(false);
		}
		RepathTimer = 2f;
		while (TargetObject == null)
		{
			Debug.Log("A");
			Health closestTarget = GetClosestTarget();
			if ((bool)closestTarget)
			{
				TargetObject = closestTarget.gameObject;
				VisionRange = int.MaxValue;
			}
			RepathTimer -= Time.deltaTime * Spine.timeScale;
			if (RepathTimer <= 0f)
			{
				if (state.CURRENT_STATE == StateMachine.State.Moving)
				{
					if (Spine.AnimationName != "run")
					{
						Spine.AnimationState.SetAnimation(0, "run", true);
					}
				}
				else if (Spine.AnimationName != "idle")
				{
					Spine.AnimationState.SetAnimation(0, "idle", true);
				}
				TargetPosition = base.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 2f;
				FindPath(TargetPosition);
				state.LookAngle = Utils.GetAngle(base.transform.position, TargetPosition);
				Spine.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
			}
			yield return null;
		}
		float LineOfSightLostTimer = 1f;
		bool InRange = false;
		while (!InRange)
		{
			if (TargetObject == null)
			{
				DoWaitForTarget();
				yield break;
			}
			float num = Vector3.Distance(TargetObject.transform.position, base.transform.position);
			bool flag = CheckLineOfSight(TargetObject.transform.position, Mathf.Min(num, VisionRange));
			if (num <= (float)VisionRange && flag)
			{
				InRange = true;
			}
			else if (!flag)
			{
				LineOfSightLostTimer -= Time.deltaTime * Spine.timeScale;
				if (LineOfSightLostTimer <= 0f)
				{
					PreventEndlessIdleState();
					yield break;
				}
			}
			yield return null;
		}
		StartCoroutine(ChasePlayer());
	}

	private void PreventEndlessIdleState()
	{
		ClearPaths();
		StopAllCoroutines();
		state.LookAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
		StartCoroutine(ShootArrowRoutine(1f, 2f));
	}

	private IEnumerator ChasePlayer()
	{
		Debug.Log("ChasePlayer()".Colour(Color.red));
		if (damageColliderEvents != null)
		{
			damageColliderEvents.SetActive(false);
		}
		MyState = State.Idle;
		bool Loop = true;
		Angle = Utils.GetAngle(TargetObject.transform.position, base.transform.position) * ((float)Math.PI / 180f);
		TargetPosition = TargetObject.transform.position + new Vector3((float)MaintainDistance * Mathf.Cos(Angle), (float)MaintainDistance * Mathf.Sin(Angle));
		while (Loop)
		{
			if (MyState == State.Idle)
			{
				if (damageColliderEvents != null)
				{
					damageColliderEvents.SetActive(false);
				}
				if (TargetObject == null)
				{
					DoWaitForTarget();
					break;
				}
				if ((CloseCombatCooldown -= Time.deltaTime * Spine.timeScale) < 0f && Vector3.Distance(base.transform.position, TargetObject.transform.position) < 2f)
				{
					StartCoroutine(CloseCombatAttack());
					break;
				}
				if (state.CURRENT_STATE == StateMachine.State.Moving)
				{
					if (Spine.AnimationName != "run")
					{
						Spine.AnimationState.SetAnimation(0, "run", true);
					}
				}
				else if (Spine.AnimationName != "idle")
				{
					Spine.AnimationState.SetAnimation(0, "idle", true);
				}
				TeleportDelay -= Time.deltaTime * Spine.timeScale;
				state.LookAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
				Spine.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
				if ((RepathTimer -= Time.deltaTime * Spine.timeScale) < 0f)
				{
					TargetPosition = TargetObject.transform.position + new Vector3((float)MaintainDistance * Mathf.Cos(Angle), (float)MaintainDistance * Mathf.Sin(Angle));
				}
				if (Vector3.Distance(TargetPosition, base.transform.position) > (float)DistanceFromTargetPosition && Time.frameCount % 5 == 0)
				{
					FindPath(TargetPosition);
				}
				if ((ShootDelay -= Time.deltaTime * Spine.timeScale) < 0f && Vector3.Distance(base.transform.position, TargetObject.transform.position) < 8f && GameManager.GetInstance().CurrentTime > (LastArcherShot + GlobalShotDelay) / Spine.timeScale)
				{
					DataManager.Instance.LastArcherShot = TimeManager.TotalElapsedGameTime;
					LastArcherShot = GameManager.GetInstance().CurrentTime;
					StartCoroutine(ShootArrowRoutine());
					break;
				}
			}
			yield return null;
		}
	}

	private IEnumerator CloseCombatAttack()
	{
		Debug.Log("CloseCombatAttack()".Colour(Color.red));
		ClearPaths();
		MyState = State.CloseCombatAttack;
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		float Progress = 0f;
		Spine.AnimationState.SetAnimation(0, "grunt-attack-charge2", false);
		state.facingAngle = (state.LookAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position));
		Spine.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * Spine.timeScale);
			if (!(num < SignPostCloseCombatDelay))
			{
				break;
			}
			if (Progress >= SignPostCloseCombatDelay - signPostParryWindow)
			{
				canBeParried = true;
			}
			SimpleSpineFlash.FlashWhite(Progress / SignPostCloseCombatDelay);
			yield return null;
		}
		speed = 0.2f;
		SimpleSpineFlash.FlashWhite(false);
		Spine.AnimationState.SetAnimation(0, "grunt-attack-impact2", false);
		if (!string.IsNullOrEmpty(AttackVO))
		{
			AudioManager.Instance.PlayOneShot(AttackVO, base.transform.position);
		}
		if (!string.IsNullOrEmpty(attackSoundPath))
		{
			AudioManager.Instance.PlayOneShot(attackSoundPath, base.transform.position);
		}
		Progress = 0f;
		float Duration = 0.2f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * Spine.timeScale);
			if (!(num < Duration))
			{
				break;
			}
			if (damageColliderEvents != null)
			{
				damageColliderEvents.SetActive(true);
			}
			canBeParried = Progress <= attackParryWindow;
			yield return null;
		}
		if (damageColliderEvents != null)
		{
			damageColliderEvents.SetActive(false);
		}
		canBeParried = false;
		yield return new WaitForSeconds(0.8f);
		CloseCombatCooldown = 1f;
		state.CURRENT_STATE = StateMachine.State.Idle;
		DoWaitForTarget();
	}

	private IEnumerator ShootArrowRoutine(float minDelay = 3f, float maxDelay = 4f)
	{
		ClearPaths();
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		MyState = State.Shooting;
		ShootDelay = UnityEngine.Random.Range(minDelay, maxDelay);
		yield return null;
		Spine.AnimationState.SetAnimation(0, "archer-attack-charge", false);
		Aiming.gameObject.SetActive(true);
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * Spine.timeScale);
			if (!(num < 1f))
			{
				break;
			}
			Aiming.transform.eulerAngles = new Vector3(0f, 0f, state.LookAngle);
			SimpleSpineFlash simpleSpineFlash = SimpleSpineFlash;
			if ((object)simpleSpineFlash != null)
			{
				simpleSpineFlash.FlashWhite(Progress / 1f);
			}
			if (Time.frameCount % 5 == 0)
			{
				Aiming.color = ((Aiming.color == Color.red) ? Color.white : Color.red);
			}
			yield return null;
		}
		SimpleSpineFlash.FlashWhite(false);
		Aiming.gameObject.SetActive(false);
		if (!string.IsNullOrEmpty(AttackVO))
		{
			AudioManager.Instance.PlayOneShot(AttackVO, base.transform.position);
		}
		int i = ShotsToFire;
		float time;
		while (true)
		{
			int num2 = i - 1;
			i = num2;
			if (num2 < 0)
			{
				break;
			}
			if (!string.IsNullOrEmpty(shootSoundPath))
			{
				AudioManager.Instance.PlayOneShot(shootSoundPath, base.transform.position);
			}
			CameraManager.shakeCamera(0.2f, state.LookAngle);
			Projectile component = ObjectPool.Spawn(Arrow, base.transform.parent).GetComponent<Projectile>();
			component.transform.position = base.transform.position + new Vector3(0.5f * Mathf.Cos(state.LookAngle * ((float)Math.PI / 180f)), 0.5f * Mathf.Sin(state.LookAngle * ((float)Math.PI / 180f)));
			component.Angle = state.LookAngle;
			component.team = health.team;
			component.Owner = health;
			Spine.AnimationState.SetAnimation(0, "archer-attack-impact", false);
			time = 0f;
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * Spine.timeScale);
				if (!(num < DelayBetweenShots))
				{
					break;
				}
				yield return null;
			}
			if (TargetObject != null && i > 0)
			{
				Aiming.gameObject.SetActive(true);
				state.LookAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
				Spine.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
				Aiming.transform.eulerAngles = new Vector3(0f, 0f, state.LookAngle);
				yield return new WaitForSeconds(DelayReaiming);
			}
		}
		Aiming.gameObject.SetActive(false);
		TargetObject = null;
		time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.3f))
			{
				break;
			}
			yield return null;
		}
		MyState = State.Idle;
		state.CURRENT_STATE = StateMachine.State.Idle;
		DoWaitForTarget();
	}

	private IEnumerator TeleportRoutine(Vector3 Position)
	{
		ClearPaths();
		state.CURRENT_STATE = StateMachine.State.Moving;
		UsePathing = false;
		health.invincible = true;
		SeperateObject = false;
		MyState = State.Teleporting;
		ClearPaths();
		ShootDelay = 1f;
		Vector3 position = base.transform.position;
		float Progress = 0f;
		Spine.AnimationState.SetAnimation(0, "roll", true);
		state.facingAngle = (state.LookAngle = Utils.GetAngle(base.transform.position, Position));
		Spine.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
		float num = Vector3.Distance(position, Position);
		float Duration = num / 12f;
		while (true)
		{
			float num2;
			Progress = (num2 = Progress + Time.deltaTime * Spine.timeScale);
			if (!(num2 < Duration))
			{
				break;
			}
			speed = 10f * Time.deltaTime;
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		Spine.AnimationState.SetAnimation(0, "roll-stop", false);
		float time = 0f;
		while (true)
		{
			float num2;
			time = (num2 = time + Time.deltaTime * Spine.timeScale);
			if (!(num2 < 0.3f))
			{
				break;
			}
			yield return null;
		}
		UsePathing = true;
		SeperateObject = true;
		TargetPosition = Position;
		RepathTimer = 0f;
		ShootDelay = UnityEngine.Random.Range(0f, 1f);
		health.invincible = false;
		TeleportDelay = 1f;
		MyState = State.Idle;
		Vector3.Distance(TargetObject.transform.position, base.transform.position);
		if (!CheckLineOfSight(TargetObject.transform.position, Mathf.Min(5f, VisionRange)))
		{
			PreventEndlessIdleState();
		}
	}

	private void FindPath(Vector3 PointToCheck)
	{
		if (MyState == State.Teleporting)
		{
			return;
		}
		if (TargetObject == null)
		{
			givePath(TargetPosition);
			RepathTimer = 2f;
			return;
		}
		Angle = Utils.GetAngle(TargetObject.transform.position, base.transform.position) * ((float)Math.PI / 180f);
		float radius = 1f;
		RaycastHit2D raycastHit2D = Physics2D.CircleCast(base.transform.position, radius, Vector3.Normalize(PointToCheck - base.transform.position), MaintainDistance, layerToCheck);
		if (raycastHit2D.collider != null)
		{
			if (Vector3.Distance(base.transform.position, raycastHit2D.centroid) > (float)AcceptableMove)
			{
				if (TeleportDelay > 0f || UnityEngine.Random.Range(0, 2) == 0)
				{
					if (ShowDebug)
					{
						Points.Add(new Vector3(raycastHit2D.centroid.x, raycastHit2D.centroid.y) + Vector3.Normalize(base.transform.position - PointToCheck) * CircleCastOffset);
						PointsLink.Add(new Vector3(base.transform.position.x, base.transform.position.y));
					}
					TargetPosition = (Vector3)raycastHit2D.centroid + Vector3.Normalize(base.transform.position - PointToCheck) * CircleCastOffset;
					givePath(TargetPosition);
					RepathTimer = 2f;
				}
				else
				{
					Teleport();
				}
			}
			else if (TeleportDelay < 0f && Vector3.Distance(base.transform.position, TargetObject.transform.position) < 2f)
			{
				Teleport();
			}
			else if (Vector3.Distance(base.transform.position, TargetObject.transform.position) > 5f)
			{
				if (TeleportDelay > 0f || UnityEngine.Random.Range(0, 2) == 0)
				{
					TargetPosition = Vector3.Lerp(base.transform.position, TargetObject.transform.position, 0.5f);
					givePath(TargetPosition);
					RepathTimer = 2f;
				}
				else
				{
					Teleport();
				}
			}
		}
		else
		{
			TargetPosition = PointToCheck;
			givePath(PointToCheck);
			RepathTimer = 2f;
		}
	}

	private void Teleport()
	{
		if (MyState != 0)
		{
			return;
		}
		if (damageColliderEvents != null)
		{
			damageColliderEvents.SetActive(false);
		}
		float num = 100f;
		if (!((num -= 1f) > 0f))
		{
			return;
		}
		float f = (Utils.GetAngle(base.transform.position, TargetObject.transform.position) + (float)UnityEngine.Random.Range(-90, 90)) * ((float)Math.PI / 180f);
		float num2 = 4f;
		float radius = 1f;
		Vector3 vector = TargetObject.transform.position + new Vector3(num2 * Mathf.Cos(f), num2 * Mathf.Sin(f));
		RaycastHit2D raycastHit2D = Physics2D.CircleCast(base.transform.position, radius, Vector3.Normalize(vector - base.transform.position), num2, layerToCheck);
		if (raycastHit2D.collider != null)
		{
			if (ShowDebug)
			{
				Points.Add(new Vector3(raycastHit2D.centroid.x, raycastHit2D.centroid.y) + Vector3.Normalize(base.transform.position - vector) * CircleCastOffset);
				PointsLink.Add(new Vector3(base.transform.position.x, base.transform.position.y));
			}
			StartCoroutine(TeleportRoutine((Vector3)raycastHit2D.centroid + Vector3.Normalize(base.transform.position - vector) * CircleCastOffset));
		}
		else
		{
			if (ShowDebug)
			{
				EndPoints.Add(new Vector3(vector.x, vector.y));
				EndPointsLink.Add(new Vector3(base.transform.position.x, base.transform.position.y));
			}
			StartCoroutine(TeleportRoutine(vector));
		}
	}

	private void OnDamageTriggerEnter(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && (component.team != health.team || health.team == Health.Team.PlayerTeam))
		{
			component.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.7f));
		}
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, VisionRange, Color.yellow);
		Utils.DrawCircleXY(TargetPosition, DistanceFromTargetPosition, Color.red);
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
