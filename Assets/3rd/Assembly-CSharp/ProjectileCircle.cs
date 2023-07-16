using System;
using System.Collections;
using CotL.Projectiles;
using DG.Tweening;
using UnityEngine;

public class ProjectileCircle : ProjectileCircleBase
{
	[SerializeField]
	private float durationTillMaxRadius;

	[SerializeField]
	private Projectile[] projectiles;

	private Projectile projectile;

	private void OnDisable()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public override void Init(float radius)
	{
		projectile = GetComponent<Projectile>();
		float num = 0f;
		float num2 = 360f / (float)projectiles.Length;
		for (int i = 0; i < projectiles.Length; i++)
		{
			Vector2 vector = Utils.DegreeToVector2(num) * radius;
			projectiles[i].transform.localPosition = Vector3.zero;
			projectiles[i].transform.DOLocalMove(vector, durationTillMaxRadius);
			projectiles[i].health = projectile.health;
			projectiles[i].team = projectile.health.team;
			projectiles[i].enabled = false;
			num += num2;
		}
	}

	public override void InitDelayed(GameObject target, float radius, float shootDelay, Action onShoot = null)
	{
		projectile = GetComponent<Projectile>();
		float num = 0f;
		float num2 = 360f / (float)projectiles.Length;
		Projectile[] array = projectiles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(false);
		}
		for (int j = 0; j < projectiles.Length; j++)
		{
			Vector2 vector = Utils.DegreeToVector2(num) * radius;
			projectiles[j].transform.localPosition = Vector3.zero;
			projectiles[j].transform.localPosition = vector;
			projectiles[j].team = projectile.health.team;
			projectiles[j].enabled = false;
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
