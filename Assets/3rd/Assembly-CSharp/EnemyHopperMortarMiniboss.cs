using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EnemyHopperMortarMiniboss : EnemyHopperMortar
{
	public enum BurpType
	{
		RandomAroundTarget,
		RandomAroundMiniboss
	}

	private new IEnumerator damageColliderRoutine;

	public ParticleSystem aoeParticles;

	private const int totalEnemiesToSpawn = 3;

	private int numbEnemiesSpawned;

	public GameObject enemyToSpawn;

	public Vector2 TimeBetweenEnemySpawn;

	public BurpType[] burpPattern;

	private int burpPatternIndex;

	public int ShotsToFireAroundMiniboss = 6;

	private float shotsAroundMinibossDistance = 4f;

	public int ShotsToFireCross;

	public int ShotsToFireLine = 4;

	private float lastEnemySpawnTime;

	private bool playedJumpSfx;

	private void Start()
	{
		if ((bool)state)
		{
			StateMachine stateMachine = state;
			stateMachine.OnStateChange = (StateMachine.StateChange)Delegate.Combine(stateMachine.OnStateChange, new StateMachine.StateChange(OnStateChange));
		}
		KnockBackMultipier = 0f;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)state)
		{
			StateMachine stateMachine = state;
			stateMachine.OnStateChange = (StateMachine.StateChange)Delegate.Remove(stateMachine.OnStateChange, new StateMachine.StateChange(OnStateChange));
		}
	}

	private void OnStateChange()
	{
		if (state.CURRENT_STATE == StateMachine.State.Moving)
		{
			maxSpeed = UnityEngine.Random.Range(0.1f, 0.3f);
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		numbEnemiesSpawned = 0;
		minTimeBetweenBurps = 1.8f;
		if (GameManager.GetInstance() != null)
		{
			lastEnemySpawnTime = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(TimeBetweenEnemySpawn.x, TimeBetweenEnemySpawn.y);
		}
	}

	protected override void BurpFlies()
	{
		if ((bool)enemyToSpawn && numbEnemiesSpawned < 3 && health != null && GameManager.GetInstance().CurrentTime >= lastEnemySpawnTime)
		{
			SpawnHelperEnemies();
		}
		else
		{
			base.BurpFlies();
		}
	}

	private void SpawnHelperEnemies()
	{
		numbEnemiesSpawned++;
		Vector2 vector = -base.transform.position.normalized * 3f;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(Vector2.zero, -base.transform.position.normalized, 3f, layerToCheck);
		if ((bool)raycastHit2D)
		{
			vector = raycastHit2D.point + (Vector2)base.transform.position.normalized;
		}
		EnemySpawner.Create(vector, base.transform.parent, enemyToSpawn);
		state.CURRENT_STATE = StateMachine.State.Dancing;
		dancingTimestamp = GameManager.GetInstance().CurrentTime;
		lastEnemySpawnTime = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(TimeBetweenEnemySpawn.x, TimeBetweenEnemySpawn.y);
	}

	protected override void UpdateStateMoving()
	{
		if (!playedJumpSfx)
		{
			AudioManager.Instance.PlayOneShot(OnJumpSoundPath, base.gameObject);
			playedJumpSfx = true;
		}
		speed = hopSpeedCurve.Evaluate(gm.TimeSince(hoppingTimestamp) / hoppingDur) * hopMoveSpeed;
		Spine[0].transform.localPosition = -Vector3.forward * hopZCurve.Evaluate(gm.TimeSince(hoppingTimestamp) / hoppingDur) * hopZHeight;
		if (gm.TimeSince(hoppingTimestamp) / hoppingDur > 0.1f && gm.TimeSince(hoppingTimestamp) / hoppingDur < 0.9f)
		{
			health.enabled = false;
		}
		else if (!health.enabled)
		{
			health.enabled = true;
		}
		canBeParried = false;
		SimpleSpineFlash.FlashWhite(1f - Mathf.Clamp01(gm.TimeSince(hoppingTimestamp) / (attackingDur * 0.5f)));
		if (gm.TimeSince(hoppingTimestamp) >= hoppingDur)
		{
			speed = 0f;
			DoAttack();
			if (ShouldStartCharging())
			{
				playedJumpSfx = false;
				state.CURRENT_STATE = StateMachine.State.Charging;
			}
			else
			{
				playedJumpSfx = false;
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
		}
	}

	private new void DoAttack()
	{
		if (damageColliderRoutine != null)
		{
			StopCoroutine(damageColliderRoutine);
		}
		AudioManager.Instance.PlayOneShot(OnLandSoundPath, base.gameObject);
		AudioManager.Instance.PlayOneShot(AttackVO, base.gameObject);
		damageColliderRoutine = TurnOnDamageColliderForDuration(attackingDur);
		StartCoroutine(damageColliderRoutine);
		if (aoeParticles != null)
		{
			aoeParticles.Play();
		}
		BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position + Vector3.back * 0.5f);
		CameraManager.shakeCamera(2f);
	}

	private IEnumerator TurnOnDamageColliderForDuration(float duration)
	{
		damageColliderEvents.SetActive(true);
		yield return new WaitForSeconds(duration);
		damageColliderEvents.SetActive(false);
	}

	protected override IEnumerator ShootProjectileRoutine()
	{
		if (burpPattern[burpPatternIndex] == BurpType.RandomAroundTarget && UnityEngine.Random.Range(0f, 1f) > 0.33f)
		{
			yield return StartCoroutine(_003C_003En__0());
		}
		else if (Vector3.Distance(base.transform.position, Vector3.zero) < 2f && (double)UnityEngine.Random.Range(0f, 1f) < 0.6)
		{
			yield return StartCoroutine(CrossShoot());
		}
		else if (UnityEngine.Random.Range(0, 2) == 0)
		{
			yield return StartCoroutine(CircleShoot());
		}
		else
		{
			yield return StartCoroutine(LineShoot());
		}
		burpPatternIndex = (burpPatternIndex + 1) % burpPattern.Length;
	}

	private IEnumerator CircleShoot()
	{
		AudioManager.Instance.PlayOneShot(AttackVO, base.gameObject);
		Vector3 position = base.transform.position;
		float aimingAngle = UnityEngine.Random.Range(0f, 360f);
		for (int i = 0; i < ShotsToFireAroundMiniboss; i++)
		{
			if (targetObject == null && GetClosestTarget() != null)
			{
				targetObject = GetClosestTarget().gameObject;
			}
			if (!(targetObject == null))
			{
				Vector3 vector = base.transform.position + (Vector3)Utils.DegreeToVector2(aimingAngle) * UnityEngine.Random.Range(2.5f, shotsAroundMinibossDistance);
				MortarBomb component = UnityEngine.Object.Instantiate(projectilePrefab, targetObject.transform.position, Quaternion.identity, base.transform.parent).GetComponent<MortarBomb>();
				if (Vector2.Distance(base.transform.position, vector) < 2.5f)
				{
					Vector2 vector2 = base.transform.position + (vector - base.transform.position).normalized * 2.5f;
					component.transform.position = vector2;
				}
				else
				{
					Vector2 vector3 = base.transform.position + (vector - base.transform.position).normalized * 5f;
					component.transform.position = vector3;
				}
				component.Play(base.transform.position + new Vector3(0f, 0f, -1.5f), bombDuration, Health.Team.Team2);
				SimpleSpineFlash.FlashWhite(false);
				aimingAngle += (float)(360 / ShotsToFireAroundMiniboss);
				yield return new WaitForSeconds(0.112500004f);
				continue;
			}
			break;
		}
	}

	private IEnumerator CrossShoot()
	{
		AudioManager.Instance.PlayOneShot(AttackVO, base.gameObject);
		Vector3 position2 = base.transform.position;
		SimpleSpineFlash.FlashWhite(false);
		float distance = 1f;
		for (int i = 0; i < ShotsToFireCross; i++)
		{
			float num = 0f;
			for (int j = 0; j < 4; j++)
			{
				Vector3 position = base.transform.position + (Vector3)Utils.DegreeToVector2(num) * distance;
				UnityEngine.Object.Instantiate(projectilePrefab, position, Quaternion.identity, base.transform.parent).GetComponent<MortarBomb>().Play(base.transform.position + new Vector3(0f, 0f, -1.5f), bombDuration, Health.Team.Team2);
				num += 90f;
			}
			distance += 2f;
			yield return new WaitForSeconds(0.112500004f);
		}
	}

	private IEnumerator LineShoot()
	{
		AudioManager.Instance.PlayOneShot(AttackVO, base.gameObject);
		Vector3 position2 = base.transform.position;
		SimpleSpineFlash.FlashWhite(false);
		float distance = 1f;
		float aimAngle = ((GetClosestTarget() != null) ? Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position) : UnityEngine.Random.Range(0f, 360f));
		for (int i = 0; i < ShotsToFireLine; i++)
		{
			Vector3 position = base.transform.position + (Vector3)Utils.DegreeToVector2(aimAngle) * distance;
			UnityEngine.Object.Instantiate(projectilePrefab, position, Quaternion.identity, base.transform.parent).GetComponent<MortarBomb>().Play(base.transform.position + new Vector3(0f, 0f, -1.5f), bombDuration, Health.Team.Team2);
			distance += 2f;
			yield return new WaitForSeconds(0.112500004f);
		}
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private IEnumerator _003C_003En__0()
	{
		return base.ShootProjectileRoutine();
	}
}
