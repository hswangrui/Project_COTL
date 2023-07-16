using System;
using System.Collections.Generic;
using UnityEngine;

public class Structures_Shrine : StructureBrain, ITaskProvider
{
	public float DURATION_PER_DEVOTION;

	public List<FollowerTask_Pray> Prayers = new List<FollowerTask_Pray>();

	public Action<int> OnSoulsGained;

	public int[] assignedSeats = new int[10];

	private DayPhase OverridePhase = DayPhase.None;

	public virtual int SoulMax
	{
		get
		{
			switch (Data.Type)
			{
			case TYPES.SHRINE:
				return 50;
			case TYPES.SHRINE_II:
				return 70;
			case TYPES.SHRINE_III:
				return 90;
			case TYPES.SHRINE_IV:
				return 175;
			default:
				return 0;
			}
		}
	}

	public virtual int PrayersMax
	{
		get
		{
			switch (Data.Type)
			{
			case TYPES.SHRINE:
				return 4;
			case TYPES.SHRINE_II:
				return 6;
			case TYPES.SHRINE_III:
				return 8;
			case TYPES.SHRINE_IV:
				return 10;
			default:
				return 0;
			}
		}
	}

	public int SoulCount
	{
		get
		{
			return Data.SoulCount;
		}
		set
		{
			int soulCount = SoulCount;
			Data.SoulCount = Mathf.Clamp(value, 0, SoulMax);
			if (SoulCount > soulCount)
			{
				Action<int> onSoulsGained = OnSoulsGained;
				if (onSoulsGained != null)
				{
					onSoulsGained(SoulCount - soulCount);
				}
			}
		}
	}

	public float DevotionSpeedMultiplier
	{
		get
		{
			if (Data.FullyFueled && UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Shrine_Flame))
			{
				return 1.2f;
			}
			if (Data.FullyFueled && UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Shrine_FlameII))
			{
				return 1.4f;
			}
			return 1f;
		}
	}

	public void AddPrayer(FollowerTask_Pray prayer)
	{
		if (!Prayers.Contains(prayer))
		{
			Prayers.Add(prayer);
		}
	}

	public void RemovePrayer(FollowerTask_Pray prayer)
	{
		if (Prayers.Contains(prayer))
		{
			Prayers.Remove(prayer);
		}
	}

	public Vector3 GetPrayerPosition(FollowerBrain follower)
	{
		Vector3 position = Data.Position;
		BuildingShrine shrine = GetShrine();
		if ((bool)shrine)
		{
			for (int i = 0; i < assignedSeats.Length; i++)
			{
				if (i < PrayersMax && assignedSeats[i] == follower.Info.ID)
				{
					return shrine.SpawnPositions[i].transform.position;
				}
			}
			for (int j = 0; j < assignedSeats.Length; j++)
			{
				if (j < PrayersMax)
				{
					FollowerInfo infoByID = FollowerInfo.GetInfoByID(assignedSeats[j]);
					if (infoByID == null || assignedSeats[j] == 0 || !(FollowerBrain.GetOrCreateBrain(infoByID).CurrentTask is FollowerTask_Pray))
					{
						assignedSeats[j] = follower.Info.ID;
						return shrine.SpawnPositions[j].transform.position;
					}
				}
			}
			follower.CurrentTask.Abort();
			return position;
		}
		return position;
	}

	private BuildingShrine GetShrine()
	{
		foreach (BuildingShrine shrine in BuildingShrine.Shrines)
		{
			if (shrine.StructureBrain == this)
			{
				return shrine;
			}
		}
		return null;
	}

	public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> tasks)
	{
		if (activity == ScheduledActivity.Work && SoulCount < SoulMax)
		{
			for (int i = 0; i < PrayersMax - Prayers.Count; i++)
			{
				FollowerTask_Pray followerTask_Pray = new FollowerTask_Pray(Data.ID);
				tasks.Add(followerTask_Pray.Priorty, followerTask_Pray);
			}
		}
	}

	public virtual FollowerTask GetOverrideTask(FollowerBrain brain)
	{
		Debug.Log("GetOverrideTask");
		return new FollowerTask_Pray(Data.ID);
	}

	public bool CheckOverrideComplete()
	{
		return false;
	}

	public override void OnAdded()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
	}

	public override void OnRemoved()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
	}

	private void OnNewPhaseStarted()
	{
		UpdateFuel();
	}
}
