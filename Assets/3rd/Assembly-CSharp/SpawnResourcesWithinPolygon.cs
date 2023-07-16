using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
[ExecuteInEditMode]
public class SpawnResourcesWithinPolygon : BaseMonoBehaviour
{
	private PolygonCollider2D Polygon;

	[Range(0f, 100f)]
	public float ChanceToSpawn = 50f;

	public List<GameObject> Prefabs = new List<GameObject>();

	[Range(0f, 10f)]
	public float RanNoiseOffsetX;

	private float prevRanNoiseOffsetX;

	[Range(0f, 10f)]
	public float RanNoiseOffsetY;

	private float prevRanNoiseOffsetY;

	public List<Vector3> Nodes;

	private int xSpacing = 2;

	private int ySpacing = 2;

	private void OnDrawGizmos()
	{
		if (prevRanNoiseOffsetX != RanNoiseOffsetX || prevRanNoiseOffsetY != RanNoiseOffsetY)
		{
			Generate();
		}
		prevRanNoiseOffsetX = RanNoiseOffsetX;
		prevRanNoiseOffsetY = RanNoiseOffsetY;
		Gizmos.color = Color.red;
		Polygon = GetComponent<PolygonCollider2D>();
		int num = -1;
		while (++num < Polygon.points.Length - 1)
		{
			Gizmos.DrawLine((Vector2)base.transform.position + Polygon.points[num], (Vector2)base.transform.position + Polygon.points[num + 1]);
		}
		Gizmos.DrawLine((Vector2)base.transform.position + Polygon.points[0], (Vector2)base.transform.position + Polygon.points[Polygon.points.Length - 1]);
	}

	private void OnEnable()
	{
		prevRanNoiseOffsetX = RanNoiseOffsetX;
		prevRanNoiseOffsetY = RanNoiseOffsetY;
		CountNodes();
	}

	public void Generate()
	{
		if (Prefabs.Count >= 1)
		{
			Clear();
			CountNodes();
		}
	}

	private void CountNodes()
	{
		Nodes = new List<Vector3>();
		Polygon = GetComponent<PolygonCollider2D>();
		float num = Polygon.bounds.center.x - Polygon.bounds.extents.x;
		float num2 = Polygon.bounds.center.y - Polygon.bounds.extents.y;
		float num3 = 0f;
		for (int i = 0; (float)i < Mathf.Ceil(Polygon.bounds.extents.x * 2f); i += xSpacing)
		{
			for (int j = 0; (float)j < Mathf.Ceil(Polygon.bounds.extents.y * 2f); j += ySpacing)
			{
				num3 = ((j % 2 == 0) ? ((float)xSpacing * 0.5f) : 0f);
				if (Polygon.OverlapPoint(new Vector3(num + (float)i + num3, num2 + (float)j, Polygon.transform.position.z)))
				{
					Vector3 item = new Vector3(num + (float)i + num3, num2 + (float)j, Polygon.transform.position.z);
					if (!Nodes.Contains(item))
					{
						Nodes.Add(item);
					}
				}
			}
		}
	}

	public void Clear()
	{
	}

	private void OnDisable()
	{
	}
}
