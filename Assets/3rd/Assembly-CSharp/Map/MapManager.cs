using System;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using Lamb.UI;
using MMBiomeGeneration;
using MMRoomGeneration;
using UnityEngine;

namespace Map
{
	public class MapManager : MonoBehaviour
	{
		public Action OnMapShown;

		public MapConfig DungeonConfig;

		public static MapManager Instance;

		private UIAdventureMapOverlayController _adventureMapInstance;

		private EventInstance _loopedSound;

		private float clipPlane;

		private Camera mainCamera;

		public Map CurrentMap { get; private set; }

		public Node CurrentNode
		{
			get
			{
				if (CurrentMap != null)
				{
					return CurrentMap.GetCurrentNode();
				}
				return null;
			}
		}

		public int CurrentLayer
		{
			get
			{
				if (CurrentMap == null)
				{
					CurrentMap = MapGenerator.GetMap(DungeonConfig);
				}
				Node currentNode = CurrentMap.GetCurrentNode();
				for (int i = 0; i < MapGenerator.Nodes.Count; i++)
				{
					if (MapGenerator.Nodes[i].Contains(currentNode))
					{
						return i;
					}
				}
				return 0;
			}
		}

		public bool MapGenerated { get; set; }

		private void Awake()
		{
			mainCamera = Camera.main;
			Instance = this;
		}

		public void OnDestroy()
		{
			Instance = null;
		}

		public UIAdventureMapOverlayController ShowMap(bool disableInput = false)
		{
			SimulationManager.Pause();
			if (!MapGenerated)
			{
				GenerateNewMap();
			}
			_adventureMapInstance = MonoSingleton<UIManager>.Instance.ShowAdventureMap(CurrentMap, disableInput);
			UIAdventureMapOverlayController adventureMapInstance = _adventureMapInstance;
			adventureMapInstance.OnShow = (Action)Delegate.Combine(adventureMapInstance.OnShow, (Action)delegate
			{
				if (mainCamera.farClipPlane != 0.02f)
				{
					clipPlane = mainCamera.farClipPlane;
				}
				mainCamera.farClipPlane = 0.02f;
				Action onMapShown = OnMapShown;
				if (onMapShown != null)
				{
					onMapShown();
				}
				GameObject go = ((PlayerFarming.Instance != null) ? PlayerFarming.Instance.gameObject : base.gameObject);
				_loopedSound = AudioManager.Instance.CreateLoop("event:/atmos/forest/temple_door_hum", go, true);
			});
			UIAdventureMapOverlayController adventureMapInstance2 = _adventureMapInstance;
			adventureMapInstance2.OnHide = (Action)Delegate.Combine(adventureMapInstance2.OnHide, (Action)delegate
			{
				if (clipPlane != 0f)
				{
					mainCamera.farClipPlane = clipPlane;
				}
				AudioManager.Instance.StopLoop(_loopedSound);
			});
			return _adventureMapInstance;
		}

		public void CloseMap()
		{
			SimulationManager.UnPause();
			_adventureMapInstance.Hide();
		}

		public Map GenerateNewMap()
		{
			BiomeGenerator.Instance.RandomiseSeed();
			CurrentMap = MapGenerator.GetMap(DungeonConfig);
			MapGenerated = true;
			return CurrentMap;
		}

		public void AddNodeToPath(Node mapNode)
		{
			CurrentMap.path.Add(mapNode.point);
			DungeonModifier.SetActiveModifier(mapNode.Modifier);
			if (mapNode.nodeType.ShouldIncrementRandomRoomsEncountered())
			{
				DataManager.Instance.MinimumRandomRoomsEncounteredAmount++;
			}
		}

