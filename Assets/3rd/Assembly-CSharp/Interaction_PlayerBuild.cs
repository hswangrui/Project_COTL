using System;
using System.Collections;
using I2.Loc;
using UnityEngine;

public class Interaction_PlayerBuild : Interaction
{
	public BuildSitePlot buildSitePlot;

	private bool Activating;

	public static Action<BuildSitePlot> PlayerActivatingStart;

	public static Action<BuildSitePlot> PlayerActivatingEnd;

	private string sBuild;

	private string sObstructed;

	private string sPrioritise;

	private string sUnprioritise;

	private string sCancel;

	private bool helpedFollower;

	private void Start()
	{
		UpdateLocalisation();
		ContinuouslyHold = true;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sBuild = ScriptLocalization.Interactions.Build;
		sCancel = ScriptLocalization.Interactions.Cancel;
	}

	public override void GetLabel()
	{
		if (!buildSitePlot.StructureBrain.IsObstructed)
		{
			Interactable = true;
			base.Label = sBuild;
		}
		else
		{
			Interactable = false;
			base.Label = sObstructed;
		}
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
		string value = (base.SecondaryLabel = (StructuresData.GetInfoByType(buildSitePlot.Structure.Brain.Data.ToBuildType, 0).IsUpgrade ? string.Empty : sCancel));
		HasSecondaryInteraction = !string.IsNullOrEmpty(value);
	}

	public override void OnSecondaryInteract(StateMachine state)
	{
		base.OnSecondaryInteract(state);
		AudioManager.Instance.PlayOneShot("event:/building/finished_wood", base.transform.position);
		MMVibrate.Haptic(MMVibrate.HapticTypes.LightImpact);
		foreach (StructuresData.ItemCost item in StructuresData.GetCost(buildSitePlot.Structure.Brain.Data.ToBuildType))
		{
			InventoryItem.Spawn(item.CostItem, item.CostValue, base.transform.position);
		}
		buildSitePlot.Structure.RemoveStructure();
		GameManager.RecalculatePaths();
		foreach (Follower item2 in FollowerManager.ActiveLocationFollowers())
		{
			item2.Brain.CheckChangeTask();
		}
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

	private new void Update()
	{
		if (buildSitePlot.StructureInfo == null)
		{
			return;
		}
		if (Activating && !helpedFollower)
		{
			foreach (Follower follower in Follower.Followers)
			{
				if (follower.Brain.CurrentTaskType == FollowerTaskType.Build && follower.Brain.CurrentTask.UsingStructureID == buildSitePlot.Structure.Structure_Info.ID && follower.Brain.CurrentTask.State == FollowerTaskState.Doing)
				{
					helpedFollower = true;
					break;
				}
			}
		}
		if (buildSitePlot.StructureInfo.Prioritised && Interactor.CurrentInteraction != this)
		{
			SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].color = Color.yellow;
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
			buildSitePlot.StructureBrain.BuildProgress += 5f;
			if (!LetterBox.IsPlaying)
			{
				CameraManager.shakeCamera(0.3f);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (Activating)
		{
			StopAllCoroutines();
			PlayerFarming.Instance.simpleSpineAnimator.OnSpineEvent -= SimpleSpineAnimator_OnSpineEvent;
			Action onCrownReturn = PlayerFarming.OnCrownReturn;
			if (onCrownReturn != null)
			{
				onCrownReturn();
			}
			if (!PlayerFarming.Instance.GoToAndStopping)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			if (helpedFollower)
			{
				CultFaithManager.AddThought(Thought.Cult_HelpFollower, -1, 1f);
			}
		}
	}

	private IEnumerator DoBuild()
	{
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		Action<BuildSitePlot> playerActivatingStart = PlayerActivatingStart;
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
		Action onCrownReturn = PlayerFarming.OnCrownReturn;
		if (onCrownReturn != null)
		{
			onCrownReturn();
		}
		if (!PlayerFarming.Instance.GoToAndStopping)
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
		}
		Action<BuildSitePlot> playerActivatingEnd = PlayerActivatingEnd;
		if (playerActivatingEnd != null)
		{
			playerActivatingEnd(buildSitePlot);
		}
		Activating = false;
	}
}
