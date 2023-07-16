using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObjectTypes
{
	public GameObject[] Objects;

	public List<FollowerLocation> Location = new List<FollowerLocation>();

	public Vector2 PercentageChanceToSpawn;
}
