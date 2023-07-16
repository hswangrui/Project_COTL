using System;
using System.Collections;
using UnityEngine;

public class FollowerTask_Imprisoned : FollowerTask
{
	private Coroutine _dissentBubbleCoroutine;

	private int _prisonID;

	private StructureBrain _prison;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Imprisoned;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Base;
		}
	}

	public override bool DisablePickUpInteraction
	{
		get
		{
			return true;
		}
	}

	public override int UsingStructureID
	{
		get
		{
			return _prisonID;
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

	public override bool BlockSermon
	{
		get
		{
			return true;
		}
	}

	public FollowerTask_Imprisoned(int prisonID)
	{
		_prisonID = prisonID;
		_prison = StructureManager.GetStructureByID<StructureBrain>(_prisonID);
	}

	protected override int GetSubTaskCode()
	{
		return _prisonID;
	}

	protected override void OnStart()
	{
		StructureManager.GetStructureByID<StructureBrain>(_prisonID);
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void OnEnd()
	{
		StructureBrain structureByID = StructureManager.GetStructureByID<StructureBrain>(_prisonID);
		Follower follower = FollowerManager.FindFollowerByID(structureByID.Data.FollowerID);
		if (follower != null)
		{
			follower.Interaction_FollowerInteraction.Interactable = true;
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "idle");
		}
		Debug.Log("END!");
		structureByID.Data.FollowerID = -1;
		base.OnEnd();
	}

	protected override void OnAbort()
	{
		OnEnd();
	}

	private void OnReeducationComplete()
	{
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		FollowerBrainStats stats = follower.Brain.Stats;
		stats.OnReeducationComplete = (Action)Delegate.Remove(stats.OnReeducationComplete, new Action(OnReeducationComplete));
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "Prison/stocks-ready");
		if (_dissentBubbleCoroutine != null)
		{
			follower.StopCoroutine(_dissentBubbleCoroutine);
		}
		_dissentBubbleCoroutine = follower.StartCoroutine(DissentBubbleRoutine(follower));
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		Prison prison = FindPrison();
		if (!(prison == null))
		{
			return prison.PrisonerLocation.position;
		}
		return _prison.Data.Position;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		Vector3 vector = UpdateDestination(follower);
		if (follower.transform.position != vector)
		{
			follower.transform.position = vector;
		}
		if (base.State == FollowerTaskState.Doing)
		{
			if (_brain.Stats.Reeducation <= 0f)
			{
				follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "Prison/stocks-ready");
				if (_dissentBubbleCoroutine != null)
				{
					follower.StopCoroutine(_dissentBubbleCoroutine);
				}
				_dissentBubbleCoroutine = follower.StartCoroutine(DissentBubbleRoutine(follower));
			}
			else
			{
				follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "Prison/stocks");
			}
			if (_brain.Stats.IsStarving)
			{
				follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "Prison/stocks-hungry");
			}
		}
		if ((bool)follower)
		{
			follower.Interaction_FollowerInteraction.Interactable = false;
			FollowerBrainStats stats = follower.Brain.Stats;
			stats.OnReeducationComplete = (Action)Delegate.Combine(stats.OnReeducationComplete, new Action(OnReeducationComplete));
		}
	}

	public override void OnDoingBegin(Follower follower)
	{
		if (_brain.Stats.Reeducation <= 0f)
		{
			if (_dissentBubbleCoroutine != null)
			{
				follower.StopCoroutine(_dissentBubbleCoroutine);
			}
			_dissentBubbleCoroutine = follower.StartCoroutine(DissentBubbleRoutine(follower));
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "Prison/stocks-ready");
		}
		else
		{
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "Prison/stocks");
		}
		if (_brain.Stats.IsStarving)
		{
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "Prison/stocks-hungry");
		}
	}

	public override void OnFinaliseBegin(Follower follower)
	{
		Prison prison = FindPrison();
		if (prison != null)
		{
			follower.GoTo(prison.PrisonerExitLocation.transform.position, base.Complete);
		}
		else
		{
			Complete();
		}
	}

	public override void OnFinaliseEnd(Follower follower)
	{
		base.OnFinaliseEnd(follower);
		FollowerBrainStats stats = follower.Brain.Stats;
		stats.OnReeducationComplete = (Action)Delegate.Remove(stats.OnReeducationComplete, new Action(OnReeducationComplete));
		if (_dissentBubbleCoroutine != null)
		{
			follower.StopCoroutine(_dissentBubbleCoroutine);
			_dissentBubbleCoroutine = null;
			follower.WorshipperBubble.Close();
		}
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		FollowerBrainStats stats = follower.Brain.Stats;
		stats.OnReeducationComplete = (Action)Delegate.Remove(stats.OnReeducationComplete, new Action(OnReeducationComplete));
		if (_dissentBubbleCoroutine != null)
		{
			follower.StopCoroutine(_dissentBubbleCoroutine);
			_dissentBubbleCoroutine = null;
			follower.WorshipperBubble.Close();
		}
	}

	private Prison FindPrison()
	{
		Prison result = null;
		foreach (Prison prison in Prison.Prisons)
		{
			if (prison.StructureInfo.ID == _prisonID)
			{
				result = prison;
				break;
			}
		}
		return result;
	}

	protected override float RestChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override float ReeducationChange(float deltaGameTime)
	{
		return 0f;
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
				WorshipperBubble.SPEECH_TYPE type = WorshipperBubble.SPEECH_TYPE.READY;
				follower.WorshipperBubble.Play(type);
				bubbleTimer = 4 + UnityEngine.Random.Range(0, 2);
			}
			yield return null;
		}
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (_brain.Stats.Starvation >= 75f || _brain.Stats.Illness >= 100f || _brain.DiedOfOldAge)
		{
			_brain.DiedOfStarvation = _brain.Stats.Starvation >= 75f && DataManager.Instance.OnboardingFinished;
			_brain.DiedOfIllness = _brain.Stats.Illness >= 100f && DataManager.Instance.OnboardingFinished;
			_brain.DiedInPrison = DataManager.Instance.OnboardingFinished;
			Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
			if ((bool)follower)
			{
				follower.DieWithAnimation("Prison/stocks-die", 3f, "Prison/stocks-dead", false, -1, _brain.DiedOfIllness ? NotificationCentre.NotificationType.DiedFromIllness : NotificationCentre.NotificationType.DiedFromStarvation, SetPrisonID);
				return;
			}
			FollowerManager.FindSimFollowerByID(_brain.Info.ID).Die(NotificationCentre.NotificationType.DiedFromStarvation, _currentDestination.Value);
			SetPrisonID(null);
		}
	}

	private void SetPrisonID(GameObject body)
	{
		GameManager.GetInstance().StartCoroutine(FrameDelay(delegate
		{
			_prison.Data.FollowerID = _brain.Info.ID;
		}));
	}

	private IEnumerator FrameDelay(Action callback)
	{
		yield return new WaitForEndOfFrame();
		if (callback != null)
		{
			callback();
		}
	}
}
