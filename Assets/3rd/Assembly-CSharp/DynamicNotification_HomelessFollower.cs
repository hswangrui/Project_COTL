using System;
using UnityEngine;

public class DynamicNotification_HomelessFollower : DynamicNotificationData
{
	private int HouseCount;

	private int FollowersCount;

	private float PrevTotalCount;

	public override NotificationCentre.NotificationType Type
	{
		get
		{
			return NotificationCentre.NotificationType.Dynamic_Homeless;
		}
	}

	public override bool IsEmpty
	{
		get
		{
			return TotalCount <= 0f;
		}
	}

	public override bool HasProgress
	{
		get
		{
			return false;
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
			return 0f;
		}
	}

	public override float TotalCount
	{
		get
		{
			return Mathf.Max(0, FollowersCount - HouseCount);
		}
	}

	public override string SkinName
	{
		get
		{
			return "";
		}
	}

	public override int SkinColor
	{
		get
		{
			return 0;
		}
	}

	public void CheckAll()
	{
		if (DataManager.Instance.OnboardedHomeless && (TimeManager.CurrentDay != 1 || TimeManager.CurrentPhase == DayPhase.Night))
		{
			PrevTotalCount = TotalCount;
			HouseCount = StructureManager.GetTotalHomesCount(false, true);
			FollowersCount = DataManager.Instance.Followers.Count;
			CheckEnoughBeds();
			Action dataChanged = DataChanged;
			if (dataChanged != null)
			{
				dataChanged();
			}
		}
	}

	public void CheckFollowerCount()
	{
		if (DataManager.Instance.OnboardedHomeless && (TimeManager.CurrentDay != 1 || TimeManager.CurrentPhase == DayPhase.Night))
		{
			HouseCount = StructureManager.GetTotalHomesCount(false, true);
			PrevTotalCount = TotalCount;
			FollowersCount = DataManager.Instance.Followers.Count;
			CheckEnoughBeds();
			Action dataChanged = DataChanged;
			if (dataChanged != null)
			{
				dataChanged();
			}
		}
	}

	public void OnStructuresPlaced()
	{
		if (DataManager.Instance.OnboardedHomeless)
		{
			PrevTotalCount = TotalCount;
			HouseCount = StructureManager.GetTotalHomesCount(false, true);
			FollowersCount = DataManager.Instance.Followers.Count;
			CheckEnoughBeds();
			Action dataChanged = DataChanged;
			if (dataChanged != null)
			{
				dataChanged();
			}
		}
	}

	public void OnStructureAdded(StructuresData structure)
	{
		if (DataManager.Instance.OnboardedHomeless)
		{
			PrevTotalCount = TotalCount;
			HouseCount = StructureManager.GetTotalHomesCount(false, true);
			FollowersCount = DataManager.Instance.Followers.Count;
			CheckEnoughBeds();
			Action dataChanged = DataChanged;
			if (dataChanged != null)
			{
				dataChanged();
			}
		}
	}

	private void CheckEnoughBeds()
	{
		if (PrevTotalCount != TotalCount && DataManager.Instance.ShowCultFaith && (bool)BaseLocationManager.Instance && BaseLocationManager.Instance.StructuresPlaced && BaseLocationManager.Instance.FollowersSpawned)
		{
			if (TotalCount > 0f && PrevTotalCount == 0f && !CultFaithManager.HasThought(Thought.Cult_NotEnoughBeds))
			{
				CultFaithManager.AddThought(Thought.Cult_NotEnoughBeds, -1, 1f);
			}
			if (TotalCount == 0f && PrevTotalCount > 0f && CultFaithManager.HasThought(Thought.Cult_NotEnoughBeds))
			{
				CultFaithManager.RemoveThought(Thought.Cult_NotEnoughBeds);
			}
		}
	}
}
