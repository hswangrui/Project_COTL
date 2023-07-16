using System;
using System.Collections.Generic;

public class Structures_Prison : StructureBrain
{
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
		if (Data.FollowerID == -1)
		{
			return;
		}
		FollowerInfo infoByID = FollowerInfo.GetInfoByID(Data.FollowerID);
		if (infoByID != null)
		{
			FollowerBrain orCreateBrain = FollowerBrain.GetOrCreateBrain(infoByID);
			if (orCreateBrain != null)
			{
				orCreateBrain.AddThought(Thought.PrisonReEducation, false, true);
			}
		}
	}

	public void Reeducate(FollowerBrain brain)
	{
		for (int i = 0; i < 4; i++)
		{
			brain.AddThought(Thought.PrisonReEducation, false, true);
		}
		brain.Stats.Reeducation -= 25f;
		if (brain.Stats.Reeducation > 0f && brain.Stats.Reeducation < 2f)
		{
			brain.Stats.Reeducation = 0f;
		}
	}

	public static void RemoveFollower(int ID)
	{
		List<Structures_Prison> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_Prison>();
		for (int i = 0; i < allStructuresOfType.Count; i++)
		{
			if (allStructuresOfType[i].Data.FollowerID == ID)
			{
				allStructuresOfType[i].Data.FollowerID = -1;
			}
		}
		FollowerInfo infoByID = FollowerInfo.GetInfoByID(ID, true);
		if (DataManager.Instance.Followers_Imprisoned_IDs.Contains(ID) && (infoByID == null || !infoByID.DiedInPrison))
		{
			DataManager.Instance.Followers_Imprisoned_IDs.Remove(ID);
		}
	}
}
