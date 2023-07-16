using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using MMBiomeGeneration;
using UnityEngine;

public class ObjectiveManager : BaseMonoBehaviour
{
	public delegate void ObjectiveUpdated(ObjectivesData objective);

	private const int kObjectiveHistoryLimit = 10;

	public static Action<string> OnObjectiveGroupCompleted;

	public static Action<string> OnObjectiveTracked;

	public static Action<string> OnObjectiveUntracked;

	private static List<Action> _eventQueue = new List<Action>();

	private static List<string> _trackingQueue = new List<string>();

	private static List<string> TrackedUniqueGroupIDs
	{
		get
		{
			return DataManager.Instance.TrackedObjectiveGroupIDs;
		}
		set
		{
			DataManager.Instance.TrackedObjectiveGroupIDs = value;
		}
	}

	public static event ObjectiveUpdated OnObjectiveAdded;

	public static event ObjectiveUpdated OnObjectiveUpdated;

	public static event ObjectiveUpdated OnObjectiveCompleted;

	public static event ObjectiveUpdated OnObjectiveRemoved;

	public static event ObjectiveUpdated OnObjectiveFailed;

	private static void InvokeOrQueue(Action action)
	{
		if (RequiresQueueing())
		{
			_eventQueue.Add(action);
		}
		else
		{
			action();
		}
	}

	private static bool RequiresQueueing()
	{
		if (!(HUD_Manager.Instance == null) && !HUD_Manager.Instance.Hidden)
		{
			return UIMenuBase.ActiveMenus.Count > 0;
		}
		return true;
	}

	private static void DispatchQueues()
	{
		if (_eventQueue.Count > 0)
		{
			Debug.Log("ObjectiveManager - Dispatch Event Queue".Colour(Color.yellow));
			foreach (Action item in _eventQueue)
			{
				if (item != null)
				{
					item();
				}
			}
			_eventQueue.Clear();
		}
		if (_trackingQueue.Count <= 0)
		{
			return;
		}
		Debug.Log("ObjectiveManager - Dispatch Tracking Queue".Colour(Color.yellow));
		foreach (string item2 in _trackingQueue)
		{
			if (IsTracked(item2))
			{
				Action<string> onObjectiveTracked = OnObjectiveTracked;
				if (onObjectiveTracked != null)
				{
					onObjectiveTracked(item2);
				}
			}
			else
			{
				Action<string> onObjectiveUntracked = OnObjectiveUntracked;
				if (onObjectiveUntracked != null)
				{
					onObjectiveUntracked(item2);
				}
			}
		}
		_trackingQueue.Clear();
	}

	public static void TrackGroup(string uniqueGroupID)
	{
		if (IsTracked(uniqueGroupID))
		{
			return;
		}
		TrackedUniqueGroupIDs.Add(uniqueGroupID);
		if (RequiresQueueing())
		{
			if (!_trackingQueue.Contains(uniqueGroupID))
			{
				_trackingQueue.Add(uniqueGroupID);
			}
			return;
		}
		Action<string> onObjectiveTracked = OnObjectiveTracked;
		if (onObjectiveTracked != null)
		{
			onObjectiveTracked(uniqueGroupID);
		}
	}

	public static void UntrackGroup(string uniqueGroupID, bool ignoreQueue = false)
	{
		if (!IsTracked(uniqueGroupID))
		{
			return;
		}
		TrackedUniqueGroupIDs.Remove(uniqueGroupID);
		if (!ignoreQueue && RequiresQueueing())
		{
			if (!_trackingQueue.Contains(uniqueGroupID))
			{
				_trackingQueue.Add(uniqueGroupID);
			}
			return;
		}
		Action<string> onObjectiveUntracked = OnObjectiveUntracked;
		if (onObjectiveUntracked != null)
		{
			onObjectiveUntracked(uniqueGroupID);
		}
	}

	public static bool IsTracked(string uniqueGroupID)
	{
		return TrackedUniqueGroupIDs.Contains(uniqueGroupID);
	}

	public static bool AnyTracked()
	{
		return TrackedUniqueGroupIDs.Count > 0;
	}

