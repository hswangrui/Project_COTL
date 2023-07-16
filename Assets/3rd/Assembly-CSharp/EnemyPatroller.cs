using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using Spine.Unity.Modules;
using UnityEngine;

public class EnemyPatroller : UnitObject
{
	private int Patrol;

	public List<Vector3> PatrolRoute = new List<Vector3>();

	private Vector3 StartPosition;

	private List<Collider2D> collider2DList;

	private Health EnemyHealth;

	private float RepathTimer;

	public Transform scaleThis;

	public SimpleSpineFlash[] SimpleSpineFlashes;

	public float AttackTriggerRange = 1f;

	public SkeletonAnimation Spine;

	public SkeletonUtilityEyeConstraint skeletonUtilityEyeConstraint;

	public ColliderEvents damageColliderEvents;

	public SkeletonAnimation warningIcon;

	public float PatrolSpeed = 0.05f;

	public float ChaseSpeed = 0.1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float psychoThreshold;

	[SerializeField]
	private float psychoDiveMultiplier = 1f;

	[SerializeField]
	private float psychoAttackRangeMultiplier = 1f;

	[SerializeField]
	private float psychoDiveSpeedMultiplier = 1f;

	private bool isPsycho;

	private float maxHealth;

	private bool NoticedPlayer;

	private bool FirstPath = true;

	public bool JumpOnHit = true;

	private float Angle;

	private Vector3 Force;

	public float KnockbackForceModifier = 1f;

	public float KnockbackDelay = 0.5f;

	private bool ApplyingForce;

	[Range(0f, 1f)]
	public float ChanceToPathTowardsPlayer = 0.8f;

	public float TurningArc = 90f;

	public Vector2 DistanceRange = new Vector2(1f, 3f);

	private bool PathingToPlayer;

	private float RandomDirection;

	private float DistanceToPathTowardsPlayer = 6f;

	private Vector3 TargetPosition;

	public int NumberOfDives = 3;

	public ParticleSystem AoEParticles;

	public float MoveSpeed = 8f;

	public float ArcHeight = 5f;

	private float DiveDelayTimer;

	public float DiveDelay = 3f;

	public List<FollowAsTail> TailPieces = new List<FollowAsTail>();

	protected virtual void Start()
	{
		StartPosition = base.transform.position;
		PatrolRoute.Insert(0, Vector3.zero);
		maxHealth = health.totalHP;
	}

	private IEnumerator ActiveRoutine()
	{
		yield return new WaitForEndOfFrame();
		Spine.AnimationState.SetAnimation(0, "animation", true);
		if (damageColliderEvents != null)
		{
			damageColliderEvents.SetActive(false);
		}
		while (!((DiveDelayTimer -= Time.deltaTime) <= 0f) || !(GetClosestTarget() != null) || !(Vector3.Distance(base.transform.position, GetClosestTarget().transform.position) < AttackTriggerRange))
		{
			if (!NoticedPlayer && PatrolRoute.Count > 1)
			{
				if (pathToFollow == null)
				{
					Debug.Log("pathToFollow " + pathToFollow);
					Patrol = ++Patrol % PatrolRoute.Count;
					givePath(StartPosition + PatrolRoute[Patrol]);
				}
				else if ((RepathTimer += Time.deltaTime) > (FirstPath ? 0f : 0.5f))
				{
					FirstPath = true;
					maxSpeed = PatrolSpeed;
					givePath(StartPosition + PatrolRoute[Patrol]);
					RepathTimer = 0f;
				}
			}
			else if (state.CURRENT_STATE == StateMachine.State.Idle)
			{
				maxSpeed = ChaseSpeed;
				GetNewTargetPosition();
			}
			yield return null;
		}
		if (!NoticedPlayer)
		{
			if (!string.IsNullOrEmpty("event:/enemy/vocals/worm/warning"))
			{
				AudioManager.Instance.PlayOneShot("event:/enemy/vocals/worm/warning", base.transform.position);
			}
			warningIcon.AnimationState.SetAnimation(0, "warn-start", false);
			warningIcon.AnimationState.AddAnimation(0, "warn-stop", false, 2f);
			StartCoroutine(DelayDiveRoutine());
		}
		else
		{
			StartCoroutine(DiveMoveRoutine());
		}
		NoticedPlayer = true;
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		if (JumpOnHit && DiveDelayTimer > 0f)
		{
			StopAllCoroutines();
			DisableForces = false;
			StartCoroutine(DelayDiveRoutine());
		}
		if (AttackType != Health.AttackTypes.NoKnockBack && !ApplyingForce)
		{
			if (!JumpOnHit)
			{
				StopAllCoroutines();
			}
			StartCoroutine(ApplyForceRoutine(Attacker));
		}
		SimpleSpineFlash[] simpleSpineFlashes = SimpleSpineFlashes;
		for (int i = 0; i < simpleSpineFlashes.Length; i++)
		{
			simpleSpineFlashes[i].FlashFillRed();
		}
		if (health.HP / maxHealth < psychoThreshold && !isPsycho)
		{
			DiveDelay *= psychoDiveMultiplier;
			DiveDelayTimer = 0f;
			AttackTriggerRange *= psychoAttackRangeMultiplier;
			MoveSpeed *= psychoDiveSpeedMultiplier;
			ChanceToPathTowardsPlayer = 1f;
			isPsycho = true;
		}
	}

