using UnityEngine;

public class FollowerTask_OldAge : FollowerTask
{
	private const float IDLE_DURATION_GAME_MINUTES_MIN = 10f;

	private const float IDLE_DURATION_GAME_MINUTES_MAX = 20f;

	private float _gameTimeToNextStateUpdate;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.OldAge;
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
		if (_state == FollowerTaskState.Idle)
		{
			_gameTimeToNextStateUpdate -= deltaGameTime;
			if (_gameTimeToNextStateUpdate <= 0f)
			{
				ClearDestination();
				SetState(FollowerTaskState.GoingTo);
				_gameTimeToNextStateUpdate = Random.Range(10f, 20f);
			}
		}
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "Old/idle-old");
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Old/walk-old");
		follower.SetOutfit(FollowerOutfitType.Old, false);
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return TownCentre.RandomCircleFromTownCentre(8f);
	}

	protected override float SatiationChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override float RestChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override float SocialChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override float VomitChange(float deltaGameTime)
	{
		return 0f;
	}
}
