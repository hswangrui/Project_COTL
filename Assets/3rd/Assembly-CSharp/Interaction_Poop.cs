using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using Spine;
using Spine.Unity;
using src.UI.Overlays.TutorialOverlay;
using UnityEngine;

public class Interaction_Poop : Interaction
{
	public Structure structure;

	public static List<Interaction_Poop> Poops = new List<Interaction_Poop>();

	private float Scale;

	private float ScaleSpeed;

	private string sString;

	private bool EventListenerActive;

	private bool playedSfx;

	private SkeletonAnimation skeletonAnimation;

	public StructuresData StructureInfo
	{
		get
		{
			return structure.Structure_Info;
		}
	}

	public StructureBrain StructureBrain
	{
		get
		{
			return structure.Brain;
		}
	}

	public bool Activating
	{
		get
		{
			if (StructureBrain == null)
			{
				return false;
			}
			return StructureBrain.ReservedByPlayer;
		}
		set
		{
			StructureBrain.ReservedByPlayer = value;
		}
	}

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		Poops.Add(this);
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		Poops.Remove(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (StructureInfo != null && StructureInfo.Destroyed && StructureInfo.LootCountToDrop != -1)
		{
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.POOP, 1, base.transform.position);
		}
		if (Activating)
		{
			StopAllCoroutines();
			Action onCrownReturn = PlayerFarming.OnCrownReturn;
			if (onCrownReturn != null)
			{
				onCrownReturn();
			}
			state.CURRENT_STATE = StateMachine.State.Idle;
		}
		if (skeletonAnimation != null && skeletonAnimation.AnimationState != null)
		{
			skeletonAnimation.AnimationState.Event -= HandleEvent;
		}
	}

	public void Play()
	{
		Scale = 2f;
		StartCoroutine(ScaleRoutine());
	}

	private IEnumerator ScaleRoutine()
	{
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < 5f))
			{
				break;
			}
			ScaleSpeed += (1f - Scale) * 0.3f * (Time.deltaTime * 60f);
			Scale += (ScaleSpeed *= 0.7f) * (Time.deltaTime * 60f);
			base.transform.localScale = Vector3.one * Scale;
			yield return null;
		}
		base.transform.localScale = Vector3.one;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = ScriptLocalization.Interactions.Clean;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (skeletonAnimation == null)
		{
			skeletonAnimation = PlayerFarming.Instance.Spine;
		}
		if (!EventListenerActive)
		{
			skeletonAnimation.AnimationState.Event += HandleEvent;
			EventListenerActive = true;
		}
		StartCoroutine(DoClean());
	}

	private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.Data.Name == "sfxTrigger")
		{
			CameraManager.shakeCamera(0.05f, Utils.GetAngle(PlayerFarming.Instance.gameObject.transform.position, base.transform.position));
			MMVibrate.Haptic(MMVibrate.HapticTypes.SoftImpact, false, true, GameManager.GetInstance());
			base.transform.DOKill();
			base.transform.DOPunchScale(Vector3.one * 0.15f, 0.25f);
			if (!playedSfx)
			{
				AudioManager.Instance.PlayOneShot("event:/player/sweep", base.transform.position);
				playedSfx = true;
			}
		}
	}

	private IEnumerator DoClean()
	{
		playedSfx = false;
		Activating = true;
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("cleaning", 0, true);
		yield return new WaitForSeconds(14f / 15f);
		StructureBrain.Remove();
		AudioManager.Instance.PlayOneShot("event:/followers/poop_pop", base.transform.position);
		BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position);
		skeletonAnimation.AnimationState.Event -= HandleEvent;
		EventListenerActive = false;
		Action onCrownReturn = PlayerFarming.OnCrownReturn;
		if (onCrownReturn != null)
		{
			onCrownReturn();
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		Activating = false;
		if (!DataManager.Instance.ShowCultIllness && DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Illness))
		{
			UITutorialOverlayController uITutorialOverlayController = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Illness);
			uITutorialOverlayController.OnHide = (Action)Delegate.Combine(uITutorialOverlayController.OnHide, new Action(IllnessBar.Instance.Reveal));
		}
		List<StructureBrain> allStructuresOfType = StructureManager.GetAllStructuresOfType(StructureInfo.Location, StructureBrain.TYPES.POOP);
		for (int num = allStructuresOfType.Count - 1; num >= 0; num--)
		{
			if (Vector3.Distance(allStructuresOfType[num].Data.Position, base.transform.position) < 0.5f)
			{
				allStructuresOfType[num].Data.LootCountToDrop = -1;
				allStructuresOfType[num].Remove();
			}
		}
	}

	public override void GetLabel()
	{
		base.Label = sString;
	}
}
