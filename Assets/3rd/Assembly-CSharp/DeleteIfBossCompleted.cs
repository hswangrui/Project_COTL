using UnityEngine;

public class DeleteIfBossCompleted : BaseMonoBehaviour
{
	public FollowerLocation Location;

	public GameObject CameraPosition;

	private void Start()
	{
		if (DataManager.Instance.DoorRoomBossLocksDestroyed.Contains(Location))
		{
			Object.Destroy(base.gameObject);
		}
	}
}
