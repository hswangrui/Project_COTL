using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using MMBiomeGeneration;
using Spine.Unity;
using UnityEngine;

public class EnemyArcherTennis : UnitObject
{
	private enum State
	{
		Idle,
		Shooting,
		Teleporting,
		CloseCombatAttack
	}

	public float GlobalShotDelay = 5f;

	public int ShotsToFire = 1;

	public float DelayBetweenShots = 0.2f;

	public float DelayReaiming = 0.5f;

	public TennisBall tennisBall;

	private GameObject TargetObject;

	public SkeletonAnimation Spine;

	public int DistanceFromTargetPosition = 3;

	public float maxTennisDist = 16f;

	public SimpleSpineFlash SimpleSpineFlash;

	public SpriteRenderer Aiming;

	public ColliderEvents damageColliderEvents;

	public List<ParticleSystem> invincibleEffects;

	public float chanceOfMissingTennisReturnBase = 0.1f;

	public float chanceOfMissingTennisReturnIncrease = 0.05f;

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

	public GameObject prefabForFleeceSwap;

	private Coroutine cWaitForTarget;

	public float KnockbackMultiplier = 1f;

	private float ShootDelay;

	private float TeleportDelay;

	[HideInInspector]
	public float Angle;

	private float RandomChangeAngle = 3f;

	private Vector3 TargetPosition;

	public int MaintainDistance = 3;

	private float RepathTimer;

	private State MyState;

	private float lastShot;

