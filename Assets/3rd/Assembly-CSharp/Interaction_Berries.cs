using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;

public class Interaction_Berries : Interaction
{
	public const int BERRY_MAX_HP = 10;

	public static List<Interaction_Berries> Berries = new List<Interaction_Berries>();

	public Structure Structure;

	private Structures_BerryBush _StructureBrain;

	private float BerryPickingTime = 0.125f;

	private float BerryPickingIncrements = 0.5f;

	public bool DisableOnDie;

	private string sLabelName;

	public bool Activated;

	public static Action<Interaction_Berries> PlayerActivatingStart;

	public static Action<Interaction_Berries> PlayerActivatingEnd;

	public int ReservedByFollowerID;

	public GameObject PlayerPositionLeft;

	public GameObject PlayerPositionRight;

	public UnityEvent callBackOnHarvest;

	public GameObject berryBush_Normal;

	public GameObject berryBush_Cut;

	public GameObject berryToShake;

	[Range(0f, 1f)]
	public float BreakCameraShake;

	public int maxParticles = 10;

	public List<Sprite> ParticleChunks;

	public GameObject particleSpawn;

	public float zSpawn;

	private SpriteRenderer spriteRenderer;

	public Color minColor = new Color(0.1764706f, 0.4039216f, 0.3294118f, 1f);

	public Color maxColor = new Color(0.1411765f, 0.2705882f, 0.2627451f, 1f);

	public UIProgressIndicator _uiProgressIndicator;

	private bool harvested;

	public InventoryItem.ITEM_TYPE BerryBushType = InventoryItem.ITEM_TYPE.BERRY;

	private float pickBerriesTime;

	public bool buttonDown;

	private float ShowTimer;

	public bool inTrigger;

	public StructuresData StructureInfo
	{
		get
		{
			if (!(Structure != null))
			{
				return null;
			}
			return Structure.Structure_Info;
		}
	}

