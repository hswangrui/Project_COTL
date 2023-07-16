using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using src.Extensions;
using src.UI.Menus.ShareHouseMenu;
using UnityEngine;

public class Interaction_Bed : Interaction
{
	public Structure Structure;

	[SerializeField]
	private interaction_CollapsedBed collapsedBed;

	[SerializeField]
	private GameObject uncollapsedBed;

	[SerializeField]
	private Dwelling dwelling;

	private Structures_Bed _StructureInfo;

	public SpriteXPBar XPBar;

	public bool cacheCollapse;

	private bool Activated;

	private FollowerInfo OldFollower;

	private Follower follower;

	private bool Activating;

	public virtual Structures_Bed StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_Bed;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	public void Collapse()
	{
		StructureBrain.Collapse();
		UpdateBed();
	}

	private void Start()
	{
		dwelling = GetComponent<Dwelling>();
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		FollowerBrain.OnDwellingAssigned = (FollowerBrain.DwellingAssignmentChanged)Delegate.Combine(FollowerBrain.OnDwellingAssigned, new FollowerBrain.DwellingAssignmentChanged(OnDwellingAssignmentChanged));
		FollowerBrain.OnDwellingAssignedAwaitClaim = (FollowerBrain.DwellingAssignmentChanged)Delegate.Combine(FollowerBrain.OnDwellingAssignedAwaitClaim, new FollowerBrain.DwellingAssignmentChanged(OnDwellingAssignmentChanged));
		FollowerBrain.OnDwellingCleared = (FollowerBrain.DwellingAssignmentChanged)Delegate.Combine(FollowerBrain.OnDwellingCleared, new FollowerBrain.DwellingAssignmentChanged(OnDwellingAssignmentChanged));
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
			return;
		}
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	private void OnBrainAssigned()
	{
		OldFollower = FollowerInfo.GetInfoByID(StructureBrain.Data.FollowerID);
		if (StructureBrain.Data.Type == global::StructureBrain.TYPES.BED_3)
		{
			HasSecondaryInteraction = true;
			UpdateBar();
		}
		StructureBrain.OnBedCollapsed += UpdateBed;
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		Structures_Bed structureBrain = StructureBrain;
		structureBrain.OnSoulsGained = (Action<int>)Delegate.Combine(structureBrain.OnSoulsGained, new Action<int>(OnSoulsGained));
		UpdateBed();
	}

	private void OnSoulsGained(int count)
	{
		UpdateBar();
	}

	private void UpdateBar()
	{
		if (!(XPBar == null) && StructureBrain != null)
		{
			XPBar.gameObject.SetActive(true);
			float value = Mathf.Clamp((float)StructureBrain.SoulCount / (float)StructureBrain.SoulMax, 0f, 1f);
			XPBar.UpdateBar(value);
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (StructureBrain != null)
		{
			StructureBrain.OnBedCollapsed -= UpdateBed;
		}
		if (StructureBrain != null)
		{
			Structures_Bed structureBrain = StructureBrain;
			structureBrain.OnSoulsGained = (Action<int>)Delegate.Remove(structureBrain.OnSoulsGained, new Action<int>(OnSoulsGained));
		}
		FollowerBrain.OnDwellingAssigned = (FollowerBrain.DwellingAssignmentChanged)Delegate.Remove(FollowerBrain.OnDwellingAssigned, new FollowerBrain.DwellingAssignmentChanged(OnDwellingAssignmentChanged));
		FollowerBrain.OnDwellingAssignedAwaitClaim = (FollowerBrain.DwellingAssignmentChanged)Delegate.Remove(FollowerBrain.OnDwellingAssignedAwaitClaim, new FollowerBrain.DwellingAssignmentChanged(OnDwellingAssignmentChanged));
		FollowerBrain.OnDwellingCleared = (FollowerBrain.DwellingAssignmentChanged)Delegate.Remove(FollowerBrain.OnDwellingCleared, new FollowerBrain.DwellingAssignmentChanged(OnDwellingAssignmentChanged));
	}

	public void UpdateBed()
	{
		if (StructureBrain.Data.Type != global::StructureBrain.TYPES.BED_3)
		{
			if (cacheCollapse != StructureBrain.IsCollapsed)
			{
				BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position);
				AudioManager.Instance.PlayOneShot("event:/building/finished_fabric", base.transform.position);
				cacheCollapse = StructureBrain.IsCollapsed;
			}
			if ((bool)collapsedBed)
			{
				collapsedBed.gameObject.SetActive(StructureBrain.IsCollapsed);
			}
			if ((bool)uncollapsedBed)
			{
				uncollapsedBed.gameObject.SetActive(!StructureBrain.IsCollapsed);
			}
			base.enabled = !StructureBrain.IsCollapsed;
		}
	}

