using UnityEngine;

public class SimpleTransformSpinner : MonoBehaviour
{
	public Transform targetTransform;

	public Vector3 rotateSpeed;

	private void Start()
	{
		if (targetTransform == null)
		{
			targetTransform = base.transform;
		}
	}

	private void FixedUpdate()
	{
		Vector3 localEulerAngles = targetTransform.transform.localEulerAngles;
		localEulerAngles += rotateSpeed * Time.deltaTime;
		targetTransform.transform.localRotation = Quaternion.EulerAngles(localEulerAngles);
	}
}
