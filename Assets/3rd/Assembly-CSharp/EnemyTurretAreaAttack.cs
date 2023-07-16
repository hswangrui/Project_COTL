using System;
using System.Collections;
using Spine.Unity;
using UnityEngine;

public class EnemyTurretAreaAttack : UnitObject
{
	public SkeletonAnimation Spine;

	public SimpleSpineFlash SimpleSpineFlash;

	public SkeletonAnimation Warning;

	private float LookAngle;

	private float ShootDelay;

	public int ShotsToFire = 12;

	public float DetectEnemyRange = 8f;

	public GameObject Arrow;

	private bool Shooting;

	private GameObject TargetObject;

	private Health EnemyHealth;

	public float ShootInterval = 2f;

	public float LifeTime = 2f;

	private void Start()
	{
		health = GetComponent<Health>();
		health.OnHit += OnHit;
		health.OnDie += OnDie;
		Spine.AnimationState.SetAnimation(0, "closed", true);
	}

	public override void OnEnable()
	{
		base.OnEnable();
		Shooting = false;
		ShootDelay = 1f;
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/worm/gethit", base.transform.position);
		StopAllCoroutines();
		Spine.AnimationState.SetAnimation(0, "idle", true);
		Shooting = false;
		if (ShootDelay < 1f)
		{
			ShootDelay = 1f;
		}
		BiomeConstants.Instance.EmitHitVFX(AttackLocation, Quaternion.identity.z, "HitFX_Weak");
		CameraManager.shakeCamera(0.1f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		SimpleSpineFlash.FlashFillRed();
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/worm/death", base.transform.position);
		CameraManager.shakeCamera(0.5f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
	}

	public override void Update()
	{
		base.Update();
		if (TargetObject == null)
		{
			if (Time.frameCount % 10 == 0)
			{
				GetNewTarget();
			}
		}
		else if (!Shooting)
		{
			if ((ShootDelay -= Time.deltaTime) < 0.5f)
			{
				StartCoroutine(ShootArrowRoutine());
			}
		}
		else if (Vector3.Distance(TargetObject.transform.position, base.transform.position) > 12f)
		{
			TargetObject = null;
			Spine.AnimationState.SetAnimation(0, "closed", true);
		}
	}

	private IEnumerator ShootArrowRoutine()
	{
		Shooting = true;
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * Spine.timeScale);
			if (!(num < ShootInterval))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/worm/warning", base.transform.position);
		yield return Spine.YieldForAnimation("anticipation");
		SimpleSpineFlash.FlashWhite(false);
		CameraManager.shakeCamera(0.2f, LookAngle);
		Spine.AnimationState.SetAnimation(0, "shoot", false);
		Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		AudioManager.Instance.PlayOneShot("event:/enemy/shoot_magicenergy", base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/worm/attack", base.transform.position);
		float num2 = ShotsToFire;
		while ((num2 -= 1f) >= 0f)
		{
			Projectile component = ObjectPool.Spawn(Arrow, base.transform.parent).GetComponent<Projectile>();
			component.transform.position = base.transform.position;
			component.Angle = 360f / (float)ShotsToFire * num2;
			component.team = health.team;
			component.Speed = 5f;
			component.LifeTime = LifeTime + UnityEngine.Random.Range(0f, 0.3f);
			component.Owner = health;
		}
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.2f))
			{
				break;
			}
			yield return null;
		}
		Shooting = false;
		ShootDelay = 2f;
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
			Spine.AnimationState.SetAnimation(0, "idle", true);
			StartCoroutine(ShowWarning());
			ShootDelay += 1f;
		}
	}

	public new bool CheckLineOfSight(Vector3 pointToCheck, float distance)
	{
		if (ColliderRadius == null)
		{
			ColliderRadius = GetComponent<CircleCollider2D>();
		}
		if (Physics2D.Raycast(base.transform.position, pointToCheck - base.transform.position, distance, layerToCheck).collider != null)
		{
			return false;
		}
		float angle = Utils.GetAngle(base.transform.position, pointToCheck);
		if (Physics2D.Raycast(base.transform.position + new Vector3(ColliderRadius.radius * Mathf.Cos((angle + 90f) * ((float)Math.PI / 180f)), ColliderRadius.radius * Mathf.Sin((angle + 90f) * ((float)Math.PI / 180f))), pointToCheck - base.transform.position, distance, layerToCheck).collider != null)
		{
			return false;
		}
		if (Physics2D.Raycast(base.transform.position + new Vector3(ColliderRadius.radius * Mathf.Cos((angle - 90f) * ((float)Math.PI / 180f)), ColliderRadius.radius * Mathf.Sin((angle - 90f) * ((float)Math.PI / 180f))), pointToCheck - base.transform.position, distance, layerToCheck).collider != null)
		{
			return false;
		}
		return true;
	}

	private IEnumerator ShowWarning()
	{
		Warning.gameObject.SetActive(true);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/worm/warning", base.transform.position);
		yield return Warning.YieldForAnimation("warn");
		Warning.gameObject.SetActive(false);
	}
}
