using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Lamb.UI;
using Lamb.UI.DeathScreen;
using MMRoomGeneration;
using MMTools;
using UnityEngine;

public class BiomeBaseManager : MonoBehaviour
{
	public static bool HasLoadedData = false;

	public static bool Testing = true;

	public static bool EnterTemple = false;

	private static BiomeBaseManager _Instance;

	public GameObject PlayerSpawnLocation;

	public GameObject PlayerReturnFromDoorRoomLocation;

	public GameObject PlayerReturnFromEndlessLocation;

	public GameObject RecruitSpawnLocation;

	public ParticleSystem recruitParticles;

	public GameObject PlayerGreetLocation;

	public GameObject RunResults;

	public GameObject Church;

	public GenerateRoom DoorRoom;

	public GameObject Lumberjack;

	public bool LoadData = true;

	[EventRef]
	public string biomeAtmosPath;

	[EventRef]
	public string nightBiomeAtmosPath;

	private EventInstance biomeAtmosInstance;

	public GenerateRoom Room;

	public static bool WalkedBack = false;

	private bool wasNight = true;

	private bool createdLoop;

	public Action OnNewRecruitRevealed;

	public BiomeLightingSettings LightingSettings;

	public OverrideLightingProperties overrideLightingProperties;

	private static readonly int DepthThreshold = Shader.PropertyToID("_DepthThreshold");

	public static BiomeBaseManager Instance
	{
		get
		{
			return _Instance;
		}
		set
		{
			_Instance = value;
		}
	}

	public bool SpawnExistingRecruits { get; set; }

	private void Awake()
	{
		if ((PlayerFarming.Location == FollowerLocation.Boss_5 || PlayerFarming.LastLocation == FollowerLocation.Endless) && !DataManager.Instance.CameFromDeathCatFight)
		{
			Room.gameObject.SetActive(false);
		}
		Instance = this;
	}

	private void PlayAtmos()
	{
		if (!createdLoop)
		{
			if (!TimeManager.IsNight)
			{
				AudioManager.Instance.PlayAtmos(biomeAtmosPath);
				wasNight = false;
			}
			else
			{
				AudioManager.Instance.PlayAtmos(nightBiomeAtmosPath);
				wasNight = false;
			}
			createdLoop = true;
		}
		else if (!TimeManager.IsNight)
		{
			if (wasNight)
			{
				AudioManager.Instance.PlayAtmos(biomeAtmosPath);
				wasNight = false;
			}
		}
		else if (!wasNight)
		{
			AudioManager.Instance.PlayAtmos(nightBiomeAtmosPath);
			wasNight = true;
		}
		DataManager.Instance.AllowSaving = true;
	}

