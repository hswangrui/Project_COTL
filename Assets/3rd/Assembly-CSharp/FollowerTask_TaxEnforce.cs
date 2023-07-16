using System;
using System.Collections;
using UnityEngine;

public class FollowerTask_TaxEnforce : FollowerTask
{
	private FollowerBrain targetFollower;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.TaxEnforce;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Base;
		}
	}

	public FollowerTask_TaxEnforce(FollowerBrain targetFollower)
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
			follower2.StartCoroutine(TaxEnforceIE(follower, follower2));
		}
		else
		{
			TaxEnforce();
		}
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

	private IEnumerator TaxEnforceIE(Follower target, Follower enforcer)
	{
		Interaction interaction = enforcer.GetComponent<interaction_FollowerInteraction>();
		if ((bool)interaction)
		{
			interaction.enabled = false;
		}
		target.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		enforcer.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		target.SetBodyAnimation("Conversations/react-mean" + UnityEngine.Random.Range(1, 4), true);
		enforcer.SetBodyAnimation("Conversations/talk-mean" + UnityEngine.Random.Range(1, 4), false);
		target.State.facingAngle = Utils.GetAngle(target.transform.position, enforcer.transform.position);
		enforcer.State.facingAngle = Utils.GetAngle(enforcer.transform.position, target.transform.position);
		yield return new WaitForSeconds(1f);
		enforcer.SetBodyAnimation("tax-enforcer", false);
		yield return new WaitForSeconds(1.9666667f);
		AudioManager.Instance.PlayOneShot("event:/followers/pop_in", target.transform.position);
		ResourceCustomTarget.Create(enforcer.gameObject, target.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
		enforcer.Brain._directInfoAccess.TaxCollected++;
		target.Brain._directInfoAccess.TaxedToday = true;
		yield return new WaitForSeconds(1f);
		if ((bool)interaction)
		{
			interaction.enabled = true;
		}
		targetFollower.AddThought(Thought.FaithEnforced);
		target.Brain.CompleteCurrentTask();
		enforcer.Brain.CompleteCurrentTask();
	}

	private void TaxEnforce()
	{
		targetFollower.AddThought(Thought.FaithEnforced);
		targetFollower.CompleteCurrentTask();
		targetFollower._directInfoAccess.TaxedToday = true;
		_brain.CompleteCurrentTask();
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		if (targetFollower == null || targetFollower.Info == null)
		{
			Abort();
			return Vector3.zero;
		}
		if (follower != null)
		{
			Follower follower2 = FollowerManager.FindFollowerByID(targetFollower.Info.ID);
			if (follower2 == null)
			{
				Abort();
				return Vector3.zero;
			}
			return follower2.transform.position + Vector3.right * ((follower2.transform.position.x < follower.transform.position.x) ? 1.5f : (-1.5f));
		}
		return targetFollower.LastPosition;
	}
}
