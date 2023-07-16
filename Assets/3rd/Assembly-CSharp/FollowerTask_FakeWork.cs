using UnityEngine;

public class FollowerTask_FakeWork : FollowerTask
{
	private float _gameTimeToNextStateUpdate;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.FakeWork;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _brain.HomeLocation;
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
		SetState(FollowerTaskState.Idle);
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (_state != FollowerTaskState.Idle)
		{
			return;
		}
		_gameTimeToNextStateUpdate -= deltaGameTime;
		if (_gameTimeToNextStateUpdate <= 0f)
		{
			_brain.CheckChangeTask();
			if (Random.Range(0f, 1f) < 0.5f)
			{
				Wander();
			}
			_gameTimeToNextStateUpdate = Random.Range(3f, 7f);
		}
	}

	private void Wander()
	{
		ClearDestination();
		SetState(FollowerTaskState.GoingTo);
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return TownCentre.RandomPositionInCachedTownCentre();
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, (Random.value < 0.5f) ? "dig" : "sweep-floor");
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		follower.SimpleAnimator.ResetAnimationsToDefaults();
		follower.ResetStateAnimations();
	}
}
