using System;
using System.Collections.Generic;
using MMBiomeGeneration;
using UnityEngine;

public class RandomResource : BaseMonoBehaviour
{
	[Serializable]
	public class Resource
	{
		public InventoryItem.ITEM_TYPE ResourceType;

		public List<GameObject> Prefab = new List<GameObject>();

		[Range(0f, 100f)]
		public int Probability = 50;
	}

	public List<Resource> Resources = new List<Resource>();

	private int value;

	private int[] Weights;

	private void Awake()
	{
		PlaceRandom();
	}

	private void PlaceNext()
	{
		ClearPrefabs();
		value = ++value % Resources.Count;
		PlaceResource();
	}

	private void TestRandom()
	{
		System.Random random = new System.Random(UnityEngine.Random.Range(-2147483647, int.MaxValue));
		Weights = new int[Resources.Count];
		int num = -1;
		while (++num < Resources.Count)
		{
			Weights[num] = Resources[num].Probability;
		}
		value = GetRandomWeightedIndex(Weights, random.NextDouble());
	}

	private void PlaceRandom()
	{
		ClearPrefabs();
		if (BiomeGenerator.Instance != null && BiomeGenerator.Instance.ForceResource)
		{
			Weights = new int[BiomeGenerator.Instance.Resources.Count];
			int num = -1;
			while (++num < BiomeGenerator.Instance.Resources.Count)
			{
				Weights[num] = BiomeGenerator.Instance.Resources[num].Probability;
			}
			BiomeGenerator instance = BiomeGenerator.Instance;
			if ((((object)instance != null) ? instance.CurrentRoom : null) != null)
			{
				value = GetRandomWeightedIndex(Weights, BiomeGenerator.Instance.CurrentRoom.RandomSeed.NextDouble());
			}
			else
			{
				value = GetRandomWeightedIndex(Weights, UnityEngine.Random.Range(0f, 1f));
			}
			num = -1;
			while (++num < Resources.Count)
			{
				if (Resources[num].ResourceType == BiomeGenerator.Instance.Resources[value].ResourceType)
				{
					value = num;
					break;
				}
			}
		}
		else
		{
			Weights = new int[Resources.Count];
			int num2 = -1;
			while (++num2 < Resources.Count)
			{
				Weights[num2] = Resources[num2].Probability;
			}
			BiomeGenerator instance2 = BiomeGenerator.Instance;
			if ((((object)instance2 != null) ? instance2.CurrentRoom : null) != null)
			{
				value = GetRandomWeightedIndex(Weights, BiomeGenerator.Instance.CurrentRoom.RandomSeed.NextDouble());
			}
			else
			{
				value = GetRandomWeightedIndex(Weights, UnityEngine.Random.Range(0f, 1f));
			}
		}
		PlaceResource();
	}

	private void PlaceResource()
	{
		GameObject obj = UnityEngine.Object.Instantiate(Resources[value].Prefab[UnityEngine.Random.Range(0, Resources[value].Prefab.Count)]);
		obj.transform.parent = base.transform;
		obj.transform.localPosition = Vector3.zero;
	}

	public static int GetRandomWeightedIndex(int[] weights, double Random)
	{
		if (weights == null || weights.Length == 0)
		{
			return -1;
		}
		int num = 0;
		for (int i = 0; i < weights.Length; i++)
		{
			if (weights[i] >= 0)
			{
				num += weights[i];
			}
		}
		float num2 = 0f;
		for (int i = 0; i < weights.Length; i++)
		{
			if (!((float)weights[i] <= 0f))
			{
				num2 += (float)weights[i] / (float)num;
				if ((double)num2 >= Random)
				{
					return i;
				}
			}
		}
		return -1;
	}

	private void ClearPrefabs()
	{
		int num = base.transform.childCount;
		while (--num >= 0)
		{
			UnityEngine.Object.DestroyImmediate(base.transform.GetChild(num).gameObject);
		}
	}
}
