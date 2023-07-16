using System;
using System.Collections;
using I2.Loc;
using UnityEngine;

public class Interaction_PlayerBuildProject : Interaction
{
	public BuildSitePlotProject buildSitePlot;

	private bool Activating;

	public static Action<BuildSitePlotProject> PlayerActivatingStart;

	public static Action<BuildSitePlotProject> PlayerActivatingEnd;

	private string sBuild;

	private string sPrioritise;

	private string sUnprioritise;

	private void Start()
	{
		UpdateLocalisation();
		ContinuouslyHold = true;
		HasSecondaryInteraction = true;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sBuild = ScriptLocalization.Interactions.Build;
	}

	public override void GetLabel()
	{
		base.Label = sBuild;
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activating)
		{
			base.OnInteract(state);
			PlayerFarming.Instance.simpleSpineAnimator.OnSpineEvent += SimpleSpineAnimator_OnSpineEvent;
			Activating = true;
			StartCoroutine(DoBuild());
		}
	}

	public override void GetSecondaryLabel()
	{
		base.SecondaryLabel = (buildSitePlot.StructureInfo.Prioritised ? sUnprioritise : sPrioritise);
	}

	public override void OnSecondaryInteract(StateMachine state)
	{
		base.OnSecondaryInteract(state);
		if (!buildSitePlot.StructureInfo.Prioritised)
		{
			SpriteRenderer[] componentsInChildren;
			foreach (BuildSitePlot buildSitePlot in BuildSitePlot.BuildSitePlots)
			{
				if (buildSitePlot.StructureInfo.Prioritised)
				{
					buildSitePlot.StructureInfo.Prioritised = false;
					buildSitePlot.StructureBrain.MarkObstructionsForClearing(buildSitePlot.StructureInfo.GridTilePosition, buildSitePlot.StructureInfo.Bounds, false);
					componentsInChildren = buildSitePlot.gameObject.GetComponentsInChildren<SpriteRenderer>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].color = Color.white;
					}
				}
			}
			foreach (BuildSitePlotProject buildSitePlot2 in BuildSitePlotProject.BuildSitePlots)
			{
				if (buildSitePlot2.StructureInfo.Prioritised)
				{
					buildSitePlot2.StructureInfo.Prioritised = false;
					buildSitePlot2.StructureBrain.MarkObstructionsForClearing(buildSitePlot2.StructureInfo.GridTilePosition, buildSitePlot2.StructureInfo.Bounds, false);
					componentsInChildren = buildSitePlot2.gameObject.GetComponentsInChildren<SpriteRenderer>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].color = Color.white;
					}
				}
			}
			this.buildSitePlot.StructureInfo.Prioritised = true;
			this.buildSitePlot.StructureBrain.MarkObstructionsForClearing(this.buildSitePlot.StructureInfo.GridTilePosition, this.buildSitePlot.StructureInfo.Bounds, true);
			componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].color = Color.yellow;
			}
		}
		else
		{
			SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].color = Color.white;
			}
			this.buildSitePlot.StructureInfo.Prioritised = false;
		}
		ForceFollowersToUpdateRubble();
	}

	private void ForceFollowersToUpdateRubble()
	{
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			Debug.Log(allBrain.CurrentTaskType);
			if (allBrain.CurrentTaskType == FollowerTaskType.ClearRubble || allBrain.CurrentTaskType == FollowerTaskType.ClearWeeds || allBrain.CurrentTaskType == FollowerTaskType.Build || allBrain.CurrentTaskType == FollowerTaskType.ChopTrees || allBrain.CurrentTaskType == FollowerTaskType.Forage)
			{
				allBrain.CheckChangeTask();
			}
		}
	}

	public override void OnBecomeCurrent()
	{
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].color = Color.white;
		}
		base.OnBecomeCurrent();
	}

	private void SimpleSpineAnimator_OnSpineEvent(string EventName)
	{
		if (EventName == "Chop")
		{
			AudioManager.Instance.PlayOneShot("event:/followers/hammering", base.transform.position);
			buildSitePlot.StructureBrain.BuildProgress += 3f;
			CameraManager.shakeCamera(0.3f);
		}
	}

	private new void OnDestroy()
	{
		if (Activating)
		{
			StopAllCoroutines();
			PlayerFarming.Instance.simpleSpineAnimator.OnSpineEvent -= SimpleSpineAnimator_OnSpineEvent;
			if (!PlayerFarming.Instance.GoToAndStopping)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
		}
	}

	private IEnumerator DoBuild()
	{
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		Action<BuildSitePlotProject> playerActivatingStart = PlayerActivatingStart;
		if (playerActivatingStart != null)
		{
			playerActivatingStart(buildSitePlot);
		}
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("actions/hammer", 0, true);
		yield return new WaitForSeconds(14f / 15f);
		while (InputManager.Gameplay.GetInteractButtonHeld())
		{
			yield return null;
		}
		PlayerFarming.Instance.simpleSpineAnimator.OnSpineEvent -= SimpleSpineAnimator_OnSpineEvent;
		if (!PlayerFarming.Instance.GoToAndStopping)
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
		}
		Action<BuildSitePlotProject> playerActivatingEnd = PlayerActivatingEnd;
		if (playerActivatingEnd != null)
		{
			playerActivatingEnd(buildSitePlot);
		}
		Activating = false;
	}
}
