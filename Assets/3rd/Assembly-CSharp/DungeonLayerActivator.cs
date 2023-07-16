using System;
using UnityEngine;

public class DungeonLayerActivator : MonoBehaviour
{
	[Serializable]
	public struct LayerObject
	{
		public int Layer;

		public GameObject Object;

		public bool KeepActiveIfLayerIsGreaterOrEqual;
	}

	[SerializeField]
	private LayerObject[] layerObjects;

	private void Start()
	{
		LayerObject[] array = layerObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Object.SetActive(false);
		}
		array = layerObjects;
		for (int i = 0; i < array.Length; i++)
		{
			LayerObject layerObject = array[i];
			layerObject.Object.SetActive(layerObject.Layer == GameManager.CurrentDungeonLayer || (GameManager.CurrentDungeonLayer >= layerObject.Layer && layerObject.KeepActiveIfLayerIsGreaterOrEqual));
		}
	}
}
