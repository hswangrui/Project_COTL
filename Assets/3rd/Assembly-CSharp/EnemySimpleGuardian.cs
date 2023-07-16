using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMODUnity;
using Spine.Unity;
using UnityEngine;

public class EnemySimpleGuardian : UnitObject
{
	private static float LastSimpleGuardianAttacked = float.MinValue;

	private static float LastSimpleGuardianRingProjectiles = float.MinValue;

	public bool CanDoRingShot = true;

	public bool CanDoBoomerangShot = true;

	[SerializeField]
	private bool requireLineOfSite = true;

	private GameObject TargetObject;

	private Health EnemyHealth;

	public ColliderEvents damageColliderEvents;

	public SkeletonAnimation Spine;

	private SimpleSpineFlash SimpleSpineFlash;

	private SimpleSpineEventListener simpleSpineEventListener;

	private List<Collider2D> collider2DList;

	private Collider2D HealthCollider;

	private GameObject guardianGameObject;

	public ParticleSystem DashParticles;

	[EventRef]
	public string attackSoundPath = string.Empty;

	[EventRef]
	public string onHitSoundPath = string.Empty;

	public DeadBodySliding deadBodySliding;

	private static List<EnemySimpleGuardian> SimpleGuardians = new List<EnemySimpleGuardian>();

	public string WalkAnimation = "walk2";

	private float GlobalAttackDelay = 2f;

	private float GlobalRingAttackDelay = 10f;

	private Vector3 TargetPosition;

	[SerializeField]
	private ProjectileCircle projectilePatternRings;

	[SerializeField]
	private float projectilePatternRingsSpeed = 2.5f;

	[SerializeField]
	private float projectilePatternRingsRadius = 1f;

	[SerializeField]
	private float projectilePatternRingsAcceleration = 7.5f;

	[SerializeField]
	private GameObject BoomerangArrow;

	[SerializeField]
	private float BoomerangCount = 20f;

	[SerializeField]
	private float BoomerangSpeed = 5f;

	[SerializeField]
	private float BoomerangReturnSpeed = -1f;

	[SerializeField]
	private float OutwardDuration = 3f;

	[SerializeField]
	private float ReturnDuration = 2f;

	[SerializeField]
	private float PauseDuration = 0.5f;

	private List<Projectile> Boomeranges = new List<Projectile>();