	private void OnEnable()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
		UpgradeSystem.OnUpgradeUnlocked += UpgradeBase;
		if (!Application.isEditor || SaveAndLoad.Loaded)
		{
			LoadData = false;
		}
		if (LoadData && !HasLoadedData)
		{
			Debug.Log("LOAD!");
			SaveAndLoad.Load(SaveAndLoad.SAVE_SLOT);
			HasLoadedData = true;
		}
	}

	private IEnumerator CheckMusic()
	{
		if (DataManager.Instance.Followers.Count == 0)
		{
			Debug.Log("0 followers play no follower track");
			AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.NoFollowers);
			while (DataManager.Instance.Followers.Count == 0)
			{
				yield return new WaitForSeconds(0.5f);
			}
			Debug.Log("1 follower play standard ambience track");
			AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.StandardAmbience);
		}
		else
		{
			Debug.Log("Follower Count = " + DataManager.Instance.Followers.Count);
			if (FollowerBrainStats.IsBloodMoon)
			{
				AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.blood_moon);
			}
		}
	}

	private void OnDisable()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
		if (Instance == this)
		{
			Instance = null;
		}
		UpgradeSystem.OnUpgradeUnlocked -= UpgradeBase;
	}

	private void OnNewPhaseStarted()
	{
		PlayAtmos();
		if (DataManager.Instance.ShopKeeperChefEnragedDay <= TimeManager.CurrentDay - 1 && DataManager.Instance.ShopKeeperChefState == 1)
		{
			DataManager.Instance.ShopKeeperChefState = 0;
		}
	}

	private void Start()
	{
		SimulationManager.Pause();
		StartCoroutine(StartSetup());
	}

	private IEnumerator StartSetup()
	{
		FollowerLocation targetLocation = (((PlayerFarming.LastLocation != FollowerLocation.Endless && PlayerFarming.Location != FollowerLocation.Boss_5) || DataManager.Instance.CameFromDeathCatFight) ? FollowerLocation.Base : FollowerLocation.DoorRoom);
		LocationManager[] array = UnityEngine.Object.FindObjectsOfType<LocationManager>();
		LocationManager[] array2 = array;
		foreach (LocationManager location in array2)
		{
			yield return new WaitUntil(() => location.IsInitialized);
		}
		while (LocationManager.GetLocationState(targetLocation) != LocationState.Active)
		{
			yield return null;
		}
		PlayAtmos();
		InitMusic();
		PlacePlayer();
		Church.SetActive(false);
		Lumberjack.SetActive(false);
		if (targetLocation == FollowerLocation.Base)
		{
			DoorRoom.gameObject.SetActive(false);
			Room.SetColliderAndUpdatePathfinding();
		}
		else
		{
			DoorRoom.gameObject.SetActive(true);
			DoorRoom.SetColliderAndUpdatePathfinding();
		}
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			if (LocationManager.GetLocationState(follower.Location) == LocationState.Unloaded && FollowerManager.FindSimFollowerByID(follower.ID) == null)
			{
				SimFollower item = new SimFollower(FollowerBrain.GetOrCreateBrain(follower));
				FollowerManager.SimFollowersAtLocation(follower.Location).Add(item);
			}
		}
		foreach (FollowerLocation item2 in LocationManager.LocationsInState(LocationState.Unloaded))
		{
			foreach (StructuresData item3 in StructureManager.StructuresDataAtLocation(item2))
			{
				if (StructureBrain.FindBrainByID(item3.ID) == null)
				{
					StructureBrain.CreateBrain(item3);
				}
			}
		}
		yield return null;
		HUD_Manager.Instance.Hidden = false;
		HUD_Manager.Instance.Hide(true);
		while ((BaseLocationManager.Instance == null || !BaseLocationManager.Instance.StructuresPlaced) && targetLocation == FollowerLocation.Base)
		{
			yield return null;
		}
		MMTransition.ResumePlay();
		if (DataManager.Instance.LastRunResults == UIDeathScreenOverlayController.Results.BeatenBoss || DataManager.Instance.LastRunResults == UIDeathScreenOverlayController.Results.BeatenBossNoDamage)
		{
			StartCoroutine(BeatenLeaderIE());
		}
		else
		{
			SimulationManager.UnPause();
		}
		if (targetLocation == FollowerLocation.DoorRoom)
		{
			HUD_Manager.Instance.Show();
		}
		yield return new WaitForSeconds(5f);
		if (DataManager.Instance.LastRunResults != UIDeathScreenOverlayController.Results.None)
		{
			switch (DataManager.Instance.LastRunResults)
			{
			case UIDeathScreenOverlayController.Results.Killed:
				CultFaithManager.AddThought(Thought.Cult_DiedInDungeon, -1, 1f);
				break;
			case UIDeathScreenOverlayController.Results.BeatenBoss:
			case UIDeathScreenOverlayController.Results.BeatenBossNoDamage:
				CultFaithManager.AddThought(Thought.Cult_KilledBoss, -1, 1f);
				break;
			case UIDeathScreenOverlayController.Results.BeatenMiniBoss:
				CultFaithManager.AddThought(Thought.Cult_KilledMiniBoss, -1, 1f);
				break;
			}
			DataManager.Instance.LastRunResults = UIDeathScreenOverlayController.Results.None;
		}
	}

	private void Update()
	{
		if (PlayerFarming.Instance != null && DataManager.Instance.InTutorial && DataManager.Instance.Followers_Recruit.Count > 0 && !SpawnExistingRecruits && !MMConversation.isPlaying && Vector3.Distance(PlayerFarming.Instance.transform.position, RecruitSpawnLocation.transform.position) < 8f)
		{
			SpawnExistingRecruits = true;
			Action onNewRecruitRevealed = OnNewRecruitRevealed;
			if (onNewRecruitRevealed != null)
			{
				onNewRecruitRevealed();
			}
			recruitParticles.Play();
			FollowerManager.SpawnExistingRecruits(RecruitSpawnLocation.transform.position);
		}
	}

	private void PlacePlayer()
	{
		if (PlayerFarming.Location == FollowerLocation.Boss_5 && !DataManager.Instance.CameFromDeathCatFight)
		{
			StartCoroutine(SpawnFromChainDoor());
		}
		else if (PlayerFarming.LastLocation == FollowerLocation.Endless)
		{
			PlayerFarming.LastLocation = PlayerFarming.Location;
			StartCoroutine(SpawnFromEndlessPortal());
		}
		else
		{
			LocationManager.LocationManagers[FollowerLocation.Base].PlacePlayer();
			Interaction_BaseTeleporter.Instance.TeleportIn();
		}
		DataManager.Instance.CameFromDeathCatFight = false;
	}

	private IEnumerator SpawnFromChainDoor()
	{
		LocationManager.LocationManagers[FollowerLocation.DoorRoom].PlacePlayer();
		PlayerFarming.Instance.transform.position = new Vector3(0.18f, 45.4f, -3f);
		GameManager.GetInstance().CameraSnapToPosition(PlayerFarming.Instance.transform.position);
		while (LocationManager.GetLocationState(FollowerLocation.DoorRoom) != LocationState.Active)
		{
			yield return null;
		}
		while (PlayerFarming.Instance == null)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		DoorRoom.gameObject.SetActive(true);
		DoorRoom.SetCollider();
		AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.DungeonDoor);
		Room.gameObject.SetActive(false);
		Church.SetActive(false);
		GameManager.RecalculatePaths(true);
	}

	public IEnumerator SpawnFromEndlessPortal()
	{
		LocationManager.LocationManagers[FollowerLocation.DoorRoom].PlacePlayer();
		PlayerFarming.Instance.transform.position = PlayerReturnFromEndlessLocation.transform.position;
		GameManager.GetInstance().CameraSnapToPosition(PlayerFarming.Instance.transform.position);
		while (LocationManager.GetLocationState(FollowerLocation.DoorRoom) != LocationState.Active)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		DoorRoom.gameObject.SetActive(true);
		DoorRoom.SetCollider();
		AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.DungeonDoor);
		Room.gameObject.SetActive(false);
		Church.SetActive(false);
		GameManager.RecalculatePaths(true);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.state.facingAngle = (PlayerFarming.Instance.state.LookAngle = 0f);
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "warp-in-up", false);
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.circleCollider2D.enabled = false;
		PlayerFarming.Instance.transform.DOMove(PlayerReturnFromEndlessLocation.transform.position - Vector3.up * 3f, 1f);
		yield return new WaitForSeconds(2.7f);
		SimulationManager.UnPause();
		PlayerFarming.Instance.circleCollider2D.enabled = true;
		GameManager.GetInstance().OnConversationEnd();
	}

	private IEnumerator EndConversationZoom()
	{
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationEnd();
	}

	public void ToggleChurch()
	{
		if (Church.activeSelf)
		{
			ActivateRoom();
		}
		else
		{
			ActivateChurch();
		}
	}

	public void ActivateChurch()
	{
		AudioManager.Instance.footstepOverride = "event:/material/footstep_hard";
		BiomeConstants.Instance.DisableIndicators();
		WeatherSystemController.Instance.EnteredBuilding();
		AudioManager.Instance.PlayOneShot("event:/boss/frog/transition_intro_zoom", AudioManager.Instance.Listener.gameObject);
		Church.SetActive(true);
		AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.Temple);
		AudioManager.Instance.StopCurrentAtmos();
		Room.gameObject.SetActive(false);
		DoorRoom.gameObject.SetActive(false);
		GameManager.RecalculatePaths(true);
		ChurchFollowerManager.Instance.UpdateChurch();
		DeviceLightingManager.ForceTempleLayout();
	}

	public void ActivateRoom(bool changeMusic = true)
	{
		AudioManager.Instance.footstepOverride = string.Empty;
		WeatherSystemController.Instance.ExitedBuilding();
		AudioManager.Instance.PlayOneShot("event:/boss/frog/transition_zoom_back", AudioManager.Instance.Listener.gameObject);
		if (changeMusic)
		{
			if (FollowerBrainStats.IsBloodMoon)
			{
				AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.blood_moon);
			}
			else
			{
				AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.StandardAmbience);
			}
		}
		Room.gameObject.SetActive(true);
		Room.SetCollider();
		PlayMusic();
		if (!biomeAtmosInstance.isValid())
		{
			if (!TimeManager.IsNight)
			{
				AudioManager.Instance.PlayAtmos(biomeAtmosPath);
			}
			else
			{
				AudioManager.Instance.PlayAtmos(nightBiomeAtmosPath);
			}
		}
		Church.SetActive(false);
		DoorRoom.gameObject.SetActive(false);
		GameManager.RecalculatePaths(true);
		if (changeMusic)
		{
			DeviceLightingManager.ForceBaseLayout();
		}
	}

	public void ActivateDoorRoom()
	{
		AudioManager.Instance.PlayOneShot("event:/boss/frog/transition_intro_zoom", PlayerFarming.Instance.gameObject);
		DoorRoom.gameObject.SetActive(true);
		DoorRoom.SetCollider();
		AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.DungeonDoor);
		Room.gameObject.SetActive(false);
		Church.SetActive(false);
		GameManager.RecalculatePaths(true);
	}

	private void InitMusic()
	{
		AudioManager.Instance.PlayMusic("event:/music/base/base_main");
		StartCoroutine(CheckMusic());
	}

	private void PlayMusic()
	{
	}

	private void PlayChurchMusic()
	{
	}

	public void BtnCreateRecruit()
	{
	}

	public void BtnCreateFollower()
	{
		FollowerManager.CreateNewFollower(FollowerLocation.Base, Vector3.zero);
	}

	public void BtnReset()
	{
		SimulationManager.Pause();
		FollowerManager.Reset();
		StructureManager.Reset();
		UIDynamicNotificationCenter.Reset();
		DeviceLightingManager.Reset();
		//TwitchManager.Abort();
		SaveAndLoad.ResetSave(SaveAndLoad.SAVE_SLOT, false);
		MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, "Base Biome 1", 0.5f, "", null);
	}

	public void BtnNextDay()
	{
		SaveAndLoad.Save();
		SimulationManager.Pause();
		FollowerManager.Followers.Clear();
		MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, "Base Biome 1", 0.5f, "", null);
	}

	public void BtnFakeRunLog()
	{
		Inventory.AddItem(1, 20);
		DataManager.Instance.FollowerTokens++;
		CheatConsole.SkipToPhase(TimeManager.CurrentPhase + 3);
	}

	public void BtnFakeRunBerries()
	{
		Inventory.AddItem(21, 35);
		DataManager.Instance.FollowerTokens++;
		CheatConsole.SkipToPhase(TimeManager.CurrentPhase + 3);
	}

	private void UpgradeBase(UpgradeSystem.Type upgradeType)
	{
		if (upgradeType == UpgradeSystem.Type.Building_Temple2 || upgradeType == UpgradeSystem.Type.Temple_III || upgradeType == UpgradeSystem.Type.Temple_IV)
		{
			StartCoroutine(UpgradeBaseRoutine(upgradeType));
		}
	}

	private IEnumerator UpgradeBaseRoutine(UpgradeSystem.Type upgradeType)
	{
		MonoSingleton<UIManager>.Instance.ForceBlockMenus = true;
		PlayerFarming p = PlayerFarming.Instance;
		PlayerFarming.Instance.gameObject.SetActive(false);
		GameObject shrine = BuildingShrine.Shrines[0].gameObject;
		SpriteRenderer componentInChildren = BuildingShrine.Shrines[0].ShrineCanLevelUp.GetComponentInChildren<SpriteRenderer>();
		if (componentInChildren != null)
		{
			Material material2 = (componentInChildren.material = new Material(componentInChildren.material));
			material2.SetFloat(DepthThreshold, 0f);
		}
		SpriteRenderer component = BuildingShrine.Shrines[0].ShrineCantLevelUp.GetComponent<SpriteRenderer>();
		if (component != null)
		{
			Material material4 = (component.material = new Material(component.material));
			material4.SetFloat(DepthThreshold, 0f);
		}
		StructureBrain shrineBrain = BuildingShrine.Shrines[0].StructureBrain;
		Vector3 shrinePosition = shrine.transform.position;
		BuildingShrine.Shrines[0].EndIndicateHighlighted();
		GameManager.GetInstance().CamFollowTarget.AddTarget(shrine, 2f);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(shrine);
		MonoSingleton<Indicator>.Instance.gameObject.SetActive(false);
		SimulationManager.Pause();
		List<Follower> followers = new List<Follower>();
		foreach (Follower follower in FollowerManager.FollowersAtLocation(FollowerLocation.Base))
		{
			if (FollowerManager.FollowerLocked(follower.Brain.Info.ID) || follower.Brain.Info.CursedState != 0)
			{
				continue;
			}
			Vector3 position = shrine.transform.position;
			Vector3 vector = UnityEngine.Random.insideUnitCircle;
			float num = UnityEngine.Random.Range(3f, 3.5f);
			Vector3 destination = position + vector * num;
			followers.Add(follower);
			follower.HideAllFollowerIcons();
			follower.Brain.HardSwapToTask(new FollowerTask_ManualControl());
			follower.GoTo(destination, delegate
			{
				if (shrine != null)
				{
					follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
					follower.SetBodyAnimation("build", true);
					follower.FacePosition(shrine.transform.position);
				}
			});
		}
		MMVibrate.RumbleContinuous(0f, 0.25f);
		CameraManager.instance.ShakeCameraForDuration(0.05f, 0.2f, 5f);
		AudioManager.Instance.PlayOneShot("event:/Stings/upgrade_cult", shrine.transform.position);
		yield return new WaitForSeconds(5f);
		MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 4f, "", null);
		yield return new WaitForSeconds(1.5f);
		MMVibrate.StopRumble();
		p.gameObject.SetActive(true);
		p.transform.position = shrine.transform.position - Vector3.up * 3f;
		p.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		StructureBrain structureBrain = StructureManager.GetAllStructuresOfType<Structures_Temple>()[0];
		Vector3 position2 = structureBrain.Data.Position;
		StructureBrain.TYPES type3 = structureBrain.Data.Type;
		StructureBrain.TYPES type = StructureBrain.TYPES.NONE;
		StructureBrain.TYPES type2 = StructureBrain.TYPES.NONE;
		StructureManager.RemoveStructure(shrineBrain);
		StructureManager.RemoveStructure(structureBrain);
		switch (upgradeType)
		{
		case UpgradeSystem.Type.Building_Temple2:
			type = StructureBrain.TYPES.SHRINE_II;
			type2 = StructureBrain.TYPES.TEMPLE_II;
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Shrine_II);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Building_Temple2);
			break;
		case UpgradeSystem.Type.Temple_III:
			type = StructureBrain.TYPES.SHRINE_III;
			type2 = StructureBrain.TYPES.TEMPLE_III;
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Shrine_III);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Temple_III);
			break;
		case UpgradeSystem.Type.Temple_IV:
			type = StructureBrain.TYPES.SHRINE_IV;
			type2 = StructureBrain.TYPES.TEMPLE_IV;
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Shrine_IV);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Temple_IV);
			break;
		}
		int counter = 0;
		StructuresData newShrineData = StructuresData.GetInfoByType(type, 0);
		newShrineData.Bounds = shrineBrain.Data.Bounds;
		newShrineData.GridTilePosition = shrineBrain.Data.GridTilePosition;
		newShrineData.PlacementRegionPosition = new Vector3Int((int)shrinePosition.x, (int)shrinePosition.y, 0);
		newShrineData.Fuel = shrineBrain.Data.Fuel;
		newShrineData.FullyFueled = shrineBrain.Data.FullyFueled;
		newShrineData.SoulCount = shrineBrain.Data.SoulCount;
		StructureManager.BuildStructure(FollowerLocation.Base, newShrineData, shrinePosition, new Vector2Int(2, 2), false, delegate(GameObject obj)
		{
			PlacementRegion.Instance.structureBrain.AddStructureToGrid(newShrineData, true);
			shrine = obj;
			counter++;
		});
		StructuresData newTempleData = StructuresData.GetInfoByType(type2, 0);
		newTempleData.Bounds = structureBrain.Data.Bounds;
		newTempleData.GridTilePosition = structureBrain.Data.GridTilePosition;
		newTempleData.PlacementRegionPosition = new Vector3Int((int)position2.x, (int)position2.y, 0);
		PlacementRegion.Instance.structureBrain.RemoveFromGrid(newTempleData.GridTilePosition);
		StructureManager.BuildStructure(FollowerLocation.Base, newTempleData, position2, new Vector2Int(5, 5), false, delegate
		{
			PlacementRegion.Instance.structureBrain.AddStructureToGrid(newTempleData, true);
			counter++;
		});
		while (counter < 2)
		{
			yield return null;
		}
		foreach (Follower item in followers)
		{
			Vector3 position3 = shrine.transform.position;
			Vector3 vector2 = UnityEngine.Random.insideUnitCircle;
			float num2 = UnityEngine.Random.Range(4f, 7f);
			Vector3 position4 = position3 + vector2 * num2;
			item.transform.position = position4;
			item.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
			item.SetBodyAnimation("cheer", true);
			item.FacePosition(shrine.transform.position);
		}
		AudioManager.Instance.PlayOneShot("event:/building/finished_stone", shrine.transform.position);
		yield return new WaitForSeconds(0.5f);
		BiomeConstants.Instance.EmitSmokeExplosionVFX(shrine.transform.position);
		HUD_DisplayName.Play("UI/BaseUpgraded", 3, HUD_DisplayName.Positions.Centre);
		yield return new WaitForSeconds(4f);
		foreach (Follower item2 in followers)
		{
			item2.Brain.CompleteCurrentTask();
		}
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().CamFollowTarget.RemoveTarget(shrine);
		MonoSingleton<UIManager>.Instance.ForceBlockMenus = false;
		p.state.CURRENT_STATE = StateMachine.State.Idle;
		MonoSingleton<Indicator>.Instance.gameObject.SetActive(true);
		SimulationManager.UnPause();
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(0.5f);
		CultFaithManager.AddThought(Thought.Cult_BaseUpgraded, -1, 1f);
	}

	public void BeatenLeaderRoutine()
	{
		StartCoroutine(BeatenLeaderIE());
	}

	private IEnumerator BeatenLeaderIE()
	{
		yield return new WaitForSeconds(0.1f);
		List<Follower> availableFollowers = new List<Follower>();
		foreach (Follower follower in Follower.Followers)
		{
			if (!FollowerManager.FollowerLocked(follower.Brain.Info.ID))
			{
				availableFollowers.Add(follower);
			}
		}
		if (availableFollowers.Count > 0)
		{
			for (int i = 0; i < availableFollowers.Count; i++)
			{
				availableFollowers[i].Brain.HardSwapToTask(new FollowerTask_ManualControl());
				availableFollowers[i].transform.position = GetCirclePosition(availableFollowers, availableFollowers[i]);
				availableFollowers[i].FacePosition(PlayerFarming.Instance.transform.position);
				availableFollowers[i].State.CURRENT_STATE = StateMachine.State.CustomAnimation;
				availableFollowers[i].SetBodyAnimation("dance", true);
			}
			while (LetterBox.IsPlaying)
			{
				yield return null;
			}
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject);
			yield return new WaitForEndOfFrame();
			DOTween.To(() => GameManager.GetInstance().CamFollowTarget.targetDistance, delegate(float x)
			{
				GameManager.GetInstance().CamFollowTarget.targetDistance = x;
			}, 4f, 2f).SetEase(Ease.InSine);
			yield return new WaitForSeconds(2f);
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "show-heart", false);
			AudioManager.Instance.PlayOneShot("event:/monster_heart/monster_heart_beat", PlayerFarming.Instance.transform.position);
			AudioManager.Instance.PlayOneShot("event:/monster_heart/monster_heart_sequence_Short", PlayerFarming.Instance.transform.position);
			MMVibrate.Haptic(MMVibrate.HapticTypes.Success);
			PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
			BiomeConstants.Instance.EmitConfettiVFX(PlayerFarming.Instance.transform.position);
			GameManager.GetInstance().CamFollowTarget.targetDistance = 10f;
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 12f);
			CameraManager.instance.ShakeCameraForDuration(1f, 1.2f, 0.5f);
			yield return new WaitForSeconds(0.25f);
			for (int j = 0; j < availableFollowers.Count; j++)
			{
				availableFollowers[j].SetBodyAnimation("cheer", true);
			}
			yield return new WaitForSeconds(2.75f);
			yield return new WaitForSeconds(2f);
			SimulationManager.UnPause();
			for (int k = 0; k < availableFollowers.Count; k++)
			{
				availableFollowers[k].Brain.CompleteCurrentTask();
			}
			GameManager.GetInstance().OnConversationEnd();
			GameManager.GetInstance().CamFollowTarget.targetDistance = 10f;
		}
	}

	public Vector3 GetCirclePosition(List<Follower> availableFollowers, Follower follower)
	{
		int num = availableFollowers.IndexOf(follower);
		float num2;
		float f;
		if (availableFollowers.Count <= 12)
		{
			num2 = 2f;
			f = (float)num * (360f / (float)availableFollowers.Count) * ((float)Math.PI / 180f);
			return PlayerFarming.Instance.transform.position + new Vector3(num2 * Mathf.Cos(f), num2 * Mathf.Sin(f));
		}
		int num3 = 8;
		if (num < num3)
		{
			num2 = 2f;
			f = (float)num * (360f / (float)Mathf.Min(availableFollowers.Count, num3)) * ((float)Math.PI / 180f);
		}
		else
		{
			num2 = 3f;
			f = (float)(num - num3) * (360f / (float)(availableFollowers.Count - num3)) * ((float)Math.PI / 180f);
		}
		return PlayerFarming.Instance.transform.position + new Vector3(num2 * Mathf.Cos(f), num2 * Mathf.Sin(f));
	}
}
