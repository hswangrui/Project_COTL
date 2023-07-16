using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySimpleChaser_Cave : UnitObject
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

	public SpriteRenderer spriteRenderer;

	public Material wormMaterial;

	private int colorID;

	private float maxSpeedIncrease;

	private SimpleSpineFlash SimpleSpineFlash;

	private Color wormColor;

	public float AttackSpeed = 0.5f;

	private void Start()
	{
		maxSpeedIncrease = maxSpeed * 3f;
		SeperateObject = true;
		colorID = Shader.PropertyToID("_GlowColour");
		wormMaterial.SetColor(colorID, Color.blue);
		SimpleSpineFlash = GetComponentInChildren<SimpleSpineFlash>();
	}

	public override void OnEnable()
	{
		wormMaterial.SetColor(colorID, Color.blue);
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
	}

	public override void OnDisable()
	{
		base.OnDisable();
		TargetObject = null;
		Delay = 0f;
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		wormMaterial.SetColor(colorID, Color.red);
		base.OnHit(Attacker, AttackLocation, AttackType);
		knockBackVX = -0.5f * Mathf.Cos(Utils.GetAngle(base.transform.position, AttackLocation) * ((float)Math.PI / 180f));
		knockBackVY = -0.5f * Mathf.Sin(Utils.GetAngle(base.transform.position, AttackLocation) * ((float)Math.PI / 180f));
		SeperateObject = true;
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
		yield return new WaitForSeconds(0.3f);
		state.CURRENT_STATE = StateMachine.State.Moving;
	}

	public override void Update()
	{
		base.Update();
		if ((Delay -= Time.deltaTime) > 0f)
		{
			return;
		}
		if (wormMaterial.GetColor(colorID) == Color.red && maxSpeed != maxSpeedIncrease)
		{
			maxSpeed = maxSpeedIncrease;
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
				if ((RepathTimer += Time.deltaTime) > 0.5f)
				{
					RepathTimer = 0f;
					givePath(TargetObject.transform.position);
				}
				else if (Vector3.Distance(base.transform.position, TargetObject.transform.position) < 2f)
				{
					state.CURRENT_STATE = StateMachine.State.SignPostAttack;
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
				if (state.Timer < 0.2f)
				{
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
				}
				if ((state.Timer += Time.deltaTime) > 1f)
				{
					state.CURRENT_STATE = StateMachine.State.Idle;
				}
				break;
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
}
