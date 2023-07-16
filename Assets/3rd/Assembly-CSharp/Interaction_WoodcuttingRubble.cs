using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class Interaction_WoodcuttingRubble : TreeBase
{
	private UIProgressIndicator _uiProgressIndicator;

	public ParticleSystem TreeHitParticles;

	private string sLabelName;

	public bool Activated;

	public GameObject PlayerPositionLeft;

	public GameObject PlayerPositionRight;

	public SkeletonAnimation skeletonAnimation;

	public string chopWoodEventName = "Chop";

	public RandomObjectPicker objectPick;

	public List<Transform> ShakeTransforms;

	private Vector2[] Shake = new Vector2[0];

	private void Start()
	{
		UpdateLocalisation();
		RandomObjectPicker randomObjectPicker = objectPick;
		randomObjectPicker.ObjectCreated = (UnityAction)Delegate.Combine(randomObjectPicker.ObjectCreated, new UnityAction(ObjectCreated));
	}

	private void ObjectCreated()
	{
		Transform[] componentsInChildren = objectPick.CreatedObject.GetComponentsInChildren<Transform>();
		foreach (Transform item in componentsInChildren)
		{
			ShakeTransforms.Add(item);
		}
		Shake = new Vector2[ShakeTransforms.Count];
		for (int j = 0; j < ShakeTransforms.Count; j++)
		{
			Shake[j] = ShakeTransforms[j].transform.localPosition;
		}
	}

	public void ShakeRubble()
	{
		float num = 0.5f;
		if (ShakeTransforms.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < ShakeTransforms.Count; i++)
		{
			if (ShakeTransforms[i] != null && ShakeTransforms[i].gameObject.activeSelf)
			{
				ShakeTransforms[i].DOKill();
				ShakeTransforms[i].transform.localPosition = Shake[i];
				ShakeTransforms[i].DOShakePosition(0.5f * num, new Vector2(UnityEngine.Random.Range(-0.25f, 0.25f) * num, 0f));
			}
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sLabelName = ScriptLocalization.Interactions.ChopWood;
	}

	public override void GetLabel()
	{
		if (!Activated)
		{
			if (base.StructureBrain != null)
			{
				Debug.Log("StructureBrain.TreeChopped: ".Colour(Color.green) + base.StructureBrain.TreeChopped);
				Debug.Log("StructureBrain.Data.IsSapling: ".Colour(Color.green) + base.StructureBrain.Data.IsSapling);
				base.Label = ((base.StructureBrain.TreeChopped || base.StructureBrain.Data.IsSapling) ? "" : sLabelName);
			}
		}
		else
		{
			base.Label = "";
		}
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		TreeBase.Trees.Add(this);
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
			return;
		}
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	public override void OnDisableInteraction()
	{
		if (skeletonAnimation != null)
		{
			skeletonAnimation.AnimationState.Event -= HandleEvent;
		}
		base.OnDisableInteraction();
		TreeBase.Trees.Remove(this);
		if (base.StructureBrain != null)
		{
			Structures_Tree structureBrain = base.StructureBrain;
			structureBrain.OnTreeProgressChanged = (Action<int>)Delegate.Remove(structureBrain.OnTreeProgressChanged, new Action<int>(OnTreeHit));
			Structures_Tree structureBrain2 = base.StructureBrain;
			structureBrain2.OnTreeComplete = (Action<bool>)Delegate.Remove(structureBrain2.OnTreeComplete, new Action<bool>(OnChoppedDown));
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_uiProgressIndicator != null)
		{
			_uiProgressIndicator.Recycle();
			_uiProgressIndicator = null;
		}
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	private void OnBrainAssigned()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (base.StructureBrain.TreeChopped || base.StructureBrain.Data.IsSapling)
		{
			base.StructureBrain.Remove();
			base.enabled = false;
			return;
		}
		Structures_Tree structureBrain = base.StructureBrain;
		structureBrain.OnTreeProgressChanged = (Action<int>)Delegate.Combine(structureBrain.OnTreeProgressChanged, new Action<int>(OnTreeHit));
		Structures_Tree structureBrain2 = base.StructureBrain;
		structureBrain2.OnTreeComplete = (Action<bool>)Delegate.Combine(structureBrain2.OnTreeComplete, new Action<bool>(OnChoppedDown));
	}

	private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.Data.Name == chopWoodEventName && base.StructureBrain != null)
		{
			base.StructureBrain.TreeHit(1f + UpgradeSystem.Chopping);
		}
		if (e.Data.Name == "swipe_1")
		{
			CameraManager.shakeCamera(0.1f, Utils.GetAngle(PlayerFarming.Instance.gameObject.transform.position, base.transform.position));
			MMVibrate.Haptic(MMVibrate.HapticTypes.SoftImpact, false, true, GameManager.GetInstance());
		}
	}

	public void OnTreeHit(int followerID)
	{
		if (PlayerFarming.Instance != null)
		{
			float angle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.gameObject.transform.position);
			BiomeConstants.Instance.EmitHitImpactEffect(base.transform.position + Vector3.back * 0.5f, angle);
		}
		TreeHitParticles.Play();
		ShakeRubble();
		AudioManager.Instance.PlayOneShot("event:/material/tree_chop", base.transform.position);
		base.gameObject.transform.DOShakePosition(0.1f, 0.1f, 13, 48.8f);
		float num = base.StructureBrain.Data.Progress / base.StructureBrain.Data.ProgressTarget;
		if (num == 0f)
		{
			return;
		}
		if (_uiProgressIndicator == null)
		{
			_uiProgressIndicator = BiomeConstants.Instance.ProgressIndicatorTemplate.Spawn(BiomeConstants.Instance.transform, base.transform.position + Vector3.back * 1.5f - BiomeConstants.Instance.transform.position);
			_uiProgressIndicator.Show(num);
			UIProgressIndicator uiProgressIndicator = _uiProgressIndicator;
			uiProgressIndicator.OnHidden = (Action)Delegate.Combine(uiProgressIndicator.OnHidden, (Action)delegate
			{
				_uiProgressIndicator = null;
			});
		}
		else
		{
			_uiProgressIndicator.SetProgress(num);
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activated)
		{
			DataManager.Instance.FirstTimeChop = true;
			if (skeletonAnimation == null)
			{
				skeletonAnimation = PlayerFarming.Instance.Spine;
			}
			if (!(skeletonAnimation == null))
			{
				Activated = true;
				skeletonAnimation.AnimationState.Event += HandleEvent;
				base.OnInteract(state);
				StopAllCoroutines();
				StartCoroutine(DoWoodCutting());
				Interactable = false;
			}
		}
	}

	public void OnChoppedDown(bool dropLoot)
	{
		TreeHitParticles.Play();
		if (dropLoot)
		{
			int num = Structure.Structure_Info.LootCountToDrop;
			if (Activated)
			{
				num += TrinketManager.GetLootIncreaseModifier(InventoryItem.ITEM_TYPE.LOG);
				num += UpgradeSystem.GetForageIncreaseModifier;
			}
			int num2 = -1;
			while (++num2 < num)
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.LOG, 1, base.transform.position);
			}
		}
		if (Activated)
		{
			EndChopping();
		}
		AudioManager.Instance.PlayOneShot("event:/material/tree_break", base.transform.position);
		Interactable = false;
		Debug.Log("REMOVE TREEEEEE");
		base.StructureBrain.Remove();
		BiomeConstants.Instance.EmitSmokeExplosionVFX(base.gameObject.transform.position);
		if ((bool)skeletonAnimation)
		{
			skeletonAnimation.AnimationState.Event -= HandleEvent;
		}
	}

	private void EndChopping()
	{
		Action onCrownReturn = PlayerFarming.OnCrownReturn;
		if (onCrownReturn != null)
		{
			onCrownReturn();
		}
		if (PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.CustomAnimation)
		{
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		}
		StopAllCoroutines();
		Activated = false;
		Interactable = true;
		base.HasChanged = true;
	}

	private IEnumerator DoWoodCutting()
	{
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("actions/chop-wood", 0, true);
		yield return new WaitForSeconds(23f / 30f);
		while (InputManager.Gameplay.GetInteractButtonHeld() && state.CURRENT_STATE == StateMachine.State.CustomAnimation)
		{
			yield return null;
		}
		skeletonAnimation.AnimationState.Event -= HandleEvent;
		EndChopping();
	}
}
