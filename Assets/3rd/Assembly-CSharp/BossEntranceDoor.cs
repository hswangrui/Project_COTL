using UnityEngine;

public class BossEntranceDoor : BaseMonoBehaviour
{
	public Bounds LimitCameraBounds;

	private void OnEnable()
	{
		GameManager.GetInstance().CamFollowTarget.SetCameraLimits(LimitCameraBounds);
	}

	private void OnDisable()
	{
		GameManager.GetInstance().CamFollowTarget.DisableCameraLimits();
	}
}