	public static List<ObjectivesData> GetAllObjectivesOfGroup(string uniqueGroupID)
	{
		List<ObjectivesData> list = new List<ObjectivesData>();
		foreach (ObjectivesData objective in DataManager.Instance.Objectives)
		{
			if (objective.UniqueGroupID == uniqueGroupID)
			{
				list.Add(objective);
			}
		}
		foreach (ObjectivesData completedObjective in DataManager.Instance.CompletedObjectives)
		{
			if (completedObjective.UniqueGroupID == uniqueGroupID)
			{
				list.Add(completedObjective);
			}
		}
		foreach (ObjectivesData failedObjective in DataManager.Instance.FailedObjectives)
		{
			if (failedObjective.UniqueGroupID == uniqueGroupID)
			{
				list.Add(failedObjective);
			}
		}
		return list;
	}

	public static bool AnyQuestsExist()
	{
		return DataManager.Instance.Objectives.Count + DataManager.Instance.CompletedObjectives.Count + DataManager.Instance.FailedObjectives.Count + DataManager.Instance.CompletedObjectivesHistory.Count + DataManager.Instance.FailedObjectivesHistory.Count > 0;
	}

	public static bool AllObjectivesComplete(string groupID)
	{
		return AllObjectivesComplete(GetAllObjectivesOfGroup(groupID));
	}

	public static bool AllObjectivesComplete(List<ObjectivesData> objectivesData)
	{
		int num = 0;
		foreach (ObjectivesData objectivesDatum in objectivesData)
		{
			if (objectivesDatum.IsComplete || objectivesDatum.IsFailed)
			{
				num++;
			}
		}
		return num == objectivesData.Count;
	}

	public static List<string> AllObjectiveGroupIDs()
	{
		List<string> list = new List<string>();
		foreach (ObjectivesData objective in DataManager.Instance.Objectives)
		{
			if (!list.Contains(objective.UniqueGroupID))
			{
				list.Add(objective.UniqueGroupID);
			}
		}
		foreach (ObjectivesData completedObjective in DataManager.Instance.CompletedObjectives)
		{
			if (!list.Contains(completedObjective.UniqueGroupID))
			{
				list.Add(completedObjective.UniqueGroupID);
			}
		}
		foreach (ObjectivesData failedObjective in DataManager.Instance.FailedObjectives)
		{
			if (!list.Contains(failedObjective.UniqueGroupID))
			{
				list.Add(failedObjective.UniqueGroupID);
			}
		}
		return list;
	}

	public static void MaintainObjectiveHistoryList(ref List<ObjectivesDataFinalized> objectivesList)
	{
		List<string> list = new List<string>();
		foreach (ObjectivesDataFinalized objectives in objectivesList)
		{
			if (!list.Contains(objectives.UniqueGroupID))
			{
				list.Add(objectives.UniqueGroupID);
			}
		}
		string text = list.LastElement();
		while (list.Count > 10)
		{
			objectivesList.RemoveAt(objectivesList.Count - 1);
			if (objectivesList.LastElement().UniqueGroupID != text)
			{
				list.Remove(text);
				text = list.LastElement();
			}
		}
	}

	public static bool GroupExists(string groupID)
	{
		foreach (ObjectivesData objective in DataManager.Instance.Objectives)
		{
			if (objective.GroupId == groupID)
			{
				return true;
			}
		}
		return false;
	}

