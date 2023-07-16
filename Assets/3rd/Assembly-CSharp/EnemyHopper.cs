using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMODUnity;
using Spine.Unity;
using UnityEngine;

public class EnemyHopper : UnitObject
{
	public ColliderEvents damageColliderEvents;

	protected IEnumerator damageColliderRoutine;

	public List<SkeletonAnimation> Spine = new List<SkeletonAnimation>();

	public SimpleSpineFlash SimpleSpineFlash;

	[EventRef]
	public string attackSoundPath = string.Empty;

	[EventRef]
	public string onHitSoundPath = string.Empty;

	[EventRef]
	public string OnLandSoundPath = string.Empty;

	[EventRef]
	public string OnJumpSoundPath = string.Empty;

	[EventRef]
	public string OnLayEggSoundPath = string.Empty;

	[EventRef]
	public string OnCroakSoundPath = string.Empty;

	[EventRef]
	public string AttackVO = string.Empty;

	[EventRef]
	public string DeathVO = string.Empty;

	[EventRef]
	public string GetHitVO = string.Empty;

	[EventRef]
	public string WarningVO = string.Empty;

	protected bool _playedVO;

	protected GameObject targetObject;

	protected float TargetAngle;

	protected List<Collider2D> collider2DList = new List<Collider2D>();

	protected Health EnemyHealth;

	protected bool canBeParried = true;

	public float signPostParryWindow = 0.2f;

	protected float attackParryWindow = 0.3f;

	protected GameManager gm;

	protected CircleCollider2D physicsCollider;

	protected float dancingTimestamp;

	public float dancingDuration;

	protected float idleTimestamp;

	public float idleDur = 0.6f;

	public float signPostDur = 0.2f;

	public bool canBeStunned = true;

	public bool stunnedStopsAttack = true;

	protected float stunnedTimestamp;

	public float stunnedDur = 0.5f;

	protected float chargingTimestamp;

	public float chargingDuration = 0.5f;

	protected float attackingTimestamp;

	public float attackRange;

	protected float hoppingTimestamp;

	public float hoppingDur = 0.4f;

	public float attackingDur = 0.2f;

	protected bool isFleeing;

	public float hopMoveSpeed = 1f;

	public AnimationCurve hopSpeedCurve;

	public AnimationCurve hopZCurve;

	public float hopZHeight;

	protected static float zFallSpeed = 15f;

	private bool hasCollidedWithObstacle;

	public bool canLayEggs;

	public GameObject eggPrefab;

	protected float lastLaidEggTimestamp;

	protected float minTimeBetweenEggs = 6f;

	public bool alwaysTargetPlayer;

	public static List<EnemyHopper> EnemyHoppers = new List<EnemyHopper>();

	protected static int maxHoppersPerRoom = 3;

	protected static int maxEggsPerRoom = 1;

	protected string chargingAnimationString = "lay-egg";

	public bool canSprayPoison;

	public GameObject poisonPrefab;

	public List<FollowAsTail> TailPieces = new List<FollowAsTail>();

	public float KnockBackMultipier = 0.7f;

