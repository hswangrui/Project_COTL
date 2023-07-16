using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.Mission;
using src.Extensions;
using src.UI.Menus;
using UnityEngine;
using UnityEngine.UI;

public class Interaction_Missionaries : Interaction
{
	[Serializable]
	public struct MissionarySlot
	{
		public GameObject Free;

		public GameObject Occupied;
	}

	public Canvas UICanvas;

	public Image UIProgress;

	private Structures_Missionary _StructureInfo;

	public MissionarySlot[] MissionarySlots;

	public static List<Interaction_Missionaries> Missionaries = new List<Interaction_Missionaries>();

	private List<int> transitoningIDs = new List<int>();

	public Structure Structure { get; private set; }

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_Missionary structureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_Missionary;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	private void Start()
	{
		UpdateLocalisation();
		Missionaries.Add(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)Structure)
		{
			Structure structure = Structure;
			structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		}
		for (int num = transitoningIDs.Count - 1; num >= 0; num--)
		{
			Follower follower = FollowerManager.FindFollowerByID(transitoningIDs[num]);
			if ((bool)follower)
			{
				follower.SetOutfit(FollowerOutfitType.Follower, false);
				follower.Interaction_FollowerInteraction.Interactable = true;
			}
			DataManager.Instance.Followers_Transitioning_IDs.Remove(transitoningIDs[num]);
		}
		Missionaries.Remove(this);
	}

	private void OnBrainAssigned()
	{
		UpdateSlots();
	}

	public override void OnEnableInteraction()
	{
		Structure = GetComponentInParent<Structure>();
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
		}
		base.OnEnableInteraction();
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		if (StructureInfo == null)
		{
			return;
		}
		foreach (int multipleFollowerID in StructureInfo.MultipleFollowerIDs)
		{
			FollowerInfo infoByID = FollowerInfo.GetInfoByID(multipleFollowerID);
			if (infoByID != null)
			{
				FollowerBrain orCreateBrain = FollowerBrain.GetOrCreateBrain(infoByID);
				if (orCreateBrain != null && orCreateBrain != null && orCreateBrain.CurrentTask != null && orCreateBrain.CurrentTask is FollowerTask_ManualControl)
				{
					orCreateBrain._directInfoAccess.MissionarySuccessful = MissionaryManager.GetReward((InventoryItem.ITEM_TYPE)orCreateBrain._directInfoAccess.MissionaryType, orCreateBrain._directInfoAccess.MissionaryChance, orCreateBrain.Info.ID).Length != 0;
					orCreateBrain.HardSwapToTask(new FollowerTask_OnMissionary());
					DataManager.Instance.Followers_OnMissionary_IDs.Add(orCreateBrain.Info.ID);
					DataManager.Instance.Followers_Transitioning_IDs.Remove(orCreateBrain.Info.ID);
				}
			}
		}
	}

	public void UpdateSlots()
	{
		MissionarySlot[] missionarySlots = MissionarySlots;
		for (int i = 0; i < missionarySlots.Length; i++)
		{
			MissionarySlot missionarySlot = missionarySlots[i];
			missionarySlot.Free.SetActive(true);
			missionarySlot.Occupied.SetActive(false);
		}
		if (StructureInfo != null)
		{
			for (int j = 0; j < StructureInfo.MultipleFollowerIDs.Count; j++)
			{
				MissionarySlots[j].Free.SetActive(false);
				MissionarySlots[j].Occupied.SetActive(true);
			}
		}
	}

	public override void GetLabel()
	{
		if (!AtMissionaryLimit())
		{
			base.Label = ScriptLocalization.Structures.MISSIONARY;
		}
		else
		{
			base.Label = "";
		}
	}

	public override void GetSecondaryLabel()
	{
		if (StructureInfo.MultipleFollowerIDs.Count > 0)
		{
			base.SecondaryLabel = ScriptLocalization.UI_Settings_Controls.Interact;
		}
		else
		{
			base.SecondaryLabel = "";
		}
	}

	public override void OnBecomeCurrent()
	{
		Interactable = !AtMissionaryLimit();
		HasSecondaryInteraction = StructureInfo.MultipleFollowerIDs.Count > 0;
		base.OnBecomeCurrent();
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (AtMissionaryLimit())
		{
			return;
		}
		GameManager.GetInstance().OnConversationNew();
		Time.timeScale = 0f;
		HUD_Manager.Instance.Hide(false, 0);
		for (int num = StructureInfo.MultipleFollowerIDs.Count - 1; num >= 0; num--)
		{
			if (!DataManager.Instance.Followers_OnMissionary_IDs.Contains(StructureInfo.MultipleFollowerIDs[num]) && !transitoningIDs.Contains(StructureInfo.MultipleFollowerIDs[num]))
			{
				StructureInfo.MultipleFollowerIDs.Remove(StructureInfo.MultipleFollowerIDs[num]);
			}
		}
		UIMissionaryMenuController missionaryMenuInstance = MonoSingleton<UIManager>.Instance.MissionaryMenuTemplate.Instantiate();
		missionaryMenuInstance.Show(MissionaryManager.FollowersAvailableForMission(), null, false, UpgradeSystem.Type.Building_Missionary);
		UIMissionaryMenuController uIMissionaryMenuController = missionaryMenuInstance;
		uIMissionaryMenuController.OnMissionaryChosen = (Action<FollowerInfo, InventoryItem.ITEM_TYPE>)Delegate.Combine(uIMissionaryMenuController.OnMissionaryChosen, new Action<FollowerInfo, InventoryItem.ITEM_TYPE>(OnSentFollowerOnMissionary));
		UIMissionaryMenuController uIMissionaryMenuController2 = missionaryMenuInstance;
		uIMissionaryMenuController2.OnHidden = (Action)Delegate.Combine(uIMissionaryMenuController2.OnHidden, (Action)delegate
		{
			missionaryMenuInstance = null;
			Time.timeScale = 1f;
			HUD_Manager.Instance.Show();
			GameManager.GetInstance().OnConversationEnd();
		});
	}

	public override void OnSecondaryInteract(StateMachine state)
	{
		base.OnSecondaryInteract(state);
		GameManager.GetInstance().OnConversationNew();
		Time.timeScale = 0f;
		HUD_Manager.Instance.Hide(false, 0);
		for (int num = StructureInfo.MultipleFollowerIDs.Count - 1; num >= 0; num--)
		{
			if (!DataManager.Instance.Followers_OnMissionary_IDs.Contains(StructureInfo.MultipleFollowerIDs[num]) && !transitoningIDs.Contains(StructureInfo.MultipleFollowerIDs[num]))
			{
				StructureInfo.MultipleFollowerIDs.Remove(StructureInfo.MultipleFollowerIDs[num]);
			}
		}
		UIMissionMenuController missionMenuInstance = MonoSingleton<UIManager>.Instance.MissionMenuTemplate.Instantiate();
		missionMenuInstance.Show(StructureInfo.MultipleFollowerIDs);
		UIMissionMenuController uIMissionMenuController = missionMenuInstance;
		uIMissionMenuController.OnHidden = (Action)Delegate.Combine(uIMissionMenuController.OnHidden, (Action)delegate
		{
			missionMenuInstance = null;
			Time.timeScale = 1f;
			HUD_Manager.Instance.Show();
			GameManager.GetInstance().OnConversationEnd();
		});
	}

	private bool AtMissionaryLimit()
	{
		int num = StructureInfo.MultipleFollowerIDs.Count + transitoningIDs.Count;
		if ((StructureInfo.Type == StructureBrain.TYPES.MISSIONARY && num >= 1) || (StructureInfo.Type == StructureBrain.TYPES.MISSIONARY_II && num >= 2) || (StructureInfo.Type == StructureBrain.TYPES.MISSIONARY_III && num >= 3))
		{
			return true;
		}
		return false;
	}

	protected override void Update()
	{
		base.Update();
	}

	private void OnSentFollowerOnMissionary(FollowerInfo followerInfo, InventoryItem.ITEM_TYPE type)
	{
		transitoningIDs.Add(followerInfo.ID);
		StructureInfo.MultipleFollowerIDs.Add(followerInfo.ID);
		Follower follower = FollowerManager.FindFollowerByID(followerInfo.ID);
		FollowerBrain brain = follower.Brain;
		float num = 1f;
		if (StructureInfo.Type == StructureBrain.TYPES.MISSIONARY_II)
		{
			num = 0.8f;
		}
		else if (StructureInfo.Type == StructureBrain.TYPES.MISSIONARY_II)
		{
			num = 0.6f;
		}
		DataManager.Instance.NextMissionarySuccessful = false;
		List<Structures_Missionary> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_Missionary>(brain.Location);
		brain._directInfoAccess.MissionaryTimestamp = TimeManager.TotalElapsedGameTime;
		brain._directInfoAccess.MissionaryDuration = (float)(MissionaryManager.GetDurationDeterministic(followerInfo, type) * 1200) / num;
		brain._directInfoAccess.MissionaryIndex = allStructuresOfType.IndexOf(structureBrain);
		brain._directInfoAccess.MissionaryType = (int)type;
		brain._directInfoAccess.MissionaryChance = MissionaryManager.GetChance(type, brain._directInfoAccess, StructureInfo.Type);
		DataManager.Instance.Followers_Transitioning_IDs.Add(followerInfo.ID);
		brain.CompleteCurrentTask();
		brain.HardSwapToTask(new FollowerTask_ManualControl());
		follower.OverridingOutfit = true;
		follower.transform.position = MissionarySlots[StructureInfo.MultipleFollowerIDs.Count - 1].Free.transform.position;
		follower.Interaction_FollowerInteraction.Interactable = false;
		StartCoroutine(SentFollowerIE(follower));
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.SendFollowerOnMissionary, follower.Brain.Info.ID);
	}

	private IEnumerator SentFollowerIE(Follower follower)
	{
		follower.HideAllFollowerIcons();
		yield return new WaitForSeconds(0.5f);
		AudioManager.Instance.PlayOneShot("event:/followers/backpack", follower.transform.position);
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.SetBodyAnimation("missionary-start", false);
		UpdateSlots();
		yield return new WaitForSeconds(0.1f);
		follower.SetOutfit(FollowerOutfitType.Sherpa, false);
		yield return new WaitForSeconds(1f);
		follower.SetBodyAnimation("wave", false);
		yield return new WaitForSeconds(1.8f);
		follower.AddBodyAnimation("idle", false, 0f);
		follower.Brain._directInfoAccess.MissionarySuccessful = MissionaryManager.GetReward((InventoryItem.ITEM_TYPE)follower.Brain._directInfoAccess.MissionaryType, follower.Brain._directInfoAccess.MissionaryChance, follower.Brain.Info.ID).Length != 0;
		follower.State.CURRENT_STATE = StateMachine.State.Idle;
		follower.Brain.HardSwapToTask(new FollowerTask_OnMissionary());
		follower.OverridingOutfit = false;
		transitoningIDs.Remove(follower.Brain.Info.ID);
		DataManager.Instance.Followers_Transitioning_IDs.Remove(follower.Brain.Info.ID);
		follower.Brain._directInfoAccess.MissionaryExhaustion += 3f;
		DataManager.Instance.Followers_OnMissionary_IDs.Add(follower.Brain.Info.ID);
	}
}
