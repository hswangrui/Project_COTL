public class Structures_ForagingShrine : Structures_NatureShrine
{
	protected override void OnNewDayStarted()
	{
		UpdateFuel(1);
	}
}
