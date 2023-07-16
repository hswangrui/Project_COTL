using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

[DefaultExecutionOrder(-50)]
public abstract class LocationManager : BaseMonoBehaviour
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CInstantiateStructureAsync_003Ed__61 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncVoidMethodBuilder _003C_003Et__builder;

		public StructuresData structuresData;

		public LocationManager _003C_003E4__this;

		private Task<GameObject> _003Coperation_003E5__2;

		private TaskAwaiter<GameObject> _003C_003Eu__1;

		private void MoveNext()
		{
			int num = _003C_003E1__state;
			LocationManager locationManager = _003C_003E4__this;
			try
			{
				TaskAwaiter<GameObject> awaiter;
				if (num != 0)
				{
					if (!structuresData.PrefabPath.Contains("Assets"))
					{
						structuresData.PrefabPath = "Assets/" + structuresData.PrefabPath + ".prefab";
					}
					_003Coperation_003E5__2 = Addressables.InstantiateAsync(structuresData.PrefabPath).Task;
					awaiter = _003Coperation_003E5__2.GetAwaiter();
					if (!awaiter.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = awaiter;
						_003C_003Et__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
						return;
					}
				}
				else
				{
					awaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(TaskAwaiter<GameObject>);
					num = (_003C_003E1__state = -1);
				}
				awaiter.GetResult();
				locationManager.structuresRequirePlacing--;
				if (_003Coperation_003E5__2.Result == null)
				{
					UnityEngine.Debug.Log("STRUCTURE COULDN'T LOAD");
				}
				else
				{
					locationManager.PlaceStructure(structuresData, _003Coperation_003E5__2.Result.gameObject);
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	public static LocationManager _Instance;

	[SerializeField]
	private Transform SafeSpawnCheckTransform;

	public bool StartsActive = true;

	public GameObject PlayerPrefab;

	public static Action OnFollowersSpawned;

	[SerializeField]
	private BiomeLightingSettings bloodMoonLUT;

	private bool halloweenLutActive;

	private bool isInitialized;

	private float initializerTimer = 5f;

	public static Action OnPlayerLocationSet;

	private int structuresRequirePlacing;

	public static Dictionary<FollowerLocation, LocationManager> LocationManagers = new Dictionary<FollowerLocation, LocationManager>();

	private static Dictionary<FollowerLocation, LocationState> _locationStates = new Dictionary<FollowerLocation, LocationState>();

	private static List<FollowerLocation> _dungeonLocations = new List<FollowerLocation>
	{
		FollowerLocation.Dungeon1_1,
		FollowerLocation.Dungeon1_2,
		FollowerLocation.Dungeon1_3,
		FollowerLocation.Dungeon1_4,
		FollowerLocation.Dungeon1_5,
		FollowerLocation.Dungeon2_2,
		FollowerLocation.Dungeon2_3,
		FollowerLocation.Dungeon2_4,
		FollowerLocation.Dungeon3_1,
		FollowerLocation.Dungeon3_2,
		FollowerLocation.Dungeon3_3,
		FollowerLocation.Dungeon3_4,
		FollowerLocation.Boss_1,
		FollowerLocation.Boss_2,
		FollowerLocation.Boss_3,
		FollowerLocation.Boss_4,
		FollowerLocation.Boss_5,
		FollowerLocation.BountyRoom1,
		FollowerLocation.BountyRoom2,
		FollowerLocation.BountyRoom3,
		FollowerLocation.BountyRoom4,
		FollowerLocation.BountyRoom5,
		FollowerLocation.IntroDungeon,
		FollowerLocation.None
	};

	public abstract FollowerLocation Location { get; }

	public abstract Transform UnitLayer { get; }

	public virtual bool SupportsStructures
	{
		get
		{
			return false;
		}
	}

	public List<StructuresData> StructuresData
	{
		get
		{
			return StructureManager.StructuresDataAtLocation(Location);
		}
	}

	public virtual Transform StructureLayer
	{
		get
		{
			return null;
		}
	}

	public Vector3 SafeSpawnCheckPosition
	{
		get
		{
			if (!(SafeSpawnCheckTransform == null))
			{
				return SafeSpawnCheckTransform.position;
			}
			return Vector3.zero;
		}
	}

	public bool Activatable { get; set; } = true;


	public bool StructuresPlaced { get; private set; }

	public bool FollowersSpawned { get; private set; }

	public System.Random Random { get; private set; }

	public bool IsInitialized
	{
		get
		{
			return isInitialized;
		}
	}

	protected virtual void Awake()
	{
		_Instance = this;
		LocationManager value = null;
		LocationManagers.TryGetValue(Location, out value);
		LocationManagers[Location] = this;
	}

	protected virtual void Start()
	{
		if (DataManager.Instance.LocationSeeds.FirstOrDefault((DataManager.LocationSeedsData x) => x.Location == Location) == null)
		{
			DataManager.Instance.LocationSeeds.Add(new DataManager.LocationSeedsData
			{
				Location = Location,
				Seed = UnityEngine.Random.Range(-2147483647, int.MaxValue)
			});
		}
		Random = new System.Random(DataManager.Instance.LocationSeeds.FirstOrDefault((DataManager.LocationSeedsData x) => x.Location == Location).Seed);
		StartCoroutine(LoadLocationRoutine());
		if (FollowerBrainStats.IsBloodMoon && bloodMoonLUT != null)
		{
			EnableBloodMoon();
		}
	}

	private IEnumerator LoadLocationRoutine()
	{
		BeginLoadLocation(Location);
		if (SupportsStructures)
		{
			yield return new WaitForEndOfFrame();
			yield return StartCoroutine(PlaceStructures());
			PostPlaceStructures();
		}
		if (GameManager.IsDungeon(Location))
		{
			BiomeConstants.Instance.DepthOfFieldTween(0.15f, 8.7f, 26f, 1f, 0f);
		}
		else
		{
			BiomeConstants.Instance.DepthOfFieldTween(0.15f, 8.7f, 26f, 1f, 200f);
		}
		SpawnFollowers();
		FollowersSpawned = true;
		Action onFollowersSpawned = OnFollowersSpawned;
		if (onFollowersSpawned != null)
		{
			onFollowersSpawned();
		}
		EndLoadLocation(Location);
		if (!StartsActive)
		{
			base.gameObject.SetActive(false);
		}
		isInitialized = true;
	}

	protected virtual void OnEnable()
	{
		StartCoroutine(ActivateLocationRoutine());
	}

	private IEnumerator ActivateLocationRoutine()
	{
		if (PlayerFarming.Instance != null)
		{
			PlayerFarming.Instance.transform.SetParent(UnitLayer);
		}
		yield return new WaitForEndOfFrame();
		if ((GetLocationState(Location) == LocationState.Inactive || GetLocationState(Location) == LocationState.Loading) && Activatable)
		{
			ActivateLocation(Location);
		}
	}

	protected virtual void OnDisable()
	{
		DeactivateLocation(Location);
	}

	public static void UpdateLocation()
	{
		Action onPlayerLocationSet = OnPlayerLocationSet;
		if (onPlayerLocationSet != null)
		{
			onPlayerLocationSet();
		}
	}

	protected virtual void Update()
	{
		if (halloweenLutActive && !FollowerBrainStats.IsBloodMoon)
		{
			DisableBloodMoon();
		}
		CheckForInitialization();
	}

	private void OnDestroy()
	{
		if (LocationManagers.ContainsKey(Location))
		{
			LocationManagers[Location] = null;
		}
		UnloadLocation(Location);
		if (_Instance == this)
		{
			_Instance = null;
		}
	}

	private void CheckForInitialization()
	{
		if (!IsInitialized && (initializerTimer -= Time.deltaTime) <= 0f)
		{
			initializerTimer = 5f;
			isInitialized = true;
		}
	}

	public GameObject PlacePlayer()
	{
		PlayerFarming.LastLocation = PlayerFarming.Location;
		PlayerFarming.Location = Location;
		Vector3 startPosition = GetStartPosition(PlayerFarming.LastLocation);
		GameObject gameObject = UnityEngine.Object.Instantiate(PlayerPrefab, startPosition, Quaternion.identity, UnitLayer);
		gameObject.GetComponent<StateMachine>().facingAngle = Utils.GetAngle(gameObject.transform.position, Vector3.zero);
		Action onPlayerLocationSet = OnPlayerLocationSet;
		if (onPlayerLocationSet != null)
		{
			onPlayerLocationSet();
		}
		return gameObject;
	}

	public void PositionPlayer()
	{
		PlayerFarming.Instance.transform.position = GetStartPosition(PlayerFarming.Location);
	}

	public FollowerRecruit SpawnRecruit(FollowerBrain brain, Vector3 position)
	{
		FollowerRecruit followerRecruit = UnityEngine.Object.Instantiate(FollowerManager.RecruitPrefab, position, Quaternion.identity, UnitLayer);
		followerRecruit.name = "Recruit " + brain.Info.Name;
		followerRecruit.Follower.Init(brain, brain.CreateOutfit());
		followerRecruit.Follower.Spine.transform.localScale = new Vector3(-1f, 1f, 1f);
		return followerRecruit;
	}

	public Follower SpawnFollower(FollowerBrain brain, Vector3 position)
	{
		Follower follower = UnityEngine.Object.Instantiate(FollowerManager.FollowerPrefab, position, Quaternion.identity, UnitLayer);
		follower.name = "Follower " + brain.Info.Name;
		follower.Init(brain, brain.CreateOutfit());
		return follower;
	}

	public void SpawnFollowers()
	{
		foreach (SimFollower item in FollowerManager.SimFollowersAtLocation(Location))
		{
			item.Retire();
		}
		for (int num = DataManager.Instance.Followers.Count - 1; num >= 0; num--)
		{
			if (DataManager.Instance.Followers[num] == null || DataManager.Instance.Followers_Dead_IDs.Contains(DataManager.Instance.Followers[num].ID))
			{
				DataManager.Instance.Followers.RemoveAt(num);
			}
		}
		foreach (FollowerInfo follower2 in DataManager.Instance.Followers)
		{
			if (follower2.Location != Location)
			{
				continue;
			}
			bool flag = false;
			foreach (Follower follower3 in Follower.Followers)
			{
				if (follower3.Brain.Info.ID == follower2.ID || DataManager.Instance.Followers_Dead_IDs.Contains(follower2.ID))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Vector3 startPosition = GetStartPosition(Location);
				FollowerBrain orCreateBrain = FollowerBrain.GetOrCreateBrain(follower2);
				Follower follower = SpawnFollower(orCreateBrain, startPosition + (Vector3)UnityEngine.Random.insideUnitCircle * 2f);
				AddFollower(follower, false);
				follower.StartTeleportToTransitionPosition();
			}
		}
	}

	public void SpawnFollower(SimFollower simFollower, bool resume = true)
	{
		simFollower.Retire();
		Vector3 position = GetStartPosition(simFollower.Brain.Location);
		List<Structures_Missionary> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_Missionary>(Location);
		if (simFollower.Brain.Location == FollowerLocation.Missionary && allStructuresOfType.Count > 0)
		{
			position = allStructuresOfType[simFollower.Brain._directInfoAccess.MissionaryIndex].Data.Position;
		}
		List<Structures_Demon_Summoner> allStructuresOfType2 = StructureManager.GetAllStructuresOfType<Structures_Demon_Summoner>(Location);
		if (simFollower.Brain.Location == FollowerLocation.Demon && allStructuresOfType2.Count > 0)
		{
			position = allStructuresOfType2[0].Data.Position;
		}
		Follower follower = SpawnFollower(simFollower.Brain, position);
		AddFollower(follower, resume);
	}

	public void AddFollower(Follower follower, bool resume = true)
	{
		FollowerLocation location = follower.Brain.Location;
		follower.transform.SetParent(UnitLayer);
		if (location != Location)
		{
			follower.transform.position = GetStartPosition(location);
		}
		else
		{
			follower.transform.position = follower.Brain.LastPosition;
		}
		foreach (FollowerPet followerPet in FollowerPet.FollowerPets)
		{
			if (followerPet.Follower == follower)
			{
				followerPet.transform.SetParent(UnitLayer);
				followerPet.transform.position = follower.transform.position;
				followerPet.gameObject.SetActive(true);
			}
		}
		List<Structures_Missionary> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_Missionary>(Location);
		if (location == FollowerLocation.Missionary && allStructuresOfType.Count > 0)
		{
			follower.transform.position = allStructuresOfType[follower.Brain._directInfoAccess.MissionaryIndex].Data.Position;
		}
		follower.Brain.Location = Location;
		FollowerManager.FollowersAtLocation(Location).Add(follower);
		if (resume)
		{
			follower.Brain.LastPosition = follower.transform.position;
		}
		FollowerManager.RetireSimFollowerByID(follower.Brain.Info.ID);
		LocationState locationState = GetLocationState(Location);
		if (locationState == LocationState.Active || locationState == LocationState.Loading)
		{
			if (resume)
			{
				follower.Resume();
			}
			return;
		}
		for (int num = FollowerPet.FollowerPets.Count - 1; num >= 0; num--)
		{
			if (FollowerPet.FollowerPets[num].Follower == follower)
			{
				FollowerPet.FollowerPets[num].gameObject.SetActive(false);
			}
		}
		SimFollower item = new SimFollower(follower.Brain);
		FollowerManager.SimFollowersAtLocation(Location).Add(item);
	}

	protected abstract Vector3 GetStartPosition(FollowerLocation prevLocation);

	public virtual Vector3 GetExitPosition(FollowerLocation destLocation)
	{
		throw new ArgumentException(string.Format("Unexpected GetExitPosition(FollowerLocation.{0}) from Location.{1}", destLocation, Location));
	}

	public IEnumerator PlaceStructures()
	{
		CheckExistingStructures();
		structuresRequirePlacing = 0;
		for (int num = StructuresData.Count - 1; num >= 0; num--)
		{
			StructuresData structuresData = StructuresData[num];
			if (structuresData.Type != 0)
			{
				if ((!DataManager.Instance.DLC_Cultist_Pack && (DataManager.CultistDLCStructures.Contains(structuresData.Type) || DataManager.CultistDLCStructures.Contains(structuresData.ToBuildType))) || (!DataManager.Instance.DLC_Heretic_Pack && (DataManager.HereticDLCStructures.Contains(structuresData.Type) || DataManager.HereticDLCStructures.Contains(structuresData.ToBuildType))))
				{
					StructuresData.RemoveAt(num);
				}
				else if (structuresData.DontLoadMe)
				{
					bool flag = false;
					foreach (Structure structure in Structure.Structures)
					{
						if (structuresData.Position == structure.transform.position)
						{
							structure.Brain = StructureBrain.GetOrCreateBrain(structuresData);
							structure.Brain.AddToGrid();
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						StructuresData.RemoveAt(num);
					}
				}
				else
				{
					if (structuresData.Location == FollowerLocation.None)
					{
						UnityEngine.Debug.LogWarning(string.Format("Placing Structure {0}.{1} with Location.None, updating to {2}", structuresData.Type, structuresData.ID, Location));
						structuresData.Location = Location;
					}
					structuresRequirePlacing++;
					InstantiateStructureAsync(structuresData);
				}
			}
		}
		float timer = 0f;
		while (structuresRequirePlacing > 0)
		{
			timer += Time.unscaledDeltaTime;
			if (timer >= 15f)
			{
				break;
			}
			yield return null;
		}
		StructuresPlaced = true;
		StructureManager.StructuresPlaced onStructuresPlaced = StructureManager.OnStructuresPlaced;
		if (onStructuresPlaced != null)
		{
			onStructuresPlaced();
		}
	}

	[AsyncStateMachine(typeof(_003CInstantiateStructureAsync_003Ed__61))]
	private void InstantiateStructureAsync(StructuresData structuresData)
	{
		_003CInstantiateStructureAsync_003Ed__61 stateMachine = default(_003CInstantiateStructureAsync_003Ed__61);
		stateMachine._003C_003E4__this = this;
		stateMachine.structuresData = structuresData;
		stateMachine._003C_003Et__builder = AsyncVoidMethodBuilder.Create();
		stateMachine._003C_003E1__state = -1;
		AsyncVoidMethodBuilder _003C_003Et__builder = stateMachine._003C_003Et__builder;
		_003C_003Et__builder.Start(ref stateMachine);
	}

	public GameObject PlaceStructure(StructuresData structure, GameObject g)
	{
		if (!(PlacementRegion.Instance == null) && PlacementRegion.Instance.structureBrain != null)
		{
			PlacementRegion.Instance.structureBrain.AddStructureToGrid(structure);
		}
		StructureBrain.ApplyConfigToData(structure);
		g.transform.parent = StructureLayer;
		g.transform.position = structure.Position + structure.Offset;
		g.transform.localScale = new Vector3(structure.Direction, g.transform.localScale.y, g.transform.localScale.z);
		Structure structure2 = g.GetComponent<Structure>();
		if (structure2 == null)
		{
			structure2 = g.GetComponentInChildren<Structure>();
		}
		if (structure2 != null)
		{
			structure2.Brain = StructureBrain.GetOrCreateBrain(structure);
		}
		WorkPlace component = g.GetComponent<WorkPlace>();
		if (component != null)
		{
			component.SetID(g.transform.position.x + "_" + g.transform.position.y);
		}
		return g;
	}

	private void CheckExistingStructures()
	{
		for (int num = Structure.Structures.Count - 1; num >= 0; num--)
		{
			if (Structure.Structures[num].Type == StructureBrain.TYPES.PLACEMENT_REGION)
			{
				CheckExistingStructure(Structure.Structures[num]);
			}
		}
		for (int num2 = Structure.Structures.Count - 1; num2 >= 0; num2--)
		{
			if (Structure.Structures[num2].Type != StructureBrain.TYPES.PLACEMENT_REGION)
			{
				CheckExistingStructure(Structure.Structures[num2]);
			}
		}
	}

	private void CheckExistingStructure(Structure s)
	{
		LocationManager componentInParent = s.GetComponentInParent<LocationManager>();
		if ((componentInParent != null && componentInParent.Location != Location) || s.Structure_Info != null)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < StructuresData.Count; i++)
		{
			if (StructuresData[i].Position == s.transform.position)
			{
				s.Brain = StructureBrain.GetOrCreateBrain(StructuresData[i]);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			s.CreateStructure(Location, s.transform.position);
		}
		if (s.Brain == null || s.Type == StructureBrain.TYPES.PLACEMENT_REGION)
		{
			return;
		}
		using (List<PlacementRegion>.Enumerator enumerator = PlacementRegion.PlacementRegions.GetEnumerator())
		{
			while (enumerator.MoveNext() && !enumerator.Current.TryPlaceExistingStructureAtWorldPosition(s))
			{
			}
		}
	}

	private bool CanPlaceStructureAtWorldPosition(StructuresData s)
	{
		foreach (PlacementRegion placementRegion in PlacementRegion.PlacementRegions)
		{
			PlacementRegion.TileGridTile tileGridTile = placementRegion.GetTileGridTile(s.GridTilePosition);
			if ((tileGridTile == null && !s.IgnoreGrid) || (tileGridTile != null && !tileGridTile.CanPlaceStructure && tileGridTile.ObjectID != -1 && tileGridTile.ObjectID != s.ID && tileGridTile.ObjectOnTile != StructureBrain.TYPES.BUILD_SITE && tileGridTile.ObjectOnTile != StructureBrain.TYPES.BUILDSITE_BUILDINGPROJECT))
			{
				return false;
			}
		}
		return true;
	}

	protected virtual void PostPlaceStructures()
	{
	}

	public static IEnumerable<FollowerLocation> LocationsInState(LocationState state)
	{
		for (int i = 0; i < 85; i++)
		{
			FollowerLocation followerLocation = (FollowerLocation)i;
			if (GetLocationState(followerLocation) == state)
			{
				yield return followerLocation;
			}
		}
	}

	public static LocationState GetLocationState(FollowerLocation location)
	{
		LocationState value = LocationState.Unloaded;
		_locationStates.TryGetValue(location, out value);
		return value;
	}

	public static bool IsDungeonActive()
	{
		bool result = false;
		foreach (FollowerLocation dungeonLocation in _dungeonLocations)
		{
			if (GetLocationState(dungeonLocation) == LocationState.Active)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static bool LocationIsDungeon(FollowerLocation location)
	{
		return _dungeonLocations.Contains(location);
	}

	private static void BeginLoadLocation(FollowerLocation location)
	{
		_locationStates[location] = LocationState.Loading;
	}

	private static void EndLoadLocation(FollowerLocation location)
	{
		_locationStates[location] = LocationState.Active;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.HomeLocation == location && allBrain.CurrentTaskType == FollowerTaskType.FollowPlayer)
			{
				allBrain.FollowingPlayer = false;
				allBrain.CurrentTask.End();
			}
		}
	}

	public static void ActivateLocation(FollowerLocation location)
	{
		_locationStates[location] = LocationState.Active;
		foreach (SimFollower item in FollowerManager.SimFollowersAtLocation(location))
		{
			item.Retire();
		}
		foreach (Follower item2 in FollowerManager.FollowersAtLocation(location))
		{
			item2.ClearPath();
			item2.StartTeleportToTransitionPosition();
		}
		PlayerFarming.LastLocation = PlayerFarming.Location;
		PlayerFarming.Location = location;
		//TwitchManager.LocationChanged(location);
	}

	public static void DeactivateLocation(FollowerLocation location)
	{
		_locationStates[location] = LocationState.Inactive;
		List<Follower> list = FollowerManager.FollowersAtLocation(location);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			list[num].Pause();
			if (list[num].Brain.CurrentTaskType == FollowerTaskType.GreetPlayer)
			{
				list[num].Brain.CurrentTask.Abort();
			}
			SimFollower simFollower = new SimFollower(list[num].Brain);
			simFollower.TransitionFromFollower(list[num]);
			FollowerManager.SimFollowersAtLocation(location).Add(simFollower);
		}
	}

	public static void UnloadLocation(FollowerLocation location)
	{
		_locationStates[location] = LocationState.Unloaded;
		foreach (Follower item in FollowerManager.FollowersAtLocation(location))
		{
			Follower follower = item;
		}
		FollowerManager.Followers[location].Clear();
	}

	public void EnableBloodMoon()
	{
		halloweenLutActive = true;
		LightingManager.Instance.isTODTransition = true;
		LightingManager.Instance.lerpActive = false;
		LightingManager.Instance.globalOverrideSettings = bloodMoonLUT;
		LightingManager.Instance.inGlobalOverride = true;
		LightingManager.Instance.transitionDurationMultiplier = 0f;
		LightingManager.Instance.UpdateLighting(false);
	}

	public void DisableBloodMoon()
	{
		halloweenLutActive = false;
		LightingManager.Instance.isTODTransition = true;
		LightingManager.Instance.lerpActive = true;
		LightingManager.Instance.globalOverrideSettings = null;
		LightingManager.Instance.inGlobalOverride = false;
		LightingManager.Instance.transitionDurationMultiplier = 1f;
		LightingManager.Instance.UpdateLighting(false);
		AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.StandardAmbience);
	}

	public void PopOutDeadBodiesFromGraves(int amount)
	{
		List<Structures_Grave> list = new List<Structures_Grave>(StructureManager.GetAllStructuresOfType<Structures_Grave>());
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].Data.FollowerID == -1)
			{
				list.RemoveAt(num);
			}
		}
		for (int i = 0; i < amount; i++)
		{
			if (list.Count <= 0)
			{
				continue;
			}
			Structures_Grave structures_Grave = list[UnityEngine.Random.Range(0, list.Count)];
			list.Remove(structures_Grave);
			foreach (Grave grafe in Grave.Graves)
			{
				if (grafe.structureBrain.Data.ID == structures_Grave.Data.ID)
				{
					grafe.SpawnDeadBody();
					break;
				}
			}
		}
	}

	public void InstantlyFillAllToilets()
	{
		List<Structures_Outhouse> list = new List<Structures_Outhouse>(StructureManager.GetAllStructuresOfType<Structures_Outhouse>());
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].IsFull)
			{
				list.RemoveAt(num);
			}
		}
		foreach (Structures_Outhouse item in list)
		{
			item.DepositItem(InventoryItem.ITEM_TYPE.POOP, Structures_Outhouse.Capacity(item.Data.Type) - item.GetPoopCount());
		}
	}

	public void InstantlyClearKitchenQueues()
	{
		foreach (Structures_Kitchen item in new List<Structures_Kitchen>(StructureManager.GetAllStructuresOfType<Structures_Kitchen>()))
		{
			item.Data.QueuedMeals.Clear();
			item.Data.QueuedResources.Clear();
			item.FoodStorage.Data.Inventory.Clear();
			item.Data.CurrentCookingMeal = null;
		}
		foreach (Interaction_FollowerKitchen followerKitchen in Interaction_FollowerKitchen.FollowerKitchens)
		{
			followerKitchen.UpdateCurrentMeal();
			followerKitchen.DisplayUI();
			followerKitchen.foodStorage.UpdateFoodDisplayed();
		}
	}

	public void InstantlyFertilizeAllCrops()
	{
		foreach (Structures_FarmerPlot item in new List<Structures_FarmerPlot>(StructureManager.GetAllStructuresOfType<Structures_FarmerPlot>()))
		{
			if (item.HasPlantedSeed())
			{
				item.AddFertilizer(InventoryItem.ITEM_TYPE.POOP);
			}
		}
		foreach (FarmPlot farmPlot in FarmPlot.FarmPlots)
		{
			farmPlot.UpdateCropImage();
		}
	}

	public void InstantlyRefineMaterials()
	{
		foreach (Structures_Refinery item in new List<Structures_Refinery>(StructureManager.GetAllStructuresOfType<Structures_Refinery>()))
		{
			for (int num = item.Data.QueuedResources.Count - 1; num >= 0; num--)
			{
				item.RefineryDeposit();
			}
		}
	}
}
