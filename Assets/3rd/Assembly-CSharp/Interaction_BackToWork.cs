using I2.Loc;
using UnityEngine;

public class Interaction_BackToWork : Interaction
{
	private Follower follower;

	private FollowerTask currentTask;

	private string label;

	public void Init(Follower follower)
	{
		this.follower = follower;
		follower.Interaction_FollowerInteraction.Interactable = false;
		currentTask = follower.Brain.CurrentTask;
		if (OutlineTarget == null)
		{
			OutlineTarget = follower.Interaction_FollowerInteraction.OutlineTarget;
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		follower.Interaction_FollowerInteraction.Interactable = true;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		label = ScriptLocalization.FollowerInteractions.BackToWork;
	}

	protected override void Update()
	{
		base.Update();
		if (follower.Brain.CurrentTask != currentTask)
		{
			follower.Interaction_FollowerInteraction.Interactable = true;
			Object.Destroy(this);
		}
		else
		{
			follower.Interaction_FollowerInteraction.Interactable = false;
		}
	}

	public override void GetLabel()
	{
		if (label == null)
		{
			UpdateLocalisation();
		}
		base.Label = label;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		CultFaithManager.AddThought(Thought.Cult_InterruptedFollower, follower.Brain.Info.ID, 1f);
		if (!(follower.Brain.CurrentTask is FollowerTask_ManualControl))
		{
			follower.Brain.HardSwapToTask(new FollowerTask_ManualControl());
			follower.TimedAnimation("Conversations/react-mean" + Random.Range(1, 4), 2f, delegate
			{
				FollowerTask followerTask = follower.Brain.CurrentTask;
				if (followerTask != null)
				{
					followerTask.Abort();
				}
				Object.Destroy(this);
			});
			follower.AddBodyAnimation("idle", true, 0f);
		}
		Interactable = false;
	}
}
