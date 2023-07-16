using System;
using System.Collections;
using UnityEngine;

public class FollowerTask_FaithEnforce : FollowerTask
{
	private FollowerBrain targetFollower;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.FaithEnforce;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Base;
		}
	}

	public FollowerTask_FaithEnforce(FollowerBrain targetFollower)
	{
		this.targetFollower = targetFollower;
	}

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	protected override void TaskTick(float deltaGameTime)
	{
	}

	protected override void OnStart()
	{
		base.OnStart();
		OnFollowerTaskStateChanged = (FollowerTaskDelegate)Delegate.Combine(OnFollowerTaskStateChanged, new FollowerTaskDelegate(StateChange));
		if (targetFollower != null)
		{
			FollowerTask currentTask = targetFollower.CurrentTask;
			if (currentTask != null)
			{
				currentTask.Abort();
			}
			targetFollower.HardSwapToTask(new FollowerTask_ManualControl());
		}
		ClearDestination();
		SetState(FollowerTaskState.GoingTo);
	}

	private void StateChange(FollowerTaskState oldState, FollowerTaskState newState)
	{
		if (oldState != FollowerTaskState.GoingTo || newState != FollowerTaskState.Doing)
		{
			return;
		}
		Follower follower = null;
		Follower follower2 = null;
		foreach (Follower item in FollowerManager.FollowersAtLocation(FollowerLocation.Base))
		{
			if (item.Brain.Info.ID == targetFollower.Info.ID)
			{
				follower = item;
			}
			else if (item.Brain.Info.ID == _brain.Info.ID)
			{
				follower2 = item;
			}
		}
		if ((bool)follower && (bool)follower2 && follower2.gameObject.activeInHierarchy)
		{
			follower2.StartCoroutine(FaithEnforceIE(follower, follower2));
		}
		else
		{
			FaithEnforce();
		}
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		if (targetFollower != null)
		{
			targetFollower.CompleteCurrentTask();
		}
		Interaction component = follower.GetComponent<interaction_FollowerInteraction>();
		if ((bool)component)
		{
			component.enabled = true;
		}
	}

	public override void SimCleanup(SimFollower simFollower)
	{
		base.SimCleanup(simFollower);
		if (targetFollower != null)
		{
			targetFollower.CompleteCurrentTask();
		}
	}

	private IEnumerator FaithEnforceIE(Follower target, Follower enforcer)
	{
		Interaction interaction = enforcer.GetComponent<interaction_FollowerInteraction>();
		if ((bool)interaction)
		{
			interaction.enabled = false;
		}
		target.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		enforcer.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		target.SetBodyAnimation("Conversations/talk-nice" + UnityEngine.Random.Range(1, 4), false);
		enforcer.SetBodyAnimation("Conversations/talk-nice" + UnityEngine.Random.Range(1, 4), false);
		target.State.facingAngle = Utils.GetAngle(target.transform.position, enforcer.transform.position);
		enforcer.State.facingAngle = Utils.GetAngle(enforcer.transform.position, target.transform.position);
		target.Brain._directInfoAccess.FaithedToday = true;
		yield return new WaitForSeconds(2f);
		targetFollower.AddThought(Thought.FaithEnforced);
		targetFollower.AddAdoration(FollowerBrain.AdorationActions.FaithEnforce, null);
		AudioManager.Instance.PlayOneShot("event:/followers/gain_loyalty", target.transform.position);
		target.Brain.CompleteCurrentTask();
		enforcer.Brain.CompleteCurrentTask();
		if ((bool)interaction)
		{
			interaction.enabled = true;
		}
	}

	private void FaithEnforce()
	{
		targetFollower.AddThought(Thought.FaithEnforced);
		targetFollower.CompleteCurrentTask();
		targetFollower._directInfoAccess.FaithedToday = true;
		_brain.CompleteCurrentTask();
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		if (follower != null)
		{
			Follower follower2 = FollowerManager.FindFollowerByID(targetFollower.Info.ID);
			if (follower2 != null)
			{
				return follower2.transform.position + Vector3.right * ((follower2.transform.position.x < follower.transform.position.x) ? 1.5f : (-1.5f));
			}
			return _brain.LastPosition;
		}
		return targetFollower.LastPosition;
	}
}
