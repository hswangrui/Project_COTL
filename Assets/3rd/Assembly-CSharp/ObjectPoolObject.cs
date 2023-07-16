using System;
using UnityEngine;

[Serializable]
public class ObjectPoolObject
{
	public enum PoolLocation
	{
		None,
		Base,
		Dungeon,
		Both
	}

	public GameObject gameObject;

	public int AmountToPool;

	public PoolLocation PoolingLocation;
}
