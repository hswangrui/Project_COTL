using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(PolygonCollider2D))]
[ExecuteInEditMode]
public class Mesh2DColliderMaker : MonoBehaviour
{
	private struct Edge2D
	{
		public Vector2 a;

		public Vector2 b;

		public override bool Equals(object obj)
		{
			if (obj is Edge2D)
			{
				Edge2D edge2D = (Edge2D)obj;
				if (!(edge2D.a == a) || !(edge2D.b == b))
				{
					if (edge2D.b == a)
					{
						return edge2D.a == b;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return a.GetHashCode() ^ b.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("[" + a.x + "," + a.y + "->" + b.x + "," + b.y + "]");
		}
	}

	private MeshFilter filter;

	private PolygonCollider2D polyCollider;

	private void Start()
	{
	}

	public void Create(MeshFilter filterVar, PolygonCollider2D polyColliderVar)
	{
		filter = filterVar;
		polyCollider = polyColliderVar;
		CreatePolygon2DColliderPoints();
	}

	public void CreatePolygon2DColliderPoints()
	{
		List<Vector2[]> paths = BuildColliderPaths(BuildEdgesFromMesh());
		ApplyPathsToPolygonCollider(paths);
	}

	private void ApplyPathsToPolygonCollider(List<Vector2[]> paths)
	{
		if (paths != null)
		{
			polyCollider.pathCount = paths.Count;
			for (int i = 0; i < paths.Count; i++)
			{
				Vector2[] points = paths[i];
				polyCollider.SetPath(i, points);
			}
		}
	}

	private Dictionary<Edge2D, int> BuildEdgesFromMesh()
	{
		Mesh sharedMesh = filter.sharedMesh;
		if (sharedMesh == null)
		{
			return null;
		}
		Vector3[] vertices = sharedMesh.vertices;
		int[] triangles = sharedMesh.triangles;
		Dictionary<Edge2D, int> dictionary = new Dictionary<Edge2D, int>();
		for (int i = 0; i < triangles.Length - 2; i += 3)
		{
			Vector3 vector = vertices[triangles[i]];
			Vector3 vector2 = vertices[triangles[i + 1]];
			Vector3 vector3 = vertices[triangles[i + 2]];
			Edge2D[] array = new Edge2D[3]
			{
				new Edge2D
				{
					a = vector,
					b = vector2
				},
				new Edge2D
				{
					a = vector2,
					b = vector3
				},
				new Edge2D
				{
					a = vector3,
					b = vector
				}
			};
			foreach (Edge2D key in array)
			{
				if (dictionary.ContainsKey(key))
				{
					dictionary[key]++;
				}
				else
				{
					dictionary[key] = 1;
				}
			}
		}
		return dictionary;
	}

	private static List<Edge2D> GetOuterEdges(Dictionary<Edge2D, int> allEdges)
	{
		List<Edge2D> list = new List<Edge2D>();
		foreach (Edge2D key in allEdges.Keys)
		{
			if (allEdges[key] == 1)
			{
				list.Add(key);
			}
		}
		return list;
	}

	private static List<Vector2[]> BuildColliderPaths(Dictionary<Edge2D, int> allEdges)
	{
		if (allEdges == null)
		{
			return null;
		}
		List<Edge2D> outerEdges = GetOuterEdges(allEdges);
		List<List<Edge2D>> list = new List<List<Edge2D>>();
		List<Edge2D> list2 = null;
		while (outerEdges.Count > 0)
		{
			if (list2 == null)
			{
				list2 = new List<Edge2D>();
				list2.Add(outerEdges[0]);
				list.Add(list2);
				outerEdges.RemoveAt(0);
			}
			bool flag = false;
			int num = 0;
			while (num < outerEdges.Count)
			{
				Edge2D item = outerEdges[num];
				bool flag2 = false;
				if (item.b == list2[0].a)
				{
					list2.Insert(0, item);
					flag2 = true;
				}
				else if (item.a == list2[list2.Count - 1].b)
				{
					list2.Add(item);
					flag2 = true;
				}
				if (flag2)
				{
					flag = true;
					outerEdges.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
			if (!flag)
			{
				list2 = null;
			}
		}
		List<Vector2[]> list3 = new List<Vector2[]>();
		foreach (List<Edge2D> item2 in list)
		{
			List<Vector2> list4 = new List<Vector2>();
			foreach (Edge2D item3 in item2)
			{
				list4.Add(item3.a);
			}
			list3.Add(CoordinatesCleaned(list4));
		}
		return list3;
	}

	private static bool CoordinatesFormLine(Vector2 a, Vector2 b, Vector2 c)
	{
		return Mathf.Approximately(a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y), 0f);
	}

	private static Vector2[] CoordinatesCleaned(List<Vector2> coordinates)
	{
		List<Vector2> list = new List<Vector2>();
		list.Add(coordinates[0]);
		int index = 0;
		for (int i = 1; i < coordinates.Count; i++)
		{
			Vector2 vector = coordinates[i];
			Vector2 a = coordinates[index];
			Vector2 c = ((i + 1 >= coordinates.Count) ? coordinates[0] : coordinates[i + 1]);
			if (!CoordinatesFormLine(a, vector, c))
			{
				list.Add(vector);
				index = i;
			}
		}
		return list.ToArray();
	}
}
