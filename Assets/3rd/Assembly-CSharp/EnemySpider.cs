using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class EnemySpider : UnitObject
{
	public static List<EnemySpider> EnemySpiders = new List<EnemySpider>();

	public ColliderEvents damageColliderEvents;

	public SkeletonAnimation Spine;

	public SimpleSpineFlash Body;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string IdleAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string MovingAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string SignPostAttackAnimation;

	public bool LoopSignPostAttackAnimation = true;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string AttackAnimation;

	public SpriteRenderer ShadowSpriteRenderer;

	public SimpleSpineFlash SimpleSpineFlash;

	public float KnockbackModifier = 1f;

	public int NumberOfAttacks = 1;

	public float AttackForceModifier = 1f;

	public bool CounterAttack;

	public bool CanBeInterrupted = true;

	public bool hasMovementDelay = true;

	[SerializeField]
	protected bool wander;

	[SerializeField]
	private bool flee;

	[Range(0f, 1f)]
	public float ChanceToPathTowardsPlayer;

	public int DistanceToPathTowardsPlayer = 6;

	public SkeletonAnimation warningIcon;

	private float RandomDirection;

	protected float initialAttackDelayTimer;

	protected float initialMovementDelay;

	[SerializeField]
	protected string attackSfx;

	[SerializeField]
	protected string breakFreeSfx;

	[SerializeField]
	protected string deathSfx;

	[SerializeField]
	protected string getHitSfx;

	[SerializeField]
	protected string jumpSfx;

	[SerializeField]
	protected string stuckSfx;

	[SerializeField]
	protected string warningSfx;

	protected bool updateDirection = true;

	public float AttackDelayTime;

	protected bool IsStunned;

	[HideInInspector]
	public float AttackDelay;

	public float AttackDuration = 1f;

	public float SignPostAttackDuration = 0.5f;

	public bool DisableKnockback;

	public float KnockBackModifier = 1f;

	private float Angle;

	private Vector3 Force;

	public float TurningArc = 90f;

	public Vector2 DistanceRange = new Vector2(1f, 3f);

	public Vector2 IdleWaitRange = new Vector2(1f, 3f);

	protected float IdleWait;

	private bool PathingToPlayer;

	protected Health EnemyHealth;

	private int DetectEnemyRange = 8;

	public Vector3 AttackingTargetPosition { get; protected set; }

	public bool Attacking { get; protected set; }

	public override void OnEnable()
	{
		base.OnEnable();
		Spine.ForceVisible = true;
		EnemySpiders.Add(this);
		if (hasMovementDelay)
		{
			initialAttackDelayTimer = (GameManager.GetInstance() ? (GameManager.GetInstance().CurrentTime + 1f) : (Time.time + 1f));
			initialMovementDelay = (GameManager.GetInstance() ? GameManager.GetInstance().CurrentTime : Time.time) + UnityEngine.Random.Range(1f, 1.5f);
		}
		RandomDirection = (float)UnityEngine.Random.Range(0, 360) * ((float)Math.PI / 180f);
		Attacking = false;
		updateDirection = true;
		health.enabled = true;
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEvents.SetActive(false);
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		StartCoroutine(ActiveRoutine());
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		AudioManager.Instance.PlayOneShot(deathSfx, base.transform.position);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		EnemySpiders.Remove(this);
		if (damageColliderEvents != null)
		{
			damageColliderEvents.SetActive(false);
			damageColliderEvents.OnTriggerEnterEvent -= OnDamageTriggerEnter;
		}
		ClearPaths();
		StopAllCoroutines();
		if ((bool)SimpleSpineFlash)
		{
			SimpleSpineFlash.FlashWhite(false);
		}
	}

	public override void Update()
	{
		base.Update();
		if (TargetEnemy == null)
		{
			TargetEnemy = GetClosestTarget();
		}
		ShadowSpriteRenderer.transform.position = Spine.transform.position;
	}

	private void LateUpdate()
	{
		bool flag2;
		int num;
		if (TargetEnemy != null)
		{
			flag2 = TargetEnemy.transform.position.y > base.transform.position.y;
		}
		else
			num = 0;
		bool flag = state.facingAngle > 90f && state.facingAngle < 270f;
		if (updateDirection && (bool)Body)
		{
			Body.transform.localScale = new Vector3(flag ? Body.transform.localScale.y : (Body.transform.localScale.y * -1f), Body.transform.localScale.y, Body.transform.localScale.z);
		}
		if (!Attacking && state.CURRENT_STATE == StateMachine.State.Idle && (bool)TargetEnemy)
		{
			LookAtTarget();
		}
	}

	protected virtual IEnumerator ActiveRoutine()
	{
		yield return new WaitForEndOfFrame();
		while (true)
		{
			if (!GameManager.RoomActive)
			{
				yield return null;
				continue;
			}
			if (state.CURRENT_STATE == StateMachine.State.Idle && (IdleWait -= Time.deltaTime) <= 0f && GameManager.GetInstance().CurrentTime > initialMovementDelay)
			{
				if (wander)
				{
					GetNewTargetPosition();
				}
				else
				{
					Flee();
				}
				speed = maxSpeed * SpeedMultiplier;
			}
			if (TargetEnemy != null && !Attacking && !IsStunned)
			{
				state.LookAngle = Utils.GetAngle(base.transform.position, TargetEnemy.transform.position);
			}
			else
			{
				state.LookAngle = state.facingAngle;
			}
			if (MovingAnimation != "" && GameManager.GetInstance().CurrentTime > initialMovementDelay)
			{
				if (state.CURRENT_STATE == StateMachine.State.Moving && Spine.AnimationName != MovingAnimation)
				{
					SetAnimation(MovingAnimation, true);
				}
				if (state.CURRENT_STATE == StateMachine.State.Idle && Spine.AnimationName != IdleAnimation)
				{
					SetAnimation(IdleAnimation, true);
				}
			}
			if (ShouldAttack())
			{
				StartCoroutine(AttackRoutine());
			}
			yield return null;
		}
	}

	protected virtual bool ShouldAttack()
	{
		if ((AttackDelay -= Time.deltaTime) < 0f && !Attacking && (bool)TargetEnemy && Vector3.Distance(base.transform.position, TargetEnemy.transform.position) < (float)VisionRange)
		{
			return GameManager.GetInstance().CurrentTime > initialAttackDelayTimer;
		}
		return false;
	}

	protected virtual IEnumerator AttackRoutine()
	{
		Attacking = true;
		ClearPaths();
		if (TargetEnemy == null)
		{
			yield break;
		}
		int CurrentAttack = 0;
		float time2;
		while (true)
		{
			int num = CurrentAttack + 1;
			CurrentAttack = num;
			if (num > NumberOfAttacks)
			{
				break;
			}
			SetAnimation(SignPostAttackAnimation, LoopSignPostAttackAnimation);
			state.CURRENT_STATE = StateMachine.State.SignPostAttack;
			AudioManager.Instance.PlayOneShot(warningSfx, base.transform.position);
			TargetEnemy = GetClosestTarget();
			LookAtTarget();
			float Progress = 0f;
			float Duration = SignPostAttackDuration;
			while (true)
			{
				float num2;
				Progress = (num2 = Progress + Time.deltaTime);
				if (!(num2 < Duration / Spine.timeScale))
				{
					break;
				}
				SimpleSpineFlash.FlashWhite(Progress / Duration);
				yield return null;
			}
			SimpleSpineFlash.FlashWhite(false);
			DisableForces = true;
			Force = new Vector2(2500f * Mathf.Cos(state.LookAngle * ((float)Math.PI / 180f)), 2500f * Mathf.Sin(state.LookAngle * ((float)Math.PI / 180f))) * AttackForceModifier;
			rb.AddForce(Force);
			damageColliderEvents.SetActive(true);
			AudioManager.Instance.PlayOneShot(attackSfx, base.transform.position);
			state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
			SetAnimation(AttackAnimation);
			AddAnimation(IdleAnimation, true);
			time2 = 0f;
			while (true)
			{
				float num2;
				time2 = (num2 = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num2 < AttackDuration))
				{
					break;
				}
				yield return null;
			}
			damageColliderEvents.SetActive(false);
			time2 = 0f;
			while (true)
			{
				float num2;
				time2 = (num2 = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num2 < 0.5f))
				{
					break;
				}
				yield return null;
			}
		}
		time2 = 0f;
		while (true)
		{
			float num2;
			time2 = (num2 = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num2 < AttackDuration * 0.5f))
			{
				break;
			}
			yield return null;
		}
		DisableForces = false;
		state.CURRENT_STATE = StateMachine.State.Idle;
		IdleWait = 0f;
		AttackDelay = AttackDelayTime;
		Attacking = false;
		TargetEnemy = null;
		GetNewTargetPosition();
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		if (!DisableKnockback)
		{
			damageColliderEvents.SetActive(false);
		}
		if (Attacking && CanBeInterrupted)
		{
			StopAllCoroutines();
			StartCoroutine(HurtRoutine());
		}
		if (AttackType != Health.AttackTypes.NoKnockBack && !DisableKnockback && CanBeInterrupted)
		{
			StartCoroutine(ApplyForceRoutine(Attacker));
		}
		SimpleSpineFlash.FlashFillRed();
		AudioManager.Instance.PlayOneShot(getHitSfx, base.transform.position);
	}

	private IEnumerator ApplyForceRoutine(GameObject Attacker)
	{
		DisableForces = true;
		Angle = Utils.GetAngle(Attacker.transform.position, base.transform.position) * ((float)Math.PI / 180f);
		Force = new Vector2(1500f * Mathf.Cos(Angle), 1500f * Mathf.Sin(Angle)) * KnockbackModifier;
		rb.AddForce(Force * KnockBackModifier);
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
		SetAnimation(IdleAnimation, true);
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
		state.CURRENT_STATE = StateMachine.State.Idle;
		StartCoroutine(ActiveRoutine());
		if (CounterAttack)
		{
			StartCoroutine(AttackRoutine());
		}
	}

	public void GetNewTargetPosition()
	{
		float num = 100f;
		if (TargetEnemy != null && ChanceToPathTowardsPlayer > 0f && UnityEngine.Random.value < ChanceToPathTowardsPlayer && Vector3.Distance(base.transform.position, TargetEnemy.transform.position) < (float)DistanceToPathTowardsPlayer && CheckLineOfSight(TargetEnemy.transform.position, DistanceToPathTowardsPlayer))
		{
			PathingToPlayer = true;
			RandomDirection = Utils.GetAngle(base.transform.position, TargetEnemy.transform.position) * ((float)Math.PI / 180f);
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
			if (Physics2D.CircleCast(base.transform.position, radius, Vector3.Normalize(vector - base.transform.position), num2, layerToCheck).collider != null)
			{
				RandomDirection = 180f - RandomDirection;
				continue;
			}
			IdleWait = UnityEngine.Random.Range(IdleWaitRange.x, IdleWaitRange.y);
			givePath(vector);
			break;
		}
	}

	protected void Flee()
	{
		if ((bool)TargetEnemy)
		{
			float num = 100f;
			while ((num -= 1f) > 0f)
			{
				float f = (float)UnityEngine.Random.Range(0, 360) * ((float)Math.PI / 180f);
				float num2 = UnityEngine.Random.Range(7, 10);
				Vector3 vector = TargetEnemy.transform.position + new Vector3(num2 * Mathf.Cos(f), num2 * Mathf.Sin(f));
				Vector3 vector2 = Vector3.Normalize(vector - TargetEnemy.transform.position);
				if (Physics2D.CircleCast(TargetEnemy.transform.position, 0.5f, vector2, num2, layerToCheck).collider == null)
				{
					IdleWait = UnityEngine.Random.Range(IdleWaitRange.x, IdleWaitRange.y);
					givePath(vector);
				}
			}
			GetNewTargetPosition();
		}
		else
		{
			IdleWait = UnityEngine.Random.Range(IdleWaitRange.x, IdleWaitRange.y);
			givePath(base.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 2f);
		}
	}

	protected void SetAnimation(string animationName, bool loop = false)
	{
		Spine.AnimationState.SetAnimation(0, animationName, loop);
	}

	protected void AddAnimation(string animationName, bool loop = false)
	{
		Spine.AnimationState.AddAnimation(0, animationName, loop, 0f);
	}

	public void GetNewTarget()
	{
		if (GameManager.RoomActive)
		{
			Health closestTarget = GetClosestTarget();
			if (closestTarget != null)
			{
				EnemyHealth = closestTarget;
			}
		}
	}

	protected virtual void OnDamageTriggerEnter(Collider2D collider)
	{
		EnemyHealth = collider.GetComponent<Health>();
		if (EnemyHealth != null && (EnemyHealth.team != health.team || health.team == Health.Team.PlayerTeam))
		{
			EnemyHealth.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, EnemyHealth.transform.position, 0.7f));
		}
	}

	protected void LookAtTarget()
	{
		if ((bool)TargetEnemy)
		{
			float angle = Utils.GetAngle(base.transform.position, TargetEnemy.transform.position);
			LookAtAngle(angle);
		}
	}

	protected void LookAtAngle(float angle)
	{
		state.facingAngle = angle;
		state.LookAngle = angle;
	}
}
