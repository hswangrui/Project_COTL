using System;
using System.Collections.Generic;
using System.Linq;

namespace Map
{
	public static class ShufflingExtension
	{
		private static Random rng = new Random();

		public static void Shuffle<T>(this IList<T> list)
		{
			int num = list.Count;
			while (num > 1)
			{
				num--;
				int index = rng.Next(num + 1);
				T value = list[index];
				list[index] = list[num];
				list[num] = value;
			}
		}

		public static T Random<T>(this IList<T> list)
		{
			return list[rng.Next(list.Count)];
		}

		public static T Last<T>(this IList<T> list)
		{
			return list[list.Count - 1];
		}

		public static List<T> GetRandomElements<T>(this List<T> list, int elementsCount)
		{
			return list.OrderBy((T arg) => Guid.NewGuid()).Take((list.Count < elementsCount) ? list.Count : elementsCount).ToList();
		}
	}
}
