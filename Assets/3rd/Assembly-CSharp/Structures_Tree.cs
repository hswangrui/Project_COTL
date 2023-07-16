using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Structures_Tree : StructureBrain, ITaskProvider
{
	public Action OnRegrowTree;

	public Action<int> OnTreeProgressChanged;

	public Action OnRegrowTreeProgressChanged;

	public Action<bool> OnTreeComplete;

	public bool RegrowTree = true;

	public float TreeHP
	{
		get
		{
			return Data.ProgressTarget - Data.Progress;
		}
	}

	public bool TreeChopped
	{
		get
		{
			return Data.Progress >= Data.ProgressTarget;
		}
	}

	public void TreeHit(float treeDamage = 1f, bool dropLoot = true, int followerID = -1)
	{
		if (TreeChopped)
		{
			return;
		}
		Data.Progress += treeDamage;
		Action<int> onTreeProgressChanged = OnTreeProgressChanged;
		if (onTreeProgressChanged != null)
		{
			onTreeProgressChanged(followerID);
		}
		if (TreeChopped)
		{
			Action<bool> onTreeComplete = OnTreeComplete;
			if (onTreeComplete != null)
			{
				onTreeComplete(dropLoot);
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
		if (Data.IsSapling)
		{
			Data.GrowthStage += 1f;
			Debug.Log("Growth: " + Data.GrowthStage);
			Action onRegrowTreeProgressChanged = OnRegrowTreeProgressChanged;
			if (onRegrowTreeProgressChanged != null)
			{
				onRegrowTreeProgressChanged();
			}
		}
		if (Data.GrowthStage >= 6f)
		{
			Data.IsSapling = false;
			Data.Progress = 0f;
			Data.GrowthStage = 0f;
			Action onRegrowTree = OnRegrowTree;
			if (onRegrowTree != null)
			{
				onRegrowTree();
			}
		}
	}

	public override void ToDebugString(StringBuilder sb)
	{
		base.ToDebugString(sb);
		sb.AppendLine(string.Format("HP: {0}/{1}, Chopped: {2}", TreeHP, Data.ProgressTarget, TreeChopped));
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
		if (activity == ScheduledActivity.Work && !ReservedForTask && !TreeChopped && !Data.IsSapling)
		{
			FollowerTask_ChopTrees followerTask_ChopTrees = new FollowerTask_ChopTrees(Data.ID);
			tasks.Add(followerTask_ChopTrees.Priorty, followerTask_ChopTrees);
		}
	}
}
