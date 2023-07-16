using System.Collections.Generic;
using UnityEngine;

public class Structures_CookingFire : StructureBrain, ITaskProvider
{
	public FollowerTask GetOverrideTask(FollowerBrain brain)
	{
		int hungerScoreIndex = FollowerManager.GetHungerScoreIndex(brain);
		Debug.Log(string.Format("{0} hunger score index: {1}", brain.Info.Name, hungerScoreIndex));
		if (hungerScoreIndex != -1)
		{
			List<Structures_Meal> list = new List<Structures_Meal>();
			foreach (Structures_Meal item in StructureManager.GetAllStructuresOfType<Structures_Meal>(Data.Location))
			{
				if (!item.ReservedForTask)
				{
					list.Add(item);
				}
			}
			Debug.Log(string.Format("MealCount: {0}", list.Count));
			if (list.Count > hungerScoreIndex && brain.CurrentTaskType != FollowerTaskType.Sleep)
			{
				return new FollowerTask_EatMeal(list[hungerScoreIndex].Data.ID);
			}
		}
		return null;
	}

	public bool CheckOverrideComplete()
	{
		bool flag = false;
		foreach (Structures_Meal item in StructureManager.GetAllStructuresOfType<Structures_Meal>(Data.Location))
		{
			if (!item.ReservedForTask)
			{
				flag = true;
				break;
			}
		}
		bool flag2 = false;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.Stats.Satiation < 80f)
			{
				flag2 = true;
				break;
			}
		}
		if (flag)
		{
			return !flag2;
		}
		return true;
	}

	public virtual void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> tasks)
	{
	}
}
