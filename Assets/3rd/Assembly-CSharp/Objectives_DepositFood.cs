using System;
using I2.Loc;

[Serializable]
public class Objectives_DepositFood : ObjectivesData
{
	[Serializable]
	public class FinalizedData_DepositFood : ObjectivesDataFinalized
	{
		public override string GetText()
		{
			return ScriptLocalization.Objectives.CookMeals;
		}
	}

	public override string Text
	{
		get
		{
			return ScriptLocalization.Objectives.CookMeals;
		}
	}

	public Objectives_DepositFood()
	{
	}

	public Objectives_DepositFood(string groupId)
		: base(groupId)
	{
		Type = Objectives.TYPES.DEPOSIT_FOOD;
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		return new FinalizedData_DepositFood
		{
			GroupId = GroupId,
			Index = Index,
			UniqueGroupID = UniqueGroupID
		};
	}

	protected override bool CheckComplete()
	{
		bool result = false;
		Structure ofType = Structure.GetOfType(StructureBrain.TYPES.KITCHEN);
		if (ofType != null)
		{
			return ofType.Inventory.Count > 0;
		}
		ofType = Structure.GetOfType(StructureBrain.TYPES.KITCHEN_II);
		if (ofType != null)
		{
			return ofType.Inventory.Count > 0;
		}
		return result;
	}
}
