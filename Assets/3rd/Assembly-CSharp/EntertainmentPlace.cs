using System.Collections.Generic;
using UnityEngine;

public class EntertainmentPlace : BaseMonoBehaviour
{
	public static List<EntertainmentPlace> AllEntertainmentPlaces = new List<EntertainmentPlace>();

	public static List<EntertainmentPlace> FreeEntertainmentPlaces = new List<EntertainmentPlace>();

	public List<Transform> Positions = new List<Transform>();

	private List<GameObject> Workers = new List<GameObject>();

	private void OnEnable()
	{
		for (int i = 0; i < Positions.Count; i++)
		{
			Workers.Add(null);
		}
		AllEntertainmentPlaces.Add(this);
		FreeEntertainmentPlaces.Add(this);
	}

	private void OnDisable()
	{
		Workers = new List<GameObject>();
		AllEntertainmentPlaces.Remove(this);
		FreeEntertainmentPlaces.Remove(this);
	}

	public static Transform GetFreeEntertainmentPlace(GameObject gameObject)
	{
		if (FreeEntertainmentPlaces.Count < 1)
		{
			return null;
		}
		int index = Random.Range(0, FreeEntertainmentPlaces.Count);
		EntertainmentPlace entertainmentPlace = FreeEntertainmentPlaces[index];
		for (int i = 0; i < entertainmentPlace.Positions.Count; i++)
		{
			if (entertainmentPlace.Workers[i] == null)
			{
				entertainmentPlace.Workers[i] = gameObject;
				if (i == entertainmentPlace.Positions.Count - 1)
				{
					FreeEntertainmentPlaces.Remove(entertainmentPlace);
				}
				return entertainmentPlace.Positions[i];
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
			if (!FreeEntertainmentPlaces.Contains(this))
			{
				FreeEntertainmentPlaces.Add(this);
			}
		}
	}
}
