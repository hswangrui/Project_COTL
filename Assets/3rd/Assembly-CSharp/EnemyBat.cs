using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using Spine;
using Spine.Unity;
using UnityEngine;

public class EnemyBat : UnitObject
{
	[SerializeField]
	private SpriteRenderer Aiming;

	public static List<EnemyBat> enemyBats = new List<EnemyBat>();

	public DeadBodyFlying DeadBody;

	public ColliderEvents damageColliderEvents;

	public SimpleSpineFlash SimpleSpineFlash;

	public SkeletonAnimation Spine;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string IdleAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string SignPostAttackAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string AttackAnimation;

	public SkeletonAnimation warningIcon;

	[EventRef]
	public string AttackVO = string.Empty;

	[EventRef]
	public string DeathVO = string.Empty;

	[EventRef]
	public string GetHitVO = string.Empty;

	[EventRef]
	public string WarningVO = string.Empty;

	protected Vector3? StartingPosition;

	protected Vector3 TargetPosition;

	public bool canBeStunned = true;

	protected int RanDirection = 1;

	public float MaximumRange = 5f;

	public float IdleSpeed = 0.03f;

	public float ChaseSpeed = 0.1f;

	public float turningSpeed = 1f;

	public float angleNoiseAmplitude;

	public float angleNoiseFrequency;

	public float timestamp;

	[SerializeField]
	protected bool useAcceleration;

	[SerializeField]
	private float acceleration = 2f;

	public float noticePlayerDistance = 5f;

	public float attackPlayerDistance = 3f;

	public bool ChasingPlayer;

	public Vector2 AttackCoolDownDuration = new Vector2(1f, 2f);

	protected float AttackCoolDown;

	private float DistanceCheck;

	protected bool NoticedPlayer;

	public bool avoidTarget;

	[SerializeField]
	private float postAttackDelay = 0.2f;

	protected bool Attacking;

	public float AttackForce = 2500f;

	protected int CurrentAttackNum;

	public int NumAttacks = 1;

	public Action<int> OnAttack;

	public System.Action OnAttackComplete;

	protected float Angle;

	protected Vector3 Force;

	public float KnockbackForceModifier = 1f;

	public float KnockbackDuration = 0.5f;

	public bool AttackAfterKnockback;

	private Health EnemyHealth;

	private void Start()
	{
		if (Aiming != null)
		{
			Aiming.gameObject.SetActive(false);
		}
	}

