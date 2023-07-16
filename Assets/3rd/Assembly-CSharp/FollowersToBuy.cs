using System;
using UnityEngine;

[Serializable]
public class FollowersToBuy
{
	public enum FollowerBuyTypes
	{
		None,
		Ill,
		Old,
		Faithful,
		Level1,
		Level2,
		Level3,
		Level4
	}

	public FollowerBuyTypes followerTypes;

	public int followerCost;

	[Range(0f, 100f)]
	public int chanceToSpawn;

	public FollowersToBuy()
	{
	}

	public FollowersToBuy(FollowerBuyTypes followerTypes, int followerCost, int chanceToSpawn)
	{
		this.followerTypes = followerTypes;
		this.followerCost = followerCost;
		this.chanceToSpawn = chanceToSpawn;
	}
}
