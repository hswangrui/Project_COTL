using UnityEngine;

public class FollowerTask_FakeLeisure : FollowerTask
{
	private float _gameTimeToNextStateUpdate;

	private float _searchCooldownGameMinutes;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.FakeLeisure;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _brain.HomeLocation;
		}
	}

	public bool Searching { get; private set; }

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	protected override void OnStart()
	{
		_searchCooldownGameMinutes = 10f;
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void OnArrive()
	{
		SetState(FollowerTaskState.Idle);
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (!Searching && base.State > FollowerTaskState.WaitingForLocation && (_searchCooldownGameMinutes -= deltaGameTime) < 0f)
		{
			Searching = true;
		}
		if (TryFindCompanionTask() || _state != FollowerTaskState.Idle)
		{
			return;
		}
		_gameTimeToNextStateUpdate -= deltaGameTime;
		if (_gameTimeToNextStateUpdate <= 0f)
		{
			if (Random.Range(0f, 1f) < 0.5f)
			{
				Wander();
			}
			_gameTimeToNextStateUpdate = Random.Range(20f, 30f);
		}
	}

	private void Wander()
	{
		ClearDestination();
		SetState(FollowerTaskState.GoingTo);
	}

	private bool TryFindCompanionTask()
	{
		if (Searching)
		{
			foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
			{
				if (allBrain == _brain || allBrain.Location != Location)
				{
					continue;
				}
				if (allBrain.CurrentTaskType == FollowerTaskType.DanceCircleLead)
				{
					FollowerTask_DanceCircleLead followerTask_DanceCircleLead = (FollowerTask_DanceCircleLead)allBrain.CurrentTask;
					if (followerTask_DanceCircleLead.RemainingSlotCount > 0)
					{
						FollowerTask_DanceCircleFollow followerTask_DanceCircleFollow = new FollowerTask_DanceCircleFollow();
						followerTask_DanceCircleFollow.LeadTask = followerTask_DanceCircleLead;
						InitiateLeisureTask(followerTask_DanceCircleFollow);
						return true;
					}
				}
				if (allBrain.CurrentTaskType != FollowerTaskType.FakeLeisure)
				{
					continue;
				}
				FollowerTask_FakeLeisure followerTask_FakeLeisure = (FollowerTask_FakeLeisure)allBrain.CurrentTask;
				if (followerTask_FakeLeisure.Searching)
				{
					if (Random.Range(0f, 1f) < 0.5f)
					{
						InitiateChat(allBrain, followerTask_FakeLeisure);
						return true;
					}
					InitiateDanceCircle(allBrain, followerTask_FakeLeisure);
					return true;
				}
			}
		}
		return false;
	}

	private void InitiateChat(FollowerBrain otherBrain, FollowerTask_FakeLeisure otherLeisure)
	{
		FollowerTask_Chat followerTask_Chat = new FollowerTask_Chat(otherBrain.Info.ID, true);
		FollowerTask_Chat followerTask_Chat2 = (followerTask_Chat.OtherChatTask = new FollowerTask_Chat(_brain.Info.ID, false));
		followerTask_Chat2.OtherChatTask = followerTask_Chat;
		otherLeisure.InitiateLeisureTask(followerTask_Chat2);
		InitiateLeisureTask(followerTask_Chat);
	}

	private void InitiateDanceCircle(FollowerBrain otherBrain, FollowerTask_FakeLeisure otherLeisure)
	{
		FollowerTask_DanceCircleLead followerTask_DanceCircleLead = new FollowerTask_DanceCircleLead();
		FollowerTask_DanceCircleFollow followerTask_DanceCircleFollow = new FollowerTask_DanceCircleFollow();
		followerTask_DanceCircleFollow.LeadTask = followerTask_DanceCircleLead;
		InitiateLeisureTask(followerTask_DanceCircleLead);
		otherLeisure.InitiateLeisureTask(followerTask_DanceCircleFollow);
	}

	public void InitiateLeisureTask(FollowerTask leisureTask)
	{
		Searching = false;
		if (base.Brain.CurrentTaskType != FollowerTaskType.ManualControl)
		{
			_brain.TransitionToTask(leisureTask);
		}
	}

	private void UndoStateAnimationChanges(Follower follower)
	{
		SimpleSpineAnimator.SpineChartacterAnimationData animationData = follower.SimpleAnimator.GetAnimationData(StateMachine.State.Idle);
		animationData.Animation = animationData.DefaultAnimation;
		follower.ResetStateAnimations();
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return TownCentre.RandomPositionInCachedTownCentre();
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "meditate");
	}

	public override void Cleanup(Follower follower)
	{
		UndoStateAnimationChanges(follower);
		if ((bool)follower && (bool)follower.GetComponent<Interaction_BackToWork>())
		{
			Object.Destroy(follower.GetComponent<Interaction_BackToWork>());
		}
		base.Cleanup(follower);
	}
}
