using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class Interaction_OfferingShrine : Interaction
{
	public Structure Structure;

	private Structures_OfferingShrine _StructureInfo;

	public static List<Interaction_OfferingShrine> Shrines = new List<Interaction_OfferingShrine>();

	public AnimationCurve bounceCurve;

	public GameObject EmptyObject;

	public GameObject FullObject;

	public PauseInventoryItem Item;

	private Vector3 IconPosition;

	[SerializeField]
	private Transform _itemTransform;

	private string sCollectOffering;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_OfferingShrine StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_OfferingShrine;
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
		ActivateDistance = 2f;
		base.OnEnableInteraction();
		Shrines.Add(this);
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		ContinuouslyHold = true;
		IconPosition = Item.transform.position;
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		Shrines.Remove(this);
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (StructureBrain != null)
		{
			Structures_OfferingShrine structureBrain = StructureBrain;
			structureBrain.OnCompleteOfferingShrine = (Action<Vector3>)Delegate.Remove(structureBrain.OnCompleteOfferingShrine, new Action<Vector3>(OnCompleteRefining));
		}
	}

	private void OnCompleteRefining(Vector3 FollowerPosition)
	{
		AudioManager.Instance.PlayOneShot("event:/followers/pop_in", FollowerPosition);
		ResourceCustomTarget.Create(Item.gameObject, FollowerPosition, (InventoryItem.ITEM_TYPE)StructureInfo.Inventory[0].type, ShowItem);
	}

	private void OnBrainAssigned()
	{
		Structures_OfferingShrine structureBrain = StructureBrain;
		structureBrain.OnCompleteOfferingShrine = (Action<Vector3>)Delegate.Combine(structureBrain.OnCompleteOfferingShrine, new Action<Vector3>(OnCompleteRefining));
		ShowItem();
	}

	private void ShowItem()
	{
		if (StructureInfo.Inventory.Count <= 0)
		{
			EmptyObject.SetActive(true);
			FullObject.SetActive(false);
		}
		else
		{
			EmptyObject.SetActive(false);
			FullObject.SetActive(true);
			Item.Init((InventoryItem.ITEM_TYPE)StructureInfo.Inventory[0].type, StructureInfo.Inventory[0].quantity);
		}
	}

	private new void Update()
	{
		Item.transform.parent.transform.localPosition = new Vector3(0f, 0.25f * bounceCurve.Evaluate(Time.time * 0.5f % 1f));
		_itemTransform.localScale = base.transform.localScale;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (StructureInfo.Inventory.Count > 0)
		{
			InventoryItem.ITEM_TYPE type = (InventoryItem.ITEM_TYPE)StructureInfo.Inventory[0].type;
			int quantity = StructureInfo.Inventory[0].quantity;
			for (int i = 0; i < quantity; i++)
			{
				InventoryItem.Spawn(type, 1, Item.transform.position, 0f).SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
			}
			StructureInfo.Inventory.Clear();
			StructureInfo.LastPrayTime = TimeManager.TotalElapsedGameTime;
			ShowItem();
			AudioManager.Instance.PlayOneShot("event:/Stings/generic_positive", base.transform.position);
			MMVibrate.Haptic(MMVibrate.HapticTypes.LightImpact);
		}
	}

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sCollectOffering = ScriptLocalization.Interactions.Collect;
	}

	public override void GetLabel()
	{
		if (StructureInfo.Inventory.Count > 0)
		{
			base.Label = sCollectOffering;
		}
		else
		{
			base.Label = "";
		}
	}
}
