using System.Collections.Generic;
using Spine.Unity;
using Spine.Unity.Modules;
using UnityEngine;

public class EnemySimpleChaser : UnitObject
{
	private List<Collider2D> collider2DList;

	public Collider2D DamageCollider;

	private Health EnemyHealth;

	private GameObject TargetObject;

	public float DetectEnemyRange = 8f;

	private float RepathTimer;

	public float SeperationRadius = 0.4f;

	private bool SetStartPosition;

	private Vector3 StartPosition = Vector3.one * 2.1474836E+09f;

	private float Delay;

	public Transform scaleMe;

	public SimpleSpineFlash SimpleSpineFlash;

	public float DamageRadius = 2f;

	public float AttackTriggerRange = 1f;

	public GameObject WarningObject;

	public float AttackCooldown = 0.5f;

	public float AttackCooldownTimer;

	public ParticleSystem particleSystem;

	public SkeletonAnimation Spine;

	public SkeletonUtilityEyeConstraint skeletonUtilityEyeConstraint;

	private float AttackSpeed = 0.7f;

	public List<FollowAsTail> TailPieces = new List<FollowAsTail>();

	[SpineSkin("", "", true, false, false, dataField = "Spine")]
	public string Head;

	[SpineSkin("", "", true, false, false, dataField = "Spine")]
	public string HeadFacingUp;

	private bool FacingUp;

	private void Start()
	{
		SeperateObject = true;
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
		WarningObject.SetActive(false);
		particleSystem.Stop();
	}

	public override void OnDisable()
	{
		base.OnDisable();
		TargetObject = null;
		Delay = 0f;
	}

	public override void Update()
	{
		base.Update();
		if ((Delay -= Time.deltaTime) > 0f)
		{
			return;
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
				AttackCooldown -= Time.deltaTime;
				skeletonUtilityEyeConstraint.targetPosition = TargetObject.transform.position + Vector3.back * 0.5f;
				if ((RepathTimer += Time.deltaTime) > 0.5f)
				{
					RepathTimer = 0f;
					givePath(TargetObject.transform.position);
				}
				else if (AttackCooldown < 0f && Vector3.Distance(base.transform.position, TargetObject.transform.position) < AttackTriggerRange)
				{
					WarningObject.SetActive(true);
					state.CURRENT_STATE = StateMachine.State.SignPostAttack;
				}
				break;
			case StateMachine.State.SignPostAttack:
				SimpleSpineFlash.FlashWhite(state.Timer / 0.5f);
				if ((state.Timer += Time.deltaTime) > 0.5f)
				{
					SimpleSpineFlash.FlashWhite(false);
					WarningObject.SetActive(false);
					particleSystem.Play();
					CameraManager.shakeCamera(0.4f);
					state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
				}
				break;
			case StateMachine.State.RecoverFromAttack:
				if (state.Timer > 0.05f && state.Timer < 0.2f)
				{
					int num = -1;
					while (++num < Health.allUnits.Count)
					{
						Health health = Health.allUnits[num];
						if (health != null && health.team != base.health.team && Vector3.Distance(health.transform.position, base.transform.position) < DamageRadius)
						{
							health.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, health.transform.position, 0.7f));
						}
					}
				}
				if ((state.Timer += Time.deltaTime) > 1f)
				{
					state.CURRENT_STATE = StateMachine.State.Moving;
					AttackCooldownTimer = 0.5f;
				}
				break;
			}
		}
		else
		{
			GetNewTarget();
		}
		Spine.skeleton.ScaleX = ((state.facingAngle > 90f && state.facingAngle < 270f) ? 1 : (-1));
		if (SeperateObject)
		{
			Seperate(SeperationRadius, true);
		}
		int num2 = -1;
		while (++num2 < TailPieces.Count)
		{
			TailPieces[num2].UpdatePosition();
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

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, AttackTriggerRange, Color.white);
		Utils.DrawCircleXY(base.transform.position, DamageRadius, Color.red);
	}
}
