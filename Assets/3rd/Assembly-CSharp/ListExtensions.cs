using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
	public static T LastElement<T>(this List<T> list) where T : class
	{
		if (list.Count > 0)
		{
			return list[list.Count - 1];
		}
		return null;
	}

	public static T RandomElement<T>(this List<T> list)
	{
		return list[list.RandomIndex()];
	}

	public static int RandomIndex<T>(this List<T> list)
	{
		return Random.Range(0, list.Count);
	}

	public static bool Equals<T>(List<T> list1, List<T> list2)
	{
		if (list1.Count != list2.Count)
		{
			return false;
		}
		foreach (T item in list1)
		{
			bool flag = false;
			foreach (T item2 in list2)
			{
				if (item.Equals(item2))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}
}
