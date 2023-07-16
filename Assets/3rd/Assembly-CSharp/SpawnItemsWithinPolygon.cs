using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class SpawnItemsWithinPolygon : BaseMonoBehaviour
{
	public bool RegenerateOnPlay;

	public bool RandomOffset;

	private PolygonCollider2D Polygon;

	[Range(0f, 100f)]
	public float ChanceToSpawn = 50f;

	public Vector2Int Spacing = new Vector2Int(1, 1);

	public List<GameObject> Prefabs = new List<GameObject>();

	[HideInInspector]
	public float spawnX;

	[HideInInspector]
	public float spawnY;

	private void OnEnable()
	{
		if (RoomManager.Instance != null)
		{
			RoomManager.Instance.OnInitEnemies += InitEnemies;
		}
	}

	private void InitEnemies()
	{
		if (RegenerateOnPlay)
		{
			GenerateAtRuntime();
		}
	}

	private void OnDisable()
	{
		if (RoomManager.Instance != null)
		{
			RoomManager.Instance.OnInitEnemies -= InitEnemies;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Polygon = GetComponent<PolygonCollider2D>();
		int num = -1;
		while (++num < Polygon.points.Length - 1)
		{
			Gizmos.DrawLine((Vector2)base.transform.position + Polygon.points[num], (Vector2)base.transform.position + Polygon.points[num + 1]);
		}
		Gizmos.DrawLine((Vector2)base.transform.position + Polygon.points[0], (Vector2)base.transform.position + Polygon.points[Polygon.points.Length - 1]);
	}

	public void GenerateAtRuntime()
	{
		if (Prefabs.Count < 1)
		{
			return;
		}
		ClearAtRuntime();
		Polygon = GetComponent<PolygonCollider2D>();
		float num = Polygon.bounds.center.x - Polygon.bounds.extents.x;
		float num2 = Polygon.bounds.center.y - Polygon.bounds.extents.y;
		spawnX = Polygon.bounds.extents.x;
		spawnY = Polygon.bounds.extents.y;
		float num3 = 0f;
		for (int i = 0; (float)i < Mathf.Ceil(Polygon.bounds.extents.x * 2f); i++)
		{
			for (int j = 0; (float)j < Mathf.Ceil(Polygon.bounds.extents.y * 2f); j++)
			{
				num3 = ((j % 2 == 0) ? 0.5f : 0f);
				if ((float)Random.Range(0, 100) <= ChanceToSpawn && Polygon.OverlapPoint(new Vector3(num + (float)i + num3, num2 + (float)j, Polygon.transform.position.z)))
				{
					Object.Instantiate(Prefabs[Random.Range(0, Prefabs.Count)], new Vector3(num + (float)i + num3, num2 + (float)j, 0f), Quaternion.identity).transform.parent = base.transform;
				}
			}
		}
	}

	public void ClearAtRuntime()
	{
		int num = base.transform.childCount;
		while (--num > -1)
		{
			Object.Destroy(base.transform.GetChild(num).gameObject);
		}
	}
}
