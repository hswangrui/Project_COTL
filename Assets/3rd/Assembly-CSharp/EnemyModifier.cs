using System.Collections.Generic;
using MMBiomeGeneration;
using UnityEngine;

[CreateAssetMenu(menuName = "COTL/Enemy Modifier")]
public class EnemyModifier : ScriptableObject
{
	public enum ModifierType
	{
		DropPoison,
		DropProjectiles,
		DropBomb
	}

	public Sprite ModifierIconSprite;

	public GameObject ModifierIcon;

	public ModifierType Modifier;

	public bool HasTimer;

	public float HealthMultiplier = 1.5f;

	[Space]
	[Range(0f, 1f)]
	public float Probability = 0.5f;

	[Space]
	public Color ColorTint = new Color(1f, 1f, 1f, 1f);

	public float Scale = 1f;

	public static float ChanceOfModifier = 0.035f;

	private static EnemyModifier[] modifiers;

	public static bool ForceModifiers = false;

	public static EnemyModifier GetModifier(float increaseChance = 0f)
	{
		if (DataManager.Instance != null && DataManager.Instance.dungeonRun <= 3 && (bool)BiomeGenerator.Instance && !BiomeGenerator.Instance.TestStartingLayer && !ForceModifiers)
		{
			return null;
		}
		float num = Random.Range(0f, 1f - increaseChance);
		if (ChanceOfModifier * DataManager.Instance.EnemyModifiersChanceMultiplier >= num)
		{
			if (modifiers == null)
			{
				modifiers = Resources.LoadAll<EnemyModifier>("Data/Enemy Modifiers");
			}
			for (int i = 0; i < 100; i++)
			{
				EnemyModifier enemyModifier = modifiers[Random.Range(0, modifiers.Length)];
				num = Random.Range(0f, 1f);
				if (enemyModifier.Probability >= num)
				{
					return enemyModifier;
				}
			}
		}
		return null;
	}

	public static EnemyModifier GetModifierExcluding(List<ModifierType> excludingTypes)
	{
		if (modifiers == null)
		{
			modifiers = Resources.LoadAll<EnemyModifier>("Data/Enemy Modifiers");
		}
		List<EnemyModifier> list = new List<EnemyModifier>();
		EnemyModifier[] array = modifiers;
		foreach (EnemyModifier enemyModifier in array)
		{
			if (!excludingTypes.Contains(enemyModifier.Modifier))
			{
				list.Add(enemyModifier);
			}
		}
		if (list.Count > 0)
		{
			return list[Random.Range(0, list.Count)];
		}
		return null;
	}
}
