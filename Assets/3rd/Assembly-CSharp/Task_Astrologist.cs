using System.Collections;
using UnityEngine;

public class Task_Astrologist : Task
{
	private Structure structure;

	private WorkPlace workplace;

	private WorshipperInfoManager wim;

	private Task_GoToAndStop GoToAndStop;

	private Astrologist astrologist;

	private Worshipper w;

	private int CurrentBoard;

	public Task_Astrologist()
	{
		Type = Task_Type.ASTROLOGIST;
	}

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		Debug.Log("START TASK!");
		base.StartTask(t, TargetObject);
		wim = t.GetComponent<WorshipperInfoManager>();
		workplace = WorkPlace.GetWorkPlaceByID(wim.v_i.WorkPlace);
		astrologist = workplace.GetComponent<Astrologist>();
		w = t.GetComponent<Worshipper>();
	}

	public override void ClearTask()
	{
		workplace.EndJob(t, wim.v_i.WorkPlaceSlot);
		if (GoToAndStop != null)
		{
			GoToAndStop.ClearTask();
		}
		GoToAndStop = null;
		if (CurrentTask != null)
		{
			CurrentTask.ClearTask();
		}
		CurrentTask = null;
		base.ClearTask();
	}

	public override void TaskUpdate()
	{
		if (GoToAndStop != null)
		{
			GoToAndStop.TaskUpdate();
		}
		if (CurrentBoard >= astrologist.MoonBoardsUpdated.Count)
		{
			if (Vector2.Distance(t.transform.position, workplace.Positions[wim.v_i.WorkPlaceSlot].transform.position) > t.StoppingDistance && GoToAndStop == null)
			{
				GoToAndStop = new Task_GoToAndStop();
				GoToAndStop.StartTask(t, workplace.Positions[wim.v_i.WorkPlaceSlot].gameObject);
				GoToAndStop.DoCallback = false;
			}
			if (Vector2.Distance(t.transform.position, workplace.Positions[wim.v_i.WorkPlaceSlot].transform.position) <= t.StoppingDistance && t.state.CURRENT_STATE != StateMachine.State.CustomAction0)
			{
				t.state.CURRENT_STATE = StateMachine.State.CustomAction0;
				t.state.facingAngle = 0f;
				w.SetAnimation("astrologer", true);
			}
		}
		else if (!astrologist.MoonBoardsUpdated[CurrentBoard] && GoToAndStop == null)
		{
			t.StartCoroutine(GoToAndUpdateImage(astrologist.MoonBoardPositions[CurrentBoard].gameObject));
		}
	}

	private IEnumerator GoToAndUpdateImage(GameObject ImageToUpdate)
	{
		GoToAndStop = new Task_GoToAndStop();
		GoToAndStop.StartTask(t, ImageToUpdate);
		GoToAndStop.DoCallback = false;
		while (Vector2.Distance(t.transform.position, ImageToUpdate.transform.position) > t.StoppingDistance || state.CURRENT_STATE == StateMachine.State.Moving)
		{
			yield return null;
		}
		t.state.CURRENT_STATE = StateMachine.State.CustomAction0;
		t.state.facingAngle = Utils.GetAngle(t.transform.position, ImageToUpdate.transform.position);
		yield return new WaitForSeconds(1f);
		astrologist.MoonBoards[CurrentBoard].sprite = astrologist.MoonBoardsImage[CurrentBoard];
		GoToAndStop = null;
		CurrentBoard++;
	}
}
