public class Structures_ChoppingShrine : Structures_NatureShrine
{
	protected override void OnNewDayStarted()
	{
		UpdateFuel(10);
	}
}
