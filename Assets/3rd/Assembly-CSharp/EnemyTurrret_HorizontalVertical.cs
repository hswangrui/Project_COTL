using System;
using System.Collections;
using Spine.Unity;
using UnityEngine;

public class EnemyTurrret_HorizontalVertical : UnitObject
{
	public enum ShootDirection
	{
		Up,
		Down,
		Left,
		Right
	}

	public SpriteRenderer Aiming;

	public SpriteRenderer Image;

	public SkeletonAnimation Spine;

	public SimpleSpineFlash SimpleSpineFlash;

	public Sprite ClosedEye;

	public Sprite OpenEye;

	public float LookAngle;

	private float ShootDelay;

	public int ShotsToFire = 5;

	public float DetectEnemyRange = 8f;

	public GameObject Arrow;

	private bool Shooting;

	private GameObject TargetObject;

	private Health EnemyHealth;

	public ShootDirection shootingDirection;

	public float shakeDuration = 0.5f;

	public Vector3 shakeStrength = new Vector3(0.25f, 0.25f, 0.01f);

	public int vibrato = 10;

	public float randomness = 90f;

	public bool ForceShootingDirection;

	public float UpAngle;

	public float DownAngle;

	public float RightAngle;

	public float LeftAngle;

	public float Offset;

	public bool changedDirection;

	private float currentAngle;

	public float anticipationWaitTime = 1f;

	public float coolDownForcedDirection = 0.5f;

	public float coolDownChangingDirection = 0.33f;

	private void Start()
	{
		health = GetComponent<Health>();
		health.OnHit += OnHit;
		health.OnDie += OnDie;
		Aiming.gameObject.SetActive(false);
		Spine.AnimationState.SetAnimation(0, "closed", true);
	}

	private new void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		BiomeConstants.Instance.EmitHitVFX(AttackLocation, Quaternion.identity.z, "HitFX_Weak");
		CameraManager.shakeCamera(0.1f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		SimpleSpineFlash.FlashFillRed();
	}

	private new void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		CameraManager.shakeCamera(0.5f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
	}

	private new void Update()
	{
		if (TargetObject == null)
		{
			if (Time.frameCount % 10 == 0)
			{
				GetNewTarget();
			}
		}
		else if (!Shooting)
		{
			if (!((ShootDelay -= Time.deltaTime) < 0f))
			{
				return;
			}
			if (!ForceShootingDirection)
			{
				if (TargetObject != null)
				{
					LookAngle = ClampAngle(Utils.GetAngle(base.transform.position, TargetObject.transform.position));
				}
			}
			else
			{
				switch (shootingDirection)
				{
				case ShootDirection.Down:
					LookAngle = 270f;
					break;
				case ShootDirection.Up:
					LookAngle = 90f;
					break;
				case ShootDirection.Left:
					LookAngle = 180f;
					break;
				case ShootDirection.Right:
					LookAngle = 0f;
					break;
				}
			}
			StartCoroutine(ShootArrowRoutine());
		}
		else if (Vector3.Distance(TargetObject.transform.position, base.transform.position) > 12f)
		{
			TargetObject = null;
			Spine.AnimationState.SetAnimation(0, "closed", true);
		}
	}

	private float ClampAngle(float a)
	{
		Debug.Log(a);
		a = ((a > RightAngle + Offset && a < UpAngle + Offset) ? 0f : ((a > UpAngle + Offset && a < LeftAngle + Offset) ? 90f : ((a > LeftAngle + Offset && a < DownAngle + Offset) ? 180f : ((!(a > DownAngle + Offset) || !(a < 360f + Offset)) ? 0f : 270f))));
		return a;
	}

