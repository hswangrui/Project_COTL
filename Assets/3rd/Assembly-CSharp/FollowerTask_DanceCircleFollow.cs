using UnityEngine;

public class FollowerTask_DanceCircleFollow : FollowerTask
{
	public FollowerTask_DanceCircleLead LeadTask;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.DanceCircleFollow;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return LeadTask.Location;
		}
	}

	public int DancerID
	{
		get
		{
			return _brain.Info.ID;
		}
	}

	protected override int GetSubTaskCode()
	{
		return LeadTask.LeaderID;
	}

	protected override void OnStart()
	{
		if (LeadTask.State != FollowerTaskState.Done && LeadTask.RemainingSlotCount > 0)
		{
			LeadTask.JoinCircle(this);
			SetState(FollowerTaskState.GoingTo);
		}
		else
		{
			End();
		}
	}

	protected override void TaskTick(float deltaGameTime)
	{
	}

	public void BecomeLeader(FollowerTask_DanceCircleLead newCircle)
	{
		if (base.Brain.CurrentTaskType != FollowerTaskType.ManualControl)
		{
			_brain.TransitionToTask(newCircle);
		}
	}

	public void TransferToNewCircle(FollowerTask_DanceCircleLead newCircle)
	{
		LeadTask = newCircle;
		LeadTask.JoinCircle(this);
		ClearDestination();
		SetState(FollowerTaskState.GoingTo);
	}

	private void UndoStateAnimationChanges(Follower follower)
	{
		SimpleSpineAnimator.SpineChartacterAnimationData animationData = follower.SimpleAnimator.GetAnimationData(StateMachine.State.Idle);
		animationData.Animation = animationData.DefaultAnimation;
		follower.ResetStateAnimations();
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return LeadTask.GetCirclePosition(_brain);
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		if (base.State == FollowerTaskState.Doing)
		{
			follower.State.facingAngle = Utils.GetAngle(follower.transform.position, LeadTask.CenterPosition);
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "dance");
		}
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.State.facingAngle = Utils.GetAngle(follower.transform.position, LeadTask.CenterPosition);
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "dance");
	}

	public override void Cleanup(Follower follower)
	{
		_brain.AddThought(Thought.DanceCircleFollowed);
		UndoStateAnimationChanges(follower);
		if ((bool)follower.GetComponent<Interaction_BackToWork>())
		{
			Object.Destroy(follower.GetComponent<Interaction_BackToWork>());
		}
		base.Cleanup(follower);
	}

	public override void SimCleanup(SimFollower simFollower)
	{
		_brain.AddThought(Thought.DanceCircleFollowed);
		base.SimCleanup(simFollower);
	}
}
