using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySlime : UnitObject
{
	public bool SleepingOnStart;

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

	public bool MoveOnState = true;

	public bool ExplodeOnDeath = true;

	public bool ExplodeToAttack = true;

	public static List<EnemySlime> Slimes = new List<EnemySlime>();

	private float AttackSpeed;

	private Coroutine ChasePlayerCoroutine;

	private float StartSpeed = 0.4f;

	public float WhiteFade;

	private List<Collider2D> collider2DList;

	public Collider2D DamageCollider;

	private Health EnemyHealth;

	public override void Awake()
	{
		base.Awake();
		rb2D = GetComponent<Rigidbody2D>();
		SeperateObject = true;
	}

	private void OnDieAny(Health Victim)
	{
		if (Victim.team == health.team && MagnitudeFindDistanceBetween(Victim.transform.position, base.transform.position) < 16f)
		{
			WarnMe(Victim.transform.position);
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		Slimes.Add(this);
		Health.OnDieAny += OnDieAny;
		if (!SleepingOnStart)
		{
			StartCoroutine(WaitForTarget());
		}
		else
		{
			StartCoroutine(GoToSleep());
		}
	}

	private IEnumerator GoToSleep()
	{
		yield return new WaitForEndOfFrame();
		state.CURRENT_STATE = StateMachine.State.Sleeping;
	}

	public override void OnDisable()
	{
		base.OnDisable();
		Slimes.Remove(this);
		Health.OnDieAny -= OnDieAny;
	}

	private void ScreamToOthers()
	{
		StartCoroutine(DoScremToOthers());
	}

	public void WarnMe(Vector3 Position)
	{
		if (state.CURRENT_STATE == StateMachine.State.Sleeping)
		{
			state.facingAngle = Utils.GetAngle(base.transform.position, Position);
			ScreamToOthers();
		}
	}

	private IEnumerator DoScremToOthers()
	{
		yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.3f));
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		yield return new WaitForSeconds(0.3f);
		foreach (EnemySlime slime in Slimes)
		{
			if (MagnitudeFindDistanceBetween(base.transform.position, slime.transform.position) < 16f)
			{
				slime.WarnMe(base.transform.position);
			}
		}
		yield return new WaitForSeconds(1.2f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		ChasePlayerCoroutine = StartCoroutine(ChasePlayer());
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
				if (MoveOnState && StartSpeed > 0f)
				{
					StartSpeed -= 1f * Time.deltaTime;
					speed = StartSpeed;
				}
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
		if (state.CURRENT_STATE == StateMachine.State.Sleeping)
		{
			simpleSpineAnimator.FlashFillRed();
			ScreamToOthers();
		}
		else
		{
			ClearPaths();
			if (ChasePlayerCoroutine != null)
			{
				StopCoroutine(ChasePlayerCoroutine);
			}
			StartCoroutine(AddForce());
		}
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
		if (state.CURRENT_STATE == StateMachine.State.Dieing)
		{
			return;
		}
		if (state.CURRENT_STATE != StateMachine.State.Sleeping)
		{
			foreach (EnemySlime slime in Slimes)
			{
				if (MagnitudeFindDistanceBetween(base.transform.position, slime.transform.position) < 16f)
				{
					slime.WarnMe(base.transform.position);
				}
			}
		}
		knockBackVX = (0f - KnockbackSpeed) * 1f * Mathf.Cos(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
		knockBackVY = (0f - KnockbackSpeed) * 1f * Mathf.Sin(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
		GameObject obj = BiomeConstants.Instance.GroundSmash_Medium.Spawn();
		obj.transform.position = base.transform.position;
		obj.transform.rotation = Quaternion.identity;
		CameraManager.shakeCamera(ExplodeOnDeath ? 0.5f : 0.2f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		ClearPaths();
		StopAllCoroutines();
		StartCoroutine(ExplodeAndSpawn());
	}

	private IEnumerator ExplodeAndSpawn()
	{
		state.CURRENT_STATE = StateMachine.State.Dieing;
		if (ExplodeOnDeath)
		{
			while ((state.Timer += Time.deltaTime) < 1f)
			{
				if (Time.frameCount % 5 == 0)
				{
					simpleSpineAnimator.FlashWhite(simpleSpineAnimator.isFillWhite = ((!simpleSpineAnimator.isFillWhite) ? true : false));
				}
				yield return null;
			}
			Explosion.CreateExplosion(base.transform.position, health.team, health, ExplosionRadius, 1f);
		}
		SpawnChildren();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void SpawnChildren()
	{
		int num = -1;
		while (++num < ToSpawn.Count)
		{
			GameObject obj = UnityEngine.Object.Instantiate(ToSpawn[num], base.transform.position, Quaternion.identity, base.transform.parent);
			StateMachine component = obj.GetComponent<StateMachine>();
			EnemySlime component2 = obj.GetComponent<EnemySlime>();
			if (component2 != null)
			{
				component2.MoveOnState = true;
			}
			component.facingAngle = state.facingAngle + 90f + (float)(360 / ToSpawn.Count * num);
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
				if (Vector2.Distance(base.transform.position, TargetObject.transform.position) < (ExplodeToAttack ? 1f : 2.5f))
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
				if ((state.Timer += Time.deltaTime) >= (ExplodeToAttack ? 1f : 0.5f))
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
