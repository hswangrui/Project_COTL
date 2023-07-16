using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyForestMushroom : BaseMonoBehaviour
{
	public static List<GameObject> enemyForestMushrooms = new List<GameObject>();

	public Health health;

	private Rigidbody2D rb2D;

	private SimpleSpineAnimator simpleSpineAnimator;

	private StateMachine state;

	public float KnockbackSpeed = 1500f;

	public int Damage = 1;

	private Coroutine ChasePlayerCoroutine;

	private GameObject Player;

	private Vector3 Seperator;

	public float SeperationRadius = 1f;

	private void OnEnable()
	{
		health = GetComponent<Health>();
		health.OnHit += OnHit;
		health.OnDie += OnDie;
		rb2D = GetComponent<Rigidbody2D>();
		simpleSpineAnimator = GetComponentInChildren<SimpleSpineAnimator>();
		state = GetComponent<StateMachine>();
		enemyForestMushrooms.Add(base.gameObject);
	}

	private void OnDisable()
	{
		health.OnHit -= OnHit;
		health.OnDie -= OnDie;
		enemyForestMushrooms.Remove(base.gameObject);
	}

	private void Start()
	{
		StartCoroutine(SpawnIn());
	}

	private IEnumerator SpawnIn()
	{
		state.CURRENT_STATE = StateMachine.State.SpawnIn;
		if (Player == null)
		{
			Player = GameObject.FindWithTag("Player");
			if (Player != null)
			{
				state.facingAngle = Utils.GetAngle(base.transform.position, Player.transform.position);
			}
		}
		yield return new WaitForSeconds(0.5f);
		ChasePlayerCoroutine = StartCoroutine(ChasePlayer());
	}

	private IEnumerator ChasePlayer()
	{
		while (Player == null)
		{
			Player = GameObject.FindWithTag("Player");
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		bool Loop = true;
		float MoveSpeed = -2f;
		while (Loop)
		{
			if (MoveSpeed < 2f)
			{
				MoveSpeed += 0.25f;
			}
			state.facingAngle = Utils.SmoothAngle(state.facingAngle, Utils.GetAngle(base.transform.position, Player.transform.position), 10f);
			float f = state.facingAngle * ((float)Math.PI / 180f);
			Vector3 vector = new Vector3(MoveSpeed * Mathf.Cos(f), MoveSpeed * Mathf.Sin(f)) * Time.deltaTime;
			base.transform.position = base.transform.position + vector;
			if (Vector3.Distance(base.transform.position, Player.transform.position) <= 1f)
			{
				Loop = false;
			}
			else
			{
				yield return null;
			}
		}
		state.CURRENT_STATE = StateMachine.State.SignPostAttack;
		float Timer = 0f;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (!(num < 0.5f))
			{
				break;
			}
			if (Time.frameCount % 5 == 0)
			{
				simpleSpineAnimator.FlashWhite(!simpleSpineAnimator.isFillWhite);
			}
			yield return null;
		}
		simpleSpineAnimator.FlashWhite(false);
		if (simpleSpineAnimator.IsVisible)
		{
			CameraManager.shakeCamera(0.5f, Utils.GetAngle(base.transform.position, Player.transform.position));
		}
		DealDamage();
		state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
		yield return new WaitForSeconds(1f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		ChasePlayerCoroutine = StartCoroutine(ChasePlayer());
	}

	private void Update()
	{
		SeperateMushrooms();
	}

	private void DealDamage()
	{
		Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position, 1f);
		for (int i = 0; i < array.Length; i++)
		{
			Health component = array[i].gameObject.GetComponent<Health>();
			if (component != null && component.team != health.team)
			{
				component.DealDamage(Damage, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.8f));
			}
		}
	}

	private void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		float f = Utils.GetAngle(Attacker.transform.position, base.transform.position) * ((float)Math.PI / 180f);
		Vector3 force = new Vector2(KnockbackSpeed * Mathf.Cos(f), KnockbackSpeed * Mathf.Sin(f));
		StartCoroutine(AddForce(force));
		simpleSpineAnimator.FillColor(Color.red);
		CameraManager.shakeCamera(0.2f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		state.CURRENT_STATE = StateMachine.State.HitThrown;
		StopCoroutine(ChasePlayerCoroutine);
	}

	private IEnumerator AddForce(Vector3 Force)
	{
		yield return new WaitForSeconds(0.05f);
		rb2D.AddForce(Force);
		simpleSpineAnimator.FlashFillRed();
		yield return new WaitForSeconds(0.3f);
		ChasePlayerCoroutine = StartCoroutine(ChasePlayer());
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		CameraManager.shakeCamera(0.5f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
	}

	private void SeperateMushrooms()
	{
		Seperator = Vector3.zero;
		foreach (GameObject enemyForestMushroom in enemyForestMushrooms)
		{
			if (enemyForestMushroom != base.gameObject && enemyForestMushroom != null && state.CURRENT_STATE != StateMachine.State.SignPostAttack && state.CURRENT_STATE != StateMachine.State.RecoverFromAttack && state.CURRENT_STATE != StateMachine.State.Defending)
			{
				float num = Vector2.Distance(enemyForestMushroom.gameObject.transform.position, base.transform.position);
				float angle = Utils.GetAngle(enemyForestMushroom.gameObject.transform.position, base.transform.position);
				if (num < SeperationRadius)
				{
					Seperator.x += (SeperationRadius - num) / 2f * Mathf.Cos(angle * ((float)Math.PI / 180f)) * GameManager.DeltaTime;
					Seperator.y += (SeperationRadius - num) / 2f * Mathf.Sin(angle * ((float)Math.PI / 180f)) * GameManager.DeltaTime;
				}
			}
		}
		base.transform.position = base.transform.position + Seperator;
	}
}
