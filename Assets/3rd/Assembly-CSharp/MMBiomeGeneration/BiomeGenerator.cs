using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using FMOD.Studio;
using FMODUnity;
using I2.Loc;
using Lamb.UI;
using Map;
using MMRoomGeneration;
using MMTools;
using src.UI.Overlays.TutorialOverlay;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

namespace MMBiomeGeneration
{
	public class BiomeGenerator : BaseMonoBehaviour
	{
		public delegate void GetKey();

		public delegate void BiomeAction();

		[Serializable]
		public class OverrideRoom
		{
			public int x;

			public int y;

			public GenerateRoom.ConnectionTypes North;

			public GenerateRoom.ConnectionTypes East;

			public GenerateRoom.ConnectionTypes South;

			public GenerateRoom.ConnectionTypes West;

			public FixedRoom.Generate Generated = FixedRoom.Generate.DontGenerate;

			public string PrefabPath;

			public bool RoomActive = true;
		}

		private class RoomEntranceExit
		{
			public BiomeRoom Room;

			public RoomEntranceExit(BiomeRoom Room, bool Create)
			{
				this.Room = Room;
			}
		}

		[Serializable]
		public class ListOfStoryRooms
		{
			public List<RoomAndStoryPosition> Rooms = new List<RoomAndStoryPosition>();

			public DataManager.Variables StoryVariable;

			public DataManager.Variables LastRun;

			public DataManager.Variables DungeonBeaten;

			public bool PutOnCriticalPath = true;
		}

		[Serializable]
		public class RoomAndStoryPosition
		{
			public int StoryCountRequirement;

			public string RoomPath;

			public bool FloorOne = true;

			public bool FloorTwo = true;

			public bool FloorThree = true;
		}

		[Serializable]
		public class ListOfCustomRoomPrefabs
		{
			public List<string> RoomPaths;

			public bool UseStaticRoomList;

			public static List<string> StaticRoomList = new List<string> { "Assets/_Rooms/Reward Room Gold Rare.prefab", "Assets/_Rooms/Special Blood Sacrafice.prefab", "Assets/_Rooms/Special Coin Gamble.prefab", "Assets/_Rooms/Special Free Tarot Cards.prefab", "Assets/_Rooms/Special Free Health.prefab", "Assets/_Rooms/Special Secret Room 1.prefab" };

			public bool PutOnCriticalPath;

			public bool RemoveIfNoNonCriticalPath;

			[Range(0f, 1f)]
			public float Probability = 1f;

			public bool Redecorate = true;

			public List<VariableAndCondition> ConditionalVariables = new List<VariableAndCondition>();

			public bool MinimumRun;

			public int MinimumRunNumber;

			public bool MaximumRun;

			public int MaximumRunNumber;

			public bool MinimumFollowerCount;

			public int MinimumFollowerCountNumber;

			public bool MaximumFollowerCount;

			public int MaximumFollowerCountNumber;

			public bool SetCustomConnectionType;

			public GenerateRoom.ConnectionTypes ConnectionType;

			public List<FollowerLocation> LocationIsUndiscovered = new List<FollowerLocation>();

			public bool LayerOne;

			public bool LayerTwo;

			public bool LayerThree;

			public bool LayerFour;

			public bool LayerFive;

			public bool LayerSix;

			public bool FloorOne = true;

			public bool FloorTwo = true;

			public bool FloorThree = true;

			internal bool AvailableOnFoor()
			{
				switch (GameManager.CurrentDungeonFloor)
				{
				case 1:
					return FloorOne;
				case 2:
					return FloorTwo;
				case 3:
					return FloorThree;
				default:
					return false;
				}
			}

			internal bool AvailableOnLayer()
			{
				switch (GameManager.CurrentDungeonLayer)
				{
				case 1:
					return LayerOne;
				case 2:
					return LayerTwo;
				case 3:
					return LayerThree;
				case 4:
					return LayerFour;
				case 5:
					return LayerFive;
				case 6:
					return LayerSix;
				default:
					return false;
				}
			}
		}

		[Serializable]
		public class VariableAndCondition
		{
			public DataManager.Variables Variable;

			public bool Condition = true;
		}

		[Serializable]
		public class VariableAndCount
		{
			public DataManager.Variables Variable;

			public int Count;
		}

		[Serializable]
		public class FixedRoom
		{
			public enum Generate
			{
				GenerateOnLoad,
				DontGenerate,
				GenerateCustomOnLoad
			}

			public enum Connection
			{
				ForceOn,
				Optional,
				ForceOff
			}

			public GameObject prefab;

			public Generate GenerateRoom;

			public Connection North;

			public Connection East;

			public Connection South;

			public Connection West;

			[Range(0f, 1f)]
			public float Probability = 1f;

			public List<VariableAndCondition> ConditionalVariables = new List<VariableAndCondition>();

			public bool HasOptional
			{
				get
				{
					if (North != Connection.Optional && East != Connection.Optional && South != Connection.Optional)
					{
						return West == Connection.Optional;
					}
					return true;
				}
			}

			public bool HasForcedOn
			{
				get
				{
					if (North != 0 && East != 0 && South != 0)
					{
						return West == Connection.ForceOn;
					}
					return true;
				}
			}

			public int RequiredConnections
			{
				get
				{
					int num = 0;
					if (North != Connection.ForceOff)
					{
						num++;
					}
					if (East != Connection.ForceOff)
					{
						num++;
					}
					if (South != Connection.ForceOff)
					{
						num++;
					}
					if (West != Connection.ForceOff)
					{
						num++;
					}
					return num;
				}
			}
		}

		private enum Direction
		{
			North,
			East,
			South,
			West
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass147_0
		{
			public bool enemyBombs;

			internal IEnumerator _003CSpawnBombsInRoom_003Eg__SpawnBomb_007C0(Vector3 position, Transform parent, float delay)
			{
				yield return new WaitForSeconds(delay);
				AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/grenade_spawn", position);
				if (enemyBombs)
				{
					Bomb.CreateBomb(position, null, parent);
				}
				else
				{
					Bomb.CreateBomb(position, PlayerFarming.Instance.health, parent);
				}
			}
		}

		public int TotalFloors = 3;

		public const int MAX_ENDLESS_LEVELS = 3;

		public bool TestStartingLayer;

		public int StartingLayer = 1;

		[FormerlySerializedAs("Layer2")]
		public bool NewGamePlus;

		public int PostMiniBossDoorFollowerCount = 3;

		public int GoldToGive = 1;

		public FollowerLocation DungeonLocation;

		[EventRef(MigrateTo = "MusicToTrigger")]
		public string biomeMusicPath;

		public EventReference MusicToTrigger;  
		

	
		[EventRef(MigrateTo = "AtmosToTrigger")]
		public string biomeAtmosPath;

		public EventReference AtmosToTrigger;

		private FMOD.Studio.EventInstance biomeAtmosInstance;

		public bool stopCurrentMusicOnLoad;

		[HideInInspector]
		public static List<string> UsedEncounters = new List<string>();

		public static GetKey OnGetKey;

		public static GetKey OnUseKey;

		[SerializeField]
		private bool _HasKey;

		public bool HasSpawnedRelic;

		private bool ReuseGeneratorRoom;

		public static BiomeGenerator Instance;

		public static Vector2Int BossCoords = new Vector2Int(0, 999);

		public static Vector2Int RespawnRoomCoords = new Vector2Int(-2147483647, -2147483647);

		private System.Random RandomSeed;

		[HideInInspector]
		public List<BiomeRoom> Rooms;

		public int Seed;

		public int NumberOfRooms = 20;

		public bool ForceResource;

		public List<RandomResource.Resource> Resources = new List<RandomResource.Resource>();

		[TermsPopup("")]
		public string DisplayName;

		public GenerateRoom GeneratorRoomPrefab;

		public string EntranceRoomPath;

		public bool LockAndKey = true;

		public string LockedRoomPath;

		public string KeyRoomPath;

		public string BossRoomPath;

		public string LeaderRoomPath;

		public string EndOfFloorRoomPath;

		public string PostBossRoomPath;

		public string BossDoorRoomPath;

		public bool StartWithBossRoomDoor;

		public string KeyPiecePath;

		public string EntranceDoorRoomPath;

		public string RespawnRoomPath;

		public string DeathCatRoomPath;

		public List<ListOfStoryRooms> StoryRooms = new List<ListOfStoryRooms>();

		public List<ListOfCustomRoomPrefabs> CustomDynamicRooms = new List<ListOfCustomRoomPrefabs>();

		public List<FixedRoom> CustomRooms = new List<FixedRoom>();

		public bool OverrideRandomWalk;

		public List<OverrideRoom> OverrideRooms = new List<OverrideRoom>();

		private BiomeRoom lastRoom;

		[Space]
		[SerializeField]
		private bool spawnDemons = true;

		private bool spawnedDemons;

		private static bool WeaponAtEnd;

		[HideInInspector]
		public BiomeRoom PostBossBiomeRoom;

		[HideInInspector]
		public BiomeRoom RoomEntrance;

		[HideInInspector]
		public BiomeRoom RoomExit;

		private List<BiomeRoom> CriticalPath;

		private List<ListOfStoryRooms> RandomiseStoryOrder;

		[HideInInspector]
		public List<ListOfCustomRoomPrefabs> RandomiseOrder;

		private List<BiomeRoom> AvailableRooms;

		public List<GeneraterDecorations> BiomeDecorationSet;

		private List<AsyncOperationHandle<GameObject>> loadedAddressableAssets = new List<AsyncOperationHandle<GameObject>>();

		public int CurrentX;

		public int CurrentY = -1;

		[HideInInspector]
		public bool IsTeleporting;

		[HideInInspector]
		public bool FirstArrival = true;

		public bool ShowDisplayName = true;

		public GameObject Player;

		private StateMachine PlayerState;

		private int PrevCurrentX = -2147483647;

		private int PrevCurrentY = -2147483647;

		private int StartX;

		private int StartY;

		public bool DoFirstArrivalRoutine = true;

		[Space]
		public float HumanoidHealthMultiplier = 1f;

		public static Action OnTutorialShown;

		public static bool RevealHudAbilityIcons = false;

		[HideInInspector]
		public BiomeRoom CurrentRoom;

		private BiomeRoom PrevRoom;

		[HideInInspector]
		public Door North;

		[HideInInspector]
		public Door East;

		[HideInInspector]
		public Door South;

		[HideInInspector]
		public Door West;

		public bool HasKey
		{
			get
			{
				return _HasKey;
			}
			set
			{
				_HasKey = value;
				if (_HasKey)
				{
					GetKey onGetKey = OnGetKey;
					if (onGetKey != null)
					{
						onGetKey();
					}
				}
				if (!_HasKey)
				{
					GetKey onUseKey = OnUseKey;
					if (onUseKey != null)
					{
						onUseKey();
					}
				}
			}
		}

		public BiomeRoom LastRoom
		{
			get
			{
				return lastRoom;
			}
		}

		public int RoomsVisited { get; private set; }

		public BiomeRoom RespawnRoom { get; private set; }

		public BiomeRoom DeathCatRoom { get; private set; }

		public BiomeRoom MysticSellerRoom { get; private set; }

		public static event BiomeAction OnBiomeGenerated;

		public static event BiomeAction OnBiomeChangeRoom;

		public static event BiomeAction OnBiomeLeftRoom;

		public static event BiomeAction OnRoomActive;

		private void InitMusic()
		{
			if (stopCurrentMusicOnLoad)
			{
				AudioManager.Instance.StopCurrentMusic();
			}
			if (!string.IsNullOrEmpty(biomeMusicPath))
			{
				AudioManager.Instance.PlayMusic(biomeMusicPath, false);
			}
			if (!string.IsNullOrEmpty(biomeAtmosPath))
			{
				AudioManager.Instance.PlayAtmos(biomeAtmosPath);
			}
			AudioManager.Instance.SetMusicCombatState(false);
		}

		public void RandomiseSeed()
		{
			Seed = UnityEngine.Random.Range(-2147483647, int.MaxValue);
		}

		private void DailyRun()
		{
			DateTime now = DateTime.Now;
			Seed = int.Parse(string.Concat(string.Concat(((now.Day < 10) ? "0" : "") + now.Day, (now.Month < 10) ? "0" : "", now.Month), now.Year));
		}

		private void Awake()
		{
			Instance = this;
		}

