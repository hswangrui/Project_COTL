using System;
using System.Collections;
using UnityEngine;

public class FollowerTask_DissentListen : FollowerTask
{
	private int _dissentingFollowerID;

	private FollowerTask_Dissent _dissenterTask;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.DissentListen;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _dissenterTask.Location;
		}
	}

	public override bool BlockTaskChanges
	{
		get
		{
			return true;
		}
	}

	private bool argueAgainst
	{
		get
		{
			return _brain.Stats.Happiness >= 80f;
		}
	}

	public FollowerTask_DissentListen(int dissentingFollowerID)
	{
		_dissentingFollowerID = dissentingFollowerID;
		FollowerBrain followerBrain = FollowerBrain.FindBrainByID(_dissentingFollowerID);
		_dissenterTask = followerBrain.CurrentTask as FollowerTask_Dissent;
	}

	protected override int GetSubTaskCode()
	{
		return _dissentingFollowerID;
	}

	protected override void OnStart()
	{
		if (_dissenterTask != null)
		{
			FollowerTask_Dissent dissenterTask = _dissenterTask;
			dissenterTask.OnFollowerTaskStateChanged = (FollowerTaskDelegate)Delegate.Combine(dissenterTask.OnFollowerTaskStateChanged, new FollowerTaskDelegate(OnTaskStateChanged));
			SetState(FollowerTaskState.GoingTo);
		}
		else
		{
			End();
		}
	}

	protected override void OnComplete()
	{
		if (_dissenterTask != null)
		{
			FollowerTask_Dissent dissenterTask = _dissenterTask;
			dissenterTask.OnFollowerTaskStateChanged = (FollowerTaskDelegate)Delegate.Remove(dissenterTask.OnFollowerTaskStateChanged, new FollowerTaskDelegate(OnTaskStateChanged));
		}
	}

	protected override void TaskTick(float deltaGameTime)
	{
	}

	private void OnTaskStateChanged(FollowerTaskState oldState, FollowerTaskState newState)
	{
		if (newState == FollowerTaskState.Doing)
		{
			return;
		}
		if (!argueAgainst)
		{
			if (_brain.HasTrait(FollowerTrait.TraitType.Zealous))
			{
				_brain.AddThought(Thought.ListenedToDissenterZealotTrait);
			}
			else
			{
				_brain.AddThought(Thought.ListenedToDissenter);
			}
		}
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if (follower != null && follower.gameObject.activeInHierarchy)
		{
			follower.StartCoroutine(DelayReaction(follower));
		}
		else
		{
			End();
		}
	}

	private IEnumerator DelayReaction(Follower follower)
	{
		yield return new WaitForSeconds(UnityEngine.Random.Range(0.4f, 0.7f));
		if (argueAgainst)
		{
			follower.TimedAnimation("Conversations/react-mean3", 2f, base.End, false);
		}
		else if (UnityEngine.Random.value < 0.5f)
		{
			follower.TimedAnimation((UnityEngine.Random.value < 0.5f) ? "Reactions/react-worried1" : "Reactions/react-worried2", 1.9f, base.End, false);
		}
		else
		{
			follower.TimedAnimation("Reactions/react-non-believers", 1.9f, base.End, false);
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		Follower follower2 = FindDissenter();
		if ((bool)follower2)
		{
			float num = UnityEngine.Random.Range(2, 3);
			float f = Utils.GetAngle(follower2.transform.position, follower.transform.position) * ((float)Math.PI / 180f);
			return follower2.transform.position + new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
		}
		return follower.Brain.LastPosition;
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		Follower follower2 = FindDissenter();
		follower.State.facingAngle = Utils.GetAngle(follower.transform.position, follower2.transform.position);
		follower.SetBodyAnimation("Dissenters/dissenter-listening", true);
		FollowerTask_Dissent dissenterTask = _dissenterTask;
		dissenterTask.OnDissentBubble = (Action<WorshipperBubble.SPEECH_TYPE>)Delegate.Combine(dissenterTask.OnDissentBubble, new Action<WorshipperBubble.SPEECH_TYPE>(OnDissentBubble));
	}

	public override void OnDoingEnd(Follower follower)
	{
		follower.State.CURRENT_STATE = StateMachine.State.Idle;
		FollowerTask_Dissent dissenterTask = _dissenterTask;
		dissenterTask.OnDissentBubble = (Action<WorshipperBubble.SPEECH_TYPE>)Delegate.Remove(dissenterTask.OnDissentBubble, new Action<WorshipperBubble.SPEECH_TYPE>(OnDissentBubble));
	}

	public override void SimDoingEnd(SimFollower simFollower)
	{
		base.SimDoingEnd(simFollower);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
	}

	private void OnDissentBubble(WorshipperBubble.SPEECH_TYPE bubbleType)
	{
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if ((bool)follower)
		{
			follower.StartCoroutine(DelayBubble(bubbleType, follower));
		}
	}

	private IEnumerator DelayBubble(WorshipperBubble.SPEECH_TYPE bubbleType, Follower self)
	{
		yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f, 0.4f));
		bubbleType = (argueAgainst ? WorshipperBubble.SPEECH_TYPE.DISSENTARGUE : bubbleType);
		self.WorshipperBubble.Play(bubbleType);
	}

	private Follower FindDissenter()
	{
		return FollowerManager.FindFollowerByID(_dissentingFollowerID);
	}
}
