using System.Collections;
using UnityEngine;

public class FollowerTask_GetAttention : FollowerTask
{
	private float _updateDestination;

	private float _giveUpTimer;

	private bool CanGiveUp = true;

	private float startTimeStamp;

	private const float MaxIgnoreTime = 240f;

	private bool showSpeechBubble = true;

	private Follower _follower;

	public Follower.ComplaintType ComplaintType;

	private Coroutine _dissentBubbleCoroutine;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.GetPlayerAttention;
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

	public override bool BlockTaskChanges
	{
		get
		{
			return true;
		}
	}

	public bool AutoInteract { get; private set; }

	public FollowerTask_GetAttention(Follower.ComplaintType ComplaintType, bool CanGiveUp = true)
	{
		this.ComplaintType = ComplaintType;
		this.CanGiveUp = CanGiveUp;
	}

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	protected override void OnArrive()
	{
		SetState(FollowerTaskState.Idle);
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Conversations/walkpast-nice");
		_dissentBubbleCoroutine = follower.StartCoroutine(DissentBubbleRoutine(follower));
		startTimeStamp = TimeManager.TotalElapsedGameTime + 240f;
		_follower = follower;
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (PlayerFarming.Location != Location)
		{
			End();
			return;
		}
		if (_state == FollowerTaskState.GoingTo && _currentDestination.HasValue)
		{
			if (PlayerFarming.Instance == null)
			{
				return;
			}
			Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
			if (follower == null || follower.transform == null)
			{
				return;
			}
			_updateDestination -= deltaGameTime;
			if (_updateDestination <= 0f)
			{
				RecalculateDestination();
				_updateDestination = 0.5f;
			}
		}
		else if (_state == FollowerTaskState.Idle)
		{
			PlayerFarming instance = PlayerFarming.Instance;
			if (instance == null)
			{
				return;
			}
			Follower follower2 = FollowerManager.FindFollowerByID(_brain.Info.ID);
			if (follower2 == null)
			{
				return;
			}
			float num = Vector3.Distance(instance.transform.position, follower2.transform.position);
			if (num > 3f)
			{
				ClearDestination();
				SetState(FollowerTaskState.GoingTo);
			}
			else
			{
				follower2.FacePosition(instance.transform.position);
				if (num <= 2f)
				{
					if (TimeManager.TotalElapsedGameTime > startTimeStamp && startTimeStamp != -1f && ComplaintType == Follower.ComplaintType.GiveOnboarding && PlayerFarming.Instance != null && (PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.Idle || PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.Moving))
					{
						AutoInteract = true;
					}
					if (ComplaintType == Follower.ComplaintType.ShowTwitchMessage)
					{
						Follower follower3 = FollowerManager.FindFollowerByID(_brain.Info.ID);
						if ((object)follower3 != null)
						{
							follower3.ShowTwitchMessage();
						}
					}
				}
			}
		}
		if (_dissentBubbleCoroutine == null && (bool)_follower && showSpeechBubble)
		{
			_dissentBubbleCoroutine = _follower.WorshipperBubble.StartCoroutine(DissentBubbleRoutine(_follower));
		}
		if (_state != FollowerTaskState.Doing && (_giveUpTimer += deltaGameTime) >= 60f && CanGiveUp)
		{
			EndInAnger();
		}
	}

	private void EndInAnger()
	{
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if (_dissentBubbleCoroutine != null)
		{
			follower.WorshipperBubble.StopCoroutine(_dissentBubbleCoroutine);
			_dissentBubbleCoroutine = null;
			follower.WorshipperBubble.Close();
		}
		SetState(FollowerTaskState.Doing);
		follower.TimedAnimation("tantrum", 3.2f, base.End);
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		if (PlayerFarming.Instance != null)
		{
			return PlayerFarming.Instance.transform.position + (follower.transform.position - PlayerFarming.Instance.transform.position).normalized * 2f;
		}
		return follower.transform.position;
	}

	private void UndoStateAnimationChanges(Follower follower)
	{
		SimpleSpineAnimator.SpineChartacterAnimationData animationData = follower.SimpleAnimator.GetAnimationData(StateMachine.State.Moving);
		animationData.Animation = animationData.DefaultAnimation;
		follower.ResetStateAnimations();
	}

	public override void OnIdleBegin(Follower follower)
	{
		base.OnIdleBegin(follower);
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.SetBodyAnimation("attention", true);
	}

	public override void OnIdleEnd(Follower follower)
	{
		base.OnIdleEnd(follower);
		follower.SetBodyAnimation(follower.AnimIdle, true);
	}

	private IEnumerator DissentBubbleRoutine(Follower follower)
	{
		float bubbleTimer = 0.3f;
		while (true)
		{
			float num;
			bubbleTimer = (num = bubbleTimer - Time.deltaTime);
			if (num < 0f)
			{
				WorshipperBubble.SPEECH_TYPE type = ((ComplaintType == Follower.ComplaintType.ShowTwitchMessage) ? WorshipperBubble.SPEECH_TYPE.TWITCH : WorshipperBubble.SPEECH_TYPE.HELP);
				follower.WorshipperBubble.gameObject.SetActive(true);
				follower.WorshipperBubble.Play(type);
				bubbleTimer = 4 + Random.Range(0, 2);
			}
			yield return null;
		}
	}

	protected override void OnComplete()
	{
		TimeManager.TimeSinceLastComplaint = 0f;
		startTimeStamp = -1f;
		AutoInteract = false;
		base.OnComplete();
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		UndoStateAnimationChanges(follower);
		if (_dissentBubbleCoroutine != null)
		{
			follower.WorshipperBubble.StopCoroutine(_dissentBubbleCoroutine);
			_dissentBubbleCoroutine = null;
			follower.WorshipperBubble.Close();
		}
	}

	public void StopSpeechBubble(Follower follower)
	{
		if (_dissentBubbleCoroutine != null)
		{
			showSpeechBubble = false;
			follower.WorshipperBubble.StopCoroutine(_dissentBubbleCoroutine);
			_dissentBubbleCoroutine = null;
			follower.WorshipperBubble.Close();
		}
	}
}
