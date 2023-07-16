using System;
using I2.Loc;
using UnityEngine;

[Serializable]
public class Objectives_CollectItem : ObjectivesData
{
	[Serializable]
	public class FinalizedData_CollectItem : ObjectivesDataFinalized
	{
		public InventoryItem.ITEM_TYPE ItemType;

		public FollowerLocation TargetLocation;

		public string LocKey;

		public int Target;

		public int Count;

		public string CustomTerm;

		public override string GetText()
		{
			if (!string.IsNullOrEmpty(CustomTerm))
			{
				return LocalizationManager.GetTranslation(CustomTerm) + string.Format(" {0}/{1}", Count, Target);
			}
			if (TargetLocation == FollowerLocation.Base)
			{
				return string.Format(LocalizationManager.GetTranslation(LocKey), InventoryItem.Name(ItemType) + FontImageNames.GetIconByType(ItemType), Count, Target);
			}
			return string.Format(LocalizationManager.GetTranslation(LocKey), InventoryItem.Name(ItemType), FontImageNames.GetIconByType(ItemType), LocalizationManager.GetTranslation(string.Format("NAMES/Places/{0}", TargetLocation)), Count, Target);
		}
	}

	public InventoryItem.ITEM_TYPE ItemType;

	public FollowerLocation TargetLocation;

	public int Target;

	public int StartingAmount = -1;

	public int Count;

	private bool countIsTotal;

	public string CustomTerm = "";

	public override string Text
	{
		get
		{
			int value = (countIsTotal ? Count : (Count - ((StartingAmount != -1) ? StartingAmount : 0)));
			value = Mathf.Clamp(value, 0, Target);
			if (!string.IsNullOrEmpty(CustomTerm))
			{
				return LocalizationManager.GetTranslation(CustomTerm) + string.Format(" {0}/{1}", value, Target);
			}
			if (TargetLocation == FollowerLocation.Base)
			{
				return string.Format(ScriptLocalization.Objectives.CollectItem, InventoryItem.Name(ItemType) + FontImageNames.GetIconByType(ItemType), value, Target);
			}
			return string.Format(ScriptLocalization.Objectives.CollectItemFromDungeon, InventoryItem.Name(ItemType), FontImageNames.GetIconByType(ItemType), LocalizationManager.GetTranslation(string.Format("NAMES/Places/{0}", TargetLocation)), value, Target);
		}
	}

	public Objectives_CollectItem()
	{
	}

	public Objectives_CollectItem(string groupId, InventoryItem.ITEM_TYPE itemType, int target, bool targetIsTotal = true, FollowerLocation targetLocation = FollowerLocation.Base, float expireTimestamp = -1f)
		: base(groupId, expireTimestamp)
	{
		Type = Objectives.TYPES.COLLECT_ITEM;
		TargetLocation = targetLocation;
		ItemType = itemType;
		Target = target;
		countIsTotal = targetIsTotal;
	}

	public override void Init(bool initialAssigning)
	{
		if (!initialised)
		{
			Inventory.OnItemAddedToInventory += OnItemAddedToInventory;
		}
		if (initialAssigning)
		{
			Count = Inventory.GetItemQuantity((int)ItemType);
			if (!countIsTotal)
			{
				StartingAmount = Inventory.GetItemQuantity((int)ItemType);
			}
		}
		base.Init(initialAssigning);
		ObjectiveManager.CheckObjectives(Type);
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		int value = (countIsTotal ? Count : (Count - StartingAmount));
		value = Mathf.Clamp(value, 0, Target);
		return new FinalizedData_CollectItem
		{
			GroupId = GroupId,
			Index = Index,
			ItemType = ItemType,
			LocKey = ((TargetLocation == FollowerLocation.Base) ? "Objectives/CollectItem" : "Objectives/CollectItemFromDungeon"),
			Target = Target,
			Count = value,
			TargetLocation = TargetLocation,
			UniqueGroupID = UniqueGroupID,
			CustomTerm = CustomTerm
		};
	}

	private void OnItemAddedToInventory(InventoryItem.ITEM_TYPE ItemType, int Delta)
	{
		if (ItemType == this.ItemType && (TargetLocation == FollowerLocation.Base || PlayerFarming.Location == TargetLocation))
		{
			Count += Delta;
		}
	}

	protected override bool CheckComplete()
	{
		int num = Count;
		if (StartingAmount != -1)
		{
			num -= StartingAmount;
		}
		return num >= Target;
	}

	public override void Complete()
	{
		base.Complete();
		Inventory.OnItemAddedToInventory -= OnItemAddedToInventory;
	}

	public override void Failed()
	{
		base.Failed();
		Inventory.OnItemAddedToInventory -= OnItemAddedToInventory;
	}
}
