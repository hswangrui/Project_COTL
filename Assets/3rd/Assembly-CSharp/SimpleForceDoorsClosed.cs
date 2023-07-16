using MMRoomGeneration;
using UnityEngine;

public class SimpleForceDoorsClosed : MonoBehaviour
{
	[SerializeField]
	private GenerateRoom generateRoom;

	private void Start()
	{
		generateRoom.OnGenerated += GenerateRoom_OnGenerated;
	}

	private void OnDestroy()
	{
		generateRoom.OnGenerated -= GenerateRoom_OnGenerated;
	}

	private void GenerateRoom_OnGenerated()
	{
		RoomLockController.CloseAll();
	}
}
