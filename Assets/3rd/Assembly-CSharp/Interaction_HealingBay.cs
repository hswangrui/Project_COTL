using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using src.Extensions;
using UnityEngine;

public class Interaction_HealingBay : Interaction
{
	public static List<Interaction_HealingBay> HealingBays = new List<Interaction_HealingBay>();

	public Structure Structure;

	private Structures_HealingBay _StructureInfo;

	public GameObject FollowerPosition;

	public GameObject EntrancePosition;

	private bool Activated;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_HealingBay structureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_HealingBay;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	public int PatientID
	{
		get
		{
			return StructureInfo.FollowerID;
		}
	}

	private int Cost
	{
		get
		{
			if (structureBrain.Data.Type != StructureBrain.TYPES.HEALING_BAY)
			{
				return 10;
			}
			return 15;
		}
	}

	private void Start()
	{
		HealingBays.Add(this);
	}

	public override void GetLabel()
	{
		base.GetLabel();
		base.Label = ScriptLocalization.Interactions_Bed.Assign;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (Activated)
		{
			return;
		}
		Activated = true;
		Time.timeScale = 0f;
		HUD_Manager.Instance.Hide(false, 0);
		UIFollowerSelectMenuController followerSelectMenu = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
	//	followerSelectMenu.VotingType = TwitchVoting.VotingType.HEALING_BAY;
		List<FollowerInfo> list = new List<FollowerInfo>();
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			if (FollowerManager.FollowerLocked(follower.ID) || follower.CursedState != Thought.Ill)
			{
				list.Add(follower);
			}
		}
		followerSelectMenu.Show(DataManager.Instance.Followers, list);
		UIFollowerSelectMenuController uIFollowerSelectMenuController = followerSelectMenu;
		uIFollowerSelectMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIFollowerSelectMenuController.OnFollowerSelected, new Action<FollowerInfo>(OnFollowerChosenForConversion));
		UIFollowerSelectMenuController uIFollowerSelectMenuController2 = followerSelectMenu;
		uIFollowerSelectMenuController2.OnCancel = (Action)Delegate.Combine(uIFollowerSelectMenuController2.OnCancel, (Action)delegate
		{
			followerSelectMenu = null;
			OnHidden();
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController3 = followerSelectMenu;
		uIFollowerSelectMenuController3.OnShow = (Action)Delegate.Combine(uIFollowerSelectMenuController3.OnShow, (Action)delegate
		{
			foreach (FollowerInformationBox followerInfoBox in followerSelectMenu.FollowerInfoBoxes)
			{
				followerInfoBox.ShowItemCostValue(InventoryItem.ITEM_TYPE.FLOWER_RED, Cost);
			}
		});
	}

	private void OnFollowerChosenForConversion(FollowerInfo followerInfo)
	{
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.FLOWER_RED) >= Cost)
		{
			Follower follower = FollowerManager.FindFollowerByID(followerInfo.ID);
			if ((bool)follower)
			{
				StartCoroutine(HealingRoutine(follower));
			}
			Inventory.ChangeItemQuantity(InventoryItem.ITEM_TYPE.FLOWER_RED, -Cost);
		}
		else
		{
			OnHidden();
		}
	}

	private IEnumerator HealingRoutine(Follower follower)
	{
		Time.timeScale = 1f;
		PlayerFarming.Instance.GoToAndStop(EntrancePosition.transform.position + Vector3.right * 2f, follower.gameObject);
		FollowerTask_ManualControl task = new FollowerTask_ManualControl();
		follower.Brain.HardSwapToTask(task);
		follower.transform.position = EntrancePosition.transform.position;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(follower.gameObject);
		yield return new WaitForSeconds(1f);
		bool waiting = true;
		task.GoToAndStop(follower, FollowerPosition.transform.position, delegate
		{
			waiting = false;
		});
		SimulationManager.Pause();
		while (waiting)
		{
			yield return null;
		}
		follower.SimpleAnimator.Animate("sleep_bedrest_justhead", 1, true, 0f);
		float illness = follower.Brain._directInfoAccess.Illness;
		float t = 0f;
		float duration = 3f;
		while (t < duration)
		{
			if (Time.deltaTime > 0f)
			{
				t += Time.deltaTime;
				float t2 = t / duration;
				follower.Brain.Stats.Illness = Mathf.Lerp(illness, 0f, t2);
				if (Time.frameCount % 10 == 0 && t > 0.5f && t < duration - 0.5f)
				{
					ResourceCustomTarget.Create(follower.gameObject, PlayerFarming.Instance.transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * 0.5f), InventoryItem.ITEM_TYPE.FLOWER_RED, null);
				}
			}
			yield return null;
		}
		FollowerBrainStats.StatStateChangedEvent onIllnessStateChanged = FollowerBrainStats.OnIllnessStateChanged;
		if (onIllnessStateChanged != null)
		{
			onIllnessStateChanged(follower.Brain.Info.ID, FollowerStatState.Off, FollowerStatState.On);
		}
		follower.Brain.ClearPersonalOverrideTaskProvider();
		yield return new WaitForSeconds(0.5f);
		follower.SimpleAnimator.Animate("idle", 1, true, 0f);
		follower.SetBodyAnimation("Reactions/react-happy1", false);
		yield return new WaitForSeconds(0.5f);
		CameraManager.shakeCamera(1.5f, UnityEngine.Random.Range(0, 360));
		BiomeConstants.Instance.EmitHeartPickUpVFX(follower.gameObject.transform.position, 0f, "black", "burst_big");
		AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", follower.gameObject);
		yield return new WaitForSeconds(1.6f);
		SimulationManager.UnPause();
		follower.Brain.CompleteCurrentTask();
		GameManager.GetInstance().OnConversationEnd();
		ObjectiveManager.CheckObjectives(Objectives.TYPES.SEND_FOLLOWER_BED_REST);
		Activated = false;
	}

	private void OnHidden()
	{
		Activated = false;
		Time.timeScale = 1f;
		HUD_Manager.Instance.Show();
		GameManager.GetInstance().OnConversationEnd();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		HealingBays.Remove(this);
	}
}
