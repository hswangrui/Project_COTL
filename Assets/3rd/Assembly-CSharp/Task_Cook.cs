using UnityEngine;

public class Task_Cook : Task
{
	private WorshipperInfoManager wim;

	private WorkPlace workplace;

	private Structure Kitchen;

	private SimpleInventory inventory;

	public Task_Cook()
	{
		Type = Task_Type.COOK;
		SpineSkin = "Cook";
		SpineHatSlot = "Hats/HAT_Che";
	}

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		wim = t.GetComponent<WorshipperInfoManager>();
		workplace = WorkPlace.GetWorkPlaceByID(wim.v_i.WorkPlace);
		Kitchen = workplace.gameObject.GetComponent<Structure>();
		inventory = t.GetComponent<SimpleInventory>();
	}

	public override void ClearTask()
	{
		workplace.EndJob(t, wim.v_i.WorkPlaceSlot);
		if (CurrentTask != null)
		{
			CurrentTask.ClearTask();
		}
		CurrentTask = null;
		base.ClearTask();
	}

	public override void TaskUpdate()
	{
		if (CurrentTask == null)
		{
			if (inventory.GetItemType() == InventoryItem.ITEM_TYPE.MEAT)
			{
				Structure ofType = Structure.GetOfType(StructureBrain.TYPES.COOKED_FOOD_SILO);
				if (ofType != null)
				{
					CurrentTask = new Task_DepositItem();
					CurrentTask.StartTask(t, ofType.gameObject);
					CurrentTask.ParentTask = this;
					return;
				}
			}
			if (Kitchen.Inventory.Count < 3)
			{
				foreach (Structure structure in Structure.Structures)
				{
					if (structure.HasInventoryType(InventoryItem.ITEM_TYPE.WHEAT) && !structure.IsType(StructureBrain.TYPES.KITCHEN) && !structure.IsType(StructureBrain.TYPES.KITCHEN_II))
					{
						CurrentTask = new Task_CollectWheatFromSilo();
						CurrentTask.StartTask(t, Kitchen.gameObject);
						CurrentTask.ParentTask = this;
						return;
					}
				}
			}
			if (Vector3.Distance(workplace.Positions[0].transform.position, t.transform.position) > Farm.FarmTileSize)
			{
				CurrentTask = new Task_ReturnToStation();
				CurrentTask.StartTask(t, workplace.Positions[0].gameObject);
				CurrentTask.ParentTask = this;
			}
			else if (Kitchen.Inventory.Count >= 3 && workplace.HasPower())
			{
				CurrentTask = new Task_DoCooking();
				CurrentTask.StartTask(t, Kitchen.gameObject);
				CurrentTask.ParentTask = this;
				(CurrentTask as Task_DoCooking).workplace = workplace;
			}
		}
		else
		{
			CurrentTask.TaskUpdate();
		}
	}
}
