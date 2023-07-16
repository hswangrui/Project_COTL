using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Interaction_FoodStorage : Interaction
{
	public static List<Interaction_FoodStorage> FoodStorages = new List<Interaction_FoodStorage>();

	public Structure Structure;

	private Structures_FoodStorage _StructureInfo;

	public Canvas CapacityIndicatorCanvas;

	public Image CapacityIndicator;

	public GameObject FoodIndicatorPrefab;

	public SpriteRenderer RangeSprite;

	public InventoryItemDisplay[] itemDisplays;

	private bool showing;

	private GameObject _player;

	private List<InventoryItem> FoodInTheAir;

	private Color FadeOut = new Color(1f, 1f, 1f, 0f);

	private float DistanceRadius = 1f;

	private float Distance = 1f;

	private int FrameIntervalOffset;

	private int UpdateInterval = 2;

	private bool distanceChanged;

	private Vector3 _updatePos;

	private float CurrentCapacity;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_FoodStorage StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_FoodStorage;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		FoodStorages.Remove(this);
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		if (GetComponentInParent<PlacementObject>() == null)
		{
			RangeSprite.DOColor(FadeOut, 0f).SetUpdate(true);
		}
		UpdateLocalisation();
		CapacityIndicatorCanvas.gameObject.SetActive(false);
		InventoryItemDisplay[] array = itemDisplays;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(false);
		}
		FoodInTheAir = new List<InventoryItem>();
		FoodStorages.Add(this);
		if (StructureInfo != null)
		{
			UpdateFoodDisplayed();
		}
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
		}
	}

	public override void OnBecomeCurrent()
	{
		base.OnBecomeCurrent();
		UpdateCapacityIndicator();
	}

	private void OnBrainAssigned()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		Structures_FoodStorage structureBrain = StructureBrain;
		structureBrain.OnItemDeposited = (Action)Delegate.Combine(structureBrain.OnItemDeposited, new Action(OnFoodWithdrawn));
		Structures_FoodStorage structureBrain2 = StructureBrain;
		structureBrain2.OnFoodWithdrawn = (Action)Delegate.Combine(structureBrain2.OnFoodWithdrawn, new Action(OnFoodWithdrawn));
		UpdateFoodDisplayed();
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		if (StructureBrain != null)
		{
			Structures_FoodStorage structureBrain = StructureBrain;
			structureBrain.OnItemDeposited = (Action)Delegate.Remove(structureBrain.OnItemDeposited, new Action(OnFoodWithdrawn));
			Structures_FoodStorage structureBrain2 = StructureBrain;
			structureBrain2.OnFoodWithdrawn = (Action)Delegate.Remove(structureBrain2.OnFoodWithdrawn, new Action(OnFoodWithdrawn));
		}
	}

	private void OnFoodWithdrawn()
	{
		UpdateCapacityIndicator();
	}

	private void UpdateCapacityIndicator()
	{
		CurrentCapacity = 0f;
		foreach (InventoryItem item in StructureInfo.Inventory)
		{
			CurrentCapacity += item.quantity;
		}
		CapacityIndicator.fillAmount = CurrentCapacity / (float)StructureBrain.Capacity;
		UpdateFoodDisplayed();
	}

	private float GetFoodCount()
	{
		float num = 0f;
		foreach (InventoryItem item in StructureInfo.Inventory)
		{
			InventoryItem inventoryItem = item;
			num += 1f;
		}
		return num / 10f;
	}

	private float GetFoodAndAirCount()
	{
		CurrentCapacity = 0f;
		foreach (InventoryItem item in StructureInfo.Inventory)
		{
			CurrentCapacity += item.quantity;
		}
		return CurrentCapacity + (float)FoodInTheAir.Count;
	}

	public void UpdateFoodDisplayed()
	{
		InventoryItemDisplay[] array = itemDisplays;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(false);
		}
		for (int j = 0; j < StructureInfo.Inventory.Count; j++)
		{
			if (j < itemDisplays.Length)
			{
				itemDisplays[j].gameObject.SetActive(true);
				itemDisplays[j].SetImage((InventoryItem.ITEM_TYPE)StructureInfo.Inventory[j].type, false);
			}
		}
	}
}
