using System.Collections.Generic;
using MMBiomeGeneration;
using UnityEngine;

[CreateAssetMenu(menuName = "Massive Monster/Story Objective Data")]
public class StoryObjectiveData : ScriptableObject
{
	public int UniqueStoryID;

	public bool IsEntryStory;

	public string GiveQuestTerm;

	public string CompleteQuestTerm;

	[Space]
	public int QuestIndex;

	public bool TargetQuestGiver = true;

	public bool RequireTarget_1;

	public int Target1FollowerID = -1;

	public bool RequireTarget_2;

	public int Target2FollowerID = -1;

	public bool RequireTarget_Deadbody;

	public int DeadBodyFollowerID = -1;

	public int QuestGiverRequiresID = -1;

	public bool HasTimer = true;

	public List<BiomeGenerator.VariableAndCondition> ConditionalVariables;

	public List<StoryObjectiveData> ChilldStoryItems;
}
