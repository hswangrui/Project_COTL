using UnityEngine;

public class Rotator : MonoBehaviour
{
	public Vector3 rotationDegrees;

	public bool localSpace;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Rotate(rotationDegrees * Time.deltaTime, localSpace ? Space.Self : Space.World);
	}
}
