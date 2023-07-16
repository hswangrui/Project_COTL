using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPrefabsPerlinNoise : BaseMonoBehaviour
{
	[Serializable]
	public class SpawnableItem
	{
		public GameObject Prefab;

		public Vector2 PerlinRange;

		public Vector3 RandomOffset = Vector3.zero;

		public bool randomFlip;
	}

	private PolygonCollider2D Polygon;

	public bool IsometricPlacement = true;

	private float IsometricOffset;

	public float PerlinScale = 1f;

	public Vector2 PerlinOffset = Vector2.zero;

	private float Noise;

	private GameObject g;

	public Vector2 Spacing = new Vector2(2f, 2f);

	public List<SpawnableItem> SpawnableItems = new List<SpawnableItem>();

	private void Generate()
	{
	}

	public void ClearPrefabs()
	{
		int num = base.transform.childCount;
		while (--num > -1)
		{
			UnityEngine.Object.DestroyImmediate(base.transform.GetChild(num).gameObject);
		}
	}
}
