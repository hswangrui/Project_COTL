using System;
using System.Collections.Generic;

public class DynamicNotification_ExhaustedFolllower : DynamicNotificationData
{
	private List<int> _exhaustedFollowerIDs = new List<int>();

	public override NotificationCentre.NotificationType Type
	{
		get
		{
			return NotificationCentre.NotificationType.Exhausted;
		}
	}

	public override bool IsEmpty
	{
		get
		{
			return _exhaustedFollowerIDs.Count == 0;
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
			float num = 0f;
			foreach (FollowerInfo item in FollowerManager.FindFollowersByID(_exhaustedFollowerIDs))
			{
				num += item.Exhaustion / 100f;
			}
			return num / (float)_exhaustedFollowerIDs.Count;
		}
	}

	public override float TotalCount
	{
		get
		{
			return _exhaustedFollowerIDs.Count;
		}
	}

	public override string SkinName
	{
		get
		{
			return FollowerBrain.FindBrainByID(_exhaustedFollowerIDs[0]).Info.SkinName;
		}
	}

	public override int SkinColor
	{
		get
		{
			return FollowerBrain.FindBrainByID(_exhaustedFollowerIDs[0]).Info.SkinColour;
		}
	}

	public void AddFollower(FollowerBrain brain)
	{
		if (!_exhaustedFollowerIDs.Contains(brain.Info.ID))
		{
			_exhaustedFollowerIDs.Add(brain.Info.ID);
			Action dataChanged = DataChanged;
			if (dataChanged != null)
			{
				dataChanged();
			}
		}
	}

	public void RemoveFollower(int brainID)
	{
		_exhaustedFollowerIDs.Remove(brainID);
		Action dataChanged = DataChanged;
		if (dataChanged != null)
		{
			dataChanged();
		}
	}

	public void UpdateFollowers()
	{
		for (int num = _exhaustedFollowerIDs.Count - 1; num >= 0; num--)
		{
			if (FollowerBrain.FindBrainByID(_exhaustedFollowerIDs[num]).Stats.Exhaustion > 0f)
			{
				RemoveFollower(_exhaustedFollowerIDs[num]);
			}
		}
	}
}
