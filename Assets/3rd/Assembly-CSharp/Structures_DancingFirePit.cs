using System;
using System.Collections.Generic;
using UnityEngine;

public class Structures_DancingFirePit : StructureBrain, ITaskProvider
{
	private const int MAX_SLOT_COUNT = 40;

	public bool[] SlotReserved = new bool[40];

	public override void OnAdded()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhase));
	}

	public override void OnRemoved()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhase));
	}

	public bool HasAvailableSlot()
	{
		bool result = false;
		for (int i = 0; i < 40; i++)
		{
			if (!SlotReserved[i])
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public bool TryClaimSlot(ref int slotIndex)
	{
		bool flag = false;
		if (slotIndex >= 0 && !SlotReserved[slotIndex])
		{
			SlotReserved[slotIndex] = true;
			flag = true;
		}
		if (!flag)
		{
			for (int i = 0; i < 40; i++)
			{
				if (!SlotReserved[i])
				{
					slotIndex = i;
					SlotReserved[i] = true;
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			slotIndex = -1;
		}
		return flag;
	}

	public void ReleaseSlot(int slotIndex)
	{
		SlotReserved[slotIndex] = false;
	}

	public Vector3 GetDancePosition(int followerId)
	{
		int num = 0;
		for (int i = 0; i < FollowerBrain.AllBrains.Count; i++)
		{
			if (FollowerBrain.AllBrains[i].Info.ID == followerId)
			{
				num = i;
				break;
			}
		}
		if (num < Interaction_FireDancePit.Instance.Positions.Length)
		{
			return Interaction_FireDancePit.Instance.Positions[num].position;
		}
		Vector3 vector = Interaction.interactions[0].transform.position + Vector3.down * 2f;
		Vector3 vector2 = UnityEngine.Random.insideUnitCircle;
		float num2 = UnityEngine.Random.Range(2f, 4f);
		return vector + vector2 * num2;
	}

	public FollowerTask GetOverrideTask(FollowerBrain brain)
	{
		return null;
	}

	public bool CheckOverrideComplete()
	{
		return true;
	}

	public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> tasks)
	{
		if (Data.IsGatheringActive)
		{
			for (int i = 0; i < 40; i++)
			{
				FollowerTask_DanceFirePit followerTask_DanceFirePit = new FollowerTask_DanceFirePit(Data.ID);
				tasks.Add(followerTask_DanceFirePit.Priorty, followerTask_DanceFirePit);
			}
		}
	}

	private void OnNewPhase()
	{
		if (!Data.IsGatheringActive && Data.GatheringEndPhase != -1)
		{
			Data.GatheringEndPhase = -1;
			Data.Fuel = 0;
		}
	}
}
