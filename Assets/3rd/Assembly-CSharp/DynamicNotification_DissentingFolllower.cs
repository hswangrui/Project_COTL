using System;
using System.Collections.Generic;
using UnityEngine;

public class DynamicNotification_DissentingFolllower : DynamicNotificationData
{
	private List<int> _dissenterFollowerIDs = new List<int>();

	private int _soonestLeaverID;

	public override NotificationCentre.NotificationType Type
	{
		get
		{
			return NotificationCentre.NotificationType.Dynamic_Dissenter;
		}
	}

	public override bool IsEmpty
	{
		get
		{
			return _dissenterFollowerIDs.Count == 0;
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
			return false;
		}
	}

	public override float CurrentProgress
	{
		get
		{
			float result = 1f;
			if (_soonestLeaverID != 0)
			{
				FollowerBrain followerBrain = FollowerBrain.FindBrainByID(_soonestLeaverID);
				if (followerBrain != null)
				{
					result = Mathf.Clamp(1f - (100f - followerBrain.Stats.Reeducation) / 100f, 0.1f, 1f);
				}
				else
				{
					RemoveFollower(_soonestLeaverID);
				}
			}
			else
			{
				_dissenterFollowerIDs.Clear();
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
			return _dissenterFollowerIDs.Count;
		}
	}

	public override string SkinName
	{
		get
		{
			return FollowerBrain.FindBrainByID(_dissenterFollowerIDs[_soonestLeaverID]).Info.SkinName;
		}
	}

	public override int SkinColor
	{
		get
		{
			return FollowerBrain.FindBrainByID(_dissenterFollowerIDs[_soonestLeaverID]).Info.SkinColour;
		}
	}

	public void AddFollower(FollowerBrain brain)
	{
		if (!_dissenterFollowerIDs.Contains(brain.Info.ID))
		{
			_dissenterFollowerIDs.Add(brain.Info.ID);
			RecalculateSoonestLeaver();
			Action dataChanged = DataChanged;
			if (dataChanged != null)
			{
				dataChanged();
			}
		}
	}

	public void RemoveFollower(int brainID)
	{
		_dissenterFollowerIDs.Remove(brainID);
		RecalculateSoonestLeaver();
		Action dataChanged = DataChanged;
		if (dataChanged != null)
		{
			dataChanged();
		}
	}

	private void RecalculateSoonestLeaver()
	{
		_soonestLeaverID = 0;
		float num = 0f;
		foreach (int dissenterFollowerID in _dissenterFollowerIDs)
		{
			FollowerBrain followerBrain = FollowerBrain.FindBrainByID(dissenterFollowerID);
			if (followerBrain != null && followerBrain.Stats.Reeducation > num)
			{
				_soonestLeaverID = dissenterFollowerID;
				num = followerBrain.Stats.Reeducation;
			}
		}
	}

	public void UpdateFollowers()
	{
		for (int num = _dissenterFollowerIDs.Count - 1; num >= 0; num--)
		{
			FollowerBrain followerBrain = FollowerBrain.FindBrainByID(_dissenterFollowerIDs[num]);
			if (followerBrain != null && followerBrain.Info.CursedState == Thought.Dissenter)
			{
				RemoveFollower(_dissenterFollowerIDs[num]);
			}
		}
	}
}
