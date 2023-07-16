using UnityEngine;

public class AlignEncounter : MonoBehaviour
{
	public GameObject containerToAlign;

	public float alignToClosestValue = 1f;

	private void DefaultSizedButton()
	{
		if (containerToAlign == null)
		{
			containerToAlign = base.gameObject;
		}
		Transform[] componentsInChildren = containerToAlign.transform.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			transform.position = new Vector3((float)Mathf.RoundToInt(transform.position.x / alignToClosestValue) * alignToClosestValue, (float)Mathf.RoundToInt(transform.position.y / alignToClosestValue) * alignToClosestValue, (float)Mathf.RoundToInt(transform.position.z / alignToClosestValue) * alignToClosestValue);
		}
	}
}
