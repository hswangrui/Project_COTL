using System;
using UnityEngine;

public class MoonParralax : BaseMonoBehaviour
{
	private GameObject Cam;

	private Vector3 Groundposition;

	public GameObject ShowGroundPos;

	private Vector3 Position;

	private CameraFollowTarget CameraFollowTarget;

	public Vector2 Parralax;

	public float YOffset;

	private void Update()
	{
		if (Cam == null)
		{
			Cam = Camera.main.gameObject;
			CameraFollowTarget = Cam.GetComponent<CameraFollowTarget>();
		}
		if (!(CameraFollowTarget == null))
		{
			Groundposition = Camera.main.transform.position - new Vector3(0f, CameraFollowTarget.distance * Mathf.Sin((float)Math.PI / 180f * CameraFollowTarget.angle), (0f - CameraFollowTarget.distance) * Mathf.Cos((float)Math.PI / 180f * CameraFollowTarget.angle));
			Vector3 groundposition = Groundposition;
			groundposition.z = 0f;
			Groundposition = groundposition;
			ShowGroundPos.transform.position = Groundposition;
			Position = base.transform.position;
			Position.x = Groundposition.x * Parralax.x;
			Position.y = (Groundposition.y + YOffset) * Parralax.y;
			base.transform.position = Position;
		}
	}
}
