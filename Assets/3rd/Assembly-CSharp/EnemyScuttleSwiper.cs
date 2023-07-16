using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using Spine.Unity;
using UnityEngine;

public class EnemyScuttleSwiper : UnitObject
{
	public enum StartingStates
	{
		Hidden,
		Wandering,
		Animation,
		Intro
	}

	private static List<EnemyScuttleSwiper> Scuttlers = new List<EnemyScuttleSwiper>();

	public StartingStates StartHidden;

	public bool DetectPlayerWhileHidden = true;

	public bool HiddenOffsetIsGlobalPosition;

	public Vector3 HiddenOffset = Vector3.zero;

	public float HiddenRadius = 5f;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string UnawareAnimation;

	public ColliderEvents damageColliderEvents;

	public SkeletonAnimation Spine;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string IdleAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string MovingAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string SignPostAttackAnimation;

	public bool LoopSignPostAttackAnimation = true;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string AttackAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string FallAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string LandAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string SignPostSlamAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string SlamAnimation;

	public SpriteRenderer ShadowSpriteRenderer;

	public SimpleSpineFlash[] SimpleSpineFlashes;

	public float KnockbackModifier = 1f;

	public int NumberOfAttacks = 1;

	public float AttackForceModifier = 1f;

	public bool CounterAttack;

	public bool SlamAttack;

	public bool CanBeInterrupted = true;

	public bool AttackTowardsPlayer;

	public float DamageColliderDuration = -1f;

	[Range(0f, 1f)]
	public float ChanceToPathTowardsPlayer;

	public int DistanceToPathTowardsPlayer = 6;

	public float SlamAttackRange;

	public float TimeBetweenSlams;

	public GameObject SlamRockPrefab;

	private float SlamTimer;

	public SkeletonAnimation warningIcon;

	[EventRef]
	public string AttackVO = string.Empty;

	[EventRef]
	public string DeathVO = string.Empty;

	[EventRef]
	public string GetHitVO = string.Empty;

	[EventRef]
	public string WarningVO = string.Empty;

	protected GameObject TargetObject;

	private float RandomDirection;

	private bool ShownWarning;

	private float GravitySpeed = 1f;

	private float HidingHeight = 5f;

	public float AttackDelayTime;

	protected bool Attacking;

	protected bool IsStunned;

	[HideInInspector]
	public float AttackDelay;

	public float AttackDuration = 1f;

	public float SignPostAttackDuration = 0.5f;

	public bool DisableKnockback;

	private float Angle;

	private Vector3 Force;

	public float TurningArc = 90f;

	public Vector2 DistanceRange = new Vector2(1f, 3f);

	public Vector2 IdleWaitRange = new Vector2(1f, 3f);

	protected float IdleWait;

	private bool PathingToPlayer;

	protected Health EnemyHealth;

	public float CircleCastRadius = 0.5f;

	public float CircleCastOffset = 1f;

	public bool ShowDebug;

	public List<Vector3> Points = new List<Vector3>();

	public List<Vector3> PointsLink = new List<Vector3>();

	public List<Vector3> EndPoints = new List<Vector3>();

	public List<Vector3> EndPointsLink = new List<Vector3>();