	private void OnDrawGizmos()
	{
		if (!ForceShootingDirection)
		{
			Utils.DrawLine(base.transform.position, base.transform.position + new Vector3(2f * Mathf.Cos((UpAngle + Offset) * ((float)Math.PI / 180f)), 2f * Mathf.Sin((UpAngle + Offset) * ((float)Math.PI / 180f))), Color.blue);
			Utils.DrawLine(base.transform.position, base.transform.position + new Vector3(2f * Mathf.Cos((DownAngle + Offset) * ((float)Math.PI / 180f)), 2f * Mathf.Sin((DownAngle + Offset) * ((float)Math.PI / 180f))), Color.blue);
			Utils.DrawLine(base.transform.position, base.transform.position + new Vector3(2f * Mathf.Cos((RightAngle + Offset) * ((float)Math.PI / 180f)), 2f * Mathf.Sin((RightAngle + Offset) * ((float)Math.PI / 180f))), Color.blue);
			Utils.DrawLine(base.transform.position, base.transform.position + new Vector3(2f * Mathf.Cos((LeftAngle + Offset) * ((float)Math.PI / 180f)), 2f * Mathf.Sin((LeftAngle + Offset) * ((float)Math.PI / 180f))), Color.blue);
			return;
		}
		switch (shootingDirection)
		{
		case ShootDirection.Down:
			Utils.DrawLine(base.transform.position, base.transform.position + new Vector3(2f * Mathf.Cos(4.712389f), 2f * Mathf.Sin(4.712389f)), Color.blue);
			break;
		case ShootDirection.Up:
			Utils.DrawLine(base.transform.position, base.transform.position + new Vector3(2f * Mathf.Cos((float)Math.PI / 2f), 2f * Mathf.Sin((float)Math.PI / 2f)), Color.blue);
			break;
		case ShootDirection.Left:
			Utils.DrawLine(base.transform.position, base.transform.position + new Vector3(2f * Mathf.Cos((float)Math.PI), 2f * Mathf.Sin((float)Math.PI)), Color.blue);
			break;
		case ShootDirection.Right:
			Utils.DrawLine(base.transform.position, base.transform.position + new Vector3(2f * Mathf.Cos(0f), 2f * Mathf.Sin(0f)), Color.blue);
			break;
		}
	}

	private IEnumerator ShootArrowRoutine()
	{
		Image.sprite = OpenEye;
		Shooting = true;
		int i = ShotsToFire;
		float flashTickTimer = 0f;
		while (true)
		{
			int num = i - 1;
			i = num;
			if (num < 0)
			{
				break;
			}
			Aiming.gameObject.SetActive(true);
			Aiming.transform.eulerAngles = new Vector3(0f, 0f, LookAngle);
			if (flashTickTimer >= 0.12f && BiomeConstants.Instance.IsFlashLightsActive)
			{
				Aiming.color = ((Aiming.color == Color.red) ? Color.white : Color.red);
				flashTickTimer = 0f;
			}
			float time = 0f;
			while (true)
			{
				float num2;
				time = (num2 = time + Time.deltaTime * Spine.timeScale);
				if (!(num2 < anticipationWaitTime / (float)ShotsToFire))
				{
					break;
				}
				flashTickTimer += Time.deltaTime;
				yield return null;
			}
		}
		i = ShotsToFire;
		while (true)
		{
			int num = i - 1;
			i = num;
			if (num < 0)
			{
				break;
			}
			Spine.AnimationState.SetAnimation(0, "shoot", false);
			Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
			CameraManager.shakeCamera(0.2f, LookAngle);
			Projectile component = ObjectPool.Spawn(Arrow, base.transform.parent).GetComponent<Projectile>();
			component.transform.position = base.transform.position;
			component.Angle = LookAngle;
			component.team = health.team;
			component.Speed = 6f;
			component.Owner = health;
			float time = 0f;
			while (true)
			{
				float num2;
				time = (num2 = time + Time.deltaTime * Spine.timeScale);
				if (!(num2 < 0.2f))
				{
					break;
				}
				yield return null;
			}
			if (!ForceShootingDirection)
			{
				if (TargetObject != null)
				{
					currentAngle = ClampAngle(Utils.GetAngle(base.transform.position, TargetObject.transform.position));
				}
				if (currentAngle == LookAngle)
				{
					ShootDelay = coolDownChangingDirection;
					changedDirection = false;
				}
				else
				{
					changedDirection = true;
					ShootDelay = coolDownChangingDirection;
				}
			}
			else
			{
				ShootDelay = coolDownForcedDirection;
			}
		}
		Shooting = false;
		Image.sprite = ClosedEye;
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
}
