using System;
using System.Collections;
using DG.Tweening;
using I2.Loc;
using UnityEngine;

public class Interaction_CollectedResources : Interaction
{
	public GameObject ChestNoItems;

	public GameObject ChestWithItems;

	public Structure structure;

	private Structures_CollectedResourceChest _Structure_Info;

	private bool Activating;

	private Vector3 PunchScale = new Vector3(0.1f, 0.1f, 0.1f);

	public StructuresData StructureInfo
	{
		get
		{
			return structure.Structure_Info;
		}
	}

	public Structures_CollectedResourceChest StructureBrain
	{
		get
		{
			if (_Structure_Info == null)
			{
				_Structure_Info = structure.Brain as Structures_CollectedResourceChest;
			}
			return _Structure_Info;
		}
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		ActivateDistance = 2f;
		if (structure != null && structure.Brain != null)
		{
			if (StructureInfo.Inventory.Count > 0 && ChestNoItems.activeSelf)
			{
				AudioManager.Instance.PlayOneShot("event:/chests/chest_small_open");
			}
			OnBrainAssigned();
		}
		else if ((bool)structure)
		{
			Structure obj = structure;
			obj.OnBrainAssigned = (Action)Delegate.Combine(obj.OnBrainAssigned, new Action(OnBrainAssigned));
		}
	}

	private void OnBrainAssigned()
	{
		UpdateImage();
		Structure obj = structure;
		obj.OnBrainAssigned = (Action)Delegate.Remove(obj.OnBrainAssigned, new Action(OnBrainAssigned));
		StructureBrain brain = structure.Brain;
		brain.OnItemDeposited = (Action)Delegate.Combine(brain.OnItemDeposited, new Action(OnItemDeposited));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (structure != null && structure.Brain != null)
		{
			StructureBrain brain = structure.Brain;
			brain.OnItemDeposited = (Action)Delegate.Remove(brain.OnItemDeposited, new Action(OnItemDeposited));
		}
	}

	private void OnItemDeposited()
	{
		if (!(this == null))
		{
			UpdateImage();
			base.transform.DOKill();
			base.transform.localScale = Vector3.one;
			base.transform.DOPunchScale(new Vector3(0.2f, 0.1f), 1f, 5);
		}
	}

	private void UpdateImage()
	{
		if (StructureInfo == null || StructureInfo.Inventory == null)
		{
			return;
		}
		if (StructureInfo.Inventory.Count > 0)
		{
			if (ChestNoItems != null)
			{
				ChestNoItems.SetActive(false);
			}
			if (ChestWithItems != null)
			{
				ChestWithItems.SetActive(true);
			}
			OutlineTarget = ChestWithItems;
			ActivatorOffset = new Vector3(2.5f, -1f);
			ActivateDistance = 3f;
		}
		else
		{
			if (ChestNoItems != null)
			{
				ChestNoItems.SetActive(true);
			}
			if (ChestWithItems != null)
			{
				ChestWithItems.SetActive(false);
			}
			OutlineTarget = ChestNoItems;
			ActivatorOffset = Vector3.zero;
			ActivateDistance = 2f;
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
	}

	private new void Update()
	{
		AutomaticallyInteract = structure != null && structure.Structure_Info != null && structure.Structure_Info.Inventory.Count > 0;
		Interactable = AutomaticallyInteract;
	}

	public override void GetLabel()
	{
		if (StructureInfo != null)
		{
			if (Activating)
			{
				base.Label = "";
				return;
			}
			string text = (base.Label = ScriptLocalization.Interactions.BaseChest);
			base.Label = text;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activating && structure.Structure_Info.Inventory.Count > 0)
		{
			Activating = true;
			base.OnInteract(state);
			StartCoroutine(GiveResourcesRoutine());
		}
	}

	private IEnumerator GiveResourcesRoutine()
	{
		if (!(structure != null) || structure.Structure_Info == null || structure.Structure_Info.Inventory == null || structure.Structure_Info.Inventory.Count <= 0)
		{
			yield break;
		}
		base.gameObject.transform.DOKill();
		base.gameObject.transform.DOPunchScale(PunchScale, 1f);
		for (int t = structure.Structure_Info.Inventory.Count - 1; t >= 0; t--)
		{
			int Target = Mathf.Min(5, structure.Structure_Info.Inventory[t].quantity);
			for (int i = 0; i < Target; i++)
			{
				ResourceCustomTarget.Create(state.gameObject, base.transform.position, (InventoryItem.ITEM_TYPE)structure.Structure_Info.Inventory[t].type, null);
				AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.transform.position);
				yield return new WaitForSeconds(0.05f);
			}
			Inventory.AddItem(structure.Structure_Info.Inventory[t].type, structure.Structure_Info.Inventory[t].quantity);
		}
		yield return new WaitForSeconds(1f);
		base.gameObject.transform.DOKill();
		base.gameObject.transform.DOPunchScale(PunchScale * 2f, 0.2f, 1);
		AudioManager.Instance.PlayOneShot("event:/chests/chest_small_land", base.transform.position);
		ChestNoItems.SetActive(true);
		ChestWithItems.SetActive(false);
		OutlineTarget = ChestNoItems;
		yield return new WaitForSeconds(1f);
		structure.Structure_Info.Inventory.Clear();
		Activating = false;
	}
}
