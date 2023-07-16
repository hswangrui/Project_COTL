using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using MMBiomeGeneration;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace MMRoomGeneration
{
	public class GenerateRoom : BaseMonoBehaviour
	{
		public delegate void GenerateEvent();

		public class RoomPath
		{
			public IslandConnector.Direction Direction;

			public int Encounters;

			public bool Door;

			public ConnectionTypes ConnectionType;

			public RoomPath(IslandConnector.Direction Direction, bool Door, ConnectionTypes ConnectionType)
			{
				this.Direction = Direction;
				this.Door = Door;
				this.ConnectionType = ConnectionType;
			}
		}

		public enum ConnectionTypes
		{
			False,
			True,
			Entrance,
			Exit,
			Boss,
			DoorRoom,
			NextLayer,
			DungeonFirstRoom,
			LeaderBoss,
			Tarot,
			WeaponShop,
			RelicShop
		}

		public static GenerateRoom Instance;

		public int Seed;

		private System.Random RandomSeed;

		public SoundConstants.RoomID roomMusicID = SoundConstants.RoomID.StandardAmbience;

		[EventRef]
		public string biomeAtmosOverridePath = string.Empty;

		public Sprite MapIcon;

		public List<GeneraterDecorations> DecorationSetList = new List<GeneraterDecorations>();

		[HideInInspector]
		public GeneraterDecorations DecorationList;

		public bool CreateOnlyOneEncounterRoom;

		public bool CreateRandomExtraPaths = true;

		[Range(0f, 1f)]
		public float EncounterWillBeEnemyOrResource = 0.5f;

		public CompositeCollider2D RoomTransform;

		public GameObject SceneryTransform;

		public GameObject CustomTransform;

		[HideInInspector]
		public int Scale = 2;

		public bool LockingDoors = true;

		public bool LockEntranceBehindPlayer;

		public ConnectionTypes North;

		public ConnectionTypes East;

		public ConnectionTypes South;

		public ConnectionTypes West;

		public GameObject BloodSplatterPrefab;

		public IslandPiece NorthIsland;

		public IslandPiece EastIsland;

		public IslandPiece SouthIsland;

		public IslandPiece WestIsland;

		public IslandPiece NorthBossDoor;

		public IslandPiece EastBossDoor;

		public IslandPiece SouthBossDoor;

		public IslandPiece WestBossDoor;

		public IslandPiece NorthEntranceDoor;

		public IslandPiece EastEntranceDoor;

		public IslandPiece SouthEntranceDoor;

		public IslandPiece WestEntranceDoor;

		public IslandPiece NorthBossDoor_P2;

		public IslandPiece EastBossDoor_P2;

		public IslandPiece SouthBossDoor_P2;

		public IslandPiece WestBossDoor_P2;

		public List<IslandPiece> StartPieces = new List<IslandPiece>();

		public List<IslandPiece> IslandPieces = new List<IslandPiece>();

		public List<IslandPiece> ResourcePieces = new List<IslandPiece>();

		private List<IslandPiece> NorthIslandPieces;

		private List<IslandPiece> EastIslandPieces;

		private List<IslandPiece> SouthIslandPieces;

		private List<IslandPiece> WestIslandPieces;

		private List<IslandPiece> NorthIslandEncounterPieces;

		private List<IslandPiece> EastIslandEncounterPieces;

		private List<IslandPiece> SouthIslandEncounterPieces;

		private List<IslandPiece> WestIslandEncounterPieces;

		private List<IslandPiece> NorthIslandResourcesPieces;

		private List<IslandPiece> EastIslandResourcesPieces;

		private List<IslandPiece> SouthIslandResourcesPieces;

		private List<IslandPiece> WestIslandResourcesPieces;

		public List<IslandPiece> Pieces = new List<IslandPiece>();

		private IslandPiece CurrentPiece;

		private IslandPiece PrevPiece;

		private List<int> PreviousSeeds = new List<int>();

		private bool Testing;

		private List<RoomPath> Paths;

		private float PrevTime;

		private float LimitNorth = -2.1474836E+09f;

		private float LimitEast = -2.1474836E+09f;

		private float LimitSouth = 2.1474836E+09f;

		private float LimitWest = 2.1474836E+09f;

		private SpriteShapeController RoomSpriteShape;

		private List<List<int>> DecorationGrid;

		private int DecorationGridWidth;

		private int DecorationGridHeight;

		private GeneraterDecorations.DecorationAndProbability d;

		private GameObject PerlinNoiseDecoration;

		private Vector3 PerlinScale;

		private string NoiseName;

		private float Noise;

		private List<SpriteShapeController> _SpriteShapeControllers;

		private static SpriteShapeController CurrentSpriteShape;

		private static List<Vector3> Points;

		private Vector2 RoomPerlinOffset;

		private List<IslandConnector> Connectors;

		private IslandConnector CurrentConnector;

		private IslandConnector Connector;

		private IslandPiece RandomPiece;

		private List<Collider2D> Collisions;

		private IslandPiece NorthTarotDoor
		{
			get
			{
				return Resources.Load<IslandPiece>("Prefabs/Custom Entrances/Door Tarot North");
			}
		}

		private IslandPiece EastTarotDoor
		{
			get
			{
				return Resources.Load<IslandPiece>("Prefabs/Custom Entrances/Door Tarot East");
			}
		}

		private IslandPiece SouthTarotDoor
		{
			get
			{
				return Resources.Load<IslandPiece>("Prefabs/Custom Entrances/Door Tarot South");
			}
		}

		private IslandPiece WestTarotDoor
		{
			get
			{
				return Resources.Load<IslandPiece>("Prefabs/Custom Entrances/Door Tarot West");
			}
		}

		private IslandPiece NorthWeaponDoor
		{
			get
			{
				return Resources.Load<IslandPiece>("Prefabs/Custom Entrances/Door Weapon North");
			}
		}

		private IslandPiece EastWeaponDoor
		{
			get
			{
				return Resources.Load<IslandPiece>("Prefabs/Custom Entrances/Door Weapon East");
			}
		}

		private IslandPiece SouthWeaponDoor
		{
			get
			{
				return Resources.Load<IslandPiece>("Prefabs/Custom Entrances/Door Weapon South");
			}
		}

		private IslandPiece WestWeaponDoor
		{
			get
			{
				return Resources.Load<IslandPiece>("Prefabs/Custom Entrances/Door Weapon West");
			}
		}

		private IslandPiece NorthRelicDoor
		{
			get
			{
				return Resources.Load<IslandPiece>("Prefabs/Custom Entrances/Door Relic North");
			}
		}

		private IslandPiece EastRelicDoor
		{
			get
			{
				return Resources.Load<IslandPiece>("Prefabs/Custom Entrances/Door Relic East");
			}
		}

		private IslandPiece SouthRelicDoor
		{
			get
			{
				return Resources.Load<IslandPiece>("Prefabs/Custom Entrances/Door Relic South");
			}
		}

		private IslandPiece WestRelicDoor
		{
			get
			{
				return Resources.Load<IslandPiece>("Prefabs/Custom Entrances/Door Relic West");
			}
		}

		public IslandPiece StartPiece { get; private set; }

		public bool generated { get; private set; }

		public bool GeneratedDecorations { get; set; }

		public List<SpriteShapeController> SpriteShapeControllers
		{
			get
			{
				if (_SpriteShapeControllers == null)
				{
					_SpriteShapeControllers = new List<SpriteShapeController>(base.gameObject.GetComponentsInChildren<SpriteShapeController>());
				}
				return _SpriteShapeControllers;
			}
		}

		public event GenerateEvent OnGenerated;

		private void OnEnable()
		{
			Instance = this;
			InitSpriteShapes();
			if (generated)
			{
				StartCoroutine(RegenerateDecorationsWithPool());
			}
		}

		private IEnumerator RegenerateDecorationsWithPool()
		{
			RoomTransform.geometryType = CompositeCollider2D.GeometryType.Polygons;
			RoomTransform.GenerateGeometry();
			Physics2D.SyncTransforms();
			yield return StartCoroutine(SpawnDecorations(true));
			SetCollider();
			Debug.Log("OBJECT POOL GenerateRoom.OnEnable PoolCount: " + ObjectPool.CountAllPooled());
		}

		private void OnDisable()
		{
			if (Instance == this)
			{
				Instance = null;
			}
			if (generated && SceneryTransform != null)
			{
				for (int num = SceneryTransform.transform.childCount - 1; num >= 0; num--)
				{
					ObjectPool.Recycle(SceneryTransform.transform.GetChild(num).gameObject);
				}
			}
			Debug.Log("OBJECT POOL GenerateRoom.OnDisable PoolCount: " + ObjectPool.CountAllPooled());
		}

		private void OnDestroy()
		{
			BloodSplatterPrefab = null;
		}

		private void Start()
		{
			PreviousSeeds.Add(Seed);
			if (BloodSplatterPrefab == null)
			{
				BloodSplatterPrefab = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/BloodParticles_Prefab"), base.transform) as GameObject;
			}


			


		}

		private void GeneratePreviousSeed()
		{
			if (PreviousSeeds.Count > 1)
			{
				PreviousSeeds.RemoveAt(PreviousSeeds.Count - 1);
				Seed = PreviousSeeds[PreviousSeeds.Count - 1];
				StartCoroutine(Generate());
			}
		}

		public void GenerateRandomSeedTest()
		{
			Testing = true;
			GenerateRandomSeed();
			Testing = false;
		}

		private void GenerateRandomSeed()
		{
			Seed = UnityEngine.Random.Range(0, int.MaxValue);
			PreviousSeeds.Add(Seed);
			StartCoroutine(Generate());
		}

		public void Generate(int Seed, ConnectionTypes North, ConnectionTypes East, ConnectionTypes South, ConnectionTypes West)
		{
			this.Seed = Seed;
			this.North = North;
			this.East = East;
			this.South = South;
			this.West = West;
			StartCoroutine(Generate());
		}

		private void GenerateRoomFunc()
		{
			StartCoroutine(Generate());
		}

		private IEnumerator Generate()
		{
			ClearPrefabs();
			Pieces = new List<IslandPiece>();
			RandomSeed = new System.Random(Seed);
			CollateLists();
			CreateStartPiece();
			CreatePaths();
			PlaceDoors();
			CompositeColliders();
			RoomTransform.geometryType = CompositeCollider2D.GeometryType.Polygons;
			RoomTransform.GenerateGeometry();
			Physics2D.SyncTransforms();
			PlaceDecorations(false);
			CreateSpriteShape();
			yield return StartCoroutine(DisableIslands());
			yield return StartCoroutine(SpawnDecorations(true));
			CreateBackgroundSpriteShape();
			SetColliderAndUpdatePathfinding();
			InitSpriteShapes();
			generated = true;
			GenerateEvent onGenerated = this.OnGenerated;
			if (onGenerated != null)
			{
				onGenerated();
			}
		}

		private void InitSpriteShapes()
		{
			SpriteShapeRenderer[] obj = (SpriteShapeRenderer[])UnityEngine.Object.FindObjectsOfType(typeof(SpriteShapeRenderer));
			CommandBuffer commandBuffer = new CommandBuffer();
			commandBuffer.GetTemporaryRT(0, 256, 256, 0);
			commandBuffer.SetRenderTarget(0);
			SpriteShapeRenderer[] array = obj;
			foreach (SpriteShapeRenderer spriteShapeRenderer in array)
			{
				SpriteShapeController component = spriteShapeRenderer.gameObject.GetComponent<SpriteShapeController>();
				if (spriteShapeRenderer != null && component != null && !spriteShapeRenderer.isVisible)
				{
					component.BakeMesh();
					commandBuffer.DrawRenderer(spriteShapeRenderer, spriteShapeRenderer.sharedMaterial);
				}
			}
			commandBuffer.ReleaseTemporaryRT(0);
			Graphics.ExecuteCommandBuffer(commandBuffer);
		}

		public void SetCollider()
		{
			RoomTransform.geometryType = CompositeCollider2D.GeometryType.Outlines;
			RoomTransform.gameObject.layer = LayerMask.NameToLayer("Island");
			RoomTransform.GenerateGeometry();
		}

		public void SetColliderAndUpdatePathfinding()
		{
			SetCollider();
			if (Testing)
			{
				RoomTransform.enabled = false;
				Physics2D.SyncTransforms();
			}
			if (Application.isPlaying && (bool)AstarPath.active)
			{
				StartCoroutine(SetAStar());
			}
		}

		private IEnumerator SetAStar()
		{
			while (AstarPath.active == null)
			{
				yield return null;
			}
			int num = 5;
			AstarPath.active.data.gridGraph.center = RoomTransform.bounds.center;
			AstarPath.active.data.gridGraph.SetDimensions((int)RoomTransform.bounds.size.x * 2 + num, (int)RoomTransform.bounds.size.y * 2 + num, 0.5f);
			GameManager.RecalculatePaths(true);
		}

		private void CreatePaths()
		{
			Paths = new List<RoomPath>();
			if (North != 0)
			{
				Paths.Add(new RoomPath(IslandConnector.Direction.North, true, North));
			}
			if (East != 0)
			{
				Paths.Add(new RoomPath(IslandConnector.Direction.East, true, East));
			}
			if (South != 0)
			{
				Paths.Add(new RoomPath(IslandConnector.Direction.South, true, South));
			}
			if (West != 0)
			{
				Paths.Add(new RoomPath(IslandConnector.Direction.West, true, West));
			}
			int num = ((!CreateOnlyOneEncounterRoom) ? RandomSeed.Next(1, 3) : 0);
			while (--num > 0)
			{
				Paths[RandomSeed.Next(0, Paths.Count)].Encounters++;
			}
			foreach (RoomPath path in Paths)
			{
				GeneratePath(path);
			}
		}

		private IslandConnector.Direction PathGetUnusedDirection()
		{
			List<IslandConnector.Direction> list = new List<IslandConnector.Direction>
			{
				IslandConnector.Direction.North,
				IslandConnector.Direction.East,
				IslandConnector.Direction.South,
				IslandConnector.Direction.West
			};
			foreach (RoomPath path in Paths)
			{
				list.Remove(path.Direction);
			}
			return list[RandomSeed.Next(0, list.Count)];
		}

		private void CompositeColliders()
		{
			foreach (IslandPiece piece in Pieces)
			{
				piece.Collider.usedByComposite = true;
			}
		}

		private IEnumerator DisableIslands()
		{
			int completed = 0;
			foreach (IslandPiece piece in Pieces)
			{
				StartCoroutine(piece.InitIsland(RandomSeed, DecorationList.SpriteShapeSecondary, delegate
				{
					completed++;
				}));
			}
			while (completed < Pieces.Count)
			{
				yield return null;
			}
		}

		private void CustomLevel()
		{
			RandomSeed = new System.Random(UnityEngine.Random.Range(-2147483647, int.MaxValue));
			Pieces = new List<IslandPiece>();
			int num = -1;
			IslandPiece islandPiece = null;
			while (++num < RoomTransform.transform.childCount)
			{
				islandPiece = RoomTransform.transform.GetChild(num).GetComponent<IslandPiece>();
				if (islandPiece != null)
				{
					Pieces.Add(islandPiece);
				}
			}
			CompositeColliders();
			RoomTransform.geometryType = CompositeCollider2D.GeometryType.Polygons;
			RoomTransform.GenerateGeometry();
			Physics2D.SyncTransforms();
			ClearPrefabs(false);
			foreach (Transform item in RoomTransform.transform)
			{
				if (item.name.Contains("Sprite shape"))
				{
					UnityEngine.Object.DestroyImmediate(item.gameObject);
				}
			}
			PlaceDecorations(true);
			CreateSpriteShape();
			SpawnDecorations(true);
			CreateBackgroundSpriteShape();
			foreach (IslandPiece piece in Pieces)
			{
				piece.HideSprites();
			}
			SetColliderAndUpdatePathfinding();
		}

		private void CreateBackgroundSpriteShape()
		{
			if (DecorationList.SpriteShapeBack != null)
			{
				GameObject obj = UnityEngine.Object.Instantiate(Resources.Load("Room Back Sprite"), new Vector3(0f, 0f, 0.01f), Quaternion.identity, RoomTransform.transform) as GameObject;
				obj.transform.localScale = new Vector3(LimitEast - LimitWest, LimitNorth - LimitSouth);
				SpriteRenderer component = obj.GetComponent<SpriteRenderer>();
				component.shadowCastingMode = ShadowCastingMode.Off;
				component.receiveShadows = false;
				component.sortingLayerName = "Island";
			}
		}

		private void CreateSpriteShape()
		{
			int num = -1;
			while (++num < RoomTransform.pathCount)
			{
				GameObject gameObject = new GameObject();
				gameObject.transform.position = new Vector3(0f, 0f, 0.0001f);
				gameObject.transform.parent = RoomTransform.transform;
				gameObject.name = "Sprite shape " + num;
				RoomSpriteShape = gameObject.AddComponent<SpriteShapeController>();
				RoomSpriteShape.spriteShape = DecorationList.SpriteShape;
				if (DecorationList.SpriteShapeMaterial != null)
				{
					Material[] sharedMaterials = RoomSpriteShape.spriteShapeRenderer.sharedMaterials;
					for (int i = 0; i < sharedMaterials.Length; i++)
					{
						sharedMaterials[i] = DecorationList.SpriteShapeMaterial;
					}
					RoomSpriteShape.spriteShapeRenderer.sharedMaterials = sharedMaterials;
				}
				RoomSpriteShape.spriteShapeRenderer.shadowCastingMode = ShadowCastingMode.Off;
				RoomSpriteShape.spriteShapeRenderer.receiveShadows = true;
				RoomSpriteShape.fillPixelsPerUnit = 200f;
				RoomSpriteShape.gameObject.layer = 17;
				RoomSpriteShape.spline.Clear();
				RoomSpriteShape.splineDetail = 4;
				RoomSpriteShape.spriteShapeRenderer.sortingLayerName = "Ground";
				Vector2[] array = new Vector2[RoomTransform.GetPathPointCount(num)];
				RoomTransform.GetPath(num, array);
				Array.Reverse((Array)array);
				int num2 = 0;
				RoomSpriteShape.spline.InsertPointAt(0, array[0]);
				while (++num2 < array.Length)
				{
					if (Vector2.Distance(RoomTransform.transform.TransformPoint(array[num2]), RoomTransform.transform.TransformPoint(array[num2 - 1])) > 0.1f)
					{
						RoomSpriteShape.spline.InsertPointAt(RoomSpriteShape.spline.GetPointCount() - 1, RoomTransform.transform.TransformPoint(array[num2]));
					}
				}
			}
		}

		private void CollateLists()
		{
			NorthIslandPieces = new List<IslandPiece>();
			EastIslandPieces = new List<IslandPiece>();
			SouthIslandPieces = new List<IslandPiece>();
			WestIslandPieces = new List<IslandPiece>();
			foreach (IslandPiece islandPiece in IslandPieces)
			{
				if (islandPiece.GetConnectorsDirection(IslandConnector.Direction.North, false).Count > 0)
				{
					NorthIslandPieces.Add(islandPiece);
				}
				if (islandPiece.GetConnectorsDirection(IslandConnector.Direction.East, false).Count > 0)
				{
					EastIslandPieces.Add(islandPiece);
				}
				if (islandPiece.GetConnectorsDirection(IslandConnector.Direction.South, false).Count > 0)
				{
					SouthIslandPieces.Add(islandPiece);
				}
				if (islandPiece.GetConnectorsDirection(IslandConnector.Direction.West, false).Count > 0)
				{
					WestIslandPieces.Add(islandPiece);
				}
			}
			NorthIslandEncounterPieces = new List<IslandPiece>();
			EastIslandEncounterPieces = new List<IslandPiece>();
			SouthIslandEncounterPieces = new List<IslandPiece>();
			WestIslandEncounterPieces = new List<IslandPiece>();
			foreach (IslandPiece startPiece in StartPieces)
			{
				if (startPiece.GetConnectorsDirection(IslandConnector.Direction.North, false).Count > 0)
				{
					NorthIslandEncounterPieces.Add(startPiece);
				}
				if (startPiece.GetConnectorsDirection(IslandConnector.Direction.East, false).Count > 0)
				{
					EastIslandEncounterPieces.Add(startPiece);
				}
				if (startPiece.GetConnectorsDirection(IslandConnector.Direction.South, false).Count > 0)
				{
					SouthIslandEncounterPieces.Add(startPiece);
				}
				if (startPiece.GetConnectorsDirection(IslandConnector.Direction.West, false).Count > 0)
				{
					WestIslandEncounterPieces.Add(startPiece);
				}
			}
			NorthIslandResourcesPieces = new List<IslandPiece>();
			EastIslandResourcesPieces = new List<IslandPiece>();
			SouthIslandResourcesPieces = new List<IslandPiece>();
			WestIslandResourcesPieces = new List<IslandPiece>();
			foreach (IslandPiece resourcePiece in ResourcePieces)
			{
				if (resourcePiece.GetConnectorsDirection(IslandConnector.Direction.North, false).Count > 0)
				{
					NorthIslandResourcesPieces.Add(resourcePiece);
				}
				if (resourcePiece.GetConnectorsDirection(IslandConnector.Direction.East, false).Count > 0)
				{
					EastIslandResourcesPieces.Add(resourcePiece);
				}
				if (resourcePiece.GetConnectorsDirection(IslandConnector.Direction.South, false).Count > 0)
				{
					SouthIslandResourcesPieces.Add(resourcePiece);
				}
				if (resourcePiece.GetConnectorsDirection(IslandConnector.Direction.West, false).Count > 0)
				{
					WestIslandResourcesPieces.Add(resourcePiece);
				}
			}
		}

		private IslandPiece GetIslandListByDirection(IslandConnector.Direction Direction)
		{
			switch (Direction)
			{
			case IslandConnector.Direction.North:
				return NorthIslandPieces[RandomSeed.Next(0, NorthIslandPieces.Count)];
			case IslandConnector.Direction.East:
				return EastIslandPieces[RandomSeed.Next(0, EastIslandPieces.Count)];
			case IslandConnector.Direction.South:
				return SouthIslandPieces[RandomSeed.Next(0, SouthIslandPieces.Count)];
			case IslandConnector.Direction.West:
				return WestIslandPieces[RandomSeed.Next(0, WestIslandPieces.Count)];
			default:
				return null;
			}
		}

		private IslandPiece GetIslandFromMultipleLists(IslandConnector.Direction Direction1, IslandConnector.Direction Direction2)
		{
			List<IslandPiece> list = new List<IslandPiece>();
			switch (Direction1)
			{
			case IslandConnector.Direction.North:
				Debug.Log("NORTH");
				switch (Direction2)
				{
				case IslandConnector.Direction.North:
					foreach (IslandPiece northIslandPiece in NorthIslandPieces)
					{
						if (NorthIslandPieces.Contains(northIslandPiece))
						{
							list.Add(northIslandPiece);
						}
					}
					break;
				case IslandConnector.Direction.East:
					foreach (IslandPiece northIslandPiece2 in NorthIslandPieces)
					{
						if (EastIslandPieces.Contains(northIslandPiece2))
						{
							list.Add(northIslandPiece2);
						}
					}
					break;
				case IslandConnector.Direction.South:
					foreach (IslandPiece northIslandPiece3 in NorthIslandPieces)
					{
						if (SouthIslandPieces.Contains(northIslandPiece3))
						{
							list.Add(northIslandPiece3);
						}
					}
					break;
				case IslandConnector.Direction.West:
					foreach (IslandPiece northIslandPiece4 in NorthIslandPieces)
					{
						if (WestIslandPieces.Contains(northIslandPiece4))
						{
							list.Add(northIslandPiece4);
						}
					}
					break;
				}
				break;
			case IslandConnector.Direction.East:
				switch (Direction2)
				{
				case IslandConnector.Direction.North:
					foreach (IslandPiece eastIslandPiece in EastIslandPieces)
					{
						if (NorthIslandPieces.Contains(eastIslandPiece))
						{
							list.Add(eastIslandPiece);
						}
					}
					break;
				case IslandConnector.Direction.East:
					foreach (IslandPiece eastIslandPiece2 in EastIslandPieces)
					{
						if (EastIslandPieces.Contains(eastIslandPiece2))
						{
							list.Add(eastIslandPiece2);
						}
					}
					break;
				case IslandConnector.Direction.South:
					foreach (IslandPiece eastIslandPiece3 in EastIslandPieces)
					{
						if (SouthIslandPieces.Contains(eastIslandPiece3))
						{
							list.Add(eastIslandPiece3);
						}
					}
					break;
				case IslandConnector.Direction.West:
					foreach (IslandPiece eastIslandPiece4 in EastIslandPieces)
					{
						if (WestIslandPieces.Contains(eastIslandPiece4))
						{
							list.Add(eastIslandPiece4);
						}
					}
					break;
				}
				break;
			case IslandConnector.Direction.South:
				Debug.Log("SOUTH");
				switch (Direction2)
				{
				case IslandConnector.Direction.North:
					foreach (IslandPiece southIslandPiece in SouthIslandPieces)
					{
						if (NorthIslandPieces.Contains(southIslandPiece))
						{
							list.Add(southIslandPiece);
						}
					}
					break;
				case IslandConnector.Direction.East:
					foreach (IslandPiece southIslandPiece2 in SouthIslandPieces)
					{
						if (EastIslandPieces.Contains(southIslandPiece2))
						{
							list.Add(southIslandPiece2);
						}
					}
					break;
				case IslandConnector.Direction.South:
					foreach (IslandPiece southIslandPiece3 in SouthIslandPieces)
					{
						if (SouthIslandPieces.Contains(southIslandPiece3))
						{
							list.Add(southIslandPiece3);
						}
					}
					break;
				case IslandConnector.Direction.West:
					foreach (IslandPiece southIslandPiece4 in SouthIslandPieces)
					{
						if (WestIslandPieces.Contains(southIslandPiece4))
						{
							list.Add(southIslandPiece4);
						}
					}
					break;
				}
				break;
			case IslandConnector.Direction.West:
				switch (Direction2)
				{
				case IslandConnector.Direction.North:
					foreach (IslandPiece westIslandPiece in WestIslandPieces)
					{
						if (NorthIslandPieces.Contains(westIslandPiece))
						{
							list.Add(westIslandPiece);
						}
					}
					break;
				case IslandConnector.Direction.East:
					foreach (IslandPiece westIslandPiece2 in WestIslandPieces)
					{
						if (EastIslandPieces.Contains(westIslandPiece2))
						{
							list.Add(westIslandPiece2);
						}
					}
					break;
				case IslandConnector.Direction.South:
					foreach (IslandPiece westIslandPiece3 in WestIslandPieces)
					{
						if (SouthIslandPieces.Contains(westIslandPiece3))
						{
							list.Add(westIslandPiece3);
						}
					}
					break;
				case IslandConnector.Direction.West:
					foreach (IslandPiece westIslandPiece4 in WestIslandPieces)
					{
						if (WestIslandPieces.Contains(westIslandPiece4))
						{
							list.Add(westIslandPiece4);
						}
					}
					break;
				}
				break;
			}
			Debug.Log("AvailableConnectors.Count " + list.Count);
			return list[RandomSeed.Next(0, list.Count)];
		}

		private IslandPiece GetRandomEncounterIsland()
		{
			List<IslandPiece> list = new List<IslandPiece>();
			foreach (IslandPiece startPiece in StartPieces)
			{
				int num = 0;
				foreach (IslandPiece.GameObjectAndProbability @object in startPiece.Encounters.ObjectList)
				{
					if (@object.AvailableOnLayer())
					{
						num++;
					}
				}
				int num2 = 0;
				foreach (IslandPiece.GameObjectAndProbability object2 in startPiece.Encounters.ObjectList)
				{
					if (BiomeGenerator.EncounterAlreadyUsed(object2.GameObjectPath) && object2.AvailableOnLayer())
					{
						num2++;
					}
				}
				if (num2 < num)
				{
					list.Add(startPiece);
				}
			}
			if (list.Count <= 0)
			{
				Debug.Log("We've used all the island's encounters - RESET AND START AGAIN!");
				if (StartPieces.Count > 1)
				{
					BiomeGenerator.UsedEncounters.Clear();
				}
				return StartPieces[RandomSeed.Next(0, StartPieces.Count)];
			}
			Debug.Log("Remaing islands with encounters: " + list.Count);
			return list[RandomSeed.Next(0, list.Count)];
		}

		private IslandPiece GetEncounterIslandListByDirection(IslandConnector.Direction Direction)
		{
			if (ResourcePieces.Count <= 0 || RandomSeed.NextDouble() < (double)EncounterWillBeEnemyOrResource)
			{
				switch (Direction)
				{
				case IslandConnector.Direction.North:
					return NorthIslandEncounterPieces[RandomSeed.Next(0, NorthIslandEncounterPieces.Count)];
				case IslandConnector.Direction.East:
					return EastIslandEncounterPieces[RandomSeed.Next(0, EastIslandEncounterPieces.Count)];
				case IslandConnector.Direction.South:
					return SouthIslandEncounterPieces[RandomSeed.Next(0, SouthIslandEncounterPieces.Count)];
				case IslandConnector.Direction.West:
					return WestIslandEncounterPieces[RandomSeed.Next(0, WestIslandEncounterPieces.Count)];
				default:
					return null;
				}
			}
			return GetResourceIslandListByDirection(Direction);
		}

		private IslandPiece GetResourceIslandListByDirection(IslandConnector.Direction Direction)
		{
			switch (Direction)
			{
			case IslandConnector.Direction.North:
				return NorthIslandResourcesPieces[RandomSeed.Next(0, NorthIslandResourcesPieces.Count)];
			case IslandConnector.Direction.East:
				return EastIslandResourcesPieces[RandomSeed.Next(0, EastIslandResourcesPieces.Count)];
			case IslandConnector.Direction.South:
				return SouthIslandResourcesPieces[RandomSeed.Next(0, SouthIslandResourcesPieces.Count)];
			case IslandConnector.Direction.West:
				return WestIslandResourcesPieces[RandomSeed.Next(0, WestIslandResourcesPieces.Count)];
			default:
				return null;
			}
		}

		private void PlaceDecorations(bool CustomLevel)
		{
			if (CustomLevel)
			{
				if (BiomeGenerator.Instance != null)
				{
					DecorationList = BiomeGenerator.Instance.BiomeDecorationSet[RandomSeed.Next(0, BiomeGenerator.Instance.BiomeDecorationSet.Count)];
				}
				else
				{
					DecorationList = DecorationSetList[RandomSeed.Next(0, DecorationSetList.Count)];
				}
			}
			else if (BiomeGenerator.Instance != null)
			{
				if (BiomeGenerator.Instance.BiomeDecorationSet != null && BiomeGenerator.Instance.BiomeDecorationSet.Count > 0)
				{
					DecorationList = BiomeGenerator.Instance.BiomeDecorationSet[Mathf.Clamp(Mathf.Min(GameManager.CurrentDungeonLayer - 1, BiomeGenerator.Instance.BiomeDecorationSet.Count), 0, int.MaxValue)];
				}
			}
			else if (Application.isEditor && !Application.isPlaying)
			{
				DecorationList = DecorationSetList[0];
			}
			else
			{
				Debug.Log(string.Concat("DecorationSetList ", DecorationSetList, "   GameManager.CurrentDungeonLayer - 1", GameManager.CurrentDungeonLayer - 1));
				DecorationList = DecorationSetList[Mathf.Min(GameManager.CurrentDungeonLayer - 1, DecorationSetList.Count - 1)];
			}
			DecorationGrid = new List<List<int>>();
			DecorationGridWidth = (int)Mathf.Max(RoomTransform.bounds.size.x, RoomTransform.bounds.size.y);
			DecorationGridHeight = DecorationGridWidth;
			for (int i = -DecorationGridHeight; i < DecorationGridHeight; i += Scale)
			{
				List<int> list = new List<int>();
				for (int j = -DecorationGridWidth; j < DecorationGridWidth; j += Scale)
				{
					if (RoomTransform.ClosestPoint(new Vector2(j, (float)i - (float)Scale * 0.5f)) != new Vector2(j, (float)i - (float)Scale * 0.5f) && RoomTransform.ClosestPoint(new Vector2(j, (float)i + (float)Scale * 0.5f)) != new Vector2(j, (float)i + (float)Scale * 0.5f))
					{
						list.Add(1);
					}
					else
					{
						list.Add(0);
					}
				}
				DecorationGrid.Add(list);
			}
			for (int k = 0; k < DecorationGrid.Count; k++)
			{
				for (int l = 0; l < DecorationGrid.Count; l++)
				{
					for (int m = -1; m <= 1; m++)
					{
						for (int n = -1; n <= 1; n++)
						{
							if (DecorationGrid[l][k] != 0 && (m != 0 || n != 0) && l + m >= 0 && k + n >= 0 && l + m < DecorationGrid.Count && k + n < DecorationGrid.Count && DecorationGrid[l + m][k + n] == 0)
							{
								DecorationGrid[l][k] = 2;
							}
						}
					}
				}
			}
			int num = 3;
			for (int num2 = 0; num2 < DecorationGrid.Count; num2++)
			{
				for (int num3 = 0; num3 < DecorationGrid.Count; num3++)
				{
					bool flag = true;
					for (int num4 = 0; num4 < num; num4++)
					{
						for (int num5 = 0; num5 < num; num5++)
						{
							if (DecorationGrid[num3][num2] != 1 || num3 + num4 >= DecorationGrid.Count || num2 + num5 >= DecorationGrid.Count || DecorationGrid[num3 + num4][num2 + num5] != 1)
							{
								flag = false;
							}
						}
					}
					if (!flag)
					{
						continue;
					}
					DecorationGrid[num3][num2] = 4;
					for (int num6 = 0; num6 < num; num6++)
					{
						for (int num7 = 0; num7 < num; num7++)
						{
							if (num6 != 0 || num7 != 0)
							{
								DecorationGrid[num3 + num6][num2 + num7] = 999;
							}
						}
					}
				}
			}
			num = 2;
			for (int num8 = 0; num8 < DecorationGrid.Count; num8++)
			{
				for (int num9 = 0; num9 < DecorationGrid.Count; num9++)
				{
					bool flag2 = true;
					for (int num10 = 0; num10 < num; num10++)
					{
						for (int num11 = 0; num11 < num; num11++)
						{
							if (DecorationGrid[num9][num8] != 1 || num9 + num10 >= DecorationGrid.Count || num8 + num11 >= DecorationGrid.Count || DecorationGrid[num9 + num10][num8 + num11] != 1)
							{
								flag2 = false;
							}
						}
					}
					if (flag2)
					{
						DecorationGrid[num9][num8] = 3;
						for (int num12 = 0; num12 < num; num12++)
						{
							for (int num13 = 0; num13 < num; num13++)
							{
								if (num12 != 0 || num13 != 0)
								{
									DecorationGrid[num9 + num12][num8 + num13] = 999;
								}
							}
						}
					}
					else if (DecorationGrid[num9][num8] == 1)
					{
						DecorationGrid[num9][num8] = 2;
					}
				}
			}
		}

		private void PlaceNoiseDecorations(float x, float y, Vector3 Position)
		{
			foreach (GeneraterDecorations.DecorationPerlinSpriteShape decorationAndProabily in DecorationList.DecorationPerlinSpriteShapePrimary.DecorationAndProabilies)
			{
				PrimarySpriteShapeNoiseDecortations(decorationAndProabily, x, y, Position + new Vector3(UnityEngine.Random.Range((float)(-Scale) * 0.25f, (float)Scale * 0.25f), UnityEngine.Random.Range((float)(-Scale) * 0.25f, (float)Scale * 0.25f)));
			}
			foreach (GeneraterDecorations.DecorationPerlinSpriteShape decorationAndProabily2 in DecorationList.DecorationPerlinSpriteShapeSecondary.DecorationAndProabilies)
			{
				SecondarySpriteShapeNoiseDecortations(decorationAndProabily2, x, y, Position + new Vector3(UnityEngine.Random.Range((float)(-Scale) * 0.25f, (float)Scale * 0.25f), UnityEngine.Random.Range((float)(-Scale) * 0.25f, (float)Scale * 0.25f)));
			}
			Noise = Mathf.PerlinNoise((x / (float)DecorationGridWidth + RoomPerlinOffset.x) * DecorationList.NoiseScale, (y / (float)DecorationGridHeight + RoomPerlinOffset.y) * DecorationList.NoiseScale);
			if (!(Noise >= DecorationList.NoiseThreshold))
			{
				return;
			}
			Position += new Vector3(UnityEngine.Random.Range((float)(-Scale) * 0.25f, (float)Scale * 0.25f), UnityEngine.Random.Range((float)(-Scale) * 0.25f, (float)Scale * 0.25f));
			if (RoomTransform.ClosestPoint(Position) == (Vector2)Position)
			{
				d = DecorationList.DecorationPerlinNoiseOnPath.GetRandomGameObject(RandomSeed.NextDouble());
				NoiseName = "PerlinOnPath";
			}
			else
			{
				d = DecorationList.DecorationPerlinNoiseOffPath.GetRandomGameObject(RandomSeed.NextDouble());
				NoiseName = "PerlinOffPath";
			}
			if (d != null)
			{
				ObjectPool.Spawn(d.ObjectPath, Position + d.GetRandomOffset(), Quaternion.identity, SceneryTransform.transform, delegate(GameObject obj)
				{
					PerlinNoiseDecoration = obj;
					PerlinNoiseDecoration.name = NoiseName;
					PerlinScale = PerlinNoiseDecoration.transform.localScale;
					PerlinScale.z *= Noise * 1.4f;
					PerlinNoiseDecoration.transform.localScale = PerlinScale;
				});
			}
		}

		private void PlacePerlinCritters(float x, float y, Vector3 Position)
		{
			Noise = Mathf.PerlinNoise(x / (float)DecorationGridWidth * DecorationList.NoiseScale, y / (float)DecorationGridHeight * DecorationList.NoiseScale);
			if (DecorationList.Critters.DecorationAndProabilies.Count <= 0 || !(Noise <= DecorationList.CritterThreshold) || !(RoomTransform.ClosestPoint(Position) == (Vector2)Position) || (DecorationList.MaxRadiusFromMiddle != -1f && !(Vector3.Distance(Vector3.zero, Position) < DecorationList.MaxRadiusFromMiddle)))
			{
				return;
			}
			d = DecorationList.Critters.GetRandomGameObject(RandomSeed.NextDouble());
			if (d.ObjectPath == "Assets/Resources_Moved/Prefabs/Units/Wild Life/Snail.prefab")
			{
				if (DataManager.GetFollowerSkinUnlocked("Snail"))
				{
					return;
				}
				int num = Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.SHELL);
				if (DataManager.Instance.ShellsGifted_0)
				{
					num++;
				}
				if (DataManager.Instance.ShellsGifted_1)
				{
					num++;
				}
				if (DataManager.Instance.ShellsGifted_2)
				{
					num++;
				}
				if (DataManager.Instance.ShellsGifted_3)
				{
					num++;
				}
				if (DataManager.Instance.ShellsGifted_4)
				{
					num++;
				}
				if (num > 4 || UnityEngine.Random.value < 0.65f)
				{
					return;
				}
			}
			ObjectPool.Spawn(d.ObjectPath, Position + d.GetRandomOffset(), Quaternion.identity, SceneryTransform.transform, delegate(GameObject obj)
			{
				obj.name = "Critter";
			});
		}

		public static GroundType GetGroundTypeFromPosition(Vector3 Position)
		{
			GenerateRoom instance = Instance;
			if (instance == null)
			{
				return GroundType.Hard;
			}
			CurrentSpriteShape = null;
			foreach (SpriteShapeController spriteShapeController in instance.SpriteShapeControllers)
			{
				if (spriteShapeController != null && spriteShapeController.transform.parent != instance.RoomTransform.transform && spriteShapeController.transform.parent != instance.SceneryTransform.transform)
				{
					Points = new List<Vector3>(spriteShapeController.spline.GetPointCount());
					for (int i = 0; i < spriteShapeController.spline.GetPointCount(); i++)
					{
						Points.Add(spriteShapeController.transform.TransformPoint(spriteShapeController.spline.GetPosition(i)));
					}
					if (Utils.PointWithinPolygon(Position, Points) && (CurrentSpriteShape == null || spriteShapeController.spriteShapeRenderer.sortingLayerID > CurrentSpriteShape.spriteShapeRenderer.sortingLayerID))
					{
						CurrentSpriteShape = spriteShapeController;
					}
				}
			}
			if (CurrentSpriteShape != null)
			{
				return SetSpriteshapeMaterial.Instance.GetGroundType(CurrentSpriteShape.spriteShape);
			}
			if (instance.RoomSpriteShape == null)
			{
				instance.RoomSpriteShape = instance.RoomTransform.GetComponentInChildren<SpriteShapeController>();
			}
			if (SetSpriteshapeMaterial.Instance == null || instance == null || instance.RoomSpriteShape == null || instance.RoomSpriteShape.spriteShape == null)
			{
				return GroundType.None;
			}
			return SetSpriteshapeMaterial.Instance.GetGroundType(instance.RoomSpriteShape.spriteShape);
		}

		private void PrimarySpriteShapeNoiseDecortations(GeneraterDecorations.DecorationPerlinSpriteShape d, float x, float y, Vector3 Position)
		{
			Noise = Mathf.PerlinNoise((x / (float)DecorationGridWidth + d.PerlinOffset) * d.PerlinScale, (y / (float)DecorationGridHeight + d.PerlinOffset) * d.PerlinScale);
			if (Noise < d.PerlinThreshold || RoomSpriteShape == null)
			{
				return;
			}
			Points = new List<Vector3>();
			for (int i = 0; i < RoomSpriteShape.spline.GetPointCount(); i++)
			{
				Points.Add(RoomSpriteShape.spline.GetPosition(i));
			}
			if (!Utils.PointWithinPolygon(Position, Points))
			{
				return;
			}
			List<Collider2D> list = new List<Collider2D>();
			foreach (IslandPiece piece in Pieces)
			{
				foreach (SpriteShapeController encounterSpriteShape in piece.EncounterSpriteShapes)
				{
					if (!(encounterSpriteShape == null) && encounterSpriteShape.spline != null)
					{
						list.Add(encounterSpriteShape.polygonCollider);
						Points = new List<Vector3>();
						for (int j = 0; j < encounterSpriteShape.spline.GetPointCount(); j++)
						{
							Points.Add(piece.transform.position + encounterSpriteShape.spline.GetPosition(j));
						}
						if (Utils.PointWithinPolygon(Position, Points))
						{
							return;
						}
					}
				}
				list.Add(piece.Collider);
			}
			Collider2D[] componentsInChildren = GetComponentsInChildren<Collider2D>();
			bool flag = true;
			Collider2D[] array = componentsInChildren;
			foreach (Collider2D collider2D in array)
			{
				if (collider2D.OverlapPoint(Position) && !list.Contains(collider2D) && (collider2D.gameObject.layer == LayerMask.NameToLayer("Obstacles") || collider2D.gameObject.layer == LayerMask.NameToLayer("ObstaclesAndPlayer") || collider2D.gameObject.layer == LayerMask.NameToLayer("Obstacles Player Ignore")))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				ObjectPool.Spawn(d.ObjectPath, Position + Vector3.back * 0.01f, Quaternion.identity, SceneryTransform.transform, delegate(GameObject obj)
				{
					obj.name = "Perlin Decoration - Primary SpriteShape";
				});
			}
		}

		private void SecondarySpriteShapeNoiseDecortations(GeneraterDecorations.DecorationPerlinSpriteShape d, float x, float y, Vector3 Position)
		{
			Noise = Mathf.PerlinNoise((x / (float)DecorationGridWidth + d.PerlinOffset) * d.PerlinScale, (y / (float)DecorationGridHeight + d.PerlinOffset) * d.PerlinScale);
			if (Noise < d.PerlinThreshold)
			{
				return;
			}
			Collider2D[] componentsInChildren = GetComponentsInChildren<Collider2D>();
			foreach (IslandPiece piece in Pieces)
			{
				foreach (SpriteShapeController encounterSpriteShape in piece.EncounterSpriteShapes)
				{
					Points = new List<Vector3>();
					for (int i = 0; i < encounterSpriteShape.spline.GetPointCount(); i++)
					{
						Points.Add(piece.transform.position + encounterSpriteShape.spline.GetPosition(i));
					}
					if (!Utils.PointWithinPolygon(Position, Points))
					{
						continue;
					}
					bool flag = true;
					Collider2D[] array = componentsInChildren;
					foreach (Collider2D collider2D in array)
					{
						if (!collider2D.OverlapPoint(Position) && collider2D != encounterSpriteShape.polygonCollider && (collider2D.gameObject.layer == LayerMask.NameToLayer("Obstacles") || collider2D.gameObject.layer == LayerMask.NameToLayer("ObstaclesAndPlayer") || collider2D.gameObject.layer == LayerMask.NameToLayer("Obstacles Player Ignore")))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						ObjectPool.Spawn(d.ObjectPath, Position + Vector3.back * 0.01f, Quaternion.identity, SceneryTransform.transform, delegate(GameObject obj)
						{
							obj.name = "Perlin Decoration - Secondary SpriteShape";
						});
					}
				}
			}
		}

		private IEnumerator SpawnDecorations(bool spawnInsideShapeDecorations)
		{
			RoomPerlinOffset = new Vector2((float)RandomSeed.NextDouble(), (float)RandomSeed.NextDouble());
			for (int y = 0; y < DecorationGrid.Count; y++)
			{
				for (int x = 0; x < DecorationGrid.Count; x++)
				{
					bool waiting = true;
					Vector3 Position = new Vector3(x * Scale - DecorationGridWidth, y * Scale - DecorationGridHeight);
					Vector3 ClosestPosition = RoomTransform.ClosestPoint(Position);
					if (!(Vector3.Distance(ClosestPosition, Position) < (float)(6 * Scale)))
					{
						continue;
					}
					float num = (float)Scale * 0.25f;
					if (spawnInsideShapeDecorations)
					{
						PlaceNoiseDecorations((float)x - num, (float)y - num, Position + new Vector3(0f - num, 0f - num));
						PlaceNoiseDecorations((float)x - num, (float)y + num, Position + new Vector3(0f - num, num));
						PlaceNoiseDecorations((float)x + num, (float)y - num, Position + new Vector3(num, 0f - num));
						PlaceNoiseDecorations((float)x + num, (float)y + num, Position + new Vector3(num, num));
						PlacePerlinCritters(x, y, Position);
					}
					GameObject g;
					if (DecorationGrid[y][x] == 2 && DecorationList.DecorationPiece.DecorationAndProabilies.Count > 0)
					{
						d = DecorationList.DecorationPiece.GetRandomGameObject(RandomSeed.NextDouble());
						if (RoomTransform.ClosestPoint((Vector2)Position + new Vector2((float)Scale * 0.5f, 0f)) == (Vector2)Position + new Vector2((float)Scale * 0.5f, 0f))
						{
							Position += Vector3.left * ((float)Scale * 0.5f);
						}
						else if (RoomTransform.ClosestPoint((Vector2)Position + new Vector2((float)(-Scale) * 0.5f, 0f)) == (Vector2)Position + new Vector2((float)(-Scale) * 0.5f, 0f))
						{
							Position += Vector3.right * ((float)Scale * 0.5f);
						}
						waiting = true;
						ObjectPool.Spawn(d.ObjectPath, Position + d.GetRandomOffset(), Quaternion.identity, SceneryTransform.transform, delegate(GameObject obj)
						{
							g = obj;
							g.name = "1x1";
							CheckLimit(Position);
							waiting = false;
						});
						while (waiting && !string.IsNullOrEmpty(d.ObjectPath))
						{
							yield return null;
						}
					}
					if (DecorationGrid[y][x] == 3 && DecorationList.DecorationPiece2x2.DecorationAndProabilies.Count > 0)
					{
						waiting = true;
						d = DecorationList.DecorationPiece2x2.GetRandomGameObject(RandomSeed.NextDouble());
						ObjectPool.Spawn(d.ObjectPath, Position + d.GetRandomOffset() + new Vector3(1f, 1f), Quaternion.identity, SceneryTransform.transform, delegate(GameObject obj)
						{
							g = obj;
							g.name = "2x2";
							CheckLimit(Position);
							waiting = false;
						});
						while (waiting && !string.IsNullOrEmpty(d.ObjectPath))
						{
							yield return null;
						}
					}
					if (DecorationGrid[y][x] != 4)
					{
						continue;
					}
					if (ClosestPosition.y > Position.y && DecorationList.DecorationPiece3x3.DecorationAndProabilies.Count > 0)
					{
						waiting = true;
						d = DecorationList.DecorationPiece3x3.GetRandomGameObject(RandomSeed.NextDouble());
						ObjectPool.Spawn(d.ObjectPath, Position + d.GetRandomOffset() + new Vector3(2f, 2f), Quaternion.identity, SceneryTransform.transform, delegate(GameObject obj)
						{
							g = obj;
							g.name = "3x3";
							CheckLimit(Position);
							waiting = false;
						});
						while (waiting && !string.IsNullOrEmpty(d.ObjectPath))
						{
							yield return null;
						}
					}
					else if (DecorationList.DecorationPiece3x3Tall.DecorationAndProabilies.Count > 0)
					{
						waiting = true;
						d = DecorationList.DecorationPiece3x3Tall.GetRandomGameObject(RandomSeed.NextDouble());
						ObjectPool.Spawn(d.ObjectPath, Position + d.GetRandomOffset() + new Vector3(2f, 2f), Quaternion.identity, SceneryTransform.transform, delegate(GameObject obj)
						{
							g = obj;
							g.name = "3x3Tall";
							CheckLimit(Position);
							waiting = false;
						});
						while (waiting && string.IsNullOrEmpty(d.ObjectPath))
						{
							yield return null;
						}
					}
				}
			}
			GeneratedDecorations = true;
		}

		private void CheckLimit(Vector3 Position)
		{
			if (Position.y > LimitNorth)
			{
				LimitNorth = Position.y;
			}
			if (Position.y < LimitSouth)
			{
				LimitSouth = Position.y;
			}
			if (Position.x > LimitEast)
			{
				LimitEast = Position.x;
			}
			if (Position.x < LimitWest)
			{
				LimitWest = Position.x;
			}
		}

		private void ReplaceDecorations()
		{
			DecorationList = DecorationSetList[UnityEngine.Random.Range(0, DecorationSetList.Count)];
			List<GameObject> list = new List<GameObject>();
			int num = SceneryTransform.transform.childCount;
			while (--num > 0)
			{
				list.Add(SceneryTransform.transform.GetChild(num).gameObject);
			}
			d = null;
			foreach (GameObject g in list)
			{
				switch (g.name)
				{
				case "1x1":
					d = DecorationList.DecorationPiece.GetRandomGameObject(UnityEngine.Random.value);
					break;
				case "2x2":
					d = DecorationList.DecorationPiece2x2.GetRandomGameObject(UnityEngine.Random.value);
					break;
				case "3x3":
					d = DecorationList.DecorationPiece3x3.GetRandomGameObject(UnityEngine.Random.value);
					break;
				case "3x3Tall":
					d = DecorationList.DecorationPiece3x3Tall.GetRandomGameObject(UnityEngine.Random.value);
					break;
				case "PerlinOnPath":
					d = DecorationList.DecorationPerlinNoiseOnPath.GetRandomGameObject(UnityEngine.Random.value);
					break;
				case "PerlinOffPath":
					d = DecorationList.DecorationPerlinNoiseOffPath.GetRandomGameObject(UnityEngine.Random.value);
					break;
				case "Critter":
					d = DecorationList.Critters.GetRandomGameObject(UnityEngine.Random.value);
					break;
				}
				if (d != null)
				{
					ObjectPool.Spawn(d.ObjectPath, g.transform.position + d.GetRandomOffset(), Quaternion.identity, SceneryTransform.transform, delegate(GameObject obj)
					{
						obj.name = g.name;
						CheckLimit(g.transform.position + d.GetRandomOffset());
					});
				}
				UnityEngine.Object.DestroyImmediate(g);
			}
			CreateBackgroundSpriteShape();
		}

		private void CreateStartPiece()
		{
			StartPiece = UnityEngine.Object.Instantiate(GetRandomEncounterIsland(), Vector3.zero, Quaternion.identity, RoomTransform.transform);
			Pieces.Add(StartPiece);
		}

		private void PlaceDoors()
		{
			foreach (RoomPath path in Paths)
			{
				if (!path.Door)
				{
					continue;
				}
				List<IslandConnector> list = new List<IslandConnector>();
				foreach (IslandPiece piece in Pieces)
				{
					foreach (IslandConnector item in piece.GetConnectorsDirection(path.Direction, false))
					{
						list.Add(item);
					}
				}
				IslandConnector islandConnector = null;
				float num = float.MaxValue;
				switch (path.Direction)
				{
				case IslandConnector.Direction.North:
					num = float.MinValue;
					foreach (IslandConnector item2 in list)
					{
						if (item2.transform.position.y > num)
						{
							num = item2.transform.position.y;
							islandConnector = item2;
						}
					}
					break;
				case IslandConnector.Direction.East:
					num = float.MinValue;
					foreach (IslandConnector item3 in list)
					{
						if (item3.transform.position.x > num)
						{
							num = item3.transform.position.x;
							islandConnector = item3;
						}
					}
					break;
				case IslandConnector.Direction.South:
					foreach (IslandConnector item4 in list)
					{
						if (item4.transform.position.y < num)
						{
							num = item4.transform.position.y;
							islandConnector = item4;
						}
					}
					break;
				case IslandConnector.Direction.West:
					foreach (IslandConnector item5 in list)
					{
						if (item5.transform.position.x < num)
						{
							num = item5.transform.position.x;
							islandConnector = item5;
						}
					}
					break;
				}
				if (islandConnector != null && islandConnector.ParentIsland.CanUseRandomDoors)
				{
					islandConnector = list[RandomSeed.Next(list.Count)];
				}
				if (islandConnector != null && GetDirectionDoor(path.Direction, path.ConnectionType) != null)
				{
					islandConnector.ParentIsland.CanSpawnEncounter = false;
					Connector = islandConnector;
					CurrentPiece = UnityEngine.Object.Instantiate(GetDirectionDoor(path.Direction, path.ConnectionType), Vector3.zero, Quaternion.identity, RoomTransform.transform);
					PositionIsland();
					Pieces.Add(CurrentPiece);
				}
				else
				{
					Debug.Log("NO PLACE TO PUT DOOR!");
				}
			}
		}

		private void GeneratePath(RoomPath Path)
		{
			CurrentPiece = StartPiece;
			List<IslandConnector> list = new List<IslandConnector>();
			foreach (IslandPiece piece in Pieces)
			{
				foreach (IslandConnector item in piece.GetConnectorsDirection(Path.Direction, false))
				{
					list.Add(item);
				}
			}
			float num = float.MaxValue;
			IslandConnector islandConnector = null;
			switch (Path.Direction)
			{
			case IslandConnector.Direction.North:
				num = float.MinValue;
				foreach (IslandConnector item2 in list)
				{
					if (item2.transform.position.y > num)
					{
						num = item2.transform.position.y;
						islandConnector = item2;
					}
				}
				break;
			case IslandConnector.Direction.East:
				num = float.MinValue;
				foreach (IslandConnector item3 in list)
				{
					if (item3.transform.position.x > num)
					{
						num = item3.transform.position.x;
						islandConnector = item3;
					}
				}
				break;
			case IslandConnector.Direction.South:
				foreach (IslandConnector item4 in list)
				{
					if (item4.transform.position.y < num)
					{
						num = item4.transform.position.y;
						islandConnector = item4;
					}
				}
				break;
			case IslandConnector.Direction.West:
				foreach (IslandConnector item5 in list)
				{
					if (item5.transform.position.x < num)
					{
						num = item5.transform.position.x;
						islandConnector = item5;
					}
				}
				break;
			}
			if (islandConnector != null)
			{
				CurrentPiece = islandConnector.GetComponentInParent<IslandPiece>();
			}
			int num2 = 0;
			bool flag = true;
			while (--num2 >= 0)
			{
				Connectors = CurrentPiece.GetConnectorsDirection(Path.Direction, true);
				if (Connectors == null || Connectors.Count <= 0)
				{
					break;
				}
				Connector = Connectors[RandomSeed.Next(0, Connectors.Count)];
				PrevPiece = CurrentPiece;
				if (num2 == 0)
				{
					if (Connector.MyDirection == Path.Direction)
					{
						if (!Path.Door)
						{
							RandomPiece = GetEncounterIslandListByDirection(GetOppositeDirection(Connector.MyDirection));
							CurrentPiece = UnityEngine.Object.Instantiate(RandomPiece, Vector3.zero, Quaternion.identity, RoomTransform.transform);
							PositionIsland();
							Pieces.Add(CurrentPiece);
						}
					}
					else
					{
						RandomPiece = GetIslandFromMultipleLists(GetOppositeDirection(Connector.MyDirection), Path.Direction);
						CurrentPiece = UnityEngine.Object.Instantiate(RandomPiece, Vector3.zero, Quaternion.identity, RoomTransform.transform);
						PositionIsland();
						Pieces.Add(CurrentPiece);
						Debug.Log("EXTENDING LEVEL");
					}
				}
				bool flag2 = Path.Encounters > 0 && !flag;
				flag = flag2;
				if (num2 > 0)
				{
					RandomPiece = (flag2 ? GetEncounterIslandListByDirection(GetOppositeDirection(Connector.MyDirection)) : GetIslandListByDirection(GetOppositeDirection(Connector.MyDirection)));
					CurrentPiece = UnityEngine.Object.Instantiate(RandomPiece, Vector3.zero, Quaternion.identity, RoomTransform.transform);
					PositionIsland();
					Pieces.Add(CurrentPiece);
				}
			}
		}

		public void TurnEnemyIntoCritter(UnitObject enemy)
		{
			Vector3 position = enemy.transform.position;
			Transform parent = enemy.transform.parent;
			if ((bool)enemy.GetComponent<SpawnDeadBodyOnDeath>())
			{
				UnityEngine.Object.Destroy(enemy.GetComponent<SpawnDeadBodyOnDeath>());
			}
			enemy.health.ImpactOnHit = false;
			enemy.health.ImpactSoundToPlay = Health.IMPACT_SFX.NONE;
			enemy.health.ImpactOnHitScale = 0f;
			enemy.health.DeathSoundToPlay = Health.DEATH_SFX.NONE;
			enemy.health.InanimateObject = true;
			enemy.health.InanimateObjectEffect = false;
			enemy.health.DealDamage(enemy.health.HP, PlayerFarming.Instance.gameObject, position, false, Health.AttackTypes.NoKnockBack, true);
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Prefabs/RelicCritter.prefab", position, Quaternion.identity, parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				SkeletonAnimation componentInChildren = obj.Result.GetComponentInChildren<SkeletonAnimation>();
				componentInChildren.AnimationState.SetAnimation(0, "appear", false);
				componentInChildren.AnimationState.AddAnimation(0, "idle", true, 0f);
				AudioManager.Instance.PlayOneShot("event:/enemy/vocals/goat/bleat", base.gameObject);
			};
			BiomeConstants.Instance.EmitSmokeExplosionVFX(position);
		}

		private void PositionIsland()
		{
			Connectors = CurrentPiece.GetConnectorsDirection(GetOppositeDirection(Connector.MyDirection), false);
			CurrentConnector = Connectors[RandomSeed.Next(0, Connectors.Count)];
			CurrentPiece.transform.position = Connector.transform.position - CurrentConnector.transform.position;
			Connector.SetActive();
			CurrentConnector.SetActive();
		}

		private IslandPiece GetDirectionDoor(IslandConnector.Direction Direction, ConnectionTypes ConnectionType)
		{
			switch (Direction)
			{
			case IslandConnector.Direction.North:
				switch (ConnectionType)
				{
				case ConnectionTypes.Boss:
					if (!GameManager.Layer2)
					{
						return NorthBossDoor;
					}
					return BiomeGenerator.Instance.GeneratorRoomPrefab.NorthBossDoor_P2;
				case ConnectionTypes.Entrance:
					return NorthEntranceDoor;
				case ConnectionTypes.Tarot:
					return NorthTarotDoor;
				case ConnectionTypes.WeaponShop:
					return NorthWeaponDoor;
				case ConnectionTypes.RelicShop:
					return NorthRelicDoor;
				default:
					return NorthIsland;
				}
			case IslandConnector.Direction.East:
				switch (ConnectionType)
				{
				case ConnectionTypes.Boss:
					if (!GameManager.Layer2)
					{
						return EastBossDoor;
					}
					return BiomeGenerator.Instance.GeneratorRoomPrefab.EastBossDoor_P2;
				case ConnectionTypes.Entrance:
					return EastEntranceDoor;
				case ConnectionTypes.Tarot:
					return EastTarotDoor;
				case ConnectionTypes.WeaponShop:
					return EastWeaponDoor;
				case ConnectionTypes.RelicShop:
					return EastRelicDoor;
				default:
					return EastIsland;
				}
			case IslandConnector.Direction.South:
				switch (ConnectionType)
				{
				case ConnectionTypes.Boss:
					if (!GameManager.Layer2)
					{
						return SouthBossDoor;
					}
					return BiomeGenerator.Instance.GeneratorRoomPrefab.SouthBossDoor_P2;
				case ConnectionTypes.Entrance:
					return SouthEntranceDoor;
				case ConnectionTypes.Tarot:
					return SouthTarotDoor;
				case ConnectionTypes.WeaponShop:
					return SouthWeaponDoor;
				case ConnectionTypes.RelicShop:
					return SouthRelicDoor;
				default:
					return SouthIsland;
				}
			case IslandConnector.Direction.West:
				switch (ConnectionType)
				{
				case ConnectionTypes.Boss:
					if (!GameManager.Layer2)
					{
						return WestBossDoor;
					}
					return BiomeGenerator.Instance.GeneratorRoomPrefab.WestBossDoor_P2;
				case ConnectionTypes.Entrance:
					return WestEntranceDoor;
				case ConnectionTypes.Tarot:
					return WestTarotDoor;
				case ConnectionTypes.WeaponShop:
					return WestWeaponDoor;
				case ConnectionTypes.RelicShop:
					return WestRelicDoor;
				default:
					return WestIsland;
				}
			default:
				return null;
			}
		}

		private IslandConnector.Direction GetOppositeDirection(IslandConnector.Direction Direction)
		{
			switch (Direction)
			{
			case IslandConnector.Direction.North:
				return IslandConnector.Direction.South;
			case IslandConnector.Direction.East:
				return IslandConnector.Direction.West;
			case IslandConnector.Direction.South:
				return IslandConnector.Direction.North;
			case IslandConnector.Direction.West:
				return IslandConnector.Direction.East;
			default:
				return IslandConnector.Direction.North;
			}
		}

		private void BtnClearPrefabs()
		{
			ClearPrefabs();
		}

		public void ClearPrefabs(bool ClearRoomTransform = true, bool ClearSceneryTransform = true)
		{
			int num = base.transform.childCount;
			while (--num >= 0)
			{
				if (base.transform.GetChild(num).name == "SceneryTransform")
				{
					SceneryTransform = base.transform.GetChild(num).gameObject;
				}
				if (base.transform.GetChild(num).name == "CustomTransform")
				{
					CustomTransform = base.transform.GetChild(num).gameObject;
				}
			}
			if (SceneryTransform == null)
			{
				SceneryTransform = new GameObject("SceneryTransform");
			}
			if (CustomTransform == null)
			{
				CustomTransform = new GameObject("CustomTransform");
			}
			SceneryTransform.transform.parent = base.transform;
			CustomTransform.transform.parent = base.transform;
			if (Application.isEditor && !Application.isPlaying)
			{
				if (ClearRoomTransform && RoomTransform != null)
				{
					while (RoomTransform.transform.childCount > 0)
					{
						UnityEngine.Object.DestroyImmediate(RoomTransform.transform.GetChild(0).gameObject);
					}
				}
				if (ClearSceneryTransform && SceneryTransform != null)
				{
					while (SceneryTransform.transform.childCount > 0)
					{
						UnityEngine.Object.DestroyImmediate(SceneryTransform.transform.GetChild(0).gameObject);
					}
				}
			}
			else
			{
				if (ClearRoomTransform)
				{
					for (num = RoomTransform.transform.childCount - 1; num >= 0; num--)
					{
						ObjectPool.Recycle(RoomTransform.transform.GetChild(num).gameObject);
					}
				}
				if (ClearSceneryTransform)
				{
					for (num = SceneryTransform.transform.childCount - 1; num >= 0; num--)
					{
						ObjectPool.Recycle(SceneryTransform.transform.GetChild(num).gameObject);
					}
				}
			}
			Physics2D.SyncTransforms();
			RoomTransform.GenerateGeometry();
		}

		public void UnlockDoors()
		{
			RoomLockController.RoomCompleted();
		}
	}
}
