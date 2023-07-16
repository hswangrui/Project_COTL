using System;
using System.Collections.Generic;
using UnityEngine;

public class RandomChildPickerWeighted : BaseMonoBehaviour
{
	[Serializable]
	public class ItemAndProbability
	{
		public GameObject GameObject;

		[Range(1f, 100f)]
		public int Probability = 1;

		public RandomChildPickerWeighted parent;

		public float TotalProbability
		{
			get
			{
				float num = 0f;
				foreach (ItemAndProbability item in parent.LootToDrop)
				{
					num += (float)item.Probability;
				}
				return num;
			}
		}
	}

	public List<ItemAndProbability> LootToDrop = new List<ItemAndProbability>();

	private void GetChildren()
	{
		LootToDrop = new List<ItemAndProbability>();
		int num = -1;
		while (++num < base.transform.childCount)
		{
			LootToDrop.Add(new ItemAndProbability
			{
				GameObject = base.transform.GetChild(num).gameObject,
				parent = this
			});
		}
	}

	private void Start()
	{
		SelectObejct();
	}

	public void SelectObejct()
	{
		int[] array = new int[LootToDrop.Count];
		int num = -1;
		while (++num < LootToDrop.Count)
		{
			array[num] = LootToDrop[num].Probability;
			LootToDrop[num].GameObject.SetActive(false);
		}
		int randomWeightedIndex = Utils.GetRandomWeightedIndex(array);
		LootToDrop[randomWeightedIndex].GameObject.SetActive(true);
	}
}
