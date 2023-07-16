using UnityEngine;

public class FollowerTask_Starving : FollowerTask
{
	private float _gameTimeToNextStateUpdate;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Starving;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Base;
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

	protected override void OnArrive()
	{
		SetState(FollowerTaskState.Idle);
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (_brain.Stats.Starvation == 0f)
		{
			_brain.RemoveCurseState(Thought.BecomeStarving);
			End();
		}
		else if (_state == FollowerTaskState.Idle)
		{
			_gameTimeToNextStateUpdate -= deltaGameTime;
			if (_gameTimeToNextStateUpdate <= 0f)
			{
				ClearDestination();
				SetState(FollowerTaskState.GoingTo);
				_gameTimeToNextStateUpdate = Random.Range(4f, 6f);
			}
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return TownCentre.RandomPositionInCachedTownCentre();
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "Hungry/idle-hungry");
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Hungry/walk-hungry");
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "idle");
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run");
	}
}
