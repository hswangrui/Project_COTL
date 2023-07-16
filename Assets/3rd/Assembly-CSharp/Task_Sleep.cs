using UnityEngine;

public class Task_Sleep : Task
{
	public bool DoYawn = true;

	private Dwelling dwelling;

	private Worshipper w;

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		t.GetComponent<SimpleInventory>().DropItem();
		Type = Task_Type.SLEEP;
		w = t.GetComponent<Worshipper>();
	}

	public void Init(Dwelling dwelling, GameObject TargetObject, bool DoYawn)
	{
		this.dwelling = dwelling;
		base.TargetObject = TargetObject;
		this.DoYawn = DoYawn;
	}

	private void EndOfPath()
	{
		GameObject gameObject = new GameObject();
		gameObject.transform.position = TargetObject.transform.position;
		gameObject.name = w.wim.v_i.SkinName + "  " + w.wim.v_i.Name;
		Timer = 0f;
		DoSleep();
		DoYawn = true;
	}

	private void DoSleep()
	{
		t.transform.position = TargetObject.transform.position;
		state.CURRENT_STATE = StateMachine.State.Sleeping;
		dwelling.SetBedImage(w.wim.v_i.DwellingSlot, Dwelling.SlotState.IN_USE);
		w.wim.v_i.DwellingClaimed = true;
	}

	public override void TaskUpdate()
	{
		base.TaskUpdate();
		if (state.CURRENT_STATE != StateMachine.State.Sleeping)
		{
			switch (state.CURRENT_STATE)
			{
			case StateMachine.State.Idle:
				if (Vector2.Distance(TargetObject.transform.position, t.transform.position) > Farm.FarmTileSize)
				{
					Timer = 0f;
					PathToPosition(TargetObject.transform.position);
				}
				else
				{
					EndOfPath();
				}
				break;
			case StateMachine.State.Moving:
				if (Vector2.Distance(TargetObject.transform.position, t.transform.position) > Farm.FarmTileSize)
				{
					if ((Timer += Time.deltaTime) > 1f)
					{
						Timer = 0f;
						PathToPosition(TargetObject.transform.position);
					}
				}
				else
				{
					state.CURRENT_STATE = StateMachine.State.Idle;
				}
				break;
			}
			return;
		}
		float num = 0f;
		float num2 = 0f;
		switch (dwelling.Structure.Type)
		{
		case StructureBrain.TYPES.BED_2:
			num = 180f;
			num2 = 25f;
			break;
		case StructureBrain.TYPES.BED_3:
			num = 120f;
			num2 = 20f;
			break;
		default:
			num = 60f;
			num2 = 10f;
			break;
		}
		w.Sleep += Time.deltaTime * num2;
		if (w.Sleep >= num)
		{
			StructureBrain.TYPES type = dwelling.Structure.Type;
			if (type != StructureBrain.TYPES.BED_2)
			{
				int num3 = 41;
			}
			ClearTask();
			t.ClearTask();
		}
		else if (Vector2.Distance(TargetObject.transform.position, t.transform.position) > Farm.FarmTileSize)
		{
			Timer = 0f;
			PathToPosition(TargetObject.transform.position);
		}
	}

	public override void ClearTask()
	{
		switch (dwelling.Structure.Type)
		{
		case StructureBrain.TYPES.BED_2:
			w.Sleep = 180f;
			break;
		case StructureBrain.TYPES.BED_3:
			w.Sleep = 120f;
			break;
		default:
			w.Sleep = 60f;
			break;
		}
		if (dwelling != null)
		{
			dwelling.SetBedImage(w.wim.v_i.DwellingSlot, Dwelling.SlotState.CLAIMED);
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		base.ClearTask();
	}
}
