using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DestructableObjectAsset : ScriptableObject
{
	[Serializable]
	public class GameObjectAndProbability
	{
		public GameObject GameObject;

		[Range(0f, 100f)]
		public int Probability = 100;
	}

	public List<GameObjectAndProbability> GameObjectAndProbabilities = new List<GameObjectAndProbability>();
}
