using System.Collections;
using UnityEngine;

public class FollowerTask_AwaitInstruction : FollowerTask
{
	private const float SPEECH_DURATION_GAME_MINUTES_MIN = 45f;

	private const float SPEECH_DURATION_GAME_MINUTES_MAX = 55f;

	private const float IDLE_DURATION_GAME_MINUTES_MIN = 20f;

	private const float IDLE_DURATION_GAME_MINUTES_MAX = 30f;

	private bool _readyForSpeech = true;

	private float _gameTimeToNextStateUpdate;

	private float _speechDurationRemaining;

	private Coroutine _dissentBubbleCoroutine;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.AwaitInstructions;
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
				_gameTimeToNextStateUpdate = Random.Range(20f, 30f);
				_readyForSpeech = true;
			}
			else if (_readyForSpeech)
			{
				SetState(FollowerTaskState.Doing);
				_speechDurationRemaining = Random.Range(45f, 55f);
				_readyForSpeech = false;
			}
		}
		else if (_state == FollowerTaskState.Doing)
		{
			Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
			if (follower != null && PlayerFarming.Instance != null)
			{
				follower.FacePosition(PlayerFarming.Instance.transform.position);
			}
			if ((_speechDurationRemaining -= deltaGameTime) <= 0f)
			{
				SetState(FollowerTaskState.Idle);
			}
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return follower.transform.position + (Vector3)Random.insideUnitCircle * 3f;
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.SetBodyAnimation("attention", true);
		_dissentBubbleCoroutine = follower.StartCoroutine(DissentBubbleRoutine(follower));
	}

	public override void OnDoingEnd(Follower follower)
	{
		follower.SetBodyAnimation(follower.AnimIdle, true);
		if (_dissentBubbleCoroutine != null)
		{
			follower.StopCoroutine(_dissentBubbleCoroutine);
			_dissentBubbleCoroutine = null;
			follower.WorshipperBubble.Close();
		}
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
				WorshipperBubble.SPEECH_TYPE type = WorshipperBubble.SPEECH_TYPE.HELP;
				follower.WorshipperBubble.Play(type);
				bubbleTimer = 4 + Random.Range(0, 2);
			}
			yield return null;
		}
	}
}
