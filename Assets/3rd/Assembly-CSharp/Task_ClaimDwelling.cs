using UnityEngine;

public class Task_ClaimDwelling : Task
{
	private Worshipper w;

	private Dwelling dwelling;

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		Type = Task_Type.CLAIM_DWELLING;
		w = t.GetComponent<Worshipper>();
		dwelling = null;
		Debug.Log(w.wim.v_i.SkinName);
		ClearOnComplete = true;
	}

	public override void TaskUpdate()
	{
		base.TaskUpdate();
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			if (Vector3.Distance(TargetObject.transform.position, t.transform.position) > Farm.FarmTileSize)
			{
				Timer = 0f;
				PathToPosition(TargetObject.transform.position);
			}
			else
			{
				Timer = 0f;
				state.CURRENT_STATE = StateMachine.State.CustomAction0;
			}
			break;
		case StateMachine.State.Moving:
			if (Vector3.Distance(TargetObject.transform.position, t.transform.position) > Farm.FarmTileSize)
			{
				if ((Timer += Time.deltaTime) > 1f)
				{
					Timer = 0f;
					PathToPosition(TargetObject.transform.position);
				}
			}
			else
			{
				Timer = 0f;
				state.CURRENT_STATE = StateMachine.State.CustomAction0;
			}
			break;
		case StateMachine.State.CustomAction0:
			if ((Timer += Time.deltaTime) > 1f)
			{
				w.wim.v_i.DwellingClaimed = true;
				w.wim.v_i.Complaint_House = false;
				state.CURRENT_STATE = StateMachine.State.Idle;
				dwelling.SetBedImage(w.wim.v_i.DwellingSlot, Dwelling.SlotState.CLAIMED);
				w.wim.SetOutfit(w.wim.v_i.Outfit, false);
				ClearTask();
				t.ClearTask();
			}
			break;
		}
	}

	public override void ClearTask()
	{
		Debug.Log("CLEAR claim dwelling! " + w.wim.v_i.SkinName);
		state.CURRENT_STATE = StateMachine.State.Idle;
		base.ClearTask();
	}
}
