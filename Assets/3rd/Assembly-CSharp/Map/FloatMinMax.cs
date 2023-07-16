using System;
using UnityEngine;

namespace Map
{
	[Serializable]
	public class FloatMinMax
	{
		public float min;

		public float max;

		public float GetValue()
		{
			return UnityEngine.Random.Range(min, max);
		}
	}
}
