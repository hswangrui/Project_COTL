using System;
using System.Collections.Generic;

public class Structures_Meal : StructureBrain
{
	public static bool GetListContainsMealType(List<Structures_Meal> List, TYPES Type)
	{
		foreach (Structures_Meal item in List)
		{
			if (item.Data.Type == Type && !item.ReservedForTask)
			{
				return true;
			}
		}
		return false;
	}

	public override void OnAdded()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
	}

	public override void OnRemoved()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
	}

	private void OnNewPhaseStarted()
	{
		if (!Data.Rotten && !Data.Burned)
		{
			Data.Age++;
			if (Data.Age >= 15 && !ReservedForTask)
			{
				Data.Rotten = true;
			}
		}
	}
}
