using System;
using UnityEngine;

namespace Map
{
	[Serializable]
	public class IntMinMax
	{
		public int min;

		public int max;

		public int GetValue()
		{
			return UnityEngine.Random.Range(min, max + 1);
		}
	}
}
