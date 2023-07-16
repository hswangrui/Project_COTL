using System;

[Serializable]
public class TaskAndTime
{
	public FollowerTaskType Task;

	public float Time;

	public static void SetTaskTime(float time, FollowerTaskType task, FollowerBrain Brain)
	{
		foreach (TaskAndTime item in Brain._taskMemory)
		{
			if (item.Task == task)
			{
				item.Time = time;
				return;
			}
		}
		Brain._taskMemory.Add(new TaskAndTime
		{
			Task = task,
			Time = time
		});
	}

	public static float GetLastTaskTime(FollowerTaskType task, FollowerBrain Brain)
	{
		foreach (TaskAndTime item in Brain._taskMemory)
		{
			if (item.Task == task)
			{
				return item.Time;
			}
		}
		return 0f;
	}
}
