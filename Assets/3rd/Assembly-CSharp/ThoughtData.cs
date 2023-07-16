using System;
using System.Collections.Generic;

[Serializable]
public class ThoughtData
{
	public Thought ThoughtType;

	public Thought ThoughtGroup;

	public float Modifier;

	public float StartingModifier;

	public float Duration;

	public int Quantity = 1;

	public int Stacking;

	public int StackModifier;

	public int TotalCountDisplay;

	public bool ReduceOverTime;

	public List<float> CoolDowns = new List<float>();

	public List<float> TimeStarted = new List<float>();

	public int FollowerID = -1;

	public bool TrackThought;

	public ThoughtData()
	{
	}

	public ThoughtData(Thought thought)
	{
		ThoughtType = (ThoughtGroup = thought);
	}

	public ThoughtData(Thought thought, Thought thoughtGroup)
	{
		ThoughtType = thought;
		ThoughtGroup = thoughtGroup;
	}

	public void Init()
	{
		TimeStarted.Add(TimeManager.TotalElapsedGameTime);
		CoolDowns.Add(Duration);
	}

	public ThoughtData Clone()
	{
		return new ThoughtData(ThoughtType, ThoughtGroup)
		{
			TimeStarted = new List<float>(TimeStarted),
			CoolDowns = new List<float>(CoolDowns),
			Modifier = Modifier,
			Duration = Duration,
			Quantity = Quantity,
			Stacking = Stacking,
			StackModifier = StackModifier,
			TotalCountDisplay = TotalCountDisplay
		};
	}
}
