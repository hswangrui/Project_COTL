using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using src.UI.Overlays.TutorialOverlay;
using UnityEngine;

public class Interaction_HarvestMeat : Interaction
{
	public static List<Interaction_HarvestMeat> Interaction_HarvestMeats = new List<Interaction_HarvestMeat>();

	public static Interaction_HarvestMeat CurrentMovingBody;

	public Structure structure;

	public MeshRenderer meshRenderer;

	public DeadWorshipper DeadWorshipper;

	public DeadWorshipper DeadWorshipperTmp;

	[SerializeField]
	private SpriteRenderer _indicator;

	private string sHarvestMeat;

	private string sHarvestRottenMeat;

	private string sPrepareForBurial;

	private string sPickup;

	private string sBury;

	private string sCompost;

	private bool CarryingBody;

	private Interaction previousStructure;

	private StructureBrain previousBrain;

	private Grave ClosestGrave;

	private Interaction_Crypt ClosestCrypt;

	private Interaction_CompostBinDeadBody ClosestCompost;

	private float ClosestPosition = 100f;

	private bool FoundOne;

	private bool FoundCompost;

	private bool NearGraveWithBody;

	private bool NearCompostWithBody;

	public bool Rotten
	{
		get
		{
			if (DeadWorshipper == null || DeadWorshipper.StructureInfo == null)
			{
				return false;
			}
			return DeadWorshipper.StructureInfo.Rotten;
		}
	}

	private void Start()
	{
		if (DeadWorshipper.StructureInfo != null && DeadWorshipper.StructureInfo.BodyWrapped)
		{
			if (DeadWorshipper.followerInfo != null && DeadWorshipper.followerInfo.Necklace != 0)
			{
				HasSecondaryInteraction = true;
			}
			else
			{
				HasSecondaryInteraction = false;
			}
		}
		else
		{
			HasSecondaryInteraction = true;
		}
		UpdateLocalisation();
		CarryingBody = false;
		Structure obj = structure;
		obj.OnBrainAssigned = (Action)Delegate.Combine(obj.OnBrainAssigned, new Action(OnBrainAssigned));
		if (structure.Brain != null)
		{
			OnBrainAssigned();
		}
	}

	private void OnBrainAssigned()
	{
		Structure obj = structure;
		obj.OnBrainAssigned = (Action)Delegate.Remove(obj.OnBrainAssigned, new Action(OnBrainAssigned));
		DeleteDuplicateBodies();
	}

