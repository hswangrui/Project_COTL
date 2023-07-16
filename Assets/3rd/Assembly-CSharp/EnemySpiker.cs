using System;
using System.Collections;
using Spine.Unity;
using UnityEngine;

public class EnemySpiker : UnitObject
{
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

	[Range(0f, 1f)]
	public float ChanceToPathTowardsPlayer;

	public int DistanceToPathTowardsPlayer = 6;

	public float SlamAttackRange;

	public float TimeBetweenSlams;

	public GameObject SlamRockPrefab;

	private float SlamTimer;

	public SkeletonAnimation warningIcon;

	protected GameObject TargetObject;

	private float RandomDirection;

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

	private int DetectEnemyRange = 8;

	public override void OnEnable()
	{
		base.OnEnable();
		SlamTimer = TimeBetweenSlams;
		RandomDirection = (float)UnityEngine.Random.Range(0, 360) * ((float)Math.PI / 180f);
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEvents.SetActive(false);
		}
		SimpleSpineFlashes = GetComponentsInChildren<SimpleSpineFlash>();
		state.CURRENT_STATE = StateMachine.State.Idle;
		StartCoroutine(ActiveRoutine());
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if (damageColliderEvents != null)
		{
			damageColliderEvents.SetActive(false);
			damageColliderEvents.OnTriggerEnterEvent -= OnDamageTriggerEnter;
		}
		ClearPaths();
		StopAllCoroutines();
		SimpleSpineFlash[] simpleSpineFlashes = SimpleSpineFlashes;
		for (int i = 0; i < simpleSpineFlashes.Length; i++)
		{
			simpleSpineFlashes[i].FlashWhite(false);
		}
	}

	protected IEnumerator ActiveRoutine()
	{
		yield return new WaitForEndOfFrame();
		while (true)
		{
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
			}
			else
			{
				if (ShouldAttack())
				{
					StartCoroutine(SlamRoutine());
				}
				if (ShouldSlam())
				{
					StartCoroutine(AttackRoutine());
				}
			}
			yield return null;
		}
	}

	protected virtual bool ShouldAttack()
	{
		if ((SlamTimer -= Time.deltaTime) < 0f && !Attacking)
		{
			return Vector3.Distance(base.transform.position, TargetObject.transform.position) < SlamAttackRange;
		}
		return false;
	}

	protected virtual bool ShouldSlam()
	{
		if ((AttackDelay -= Time.deltaTime) < 0f && !Attacking)
		{
			return Vector3.Distance(base.transform.position, TargetObject.transform.position) < (float)VisionRange;
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
			yield return new WaitForSeconds(0.2f);
			SlamDistance += 1f;
			Rocks += 2f;
		}
		yield return new WaitForSeconds(1f);
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
				if (!(num2 < Duration))
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
			DisableForces = true;
			Force = new Vector2(2500f * Mathf.Cos(state.LookAngle * ((float)Math.PI / 180f)), 2500f * Mathf.Sin(state.LookAngle * ((float)Math.PI / 180f))) * AttackForceModifier;
			rb.AddForce(Force);
			damageColliderEvents.SetActive(true);
			AudioManager.Instance.PlayOneShot("event:/enemy/chaser/chaser_attack", base.transform.position);
			state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
			Spine.AnimationState.SetAnimation(0, AttackAnimation, false);
			Spine.AnimationState.AddAnimation(0, IdleAnimation, true, 0f);
			yield return new WaitForSeconds(AttackDuration);
			damageColliderEvents.SetActive(false);
			yield return new WaitForSeconds(0.5f);
		}
		yield return new WaitForSeconds(AttackDuration * 0.3f);
		DisableForces = false;
		state.CURRENT_STATE = StateMachine.State.Idle;
		IdleWait = 0f;
		AttackDelay = AttackDelayTime;
		TargetObject = null;
		Attacking = false;
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		if (!DisableKnockback)
		{
			damageColliderEvents.SetActive(false);
		}
		if (!Attacking && CanBeInterrupted)
		{
			StopAllCoroutines();
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
		yield return new WaitForSeconds(0.5f);
		DisableForces = false;
	}

	protected IEnumerator ApplyForceRoutine(Vector3 forcePosition)
	{
		DisableForces = true;
		Angle = Utils.GetAngle(forcePosition, base.transform.position) * ((float)Math.PI / 180f);
		Force = new Vector2(1500f * Mathf.Cos(Angle), 1500f * Mathf.Sin(Angle)) * KnockbackModifier;
		rb.AddForce(Force);
		yield return new WaitForSeconds(0.5f);
		DisableForces = false;
	}

	private IEnumerator HurtRoutine()
	{
		damageColliderEvents.SetActive(false);
		Attacking = false;
		ClearPaths();
		state.CURRENT_STATE = StateMachine.State.KnockBack;
		yield return new WaitForSeconds(0.5f);
		DisableForces = false;
		IdleWait = 0f;
		state.CURRENT_STATE = StateMachine.State.Idle;
		Spine.AnimationState.SetAnimation(0, IdleAnimation, true);
		StartCoroutine(ActiveRoutine());
		if (CounterAttack)
		{
			StartCoroutine(SlamAttack ? SlamRoutine() : AttackRoutine());
		}
	}

	public void GetNewTargetPosition()
	{
		float num = 100f;
		if (PlayerFarming.Instance != null && ChanceToPathTowardsPlayer > 0f && UnityEngine.Random.value < ChanceToPathTowardsPlayer && Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) < (float)DistanceToPathTowardsPlayer && CheckLineOfSight(PlayerFarming.Instance.transform.position, DistanceToPathTowardsPlayer))
		{
			PathingToPlayer = true;
			RandomDirection = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position) * ((float)Math.PI / 180f);
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

	public void GetNewTarget()
	{
		if (!GameManager.RoomActive)
		{
			return;
		}
		Health health = null;
		float num = float.MaxValue;
		foreach (Health allUnit in Health.allUnits)
		{
			if (allUnit.team != base.health.team && !allUnit.InanimateObject && allUnit.team != 0 && (base.health.team != Health.Team.PlayerTeam || (base.health.team == Health.Team.PlayerTeam && allUnit.team != Health.Team.DangerousAnimals)) && Vector2.Distance(base.transform.position, allUnit.gameObject.transform.position) < (float)VisionRange && CheckLineOfSight(allUnit.gameObject.transform.position, Vector2.Distance(allUnit.gameObject.transform.position, base.transform.position)))
			{
				float num2 = Vector3.Distance(base.transform.position, allUnit.gameObject.transform.position);
				if (num2 < num)
				{
					health = allUnit;
					num = num2;
				}
			}
		}
		if (health != null)
		{
			TargetObject = health.gameObject;
			EnemyHealth = health;
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
}
