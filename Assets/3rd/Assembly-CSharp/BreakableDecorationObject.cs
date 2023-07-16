using System.Collections.Generic;
using MMBiomeGeneration;
using UnityEngine;

public class BreakableDecorationObject : BaseMonoBehaviour
{
	public BiomeConstants.TypeOfParticle ParticleType;

	public DestructableObjectAsset DestructableAsset;

	public GameObject DestroyPlaceholder;

	public GameObject DestroyRubble;

	private GameObject SpawnedDecoration;

	public static Dictionary<DestructableObjectAsset, GameObject> AssignedDecorationObject = new Dictionary<DestructableObjectAsset, GameObject>();

	private void Start()
	{
		Object.Destroy(DestroyPlaceholder);
		GameObject gameObject = null;
		if (AssignedDecorationObject.ContainsKey(DestructableAsset))
		{
			gameObject = AssignedDecorationObject[DestructableAsset].gameObject;
		}
		else
		{
			int num = -1;
			int[] array = new int[DestructableAsset.GameObjectAndProbabilities.Count];
			while (++num < DestructableAsset.GameObjectAndProbabilities.Count)
			{
				array[num] = DestructableAsset.GameObjectAndProbabilities[num].Probability;
			}
			int randomWeightedIndex = Utils.GetRandomWeightedIndex(array);
			gameObject = DestructableAsset.GameObjectAndProbabilities[randomWeightedIndex].GameObject;
			AssignedDecorationObject.Add(DestructableAsset, gameObject);
		}
		SpawnedDecoration = Object.Instantiate(gameObject, base.transform);
		BiomeGenerator.OnBiomeLeftRoom += OnBiomeLeftRoom;
	}

	private void OnBiomeLeftRoom()
	{
		AssignedDecorationObject.Clear();
		BiomeGenerator.OnBiomeLeftRoom -= OnBiomeLeftRoom;
	}
}
