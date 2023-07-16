using UnityEngine;

public class FollowerTask_EatFeastTable : FollowerTask
{
	private float _feastTabletEndPhase;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.EatFeastTable;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Church;
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

	public FollowerTask_EatFeastTable(int feastTableID)
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
		_brain.AddThought(Thought.FeastTable, true);
		_brain.Stats.Satiation = 100f;
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return Interaction_FeastTable.FeastTables[0].GetEatPosition(follower);
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		if (base.State == FollowerTaskState.Doing)
		{
			follower.State.facingAngle = Utils.GetAngle(follower.transform.position, Interaction_FeastTable.FeastTables[0].transform.position);
		}
		follower.HideStats();
		follower.Interaction_FollowerInteraction.Interactable = false;
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.State.facingAngle = Utils.GetAngle(follower.transform.position, Interaction_FeastTable.FeastTables[0].transform.position);
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		follower.Interaction_FollowerInteraction.Interactable = true;
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "idle");
	}
}
