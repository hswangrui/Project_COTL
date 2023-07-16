using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Spine;
using UnityEngine;

public class Interaction_Weed : Interaction
{
	public static List<Interaction_Weed> Weeds = new List<Interaction_Weed>();

	public Structure Structure;

	private Structures_Weeds _StructureBrain;

	public static Action<Interaction_Weed> PlayerActivatingStart;

	public static Action<Interaction_Weed> PlayerActivatingEnd;

	public GameObject BuildSiteProgressUIPrefab;

	private BuildSitePlotProgressUI ProgressUI;

	private string sString;

	private float ShowTimer;

	private bool SubscribedEvent;

	private Vector3 RandomRot;

	private bool EventListenerActive;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_Weeds StructureBrain
	{
		get
		{
			if (_StructureBrain == null && Structure.Brain != null)
			{
				_StructureBrain = Structure.Brain as Structures_Weeds;
			}
			return _StructureBrain;
		}
		set
		{
			_StructureBrain = value;
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

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		Weeds.Add(this);
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		Weeds.Remove(this);
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (PlayerFarming.Instance != null && PlayerFarming.Instance.Spine != null)
		{
			PlayerFarming.Instance.Spine.AnimationState.Event -= HandleEvent;
			SubscribedEvent = false;
		}
		if (StructureBrain != null && StructureBrain.Data != null)
		{
			StructureBrain.Data.Progress = 0f;
			Structures_Weeds structureBrain = StructureBrain;
			structureBrain.OnProgressChanged = (Action)Delegate.Remove(structureBrain.OnProgressChanged, new Action(OnRemovalProgressChanged));
			Structures_Weeds structureBrain2 = StructureBrain;
			structureBrain2.OnComplete = (Action)Delegate.Remove(structureBrain2.OnComplete, new Action(WeedsPulled));
		}
		EventListenerActive = false;
		if (StructureInfo != null && StructureInfo.Destroyed && StructureBrain != null && !StructureBrain.ForceRemoved)
		{
			AudioManager.Instance.PlayOneShot("event:/player/weed_done", base.transform.position);
			BiomeConstants instance = BiomeConstants.Instance;
			if ((object)instance != null)
			{
				instance.EmitSmokeExplosionVFX(base.gameObject.transform.position);
			}
			Vector3 position = PlayerFarming.Instance.transform.position;
			Vector3 velocity = (base.gameObject.transform.position - position) * 5f;
			BiomeConstants.Instance.EmitParticleChunk(BiomeConstants.TypeOfParticle.grass, base.transform.position, velocity, 7);
			if (StructureBrain.DropWeed)
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.GRASS, 1, base.transform.position);
			}
		}
	}

	private void OnBrainAssigned()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		OnRemovalProgressChanged();
		Structures_Weeds structureBrain = StructureBrain;
		structureBrain.OnProgressChanged = (Action)Delegate.Combine(structureBrain.OnProgressChanged, new Action(OnRemovalProgressChanged));
		Structures_Weeds structureBrain2 = StructureBrain;
		structureBrain2.OnComplete = (Action)Delegate.Combine(structureBrain2.OnComplete, new Action(WeedsPulled));
	}

	private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.Data.Name == "Chop")
		{
			EventListenerActive = true;
			AudioManager.Instance.PlayOneShot("event:/player/weed_pick", base.gameObject);
			StructureBrain.PickWeeds(2f);
		}
	}

	private void Start()
	{
		OnRemovalProgressChanged();
		UpdateLocalisation();
		ContinuouslyHold = true;
	}

	public void ShowUI()
	{
		if (ProgressUI != null && !ProgressUI.gameObject.activeSelf)
		{
			ProgressUI.Show();
		}
		ShowTimer = 3f;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = ScriptLocalization.Interactions.ClearWeeds;
	}

	public override void GetLabel()
	{
		if (StructureInfo != null && !StructureInfo.PrioritisedAsBuildingObstruction)
		{
			base.Label = "";
		}
		else
		{
			base.Label = sString;
		}
	}

	private new void Update()
	{
		if (ShowTimer > 0f)
		{
			ShowTimer -= Time.deltaTime;
			if (ShowTimer <= 0f)
			{
				ProgressUI.Hide();
			}
		}
	}

	private void OnRemovalProgressChanged()
	{
		if (StructureBrain == null || StructureBrain.Data == null)
		{
			return;
		}
		if (StructureBrain.Data.Progress == 0f)
		{
			if (ProgressUI != null)
			{
				ProgressUI.gameObject.SetActive(false);
			}
			return;
		}
		if (ProgressUI == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(CanvasConstants.instance.BuildSiteProgressUIPrefab, CanvasConstants.instance.transform);
			ProgressUI = gameObject.GetComponent<BuildSitePlotProgressUI>();
		}
		ShowUI();
		if (ProgressUI != null)
		{
			ProgressUI.UpdateProgress(StructureBrain.Data.Progress / StructureBrain.Data.ProgressTarget);
		}
	}

	private void LateUpdate()
	{
		if (ProgressUI != null && ProgressUI.canvasGroup.alpha > 0f)
		{
			ProgressUI.SetPosition(base.transform.position);
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activating)
		{
			base.OnInteract(state);
			if (PlayerFarming.Instance != null && !SubscribedEvent)
			{
				PlayerFarming.Instance.Spine.AnimationState.Event += HandleEvent;
				SubscribedEvent = true;
			}
			StartCoroutine(DoBuild());
		}
	}

	private IEnumerator DoBuild()
	{
		Activating = true;
		Action<Interaction_Weed> playerActivatingStart = PlayerActivatingStart;
		if (playerActivatingStart != null)
		{
			playerActivatingStart(this);
		}
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("actions/collect-berries", 0, true);
		yield return new WaitForSeconds(14f / 15f);
		while (InputManager.Gameplay.GetInteractButtonHeld())
		{
			yield return null;
		}
		if (EventListenerActive)
		{
			PlayerFarming.Instance.Spine.AnimationState.Event -= HandleEvent;
		}
		SubscribedEvent = false;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		StopAllCoroutines();
		Activating = false;
		Interactable = true;
	}

	private void WeedsPulled()
	{
		if (state == PlayerFarming.Instance._state)
		{
			RumbleManager.Instance.Rumble();
		}
		StructureBrain.Remove();
		if (Activating)
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
		}
		Activating = false;
		Action<Interaction_Weed> playerActivatingEnd = PlayerActivatingEnd;
		if (playerActivatingEnd != null)
		{
			playerActivatingEnd(this);
		}
	}

	private new void OnDestroy()
	{
		if (ProgressUI != null)
		{
			UnityEngine.Object.Destroy(ProgressUI.gameObject);
		}
		if (StructureInfo != null || StructureBrain != null)
		{
			return;
		}
		if (StructureInfo != null && StructureBrain != null && StructureInfo.Destroyed && !StructureBrain.ForceRemoved)
		{
			AudioManager.Instance.PlayOneShot("event:/player/weed_done", base.transform.position);
			BiomeConstants instance = BiomeConstants.Instance;
			if ((object)instance != null)
			{
				instance.EmitSmokeExplosionVFX(base.gameObject.transform.position);
			}
			if (StructureBrain.DropWeed)
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.GRASS, 1, base.transform.position);
			}
		}
		if (Activating)
		{
			StopAllCoroutines();
			state.CURRENT_STATE = StateMachine.State.Idle;
		}
		if (StructureBrain != null)
		{
			Structures_Weeds structureBrain = StructureBrain;
			structureBrain.OnProgressChanged = (Action)Delegate.Remove(structureBrain.OnProgressChanged, new Action(OnRemovalProgressChanged));
			Structures_Weeds structureBrain2 = StructureBrain;
			structureBrain2.OnComplete = (Action)Delegate.Remove(structureBrain2.OnComplete, new Action(WeedsPulled));
		}
	}
}
