using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class DropMultipleLootOnDeath : BaseMonoBehaviour
{
	[Serializable]
	public class ItemAndProbability
	{
		public InventoryItem.ITEM_TYPE Type;

		[Range(1f, 100f)]
		public int Probability = 1;

		public ItemAndProbability(InventoryItem.ITEM_TYPE Type, int Probability)
		{
			this.Type = Type;
			this.Probability = Probability;
		}
	}

	private Health health;

	public List<ItemAndProbability> LootToDrop = new List<ItemAndProbability>();

	public bool useGenericForestEnemyLoot;

	public bool IsNaturalResource;

	public Vector2 RandomAmountToDrop = Vector2.one;

	[Range(0f, 100f)]
	public float chanceToDropLoot = 100f;

	private void OnEnable()
	{
		if (health == null)
		{
			health = GetComponent<Health>();
		}
		if (health != null)
		{
			health.OnDie += OnDie;
		}
	}

	private void OnDisable()
	{
		if (health != null)
		{
			health.OnDie -= OnDie;
		}
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (Attacker != null)
		{
			CameraManager.shakeCamera(0.25f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		}
		for (int num = LootToDrop.Count - 1; num >= 0; num--)
		{
			if (LootToDrop[num].Type == InventoryItem.ITEM_TYPE.MUSHROOM_SMALL && DataManager.Instance.SozoStoryProgress == -1)
			{
				LootToDrop.RemoveAt(num);
				break;
			}
		}
		int[] array = new int[LootToDrop.Count];
		int num2 = -1;
		while (++num2 < LootToDrop.Count)
		{
			array[num2] = LootToDrop[num2].Probability;
		}
		for (int i = 0; (float)i < UnityEngine.Random.Range(RandomAmountToDrop.x, RandomAmountToDrop.y + 1f); i++)
		{
			if (!(UnityEngine.Random.Range(0f, 100f) > chanceToDropLoot * DifficultyManager.GetLuckMultiplier()))
			{
				int randomWeightedIndex = Utils.GetRandomWeightedIndex(array);
				InventoryItem.ITEM_TYPE type = LootToDrop[randomWeightedIndex].Type;
				int num3 = 1;
				if (IsNaturalResource)
				{
					num3 += TrinketManager.GetLootIncreaseModifier(type);
					num3 += UpgradeSystem.GetForageIncreaseModifier;
				}
				InventoryItem.Spawn(type, num3, base.transform.position);
			}
		}
	}

	public void spawnLoot()
	{
		int[] array = new int[LootToDrop.Count];
		int num = -1;
		while (++num < LootToDrop.Count)
		{
			array[num] = LootToDrop[num].Probability;
		}
		int randomWeightedIndex = Utils.GetRandomWeightedIndex(array);
		InventoryItem.Spawn(LootToDrop[randomWeightedIndex].Type, 1, base.transform.position);
	}
}
