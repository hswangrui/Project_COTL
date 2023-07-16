using UnityEngine;

internal class Task_WaterCrops : Task
{
	public WorkPlace workplace;

	private Crop crop;

	private float WateringDuration = 4f;

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
	}

	public override void TaskUpdate()
	{
		if (!workplace.HasPower())
		{
			if (ParentTask != null)
			{
				ParentTask.ClearCurrentTask();
			}
			return;
		}
		if (TargetObject == null)
		{
			GetCrop();
			if (TargetObject == null && ParentTask != null)
			{
				ParentTask.ClearCurrentTask();
			}
			return;
		}
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			Timer = 0f;
			PathToPosition(TargetObject.gameObject.transform.position);
			break;
		case StateMachine.State.Moving:
			if (Vector3.Distance(t.transform.position, TargetObject.transform.position) <= Farm.FarmTileSize)
			{
				Timer = 0f;
				state.CURRENT_STATE = StateMachine.State.CustomAction0;
			}
			else if ((Timer += Time.deltaTime) > 1f)
			{
				Timer = 0f;
				PathToPosition(TargetObject.gameObject.transform.position);
			}
			break;
		case StateMachine.State.CustomAction0:
			crop.DoWork(1f / (WateringDuration / Time.deltaTime));
			if ((Timer += Time.deltaTime) > WateringDuration)
			{
				(ParentTask as Task_Farmer).Crops.Remove(crop);
				crop.Reserved = null;
				crop = null;
				TargetObject = null;
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			break;
		}
	}

	private void GetCrop()
	{
		Crop crop = null;
		float num = float.MaxValue;
		foreach (Crop crop2 in (ParentTask as Task_Farmer).Crops)
		{
			if (crop2 != null && crop2.Reserved == null)
			{
				float num2 = Vector3.Distance(crop2.gameObject.transform.position, t.transform.position);
				if (num2 < num)
				{
					crop = crop2;
					num = num2;
				}
			}
		}
		if (crop != null)
		{
			TargetObject = crop.gameObject;
			this.crop = crop;
			crop.Reserved = t.gameObject;
		}
	}

	public override void ClearTask()
	{
		TargetObject = null;
		if (crop != null)
		{
			crop.Reserved = null;
		}
		crop = null;
	}
}
