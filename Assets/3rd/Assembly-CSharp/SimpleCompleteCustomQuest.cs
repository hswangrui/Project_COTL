using UnityEngine;

public class SimpleCompleteCustomQuest : MonoBehaviour
{
	public Objectives.CustomQuestTypes CustomQuestType;

	public void CompleteQuest()
	{
		ObjectiveManager.CompleteCustomObjective(CustomQuestType);
	}
}
