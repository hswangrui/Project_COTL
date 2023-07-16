using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScout : EnemySwordsman
{
	private List<Vector3> TeleportPositions;

	private Vector3 Direction;

	private RaycastHit2D Results;

	private float SpawnDelay = 0.2f;

	private int SummonedCount;

	public List<GameObject> EnemiesToSpawn = new List<GameObject>();

	private float Distance = 2f;

	private float SpawnCircleCastRadius = 1f;

	private void SpawnMoreEnemies()
	{
		if (GetAvailableSpawnPositions().Count > 0)
		{
			StopAllCoroutines();
			StartCoroutine(SpawnEnemiesRoutine());
		}
	}

	private List<Vector3> GetAvailableSpawnPositions()
	{
		TeleportPositions = new List<Vector3>();
		int num = -3;
		while ((num += 2) <= 1)
		{
			Angle = (state.LookAngle + (float)(45 * num)) * ((float)Math.PI / 180f);
			Direction = new Vector3(Mathf.Cos(Angle), Mathf.Sin(Angle));
			Results = Physics2D.CircleCast(base.transform.position, SpawnCircleCastRadius, Direction, Distance, layerToCheck);
			if (Results.collider == null)
			{
				TeleportPositions.Add(base.transform.position + Direction * Distance);
			}
		}
		if (TeleportPositions.Count <= 0)
		{
			Direction = new Vector3(Mathf.Cos(state.LookAngle), Mathf.Sin(state.LookAngle));
			Results = Physics2D.CircleCast(base.transform.position, SpawnCircleCastRadius, Direction, Distance, layerToCheck);
			if (Results.collider == null)
			{
				TeleportPositions.Add(base.transform.position + Direction * Distance);
			}
		}
		return TeleportPositions;
	}

	public override bool CustomAttackLogic()
	{
		if ((SpawnDelay += Time.deltaTime) > 0.2f && SummonedCount <= 0)
		{
			SpawnDelay = 0f;
			SpawnMoreEnemies();
			return true;
		}
		return false;
	}

	private IEnumerator SpawnEnemiesRoutine()
	{
		Spine.AnimationState.SetAnimation(0, "alarm", false);
		Spine.AnimationState.AddAnimation(0, "dance", true, 0f);
		yield return new WaitForSeconds(5f / 6f);
		foreach (Vector3 teleportPosition in TeleportPositions)
		{
			EnemySpawner.Create(teleportPosition, base.transform.parent, EnemiesToSpawn[UnityEngine.Random.Range(0, EnemiesToSpawn.Count)]).GetComponent<Health>().OnDie += RemoveSpawned;
			SummonedCount++;
		}
		yield return new WaitForSeconds(1.2666667f);
		StartCoroutine(WaitForTarget());
	}

	private void RemoveSpawned(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		SummonedCount--;
		Victim.OnDie -= RemoveSpawned;
	}

	private void OnDrawGizmos()
	{
		if (state == null)
		{
			return;
		}
		int num = -3;
		while ((num += 2) <= 1)
		{
			float f = (state.LookAngle + (float)(45 * num)) * ((float)Math.PI / 180f);
			Vector3 vector = new Vector3(Mathf.Cos(f), Mathf.Sin(f));
			RaycastHit2D raycastHit2D = Physics2D.CircleCast(base.transform.position, SpawnCircleCastRadius, vector, Distance, layerToCheck);
			Color green = Color.green;
			if (raycastHit2D.collider != null)
			{
				Utils.DrawCircleXY(raycastHit2D.centroid, SpawnCircleCastRadius, Color.red);
			}
			else
			{
				Utils.DrawCircleXY(base.transform.position + vector * Distance, SpawnCircleCastRadius, Color.green);
			}
		}
	}
}
