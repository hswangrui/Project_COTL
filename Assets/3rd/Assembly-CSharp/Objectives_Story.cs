using System;

[Serializable]
public class Objectives_Story : ObjectivesData
{
	[Serializable]
	public class FinalizedData : ObjectivesDataFinalized
	{
		public override string GetText()
		{
			return "";
		}
	}

	public StoryDataItem ParentStoryDataItem;

	public StoryDataItem StoryDataItem;

	public override string Text
	{
		get
		{
			return "";
		}
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		return new FinalizedData
		{
			GroupId = GroupId,
			Index = Index,
			UniqueGroupID = UniqueGroupID
		};
	}

	public Objectives_Story()
	{
	}

	public Objectives_Story(StoryDataItem storyDataItem, StoryDataItem parentStoryDataItem)
		: base("")
	{
		ParentStoryDataItem = parentStoryDataItem;
		StoryDataItem = storyDataItem;
	}
}
