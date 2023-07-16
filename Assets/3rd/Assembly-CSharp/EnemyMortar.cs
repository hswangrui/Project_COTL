using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMortar : UnitObject
{
	public GameObject ThrowBone;

	public GameObject RockToThrow;

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

	private SimpleSpineEventListener simpleSpineEventListener;

	private BruteRock b;

	private Coroutine ChasePlayerCoroutine;

	private float StartSpeed = 0.4f;

	private bool Thrown;

	private GameObject g;

	private bool Attacked;

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
		simpleSpineEventListener = GetComponent<SimpleSpineEventListener>();
		simpleSpineEventListener.OnSpineEvent += OnSpineEvent;
	}

	private void OnSpineEvent(string EventName)
	{
		switch (EventName)
		{
		case "shoot":
			if (simpleSpineAnimator.IsVisible)
			{
				CameraManager.shakeCamera(0.4f, Utils.GetAngle(base.transform.position, TargetObject.transform.position));
			}
			state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
			Thrown = true;
			b.Play(ThrowBone.transform.position);
			break;
		case "shoot complete":
			state.CURRENT_STATE = StateMachine.State.Idle;
			StartCoroutine(ChasePlayer());
			break;
		case "attack":
			if (simpleSpineAnimator.IsVisible)
			{
				CameraManager.shakeCamera(0.3f, Utils.GetAngle(base.transform.position, TargetObject.transform.position));
			}
			Attacked = true;
			collider2DList = new List<Collider2D>();
			DamageCollider.GetContacts(collider2DList);
			{
				foreach (Collider2D collider2D in collider2DList)
				{
					EnemyHealth = collider2D.gameObject.GetComponent<Health>();
					if (EnemyHealth != null && (EnemyHealth.team != health.team || health.team == Health.Team.PlayerTeam))
					{
						EnemyHealth.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, EnemyHealth.transform.position, 0.7f));
					}
				}
				break;
			}
		case "attack complete":
			state.CURRENT_STATE = StateMachine.State.Idle;
			StartCoroutine(ChasePlayer());
			break;
		}
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
		while (Vector3.Distance(TargetObject.transform.position, base.transform.position) > Range)
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
		simpleSpineAnimator.FlashFillRed();
		BiomeConstants.Instance.EmitHitVFX(AttackLocation - Vector3.back * 0.5f, Quaternion.identity.z, "HitFX_Weak");
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		knockBackVX = (0f - KnockbackSpeed) * 1f * Mathf.Cos(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
		knockBackVY = (0f - KnockbackSpeed) * 1f * Mathf.Sin(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
		GameObject obj = BiomeConstants.Instance.GroundSmash_Medium.Spawn();
		obj.transform.position = base.transform.position;
		obj.transform.rotation = Quaternion.identity;
		CameraManager.shakeCamera(0.3f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		ClearPaths();
		StopAllCoroutines();
	}

	private IEnumerator ThrowRock()
	{
		float RandomDelay = UnityEngine.Random.Range(0.2f, 1f);
		while (true)
		{
			float num;
			RandomDelay = (num = RandomDelay - Time.deltaTime);
			if (!(num > 0f))
			{
				break;
			}
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
		simpleSpineAnimator.Animate("shoot", 0, false);
		Thrown = false;
		g = UnityEngine.Object.Instantiate(RockToThrow, TargetObject.transform.position, Quaternion.identity, base.transform.parent);
		b = g.GetComponent<BruteRock>();
		while (!Thrown)
		{
			if (Vector2.Distance(base.transform.position, TargetObject.transform.position) > 3f)
			{
				g.transform.position = Vector3.Lerp(g.transform.position, TargetObject.transform.position, 10f * Time.deltaTime);
			}
			yield return null;
		}
		simpleSpineAnimator.FlashWhite(false);
	}

	private IEnumerator Attack()
	{
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
		simpleSpineAnimator.Animate("attack", 0, false);
		Attacked = false;
		while (!Attacked)
		{
			if (Time.frameCount % 5 == 0)
			{
				simpleSpineAnimator.FlashWhite(simpleSpineAnimator.isFillWhite = ((!simpleSpineAnimator.isFillWhite) ? true : false));
			}
			yield return null;
		}
		simpleSpineAnimator.FlashWhite(false);
	}

	private IEnumerator ChasePlayer()
	{
		state.CURRENT_STATE = StateMachine.State.Idle;
		bool Loop = true;
		float AttackTimer = UnityEngine.Random.Range(0.5f, 1.5f);
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
			if (state.CURRENT_STATE == StateMachine.State.Idle)
			{
				AttackTimer -= Time.deltaTime;
				float num = Vector2.Distance(base.transform.position, TargetObject.transform.position);
				if (num < 20f && num > 3f && AttackTimer < 0f)
				{
					StartCoroutine(ThrowRock());
					break;
				}
			}
			yield return null;
		}
	}
}
