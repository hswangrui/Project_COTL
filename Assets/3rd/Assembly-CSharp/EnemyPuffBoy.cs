using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPuffBoy : UnitObject
{
	public float SeperationRadius = 0.5f;

	private GameObject TargetObject;

	public float Range = 6f;

	public float KnockbackSpeed = 0.6f;

	public Collider2D DamageCollider;

	private List<Collider2D> collider2DList;

	private Health EnemyHealth;

	public SimpleSpineAnimator simpleSpineAnimator;

	public GameObject SpawnSlime;

	private GameObject SlimeChild;

	private float SpawnDelay;

	private Coroutine ChasePlayerCoroutine;

	private float StartSpeed = 0.4f;

	private void Start()
	{
		StartCoroutine(WaitForTarget());
		SeperateObject = true;
	}

	public override void OnEnable()
	{
		base.OnEnable();
	}

	public override void OnDisable()
	{
		base.OnDisable();
		ClearPaths();
		StopAllCoroutines();
	}

	private IEnumerator WaitForTarget()
	{
		while (TargetObject == null)
		{
			TargetObject = GameObject.FindWithTag("Player");
			yield return null;
		}
		while (Vector3.Distance(TargetObject.transform.position, base.transform.position) > Range)
		{
			yield return null;
		}
		ChasePlayerCoroutine = StartCoroutine(ChasePlayer());
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (AttackType == Health.AttackTypes.Projectile)
		{
			simpleSpineAnimator.FlashFillRed();
			knockBackVX = (0f - KnockbackSpeed) * Mathf.Cos(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
			knockBackVY = (0f - KnockbackSpeed) * Mathf.Sin(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
			CameraManager.shakeCamera(0.3f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
			GameManager.GetInstance().HitStop();
			simpleSpineAnimator.Animate("hurt", 0, false);
			simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		}
		else
		{
			simpleSpineAnimator.FlashFillRed();
			CameraManager.shakeCamera(0.3f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
			if (state.CURRENT_STATE != StateMachine.State.SignPostAttack)
			{
				state.CURRENT_STATE = StateMachine.State.SignPostAttack;
				simpleSpineAnimator.Animate("attack", 0, false);
				simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
			}
		}
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (state.CURRENT_STATE != StateMachine.State.Dieing)
		{
			knockBackVX = (0f - KnockbackSpeed) * 1f * Mathf.Cos(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
			knockBackVY = (0f - KnockbackSpeed) * 1f * Mathf.Sin(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
			CameraManager.shakeCamera(0.5f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		}
	}

	private IEnumerator ChasePlayer()
	{
		state.CURRENT_STATE = StateMachine.State.Idle;
		float RepathTimer = 0f;
		bool Loop = true;
		while (Loop)
		{
			if (TargetObject == null)
			{
				StartCoroutine(WaitForTarget());
				break;
			}
			Seperate(SeperationRadius);
			switch (state.CURRENT_STATE)
			{
			case StateMachine.State.Idle:
				if (Vector2.Distance(base.transform.position, TargetObject.transform.position) < Range)
				{
					givePath(TargetObject.transform.position);
				}
				break;
			case StateMachine.State.Moving:
			{
				if (Vector2.Distance(base.transform.position, TargetObject.transform.position) < 1f)
				{
					state.CURRENT_STATE = StateMachine.State.SignPostAttack;
					simpleSpineAnimator.Animate("attack", 0, false);
					simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
					break;
				}
				if (DataManager.Instance.PLAYER_ARROW_AMMO <= 0 && SlimeChild == null && (SpawnDelay -= Time.deltaTime) < 0f)
				{
					SlimeChild = UnityEngine.Object.Instantiate(SpawnSlime, base.transform.position, Quaternion.identity, base.transform.parent);
					SpawnDelay = 3f;
				}
				float num;
				RepathTimer = (num = RepathTimer + Time.deltaTime);
				if (num > 0.2f)
				{
					RepathTimer = 0f;
					givePath(TargetObject.transform.position);
				}
				break;
			}
			case StateMachine.State.SignPostAttack:
				state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
				if (Time.frameCount % 5 == 0)
				{
					simpleSpineAnimator.FlashWhite(!simpleSpineAnimator.isFillWhite);
				}
				if (!((state.Timer += Time.deltaTime) >= 0.25f))
				{
					break;
				}
				state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
				simpleSpineAnimator.FlashWhite(false);
				CameraManager.shakeCamera(0.2f, Utils.GetAngle(base.transform.position, TargetObject.transform.position));
				collider2DList = new List<Collider2D>();
				DamageCollider.GetContacts(collider2DList);
				foreach (Collider2D collider2D in collider2DList)
				{
					EnemyHealth = collider2D.gameObject.GetComponent<Health>();
					if (EnemyHealth != null && EnemyHealth.team != health.team)
					{
						EnemyHealth.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, EnemyHealth.transform.position, 0.7f));
					}
				}
				break;
			case StateMachine.State.RecoverFromAttack:
				if ((state.Timer += Time.deltaTime) >= 2f)
				{
					state.CURRENT_STATE = StateMachine.State.Idle;
				}
				break;
			}
			yield return null;
		}
	}
}
