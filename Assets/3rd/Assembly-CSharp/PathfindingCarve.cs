using UnityEngine;

public class PathfindingCarve : MonoBehaviour
{
	private void Start()
	{
		Collider2D[] componentsInChildren = GetComponentsInChildren<Collider2D>();
		foreach (Collider2D collider2D in componentsInChildren)
		{
			if (AstarPath.active != null)
			{
				AstarPath.active.UpdateGraphs(collider2D.bounds);
			}
		}
	}
}