	private float CloseCombatCooldown;

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
		if (!PlayerFleeceManager.FleeceSwapsWeaponForCurse())
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(prefabForFleeceSwap);
		gameObject.transform.SetParent(base.transform.parent);
		gameObject.transform.localPosition = Vector3.zero;
		Interaction_Chest instance = Interaction_Chest.Instance;
		if ((object)instance != null)
		{
			instance.AddEnemy(gameObject.GetComponent<Health>());
		}
		Health[] componentsInChildren = gameObject.GetComponentsInChildren<Health>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Interaction_Chest instance2 = Interaction_Chest.Instance;
			if ((object)instance2 != null)
			{
				instance2.AddEnemy(componentsInChildren[i]);
			}
		}
		Interaction_Chest instance3 = Interaction_Chest.Instance;
		if ((object)instance3 != null)
		{
			instance3.Enemies.Remove(health);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void Start()
	{
		if (!PlayerFleeceManager.FleeceSwapsWeaponForCurse())
		{
			SeperateObject = true;
			Aiming.gameObject.SetActive(false);
			SetInvincible(true);
		}
	}

	public void SetInvincible(bool enabled)
	{
		if (enabled)
		{
			for (int i = 0; i < invincibleEffects.Count; i++)
			{
				if (invincibleEffects[i] != null)
				{
					invincibleEffects[i].Play();
				}
			}
			health.invincible = false;
			health.DamageModifier = 0.005f;
			Cower component = GetComponent<Cower>();
			if (component != null)
			{
				component.preventStandardStagger = true;
			}
			return;
		}
		for (int j = 0; j < invincibleEffects.Count; j++)
		{
			if (invincibleEffects[j] != null)
			{
				invincibleEffects[j].Stop();
			}
		}
		health.invincible = false;
		health.DamageModifier = 1.5f;
		Cower component2 = GetComponent<Cower>();
		if (component2 != null)
		{
			component2.preventStandardStagger = false;
		}
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
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEvents.SetActive(false);
		}
		SetInvincible(true);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		Aiming.gameObject.SetActive(false);
		ClearPaths();
		StopAllCoroutines();
		DisableForces = false;
		SimpleSpineFlash.FlashWhite(false);
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

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (!health.WasJustParried)
		{
			if (health.DamageModifier < 1f)
			{
				Vector3 position = base.transform.position;
				position.y -= 0.5f;
				position.z -= 0.75f;
				BiomeConstants.Instance.EmitBlockImpact(position, Angle, base.transform);
				DoKnockBack(Utils.GetAngle(Attacker.transform.position, base.transform.position) * ((float)Math.PI / 180f), KnockbackMultiplier * 0.5f, 0.25f);
			}
			else
			{
				SimpleSpineFlash.FlashWhite(false);
				SimpleSpineFlash.FlashFillRed();
				base.OnHit(Attacker, AttackLocation, AttackType);
				DoKnockBack(Utils.GetAngle(Attacker.transform.position, base.transform.position) * ((float)Math.PI / 180f), KnockbackMultiplier, 0.5f);
			}
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
		if (TeleportDelay < 0f && UnityEngine.Random.Range(0f, 0.75f) < 0.5f)
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
		RepathTimer = 0.5f;
		while (TargetObject == null)
		{
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
		bool InRange = false;
		while (!InRange)
		{
			if (TargetObject == null)
			{
				DoWaitForTarget();
				yield break;
			}
			float num = Vector3.Distance(TargetObject.transform.position, base.transform.position);
			if (num <= (float)VisionRange && CheckLineOfSight(TargetObject.transform.position, Mathf.Min(num, VisionRange)))
			{
				InRange = true;
				yield return null;
				continue;
			}
			StartCoroutine(ChasePlayer());
			yield break;
		}
		StartCoroutine(ChasePlayer());
	}

	public void ReturnFire(bool success)
	{
		if (success)
		{
			CameraManager.shakeCamera(2f);
			AudioManager.Instance.PlayOneShot("event:/player/Curses/arrow_hit", base.transform.position);
			StartCoroutine(CloseCombatAttack(0.1f, 0.1f));
		}
		else
		{
			AudioManager.Instance.PlayOneShot("event:/player/Curses/arrow_hit", base.transform.position);
			CameraManager.shakeCamera(5f);
		}
	}

	private IEnumerator ChasePlayer()
	{
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
				if (!tennisBall.isActive && (ShootDelay -= Time.deltaTime * Spine.timeScale) < 0f && Vector3.Distance(base.transform.position, TargetObject.transform.position) < maxTennisDist && Time.realtimeSinceStartup > (lastShot + GlobalShotDelay) / Spine.timeScale)
				{
					lastShot = Time.realtimeSinceStartup;
					StartCoroutine(ShootArrowRoutine());
					break;
				}
			}
			yield return null;
		}
	}

	private IEnumerator CloseCombatAttack(float waitTime = 0.4f, float signpostDelay = 0.5f)
	{
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
			if (!(num < signpostDelay))
			{
				break;
			}
			if (Progress >= signpostDelay - signPostParryWindow)
			{
				canBeParried = true;
			}
			SimpleSpineFlash.FlashWhite(Progress / signpostDelay);
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
		yield return new WaitForSeconds(waitTime);
		CloseCombatCooldown = 0.5f;
		state.CURRENT_STATE = StateMachine.State.Idle;
		DoWaitForTarget();
	}

	private IEnumerator ShootArrowRoutine()
	{
		ClearPaths();
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		MyState = State.Shooting;
		ShootDelay = UnityEngine.Random.Range(1f, 1.5f);
		yield return null;
		Spine.AnimationState.SetAnimation(0, "archer-attack-charge2", false);
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
			tennisBall.transform.position = base.transform.position + new Vector3(0.5f * Mathf.Cos(state.LookAngle * ((float)Math.PI / 180f)), 0.5f * Mathf.Sin(state.LookAngle * ((float)Math.PI / 180f)));
			tennisBall.Init(PlayerFarming.Instance.health, health);
			Spine.AnimationState.SetAnimation(0, "archer-attack-impact2", false);
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
		RepathTimer = 0.5f;
		ShootDelay = UnityEngine.Random.Range(0f, 0.25f);
		TeleportDelay = 0.5f;
		MyState = State.Idle;
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
			RepathTimer = 0.5f;
			return;
		}
		Angle = Utils.GetAngle(TargetObject.transform.position, base.transform.position) * ((float)Math.PI / 180f);
		float radius = 1f;
		RaycastHit2D raycastHit2D = Physics2D.CircleCast(base.transform.position, radius, Vector3.Normalize(PointToCheck - base.transform.position), MaintainDistance, layerToCheck);
		if (raycastHit2D.collider != null)
		{
			if (Vector3.Distance(base.transform.position, raycastHit2D.centroid) > (float)AcceptableMove)
			{
				if (TeleportDelay > 0f || UnityEngine.Random.Range(0f, 0.5f) == 0f)
				{
					if (ShowDebug)
					{
						Points.Add(new Vector3(raycastHit2D.centroid.x, raycastHit2D.centroid.y) + Vector3.Normalize(base.transform.position - PointToCheck) * CircleCastOffset);
						PointsLink.Add(new Vector3(base.transform.position.x, base.transform.position.y));
					}
					TargetPosition = (Vector3)raycastHit2D.centroid + Vector3.Normalize(base.transform.position - PointToCheck) * CircleCastOffset;
					givePath(TargetPosition);
					RepathTimer = 0.5f;
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
				if (TeleportDelay > 0f || UnityEngine.Random.Range(0f, 0.5f) == 0f)
				{
					TargetPosition = Vector3.Lerp(base.transform.position, TargetObject.transform.position, 0.5f);
					givePath(TargetPosition);
					RepathTimer = 0.5f;
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
			RepathTimer = 0.5f;
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
