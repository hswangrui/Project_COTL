using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowerTask_ZombieKillFollower : FollowerTask
{
	private FollowerBrain targetFollower;

	private Follower f;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.ZombieKillFollower;
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

	public FollowerTask_ZombieKillFollower(FollowerBrain targetFollower)
	{
		this.targetFollower = targetFollower;
	}

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		OnFollowerTaskStateChanged = (FollowerTaskDelegate)Delegate.Combine(OnFollowerTaskStateChanged, new FollowerTaskDelegate(StateChange));
		f = follower;
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
		if ((bool)follower && (bool)follower2)
		{
			follower2.StartCoroutine(KillFollowerIE(follower, follower2));
		}
		else
		{
			KillFollower();
		}
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (f == null)
		{
			KillFollower();
		}
	}

	public override void OnIdleBegin(Follower follower)
	{
		base.OnIdleBegin(follower);
		SetState(FollowerTaskState.GoingTo);
		if (targetFollower != null)
		{
			targetFollower.HardSwapToTask(new FollowerTask_ManualControl());
		}
	}

	private IEnumerator KillFollowerIE(Follower target, Follower zombie)
	{
		target.Die(NotificationCentre.NotificationType.ZombieKilledFollower);
		yield return new WaitForSeconds(2.5f);
		zombie.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		zombie.SetBodyAnimation("Insane/eat-poop", false);
		zombie.AddBodyAnimation("Insane/idle-insane", false, 0f);
		target.State.facingAngle = Utils.GetAngle(target.transform.position, zombie.transform.position);
		zombie.State.facingAngle = Utils.GetAngle(zombie.transform.position, target.transform.position);
		yield return new WaitForSeconds(1f);
		Interaction_HarvestMeat interaction_HarvestMeat = null;
		foreach (Interaction_HarvestMeat interaction_HarvestMeat2 in Interaction_HarvestMeat.Interaction_HarvestMeats)
		{
			if (interaction_HarvestMeat == null || Vector3.Distance(interaction_HarvestMeat2.transform.position, target.transform.position) < Vector3.Distance(interaction_HarvestMeat.transform.position, target.transform.position))
			{
				interaction_HarvestMeat = interaction_HarvestMeat2;
			}
		}
		if ((bool)interaction_HarvestMeat)
		{
			StructureManager.RemoveStructure(interaction_HarvestMeat.DeadWorshipper.Structure.Brain);
			UnityEngine.Object.Destroy(interaction_HarvestMeat.gameObject);
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", interaction_HarvestMeat.transform.position);
			for (int i = 0; i < 5; i++)
			{
				ResourceCustomTarget.Create(zombie.gameObject, interaction_HarvestMeat.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle, InventoryItem.ITEM_TYPE.FOLLOWER_MEAT, null);
			}
		}
		zombie.State.CURRENT_STATE = StateMachine.State.Idle;
		yield return new WaitForSeconds(1f);
		_brain.Stats.Satiation = 100f;
		_brain.HardSwapToTask(new FollowerTask_Zombie());
		_brain.AddThought(Thought.ZombieAteMeal);
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.Info.CursedState != Thought.Zombie)
			{
				allBrain.AddThought(Thought.ZombieKilledFollower);
			}
		}
	}

	private void KillFollower()
	{
		int iD = targetFollower.Info.ID;
		targetFollower.Die(NotificationCentre.NotificationType.ZombieKilledFollower);
		foreach (FollowerInfo item in DataManager.Instance.Followers_Dead)
		{
			if (item.ID != iD)
			{
				continue;
			}
			using (List<StructureBrain>.Enumerator enumerator2 = StructureManager.GetAllStructuresOfType(FollowerLocation.Base, StructureBrain.TYPES.DEAD_WORSHIPPER).GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					StructureManager.RemoveStructure(StructureBrain.GetOrCreateBrain(enumerator2.Current.Data));
				}
			}
			break;
		}
		_brain.Stats.Satiation = 100f;
		_brain.HardSwapToTask(new FollowerTask_Zombie());
		_brain.AddThought(Thought.ZombieAteMeal);
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.Info.CursedState != Thought.Zombie)
			{
				allBrain.AddThought(Thought.ZombieKilledFollower);
			}
		}
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		if (targetFollower != null && FollowerBrain.AllBrains.Contains(targetFollower))
		{
			targetFollower.CompleteCurrentTask();
		}
		OnFollowerTaskStateChanged = (FollowerTaskDelegate)Delegate.Remove(OnFollowerTaskStateChanged, new FollowerTaskDelegate(StateChange));
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return targetFollower.LastPosition + Vector3.right * ((targetFollower.LastPosition.x < follower.transform.position.x) ? 0.5f : (-0.5f));
	}
}
