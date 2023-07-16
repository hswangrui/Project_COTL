using System;
using UnityEngine;

[Serializable]
public class GroupProbability
{
	public GameObject GroupObject;

	[Range(1f, 100f)]
	public int Probability = 50;

	public GroupProbability(GameObject g)
	{
		GroupObject = g;
	}
}
