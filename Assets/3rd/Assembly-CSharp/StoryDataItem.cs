using System;
using System.Collections.Generic;

[Serializable]
public class StoryDataItem
{
	public int QuestGiverFollowerID = -1;

	public int TargetFollowerID_1 = -1;

	public int TargetFollowerID_2 = -1;

	public int DeadFollowerID = -1;

	public int FollowerID = -1;

	public bool QuestGiven;

	public bool QuestLocked;

	public bool QuestDeclined;

	public StoryObjectiveData StoryObjectiveData;

	public List<StoryDataItem> ChildStoryDataItems = new List<StoryDataItem>();

	public ObjectivesData Objective;
}
