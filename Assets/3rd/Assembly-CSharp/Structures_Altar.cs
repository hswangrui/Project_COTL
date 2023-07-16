using System.Collections.Generic;

public class Structures_Altar : StructureBrain, ITaskProvider
{
	private DayPhase OverridePhase = DayPhase.None;

	public FollowerTask GetOverrideTask(FollowerBrain brain)
	{
		return new FollowerTask_AttendTeaching();
	}

	public bool CheckOverrideComplete()
	{
		return DataManager.Instance.PreviousSermonDayIndex >= TimeManager.CurrentDay;
	}

	public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> tasks)
	{
	}
}
