using System;
using UnityEngine;

public class FollowerTask_ManualControl : FollowerTask
{
	private bool CustomDestination;

	private Vector3 Destination;

	private Action Callback;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.ManualControl;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _brain.Location;
		}
	}

	public override bool DisablePickUpInteraction
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

	public override bool BlockReactTasks
	{
		get
		{
			return true;
		}
	}

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	protected override void TaskTick(float deltaGameTime)
	{
	}

	public void GoToAndStop(Follower Follower, Vector3 Destination, Action Callback)
	{
		ClearDestination();
		CustomDestination = true;
		this.Destination = Destination;
		this.Callback = Callback;
		OnGoingToBegin(Follower);
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		if (!CustomDestination)
		{
			return follower.transform.position;
		}
		return Destination;
	}

	protected override void OnArrive()
	{
		base.OnArrive();
		Action callback = Callback;
		if (callback != null)
		{
			callback();
		}
	}

	protected override void OnEnd()
	{
		base.OnEnd();
	}

	public override void OnFinaliseBegin(Follower follower)
	{
		base.OnFinaliseBegin(follower);
	}

	protected override float SatiationChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override float RestChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override void OnComplete()
	{
		base.OnComplete();
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
	}

	public override void SimCleanup(SimFollower simFollower)
	{
		base.SimCleanup(simFollower);
	}
}
