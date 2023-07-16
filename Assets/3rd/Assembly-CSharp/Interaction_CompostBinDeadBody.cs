using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class Interaction_CompostBinDeadBody : Interaction
{
	public static List<Interaction_CompostBinDeadBody> DeadBodyCompost = new List<Interaction_CompostBinDeadBody>();

	public List<GameObject> PoopProgress;

	public List<GameObject> GrassProgress;

	public Structure Structure;

	private Structures_DeadBodyCompost _StructureInfo;

	private bool Activating;

	private Vector3 previousPosition;

	private UIProgressIndicator _uiProgressIndicator;

	private string sString;

	private string sCollect;

	private string sComposting;

	private string sPlaceDeadFollower;

	private bool Activated;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_DeadBodyCompost StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_DeadBodyCompost;
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
		ContinuouslyHold = true;
		HasSecondaryInteraction = false;
		UpdateLocalisation();
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
		}
		DeadBodyCompost.Add(this);
	}

	private void OnBrainAssigned()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		Structures_DeadBodyCompost structureBrain = StructureBrain;
		structureBrain.UpdateCompostState = (Action)Delegate.Combine(structureBrain.UpdateCompostState, new Action(UpdateImages));
		UpdateImages();
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		if (StructureBrain != null)
		{
			Structures_DeadBodyCompost structureBrain = StructureBrain;
			structureBrain.UpdateCompostState = (Action)Delegate.Remove(structureBrain.UpdateCompostState, new Action(UpdateImages));
		}
		DeadBodyCompost.Remove(this);
	}

	private void UpdateImages()
	{
		int num = -1;
		while (++num < PoopProgress.Count)
		{
			PoopProgress[num].SetActive(false);
		}
		if (StructureBrain.PoopCount > 0)
		{
			PoopProgress[PoopProgress.Count - 1].SetActive(true);
		}
		num = -1;
		while (++num < GrassProgress.Count)
		{
			GrassProgress[num].SetActive(false);
		}
		if (StructureBrain.CompostCount > 0)
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
		sString = ScriptLocalization.Interactions.DepositCompost;
		sCollect = ScriptLocalization.Interactions.Collect;
		sComposting = ScriptLocalization.Interactions.Composting;
		sPlaceDeadFollower = ScriptLocalization.Interactions.PlaceBodyToCompost;
		previousPosition = base.transform.position;
	}

	public override void GetLabel()
	{
		if (!Activating && StructureBrain.CompostCount > 0 && StructureBrain.PoopCount <= 0)
		{
			base.Label = sComposting;
			Interactable = false;
		}
		else if (!Activating && StructureBrain.PoopCount > 0)
		{
			base.Label = sCollect + " <sprite name=\"icon_Poop\"> x" + StructureBrain.PoopCount;
			Interactable = true;
		}
		else
		{
			base.Label = sPlaceDeadFollower;
			Interactable = false;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (StructureBrain.PoopCount > 0)
		{
			StartCoroutine(GivePoopRoutine());
		}
	}

	public void BuryBody()
	{
		Debug.Log("BURY BDDY!");
		StructureBrain.AddGrass();
	}

	private IEnumerator DepositGrassRoutine()
	{
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= StructureBrain.CompostCost)
			{
				break;
			}
			AudioManager.Instance.PlayOneShot("event:/material/footstep_grass", base.gameObject);
			ResourceCustomTarget.Create(base.gameObject, state.transform.position, InventoryItem.ITEM_TYPE.GRASS, delegate
			{
			});
			yield return new WaitForSeconds(0.05f);
		}
		StructureBrain.AddGrass();
		Inventory.ChangeItemQuantity(35, -StructureBrain.CompostCost);
		Activated = false;
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
			AudioManager.Instance.PlayOneShot("event:/followers/poop_pop", base.gameObject);
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
