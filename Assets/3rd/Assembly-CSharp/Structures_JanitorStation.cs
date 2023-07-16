using System.Collections.Generic;

public class Structures_JanitorStation : StructureBrain, ITaskProvider
{
	public FollowerTask GetOverrideTask(FollowerBrain brain)
	{
		return null;
	}

	public bool CheckOverrideComplete()
	{
		return true;
	}

	public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> tasks)
	{
		if (activity != 0 || ReservedForTask)
		{
			return;
		}
		List<StructureBrain> list = new List<StructureBrain>();
		list.AddRange(StructureManager.GetAllStructuresOfType(Data.Location, TYPES.POOP));
		list.AddRange(StructureManager.GetAllStructuresOfType(Data.Location, TYPES.VOMIT));
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].ReservedByPlayer || list[num].ReservedForTask)
			{
				list.RemoveAt(num);
			}
		}
		if (!ReservedForTask && list.Count > 0)
		{
			FollowerTask_Janitor followerTask_Janitor = new FollowerTask_Janitor(Data.ID);
			tasks.Add(followerTask_Janitor.Priorty, followerTask_Janitor);
		}
	}
}
