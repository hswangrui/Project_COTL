using System.Collections;
using MMRoomGeneration;
using UnityEngine;

public class DeleteIfDoor : BaseMonoBehaviour
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
		if ((bool)generateRoom)
		{
			if (generateRoom.generated)
			{
				Delete();
			}
			else
			{
				GameManager.GetInstance().StartCoroutine(DeleteIE());
			}
		}
	}

	private IEnumerator DeleteIE()
	{
		while (generateRoom == null || !generateRoom.generated)
		{
			yield return null;
		}
		Delete();
	}

	private void Delete()
	{
		switch (MyDirection)
		{
		case Direction.North:
			if (generateRoom.North != 0)
			{
				Object.Destroy(base.gameObject);
			}
			break;
		case Direction.East:
			if (generateRoom.East != 0)
			{
				Object.Destroy(base.gameObject);
			}
			break;
		case Direction.South:
			if (generateRoom.South != 0)
			{
				Object.Destroy(base.gameObject);
			}
			break;
		case Direction.West:
			if (generateRoom.West != 0)
			{
				Object.Destroy(base.gameObject);
			}
			break;
		}
	}
}
