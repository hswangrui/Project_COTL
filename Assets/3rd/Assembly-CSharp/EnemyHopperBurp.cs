using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHopperBurp : EnemyHopper
{
	public int ShotsToFire = 10;

	public float DetectEnemyRange = 10f;

	public GameObject projectilePrefab;

	protected float LookAngle;

	[SerializeField]
	protected float minTimeBetweenBurps = 6f;

	protected float lastBurpedFliesTimestamp;

	protected List<Projectile> activeProjectiles = new List<Projectile>();

	public override void OnEnable()
	{
		base.OnEnable();
		if (gm != null)
		{
			lastBurpedFliesTimestamp = gm.CurrentTime - Random.Range(0f, minTimeBetweenBurps);
		}
	}

	protected override void UpdateStateCharging()
	{
		SimpleSpineFlash.FlashMeWhite();
		if (gm.TimeSince(chargingTimestamp) >= chargingDuration / Spine[0].timeScale)
		{
			BurpFlies();
			state.CURRENT_STATE = StateMachine.State.Idle;
		}
	}

	protected virtual void BurpFlies()
	{
		if (gm != null)
		{
			lastBurpedFliesTimestamp = gm.CurrentTime;
		}
		if (!string.IsNullOrEmpty(OnCroakSoundPath))
		{
			AudioManager.Instance.PlayOneShot(OnCroakSoundPath, base.transform.position);
		}
		SimpleSpineFlash.FlashWhite(false);
		StartCoroutine(ShootProjectileRoutine());
	}

	protected virtual IEnumerator ShootProjectileRoutine()
	{
		CameraManager.shakeCamera(0.2f, LookAngle);
		List<float> shootAngles = new List<float>(ShotsToFire);
		for (int j = 0; j < ShotsToFire; j++)
		{
			shootAngles.Add(360f / (float)ShotsToFire * (float)j);
		}
		shootAngles.Shuffle();
		Health h = GetClosestTarget();
		float initAngle = Random.Range(0f, 360f);
		for (int i = 0; i < shootAngles.Count; i++)
		{
			Projectile component = Object.Instantiate(projectilePrefab, base.transform.parent).GetComponent<Projectile>();
			component.transform.position = base.transform.position;
			component.Angle = initAngle + shootAngles[i];
			component.team = health.team;
			component.Speed += Random.Range(-0.5f, 0.5f);
			component.turningSpeed += Random.Range(-0.1f, 0.1f);
			component.angleNoiseFrequency += Random.Range(-0.1f, 0.1f);
			component.LifeTime += Random.Range(0f, 0.3f);
			component.Owner = health;
			component.SetTarget(h);
			activeProjectiles.Add(component);
			float time = 0f;
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * Spine[0].timeScale);
				if (!(num < 0.03f))
				{
					break;
				}
				yield return null;
			}
		}
	}

	protected override bool ShouldStartCharging()
	{
		if (!GameManager.RoomActive)
		{
			return false;
		}
		if (gm.TimeSince(lastBurpedFliesTimestamp) >= minTimeBetweenBurps / Spine[0].timeScale)
		{
			return IsPlayerNearby();
		}
		return false;
	}

	protected bool IsPlayerNearby()
	{
		if (!GameManager.RoomActive)
		{
			return false;
		}
		foreach (Health allUnit in Health.allUnits)
		{
			if (allUnit.team != health.team && !allUnit.InanimateObject && allUnit.team != 0 && (health.team != Health.Team.PlayerTeam || (health.team == Health.Team.PlayerTeam && allUnit.team != Health.Team.DangerousAnimals)) && Vector2.Distance(base.transform.position, allUnit.gameObject.transform.position) < DetectEnemyRange)
			{
				return true;
			}
		}
		return false;
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		for (int i = 0; i < activeProjectiles.Count; i++)
		{
			if (activeProjectiles[i] != null && activeProjectiles[i].gameObject.activeSelf)
			{
				activeProjectiles[i].DestroyWithVFX();
			}
		}
		activeProjectiles.Clear();
	}
}
