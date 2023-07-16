using MMBiomeGeneration;
using UnityEngine;

public class DeleteIfNotCriticalPath : BaseMonoBehaviour
{
	public enum Direction
	{
		North,
		East,
		South,
		West
	}

	public Direction MyDirection;

	private void Start()
	{
		if (BiomeGenerator.Instance == null)
		{
			return;
		}
		BiomeRoom.Direction criticalPathDirection = BiomeGenerator.Instance.CurrentRoom.CriticalPathDirection;
		switch (MyDirection)
		{
		case Direction.North:
			if (criticalPathDirection != BiomeRoom.Direction.North)
			{
				Object.Destroy(base.gameObject);
			}
			break;
		case Direction.East:
			if (criticalPathDirection != BiomeRoom.Direction.East)
			{
				Object.Destroy(base.gameObject);
			}
			break;
		case Direction.South:
			if (criticalPathDirection != BiomeRoom.Direction.South)
			{
				Object.Destroy(base.gameObject);
			}
			break;
		case Direction.West:
			if (criticalPathDirection != BiomeRoom.Direction.West)
			{
				Object.Destroy(base.gameObject);
			}
			break;
		}
	}
}
