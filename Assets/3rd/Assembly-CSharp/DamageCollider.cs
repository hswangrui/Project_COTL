using System.Collections.Generic;
using UnityEngine;

public class DamageCollider : BaseMonoBehaviour
{
	private Health health;

	public Collider2D damageCollider;

	public float Damage = 1f;

	public float DamageToObstacles = 100f;

	public float Knockback;

	public bool BreakArmor;

	public bool IgnorePlayer;

	public bool DeflectBullets;

	public bool DestroyBullets;

	public bool UseTriggerCollisionWithProjectiles;

	public Health.AttackTypes AttackType;

	public bool DealDamageToAllNonEnemyHealth;

	private List<Health> hitEnemies = new List<Health>();

	private List<Projectile> hitProjectiles = new List<Projectile>();

	private void Start()
	{
		health = GetComponentInParent<Health>();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Health component = collision.gameObject.GetComponent<Health>();
		if (component != null && (health == null || component.team != health.team || (component.team == Health.Team.PlayerTeam && health.IsCharmedEnemy)))
		{
			if ((IgnorePlayer && (component == PlayerFarming.Instance.health || component.isPlayerAlly)) || hitEnemies.Contains(component))
			{
				return;
			}
			hitEnemies.Add(health);
			if (collision.gameObject.layer == LayerMask.NameToLayer("Scenery") || collision.gameObject.layer == LayerMask.NameToLayer("Obstacles") || (DealDamageToAllNonEnemyHealth && component.team != Health.Team.Team2))
			{
				component.DealDamage(DamageToObstacles, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.8f), BreakArmor, AttackType);
				return;
			}
			component.DealDamage(Damage, base.gameObject, base.transform.position, BreakArmor, AttackType);
			if (Knockback != 0f)
			{
				UnitObject component2 = component.GetComponent<UnitObject>();
				if ((object)component2 != null)
				{
					component2.DoKnockBack(base.gameObject, Knockback, 1f);
				}
			}
		}
		else
		{
			if (!UseTriggerCollisionWithProjectiles)
			{
				return;
			}
			Projectile componentInParent = collision.GetComponentInParent<Projectile>();
			if ((bool)componentInParent && !componentInParent.IsProjectilesParent && !hitProjectiles.Contains(componentInParent))
			{
				hitProjectiles.Add(componentInParent);
				if ((bool)componentInParent && DeflectBullets)
				{
					DeflectProjectile(componentInParent);
				}
				DamageProjetile(componentInParent);
			}
		}
	}

	public void TriggerCheckCollision()
	{
		if (UseTriggerCollisionWithProjectiles)
		{
			return;
		}
		for (int i = 0; i < Projectile.Projectiles.Count; i++)
		{
			Projectile projectile = Projectile.Projectiles[i];
			if (projectile.IsProjectilesParent || hitProjectiles.Contains(projectile))
			{
				continue;
			}
			Vector2 point = projectile.transform.position;
			if (damageCollider.OverlapPoint(point))
			{
				hitProjectiles.Add(projectile);
				if (DeflectBullets)
				{
					DeflectProjectile(projectile);
				}
				DamageProjetile(projectile);
			}
		}
	}

	private void DamageProjetile(Projectile projectile)
	{
		if ((bool)projectile && DestroyBullets && !projectile.IsAttachedToProjectileTrap() && !projectile.IsProjectilesParent)
		{
			projectile.DestroyProjectile(true);
		}
		if ((bool)health && health.team == Health.Team.Neutral)
		{
			health.DealDamage(10f, base.gameObject, base.transform.position, false, Health.AttackTypes.Projectile);
		}
	}

	private void DeflectProjectile(Projectile projectile)
	{
		if (!projectile.IsAttachedToProjectileTrap() && !projectile.IsProjectilesParent)
		{
			float angle = Utils.GetAngle(base.transform.position, projectile.transform.position);
			projectile.Angle = angle;
			if (projectile.angleNoiseFrequency == 0f)
			{
				projectile.Speed *= 2f;
			}
			projectile.KnockedBack = true;
			projectile.health.team = Health.Team.PlayerTeam;
			projectile.team = Health.Team.PlayerTeam;
		}
	}
}
