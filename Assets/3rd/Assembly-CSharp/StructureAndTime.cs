using System;

[Serializable]
public class StructureAndTime
{
	public enum IDTypes
	{
		Structure,
		Follower
	}

	public int StructureID;

	public float TimeReacted;

	public IDTypes IDType;

	public static float GetOrAddTime(int StructureID, FollowerBrain Brain, IDTypes Type)
	{
		foreach (StructureAndTime item in Brain.Stats.ReactionsAndTime)
		{
			if (item.StructureID == StructureID && item.IDType == Type)
			{
				return item.TimeReacted;
			}
		}
		return AddNewTime(StructureID, Brain, float.MinValue, Type).TimeReacted;
	}

	private static StructureAndTime AddNewTime(int ID, FollowerBrain Brain, float Time, IDTypes Type)
	{
		StructureAndTime structureAndTime = new StructureAndTime
		{
			TimeReacted = Time,
			StructureID = ID,
			IDType = Type
		};
		Brain.Stats.ReactionsAndTime.Add(structureAndTime);
		return structureAndTime;
	}

	public static void SetTime(int StructureID, FollowerBrain Brain, IDTypes Type)
	{
		foreach (StructureAndTime item in Brain.Stats.ReactionsAndTime)
		{
			if (item.StructureID == StructureID && item.IDType == Type)
			{
				item.TimeReacted = TimeManager.TotalElapsedGameTime;
				return;
			}
		}
		AddNewTime(StructureID, Brain, TimeManager.TotalElapsedGameTime, Type);
	}
}
