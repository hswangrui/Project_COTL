using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using Spine.Unity.Modules;
using UnityEngine;

public class EnemySimpleChaser_SmallWorm : UnitObject
{
	private List<Collider2D> collider2DList;

	public ColliderEvents damageColliderEvents;

	private Health EnemyHealth;

	private GameObject TargetObject;

	public float DetectEnemyRange = 8f;

	private float RepathTimer;

	public float SeperationRadius = 0.4f;

	private bool SetStartPosition;

	private Vector3 StartPosition = Vector3.one * 2.1474836E+09f;

	private float Delay;

	public SpriteRenderer spriteRenderer;

	public Material wormMaterial;

	private int colorID;

	private float maxSpeedIncrease;

	private SimpleSpineFlash SimpleSpineFlash;

	public SkeletonAnimation Spine;

	public SkeletonUtilityEyeConstraint skeletonUtilityEyeConstraint;

	[SpineSkin("", "", true, false, false, dataField = "Spine")]
	public string Head;

	[SpineSkin("", "", true, false, false, dataField = "Spine")]
	public string HeadFacingUp;

	private bool FacingUp;

	private float tailDistance;

	private float headDistance;

	public float shakeDuration = 0.5f;

	public Vector3 shakeStrength = new Vector3(0.5f, 0.5f, 0.01f);

	[Range(0f, 30f)]
	public int vibrato = 10;

	[Range(0f, 180f)]
	public float randomness = 90f;

	public float AttackSpeed = 0.5f;

	private bool reacted;

	public List<FollowAsTail> TailPieces = new List<FollowAsTail>();

	private void LateUpdate()
	{
		if (state.facingAngle > 45f && state.facingAngle < 135f)
		{
			if (!FacingUp)
			{
				Spine.skeleton.SetSkin(HeadFacingUp);
				Spine.skeleton.SetSlotsToSetupPose();
				FacingUp = true;
			}
		}
		else if (FacingUp)
		{
			Spine.skeleton.SetSkin(Head);
			Spine.skeleton.SetSlotsToSetupPose();
			FacingUp = false;
		}
	}

	private void Start()
	{
		maxSpeedIncrease = maxSpeed * 3f;
		SimpleSpineFlash = GetComponentInChildren<SimpleSpineFlash>();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (SetStartPosition)
		{
			base.transform.position = StartPosition;
		}
		else
		{
			StartPosition = base.transform.position;
			SetStartPosition = true;
		}
		Delay = 0f;
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEvents.SetActive(true);
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		TargetObject = null;
		Delay = 0f;
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		base.OnHit(Attacker, AttackLocation, AttackType);
		knockBackVX = -1f * Mathf.Cos(Utils.GetAngle(base.transform.position, AttackLocation) * ((float)Math.PI / 180f));
		knockBackVY = -1f * Mathf.Sin(Utils.GetAngle(base.transform.position, AttackLocation) * ((float)Math.PI / 180f));
		UsePathing = true;
		health.invincible = false;
		StopAllCoroutines();
		if (AttackLocation.x > base.transform.position.x && state.CURRENT_STATE != StateMachine.State.HitRight)
		{
			state.CURRENT_STATE = StateMachine.State.HitRight;
		}
		if (AttackLocation.x < base.transform.position.x && state.CURRENT_STATE != StateMachine.State.HitLeft)
		{
			state.CURRENT_STATE = StateMachine.State.HitLeft;
		}
		StartCoroutine(HurtRoutine());
	}

	private IEnumerator HurtRoutine()
	{
		Spine.AnimationState.SetAnimation(0, "attack-impact", true);
		Spine.AnimationState.AddAnimation(0, "animation", true, 0f);
		Spine.GetComponent<SimpleSpineFlash>().FlashFillRed();
		spriteRenderer.color = Color.red;
		foreach (FollowAsTail tailPiece in TailPieces)
		{
			tailPiece.GetComponent<SpriteRenderer>().color = Color.red;
		}
		yield return new WaitForSeconds(0.2f);
		spriteRenderer.color = Color.white;
		foreach (FollowAsTail tailPiece2 in TailPieces)
		{
			tailPiece2.GetComponent<SpriteRenderer>().color = Color.white;
		}
		state.CURRENT_STATE = StateMachine.State.Moving;
	}

	public override void Update()
	{
		base.Update();
		if ((Delay -= Time.deltaTime) > 0f)
		{
			return;
		}
		if (damageColliderEvents != null)
		{
			damageColliderEvents.SetActive(true);
		}
		if (TargetObject != null)
		{
			switch (state.CURRENT_STATE)
			{
			case StateMachine.State.Idle:
				if (Vector3.Distance(base.transform.position, TargetObject.transform.position) < 8f)
				{
					givePath(TargetObject.transform.position);
				}
				break;
			case StateMachine.State.Moving:
				if (!reacted)
				{
					Spine.AnimationState.SetAnimation(0, "notice-player", true);
					Spine.AnimationState.AddAnimation(0, "animation", true, 0f);
					reacted = true;
				}
				skeletonUtilityEyeConstraint.targetPosition = TargetObject.transform.position + Vector3.back * 0.5f;
				if ((RepathTimer += Time.deltaTime) > 0.5f)
				{
					RepathTimer = 0f;
					givePath(TargetObject.transform.position);
				}
				break;
			case StateMachine.State.SignPostAttack:
				if (SimpleSpineFlash != null)
				{
					SimpleSpineFlash.FlashWhite(state.Timer / 0.5f);
				}
				if ((state.Timer += Time.deltaTime) > 0.5f)
				{
					speed = AttackSpeed;
					if (SimpleSpineFlash != null)
					{
						SimpleSpineFlash.FlashWhite(false);
					}
					state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
				}
				break;
			case StateMachine.State.RecoverFromAttack:
				if (damageColliderEvents != null)
				{
					damageColliderEvents.SetActive(true);
				}
				if ((state.Timer += Time.deltaTime) > 1f)
				{
					state.CURRENT_STATE = StateMachine.State.Idle;
				}
				break;
			}
			int num = -1;
			while (++num < TailPieces.Count)
			{
				TailPieces[num].UpdatePosition();
			}
		}
		else
		{
			GetNewTarget();
		}
		spriteRenderer.transform.localScale = new Vector3((!(state.facingAngle < 90f) || !(state.facingAngle > -90f)) ? 1 : (-1), 1f, 1f);
		if (SeperateObject)
		{
			Seperate(SeperationRadius, true);
		}
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

	public void GetNewTarget()
	{
		Health health = null;
		float num = float.MaxValue;
		foreach (Health allUnit in Health.allUnits)
		{
			if (allUnit.team != base.health.team && !allUnit.InanimateObject && allUnit.team != 0 && (base.health.team != Health.Team.PlayerTeam || (base.health.team == Health.Team.PlayerTeam && allUnit.team != Health.Team.DangerousAnimals)) && Vector2.Distance(base.transform.position, allUnit.gameObject.transform.position) < DetectEnemyRange && CheckLineOfSight(allUnit.gameObject.transform.position, Vector2.Distance(allUnit.gameObject.transform.position, base.transform.position)))
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
			EnemyHealth.attackers.Add(base.gameObject);
		}
	}

	private void OnDamageTriggerEnter(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && component.team != health.team)
		{
			component.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.7f));
		}
	}
}