	public override void OnEnable()
	{
		SeperateObject = true;
		base.OnEnable();
		SimpleGuardians.Add(this);
		SimpleSpineFlash = GetComponentInChildren<SimpleSpineFlash>();
		simpleSpineEventListener = GetComponent<SimpleSpineEventListener>();
		simpleSpineEventListener.OnSpineEvent += OnSpineEvent;
		HealthCollider = GetComponent<Collider2D>();
		DashParticles.Stop();
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEvents.SetActive(false);
		}
		StartCoroutine(WaitForTarget());
		LastSimpleGuardianAttacked = GameManager.GetInstance().CurrentTime;
		LastSimpleGuardianRingProjectiles = GameManager.GetInstance().CurrentTime;
	}

	public override void OnDisable()
	{
		base.OnDisable();
		SimpleGuardians.Remove(this);
		simpleSpineEventListener.OnSpineEvent -= OnSpineEvent;
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent -= OnDamageTriggerEnter;
		}
	}

	public override void Awake()
	{
		base.Awake();
		guardianGameObject = base.gameObject;
		DataManager.Instance.LastSimpleGuardianAttacked = GameManager.GetInstance().CurrentTime;
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
			Health closestTarget = GetClosestTarget();
			if ((bool)closestTarget)
			{
				TargetObject = closestTarget.gameObject;
				requireLineOfSite = false;
				VisionRange = int.MaxValue;
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
		StartCoroutine(FightPlayer());
	}

	private void LookAtTarget()
	{
		if (!(GetClosestTarget() == null))
		{
			float angle = Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position);
			state.facingAngle = angle;
			state.LookAngle = angle;
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/humanoid_large/gethit", base.transform.position);
		if (!string.IsNullOrEmpty(onHitSoundPath))
		{
			AudioManager.Instance.PlayOneShot(onHitSoundPath, base.transform.position);
		}
		CameraManager.shakeCamera(0.5f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		SimpleSpineFlash.FlashFillRed();
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/humanoid_large/death", base.transform.position);
		AudioManager.Instance.SetMusicRoomID(0, "deathcat_room_id");
		foreach (Projectile boomerange in Boomeranges)
		{
			boomerange.DestroyProjectile();
		}
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		SimpleSpineFlash.FlashWhite(false);
		StopAllCoroutines();
	}

	private void OnSpineEvent(string EventName)
	{
		if (!(EventName == "Invincible On"))
		{
			if (EventName == "Invincible Off")
			{
				health.invincible = false;
				HealthCollider.enabled = true;
			}
		}
		else
		{
			health.invincible = true;
			HealthCollider.enabled = false;
		}
	}

	private void GetPath()
	{
		if (SimpleGuardians.Count > 1)
		{
			Debug.Log("CHECK! ");
			float num = float.MaxValue;
			EnemySimpleGuardian enemySimpleGuardian = null;
			foreach (EnemySimpleGuardian simpleGuardian in SimpleGuardians)
			{
				float num2 = Vector3.Distance(simpleGuardian.transform.position, TargetObject.transform.position);
				if (num2 < num)
				{
					num = num2;
					enemySimpleGuardian = simpleGuardian;
				}
			}
			if (enemySimpleGuardian == this)
			{
				TargetPosition = TargetObject.transform.position;
			}
			else
			{
				TargetPosition = Vector3.zero;
			}
		}
		else
		{
			TargetPosition = TargetObject.transform.position;
		}
		if (Vector3.Distance(TargetPosition, base.transform.position) > StoppingDistance)
		{
			givePath(TargetPosition);
			if (Spine.AnimationName != WalkAnimation)
			{
				Spine.AnimationState.SetAnimation(0, WalkAnimation, true);
			}
		}
		else if (Spine.AnimationName != "idle")
		{
			Spine.AnimationState.SetAnimation(0, "idle", true);
		}
	}

	private IEnumerator FightPlayer()
	{
		while (SimpleGuardians.Count <= 1 && (TargetObject == null || Vector3.Distance(TargetObject.transform.position, base.transform.position) > 12f))
		{
			if (TargetObject != null)
			{
				LookAtTarget();
			}
			yield return null;
		}
		GetPath();
		float RepathTimer = 0f;
		int NumAttacks = 2;
		float AttackSpeed = 15f;
		bool Loop = true;
		while (Loop)
		{
			switch (state.CURRENT_STATE)
			{
			case StateMachine.State.Idle:
			case StateMachine.State.Moving:
				LookAtTarget();
				if (Vector2.Distance(base.transform.position, TargetObject.transform.position) < 3f)
				{
					if (GameManager.GetInstance().CurrentTime > (LastSimpleGuardianAttacked + GlobalAttackDelay) / Spine.timeScale)
					{
						DataManager.Instance.LastSimpleGuardianAttacked = TimeManager.TotalElapsedGameTime;
						LastSimpleGuardianAttacked = GameManager.GetInstance().CurrentTime;
						state.CURRENT_STATE = StateMachine.State.SignPostAttack;
						Spine.AnimationState.SetAnimation(0, "attack" + (4 - NumAttacks), false);
						Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
					}
					else if (state.CURRENT_STATE != 0)
					{
						state.CURRENT_STATE = StateMachine.State.Idle;
						Spine.AnimationState.SetAnimation(0, "idle", true);
					}
				}
				if (CanDoRingShot && Vector2.Distance(base.transform.position, TargetObject.transform.position) >= 5f && GameManager.GetInstance().CurrentTime > (LastSimpleGuardianRingProjectiles + GlobalRingAttackDelay) / Spine.timeScale)
				{
					DataManager.Instance.LastSimpleGuardianRingProjectiles = TimeManager.TotalElapsedGameTime;
					LastSimpleGuardianRingProjectiles = GameManager.GetInstance().CurrentTime;
					ProjectileRings();
					yield break;
				}
				if (CanDoBoomerangShot && (Vector2.Distance(base.transform.position, TargetObject.transform.position) >= 5f || Vector2.Distance(base.transform.position, TargetObject.transform.position) < 2f) && GameManager.GetInstance().CurrentTime > (LastSimpleGuardianRingProjectiles + GlobalRingAttackDelay) / Spine.timeScale)
				{
					DataManager.Instance.LastSimpleGuardianRingProjectiles = TimeManager.TotalElapsedGameTime;
					LastSimpleGuardianRingProjectiles = GameManager.GetInstance().CurrentTime;
					ProjectileBoomerangs();
					yield break;
				}
				if (Vector2.Distance(base.transform.position, TargetObject.transform.position) >= 3f)
				{
					float num2;
					RepathTimer = (num2 = RepathTimer + Time.deltaTime * Spine.timeScale);
					if (num2 > 0.2f)
					{
						RepathTimer = 0f;
						GetPath();
					}
				}
				if (damageColliderEvents != null)
				{
					damageColliderEvents.SetActive(false);
				}
				break;
			case StateMachine.State.SignPostAttack:
				state.facingAngle = Utils.GetAngle(base.transform.position, TargetPosition);
				if ((state.Timer += Time.deltaTime * Spine.timeScale) >= 0.5f)
				{
					SimpleSpineFlash.FlashWhite(false);
					DashParticles.Play();
					CameraManager.shakeCamera(0.4f, state.facingAngle);
					state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
					speed = AttackSpeed * Time.deltaTime;
					if (damageColliderEvents != null)
					{
						damageColliderEvents.SetActive(true);
					}
					if (!string.IsNullOrEmpty(attackSoundPath))
					{
						AudioManager.Instance.PlayOneShot(attackSoundPath, base.transform.position);
					}
					AudioManager.Instance.PlayOneShot("event:/enemy/vocals/humanoid_large/warning", base.transform.position);
				}
				else
				{
					SimpleSpineFlash.FlashWhite(state.Timer / 0.5f);
				}
				break;
			case StateMachine.State.RecoverFromAttack:
				if (AttackSpeed > 0f)
				{
					AttackSpeed -= 1f * GameManager.DeltaTime * Spine.timeScale;
				}
				speed = AttackSpeed * Time.deltaTime * Spine.timeScale;
				if (state.Timer >= 0.25f && damageColliderEvents != null)
				{
					damageColliderEvents.SetActive(false);
				}
				if ((state.Timer += Time.deltaTime * Spine.timeScale) >= 0.5f)
				{
					DashParticles.Stop();
					AudioManager.Instance.PlayOneShot("event:/enemy/vocals/humanoid_large/attack", base.transform.position);
					int num = NumAttacks - 1;
					NumAttacks = num;
					if (num > 0)
					{
						AttackSpeed = 15 + (3 - NumAttacks) * 2;
						state.CURRENT_STATE = StateMachine.State.SignPostAttack;
						Spine.AnimationState.SetAnimation(0, "attack" + (4 - NumAttacks), false);
						Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
					}
					else
					{
						Loop = false;
						state.CURRENT_STATE = StateMachine.State.Idle;
						Spine.AnimationState.SetAnimation(0, "idle", true);
					}
				}
				break;
			}
			yield return null;
		}
		float time = 0f;
		while (true)
		{
			float num2;
			time = (num2 = time + Time.deltaTime * Spine.timeScale);
			if (!(num2 < 0.5f))
			{
				break;
			}
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		Spine.AnimationState.SetAnimation(0, "idle", true);
		if (TargetObject != null && Vector3.Distance(TargetObject.transform.position, base.transform.position) > 5f)
		{
			LookAtTarget();
			time = 0f;
			while (true)
			{
				float num2;
				time = (num2 = time + Time.deltaTime * Spine.timeScale);
				if (!(num2 < 1f))
				{
					break;
				}
				yield return null;
			}
		}
		StartCoroutine(FightPlayer());
	}

	private void OnDamageTriggerEnter(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && component.team != health.team)
		{
			component.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.7f));
		}
	}

	private void ProjectileRings()
	{
		StopAllCoroutines();
		StartCoroutine(ProjectileRingsRoutine());
	}

	private IEnumerator ProjectileRingsRoutine()
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/humanoid_large/warning", base.transform.position);
		state.CURRENT_STATE = StateMachine.State.Idle;
		yield return null;
		Spine.AnimationState.SetAnimation(0, "summon", false);
		Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 1f))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/humanoid_large/attack", base.transform.position);
		Projectile arrow = Object.Instantiate(projectilePatternRings, base.transform.parent).GetComponent<Projectile>();
		arrow.transform.position = base.transform.position;
		arrow.health = health;
		arrow.team = health.team;
		arrow.Speed = projectilePatternRingsSpeed;
		arrow.Acceleration = projectilePatternRingsAcceleration;
		arrow.GetComponent<ProjectileCircle>().InitDelayed(PlayerFarming.Instance.gameObject, projectilePatternRingsRadius, 0f, delegate
		{
			CameraManager.instance.ShakeCameraForDuration(0.8f, 0.9f, 0.3f, false);
			if (guardianGameObject != null)
			{
				AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/grenade_mass_launch", guardianGameObject);
				arrow.Angle = Mathf.Round(arrow.Angle / 45f) * 45f;
			}
			else
			{
				arrow.DestroyProjectile();
			}
		});
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 1.5666667f))
			{
				break;
			}
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		Spine.AnimationState.SetAnimation(0, "idle", true);
		StartCoroutine(FightPlayer());
	}

	private void ProjectileBoomerangs()
	{
		StopAllCoroutines();
		StartCoroutine(ProjectileBoomerangsRoutine());
	}

	private IEnumerator ProjectileBoomerangsRoutine()
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/humanoid_large/warning", base.transform.position);
		state.CURRENT_STATE = StateMachine.State.Idle;
		yield return null;
		Spine.AnimationState.SetAnimation(0, "projectiles-start", false);
		Spine.AnimationState.AddAnimation(0, "projectiles-loop", true, 0f);
		float time3 = 0f;
		while (true)
		{
			float num;
			time3 = (num = time3 + Time.deltaTime * Spine.timeScale);
			if (!(num < 1.7666667f))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/humanoid_large/attack", base.transform.position);
		CameraManager.instance.ShakeCameraForDuration(0.8f, 0.9f, 0.3f, false);
		Boomeranges = new List<Projectile>();
		float boomerangCount = BoomerangCount;
		for (float num2 = 0f; num2 < boomerangCount; num2 += 1f)
		{
			Projectile component = Object.Instantiate(BoomerangArrow, base.transform.parent).GetComponent<Projectile>();
			component.transform.position = base.transform.position;
			component.team = health.team;
			component.Speed = BoomerangSpeed;
			component.Angle = 360f / boomerangCount * num2;
			component.LifeTime = 30f;
			component.IgnoreIsland = true;
			component.Trail.time = 0.3f;
			Boomeranges.Add(component);
		}
		AudioManager.Instance.PlayOneShot("event:/enemy/shoot_magicenergy", base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/grenade_mass_launch", base.gameObject);
		time3 = 0f;
		while (true)
		{
			float num;
			time3 = (num = time3 + Time.deltaTime * Spine.timeScale);
			if (!(num < OutwardDuration))
			{
				break;
			}
			yield return null;
		}
		foreach (Projectile a2 in Boomeranges)
		{
			if (a2 != null)
			{
				DOTween.To(() => a2.SpeedMultiplier, delegate(float x)
				{
					a2.SpeedMultiplier = x;
				}, 0f, 0.2f).SetEase(Ease.OutSine);
			}
		}
		time3 = 0f;
		while (true)
		{
			float num;
			time3 = (num = time3 + Time.deltaTime * Spine.timeScale);
			if (!(num < PauseDuration))
			{
				break;
			}
			yield return null;
		}
		foreach (Projectile a in Boomeranges)
		{
			if (a != null)
			{
				DOTween.To(() => a.SpeedMultiplier, delegate(float x)
				{
					a.SpeedMultiplier = x;
				}, BoomerangReturnSpeed, 0.3f).SetEase(Ease.OutBack);
			}
		}
		AudioManager.Instance.PlayOneShot("event:/enemy/shoot_magicenergy", base.transform.position);
		time3 = 0f;
		while (true)
		{
			float num;
			time3 = (num = time3 + Time.deltaTime * Spine.timeScale);
			if (!(num < ReturnDuration))
			{
				break;
			}
			yield return null;
		}
		bool flag = true;
		foreach (Projectile boomerange in Boomeranges)
		{
			if (boomerange != null)
			{
				boomerange.EndOfLife();
				if (flag)
				{
					boomerange.EndOfLife();
					flag = false;
				}
				else
				{
					boomerange.DestroyProjectile();
				}
			}
		}
		Boomeranges.Clear();
		Spine.AnimationState.SetAnimation(0, "projectiles-stop", true);
		Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		time3 = 0f;
		while (true)
		{
			float num;
			time3 = (num = time3 + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.7f))
			{
				break;
			}
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		StartCoroutine(FightPlayer());
	}
}
