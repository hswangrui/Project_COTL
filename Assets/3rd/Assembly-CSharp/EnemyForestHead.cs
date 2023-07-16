using System;
using System.Collections;
using UnityEngine;

public class EnemyForestHead : BaseMonoBehaviour
{
	public Health health;

	private Rigidbody2D rb2D;

	public SimpleSpineAnimator simpleSpineAnimator;

	private StateMachine state;

	public float KnockbackSpeed = 1500f;

	public Transform SpawnPoint1;

	public Transform SpawnPoint2;

	public GameObject ToSpawn;

	private float MoveSpeed = -2f;

	private GameObject Player;

	private Vector3 Seperator;

	public float SeperationRadius = 3f;

	private void OnEnable()
	{
		health = GetComponent<Health>();
		health.OnHit += OnHit;
		rb2D = GetComponent<Rigidbody2D>();
		state = GetComponent<StateMachine>();
		EnemyForestMushroom.enemyForestMushrooms.Add(base.gameObject);
	}

	private void Start()
	{
		StartCoroutine(ChasePlayer());
	}

	private void OnDisable()
	{
		health.OnHit -= OnHit;
		EnemyForestMushroom.enemyForestMushrooms.Remove(base.gameObject);
	}

	private void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		float f = Utils.GetAngle(Attacker.transform.position, base.transform.position) * ((float)Math.PI / 180f);
		Vector3 force = new Vector2(KnockbackSpeed * Mathf.Cos(f), KnockbackSpeed * Mathf.Sin(f));
		StartCoroutine(AddForce(force));
		simpleSpineAnimator.FillColor(Color.red);
		CameraManager.shakeCamera(0.2f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
	}

	private IEnumerator AddForce(Vector3 Force)
	{
		MoveSpeed = 0f;
		yield return new WaitForSeconds(0.05f);
		rb2D.AddForce(Force);
		simpleSpineAnimator.FlashFillRed();
		float Timer = 0f;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (num < 0.5f)
			{
				MoveSpeed = 0f;
				yield return null;
				continue;
			}
			break;
		}
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
		bool FacingUp = false;
		float AttackDelay = 3f;
		while (Loop)
		{
			float num = Vector3.Distance(base.transform.position, Player.transform.position);
			if (num > 5f)
			{
				if (MoveSpeed < 1f)
				{
					MoveSpeed += 0.1f;
				}
			}
			else
			{
				MoveSpeed = Mathf.Lerp(MoveSpeed, 0f, 15f * Time.deltaTime);
			}
			state.facingAngle = Utils.SmoothAngle(state.facingAngle, Utils.GetAngle(base.transform.position, Player.transform.position), 30f);
			float f = state.facingAngle * ((float)Math.PI / 180f);
			Vector3 vector = new Vector3(MoveSpeed * Mathf.Cos(f), MoveSpeed * Mathf.Sin(f)) * Time.deltaTime;
			base.transform.position = base.transform.position + vector;
			if (state.facingAngle > 30f && state.facingAngle < 150f)
			{
				if (!FacingUp)
				{
					FacingUp = true;
					simpleSpineAnimator.Animate("moving_backView", 0, true);
				}
			}
			else if (FacingUp)
			{
				FacingUp = false;
				simpleSpineAnimator.Animate("moving", 0, true);
			}
			if (num < 5f)
			{
				float num2;
				AttackDelay = (num2 = AttackDelay - Time.deltaTime);
				if (num2 < 0f && EnemyForestMushroom.enemyForestMushrooms.Count < 5)
				{
					Loop = false;
					StartCoroutine(SpawnEnemy());
					continue;
				}
			}
			yield return null;
		}
	}

	private IEnumerator SpawnEnemy()
	{
		MoveSpeed = 0f;
		state.CURRENT_STATE = StateMachine.State.SignPostAttack;
		yield return new WaitForSeconds(1f);
		StartCoroutine(DoSpawn());
		yield return new WaitForSeconds(1f);
		state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(ChasePlayer());
	}

	private IEnumerator DoSpawn()
	{
		if (simpleSpineAnimator.IsVisible)
		{
			CameraManager.shakeCamera(0.5f, UnityEngine.Random.Range(0, 360));
		}
		GameObject obj = UnityEngine.Object.Instantiate(ToSpawn);
		Vector3 position = SpawnPoint1.position;
		position.z = 0f;
		obj.transform.position = position;
		BiomeConstants.Instance.SpawnInWhite.Spawn().transform.position = SpawnPoint1.position;
		yield return new WaitForSeconds(0.5f);
		if (simpleSpineAnimator.IsVisible)
		{
			CameraManager.shakeCamera(0.5f, UnityEngine.Random.Range(0, 360));
		}
		GameObject obj2 = UnityEngine.Object.Instantiate(ToSpawn);
		position = SpawnPoint2.position;
		position.z = 0f;
		obj2.transform.position = position;
		BiomeConstants.Instance.SpawnInWhite.Spawn().transform.position = SpawnPoint2.position;
	}

	private void Update()
	{
	}

	private void SeperateMushrooms()
	{
		Seperator = Vector3.zero;
		foreach (GameObject enemyForestMushroom in EnemyForestMushroom.enemyForestMushrooms)
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
