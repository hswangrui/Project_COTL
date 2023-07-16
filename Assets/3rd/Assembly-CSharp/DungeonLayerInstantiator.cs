using System;
using UnityEngine;

public class DungeonLayerInstantiator : MonoBehaviour
{
	[Serializable]
	public struct LayerObject
	{
		public int Layer;

		public GameObject Object;
	}

	[SerializeField]
	private LayerObject[] layerObjects;

	private void Start()
	{
		LayerObject[] array = layerObjects;
		for (int i = 0; i < array.Length; i++)
		{
			LayerObject layerObject = array[i];
			if (layerObject.Layer == GameManager.CurrentDungeonLayer)
			{
				UnityEngine.Object.Instantiate(layerObject.Object, base.transform.position, Quaternion.identity, base.transform);
			}
		}
	}
}