	public override void Awake()
	{
		base.Awake();
		SimpleSpineFlashes = GetComponentsInChildren<SimpleSpineFlash>();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		Scuttlers.Add(this);
		SlamTimer = TimeBetweenSlams;
		RandomDirection = (float)UnityEngine.Random.Range(0, 360) * ((float)Math.PI / 180f);
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEvents.SetActive(false);
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		if (GameManager.RoomActive)
		{
			IdleWait = 0f;
			TargetObject = null;
			Attacking = false;
			IsStunned = false;
			health.invincible = false;
			SimpleSpineFlash[] simpleSpineFlashes = SimpleSpineFlashes;
			for (int i = 0; i < simpleSpineFlashes.Length; i++)
			{
				simpleSpineFlashes[i].FlashWhite(false);
			}
			StartCoroutine(ActiveRoutine());
		}
		else
		{
			switch (StartHidden)
			{
			case StartingStates.Hidden:
				StartCoroutine(Hidden());
				break;
			case StartingStates.Wandering:
				StartCoroutine(ActiveRoutine());
				break;
			case StartingStates.Animation:
				StartCoroutine(AnimationRoutine());
				break;
			}
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		Scuttlers.Remove(this);
		if (damageColliderEvents != null)
		{
			damageColliderEvents.SetActive(false);
			damageColliderEvents.OnTriggerEnterEvent -= OnDamageTriggerEnter;
		}
		ClearPaths();
		StopAllCoroutines();
		DisableForces = false;
		SimpleSpineFlash[] simpleSpineFlashes = SimpleSpineFlashes;
		for (int i = 0; i < simpleSpineFlashes.Length; i++)
		{
			simpleSpineFlashes[i].FlashWhite(false);
		}
	}

	public void ShowWarningIcon()
	{
		if (!(warningIcon == null) && !ShownWarning)
		{
			warningIcon.AnimationState.SetAnimation(0, "warn-start", false);
			warningIcon.AnimationState.AddAnimation(0, "warn-stop", false, 2f);
			ShownWarning = true;
			if (!string.IsNullOrEmpty(WarningVO))
			{
				AudioManager.Instance.PlayOneShot(WarningVO, base.transform.position);
			}
		}
	}

	private IEnumerator AnimationRoutine()
	{
		yield return new WaitForEndOfFrame();
		health.invincible = true;
		Debug.Log("Spine " + Spine);
		Debug.Log("Spine.AnimationState " + Spine.AnimationState);
		Debug.Log("UnawareAnimation " + UnawareAnimation);
		Spine.AnimationState.SetAnimation(0, UnawareAnimation, true);
		while (GameManager.RoomActive)
		{
			yield return null;
		}
		while (TargetObject == null)
		{
			if (Time.frameCount % 5 == 0)
			{
				GetNewTarget();
			}
			yield return null;
		}
		while (TargetObject != null && Vector3.Distance((HiddenOffsetIsGlobalPosition ? Vector3.zero : base.transform.position) + HiddenOffset, TargetObject.transform.position) > HiddenRadius)
		{
			yield return null;
		}
		if (TargetObject != null)
		{
			state.LookAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
		}
		ShowWarningIcon();
		health.invincible = false;
		Spine.AnimationState.SetAnimation(0, IdleAnimation, true);
		AttackDelay = 0f;
		StartCoroutine(ActiveRoutine());
	}

	private IEnumerator Hidden()
	{
		health.invincible = true;
		Spine.transform.localPosition = Vector3.back * HidingHeight;
		ShadowSpriteRenderer.enabled = false;
		Spine.gameObject.GetComponent<MeshRenderer>().enabled = false;
		while (GameManager.RoomActive)
		{
			yield return null;
		}
		while (TargetObject == null)
		{
			if (Time.frameCount % 5 == 0)
			{
				GetNewTarget();
			}
			yield return null;
		}
		while (DetectPlayerWhileHidden && Vector3.Distance((HiddenOffsetIsGlobalPosition ? Vector3.zero : base.transform.position) + HiddenOffset, TargetObject.transform.position) > HiddenRadius)
		{
			yield return null;
		}
		RevealAll();
	}

	private void RevealAll()
	{
		float num = -0.2f;
		foreach (EnemyScuttleSwiper scuttler in Scuttlers)
		{
			if (scuttler.StartHidden == StartingStates.Hidden)
			{
				scuttler.StopAllCoroutines();
				DisableForces = false;
				scuttler.StartCoroutine(scuttler.Reveal(num += 0.2f));
			}
		}
	}

	public IEnumerator Reveal(float Delay)
	{
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < Delay))
			{
				break;
			}
			yield return null;
		}
		Spine.gameObject.GetComponent<MeshRenderer>().enabled = true;
		Spine.AnimationState.SetAnimation(0, FallAnimation, true);
		AudioManager.Instance.PlayOneShot("event:/enemy/fall_from_sky", Spine.transform.gameObject);
		ShadowSpriteRenderer.enabled = true;
		float Grav = 0f;
		while (Spine.transform.localPosition.z + Grav < 0f)
		{
			Grav += Time.fixedDeltaTime * GravitySpeed;
			Spine.transform.localPosition = Spine.transform.localPosition + Vector3.forward * Grav;
			ShadowSpriteRenderer.transform.localScale = Vector3.one * ((0f - Spine.transform.localPosition.z - HidingHeight) / HidingHeight);
			yield return new WaitForFixedUpdate();
		}
		if (orderIndicator != null)
		{
			orderIndicator.gameObject.SetActive(true);
			orderIndicator.transform.parent.gameObject.SetActive(true);
		}
		Spine.transform.localPosition = Vector3.zero;
		ShadowSpriteRenderer.transform.localScale = Vector3.one;
		health.invincible = false;
		Spine.AnimationState.SetAnimation(0, LandAnimation, false);
		AudioManager.Instance.PlayOneShot("event:/enemy/land_normal", Spine.transform.gameObject);
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
		Spine.AnimationState.SetAnimation(0, IdleAnimation, true);
		AttackDelay = 0f;
		StartCoroutine(ActiveRoutine());
	}

	protected virtual IEnumerator ActiveRoutine()
	{
		yield return new WaitForEndOfFrame();
		while (true)
		{
			if (Spine.timeScale == 0.0001f)
			{
				yield return null;
				continue;
			}
			if (state.CURRENT_STATE == StateMachine.State.Idle && (IdleWait -= Time.deltaTime) <= 0f)
			{
				GetNewTargetPosition();
			}
			if (TargetObject != null && !Attacking && !IsStunned && GameManager.RoomActive)
			{
				state.LookAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
			}
			else
			{
				state.LookAngle = state.facingAngle;
			}
			if (MovingAnimation != "")
			{
				if (state.CURRENT_STATE == StateMachine.State.Moving && Spine.AnimationName != MovingAnimation)
				{
					Spine.AnimationState.SetAnimation(0, MovingAnimation, true);
				}
				if (state.CURRENT_STATE == StateMachine.State.Idle && Spine.AnimationName != IdleAnimation)
				{
					Spine.AnimationState.SetAnimation(0, IdleAnimation, true);
				}
			}
			if (TargetObject == null)
			{
				GetNewTarget();
				if (TargetObject != null)
				{
					ShowWarningIcon();
				}
			}
			else
			{
				if (ShouldSlam())
				{
					StartCoroutine(SlamRoutine());
				}
				if (ShouldAttack())
				{
					StartCoroutine(AttackRoutine());
				}
			}
			yield return null;
		}
	}

	protected virtual bool ShouldSlam()
	{
		if ((SlamTimer -= Time.deltaTime) < 0f && !Attacking && Vector3.Distance(base.transform.position, TargetObject.transform.position) < SlamAttackRange)
		{
			return GameManager.RoomActive;
		}
		return false;
	}

	protected virtual bool ShouldAttack()
	{
		if ((AttackDelay -= Time.deltaTime) < 0f && !Attacking && Vector3.Distance(base.transform.position, TargetObject.transform.position) < (float)VisionRange)
		{
			return GameManager.RoomActive;
		}
		return false;
	}

	private IEnumerator SlamRoutine()
	{
		Attacking = true;
		ClearPaths();
		Spine.AnimationState.SetAnimation(0, SignPostSlamAnimation, false);
		state.CURRENT_STATE = StateMachine.State.SignPostAttack;
		float Progress = 0f;
		float Duration = 0.5f;
		SimpleSpineFlash[] simpleSpineFlashes;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			simpleSpineFlashes = SimpleSpineFlashes;
			for (int j = 0; j < simpleSpineFlashes.Length; j++)
			{
				simpleSpineFlashes[j].FlashWhite(Progress / Duration);
			}
			yield return null;
		}
		simpleSpineFlashes = SimpleSpineFlashes;
		for (int j = 0; j < simpleSpineFlashes.Length; j++)
		{
			simpleSpineFlashes[j].FlashWhite(false);
		}
		CameraManager.instance.ShakeCameraForDuration(0.4f, 0.6f, 0.5f);
		state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
		Spine.AnimationState.SetAnimation(0, SlamAnimation, false);
		float time = 0f;
		float SlamDistance = 1.5f;
		float Rocks = 10f;
		int i = -1;
		while (true)
		{
			int j = i + 1;
			i = j;
			if (j >= 3)
			{
				break;
			}
			int num2 = -1;
			float num3 = 0f;
			while ((float)(++num2) <= Rocks)
			{
				num3 += 360f / Rocks * ((float)Math.PI / 180f);
				UnityEngine.Object.Instantiate(SlamRockPrefab, base.transform.position + new Vector3(SlamDistance * Mathf.Cos(num3), SlamDistance * Mathf.Sin(num3)), Quaternion.identity, base.transform.parent).GetComponent<ForestScuttlerSlamBarricade>().Play(0f);
			}
			time = 0f;
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * Spine.timeScale);
				if (!(num < 0.2f))
				{
					break;
				}
				yield return null;
			}
			SlamDistance += 1f;
			Rocks += 2f;
		}
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
		Spine.AnimationState.SetAnimation(0, IdleAnimation, true);
		state.CURRENT_STATE = StateMachine.State.Idle;
		IdleWait = 0f;
		SlamTimer = TimeBetweenSlams;
		TargetObject = null;
		Attacking = false;
	}

	protected virtual IEnumerator AttackRoutine()
	{
		Attacking = true;
		ClearPaths();
		int CurrentAttack = 0;
		float time;
		while (true)
		{
			int num = CurrentAttack + 1;
			CurrentAttack = num;
			if (num > NumberOfAttacks)
			{
				break;
			}
			Spine.AnimationState.SetAnimation(0, SignPostAttackAnimation, LoopSignPostAttackAnimation);
			state.CURRENT_STATE = StateMachine.State.SignPostAttack;
			AudioManager.Instance.PlayOneShot("event:/enemy/chaser/chaser_charge", base.transform.position);
			if (TargetObject != null)
			{
				state.LookAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
				state.facingAngle = state.LookAngle;
			}
			float Progress = 0f;
			float Duration = SignPostAttackDuration;
			SimpleSpineFlash[] simpleSpineFlashes;
			while (true)
			{
				float num2;
				Progress = (num2 = Progress + Time.deltaTime);
				if (!(num2 < Duration / Spine.timeScale))
				{
					break;
				}
				simpleSpineFlashes = SimpleSpineFlashes;
				for (num = 0; num < simpleSpineFlashes.Length; num++)
				{
					simpleSpineFlashes[num].FlashWhite(Progress / Duration);
				}
				yield return null;
			}
			simpleSpineFlashes = SimpleSpineFlashes;
			for (num = 0; num < simpleSpineFlashes.Length; num++)
			{
				simpleSpineFlashes[num].FlashWhite(false);
			}
			if (AttackTowardsPlayer)
			{
				if (TargetObject != null)
				{
					state.LookAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
					state.facingAngle = state.LookAngle;
				}
				DoKnockBack(TargetObject, -1f, 1f);
			}
			else
			{
				DisableForces = true;
				Force = new Vector2(2500f * Mathf.Cos(state.LookAngle * ((float)Math.PI / 180f)), 2500f * Mathf.Sin(state.LookAngle * ((float)Math.PI / 180f))) * AttackForceModifier;
				rb.AddForce(Force);
			}
			damageColliderEvents.SetActive(true);
			if (!string.IsNullOrEmpty(AttackVO))
			{
				AudioManager.Instance.PlayOneShot(AttackVO, base.transform.position);
			}
			AudioManager.Instance.PlayOneShot("event:/enemy/chaser/chaser_attack", base.transform.position);
			state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
			Spine.AnimationState.SetAnimation(0, AttackAnimation, false);
			Spine.AnimationState.AddAnimation(0, IdleAnimation, true, 0f);
			if (DamageColliderDuration != -1f)
			{
				StartCoroutine(EnableCollider(DamageColliderDuration));
			}
			time = 0f;
			while (true)
			{
				float num2;
				time = (num2 = time + Time.deltaTime * Spine.timeScale);
				if (!(num2 < AttackDuration * 0.7f))
				{
					break;
				}
				yield return null;
			}
			damageColliderEvents.SetActive(false);
		}
		time = 0f;
		while (true)
		{
			float num2;
			time = (num2 = time + Time.deltaTime * Spine.timeScale);
			if (!(num2 < AttackDuration * 0.3f))
			{
				break;
			}
			yield return null;
		}
		DisableForces = false;
		state.CURRENT_STATE = StateMachine.State.Idle;
		IdleWait = 0f;
		AttackDelay = AttackDelayTime;
		TargetObject = null;
		Attacking = false;
	}

	private IEnumerator EnableCollider(float dur)
	{
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < dur))
			{
				break;
			}
			yield return null;
		}
		damageColliderEvents.SetActive(false);
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (!string.IsNullOrEmpty(DeathVO))
		{
			AudioManager.Instance.PlayOneShot(DeathVO, base.transform.position);
		}
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		if (!string.IsNullOrEmpty(GetHitVO))
		{
			AudioManager.Instance.PlayOneShot(GetHitVO, base.transform.position);
		}
		if (!DisableKnockback)
		{
			damageColliderEvents.SetActive(false);
		}
		if (CanBeInterrupted)
		{
			StopAllCoroutines();
			DisableForces = false;
			StartCoroutine(HurtRoutine());
		}
		if (AttackType != Health.AttackTypes.NoKnockBack && !DisableKnockback && CanBeInterrupted)
		{
			StartCoroutine(ApplyForceRoutine(Attacker));
		}
		SimpleSpineFlash[] simpleSpineFlashes = SimpleSpineFlashes;
		for (int i = 0; i < simpleSpineFlashes.Length; i++)
		{
			simpleSpineFlashes[i].FlashFillRed();
		}
	}

	private IEnumerator ApplyForceRoutine(GameObject Attacker)
	{
		DisableForces = true;
		Angle = Utils.GetAngle(Attacker.transform.position, base.transform.position) * ((float)Math.PI / 180f);
		Force = new Vector2(1500f * Mathf.Cos(Angle), 1500f * Mathf.Sin(Angle)) * KnockbackModifier;
		rb.AddForce(Force);
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

	protected IEnumerator ApplyForceRoutine(Vector3 forcePosition)
	{
		DisableForces = true;
		Angle = Utils.GetAngle(forcePosition, base.transform.position) * ((float)Math.PI / 180f);
		Force = new Vector2(1500f * Mathf.Cos(Angle), 1500f * Mathf.Sin(Angle)) * KnockbackModifier;
		rb.AddForce(Force);
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
		damageColliderEvents.SetActive(false);
		Attacking = false;
		ClearPaths();
		state.CURRENT_STATE = StateMachine.State.KnockBack;
		state.CURRENT_STATE = StateMachine.State.Idle;
		Spine.AnimationState.SetAnimation(0, IdleAnimation, true);
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
		IdleWait = 0f;
		StartCoroutine(ActiveRoutine());
		if (CounterAttack)
		{
			StartCoroutine(SlamAttack ? SlamRoutine() : AttackRoutine());
		}
	}

	public void GetNewTargetPosition()
	{
		float num = 100f;
		if (GetClosestTarget() != null && ChanceToPathTowardsPlayer > 0f && UnityEngine.Random.value < ChanceToPathTowardsPlayer && Vector3.Distance(base.transform.position, GetClosestTarget().transform.position) < (float)DistanceToPathTowardsPlayer && CheckLineOfSight(GetClosestTarget().transform.position, DistanceToPathTowardsPlayer))
		{
			PathingToPlayer = true;
			RandomDirection = Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position) * ((float)Math.PI / 180f);
		}
		while ((num -= 1f) > 0f)
		{
			float num2 = UnityEngine.Random.Range(DistanceRange.x, DistanceRange.y);
			if (!PathingToPlayer)
			{
				RandomDirection += UnityEngine.Random.Range(0f - TurningArc, TurningArc) * ((float)Math.PI / 180f);
			}
			PathingToPlayer = false;
			float radius = 0.2f;
			Vector3 vector = base.transform.position + new Vector3(num2 * Mathf.Cos(RandomDirection), num2 * Mathf.Sin(RandomDirection));
			RaycastHit2D raycastHit2D = Physics2D.CircleCast(base.transform.position, radius, Vector3.Normalize(vector - base.transform.position), num2, layerToCheck);
			if (raycastHit2D.collider != null)
			{
				if (ShowDebug)
				{
					Points.Add(new Vector3(raycastHit2D.centroid.x, raycastHit2D.centroid.y) + Vector3.Normalize(base.transform.position - vector) * CircleCastOffset);
					PointsLink.Add(new Vector3(base.transform.position.x, base.transform.position.y));
				}
				RandomDirection = 180f - RandomDirection;
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
		if (GameManager.RoomActive)
		{
			Health closestTarget = GetClosestTarget();
			if (closestTarget != null)
			{
				TargetObject = closestTarget.gameObject;
				EnemyHealth = closestTarget;
			}
		}
	}

	public void DoBusiness()
	{
		StartCoroutine(BusinessRoutine());
	}

	private IEnumerator BusinessRoutine()
	{
		yield return new WaitForEndOfFrame();
		AudioManager.Instance.PlayOneShot("event:/Stings/Choir_mid", base.gameObject);
		GameManager.GetInstance().OnConversationNew();
		PlayerFarming.Instance._state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		DecorationCustomTarget.Create(base.transform.position, PlayerFarming.Instance.gameObject.transform.position, 1f, StructureBrain.TYPES.DECORATION_MONSTERSHRINE, FinishedGettingDecoration);
	}

	private void FinishedGettingDecoration()
	{
		GameManager.GetInstance().OnConversationEnd();
		PlayerFarming.Instance._state.CURRENT_STATE = StateMachine.State.Idle;
	}

	protected virtual void OnDamageTriggerEnter(Collider2D collider)
	{
		EnemyHealth = collider.GetComponent<Health>();
		if (EnemyHealth != null && (EnemyHealth.team != health.team || health.team == Health.Team.PlayerTeam))
		{
			EnemyHealth.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, EnemyHealth.transform.position, 0.7f));
		}
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, VisionRange, Color.red);
		if (DetectPlayerWhileHidden && (StartHidden == StartingStates.Hidden || StartHidden == StartingStates.Animation))
		{
			Utils.DrawCircleXY((HiddenOffsetIsGlobalPosition ? Vector3.zero : base.transform.position) + HiddenOffset, HiddenRadius, Color.yellow);
		}
		if (ChanceToPathTowardsPlayer > 0f)
		{
			Utils.DrawCircleXY(base.transform.position, DistanceToPathTowardsPlayer, Color.cyan);
		}
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
