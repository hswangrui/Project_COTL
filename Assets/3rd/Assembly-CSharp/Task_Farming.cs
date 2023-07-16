using System.Collections;
using UnityEngine;

public class Task_Farming : Task
{
	private Worshipper worshipper;

	private FarmStation farmStation;

	private FarmPlot TargetFarmPlot;

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		worshipper = t.GetComponent<Worshipper>();
		farmStation = TargetObject.GetComponent<FarmStation>();
		Type = Task_Type.FARMER;
		worshipper.GoToAndStop(farmStation.WorshipperPosition, ContinueFarmStation, farmStation.gameObject, false);
	}

	private void ContinueFarmStation()
	{
		t.StartCoroutine(GoToFarmStation());
	}

	private IEnumerator GoToFarmStation()
	{
		while (worshipper.GoToAndStopping)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		worshipper.simpleAnimator.SetAttachment("HAT", "Hats/HAT_Farm");
		yield return new WaitForSeconds(0.5f);
		t.StartCoroutine(GoToPlots());
	}

	private IEnumerator GoToPlots()
	{
		yield return new WaitForSeconds(0.5f);
	}

	public override void ClearTask()
	{
		worshipper.simpleAnimator.SetAttachment("HAT", null);
		base.ClearTask();
	}
}
