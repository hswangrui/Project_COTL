using System.Collections.Generic;

public interface ITaskProvider
{
	FollowerTask GetOverrideTask(FollowerBrain brain);

	bool CheckOverrideComplete();

	void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> sortedTasks);
}