	public override void OnEnable()
	{
		Spine.AnimationState.Event += HandleEvent;
		base.OnEnable();
		enemyBats.Add(this);
		if (!StartingPosition.HasValue)
		{
			StartingPosition = base.transform.position;
			TargetPosition = StartingPosition.Value;
			maxSpeed = IdleSpeed;
		}
		if (GameManager.GetInstance() != null)
		{
			timestamp = GameManager.GetInstance().CurrentTime;
		}
		else
		{
			timestamp = Time.time;
		}
		turningSpeed += UnityEngine.Random.Range(-0.1f, 0.1f);
		angleNoiseFrequency += UnityEngine.Random.Range(-0.1f, 0.1f);
		angleNoiseAmplitude += UnityEngine.Random.Range(-0.1f, 0.1f);
		state.CURRENT_STATE = StateMachine.State.Moving;
		AttackCoolDown = UnityEngine.Random.Range(AttackCoolDownDuration.x, AttackCoolDownDuration.y);
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEvents.SetActive(false);
		}
		RanDirection = ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
		StartCoroutine(ActiveRoutine());
	}

	private void HandleEvent(TrackEntry trackEntry, global::Spine.Event e)
	{
		if (e.Data.Name == "wingFlap")
		{
			AudioManager.Instance.PlayOneShot("event:/enemy/small_wings_flap", base.gameObject);
		}
	}

	public override void Update()
	{
		if (useAcceleration)
		{
			if (UsePathing)
			{
				if (pathToFollow == null)
				{
					speed += (0f - speed) / (4f * acceleration) * GameManager.DeltaTime * Spine.timeScale;
					move();
					return;
				}
				if (currentWaypoint >= pathToFollow.Count)
				{
					speed += (0f - speed) / (4f * acceleration) * GameManager.DeltaTime * Spine.timeScale;
					move();
					return;
				}
			}
			if (state.CURRENT_STATE == StateMachine.State.Moving || state.CURRENT_STATE == StateMachine.State.Fleeing)
			{
				speed += (maxSpeed * SpeedMultiplier - speed) / (7f * acceleration) * GameManager.DeltaTime * Spine.timeScale;
				if (UsePathing)
				{
					if (Spine.timeScale != 0.0001f)
					{
						state.facingAngle = Mathf.LerpAngle(state.facingAngle, Utils.GetAngle(base.transform.position, pathToFollow[currentWaypoint]), Time.deltaTime * turningSpeed);
					}
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
				speed += (0f - speed) / (4f * acceleration) * GameManager.DeltaTime * Spine.timeScale;
			}
			move();
		}
		else
		{
			base.Update();
		}
	}

	protected override void FixedUpdate()
	{
		if (Spine.timeScale != 0.0001f)
		{
			base.FixedUpdate();
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		if (!ChasingPlayer)
		{
			if (!NoticedPlayer)
			{
				if (!string.IsNullOrEmpty(WarningVO))
				{
					AudioManager.Instance.PlayOneShot(WarningVO, base.gameObject);
				}
				if ((bool)warningIcon)
				{
					warningIcon.AnimationState.SetAnimation(0, "warn-start", false);
					warningIcon.AnimationState.AddAnimation(0, "warn-stop", false, 2f);
				}
				NoticedPlayer = true;
			}
			maxSpeed = ChaseSpeed;
			ChasingPlayer = true;
		}
		if (Attacking && canBeStunned)
		{
			Spine.AnimationState.SetAnimation(0, IdleAnimation, true);
			StopAllCoroutines();
			StartCoroutine(HurtRoutine());
		}
		else if (damageColliderEvents.gameObject.activeSelf)
		{
			Spine.AnimationState.SetAnimation(0, IdleAnimation, true);
			StopAllCoroutines();
			StartCoroutine(HurtRoutine());
		}
		if (AttackType != Health.AttackTypes.NoKnockBack && KnockbackForceModifier != 0f)
		{
			StartCoroutine(ApplyForceRoutine(Attacker));
		}
		SimpleSpineFlash.FlashFillRed();
	}

	private IEnumerator HurtRoutine()
	{
		if (!string.IsNullOrEmpty(GetHitVO))
		{
			AudioManager.Instance.PlayOneShot(GetHitVO, base.gameObject);
		}
		damageColliderEvents.SetActive(false);
		Attacking = false;
		ClearPaths();
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
		StartCoroutine(ActiveRoutine());
	}

	protected virtual IEnumerator ActiveRoutine()
	{
		yield return new WaitForEndOfFrame();
		while (true)
		{
			float num = turningSpeed;
			if (!ChasingPlayer)
			{
				state.LookAngle = state.facingAngle;
				if (GameManager.RoomActive && GetClosestTarget() != null && Vector3.Distance(base.transform.position, GetClosestTarget().transform.position) < noticePlayerDistance)
				{
					if (!NoticedPlayer)
					{
						if (!string.IsNullOrEmpty(WarningVO))
						{
							AudioManager.Instance.PlayOneShot(WarningVO, base.gameObject);
						}
						if (warningIcon != null)
						{
							warningIcon.AnimationState.SetAnimation(0, "warn-start", false);
							warningIcon.AnimationState.AddAnimation(0, "warn-stop", false, 2f);
						}
						NoticedPlayer = true;
					}
					maxSpeed = ChaseSpeed;
					ChasingPlayer = true;
				}
			}
			else
			{
				if (GetClosestTarget() == null || Vector3.Distance(base.transform.position, GetClosestTarget().transform.position) > 12f)
				{
					TargetPosition = StartingPosition.Value;
					maxSpeed = IdleSpeed;
					ChasingPlayer = false;
				}
				else
				{
					if (avoidTarget)
					{
						TargetPosition = -GetClosestTarget().transform.position;
						int num2 = 0;
						while (num2 < 10 && Vector3.Magnitude(TargetPosition - base.transform.position) < 3f)
						{
							Debug.Log("Dist " + Vector3.Magnitude(TargetPosition - base.transform.position) + " " + num2);
							num2++;
							TargetPosition *= 3f;
						}
					}
					else
					{
						TargetPosition = GetClosestTarget().transform.position;
					}
					state.LookAngle = Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position);
				}
				if (avoidTarget)
				{
					StartCoroutine(FleeRoutine());
				}
				else if ((AttackCoolDown -= Time.deltaTime) < 0f)
				{
					if (ShouldStartCharging())
					{
						StartCoroutine(ChargingRoutine());
						yield break;
					}
					if (ShouldAttack())
					{
						break;
					}
				}
			}
			Angle = Mathf.LerpAngle(Angle, Utils.GetAngle(base.transform.position, TargetPosition), Time.deltaTime * num);
			if (GameManager.GetInstance() != null && angleNoiseAmplitude > 0f && angleNoiseFrequency > 0f && Vector3.Distance(TargetPosition, base.transform.position) < MaximumRange)
			{
				Angle += (-0.5f + Mathf.PerlinNoise(GameManager.GetInstance().TimeSince(timestamp) * angleNoiseFrequency, 0f)) * angleNoiseAmplitude * (float)RanDirection;
			}
			if (!useAcceleration)
			{
				speed = maxSpeed * SpeedMultiplier;
			}
			state.facingAngle = Angle;
			yield return null;
		}
		CurrentAttackNum = 0;
		StartCoroutine(AttackRoutine());
	}

	protected virtual IEnumerator AttackRoutine()
	{
		Attacking = true;
		Spine.AnimationState.SetAnimation(0, SignPostAttackAnimation, false);
		float Progress = 0f;
		float Duration = 1f;
		float CurrentSpeed = speed;
		float flashTickTimer = 0f;
		AudioManager.Instance.PlayOneShot("event:/enemy/chaser/chaser_charge", base.transform.position);
		Action<int> onAttack = OnAttack;
		if (onAttack != null)
		{
			onAttack(CurrentAttackNum);
		}
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * Spine.timeScale);
			if (!(num < Duration))
			{
				break;
			}
			if (GetClosestTarget() != null)
			{
				state.LookAngle = Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position);
			}
			speed = Mathf.SmoothStep(CurrentSpeed, 0f, Progress / Duration);
			if (Aiming != null)
			{
				Aiming.gameObject.SetActive(true);
				if (Spine.timeScale != 0.0001f)
				{
					Aiming.transform.eulerAngles = new Vector3(0f, 0f, state.LookAngle);
				}
				if (flashTickTimer >= 0.12f && BiomeConstants.Instance.IsFlashLightsActive)
				{
					Aiming.color = ((Aiming.color == Color.red) ? Color.white : Color.red);
					flashTickTimer = 0f;
				}
				flashTickTimer += Time.deltaTime;
			}
			SimpleSpineFlash.FlashWhite(Progress / Duration);
			yield return null;
		}
		SimpleSpineFlash.FlashWhite(false);
		Spine.AnimationState.SetAnimation(0, AttackAnimation, false);
		Spine.AnimationState.AddAnimation(0, IdleAnimation, true, 0f);
		damageColliderEvents.SetActive(true);
		AudioManager.Instance.PlayOneShot("event:/enemy/chaser/chaser_attack", base.transform.position);
		if (!string.IsNullOrEmpty(WarningVO))
		{
			AudioManager.Instance.PlayOneShot(AttackVO, base.gameObject);
		}
		DisableForces = true;
		Angle = state.LookAngle * ((float)Math.PI / 180f);
		Force = new Vector2(AttackForce * Mathf.Cos(Angle), AttackForce * Mathf.Sin(Angle));
		rb.AddForce(Force);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.3f))
			{
				break;
			}
			yield return null;
		}
		damageColliderEvents.SetActive(false);
		if (Aiming != null)
		{
			Aiming.gameObject.SetActive(false);
		}
		if (++CurrentAttackNum < NumAttacks)
		{
			StartCoroutine(AttackRoutine());
			yield break;
		}
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < postAttackDelay))
			{
				break;
			}
			yield return null;
		}
		DisableForces = false;
		Attacking = false;
		AttackCoolDown = UnityEngine.Random.Range(AttackCoolDownDuration.x, AttackCoolDownDuration.y);
		System.Action onAttackComplete = OnAttackComplete;
		if (onAttackComplete != null)
		{
			onAttackComplete();
		}
		StartCoroutine(ActiveRoutine());
	}

	protected virtual IEnumerator ChargingRoutine()
	{
		yield return null;
	}

	protected virtual IEnumerator FleeRoutine()
	{
		yield return null;
	}

	protected virtual IEnumerator ApplyForceRoutine(GameObject Attacker)
	{
		DisableForces = true;
		Angle = Utils.GetAngle(Attacker.transform.position, base.transform.position) * ((float)Math.PI / 180f);
		Force = new Vector2(1000f * Mathf.Cos(Angle), 1000f * Mathf.Sin(Angle));
		rb.AddForce(Force * KnockbackForceModifier);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < KnockbackDuration))
			{
				break;
			}
			yield return null;
		}
		DisableForces = false;
		rb.velocity = Vector2.zero;
		if (AttackAfterKnockback)
		{
			AttackCoolDown = 0f;
		}
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (!string.IsNullOrEmpty(DeathVO))
		{
			AudioManager.Instance.PlayOneShot(DeathVO, base.gameObject);
		}
		Spine.AnimationState.Event -= HandleEvent;
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		if ((bool)DeadBody)
		{
			GameObject obj = DeadBody.gameObject.Spawn();
			obj.transform.position = base.transform.position;
			obj.GetComponent<DeadBodyFlying>().Init(Utils.GetAngle(AttackLocation, base.transform.position));
		}
	}

	public override void OnDisable()
	{
		if (Spine.AnimationState != null)
		{
			Spine.AnimationState.Event -= HandleEvent;
		}
		enemyBats.Remove(this);
		base.OnDisable();
		if (damageColliderEvents != null)
		{
			damageColliderEvents.SetActive(false);
			damageColliderEvents.OnTriggerEnterEvent -= OnDamageTriggerEnter;
		}
	}

	private void OnDrawGizmos()
	{
		if (StartingPosition.HasValue)
		{
			Utils.DrawCircleXY(TargetPosition, 0.3f, Color.red);
		}
		if (StartingPosition.HasValue)
		{
			Utils.DrawCircleXY(TargetPosition, MaximumRange, Color.red);
		}
		else
		{
			Utils.DrawCircleXY(base.transform.position, MaximumRange, Color.red);
		}
	}

	private void OnDamageTriggerEnter(Collider2D collider)
	{
		EnemyHealth = collider.GetComponent<Health>();
		if (EnemyHealth != null && (EnemyHealth.team != health.team || health.team == Health.Team.PlayerTeam))
		{
			EnemyHealth.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, EnemyHealth.transform.position, 0.7f));
		}
	}

	protected virtual bool ShouldStartCharging()
	{
		return false;
	}

	protected virtual bool ShouldAttack()
	{
		if (DistanceCheck < attackPlayerDistance)
		{
			return GameManager.RoomActive;
		}
		return false;
	}
}
