using MMRoomGeneration;
using UnityEngine;

public class DeleteIfNoDoor : BaseMonoBehaviour
{
	public enum Direction
	{
		North,
		East,
		South,
		West
	}

	public GenerateRoom generateRoom;

	public Direction MyDirection;

	private void Start()
	{
		if (generateRoom == null)
		{
			generateRoom = GetComponentInParent<GenerateRoom>();
		}
		switch (MyDirection)
		{
		case Direction.North:
			if (generateRoom.North == GenerateRoom.ConnectionTypes.False)
			{
				Object.Destroy(base.gameObject);
			}
			break;
		case Direction.East:
			if (generateRoom.East == GenerateRoom.ConnectionTypes.False)
			{
				Object.Destroy(base.gameObject);
			}
			break;
		case Direction.South:
			if (generateRoom.South == GenerateRoom.ConnectionTypes.False)
			{
				Object.Destroy(base.gameObject);
			}
			break;
		case Direction.West:
			if (generateRoom.West == GenerateRoom.ConnectionTypes.False)
			{
				Object.Destroy(base.gameObject);
			}
			break;
		}
	}
}
