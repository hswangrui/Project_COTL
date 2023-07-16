using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePattern : ProjectilePatternBase
{
	[Serializable]
	public struct BulletWave
	{
		public int Bullets;

		public float Speed;

		public float Acceleration;

		public float Deceleration;

		public float Offset;

		public float AngleBetweenBullets;

		public float FinishDelay;

		public int WaveGroupID;

		public Vector2 Randomness;

		public string AnimationToPlay;
	}

	public delegate void ProjectileWaveEvent(BulletWave wave);

	[Space]
	[SerializeField]
	private Projectile bulletPrefab;

	[SerializeField]
	private BulletWave[] bulletWaves;

	[Space]
	[SerializeField]
	private float globalSpeed = -1f;

	[SerializeField]
	private float globalDelayBetweenGroups = -1f;

	[SerializeField]
	private float globalAcceleration = -1f;

	[Space]
	[SerializeField]
	private bool targetPlayer = true;

	[SerializeField]
	private bool recalculatePlayerEachWave;

	[SerializeField]
	private bool repositionEachWave = true;

	[SerializeField]
	private float distance;

	[SerializeField]
	private bool debug;

	private Health health;

	private const int MAX_FRAMES_DELAY = 3;

	private int framesDelayed;

	private bool isBulletShotPlayedOnce;

	public BulletWave[] Waves
	{
		get
		{
			return bulletWaves;
		}
	}

	public Projectile BulletPrefab
	{
		get
		{
			return bulletPrefab;
		}
	}

	public event ProjectileWaveEvent OnProjectileWaveShot;

	private void Start()
	{
		health = GetComponentInParent<Health>();
		if ((bool)health)
		{
			health.OnDie += Health_OnDie;
		}
		int b = CountProjectiles(3f);
		ObjectPool.CreatePool(bulletPrefab, Mathf.Max(25, b));
	}

	private void OnDestroy()
	{
		if ((bool)health)
		{
			health.OnDie -= Health_OnDie;
		}
	}

	private void Health_OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		StopAllCoroutines();
	}

	public void Shoot()
	{
		StartCoroutine(ShootIE());
	}

	public void Shoot(float delay, GameObject target, Transform parent)
	{
		StartCoroutine(ShootIE(delay, target, parent));
	}

	public override IEnumerator ShootIE(float delay = 0f, GameObject target = null, Transform parent = null)
	{
		target = ((target == null) ? PlayerFarming.Instance.gameObject : target);
		float angle = Utils.GetAngle(base.transform.position, target.transform.position);
		angle = (targetPlayer ? angle : Utils.GetAngle(Vector3.zero, base.transform.right));
		if (delay != 0f)
		{
			float time2 = 0f;
			while (true)
			{
				float num;
				time2 = (num = time2 + Time.deltaTime * base.timeScale);
				if (!(num < delay))
				{
					break;
				}
				yield return null;
			}
		}
		int currentGroupID = 0;
		Vector3 startPosition = base.transform.position - Vector3.forward * 0.2f;
		List<Projectile[]> projectiles = new List<Projectile[]>();
		bool isSpawnedAllAtOnce = true;
		framesDelayed = 0;
		for (int j = 0; j < bulletWaves.Length; j++)
		{
			if (bulletWaves[j].FinishDelay > 0f)
			{
				isSpawnedAllAtOnce = false;
			}
		}
		if (globalDelayBetweenGroups != -1f)
		{
			isSpawnedAllAtOnce = false;
		}
		isBulletShotPlayedOnce = false;
		for (int i = 0; i < bulletWaves.Length; i++)
		{
			if (bulletWaves[i].WaveGroupID != currentGroupID && globalDelayBetweenGroups != -1f)
			{
				currentGroupID = bulletWaves[i].WaveGroupID;
				float time2 = 0f;
				while (true)
				{
					float num;
					time2 = (num = time2 + Time.deltaTime * base.timeScale);
					if (!(num < globalDelayBetweenGroups))
					{
						break;
					}
					yield return null;
				}
				isBulletShotPlayedOnce = false;
			}
			if (recalculatePlayerEachWave)
			{
				angle = (targetPlayer ? Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position) : angle);
			}
			if (repositionEachWave)
			{
				startPosition = base.transform.position - Vector3.forward * 0.2f;
			}
			projectiles.Add(SpawnBullets(bulletWaves[i], angle, parent, startPosition, !isSpawnedAllAtOnce));
			if (globalDelayBetweenGroups == -1f && bulletWaves[i].FinishDelay != 0f)
			{
				float time2 = 0f;
				while (true)
				{
					float num;
					time2 = (num = time2 + Time.deltaTime * base.timeScale);
					if (!(num < bulletWaves[i].FinishDelay))
					{
						break;
					}
					yield return null;
				}
				isBulletShotPlayedOnce = false;
			}
			if (isSpawnedAllAtOnce && i != 0 && i % 2 == 0 && framesDelayed <= 3)
			{
				yield return null;
				framesDelayed++;
			}
		}
		framesDelayed = 0;
		if (isSpawnedAllAtOnce)
		{
			yield return AddSpeedToEachWave(projectiles);
		}
	}

	private IEnumerator AddSpeedToEachWave(List<Projectile[]> projectiles)
	{
		for (int i = 0; i < projectiles.Count; i++)
		{
			AddSpeed(bulletWaves[i], projectiles[i]);
		}
		yield return null;
	}

	private void AddSpeed(BulletWave bulletWave, Projectile[] projectiles)
	{
		for (int i = 0; i < projectiles.Length; i++)
		{
			projectiles[i].ArrowImage.gameObject.SetActive(true);
			projectiles[i].Speed = ((globalSpeed == -1f) ? bulletWave.Speed : globalSpeed) + UnityEngine.Random.Range(bulletWave.Randomness.x, bulletWave.Randomness.y);
			projectiles[i].Acceleration = ((globalAcceleration == -1f) ? bulletWave.Acceleration : globalAcceleration) + UnityEngine.Random.Range(bulletWave.Randomness.x, bulletWave.Randomness.y);
			projectiles[i].Deceleration = bulletWave.Deceleration + UnityEngine.Random.Range(bulletWave.Randomness.x, bulletWave.Randomness.y);
		}
	}

	private Projectile[] SpawnBullets(BulletWave bulletWave, float playerAngle, Transform parent, Vector3 spawnPosition, bool addSpeedImmediately = true)
	{
		Projectile[] array = new Projectile[bulletWave.Bullets];
		float num = bulletWave.AngleBetweenBullets * (float)(bulletWave.Bullets - 1);
		float num2 = playerAngle - num / 2f + bulletWave.Offset;
		for (int i = 0; i < bulletWave.Bullets; i++)
		{
			if (!isBulletShotPlayedOnce)
			{
				AudioManager.Instance.PlayOneShot("event:/enemy/patrol_boss/patrol_boss_fire_projectile", base.gameObject);
				AudioManager.Instance.PlayOneShot("event:/enemy/shoot_acidslime", base.gameObject);
				isBulletShotPlayedOnce = true;
			}
			Projectile component = ObjectPool.Spawn(bulletPrefab, (parent != null) ? parent : base.transform.parent).GetComponent<Projectile>();
			component.transform.position = spawnPosition;
			component.Angle = num2;
			component.team = health.team;
			if (addSpeedImmediately)
			{
				component.Speed = ((globalSpeed == -1f) ? bulletWave.Speed : globalSpeed) + UnityEngine.Random.Range(bulletWave.Randomness.x, bulletWave.Randomness.y);
				component.Acceleration = ((globalAcceleration == -1f) ? bulletWave.Acceleration : globalAcceleration) + UnityEngine.Random.Range(bulletWave.Randomness.x, bulletWave.Randomness.y);
				component.Deceleration = bulletWave.Deceleration + UnityEngine.Random.Range(bulletWave.Randomness.x, bulletWave.Randomness.y);
			}
			else
			{
				component.Speed = 0f;
				component.Acceleration = 0f;
				component.Deceleration = 0f;
				component.ArrowImage.gameObject.SetActive(false);
			}
			array[i] = component;
			SpawnedProjectile();
			num2 = Mathf.Repeat(num2 + bulletWave.AngleBetweenBullets, 360f);
		}
		ProjectileWaveEvent onProjectileWaveShot = this.OnProjectileWaveShot;
		if (onProjectileWaveShot != null)
		{
			onProjectileWaveShot(bulletWave);
		}
		return array;
	}

	private int CountProjectiles(float maxCountTime = 0f)
	{
		int num = 0;
		float num2 = 0f;
		int num3 = 0;
		for (int i = 0; i < bulletWaves.Length; i++)
		{
			num += bulletWaves[i].Bullets;
			if (maxCountTime > 0f)
			{
				num2 += bulletWaves[i].FinishDelay;
				if (globalDelayBetweenGroups > 0f && bulletWaves[i].WaveGroupID != num3)
				{
					num3 = bulletWaves[i].WaveGroupID;
					num2 += globalDelayBetweenGroups;
				}
				if (num2 >= maxCountTime)
				{
					break;
				}
			}
		}
		return num;
	}

	private void OnDrawGizmos()
	{
		if (bulletWaves == null || !(distance > 0f) || !debug)
		{
			return;
		}
		float num = distance;
		int num2 = 0;
		BulletWave[] array = bulletWaves;
		for (int i = 0; i < array.Length; i++)
		{
			BulletWave bulletWave = array[i];
			if (bulletWave.WaveGroupID != num2 && globalDelayBetweenGroups != -1f)
			{
				num2 = bulletWave.WaveGroupID;
				num += globalDelayBetweenGroups / 3f;
			}
			float num3 = 0f;
			float num4 = bulletWave.AngleBetweenBullets * (float)(bulletWave.Bullets - 1);
			float num5 = Mathf.Repeat(Utils.GetAngle(Vector3.zero, base.transform.right) + (num3 - num4 / 2f + bulletWave.Offset), 360f);
			for (int j = 0; j < bulletWave.Bullets; j++)
			{
				Vector2 normalized = new Vector2(Mathf.Cos(num5 * ((float)Math.PI / 180f)), Mathf.Sin(num5 * ((float)Math.PI / 180f))).normalized;
				Utils.DrawCircleXY(Vector3.Lerp(base.transform.position, base.transform.position + (Vector3)normalized * 10f, num), 0.15f, Color.green);
				num5 = Mathf.Repeat(num5 + bulletWave.AngleBetweenBullets, 360f);
			}
			num += bulletWave.FinishDelay / 3f;
		}
	}
}
