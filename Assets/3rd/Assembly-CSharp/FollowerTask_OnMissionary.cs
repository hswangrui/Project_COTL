using System.Collections.Generic;
using UnityEngine;

public class FollowerTask_OnMissionary : FollowerTask
{
	private Follower follower;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.MissionaryInProgress;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Missionary;
		}
	}

	public override bool ShouldSaveDestination
	{
		get
		{
			return true;
		}
	}

	public override bool DisablePickUpInteraction
	{
		get
		{
			return true;
		}
	}

	public override bool BlockReactTasks
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

	public override bool BlockTaskChanges
	{
		get
		{
			return true;
		}
	}

	public override bool BlockThoughts
	{
		get
		{
			return true;
		}
	}

	public override float Priorty
	{
		get
		{
			return 1000f;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		return PriorityCategory.ExtremelyUrgent;
	}

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	protected override float SatiationChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override float RestChange(float deltaGameTime)
	{
		return 0f;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		this.follower = follower;
	}

	protected override void OnAbort()
	{
		base.OnAbort();
		_brain.DesiredLocation = FollowerLocation.Base;
		DataManager.Instance.Followers_OnMissionary_IDs.Remove(_brain.Info.ID);
		List<Structures_Missionary> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_Missionary>(FollowerLocation.Base);
		((allStructuresOfType != null) ? allStructuresOfType[0] : null).Data.MultipleFollowerIDs.Remove(_brain.Info.ID);
		if ((bool)follower)
		{
			follower.SetOutfit(FollowerOutfitType.Follower, false);
			follower.Interaction_FollowerInteraction.Interactable = true;
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if (follower != null)
		{
			follower.SetOutfit(FollowerOutfitType.Follower, false);
		}
		else
		{
			_brain._directInfoAccess.Outfit = FollowerOutfitType.Follower;
		}
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (TimeManager.TotalElapsedGameTime >= _brain._directInfoAccess.MissionaryTimestamp + _brain._directInfoAccess.MissionaryDuration)
		{
			_brain.CompleteCurrentTask();
			if (DataManager.Instance.Followers_OnMissionary_IDs.Contains(base.Brain.Info.ID))
			{
				_brain.HardSwapToTask(new FollowerTask_MissionaryComplete());
				DataManager.Instance.MissionariesCompleted++;
				if (DataManager.Instance.MissionariesCompleted <= 2)
				{
					_brain._directInfoAccess.MissionaryChance = 100f;
				}
				_brain._directInfoAccess.MissionaryRewards = (_brain._directInfoAccess.MissionarySuccessful ? MissionaryManager.GetReward((InventoryItem.ITEM_TYPE)_brain._directInfoAccess.MissionaryType, float.MaxValue, _brain.Info.ID) : new InventoryItem[0]);
				_brain._directInfoAccess.MissionaryFinished = true;
				int num = Random.Range(0, 10);
				if (num == 0)
				{
					_brain.AddThought(Thought.TiredFromMissionaryScared);
				}
				if (num == 1)
				{
					_brain.AddThought(Thought.TiredFromMissionaryHappy);
				}
				else
				{
					_brain.AddThought(Thought.TiredFromMissionary);
				}
				if (_brain.CurrentTaskType == FollowerTaskType.ChangeLocation)
				{
					_brain.CurrentTask.Arrive();
				}
			}
		}
		if (PlayerFarming.Location == FollowerLocation.Base)
		{
			Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
			if ((bool)follower)
			{
				FollowerManager.FollowersAtLocation(FollowerLocation.Base).Remove(follower);
				FollowerManager.FollowersAtLocation(FollowerLocation.Missionary).Add(follower);
				Object.Destroy(follower.gameObject);
			}
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return Vector3.zero;
	}
}
