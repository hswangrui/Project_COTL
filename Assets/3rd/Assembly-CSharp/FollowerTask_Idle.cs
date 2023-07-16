using UnityEngine;

public class FollowerTask_Idle : FollowerTask
{
	private float _gameTimeToNextStateUpdate;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Idle;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Base;
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
}
