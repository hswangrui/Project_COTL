using System.Collections;
using UnityEngine;

public class BlastPush : BaseMonoBehaviour, ICurseProduct
{
	[SerializeField]
	private float force = 10f;

	[SerializeField]
	private float radius = 3f;

	[SerializeField]
	private bool dealDamage = true;

	private float LifeTime = 2f;

	private float Timer;

	private void Start()
	{
		if (dealDamage)
		{
			StartCoroutine(PushEnemies());
		}
	}

	private void OnDisable()
	{
		Object.Destroy(base.gameObject);
	}

	private IEnumerator PushEnemies()
	{
		yield return new WaitForSeconds(0.3f);
		Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position, radius);
		foreach (Collider2D collider2D in array)
		{
			UnitObject component = collider2D.GetComponent<UnitObject>();
			if ((bool)component && component.health.team == Health.Team.Team2)
			{
				float num = 1f - Vector3.Distance(base.transform.position, collider2D.transform.position) / radius;
				component.DoKnockBack(base.gameObject, num * force, 1f);
				component.health.DealDamage(EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).Damage * PlayerSpells.GetCurseDamageMultiplier(), base.gameObject, base.transform.position, false, Health.AttackTypes.Projectile);
				if (IsPoisonPush())
				{
					component.health.AddPoison(PlayerFarming.Instance.gameObject);
				}
				else if (IsIcePush())
				{
					component.health.AddIce();
				}
				continue;
			}
			Health component2 = collider2D.GetComponent<Health>();
			if (!component2)
			{
				continue;
			}
			if (component2.team == Health.Team.Team2)
			{
				if (IsPoisonPush())
				{
					component2.DealDamage(10f, base.gameObject, base.transform.position, false, Health.AttackTypes.Projectile, false, Health.AttackFlags.Poison);
				}
				else if (IsIcePush())
				{
					component2.DealDamage(10f, base.gameObject, base.transform.position, false, Health.AttackTypes.Projectile, false, Health.AttackFlags.Ice);
				}
				else
				{
					component2.DealDamage(10f, base.gameObject, base.transform.position, false, Health.AttackTypes.Projectile);
				}
			}
			else if (component2.team != Health.Team.PlayerTeam)
			{
				component2.DealDamage(10f, base.gameObject, base.transform.position, false, Health.AttackTypes.Projectile);
			}
		}
		if (EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).EquipmentType == EquipmentType.EnemyBlast_DeflectsProjectiles)
		{
			KnockbackProjectiles();
		}
		else
		{
			DestroyProjectiles();
		}
		CameraManager.instance.ShakeCameraForDuration(0.8f, 1f, 0.1f);
	}

	private void Update()
	{
		if ((Timer += Time.deltaTime) > LifeTime)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void KnockbackProjectiles()
	{
		Vector3 position = base.transform.position;
		foreach (Projectile projectile in Projectile.Projectiles)
		{
			if (projectile.IsProjectilesParent)
			{
				continue;
			}
			if (projectile.destroyOnParry)
			{
				projectile.DestroyProjectile(true);
				continue;
			}
			Vector3 position2 = projectile.transform.position;
			if ((position - position2).magnitude <= radius && !projectile.IsAttachedToProjectileTrap())
			{
				float angle = Utils.GetAngle(position, position2);
				projectile.Angle = angle;
				if (projectile.angleNoiseFrequency == 0f)
				{
					projectile.Speed *= 2f;
				}
				projectile.KnockedBack = true;
				projectile.team = Health.Team.PlayerTeam;
				projectile.health.team = Health.Team.PlayerTeam;
			}
		}
	}

	private void DestroyProjectiles()
	{
		Vector3 position = base.transform.position;
		foreach (Projectile projectile in Projectile.Projectiles)
		{
			if (!projectile.IsProjectilesParent)
			{
				Vector3 position2 = projectile.transform.position;
				if ((position - position2).magnitude <= radius && !projectile.IsAttachedToProjectileTrap())
				{
					projectile.DestroyProjectile(true);
				}
			}
		}
	}

	private bool IsPoisonPush()
	{
		if (DataManager.Instance.CurrentCurse == EquipmentType.EnemyBlast_Poison)
		{
			return Random.value <= EquipmentManager.GetCurseData(EquipmentType.EnemyBlast_Poison).Chance;
		}
		return false;
	}

	private bool IsIcePush()
	{
		if (DataManager.Instance.CurrentCurse == EquipmentType.EnemyBlast_Ice)
		{
			return Random.value <= EquipmentManager.GetCurseData(EquipmentType.EnemyBlast_Ice).Chance;
		}
		return false;
	}
}