	public override void Awake()
	{
		base.Awake();
		physicsCollider = GetComponent<CircleCollider2D>();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		health.OnHitEarly += OnHitEarly;
		StateMachine stateMachine = state;
		stateMachine.OnStateChange = (StateMachine.StateChange)Delegate.Combine(stateMachine.OnStateChange, new StateMachine.StateChange(OnStateChange));
		gm = GameManager.GetInstance();
		state.CURRENT_STATE = StateMachine.State.Idle;
		if (gm != null)
		{
			idleTimestamp = gm.CurrentTime;
		}
		if (EnemyHoppers != null)
		{
			idleTimestamp += (float)EnemyHoppers.Count * 0.1f;
		}
		else
		{
			idleTimestamp += UnityEngine.Random.Range(0f, idleDur);
		}
		EnemyHoppers.Add(this);
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEvents.SetActive(false);
		}
	}

	public override void OnDisable()
	{
		SimpleSpineFlash.FlashWhite(false);
		base.OnDisable();
		health.OnHitEarly -= OnHitEarly;
		StateMachine stateMachine = state;
		stateMachine.OnStateChange = (StateMachine.StateChange)Delegate.Remove(stateMachine.OnStateChange, new StateMachine.StateChange(OnStateChange));
		ClearPaths();
		EnemyHoppers.Remove(this);
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent -= OnDamageTriggerEnter;
		}
	}

	public override void Update()
	{
		base.Update();
		Seperate(0.5f, true);
		if (gm == null)
		{
			gm = GameManager.GetInstance();
			if (gm == null)
			{
				return;
			}
		}
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Vulnerable:
			UpdateStateVulnerable();
			break;
		case StateMachine.State.Dancing:
			UpdateStateDancing();
			break;
		case StateMachine.State.Idle:
			UpdateStateIdle();
			break;
		case StateMachine.State.Moving:
			UpdateStateMoving();
			break;
		case StateMachine.State.Charging:
			UpdateStateCharging();
			break;
		}
		int num = -1;
		while (++num < TailPieces.Count)
		{
			if (TailPieces[num] != null)
			{
				TailPieces[num].UpdatePosition();
			}
		}
		if (state.CURRENT_STATE != StateMachine.State.Moving)
		{
			Spine[0].transform.localPosition = Vector3.Lerp(Spine[0].transform.localPosition, Vector3.zero, Time.deltaTime * zFallSpeed);
		}
		move();
	}

	protected virtual void UpdateStateIdle()
	{
		speed = 0f;
		if (gm.TimeSince(idleTimestamp) >= (idleDur - signPostParryWindow) * Spine[0].timeScale)
		{
			canBeParried = true;
		}
		float num = idleDur - signPostDur;
		if (gm.TimeSince(idleTimestamp) >= num * Spine[0].timeScale)
		{
			SimpleSpineFlash.FlashWhite(0.75f * (gm.TimeSince(idleTimestamp) - num / signPostDur) * Spine[0].timeScale);
		}
		if (!(gm.TimeSince(idleTimestamp) >= idleDur * Spine[0].timeScale))
		{
			return;
		}
		if (targetObject == null && GetClosestTarget() != null)
		{
			targetObject = GetClosestTarget().gameObject;
		}
		bool flag = TargetIsVisible();
		if (canLayEggs && gm.TimeSince(lastLaidEggTimestamp - 0.5f) >= minTimeBetweenEggs * Spine[0].timeScale && EnemyHoppers.Count < maxHoppersPerRoom && EnemyEgg.EnemyEggs.Count < maxEggsPerRoom)
		{
			alwaysTargetPlayer = false;
			isFleeing = true;
		}
		if (alwaysTargetPlayer)
		{
			if (GetClosestTarget() != null)
			{
				TargetAngle = GetAngleToTarget();
			}
			else
			{
				TargetAngle = GetFleeAngle();
			}
		}
		else if (isFleeing)
		{
			TargetAngle = GetFleeAngle();
		}
		else if (flag)
		{
			TargetAngle = GetAngleToTarget();
		}
		else
		{
			TargetAngle = GetRandomFacingAngle();
		}
		state.LookAngle = TargetAngle;
		state.facingAngle = TargetAngle;
		foreach (SkeletonAnimation item in Spine)
		{
			if (Spine[0].timeScale != 0.0001f)
			{
				item.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
			}
		}
		if (ShouldSprayPoison())
		{
			SprayPoison();
		}
		state.CURRENT_STATE = StateMachine.State.Moving;
	}

	protected virtual void UpdateStateMoving()
	{
		if (!_playedVO)
		{
			AudioManager.Instance.PlayOneShot(WarningVO, base.gameObject);
			_playedVO = true;
		}
		speed = hopSpeedCurve.Evaluate(gm.TimeSince(hoppingTimestamp) / (hoppingDur * Spine[0].timeScale)) * hopMoveSpeed;
		if (hasCollidedWithObstacle || TargetIsInAttackRange())
		{
			speed *= 0.5f;
		}
		speed *= Spine[0].timeScale;
		Spine[0].transform.localPosition = -Vector3.forward * hopZCurve.Evaluate(gm.TimeSince(hoppingTimestamp) / hoppingDur) * hopZHeight * Spine[0].timeScale;
		canBeParried = gm.TimeSince(hoppingTimestamp) <= attackParryWindow * Spine[0].timeScale;
		if (gm.TimeSince(hoppingTimestamp) >= hoppingDur / Spine[0].timeScale)
		{
			speed = 0f;
			DoAttack();
			_playedVO = false;
			if (ShouldStartCharging())
			{
				state.CURRENT_STATE = StateMachine.State.Charging;
			}
			else
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
		}
	}

	protected virtual void DoAttack()
	{
		AudioManager.Instance.PlayOneShot(AttackVO, base.gameObject);
		if (damageColliderRoutine != null)
		{
			StopCoroutine(damageColliderRoutine);
		}
		damageColliderRoutine = TurnOnDamageColliderForDuration(attackingDur);
		StartCoroutine(damageColliderRoutine);
	}

	private IEnumerator TurnOnDamageColliderForDuration(float duration)
	{
		damageColliderEvents.SetActive(Spine[0].timeScale != 0.0001f);
		float t = 0f;
		while (t < duration)
		{
			t += Time.deltaTime;
			SimpleSpineFlash.FlashWhite(1f - Mathf.Clamp01(t / duration));
			yield return null;
		}
		SimpleSpineFlash.FlashWhite(false);
		damageColliderEvents.SetActive(false);
	}

	protected virtual void UpdateStateVulnerable()
	{
		speed = 0f;
		if (gm.TimeSince(stunnedTimestamp) >= stunnedDur * Spine[0].timeScale)
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
		}
	}

	protected virtual void UpdateStateCharging()
	{
		if (gm.TimeSince(chargingTimestamp) >= chargingDuration * Spine[0].timeScale && canLayEggs && EnemyHoppers.Count < maxHoppersPerRoom && EnemyEgg.EnemyEggs.Count < maxEggsPerRoom)
		{
			LayEgg();
			idleDur = signPostParryWindow;
			state.CURRENT_STATE = StateMachine.State.Idle;
			alwaysTargetPlayer = true;
			isFleeing = false;
		}
	}

	protected virtual void UpdateStateDancing()
	{
		speed = 0f;
		if (gm.TimeSince(dancingTimestamp) >= dancingDuration * Spine[0].timeScale)
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
		}
	}

	private void GetTailPieces()
	{
		TailPieces = new List<FollowAsTail>(GetComponentsInChildren<FollowAsTail>());
	}

	protected bool TargetIsVisible()
	{
		if (!GameManager.RoomActive)
		{
			return false;
		}
		if (targetObject == null)
		{
			return false;
		}
		float num = Mathf.Sqrt(MagnitudeFindDistanceBetween(targetObject.transform.position, base.transform.position));
		if (num <= (float)VisionRange)
		{
			return CheckLineOfSight(targetObject.transform.position, Mathf.Min(num, VisionRange));
		}
		return false;
	}

	private bool TargetIsInAttackRange()
	{
		if (!GameManager.RoomActive)
		{
			return false;
		}
		if (targetObject == null)
		{
			return false;
		}
		return Mathf.Sqrt(MagnitudeFindDistanceBetween(targetObject.transform.position, base.transform.position)) <= attackRange * 0.75f;
	}

	private Vector3 GetHopPositionTowardsTarget(Vector3 targetPos)
	{
		return (targetPos - base.transform.position).normalized * attackRange + base.transform.position;
	}

	protected float GetAngleToTarget()
	{
		float num = Utils.GetAngle(base.transform.position, targetObject.transform.position);
		if (physicsCollider == null)
		{
			return num;
		}
		float num2 = 32f;
		for (int i = 0; (float)i < num2; i++)
		{
			if (!Physics2D.CircleCast(base.transform.position, physicsCollider.radius, new Vector2(Mathf.Cos(num * ((float)Math.PI / 180f)), Mathf.Sin(num * ((float)Math.PI / 180f))), attackRange * 0.5f, layerToCheck))
			{
				break;
			}
			num += 360f / (num2 + 1f);
		}
		return num;
	}

	protected float GetRandomFacingAngle()
	{
		float num = UnityEngine.Random.Range(0, 360);
		if (physicsCollider == null)
		{
			return num;
		}
		float num2 = 16f;
		for (int i = 0; (float)i < num2; i++)
		{
			if (!Physics2D.CircleCast(base.transform.position, physicsCollider.radius, new Vector2(Mathf.Cos(num * ((float)Math.PI / 180f)), Mathf.Sin(num * ((float)Math.PI / 180f))), attackRange * 0.5f, layerToCheck))
			{
				break;
			}
			num += 360f / (num2 + 1f);
		}
		return num;
	}

	protected float GetFleeAngle()
	{
		if (targetObject == null)
		{
			return GetRandomFacingAngle();
		}
		float num = 100f;
		while ((num -= 1f) > 0f)
		{
			float f = (float)UnityEngine.Random.Range(0, 360) * ((float)Math.PI / 180f);
			float num2 = UnityEngine.Random.Range(4, 7);
			Vector3 vector = targetObject.transform.position + new Vector3(num2 * Mathf.Cos(f), num2 * Mathf.Sin(f));
			Vector3 vector2 = Vector3.Normalize(vector - targetObject.transform.position);
			RaycastHit2D raycastHit2D = Physics2D.CircleCast(targetObject.transform.position, 1.5f, vector2, num2, layerToCheck);
			if (raycastHit2D.collider != null)
			{
				if (MagnitudeFindDistanceBetween(targetObject.transform.position, raycastHit2D.centroid) > 9f)
				{
					return Utils.GetAngle(base.transform.position, vector);
				}
				continue;
			}
			return Utils.GetAngle(base.transform.position, vector);
		}
		return GetRandomFacingAngle();
	}

	protected virtual bool ShouldStartCharging()
	{
		if (!GameManager.RoomActive)
		{
			return false;
		}
		if (canLayEggs && gm.TimeSince(lastLaidEggTimestamp) >= minTimeBetweenEggs * Spine[0].timeScale && EnemyHoppers.Count < maxHoppersPerRoom)
		{
			return EnemyEgg.EnemyEggs.Count < maxEggsPerRoom;
		}
		return false;
	}

	private bool ShouldSprayPoison()
	{
		return canSprayPoison;
	}

	protected virtual void OnStateChange(StateMachine.State newState, StateMachine.State prevState)
	{
		if (gm == null)
		{
			return;
		}
		switch (newState)
		{
		case StateMachine.State.Idle:
			if (newState == prevState)
			{
				break;
			}
			idleTimestamp = gm.CurrentTime;
			foreach (SkeletonAnimation item in Spine)
			{
				if (item.AnimationState != null)
				{
					if (prevState == StateMachine.State.Moving)
					{
						item.AnimationState.SetAnimation(0, "jump-end", false);
					}
					item.AnimationState.AddAnimation(0, "idle", true, 0f);
				}
			}
			if (!string.IsNullOrEmpty(OnLandSoundPath))
			{
				AudioManager.Instance.PlayOneShot(OnLandSoundPath, base.transform.position);
			}
			targetObject = null;
			SimpleSpineFlash.FlashWhite(false);
			break;
		case StateMachine.State.Moving:
			hasCollidedWithObstacle = false;
			hoppingTimestamp = gm.CurrentTime;
			SimpleSpineFlash.FlashWhite(false);
			foreach (SkeletonAnimation item2 in Spine)
			{
				if (item2.AnimationState != null)
				{
					item2.AnimationState.SetAnimation(0, "jump", false);
				}
			}
			if (!string.IsNullOrEmpty(OnJumpSoundPath))
			{
				AudioManager.Instance.PlayOneShot(OnJumpSoundPath, base.transform.position);
			}
			break;
		case StateMachine.State.Vulnerable:
			stunnedTimestamp = gm.CurrentTime;
			SimpleSpineFlash.FlashWhite(false);
			if (stunnedStopsAttack && damageColliderEvents != null)
			{
				damageColliderEvents.SetActive(false);
			}
			break;
		case StateMachine.State.Charging:
			foreach (SkeletonAnimation item3 in Spine)
			{
				if (item3.AnimationState != null)
				{
					item3.AnimationState.SetAnimation(0, chargingAnimationString, false);
					item3.AnimationState.AddAnimation(0, "idle", true, 0f);
				}
			}
			if (!string.IsNullOrEmpty(OnLayEggSoundPath))
			{
				AudioManager.Instance.PlayOneShot(OnLayEggSoundPath, base.transform.position);
			}
			chargingTimestamp = gm.CurrentTime;
			break;
		case StateMachine.State.Dancing:
			dancingTimestamp = gm.CurrentTime;
			foreach (SkeletonAnimation item4 in Spine)
			{
				if (item4.AnimationState != null)
				{
					item4.AnimationState.SetAnimation(0, "roar", false);
					item4.AnimationState.AddAnimation(0, "idle", true, 0f);
				}
			}
			if (damageColliderEvents != null)
			{
				damageColliderEvents.SetActive(false);
			}
			if (!string.IsNullOrEmpty(OnCroakSoundPath))
			{
				AudioManager.Instance.PlayOneShot(OnCroakSoundPath, base.transform.position);
			}
			break;
		}
	}

	private void OnHitEarly(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (canBeParried)
		{
			health.WasJustParried = true;
		}
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		if (!string.IsNullOrEmpty(DeathVO))
		{
			AudioManager.Instance.PlayOneShot(DeathVO, base.transform.position);
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		if (health.HasShield)
		{
			return;
		}
		if (KnockBackMultipier != 0f)
		{
			DoKnockBack(Attacker, KnockBackMultipier, 1f);
		}
		if (!string.IsNullOrEmpty(onHitSoundPath))
		{
			AudioManager.Instance.PlayOneShot(onHitSoundPath, base.transform.position);
		}
		if (!string.IsNullOrEmpty(GetHitVO))
		{
			AudioManager.Instance.PlayOneShot(GetHitVO, base.transform.position);
		}
		SimpleSpineFlash.FlashWhite(false);
		UsePathing = true;
		health.invincible = false;
		targetObject = null;
		if (canBeStunned && (state.CURRENT_STATE == StateMachine.State.Moving || state.CURRENT_STATE == StateMachine.State.Attacking))
		{
			state.CURRENT_STATE = StateMachine.State.Vulnerable;
			foreach (SkeletonAnimation item in Spine)
			{
				if (item.AnimationState != null)
				{
					item.AnimationState.SetAnimation(0, "hit", false);
					item.AnimationState.AddAnimation(0, "idle", false, 0f);
				}
			}
		}
		SimpleSpineFlash.FlashFillRed();
	}

	private void LayEgg()
	{
		if (gm != null)
		{
			lastLaidEggTimestamp = gm.CurrentTime;
		}
		SimpleSpineFlash.FlashWhite(false);
		int num = 1;
		for (int i = 0; i < num; i++)
		{
			GameObject obj = UnityEngine.Object.Instantiate(eggPrefab, base.transform.position + new Vector3(0f, -0.5f, 0f), Quaternion.identity, (base.transform.parent != null) ? base.transform.parent : null);
			obj.GetComponent<UnitObject>().DisableForces = true;
			obj.GetComponent<DeadBodySliding>().Init(base.gameObject, UnityEngine.Random.Range(0f, 360f), 1200f);
			obj.transform.localScale = Vector3.zero;
			obj.transform.DOScale(Vector3.one, 1f);
		}
	}

	private void SprayPoison()
	{
		SimpleSpineFlash.FlashWhite(false);
		int num = 1;
		for (int i = 0; i < num; i++)
		{
			UnityEngine.Object.Instantiate(poisonPrefab, base.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle, Quaternion.identity, null);
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

	private void OnTriggerEnter2D(Collider2D collider)
	{
		if (state.CURRENT_STATE == StateMachine.State.Moving && collider.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
		{
			Debug.Log("I have hopped into an obstacle", base.gameObject);
			hasCollidedWithObstacle = true;
			TargetAngle += ((TargetAngle > 180f) ? (-180f) : 180f);
			state.LookAngle = TargetAngle;
			state.facingAngle = TargetAngle;
		}
	}

	private float MagnitudeFindDistanceBetween(Vector3 a, Vector3 b)
	{
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		float num3 = a.z - b.z;
		return num * num + num2 * num2 + num3 * num3;
	}
}
