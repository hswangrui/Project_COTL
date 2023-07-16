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

public class Vomit : Interaction
{
	public Structure structure;

	public static List<Vomit> Vomits = new List<Vomit>();

	[SerializeField]
	private Transform vomitSpriteTransform;

	private string sString;

	private SkeletonAnimation skeletonAnimation;

	private bool EventListenerActive;

	private bool playedSfx;

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
			if (StructureBrain != null)
			{
				return StructureBrain.ReservedByPlayer;
			}
			return false;
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
		Vomits.Add(this);
		Structure obj = structure;
		obj.OnBrainAssigned = (Action)Delegate.Combine(obj.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		Vomits.Remove(this);
		Structure obj = structure;
		obj.OnBrainAssigned = (Action)Delegate.Remove(obj.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	private void OnBrainAssigned()
	{
		if (!StructureInfo.Picked)
		{
			StructureInfo.Picked = true;
			base.transform.localScale = Vector3.one;
			base.transform.DOPunchScale(Vector3.one * 0.25f, 0.25f);
			vomitSpriteTransform.transform.localPosition = new Vector3(0f, 0f, UnityEngine.Random.Range(-0.02f, 0f));
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = ScriptLocalization.Interactions.Clean;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		StartCoroutine(DoClean());
		if (skeletonAnimation == null)
		{
			skeletonAnimation = PlayerFarming.Instance.Spine;
		}
		if (!EventListenerActive)
		{
			skeletonAnimation.AnimationState.Event += HandleEvent;
			EventListenerActive = true;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
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
		Activating = true;
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		Vector3 position = base.transform.position;
		state.facingAngle = Utils.GetAngle(state.transform.position, position);
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("cleaning", 0, true);
		yield return new WaitForSeconds(14f / 15f);
		base.transform.DOKill();
		base.transform.DOScale(0f, 0.15f).OnComplete(delegate
		{
			AudioManager.Instance.PlayOneShot("event:/followers/poop_pop", base.transform.position);
			List<StructureBrain> allStructuresOfType = StructureManager.GetAllStructuresOfType(StructureInfo.Location, StructureBrain.TYPES.VOMIT);
			for (int num = allStructuresOfType.Count - 1; num >= 0; num--)
			{
				if (Vector3.Distance(allStructuresOfType[num].Data.Position, base.transform.position) < 0.5f)
				{
					allStructuresOfType[num].Remove();
				}
			}
			skeletonAnimation.AnimationState.Event -= HandleEvent;
			EventListenerActive = false;
			SpawnLoot(base.transform.position);
			BiomeConstants.Instance.EmitBloodSplatter(base.transform.position, Vector3.back, Color.green);
			BiomeConstants.Instance.EmitBloodDieEffect(base.transform.position, Vector3.back, Color.green);
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
			StructureBrain.Remove();
		});
	}

	public static void SpawnLoot(Vector3 position)
	{
		switch (UnityEngine.Random.Range(0, 100))
		{
		case 87:
		case 88:
		case 89:
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, position);
			break;
		case 90:
		case 91:
		case 92:
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BONE, 1, position);
			break;
		case 93:
		case 94:
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.MEAT, 1, position);
			break;
		case 95:
		case 96:
		case 97:
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BERRY, 3, position);
			break;
		case 98:
		case 99:
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.Necklace_4, 1, position);
			break;
		}
	}

	public override void GetLabel()
	{
		base.Label = sString;
	}
}
