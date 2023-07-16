using UnityEngine;

public class CameraWobble : MonoBehaviour
{
	public float wobbleAmount = 0.025f;

	public float speedMulti = 0.25f;

	private Vector3 initPos;

	public bool updateInitPos;

	private void Start()
	{
		initPos = base.transform.position;
	}

	private void OnEnable()
	{
		initPos = base.transform.position;
	}

	private void Update()
	{
		if (updateInitPos)
		{
			initPos = base.transform.position;
			updateInitPos = false;
		}
		Vector3 vector = Mathf.Sin(Time.time * 2f * speedMulti) * Vector3.right * wobbleAmount;
		vector += Mathf.Sin(Time.time * 1.5f * speedMulti) * Vector3.up * wobbleAmount;
		vector += Mathf.Sin(Time.time * 1.8f * speedMulti) * Vector3.forward * wobbleAmount;
		base.transform.position = initPos + vector;
	}
}
