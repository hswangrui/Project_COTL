using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using UnityEngine;

public class Interaction_FireDancePit : Interaction
{
	public static Interaction_FireDancePit Instance;

	public Structure Structure;

	private Structures_DancingFirePit _StructureInfo;

	public AudioClip DanceSong;

	public List<InventoryItem.ITEM_TYPE> AllowedFuel;

	public GameObject BonfireOn;

	public GameObject BonfireOff;

	public GameObject Center;

	public GameObject PlayerPosition;

	public Transform[] Positions;

	private bool isLit = true;

	private bool isDancing;

	[SerializeField]
	private GameObject _resourceTarget;

	private string sAddFuel;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_DancingFirePit Brain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_DancingFirePit;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		UpdateLocalisation();
		StructureInfo.MaxFuel = InventoryItem.FuelWeight(InventoryItem.ITEM_TYPE.LOG) * 50;
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		ContinuouslyHold = true;
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Combine(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(SetImages));
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (Structure.Brain == null)
		{
			Structure.Brain = StructureBrain.GetOrCreateBrain(StructuresData.GetInfoByType(StructureBrain.TYPES.DANCING_FIREPIT, 0));
		}
		if (Structure.Brain != null)
		{
			SetImages();
		}
	}

	private void OnBrainAssigned()
	{
		SetImages();
		if (!StructureInfo.IsGatheringActive)
		{
			StructureInfo.GivenHealth = false;
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Remove(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(SetImages));
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	private void OnStructureAdded(StructuresData structure)
	{
		if (structure.ID == StructureInfo.ID)
		{
			SetImages();
			StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
		}
	}

	private void SetImages()
	{
		if (StructureInfo.IsGatheringActive)
		{
			BonfireOn.SetActive(true);
			BonfireOff.SetActive(false);
		}
		else
		{
			BonfireOn.SetActive(false);
			BonfireOff.SetActive(true);
		}
	}

	public Vector3 GetDancePosition(int followerId)
	{
		int num = 0;
		for (int i = 0; i < FollowerBrain.AllBrains.Count; i++)
		{
			if (FollowerBrain.AllBrains[i].Info.ID == followerId)
			{
				num = i;
				break;
			}
		}
		if (num < Positions.Length)
		{
			return Positions[num].position;
		}
		Vector3 vector = base.transform.position + Vector3.down * 2f;
		Vector3 vector2 = UnityEngine.Random.insideUnitCircle;
		float num2 = UnityEngine.Random.Range(2f, 4f);
		return vector + vector2 * num2;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (!StructureInfo.IsGatheringActive)
		{
			state.CURRENT_STATE = StateMachine.State.InActive;
			state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
			CameraFollowTarget cameraFollowTarget = CameraFollowTarget.Instance;
			cameraFollowTarget.SetOffset(new Vector3(0f, 4.5f, 2f));
			cameraFollowTarget.AddTarget(base.gameObject, 1f);
			UIItemSelectorOverlayController itemSelector = MonoSingleton<UIManager>.Instance.ShowItemSelector(InventoryItem.AllBurnableFuel, new ItemSelector.Params
			{
				Key = "addfuel",
				Context = ItemSelector.Context.Add,
				Offset = new Vector2(0f, 125f),
				HideOnSelection = false,
				ShowEmpty = true
			});
			UIItemSelectorOverlayController uIItemSelectorOverlayController = itemSelector;
			uIItemSelectorOverlayController.OnItemChosen = (Action<InventoryItem.ITEM_TYPE>)Delegate.Combine(uIItemSelectorOverlayController.OnItemChosen, (Action<InventoryItem.ITEM_TYPE>)delegate(InventoryItem.ITEM_TYPE chosenItem)
			{
				Debug.Log(string.Format("Deposit {0} to fuel fire pit", chosenItem).Colour(Color.yellow));
				AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.transform.position);
				ResourceCustomTarget.Create(_resourceTarget, PlayerFarming.Instance.transform.position, chosenItem, null);
				Inventory.ChangeItemQuantity((int)chosenItem, -1);
				StructureInfo.Fuel = Mathf.Clamp(StructureInfo.Fuel + InventoryItem.FuelWeight(chosenItem), 0, StructureInfo.MaxFuel);
				if (!RequiresFuel())
				{
					itemSelector.Hide();
					BonfireLit();
				}
			});
			UIItemSelectorOverlayController uIItemSelectorOverlayController2 = itemSelector;
			uIItemSelectorOverlayController2.OnHidden = (Action)Delegate.Combine(uIItemSelectorOverlayController2.OnHidden, (Action)delegate
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
				itemSelector = null;
				Interactable = true;
				base.HasChanged = true;
				cameraFollowTarget.SetOffset(Vector3.zero);
				cameraFollowTarget.RemoveTarget(base.gameObject);
			});
		}
		else if (!isDancing)
		{
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 6f);
			StartCoroutine(DanceIE());
		}
		else if (isDancing)
		{
			CancelDance();
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		}
	}

	private IEnumerator DanceIE()
	{
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		isDancing = true;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "dance", true);
	}

	private void CancelDance()
	{
		GameManager.GetInstance().OnConversationEnd();
		isDancing = false;
		if (!StructureInfo.GivenHealth)
		{
			StartCoroutine(CancelDanceIE());
		}
		StructureInfo.GivenHealth = true;
	}

	private IEnumerator CancelDanceIE()
	{
		yield return new WaitForSeconds(1f);
		PlayerFarming.Instance.health.BlueHearts += 4f;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sAddFuel = "<sprite name=\"icon_wood\">";
	}

	public override void GetLabel()
	{
		if (RequiresFuel())
		{
			base.Label = ScriptLocalization.Interactions.AddFuel;
		}
		else if (isDancing)
		{
			base.Label = ScriptLocalization.Interactions.Cancel;
		}
		else
		{
			base.Label = ScriptLocalization.Interactions.JoinDance;
		}
		if (StructureInfo.GivenHealth)
		{
			base.Label = "";
			Interactable = false;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (isLit && !StructureInfo.IsGatheringActive)
		{
			SetImages();
			isLit = false;
			StructureInfo.GivenHealth = false;
			Interactable = true;
		}
		if (isDancing && (!StructureInfo.IsGatheringActive || InputManager.General.GetAnyButton()))
		{
			CancelDance();
		}
	}

	private bool RequiresFuel()
	{
		return StructureInfo.Fuel < StructureInfo.MaxFuel;
	}

	private void BonfireLit()
	{
		StructureInfo.GatheringEndPhase = (int)(TimeManager.CurrentPhase + 2) % 5;
		SetImages();
		isLit = true;
		NotificationCentreScreen.Play(NotificationCentre.NotificationType.FirePitBegan);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.UseFirePit);
		foreach (Follower item in FollowerManager.FollowersAtLocation(Brain.Data.Location))
		{
			if (!FollowerManager.FollowerLocked(item.Brain.Info.ID) && item.Brain.Info.CursedState != Thought.Dissenter && item.Brain.Info.CursedState != Thought.Zombie)
			{
				item.Brain.CompleteCurrentTask();
				item.Brain.HardSwapToTask(new FollowerTask_DanceFirePit(Brain.Data.ID));
			}
		}
	}
}
