using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using MMBiomeGeneration;
using Spine.Unity;
using UnityEngine;

public class EnemySwordsman : UnitObject
{
	private enum State
	{
		WaitAndTaunt,
		Teleporting,
		Attacking
	}

	public int NewPositionDistance = 3;

	public float MaintainTargetDistance = 4.5f;

	public float MoveCloserDistance = 4f;

	public float AttackWithinRange = 4f;

	public bool DoubleAttack = true;

	public bool ChargeAndAttack = true;

	public Vector2 MaxAttackDelayRandomRange = new Vector2(4f, 6f);

	public Vector2 AttackDelayRandomRange = new Vector2(0.5f, 2f);

	public ColliderEvents damageColliderEvents;

	[SerializeField]
	private bool requireLineOfSite = true;

	public SkeletonAnimation Spine;

	public SimpleSpineFlash SimpleSpineFlash;

	public float CircleCastRadius = 0.5f;

	public float CircleCastOffset = 1f;

	public float TeleportDelayTarget = 1f;

	[EventRef]
	public string attackSoundPath = string.Empty;

	[EventRef]
	public string onHitSoundPath = string.Empty;

	[EventRef]
	public string AttackVO = string.Empty;

	[EventRef]
	public string DeathVO = string.Empty;

	[EventRef]
	public string GetHitVO = string.Empty;

	[EventRef]
	public string WarningVO = string.Empty;

	private GameObject TargetObject;

	private float AttackDelay;

	private bool canBeParried;

	private static float signPostParryWindow = 0.2f;

	private static float attackParryWindow = 0.15f;

	protected Vector3 Force;

	public float KnockbackModifier = 1f;

	[HideInInspector]
	public float Angle;

	private Vector3 TargetPosition;

	private float RepathTimer;

	private float TeleportDelay;

	private State MyState;

	private float MaxAttackDelay;

	private Health EnemyHealth;

	public bool ShowDebug;

	public List<Vector3> Points = new List<Vector3>();

	public List<Vector3> PointsLink = new List<Vector3>();

	public List<Vector3> EndPoints = new List<Vector3>();

	public List<Vector3> EndPointsLink = new List<Vector3>();

	public float Damage { get; set; } = 1f;


	public bool FollowPlayer { get; set; }

	public override void Awake()
	{
		base.Awake();
		if (BiomeGenerator.Instance != null)
		{
			GetComponent<Health>().totalHP *= BiomeGenerator.Instance.HumanoidHealthMultiplier;
		}
		BiomeGenerator.OnBiomeChangeRoom += BiomeGenerator_OnBiomeChangeRoom;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		BiomeGenerator.OnBiomeChangeRoom -= BiomeGenerator_OnBiomeChangeRoom;
	}

