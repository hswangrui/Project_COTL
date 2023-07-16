using System.Collections;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public class EnemyTurrret : UnitObject
{
	public SkeletonAnimation Spine;

	public SkeletonAnimation Warning;

	public SimpleSpineFlash SimpleSpineFlash;

	public SpriteRenderer Aiming;

	private float LookAngle;

	private float ShootDelay;

	public int ShotsToFire = 5;

	public float DetectEnemyRange = 8f;

	public GameObject Arrow;

	private bool Shooting;

	private GameObject TargetObject;

	private Health EnemyHealth;

	private bool playedAnticipation;

	public bool LockAngleTo90Degrees;

	public float ShootDelayTime = 1f;

	public float TimeBetweenShooting = 1f;

	private void Start()
	{
		health = GetComponent<Health>();
		health.OnHit += OnHit;
		health.OnDie += OnDie;
		Aiming.DOFade(0f, 0f);
		Spine.AnimationState.SetAnimation(0, "closed", true);
		Warning.gameObject.SetActive(false);
	}

	public override void OnEnable()
	{
		base.OnEnable();
		Shooting = false;
		ShootDelay = ((Health.team2.Count > 1) ? Random.Range(0f, 1f) : 0f);
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/worm/gethit", base.transform.position);
		StopAllCoroutines();
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
		if (!CheatConsole.HidingUI)
		{
			Aiming.enabled = false;
		}
		else
		{
			Aiming.enabled = true;
		}
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
		int i = ShotsToFire;
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/worm/warning", base.transform.position);
		Spine.AnimationState.SetAnimation(0, "anticipation", false);
		Spine.AnimationState.AddAnimation(0, "shoot", false, 0f);
		Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (num < 0.83f - TimeBetweenShooting)
			{
				yield return null;
				continue;
			}
			break;
		}
		while (true)
		{
			int num2 = i - 1;
			i = num2;
			if (num2 < 0)
			{
				break;
			}
			Aiming.DOFade(1f, 0.33f);
			float Progress = 0f;
			while (true)
			{
				float num;
				Progress = (num = Progress + Time.deltaTime * Spine.timeScale);
				if (!(num < TimeBetweenShooting))
				{
					break;
				}
				if (TargetObject != null)
				{
					LookAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
				}
				Aiming.transform.eulerAngles = new Vector3(0f, 0f, LookAngle);
				yield return null;
			}
			AudioManager.Instance.PlayOneShot("event:/enemy/shoot_magicenergy", base.transform.position);
			AudioManager.Instance.PlayOneShot("event:/enemy/vocals/worm/attack", base.transform.position);
			Aiming.DOFade(0f, 0.33f);
			CameraManager.shakeCamera(0.2f, LookAngle);
			Projectile component = ObjectPool.Spawn(Arrow, base.transform.parent).GetComponent<Projectile>();
			component.transform.position = base.transform.position;
			if (!LockAngleTo90Degrees)
			{
				component.Angle = LookAngle;
			}
			else
			{
				component.Angle = (float)Mathf.FloorToInt(LookAngle / 90f) * 90f;
			}
			component.team = health.team;
			component.Speed = 6f;
			component.Owner = health;
		}
		time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.1f))
			{
				break;
			}
			yield return null;
		}
		Shooting = false;
		ShootDelay = ShootDelayTime;
	}

	public void GetNewTarget()
	{
		Health health = null;
		health = GetClosestTarget();
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

	private IEnumerator ShowWarning()
	{
		Warning.gameObject.SetActive(true);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/worm/warning", base.transform.position);
		yield return Warning.YieldForAnimation("warn");
		Warning.gameObject.SetActive(false);
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, DetectEnemyRange, Color.red);
	}
}
