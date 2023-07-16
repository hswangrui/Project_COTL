using System;

public class Structures_DeadWorshipper : StructureBrain
{
	public override void OnAdded()
	{
		TimeManager.OnNewDayStarted = (Action)Delegate.Combine(TimeManager.OnNewDayStarted, new Action(OnNewDayStarted));
	}

	public override void OnRemoved()
	{
		TimeManager.OnNewDayStarted = (Action)Delegate.Remove(TimeManager.OnNewDayStarted, new Action(OnNewDayStarted));
	}

	private void OnNewDayStarted()
	{
		Data.Age++;
		if (Data.Age >= 2)
		{
			Data.Rotten = true;
		}
	}
}
