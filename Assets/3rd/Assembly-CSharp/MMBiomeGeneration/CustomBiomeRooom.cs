using System;
using UnityEngine;

namespace MMBiomeGeneration
{
	[Serializable]
	public class CustomBiomeRooom
	{
		public GameObject Prefab;

		public bool North;

		public bool East;

		public bool South;

		public bool West;

		public int NumDesiredConnections
		{
			get
			{
				int num = 0;
				if (North)
				{
					num++;
				}
				if (East)
				{
					num++;
				}
				if (South)
				{
					num++;
				}
				if (West)
				{
					num++;
				}
				return num;
			}
		}
	}
}