	public static void Add(ObjectivesData objective, bool autoTrack = false)
	{
		foreach (ObjectivesData objective2 in DataManager.Instance.Objectives)
		{
			if (objective2.ID == objective.ID)
			{
				return;
			}
		}
		List<ObjectivesData> list = new List<ObjectivesData>(DataManager.Instance.Objectives);
		list.AddRange(DataManager.Instance.CompletedObjectives);
		list.AddRange(DataManager.Instance.FailedObjectives);
		string text = "";
		foreach (ObjectivesData item in list)
		{
			if (item.GroupId == objective.GroupId)
			{
				text = item.UniqueGroupID;
				break;
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			text = GetUniqueID();
		}
		objective.UniqueGroupID = text;
		DataManager.Instance.Objectives.Add(objective);
		if (!objective.IsInitialised())
		{
			objective.Init(true);
		}
		InvokeOrQueue(delegate
		{
			ObjectiveUpdated onObjectiveAdded = ObjectiveManager.OnObjectiveAdded;
			if (onObjectiveAdded != null)
			{
				onObjectiveAdded(objective);
			}
		});
		if (!AnyTracked() || objective.AutoTrack || objective.ExpireTimestamp != -1f)
		{
			TrackGroup(objective.UniqueGroupID);
		}
		if (objective.Type != Objectives.TYPES.CUSTOM && objective.Type != Objectives.TYPES.SEND_FOLLOWER_BED_REST)
		{
			UpdateObjective(objective);
		}
		if (autoTrack)
		{
			TrackGroup(objective.UniqueGroupID);
		}
	}

	public static void CompleteCustomObjective(Objectives.CustomQuestTypes customQuestType, int targetFollowerID = -1)
	{
		for (int i = 0; i < DataManager.Instance.Objectives.Count; i++)
		{
			ObjectivesData objectivesData = DataManager.Instance.Objectives[i];
			if (objectivesData.Type == Objectives.TYPES.CUSTOM && ((Objectives_Custom)objectivesData).CustomQuestType == customQuestType)
			{
				((Objectives_Custom)objectivesData).ResultFollowerID = targetFollowerID;
				if (UpdateObjective(objectivesData))
				{
					i--;
				}
			}
		}
	}

	public static void CompleteDefeatKnucklebones(string CharacterNameTerm)
	{
		for (int i = 0; i < DataManager.Instance.Objectives.Count; i++)
		{
			ObjectivesData objectivesData = DataManager.Instance.Objectives[i];
			if (objectivesData.Type == Objectives.TYPES.DEFEAT_KNUCKLEBONES)
			{
				Debug.Log(((Objectives_DefeatKnucklebones)objectivesData).CharacterNameTerm + "      " + CharacterNameTerm);
			}
			if (objectivesData.Type == Objectives.TYPES.DEFEAT_KNUCKLEBONES && ((Objectives_DefeatKnucklebones)objectivesData).CharacterNameTerm == CharacterNameTerm)
			{
				Debug.Log("REMOVE!".Colour(Color.yellow));
				if (UpdateObjective(objectivesData))
				{
					i--;
				}
			}
		}
	}

	public static void FailDefeatKnucklebones(string CharacterNameTerm)
	{
		for (int i = 0; i < DataManager.Instance.Objectives.Count; i++)
		{
			ObjectivesData objectivesData = DataManager.Instance.Objectives[i];
			if (objectivesData.Type == Objectives.TYPES.DEFEAT_KNUCKLEBONES)
			{
				Debug.Log(((Objectives_DefeatKnucklebones)objectivesData).CharacterNameTerm + "      " + CharacterNameTerm);
			}
			if (objectivesData.Type == Objectives.TYPES.DEFEAT_KNUCKLEBONES && ((Objectives_DefeatKnucklebones)objectivesData).CharacterNameTerm == CharacterNameTerm)
			{
				objectivesData.FailLocked = false;
				objectivesData.Failed();
				break;
			}
		}
	}

	public static void FailCustomObjective(Objectives.CustomQuestTypes customQuestType, int targetFollowerID = -1)
	{
		for (int i = 0; i < DataManager.Instance.Objectives.Count; i++)
		{
			ObjectivesData objectivesData = DataManager.Instance.Objectives[i];
			if (objectivesData.Type == Objectives.TYPES.CUSTOM && ((Objectives_Custom)objectivesData).CustomQuestType == customQuestType && ((Objectives_Custom)objectivesData).TargetFollowerID == targetFollowerID)
			{
				objectivesData.FailLocked = false;
				objectivesData.Failed();
			}
		}
	}

	public static void FailLockCustomObjective(Objectives.CustomQuestTypes customQuestType, bool locked)
	{
		Debug.Log("Unlock custom objective: " + customQuestType);
		for (int i = 0; i < DataManager.Instance.Objectives.Count; i++)
		{
			ObjectivesData objectivesData = DataManager.Instance.Objectives[i];
			if (objectivesData.Type == Objectives.TYPES.CUSTOM && ((Objectives_Custom)objectivesData).CustomQuestType == customQuestType)
			{
				objectivesData.FailLocked = locked;
			}
		}
	}

	public static void SetRitualObjectivesFailLocked()
	{
		for (int i = 0; i < DataManager.Instance.Objectives.Count; i++)
		{
			if (DataManager.Instance.Objectives[i].Type == Objectives.TYPES.PERFORM_RITUAL)
			{
				DataManager.Instance.Objectives[i].FailLocked = true;
			}
		}
	}

	public static void CompleteRitualObjective(UpgradeSystem.Type ritualType, int targetFollowerID_1 = -1, int targetFollowerID_2 = -1)
	{
		for (int i = 0; i < DataManager.Instance.Objectives.Count; i++)
		{
			ObjectivesData objectivesData = DataManager.Instance.Objectives[i];
			objectivesData.FailLocked = false;
			if (objectivesData.Type == Objectives.TYPES.PERFORM_RITUAL)
			{
				if (((Objectives_PerformRitual)objectivesData).Ritual == ritualType)
				{
					((Objectives_PerformRitual)objectivesData).CheckComplete(targetFollowerID_1, targetFollowerID_2);
				}
				if (((Objectives_PerformRitual)objectivesData).Ritual == UpgradeSystem.Type.Ritual_Ressurect || ((Objectives_PerformRitual)objectivesData).Ritual == UpgradeSystem.Type.Ritual_Funeral)
				{
					objectivesData.FailLocked = true;
				}
				if (UpdateObjective(objectivesData))
				{
					i--;
				}
			}
		}
	}

	public static void CompleteEatMealObjective(StructureBrain.TYPES mealType, int targetFollowerID_1 = -1)
	{
		for (int i = 0; i < DataManager.Instance.Objectives.Count; i++)
		{
			ObjectivesData objectivesData = DataManager.Instance.Objectives[i];
			if (objectivesData != null && objectivesData.Type == Objectives.TYPES.EAT_MEAL)
			{
				((Objectives_EatMeal)objectivesData).CheckComplete(mealType, targetFollowerID_1);
				if (UpdateObjective(objectivesData))
				{
					i--;
				}
			}
		}
	}

	private void OnEnable()
	{
		Debug.Log("ObjectiveManager - OnDisable".Colour(Color.cyan));
		InvokeOrQueue(delegate
		{
			foreach (string item in AllObjectiveGroupIDs())
			{
				List<ObjectivesData> allObjectivesOfGroup = GetAllObjectivesOfGroup(item);
				if (AllObjectivesComplete(allObjectivesOfGroup))
				{
					foreach (ObjectivesData item2 in allObjectivesOfGroup)
					{
						if (item2.IsComplete)
						{
							ObjectiveUpdated onObjectiveCompleted = ObjectiveManager.OnObjectiveCompleted;
							if (onObjectiveCompleted != null)
							{
								onObjectiveCompleted(item2);
							}
						}
						else if (item2.IsFailed)
						{
							ObjectiveUpdated onObjectiveFailed = ObjectiveManager.OnObjectiveFailed;
							if (onObjectiveFailed != null)
							{
								onObjectiveFailed(item2);
							}
						}
					}
				}
			}
		});
		Inventory.OnInventoryUpdated = (Inventory.InventoryUpdated)Delegate.Combine(Inventory.OnInventoryUpdated, new Inventory.InventoryUpdated(OnInventoryUpdated));
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
		FollowerManager.OnFollowerAdded = (FollowerManager.FollowerChanged)Delegate.Combine(FollowerManager.OnFollowerAdded, new FollowerManager.FollowerChanged(OnFollowerChanged));
		FollowerManager.OnFollowerRemoved = (FollowerManager.FollowerChanged)Delegate.Combine(FollowerManager.OnFollowerRemoved, new FollowerManager.FollowerChanged(OnFollowerChanged));
		Structure.OnItemDeposited = (Structure.StructureInventoryChanged)Delegate.Combine(Structure.OnItemDeposited, new Structure.StructureInventoryChanged(OnStructureItemDeposited));
		UnitObject.OnEnemyKilled += OnEnemyKilled;
		RoomLockController.OnRoomCleared += RoomLockController_OnRoomCleared;
		RatauGiveSpells.OnDummyShot = (Action)Delegate.Combine(RatauGiveSpells.OnDummyShot, new Action(OnDummyShot));
		BiomeGenerator.OnBiomeChangeRoom += BiomeGenerator_OnBiomeChangeRoom;
		StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(StructureBrainRemoved));
		FollowerBrain.OnBrainRemoved = (Action<int>)Delegate.Combine(FollowerBrain.OnBrainRemoved, new Action<int>(OnBrainRemoved));
		HUD_Manager.OnShown = (Action)Delegate.Combine(HUD_Manager.OnShown, new Action(DispatchQueues));
		UIMenuBase.OnFinalMenuHidden = (Action)Delegate.Combine(UIMenuBase.OnFinalMenuHidden, new Action(DispatchQueues));
		UIObjectiveGroup.OnObjectiveGroupBeginHide = (Action<string>)Delegate.Combine(UIObjectiveGroup.OnObjectiveGroupBeginHide, new Action<string>(OnObjectiveGroupBeginHide));
		OnObjectiveCompleted += OnObjectiveCompletedEvent;
		OnObjectiveAdded += ObjectiveAdded;
	}

