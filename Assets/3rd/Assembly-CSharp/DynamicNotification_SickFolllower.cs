using System;
using System.Collections.Generic;
using UnityEngine;

public class DynamicNotification_SickFolllower : DynamicNotificationData
{
	private List<int> _sickFollowerIDs = new List<int>();

	private int _soonestDeathID;

	public override NotificationCentre.NotificationType Type
	{
		get
		{
			return NotificationCentre.NotificationType.Dynamic_Sick;
		}
	}

	public override bool IsEmpty
	{
		get
		{
			return _sickFollowerIDs.Count == 0;
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
			if (_soonestDeathID != 0)
			{
				FollowerBrain followerBrain = FollowerBrain.FindBrainByID(_soonestDeathID);
				if (followerBrain != null)
				{
					result = Mathf.Clamp(1f - (100f - followerBrain.Stats.Illness) / 100f, 0.1f, 1f);
				}
				else
				{
					RemoveFollower(_soonestDeathID);
				}
			}
			else
			{
				_sickFollowerIDs.Clear();
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
			return _sickFollowerIDs.Count;
		}
	}

	public override string SkinName
	{
		get
		{
			return FollowerBrain.FindBrainByID(_sickFollowerIDs[0]).Info.SkinName;
		}
	}

	public override int SkinColor
	{
		get
		{
			return FollowerBrain.FindBrainByID(_sickFollowerIDs[0]).Info.SkinColour;
		}
	}

	public void AddFollower(FollowerBrain brain)
	{
		if (!_sickFollowerIDs.Contains(brain.Info.ID))
		{
			_sickFollowerIDs.Add(brain.Info.ID);
			RecalculateSoonestDeath();
			Action dataChanged = DataChanged;
			if (dataChanged != null)
			{
				dataChanged();
			}
		}
	}

	public void RemoveFollower(int brainID)
	{
		_sickFollowerIDs.Remove(brainID);
		RecalculateSoonestDeath();
		Action dataChanged = DataChanged;
		if (dataChanged != null)
		{
			dataChanged();
		}
	}

	private void RecalculateSoonestDeath()
	{
		_soonestDeathID = 0;
		float num = 0f;
		foreach (int sickFollowerID in _sickFollowerIDs)
		{
			FollowerBrain followerBrain = FollowerBrain.FindBrainByID(sickFollowerID);
			if (followerBrain.Stats.Illness > num)
			{
				_soonestDeathID = sickFollowerID;
				num = followerBrain.Stats.Illness;
			}
		}
	}

	public void UpdateFollowers()
	{
		for (int num = _sickFollowerIDs.Count - 1; num >= 0; num--)
		{
			if (FollowerBrain.FindBrainByID(_sickFollowerIDs[num]).Info.CursedState != Thought.Ill)
			{
				RemoveFollower(_sickFollowerIDs[num]);
			}
		}
	}
}
