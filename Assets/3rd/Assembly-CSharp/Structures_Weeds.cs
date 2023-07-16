using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Structures_Weeds : StructureBrain, ITaskProvider
{
	public Action OnPrioritised;

	public Action OnProgressChanged;

	public Action OnComplete;

	public int WeedType = -1;

	public const int MaxGrowth = 5;

	public bool DropWeed
	{
		get
		{
			return false;
		}
	}

	public float WeedHP
	{
		get
		{
			return Data.ProgressTarget - Data.Progress;
		}
	}

	public bool PickedWeeds
	{
		get
		{
			return Data.Progress >= Data.ProgressTarget;
		}
	}

	public void PickWeeds(float Progress)
	{
		if (PickedWeeds)
		{
			return;
		}
		Data.Progress += Progress;
		Action onProgressChanged = OnProgressChanged;
		if (onProgressChanged != null)
		{
			onProgressChanged();
		}
		if (PickedWeeds)
		{
			Action onComplete = OnComplete;
			if (onComplete != null)
			{
				onComplete();
			}
		}
	}

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
		Data.GrowthStage = Mathf.Clamp(Data.GrowthStage += 1f, 0f, 5f);
	}

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
		if (Data.PrioritisedAsBuildingObstruction && activity == ScheduledActivity.Work && !ReservedForTask)
		{
			FollowerTask_ClearWeeds followerTask_ClearWeeds = new FollowerTask_ClearWeeds(Data.ID);
			tasks.Add(followerTask_ClearWeeds.Priorty, followerTask_ClearWeeds);
		}
	}

	public override void ToDebugString(StringBuilder sb)
	{
		base.ToDebugString(sb);
		sb.AppendLine(string.Format("StartingScale: {0}; GrowthStage: {1}", Data.StartingScale, Data.GrowthStage));
		sb.AppendLine(string.Format("Prioritised: {0}", Data.Prioritised));
	}

	public void MarkMeAsPriorityWhenBuildingPlaced(bool value)
	{
		Data.PrioritisedAsBuildingObstruction = value;
		if (value)
		{
			Action onPrioritised = OnPrioritised;
			if (onPrioritised != null)
			{
				onPrioritised();
			}
		}
	}
}
