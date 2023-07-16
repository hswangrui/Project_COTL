using System.Collections.Generic;

public class Structures_Waste : StructureBrain, ITaskProvider
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
		if (activity == ScheduledActivity.Work && !ReservedForTask)
		{
			FollowerTask_CleanWaste followerTask_CleanWaste = new FollowerTask_CleanWaste(Data.ID);
			tasks.Add(followerTask_CleanWaste.Priorty, followerTask_CleanWaste);
		}
	}
}
