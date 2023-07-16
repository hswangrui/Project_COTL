using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using MMBiomeGeneration;
using Pathfinding;
using Spine.Unity;
using UnityEngine;

public class EnemyFollower : UnitObject
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

	public SkeletonAnimation Spine;

	public SimpleSpineFlash SimpleSpineFlash;

	public float CircleCastRadius = 0.5f;

	public float CircleCastOffset = 1f;

	public float TeleportDelayTarget = 1f;

	[EventRef]
	public string attackSoundPath = string.Empty;

	[EventRef]
	public string onHitSoundPath = string.Empty;

	private GameObject TargetObject;

	private float AttackDelay;

	private bool canBeParried;

	private static float signPostParryWindow = 0.2f;

	private static float attackParryWindow = 0.15f;

	private float followerTimestamp;

	private int variant;

	private Coroutine artificialUpdate;

	protected Vector3 Force;

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


	public bool VanishOnRoomComplete { get; set; }

	private void Start()
	{
		BiomeGenerator.OnBiomeChangeRoom += BiomeGenerator_OnBiomeChangeRoom;
		artificialUpdate = GameManager.GetInstance().StartCoroutine(ArtificialUpdate());
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
		health.OnDie += Health_OnDie;
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEvents.SetActive(false);
		}
		StartCoroutine(WaitForTarget());
		RoomLockController.OnRoomCleared += RoomLockController_OnRoomCleared;
	}

	private void Health_OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		StopAllCoroutines();
		StopCoroutine(artificialUpdate);
	}

	public override void OnDisable()
	{
		health.invincible = false;
		SimpleSpineFlash.FlashWhite(false);
		base.OnDisable();
		health.OnHitEarly -= OnHitEarly;
		health.OnDie -= Health_OnDie;
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent -= OnDamageTriggerEnter;
		}
		ClearPaths();
		StopAllCoroutines();
		RoomLockController.OnRoomCleared -= RoomLockController_OnRoomCleared;
	}

	private IEnumerator ArtificialUpdate()
	{
		while (true)
		{
			base.gameObject.SetActive(RespawnRoomManager.Instance == null || !RespawnRoomManager.Instance.gameObject.activeSelf);
			yield return null;
		}
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
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		if (!health.HasShield && !health.WasJustParried)
		{
			if (damageColliderEvents != null)
			{
				damageColliderEvents.SetActive(false);
			}
			if (!string.IsNullOrEmpty(onHitSoundPath))
			{
				AudioManager.Instance.PlayOneShot(onHitSoundPath, base.transform.position);
			}
			SimpleSpineFlash.FlashFillRed();
		}
	}

	protected virtual IEnumerator ApplyForceRoutine(GameObject Attacker)
	{
		DisableForces = true;
		Force = (base.transform.position - Attacker.transform.position).normalized * 500f;
		rb.AddForce(Force);
		yield return new WaitForSeconds(0.5f);
		DisableForces = false;
	}

	private IEnumerator HurtRoutine()
	{
		yield return new WaitForSeconds(0.3f);
		if (TargetObject != null && Vector2.Distance(base.transform.position, TargetObject.transform.position) < 3f)
		{
			StartCoroutine(FightPlayer(3.5f));
		}
		else
		{
			StartCoroutine(ChasePlayer());
		}
	}

	private IEnumerator WaitForTarget()
	{
		Spine.Initialize(false);
		state.CURRENT_STATE = StateMachine.State.Idle;
		Spine.AnimationState.SetAnimation(0, "idle-enemy", true);
		while (!GameManager.RoomActive)
		{
			yield return null;
		}
		while (TargetObject == null)
		{
			if (GetClosestTarget() != null)
			{
				TargetObject = GetClosestTarget().gameObject;
			}
			else if (PlayerFarming.Instance != null && Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) > 2f && Time.time > followerTimestamp && PathUtilities.IsPathPossible(AstarPath.active.GetNearest(base.transform.position).node, AstarPath.active.GetNearest(PlayerFarming.Instance.transform.position).node))
			{
				givePath(PlayerFarming.Instance.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 2f);
				followerTimestamp = Time.time + 0.25f;
			}
			yield return null;
		}
		bool InRange = false;
		while (!InRange)
		{
			if (TargetObject == null)
			{
				StartCoroutine(WaitForTarget());
				yield return null;
				continue;
			}
			if (Vector3.Distance(TargetObject.transform.position, base.transform.position) <= (float)VisionRange)
			{
				InRange = true;
			}
			yield return null;
		}
		StartCoroutine(ChasePlayer());
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
				if (!(GetClosestTarget() != null))
				{
					if (PlayerFarming.Instance != null && Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) > 2f && Time.time > followerTimestamp && PathUtilities.IsPathPossible(AstarPath.active.GetNearest(base.transform.position).node, AstarPath.active.GetNearest(PlayerFarming.Instance.transform.position).node))
					{
						givePath(PlayerFarming.Instance.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 2f);
						followerTimestamp = Time.time + 0.25f;
					}
					yield return null;
					continue;
				}
				TargetObject = GetClosestTarget().gameObject;
			}
			if (damageColliderEvents != null)
			{
				damageColliderEvents.SetActive(false);
			}
			TeleportDelay -= Time.deltaTime;
			AttackDelay -= Time.deltaTime;
			MaxAttackDelay -= Time.deltaTime;
			if (MyState == State.WaitAndTaunt)
			{
				if (Spine.AnimationName != "roll-stop" && state.CURRENT_STATE == StateMachine.State.Moving && Spine.AnimationName != "run-enemy")
				{
					Spine.AnimationState.SetAnimation(1, "run-enemy", true);
				}
				state.LookAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
				Spine.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
				if (state.CURRENT_STATE == StateMachine.State.Idle && (RepathTimer -= Time.deltaTime) < 0f)
				{
					if (CustomAttackLogic())
					{
						break;
					}
					if (MaxAttackDelay < 0f || Vector3.Distance(base.transform.position, TargetObject.transform.position) < AttackWithinRange)
					{
						if (ChargeAndAttack && (MaxAttackDelay < 0f || AttackDelay < 0f))
						{
							health.invincible = false;
							StopAllCoroutines();
							StartCoroutine(FightPlayer());
						}
						else if (!health.HasShield)
						{
							Angle = (Utils.GetAngle(TargetObject.transform.position, base.transform.position) + (float)UnityEngine.Random.Range(-20, 20)) * ((float)Math.PI / 180f);
							TargetPosition = TargetObject.transform.position + new Vector3(MaintainTargetDistance * Mathf.Cos(Angle), MaintainTargetDistance * Mathf.Sin(Angle));
							FindPath(TargetPosition);
						}
					}
					else if (Vector3.Distance(base.transform.position, TargetObject.transform.position) > MoveCloserDistance + (float)((!health.HasShield) ? 1 : 0))
					{
						Angle = (Utils.GetAngle(TargetObject.transform.position, base.transform.position) + (float)UnityEngine.Random.Range(-20, 20)) * ((float)Math.PI / 180f);
						TargetPosition = TargetObject.transform.position + new Vector3(MaintainTargetDistance * Mathf.Cos(Angle), MaintainTargetDistance * Mathf.Sin(Angle));
						FindPath(TargetPosition);
					}
				}
			}
			Seperate(0.5f);
			yield return null;
		}
	}

	public override void BeAlarmed(GameObject TargetObject)
	{
		base.BeAlarmed(TargetObject);
		this.TargetObject = TargetObject;
		StartCoroutine(ChasePlayer());
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
		if (!((num -= 1f) > 0f))
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
		Spine.AnimationState.SetAnimation(1, "roll", true);
		state.facingAngle = (state.LookAngle = Utils.GetAngle(base.transform.position, Position));
		Spine.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
		float num = Vector3.Distance(position, Position);
		float Duration = num / 10f;
		while (true)
		{
			float num2;
			Progress = (num2 = Progress + Time.deltaTime);
			if (!(num2 < Duration))
			{
				break;
			}
			speed = 10f * Time.deltaTime;
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		Spine.AnimationState.SetAnimation(1, "roll-stop", false);
		yield return new WaitForSeconds(0.3f);
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
		Spine.AnimationState.SetAnimation(1, "run-enemy", true);
		RepathTimer = 0f;
		int NumAttacks = ((!DoubleAttack) ? 1 : 2);
		int AttackCount = 1;
		float MaxAttackSpeed = 15f;
		float AttackSpeed = MaxAttackSpeed;
		bool Loop = true;
		float SignPostDelay = 0.5f;
		while (Loop)
		{
			Seperate(0.5f);
			if (TargetObject == null)
			{
				yield return new WaitForSeconds(0.3f);
				StartCoroutine(WaitForTarget());
				yield break;
			}
			if (state.CURRENT_STATE == StateMachine.State.Idle)
			{
				state.CURRENT_STATE = StateMachine.State.Moving;
			}
			switch (state.CURRENT_STATE)
			{
			case StateMachine.State.Moving:
				state.LookAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
				Spine.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
				state.LookAngle = (state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position));
				if (Vector2.Distance(base.transform.position, TargetObject.transform.position) < AttackDistance)
				{
					state.CURRENT_STATE = StateMachine.State.SignPostAttack;
					variant = UnityEngine.Random.Range(0, 2);
					string animationName2 = ((variant == 0) ? "attack-charge" : "attack-charge2");
					Spine.AnimationState.SetAnimation(1, animationName2, false);
				}
				else
				{
					if ((RepathTimer += Time.deltaTime) > 0.2f)
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
				state.Timer += Time.deltaTime;
				if (state.Timer >= SignPostDelay - signPostParryWindow)
				{
					canBeParried = true;
				}
				if (health.team == Health.Team.PlayerTeam && TargetObject != null)
				{
					state.LookAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
					Spine.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
					state.LookAngle = (state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position));
				}
				if (state.Timer >= SignPostDelay)
				{
					SimpleSpineFlash.FlashWhite(false);
					CameraManager.shakeCamera(0.4f, state.LookAngle);
					state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
					speed = AttackSpeed * (1f / 60f);
					string animationName3 = ((variant == 0) ? "attack-impact" : "attack-impact2");
					Spine.AnimationState.SetAnimation(1, animationName3, false);
					canBeParried = true;
					StartCoroutine(EnableDamageCollider(0f));
					if (!string.IsNullOrEmpty(attackSoundPath))
					{
						AudioManager.Instance.PlayOneShot(attackSoundPath, base.transform.position);
					}
				}
				break;
			case StateMachine.State.RecoverFromAttack:
				if (AttackSpeed > 0f)
				{
					AttackSpeed -= 1f * GameManager.DeltaTime;
				}
				speed = AttackSpeed * Time.deltaTime;
				SimpleSpineFlash.FlashWhite(false);
				canBeParried = state.Timer <= attackParryWindow;
				if ((state.Timer += Time.deltaTime) >= ((AttackCount + 1 <= NumAttacks) ? 0.5f : 1f))
				{
					int num = AttackCount + 1;
					AttackCount = num;
					if (num <= NumAttacks)
					{
						AttackSpeed = MaxAttackSpeed + (float)((3 - NumAttacks) * 2);
						state.CURRENT_STATE = StateMachine.State.SignPostAttack;
						variant = UnityEngine.Random.Range(0, 2);
						string animationName = ((variant == 0) ? "attack-charge" : "attack-charge2");
						Spine.AnimationState.SetAnimation(1, animationName, false);
						SignPostDelay = 0.3f;
					}
					else
					{
						Loop = false;
						SimpleSpineFlash.FlashWhite(false);
					}
				}
				break;
			}
			yield return null;
		}
		StartCoroutine(ChasePlayer());
	}

	private void BiomeGenerator_OnBiomeChangeRoom()
	{
		if (PlayerFarming.Instance != null)
		{
			ClearPaths();
			GameManager.GetInstance().StartCoroutine(PlaceIE());
		}
	}

	private void RoomLockController_OnRoomCleared()
	{
		if (VanishOnRoomComplete)
		{
			ClearPaths();
			StopAllCoroutines();
			StopCoroutine(artificialUpdate);
			state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			Spine.AnimationState.ClearTracks();
			Spine.AnimationState.SetAnimation(0, "spawn-out3", false);
			GameManager.GetInstance().StartCoroutine(Delay(1.5f, delegate
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}));
		}
	}

	private IEnumerator Delay(float delay, System.Action callback)
	{
		yield return new WaitForSeconds(delay);
		if (callback != null)
		{
			callback();
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
		if (EnemyHealth != null && EnemyHealth.team != health.team)
		{
			EnemyHealth.DealDamage(Damage, base.gameObject, Vector3.Lerp(base.transform.position, EnemyHealth.transform.position, 0.7f));
		}
	}

	private IEnumerator EnableDamageCollider(float initialDelay)
	{
		if ((bool)damageColliderEvents)
		{
			yield return new WaitForSeconds(initialDelay);
			damageColliderEvents.SetActive(true);
			yield return new WaitForSeconds(0.2f);
			damageColliderEvents.SetActive(false);
		}
	}
}
