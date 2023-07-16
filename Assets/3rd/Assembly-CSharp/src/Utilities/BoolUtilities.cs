using UnityEngine;

namespace src.Utilities
{
	public class BoolUtilities
	{
		public static bool RandomBool()
		{
			return Random.Range(0, 2).ToBool();
		}
	}
}
