using System.Collections;
using UnityEngine;

public class EnemyHopperMortar : EnemyHopperBurp
{
	protected const float aimDuration = 1f;

	protected const float minBombRange = 2.5f;

	protected const float maxBombRange = 5f;

	protected const float timeBetweenShots = 0.15f;

	public float bombDuration = 0.75f;

	public override void OnEnable()
	{
		base.OnEnable();
		minTimeBetweenBurps = 2f;
		chargingAnimationString = "burp";
	}

	protected override void UpdateStateIdle()
	{
		if (targetObject != null)
		{
			float magnitude = (targetObject.transform.position - base.transform.position).magnitude;
			isFleeing = magnitude > attackRange;
		}
		base.UpdateStateIdle();
	}

	protected override void UpdateStateCharging()
	{
		SimpleSpineFlash.FlashMeWhite();
		if (gm.TimeSince(chargingTimestamp) >= chargingDuration / Spine[0].timeScale)
		{
			BurpFlies();
			if (ShouldStartCharging() && Random.Range(0f, 1f) < 0.5f)
			{
				state.CURRENT_STATE = StateMachine.State.Charging;
			}
			else
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
		}
	}

	protected override bool ShouldStartCharging()
	{
		if (targetObject == null)
		{
			return false;
		}
		return base.ShouldStartCharging();
	}

	protected override IEnumerator ShootProjectileRoutine()
	{
		if (targetObject == null)
		{
			EnemyHopperMortar enemyHopperMortar = this;
			Health closestTarget = GetClosestTarget();
			enemyHopperMortar.targetObject = (((object)closestTarget != null) ? closestTarget.gameObject : null);
		}
		if (targetObject == null)
		{
			yield break;
		}
		state.facingAngle = Utils.GetAngle(base.transform.position, targetObject.transform.position);
		Vector3 targetPosition = targetObject.transform.position;
		for (int i = 0; i < ShotsToFire; i++)
		{
			if (targetObject == null)
			{
				break;
			}
			if (ShotsToFire > 1)
			{
				targetPosition = targetObject.transform.position + (Vector3)Random.insideUnitCircle * 2f;
			}
			MortarBomb component = Object.Instantiate(projectilePrefab, targetObject.transform.position, Quaternion.identity, base.transform.parent).GetComponent<MortarBomb>();
			if (Vector2.Distance(base.transform.position, targetPosition) < 2.5f)
			{
				component.transform.position = base.transform.position + (targetPosition - base.transform.position).normalized * 2.5f;
			}
			else
			{
				component.transform.position = base.transform.position + (targetPosition - base.transform.position).normalized * 5f;
			}
			component.Play(base.transform.position + new Vector3(0f, 0f, -1.5f), bombDuration, Health.Team.Team2);
			SimpleSpineFlash.FlashWhite(false);
			float time = 0f;
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * Spine[0].timeScale);
				if (!(num < 0.15f))
				{
					break;
				}
				yield return null;
			}
		}
	}
}
