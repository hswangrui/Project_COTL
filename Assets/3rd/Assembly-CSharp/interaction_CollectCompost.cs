using System.Collections;
using I2.Loc;
using UnityEngine;

public class interaction_CollectCompost : Interaction
{
	public Structure Structure;

	private Structures_CompostBin _StructureInfo;

	private string sCollect;

	private bool Activating;

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
		ContinuouslyHold = true;
		HasSecondaryInteraction = false;
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sCollect = ScriptLocalization.Interactions.Collect;
	}

	public override void GetLabel()
	{
		Interactable = StructureBrain.PoopCount > 0;
		base.Label = (Activating ? string.Empty : string.Join(" ", sCollect, CostFormatter.FormatCost(InventoryItem.ITEM_TYPE.POOP, StructureBrain.PoopCount, true, true)));
	}

	public override void OnInteract(StateMachine state)
	{
		if (StructureBrain.PoopCount > 0)
		{
			base.OnInteract(state);
			StartCoroutine(GivePoopRoutine());
		}
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
			AudioManager.Instance.PlayOneShot("event:/followers/poop_pop", base.transform.position);
			ResourceCustomTarget.Create(state.gameObject, base.transform.position, InventoryItem.ITEM_TYPE.POOP, delegate
			{
				Inventory.AddItem(39, 1);
			});
			yield return new WaitForSeconds(0.1f);
		}
		Activating = false;
	}
}
