using System.Collections;
using UnityEngine;

public class EnemyJellyFish : UnitObject
{
	public float turningSpeed = 1f;

	public float angleNoiseAmplitude;

	public float angleNoiseFrequency;

	public float timestamp;

	public float MaximumRange = 5f;

	public float attackPlayerDistance = 2f;

	public float KnockbackModifier = 0.3f;

	public SimpleSpineFlash SimpleSpineFlash;

	public ColliderEvents damageColliderEvents;

	private Vector3? StartingPosition;

	protected Vector3 TargetPosition;

	private int RanDirection = 1;

	private float Angle;

	private float AttackCoolDown;

	private Vector2 AttackCoolDownDuration = new Vector2(1.5f, 2.5f);

	public float SignPostAttackDuration = 0.5f;

	private Health EnemyHealth;

	public override void OnEnable()
	{
		base.OnEnable();
		if (!StartingPosition.HasValue)
		{
			StartingPosition = base.transform.position;
			TargetPosition = StartingPosition.Value;
		}
		if (GameManager.GetInstance() != null)
		{
			timestamp = GameManager.GetInstance().CurrentTime;
		}
		else
		{
			timestamp = Time.time;
		}
		turningSpeed += Random.Range(-0.1f, 0.1f);
		angleNoiseFrequency += Random.Range(-0.1f, 0.1f);
		angleNoiseAmplitude += Random.Range(-0.1f, 0.1f);
		RanDirection = ((!(Random.value < 0.5f)) ? 1 : (-1));
		damageColliderEvents.SetActive(false);
		damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
		StartCoroutine(ActiveRoutine());
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if (damageColliderEvents != null)
		{
			damageColliderEvents.SetActive(false);
			damageColliderEvents.OnTriggerEnterEvent -= OnDamageTriggerEnter;
		}
	}

	protected IEnumerator ActiveRoutine()
	{
		while (true)
		{
			float num = turningSpeed;
			state.LookAngle = state.facingAngle;
			Angle = Mathf.LerpAngle(Angle, Utils.GetAngle(base.transform.position, TargetPosition), Time.deltaTime * num);
			if (GameManager.GetInstance() != null && angleNoiseAmplitude > 0f && angleNoiseFrequency > 0f && MagnitudeFindDistanceBetween(TargetPosition, base.transform.position) < MaximumRange * MaximumRange)
			{
				Angle += (-0.5f + Mathf.PerlinNoise(GameManager.GetInstance().TimeSince(timestamp) * angleNoiseFrequency, 0f)) * angleNoiseAmplitude * (float)RanDirection;
			}
			speed = maxSpeed * SpeedMultiplier;
			state.facingAngle = Angle;
			yield return null;
		}
	}

	private IEnumerator AttackRoutine(bool DestroyOnComplete)
	{
		float Progress = 0f;
		float Duration = SignPostAttackDuration;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			SimpleSpineFlash.FlashWhite(Progress / Duration);
			foreach (Health item in Health.team2)
			{
				if (item != health && MagnitudeFindDistanceBetween(item.transform.position, base.transform.position) <= 1f)
				{
					Progress = Duration;
				}
			}
			yield return null;
		}
		Explosion.CreateExplosion(base.transform.position + Vector3.back, Health.Team.KillAll, health, 1f, 1f, 5f);
		SimpleSpineFlash.FlashWhite(false);
		yield return new WaitForSeconds(0.2f);
		AttackCoolDown = Random.Range(AttackCoolDownDuration.x, AttackCoolDownDuration.y);
		if (DestroyOnComplete)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			StartCoroutine(ActiveRoutine());
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		if (AttackType != Health.AttackTypes.NoKnockBack)
		{
			DoKnockBack(Attacker, KnockbackModifier, 0.5f);
		}
		AttackCoolDown = Mathf.Min(AttackCoolDown, 0.5f);
		StartCoroutine(HurtRoutine());
		SimpleSpineFlash.FlashWhite(false);
		SimpleSpineFlash.FlashFillRed();
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		StopAllCoroutines();
		if (AttackType != Health.AttackTypes.NoKnockBack)
		{
			DoKnockBack(Attacker, KnockbackModifier, 0.5f);
		}
		StartCoroutine(AttackRoutine(true));
	}

	private IEnumerator HurtRoutine()
	{
		Debug.Log("Start hurt routine");
		damageColliderEvents.SetActive(false);
		ClearPaths();
		yield return new WaitForSeconds(0.5f);
		DisableForces = false;
		StartingPosition = base.transform.position;
		TargetPosition = StartingPosition.Value;
	}

	private void OnDamageTriggerEnter(Collider2D collider)
	{
		EnemyHealth = collider.GetComponent<Health>();
		if (EnemyHealth != null && (EnemyHealth.team != health.team || health.team == Health.Team.PlayerTeam))
		{
			EnemyHealth.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, EnemyHealth.transform.position, 0.7f));
		}
	}

	private float MagnitudeFindDistanceBetween(Vector3 a, Vector3 b)
	{
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		float num3 = a.z - b.z;
		return num * num + num2 * num2 + num3 * num3;
	}

	private void OnDrawGizmos()
	{
		if (StartingPosition.HasValue)
		{
			Utils.DrawCircleXY(TargetPosition, 0.3f, Color.red);
		}
		if (StartingPosition.HasValue)
		{
			Utils.DrawCircleXY(TargetPosition, MaximumRange, Color.red);
		}
		else
		{
			Utils.DrawCircleXY(base.transform.position, MaximumRange, Color.red);
		}
	}
}
