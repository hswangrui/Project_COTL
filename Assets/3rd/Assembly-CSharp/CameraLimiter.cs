using UnityEngine;

public class CameraLimiter : BaseMonoBehaviour
{
	public Bounds LimitBounds;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Player")
		{
			CameraFollowTarget instance = CameraFollowTarget.Instance;
			Bounds limitBounds = LimitBounds;
			limitBounds.center += base.transform.position;
			instance.SetCameraLimits(limitBounds);
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Player")
		{
			CameraFollowTarget.Instance.DisableCameraLimits();
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(base.transform.position + LimitBounds.center, LimitBounds.size);
	}
}
