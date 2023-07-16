using System.Collections.Generic;

public class Structures_Morgue : StructureBrain, ITaskProvider
{
	public delegate void MorgueEvent();

	public override bool IsFull
	{
		get
		{
			return Data.MultipleFollowerIDs.Count >= DEAD_BODY_SLOTS;
		}
	}

	public int DEAD_BODY_SLOTS
	{
		get
		{
			return GetCapacity(Data.Type);
		}
	}

	public event MorgueEvent OnBodyDeposited;

	public event MorgueEvent OnBodyWithdrawn;

	public static int GetCapacity(TYPES Type)
	{
		switch (Type)
		{
		case TYPES.MORGUE_1:
			return 3;
		case TYPES.MORGUE_2:
			return 6;
		default:
			return 6;
		}
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
		if (activity != 0 || ReservedForTask || IsFull)
		{
			return;
		}
		List<StructureBrain> list = new List<StructureBrain>();
		list.AddRange(StructureManager.GetAllStructuresOfType(Data.Location, TYPES.DEAD_WORSHIPPER));
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].ReservedByPlayer || list[num].ReservedForTask || list[num].Data.BeenInMorgueAlready || list[num].Data.BodyWrapped)
			{
				list.RemoveAt(num);
			}
		}
		if (!ReservedForTask && list.Count > 0)
		{
			FollowerTask_Undertaker followerTask_Undertaker = new FollowerTask_Undertaker(Data.ID);
			tasks.Add(followerTask_Undertaker.Priorty, followerTask_Undertaker);
		}
	}

	public void DepositBody(int followerID)
	{
		if (!Data.MultipleFollowerIDs.Contains(followerID))
		{
			Data.MultipleFollowerIDs.Add(followerID);
		}
		if (Data.MultipleFollowerIDs.Count >= DEAD_BODY_SLOTS && NotificationCentre.Instance != null)
		{
			NotificationCentre.Instance.PlayGenericNotification("Notifications/Morgue_Full/Notification/On");
		}
		MorgueEvent onBodyDeposited = this.OnBodyDeposited;
		if (onBodyDeposited != null)
		{
			onBodyDeposited();
		}
	}

	public void WithdrawBody(int followerID)
	{
		Data.MultipleFollowerIDs.Remove(followerID);
		MorgueEvent onBodyWithdrawn = this.OnBodyWithdrawn;
		if (onBodyWithdrawn != null)
		{
			onBodyWithdrawn();
		}
	}

	public void WithdrawBodies(List<FollowerInfo> followers)
	{
		foreach (FollowerInfo follower in followers)
		{
			WithdrawBody(follower.ID);
		}
	}
}
