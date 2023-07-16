using System;
using System.Collections.Generic;
using UnityEngine;

public class DynamicNotification_StarvingFollower : DynamicNotificationData
{
	public List<int> _starvingFollowerIDs = new List<int>();

	private int _soonestStarverID;

	private float PrevTotalCount;

	public override NotificationCentre.NotificationType Type
	{
		get
		{
			return NotificationCentre.NotificationType.Dynamic_Starving;
		}
	}

	public override bool IsEmpty
	{
		get
		{
			return _starvingFollowerIDs.Count == 0;
		}
	}

	public override bool HasProgress
	{
		get
		{
			return true;
		}
	}

	public override bool HasDynamicProgress
	{
		get
		{
			return true;
		}
	}

	public override float CurrentProgress
	{
		get
		{
			float result = 1f;
			if (_soonestStarverID != 0)
			{
				FollowerBrain followerBrain = FollowerBrain.FindBrainByID(_soonestStarverID);
				if (followerBrain != null)
				{
					result = Mathf.Clamp(1f - (75f - followerBrain.Stats.Starvation) / 75f, 0.1f, 1f);
				}
				else
				{
					RemoveFollower(_soonestStarverID);
				}
			}
			else
			{
				_starvingFollowerIDs.Clear();
				Action dataChanged = DataChanged;
				if (dataChanged != null)
				{
					dataChanged();
				}
			}
			return result;
		}
	}

	public override float TotalCount
	{
		get
		{
			return _starvingFollowerIDs.Count;
		}
	}

	public override string SkinName
	{
		get
		{
			return FollowerBrain.FindBrainByID(_soonestStarverID).Info.SkinName;
		}
	}

	public override int SkinColor
	{
		get
		{
			return FollowerBrain.FindBrainByID(_soonestStarverID).Info.SkinColour;
		}
	}

	public void AddFollower(FollowerBrain brain)
	{
		PrevTotalCount = TotalCount;
		if (!_starvingFollowerIDs.Contains(brain.Info.ID))
		{
			_starvingFollowerIDs.Add(brain.Info.ID);
			RecalculateSoonestStarver();
		}
		CheckThoughts();
		Action dataChanged = DataChanged;
		if (dataChanged != null)
		{
			dataChanged();
		}
	}

	public void RemoveFollower(int brainID)
	{
		PrevTotalCount = TotalCount;
		_starvingFollowerIDs.Remove(brainID);
		RecalculateSoonestStarver();
		CheckThoughts();
		Action dataChanged = DataChanged;
		if (dataChanged != null)
		{
			dataChanged();
		}
	}

	private void CheckThoughts()
	{
		if (PrevTotalCount != TotalCount)
		{
			if (PrevTotalCount == 0f && TotalCount >= 0f && StructureManager.GetAllStructuresOfType<Structures_Meal>().Count == 0)
			{
				CultFaithManager.AddThought(Thought.Cult_Starving, -1, 1f);
			}
			if (PrevTotalCount >= 0f && TotalCount == 0f)
			{
				CultFaithManager.AddThought(Thought.Cult_No_Longer_Starving, -1, 1f);
			}
		}
	}

	private void RecalculateSoonestStarver()
	{
		_soonestStarverID = 0;
		float num = 0f;
		foreach (int starvingFollowerID in _starvingFollowerIDs)
		{
			FollowerBrain followerBrain = FollowerBrain.FindBrainByID(starvingFollowerID);
			if (followerBrain != null && followerBrain.Stats.Starvation > num)
			{
				_soonestStarverID = starvingFollowerID;
				num = followerBrain.Stats.Starvation;
			}
		}
	}

	public void UpdateFollowers()
	{
		for (int num = _starvingFollowerIDs.Count - 1; num >= 0; num--)
		{
			if (FollowerBrain.FindBrainByID(_starvingFollowerIDs[num]).Info.CursedState == Thought.BecomeStarving)
			{
				RemoveFollower(_starvingFollowerIDs[num]);
			}
		}
	}
}
