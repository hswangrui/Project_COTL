using System;
using System.Collections;
using I2.Loc;
using UnityEngine;

public class Interaction_PlayerClearRubble : Interaction
{
	public Rubble rubble;

	public bool RequireUseOthersFirst;

	private bool Activating;

	public static Action<Rubble> PlayerActivatingStart;

	public static Action<Rubble> PlayerActivatingEnd;

	[SerializeField]
	private ParticleSystem _particleSystem;

	private string sString;

	private string sMine;

	private bool helpedFollower;

	private float size;

	private void Start()
	{
		UpdateLocalisation();
		ContinuouslyHold = true;
		HasSecondaryInteraction = false;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = ScriptLocalization.Interactions.ClearRubble;
		sMine = ScriptLocalization.Interactions.MineBloodStone;
	}

	public override void GetLabel()
	{
		if (!RequireUseOthersFirst || (RequireUseOthersFirst && DataManager.Instance.FirstTimeMine))
		{
			base.Label = sMine + " " + FontImageNames.GetIconByType(rubble.StructureInfo.LootToDrop);
		}
		else
		{
			base.Label = "";
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activating)
		{
			base.OnInteract(state);
			DataManager.Instance.FirstTimeMine = true;
			PlayerFarming.Instance.simpleSpineAnimator.OnSpineEvent += SimpleSpineAnimator_OnSpineEvent;
			Activating = true;
			StartCoroutine(DoBuild());
		}
	}

	private new void Update()
	{
		if (rubble.StructureInfo == null)
		{
			return;
		}
		if (Activating && !helpedFollower)
		{
			foreach (Follower follower in Follower.Followers)
			{
				FollowerTask_ClearRubble followerTask_ClearRubble;
				if (follower != null && (followerTask_ClearRubble = follower.Brain.CurrentTask as FollowerTask_ClearRubble) != null && followerTask_ClearRubble.RubbleID == rubble.Structure.Structure_Info.ID && follower.Brain.CurrentTask.State == FollowerTaskState.Doing)
				{
					helpedFollower = true;
					break;
				}
			}
		}
		if (rubble.StructureInfo.Prioritised && Interactor.CurrentInteraction != this)
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

	public override void OnSecondaryInteract(StateMachine state)
	{
		base.OnSecondaryInteract(state);
		if (!rubble.StructureInfo.Prioritised)
		{
			rubble.StructureInfo.Prioritised = true;
			SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
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
			rubble.StructureInfo.Prioritised = false;
		}
		ForceFollowersToUpdateRubble();
	}

	private void ForceFollowersToUpdateRubble()
	{
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.CurrentTaskType == FollowerTaskType.ClearRubble || allBrain.CurrentTaskType == FollowerTaskType.ClearWeeds || allBrain.CurrentTaskType == FollowerTaskType.Build)
			{
				allBrain.CheckChangeTask();
			}
		}
	}

	private void SimpleSpineAnimator_OnSpineEvent(string EventName)
	{
		Debug.Log("Eventname: " + EventName);
		if (EventName == "Chop")
		{
			rubble.StructureBrain.RemovalProgress += 6f + UpgradeSystem.Mining;
			rubble.StructureBrain.UpdateProgress();
			rubble.PlayerRubbleFX();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (Activating)
		{
			if (rubble._uiProgressIndicator != null)
			{
				rubble._uiProgressIndicator.Recycle();
				rubble._uiProgressIndicator = null;
			}
			CameraManager.shakeCamera(0.3f);
			StopAllCoroutines();
			MMVibrate.StopRumble();
			PlayerFarming.Instance.simpleSpineAnimator.OnSpineEvent -= SimpleSpineAnimator_OnSpineEvent;
			state.CURRENT_STATE = StateMachine.State.Idle;
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
		Action<Rubble> playerActivatingStart = PlayerActivatingStart;
		if (playerActivatingStart != null)
		{
			playerActivatingStart(rubble);
		}
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("actions/chop-stone", 0, true);
		yield return new WaitForSeconds(14f / 15f);
		while (InputManager.Gameplay.GetInteractButtonHeld() && state.CURRENT_STATE == StateMachine.State.CustomAction0)
		{
			yield return null;
		}
		PlayerFarming.Instance.simpleSpineAnimator.OnSpineEvent -= SimpleSpineAnimator_OnSpineEvent;
		if (PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.CustomAction0)
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
		}
		Action onCrownReturn = PlayerFarming.OnCrownReturn;
		if (onCrownReturn != null)
		{
			onCrownReturn();
		}
		Action<Rubble> playerActivatingEnd = PlayerActivatingEnd;
		if (playerActivatingEnd != null)
		{
			playerActivatingEnd(rubble);
		}
		Activating = false;
	}
}
