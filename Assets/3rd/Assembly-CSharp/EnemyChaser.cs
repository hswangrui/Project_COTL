using FMODUnity;
using Spine.Unity;
using UnityEngine;

public class EnemyChaser : UnitObject
{
	[SerializeField]
	protected SimpleSpineFlash simpleSpineFlash;

	[SerializeField]
	protected ColliderEvents damageColliderEvents;

	[EventRef]
	public string onHitSoundPath = string.Empty;

	[EventRef]
	public string onDeathSoundPath = string.Empty;

	[Space]
	[SerializeField]
	private float checkPlayerInterval;

	[SerializeField]
	private float targetTrackingOffset;

	[SerializeField]
	private float directTargetDistance;

	[SerializeField]
	private bool canLoseRange;

	[SerializeField]
	private bool damageOnTouch;

	[SerializeField]
	protected float knockbackMultiplier;

	protected Health targetObject;

	protected GameManager gm;

	protected bool inRange;

	private Vector3 offset;

	private float checkPlayerTimestamp;

	protected SkeletonAnimation spine;

	public override void Awake()
	{
		base.Awake();
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEvents.SetActive(false);
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		offset = Random.insideUnitCircle * targetTrackingOffset;
		damageColliderEvents.SetActive(damageOnTouch);
	}

	protected virtual void Start()
	{
		if (gm == null)
		{
			gm = GameManager.GetInstance();
		}
	}

	public override void Update()
	{
		base.Update();
		if (gm == null)
		{
			gm = GameManager.GetInstance();
		}
		if (targetObject == null)
		{
			targetObject = GetClosestTarget();
		}
		if (inRange)
		{
			UpdateMoving();
		}
		if (targetObject != null)
		{
			float num = Vector3.Distance(base.transform.position, targetObject.transform.position);
			if (num < directTargetDistance)
			{
				offset = Vector3.zero;
			}
			else if (offset == Vector3.zero)
			{
				offset = Random.insideUnitCircle * targetTrackingOffset;
			}
			if (!inRange || (inRange && canLoseRange))
			{
				inRange = num < (float)VisionRange;
			}
		}
	}

	protected virtual void UpdateMoving()
	{
		if (gm.TimeSince(checkPlayerTimestamp) > checkPlayerInterval / spine.timeScale && GameManager.RoomActive)
		{
			targetObject = GetClosestTarget();
			if (targetObject != null)
			{
				checkPlayerTimestamp = gm.CurrentTime + checkPlayerInterval;
				givePath(targetObject.transform.position + offset);
				float angle = Utils.GetAngle(base.transform.position, targetObject.transform.position + offset);
				LookAtAngle(angle);
			}
		}
	}

	protected void LookAtAngle(float angle)
	{
		state.facingAngle = angle;
		state.LookAngle = angle;
	}

	protected virtual void OnDamageTriggerEnter(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && (component.team != health.team || health.team == Health.Team.PlayerTeam))
		{
			component.DealDamage((component.team == Health.Team.PlayerTeam) ? 1 : int.MaxValue, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.7f));
		}
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		if (!string.IsNullOrEmpty(onDeathSoundPath))
		{
			AudioManager.Instance.PlayOneShot(onDeathSoundPath, base.transform.position);
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		simpleSpineFlash.FlashFillRed();
		targetObject = null;
		if (knockbackMultiplier != 0f)
		{
			DoKnockBack(Attacker, knockbackMultiplier, 1f);
		}
		if (!string.IsNullOrEmpty(onHitSoundPath))
		{
			AudioManager.Instance.PlayOneShot(onHitSoundPath, base.transform.position);
		}
	}
}
