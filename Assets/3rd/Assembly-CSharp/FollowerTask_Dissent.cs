using System;
using System.Collections;
using UnityEngine;

public class FollowerTask_Dissent : FollowerTask
{
	private const float SPEECH_DURATION_GAME_MINUTES_MIN = 45f;

	private const float SPEECH_DURATION_GAME_MINUTES_MAX = 60f;

	private const float IDLE_DURATION_GAME_MINUTES_MIN = 10f;

	private const float IDLE_DURATION_GAME_MINUTES_MAX = 30f;

	private bool _readyForSpeech = true;

	private float _gameTimeToNextStateUpdate;

	private float _speechDurationRemaining;

	private Coroutine _dissentBubbleCoroutine;

	public Action<WorshipperBubble.SPEECH_TYPE> OnDissentBubble;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Dissent;
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
				_gameTimeToNextStateUpdate = UnityEngine.Random.Range(10f, 30f);
				_readyForSpeech = true;
			}
			else if (_readyForSpeech && UnityEngine.Random.value < 0.005f)
			{
				SetState(FollowerTaskState.Doing);
				_speechDurationRemaining = UnityEngine.Random.Range(45f, 60f);
				_readyForSpeech = false;
			}
		}
		else
		{
			if (_state != FollowerTaskState.Doing || !((_speechDurationRemaining -= deltaGameTime) <= 0f))
			{
				return;
			}
			Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
			if (follower != null)
			{
				follower.TimedAnimation("Reactions/react-determined1", 2f, delegate
				{
					SetState(FollowerTaskState.Idle);
				}, false);
			}
			else
			{
				SetState(FollowerTaskState.Idle);
			}
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return TownCentre.RandomPositionInCachedTownCentre();
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.SetBodyAnimation("Dissenters/dissenter", true);
		_dissentBubbleCoroutine = follower.StartCoroutine(DissentBubbleRoutine(follower));
	}

	public override void OnDoingEnd(Follower follower)
	{
		follower.SetBodyAnimation(follower.AnimIdle, true);
		if (_dissentBubbleCoroutine != null)
		{
			follower.StopCoroutine(_dissentBubbleCoroutine);
			_dissentBubbleCoroutine = null;
		}
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		if (_dissentBubbleCoroutine != null)
		{
			follower.StopCoroutine(_dissentBubbleCoroutine);
			_dissentBubbleCoroutine = null;
			follower.WorshipperBubble.Close();
		}
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "idle");
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run");
	}

	private IEnumerator DissentBubbleRoutine(Follower follower)
	{
		float bubbleTimer = 0.3f;
		while (true)
		{
			float num;
			bubbleTimer = (num = bubbleTimer - Time.deltaTime);
			if (num < 0f && _speechDurationRemaining > 6f)
			{
				WorshipperBubble.SPEECH_TYPE sPEECH_TYPE = (WorshipperBubble.SPEECH_TYPE)(6 + UnityEngine.Random.Range(0, 3));
				follower.WorshipperBubble.Play(sPEECH_TYPE);
				bubbleTimer = 4 + UnityEngine.Random.Range(0, 2);
				Action<WorshipperBubble.SPEECH_TYPE> onDissentBubble = OnDissentBubble;
				if (onDissentBubble != null)
				{
					onDissentBubble(sPEECH_TYPE);
				}
			}
			yield return null;
		}
	}
}