		private void OnEnable()
		{
			Instance = this;
			if (TestStartingLayer && Application.isEditor)
			{
				DataManager.Instance.OnboardedRelics = true;
				UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Relic_Pack_Default);
				UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Relics_Blessed_1);
				UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Relics_Dammed_1);
				UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Relic_Pack1);
				UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Relic_Pack2);
				GameManager.CurrentDungeonLayer = StartingLayer;
				DataManager.Instance.DungeonBossFight = StartingLayer == 4;
				if (NewGamePlus)
				{
					DataManager.Instance.DeathCatBeaten = true;
				}
				if (GameManager.CurrentDungeonLayer == 5)
				{
					GameManager.CurrentDungeonLayer = 4;
					DataManager.Instance.BossesCompleted.Add(DungeonLocation);
				}
				DataManager.SetNewRun();
			}
		}

		private void Start()
		{
			if (DataManager.UseDataManagerSeed)
			{
				Seed = DataManager.RandomSeed.Next(-2147483647, int.MaxValue);
			}
			if (!DungeonSandboxManager.Active)
			{
				StartCoroutine(GenerateRoutine());
			}
		}

		public static bool EncounterAlreadyUsed(string EncounterName)
		{
			return UsedEncounters.Contains(EncounterName);
		}

		public static void SetEncounterAsUsed(string EncounterName)
		{
			UsedEncounters.Add(EncounterName);
		}

		public static void RemoveEncounterAsUsed(string EncounterName)
		{
			UsedEncounters.Remove(EncounterName);
		}

		private IEnumerator GenerateRoutine()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			float Progress19 = -1f;
			float Total = 18f;
			base.transform.position = Vector3.zero;
			RandomSeed = new System.Random(Seed);
			CreateRandomWalk();
			GenerateRoom component = GeneratorRoomPrefab.GetComponent<GenerateRoom>();
			BiomeDecorationSet = new List<GeneraterDecorations>(component.DecorationSetList);
			float num = Progress19 + 1f;
			Progress19 = num;
			MMTransition.UpdateProgress(num / Total, ScriptLocalization.Interactions.GeneratingDungeon + " 1");
			if ((float)stopwatch.ElapsedMilliseconds > 1f / 60f)
			{
				stopwatch.Reset();
				yield return null;
			}
			num = Progress19 + 1f;
			Progress19 = num;
			MMTransition.UpdateProgress(num / Total, ScriptLocalization.Interactions.GeneratingDungeon + " 2");
			SetNeighbours();
			num = Progress19 + 1f;
			Progress19 = num;
			MMTransition.UpdateProgress(num / Total, ScriptLocalization.Interactions.GeneratingDungeon + " 3");
			if ((float)stopwatch.ElapsedMilliseconds > 1f / 60f)
			{
				stopwatch.Reset();
				yield return null;
			}
			num = Progress19 + 1f;
			Progress19 = num;
			MMTransition.UpdateProgress(num / Total, ScriptLocalization.Interactions.GeneratingDungeon + " 4");
			PlaceEntranceAndExit();
			num = Progress19 + 1f;
			Progress19 = num;
			MMTransition.UpdateProgress(num / Total, ScriptLocalization.Interactions.GeneratingDungeon + " 5");
			if ((float)stopwatch.ElapsedMilliseconds > 1f / 60f)
			{
				stopwatch.Reset();
				yield return null;
			}
			num = Progress19 + 1f;
			Progress19 = num;
			MMTransition.UpdateProgress(num / Total, ScriptLocalization.Interactions.GeneratingDungeon + " 6");
			GetCriticalPath();
			if (LockAndKey)
			{
				PlaceLockAndKey();
			}
			if (!string.IsNullOrEmpty(RespawnRoomPath))
			{
				PlaceRespawnRoom();
			}
			if (!string.IsNullOrEmpty(DeathCatRoomPath))
			{
				PlaceDeathCatRoom();
			}
			if (!DataManager.Instance.OnboardedMysticShop && !DataManager.Instance.ForeshadowedMysticShop && DataManager.Instance.GetDungeonLayer(FollowerLocation.Dungeon1_4) >= 2)
			{
				PlaceMysticSellerRoom();
			}
			num = Progress19 + 1f;
			Progress19 = num;
			MMTransition.UpdateProgress(num / Total, ScriptLocalization.Interactions.GeneratingDungeon + " 7");
			if ((float)stopwatch.ElapsedMilliseconds > 1f / 60f)
			{
				stopwatch.Reset();
				yield return null;
			}
			num = Progress19 + 1f;
			Progress19 = num;
			MMTransition.UpdateProgress(num / Total, ScriptLocalization.Interactions.GeneratingDungeon + " 8");
			PlaceFixedCustomRooms();
			num = Progress19 + 1f;
			Progress19 = num;
			MMTransition.UpdateProgress(num / Total, ScriptLocalization.Interactions.GeneratingDungeon + " 9");
			if ((float)stopwatch.ElapsedMilliseconds > 1f / 60f)
			{
				stopwatch.Reset();
				yield return null;
			}
			num = Progress19 + 1f;
			Progress19 = num;
			MMTransition.UpdateProgress(num / Total, ScriptLocalization.Interactions.GeneratingDungeon + " 10");
			yield return StartCoroutine(PlaceStoryRooms());
			if ((float)stopwatch.ElapsedMilliseconds > 1f / 60f)
			{
				stopwatch.Reset();
				yield return null;
			}
			num = Progress19 + 1f;
			Progress19 = num;
			MMTransition.UpdateProgress(num / Total, ScriptLocalization.Interactions.GeneratingDungeon + " 11");
			yield return StartCoroutine(PlaceDynamicCustomRooms());
			num = Progress19 + 1f;
			Progress19 = num;
			MMTransition.UpdateProgress(num / Total, ScriptLocalization.Interactions.GeneratingDungeon + " 12");
			if ((float)stopwatch.ElapsedMilliseconds > 1f / 60f)
			{
				stopwatch.Reset();
				yield return null;
			}
			num = Progress19 + 1f;
			Progress19 = num;
			MMTransition.UpdateProgress(num / Total, ScriptLocalization.Interactions.GeneratingDungeon + " 13");
			yield return StartCoroutine(InstantiatePrefabs());
			InitMusic();
			num = Progress19 + 1f;
			Progress19 = num;
			MMTransition.UpdateProgress(num / Total, ScriptLocalization.Interactions.GeneratingDungeon + " 14");
			if ((float)stopwatch.ElapsedMilliseconds > 1f / 60f)
			{
				stopwatch.Reset();
				yield return null;
			}
			num = Progress19 + 1f;
			Progress19 = num;
			MMTransition.UpdateProgress(num / Total, ScriptLocalization.Interactions.GeneratingDungeon + " 15");
			while (LocationManager.GetLocationState(DungeonLocation) != LocationState.Active && !DungeonSandboxManager.Active)
			{
				yield return null;
			}
			SetRoom(StartX, StartY);
			num = Progress19 + 1f;
			Progress19 = num;
			MMTransition.UpdateProgress(num / Total, ScriptLocalization.Interactions.GeneratingDungeon + " 16");
			if ((float)stopwatch.ElapsedMilliseconds > 1f / 60f)
			{
				stopwatch.Reset();
				yield return null;
			}
			num = Progress19 + 1f;
			Progress19 = num;
			MMTransition.UpdateProgress(num / Total, ScriptLocalization.Interactions.GeneratingDungeon + " 17");
			num = Progress19 + 1f;
			Progress19 = num;
			MMTransition.UpdateProgress(num / Total, ScriptLocalization.Interactions.GeneratingDungeon + " 18");
			if ((float)stopwatch.ElapsedMilliseconds > 1f / 60f)
			{
				stopwatch.Reset();
				yield return null;
			}
			num = Progress19 + 1f;
			MMTransition.UpdateProgress(num / Total, ScriptLocalization.Interactions.GeneratingDungeon + " ");
			while (!CurrentRoom.generateRoom.GeneratedDecorations)
			{
				yield return null;
			}
			BiomeAction onBiomeGenerated = BiomeGenerator.OnBiomeGenerated;
			if (onBiomeGenerated != null)
			{
				onBiomeGenerated();
			}
			EnableRecieveShadowsSpriteRenderer.UpdateSpriteShadows();
			MMTransition.ResumePlay();
			SimulationManager.UnPause();
			stopwatch.Stop();
		}

		public void Generate()
		{
			base.transform.position = Vector3.zero;
			RandomSeed = new System.Random(Seed);
			CreateRandomWalk();
			SetNeighbours();
			PlaceEntranceAndExit();
			GetCriticalPath();
			if (LockAndKey)
			{
				PlaceLockAndKey();
			}
			PlaceStoryRooms();
			PlaceDynamicCustomRooms();
			PlaceFixedCustomRooms();
			StartCoroutine(InstantiatePrefabs());
			LocationManager.LocationManagers[DungeonLocation].SpawnFollowers();
			SetRoom(StartX, StartY);
			BiomeAction onBiomeGenerated = BiomeGenerator.OnBiomeGenerated;
			if (onBiomeGenerated != null)
			{
				onBiomeGenerated();
			}
		}

		private void CreateRandomWalk()
		{
			Rooms = new List<BiomeRoom>();
			int num = NumberOfRooms;
			int x = 0;
			int y = 0;
			BiomeRoom NewRoom;
			if (OverrideRandomWalk)
			{
				StartX = 0;
				StartY = 0;
				foreach (OverrideRoom overrideRoom in OverrideRooms)
				{
					NewRoom = new BiomeRoom(overrideRoom.x, overrideRoom.y, RandomSeed.Next(-2147483647, int.MaxValue), GeneratorRoomPrefab.gameObject);
					Rooms.Add(NewRoom);
					if (overrideRoom.North != 0)
					{
						NewRoom.N_Room = new RoomConnection(overrideRoom.North);
					}
					if (overrideRoom.East != 0)
					{
						NewRoom.E_Room = new RoomConnection(overrideRoom.East);
					}
					if (overrideRoom.South != 0)
					{
						NewRoom.S_Room = new RoomConnection(overrideRoom.South);
					}
					if (overrideRoom.West != 0)
					{
						NewRoom.W_Room = new RoomConnection(overrideRoom.West);
					}
					NewRoom.Active = overrideRoom.RoomActive;
					NewRoom.IsCustom = true;
					NewRoom.Generated = overrideRoom.Generated == FixedRoom.Generate.DontGenerate;
					AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(overrideRoom.PrefabPath);
					asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
					{
						loadedAddressableAssets.Add(obj);
						NewRoom.GameObject = obj.Result;
					};
					NewRoom.GameObjectPath = overrideRoom.PrefabPath;
				}
				{
					foreach (BiomeRoom room2 in Rooms)
					{
						BiomeRoom room;
						if (room2.N_Room != null && (room = BiomeRoom.GetRoom(room2.x, room2.y + 1)) != null)
						{
							room2.N_Room.SetConnectionAndRoom(room, room2.N_Room.ConnectionType);
							room.S_Room = new RoomConnection(room2);
							room.S_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
						}
						if (room2.E_Room != null && (room = BiomeRoom.GetRoom(room2.x + 1, room2.y)) != null)
						{
							room2.E_Room.SetConnectionAndRoom(room, room2.E_Room.ConnectionType);
							room.W_Room = new RoomConnection(room2);
							room.W_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
						}
						if (room2.S_Room != null && (room = BiomeRoom.GetRoom(room2.x, room2.y - 1)) != null)
						{
							room2.S_Room.SetConnectionAndRoom(room, room2.S_Room.ConnectionType);
							room.N_Room = new RoomConnection(room2);
							room.N_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
						}
						if (room2.W_Room != null && (room = BiomeRoom.GetRoom(room2.x - 1, room2.y)) != null)
						{
							room2.W_Room.SetConnectionAndRoom(room, room2.W_Room.ConnectionType);
							room.E_Room = new RoomConnection(room2);
							room.E_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
						}
					}
					return;
				}
			}
			BiomeRoom item = new BiomeRoom(x, y, RandomSeed.Next(-2147483647, int.MaxValue), GeneratorRoomPrefab.gameObject);
			Rooms.Add(item);
			int num2 = 0;
			while (num > 0)
			{
				item = Rooms[RandomSeed.Next(0, Rooms.Count)];
				x = item.x;
				y = item.y;
				int num3 = RandomSeed.Next(0, 4);
				NewRoom = null;
				switch (num3)
				{
				case 0:
					if (BiomeRoom.GetRoom(x, y + 1) == null)
					{
						NewRoom = new BiomeRoom(x, y + 1, RandomSeed.Next(-2147483647, int.MaxValue), GeneratorRoomPrefab.gameObject);
						Rooms.Add(NewRoom);
						item.N_Room = new RoomConnection(NewRoom);
						NewRoom.S_Room = new RoomConnection(item);
						item.N_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
						NewRoom.S_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
					}
					break;
				case 1:
					if (BiomeRoom.GetRoom(x + 1, y) == null)
					{
						NewRoom = new BiomeRoom(x + 1, y, RandomSeed.Next(-2147483647, int.MaxValue), GeneratorRoomPrefab.gameObject);
						Rooms.Add(NewRoom);
						item.E_Room = new RoomConnection(NewRoom);
						NewRoom.W_Room = new RoomConnection(item);
						item.E_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
						NewRoom.W_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
					}
					break;
				case 2:
					if (BiomeRoom.GetRoom(x, y - 1) == null)
					{
						NewRoom = new BiomeRoom(x, y - 1, RandomSeed.Next(-2147483647, int.MaxValue), GeneratorRoomPrefab.gameObject);
						Rooms.Add(NewRoom);
						item.S_Room = new RoomConnection(NewRoom);
						NewRoom.N_Room = new RoomConnection(item);
						item.S_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
						NewRoom.N_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
					}
					break;
				case 3:
					if (BiomeRoom.GetRoom(x - 1, y) == null)
					{
						NewRoom = new BiomeRoom(x - 1, y, RandomSeed.Next(-2147483647, int.MaxValue), GeneratorRoomPrefab.gameObject);
						Rooms.Add(NewRoom);
						item.W_Room = new RoomConnection(NewRoom);
						NewRoom.E_Room = new RoomConnection(item);
						item.W_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
						NewRoom.E_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
					}
					break;
				}
				if (NewRoom != null)
				{
					num--;
				}
				else
				{
					num2++;
				}
			}
		}

		private void ResetFloodDistance()
		{
			foreach (BiomeRoom room in Rooms)
			{
				room.Distance = int.MaxValue;
			}
		}

		private void FloodFillDistance(BiomeRoom r, int Distance)
		{
			if (r != null && r.Distance >= Distance + 1)
			{
				r.Distance = ++Distance;
				if (r.N_Room.Connected)
				{
					FloodFillDistance(r.N_Room.Room, Distance);
				}
				if (r.E_Room.Connected)
				{
					FloodFillDistance(r.E_Room.Room, Distance);
				}
				if (r.S_Room.Connected)
				{
					FloodFillDistance(r.S_Room.Room, Distance);
				}
				if (r.W_Room.Connected)
				{
					FloodFillDistance(r.W_Room.Room, Distance);
				}
			}
		}

		private void PlaceEntranceAndExit()
		{
			if (OverrideRandomWalk)
			{
				return;
			}
			List<RoomEntranceExit> list = new List<RoomEntranceExit>();
			int num = 0;
			int num2 = -1;
			while (++num2 <= 4)
			{
				if (DataManager.HasKeyPieceFromLocation(DungeonLocation, num2))
				{
					num++;
				}
			}
			bool flag = num < 4;
			flag = false;
			foreach (BiomeRoom room in Rooms)
			{
				if (flag)
				{
					if (room.NumConnections == 1 && room.N_Room.Connected && room.EmptyNeighbourPositions.Count > 0 && room.DoAnyOfMyEmptyNeighboursHaveEmptyNeighbours())
					{
						list.Add(new RoomEntranceExit(room, false));
					}
					if (room.NumConnections == 1 && room.E_Room.Connected && room.EmptyNeighbourPositions.Count > 0 && room.DoAnyOfMyEmptyNeighboursHaveEmptyNeighbours())
					{
						list.Add(new RoomEntranceExit(room, false));
					}
					if (room.NumConnections == 1 && room.S_Room.Connected && room.EmptyNeighbourPositions.Count > 0 && room.DoAnyOfMyEmptyNeighboursHaveEmptyNeighbours())
					{
						list.Add(new RoomEntranceExit(room, false));
					}
					if (room.NumConnections == 1 && room.W_Room.Connected && room.EmptyNeighbourPositions.Count > 0 && room.DoAnyOfMyEmptyNeighboursHaveEmptyNeighbours())
					{
						list.Add(new RoomEntranceExit(room, false));
					}
				}
				else
				{
					if (room.NumConnections == 1 && room.N_Room.Connected && room.EmptyNeighbourPositionsIgnoreSouth.Count > 0)
					{
						list.Add(new RoomEntranceExit(room, false));
					}
					if (room.NumConnections == 1 && room.E_Room.Connected && room.EmptyNeighbourPositionsIgnoreSouth.Count > 0)
					{
						list.Add(new RoomEntranceExit(room, false));
					}
					if (room.NumConnections == 1 && room.S_Room.Connected && room.EmptyNeighbourPositionsIgnoreSouth.Count > 0)
					{
						list.Add(new RoomEntranceExit(room, false));
					}
					if (room.NumConnections == 1 && room.W_Room.Connected && room.EmptyNeighbourPositionsIgnoreSouth.Count > 0)
					{
						list.Add(new RoomEntranceExit(room, false));
					}
				}
			}
			List<RoomEntranceExit> list2 = new List<RoomEntranceExit>();
			foreach (BiomeRoom room2 in Rooms)
			{
				if (room2.NumConnections == 1 && room2.N_Room.Connected)
				{
					list2.Add(new RoomEntranceExit(room2, false));
				}
				if (room2.NumConnections == 1 && room2.E_Room.Connected)
				{
					list2.Add(new RoomEntranceExit(room2, false));
				}
				if (room2.NumConnections == 1 && room2.S_Room.Connected)
				{
					list2.Add(new RoomEntranceExit(room2, false));
				}
				if (room2.NumConnections == 1 && room2.W_Room.Connected)
				{
					list2.Add(new RoomEntranceExit(room2, false));
				}
			}
			RoomEntranceExit roomEntranceExit = null;
			RoomEntranceExit roomEntranceExit2 = null;
			int num3 = 0;
			foreach (RoomEntranceExit item in list)
			{
				ResetFloodDistance();
				FloodFillDistance(item.Room, 0);
				foreach (RoomEntranceExit item2 in list2)
				{
					if (num3 < item2.Room.Distance && item != item2)
					{
						num3 = item2.Room.Distance;
						roomEntranceExit = item;
						roomEntranceExit2 = item2;
					}
				}
			}
			WeaponAtEnd = GameManager.CurrentDungeonFloor > 1 && RandomSeed.NextDouble() < 0.6499999761581421 && MapManager.Instance.CurrentNode.nodeType != NodeType.MiniBossFloor;
			if (MapManager.Instance != null && MapManager.Instance.CurrentNode != null && !DataManager.Instance.DungeonBossFight && MapManager.Instance.CurrentNode.nodeType == NodeType.MiniBossFloor)
			{
				roomEntranceExit.Room.IsCustom = true;
				roomEntranceExit.Room.GameObjectPath = BossRoomPath;
				if (GameManager.Layer2 && !roomEntranceExit.Room.GameObjectPath.Contains("_P2.prefab"))
				{
					roomEntranceExit.Room.GameObjectPath = roomEntranceExit.Room.GameObjectPath.Replace(".prefab", "_P2.prefab");
				}
				if (roomEntranceExit.Room.N_Room.Connected)
				{
					roomEntranceExit.Room.N_Room.Room.S_Room.SetConnection(GenerateRoom.ConnectionTypes.Boss);
				}
				if (roomEntranceExit.Room.E_Room.Connected)
				{
					roomEntranceExit.Room.E_Room.Room.W_Room.SetConnection(GenerateRoom.ConnectionTypes.Boss);
				}
				if (roomEntranceExit.Room.S_Room.Connected)
				{
					roomEntranceExit.Room.S_Room.Room.N_Room.SetConnection(GenerateRoom.ConnectionTypes.Boss);
				}
				if (roomEntranceExit.Room.W_Room.Connected)
				{
					roomEntranceExit.Room.W_Room.Room.E_Room.SetConnection(GenerateRoom.ConnectionTypes.Boss);
				}
				roomEntranceExit.Room.IsBoss = true;
				roomEntranceExit.Room.Generated = false;
				new Vector2Int(roomEntranceExit.Room.x, roomEntranceExit.Room.y);
				RoomExit = roomEntranceExit.Room;
				if (!string.IsNullOrEmpty(KeyPiecePath))
				{
					BiomeRoom biomeRoom = PlacePostBossRoom(roomEntranceExit.Room.EmptyNeighbourPositions, KeyPiecePath, roomEntranceExit);
					if (GameManager.Layer2 && !PostBossRoomPath.Contains("_P2.prefab"))
					{
						PostBossRoomPath = PostBossRoomPath.Replace(".prefab", "_P2.prefab");
					}
					PlacePostBossRoom(biomeRoom.EmptyNeighbourPositionsIgnoreSouth, PostBossRoomPath, new RoomEntranceExit(biomeRoom, false)).N_Room.SetConnection(GenerateRoom.ConnectionTypes.LeaderBoss);
				}
				else
				{
					if (GameManager.Layer2 && !PostBossRoomPath.Contains("_P2.prefab"))
					{
						PostBossRoomPath = PostBossRoomPath.Replace(".prefab", "_P2.prefab");
					}
					if (GameManager.Layer2 && !EndOfFloorRoomPath.Contains("_P2.prefab"))
					{
						EndOfFloorRoomPath = EndOfFloorRoomPath.Replace(".prefab", "_P2.prefab");
					}
					if (GameManager.Layer2 && !BossDoorRoomPath.Contains("_P2.prefab"))
					{
						BossDoorRoomPath = BossDoorRoomPath.Replace(".prefab", "_P2.prefab");
					}
					if (DungeonSandboxManager.Active)
					{
						if (MapManager.Instance.CurrentNode == MapManager.Instance.CurrentMap.GetFinalBossNode())
						{
							BiomeRoom biomeRoom2 = PlacePostBossRoom(roomEntranceExit.Room.EmptyNeighbourPositionsIgnoreSouth, DataManager.Instance.DungeonCompleted(DungeonLocation, GameManager.Layer2) ? PostBossRoomPath : BossDoorRoomPath, roomEntranceExit);
							biomeRoom2.N_Room.SetConnection(GenerateRoom.ConnectionTypes.False);
							lastRoom = biomeRoom2;
						}
						else
						{
							BiomeRoom biomeRoom3 = PlacePostBossRoom(roomEntranceExit.Room.EmptyNeighbourPositionsIgnoreSouth, EndOfFloorRoomPath, roomEntranceExit);
							biomeRoom3.N_Room.SetConnection(GenerateRoom.ConnectionTypes.NextLayer);
							lastRoom = biomeRoom3;
						}
					}
					else if (MapManager.Instance.CurrentNode == MapManager.Instance.CurrentMap.GetBossNode())
					{
						BiomeRoom biomeRoom4 = PlacePostBossRoom(roomEntranceExit.Room.EmptyNeighbourPositionsIgnoreSouth, DataManager.Instance.DungeonCompleted(DungeonLocation, GameManager.Layer2) ? PostBossRoomPath : BossDoorRoomPath, roomEntranceExit);
						if (GameManager.DungeonEndlessLevel >= 3 && DataManager.Instance.DungeonCompleted(DungeonLocation, GameManager.Layer2))
						{
							biomeRoom4.N_Room.SetConnection(GenerateRoom.ConnectionTypes.False);
						}
						else
						{
							biomeRoom4.N_Room.SetConnection(DataManager.Instance.DungeonCompleted(DungeonLocation, GameManager.Layer2) ? GenerateRoom.ConnectionTypes.NextLayer : GenerateRoom.ConnectionTypes.LeaderBoss);
						}
						lastRoom = biomeRoom4;
					}
					else
					{
						BiomeRoom biomeRoom5 = PlacePostBossRoom(roomEntranceExit.Room.EmptyNeighbourPositionsIgnoreSouth, EndOfFloorRoomPath, roomEntranceExit);
						biomeRoom5.N_Room.SetConnection(GenerateRoom.ConnectionTypes.NextLayer);
						lastRoom = biomeRoom5;
					}
				}
			}
			else
			{
				GenerateRoom.ConnectionTypes connection = GenerateRoom.ConnectionTypes.NextLayer;
				string text = EndOfFloorRoomPath;
				if (GameManager.Layer2 && !text.Contains("_P2.prefab"))
				{
					text = text.Replace(".prefab", "_P2.prefab");
				}
				if (GameManager.Layer2 && !BossDoorRoomPath.Contains("_P2.prefab"))
				{
					BossDoorRoomPath = BossDoorRoomPath.Replace(".prefab", "_P2.prefab");
				}
				if (MapManager.Instance != null && MapManager.Instance.CurrentNode != null && ((MapManager.Instance.CurrentNode.nodeType == NodeType.MiniBossFloor && DataManager.Instance.DungeonBossFight) || MapManager.Instance.CurrentNode.nodeType == NodeType.Boss))
				{
					roomEntranceExit.Room.GameObjectPath = BossDoorRoomPath;
					connection = GenerateRoom.ConnectionTypes.LeaderBoss;
					text = BossDoorRoomPath;
				}
				roomEntranceExit.Room.IsCustom = false;
				roomEntranceExit.Room.Generated = false;
				RoomExit = roomEntranceExit.Room;
				BiomeRoom biomeRoom6 = PlacePostBossRoom(roomEntranceExit.Room.EmptyNeighbourPositionsIgnoreSouth, text, roomEntranceExit);
				biomeRoom6.N_Room.SetConnection(connection);
				biomeRoom6.HasWeapon = WeaponAtEnd;
				lastRoom = biomeRoom6;
			}
			if (!string.IsNullOrEmpty(EntranceRoomPath))
			{
				roomEntranceExit2.Room.IsCustom = true;
				roomEntranceExit2.Room.GameObjectPath = EntranceRoomPath;
				if (GameManager.Layer2 && !roomEntranceExit2.Room.GameObjectPath.Contains("_P2.prefab"))
				{
					roomEntranceExit2.Room.GameObjectPath = roomEntranceExit2.Room.GameObjectPath.Replace(".prefab", "_P2.prefab");
				}
			}
			else
			{
				roomEntranceExit2.Room.IsCustom = false;
				roomEntranceExit2.Room.GameObject = GeneratorRoomPrefab.gameObject;
			}
			StartX = roomEntranceExit2.Room.x;
			StartY = roomEntranceExit2.Room.y;
			RoomEntrance = roomEntranceExit2.Room;
			roomEntranceExit2.Room.Active = false;
			if (!roomEntranceExit2.Room.S_Room.Connected)
			{
				roomEntranceExit2.Room.S_Room.SetConnection(GenerateRoom.ConnectionTypes.Entrance);
			}
			else if (!roomEntranceExit2.Room.W_Room.Connected)
			{
				roomEntranceExit2.Room.W_Room.SetConnection(GenerateRoom.ConnectionTypes.Entrance);
			}
			else if (!roomEntranceExit2.Room.E_Room.Connected)
			{
				roomEntranceExit2.Room.E_Room.SetConnection(GenerateRoom.ConnectionTypes.Entrance);
			}
			else if (!roomEntranceExit2.Room.N_Room.Connected)
			{
				roomEntranceExit2.Room.N_Room.SetConnection(GenerateRoom.ConnectionTypes.Entrance);
			}
			if (StartWithBossRoomDoor && GameManager.CurrentDungeonFloor == 1)
			{
				if (GameManager.Layer2 && !BossDoorRoomPath.Contains("_P2.prefab"))
				{
					BossDoorRoomPath = BossDoorRoomPath.Replace(".prefab", "_P2.prefab");
				}
				BiomeRoom biomeRoom7 = new BiomeRoom(-999, -999, RandomSeed.Next(-2147483647, int.MaxValue), GeneratorRoomPrefab.gameObject);
				Rooms.Add(biomeRoom7);
				biomeRoom7.GameObjectPath = BossDoorRoomPath;
				biomeRoom7.Active = true;
				biomeRoom7.IsCustom = true;
				biomeRoom7.Generated = false;
				StartX = biomeRoom7.x;
				StartY = biomeRoom7.y;
				biomeRoom7.N_Room = new RoomConnection(GenerateRoom.ConnectionTypes.Exit);
				biomeRoom7.E_Room = new RoomConnection(GenerateRoom.ConnectionTypes.DungeonFirstRoom);
				biomeRoom7.S_Room = new RoomConnection(GenerateRoom.ConnectionTypes.Entrance);
				biomeRoom7.W_Room = new RoomConnection(GenerateRoom.ConnectionTypes.False);
			}
			if (!string.IsNullOrEmpty(LeaderRoomPath) && MapManager.Instance != null && MapManager.Instance.CurrentNode != null && !DungeonSandboxManager.Active)
			{
				BiomeRoom biomeRoom8 = PlacePostBossRoom(new List<Vector2Int> { BossCoords }, LeaderRoomPath, roomEntranceExit, false);
				biomeRoom8.S_Room.SetConnectionAndRoom(roomEntranceExit.Room, GenerateRoom.ConnectionTypes.False);
				biomeRoom8.N_Room.ConnectionType = GenerateRoom.ConnectionTypes.DoorRoom;
				biomeRoom8.Generated = true;
				biomeRoom8.IsBoss = true;
			}
		}

		private BiomeRoom PlacePostBossRoom(List<Vector2Int> EmptyNeighbourPositions, string RoomToPlace, RoomEntranceExit ConnectionRoom, bool setConnections = true)
		{
			Vector2Int vector2Int = EmptyNeighbourPositions[RandomSeed.Next(0, EmptyNeighbourPositions.Count)];
			BiomeRoom biomeRoom = new BiomeRoom(vector2Int.x, vector2Int.y, RandomSeed.Next(-2147483647, int.MaxValue), null);
			Rooms.Add(biomeRoom);
			SetNeighbours(biomeRoom);
			biomeRoom.IsCustom = true;
			biomeRoom.Generated = false;
			biomeRoom.GameObjectPath = RoomToPlace;
			if (setConnections)
			{
				if (vector2Int.y > ConnectionRoom.Room.y)
				{
					biomeRoom.S_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
					biomeRoom.S_Room.Room.N_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
					biomeRoom.S_Room.Room.N_Room.Room = biomeRoom;
				}
				else if (vector2Int.y < ConnectionRoom.Room.y)
				{
					biomeRoom.N_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
					biomeRoom.N_Room.Room.S_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
					biomeRoom.N_Room.Room.S_Room.Room = biomeRoom;
				}
				else if (vector2Int.x > ConnectionRoom.Room.x)
				{
					biomeRoom.W_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
					biomeRoom.W_Room.Room.E_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
					biomeRoom.W_Room.Room.E_Room.Room = biomeRoom;
				}
				else if (vector2Int.x < ConnectionRoom.Room.x)
				{
					biomeRoom.E_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
					biomeRoom.E_Room.Room.W_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
					biomeRoom.E_Room.Room.W_Room.Room = biomeRoom;
				}
			}
			return biomeRoom;
		}

		public Vector2Int GetBossRoom()
		{
			foreach (BiomeRoom room in Rooms)
			{
				if (room.IsBoss)
				{
					return new Vector2Int(room.x, room.y);
				}
			}
			return new Vector2Int(0, 0);
		}

		public Vector2Int GetLastRoom()
		{
			return new Vector2Int(lastRoom.x, lastRoom.y);
		}

		private void GetCriticalPath()
		{
			if (OverrideRandomWalk)
			{
				return;
			}
			ResetFloodDistance();
			FloodFillDistance(RoomEntrance, 0);
			int num = RoomExit.Distance;
			BiomeRoom biomeRoom = RoomExit;
			CriticalPath = new List<BiomeRoom>();
			CriticalPath.Add(biomeRoom);
			while (--num > 0)
			{
				if (biomeRoom.N_Room.Connected && biomeRoom.N_Room.Room.Distance == num)
				{
					biomeRoom = biomeRoom.N_Room.Room;
					biomeRoom.CriticalPathDirection = BiomeRoom.Direction.South;
				}
				if (biomeRoom.E_Room.Connected && biomeRoom.E_Room.Room.Distance == num)
				{
					biomeRoom = biomeRoom.E_Room.Room;
					biomeRoom.CriticalPathDirection = BiomeRoom.Direction.West;
				}
				if (biomeRoom.S_Room.Connected && biomeRoom.S_Room.Room.Distance == num)
				{
					biomeRoom = biomeRoom.S_Room.Room;
					biomeRoom.CriticalPathDirection = BiomeRoom.Direction.North;
				}
				if (biomeRoom.W_Room.Connected && biomeRoom.W_Room.Room.Distance == num)
				{
					biomeRoom = biomeRoom.W_Room.Room;
					biomeRoom.CriticalPathDirection = BiomeRoom.Direction.East;
				}
				CriticalPath.Add(biomeRoom);
			}
		}

		private void PlaceLockAndKey()
		{
			if (string.IsNullOrEmpty(LockedRoomPath) || string.IsNullOrEmpty(KeyRoomPath) || OverrideRandomWalk)
			{
				return;
			}
			ResetFloodDistance();
			FloodFillDistance(RoomEntrance, 0);
			int num = RoomExit.Distance;
			BiomeRoom biomeRoom = RoomExit;
			CriticalPath = new List<BiomeRoom>();
			CriticalPath.Add(biomeRoom);
			while (--num > 0)
			{
				if (biomeRoom.N_Room.Connected && biomeRoom.N_Room.Room.Distance == num)
				{
					biomeRoom = biomeRoom.N_Room.Room;
					biomeRoom.CriticalPathDirection = BiomeRoom.Direction.South;
				}
				if (biomeRoom.E_Room.Connected && biomeRoom.E_Room.Room.Distance == num)
				{
					biomeRoom = biomeRoom.E_Room.Room;
					biomeRoom.CriticalPathDirection = BiomeRoom.Direction.West;
				}
				if (biomeRoom.S_Room.Connected && biomeRoom.S_Room.Room.Distance == num)
				{
					biomeRoom = biomeRoom.S_Room.Room;
					biomeRoom.CriticalPathDirection = BiomeRoom.Direction.North;
				}
				if (biomeRoom.W_Room.Connected && biomeRoom.W_Room.Room.Distance == num)
				{
					biomeRoom = biomeRoom.W_Room.Room;
					biomeRoom.CriticalPathDirection = BiomeRoom.Direction.East;
				}
				CriticalPath.Add(biomeRoom);
			}
			float num2 = -2.1474836E+09f;
			BiomeRoom biomeRoom2 = null;
			foreach (BiomeRoom room in Rooms)
			{
				if (!room.IsCustom && (float)room.Distance > num2 && !CriticalPath.Contains(room))
				{
					biomeRoom2 = room;
					num2 = room.Distance;
				}
			}
			biomeRoom2.IsCustom = true;
			biomeRoom2.GameObjectPath = KeyRoomPath;
			biomeRoom = biomeRoom2;
			num = biomeRoom.Distance;
			bool flag = true;
			while (flag && !CriticalPath.Contains(biomeRoom))
			{
				if (biomeRoom.N_Room.Connected && biomeRoom.N_Room.Room.Distance < num)
				{
					biomeRoom = biomeRoom.N_Room.Room;
					num = biomeRoom.Distance;
				}
				else if (biomeRoom.E_Room.Connected && biomeRoom.E_Room.Room.Distance < num)
				{
					biomeRoom = biomeRoom.E_Room.Room;
					num = biomeRoom.Distance;
				}
				else if (biomeRoom.S_Room.Connected && biomeRoom.S_Room.Room.Distance < num)
				{
					biomeRoom = biomeRoom.S_Room.Room;
					num = biomeRoom.Distance;
				}
				else if (biomeRoom.W_Room.Connected && biomeRoom.W_Room.Room.Distance < num)
				{
					biomeRoom = biomeRoom.W_Room.Room;
					num = biomeRoom.Distance;
				}
			}
			int index = RandomSeed.Next(1, CriticalPath.IndexOf(biomeRoom));
			CriticalPath[index].IsCustom = true;
			CriticalPath[index].GameObjectPath = LockedRoomPath;
		}

		private void PlaceRespawnRoom()
		{
			if (RespawnRoom == null)
			{
				RespawnRoom = new BiomeRoom(RespawnRoomCoords.x, RespawnRoomCoords.y, RandomSeed.Next(-2147483647, int.MaxValue), null);
				Rooms.Add(RespawnRoom);
				RespawnRoom.IsCustom = true;
				RespawnRoom.GameObjectPath = RespawnRoomPath;
				RespawnRoom.Generated = true;
				RespawnRoom.IsRespawnRoom = true;
			}
		}

		private void PlaceDeathCatRoom()
		{
			if (DeathCatRoom == null)
			{
				DeathCatRoom = new BiomeRoom(int.MaxValue, int.MaxValue, RandomSeed.Next(-2147483647, int.MaxValue), null);
				Rooms.Add(DeathCatRoom);
				DeathCatRoom.IsCustom = true;
				DeathCatRoom.GameObjectPath = DeathCatRoomPath;
				DeathCatRoom.Generated = true;
				DeathCatRoom.IsDeathCatRoom = true;
			}
		}

		private void PlaceMysticSellerRoom()
		{
			MysticSellerRoom = new BiomeRoom(33333, 33333, RandomSeed.Next(-2147483647, int.MaxValue), null);
			Rooms.Add(MysticSellerRoom);
			MysticSellerRoom.IsCustom = true;
			MysticSellerRoom.GameObjectPath = "Assets/_Rooms/Mystic Shop Keeper Room.prefab";
			MysticSellerRoom.Generated = true;
		}

		private IEnumerator PlaceStoryRooms()
		{
			if (OverrideRandomWalk)
			{
				yield break;
			}
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			RandomiseStoryOrder = new List<ListOfStoryRooms>();
			int count = StoryRooms.Count;
			while (RandomiseStoryOrder.Count < count)
			{
				int index = RandomSeed.Next(0, StoryRooms.Count);
				RandomiseStoryOrder.Add(StoryRooms[index]);
				StoryRooms.Remove(StoryRooms[index]);
			}
			StoryRooms = new List<ListOfStoryRooms>(RandomiseStoryOrder);
			int i = -1;
			while (true)
			{
				int num = i + 1;
				i = num;
				if (num >= StoryRooms.Count)
				{
					break;
				}
				ListOfStoryRooms listOfStoryRooms = StoryRooms[i];
				if (GameManager.CurrentDungeonLayer - 1 != DataManager.Instance.GetVariableInt(listOfStoryRooms.StoryVariable) || DataManager.Instance.GetVariableInt(listOfStoryRooms.LastRun) >= DataManager.Instance.dungeonRun || DataManager.Instance.GetVariable(listOfStoryRooms.DungeonBeaten) || DataManager.Instance.GetVariableInt(listOfStoryRooms.StoryVariable) >= listOfStoryRooms.Rooms.Count || string.IsNullOrEmpty(listOfStoryRooms.Rooms[DataManager.Instance.GetVariableInt(listOfStoryRooms.StoryVariable)].RoomPath))
				{
					continue;
				}
				int num2 = RandomSeed.Next(0, 10);
				int variableInt = DataManager.Instance.GetVariableInt(listOfStoryRooms.StoryVariable);
				if (GameManager.CurrentDungeonFloor == 1 && (num2 < 5 || !listOfStoryRooms.Rooms[variableInt].FloorOne) && (listOfStoryRooms.Rooms[variableInt].FloorTwo || listOfStoryRooms.Rooms[variableInt].FloorThree))
				{
					continue;
				}
				string roomPath = listOfStoryRooms.Rooms[DataManager.Instance.GetVariableInt(listOfStoryRooms.StoryVariable)].RoomPath;
				AvailableRooms = new List<BiomeRoom>();
				if (listOfStoryRooms.PutOnCriticalPath)
				{
					foreach (BiomeRoom item in CriticalPath)
					{
						if (!item.IsCustom)
						{
							AvailableRooms.Add(item);
						}
					}
				}
				if (!listOfStoryRooms.PutOnCriticalPath || (listOfStoryRooms.PutOnCriticalPath && AvailableRooms.Count <= 0))
				{
					int num3 = 0;
					while (AvailableRooms.Count <= 0 && ++num3 <= 4)
					{
						foreach (BiomeRoom room in Rooms)
						{
							if (!room.IsCustom && room.NumConnections == num3)
							{
								AvailableRooms.Add(room);
							}
						}
					}
				}
				BiomeRoom biomeRoom = AvailableRooms[RandomSeed.Next(0, AvailableRooms.Count)];
				biomeRoom.IsCustom = true;
				biomeRoom.GameObjectPath = roomPath;
				MMTransition.UpdateProgress((float)i / (float)CustomDynamicRooms.Count, ScriptLocalization.Interactions.GeneratingDungeon);
				if ((float)stopwatch.ElapsedMilliseconds > 1f / 60f)
				{
					stopwatch.Reset();
					yield return null;
				}
			}
			stopwatch.Stop();
		}

		private IEnumerator PlaceDynamicCustomRooms()
		{
			bool spawnedWeaponRoom = false;
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			if (OverrideRandomWalk)
			{
				yield break;
			}
			RandomiseOrder = new List<ListOfCustomRoomPrefabs>();
			int count = CustomDynamicRooms.Count;
			while (RandomiseOrder.Count < count)
			{
				int index = RandomSeed.Next(0, CustomDynamicRooms.Count);
				RandomiseOrder.Add(CustomDynamicRooms[index]);
				CustomDynamicRooms.Remove(CustomDynamicRooms[index]);
			}
			CustomDynamicRooms = new List<ListOfCustomRoomPrefabs>(RandomiseOrder);
			int i = -1;
			while (true)
			{
				int num = i + 1;
				i = num;
				if (num >= CustomDynamicRooms.Count)
				{
					break;
				}
				ListOfCustomRoomPrefabs listOfCustomRoomPrefabs = CustomDynamicRooms[i];
				bool flag = false;
				foreach (VariableAndCondition conditionalVariable in listOfCustomRoomPrefabs.ConditionalVariables)
				{
					if (DataManager.Instance.GetVariable(conditionalVariable.Variable) != conditionalVariable.Condition)
					{
						flag = true;
					}
				}
				if (flag)
				{
					continue;
				}
				flag = false;
				foreach (FollowerLocation item in listOfCustomRoomPrefabs.LocationIsUndiscovered)
				{
					if (DataManager.Instance.DiscoveredLocations.Contains(item))
					{
						flag = true;
					}
				}
				if (flag)
				{
					continue;
				}
				flag = false;
				if (listOfCustomRoomPrefabs.MinimumRun && DataManager.Instance.dungeonRun < listOfCustomRoomPrefabs.MinimumRunNumber)
				{
					flag = true;
				}
				if (flag)
				{
					continue;
				}
				flag = false;
				if (listOfCustomRoomPrefabs.MaximumRun && DataManager.Instance.dungeonRun > listOfCustomRoomPrefabs.MaximumRunNumber)
				{
					flag = true;
				}
				if (flag)
				{
					continue;
				}
				flag = false;
				if (listOfCustomRoomPrefabs.MinimumFollowerCount && DataManager.Instance.Followers.Count < listOfCustomRoomPrefabs.MinimumFollowerCountNumber)
				{
					flag = true;
				}
				if (flag)
				{
					continue;
				}
				flag = false;
				if (listOfCustomRoomPrefabs.MaximumFollowerCount && DataManager.Instance.Followers.Count > listOfCustomRoomPrefabs.MaximumFollowerCountNumber)
				{
					flag = true;
				}
				if (flag)
				{
					continue;
				}
				if (listOfCustomRoomPrefabs.RoomPaths.Count == 1 && listOfCustomRoomPrefabs.RoomPaths[0] == "Assets/_Rooms/Reward Room Tarot.prefab" && DataManager.Instance.CanUnlockRelics && !DataManager.Instance.OnboardedRelics && !DungeonSandboxManager.Active)
				{
					listOfCustomRoomPrefabs.PutOnCriticalPath = true;
				}
				if (!((float)RandomSeed.NextDouble() <= listOfCustomRoomPrefabs.Probability) || !listOfCustomRoomPrefabs.AvailableOnLayer() || !listOfCustomRoomPrefabs.AvailableOnFoor())
				{
					continue;
				}
				if (listOfCustomRoomPrefabs.UseStaticRoomList)
				{
					listOfCustomRoomPrefabs.RoomPaths = listOfCustomRoomPrefabs.RoomPaths.Union(ListOfCustomRoomPrefabs.StaticRoomList).ToList();
					if (DoctrineUpgradeSystem.TrySermonsStillAvailable() && DoctrineUpgradeSystem.TryGetStillDoctrineStone() && DataManager.Instance.FirstDoctrineStone)
					{
						listOfCustomRoomPrefabs.RoomPaths.Add("Assets/_Rooms/Reward Doctrine Room.prefab");
					}
					if (!DataManager.Instance.HaroConversationCompleted)
					{
						listOfCustomRoomPrefabs.RoomPaths.Add("Assets/_Rooms/Lore Haro.prefab");
					}
					if (PlayerFleeceManager.FleecePreventsHealthPickups())
					{
						listOfCustomRoomPrefabs.RoomPaths.Remove("Assets/_Rooms/Special Blood Sacrafice.prefab");
					}
				}
				string gameObjectPath = listOfCustomRoomPrefabs.RoomPaths[RandomSeed.Next(0, listOfCustomRoomPrefabs.RoomPaths.Count)];
				AvailableRooms = new List<BiomeRoom>();
				if (listOfCustomRoomPrefabs.PutOnCriticalPath)
				{
					foreach (BiomeRoom item2 in CriticalPath)
					{
						if (!item2.IsCustom)
						{
							AvailableRooms.Add(item2);
						}
					}
				}
				if (!listOfCustomRoomPrefabs.PutOnCriticalPath || (listOfCustomRoomPrefabs.PutOnCriticalPath && AvailableRooms.Count <= 0))
				{
					int num2 = 0;
					while (AvailableRooms.Count <= 0 && ++num2 <= 4)
					{
						foreach (BiomeRoom room in Rooms)
						{
							if (listOfCustomRoomPrefabs.RemoveIfNoNonCriticalPath)
							{
								if (!room.IsCustom && room.NumConnections == num2 && !CriticalPath.Contains(room))
								{
									AvailableRooms.Add(room);
								}
							}
							else if (!room.IsCustom && room.NumConnections == num2)
							{
								AvailableRooms.Add(room);
							}
						}
					}
				}
				if (AvailableRooms.Count > 0)
				{
					BiomeRoom biomeRoom = AvailableRooms[RandomSeed.Next(0, AvailableRooms.Count)];
					biomeRoom.GameObjectPath = gameObjectPath;
					if (biomeRoom.GameObjectPath == "Assets/_Rooms/Reward Room Tarot.prefab")
					{
						if (((DataManager.Instance.CanUnlockRelics && !DataManager.Instance.OnboardedRelics && !DungeonSandboxManager.Active) || ((DataManager.Instance.OnboardedRelics || DungeonSandboxManager.Active) && RandomSeed.Next(0, 100) >= 85)) && !spawnedWeaponRoom)
						{
							spawnedWeaponRoom = true;
							biomeRoom.GameObjectPath = "Assets/_Rooms/Marketplace Relics.prefab";
							listOfCustomRoomPrefabs.ConnectionType = GenerateRoom.ConnectionTypes.RelicShop;
						}
						else if (GameManager.CurrentDungeonFloor > 1 && RandomSeed.Next(0, 100) >= 65 && DataManager.Instance.WeaponPool.Count + DataManager.Instance.CursePool.Count > 3 && !spawnedWeaponRoom)
						{
							spawnedWeaponRoom = true;
							biomeRoom.GameObjectPath = "Assets/_Rooms/Marketplace Weapons.prefab";
							listOfCustomRoomPrefabs.ConnectionType = GenerateRoom.ConnectionTypes.WeaponShop;
							UnityEngine.Debug.Log("2".Colour(Color.yellow));
							foreach (BiomeRoom room2 in Rooms)
							{
								room2.HasWeapon = false;
							}
						}
						else
						{
							if (DataManager.Instance.PlayerFleece == 4)
							{
								continue;
							}
							listOfCustomRoomPrefabs.ConnectionType = GenerateRoom.ConnectionTypes.Tarot;
							biomeRoom.GameObjectPath = gameObjectPath;
						}
					}
					biomeRoom.IsCustom = true;
					if (listOfCustomRoomPrefabs.SetCustomConnectionType)
					{
						if (biomeRoom.N_Room.Connected)
						{
							biomeRoom.N_Room.Room.S_Room.SetConnection(listOfCustomRoomPrefabs.ConnectionType);
						}
						if (biomeRoom.E_Room.Connected)
						{
							biomeRoom.E_Room.Room.W_Room.SetConnection(listOfCustomRoomPrefabs.ConnectionType);
						}
						if (biomeRoom.S_Room.Connected)
						{
							biomeRoom.S_Room.Room.N_Room.SetConnection(listOfCustomRoomPrefabs.ConnectionType);
						}
						if (biomeRoom.W_Room.Connected)
						{
							biomeRoom.W_Room.Room.E_Room.SetConnection(listOfCustomRoomPrefabs.ConnectionType);
						}
					}
				}
				MMTransition.UpdateProgress((float)i / (float)CustomDynamicRooms.Count, ScriptLocalization.Interactions.GeneratingDungeon);
				if ((float)stopwatch.ElapsedMilliseconds > 1f / 60f)
				{
					stopwatch.Reset();
					yield return null;
				}
			}
			List<BiomeRoom> list = new List<BiomeRoom>();
			foreach (BiomeRoom room3 in Rooms)
			{
				if (!room3.IsCustom)
				{
					list.Add(room3);
				}
			}
			if (!WeaponAtEnd && GameManager.CurrentDungeonFloor > 1 && list.Count > 0 && PlayerFarming.Location != FollowerLocation.IntroDungeon)
			{
				list[RandomSeed.Next(0, list.Count)].HasWeapon = true;
			}
			stopwatch.Stop();
		}

		private void PlaceFixedCustomRooms()
		{
			if (OverrideRandomWalk)
			{
				return;
			}
			HasSpawnedRelic = false;
			foreach (FixedRoom customRoom in CustomRooms)
			{
				bool flag = false;
				foreach (VariableAndCondition conditionalVariable in customRoom.ConditionalVariables)
				{
					if (DataManager.Instance.GetVariable(conditionalVariable.Variable) != conditionalVariable.Condition)
					{
						flag = true;
					}
				}
				if (flag || (float)RandomSeed.NextDouble() > customRoom.Probability)
				{
					continue;
				}
				AvailableRooms = new List<BiomeRoom>();
				foreach (BiomeRoom room in Rooms)
				{
					bool flag2 = true;
					if (room.IsCustom)
					{
						flag2 = false;
					}
					if (room.N_Room.Connected && customRoom.North == FixedRoom.Connection.ForceOff)
					{
						flag2 = false;
					}
					if (room.E_Room.Connected && customRoom.East == FixedRoom.Connection.ForceOff)
					{
						flag2 = false;
					}
					if (room.S_Room.Connected && customRoom.South == FixedRoom.Connection.ForceOff)
					{
						flag2 = false;
					}
					if (room.W_Room.Connected && customRoom.West == FixedRoom.Connection.ForceOff)
					{
						flag2 = false;
					}
					if (!room.N_Room.Connected && customRoom.North == FixedRoom.Connection.ForceOn)
					{
						flag2 = false;
					}
					if (!room.E_Room.Connected && customRoom.East == FixedRoom.Connection.ForceOn)
					{
						flag2 = false;
					}
					if (!room.S_Room.Connected && customRoom.South == FixedRoom.Connection.ForceOn)
					{
						flag2 = false;
					}
					if (!room.W_Room.Connected && customRoom.West == FixedRoom.Connection.ForceOn)
					{
						flag2 = false;
					}
					if (flag2)
					{
						AvailableRooms.Add(room);
					}
				}
				if (AvailableRooms.Count <= 0 && !customRoom.HasForcedOn && customRoom.HasOptional)
				{
					foreach (BiomeRoom room2 in Rooms)
					{
						bool isCustom = room2.IsCustom;
						if (room2.N_Room.Connected)
						{
							FixedRoom.Connection north = customRoom.North;
							int num3 = 2;
						}
						if (room2.E_Room.Connected)
						{
							FixedRoom.Connection east = customRoom.East;
							int num4 = 2;
						}
						if (room2.S_Room.Connected)
						{
							FixedRoom.Connection south = customRoom.South;
							int num5 = 2;
						}
						if (room2.W_Room.Connected)
						{
							FixedRoom.Connection west = customRoom.West;
							int num6 = 2;
						}
						int num = 0;
						if (customRoom.North == FixedRoom.Connection.Optional && room2.N_Room.Connected)
						{
							num++;
						}
						if (customRoom.East == FixedRoom.Connection.Optional && room2.E_Room.Connected)
						{
							num++;
						}
						if (customRoom.South == FixedRoom.Connection.Optional && room2.S_Room.Connected)
						{
							num++;
						}
						if (customRoom.West == FixedRoom.Connection.Optional && room2.W_Room.Connected)
						{
							num++;
						}
						if (num > 0)
						{
							AvailableRooms.Add(room2);
						}
					}
				}
				if (AvailableRooms.Count <= 0)
				{
					if (customRoom.RequiredConnections != 1)
					{
						continue;
					}
					int num2 = 0;
					while (AvailableRooms.Count <= 0 && ++num2 <= 3)
					{
						foreach (BiomeRoom room3 in Rooms)
						{
							if (!room3.IsCustom && room3.NumConnections == num2 && ((customRoom.North != FixedRoom.Connection.ForceOff && room3.S_Room.Room == null) || (customRoom.East != FixedRoom.Connection.ForceOff && room3.W_Room.Room == null) || (customRoom.South != FixedRoom.Connection.ForceOff && room3.N_Room.Room == null) || (customRoom.West != FixedRoom.Connection.ForceOff && room3.E_Room.Room == null)))
							{
								AvailableRooms.Add(room3);
							}
						}
					}
					if (AvailableRooms.Count > 0)
					{
						UnityEngine.Debug.Log("CREATE ! " + customRoom.prefab.name);
						BiomeRoom biomeRoom = AvailableRooms[RandomSeed.Next(0, AvailableRooms.Count)];
						if (customRoom.North != FixedRoom.Connection.ForceOff)
						{
							CreateCustomRoom(biomeRoom.x, biomeRoom.y - 1, customRoom.prefab, Direction.North, customRoom.GenerateRoom == FixedRoom.Generate.DontGenerate);
						}
						else if (customRoom.East != FixedRoom.Connection.ForceOff)
						{
							CreateCustomRoom(biomeRoom.x - 1, biomeRoom.y, customRoom.prefab, Direction.East, customRoom.GenerateRoom == FixedRoom.Generate.DontGenerate);
						}
						else if (customRoom.South != FixedRoom.Connection.ForceOff)
						{
							CreateCustomRoom(biomeRoom.x, biomeRoom.y + 1, customRoom.prefab, Direction.South, customRoom.GenerateRoom == FixedRoom.Generate.DontGenerate);
						}
						else if (customRoom.West != FixedRoom.Connection.ForceOff)
						{
							CreateCustomRoom(biomeRoom.x + 1, biomeRoom.y, customRoom.prefab, Direction.West, customRoom.GenerateRoom == FixedRoom.Generate.DontGenerate);
						}
					}
				}
				else
				{
					UnityEngine.Debug.Log("WORKS! " + customRoom.prefab.name);
					BiomeRoom biomeRoom2 = AvailableRooms[RandomSeed.Next(0, AvailableRooms.Count)];
					biomeRoom2.IsCustom = true;
					biomeRoom2.GameObject = customRoom.prefab;
					biomeRoom2.Generated = customRoom.GenerateRoom == FixedRoom.Generate.DontGenerate;
				}
			}
		}

		private void CreateCustomRoom(int x, int y, GameObject Prefab, Direction direction, bool Generated)
		{
			BiomeRoom biomeRoom = new BiomeRoom(x, y, RandomSeed.Next(-2147483647, int.MaxValue), Prefab);
			biomeRoom.IsCustom = true;
			biomeRoom.Generated = Generated;
			Rooms.Add(biomeRoom);
			UnityEngine.Debug.Log("NEW ROOM CREATED: " + Prefab.name + "  " + biomeRoom.Generated);
			switch (direction)
			{
			case Direction.North:
				SetNeighbours(biomeRoom);
				biomeRoom.N_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
				biomeRoom.N_Room.Room.S_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
				break;
			case Direction.East:
				SetNeighbours(biomeRoom);
				biomeRoom.E_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
				biomeRoom.E_Room.Room.W_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
				break;
			case Direction.South:
				SetNeighbours(biomeRoom);
				biomeRoom.S_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
				biomeRoom.S_Room.Room.N_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
				break;
			case Direction.West:
				SetNeighbours(biomeRoom);
				biomeRoom.W_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
				biomeRoom.W_Room.Room.E_Room.SetConnection(GenerateRoom.ConnectionTypes.True);
				break;
			}
		}

		private void SetNeighbours(BiomeRoom b)
		{
			if (b.N_Room == null)
			{
				b.N_Room = new RoomConnection(BiomeRoom.GetRoom(b.x, b.y + 1));
			}
			if (b.E_Room == null)
			{
				b.E_Room = new RoomConnection(BiomeRoom.GetRoom(b.x + 1, b.y));
			}
			if (b.S_Room == null)
			{
				b.S_Room = new RoomConnection(BiomeRoom.GetRoom(b.x, b.y - 1));
			}
			if (b.W_Room == null)
			{
				b.W_Room = new RoomConnection(BiomeRoom.GetRoom(b.x - 1, b.y));
			}
		}

		private void SetNeighbours()
		{
			foreach (BiomeRoom room in Rooms)
			{
				if (room.N_Room == null)
				{
					room.N_Room = new RoomConnection(BiomeRoom.GetRoom(room.x, room.y + 1));
				}
				if (room.E_Room == null)
				{
					room.E_Room = new RoomConnection(BiomeRoom.GetRoom(room.x + 1, room.y));
				}
				if (room.S_Room == null)
				{
					room.S_Room = new RoomConnection(BiomeRoom.GetRoom(room.x, room.y - 1));
				}
				if (room.W_Room == null)
				{
					room.W_Room = new RoomConnection(BiomeRoom.GetRoom(room.x - 1, room.y));
				}
			}
		}

		private IEnumerator InstantiatePrefabs()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			int i = -1;
			foreach (BiomeRoom r in Rooms)
			{
				if (r.IsCustom)
				{
					if (!string.IsNullOrEmpty(r.GameObjectPath))
					{
						bool loaded = false;
						AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(r.GameObjectPath, base.transform);
						asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
						{
							r.GameObject = obj.Result;
							if (r.GameObject != null)
							{
								r.GameObject.SetActive(false);
							}
							loaded = true;
						};
						while (!loaded)
						{
							yield return null;
						}
					}
					else
					{
						r.GameObject = UnityEngine.Object.Instantiate(r.GameObject, base.transform);
						r.GameObject.SetActive(false);
					}
				}
				else if (!ReuseGeneratorRoom)
				{
					r.GameObject = UnityEngine.Object.Instantiate(GeneratorRoomPrefab.gameObject, base.transform);
					r.GameObject.SetActive(false);
				}
				if (r.IsRespawnRoom)
				{
					r.GameObject.transform.parent = base.transform.parent;
					r.GameObject.gameObject.GetComponent<RespawnRoomManager>().Init(this);
				}
				if (r.IsDeathCatRoom)
				{
					r.GameObject.transform.parent = base.transform.parent;
					r.GameObject.gameObject.GetComponent<DeathCatRoomManager>().Init(this);
				}
				int num = i + 1;
				i = num;
				MMTransition.UpdateProgress((float)num / (float)Rooms.Count, ScriptLocalization.Interactions.GeneratingDungeon + " " + ScriptLocalization.UI.PlacingRooms + " " + i + "/" + Rooms.Count);
				yield return new WaitForEndOfFrame();
			}
			if (!ReuseGeneratorRoom)
			{
				GeneratorRoomPrefab.gameObject.SetActive(false);
			}
			stopwatch.Stop();
		}

		private void OnDestroy()
		{
			Clear();
			StopMusicAndAtmos();
			if (loadedAddressableAssets != null)
			{
				foreach (AsyncOperationHandle<GameObject> loadedAddressableAsset in loadedAddressableAssets)
				{
					Addressables.Release((AsyncOperationHandle)loadedAddressableAsset);
				}
				loadedAddressableAssets.Clear();
			}
			if (Instance == this)
			{
				Instance = null;
			}
		}

		private void Clear()
		{
			foreach (BiomeRoom room in Rooms)
			{
				room.Clear();
			}
			Rooms.Clear();
		}

		private void StopMusicAndAtmos()
		{
			AudioManager.Instance.StopCurrentMusic();
			AudioManager.Instance.StopLoop(biomeAtmosInstance);
		}

		public void Regenerate(Action Callback)
		{
			MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 0.5f, "", delegate
			{
				Clear();
				ObjectPool.DestroyAll();
				PrevRoom = null;
				FirstArrival = true;
				for (int num = base.transform.childCount - 1; num > 0; num--)
				{
					if (base.transform.GetChild(num).gameObject != GeneratorRoomPrefab)
					{
						UnityEngine.Object.Destroy(base.transform.GetChild(num).gameObject);
					}
				}
				Seed = DataManager.RandomSeed.Next(-2147483647, int.MaxValue);
				StartCoroutine(GenerateRoutine());
				Action action = Callback;
				if (action != null)
				{
					action();
				}
			});
		}

		private void SetRoom(int x, int y)
		{
			CurrentX = x;
			CurrentY = y;
			StartCoroutine(ChangeRoomRoutine(BiomeRoom.GetRoom(CurrentX, CurrentY)));
		}

		public static void ChangeRoom(int X, int Y)
		{
			Instance.CurrentX = X;
			Instance.CurrentY = Y;
			Instance.StartCoroutine(Instance.ChangeRoomRoutine(BiomeRoom.GetRoom(Instance.CurrentX, Instance.CurrentY)));
		}

		public static void ChangeRoom(Vector2Int Direction)
		{
			Instance.CurrentX += Direction.x;
			Instance.CurrentY += Direction.y;
			Instance.StartCoroutine(Instance.ChangeRoomRoutine(BiomeRoom.GetRoom(Instance.CurrentX, Instance.CurrentY)));
		}

		private IEnumerator ChangeRoomRoutine(BiomeRoom CurrentRoom)
		{
			Health.team2.Clear();
			BiomeAction onBiomeLeftRoom = BiomeGenerator.OnBiomeLeftRoom;
			if (onBiomeLeftRoom != null)
			{
				onBiomeLeftRoom();
			}
			this.CurrentRoom = CurrentRoom;
			if (!CurrentRoom.Visited)
			{
				RoomsVisited++;
			}
			CurrentRoom.Activate(PrevRoom, ReuseGeneratorRoom);
			yield return new WaitForEndOfFrame();
			GetDoors();
			while (!CurrentRoom.generateRoom.GeneratedDecorations)
			{
				yield return null;
			}
			CurrentRoom.generateRoom.SetColliderAndUpdatePathfinding();
			UnityEngine.Resources.UnloadUnusedAssets();
			PlacePlayer();
			BiomeAction onBiomeChangeRoom = BiomeGenerator.OnBiomeChangeRoom;
			if (onBiomeChangeRoom != null)
			{
				onBiomeChangeRoom();
			}
			if (PrevRoom == null)
			{
				if (CurrentRoom.generateRoom.roomMusicID == SoundConstants.RoomID.StandardRoom && GameManager.Layer2)
				{
					AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.AltStandardRoom);
				}
				else
				{
					AudioManager.Instance.SetMusicRoomID(CurrentRoom.generateRoom.roomMusicID);
				}
				AudioManager.Instance.StartMusic();
			}
			else
			{
				if (PrevRoom.generateRoom.roomMusicID == SoundConstants.RoomID.NoMusic && CurrentRoom.generateRoom.roomMusicID != SoundConstants.RoomID.NoMusic)
				{
					AudioManager.Instance.PlayMusic(biomeMusicPath);
				}
				if (CurrentRoom.generateRoom.roomMusicID == SoundConstants.RoomID.StandardRoom && GameManager.Layer2)
				{
					AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.AltStandardRoom);
				}
				else
				{
					AudioManager.Instance.SetMusicRoomID(CurrentRoom.generateRoom.roomMusicID);
				}
			}
			string soundPath = ((CurrentRoom.generateRoom.biomeAtmosOverridePath != string.Empty) ? CurrentRoom.generateRoom.biomeAtmosOverridePath : biomeAtmosPath);
			if (!AudioManager.Instance.CurrentEventIsPlayingPath(biomeAtmosInstance, soundPath))
			{
				AudioManager.Instance.PlayAtmos(soundPath);
			}
			bool flag = false;
			foreach (Health allUnit in Health.allUnits)
			{
				if (allUnit.team == Health.Team.Team2 && allUnit.HP > 0f)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				AudioManager.Instance.SetMusicCombatState(false);
			}
			PrevRoom = CurrentRoom;
		}

		public static void SpawnBombsInRoom(int amount, bool enemyBombs = true)
		{
			_003C_003Ec__DisplayClass147_0 _003C_003Ec__DisplayClass147_ = new _003C_003Ec__DisplayClass147_0();
			_003C_003Ec__DisplayClass147_.enemyBombs = enemyBombs;
			LayerMask layerMask = (int)default(LayerMask) | (1 << LayerMask.NameToLayer("Island"));
			layerMask = (int)layerMask | (1 << LayerMask.NameToLayer("Obstacles Player Ignore"));
			float x = Physics2D.Raycast(Vector3.zero, Vector3.right, 100f, layerMask).point.x;
			float y = Physics2D.Raycast(Vector3.zero, Vector3.up, 100f, layerMask).point.y;
			float num = 0f;
			float num2 = 0.25f;
			for (int i = 0; i < amount; i++)
			{
				Vector3 position = new Vector3(UnityEngine.Random.Range(0f - x, x), UnityEngine.Random.Range(0f - y, y));
				GameManager.GetInstance().StartCoroutine(_003C_003Ec__DisplayClass147_._003CSpawnBombsInRoom_003Eg__SpawnBomb_007C0(position, Instance.CurrentRoom.generateRoom.transform, num));
				num += num2;
			}
		}

		public static Vector3 GetRandomPositionInIsland()
		{
			LayerMask layerMask = (int)default(LayerMask) | (1 << LayerMask.NameToLayer("Island"));
			layerMask = (int)layerMask | (1 << LayerMask.NameToLayer("Obstacles Player Ignore"));
			float x = Physics2D.Raycast(Vector3.zero, Vector3.right, 100f, layerMask).point.x;
			float y = Physics2D.Raycast(Vector3.zero, Vector3.up, 100f, layerMask).point.y;
			return new Vector3(UnityEngine.Random.Range(0f - x + 0.5f, x - 0.5f), UnityEngine.Random.Range(0f - y + 0.5f, y - 0.5f));
		}

		private IEnumerator DelayEndConversation()
		{
			yield return new WaitForSeconds(0.3f);
			GameManager.GetInstance().OnConversationEnd(false);
			GameManager.GetInstance().CameraSetOffset(Vector3.zero);
			GameManager.GetInstance().AddPlayerToCamera();
		}

		private IEnumerator DelayPlayerGoToAndStop(Vector3 TargetPosition)
		{
			PlayerFarming.Instance.GoToAndStop(TargetPosition, null, true, true, delegate
			{
				PlayerState.facingAngle = Utils.GetAngle(Door.GetEntranceDoor().transform.position, TargetPosition);
				StartCoroutine(DelayEndConversation());
				if (CurrentRoom == RoomEntrance || DungeonSandboxManager.Active)
				{
					GenerateRoom.Instance.LockEntranceBehindPlayer = true;
				}
				Door entranceDoor = Door.GetEntranceDoor();
				if (GenerateRoom.Instance.LockEntranceBehindPlayer)
				{
					entranceDoor.RoomLockController.gameObject.SetActive(true);
					entranceDoor.RoomLockController.DoorUp();
					PlayerDistanceMovement[] componentsInChildren = entranceDoor.GetComponentsInChildren<PlayerDistanceMovement>();
					foreach (PlayerDistanceMovement obj in componentsInChildren)
					{
						obj.ForceReset();
						obj.enabled = false;
					}
					entranceDoor.VisitedIcon.SetActive(false);
				}
				entranceDoor.PlayerFinishedEnteringDoor();
				StartCoroutine(DelayActivateRoom(DungeonLocation != FollowerLocation.Boss_5));
			}, -1f);
			yield return new WaitForSeconds(0.5f);
			if (ShowDisplayName)
			{
				if (DataManager.Instance.dungeonRun == 1 && PlayerFarming.Location == FollowerLocation.Dungeon1_1)
				{
					yield return new WaitForSeconds(1f);
					UIHeartsIntro uIHeartsIntro = UnityEngine.Object.Instantiate(UnityEngine.Resources.Load<UIHeartsIntro>("Prefabs/UI/UI Hearts Intro"), GameObject.FindGameObjectWithTag("Canvas").transform);
					yield return StartCoroutine(uIHeartsIntro.HeartRoutine());
					DataManager.Instance.PlayerHasBeenGivenHearts = true;
				}
				HUD_DisplayName.Play(DisplayName, 3, HUD_DisplayName.Positions.Centre);
				yield return new WaitForSeconds(1f);
			}
			ShowDisplayName = false;
			DataManager.Instance.FirstTimeInDungeon = true;
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.GoToDungeon);
		}

		public void RoomBecameActive()
		{
			BiomeAction onRoomActive = BiomeGenerator.OnRoomActive;
			if (onRoomActive != null)
			{
				onRoomActive();
			}
		}

		public IEnumerator DelayActivateRoom(bool applyModifiers)
		{
			if (applyModifiers)
			{
				yield return new WaitForEndOfFrame();
				while (PlayerFarming.Instance.GoToAndStopping)
				{
					yield return null;
				}
				if (GameManager.InitialDungeonEnter)
				{
					if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Ability_BlackHeart))
					{
						PlayerFarming.Instance.health.BlackHearts += 2f;
					}
					yield return StartCoroutine(ApplyCurrentFleeceModifiersIE());
					yield return StartCoroutine(ApplyCurrentDemonModifiersIE());
					yield return StartCoroutine(ShowHeavyAttackTutorial());
				}
				yield return StartCoroutine(ApplyCurrentDungeonModifiersIE());
			}
			yield return new WaitForSeconds(1f);
			GameManager.InitialDungeonEnter = false;
			CurrentRoom.Active = true;
			BiomeAction onRoomActive = BiomeGenerator.OnRoomActive;
			if (onRoomActive != null)
			{
				onRoomActive();
			}
			CurrentRoom.Completed = true;
		}

		private IEnumerator ShowHeavyAttackTutorial()
		{
			bool Loop = false;
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HeavyAttacks) && DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.HeavyAttacks))
			{
				Loop = true;
				UITutorialOverlayController uITutorialOverlayController = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.HeavyAttacks);
				uITutorialOverlayController.OnHidden = (Action)Delegate.Combine(uITutorialOverlayController.OnHidden, (Action)delegate
				{
					Loop = false;
					Action onTutorialShown = OnTutorialShown;
					if (onTutorialShown != null)
					{
						onTutorialShown();
					}
				});
			}
			while (Loop)
			{
				yield return null;
			}
		}

		public GameObject PlacePlayer()
		{
			if (Player == null)
			{
				if (DungeonSandboxManager.Active)
				{
					foreach (KeyValuePair<FollowerLocation, LocationManager> locationManager in LocationManager.LocationManagers)
					{
						if (locationManager.Value != null)
						{
							Player = locationManager.Value.PlacePlayer();
							break;
						}
					}
				}
				if (Player == null)
				{
					Player = LocationManager.LocationManagers[DungeonLocation].PlacePlayer();
				}
				PlayerState = Player.GetComponent<StateMachine>();
				RevealHudAbilityIcons = true;
			}
			else
			{
				Action onPlayerLocationSet = LocationManager.OnPlayerLocationSet;
				if (onPlayerLocationSet != null)
				{
					onPlayerLocationSet();
				}
			}
			if (FirstArrival && DoFirstArrivalRoutine)
			{
				GameManager.GetInstance().OnConversationNew(true, true);
				GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 6f);
				GameManager.GetInstance().CameraSetZoom(6f);
				GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0f, -1f));
				Door entranceDoor = Door.GetEntranceDoor();
				if (entranceDoor != null)
				{
					Player.transform.position = entranceDoor.PlayerPosition.position;
					StartCoroutine(DelayPlayerGoToAndStop(Player.transform.position + entranceDoor.GetDoorDirection() * 7f));
				}
				foreach (Objective_FindRelic item in new List<Objective_FindRelic>(ObjectiveManager.GetObjectivesOfType<Objective_FindRelic>()))
				{
					if (item.TargetLocation == DungeonLocation)
					{
						DataManager.Instance.CanFindLeaderRelic = true;
						break;
					}
				}
				FirstArrival = false;
				SpawnDemons();
			}
			else
			{
				if (IsTeleporting)
				{
					Player.transform.position = ((Interaction_Chest.Instance == null) ? Vector3.zero : Interaction_Chest.Instance.transform.position);
				}
				else if (PrevCurrentX < CurrentX && (bool)West)
				{
					Player.transform.position = West.PlayerPosition.position;
					PlayerState.facingAngle = 0f;
				}
				else if (PrevCurrentX > CurrentX && (bool)East)
				{
					Player.transform.position = East.PlayerPosition.position;
					PlayerState.facingAngle = 180f;
				}
				else if (PrevCurrentY > CurrentY && (bool)North)
				{
					Player.transform.position = North.PlayerPosition.position;
					PlayerState.facingAngle = 270f;
				}
				else if (PrevCurrentY < CurrentY && (bool)South)
				{
					Player.transform.position = South.PlayerPosition.position;
					PlayerState.facingAngle = 90f;
				}
				else if (South != null)
				{
					UnityEngine.Debug.Log("NO WHERE TO PLACE YOU - so put you south!");
					Player.transform.position = South.PlayerPosition.position;
					PlayerState.facingAngle = 90f;
				}
				else
				{
					UnityEngine.Debug.Log("NO WHERE TO PLACE YOU!");
				}
				if (!CurrentRoom.Completed)
				{
					if (PlayerFarming.Instance != null && PlayerFarming.Instance.playerController != null)
					{
						PlayerFarming.Instance.playerController.MakeUntouchable(TrinketManager.GetInvincibilityTimeEnteringNewRoom());
					}
					bool flag = false;
					UnitObject[] componentsInChildren = CurrentRoom.generateRoom.GetComponentsInChildren<UnitObject>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						if (componentsInChildren[i].health.team == Health.Team.Team2)
						{
							flag = true;
							break;
						}
					}
					bool flag2 = CurrentRoom.generateRoom.GetComponentInChildren<DungeonLeaderMechanics>() != null;
					if (flag && TrinketManager.HasTrinket(TarotCards.Card.Arrows))
					{
						PlayerFarming.Instance.GetBlackSoul(Mathf.RoundToInt(FaithAmmo.Total - FaithAmmo.Ammo), false);
					}
					if ((flag || flag2) && Door.Doors.Count > 0)
					{
						Vector3 vector = Utils.DegreeToVector2(PlayerState.facingAngle);
						PlayerFarming instance = PlayerFarming.Instance;
						if ((object)instance != null)
						{
							instance.GoToAndStop(Player.transform.position + vector * 3f, null, true, false, delegate
							{
								StartCoroutine(DelayActivateRoom());
							}, 2f, true);
						}
					}
					else
					{
						Vector3 vector2 = Utils.DegreeToVector2(PlayerState.facingAngle);
						PlayerFarming instance2 = PlayerFarming.Instance;
						if ((object)instance2 != null)
						{
							instance2.GoToAndStop(Player.transform.position + vector2, null, true, false, delegate
							{
								StartCoroutine(DelayActivateRoom());
							});
						}
					}
				}
				else
				{
					Vector3 vector3 = Utils.DegreeToVector2(PlayerState.facingAngle);
					PlayerFarming instance3 = PlayerFarming.Instance;
					if ((object)instance3 != null)
					{
						instance3.GoToAndStop(Player.transform.position + vector3, null, true, false, delegate
						{
							StartCoroutine(DelayActivateRoom());
						});
					}
				}
			}
			IsTeleporting = false;
			PrevCurrentX = CurrentX;
			PrevCurrentY = CurrentY;
			GameManager.GetInstance().CameraSnapToPosition(Player.transform.position);
			GameManager.GetInstance().AddPlayerToCamera();
			return Player;
		}

		private IEnumerator DelayActivateRoom()
		{
			yield return new WaitForSeconds(0.5f);
			Instance.CurrentRoom.Active = true;
		}

		public void SpawnDemons()
		{
			if (!DungeonSandboxManager.Active && spawnDemons && !spawnedDemons)
			{
				spawnedDemons = true;
				int num = -1;
				while (++num < DataManager.Instance.Followers_Demons_IDs.Count)
				{
					SpawnDemon(DataManager.Instance.Followers_Demons_Types[num], DataManager.Instance.Followers_Demons_IDs[num]);
				}
			}
		}

		public void SpawnDemon(int type, int followerID, int forcedLevel = -1, bool doEffects = false, Action<Demon> spawnedDemon = null)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(new List<string> { "Assets/Prefabs/Enemies/Demons/Demon_Shooty.prefab", "Assets/Prefabs/Enemies/Demons/Demon_Chomp.prefab", "Assets/Prefabs/Enemies/Demons/Demon_Arrows.prefab", "Assets/Prefabs/Enemies/Demons/Demon_Collector.prefab", "Assets/Prefabs/Enemies/Demons/Demon_Exploder.prefab", "Assets/Prefabs/Enemies/Demons/Demon_Spirit.prefab", "Assets/Prefabs/Enemies/Demons/Demon_Baal.prefab", "Assets/Prefabs/Enemies/Demons/Demon_Aym.prefab" }[type], Player.transform.position, Quaternion.identity, Player.transform.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				Demon component = obj.Result.GetComponent<Demon>();
				obj.Result.transform.position = Player.transform.position;
				component.Init(followerID, forcedLevel);
				Demon_Spirit demon_Spirit;
				if (doEffects && (object)(demon_Spirit = component as Demon_Spirit) != null)
				{
					(PlayerFarming.Instance.health as HealthPlayer).TotalSpiritHearts += Mathf.Ceil((float)demon_Spirit.Level / 2f);
				}
				Action<Demon> action = spawnedDemon;
				if (action != null)
				{
					action(component);
				}
			};
		}

		public void ApplyCurrentDungeonModifiers()
		{
			StartCoroutine(ApplyCurrentDungeonModifiersIE());
		}

		private IEnumerator ApplyCurrentDungeonModifiersIE()
		{
			HealthPlayer healthPlayer = PlayerFarming.Instance.health as HealthPlayer;
			if (DataManager.Instance.PlayerFleece == 7)
			{
				DataManager.Instance.RedHeartsTemporarilyRemoved += (int)(DataManager.Instance.PLAYER_HEALTH - PlayerFleeceManager.OneHitKillHP);
				PlayerFarming.Instance.health.totalHP = PlayerFleeceManager.OneHitKillHP;
				healthPlayer.HP = PlayerFleeceManager.OneHitKillHP;
				healthPlayer.BlackHearts = 0f;
				healthPlayer.BlueHearts = 0f;
			}
			else if (DungeonModifier.HasNeutralModifier(DungeonNeutralModifier.LoseRedGainBlackHeart))
			{
				DataManager.Instance.RedHeartsTemporarilyRemoved += 2;
				healthPlayer.totalHP -= 2f;
				healthPlayer.BlackHearts += 4f;
			}
			else if (DungeonModifier.HasNeutralModifier(DungeonNeutralModifier.LoseRedGainTarot))
			{
				DataManager.Instance.RedHeartsTemporarilyRemoved += 2;
				healthPlayer.totalHP -= 2f;
				yield return StartCoroutine(DoTarotRoutine(Vector3.zero, Vector3.one));
			}
		}

		private IEnumerator ApplyCurrentFleeceModifiersIE()
		{
			if (DataManager.Instance.PlayerFleece == 5)
			{
				DataManager.Instance.RedHeartsTemporarilyRemoved += (int)DataManager.Instance.PLAYER_HEALTH;
				PlayerFarming.Instance.health.totalHP -= DataManager.Instance.PLAYER_HEALTH;
				PlayerFarming.Instance.health.BlueHearts += DataManager.Instance.PLAYER_HEALTH * 1.5f;
			}
			else if (DataManager.Instance.PlayerFleece == 2)
			{
				DataManager.Instance.RedHeartsTemporarilyRemoved += (DataManager.Instance.PLAYER_STARTING_HEALTH + DataManager.Instance.PLAYER_HEARTS_LEVEL) / 2;
				PlayerFarming.Instance.health.totalHP -= (DataManager.Instance.PLAYER_STARTING_HEALTH + DataManager.Instance.PLAYER_HEARTS_LEVEL) / 2;
			}
			if (PlayerFleeceManager.GetFreeTarotCards() > 0)
			{
				yield return DoFleeceTarotRoutine();
			}
		}

		private IEnumerator ApplyCurrentDemonModifiersIE()
		{
			foreach (GameObject demon in Demon_Arrows.Demons)
			{
				if ((bool)demon.GetComponent<Demon_Spirit>())
				{
					Demon_Spirit component = demon.GetComponent<Demon_Spirit>();
					(PlayerFarming.Instance.health as HealthPlayer).TotalSpiritHearts += Mathf.Ceil((float)component.Level / 2f);
				}
			}
			yield return new WaitForSeconds(0.5f);
		}

		private IEnumerator DoFleeceTarotRoutine()
		{
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			PlayerFarming.Instance.state.facingAngle = -90f;
			GameManager.GetInstance().CamFollowTarget.DisablePlayerLook = true;
			GameManager.GetInstance().CameraSetTargetZoom(4f);
			yield return new WaitForSecondsRealtime(0.35f);
			HUD_Manager.Instance.Hide(false, 0);
			LetterBox.Show(false);
			AudioManager.Instance.PlayOneShot("event:/tarot/tarot_card_pull", base.gameObject);
			PlayerFarming.Instance.simpleSpineAnimator.Animate("cards/cards-start", 0, false);
			PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("cards/cards-loop", 0, true, 0f);
			yield return new WaitForSeconds(1f);
			GameManager.GetInstance().CameraSetTargetZoom(6f);
			TarotCards.TarotCard drawnCard1;
			TarotCards.TarotCard drawnCard2;
			TarotCards.TarotCard drawnCard3;
			TarotCards.TarotCard drawnCard4;
			if (PlayerFleeceManager.FleeceSwapsWeaponForCurse())
			{
				drawnCard1 = new TarotCards.TarotCard(TarotCards.Card.BlackSoulAutoRecharge, 1);
				drawnCard2 = new TarotCards.TarotCard(TarotCards.Card.IncreaseBlackSoulsDrop, 1);
				drawnCard3 = new TarotCards.TarotCard(TarotCards.Card.AmmoEfficient, 1);
				drawnCard4 = new TarotCards.TarotCard(TarotCards.Card.Arrows, 0);
				DataManager.Instance.PlayerRunTrinkets.Add(drawnCard1);
				DataManager.Instance.PlayerRunTrinkets.Add(drawnCard2);
				DataManager.Instance.PlayerRunTrinkets.Add(drawnCard3);
				DataManager.Instance.PlayerRunTrinkets.Add(drawnCard4);
			}
			else
			{
				drawnCard1 = TarotCards.DrawRandomCard();
				DataManager.Instance.PlayerRunTrinkets.Add(drawnCard1);
				drawnCard2 = TarotCards.DrawRandomCard();
				DataManager.Instance.PlayerRunTrinkets.Add(drawnCard2);
				drawnCard3 = TarotCards.DrawRandomCard();
				DataManager.Instance.PlayerRunTrinkets.Add(drawnCard3);
				drawnCard4 = TarotCards.DrawRandomCard();
				DataManager.Instance.PlayerRunTrinkets.Add(drawnCard4);
			}
			UIFleeceTarotRewardOverlayController tarotRewardOverlay = MonoSingleton<UIManager>.Instance.ShowFleeceTarotReward(drawnCard1, drawnCard2, drawnCard3, drawnCard4);
			UIFleeceTarotRewardOverlayController uIFleeceTarotRewardOverlayController = tarotRewardOverlay;
			uIFleeceTarotRewardOverlayController.OnHidden = (Action)Delegate.Combine(uIFleeceTarotRewardOverlayController.OnHidden, (Action)delegate
			{
				tarotRewardOverlay = null;
			});
			while (tarotRewardOverlay != null)
			{
				yield return null;
			}
			LetterBox.Hide();
			HUD_Manager.Instance.Show(0);
			AudioManager.Instance.PlayOneShot("event:/tarot/tarot_card_close", base.gameObject);
			GameManager.GetInstance().CameraResetTargetZoom();
			GameManager.GetInstance().CamFollowTarget.DisablePlayerLook = false;
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
			yield return null;
			PlayerFarming.Instance.simpleSpineAnimator.Animate("cards/cards-stop-seperate", 0, false);
			PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
			yield return new WaitForSeconds(0.2f);
			if (drawnCard1 != null)
			{
				TrinketManager.AddTrinket(drawnCard1);
			}
			if (drawnCard2 != null)
			{
				TrinketManager.AddTrinket(drawnCard2);
			}
			if (drawnCard3 != null)
			{
				TrinketManager.AddTrinket(drawnCard3);
			}
			if (drawnCard4 != null)
			{
				TrinketManager.AddTrinket(drawnCard4);
			}
		}

		private IEnumerator DoTarotRoutine(Vector3 position, Vector3 scale)
		{
			HUD_Manager.Instance.Hide(false, 0);
			GameManager.GetInstance().CameraSetTargetZoom(4f);
			LetterBox.Show(false);
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			PlayerFarming.Instance.state.facingAngle = -90f;
			GameManager.GetInstance().CamFollowTarget.DisablePlayerLook = true;
			AudioManager.Instance.PlayOneShot("event:/tarot/tarot_card_pull", base.gameObject);
			PlayerFarming.Instance.simpleSpineAnimator.Animate("cards/cards-start", 0, false);
			PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("cards/cards-loop", 0, true, 0f);
			TarotCards.TarotCard drawnCard = TarotCards.DrawRandomCard();
			DataManager.Instance.PlayerRunTrinkets.Add(drawnCard);
			yield return new WaitForSeconds(1f);
			GameManager.GetInstance().CameraSetTargetZoom(6f);
			bool waiting = true;
			GameObject gameObject = UITrinketCards.Play(drawnCard, delegate
			{
				waiting = false;
			});
			if ((bool)gameObject.GetComponent<MMButton>())
			{
				gameObject.GetComponent<MMButton>().enabled = false;
			}
			gameObject.transform.localScale = scale;
			((RectTransform)gameObject.transform).anchoredPosition = position;
			while (waiting)
			{
				yield return null;
			}
			StartCoroutine(BackToIdleRoutine(drawnCard));
		}

		private IEnumerator BackToIdleRoutine(TarotCards.TarotCard card)
		{
			LetterBox.Hide();
			HUD_Manager.Instance.Show(0);
			AudioManager.Instance.PlayOneShot("event:/tarot/tarot_card_close", base.gameObject);
			GameManager.GetInstance().CameraResetTargetZoom();
			GameManager.GetInstance().CamFollowTarget.DisablePlayerLook = false;
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
			yield return null;
			PlayerFarming.Instance.simpleSpineAnimator.Animate("cards/cards-stop-seperate", 0, false);
			PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
			GameManager.GetInstance().StartCoroutine(DelayEffectsRoutine(card));
		}

		private IEnumerator DelayEffectsRoutine(TarotCards.TarotCard card)
		{
			yield return new WaitForSeconds(0.2f);
			TrinketManager.AddTrinket(card);
		}

		private void GetDoors()
		{
			GameObject obj = GameObject.FindGameObjectWithTag("North Door");
			North = (((object)obj != null) ? obj.GetComponent<Door>() : null);
			GameObject obj2 = GameObject.FindGameObjectWithTag("East Door");
			East = (((object)obj2 != null) ? obj2.GetComponent<Door>() : null);
			GameObject obj3 = GameObject.FindGameObjectWithTag("South Door");
			South = (((object)obj3 != null) ? obj3.GetComponent<Door>() : null);
			GameObject obj4 = GameObject.FindGameObjectWithTag("West Door");
			West = (((object)obj4 != null) ? obj4.GetComponent<Door>() : null);
			if (CurrentRoom.N_Room != null)
			{
				Door north = North;
				if ((object)north != null)
				{
					north.Init(CurrentRoom.N_Room.ConnectionType);
				}
			}
			if (CurrentRoom.E_Room != null)
			{
				Door east = East;
				if ((object)east != null)
				{
					east.Init(CurrentRoom.E_Room.ConnectionType);
				}
			}
			if (CurrentRoom.S_Room != null)
			{
				Door south = South;
				if ((object)south != null)
				{
					south.Init(CurrentRoom.S_Room.ConnectionType);
				}
			}
			if (CurrentRoom.W_Room != null)
			{
				Door west = West;
				if ((object)west != null)
				{
					west.Init(CurrentRoom.W_Room.ConnectionType);
				}
			}
		}

		public void Left()
		{
			ChangeRoom(new Vector2Int(-1, 0));
		}

		public void Right()
		{
			ChangeRoom(new Vector2Int(1, 0));
		}

		public void Up()
		{
			ChangeRoom(new Vector2Int(0, 1));
		}

		public void Down()
		{
			ChangeRoom(new Vector2Int(0, -1));
		}
	}
}