	private void OnDisable()
	{
		Inventory.OnInventoryUpdated = (Inventory.InventoryUpdated)Delegate.Remove(Inventory.OnInventoryUpdated, new Inventory.InventoryUpdated(OnInventoryUpdated));
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
		FollowerManager.OnFollowerAdded = (FollowerManager.FollowerChanged)Delegate.Remove(FollowerManager.OnFollowerAdded, new FollowerManager.FollowerChanged(OnFollowerChanged));
		FollowerManager.OnFollowerRemoved = (FollowerManager.FollowerChanged)Delegate.Remove(FollowerManager.OnFollowerRemoved, new FollowerManager.FollowerChanged(OnFollowerChanged));
		Structure.OnItemDeposited = (Structure.StructureInventoryChanged)Delegate.Remove(Structure.OnItemDeposited, new Structure.StructureInventoryChanged(OnStructureItemDeposited));
		UnitObject.OnEnemyKilled -= OnEnemyKilled;
		RatauGiveSpells.OnDummyShot = (Action)Delegate.Remove(RatauGiveSpells.OnDummyShot, new Action(OnDummyShot));
		RoomLockController.OnRoomCleared -= RoomLockController_OnRoomCleared;
		BiomeGenerator.OnBiomeChangeRoom -= BiomeGenerator_OnBiomeChangeRoom;
		StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(StructureBrainRemoved));
		FollowerBrain.OnBrainRemoved = (Action<int>)Delegate.Remove(FollowerBrain.OnBrainRemoved, new Action<int>(OnBrainRemoved));
		HUD_Manager.OnShown = (Action)Delegate.Remove(HUD_Manager.OnShown, new Action(DispatchQueues));
		UIMenuBase.OnFinalMenuHidden = (Action)Delegate.Remove(UIMenuBase.OnFinalMenuHidden, new Action(DispatchQueues));
		UIObjectiveGroup.OnObjectiveGroupBeginHide = (Action<string>)Delegate.Remove(UIObjectiveGroup.OnObjectiveGroupBeginHide, new Action<string>(OnObjectiveGroupBeginHide));
		OnObjectiveCompleted -= OnObjectiveCompletedEvent;
		OnObjectiveAdded -= ObjectiveAdded;
	}

	private void Update()
	{
		if (DataManager.Instance != null)
		{
			for (int num = DataManager.Instance.Objectives.Count - 1; num >= 0; num--)
			{
				DataManager.Instance.Objectives[num].Update();
			}
		}
	}

	private void OnInventoryUpdated()
	{
		CheckObjectives(Objectives.TYPES.COLLECT_ITEM);
	}

	private void OnBrainRemoved(int followerID)
	{
		CheckObjectives();
	}

	private void OnStructureAdded(StructuresData structure)
	{
		CheckObjectives(Objectives.TYPES.BUILD_STRUCTURE);
	}

	private void OnFollowerChanged(int followerID)
	{
		CheckObjectives(Objectives.TYPES.RECRUIT_FOLLOWER);
	}

	private void RoomLockController_OnRoomCleared()
	{
		StartCoroutine(DelayedCheckCombatObjectives());
	}

	private void StructureBrainRemoved(StructuresData structure)
	{
		StartCoroutine(DelayedStructureRemoved());
	}

	private void OnObjectiveCompletedEvent(ObjectivesData objective)
	{
		Objectives_Custom objectives_Custom;
		if (objective.Type == Objectives.TYPES.CUSTOM && ((Objectives_Custom)objective).CustomQuestType == Objectives.CustomQuestTypes.CookFirstMeal)
		{
			for (int num = DataManager.Instance.Objectives.Count - 1; num >= 0; num--)
			{
				ObjectivesData objectivesData = DataManager.Instance.Objectives[num];
				if (objectivesData.Type == Objectives.TYPES.COLLECT_ITEM && ((Objectives_CollectItem)objectivesData).ItemType == InventoryItem.ITEM_TYPE.BERRY)
				{
					((Objectives_CollectItem)objectivesData).Count = ((Objectives_CollectItem)objectivesData).Target;
					UpdateObjective(objectivesData);
				}
			}
		}
		else if ((objectives_Custom = objective as Objectives_Custom) != null && (objectives_Custom.CustomQuestType == Objectives.CustomQuestTypes.NewGamePlus1 || objectives_Custom.CustomQuestType == Objectives.CustomQuestTypes.NewGamePlus2 || objectives_Custom.CustomQuestType == Objectives.CustomQuestTypes.NewGamePlus3 || objectives_Custom.CustomQuestType == Objectives.CustomQuestTypes.NewGamePlus4) && DataManager.Instance.BeatenLeshyLayer2 && DataManager.Instance.BeatenHeketLayer2 && DataManager.Instance.BeatenKallamarLayer2 && DataManager.Instance.BeatenShamuraLayer2 && !HasCustomObjectiveOfType(Objectives.CustomQuestTypes.MysticShopReturn) && !HasCompletedCustomObjectiveOfType(Objectives.CustomQuestTypes.MysticShopReturn))
		{
			Add(new Objectives_Custom(objective.GroupId, Objectives.CustomQuestTypes.MysticShopReturn));
		}
		Objectives_CollectItem objectives_CollectItem;
		if ((objectives_CollectItem = objective as Objectives_CollectItem) != null)
		{
			if (objectives_CollectItem.ItemType == InventoryItem.ITEM_TYPE.CRYSTAL && objectives_CollectItem.CustomTerm == "Objectives/Custom/CrystalForLighthouse" && !HasCustomObjectiveOfType(Objectives.CustomQuestTypes.LighthouseReturn) && !HasCompletedCustomObjectiveOfType(Objectives.CustomQuestTypes.LighthouseReturn))
			{
				Add(new Objectives_Custom("Objectives/GroupTitles/CrystalForLighthouse", Objectives.CustomQuestTypes.LighthouseReturn));
			}
			else if (objectives_CollectItem.ItemType == InventoryItem.ITEM_TYPE.GOD_TEAR && objectives_CollectItem.CustomTerm == "Objectives/CollectItem/DivineCrystals" && !HasCustomObjectiveOfType(Objectives.CustomQuestTypes.MysticShopReturn) && !HasCompletedCustomObjectiveOfType(Objectives.CustomQuestTypes.MysticShopReturn))
			{
				Add(new Objectives_Custom("Objectives/GroupTitles/MysticShop", Objectives.CustomQuestTypes.MysticShopReturn));
			}
		}
	}

	private void ObjectiveAdded(ObjectivesData objective)
	{
		Objectives_BuildStructure objectives_BuildStructure;
		if ((objectives_BuildStructure = objective as Objectives_BuildStructure) != null && objectives_BuildStructure.StructureType == StructureBrain.TYPES.DECORATION_BONE_CANDLE && !UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Decorations2))
		{
			Add(new Objectives_UnlockUpgrade("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Building_Decorations2));
		}
	}

	private IEnumerator DelayedStructureRemoved()
	{
		yield return new WaitForEndOfFrame();
		CheckObjectives(Objectives.TYPES.REMOVE_STRUCTURES);
	}

	private IEnumerator DelayedCheckCombatObjectives()
	{
		yield return new WaitForEndOfFrame();
		CheckObjectives(Objectives.TYPES.NO_DODGE);
		CheckObjectives(Objectives.TYPES.NO_DAMAGE);
		CheckObjectives(Objectives.TYPES.NO_CURSES);
		CheckObjectives(Objectives.TYPES.NO_HEALING);
	}

	private void OnStructureItemDeposited(Structure structure, InventoryItem item)
	{
		if (structure.Type == StructureBrain.TYPES.KITCHEN || structure.Type == StructureBrain.TYPES.KITCHEN_II)
		{
			CheckObjectives(Objectives.TYPES.DEPOSIT_FOOD);
		}
	}

	private void BiomeGenerator_OnBiomeChangeRoom()
	{
		for (int num = DataManager.Instance.FailedObjectives.Count - 1; num >= 0; num--)
		{
			if (DataManager.Instance.FailedObjectives[num] is Objectives_RoomChallenge)
			{
				ObjectiveRemoved(DataManager.Instance.FailedObjectives[num]);
			}
		}
	}

	private void OnEnemyKilled(Enemy enemy)
	{
		CheckObjectives(Objectives.TYPES.KILL_ENEMIES);
	}

	private void OnDummyShot()
	{
		Debug.Log("UPDATE!");
		CheckObjectives(Objectives.TYPES.SHOOT_DUMMIES);
	}

	public static void CheckObjectives(Objectives.TYPES objectiveType)
	{
		for (int i = 0; i < DataManager.Instance.Objectives.Count; i++)
		{
			ObjectivesData objectivesData = DataManager.Instance.Objectives[i];
			if (objectivesData.Type == objectiveType && UpdateObjective(objectivesData))
			{
				i--;
			}
		}
	}

	public static void CheckObjectives()
	{
		for (int i = 0; i < DataManager.Instance.Objectives.Count; i++)
		{
			ObjectivesData objectivesData = DataManager.Instance.Objectives[i];
			if (!(objectivesData is Objectives_Custom) && UpdateObjective(objectivesData))
			{
				i--;
			}
		}
	}

	public static void ObjectiveRemoved(ObjectivesData objective)
	{
		ObjectiveUpdated onObjectiveRemoved = ObjectiveManager.OnObjectiveRemoved;
		if (onObjectiveRemoved != null)
		{
			onObjectiveRemoved(objective);
		}
	}

	public static bool UpdateObjective(ObjectivesData objective)
	{
		bool result = false;
		InvokeOrQueue(delegate
		{
			ObjectiveUpdated onObjectiveUpdated = ObjectiveManager.OnObjectiveUpdated;
			if (onObjectiveUpdated != null)
			{
				onObjectiveUpdated(objective);
			}
		});
		if (objective.TryComplete())
		{
			DataManager.Instance.Objectives.Remove(objective);
			DataManager.Instance.CompletedObjectives.Add(objective);
			InvokeOrQueue(delegate
			{
				ObjectiveUpdated onObjectiveCompleted = ObjectiveManager.OnObjectiveCompleted;
				if (onObjectiveCompleted != null)
				{
					onObjectiveCompleted(objective);
				}
			});
			result = true;
		}
		else if (objective.IsFailed)
		{
			DataManager.Instance.Objectives.Remove(objective);
			DataManager.Instance.FailedObjectives.Add(objective);
			InvokeOrQueue(delegate
			{
				ObjectiveUpdated onObjectiveFailed = ObjectiveManager.OnObjectiveFailed;
				if (onObjectiveFailed != null)
				{
					onObjectiveFailed(objective);
				}
			});
			result = true;
		}
		return result;
	}

	private static void OnObjectiveGroupBeginHide(string uniqueGroupID)
	{
		UpdateQuestStatus(uniqueGroupID);
	}

	public static void UpdateQuestStatus(string uniqueGroupID, bool playAnimation = true)
	{
		List<ObjectivesData> allObjectivesOfGroup = GetAllObjectivesOfGroup(uniqueGroupID);
		if (!AllObjectivesComplete(allObjectivesOfGroup))
		{
			return;
		}
		string groupId = allObjectivesOfGroup[0].GroupId;
		foreach (ObjectivesData item in allObjectivesOfGroup)
		{
			RemoveCompleteObjective(item);
			RemoveFailedObjective(item);
		}
		if (IsTracked(uniqueGroupID))
		{
			TrackedUniqueGroupIDs.Remove(uniqueGroupID);
		}
		Action<string> onObjectiveGroupCompleted = OnObjectiveGroupCompleted;
		if (onObjectiveGroupCompleted != null)
		{
			onObjectiveGroupCompleted(groupId);
		}
	}

	public static void RemoveCompleteObjective(ObjectivesData objective)
	{
		if (DataManager.Instance.CompletedObjectives.Contains(objective))
		{
			DataManager.Instance.CompletedObjectives.Remove(objective);
		}
	}

	public static void RemoveFailedObjective(ObjectivesData objective)
	{
		if (DataManager.Instance.FailedObjectives.Contains(objective))
		{
			DataManager.Instance.FailedObjectives.Remove(objective);
		}
	}

	public static int GetNumberOfObjectivesInGroup(string groupID)
	{
		int num = 0;
		foreach (ObjectivesData objective in DataManager.Instance.Objectives)
		{
			if (objective.GroupId == groupID || LocalizationManager.GetTranslation(groupID) == objective.GroupId)
			{
				num++;
			}
		}
		foreach (ObjectivesData completedObjective in DataManager.Instance.CompletedObjectives)
		{
			if (completedObjective.GroupId == groupID || LocalizationManager.GetTranslation(groupID) == completedObjective.GroupId)
			{
				num++;
			}
		}
		return num;
	}

	public static List<ObjectivesData> GetCompletedObjectivesInGroup(string groupID)
	{
		List<ObjectivesData> list = new List<ObjectivesData>();
		foreach (ObjectivesData completedObjective in DataManager.Instance.CompletedObjectives)
		{
			if (completedObjective.GroupId == groupID)
			{
				list.Add(completedObjective);
			}
		}
		return list;
	}

	public static void ObjectiveFailed(ObjectivesData objective)
	{
		if (objective.Type != Objectives.TYPES.CUSTOM)
		{
			return;
		}
		switch (((Objectives_Custom)objective).CustomQuestType)
		{
		case Objectives.CustomQuestTypes.CrisisOfFaith:
		{
			List<FollowerBrain> list = new List<FollowerBrain>();
			for (int num = FollowerBrain.AllBrains.Count - 1; num >= 0; num--)
			{
				if (!FollowerManager.FollowerLocked(FollowerBrain.AllBrains[num].Info.ID) && FollowerBrain.AllBrains[num].Info.CursedState == Thought.None)
				{
					list.Add(FollowerBrain.AllBrains[num]);
				}
			}
			for (int i = 0; i < 2; i++)
			{
				if (list.Count == 0)
				{
					break;
				}
				float num2 = Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD);
				FollowerBrain followerBrain = list[UnityEngine.Random.Range(0, list.Count)];
				followerBrain.Stats.DissentGold = Mathf.Floor(UnityEngine.Random.Range(num2 * 0.1f, num2 * 0.4f));
				followerBrain.LeavingCult = true;
				list.Remove(followerBrain);
			}
			break;
		}
		case Objectives.CustomQuestTypes.GameOver:
			DataManager.Instance.GameOver = true;
			break;
		}
	}

	public static bool HasCustomObjectiveOfType(Objectives.CustomQuestTypes type)
	{
		foreach (ObjectivesData objective in DataManager.Instance.Objectives)
		{
			Objectives_Custom objectives_Custom;
			if ((objectives_Custom = objective as Objectives_Custom) != null && objectives_Custom.CustomQuestType == type)
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasCustomObjective<T>()
	{
		foreach (ObjectivesData objective in DataManager.Instance.Objectives)
		{
			ObjectivesData current;
			if ((current = objective) is T)
			{
				T val = (T)(object)current;
				return true;
			}
		}
		return false;
	}

	public static bool HasCustomObjective(Objectives.TYPES objectiveType)
	{
		foreach (ObjectivesData objective in DataManager.Instance.Objectives)
		{
			if (objective.Type == objectiveType)
			{
				return true;
			}
		}
		return false;
	}

	public static List<T> GetObjectivesOfType<T>()
	{
		List<T> list = new List<T>();
		foreach (ObjectivesData objective in DataManager.Instance.Objectives)
		{
			ObjectivesData current;
			if ((current = objective) is T)
			{
				T item = (T)(object)current;
				list.Add(item);
			}
		}
		return list;
	}

	public static bool HasCompletedCustomObjectiveOfType(Objectives.CustomQuestTypes type)
	{
		foreach (ObjectivesData completedObjective in DataManager.Instance.CompletedObjectives)
		{
			Objectives_Custom objectives_Custom;
			if ((objectives_Custom = completedObjective as Objectives_Custom) != null && objectives_Custom.CustomQuestType == type)
			{
				return true;
			}
		}
		return false;
	}

	public static string GetUniqueID()
	{
		return (++DataManager.Instance.ObjectiveGroupID).ToString();
	}
}
