using System.Collections.Generic;
using UnityEngine;

public class randomChildPicker : BaseMonoBehaviour
{
	public enum SelectionMode
	{
		RandomChance,
		RandomSingle,
		RandomRange
	}

	public GameObject[] GameObjects;

	public SelectionMode selectionMode;

	public bool pickingMultiple;

	[Range(0f, 100f)]
	public int chanceToEnable;

	public Vector2Int MinMax = new Vector2Int(5, 8);

	private void GetChildren()
	{
		GameObjects = new GameObject[base.transform.childCount];
		int num = -1;
		while (++num < base.transform.childCount)
		{
			GameObjects[num] = base.transform.GetChild(num).gameObject;
		}
	}

	public void SetAllActive()
	{
		GameObject[] gameObjects = GameObjects;
		for (int i = 0; i < gameObjects.Length; i++)
		{
			gameObjects[i].SetActive(true);
		}
	}

	private void Start()
	{
		switch (selectionMode)
		{
		case SelectionMode.RandomChance:
		{
			if (GameObjects == null || GameObjects.Length == 0)
			{
				break;
			}
			for (int i = 0; i < GameObjects.Length; i++)
			{
				if (Random.Range(0, 100) <= chanceToEnable)
				{
					GameObjects[i].SetActive(true);
				}
				else
				{
					GameObjects[i].SetActive(false);
				}
			}
			break;
		}
		case SelectionMode.RandomSingle:
		{
			if (GameObjects == null || GameObjects.Length == 0)
			{
				break;
			}
			for (int j = 0; j < GameObjects.Length; j++)
			{
				if (GameObjects[j] != null)
				{
					GameObjects[j].SetActive(false);
				}
			}
			int num2 = Random.Range(0, GameObjects.Length);
			if (GameObjects[num2] != null)
			{
				GameObjects[num2].SetActive(true);
			}
			break;
		}
		case SelectionMode.RandomRange:
		{
			List<GameObject> list = new List<GameObject>(GameObjects);
			int num = Mathf.Min(Random.Range(MinMax.x, MinMax.y), list.Count);
			foreach (GameObject item in list)
			{
				item.SetActive(false);
			}
			while (num > 0)
			{
				int index = Random.Range(0, list.Count);
				list[index].SetActive(true);
				list.RemoveAt(index);
				num--;
			}
			break;
		}
		}
	}
}
