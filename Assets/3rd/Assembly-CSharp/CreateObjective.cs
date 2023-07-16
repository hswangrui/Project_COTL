using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class CreateObjective : BaseMonoBehaviour
{
	[Serializable]
	public class ObjectiveToGive
	{
		public enum QuestType
		{
			CollectItem,
			BuildStructure,
			RecruitFollower,
			DepositFood,
			Custom,
			PlaceDecoration,
			Knucklebones
		}

		public QuestType Quest;

		public InventoryItem.ITEM_TYPE Item;

		public StructureBrain.TYPES Structure;

		public int Count = 10;

		public Objectives.CustomQuestTypes CustomQuestType;

		public int DecorationCount = 1;

		public bool ContainsSubQuest;

		public List<ObjectiveToGive> SubObjectives = new List<ObjectiveToGive>();

		[TermsPopup("")]
		public string OpponentName;
	}

	[TermsPopup("")]
	public string GroupName;

	public List<ObjectiveToGive> Objectives = new List<ObjectiveToGive>();

	public void Play()
	{
		Debug.Log("CREATE OBJECTIVE!");
		foreach (ObjectiveToGive objective in Objectives)
		{
			switch (objective.Quest)
			{
			case ObjectiveToGive.QuestType.CollectItem:
				ObjectiveManager.Add(new Objectives_CollectItem(GroupName, objective.Item, objective.Count));
				break;
			case ObjectiveToGive.QuestType.BuildStructure:
				ObjectiveManager.Add(new Objectives_BuildStructure(GroupName, objective.Structure));
				break;
			case ObjectiveToGive.QuestType.RecruitFollower:
				ObjectiveManager.Add(new Objectives_RecruitFollower(GroupName));
				break;
			case ObjectiveToGive.QuestType.DepositFood:
				ObjectiveManager.Add(new Objectives_DepositFood(GroupName));
				break;
			case ObjectiveToGive.QuestType.Custom:
				Debug.Log("CUSTOM!");
				ObjectiveManager.Add(new Objectives_Custom(GroupName, objective.CustomQuestType));
				break;
			case ObjectiveToGive.QuestType.PlaceDecoration:
				ObjectiveManager.Add(new Objectives_PlaceStructure(GroupName, StructureBrain.Categories.AESTHETIC, objective.DecorationCount, -1f));
				break;
			case ObjectiveToGive.QuestType.Knucklebones:
				ObjectiveManager.Add(new Objectives_DefeatKnucklebones(GroupName, objective.OpponentName));
				break;
			}
		}
	}
}
