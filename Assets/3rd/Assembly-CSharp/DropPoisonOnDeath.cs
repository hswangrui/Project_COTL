using System;
using UnityEngine;

public class DropPoisonOnDeath : BaseMonoBehaviour
{
	[Serializable]
	private enum SpawnType
	{
		OnDeath,
		OnDisable
	}

	[SerializeField]
	private GameObject poisonPrefab;

	[SerializeField]
	private SpawnType spawnType;

	[SerializeField]
	private int amount;

	[SerializeField]
	private float radius;

	private Health health;

	private void Awake()
	{
		health = GetComponent<Health>();
		health.OnDie += OnDie;
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (spawnType == SpawnType.OnDeath)
		{
			SpawnPoison();
		}
	}

	private void OnDisable()
	{
		if (spawnType == SpawnType.OnDisable)
		{
			SpawnPoison();
		}
	}

	private void SpawnPoison()
	{
		for (int i = 0; i < amount; i++)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle * radius;
			UnityEngine.Object.Instantiate(poisonPrefab, base.transform.position + (Vector3)vector, Quaternion.identity, base.transform.parent);
		}
	}
}
