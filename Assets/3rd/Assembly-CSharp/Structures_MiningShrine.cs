public class Structures_MiningShrine : Structures_NatureShrine
{
	protected override void OnNewDayStarted()
	{
		UpdateFuel(1);
	}
}
