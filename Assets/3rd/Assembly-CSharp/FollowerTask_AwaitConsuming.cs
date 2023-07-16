using System.Collections.Generic;
using UnityEngine;

public class FollowerTask_AwaitConsuming : FollowerTask
{
	public static List<Follower> awaitingConsumingFollowers = new List<Follower>();

	private Follower follower;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.AwaitConsuming;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _brain.Location;
		}
	}

	public override bool BlockReactTasks
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

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		this.follower = follower;
		awaitingConsumingFollowers.Add(follower);
		follower.transform.position = UpdateDestination(follower);
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return follower.transform.position;
	}

	protected override void TaskTick(float deltaGameTime)
	{
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
	}
}
