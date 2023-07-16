using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemyBatBurpSwarm : UnitObject
{
	private enum AttackType
	{
		Beam,
		Circle,
		Scatter
	}

	[SerializeField]
	private GameObject projectilePrefab;

	[SerializeField]
	private float anticipation;

	[SerializeField]
	private Vector2 attackCoolDownDuration;

	[SerializeField]
	private float radius;

	[SerializeField]
	private int beamAmount = 8;

	[SerializeField]
	private float beamTimeBetween;

	[SerializeField]
	private Vector2 beamSetTargetTime;

	[SerializeField]
	private int circleAmount = 8;

	[SerializeField]
	private float circleTimeBetween;

	[SerializeField]
	private Vector2 circleSetTargetTime;

	[SerializeField]
	private int scatterAmount = 8;

	[SerializeField]
	private float scatterTimeBetween;

	[SerializeField]
	private Vector2 scatterSetTargetTime;

	[SerializeField]
	private SpriteRenderer spriteRenderer;

	private float attackTimestamp = -1f;

	private bool attacking;

	private List<Projectile> activeProjectiles = new List<Projectile>();

	private AttackType previousAttackType;

	public override void Update()
	{
		base.Update();
		if (ShouldAttack())
		{
			Attack();
		}
		if (GameManager.RoomActive && attackTimestamp == -1f)
		{
			attackTimestamp = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(attackCoolDownDuration.x, attackCoolDownDuration.y);
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		attacking = false;
		spriteRenderer.color = Color.white;
	}

	private void Attack()
	{
		AttackType attackType;
		do
		{
			attackType = (AttackType)UnityEngine.Random.Range(0, Enum.GetNames(typeof(AttackType)).Length);
		}
		while (attackType == previousAttackType);
		switch (attackType)
		{
		case AttackType.Beam:
			SingleBeam();
			break;
		case AttackType.Circle:
			Circle();
			break;
		case AttackType.Scatter:
			Scatter();
			break;
		}
		previousAttackType = attackType;
	}

	private bool ShouldAttack()
	{
		if (!attacking)
		{
			GameManager instance = GameManager.GetInstance();
			return (((object)instance != null) ? new float?(instance.CurrentTime) : null) > attackTimestamp;
		}
		return false;
	}

	private void SingleBeam()
	{
		StartCoroutine(SingleBeamIE());
	}

	private IEnumerator SingleBeamIE()
	{
		attacking = true;
		Sequence s = DOTween.Sequence();
		s.Append(spriteRenderer.transform.DOScale(new Vector3(3f, 1.5f, 2f), anticipation / 3f).SetEase(Ease.InOutBounce));
		s.Append(spriteRenderer.transform.DOScale(new Vector3(1.75f, 2.25f, 2f), anticipation / 3f).SetEase(Ease.InOutBounce));
		s.Append(spriteRenderer.transform.DOScale(new Vector3(2f, 2f, 2f), anticipation / 3f).SetEase(Ease.InOutSine));
		yield return new WaitForSeconds(anticipation);
		for (int i = 0; i < beamAmount; i++)
		{
			StartCoroutine(SpawnProjectile(base.transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * radius), Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position), UnityEngine.Random.Range(beamSetTargetTime.x, beamSetTargetTime.y)));
			if (beamTimeBetween != 0f)
			{
				yield return new WaitForSeconds(beamTimeBetween);
			}
		}
		attacking = false;
		attackTimestamp = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(attackCoolDownDuration.x, attackCoolDownDuration.y);
	}

	private void Circle()
	{
		StartCoroutine(CircleIE());
	}

	private IEnumerator CircleIE()
	{
		attacking = true;
		Sequence s = DOTween.Sequence();
		s.Append(spriteRenderer.transform.DOScale(new Vector3(3f, 1.5f, 2f), anticipation / 3f).SetEase(Ease.InOutBounce));
		s.Append(spriteRenderer.transform.DOScale(new Vector3(1.75f, 2.25f, 2f), anticipation / 3f).SetEase(Ease.InOutBounce));
		s.Append(spriteRenderer.transform.DOScale(new Vector3(2f, 2f, 2f), anticipation / 3f).SetEase(Ease.InOutSine));
		yield return new WaitForSeconds(anticipation);
		List<float> shootAngles = new List<float>(circleAmount);
		for (int j = 0; j < circleAmount; j++)
		{
			shootAngles.Add(360f / (float)circleAmount * (float)j);
		}
		float initAngle = UnityEngine.Random.Range(0f, 360f);
		for (int i = 0; i < shootAngles.Count; i++)
		{
			StartCoroutine(SpawnProjectile(base.transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * radius), initAngle + shootAngles[i], UnityEngine.Random.Range(circleSetTargetTime.x, circleSetTargetTime.y)));
			if (circleTimeBetween != 0f)
			{
				yield return new WaitForSeconds(circleTimeBetween);
			}
		}
		attacking = false;
		attackTimestamp = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(attackCoolDownDuration.x, attackCoolDownDuration.y);
	}

	private void Scatter()
	{
		StartCoroutine(ScatterIE());
	}

	private IEnumerator ScatterIE()
	{
		attacking = true;
		Sequence s = DOTween.Sequence();
		s.Append(spriteRenderer.transform.DOScale(new Vector3(3f, 1.5f, 2f), anticipation / 3f).SetEase(Ease.InOutBounce));
		s.Append(spriteRenderer.transform.DOScale(new Vector3(1.75f, 2.25f, 2f), anticipation / 3f).SetEase(Ease.InOutBounce));
		s.Append(spriteRenderer.transform.DOScale(new Vector3(2f, 2f, 2f), anticipation / 3f).SetEase(Ease.InOutSine));
		yield return new WaitForSeconds(anticipation);
		for (int i = 0; i < scatterAmount; i++)
		{
			StartCoroutine(SpawnProjectile(base.transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * radius), UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(scatterSetTargetTime.x, scatterSetTargetTime.y)));
			if (scatterTimeBetween != 0f)
			{
				yield return new WaitForSeconds(scatterTimeBetween);
			}
		}
		attacking = false;
		attackTimestamp = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(attackCoolDownDuration.x, attackCoolDownDuration.y);
	}

	private IEnumerator SpawnProjectile(Vector3 spawnPosition, float angle, float setTargetTime)
	{
		Projectile projectile = UnityEngine.Object.Instantiate(projectilePrefab, base.transform.parent).GetComponent<Projectile>();
		projectile.transform.position = spawnPosition;
		projectile.Angle = angle;
		projectile.team = health.team;
		projectile.Speed += UnityEngine.Random.Range(-0.5f, 0.5f);
		projectile.turningSpeed += UnityEngine.Random.Range(-0.1f, 0.1f);
		projectile.angleNoiseFrequency += UnityEngine.Random.Range(-0.1f, 0.1f);
		projectile.LifeTime += UnityEngine.Random.Range(0f, 0.3f);
		projectile.Owner = health;
		activeProjectiles.Add(projectile);
		yield return new WaitForSeconds(setTargetTime);
		projectile.SetTarget(PlayerFarming.Health);
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		spriteRenderer.color = Color.red;
		spriteRenderer.DOColor(Color.white, 0.25f);
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		for (int num = activeProjectiles.Count - 1; num >= 0; num--)
		{
			if (activeProjectiles[num] != null)
			{
				activeProjectiles[num].health.DealDamage(100f, base.gameObject, base.transform.position);
			}
		}
	}
}
