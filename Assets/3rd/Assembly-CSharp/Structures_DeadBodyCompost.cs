public class Structures_DeadBodyCompost : Structures_CompostBin
{
	public override int CompostCost
	{
		get
		{
			return 20;
		}
	}

	public override int PoopToCreate
	{
		get
		{
			return 10;
		}
	}

	public override float COMPOST_DURATION
	{
		get
		{
			return 120f;
		}
	}
}
