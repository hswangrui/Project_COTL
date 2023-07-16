using System.Collections.Generic;
using UnityEngine;

public class Task_Farmer : Task
{
	private WorshipperInfoManager wim;

	private WorkPlace workplace;

	public List<Crop> Crops = new List<Crop>();

	public Task_Farmer()
	{
		Type = Task_Type.FARMER;
		SpineSkin = "Farmer";
		SpineHatSlot = "Hats/HAT_Farm";
	}

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		wim = t.GetComponent<WorshipperInfoManager>();
		workplace = WorkPlace.GetWorkPlaceByID(wim.v_i.WorkPlace);
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
			if (workplace.HasPower())
			{
				if (Crops.Count <= 0)
				{
					CreateCropList();
				}
				else
				{
					CurrentTask = new Task_WaterCrops();
					CurrentTask.StartTask(t, null);
					CurrentTask.ParentTask = this;
					(CurrentTask as Task_WaterCrops).workplace = workplace;
				}
			}
			if (Vector3.Distance(workplace.Positions[wim.v_i.WorkPlaceSlot].transform.position, t.transform.position) > Farm.FarmTileSize)
			{
				CurrentTask = new Task_ReturnToStation();
				CurrentTask.StartTask(t, workplace.Positions[wim.v_i.WorkPlaceSlot].gameObject);
				CurrentTask.ParentTask = this;
			}
		}
		else
		{
			CurrentTask.TaskUpdate();
		}
	}

	private void CreateCropList()
	{
		foreach (Crop crop in Crop.Crops)
		{
			if (!crop.WorkCompleted)
			{
				Crops.Add(crop);
			}
		}
	}
}
