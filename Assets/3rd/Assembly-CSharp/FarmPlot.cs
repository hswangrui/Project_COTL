using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using I2.Loc;
using Lamb.UI;
using UnityEngine;

public class FarmPlot : Interaction
{
	private const float WATERING_DURATION_SECONDS = 0.95f;

	public GameObject WateredGameObject;

	public GameObject UnWateredGameObject;

	public GameObject CropParent;

	public GameObject FertilizedObject;

	public GameObject ScareCrowSymbol;

	private bool _scareCrowSymbol;

	public static List<FarmPlot> FarmPlots = new List<FarmPlot>();

	public Structure Structure;

	private Structures_FarmerPlot _StructureInfo;

	public List<InventoryItem.ITEM_TYPE> AllowedItemTypesToPlant = new List<InventoryItem.ITEM_TYPE>();

	public List<InventoryItem.ITEM_TYPE> AllowedItemTypesToFertilize = new List<InventoryItem.ITEM_TYPE>();

	public GameObject SeedIndicatorPrefab;

	public List<CropController> CropPrefabs;

	private Dictionary<InventoryItem.ITEM_TYPE, CropController> _cropPrefabsBySeedType;

	[SerializeField]
	private GameObject wateringIndicator;

	private GameObject Player;

	private List<InventoryItem> ToDeposit = new List<InventoryItem>();

	private float WateringTime = 0.95f;

	private float Delay;

	private bool _watered;

	private bool _isCurrentInteraction;

	private bool beingMoved;

	private string sPlant;

	private string sWater;

	private string sFertilize;

	private EventInstance loopedSound;

	public GameObject BirdPrefab;

	public CritterBaseBird Bird;

	private Coroutine cBirdRoutine;

	private GameObject g;

	private bool BirdIsPlaying;

	private bool BirdHasLanded;

	private Vector3 RandomPosition;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_FarmerPlot StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_FarmerPlot;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	public CropController _activeCropController { get; private set; }

	public static FarmPlot GetFarmPlot(int ID)
	{
		foreach (FarmPlot farmPlot in FarmPlots)
		{
			if (farmPlot.StructureInfo.ID == ID)
			{
				return farmPlot;
			}
		}
		return null;
	}

