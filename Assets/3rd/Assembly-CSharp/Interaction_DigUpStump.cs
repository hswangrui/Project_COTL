using System;
using System.Collections;
using DG.Tweening;
using I2.Loc;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class Interaction_DigUpStump : Interaction
{
	public Interaction_Woodcutting woodcutting;

	public ParticleSystem TreeDigParticles;

	private UIProgressIndicator _uiProgressIndicator;

	private Structure _Structure;

	private Structures_Tree _StructureBrain;

	private string sLabelName;

	public bool Activated;

	public static Action<Interaction_DigUpStump> PlayerActivatingStart;

	public static Action<Interaction_DigUpStump> PlayerActivatingEnd;

	public GameObject PlayerPositionLeft;

	public GameObject PlayerPositionRight;

	public SkeletonAnimation skeletonAnimation;

	[SpineEvent("", "", true, false, false)]
	public string digTreeEventName = "dig";

	public SkeletonAnimation TreeSpine;

	public float Progress;

	public float ProgressTotal = 5f;

	[SerializeField]
	private SkeletonRendererCustomMaterials _materialOverride;

	[SerializeField]
	private UnityEvent onDugUp;

	private string ChoppedLabelName;

	public bool EventListenerActive;

	private bool Chopped;

	public Structure Structure
	{
		get
		{
			if (_Structure == null)
			{
				_Structure = GetComponent<Structure>();
			}
			return _Structure;
		}
	}

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_Tree StructureBrain
	{
		get
		{
			if (_StructureBrain == null && Structure.Brain != null)
			{
				_StructureBrain = Structure.Brain as Structures_Tree;
			}
			return _StructureBrain;
		}
		set
		{
			_StructureBrain = value;
		}
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		if (_materialOverride != null)
		{
			_materialOverride.enabled = true;
		}
	}

	public override void OnDisableInteraction()
	{
		if (skeletonAnimation != null)
		{
			skeletonAnimation.AnimationState.Event -= HandleEvent;
		}
		base.OnDisableInteraction();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_uiProgressIndicator != null)
		{
			_uiProgressIndicator.Recycle();
			_uiProgressIndicator = null;
		}
	}

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		ChoppedLabelName = ScriptLocalization.Interactions.DigTree;
	}

	private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.Data.Name == digTreeEventName)
		{
			Progress += 1f;
			OnTreeHit();
			if (Progress >= ProgressTotal)
			{
				OnChoppedDown(true);
			}
		}
	}

	public override void GetLabel()
	{
		base.Label = ChoppedLabelName;
	}

	private void OnTreeHit()
	{
		float angle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.gameObject.transform.position);
		BiomeConstants.Instance.EmitHitImpactEffect(base.transform.position + Vector3.back * 0.5f, angle);
		CameraManager.shakeCamera(0.1f, Utils.GetAngle(PlayerFarming.Instance.gameObject.transform.position, base.transform.position));
		TreeDigParticles.Play();
		if (TreeSpine != null)
		{
			TreeSpine.AnimationState.SetAnimation(0, "hit", true);
			TreeSpine.AnimationState.AddAnimation(0, "animation", true, 0f);
		}
		AudioManager.Instance.PlayOneShot("event:/material/tree_chop", base.transform.position);
		TreeSpine.gameObject.transform.DORestart();
		TreeSpine.gameObject.transform.DOShakePosition(0.1f, 0.1f, 13, 48.8f);
		float progress = Progress / ProgressTotal;
		if (!(BiomeConstants.Instance != null))
		{
			return;
		}
		if (_uiProgressIndicator == null)
		{
			_uiProgressIndicator = BiomeConstants.Instance.ProgressIndicatorTemplate.Spawn(BiomeConstants.Instance.transform, base.transform.position + Vector3.back * 1.5f - BiomeConstants.Instance.transform.position);
			_uiProgressIndicator.Show(progress);
			UIProgressIndicator uiProgressIndicator = _uiProgressIndicator;
			uiProgressIndicator.OnHidden = (Action)Delegate.Combine(uiProgressIndicator.OnHidden, (Action)delegate
			{
				_uiProgressIndicator = null;
			});
		}
		else
		{
			_uiProgressIndicator.SetProgress(progress, 0.1f);
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activated)
		{
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
				StartCoroutine(DoDigStump());
			}
		}
	}

	private void DigStump()
	{
		if (PlayerFarming.Instance.gameObject.transform.position.x < base.transform.position.x)
		{
			PlayerFarming.Instance.GoToAndStop(PlayerPositionLeft, base.gameObject, false, false, delegate
			{
				StartCoroutine(DoDigStump());
			});
		}
		else
		{
			PlayerFarming.Instance.GoToAndStop(PlayerPositionRight, base.gameObject, false, false, delegate
			{
				StartCoroutine(DoDigStump());
			});
		}
	}

	private IEnumerator DoDigStump()
	{
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("actions/dig", 0, true);
		yield return new WaitForSeconds(5f / 6f);
		while (InputManager.Gameplay.GetInteractButtonHeld())
		{
			yield return null;
		}
		skeletonAnimation.AnimationState.Event -= HandleEvent;
		EndChopping();
		Action<Interaction_DigUpStump> playerActivatingEnd = PlayerActivatingEnd;
		if (playerActivatingEnd != null)
		{
			playerActivatingEnd(this);
		}
	}

	public void OnChoppedDown(bool dropLoot)
	{
		TreeDigParticles.Play();
		if (dropLoot)
		{
			int num = 2;
			if (StructureBrain.Data.GrowthStage >= 0f && StructureBrain.Data.GrowthStage <= 2f)
			{
				num = 1;
			}
			else if (StructureBrain.Data.GrowthStage >= 3f && StructureBrain.Data.GrowthStage <= 4f)
			{
				num = 2;
			}
			else if (StructureBrain.Data.GrowthStage >= 5f)
			{
				num = 3;
			}
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
		BiomeConstants.Instance.EmitSmokeExplosionVFX(base.gameObject.transform.position);
		if (Activated)
		{
			EndChopping();
		}
		CameraManager.shakeCamera(1f, Utils.GetAngle(PlayerFarming.Instance.gameObject.transform.position, base.transform.position));
		UnityEvent unityEvent = onDugUp;
		if (unityEvent != null)
		{
			unityEvent.Invoke();
		}
		AudioManager.Instance.PlayOneShot("event:/material/tree_break", base.transform.position);
		woodcutting.StructureBrain.Remove();
	}

	private void EndChopping()
	{
		if (EventListenerActive)
		{
			skeletonAnimation.AnimationState.Event -= HandleEvent;
		}
		Action onCrownReturn = PlayerFarming.OnCrownReturn;
		if (onCrownReturn != null)
		{
			onCrownReturn();
		}
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		StopAllCoroutines();
		Activated = false;
		Interactable = true;
	}
}
