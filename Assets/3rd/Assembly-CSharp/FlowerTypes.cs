using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FlowerTypes
{
	public string Flower;

	public string Grass;

	public List<FollowerLocation> Location = new List<FollowerLocation>();

	public Vector2 PercentageChanceToSpawn;
}
