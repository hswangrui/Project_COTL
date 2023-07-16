using System.Collections;
using FMOD.Studio;
using Spine.Unity;
using UnityEngine;

public class EnemyJellyCharger : EnemyExploder
{
	[SerializeField]
	private SpriteRenderer Aiming;

	[SerializeField]
	private SkeletonAnimation Warning;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string movingAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string chargeAttackAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string chargeAttackEndAnimation;

	public SimpleSpineFlash[] SimpleSpineFlashes;

	[Header("Charging")]
	[SerializeField]
	private bool canCharge = true;

	[SerializeField]
	private bool canDeflect;

	[SerializeField]
	private float distanceToCharge;

	[SerializeField]
	private float chargeTime;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("0.75 means charger will stop targeting the player in the last 25% of charge time")]
	private float followPercentage = 0.7f;

	[SerializeField]
	private float chargeSpeed;

	[SerializeField]
	private float attackEndCooldown;

	private float warmingTimer;

	private bool warming;

	private bool chargeAttacking;

	private float chargeAttackEndTimestamp;

	private float flashTickTimer;

	private PoisonTrail poisonTrail;

	public static EnemyJellyCharger CurrentCharger;

	private float TargetAngle;

	private bool playedSfx;

	private EventInstance LoopedSound;

	public bool AllowMultipleChargers { get; set; }

	protected override void Start()
	{
		base.Start();
		poisonTrail = GetComponent<PoisonTrail>();
		if (Aiming != null)
		{
			Aiming.gameObject.SetActive(false);
		}
	}

	private IEnumerator ShowWarning()
	{
		Warning.gameObject.SetActive(true);
		yield return Warning.YieldForAnimation("warn");
		Warning.gameObject.SetActive(false);
	}

	public override void OnEnable()
	{
		base.OnEnable();
		chargeAttacking = false;
		warming = false;
	}

	public override void Update()
	{
		base.Update();
		if (canCharge)
		{
			if (!chargeAttacking && !warming && !isExploding && targetObject != null)
			{
				state.LookAngle = Utils.GetAngle(base.transform.position, targetObject.transform.position);
			}
			if ((CurrentCharger == null || CurrentCharger == this || AllowMultipleChargers) && inRange && !warming && !chargeAttacking && (bool)gm && gm.CurrentTime > chargeAttackEndTimestamp && distanceToTarget < distanceToCharge && (bool)gm && gm.CurrentTime > initialSpawnTimestamp / Spine.timeScale && (bool)targetObject && targetObject.state != null && targetObject.state.CURRENT_STATE != StateMachine.State.HitRecover && CheckLineOfSight(distanceToCharge))
			{
				WarmUp();
			}
			else if (warming && !isExploding)
			{
				warmingTimer += Time.deltaTime * Spine.timeScale;
				float num = warmingTimer / chargeTime;
				simpleSpineFlash.FlashWhite(num);
				if (!playedSfx)
				{
					AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish/warning", base.gameObject);
					LoopedSound = AudioManager.Instance.CreateLoop("event:/enemy/jellyfish_charge", base.gameObject, true);
					playedSfx = true;
				}
				if (num < followPercentage)
				{
					TargetAngle = LookAtTarget(true);
				}
				if (num > 1f && targetObject.state.CURRENT_STATE != StateMachine.State.HitRecover)
				{
					AudioManager.Instance.StopLoop(LoopedSound);
					playedSfx = false;
					ChargeAtTarget(TargetAngle);
				}
			}
			if (chargeAttacking && !isExploding)
			{
				DisableForces = false;
				speed = chargeSpeed * Spine.timeScale;
				maxSpeed = chargeSpeed;
				move();
			}
			else
			{
				maxSpeed = 0.025f;
			}
		}
		if ((bool)poisonTrail)
		{
			poisonTrail.enabled = chargeAttacking;
		}
	}

	protected override void FixedUpdate()
	{
		if (Spine.timeScale != 0.0001f)
		{
			base.FixedUpdate();
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		if (canDeflect && (chargeAttacking || warming))
		{
			warming = false;
			float angle = Utils.GetAngle(Attacker.transform.position, AttackLocation);
			LookAtAngle(angle);
			ChargeAtTarget(angle);
		}
		SimpleSpineFlash[] simpleSpineFlashes = SimpleSpineFlashes;
		for (int i = 0; i < simpleSpineFlashes.Length; i++)
		{
			simpleSpineFlashes[i].FlashFillRed();
		}
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (canDeflect && chargeAttacking && Attacker == PlayerFarming.Instance.gameObject)
		{
			warming = false;
			float angle = Utils.GetAngle(Attacker.transform.position, AttackLocation);
			LookAtAngle(angle);
			ChargeAtTarget(angle);
		}
		else
		{
			base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		}
	}

	private void WarmUp()
	{
		if (Spine != null && Spine.AnimationState != null && Spine.AnimationState.GetCurrent(0) != null && Spine.AnimationState.GetCurrent(0).Animation.Name != anticipationAnimation)
		{
			AudioManager.Instance.PlayOneShot("event:/enemy/chaser/chaser_charge", base.gameObject);
			AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish/warning", base.gameObject);
			StartCoroutine(ShowWarning());
		}
		warmingTimer = 0f;
		warming = true;
		if (Spine != null && Spine.AnimationState != null)
		{
			Spine.AnimationState.SetAnimation(0, anticipationAnimation, true);
		}
		ClearPaths();
		CurrentCharger = this;
	}

	protected override void WithinDistanceOfTarget()
	{
		if (!chargeAttacking)
		{
			base.WithinDistanceOfTarget();
		}
	}

	private void ChargeAtTarget(float angle)
	{
		angle = Mathf.Repeat(angle, 360f);
		angle = Mathf.Round(angle / 45f) * 45f;
		LookAtAngle(angle);
		if (Aiming != null && Spine.timeScale != 0.0001f)
		{
			Aiming.transform.eulerAngles = new Vector3(0f, 0f, angle);
		}
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish/attack", base.gameObject);
		AudioManager.Instance.PlayOneShot("event:/enemy/chaser/chaser_attack", base.gameObject);
		damageColliderEvents.gameObject.SetActive(true);
		warming = false;
		chargeAttacking = true;
		UsePathing = false;
		state.CURRENT_STATE = StateMachine.State.Charging;
		StartCoroutine(FlashDelay());
		string animationName = chargeAttackAnimation;
		float obj = angle;
		if (!0f.Equals(obj))
		{
			if (!45f.Equals(obj))
			{
				if (!90f.Equals(obj))
				{
					if (!135f.Equals(obj))
					{
						if (!180f.Equals(obj))
						{
							if (!225f.Equals(obj))
							{
								if (!270f.Equals(obj))
								{
									if (315f.Equals(obj))
									{
										animationName = "attacking-down-diagonal";
									}
								}
								else
								{
									animationName = "attacking-down";
								}
							}
							else
							{
								animationName = "attacking-down-diagonal";
							}
						}
						else
						{
							animationName = "attacking";
						}
					}
					else
					{
						animationName = "attacking-up-diagonal";
					}
				}
				else
				{
					animationName = "attacking-up";
				}
			}
			else
			{
				animationName = "attacking-up-diagonal";
			}
		}
		else
		{
			animationName = "attacking";
		}
		Spine.AnimationState.SetAnimation(0, animationName, true).MixDuration = 0f;
	}

	private float LookAtTarget(bool LimitTo45 = false)
	{
		float num = Utils.GetAngle(base.transform.position, targetObject.transform.position);
		if (LimitTo45)
		{
			num = Mathf.Round(num / 45f) * 45f;
		}
		LookAtAngle(num);
		if (Aiming != null)
		{
			if (!Aiming.gameObject.activeSelf && Spine.timeScale != 0.0001f)
			{
				Aiming.transform.eulerAngles = new Vector3(0f, 0f, num);
			}
			Aiming.gameObject.SetActive(true);
			float z = Mathf.LerpAngle(Aiming.transform.eulerAngles.z, num, 5f * Time.deltaTime);
			if (Spine.timeScale != 0.0001f)
			{
				Aiming.transform.eulerAngles = new Vector3(0f, 0f, z);
			}
			if (flashTickTimer >= 0.12f && BiomeConstants.Instance.IsFlashLightsActive)
			{
				Aiming.color = ((Aiming.color == Color.red) ? Color.white : Color.red);
				flashTickTimer = 0f;
			}
			flashTickTimer += Time.deltaTime;
		}
		return num;
	}

	private IEnumerator FlashDelay()
	{
		yield return new WaitForEndOfFrame();
		simpleSpineFlash.FlashWhite(false);
	}

	protected override void UpdateMoving()
	{
		if ((bool)gm && gm.CurrentTime > chargeAttackEndTimestamp / Spine.timeScale && !chargeAttacking && !warming)
		{
			if (state.CURRENT_STATE == StateMachine.State.Idle && movingAnimation != "")
			{
				Spine.AnimationState.SetAnimation(0, movingAnimation, true);
			}
			base.UpdateMoving();
		}
	}

	protected virtual void AttackEnd()
	{
		CameraManager.instance.ShakeCameraForDuration(0.8f, 1f, 0.35f);
		AudioManager.Instance.PlayOneShot("event:/enemy/impact_squishy", base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish/gethit", base.transform.position);
		chargeAttacking = false;
		chargeAttackEndTimestamp = gm.CurrentTime + attackEndCooldown;
		ClearPaths();
		if (Aiming != null)
		{
			Aiming.gameObject.SetActive(false);
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		Spine.AnimationState.SetAnimation(0, chargeAttackEndAnimation, false);
		damageColliderEvents.gameObject.SetActive(false);
		speed = 0f;
		targetObject = null;
		if (CurrentCharger == this)
		{
			CurrentCharger = null;
		}
		if (canExplode)
		{
			Explode();
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (CurrentCharger == this)
		{
			CurrentCharger = null;
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if (CurrentCharger == this)
		{
			CurrentCharger = null;
		}
	}

	protected override void KnockTowardsEnemy(GameObject Attacker, Health.AttackTypes AttackType)
	{
		if (!chargeAttacking || isHit)
		{
			base.KnockTowardsEnemy(Attacker, AttackType);
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (chargeAttacking && (layerToCheck.value & (1 << collision.gameObject.layer)) > 0)
		{
			OnDamageTriggerEnter(collision.collider);
			AttackEnd();
		}
	}

	protected override void OnDamageTriggerEnter(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		UnitObject component2 = collider.GetComponent<UnitObject>();
		if (canExplode)
		{
			if (component != null && component.team == Health.Team.PlayerTeam && canExplode && component2.state.CURRENT_STATE != StateMachine.State.Dodging)
			{
				Explode();
			}
		}
		else
		{
			base.OnDamageTriggerEnter(collider);
		}
	}
}