	private void Awake()
	{
		_cropPrefabsBySeedType = new Dictionary<InventoryItem.ITEM_TYPE, CropController>();
		foreach (CropController cropPrefab in CropPrefabs)
		{
			_cropPrefabsBySeedType.Add(cropPrefab.SeedType, cropPrefab);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (StructureBrain != null)
		{
			Structures_FarmerPlot structureBrain = StructureBrain;
			structureBrain.OnGrowthStageChanged = (Action)Delegate.Remove(structureBrain.OnGrowthStageChanged, new Action(UpdateCropImage));
			Structures_FarmerPlot structureBrain2 = StructureBrain;
			structureBrain2.OnBirdAttack = (Action)Delegate.Remove(structureBrain2.OnBirdAttack, new Action(OnBirdAttack));
		}
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		HasSecondaryInteraction = false;
		FarmPlots.Add(this);
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Combine(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(OnStructuresPlaced));
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
		StructureManager.OnStructureMoved = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureMoved, new StructureManager.StructureChanged(OnStructuresMoved));
		StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(OnStructuresMoved));
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
		}
		else
		{
			Structure structure = Structure;
			structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		}
		PlacementRegion.OnBuildingBeganMoving += OnBuildingBeganMoving;
		PlacementRegion.OnBuildingPlaced += OnBuildingPlaced;
	}

	private void OnStructuresPlaced()
	{
		UpdateCropImage();
		UpdateScareCrowSymbol();
	}

	private void OnStructuresMoved(StructuresData structure)
	{
		UpdateCropImage();
		UpdateScareCrowSymbol();
	}

	private void OnBrainAssigned()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		DataManager.Instance.HasBuiltFarmPlot = true;
		Structures_FarmerPlot structureBrain = StructureBrain;
		structureBrain.OnGrowthStageChanged = (Action)Delegate.Combine(structureBrain.OnGrowthStageChanged, new Action(UpdateCropImage));
		Structures_FarmerPlot structureBrain2 = StructureBrain;
		structureBrain2.OnBirdAttack = (Action)Delegate.Combine(structureBrain2.OnBirdAttack, new Action(OnBirdAttack));
		if (StructureInfo.HasBird)
		{
			OnBirdAttack();
		}
		if (BirdIsPlaying)
		{
			if (!BirdHasLanded)
			{
				OnBirdAttack();
			}
			else if ((bool)Bird)
			{
				Bird.gameObject.SetActive(true);
				Bird.skeletonAnimationLODManager.DoUpdate = true;
				cBirdRoutine = StartCoroutine(LandedBird());
			}
		}
		UpdateLocalisation();
		UpdateWatered();
		UpdateCropImage();
	}

	private void OnStructureAdded(StructuresData structure)
	{
		StructureBrain.TYPES type = structure.Type;
		if (type == global::StructureBrain.TYPES.SCARECROW || type == global::StructureBrain.TYPES.SCARECROW_2)
		{
			UpdateScareCrowSymbol();
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		FarmPlots.Remove(this);
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Remove(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(OnStructuresPlaced));
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
		PlacementRegion.OnBuildingBeganMoving -= OnBuildingBeganMoving;
		PlacementRegion.OnBuildingPlaced -= OnBuildingPlaced;
		StructureManager.OnStructureMoved = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureMoved, new StructureManager.StructureChanged(OnStructuresMoved));
		StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(OnStructuresMoved));
		if (StructureBrain != null)
		{
			Structures_FarmerPlot structureBrain = StructureBrain;
			structureBrain.OnGrowthStageChanged = (Action)Delegate.Remove(structureBrain.OnGrowthStageChanged, new Action(UpdateCropImage));
			Structures_FarmerPlot structureBrain2 = StructureBrain;
			structureBrain2.OnBirdAttack = (Action)Delegate.Remove(structureBrain2.OnBirdAttack, new Action(OnBirdAttack));
		}
		if ((bool)Bird)
		{
			Bird.gameObject.SetActive(false);
		}
	}

	private void OnBuildingBeganMoving(int structureID)
	{
		Structure structure = Structure;
		int? obj;
		if ((object)structure == null)
		{
			obj = null;
		}
		else
		{
			StructuresData structure_Info = structure.Structure_Info;
			obj = ((structure_Info != null) ? new int?(structure_Info.ID) : null);
		}
		if (structureID == obj)
		{
			beingMoved = true;
		}
	}

	private void OnBuildingPlaced(int structureID)
	{
		Structure structure = Structure;
		int? obj;
		if ((object)structure == null)
		{
			obj = null;
		}
		else
		{
			StructuresData structure_Info = structure.Structure_Info;
			obj = ((structure_Info != null) ? new int?(structure_Info.ID) : null);
		}
		if (structureID == obj)
		{
			beingMoved = false;
		}
	}

	public override void OnBecomeCurrent()
	{
		base.OnBecomeCurrent();
		_isCurrentInteraction = true;
	}

	public override void OnBecomeNotCurrent()
	{
		base.OnBecomeNotCurrent();
		_isCurrentInteraction = false;
	}

	public override void GetLabel()
	{
		if (StructureBrain.CanPickCrop())
		{
			ContinuouslyHold = false;
			base.Label = "";
		}
		else if (StructureBrain.CanPlantSeed())
		{
			ContinuouslyHold = false;
			base.Label = ((ToDeposit.Count == 0) ? sPlant : "");
		}
		else if (StructureBrain.CanWater())
		{
			ContinuouslyHold = false;
			base.Label = sWater;
		}
		else if (StructureBrain.CanFertilize())
		{
			ContinuouslyHold = true;
			base.Label = sFertilize + " " + InventoryItem.CapacityString(InventoryItem.ITEM_TYPE.POOP, 1);
		}
		else
		{
			ContinuouslyHold = false;
			base.Label = "";
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sPlant = ScriptLocalization.Interactions.FarmPlant;
		sWater = ScriptLocalization.Interactions.FarmWater;
		sFertilize = ScriptLocalization.Interactions.FarmFertilize;
	}

	public override void OnInteract(StateMachine state)
	{
		if (StructureBrain.CanPlantSeed())
		{
			base.OnInteract(state);
			IndicateHighlighted();
			Interactable = false;
			state.CURRENT_STATE = StateMachine.State.InActive;
			state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
			UIItemSelectorOverlayController itemSelector = MonoSingleton<UIManager>.Instance.ShowItemSelector(InventoryItem.AllSeeds, new ItemSelector.Params
			{
				Key = "plant_seeds",
				Context = ItemSelector.Context.Plant,
				Offset = new Vector2(0f, 100f),
				HideOnSelection = true,
				RequiresDiscovery = true,
				ShowEmpty = true
			});
			UIItemSelectorOverlayController uIItemSelectorOverlayController = itemSelector;
			uIItemSelectorOverlayController.OnItemChosen = (Action<InventoryItem.ITEM_TYPE>)Delegate.Combine(uIItemSelectorOverlayController.OnItemChosen, (Action<InventoryItem.ITEM_TYPE>)delegate(InventoryItem.ITEM_TYPE chosenItem)
			{
				Debug.Log(string.Format("ItemToDeposit {0}", chosenItem).Colour(Color.yellow));
				InventoryItem inventoryItem = new InventoryItem();
				inventoryItem.Init((int)chosenItem, 1);
				ToDeposit.Add(inventoryItem);
				AudioManager.Instance.PlayOneShot("event:/material/footstep_sand", base.gameObject);
				ResourceCustomTarget.Create(base.gameObject, PlayerFarming.Instance.transform.position, chosenItem, PlantSeed);
				Inventory.ChangeItemQuantity((int)chosenItem, -1);
			});
			UIItemSelectorOverlayController uIItemSelectorOverlayController2 = itemSelector;
			uIItemSelectorOverlayController2.OnHidden = (Action)Delegate.Combine(uIItemSelectorOverlayController2.OnHidden, (Action)delegate
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
				itemSelector = null;
				Interactable = true;
				base.HasChanged = true;
			});
		}
		else if (StructureBrain.CanWater())
		{
			base.OnInteract(state);
			IndicateHighlighted();
			StopAllCoroutines();
			StartCoroutine(WaterRoutine());
		}
		else if (StructureBrain.CanFertilize())
		{
			base.OnInteract(state);
			IndicateHighlighted();
			InventoryItem.ITEM_TYPE iTEM_TYPE = InventoryItem.ITEM_TYPE.POOP;
			if (Inventory.GetItemQuantity((int)iTEM_TYPE) > 0)
			{
				StructureBrain.AddFertilizer(iTEM_TYPE);
				ResourceCustomTarget.Create(base.gameObject, PlayerFarming.Instance.transform.position, iTEM_TYPE, AddFertilizer);
				Inventory.ChangeItemQuantity((int)iTEM_TYPE, -1);
			}
			else
			{
				MonoSingleton<Indicator>.Instance.PlayShake();
			}
		}
	}

	private IEnumerator WaterRoutine()
	{
		EndIndicateHighlighted();
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		loopedSound = AudioManager.Instance.CreateLoop("event:/player/watering", base.gameObject, true);
		yield return null;
		PlayerFarming.Instance.simpleSpineAnimator.Animate("oldstuff/Farming-water_track", 0, true);
		while (!((WateringTime -= Time.deltaTime) < 0f))
		{
			yield return null;
		}
		AudioManager.Instance.StopLoop(loopedSound);
		StructureInfo.Watered = true;
		StructureInfo.WateredCount = 0;
		WateringTime = 0.95f;
		state.CURRENT_STATE = StateMachine.State.Idle;
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.WaterCrops);
		IndicateHighlighted();
		IndicateHighlighted();
	}

	protected override void Update()
	{
		base.Update();
		if (StructureInfo != null)
		{
			if (WeatherSystemController.Instance.IsRaining && !StructureInfo.Watered)
			{
				StructureInfo.Watered = UnityEngine.Random.Range(0f, 1f) < 0.005f;
			}
			if (StructureInfo.Watered != _watered)
			{
				UpdateWatered();
			}
		}
	}

	private bool IsEnoughFertilizerEnRoute()
	{
		InventoryItem fertilizer = StructureBrain.GetFertilizer();
		return ((fertilizer != null) ? fertilizer.quantity : 0) + ToDeposit.Count >= 1;
	}

	private void PlantSeed()
	{
		InventoryItem.ITEM_TYPE type = (InventoryItem.ITEM_TYPE)ToDeposit[0].type;
		StructureBrain.PlantSeed(type);
		ToDeposit.RemoveAt(0);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.PlantCrops);
		UpdateCropImage();
		base.HasChanged = true;
		checkWaterIndicator();
	}

	private void AddFertilizer()
	{
		UpdateCropImage();
		if (!StructureBrain.CanFertilize())
		{
			base.HasChanged = true;
		}
		checkWaterIndicator();
	}

	private void CheckState()
	{
		Debug.Log("========================= ");
		Debug.Log("checkWaterIndicator()  " + StructureBrain.CanWater());
		Debug.Log("HasPlantedSeed() " + StructureBrain.HasPlantedSeed());
		Debug.Log("!Data.Watered " + StructureBrain.Data.Watered);
		Debug.Log("!IsFullyGrown " + StructureBrain.IsFullyGrown);
	}

	private void checkWaterIndicator()
	{
		if (StructureBrain.CanWater())
		{
			wateringIndicator.SetActive(true);
		}
		else
		{
			wateringIndicator.SetActive(false);
		}
	}

	private void UpdateWatered()
	{
		_watered = StructureInfo.Watered;
		WateredGameObject.SetActive(_watered);
		UnWateredGameObject.SetActive(!_watered);
		base.HasChanged = true;
		checkWaterIndicator();
	}

	private void UpdateScareCrowSymbol()
	{
		if (StructureBrain == null)
		{
			return;
		}
		bool scareCrowSymbol = _scareCrowSymbol;
		_scareCrowSymbol = StructureBrain.NearbyScarecrow();
		if (_scareCrowSymbol == scareCrowSymbol || !(ScareCrowSymbol != null))
		{
			return;
		}
		if (_scareCrowSymbol)
		{
			ScareCrowSymbol.SetActive(true);
			ScareCrowSymbol.transform.localScale = Vector3.zero;
			ScareCrowSymbol.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
		}
		else
		{
			ScareCrowSymbol.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InBack).OnComplete(delegate
			{
				ScareCrowSymbol.SetActive(false);
			});
		}
	}

	public void Harvested()
	{
		StructureBrain.Harvest();
		UpdateCropImage();
		base.HasChanged = true;
	}

	public virtual void UpdateCropImage()
	{
		if (StructureBrain == null)
		{
			return;
		}
		if (StructureBrain.HasPlantedSeed())
		{
			if (_activeCropController == null)
			{
				InventoryItem.ITEM_TYPE iTEM_TYPE = ((StructureBrain.GetPlantedSeed() != null) ? ((InventoryItem.ITEM_TYPE)StructureBrain.GetPlantedSeed().type) : InventoryItem.ITEM_TYPE.NONE);
				if (iTEM_TYPE == InventoryItem.ITEM_TYPE.NONE)
				{
					return;
				}
				CropController cropController = (_cropPrefabsBySeedType.ContainsKey(iTEM_TYPE) ? _cropPrefabsBySeedType[iTEM_TYPE] : null);
				if ((bool)cropController)
				{
					_activeCropController = UnityEngine.Object.Instantiate(cropController, CropParent.transform);
				}
			}
			try
			{
				_activeCropController.SetCropImage(StructureInfo.GrowthStage, StructureInfo.BenefitedFromFertilizer, StructureInfo.Location);
			}
			catch (Exception message)
			{
				Debug.Log(message);
			}
		}
		else if (_activeCropController != null)
		{
			_activeCropController.HideAll();
			UnityEngine.Object.Destroy(_activeCropController.gameObject);
			_activeCropController = null;
		}
		FertilizedObject.SetActive(StructureBrain.HasFertilized());
		if ((bool)_activeCropController)
		{
			_activeCropController.SetGrowRateIcons(StructureBrain.NearbyHarvestTotem());
		}
		checkWaterIndicator();
	}

	public void OnBirdAttack()
	{
		if (cBirdRoutine == null)
		{
			BirdIsPlaying = true;
			BirdHasLanded = false;
			g = UnityEngine.Object.Instantiate(BirdPrefab, new Vector3(0f, 0f, -10f), Quaternion.identity, base.transform);
			g.SetActive(true);
			Bird = g.GetComponent<CritterBaseBird>();
			Bird.ManualControl = true;
			Bird.FlipTimerIntervals /= 2f;
			Bird.EatChance = 0.75f;
			cBirdRoutine = StartCoroutine(BirdRoutine());
		}
	}

	private IEnumerator BirdRoutine()
	{
		yield return new WaitForEndOfFrame();
		Vector3 vector = Vector3.forward * -10f + (Vector3)(UnityEngine.Random.insideUnitCircle * 3f);
		Bird.transform.position = base.transform.position + vector;
		Bird.FlyOutPosition = Vector3.forward * -10f;
		RandomPosition = base.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 0.2f;
		Bird.TargetPosition = RandomPosition;
		Bird.skeletonAnimationLODManager.DoUpdate = true;
		yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 5f));
		Bird.CurrentState = CritterBaseBird.State.FlyingIn;
		while (Bird.CurrentState != CritterBaseBird.State.Idle)
		{
			yield return null;
		}
		Bird.CurrentState = CritterBaseBird.State.Idle;
		cBirdRoutine = StartCoroutine(LandedBird());
	}

	private IEnumerator LandedBird()
	{
		BirdHasLanded = true;
		yield return new WaitForEndOfFrame();
		if (Bird.transform.position == Bird.FlyOutPosition)
		{
			Bird.transform.position = base.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 0.2f;
		}
		float EatingTimer = 0f;
		while (PlayerFarming.Instance == null || Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) > 2f)
		{
			EatingTimer += Time.deltaTime;
			if (PlayerFarming.Instance != null && Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) < 5f && EatingTimer > 5f)
			{
				break;
			}
			yield return null;
		}
		if (EatingTimer > 5f && (bool)PlayerFarming.Instance && Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) > 2f)
		{
			StructureBrain.Harvest();
			StructureInfo.Watered = WeatherSystemController.Instance.IsRaining;
			StructureInfo.WateredCount = 0;
			UpdateCropImage();
		}
		checkWaterIndicator();
		StructureInfo.HasBird = false;
		Bird.FlyOut();
		while (Bird.CurrentState == CritterBaseBird.State.FlyingOut)
		{
			yield return null;
		}
		UnityEngine.Object.Destroy(g);
		cBirdRoutine = null;
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		StopCoroutine(cBirdRoutine);
		cBirdRoutine = null;
		StructureInfo.HasBird = false;
	}
}
