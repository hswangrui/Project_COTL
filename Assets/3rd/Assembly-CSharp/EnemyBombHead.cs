using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBombHead : UnitObject
{
	public float ExplosionDelay = 0.5f;

	public float SeperationRadius = 0.5f;

	public SimpleSpineAnimator simpleSpineAnimator;

	public SpriteRenderer sprite;

	private const string SHADER_COLOR_NAME = "_Color";

	private Rigidbody2D rb2D;

	private GameObject TargetObject;

	public float Range = 6f;

	public float KnockbackSpeed = 1f;

	public float ExplosionRadius = 1f;

	public List<GameObject> ToSpawn = new List<GameObject>();

	public bool ExplodeOnDeath = true;

	public bool ExplodeToAttack = true;

	private static List<EnemySlime> Slimes = new List<EnemySlime>();

	private float AttackSpeed;

	private Coroutine ChasePlayerCoroutine;

	private float StartSpeed = 0.4f;

	private float Angle;

	public float WhiteFade;

	private List<Collider2D> collider2DList;

	public Collider2D DamageCollider;

	private Health EnemyHealth;

	private void Start()
	{
		StartCoroutine(WaitForTarget());
		rb2D = GetComponent<Rigidbody2D>();
		SeperateObject = true;
	}

	public override void OnEnable()
	{
		base.OnEnable();
	}

	public override void OnDisable()
	{
		base.OnDisable();
	}

	private IEnumerator WaitForTarget()
	{
		float Timer = 0f;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (num < 0.5f)
			{
				Seperate(SeperationRadius);
				yield return null;
				continue;
			}
			break;
		}
		while (TargetObject == null)
		{
			TargetObject = GameObject.FindWithTag("Player");
			yield return null;
		}
		while (MagnitudeFindDistanceBetween(TargetObject.transform.position, base.transform.position) > Range * Range)
		{
			yield return null;
		}
		ChasePlayerCoroutine = StartCoroutine(ChasePlayer());
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		CameraManager.shakeCamera(0.3f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		knockBackVX = (0f - KnockbackSpeed) * Mathf.Cos(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
		knockBackVY = (0f - KnockbackSpeed) * Mathf.Sin(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
		ClearPaths();
		if (ChasePlayerCoroutine != null)
		{
			StopCoroutine(ChasePlayerCoroutine);
		}
		StartCoroutine(AddForce());
		BiomeConstants.Instance.EmitHitVFX(AttackLocation - Vector3.back * 0.5f, Quaternion.identity.z, "HitFX_Weak");
	}

	private IEnumerator AddForce()
	{
		simpleSpineAnimator.FlashFillRed();
		yield return new WaitForSeconds(0.5f);
		ChasePlayerCoroutine = StartCoroutine(ChasePlayer());
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (state.CURRENT_STATE != StateMachine.State.Dieing)
		{
			Angle = Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f);
			CameraManager.shakeCamera(ExplodeOnDeath ? 0.5f : 0.2f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
			ClearPaths();
			StopAllCoroutines();
			StartCoroutine(ExplodeAndSpawn());
		}
	}

	private IEnumerator ExplodeAndSpawn()
	{
		yield return new WaitForSeconds(0.05f);
		float angle = Angle;
		Health health = null;
		float num = float.MaxValue;
		foreach (Health allUnit in Health.allUnits)
		{
			float num2 = Utils.GetAngle(allUnit.transform.position, base.transform.position) * ((float)Math.PI / 180f);
			if (allUnit != base.health && allUnit.team != Health.Team.PlayerTeam && Mathf.Abs(angle - num2) < (float)Math.PI / 2f)
			{
				float num3 = MagnitudeFindDistanceBetween(base.transform.position, allUnit.transform.position);
				if (num3 < num * num && num3 < 10f)
				{
					health = allUnit;
					num = Mathf.Sqrt(num3);
				}
			}
		}
		if (health != null)
		{
			Angle = Utils.GetAngle(health.transform.position, base.transform.position) * ((float)Math.PI / 180f);
		}
		knockBackVX = (0f - KnockbackSpeed) * 1.5f * Mathf.Cos(Angle);
		knockBackVY = (0f - KnockbackSpeed) * 1.5f * Mathf.Sin(Angle);
		state.CURRENT_STATE = StateMachine.State.Dieing;
		if (ExplodeOnDeath)
		{
			while ((state.Timer += Time.deltaTime) < 0.5f)
			{
				if (Time.frameCount % 5 == 0)
				{
					simpleSpineAnimator.FlashWhite(simpleSpineAnimator.isFillWhite = ((!simpleSpineAnimator.isFillWhite) ? true : false));
				}
				yield return null;
			}
			Explosion.CreateExplosion(base.transform.position, Health.Team.PlayerTeam, base.health, ExplosionRadius, 10f);
		}
		SpawnChildren();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void SpawnChildren()
	{
		int num = -1;
		while (++num < ToSpawn.Count)
		{
			StateMachine component = UnityEngine.Object.Instantiate(ToSpawn[num], base.transform.position, Quaternion.identity, base.transform.parent).GetComponent<StateMachine>();
			component.facingAngle = state.facingAngle + 90f + (float)(360 / ToSpawn.Count * num);
			Debug.Log(component.facingAngle);
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
			if (state.CURRENT_STATE != StateMachine.State.RecoverFromAttack)
			{
				Seperate(SeperationRadius);
			}
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
				if (Time.frameCount % 5 == 0)
				{
					simpleSpineAnimator.FlashWhite(simpleSpineAnimator.isFillWhite = ((!simpleSpineAnimator.isFillWhite) ? true : false));
				}
				if (Vector2.Distance(base.transform.position, TargetObject.transform.position) < 1f)
				{
					state.CURRENT_STATE = StateMachine.State.SignPostAttack;
					break;
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
					simpleSpineAnimator.FlashWhite(simpleSpineAnimator.isFillWhite = ((!simpleSpineAnimator.isFillWhite) ? true : false));
				}
				if ((state.Timer += Time.deltaTime) >= ExplosionDelay)
				{
					simpleSpineAnimator.FlashWhite(false);
					if (ExplodeToAttack)
					{
						Explosion.CreateExplosion(base.transform.position, health.team, health, ExplosionRadius, 1f);
						SpawnChildren();
						UnityEngine.Object.Destroy(base.gameObject);
					}
					else
					{
						state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
						CameraManager.shakeCamera(0.2f, state.facingAngle);
						state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
						SeperateObject = false;
						AttackSpeed = 0.75f;
					}
				}
				break;
			case StateMachine.State.RecoverFromAttack:
				if (AttackSpeed > 0f)
				{
					WhiteFade = Mathf.Lerp(1f, 0f, 1f - AttackSpeed / 0.75f);
					simpleSpineAnimator.FillColor(Color.white, WhiteFade);
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
					AttackSpeed -= 3f * Time.deltaTime;
					speed = AttackSpeed;
				}
				if ((state.Timer += Time.deltaTime) >= 1f)
				{
					SeperateObject = true;
					state.CURRENT_STATE = StateMachine.State.Idle;
					sprite.material.SetColor("_Color", new Color(1f, 1f, 1f, 0f));
				}
				break;
			}
			yield return null;
		}
	}

	private float MagnitudeFindDistanceBetween(Vector3 a, Vector3 b)
	{
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		float num3 = a.z - b.z;
		return num * num + num2 * num2 + num3 * num3;
	}
}
