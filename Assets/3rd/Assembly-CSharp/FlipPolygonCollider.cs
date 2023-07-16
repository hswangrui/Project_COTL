using System.Collections.Generic;
using MMRoomGeneration;
using UnityEngine;

public class FlipPolygonCollider : MonoBehaviour
{
	private PolygonCollider2D p;

	private IslandPiece i;

	public List<Vector2> NewPoints;

	private void Play()
	{
		this.i = GetComponent<IslandPiece>();
		IslandConnector[] connectors = this.i.Connectors;
		foreach (IslandConnector islandConnector in connectors)
		{
			islandConnector.transform.position = new Vector3(islandConnector.transform.position.x * -1f, islandConnector.transform.position.y, islandConnector.transform.position.z);
			if (islandConnector.MyDirection == IslandConnector.Direction.East)
			{
				islandConnector.MyDirection = IslandConnector.Direction.West;
			}
			else if (islandConnector.MyDirection == IslandConnector.Direction.West)
			{
				islandConnector.MyDirection = IslandConnector.Direction.East;
			}
		}
		p = this.i.Collider;
		NewPoints = new List<Vector2>();
		Vector2[] path = p.GetPath(0);
		for (int i = 0; i < path.Length; i++)
		{
			Vector2 vector = path[i];
			NewPoints.Add(new Vector2(vector.x * -1f, vector.y));
		}
		p.SetPath(0, NewPoints);
	}
}