		public void EnterNode(Node mapNode)
		{
			if (DungeonSandboxManager.Active)
			{
				GameManager.DungeonEndlessLevel++;
				if (mapNode.DungeonLocation != FollowerLocation.None)
				{
					DungeonSandboxManager.Instance.SetDungeonType(mapNode.DungeonLocation);
				}
				else if (mapNode.blueprint.ForcedDungeon == FollowerLocation.None)
				{
					DungeonSandboxManager.Instance.UpdateDungeonType();
				}
				else
				{
					DungeonSandboxManager.Instance.SetDungeonType(mapNode.blueprint.ForcedDungeon);
				}
			}
			Debug.Log("EnterNode " + mapNode.nodeType);
			switch (mapNode.nodeType)
			{
			case NodeType.FirstFloor:
				GameManager.CurrentDungeonFloor = 1;
				if (GameManager.DungeonEndlessLevel <= 1 && !DungeonSandboxManager.Active)
				{
					Inteaction_DoorRoomDoor.GetFloor(PlayerFarming.Location, true);
				}
				BiomeGenerator.Instance.OverrideRandomWalk = false;
				break;
			case NodeType.Boss:
			case NodeType.DungeonFloor:
			case NodeType.MiniBossFloor:
			case NodeType.FinalBoss:
				Debug.Log("AAA " + mapNode.nodeType);
				if (!GameManager.InitialDungeonEnter)
				{
					GameManager.CurrentDungeonFloor++;
				}
				BiomeGenerator.Instance.OverrideRandomWalk = false;
				if (!DungeonSandboxManager.Active || ((mapNode.nodeType != NodeType.FinalBoss || mapNode.blueprint.nodeType != NodeType.MiniBossFloor) && mapNode.nodeType != NodeType.MiniBossFloor && mapNode.nodeType != NodeType.Boss && mapNode.blueprint.RoomPrefabs.Length == 0))
				{
					break;
				}
				BiomeGenerator.Instance.OverrideRooms = new List<BiomeGenerator.OverrideRoom>
				{
					new BiomeGenerator.OverrideRoom
					{
						North = GenerateRoom.ConnectionTypes.DoorRoom,
						South = GenerateRoom.ConnectionTypes.Entrance,
						Generated = BiomeGenerator.FixedRoom.Generate.DontGenerate,
						PrefabPath = BiomeGenerator.Instance.LeaderRoomPath
					}
				};
				if ((mapNode.nodeType == NodeType.MiniBossFloor || mapNode.blueprint.nodeType == NodeType.FinalBoss || (mapNode.nodeType != NodeType.Boss && mapNode.blueprint.nodeType == NodeType.MiniBossFloor)) && mapNode.blueprint.RoomPrefabs.Length != 0)
				{
					BiomeGenerator.Instance.OverrideRooms = new List<BiomeGenerator.OverrideRoom>
					{
						new BiomeGenerator.OverrideRoom
						{
							North = GenerateRoom.ConnectionTypes.DoorRoom,
							South = GenerateRoom.ConnectionTypes.Entrance,
							Generated = ((mapNode.blueprint.nodeType == NodeType.FinalBoss) ? BiomeGenerator.FixedRoom.Generate.DontGenerate : BiomeGenerator.FixedRoom.Generate.GenerateOnLoad),
							PrefabPath = mapNode.blueprint.RoomPrefabs[UnityEngine.Random.Range(0, mapNode.blueprint.RoomPrefabs.Length)],
							x = 0,
							y = 0
						}
					};
					if (Instance.CurrentMap.GetFinalBossNode() == Instance.CurrentNode)
					{
						BiomeGenerator.Instance.OverrideRooms[0].North = GenerateRoom.ConnectionTypes.True;
						BiomeGenerator.Instance.OverrideRooms.Insert(0, new BiomeGenerator.OverrideRoom
						{
							North = GenerateRoom.ConnectionTypes.False,
							South = GenerateRoom.ConnectionTypes.True,
							Generated = BiomeGenerator.FixedRoom.Generate.GenerateOnLoad,
							PrefabPath = BiomeGenerator.Instance.EndOfFloorRoomPath,
							x = 0,
							y = 1
						});
					}
				}
				if (MapGenerator.GetNodesOnLayer(1).Contains(mapNode) && DungeonSandboxManager.Instance.CurrentScenarioType == DungeonSandboxManager.ScenarioType.BossRushMode)
				{
					BiomeGenerator.Instance.OverrideRooms[0].y = 1;
					BiomeGenerator.Instance.OverrideRooms[0].South = GenerateRoom.ConnectionTypes.True;
					BiomeGenerator.Instance.OverrideRooms.Insert(0, new BiomeGenerator.OverrideRoom
					{
						North = GenerateRoom.ConnectionTypes.True,
						South = GenerateRoom.ConnectionTypes.Entrance,
						Generated = BiomeGenerator.FixedRoom.Generate.GenerateOnLoad,
						PrefabPath = BiomeGenerator.Instance.EntranceRoomPath,
						x = 0,
						y = 0
					});
				}
				BiomeGenerator.Instance.DoFirstArrivalRoutine = true;
				BiomeGenerator.Instance.OverrideRandomWalk = true;
				break;
			case NodeType.Intro_TeleportHome:
				BiomeGenerator.Instance.OverrideRooms = new List<BiomeGenerator.OverrideRoom>
				{
					new BiomeGenerator.OverrideRoom
					{
						North = GenerateRoom.ConnectionTypes.False,
						South = GenerateRoom.ConnectionTypes.Entrance,
						Generated = BiomeGenerator.FixedRoom.Generate.GenerateOnLoad,
						PrefabPath = mapNode.blueprint.RoomPrefabs[UnityEngine.Random.Range(0, mapNode.blueprint.RoomPrefabs.Length)]
					}
				};
				BiomeGenerator.Instance.OverrideRandomWalk = true;
				break;
			case NodeType.Follower:
			case NodeType.Follower_Beginner:
			case NodeType.Follower_Easy:
			case NodeType.Follower_Medium:
			case NodeType.Follower_Hard:
			{
				string prefabPath = mapNode.blueprint.RoomPrefabs[0];
				bool flag = false;
				foreach (ObjectivesData objective in DataManager.Instance.Objectives)
				{
					if (objective.Type == Objectives.TYPES.FIND_FOLLOWER && ((Objectives_FindFollower)objective).TargetLocation == BiomeGenerator.Instance.DungeonLocation)
					{
						flag = true;
						break;
					}
				}
				if (!flag && mapNode.blueprint.RoomPrefabs.Length >= 3)
				{
					float value = UnityEngine.Random.value;
					if (value < 0.2f)
					{
						prefabPath = mapNode.blueprint.RoomPrefabs[1];
					}
					else if (value < 0.4f && DataManager.Instance.BeatenFirstMiniBoss && !DataManager.Instance.DissentingFolllowerRooms.Contains(PlayerFarming.Location))
					{
						prefabPath = mapNode.blueprint.RoomPrefabs[2];
						DataManager.Instance.DissentingFolllowerRooms.Add(PlayerFarming.Location);
					}
				}
				BiomeGenerator.Instance.OverrideRooms = new List<BiomeGenerator.OverrideRoom>
				{
					new BiomeGenerator.OverrideRoom
					{
						North = GenerateRoom.ConnectionTypes.NextLayer,
						South = GenerateRoom.ConnectionTypes.Entrance,
						Generated = BiomeGenerator.FixedRoom.Generate.GenerateOnLoad,
						PrefabPath = prefabPath
					}
				};
				BiomeGenerator.Instance.OverrideRandomWalk = true;
				break;
			}
			default:
				Debug.Log("OVERRIDE!");
				BiomeGenerator.Instance.OverrideRooms = new List<BiomeGenerator.OverrideRoom>
				{
					new BiomeGenerator.OverrideRoom
					{
						North = GenerateRoom.ConnectionTypes.NextLayer,
						South = GenerateRoom.ConnectionTypes.Entrance,
						Generated = BiomeGenerator.FixedRoom.Generate.GenerateOnLoad,
						PrefabPath = mapNode.blueprint.RoomPrefabs[UnityEngine.Random.Range(0, mapNode.blueprint.RoomPrefabs.Length)]
					}
				};
				BiomeGenerator.Instance.OverrideRandomWalk = true;
				if (DungeonSandboxManager.Active && mapNode.nodeType == NodeType.FinalBoss)
				{
					BiomeGenerator.Instance.DoFirstArrivalRoutine = false;
					BiomeGenerator.Instance.OverrideRooms[0].Generated = BiomeGenerator.FixedRoom.Generate.DontGenerate;
				}
				break;
			}
			DataManager.Instance.dungeonVisitedRooms.Add(mapNode.nodeType);
			DataManager.Instance.dungeonLocationsVisited.Add(PlayerFarming.Location);
			DataManager.Instance.FollowersRecruitedInNodes.Add(DataManager.Instance.FollowersRecruitedThisNode);
			DataManager.Instance.FollowersRecruitedThisNode = 0;
			BiomeGenerator.Instance.Regenerate(delegate
			{
				if (_adventureMapInstance != null)
				{
					_adventureMapInstance.Hide();
				}
				BiomeConstants.Instance.CreatePools();
			});
		}

		public static NodeBlueprint GetBlueprint(NodeType type, MapConfig config)
		{
			NodeBlueprint nodeBlueprint = config.nodeBlueprints.FirstOrDefault((NodeBlueprint n) => n.nodeType == type);
			if (nodeBlueprint != null)
			{
				return nodeBlueprint;
			}
			if (type == config.FirstFloorBluePrint.nodeType)
			{
				return config.FirstFloorBluePrint;
			}
			if (type == config.SecondFloorBluePrint.nodeType)
			{
				return config.SecondFloorBluePrint;
			}
			if (type == config.MiniBossFloorBluePrint.nodeType)
			{
				return config.MiniBossFloorBluePrint;
			}
			if (type == config.TreasureBluePrint.nodeType)
			{
				return config.TreasureBluePrint;
			}
			if (type == config.LeaderFloorBluePrint.nodeType)
			{
				return config.LeaderFloorBluePrint;
			}
			foreach (MapLayer layer in config.layers)
			{
				if (layer.nodeType == type)
				{
					return layer.BluePrint;
				}
			}
			return nodeBlueprint;
		}
	}
}
