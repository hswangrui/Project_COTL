using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace CotL.Projectiles
{
	public class ProjectileCirclePattern : ProjectileCircleBase
	{
		[SerializeField]
		private float durationTillMaxRadius;

		[SerializeField]
		private Projectile projectilePrefab;

		[SerializeField]
		private int baseProjectilesCount = 15;

		private Projectile[] projectiles;

		private Projectile projectile;

		public Projectile ProjectilePrefab
		{
			get
			{
				return projectilePrefab;
			}
		}

		public int BaseProjectilesCount
		{
			get
			{
				return baseProjectilesCount;
			}
		}

		private void Awake()
		{
			projectile = GetComponent<Projectile>();
			projectiles = new Projectile[baseProjectilesCount];
			InitializeProjectiles();
		}

		private void InitializeProjectiles()
		{
			if (ObjectPool.CountPooled(projectilePrefab) == 0)
			{
				ObjectPool.CreatePool(projectilePrefab, baseProjectilesCount);
			}
		}

		public override void Init(float radius)
		{
			float num = 0f;
			float num2 = 360f / (float)baseProjectilesCount;
			for (int i = 0; i < baseProjectilesCount; i++)
			{
				Projectile projectile = ObjectPool.Spawn(projectilePrefab, base.transform);
				Vector2 vector = Utils.DegreeToVector2(num) * radius;
				projectile.transform.localPosition = Vector3.zero;
				projectile.transform.DOLocalMove(vector, durationTillMaxRadius);
				projectile.health = this.projectile.health;
				projectile.team = Health.Team.Team2;
				projectile.enabled = false;
				projectiles[i] = projectile;
				num += num2;
			}
		}

		public override void InitDelayed(GameObject target, float radius, float shootDelay, Action onShoot = null)
		{
			float num = 0f;
			float num2 = 360f / (float)baseProjectilesCount;
			for (int i = 0; i < baseProjectilesCount; i++)
			{
				Projectile projectile = ObjectPool.Spawn(projectilePrefab, base.transform);
				Vector2 vector = Utils.DegreeToVector2(num) * radius;
				projectile.transform.localPosition = Vector3.zero;
				projectile.transform.localPosition = vector;
				projectile.health = this.projectile.health;
				projectile.team = Health.Team.Team2;
				projectile.enabled = false;
				projectile.gameObject.SetActive(false);
				projectiles[i] = projectile;
				num += num2;
			}
			StartCoroutine(EnableProjectiles(target, shootDelay, onShoot));
		}

		private IEnumerator EnableProjectiles(GameObject target, float delay, Action onShoot)
		{
			this.projectile.SpeedMultiplier = 0f;
			Projectile[] array = projectiles;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(true);
				yield return new WaitForSeconds(0.02f);
			}
			yield return new WaitForSeconds(0.25f + delay);
			Projectile[] array2 = projectiles;
			foreach (Projectile projectile in array2)
			{
				if ((bool)projectile)
				{
					projectile.enabled = true;
				}
			}
			if ((bool)target)
			{
				this.projectile.Angle = Utils.GetAngle(base.transform.position, target.transform.position);
			}
			if (onShoot != null)
			{
				onShoot();
			}
			this.projectile.SpeedMultiplier = 1f;
		}

		public void TargetMiddle(float speed, float lifetime, float acceleration)
		{
			Projectile[] array = projectiles;
			foreach (Projectile obj in array)
			{
				obj.SpeedMultiplier = speed;
				obj.LifeTime = lifetime;
				obj.Acceleration = acceleration;
				obj.Angle = Utils.GetAngle(obj.transform.position, base.transform.position);
				obj.GetComponent<Rigidbody2D>().isKinematic = false;
				obj.enabled = true;
			}
		}

		public void TargetMiddleInverse(float speed, float lifetime, float acceleration)
		{
			Projectile[] array = projectiles;
			foreach (Projectile projectile in array)
			{
				projectile.SpeedMultiplier = speed;
				projectile.LifeTime = lifetime;
				projectile.Acceleration = acceleration;
				projectile.Angle = Utils.GetAngle(base.transform.position, projectile.transform.position);
				projectile.GetComponent<Rigidbody2D>().isKinematic = false;
				projectile.enabled = true;
			}
		}
	}
}
