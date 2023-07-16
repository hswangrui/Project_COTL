using UnityEngine;

public class DrawGizmo : BaseMonoBehaviour
{
	public GameObject[] gameObjects;

	private GameObject pastObject;

	public Color color = Color.yellow;

	public float sphereSize = 0.1f;

	private void Start()
	{
	}

	private void OnDrawGizmosSelected()
	{
		if (gameObjects == null)
		{
			return;
		}
		GameObject[] array = gameObjects;
		foreach (GameObject gameObject in array)
		{
			Gizmos.color = color;
			Gizmos.DrawSphere(gameObject.transform.position, sphereSize);
			if (pastObject != null)
			{
				Gizmos.DrawLine(gameObject.transform.position, pastObject.transform.position);
			}
			pastObject = gameObject;
		}
	}

	private void Update()
	{
	}
}