	private void OnDwellingAssignmentChanged(int followerID, Dwelling.DwellingAndSlot d)
	{
		if (Structure != null && Structure.Structure_Info != null && d.ID == Structure.Structure_Info.ID)
		{
			OldFollower = FollowerInfo.GetInfoByID(StructureBrain.Data.FollowerID);
		}
	}

	public override void GetLabel()
	{
		if (StructureBrain == null)
		{
			return;
		}
		if (OldFollower != null && StructureBrain.Data.FollowerID != -1)
		{
			base.Label = ScriptLocalization.Interactions_Bed.Re_Assign + " | ";
			string livesHere = ScriptLocalization.Interactions_Bed.LivesHere;
			base.SecondaryLabel = " " + livesHere.Replace("{0}", OldFollower.Name);
		}
		else
		{
			base.Label = ScriptLocalization.Interactions_Bed.Assign + " | ";
			base.SecondaryLabel = " " + ScriptLocalization.Interactions_Bed.Unoccupied;
		}
		if (StructureBrain.Data.Type == global::StructureBrain.TYPES.BED_3)
		{
			XPBar.gameObject.SetActive(true);
			if (StructureBrain.SoulCount > 0)
			{
				string text = ((GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten) ? "<sprite name=\"icon_spirits\">" : "<sprite name=\"icon_blackgold\">");
				string receiveDevotion = ScriptLocalization.Interactions.ReceiveDevotion;
				base.SecondaryLabel = receiveDevotion + " " + text + " " + _StructureInfo.SoulCount + StaticColors.GreyColorHex + " / " + StructureBrain.SoulMax;
			}
		}
		else if (StructureBrain.Data.Type == global::StructureBrain.TYPES.SHARED_HOUSE)
		{
			base.Label = LocalizationManager.GetTranslation("Interactions/View");
			base.SecondaryLabel = string.Empty;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (Activated)
		{
			return;
		}
		Activated = true;
		GameManager.GetInstance().OnConversationNew();
		Time.timeScale = 0f;
		HUD_Manager.Instance.Hide(false, 0);
		if (Structure.Type == global::StructureBrain.TYPES.SHARED_HOUSE)
		{
			UIShareHouseMenuController uIShareHouseMenuController = MonoSingleton<UIManager>.Instance.ShareHouseMenuTemplate.Instantiate();
			uIShareHouseMenuController.Show(this);
			uIShareHouseMenuController.OnHidden = (Action)Delegate.Combine(uIShareHouseMenuController.OnHidden, (Action)delegate
			{
				OnHidden();
				base.HasChanged = true;
			});
			return;
		}
		UIFollowerSelectMenuController uIFollowerSelectMenuController = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
	//	uIFollowerSelectMenuController.VotingType = TwitchVoting.VotingType.BED;
		uIFollowerSelectMenuController.Show(DataManager.Instance.Followers, GetFollowerBlacklist());
		uIFollowerSelectMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIFollowerSelectMenuController.OnFollowerSelected, new Action<FollowerInfo>(OnFollowerChosenForConversion));
		uIFollowerSelectMenuController.OnHidden = (Action)Delegate.Combine(uIFollowerSelectMenuController.OnHidden, (Action)delegate
		{
			OnHidden();
			base.HasChanged = true;
		});
	}

