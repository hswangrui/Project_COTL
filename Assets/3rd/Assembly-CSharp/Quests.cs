using System.Collections.Generic;
using System.Linq;
using MMBiomeGeneration;
using UnityEngine;

public static class Quests
{
	public const int MaxQuestsActive = 1;

	private static List<ObjectivesData> QuestsAll = new List<ObjectivesData>
	{
		new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_AlmsToPoor, -1, 0, 4800f),
		new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_AssignFaithEnforcer, -1, 1, 4800f),
		new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_AssignTaxCollector, -1, 1, 4800f),
		new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_Brainwashing, -1, 0, 4800f),
		new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_ConsumeFollower, -1, 1, 4800f),
		new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_DonationRitual, -1, 0, 4800f),
		new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_Enlightenment, -1, 0, 4800f),
		new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_Fast, -1, 0, 4800f),
		new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_Feast, -1, 0, 4800f),
		new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_FasterBuilding, -1, 0, 4800f),
		new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_Fightpit, -1, 2, 4800f),
		new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_FishingRitual, -1, 0, 4800f),
		new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_Funeral, -1, 1, 4800f),
		new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_HarvestRitual, -1, 0, 4800f),
		new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_Holiday, -1, 0, 4800f),
		new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_Ressurect, -1, 1, 4800f),
		new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_Sacrifice, -1, 1, 3600f),
		new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_Wedding, -1, 1, 4800f),
		new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_WorkThroughNight, -1, 0, 4800f),
		new Objectives_Custom("Objectives/GroupTitles/Quest", Objectives.CustomQuestTypes.SendFollowerOnMissionary, -1, 3600f),
		new Objectives_Custom("Objectives/GroupTitles/Quest", Objectives.CustomQuestTypes.SendFollowerToPrison, -1, 3600f),
		new Objectives_CookMeal("Objectives/GroupTitles/Quest", InventoryItem.ITEM_TYPE.NONE, 10, 4800f),
		new Objectives_CookMeal("Objectives/GroupTitles/Quest", InventoryItem.ITEM_TYPE.MEAL_GREAT, 3, 9600f),
		new Objectives_CookMeal("Objectives/GroupTitles/Quest", InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH, 2, 4800f),
		new Objectives_PlaceStructure("Objectives/GroupTitles/Quest", StructureBrain.Categories.AESTHETIC, 3, 4800f),
		new Objectives_Custom("Objectives/GroupTitles/Quest", Objectives.CustomQuestTypes.UseFirePit, -1, 4800f),
		new Objectives_BuildStructure("Objectives/GroupTitles/Quest", StructureBrain.TYPES.OUTHOUSE, 2, 3600f),
		new Objectives_Custom("Objectives/GroupTitles/Quest", Objectives.CustomQuestTypes.MurderFollower, -1, 4800f),
		new Objectives_Custom("Objectives/GroupTitles/Quest", Objectives.CustomQuestTypes.MurderFollowerAtNight, -1, 4800f),
		new Objectives_EatMeal("Objectives/GroupTitles/Quest", StructureBrain.TYPES.MEAL_POOP, 3600f),
		new Objectives_EatMeal("Objectives/GroupTitles/Quest", StructureBrain.TYPES.MEAL_FOLLOWER_MEAT, 4800f),
		new Objectives_RecruitCursedFollower("Objectives/GroupTitles/Quest", Thought.BecomeStarving, Random.Range(2, 3)),
		new Objectives_RecruitCursedFollower("Objectives/GroupTitles/Quest", Thought.Ill, Random.Range(2, 3)),
		new Objectives_RecruitCursedFollower("Objectives/GroupTitles/Quest", Thought.OldAge, Random.Range(2, 3) - 1),
		new Objectives_RecruitCursedFollower("Objectives/GroupTitles/Quest", Thought.Dissenter, Random.Range(2, 3) - 1),
		new Objectives_CollectItem("Objectives/GroupTitles/Quest", InventoryItem.ITEM_TYPE.FLOWER_RED, 10, false, FollowerLocation.Dungeon1_1, 4800f),
		new Objectives_CollectItem("Objectives/GroupTitles/Quest", InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, 10, false, FollowerLocation.Dungeon1_2, 4800f),
		new Objectives_CollectItem("Objectives/GroupTitles/Quest", InventoryItem.ITEM_TYPE.CRYSTAL, 10, false, FollowerLocation.Dungeon1_3, 4800f),
		new Objectives_CollectItem("Objectives/GroupTitles/Quest", InventoryItem.ITEM_TYPE.SPIDER_WEB, 10, false, FollowerLocation.Dungeon1_4, 4800f),
		new Objectives_FindFollower("Objectives/GroupTitles/Quest", FollowerLocation.Dungeon1_1, "Cat", 0, 0, "Test", 0, 4800f),
		new Objectives_BuildStructure("Objectives/GroupTitles/Quest", StructureBrain.TYPES.DECORATION_BONE_CANDLE, 1, 3600f),
		new Objectives_TalkToFollower("Objectives/GroupTitles/Quest", "Story_6_0/Response", 3600f),
		new Objectives_Custom("Objectives/GroupTitles/Quest", Objectives.CustomQuestTypes.KillFollower, -1, 3600f),
		new Objectives_FindFollower("Objectives/GroupTitles/Quest", FollowerLocation.Dungeon1_2, "Cat", 0, 0, "Test", 1, 4800f),
		new Objectives_FindFollower("Objectives/GroupTitles/Quest", FollowerLocation.Dungeon1_3, "Cat", 0, 0, "Test", 2, 4800f),
		new Objectives_FindFollower("Objectives/GroupTitles/Quest", FollowerLocation.Dungeon1_4, "Cat", 0, 0, "Test", 3, 4800f),
		new Objectives_EatMeal("Objectives/GroupTitles/Quest", StructureBrain.TYPES.MEAL_GRASS, 3600f)
	};

	private static StoryObjectiveData[] allStoryObjectiveDatas;

	public static bool IsDebug = false;

	public static List<Objectives_RoomChallenge> DungeonRoomChallenges = new List<Objectives_RoomChallenge>
	{
		new Objectives_NoCurses("Objectives/GroupTitles/Challenge", 3),
		new Objectives_NoDamage("Objectives/GroupTitles/Challenge", 3),
		new Objectives_NoDodge("Objectives/GroupTitles/Challenge", 3)
	};

	public static StoryObjectiveData[] AllStoryObjectiveDatas
	{
		get
		{
			if (allStoryObjectiveDatas == null)
			{
				allStoryObjectiveDatas = Resources.LoadAll<StoryObjectiveData>("Data/Story Data");
			}
			return allStoryObjectiveDatas;
		}
	}

	public static ObjectivesData GetQuest(int followerID, int targetFollowerID_1 = -1, int targetFollowerID_2 = -1, int deadFollowerID = -1, bool assignTargetFollowers = false, ObjectivesData targetQuest = null)
	{
		float num = 1f;
		List<ObjectivesData> list = new List<ObjectivesData>();
		List<ObjectivesData> list2 = new List<ObjectivesData>();
		if (targetFollowerID_1 != -1 && (!FollowerManager.UniqueFollowerIDs.Contains(followerID) || targetQuest != null))
		{
			list.AddRange(QuestsAll);
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				list[num2].Index = num2;
				if (num2 == 25)
				{
					list[num2] = null;
					continue;
				}
				bool flag = false;
				for (int i = 0; i < DataManager.Instance.CompletedQuestsHistorys.Count; i++)
				{
					if (DataManager.Instance.CompletedQuestsHistorys[i].QuestIndex == num2 && !DataManager.Instance.CompletedQuestsHistorys[i].IsStory && TimeManager.TotalElapsedGameTime - DataManager.Instance.CompletedQuestsHistorys[i].QuestTimestamp < DataManager.Instance.CompletedQuestsHistorys[i].QuestCooldownDuration)
					{
						flag = true;
					}
				}
				if (flag && targetQuest == null)
				{
					list[num2] = null;
					continue;
				}
				if (CultFaithManager.CurrentFaith < 25f && list[num2] != null && list[num2] is Objectives_PlaceStructure && ((Objectives_PlaceStructure)list[num2]).category == StructureBrain.Categories.AESTHETIC)
				{
					list2.Add(list[num2]);
				}
				if (list[num2] is Objectives_PerformRitual)
				{
					Objectives_PerformRitual objectives_PerformRitual = (Objectives_PerformRitual)list[num2];
					if (assignTargetFollowers)
					{
						objectives_PerformRitual.TargetFollowerID_1 = targetFollowerID_1;
						objectives_PerformRitual.TargetFollowerID_2 = targetFollowerID_2;
					}
					if (objectives_PerformRitual.Ritual == UpgradeSystem.Type.Ritual_Sacrifice || objectives_PerformRitual.Ritual == UpgradeSystem.Type.Ritual_Wedding || objectives_PerformRitual.Ritual == UpgradeSystem.Type.Ritual_Fightpit || objectives_PerformRitual.Ritual == UpgradeSystem.Type.Ritual_AssignFaithEnforcer || objectives_PerformRitual.Ritual == UpgradeSystem.Type.Ritual_AssignTaxCollector || objectives_PerformRitual.Ritual == UpgradeSystem.Type.Ritual_ConsumeFollower)
					{
						if (objectives_PerformRitual.Ritual == UpgradeSystem.Type.Ritual_Fightpit && assignTargetFollowers)
						{
							objectives_PerformRitual.TargetFollowerID_2 = followerID;
							if (objectives_PerformRitual.TargetFollowerID_1 == objectives_PerformRitual.TargetFollowerID_2)
							{
								objectives_PerformRitual.TargetFollowerID_2 = targetFollowerID_2;
							}
						}
						else if (assignTargetFollowers)
						{
							objectives_PerformRitual.TargetFollowerID_1 = followerID;
							objectives_PerformRitual.TargetFollowerID_2 = -1;
						}
						if (FollowerInfo.GetInfoByID(followerID) == null)
						{
							list[num2] = null;
						}
					}
					if (objectives_PerformRitual.Ritual == UpgradeSystem.Type.Ritual_Ressurect)
					{
						if (deadFollowerID != -1 && FollowerManager.GetDeadFollowerInfoByID(deadFollowerID) != null)
						{
							if (assignTargetFollowers)
							{
								objectives_PerformRitual.TargetFollowerID_1 = deadFollowerID;
							}
							objectives_PerformRitual.FailLocked = true;
						}
						else
						{
							list[num2] = null;
						}
					}
					if (objectives_PerformRitual.Ritual == UpgradeSystem.Type.Ritual_Funeral)
					{
						if (deadFollowerID != -1 && FollowerManager.GetDeadFollowerInfoByID(deadFollowerID) != null)
						{
							List<Structures_Grave> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_Grave>(FollowerLocation.Base);
							bool flag2 = false;
							foreach (Structures_Grave item2 in allStructuresOfType)
							{
								if (item2.Data.FollowerID == deadFollowerID)
								{
									flag2 = true;
									break;
								}
							}
							if (flag2)
							{
								if (assignTargetFollowers)
								{
									objectives_PerformRitual.TargetFollowerID_1 = deadFollowerID;
								}
								objectives_PerformRitual.FailLocked = true;
							}
							else
							{
								list[num2] = null;
							}
						}
						else
						{
							list[num2] = null;
						}
					}
					if (!UpgradeSystem.GetUnlocked(objectives_PerformRitual.Ritual) || UpgradeSystem.GetCoolDownNormalised(objectives_PerformRitual.Ritual) > 0f)
					{
						list[num2] = null;
					}
					if (objectives_PerformRitual.Ritual == UpgradeSystem.Type.Ritual_Sacrifice && DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.AgainstSacrifice))
					{
						list[num2] = null;
					}
					if (objectives_PerformRitual.Ritual == UpgradeSystem.Type.Ritual_Wedding && FollowerInfo.GetInfoByID(followerID) != null && FollowerInfo.GetInfoByID(followerID).MarriedToLeader)
					{
						list[num2] = null;
					}
					if (objectives_PerformRitual.Ritual == UpgradeSystem.Type.Ritual_Funeral && FollowerInfo.GetInfoByID(followerID, true) != null && FollowerInfo.GetInfoByID(followerID, true).HadFuneral)
					{
						list[num2] = null;
					}
					if (objectives_PerformRitual.Ritual == UpgradeSystem.Type.Ritual_Fightpit && (targetFollowerID_2 == -1 || targetFollowerID_2 == targetFollowerID_1 || FollowerInfo.GetInfoByID(targetFollowerID_1) == null || FollowerInfo.GetInfoByID(targetFollowerID_2) == null))
					{
						list[num2] = null;
					}
					if ((objectives_PerformRitual.Ritual == UpgradeSystem.Type.Ritual_AssignFaithEnforcer || objectives_PerformRitual.Ritual == UpgradeSystem.Type.Ritual_AssignTaxCollector) && targetQuest == null)
					{
						FollowerInfo infoByID = FollowerInfo.GetInfoByID(followerID);
						if (infoByID != null && (infoByID.TaxEnforcer || infoByID.FaithEnforcer))
						{
							list[num2] = null;
						}
					}
					if ((objectives_PerformRitual.Ritual == UpgradeSystem.Type.Ritual_Sacrifice || objectives_PerformRitual.Ritual == UpgradeSystem.Type.Ritual_ConsumeFollower) && DataManager.Instance.Followers.Count < 5 && targetQuest == null)
					{
						list[num2] = null;
					}
				}
				else if (list[num2] is Objectives_Custom)
				{
					Objectives_Custom objectives_Custom = (Objectives_Custom)list[num2];
					if (assignTargetFollowers)
					{
						objectives_Custom.TargetFollowerID = targetFollowerID_1;
					}
					if (FollowerInfo.GetInfoByID(targetFollowerID_1) == null)
					{
						list[num2] = null;
					}
					if (assignTargetFollowers)
					{
						if (objectives_Custom.CustomQuestType == Objectives.CustomQuestTypes.MurderFollowerAtNight)
						{
							objectives_Custom.TargetFollowerID = followerID;
						}
						else if (objectives_Custom.CustomQuestType == Objectives.CustomQuestTypes.SendFollowerOnMissionary)
						{
							objectives_Custom.TargetFollowerAllowOldAge = false;
						}
						else if (objectives_Custom.CustomQuestType == Objectives.CustomQuestTypes.UseFirePit)
						{
							objectives_Custom.TargetFollowerID = -1;
						}
					}
					if ((objectives_Custom.CustomQuestType == Objectives.CustomQuestTypes.SendFollowerOnMissionary && StructureManager.GetAllStructuresOfType<Structures_Missionary>().Count <= 0) || (objectives_Custom.CustomQuestType == Objectives.CustomQuestTypes.SendFollowerToPrison && StructureManager.GetAllStructuresOfType<Structures_Prison>().Count <= 0) || (objectives_Custom.CustomQuestType == Objectives.CustomQuestTypes.MurderFollower && !DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.LawOrder_MurderFollower)) || (objectives_Custom.CustomQuestType == Objectives.CustomQuestTypes.MurderFollowerAtNight && !DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.LawOrder_MurderFollower)) || (objectives_Custom.CustomQuestType == Objectives.CustomQuestTypes.UseFirePit && StructureManager.GetAllStructuresOfType<Structures_DancingFirePit>().Count <= 0))
					{
						list[num2] = null;
					}
					if (objectives_Custom.CustomQuestType == Objectives.CustomQuestTypes.KillFollower && targetQuest == null)
					{
						list[num2] = null;
					}
					if ((objectives_Custom.CustomQuestType == Objectives.CustomQuestTypes.MurderFollower || objectives_Custom.CustomQuestType == Objectives.CustomQuestTypes.MurderFollowerAtNight) && DataManager.Instance.Followers.Count < 5 && targetQuest == null)
					{
						list[num2] = null;
					}
				}
				else if (list[num2] is Objectives_FindFollower)
				{
					FollowerInfo infoByID2 = FollowerInfo.GetInfoByID(followerID);
					bool flag3 = false;
					foreach (int uniqueFollowerID in FollowerManager.UniqueFollowerIDs)
					{
						if (followerID == uniqueFollowerID)
						{
							flag3 = true;
							break;
						}
					}
					if (flag3)
					{
						list[num2] = null;
						continue;
					}
					if (infoByID2 == null)
					{
						list[num2] = null;
						continue;
					}
					Objectives_FindFollower objectives_FindFollower = (Objectives_FindFollower)list[num2];
					objectives_FindFollower.TargetFollowerName = FollowerInfo.GenerateName();
					objectives_FindFollower.FollowerSkin = infoByID2.SkinName;
					objectives_FindFollower.FollowerVariant = infoByID2.SkinVariation;
					objectives_FindFollower.FollowerColour = infoByID2.SkinColour;
					if (objectives_FindFollower.TargetLocation != FollowerLocation.Base && (!DataManager.Instance.DungeonCompleted(objectives_FindFollower.TargetLocation) || (DataManager.Instance.DeathCatBeaten && !DataManager.Instance.UnlockedDungeonDoor.Contains(objectives_FindFollower.TargetLocation))))
					{
						list[num2] = null;
					}
					if (objectives_FindFollower.TargetLocation == FollowerLocation.Dungeon1_1 && targetQuest == null)
					{
						list[num2] = null;
					}
				}
				else if (list[num2] is Objectives_BuildStructure)
				{
					Objectives_BuildStructure objectives_BuildStructure = (Objectives_BuildStructure)list[num2];
					if (objectives_BuildStructure.StructureType == StructureBrain.TYPES.DECORATION_BONE_CANDLE)
					{
						if (followerID == FollowerManager.DeathCatID && targetQuest != null)
						{
							continue;
						}
						list[num2] = null;
					}
					if (objectives_BuildStructure.StructureType == StructureBrain.TYPES.OUTHOUSE && (!UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Outhouse) || StructureManager.GetAllStructuresOfType(FollowerLocation.Base, StructureBrain.TYPES.OUTHOUSE_2).Count > 0))
					{
						list[num2] = null;
					}
					else if (StructureManager.GetAllStructuresOfType(FollowerLocation.Base, objectives_BuildStructure.StructureType).Count > 0)
					{
						list[num2] = null;
					}
				}
				else if (list[num2] is Objectives_CookMeal)
				{
					Objectives_CookMeal objectives_CookMeal = (Objectives_CookMeal)list[num2];
					if (!CookingData.CanMakeMeal(objectives_CookMeal.MealType) && CookingData.GetCookedMeal(objectives_CookMeal.MealType) <= 0 && targetQuest == null)
					{
						list[num2] = null;
					}
				}
				else if (list[num2] is Objectives_EatMeal)
				{
					Objectives_EatMeal objectives_EatMeal = (Objectives_EatMeal)list[num2];
					if (objectives_EatMeal.MealType == StructureBrain.TYPES.MEAL_FOLLOWER_MEAT)
					{
						if (DataManager.Instance.Followers.Count < 5 && targetQuest == null)
						{
							list[num2] = null;
						}
						if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.FOLLOWER_MEAT) <= 0 && !UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Ritual_Sacrifice) && !DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.LawOrder_MurderFollower))
						{
							list[num2] = null;
						}
						if (DataManager.Instance.RecipesDiscovered.Contains(InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT) && targetQuest == null)
						{
							list[num2] = null;
						}
					}
					if (objectives_EatMeal.MealType == StructureBrain.TYPES.MEAL_POOP && TimeManager.CurrentDay < 10 && targetQuest == null)
					{
						list[num2] = null;
					}
					else if (objectives_EatMeal.MealType == StructureBrain.TYPES.MEAL_POOP && TimeManager.CurrentDay >= 10 && !DataManager.Instance.RecipesDiscovered.Contains(InventoryItem.ITEM_TYPE.MEAL_POOP) && targetQuest == null)
					{
						list2.Add(objectives_EatMeal);
					}
					if (objectives_EatMeal.MealType == StructureBrain.TYPES.MEAL_GRASS && DataManager.Instance.RecipesDiscovered.Contains(InventoryItem.ITEM_TYPE.MEAL_GRASS) && targetQuest == null)
					{
						list[num2] = null;
					}
					else if (objectives_EatMeal.MealType == StructureBrain.TYPES.MEAL_GRASS && !DataManager.Instance.RecipesDiscovered.Contains(InventoryItem.ITEM_TYPE.MEAL_GRASS) && targetQuest == null)
					{
						list2.Add(objectives_EatMeal);
					}
					objectives_EatMeal.TargetFollower = followerID;
				}
				else if (list[num2] is Objectives_CollectItem)
				{
					Objectives_CollectItem objectives_CollectItem = (Objectives_CollectItem)list[num2];
					if (objectives_CollectItem.TargetLocation != FollowerLocation.Base && (!DataManager.Instance.DungeonCompleted(objectives_CollectItem.TargetLocation) || (DataManager.Instance.DeathCatBeaten && !DataManager.Instance.UnlockedDungeonDoor.Contains(objectives_CollectItem.TargetLocation))))
					{
						list[num2] = null;
					}
					if (objectives_CollectItem.ItemType == InventoryItem.ITEM_TYPE.MUSHROOM_SMALL && !DataManager.Instance.VisitedLocations.Contains(FollowerLocation.Hub1_Sozo))
					{
						list[num2] = null;
					}
				}
				else if (list[num2] is Objectives_RecruitCursedFollower)
				{
					Objectives_RecruitCursedFollower item = (Objectives_RecruitCursedFollower)list[num2];
					foreach (DataManager.QuestHistoryData completedQuestsHistory in DataManager.Instance.CompletedQuestsHistorys)
					{
						if (QuestsAll[completedQuestsHistory.QuestIndex].Type == Objectives.TYPES.RECRUIT_CURSED_FOLLOWER && TimeManager.TotalElapsedGameTime - completedQuestsHistory.QuestTimestamp < completedQuestsHistory.QuestCooldownDuration)
						{
							list[num2] = null;
							break;
						}
					}
					if (list[num2] != null)
					{
						if (TimeManager.CurrentDay >= 10 || (TimeManager.CurrentDay > 5 && DataManager.Instance.Followers.Count < 3))
						{
							list2.Add(item);
						}
						else
						{
							list[num2] = null;
						}
					}
				}
				else if (list[num2] is Objectives_TalkToFollower)
				{
					if (targetQuest == null)
					{
						list[num2] = null;
					}
					else
					{
						((Objectives_TalkToFollower)list[num2]).TargetFollower = targetFollowerID_1;
					}
				}
				else
				{
					Objectives_PlaceStructure objectives_PlaceStructure;
					if ((objectives_PlaceStructure = list[num2] as Objectives_PlaceStructure) == null)
					{
						continue;
					}
					bool flag4 = false;
					foreach (StructureBrain.TYPES allStructure in StructuresData.AllStructures)
					{
						if (StructuresData.GetCategory(allStructure) == StructureBrain.Categories.AESTHETIC && StructuresData.GetUnlocked(allStructure))
						{
							flag4 = true;
							break;
						}
					}
					if (objectives_PlaceStructure.category == StructureBrain.Categories.AESTHETIC && !flag4)
					{
						list[num2] = null;
					}
				}
			}
		}
		if (targetQuest != null)
		{
			if (list2.Contains(targetQuest) || list.Contains(targetQuest))
			{
				return targetQuest;
			}
			return null;
		}
		if (list2.Count > 0 && targetQuest == null)
		{
			return GetQuest(followerID, targetFollowerID_1, targetFollowerID_2, deadFollowerID, true, list2[Random.Range(0, list2.Count)]);
		}
		if (targetQuest == null)
		{
			Objectives_Story currentStoryObjective = GetCurrentStoryObjective(followerID);
			if (currentStoryObjective != null)
			{
				return currentStoryObjective;
			}
			if (Random.value < num && TimeManager.CurrentDay > 3)
			{
				StoryData newStory = GetNewStory(followerID, targetFollowerID_1, targetFollowerID_2, deadFollowerID);
				if (newStory != null)
				{
					DataManager.Instance.StoryObjectives.Add(newStory);
					return GetCurrentStoryObjective(followerID);
				}
			}
		}
		if (targetQuest == null)
		{
			Dictionary<int, ObjectivesData> dictionary = new Dictionary<int, ObjectivesData>();
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j] != null)
				{
					dictionary.Add(j, list[j]);
				}
			}
			if (dictionary.Count == 0)
			{
				return null;
			}
			return GetQuest(followerID, targetFollowerID_1, targetFollowerID_2, deadFollowerID, true, dictionary.ElementAt(Random.Range(0, dictionary.Count)).Value);
		}
		return targetQuest;
	}

	public static ObjectivesData GetRandomBaseQuest(int followerID, int targetFollowerID_1 = -1, int targetFollowerID_2 = -1, int deadFollowerID = -1)
	{
		int num = 0;
		while (num++ < 32)
		{
			ObjectivesData quest = GetQuest(followerID, targetFollowerID_1, targetFollowerID_2, deadFollowerID);
			if (quest != null && !ObjectiveAlreadyActive(quest))
			{
				quest.ResetInitialisation();
				return quest;
			}
		}
		return null;
	}

	private static bool ObjectiveAlreadyActive(ObjectivesData objective)
	{
		foreach (ObjectivesData objective2 in DataManager.Instance.Objectives)
		{
			if (objective2.Type == objective.Type && objective2.GroupId == objective.GroupId && objective2.Text == objective.Text)
			{
				return true;
			}
		}
		foreach (ObjectivesData completedObjective in DataManager.Instance.CompletedObjectives)
		{
			if (completedObjective.Type == objective.Type && completedObjective.GroupId == objective.GroupId && completedObjective.Text == objective.Text)
			{
				return true;
			}
		}
		return false;
	}

	public static StoryData GetNewStory(int questGiverFollowerID, int targetFollowerID_1 = -1, int targetFollowerID_2 = -1, int deadFollowerID = -1)
	{
		List<int> list = new List<int>();
		list.Add(questGiverFollowerID);
		list.Add(targetFollowerID_1);
		list.Add(targetFollowerID_2);
		list.Add(deadFollowerID);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num] == -1)
			{
				list.RemoveAt(num);
			}
		}
		foreach (StoryData storyObjective in DataManager.Instance.StoryObjectives)
		{
			if (list.Contains(storyObjective.EntryStoryItem.QuestGiverFollowerID) || list.Contains(storyObjective.EntryStoryItem.TargetFollowerID_1) || list.Contains(storyObjective.EntryStoryItem.TargetFollowerID_2) || list.Contains(storyObjective.EntryStoryItem.DeadFollowerID))
			{
				return null;
			}
		}
		List<StoryObjectiveData> list2 = new List<StoryObjectiveData>();
		StoryObjectiveData[] array = AllStoryObjectiveDatas;
		foreach (StoryObjectiveData storyObjectiveData in array)
		{
			if (!storyObjectiveData.IsEntryStory)
			{
				continue;
			}
			bool flag = false;
			foreach (StoryData storyObjective2 in DataManager.Instance.StoryObjectives)
			{
				if (storyObjectiveData.UniqueStoryID == storyObjective2.EntryStoryItem.StoryObjectiveData.UniqueStoryID)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			bool flag2 = false;
			for (int j = 0; j < DataManager.Instance.CompletedQuestsHistorys.Count; j++)
			{
				if (DataManager.Instance.CompletedQuestsHistorys[j].QuestIndex == storyObjectiveData.UniqueStoryID && DataManager.Instance.CompletedQuestsHistorys[j].IsStory && TimeManager.TotalElapsedGameTime - DataManager.Instance.CompletedQuestsHistorys[j].QuestTimestamp < DataManager.Instance.CompletedQuestsHistorys[j].QuestCooldownDuration)
				{
					flag2 = true;
				}
			}
			bool flag3 = true;
			if (storyObjectiveData.ConditionalVariables != null)
			{
				foreach (BiomeGenerator.VariableAndCondition conditionalVariable in storyObjectiveData.ConditionalVariables)
				{
					if (DataManager.Instance.GetVariable(conditionalVariable.Variable) != conditionalVariable.Condition)
					{
						flag3 = false;
					}
				}
			}
			if (!flag3)
			{
				continue;
			}
			bool flag4 = false;
			if (GetQuest(questGiverFollowerID, targetFollowerID_1, targetFollowerID_2, deadFollowerID, false, QuestsAll[storyObjectiveData.QuestIndex]) != null)
			{
				flag4 = true;
			}
			if (!flag2 && flag4)
			{
				if (storyObjectiveData.QuestGiverRequiresID == questGiverFollowerID)
				{
					list2.Clear();
					list2.Add(storyObjectiveData);
					break;
				}
				list2.Add(storyObjectiveData);
			}
		}
		if (list2.Count > 0)
		{
			StoryDataItem storyDataItem = new StoryDataItem();
			storyDataItem.StoryObjectiveData = list2[Random.Range(0, list2.Count)];
			storyDataItem.QuestGiverFollowerID = questGiverFollowerID;
			storyDataItem.FollowerID = questGiverFollowerID;
			storyDataItem.TargetFollowerID_1 = targetFollowerID_1;
			storyDataItem.TargetFollowerID_2 = targetFollowerID_2;
			storyDataItem.DeadFollowerID = deadFollowerID;
			StoryData obj = new StoryData
			{
				EntryStoryItem = storyDataItem
			};
			CreateStoryDataItemForStoryObjectiveData(obj.EntryStoryItem);
			return obj;
		}
		return null;
	}

	public static Objectives_Story GetCurrentStoryObjective(int followerID)
	{
		StoryDataItem storyDataItem = null;
		List<StoryData> list = new List<StoryData>(DataManager.Instance.StoryObjectives);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			StoryData storyData = list[num];
			if (FollowerInfo.GetInfoByID(storyData.EntryStoryItem.QuestGiverFollowerID) != null)
			{
				List<StoryDataItem> childStoryDataItemsFromStoryDataItem = GetChildStoryDataItemsFromStoryDataItem(storyData.EntryStoryItem);
				bool flag = false;
				foreach (StoryDataItem item in childStoryDataItemsFromStoryDataItem)
				{
					if ((item.StoryObjectiveData.RequireTarget_1 && FollowerInfo.GetInfoByID(item.TargetFollowerID_1) == null) || (item.StoryObjectiveData.RequireTarget_2 && FollowerInfo.GetInfoByID(item.TargetFollowerID_2) == null))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					storyDataItem = GetCurrentStoryDataItem(storyData.EntryStoryItem.QuestGiverFollowerID, list);
					if (storyDataItem != null)
					{
						if (GetQuest(storyDataItem.FollowerID, storyDataItem.TargetFollowerID_1, storyDataItem.TargetFollowerID_2, storyDataItem.DeadFollowerID, false, storyDataItem.Objective) == null)
						{
							list.RemoveAt(num);
						}
					}
					else
					{
						list.RemoveAt(num);
					}
					continue;
				}
			}
			DataManager.Instance.StoryObjectives.RemoveAt(num);
			list.RemoveAt(num);
			AddObjectiveToHistory(storyData.EntryStoryItem.StoryObjectiveData.UniqueStoryID, 120000f, true);
		}
		storyDataItem = GetCurrentStoryDataItem(followerID, list);
		StoryDataItem storyObjectiveParent = GetStoryObjectiveParent(GetFollowerStoryData(followerID), storyDataItem);
		if (storyDataItem != null)
		{
			return new Objectives_Story(storyDataItem, storyObjectiveParent);
		}
		return null;
	}

	private static StoryDataItem GetStoryObjectiveParent(StoryData rootStoryData, StoryDataItem childDataItem)
	{
		if (rootStoryData == null || childDataItem == null)
		{
			return null;
		}
		return GetStoryObjectiveParent(rootStoryData.EntryStoryItem, childDataItem);
	}

	private static StoryDataItem GetStoryObjectiveParent(StoryDataItem parentDataItem, StoryDataItem childDataItem)
	{
		foreach (StoryDataItem childStoryDataItem in parentDataItem.ChildStoryDataItems)
		{
			if (childStoryDataItem == childDataItem)
			{
				return parentDataItem;
			}
			if (GetStoryObjectiveParent(childStoryDataItem, childDataItem) != null)
			{
				return childStoryDataItem;
			}
		}
		return null;
	}

	public static List<StoryDataItem> GetChildStoryDataItemsFromStoryDataItem(StoryDataItem storyDataItem)
	{
		List<StoryDataItem> list = new List<StoryDataItem>();
		list.AddRange(storyDataItem.ChildStoryDataItems);
		foreach (StoryDataItem childStoryDataItem in storyDataItem.ChildStoryDataItems)
		{
			list.AddRange(GetChildStoryDataItemsFromStoryDataItem(childStoryDataItem));
		}
		return list;
	}

	private static void CreateStoryDataItemForStoryObjectiveData(StoryDataItem parentItem)
	{
		foreach (StoryObjectiveData chilldStoryItem in parentItem.StoryObjectiveData.ChilldStoryItems)
		{
			StoryDataItem storyDataItem = new StoryDataItem();
			storyDataItem.StoryObjectiveData = chilldStoryItem;
			storyDataItem.QuestGiverFollowerID = parentItem.QuestGiverFollowerID;
			storyDataItem.FollowerID = parentItem.QuestGiverFollowerID;
			storyDataItem.TargetFollowerID_1 = parentItem.TargetFollowerID_1;
			storyDataItem.TargetFollowerID_2 = parentItem.TargetFollowerID_2;
			storyDataItem.DeadFollowerID = parentItem.DeadFollowerID;
			parentItem.ChildStoryDataItems.Add(storyDataItem);
			CreateStoryDataItemForStoryObjectiveData(storyDataItem);
		}
	}

	public static StoryDataItem GetCurrentStoryDataItem(int followerID, List<StoryData> stories)
	{
		foreach (StoryData story in stories)
		{
			if (story.EntryStoryItem.QuestGiverFollowerID != followerID)
			{
				continue;
			}
			StoryDataItem currentStoryDataItem = GetCurrentStoryDataItem(story.EntryStoryItem);
			if (currentStoryDataItem != null)
			{
				if (currentStoryDataItem.QuestDeclined)
				{
					return null;
				}
				if (FollowerInfo.GetInfoByID(currentStoryDataItem.FollowerID) == null || FollowerInfo.GetInfoByID(currentStoryDataItem.TargetFollowerID_1) == null || FollowerInfo.GetInfoByID(currentStoryDataItem.TargetFollowerID_2) == null)
				{
					return null;
				}
				return currentStoryDataItem;
			}
		}
		return null;
	}

	private static StoryDataItem GetCurrentStoryDataItem(StoryDataItem storyDataItem)
	{
		int targetFollowerID_ = -1;
		int deadFollowerID = -1;
		int followerID = storyDataItem.QuestGiverFollowerID;
		int targetFollowerID_2 = storyDataItem.QuestGiverFollowerID;
		if (storyDataItem.TargetFollowerID_1 != -1 && storyDataItem.StoryObjectiveData.RequireTarget_1)
		{
			targetFollowerID_2 = storyDataItem.TargetFollowerID_1;
		}
		if (storyDataItem.StoryObjectiveData.RequireTarget_1 && storyDataItem.StoryObjectiveData.RequireTarget_2 && storyDataItem.StoryObjectiveData.TargetQuestGiver)
		{
			targetFollowerID_2 = storyDataItem.QuestGiverFollowerID;
			targetFollowerID_ = storyDataItem.TargetFollowerID_1;
		}
		if (storyDataItem.TargetFollowerID_1 != -1 && storyDataItem.StoryObjectiveData.RequireTarget_1 && !storyDataItem.StoryObjectiveData.TargetQuestGiver)
		{
			followerID = storyDataItem.TargetFollowerID_1;
		}
		if (storyDataItem.TargetFollowerID_2 != -1 && storyDataItem.StoryObjectiveData.RequireTarget_2 && !storyDataItem.StoryObjectiveData.TargetQuestGiver)
		{
			targetFollowerID_ = storyDataItem.TargetFollowerID_2;
		}
		if (storyDataItem.DeadFollowerID != -1 && storyDataItem.StoryObjectiveData.RequireTarget_Deadbody)
		{
			deadFollowerID = storyDataItem.DeadFollowerID;
		}
		ObjectivesData quest = GetQuest(followerID, targetFollowerID_2, targetFollowerID_, deadFollowerID, false, QuestsAll[storyDataItem.StoryObjectiveData.QuestIndex]);
		if (!storyDataItem.QuestGiven)
		{
			if (quest != null)
			{
				quest = GetQuest(followerID, targetFollowerID_2, targetFollowerID_, deadFollowerID, true, QuestsAll[storyDataItem.StoryObjectiveData.QuestIndex]);
				storyDataItem.Objective = quest;
				storyDataItem.Objective.CompleteTerm = storyDataItem.StoryObjectiveData.CompleteQuestTerm;
				return storyDataItem;
			}
			return null;
		}
		foreach (StoryDataItem childStoryDataItem in storyDataItem.ChildStoryDataItems)
		{
			StoryDataItem currentStoryDataItem = GetCurrentStoryDataItem(childStoryDataItem);
			if (currentStoryDataItem != null)
			{
				return currentStoryDataItem;
			}
		}
		return null;
	}

	public static StoryData GetFollowerStoryData(int followerID)
	{
		foreach (StoryData storyObjective in DataManager.Instance.StoryObjectives)
		{
			if (storyObjective.EntryStoryItem.QuestGiverFollowerID == followerID)
			{
				return storyObjective;
			}
		}
		return null;
	}

	public static bool IsFollowerLeaderInStory(int followerID)
	{
		foreach (StoryData storyObjective in DataManager.Instance.StoryObjectives)
		{
			if (storyObjective.EntryStoryItem.QuestGiverFollowerID == followerID)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsFollowerInStory(int followerID)
	{
		foreach (StoryData storyObjective in DataManager.Instance.StoryObjectives)
		{
			if (IsFollowerInStoryChild(followerID, storyObjective.EntryStoryItem))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsFollowerInStoryChild(int followerID, StoryDataItem storyItem)
	{
		if (followerID == storyItem.QuestGiverFollowerID || followerID == storyItem.TargetFollowerID_1 || followerID == storyItem.TargetFollowerID_2 || followerID == storyItem.DeadFollowerID)
		{
			return true;
		}
		foreach (StoryDataItem childStoryDataItem in storyItem.ChildStoryDataItems)
		{
			if (IsFollowerInStoryChild(followerID, childStoryDataItem))
			{
				return true;
			}
		}
		return false;
	}

	public static ObjectivesData GetRandomDungeonChallenge()
	{
		int num = 0;
		while (num++ < 32)
		{
			ObjectivesData randomRoomChallenge = GetRandomRoomChallenge();
			if (IsDungeonChallengeAvailable(randomRoomChallenge))
			{
				DataManager.Instance.DungeonObjectives.Add(randomRoomChallenge);
				randomRoomChallenge.Init(true);
				return randomRoomChallenge;
			}
		}
		return null;
	}

	private static ObjectivesData GetRandomRoomChallenge()
	{
		return DungeonRoomChallenges[Random.Range(0, DungeonRoomChallenges.Count)];
	}

	private static bool IsDungeonChallengeAvailable(ObjectivesData objective)
	{
		if (objective == null || ObjectiveAlreadyActive(objective))
		{
			return false;
		}
		if (objective.Type == Objectives.TYPES.NO_CURSES && PlayerFleeceManager.FleeceSwapsWeaponForCurse())
		{
			return false;
		}
		if (objective.Type == Objectives.TYPES.NO_DODGE && PlayerFleeceManager.FleecePreventsRoll())
		{
			return false;
		}
		return true;
	}

	public static List<ObjectivesData> GetCurrentFollowerQuests(int followerID)
	{
		List<ObjectivesData> list = new List<ObjectivesData>();
		foreach (ObjectivesData objective in DataManager.Instance.Objectives)
		{
			if (objective.Follower == followerID)
			{
				list.Add(objective);
			}
		}
		foreach (ObjectivesData failedObjective in DataManager.Instance.FailedObjectives)
		{
			if (failedObjective.Follower == followerID)
			{
				list.Add(failedObjective);
			}
		}
		foreach (ObjectivesData completedObjective in DataManager.Instance.CompletedObjectives)
		{
			if (completedObjective.Follower == followerID)
			{
				list.Add(completedObjective);
			}
		}
		return list;
	}

	public static List<ObjectivesData> GetUnCompletedFollowerQuests(int followerID, string groupID)
	{
		List<ObjectivesData> list = new List<ObjectivesData>();
		foreach (ObjectivesData objective in DataManager.Instance.Objectives)
		{
			if (objective.Follower == followerID && (groupID == "" || objective.GroupId == groupID))
			{
				list.Add(objective);
			}
		}
		return list;
	}

	public static void AddObjectiveToHistory(int questIndex, float questCooldownDuration, bool isStory = false)
	{
		bool flag = false;
		for (int i = 0; i < DataManager.Instance.CompletedQuestsHistorys.Count; i++)
		{
			if (DataManager.Instance.CompletedQuestsHistorys[i].QuestIndex == questIndex && DataManager.Instance.CompletedQuestsHistorys[i].IsStory == isStory)
			{
				DataManager.Instance.CompletedQuestsHistorys[i].QuestTimestamp = TimeManager.TotalElapsedGameTime;
				DataManager.Instance.CompletedQuestsHistorys[i].QuestCooldownDuration = questCooldownDuration;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			DataManager.Instance.CompletedQuestsHistorys.Add(new DataManager.QuestHistoryData
			{
				QuestIndex = questIndex,
				QuestTimestamp = TimeManager.TotalElapsedGameTime,
				IsStory = isStory,
				QuestCooldownDuration = questCooldownDuration
			});
		}
	}
}
