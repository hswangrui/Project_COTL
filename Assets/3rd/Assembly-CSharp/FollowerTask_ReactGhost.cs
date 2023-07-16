using UnityEngine;

public class FollowerTask_ReactGhost : FollowerTask
{
	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.ReactGrave;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Base;
		}
	}

	public override bool BlockTaskChanges
	{
		get
		{
			return true;
		}
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (_brain.Location != PlayerFarming.Location)
		{
			End();
		}
	}

	public override void ProgressTask()
	{
		End();
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.TimedAnimation("Reactions/react-spooked", 0.7f, delegate
		{
			follower.TimedAnimation("Reactions/react-worried2", 1.933f, delegate
			{
				ProgressTask();
			});
		});
	}

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return follower.transform.position;
	}
}
