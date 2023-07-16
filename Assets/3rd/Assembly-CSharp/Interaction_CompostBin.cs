using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class Interaction_CompostBin : Interaction
{
	public List<GameObject> PoopProgress;

	public List<GameObject> GrassProgress;

	public Structure Structure;

	private UIProgressIndicator _uiProgressIndicator;

	private Structures_CompostBin _StructureInfo;

	private bool Activating;

	private Vector3 previousPosition;

	private string sString;

	private string sCollect;

	private string sComposting;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_CompostBin StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_CompostBin;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		UpdateLocalisation();
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
		}
	}

	private void OnBrainAssigned()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		Structures_CompostBin structureBrain = StructureBrain;
		structureBrain.UpdateCompostState = (Action)Delegate.Combine(structureBrain.UpdateCompostState, new Action(UpdateImages));
		UpdateImages();
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		if (StructureBrain != null)
		{
			Structures_CompostBin structureBrain = StructureBrain;
			structureBrain.UpdateCompostState = (Action)Delegate.Remove(structureBrain.UpdateCompostState, new Action(UpdateImages));
		}
	}

	private void UpdateImages()
	{
		if (StructureBrain == null)
		{
			return;
		}
		int num = -1;
		while (++num < PoopProgress.Count)
		{
			if (PoopProgress[num] != null)
			{
				PoopProgress[num].SetActive(false);
			}
		}
		if (StructureBrain.PoopCount > 0 && PoopProgress[PoopProgress.Count - 1] != null)
		{
			PoopProgress[PoopProgress.Count - 1].SetActive(true);
		}
		num = -1;
		while (++num < GrassProgress.Count)
		{
			if (GrassProgress[num] != null)
			{
				GrassProgress[num].SetActive(false);
			}
		}
		if (StructureBrain.CompostCount > 0 && GrassProgress[GrassProgress.Count - 1] != null)
		{
			GrassProgress[GrassProgress.Count - 1].SetActive(true);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_uiProgressIndicator != null)
		{
			_uiProgressIndicator.Recycle();
			_uiProgressIndicator = null;
		}
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = ScriptLocalization.Interactions_Bank.Deposit;
		sCollect = ScriptLocalization.Interactions.Collect;
		sComposting = ScriptLocalization.Interactions.Composting;
		previousPosition = base.transform.position;
	}

	private string GetAffordColor()
	{
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.GRASS) > 0)
		{
			return "<color=#f4ecd3>";
		}
		return "<color=red>";
	}

	public override void GetLabel()
	{
		if (Activating || StructureBrain == null)
		{
			base.Label = "";
		}
		else if (StructureBrain.CompostCount <= 0 && StructureBrain.PoopCount <= 0)
		{
			base.Label = sString + " " + CostFormatter.FormatCost(InventoryItem.ITEM_TYPE.GRASS, StructureBrain.CompostCost);
			Interactable = true;
		}
		else if (StructureBrain.CompostCount > 0 && StructureBrain.PoopCount <= 0)
		{
			base.Label = sComposting;
			Interactable = false;
		}
		else if (StructureBrain.PoopCount > 0)
		{
			base.Label = sCollect + " <sprite name=\"icon_Poop\"> x" + StructureBrain.PoopCount;
			Interactable = true;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (Activating)
		{
			return;
		}
		if (StructureBrain.CompostCount <= 0 && StructureBrain.PoopCount <= 0)
		{
			if (Inventory.GetItemQuantity(35) >= StructureBrain.CompostCost)
			{
				base.OnInteract(state);
				StartCoroutine(DepositGrassRoutine());
			}
			else
			{
				MonoSingleton<Indicator>.Instance.PlayShake();
			}
		}
		else if (StructureBrain.PoopCount > 0)
		{
			StartCoroutine(GivePoopRoutine());
		}
	}

	private IEnumerator DepositGrassRoutine()
	{
		Activating = true;
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= 10)
			{
				break;
			}
			AudioManager.Instance.PlayOneShot("event:/material/footstep_bush", base.transform.position);
			ResourceCustomTarget.Create(base.gameObject, state.transform.position, InventoryItem.ITEM_TYPE.GRASS, null);
			yield return new WaitForSeconds(0.1f);
		}
		StructureBrain.AddGrass();
		Inventory.ChangeItemQuantity(35, -StructureBrain.CompostCost);
		Activating = false;
	}

	private IEnumerator GivePoopRoutine()
	{
		Activating = true;
		int Count = StructureBrain.PoopCount;
		StructureBrain.CollectPoop();
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= Count)
			{
				break;
			}
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.transform.position);
			ResourceCustomTarget.Create(PlayerFarming.Instance.gameObject, base.transform.position, InventoryItem.ITEM_TYPE.POOP, delegate
			{
				Inventory.AddItem(39, 1);
			});
			yield return new WaitForSeconds(0.1f);
		}
		Activating = false;
	}

	private void ClearAll()
	{
		StructureBrain.SetGrass(0);
		StructureBrain.SetPoop(0);
		UpdateImages();
	}

	protected override void Update()
	{
		base.Update();
		if (StructureBrain == null || !(StructureBrain.Data.Progress > 0f))
		{
			return;
		}
		float num = (TimeManager.TotalElapsedGameTime - StructureBrain.Data.Progress) / StructureBrain.COMPOST_DURATION;
		if (_uiProgressIndicator == null)
		{
			_uiProgressIndicator = BiomeConstants.Instance.ProgressIndicatorTemplate.Spawn(BiomeConstants.Instance.transform, GetCurrentUIProgressIndicatorPosition());
			_uiProgressIndicator.Show(num);
			UIProgressIndicator uiProgressIndicator = _uiProgressIndicator;
			uiProgressIndicator.OnHidden = (Action)Delegate.Combine(uiProgressIndicator.OnHidden, (Action)delegate
			{
				_uiProgressIndicator = null;
			});
			return;
		}
		_uiProgressIndicator.SetProgress(num);
		if (num >= 1f)
		{
			StructureBrain.AddPoop();
			StructureBrain.Data.Progress = 0f;
			_uiProgressIndicator.Hide();
		}
		if (StructureInfo.Position != previousPosition)
		{
			_uiProgressIndicator.transform.localPosition = GetCurrentUIProgressIndicatorPosition();
		}
	}

	private Vector3 GetCurrentUIProgressIndicatorPosition()
	{
		return base.transform.position + Vector3.up * 1.5f + Vector3.back * 1.5f - BiomeConstants.Instance.transform.position;
	}
}
