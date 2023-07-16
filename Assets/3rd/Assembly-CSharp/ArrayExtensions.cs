using System;
using UnityEngine;

public static class ArrayExtensions
{
	public static int IndexOf<T>(this T[] array, T item)
	{
		return Array.IndexOf(array, item);
	}

	public static T LastElement<T>(this T[] array)
	{
		if (array.Length != 0)
		{
			return array[array.Length - 1];
		}
		return default(T);
	}

	public static bool Contains<T>(this T[] array, T item)
	{
		return array.IndexOf(item) != -1;
	}

	public static T RandomElement<T>(this T[] array)
	{
		return array[UnityEngine.Random.Range(0, array.Length)];
	}
}