	private IEnumerator DelayDiveRoutine()
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
		StartCoroutine(DiveMoveRoutine());
	}

	private IEnumerator ApplyForceRoutine(GameObject Attacker)
	{
		if (!JumpOnHit)
		{
			ApplyingForce = true;
			Spine.AnimationState.SetAnimation(0, "attack-impact", false);
			Spine.AnimationState.AddAnimation(0, "attack-charge", true, 0f);
		}
		DisableForces = true;
		Angle = Utils.GetAngle(Attacker.transform.position, base.transform.position) * ((float)Math.PI / 180f);
		Force = new Vector2(1000f * Mathf.Cos(Angle), 1000f * Mathf.Sin(Angle));
		rb.AddForce(Force * KnockbackForceModifier);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < KnockbackDelay))
			{
				break;
			}
			yield return null;
		}
		DisableForces = false;
		if (!JumpOnHit)
		{
			ApplyingForce = false;
			Spine.AnimationState.SetAnimation(0, "animation", true);
			StartCoroutine(ActiveRoutine());
		}
	}

	private bool GetNewPlayerPosition()
	{
		Health closestTarget = GetClosestTarget();
		if (closestTarget == null)
		{
			return false;
		}
		float num = 4f;
		float num2 = Utils.GetAngle(base.transform.position, closestTarget.transform.position) * ((float)Math.PI / 180f);
		bool flag = true;
		float num3 = 100f;
		while ((num3 -= 1f) > 0f)
		{
			if (!flag)
			{
				num2 += (float)UnityEngine.Random.Range(-90, 90) * ((float)Math.PI / 180f);
			}
			flag = false;
			Vector3 vector = base.transform.position + new Vector3(num * Mathf.Cos(num2), num * Mathf.Sin(num2));
			float radius = 0.2f;
			if (Physics2D.CircleCast(base.transform.position, radius, Vector3.Normalize(vector - base.transform.position), num, layerToCheck).collider != null)
			{
				num2 = 180f - num2;
				continue;
			}
			TargetPosition = vector;
			return true;
		}
		return false;
	}

	public void GetNewTargetPosition()
	{
		float num = 100f;
		if (GetClosestTarget() != null && ChanceToPathTowardsPlayer > 0f && UnityEngine.Random.value < ChanceToPathTowardsPlayer && Vector3.Distance(base.transform.position, GetClosestTarget().transform.position) < DistanceToPathTowardsPlayer && CheckLineOfSight(PlayerFarming.Instance.transform.position, DistanceToPathTowardsPlayer))
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
			if (Physics2D.CircleCast(base.transform.position, radius, Vector3.Normalize(vector - base.transform.position), num2, layerToCheck).collider != null)
			{
				RandomDirection = 180f - RandomDirection;
				continue;
			}
			givePath(vector);
			break;
		}
	}

	private IEnumerator DiveMoveRoutine()
	{
		ClearPaths();
		state.CURRENT_STATE = StateMachine.State.Idle;
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= NumberOfDives)
			{
				break;
			}
			if (!GetNewPlayerPosition())
			{
				continue;
			}
			while (Spine.timeScale == 0.0001f)
			{
				yield return null;
			}
			AudioManager.Instance.PlayOneShot("event:/enemy/patrol_worm/patrol_worm_jump", base.transform.position);
			health.invincible = true;
			Spine.AnimationState.SetAnimation(0, "jump", false);
			Vector3 StartPosition = base.transform.position;
			float Progress = 0f;
			float Duration = Vector3.Distance(StartPosition, TargetPosition) / MoveSpeed;
			Vector3 Curve = StartPosition + (TargetPosition - StartPosition) / 2f + Vector3.back * ArcHeight;
			while (true)
			{
				float num2;
				Progress = (num2 = Progress + Time.deltaTime * Spine.timeScale);
				if (!(num2 < Duration))
				{
					break;
				}
				Vector3 a = Vector3.Lerp(StartPosition, Curve, Progress / Duration);
				Vector3 b = Vector3.Lerp(Curve, TargetPosition, Progress / Duration);
				base.transform.position = Vector3.Lerp(a, b, Progress / Duration);
				yield return null;
			}
			TargetPosition.z = 0f;
			base.transform.position = TargetPosition;
			Spine.transform.localPosition = Vector3.zero;
			Spine.AnimationState.SetAnimation(0, "land", false);
			AudioManager.Instance.PlayOneShot("event:/enemy/patrol_worm/patrol_worm_land", base.transform.position);
			CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
			AoEParticles.Play();
			health.invincible = false;
			float time2 = 0f;
			while (true)
			{
				float num2;
				time2 = (num2 = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num2 < 0.15f))
				{
					break;
				}
				yield return null;
			}
			damageColliderEvents.SetActive(true);
			time2 = 0f;
			while (true)
			{
				float num2;
				time2 = (num2 = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num2 < 0.15f))
				{
					break;
				}
				yield return null;
			}
			damageColliderEvents.SetActive(false);
			if (i >= NumberOfDives - 1)
			{
				continue;
			}
			while (true)
			{
				float num2;
				time2 = (num2 = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num2 < 0.2f))
				{
					break;
				}
				yield return null;
			}
		}
		DiveDelayTimer = DiveDelay;
		state.CURRENT_STATE = StateMachine.State.Idle;
		StartCoroutine(ActiveRoutine());
	}

	private void GetTailPieces()
	{
		TailPieces = new List<FollowAsTail>(GetComponentsInChildren<FollowAsTail>());
	}

	private void SetTailPieces()
	{
		int num = -1;
		while (++num < TailPieces.Count)
		{
			if (num == 0)
			{
				TailPieces[num].FollowObject = Spine.transform;
			}
			else
			{
				TailPieces[num].FollowObject = TailPieces[num - 1].transform;
			}
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEvents.SetActive(false);
		}
		StartCoroutine(ActiveRoutine());
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent -= OnDamageTriggerEnter;
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
		Utils.DrawCircleXY(base.transform.position, AttackTriggerRange, Color.white);
		if (!Application.isPlaying)
		{
			int num = -1;
			while (++num < PatrolRoute.Count)
			{
				if (num == PatrolRoute.Count - 1 || num == 0)
				{
					Utils.DrawLine(base.transform.position, base.transform.position + PatrolRoute[num], Color.yellow);
				}
				if (num > 0)
				{
					Utils.DrawLine(base.transform.position + PatrolRoute[num - 1], base.transform.position + PatrolRoute[num], Color.yellow);
				}
				Utils.DrawCircleXY(base.transform.position + PatrolRoute[num], 0.2f, Color.yellow);
			}
			return;
		}
		int num2 = -1;
		while (++num2 < PatrolRoute.Count)
		{
			if (num2 == PatrolRoute.Count - 1 || num2 == 0)
			{
				Utils.DrawLine(StartPosition, StartPosition + PatrolRoute[num2], Color.yellow);
			}
			if (num2 > 0)
			{
				Utils.DrawLine(StartPosition + PatrolRoute[num2 - 1], StartPosition + PatrolRoute[num2], Color.yellow);
			}
			Utils.DrawCircleXY(StartPosition + PatrolRoute[num2], 0.2f, Color.yellow);
		}
	}
}
