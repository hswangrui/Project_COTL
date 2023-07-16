using System.Collections;
using Spine;
using UnityEngine;

public class Task_Build : Task
{
	private Worshipper worshipper;

	private BuildSitePlot buildsitePlot;

	private FarmPlot TargetFarmPlot;

	private GameObject GoToMarker;

	private Vector3 RandomPosition;

	private Coroutine cGoToAndBuild;

	private float RepathTimer = 1f;

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		worshipper = t.GetComponent<Worshipper>();
		buildsitePlot = TargetObject.GetComponent<BuildSitePlot>();
		Type = Task_Type.BUILDING;
		RandomPosition = buildsitePlot.transform.position + new Vector3(0f, buildsitePlot.Bounds.y / 2) + (Vector3)(Random.insideUnitCircle * buildsitePlot.Bounds.x);
		GoToMarker = new GameObject();
		GoToMarker.transform.position = RandomPosition;
		worshipper.GoToAndStop(GoToMarker, GoToAndBuild, TargetObject, false);
	}

	public override void TaskUpdate()
	{
		if (buildsitePlot == null)
		{
			WorkCompleted();
		}
	}

	private void GoToAndBuild()
	{
		cGoToAndBuild = t.StartCoroutine(GoToAndBuildRoutine());
	}

	private IEnumerator GoToAndBuildRoutine()
	{
		yield return new WaitForSeconds(0.5f);
		state.facingAngle = Utils.GetAngle(t.transform.position, buildsitePlot.transform.position);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		worshipper.SetAnimation(worshipper.Motivated ? "build-fast-scared" : "build", true);
		worshipper.Spine.AnimationState.Event += HandleAnimationStateEvent;
		worshipper.health.OnHit += Health_OnHit;
		while (buildsitePlot != null)
		{
			yield return null;
		}
		WorkCompleted();
	}

	private void Health_OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		Debug.Log("HIT AND CHANGE ANIMATION!");
		worshipper.SetAnimation("build-fast-scared", true);
	}

	private void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
	{
	}

	public void WorkCompleted()
	{
		t.StopAllCoroutines();
		state.CURRENT_STATE = StateMachine.State.Idle;
		ClearTask();
	}

	public override void ClearTask()
	{
		if (GoToMarker != null)
		{
			Object.Destroy(GoToMarker);
		}
		if (buildsitePlot != null)
		{
			buildsitePlot.Worshippers.Remove(worshipper);
		}
		worshipper.health.OnHit -= Health_OnHit;
		worshipper.Spine.AnimationState.Event -= HandleAnimationStateEvent;
		base.ClearTask();
	}
}
