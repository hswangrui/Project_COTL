using System.Collections.Generic;
using UnityEngine;

public class WorshipPlace : BaseMonoBehaviour
{
	public static List<WorshipPlace> AllWorshipPlaces = new List<WorshipPlace>();

	public static List<WorshipPlace> FreeWorshipPlaces = new List<WorshipPlace>();

	public List<Transform> Positions = new List<Transform>();

	private List<GameObject> Workers = new List<GameObject>();

	private void Start()
	{
		for (int i = 0; i < Positions.Count; i++)
		{
			Workers.Add(null);
		}
	}

	private void OnEnable()
	{
		AllWorshipPlaces.Add(this);
		FreeWorshipPlaces.Add(this);
	}

	private void OnDisable()
	{
		AllWorshipPlaces.Remove(this);
		FreeWorshipPlaces.Remove(this);
	}

	public static Transform GetFreeWorshipPlace(GameObject gameObject)
	{
		if (FreeWorshipPlaces.Count < 1)
		{
			return null;
		}
		int index = Random.Range(0, FreeWorshipPlaces.Count);
		WorshipPlace worshipPlace = FreeWorshipPlaces[index];
		for (int i = 0; i < worshipPlace.Positions.Count; i++)
		{
			if (worshipPlace.Workers[i] == null)
			{
				worshipPlace.Workers[i] = gameObject;
				if (i == worshipPlace.Positions.Count - 1)
				{
					FreeWorshipPlaces.Remove(worshipPlace);
				}
				return worshipPlace.Positions[i];
			}
		}
		return null;
	}

	public void RemoveFromPositions(GameObject gameObject)
	{
		int num = Workers.IndexOf(gameObject);
		if (num != -1)
		{
			Workers[num] = null;
			if (!FreeWorshipPlaces.Contains(this))
			{
				FreeWorshipPlaces.Add(this);
			}
		}
	}
}
