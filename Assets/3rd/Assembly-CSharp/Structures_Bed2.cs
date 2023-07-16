public class Structures_Bed2 : Structures_Bed
{
	public override int Level
	{
		get
		{
			return 2;
		}
	}

	public override float ChanceToCollapse
	{
		get
		{
			if (WeatherSystemController.Instance.IsRaining)
			{
				return 0.05f;
			}
			return 0.025f;
		}
	}

	public override TYPES CollapsedType
	{
		get
		{
			return TYPES.BED_2_COLLAPSED;
		}
	}
}
