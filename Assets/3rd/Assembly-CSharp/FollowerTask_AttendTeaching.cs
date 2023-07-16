using UnityEngine;

public class FollowerTask_AttendTeaching : FollowerTask
{
	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.AttendTeaching;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Church;
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

	public override bool BlockSermon
	{
		get
		{
			return false;
		}
	}

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void OnArrive()
	{
		SetState(FollowerTaskState.Doing);
	}

	protected override float SatiationChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (PlayerFarming.Location != 0)
		{
			End();
			return;
		}
		if (_brain.Location != Location)
		{
			Start();
		}
		if (base.State == FollowerTaskState.Idle)
		{
			FollowerManager.FindFollowerByID(_brain.Info.ID).transform.position = ChurchFollowerManager.Instance.GetAudienceMemberPosition(_brain);
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return ChurchFollowerManager.Instance.GetAudienceMemberPosition(_brain);
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.UseUnscaledTime = true;
		if (base.State == FollowerTaskState.Doing)
		{
			follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
			follower.SetBodyAnimation("idle-ritual-up", true);
		}
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.SetBodyAnimation("idle-ritual-up", true);
	}

	public override void OnFinaliseBegin(Follower follower)
	{
		follower.SetBodyAnimation("idle", true);
		base.OnFinaliseBegin(follower);
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		follower.UseUnscaledTime = false;
		follower.State.CURRENT_STATE = StateMachine.State.Idle;
		follower.SetBodyAnimation("idle", true);
	}

	public override void SimCleanup(SimFollower simFollower)
	{
		base.SimCleanup(simFollower);
	}
}
