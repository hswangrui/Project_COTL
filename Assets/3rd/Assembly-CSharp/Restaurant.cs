using System.Collections.Generic;
using UnityEngine;

public class Restaurant : BaseMonoBehaviour
{
	public static List<Restaurant> AllRestaurants = new List<Restaurant>();

	public static List<Restaurant> FreeRestaurants = new List<Restaurant>();

	public List<Transform> Positions = new List<Transform>();

	private List<GameObject> Workers = new List<GameObject>();

	private void OnEnable()
	{
		for (int i = 0; i < Positions.Count; i++)
		{
			Workers.Add(null);
		}
		AllRestaurants.Add(this);
		FreeRestaurants.Add(this);
	}

	private void OnDisable()
	{
		Workers = new List<GameObject>();
		AllRestaurants.Remove(this);
		FreeRestaurants.Remove(this);
	}

	public static Transform GetFreeRestaurant(GameObject gameObject)
	{
		if (FreeRestaurants.Count < 1)
		{
			return null;
		}
		int index = Random.Range(0, FreeRestaurants.Count);
		Restaurant restaurant = FreeRestaurants[index];
		for (int i = 0; i < restaurant.Positions.Count; i++)
		{
			if (restaurant.Workers[i] == null)
			{
				restaurant.Workers[i] = gameObject;
				if (i == restaurant.Positions.Count - 1)
				{
					FreeRestaurants.Remove(restaurant);
				}
				return restaurant.Positions[i];
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
			if (!FreeRestaurants.Contains(this))
			{
				FreeRestaurants.Add(this);
			}
		}
	}
}
