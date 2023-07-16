using System.Collections.Generic;

public class Structures_FishingHut : StructureBrain, ITaskProvider
{
	public const int MaxFish = 5;

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
			FollowerTask_Fisherman followerTask_Fisherman = new FollowerTask_Fisherman(Data.ID);
			tasks.Add(followerTask_Fisherman.Priorty, followerTask_Fisherman);
		}
	}
}