	public override void OnEnable()
	{
		SeperateObject = true;
		base.OnEnable();
		health.OnHitEarly += OnHitEarly;
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEvents.SetActive(false);
		}
		StartCoroutine(WaitForTarget());
		rb.simulated = true;
	}

	public override void OnDisable()
	{
		health.invincible = false;
		SimpleSpineFlash.FlashWhite(false);
		base.OnDisable();
		health.OnHitEarly -= OnHitEarly;
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent -= OnDamageTriggerEnter;
		}
		ClearPaths();
		StopAllCoroutines();
		DisableForces = false;
	}

	private void OnHitEarly(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (PlayerController.CanParryAttacks && canBeParried && !FromBehind && AttackType == Health.AttackTypes.Melee)
		{
			health.WasJustParried = true;
			SimpleSpineFlash.FlashWhite(false);
			SeperateObject = true;
			UsePathing = true;
			health.invincible = false;
			StopAllCoroutines();
			DisableForces = false;
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

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		if (health.HasShield || health.WasJustParried)
		{
			return;
		}
		if (damageColliderEvents != null)
		{
			damageColliderEvents.SetActive(false);
		}
		if (!string.IsNullOrEmpty(onHitSoundPath))
		{
			AudioManager.Instance.PlayOneShot(onHitSoundPath, base.transform.position);
		}
		if (!string.IsNullOrEmpty(GetHitVO))
		{
			AudioManager.Instance.PlayOneShot(GetHitVO, base.transform.position);
		}
		Spine.AnimationState.SetAnimation(1, "hurt-eyes", false);
		if (MyState != State.Attacking)
		{
			SimpleSpineFlash.FlashWhite(false);
			SeperateObject = true;
			UsePathing = true;
			health.invincible = false;
			StopAllCoroutines();
			DisableForces = false;
			if (AttackLocation.x > base.transform.position.x && state.CURRENT_STATE != StateMachine.State.HitRight)
			{
				state.CURRENT_STATE = StateMachine.State.HitRight;
			}
			if (AttackLocation.x < base.transform.position.x && state.CURRENT_STATE != StateMachine.State.HitLeft)
			{
				state.CURRENT_STATE = StateMachine.State.HitLeft;
			}
			if (AttackType != Health.AttackTypes.Heavy && (!(AttackType == Health.AttackTypes.Projectile && FromBehind) || health.HasShield))
			{
				StartCoroutine(HurtRoutine());
			}
		}
		if (AttackType == Health.AttackTypes.Projectile && !health.HasShield)
		{
			state.facingAngle = (state.LookAngle = Utils.GetAngle(base.transform.position, Attacker.transform.position));
			Spine.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
		}
		if (AttackType != Health.AttackTypes.NoKnockBack)
		{
			StartCoroutine(ApplyForceRoutine(Attacker));
		}
		SimpleSpineFlash.FlashFillRed();
	}

	protected virtual IEnumerator ApplyForceRoutine(GameObject Attacker)
	{
		DisableForces = true;
		Force = (base.transform.position - Attacker.transform.position).normalized * 500f;
		rb.AddForce(Force * KnockbackModifier);
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
		DisableForces = false;
	}

	private IEnumerator HurtRoutine()
	{
		float time = 0f;
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
		StartCoroutine(WaitForTarget());
	}

	protected IEnumerator WaitForTarget()
	{
		Spine.Initialize(false);
		while (!GameManager.RoomActive)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		while (TargetObject == null)
		{
			Health closestTarget = GetClosestTarget(health.team == Health.Team.PlayerTeam);
			if ((bool)closestTarget)
			{
				TargetObject = closestTarget.gameObject;
				requireLineOfSite = false;
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
				if (!FollowPlayer)
				{
					TargetPosition = base.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 2f;
				}
				FindPath(TargetPosition);
				state.LookAngle = Utils.GetAngle(base.transform.position, TargetPosition);
				Spine.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
			}
			yield return null;
		}
		bool InRange = false;
		while (!InRange)
		{
			if (TargetObject == null)
			{
				StartCoroutine(WaitForTarget());
				yield break;
			}
			float num = Vector3.Distance(TargetObject.transform.position, base.transform.position);
			if (num <= (float)VisionRange)
			{
				if (!requireLineOfSite || CheckLineOfSight(TargetObject.transform.position, Mathf.Min(num, VisionRange)))
				{
					InRange = true;
				}
				else
				{
					LookAtTarget();
				}
			}
			yield return null;
		}
		StartCoroutine(ChasePlayer());
	}

	private void LookAtTarget()
	{
		if (!(GetClosestTarget() == null))
		{
			float angle = Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position);
			state.facingAngle = angle;
			state.LookAngle = angle;
			if (Spine.AnimationName != "jeer")
			{
				Spine.randomOffset = true;
				Spine.AnimationState.SetAnimation(0, "jeer", true);
			}
		}
	}

	public IEnumerator ChasePlayer()
	{
		MyState = State.WaitAndTaunt;
		state.CURRENT_STATE = StateMachine.State.Idle;
		AttackDelay = UnityEngine.Random.Range(AttackDelayRandomRange.x, AttackDelayRandomRange.y);
		if (health.HasShield)
		{
			AttackDelay = 2.5f;
		}
		MaxAttackDelay = UnityEngine.Random.Range(MaxAttackDelayRandomRange.x, MaxAttackDelayRandomRange.y);
		bool Loop = true;
		while (Loop)
		{
			if (TargetObject == null)
			{
				StartCoroutine(WaitForTarget());
				break;
			}
			if (damageColliderEvents != null)
			{
				damageColliderEvents.SetActive(false);
			}
			TeleportDelay -= Time.deltaTime * Spine.timeScale;
			AttackDelay -= Time.deltaTime * Spine.timeScale;
			MaxAttackDelay -= Time.deltaTime * Spine.timeScale;
			if (MyState == State.WaitAndTaunt)
			{
				if (Spine.AnimationName != "roll-stop" && state.CURRENT_STATE == StateMachine.State.Moving)
				{
					if (Spine.AnimationName != "run")
					{
						Spine.AnimationState.SetAnimation(0, "run", true);
					}
				}
				else if (Spine.AnimationName != "cheer1")
				{
					Spine.AnimationState.SetAnimation(0, "cheer1", true);
				}
				if (TargetObject == PlayerFarming.Instance.gameObject && health.IsCharmed && GetClosestTarget() != null)
				{
					TargetObject = GetClosestTarget().gameObject;
				}
				state.LookAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
				Spine.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
				if (state.CURRENT_STATE == StateMachine.State.Idle)
				{
					if ((RepathTimer -= Time.deltaTime * Spine.timeScale) < 0f)
					{
						if (CustomAttackLogic())
						{
							break;
						}
						if (MaxAttackDelay < 0f || Vector3.Distance(base.transform.position, TargetObject.transform.position) < AttackWithinRange)
						{
							AttackWithinRange = float.MaxValue;
							if ((bool)TargetObject)
							{
								if (ChargeAndAttack && (MaxAttackDelay < 0f || AttackDelay < 0f))
								{
									health.invincible = false;
									StopAllCoroutines();
									DisableForces = false;
									StartCoroutine(FightPlayer());
								}
								else if (!health.HasShield)
								{
									Angle = (Utils.GetAngle(TargetObject.transform.position, base.transform.position) + (float)UnityEngine.Random.Range(-20, 20)) * ((float)Math.PI / 180f);
									TargetPosition = TargetObject.transform.position + new Vector3(MaintainTargetDistance * Mathf.Cos(Angle), MaintainTargetDistance * Mathf.Sin(Angle));
									FindPath(TargetPosition);
								}
							}
						}
						else if ((bool)TargetObject && Vector3.Distance(base.transform.position, TargetObject.transform.position) > MoveCloserDistance + (float)((!health.HasShield) ? 1 : 0))
						{
							Angle = (Utils.GetAngle(TargetObject.transform.position, base.transform.position) + (float)UnityEngine.Random.Range(-20, 20)) * ((float)Math.PI / 180f);
							TargetPosition = TargetObject.transform.position + new Vector3(MaintainTargetDistance * Mathf.Cos(Angle), MaintainTargetDistance * Mathf.Sin(Angle));
							FindPath(TargetPosition);
						}
					}
				}
				else if ((RepathTimer += Time.deltaTime * Spine.timeScale) > 2f)
				{
					RepathTimer = 0f;
					state.CURRENT_STATE = StateMachine.State.Idle;
				}
			}
			Seperate(0.5f);
			yield return null;
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
		float num = Vector3.Distance(TargetObject.transform.position, base.transform.position);
		if (num <= (float)VisionRange)
		{
			if (!requireLineOfSite || CheckLineOfSight(TargetObject.transform.position, Mathf.Min(num, VisionRange)))
			{
				StartCoroutine(WaitForTarget());
			}
			else
			{
				LookAtTarget();
			}
		}
	}

	public virtual bool CustomAttackLogic()
	{
		return false;
	}

	private void FindPath(Vector3 PointToCheck)
	{
		RepathTimer = 0.2f;
		float radius = 0.2f;
		RaycastHit2D raycastHit2D = Physics2D.CircleCast(base.transform.position, radius, Vector3.Normalize(PointToCheck - base.transform.position), NewPositionDistance, layerToCheck);
		if (raycastHit2D.collider != null)
		{
			if (Vector3.Distance(base.transform.position, raycastHit2D.centroid) > 1f)
			{
				if (ShowDebug)
				{
					Points.Add(new Vector3(raycastHit2D.centroid.x, raycastHit2D.centroid.y) + Vector3.Normalize(base.transform.position - PointToCheck) * CircleCastOffset);
					PointsLink.Add(new Vector3(base.transform.position.x, base.transform.position.y));
				}
				TargetPosition = (Vector3)raycastHit2D.centroid + Vector3.Normalize(base.transform.position - PointToCheck) * CircleCastOffset;
				givePath(TargetPosition);
			}
			else if (TeleportDelay < 0f)
			{
				Teleport();
			}
		}
		else if (FollowPlayer && PlayerFarming.Instance != null)
		{
			if (Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) > 1.5f)
			{
				TargetPosition = PlayerFarming.Instance.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle;
				givePath(TargetPosition);
			}
		}
		else
		{
			TargetPosition = PointToCheck;
			givePath(PointToCheck);
		}
	}

	private void Teleport()
	{
		if (MyState != 0 || health.HP <= 0f)
		{
			return;
		}
		float num = 100f;
		if (!((num -= 1f) > 0f) || TargetObject == null)
		{
			return;
		}
		float f = (Utils.GetAngle(base.transform.position, TargetObject.transform.position) + (float)UnityEngine.Random.Range(-90, 90)) * ((float)Math.PI / 180f);
		float num2 = 4.5f;
		float radius = 0.2f;
		Vector3 vector = TargetObject.transform.position + new Vector3(num2 * Mathf.Cos(f), num2 * Mathf.Sin(f));
		RaycastHit2D raycastHit2D = Physics2D.CircleCast(base.transform.position, radius, Vector3.Normalize(vector - base.transform.position), num2, layerToCheck);
		if (raycastHit2D.collider != null)
		{
			if (ShowDebug)
			{
				Points.Add(new Vector3(raycastHit2D.centroid.x, raycastHit2D.centroid.y) + Vector3.Normalize(base.transform.position - vector) * CircleCastOffset);
				PointsLink.Add(new Vector3(base.transform.position.x, base.transform.position.y));
			}
			StartCoroutine(TeleportRoutine(raycastHit2D.centroid));
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

	private IEnumerator TeleportRoutine(Vector3 Position)
	{
		ClearPaths();
		state.CURRENT_STATE = StateMachine.State.Moving;
		UsePathing = false;
		health.invincible = true;
		SeperateObject = false;
		MyState = State.Teleporting;
		ClearPaths();
		Vector3 position = base.transform.position;
		float Progress = 0f;
		Spine.AnimationState.SetAnimation(0, "roll", true);
		state.facingAngle = (state.LookAngle = Utils.GetAngle(base.transform.position, Position));
		Spine.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
		float num = Vector3.Distance(position, Position);
		float Duration = num / 10f;
		while (true)
		{
			float num2;
			Progress = (num2 = Progress + Time.deltaTime * Spine.timeScale);
			if (!(num2 < Duration))
			{
				break;
			}
			speed = 10f * Time.deltaTime * Spine.timeScale;
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
		RepathTimer = 0.5f;
		TeleportDelay = TeleportDelayTarget;
		SeperateObject = true;
		health.invincible = false;
		MyState = State.WaitAndTaunt;
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

	private IEnumerator FightPlayer(float AttackDistance = 1.5f)
	{
		MyState = State.Attacking;
		UsePathing = true;
		givePath(TargetObject.transform.position);
		Spine.AnimationState.SetAnimation(0, "run-charge", true);
		RepathTimer = 0f;
		int NumAttacks = ((!DoubleAttack) ? 1 : 2);
		int AttackCount = 1;
		float MaxAttackSpeed = 15f;
		float AttackSpeed = MaxAttackSpeed;
		bool Loop = true;
		float SignPostDelay = 0.5f;
		while (Loop)
		{
			if (Spine == null || Spine.AnimationState == null || Spine.Skeleton == null)
			{
				yield return null;
				continue;
			}
			Seperate(0.5f);
			switch (state.CURRENT_STATE)
			{
			case StateMachine.State.Moving:
				if ((bool)TargetObject)
				{
					state.LookAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
					Spine.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
					state.LookAngle = (state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position));
				}
				if (TargetObject != null && Vector2.Distance(base.transform.position, TargetObject.transform.position) < AttackDistance)
				{
					state.CURRENT_STATE = StateMachine.State.SignPostAttack;
					Spine.AnimationState.SetAnimation(0, (AttackCount == NumAttacks) ? "grunt-attack-charge2" : "grunt-attack-charge", false);
				}
				else
				{
					if ((RepathTimer += Time.deltaTime * Spine.timeScale) > 0.2f && (bool)TargetObject)
					{
						RepathTimer = 0f;
						givePath(TargetObject.transform.position);
					}
					if (damageColliderEvents != null)
					{
						if (state.Timer < 0.2f && !health.WasJustParried)
						{
							damageColliderEvents.SetActive(true);
						}
						else
						{
							damageColliderEvents.SetActive(false);
						}
					}
				}
				if (damageColliderEvents != null)
				{
					damageColliderEvents.SetActive(false);
				}
				break;
			case StateMachine.State.SignPostAttack:
				if (damageColliderEvents != null)
				{
					damageColliderEvents.SetActive(false);
				}
				SimpleSpineFlash.FlashWhite(state.Timer / SignPostDelay);
				state.Timer += Time.deltaTime * Spine.timeScale;
				if (state.Timer >= SignPostDelay - signPostParryWindow)
				{
					canBeParried = true;
				}
				if (state.Timer >= SignPostDelay)
				{
					SimpleSpineFlash.FlashWhite(false);
					CameraManager.shakeCamera(0.4f, state.LookAngle);
					state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
					speed = AttackSpeed * (1f / 60f);
					Spine.AnimationState.SetAnimation(0, (AttackCount == NumAttacks) ? "grunt-attack-impact2" : "grunt-attack-impact", false);
					canBeParried = true;
					StartCoroutine(EnableDamageCollider(0f));
					if (!string.IsNullOrEmpty(attackSoundPath))
					{
						AudioManager.Instance.PlayOneShot(attackSoundPath, base.transform.position);
					}
					if (!string.IsNullOrEmpty(AttackVO))
					{
						AudioManager.Instance.PlayOneShot(AttackVO, base.transform.position);
					}
				}
				break;
			case StateMachine.State.RecoverFromAttack:
				if (AttackSpeed > 0f)
				{
					AttackSpeed -= 1f * GameManager.DeltaTime * Spine.timeScale;
				}
				speed = AttackSpeed * Time.deltaTime * Spine.timeScale;
				SimpleSpineFlash.FlashWhite(false);
				canBeParried = state.Timer <= attackParryWindow;
				if ((state.Timer += Time.deltaTime * Spine.timeScale) >= ((AttackCount + 1 <= NumAttacks) ? 0.5f : 1f))
				{
					int num = AttackCount + 1;
					AttackCount = num;
					if (num <= NumAttacks)
					{
						AttackSpeed = MaxAttackSpeed + (float)((3 - NumAttacks) * 2);
						state.CURRENT_STATE = StateMachine.State.SignPostAttack;
						Spine.AnimationState.SetAnimation(0, "grunt-attack-charge2", false);
						SignPostDelay = 0.3f;
					}
					else
					{
						Loop = false;
						SimpleSpineFlash.FlashWhite(false);
					}
				}
				break;
			case StateMachine.State.Idle:
				TargetObject = null;
				StartCoroutine(WaitForTarget());
				yield break;
			}
			yield return null;
		}
		TargetObject = null;
		StartCoroutine(WaitForTarget());
	}

	private void BiomeGenerator_OnBiomeChangeRoom()
	{
		if (PlayerFarming.Instance != null && FollowPlayer)
		{
			ClearPaths();
			GameManager.GetInstance().StartCoroutine(PlaceIE());
		}
	}

	private IEnumerator PlaceIE()
	{
		ClearPaths();
		Vector3 offset = UnityEngine.Random.insideUnitCircle;
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			state.CURRENT_STATE = StateMachine.State.Moving;
			Vector3 position = PlayerFarming.Instance.transform.position + offset;
			position.z = 0f;
			base.transform.position = position;
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		Spine.AnimationState.SetAnimation(0, "idle-enemy", true);
	}

	private void OnDamageTriggerEnter(Collider2D collider)
	{
		EnemyHealth = collider.GetComponent<Health>();
		if (EnemyHealth != null)
		{
			if (EnemyHealth.team != health.team)
			{
				EnemyHealth.DealDamage(Damage, base.gameObject, Vector3.Lerp(base.transform.position, EnemyHealth.transform.position, 0.7f));
			}
			else if (health.team == Health.Team.PlayerTeam && !health.isPlayerAlly && !EnemyHealth.isPlayer)
			{
				EnemyHealth.DealDamage(Damage, base.gameObject, Vector3.Lerp(base.transform.position, EnemyHealth.transform.position, 0.7f));
			}
		}
	}

	private IEnumerator EnableDamageCollider(float initialDelay)
	{
		if (!damageColliderEvents)
		{
			yield break;
		}
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < initialDelay))
			{
				break;
			}
			yield return null;
		}
		damageColliderEvents.SetActive(true);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.2f))
			{
				break;
			}
			yield return null;
		}
		damageColliderEvents.SetActive(false);
	}
}
