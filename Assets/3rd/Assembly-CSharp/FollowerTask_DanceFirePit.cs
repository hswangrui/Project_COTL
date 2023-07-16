using UnityEngine;

public class FollowerTask_DanceFirePit : FollowerTask
{
	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.DanceFirePit;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Church;
		}
	}

	public override int UsingStructureID
	{
		get
		{
			return 0;
		}
	}

	public override float Priorty
	{
		get
		{
			return 1000f;
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

	public override bool BlockThoughts
	{
		get
		{
			return true;
		}
	}

	public FollowerTask_DanceFirePit(int firePitID)
	{
	}

	protected override float SatiationChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override float RestChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void TaskTick(float deltaGameTime)
	{
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		_brain.AddThought(Thought.DancePit, true);
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return Interaction_FireDancePit.Instance.GetDancePosition(follower.Brain.Info.ID);
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		if (base.State == FollowerTaskState.Doing)
		{
			follower.State.facingAngle = Utils.GetAngle(follower.transform.position, Interaction_FireDancePit.Instance.transform.position);
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "dance");
		}
		follower.HideStats();
		follower.Interaction_FollowerInteraction.Interactable = false;
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.State.facingAngle = Utils.GetAngle(follower.transform.position, Interaction_FireDancePit.Instance.transform.position);
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "dance");
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		follower.Interaction_FollowerInteraction.Interactable = true;
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "idle");
	}
}
