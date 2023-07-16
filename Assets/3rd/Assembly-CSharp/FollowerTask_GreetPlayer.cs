using System;
using System.Collections;
using UnityEngine;

public class FollowerTask_GreetPlayer : FollowerTask
{
	public static int MAX_GREETERS = 3;

	private int _slotIndex;

	private Coroutine _greetCoroutine;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.GreetPlayer;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Base;
		}
	}

	public override bool BlockTaskChanges
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

	public FollowerTask_GreetPlayer(int slotIndex)
	{
		_slotIndex = slotIndex;
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void OnArrive()
	{
		SetState(FollowerTaskState.Doing);
	}

	protected override void TaskTick(float deltaGameTime)
	{
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		Vector3 position = BiomeBaseManager.Instance.PlayerGreetLocation.transform.position;
		float num = 1.5f;
		float f = (float)_slotIndex * (-180f / (float)(MAX_GREETERS - 1)) * ((float)Math.PI / 180f);
		return position + new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
	}

	public override void OnGoingToBegin(Follower follower)
	{
		follower.transform.position = Vector3.Lerp(follower.transform.position, GetDestination(follower), 0.8f);
		base.OnGoingToBegin(follower);
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "pray");
		_greetCoroutine = follower.StartCoroutine(WaitForGreetCoroutine(follower));
	}

	public override void Cleanup(Follower follower)
	{
		if (_greetCoroutine != null)
		{
			follower.StopCoroutine(_greetCoroutine);
			_greetCoroutine = null;
		}
	}

	private IEnumerator WaitForGreetCoroutine(Follower follower)
	{
		while (Vector3.Distance(follower.transform.position, PlayerFarming.Instance.transform.position) > 2.5f)
		{
			yield return null;
		}
		follower.SetBodyAnimation("cheer", false);
		yield return new WaitForSeconds(3f);
		_greetCoroutine = null;
		End();
	}

	public override void SimSetup(SimFollower simFollower)
	{
		throw new InvalidOperationException("FollowerTask_GreetPlayer is not compatible with SimFollower");
	}
}