	private void DeleteDuplicateBodies()
	{
		for (int num = Interaction_HarvestMeats.Count - 1; num >= 0; num--)
		{
			if (Interaction_HarvestMeats[num] != null && Interaction_HarvestMeats[num].structure != null && Interaction_HarvestMeats[num].structure.Brain != null && DeadWorshipper != null && DeadWorshipper.followerInfo != null && Interaction_HarvestMeats[num] != this && Interaction_HarvestMeats[num].DeadWorshipper != null && Interaction_HarvestMeats[num].DeadWorshipper.followerInfo != null && Interaction_HarvestMeats[num].DeadWorshipper.followerInfo.ID == DeadWorshipper.followerInfo.ID)
			{
				StructureManager.RemoveStructure(Interaction_HarvestMeats[num].structure.Brain);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)structure)
		{
			Structure obj = structure;
			obj.OnBrainAssigned = (Action)Delegate.Remove(obj.OnBrainAssigned, new Action(OnBrainAssigned));
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sHarvestMeat = ScriptLocalization.Interactions.HarvestMeat;
		sHarvestRottenMeat = ScriptLocalization.Interactions.HarvestRottenMeat;
		sPrepareForBurial = ScriptLocalization.Interactions.PrepareForBurial;
		sPickup = ScriptLocalization.Interactions.PickUp;
		sBury = ScriptLocalization.Interactions.BuryBody;
		sCompost = ScriptLocalization.Interactions.CompostBody;
	}

	public override void GetLabel()
	{
		if (!NearGraveWithBody && !NearCompostWithBody)
		{
			if (DeadWorshipper != null && DeadWorshipper.StructureInfo != null && DeadWorshipper.StructureInfo.BodyWrapped)
			{
				base.Label = sPickup;
			}
			else
			{
				base.Label = sPrepareForBurial;
			}
		}
		else if (NearGraveWithBody)
		{
			base.Label = sBury;
		}
		else if (NearCompostWithBody)
		{
			base.Label = sCompost;
		}
	}

	public override void GetSecondaryLabel()
	{
		if (DeadWorshipper.StructureInfo != null)
		{
			if (DeadWorshipper.StructureInfo.BodyWrapped)
			{
				if (DeadWorshipper.followerInfo != null && DeadWorshipper.followerInfo.Necklace != 0)
				{
					SecondaryInteractable = true;
					base.SecondaryLabel = ScriptLocalization.Interactions.TakeLoot;
				}
				else
				{
					SecondaryInteractable = false;
					base.SecondaryLabel = "";
				}
			}
			else if (DeadWorshipper.StructureInfo.Rotten)
			{
				base.SecondaryLabel = sHarvestRottenMeat;
			}
			else
			{
				base.SecondaryLabel = sHarvestMeat;
			}
		}
		else
		{
			base.SecondaryLabel = "";
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (!DataManager.Instance.OnboardedDeadFollower)
		{
			if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.DeadFollower))
			{
				UITutorialOverlayController uITutorialOverlayController = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.DeadFollower);
				uITutorialOverlayController.OnHidden = (Action)Delegate.Combine(uITutorialOverlayController.OnHidden, (Action)delegate
				{
					if (!UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_BodyPit))
					{
						ObjectiveManager.Add(new Objectives_UnlockUpgrade("Objectives/GroupTitles/BuryDead", UpgradeSystem.Type.Building_BodyPit));
					}
					ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/BuryDead", Objectives.CustomQuestTypes.BuryBody));
				});
			}
			DataManager.Instance.OnboardedDeadFollower = true;
		}
		else
		{
			if (CarryingBody)
			{
				return;
			}
			base.OnInteract(state);
			if (!DeadWorshipper.StructureInfo.BodyWrapped)
			{
				StartCoroutine(PrepareForBurial());
			}
			else
			{
				DeadWorshipperTmp = DeadWorshipper;
				Debug.Log("PICKUP! DeadWorshipperTmp " + DeadWorshipperTmp.StructureInfo.FollowerID);
				structure.enabled = false;
				DeadWorshipper.enabled = false;
				StartCoroutine(PickUpBody());
			}
			foreach (Structures_Prison item in StructureManager.GetAllStructuresOfType<Structures_Prison>())
			{
				if (item.Data.FollowerID == DeadWorshipper.StructureInfo.FollowerID)
				{
					item.Data.FollowerID = -1;
				}
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Interaction_HarvestMeats.Add(this);
	}

	public override void OnDisableInteraction()
	{
		if (CarryingBody && this.structure != null && PlayerFarming.Instance != null)
		{
			Debug.Log("Carrying body! ");
			Debug.Log("Drop body ID: " + DeadWorshipperTmp.StructureInfo.FollowerID);
			StructuresData structure = StructuresData.GetInfoByType(StructureBrain.TYPES.DEAD_WORSHIPPER, 0);
			structure.BodyWrapped = true;
			structure.FollowerID = DeadWorshipperTmp.StructureInfo.FollowerID;
			CarryingBody = false;
			StructureManager.BuildStructure(FollowerLocation.Base, structure, PlayerFarming.Instance.transform.position, Vector2Int.one, false, delegate(GameObject g)
			{
				DeadWorshipper component = g.GetComponent<DeadWorshipper>();
				component.WrapBody();
				component.Setup();
				PlacementRegion.TileGridTile closestTileGridTileAtWorldPosition = PlacementRegion.Instance.GetClosestTileGridTileAtWorldPosition(structure.Position);
				if (closestTileGridTileAtWorldPosition != null && closestTileGridTileAtWorldPosition.CanPlaceStructure)
				{
					component.Structure.Brain.AddToGrid(closestTileGridTileAtWorldPosition.Position);
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}, delegate
			{
				UnityEngine.Object.Destroy(base.gameObject);
			});
		}
		Interaction_HarvestMeats.Remove(this);
	}

	public override void OnBecomeCurrent()
	{
		base.OnBecomeCurrent();
		if (!DeadWorshipper.StructureInfo.BodyWrapped)
		{
			string deathText = DeadWorshipper.followerInfo.GetDeathText();
			if (!string.IsNullOrEmpty(deathText))
			{
				MonoSingleton<Indicator>.Instance.ShowTopInfo(deathText);
			}
		}
	}

	public override void OnBecomeNotCurrent()
	{
		base.OnBecomeNotCurrent();
		MonoSingleton<Indicator>.Instance.HideTopInfo();
	}

	private new void Update()
	{
		if (!CarryingBody)
		{
			return;
		}
		FoundOne = false;
		FoundCompost = false;
		NearGraveWithBody = false;
		NearCompostWithBody = false;
		PlayerFarming.Instance.NearGrave = false;
		PlayerFarming.Instance.NearStructure = null;
		PlayerFarming.Instance.NearCompostBody = false;
		ClosestPosition = 100f;
		ClosestGrave = null;
		ClosestCrypt = null;
		foreach (Grave grafe in Grave.Graves)
		{
			float num = Vector3.Distance(grafe.gameObject.transform.position, PlayerFarming.Instance.gameObject.transform.position);
			if (!(num < 0.45f) || grafe.StructureInfo.FollowerID != -1)
			{
				continue;
			}
			if (num < ClosestPosition)
			{
				ClosestPosition = num;
				ClosestGrave = grafe;
				if (previousStructure != ClosestGrave)
				{
					Outliner.OutlineLayers[0].Clear();
					Outliner.OutlineLayers[0].Add(ClosestGrave.gameObject);
				}
			}
			FoundOne = true;
			ClosestGrave = grafe;
		}
		if (!FoundOne)
		{
			ClosestPosition = 100f;
			foreach (Interaction_Crypt crypt in Interaction_Crypt.Crypts)
			{
				float num2 = Vector3.Distance(crypt.gameObject.transform.position, PlayerFarming.Instance.gameObject.transform.position);
				if (!(num2 < 1f))
				{
					continue;
				}
				if (num2 < ClosestPosition)
				{
					ClosestPosition = num2;
					ClosestCrypt = crypt;
					if (previousStructure != ClosestCrypt && !crypt.structureBrain.IsFull)
					{
						Outliner.OutlineLayers[0].Clear();
						Outliner.OutlineLayers[0].Add(ClosestCrypt.gameObject);
					}
				}
				FoundOne = true;
				ClosestCrypt = crypt;
			}
		}
		if ((ClosestGrave == null || previousBrain != ClosestGrave.structureBrain) && (ClosestCrypt == null || previousBrain != ClosestCrypt.structureBrain) && previousStructure != null)
		{
			if (previousStructure is Interaction_Crypt)
			{
				((Interaction_Crypt)previousStructure).SetDoors(false);
			}
			Outliner.OutlineLayers[0].Clear();
			Outliner.OutlineLayers[0].Remove(previousStructure.gameObject);
			previousStructure = null;
			previousBrain = null;
		}
		if (FoundOne)
		{
			PlayerFarming.Instance.NearGrave = true;
			PlayerFarming.Instance.NearStructure = ((ClosestGrave != null) ? ((StructureBrain)ClosestGrave.structureBrain) : ((StructureBrain)ClosestCrypt.structureBrain));
			previousBrain = PlayerFarming.Instance.NearStructure;
			previousStructure = ((ClosestGrave != null) ? ((Interaction)ClosestGrave) : ((Interaction)ClosestCrypt));
			NearGraveWithBody = true;
			GetLabel();
			if (ClosestCrypt != null && PlayerFarming.Instance.NearStructure == ClosestCrypt.structureBrain && !ClosestCrypt.structureBrain.IsFull)
			{
				ClosestCrypt.SetDoors(true);
			}
		}
		else
		{
			if (ClosestGrave != null)
			{
				Outliner.OutlineLayers[0].Clear();
			}
			ClosestPosition = 100f;
			foreach (Interaction_CompostBinDeadBody item in Interaction_CompostBinDeadBody.DeadBodyCompost)
			{
				float num3 = Vector3.Distance(item.transform.position, PlayerFarming.Instance.transform.position);
				Debug.Log(num3);
				if (num3 < 1.5f && item.StructureBrain.CompostCount <= 0 && item.StructureBrain.PoopCount <= 0)
				{
					Debug.Log("1 " + item);
					if (num3 < ClosestPosition)
					{
						ClosestPosition = num3;
						ClosestCompost = item;
						FoundCompost = true;
						previousStructure = item;
						Outliner.OutlineLayers[0].Clear();
						Outliner.OutlineLayers[0].Add(item.gameObject);
					}
				}
			}
			if (FoundCompost)
			{
				PlayerFarming.Instance.NearCompostBody = true;
				NearCompostWithBody = true;
				GetLabel();
			}
			else if (previousStructure != null)
			{
				Outliner.OutlineLayers[0].Clear();
				Outliner.OutlineLayers[0].Remove(previousStructure.gameObject);
				previousStructure = null;
			}
		}
		if (PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.InActive)
		{
			DropBody();
		}
	}

	private IEnumerator PickUpBody()
	{
		DeadWorshipper.StructureInfo.Animation = "dead";
		CurrentMovingBody = this;
		_indicator.DOColor(Color.black, 0.5f);
		CarryingBody = true;
		AudioManager.Instance.PlayOneShot("event:/player/body_pickup", base.gameObject);
		PlayerFarming.Instance.CarryingDeadFollowerID = DeadWorshipperTmp.StructureInfo.FollowerID;
		bool wasGoopDoorOpen = BaseGoopDoor.Instance.IsOpen;
		BaseGoopDoor.Instance.DoorUp();
		base.Label = ScriptLocalization.Interactions.Drop;
		meshRenderer.gameObject.SetActive(false);
		_indicator.gameObject.SetActive(false);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle_CarryingBody;
		while (!InputManager.Gameplay.GetInteractButtonUp())
		{
			yield return null;
		}
		while (!InputManager.Gameplay.GetInteractButtonHeld() || MonoSingleton<UIManager>.Instance.MenusBlocked || Time.deltaTime <= 0f)
		{
			yield return null;
		}
		PlacementRegion.TileGridTile closestTileGridTileAtWorldPosition = PlacementRegion.Instance.GetClosestTileGridTileAtWorldPosition(DeadWorshipper.transform.position);
		if (closestTileGridTileAtWorldPosition != null && closestTileGridTileAtWorldPosition.ObjectOnTile == StructureBrain.TYPES.DEAD_WORSHIPPER)
		{
			DeadWorshipper.Structure.Brain.RemoveFromGrid(closestTileGridTileAtWorldPosition.Position);
		}
		StructureManager.RemoveStructure(DeadWorshipper.Structure.Brain);
		CurrentMovingBody = null;
		if (FoundOne)
		{
			bool flag = true;
			if ((bool)ClosestGrave)
			{
				ClosestGrave.OutlineEffect.OutlineLayers[0].Clear();
				ClosestGrave.OutlineEffect.RemoveGameObject(ClosestGrave.OutlineTarget);
				ClosestGrave.StructureInfo.FollowerID = DeadWorshipperTmp.StructureInfo.FollowerID;
				ClosestGrave.SetGameObjects();
				foreach (FollowerInfo item in DataManager.Instance.Followers_Dead)
				{
					if (item.ID == DeadWorshipperTmp.StructureInfo.FollowerID)
					{
						item.LastPosition = ClosestGrave.structureBrain.Data.Position;
						break;
					}
				}
				BiomeConstants.Instance.EmitSmokeExplosionVFX(ClosestGrave.transform.position);
			}
			else if ((bool)ClosestCrypt)
			{
				if (!ClosestCrypt.structureBrain.IsFull)
				{
					ClosestCrypt.OutlineEffect.OutlineLayers[0].Clear();
					ClosestCrypt.OutlineEffect.RemoveGameObject(ClosestCrypt.OutlineTarget);
					ClosestCrypt.structureBrain.DepositBody(DeadWorshipperTmp.StructureInfo.FollowerID);
					foreach (FollowerInfo item2 in DataManager.Instance.Followers_Dead)
					{
						if (item2.ID == DeadWorshipperTmp.StructureInfo.FollowerID)
						{
							item2.LastPosition = ClosestCrypt.structureBrain.Data.Position;
							break;
						}
					}
					ClosestCrypt.SetDoors(false);
					BiomeConstants.Instance.EmitSmokeExplosionVFX(ClosestCrypt.transform.position);
				}
				else
				{
					flag = false;
				}
			}
			if (flag)
			{
				AudioManager.Instance.PlayOneShot("event:/player/body_drop_grave", base.gameObject);
				if (!DeadWorshipper.followerInfo.HasBeenBuried)
				{
					CultFaithManager.AddThought(Thought.Cult_FollowerBuried, PlayerFarming.Instance.CarryingDeadFollowerID, 1f);
				}
				DeadWorshipper.followerInfo.HasBeenBuried = true;
				PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
				CarryingBody = false;
				PlayerFarming.Instance.CarryingDeadFollowerID = -1;
				ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.BuryBody);
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				DropBody();
			}
		}
		else if (FoundCompost)
		{
			ClosestCompost.BuryBody();
			BiomeConstants.Instance.EmitSmokeExplosionVFX(ClosestCompost.transform.position);
			AudioManager.Instance.PlayOneShot("event:/player/body_drop_grave", base.gameObject);
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
			CarryingBody = false;
			PlayerFarming.Instance.CarryingDeadFollowerID = -1;
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.BuryBody);
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			DropBody();
		}
		if (wasGoopDoorOpen)
		{
			BaseGoopDoor.Instance.DoorDown();
		}
	}

	public void DropBody()
	{
		CurrentMovingBody = null;
		if (!CarryingBody)
		{
			return;
		}
		AudioManager.Instance.PlayOneShot("event:/player/body_drop", base.gameObject);
		StructuresData infoByType = StructuresData.GetInfoByType(StructureBrain.TYPES.DEAD_WORSHIPPER, 0);
		infoByType.BodyWrapped = true;
		infoByType.FollowerID = DeadWorshipperTmp.StructureInfo.FollowerID;
		CarryingBody = false;
		StructureManager.BuildStructure(FollowerLocation.Base, infoByType, PlayerFarming.Instance.transform.position, Vector2Int.one, false, delegate(GameObject g)
		{
			DeadWorshipper component = g.GetComponent<DeadWorshipper>();
			component.WrapBody();
			component.Setup();
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
			PlayerFarming.Instance.CarryingDeadFollowerID = -1;
			PlacementRegion.TileGridTile closestTileGridTileAtWorldPosition = PlacementRegion.Instance.GetClosestTileGridTileAtWorldPosition(component.transform.position);
			if (closestTileGridTileAtWorldPosition != null && closestTileGridTileAtWorldPosition.CanPlaceStructure)
			{
				component.Structure.Brain.AddToGrid(closestTileGridTileAtWorldPosition.Position);
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}, delegate
		{
			UnityEngine.Object.Destroy(base.gameObject);
		});
	}

	private IEnumerator PrepareForBurial()
	{
		MonoSingleton<Indicator>.Instance.HideTopInfo();
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().AddPlayerToCamera();
		yield return new WaitForEndOfFrame();
		AudioManager.Instance.PlayOneShot("event:/player/body_wrap", base.gameObject);
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 6f);
		yield return new WaitForSeconds(2f);
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.4f, 0.3f);
		AudioManager.Instance.PlayOneShot("event:/player/body_wrap_done", base.gameObject);
		DeadWorshipper.WrapBody();
		state.CURRENT_STATE = StateMachine.State.InActive;
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationEnd();
		state.CURRENT_STATE = StateMachine.State.Idle;
		base.HasChanged = true;
	}

	public override void OnSecondaryInteract(StateMachine state)
	{
		if (!DataManager.Instance.OnboardedDeadFollower)
		{
			if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.DeadFollower))
			{
				UITutorialOverlayController uITutorialOverlayController = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.DeadFollower);
				uITutorialOverlayController.OnHidden = (Action)Delegate.Combine(uITutorialOverlayController.OnHidden, (Action)delegate
				{
					if (!UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_BodyPit))
					{
						ObjectiveManager.Add(new Objectives_UnlockUpgrade("Objectives/GroupTitles/BuryDead", UpgradeSystem.Type.Building_BodyPit));
					}
					ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/BuryDead", Objectives.CustomQuestTypes.BuryBody));
				});
			}
			DataManager.Instance.OnboardedDeadFollower = true;
		}
		else if (!DeadWorshipper.StructureInfo.BodyWrapped)
		{
			base.OnSecondaryInteract(state);
			StartCoroutine(HarvestMeatIE());
		}
		else if (DeadWorshipper.followerInfo != null && DeadWorshipper.followerInfo.Necklace != 0)
		{
			base.OnSecondaryInteract(state);
			StartCoroutine(LootBody());
		}
	}

	private IEnumerator HarvestMeatIE()
	{
		MonoSingleton<Indicator>.Instance.HideTopInfo();
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().AddPlayerToCamera();
		PlayerFarming.Instance.GoToAndStop(base.transform.position, base.gameObject);
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		AudioManager.Instance.PlayOneShot("event:/player/harvest_meat", base.gameObject);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		PlayerFarming.Instance.CustomAnimation("actions/harvest_meat", true);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 6f);
		yield return new WaitForSeconds(2f);
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.4f, 0.3f);
		meshRenderer.enabled = false;
		AudioManager.Instance.PlayOneShot("event:/player/harvest_meat_done", base.gameObject);
		InventoryItem.Spawn(InventoryItem.ITEM_TYPE.FOLLOWER_MEAT, 5, base.transform.position + Vector3.back * 0.5f);
		InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BONE, 2, base.transform.position + Vector3.back * 0.5f);
		if (DeadWorshipper.followerInfo.Necklace != 0)
		{
			InventoryItem.Spawn(DeadWorshipper.followerInfo.Necklace, 1, base.transform.position + Vector3.back * 0.5f);
		}
		DeadWorshipper.followerInfo.Necklace = InventoryItem.ITEM_TYPE.NONE;
		state.CURRENT_STATE = StateMachine.State.InActive;
		yield return new WaitForSeconds(0.5f);
		DataManager.Instance.TotalBodiesHarvested++;
		if (DataManager.Instance.TotalBodiesHarvested >= 5 && !DataManager.Instance.PlayerFoundRelics.Contains(RelicType.SpawnCombatFollowerFromBodies) && DataManager.Instance.OnboardedRelics)
		{
			bool waiting = true;
			RelicCustomTarget.Create(base.transform.position, base.transform.position - Vector3.forward, 1.5f, RelicType.SpawnCombatFollowerFromBodies, delegate
			{
				waiting = false;
			});
			while (waiting)
			{
				yield return null;
			}
		}
		GameManager.GetInstance().OnConversationEnd();
		state.CURRENT_STATE = StateMachine.State.Idle;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.Location == PlayerFarming.Location)
			{
				if (allBrain.CurrentTaskType == FollowerTaskType.Sleep || allBrain.CurrentTaskType == FollowerTaskType.SleepBedRest)
				{
					allBrain.AddThought(Thought.SleptThroughYouButcheringDeadFollower);
				}
				else if (allBrain.HasTrait(FollowerTrait.TraitType.Cannibal))
				{
					allBrain.AddThought(Thought.SawYouButcheringDeadFollowerCannibalTrait);
				}
				else
				{
					allBrain.AddThought(Thought.SawYouButcheringDeadFollower);
				}
			}
		}
		JudgementMeter.ShowModify(-1);
		ObjectiveManager.FailCustomObjective(Objectives.CustomQuestTypes.BuryBody);
		if (!TimeManager.IsNight)
		{
			CultFaithManager.AddThought(Thought.Cult_ButcheredFollowerMeat, -1, (!DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.Cannibal)) ? 1 : 0);
		}
		StructureManager.RemoveStructure(DeadWorshipper.Structure.Brain);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private IEnumerator LootBody()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().AddPlayerToCamera();
		yield return new WaitForEndOfFrame();
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 6f);
		yield return new WaitForSeconds(2f);
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.4f, 0.3f);
		InventoryItem.Spawn(DeadWorshipper.followerInfo.Necklace, 1, base.transform.position + Vector3.back * 0.5f);
		RemoveTraitGaveByItem();
		DeadWorshipper.followerInfo.Necklace = InventoryItem.ITEM_TYPE.NONE;
		DeadWorshipper.SetOutfit();
		state.CURRENT_STATE = StateMachine.State.InActive;
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationEnd();
		state.CURRENT_STATE = StateMachine.State.Idle;
		CultFaithManager.AddThought(Thought.Cult_LootedCorpse, -1, 1f);
	}

	private void RemoveTraitGaveByItem()
	{
		InventoryItem.ITEM_TYPE necklace = DeadWorshipper.followerInfo.Necklace;
		if (necklace == InventoryItem.ITEM_TYPE.Necklace_Gold_Skull && FollowerManager.DeathCatID != DeadWorshipper.followerInfo.ID)
		{
			DeadWorshipper.followerInfo.Traits.Remove(FollowerTrait.TraitType.Immortal);
		}
	}
}
