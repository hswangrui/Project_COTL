using System;
using System.Collections;
using UnityEngine;

public class ProjectilePatternBeam : ProjectilePatternBase
{
	[Serializable]
	public struct BulletWave
	{
		public int Bullets;

		public float Speed;

		public float Acceleration;

		public float Deceleration;

		public float BulletOffset;

		public float Offset;

		public float OffsetIncrement;

		public float DelayBetweenBullets;

		[Space]
		public Vector2 SinMinMax;

		public float SinAmountPerBullet;

		public bool InvertSin;

		[Space]
		public bool TargetPlayer;

		public Vector2 Randomness;
	}

	[Space]
	[SerializeField]
	private Projectile bulletPrefab;

	[SerializeField]
	private BulletWave[] bulletWaves;

	[Space]
	[SerializeField]
	private bool ContinuouslyTargetPlayer = true;

	[SerializeField]
	private float distance;

	[SerializeField]
	private bool debug;

	private Health health;

	public BulletWave[] BulletWaves
	{
		get
		{
			return bulletWaves;
		}
	}

	private void Start()
	{
		health = GetComponent<Health>();
		if ((bool)health)
		{
			health.OnDie += Health_OnDie;
		}
		ObjectPool.CreatePool(bulletPrefab, 25);
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

	public void Shoot(float delay)
	{
		StartCoroutine(ShootIE(delay));
	}

	public override IEnumerator ShootIE(float delay = 0f, GameObject target = null, Transform parent = null)
	{
		if (delay != 0f)
		{
			float time = 0f;
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * base.timeScale);
				if (!(num < delay))
				{
					break;
				}
				yield return null;
			}
		}
		BulletWave[] array = bulletWaves;
		foreach (BulletWave bulletWave in array)
		{
			StartCoroutine(SpawnBullets(bulletWave, target, parent));
		}
	}

	private IEnumerator SpawnBullets(BulletWave bulletWave, GameObject target, Transform parent)
	{
		if (health == null)
		{
			health = GetComponent<Health>();
		}
		target = ((target == null) ? PlayerFarming.Instance.gameObject : target);
		float targetAngle = ((bulletWave.TargetPlayer && target != null) ? Utils.GetAngle(base.transform.position, target.transform.position) : Utils.GetAngle(Vector3.zero, base.transform.right));
		float pingPongTime = 0f;
		float offsetIncrement = 0f;
		for (int i = 0; i < bulletWave.Bullets; i++)
		{
			if (ContinuouslyTargetPlayer && bulletWave.TargetPlayer && target != null)
			{
				targetAngle = Utils.GetAngle(base.transform.position, target.transform.position);
			}
			else if (!bulletWave.TargetPlayer || target == null)
			{
				targetAngle = Utils.GetAngle(Vector3.zero, base.transform.right);
			}
			float num = targetAngle + bulletWave.Offset;
			float num2 = (Mathf.PingPong(pingPongTime, bulletWave.SinMinMax.y - bulletWave.SinMinMax.x) + bulletWave.SinMinMax.x) * (float)((!bulletWave.InvertSin) ? 1 : (-1));
			float angle = Mathf.Repeat(num + offsetIncrement + num2, 360f);
			Projectile component = ObjectPool.Spawn(bulletPrefab, (parent != null) ? parent : base.transform.parent, base.transform.position, Quaternion.Euler(0f, 0f, 0f)).GetComponent<Projectile>();
			component.transform.position = base.transform.position - Vector3.forward * 0.2f;
			component.Angle = angle;
			component.team = ((health != null) ? health.team : Health.Team.Team2);
			component.Speed = bulletWave.Speed + UnityEngine.Random.Range(bulletWave.Randomness.x, bulletWave.Randomness.y);
			component.Acceleration = bulletWave.Acceleration + UnityEngine.Random.Range(bulletWave.Randomness.x, bulletWave.Randomness.y);
			component.Deceleration = bulletWave.Deceleration + UnityEngine.Random.Range(bulletWave.Randomness.x, bulletWave.Randomness.y);
			SpawnedProjectile();
			offsetIncrement += bulletWave.OffsetIncrement;
			pingPongTime += bulletWave.SinAmountPerBullet;
			if (bulletWave.DelayBetweenBullets == 0f)
			{
				continue;
			}
			float time = 0f;
			while (true)
			{
				float num3;
				time = (num3 = time + Time.deltaTime * base.timeScale);
				if (!(num3 < bulletWave.DelayBetweenBullets))
				{
					break;
				}
				yield return null;
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (bulletWaves == null || !(distance > 0f) || !debug)
		{
			return;
		}
		BulletWave[] array = bulletWaves;
		for (int i = 0; i < array.Length; i++)
		{
			BulletWave bulletWave = array[i];
			float num = distance;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = Mathf.Repeat(Utils.GetAngle(Vector3.zero, base.transform.right) + num2 - num3 / 2f + bulletWave.Offset, 360f);
			float num5 = 0f;
			for (int j = 0; j < bulletWave.Bullets; j++)
			{
				float num6 = num4;
				num6 += (Mathf.PingPong(num5, bulletWave.SinMinMax.y - bulletWave.SinMinMax.x) + bulletWave.SinMinMax.x) * (float)((!bulletWave.InvertSin) ? 1 : (-1));
				Vector2 normalized = new Vector2(Mathf.Cos(num6 * ((float)Math.PI / 180f)), Mathf.Sin(num6 * ((float)Math.PI / 180f))).normalized;
				Utils.DrawCircleXY(Vector3.Lerp(base.transform.position, base.transform.position + (Vector3)normalized * 10f, num), 0.15f, Color.green);
				num4 = Mathf.Repeat(num4 + bulletWave.OffsetIncrement, 360f);
				num += bulletWave.DelayBetweenBullets / 3f;
				num5 += bulletWave.SinAmountPerBullet;
			}
		}
	}
}