	public Structures_BerryBush StructureBrain
	{
		get
		{
			if (_StructureBrain == null && Structure != null && Structure.Brain != null)
			{
				_StructureBrain = Structure.Brain as Structures_BerryBush;
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
		Berries.Add(this);
		base.transform.localScale = Vector3.one;
		if ((bool)Structure)
		{
			Structure structure = Structure;
			structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
			if (Structure.Brain != null)
			{
				OnBrainAssigned();
			}
		}
		harvested = false;
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		Berries.Remove(this);
		if (StructureBrain != null)
		{
			Structures_BerryBush structureBrain = StructureBrain;
			structureBrain.OnTreeProgressChanged = (Action)Delegate.Remove(structureBrain.OnTreeProgressChanged, new Action(OnRemovalProgressChanged));
			Structures_BerryBush structureBrain2 = StructureBrain;
			structureBrain2.OnTreeComplete = (Action<bool>)Delegate.Remove(structureBrain2.OnTreeComplete, new Action<bool>(PickedBerries));
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
		if (StructureBrain != null)
		{
			Structures_BerryBush structureBrain = StructureBrain;
			structureBrain.OnTreeComplete = (Action<bool>)Delegate.Remove(structureBrain.OnTreeComplete, new Action<bool>(PickedBerries));
			Structures_BerryBush structureBrain2 = StructureBrain;
			structureBrain2.OnTreeProgressChanged = (Action)Delegate.Remove(structureBrain2.OnTreeProgressChanged, new Action(OnRemovalProgressChanged));
			Structures_BerryBush structureBrain3 = StructureBrain;
			structureBrain3.OnTreeComplete = (Action<bool>)Delegate.Remove(structureBrain3.OnTreeComplete, new Action<bool>(PickedBerries));
		}
	}

	private void OnBrainAssigned()
	{
		UpdateLocalisation();
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (StructureBrain != null)
		{
			if (StructureBrain.BerryPicked)
			{
				if (DisableOnDie)
				{
					base.gameObject.SetActive(false);
				}
				else
				{
					berryBush_Normal.SetActive(false);
					berryBush_Cut.SetActive(true);
					Interactable = false;
				}
			}
			else
			{
				Structures_BerryBush structureBrain = StructureBrain;
				structureBrain.OnTreeProgressChanged = (Action)Delegate.Combine(structureBrain.OnTreeProgressChanged, new Action(OnRemovalProgressChanged));
				Structures_BerryBush structureBrain2 = StructureBrain;
				structureBrain2.OnTreeComplete = (Action<bool>)Delegate.Combine(structureBrain2.OnTreeComplete, new Action<bool>(PickedBerries));
			}
		}
		if (GetComponentInParent<CropController>() != null)
		{
			StructureBrain.IsCrop = true;
			StructureBrain.CropID = ((GetComponentInParent<FarmPlot>() != null && GetComponentInParent<FarmPlot>().StructureInfo != null) ? GetComponentInParent<FarmPlot>().StructureInfo.ID : (-1));
		}
		else
		{
			StructureBrain.IsCrop = false;
		}
	}

	private void OnStructuresPlaced()
	{
		if (StructureInfo != null && StructureBrain.BerryPicked)
		{
			if (DisableOnDie)
			{
				base.gameObject.SetActive(false);
				return;
			}
			berryBush_Normal.SetActive(false);
			berryBush_Cut.SetActive(true);
			Interactable = false;
		}
	}

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		switch (BerryBushType)
		{
		case InventoryItem.ITEM_TYPE.BERRY:
			sLabelName = ScriptLocalization.Interactions.PickBerries;
			break;
		case InventoryItem.ITEM_TYPE.PUMPKIN:
			sLabelName = ScriptLocalization.Interactions.PickPumpkins;
			break;
		case InventoryItem.ITEM_TYPE.MUSHROOM_BIG:
		{
			string pickMushrooms = ScriptLocalization.Interactions.PickMushrooms;
			pickMushrooms = pickMushrooms.Replace("brown", "#D95B5C");
			sLabelName = pickMushrooms;
			break;
		}
		case InventoryItem.ITEM_TYPE.FLOWER_RED:
			sLabelName = ScriptLocalization.Interactions.PickRedFlower;
			break;
		case InventoryItem.ITEM_TYPE.FLOWER_WHITE:
			sLabelName = "";
			break;
		case InventoryItem.ITEM_TYPE.BEETROOT:
			sLabelName = ScriptLocalization.Interactions.PickBeetroots;
			break;
		case InventoryItem.ITEM_TYPE.CAULIFLOWER:
			sLabelName = ScriptLocalization.Interactions.PickCauliflower;
			break;
		}
	}

	public override void GetLabel()
	{
		if (!Activated && StructureBrain != null)
		{
			base.Label = (StructureBrain.BerryPicked ? "" : sLabelName);
		}
		else
		{
			base.Label = "";
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activated)
		{
			Activated = true;
			base.OnInteract(state);
			StopAllCoroutines();
			StartCoroutine(PickBerries());
			Interactable = false;
		}
	}

	private IEnumerator PickBerries()
	{
		buttonDown = true;
		Activating = true;
		Action<Interaction_Berries> playerActivatingStart = PlayerActivatingStart;
		if (playerActivatingStart != null)
		{
			playerActivatingStart(this);
		}
		PlayerFarming.Instance.TimedAction(10f, null, "actions/collect-berries");
		PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(base.transform.position, base.transform.position);
		StartCoroutine(berryTimer());
		pickBerriesTime = 0.5f + UpgradeSystem.Foraging;
		while (buttonDown && !StructureBrain.BerryPicked && PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.TimedAction)
		{
			yield return null;
		}
		EndPicking();
		Action<Interaction_Berries> playerActivatingEnd = PlayerActivatingEnd;
		if (playerActivatingEnd != null)
		{
			playerActivatingEnd(this);
		}
	}

	private IEnumerator berryTimer()
	{
		while (!StructureBrain.BerryPicked)
		{
			yield return new WaitForSeconds(BerryPickingTime);
			BerryRummage();
		}
	}

	public void BerryRummage()
	{
		StructureBrain.PickBerries(BerryPickingIncrements);
		AudioManager.Instance.PlayOneShot("event:/material/footstep_bush", base.transform.position);
		CameraManager.shakeCamera(0.1f, Utils.GetAngle(PlayerFarming.Instance.gameObject.transform.position, base.transform.position));
		int num = -1;
		if (ParticleChunks.Count > 0)
		{
			while (++num < maxParticles / 5)
			{
				float t = UnityEngine.Random.Range(0, 100) / 100;
				Particle_Chunk.AddNew(base.transform.position, Utils.GetAngle(PlayerFarming.Instance.gameObject.transform.position, base.transform.position) + (float)UnityEngine.Random.Range(-20, 20), Color.Lerp(minColor, maxColor, t), ParticleChunks);
			}
		}
		berryToShake.transform.DORestart();
		berryToShake.transform.DOShakePosition(0.033f, 0.033f, 13, 48.8f);
	}

	public void PickedBerries(bool dropLoot)
	{
		if (harvested)
		{
			return;
		}
		harvested = true;
		BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/player/weed_done", base.transform.position);
		int num = -1;
		if (ParticleChunks.Count > 0)
		{
			while (++num < maxParticles)
			{
				float t = UnityEngine.Random.Range(0, 100) / 100;
				Particle_Chunk.AddNew(base.transform.position, Utils.GetAngle((PlayerFarming.Instance == null) ? base.transform.position : PlayerFarming.Instance.gameObject.transform.position, base.transform.position) + (float)UnityEngine.Random.Range(-20, 20), Color.Lerp(minColor, maxColor, t), ParticleChunks);
			}
		}
		if (Activated)
		{
			EndPicking();
		}
		if (dropLoot)
		{
			StructureBrain.DropBerries(base.transform.position, GetComponentInParent<CropController>() != null, InventoryItem.GetSeedType(BerryBushType));
		}
		CameraManager.shakeCamera(1.5f, Utils.GetAngle((PlayerFarming.Instance == null) ? base.transform.position : PlayerFarming.Instance.gameObject.transform.position, base.transform.position));
		if (callBackOnHarvest != null)
		{
			callBackOnHarvest.Invoke();
		}
		if (Structure != null && StructureInfo != null && !StructureInfo.CanRegrow)
		{
			StructureBrain.Remove();
			return;
		}
		if (DisableOnDie)
		{
			base.gameObject.SetActive(false);
		}
		else
		{
			berryBush_Normal.SetActive(false);
			berryBush_Cut.SetActive(true);
			Interactable = false;
		}
		if (_uiProgressIndicator != null)
		{
			_uiProgressIndicator.Recycle();
			_uiProgressIndicator = null;
		}
	}

	protected override void Update()
	{
		if (Activated && ((InputManager.Gameplay.GetInteractButtonUp() && SettingsManager.Settings.Accessibility.HoldActions) || (PlayerFarming.Instance != null && PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.Meditate)))
		{
			Debug.Log("Up");
			buttonDown = false;
		}
	}

	private void EndPicking()
	{
		Debug.Log("END PICKING!");
		if (PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.TimedAction)
		{
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		}
		StopAllCoroutines();
		Activated = false;
		Interactable = true;
		Activating = false;
	}

	private void UpdateBar()
	{
		if (LetterBox.IsPlaying || StructureBrain == null || StructureBrain.Data == null)
		{
			return;
		}
		float progress = StructureBrain.Data.Progress / StructureBrain.Data.ProgressTarget;
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

	private void OnRemovalProgressChanged()
	{
		UpdateBar();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!inTrigger)
		{
			inTrigger = true;
			StartCoroutine(ShakeObjectTimer());
			base.transform.DOShakeScale(0.5f, new Vector3(-0.1f, 0.05f, 0.01f), 10, 1f);
			AudioManager.Instance.PlayOneShot("event:/material/footstep_bush", collision.transform.position);
		}
	}

	private IEnumerator ShakeObjectTimer()
	{
		yield return new WaitForSeconds(1f);
		inTrigger = false;
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (!inTrigger)
		{
			inTrigger = true;
			if (base.gameObject.activeSelf)
			{
				StartCoroutine(ShakeObjectTimer());
			}
			base.transform.DOShakeScale(0.5f, new Vector3(-0.1f, 0.05f, 0.01f), 10, 1f);
			AudioManager.Instance.PlayOneShot("event:/material/footstep_bush", collision.transform.position);
		}
	}
}