	public List<FollowerInfo> GetFollowerBlacklist()
	{
		List<FollowerInfo> list = new List<FollowerInfo>();
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			if (FollowerManager.FollowerLocked(follower.ID) && !StructureBrain.Data.MultipleFollowerIDs.Contains(follower.ID))
			{
				list.Add(follower);
			}
		}
		if (OldFollower != null)
		{
			list.Add(OldFollower);
		}
		return list;
	}

	private void WakeUpFollower()
	{
		OldFollower = FollowerInfo.GetInfoByID(StructureBrain.Data.FollowerID);
		follower = FollowerManager.FindFollowerByID(OldFollower.ID);
		if (!(follower == null))
		{
			follower.Brain.ClearPersonalOverrideTaskProvider();
			follower.Brain.AddThought(Thought.SleepInterrupted);
			follower.Brain.CompleteCurrentTask();
			follower.Brain.HardSwapToTask(new FollowerTask_ManualControl());
			follower.Brain._directInfoAccess.WakeUpDay = TimeManager.CurrentDay;
			CultFaithManager.AddThought(Thought.Cult_WokeUpFollower, follower.Brain.Info.ID, 1f);
			follower.TimedAnimation("tantrum", 3.1666667f, delegate
			{
				follower.Brain.CompleteCurrentTask();
			});
		}
	}

	public void OnFollowerChosenForConversion(FollowerInfo followerInfo)
	{
		int num = -1;
		int dwellingslot = dwelling.StructureInfo.MultipleFollowerIDs.Count;
		if (StructureBrain.Data.FollowerID == num)
		{
			StructureBrain.Data.FollowerID = -1;
		}
		if (StructureBrain.Data.MultipleFollowerIDs.Count >= StructureBrain.SlotCount)
		{
			num = StructureBrain.Data.MultipleFollowerIDs[0];
			StructureBrain.Data.MultipleFollowerIDs.Remove(num);
			if (FollowerInfo.GetInfoByID(num) != null)
			{
				Follower follower = FollowerManager.FindFollowerByID(num);
				if ((bool)follower)
				{
					if (follower.Brain.CurrentTaskType == FollowerTaskType.ClaimDwelling)
					{
						follower.Brain.CurrentTask.Abort();
					}
					dwellingslot = follower.Brain._directInfoAccess.DwellingSlot;
					follower.Brain.ClearDwelling();
				}
			}
		}
		int num2 = Dwelling.NO_HOME;
		int dwellingslot2 = Dwelling.NO_HOME;
		if (followerInfo != null)
		{
			Follower follower2 = FollowerManager.FindFollowerByID(followerInfo.ID);
			if (follower2.Brain.CurrentTaskType == FollowerTaskType.ClaimDwelling)
			{
				follower2.Brain.CurrentTask.Abort();
			}
			num2 = ((follower2.Brain.GetDwellingAndSlot() != null) ? follower2.Brain.GetDwellingAndSlot().ID : Dwelling.NO_HOME);
			dwellingslot2 = ((follower2.Brain.GetDwellingAndSlot() != null) ? follower2.Brain.GetDwellingAndSlot().dwellingslot : Dwelling.NO_HOME);
			follower2.Brain.ClearDwelling();
			follower2.Brain.AssignDwelling(new Dwelling.DwellingAndSlot(dwelling.StructureInfo.ID, dwellingslot, 0), followerInfo.ID, false);
			follower2.Brain._directInfoAccess.PreviousDwellingID = Dwelling.NO_HOME;
			follower2.Brain._directInfoAccess.WakeUpDay = -1;
			follower2.Brain.CheckChangeTask();
		}
		if (num != -1 && FollowerInfo.GetInfoByID(num) != null)
		{
			Follower follower3 = FollowerManager.FindFollowerByID(num);
			if (follower3 == null)
			{
				base.HasChanged = true;
				return;
			}
			if (follower3.Brain.CurrentTaskType == FollowerTaskType.ClaimDwelling)
			{
				follower3.Brain.CurrentTask.Abort();
			}
			if (num2 != Dwelling.NO_HOME)
			{
				follower3.Brain.AssignDwelling(new Dwelling.DwellingAndSlot(num2, dwellingslot2, 0), follower3.Brain.Info.ID, false);
				StructureManager.GetStructureByID<Structures_Bed>(num2).ReservedForTask = true;
			}
			else
			{
				Dwelling.DwellingAndSlot freeDwellingAndSlot = StructureManager.GetFreeDwellingAndSlot(FollowerLocation.Base, OldFollower);
				if (freeDwellingAndSlot != null)
				{
					follower3.Brain.AssignDwelling(freeDwellingAndSlot, follower3.Brain.Info.ID, false);
					StructureManager.GetStructureByID<Structures_Bed>(freeDwellingAndSlot.ID);
				}
			}
			follower3.Brain._directInfoAccess.PreviousDwellingID = Dwelling.NO_HOME;
			follower3.Brain._directInfoAccess.WakeUpDay = -1;
			follower3.Brain.CheckChangeTask();
		}
		base.HasChanged = true;
	}

	private void OnHidden()
	{
		Activated = false;
		Time.timeScale = 1f;
		HUD_Manager.Instance.Show();
		GameManager.GetInstance().OnConversationEnd();
	}

	public override void OnSecondaryInteract(StateMachine state)
	{
		base.OnSecondaryInteract(state);
		if (StructureBrain.SoulCount > 0)
		{
			if (!Activating)
			{
				Activating = true;
				StartCoroutine(GiveReward());
			}
		}
		else
		{
			MonoSingleton<Indicator>.Instance.PlayShake();
		}
	}

	private IEnumerator GiveReward()
	{
		int Souls = StructureBrain.SoulCount;
		for (int i = 0; i < Souls; i++)
		{
			if (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten)
			{
				SoulCustomTarget.Create(PlayerFarming.Instance.gameObject, base.transform.position, Color.white, delegate
				{
					PlayerFarming.Instance.GetSoul(1);
					Structures_Bed structureBrain = StructureBrain;
					int soulCount = structureBrain.SoulCount - 1;
					structureBrain.SoulCount = soulCount;
					UpdateBar();
				});
			}
			else
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(8f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
			}
			float value = Mathf.Clamp((float)(Souls - i) / (float)StructureBrain.SoulMax, 0f, 1f);
			XPBar.UpdateBar(value);
			yield return new WaitForSeconds(0.1f);
		}
		XPBar.UpdateBar(0f);
		StructureBrain.SoulCount = 0;
		GetSecondaryLabel();
		base.HasChanged = true;
		Activating = false;
	}
}
