public class Structures_HealingBay : StructureBrain
{
	public int Level;

	public float Multiplier
	{
		get
		{
			if (Level != 0)
			{
				return 1.5f;
			}
			return 1f;
		}
	}

	public Structures_HealingBay(int level)
	{
		Level = level;
	}

	public bool CheckIfOccupied()
	{
		return Data.FollowerID != -1;
	}
}
