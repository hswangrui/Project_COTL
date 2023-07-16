using System;
using System.Collections.Generic;
using UnityEngine;

public class FollowerTask_DanceCircleLead : FollowerTask
{
	public const float DANCE_DURATION_GAME_MINUTES = 180f;

	public const float EMPTY_CIRCLE_TIMEOUT_GAME_MINUTES = 10f;

	public List<FollowerTask_DanceCircleFollow> _dancers = new List<FollowerTask_DanceCircleFollow>();

	private float _emptyDancerListCountdown;

	private float _remainingDanceDuration;

	private Interaction_BackToWork backToWorkInteraction;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.DanceCircleLead;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Base;
		}
	}

	public int LeaderID
	{
		get
		{
			if (_brain == null || _brain.Info == null)
			{
				return 0;
			}
			return _brain.Info.ID;
		}
	}

	public int RemainingSlotCount
	{
		get
		{
			return 4 - _dancers.Count;
		}
	}

	public Vector3 CenterPosition { get; private set; }

	public FollowerTask_DanceCircleLead()
	{
		_remainingDanceDuration = 180f;
	}

	private FollowerTask_DanceCircleLead(float remainingDuration, Vector3 centerPosition)
	{
		_remainingDanceDuration = remainingDuration;
		CenterPosition = centerPosition;
	}

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	protected override void OnStart()
	{
		LocationState locationState = LocationManager.GetLocationState(Location);
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if (locationState == LocationState.Active && follower != null)
		{
			CenterPosition = follower.transform.position;
		}
		else
		{
			CenterPosition = TownCentre.RandomPositionInCachedTownCentre();
		}
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void OnAbort()
	{
		base.OnAbort();
		_remainingDanceDuration = -1f;
	}

	protected override void OnComplete()
	{
		if (_remainingDanceDuration > 0f && _emptyDancerListCountdown > 0f && _dancers.Count > 1)
		{
			TransferLeadership();
		}
		else
		{
			foreach (FollowerTask_DanceCircleFollow dancer in _dancers)
			{
				dancer.End();
			}
		}
		_dancers.Clear();
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if ((_remainingDanceDuration -= deltaGameTime) <= 0f)
		{
			End();
		}
		else if (_dancers.Count == 0)
		{
			if ((_emptyDancerListCountdown -= deltaGameTime) <= 0f)
			{
				End();
			}
		}
		else
		{
			_emptyDancerListCountdown = 10f;
		}
		if (TimeManager.IsNight && !_brain._directInfoAccess.WorkThroughNight)
		{
			_brain.CheckChangeTask();
		}
	}

	private void TransferLeadership()
	{
		FollowerTask_DanceCircleFollow followerTask_DanceCircleFollow = _dancers[0];
		FollowerTask_DanceCircleLead newCircle = new FollowerTask_DanceCircleLead(_remainingDanceDuration, CenterPosition);
		followerTask_DanceCircleFollow.BecomeLeader(newCircle);
		for (int i = 1; i < _dancers.Count; i++)
		{
			_dancers[i].TransferToNewCircle(newCircle);
		}
	}

	public void JoinCircle(FollowerTask_DanceCircleFollow newDancer)
	{
		_dancers.Add(newDancer);
		foreach (FollowerTask_DanceCircleFollow dancer in _dancers)
		{
			dancer.RecalculateDestination();
		}
	}

	public Vector3 GetCirclePosition(FollowerBrain brain)
	{
		int num = -1;
		if (_brain != null)
		{
			if (_brain.Info.ID == brain.Info.ID)
			{
				num = 0;
			}
			else
			{
				for (int i = 0; i < _dancers.Count; i++)
				{
					if (_dancers[i].DancerID == brain.Info.ID)
					{
						num = i + 1;
						break;
					}
				}
			}
		}
		if (num >= 0)
		{
			int num2 = _dancers.Count + 1;
			float f = (float)num * (360f / (float)num2) * ((float)Math.PI / 180f);
			return CenterPosition + new Vector3(1f * Mathf.Cos(f), 1f * Mathf.Sin(f));
		}
		return Vector3.zero;
	}

	private void UndoStateAnimationChanges(Follower follower)
	{
		SimpleSpineAnimator.SpineChartacterAnimationData animationData = follower.SimpleAnimator.GetAnimationData(StateMachine.State.Idle);
		animationData.Animation = animationData.DefaultAnimation;
		follower.ResetStateAnimations();
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return GetCirclePosition(_brain);
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		if (base.State == FollowerTaskState.Doing)
		{
			follower.State.facingAngle = Utils.GetAngle(follower.transform.position, CenterPosition);
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "dance");
		}
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.State.facingAngle = Utils.GetAngle(follower.transform.position, CenterPosition);
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "dance");
	}

	public override void Cleanup(Follower follower)
	{
		_brain.AddThought(Thought.DanceCircleLed);
		UndoStateAnimationChanges(follower);
		if ((bool)follower.GetComponent<Interaction_BackToWork>())
		{
			UnityEngine.Object.Destroy(follower.GetComponent<Interaction_BackToWork>());
		}
		base.Cleanup(follower);
	}

	public override void SimCleanup(SimFollower simFollower)
	{
		_brain.AddThought(Thought.DanceCircleLed);
		base.SimCleanup(simFollower);
	}
}
