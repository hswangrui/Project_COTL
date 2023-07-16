using System;
using System.Collections;
using DG.Tweening;
using I2.Loc;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class Interaction_Woodcutting : TreeBase
{
	public bool RequireUseOthersFirst;

	private UIProgressIndicator _uiProgressIndicator;

	public Interaction_DigUpStump digUpStump;

	public ParticleSystem TreeHitParticles;

	private string sLabelName;

	public bool Activated;

	public GameObject PlayerPositionLeft;

	public GameObject PlayerPositionRight;

	public SkeletonAnimation skeletonAnimation;

	[SpineEvent("", "", true, false, false)]
	public string chopWoodEventName = "Chop";

	public SkeletonAnimation TreeSpine;

	public GameObject disableOnCut;

	private CircleCollider2D collider;

	private LayerMask collisionMask;

	private bool harvested;

	public UnityEvent onChoppedDown;

	private float growthStageCache;

	public bool EventListenerActive;

	private bool Chopped;

	public bool buttonDown;

	private float ShowTimer;

	private bool helpedFollower;

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		TreeBase.Trees.Add(this);
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
		}
		else
		{
			Structure structure = Structure;
			structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		}
		collisionMask = (int)collisionMask | (1 << LayerMask.NameToLayer("Island"));
		collisionMask = (int)collisionMask | (1 << LayerMask.NameToLayer("Obstacles"));
		if (TreeSpine != null)
		{
			TreeSpine.skeleton.SetSlotsToSetupPose();
		}
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
			Structures_Tree structureBrain3 = base.StructureBrain;
			structureBrain3.OnRegrowTree = (Action)Delegate.Remove(structureBrain3.OnRegrowTree, new Action(OnRegrowTree));
			Structures_Tree structureBrain4 = base.StructureBrain;
			structureBrain4.OnRegrowTreeProgressChanged = (Action)Delegate.Remove(structureBrain4.OnRegrowTreeProgressChanged, new Action(SetSaplingState));
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
		if (base.StructureBrain != null)
		{
			Structure structure = Structure;
			structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
			Structures_Tree structureBrain = base.StructureBrain;
			structureBrain.OnTreeProgressChanged = (Action<int>)Delegate.Remove(structureBrain.OnTreeProgressChanged, new Action<int>(OnTreeHit));
			Structures_Tree structureBrain2 = base.StructureBrain;
			structureBrain2.OnTreeComplete = (Action<bool>)Delegate.Remove(structureBrain2.OnTreeComplete, new Action<bool>(OnChoppedDown));
			Structures_Tree structureBrain3 = base.StructureBrain;
			structureBrain3.OnRegrowTree = (Action)Delegate.Remove(structureBrain3.OnRegrowTree, new Action(OnRegrowTree));
			Structures_Tree structureBrain4 = base.StructureBrain;
			structureBrain4.OnRegrowTreeProgressChanged = (Action)Delegate.Remove(structureBrain4.OnRegrowTreeProgressChanged, new Action(SetSaplingState));
		}
	}

	private void OnBrainAssigned()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (base.StructureBrain.TreeChopped || base.StructureBrain.Data.IsSapling)
		{
			collider = GetComponent<CircleCollider2D>();
			if (collider != null)
			{
				collider.enabled = false;
			}
			if (TreeSpine != null && base.StructureBrain.Data.GrowthStage == 0f)
			{
				TreeSpine.skeleton.SetSkin("cut");
				TreeSpine.skeleton.SetSlotsToSetupPose();
				base.StructureBrain.Remove();
			}
			else
			{
				SetSaplingState();
				TreeSpine.skeleton.SetSlotsToSetupPose();
			}
			if (digUpStump != null)
			{
				digUpStump.enabled = true;
			}
			disableOnCut.SetActive(false);
			Interactable = false;
			if (base.StructureBrain.TreeChopped && !base.StructureBrain.Data.IsSapling)
			{
				base.enabled = false;
			}
		}
		else
		{
			if (digUpStump != null)
			{
				digUpStump.enabled = false;
			}
			SetMidChopState();
		}
		Structures_Tree structureBrain = base.StructureBrain;
		structureBrain.OnTreeProgressChanged = (Action<int>)Delegate.Combine(structureBrain.OnTreeProgressChanged, new Action<int>(OnTreeHit));
		Structures_Tree structureBrain2 = base.StructureBrain;
		structureBrain2.OnTreeComplete = (Action<bool>)Delegate.Combine(structureBrain2.OnTreeComplete, new Action<bool>(OnChoppedDown));
		Structures_Tree structureBrain3 = base.StructureBrain;
		structureBrain3.OnRegrowTree = (Action)Delegate.Combine(structureBrain3.OnRegrowTree, new Action(OnRegrowTree));
		Structures_Tree structureBrain4 = base.StructureBrain;
		structureBrain4.OnRegrowTreeProgressChanged = (Action)Delegate.Combine(structureBrain4.OnRegrowTreeProgressChanged, new Action(SetSaplingState));
		base.transform.position = base.StructureBrain.Data.Position + new Vector3(0f, UnityEngine.Random.Range(-0.02f, 0.02f), 0f);
	}

	private void Start()
	{
		collider = GetComponent<CircleCollider2D>();
		UpdateLocalisation();
	}

	private void CreatUI()
	{
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sLabelName = ScriptLocalization.Interactions.ChopWood;
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

	public override void GetLabel()
	{
		if ((!RequireUseOthersFirst || (RequireUseOthersFirst && DataManager.Instance.FirstTimeChop)) && !Activated && !Chopped)
		{
			EventListenerActive = false;
			if (base.StructureBrain != null)
			{
				base.Label = ((base.StructureBrain.TreeChopped || base.StructureBrain.Data.IsSapling) ? "" : sLabelName);
			}
		}
		else
		{
			base.Label = "";
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
		if (TreeSpine != null)
		{
			SetMidChopState();
			TreeSpine.AnimationState.SetAnimation(0, "hit", true);
			TreeSpine.AnimationState.AddAnimation(0, "animation", true, 0f);
		}
		AudioManager.Instance.PlayOneShot("event:/material/tree_chop", base.transform.position);
		TreeSpine.gameObject.transform.DORestart();
		TreeSpine.gameObject.transform.DOShakePosition(0.1f, 0.1f, 13, 48.8f);
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

	private void SetMidChopState()
	{
		if (base.StructureBrain.Data.Progress > 0f)
		{
			if (base.StructureBrain.TreeHP > 5f)
			{
				TreeSpine.skeleton.SetSkin("normal-chop1");
				TreeSpine.skeleton.SetSlotsToSetupPose();
			}
			else
			{
				TreeSpine.skeleton.SetSkin("normal-chop2");
				TreeSpine.skeleton.SetSlotsToSetupPose();
			}
		}
	}

	private void SetSaplingState()
	{
		if (base.StructureBrain.Data.GrowthStage >= 1f && base.StructureBrain.Data.GrowthStage <= 2f)
		{
			TreeSpine.skeleton.SetSkin("sapling1");
		}
		else if (base.StructureBrain.Data.GrowthStage >= 3f && base.StructureBrain.Data.GrowthStage <= 4f)
		{
			TreeSpine.skeleton.SetSkin("sapling2");
		}
		else if (base.StructureBrain.Data.GrowthStage >= 5f)
		{
			TreeSpine.skeleton.SetSkin("sapling3");
		}
		if (growthStageCache != base.StructureBrain.Data.GrowthStage)
		{
			TreeSpine.AnimationState.SetAnimation(0, "grow", true);
			TreeSpine.AnimationState.AddAnimation(0, "animation", true, 0f);
		}
		growthStageCache = base.StructureBrain.Data.GrowthStage;
		TreeSpine.skeleton.SetSlotsToSetupPose();
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

	public void OnRegrowTree()
	{
		Debug.Log("OnRegrowTree!!!! " + digUpStump);
		base.enabled = true;
		if (TreeSpine != null)
		{
			TreeSpine.skeleton.SetSkin("normal");
			TreeSpine.skeleton.SetSlotsToSetupPose();
		}
		if (digUpStump != null)
		{
			digUpStump.enabled = false;
		}
		if (digUpStump != null)
		{
			Debug.Log("digUpStump.enabled  " + digUpStump.enabled);
		}
		disableOnCut.SetActive(true);
		Interactable = true;
		Chopped = false;
	}

	public void OnChoppedDown(bool dropLoot)
	{
		if (harvested)
		{
			return;
		}
		harvested = true;
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
			if (helpedFollower)
			{
				CultFaithManager.AddThought(Thought.Cult_HelpFollower, -1, 1f);
			}
			EndChopping();
		}
		if (collider != null)
		{
			collider.enabled = false;
		}
		if (TreeSpine != null)
		{
			TreeSpine.skeleton.SetSkin("cut");
			TreeSpine.skeleton.SetSlotsToSetupPose();
		}
		AudioManager.Instance.PlayOneShot("event:/material/tree_break", base.transform.position);
		disableOnCut.SetActive(false);
		Interactable = false;
		Chopped = true;
		if (digUpStump != null)
		{
			digUpStump.enabled = true;
		}
		Debug.Log("REMOVE TREEEEEE");
		base.StructureBrain.Remove();
		BiomeConstants.Instance.EmitSmokeExplosionVFX(base.gameObject.transform.position);
		if ((bool)skeletonAnimation)
		{
			skeletonAnimation.AnimationState.Event -= HandleEvent;
		}
		UnityEvent unityEvent = onChoppedDown;
		if (unityEvent != null)
		{
			unityEvent.Invoke();
		}
	}

	private new void Update()
	{
		if (Structure.Structure_Info == null || !Activated || helpedFollower)
		{
			return;
		}
		foreach (Follower follower in Follower.Followers)
		{
			FollowerTask_ChopTrees followerTask_ChopTrees;
			if ((followerTask_ChopTrees = follower.Brain.CurrentTask as FollowerTask_ChopTrees) != null && followerTask_ChopTrees._treeID == base.StructureInfo.ID && follower.Brain.CurrentTask.State == FollowerTaskState.Doing)
			{
				helpedFollower = true;
				break;
			}
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

	private void ChopTree()
	{
		buttonDown = true;
		if (PlayerFarming.Instance.gameObject.transform.position.x < base.transform.position.x)
		{
			float distance = Vector3.Distance(base.transform.position, PlayerPositionRight.transform.position);
			Vector3 normalized = (PlayerPositionLeft.transform.position - base.transform.position).normalized;
			Vector3 targetPosition = ((Physics2D.Raycast(base.transform.position, normalized, distance, collisionMask).collider != null) ? PlayerPositionRight.transform.position : PlayerPositionLeft.transform.position);
			PlayerFarming.Instance.GoToAndStop(targetPosition, base.gameObject, false, false, delegate
			{
				StartCoroutine(DoWoodCutting());
			});
		}
		else
		{
			float distance2 = Vector3.Distance(base.transform.position, PlayerPositionLeft.transform.position);
			Vector3 normalized2 = (PlayerPositionRight.transform.position - base.transform.position).normalized;
			Vector3 targetPosition2 = ((Physics2D.Raycast(base.transform.position, normalized2, distance2, collisionMask).collider != null) ? PlayerPositionLeft.transform.position : PlayerPositionRight.transform.position);
			PlayerFarming.Instance.GoToAndStop(targetPosition2, base.gameObject, false, false, delegate
			{
				StartCoroutine(DoWoodCutting());
			});
		}
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
