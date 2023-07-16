using System;

public class Structures_NatureShrine : StructureBrain
{
	public override void OnAdded()
	{
		TimeManager.OnNewDayStarted = (Action)Delegate.Combine(TimeManager.OnNewDayStarted, new Action(OnNewDayStarted));
	}

	public override void OnRemoved()
	{
		TimeManager.OnNewDayStarted = (Action)Delegate.Remove(TimeManager.OnNewDayStarted, new Action(OnNewDayStarted));
	}

	protected virtual void OnNewDayStarted()
	{
		UpdateFuel();
	}
}
