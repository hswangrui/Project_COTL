using UnityEngine;

public class FollowerTask_IsDemon : FollowerTask
{
	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.IsDemon;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Demon;
		}
	}

	public override bool ShouldSaveDestination
	{
		get
		{
			return true;
		}
	}

	public override bool DisablePickUpInteraction
	{
		get
		{
			return true;
		}
	}

	public override bool BlockReactTasks
	{
		get
		{
			return true;
		}
	}

	public override bool BlockSermon
	{
		get
		{
			return true;
		}
	}

	public override bool BlockTaskChanges
	{
		get
		{
			return true;
		}
	}

	public override bool BlockThoughts
	{
		get
		{
			return true;
		}
	}

	public override float Priorty
	{
		get
		{
			return 1000f;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		return PriorityCategory.ExtremelyUrgent;
	}

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	protected override float SatiationChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override float RestChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (!DataManager.Instance.Followers_Demons_IDs.Contains(_brain.Info.ID))
		{
			_brain.HardSwapToTask(new FollowerTask_Idle());
			if (_brain.CurrentTaskType == FollowerTaskType.ChangeLocation)
			{
				_brain.CurrentTask.Arrive();
			}
			_brain.CompleteCurrentTask();
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return Vector3.zero;
	}
}
