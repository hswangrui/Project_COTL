using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using UnityEngine;

public class EnemyMillipede : UnitObject
{
	public SkeletonAnimation Spine;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string idleAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string movingAnimation;

	[SerializeField]
	protected float rayDistance;

	[SerializeField]
	protected float turnDamper;

	[SerializeField]
	private LayerMask avoidLayers;

	[SerializeField]
	private float knockModifier;

	[SerializeField]
	private Vector2 turnExaggerationRange = new Vector2(30f, 120f);

	[SerializeField]
	private float invincibleTime;

	private Health[] bodyParts;

	protected List<SimpleSpineFlash> flashes;

	protected List<SkeletonAnimation> spines;

	private ColliderEvents[] colliderEvents;

	protected bool focusOnTarget;

	private bool damaged;

	private float directionChanger;

	private float directionRand = 1f;

	private float subtleOffset;

	public override void Awake()
	{
		base.Awake();
		bodyParts = GetComponentsInChildren<Health>();
		spines = GetComponentsInChildren<SkeletonAnimation>().ToList();
		flashes = GetComponentsInChildren<SimpleSpineFlash>().ToList();
		colliderEvents = GetComponentsInChildren<ColliderEvents>(true);
		ColliderEvents[] array = colliderEvents;
		foreach (ColliderEvents obj in array)
		{
			obj.OnTriggerEnterEvent += OnDamageTriggerEnter;
			obj.gameObject.SetActive(false);
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		foreach (SimpleSpineFlash flash in flashes)
		{
			flash.FlashWhite(false);
		}
		foreach (SkeletonAnimation spine in spines)
		{
			if ((bool)spine)
			{
				spine.AnimationState.SetAnimation(0, "run", true);
			}
		}
		Health[] array = bodyParts;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].invincible = false;
		}
		DisableForces = false;
	}

	protected virtual void Start()
	{
		LookAtAngle(Random.Range(0, 360));
	}

	public override void Update()
	{
		if (!DisableForces)
		{
			speed = maxSpeed * SpeedMultiplier;
			if (Time.time > directionChanger / Spine.timeScale)
			{
				directionChanger = Time.time + Random.Range(0.5f, 1.5f);
				directionRand *= -1f;
			}
			RaycastHit2D raycastHit2D;
			RaycastHit2D raycastHit2D2 = (raycastHit2D = Physics2D.Raycast(base.transform.position, new Vector2(moveVX, moveVY).normalized, rayDistance, avoidLayers));
			if (raycastHit2D2.collider != null)
			{
				float angle = Utils.GetAngle(Vector3.zero, new Vector2(moveVX, moveVY));
				float degree = Mathf.Repeat(angle + 90f, 360f);
				float degree2 = Mathf.Repeat(angle - 90f, 360f);
				RaycastHit2D raycastHit2D3 = Physics2D.Raycast(base.transform.position, Utils.DegreeToVector2(degree).normalized, rayDistance / 2f, avoidLayers);
				RaycastHit2D raycastHit2D4 = Physics2D.Raycast(base.transform.position, Utils.DegreeToVector2(degree2).normalized, rayDistance / 2f, avoidLayers);
				float num = ((raycastHit2D3.collider != null) ? Vector3.Distance(base.transform.position, raycastHit2D3.point) : float.PositiveInfinity);
				float num2 = ((raycastHit2D4.collider != null) ? Vector3.Distance(base.transform.position, raycastHit2D4.point) : float.PositiveInfinity);
				if (float.IsNaN(state.facingAngle))
				{
					state.facingAngle = angle;
				}
				float num3 = ((num >= num2) ? 0.5f : (-0.5f));
				if (num == num2)
				{
					num3 = directionRand;
				}
				float targetAngle = Mathf.Repeat(state.facingAngle + Random.Range(turnExaggerationRange.x, turnExaggerationRange.y), 360f);
				if (((raycastHit2D3.collider == null && raycastHit2D4.collider == null) || focusOnTarget) && GetClosestTarget() != null)
				{
					targetAngle = Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position);
					num3 = 1f;
				}
				float num4 = Vector3.Distance(base.transform.position, raycastHit2D.point);
				float angle2 = Utils.SmoothAngle(state.facingAngle, targetAngle, turnDamper * num4 * num3);
				LookAtAngle(angle2);
			}
			else
			{
				subtleOffset = Mathf.PingPong(Time.time, 4f) - 2f;
				LookAtAngle(state.facingAngle + subtleOffset);
			}
		}
		base.Update();
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/spider/gethit", base.gameObject);
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		foreach (SimpleSpineFlash flash in flashes)
		{
			flash.FlashFillRed();
		}
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/spider/death", base.gameObject);
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		Health[] array = bodyParts;
		foreach (Health health in array)
		{
			if (health != base.health)
			{
				health.enabled = true;
				health.DamageModifier = 1f;
				health.DealDamage(health.totalHP, base.gameObject, AttackLocation, false, Health.AttackTypes.Heavy);
			}
		}
	}

	protected void LookAtAngle(float angle)
	{
		state.facingAngle = angle;
		state.LookAngle = angle;
	}

	protected void LookAtTarget()
	{
		if ((bool)GetClosestTarget())
		{
			float angle = Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position);
			LookAtAngle(angle);
		}
	}

	public void EnableDamageColliders()
	{
		StartCoroutine(EnableDamageCollidersIE());
	}

	private IEnumerator EnableDamageCollidersIE()
	{
		ColliderEvents[] array = colliderEvents;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(true);
		}
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.1f))
			{
				break;
			}
			yield return null;
		}
		array = colliderEvents;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(false);
		}
	}

	protected virtual void OnDamageTriggerEnter(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && (component.team != health.team || health.team == Health.Team.PlayerTeam) && !bodyParts.Contains(component))
		{
			component.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.7f));
		}
	}

	protected void SetAnimation(string animationName, bool loop = false)
	{
		foreach (SkeletonAnimation spine in spines)
		{
			spine.AnimationState.SetAnimation(0, animationName, loop);
		}
	}

	protected void AddAnimation(string animationName, bool loop = false)
	{
		foreach (SkeletonAnimation spine in spines)
		{
			spine.AnimationState.AddAnimation(0, animationName, loop, 0f);
		}
	}

	protected Vector3 GetCenterPosition()
	{
		Vector3 zero = Vector3.zero;
		foreach (SkeletonAnimation spine in spines)
		{
			zero += spine.transform.position;
		}
		return zero / spines.Count;
	}

	public void DamageFromBody(GameObject attacker, Vector3 attackLocation, float damage, Health.AttackTypes attackType, Health.AttackFlags attackFlag)
	{
		if (!damaged)
		{
			health.DealDamage(damage, attacker, attackLocation, false, attackType, false, attackFlag);
			StartCoroutine(InvincibleDelay());
		}
	}

	private IEnumerator InvincibleDelay()
	{
		damaged = true;
		Health[] array = bodyParts;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].invincible = true;
		}
		yield return new WaitForSeconds(invincibleTime);
		damaged = false;
		array = bodyParts;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].invincible = false;
		}
	}
}
