using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlacementRegion : BaseMonoBehaviour
{
	public enum Direction
	{
		Left,
		Right,
		Up,
		Down,
		UpLeft,
		DownLeft,
		UpRight,
		DownRight
	}

	[Serializable]
	public class ResourcesAndCount
	{
		public StructureBrain.TYPES Resource;

		public int Count;

		public int Variant;

		public Vector2 MinMaxDistanceFromCenter = Vector2.zero;

		public float MinDistanceBetweenSameStructure;

		public List<Direction> BlockNeighbouringTiles = new List<Direction>();

		public Vector2Int RandomVariation = Vector2Int.zero;
	}

	public delegate void NewBuilding(StructureBrain.TYPES Type);

	public delegate void BuildingEvent(int structureID);

	[Serializable]
	public class TileGridTile
	{
		public Vector2Int Position;

		public Vector3 WorldPosition;

		public bool Occupied;

		public bool Obstructed;

		public bool ReservedForWaste;

		public int BlockNeighbouringTiles;

		public bool IsUpgrade;

		public int PathID = -1;

		public StructureBrain.TYPES ObjectOnTile;

		public int ObjectID = -1;

		public int OldObjectID = -1;

		public bool CanPlaceStructure
		{
			get
			{
				return !Occupied;
			}
		}

		public bool CanPlaceObstruction
		{
			get
			{
				if (!Occupied && !Obstructed && !ReservedForWaste && PathID == -1)
				{
					return BlockNeighbouringTiles <= 0;
				}
				return false;
			}
		}

		public static TileGridTile Create(PlacementRegion p, Vector2Int Position, bool Occupied, bool Obstructed)
		{
			return new TileGridTile
			{
				Position = Position,
				WorldPosition = Utils.RotatePointAroundPivot(p.transform.position + new Vector3(Position.x, Position.y), p.transform.position, new Vector3(0f, 0f, 45f)),
				Occupied = Occupied,
				Obstructed = Obstructed,
				BlockNeighbouringTiles = 0
			};
		}
	}

	public enum Mode
	{
		None,
		Building,
		Upgrading,
		Demolishing,
		Moving,
		MultiBuild
	}

	public static PlacementRegion Instance;

	public bool PlaceWeeds = true;

	public bool PlaceRubble = true;

	public List<ResourcesAndCount> ResourcesToPlace = new List<ResourcesAndCount>();

	public static NewBuilding OnNewBuilding;

	public static List<PlacementRegion> PlacementRegions = new List<PlacementRegion>();

	public GameObject PlacementGameObject;

	public GameObject PlacementSquare;

	public StructureBrain.TYPES StructureType;

	public PlacementObjectUI PlacementObjectUI;

	private PlacementObject placementObject;

	private bool isEditingBuildings;

	private Vector3 previousEditingPosition = Vector3.zero;

	private Structures_PlacementRegion _StructureBrain;

	public Dictionary<Vector2Int, TileGridTile> GridTileLookup = new Dictionary<Vector2Int, TileGridTile>();

	private float InputDelay;

	private PlacementObjectUI placementUI;

	private bool canPlaceObjectOnBuildings;

	private bool isPath;

	private List<Vector3> placingPathsPositions = new List<Vector3>();

	private int direction = 1;

	private Tween moveTween;

	private Tween shakeTween;

	private Vector3 cachedDirection;

	private Plane plane = new Plane(Vector3.forward, Vector3.zero);

	private Vector3 _PlacementPosition;

	public Structure structure;

	public BiomeLightingSettings LightingSettings;

	public OverrideLightingProperties overrideLightingProperties;

	private List<PlacementTile> Tiles;

	private List<TileGridTile> PreviewTilesList = new List<TileGridTile>();

	public PolygonCollider2D polygonCollider2D;

	private int Count;

	public int MaxTileCount = 50;

	private static Vector3Int Left = new Vector3Int(-1, 1, 0);

	private static Vector3Int Right = new Vector3Int(1, -1, 0);

	private static Vector3Int Up = new Vector3Int(1, 1, 0);

	private static Vector3Int Down = new Vector3Int(-1, -1, 0);

	private static Vector3Int UpLeft = new Vector3Int(0, 1, 0);

	private static Vector3Int DownLeft = new Vector3Int(-1, 0, 0);

	private static Vector3Int UpRight = new Vector3Int(1, 0, 0);

	private static Vector3Int DownRight = new Vector3Int(0, -1, 0);

	private float Lerp;

	private float LerpSpeed = 7f;

	private PlacementTile CurrentTile;

	private PlacementTile PreviousTile;

	public Mode CurrentMode;

	private Structure PreviousStructure;

	private Structure CurrentStructureToUpgrade;

	private Structure CurrentStructureToMove;

	private float TopSpeed = 10f;

	private float LerpToTileSpeed = 10f;

	public GameObject BuildSitePrefab;

	public GameObject BuildSiteBuildingProjectPrefab;

	public Structures_PlacementRegion structureBrain
	{
		get
		{
			if (_StructureBrain == null)
			{
				_StructureBrain = structure.Brain as Structures_PlacementRegion;
			}
			return _StructureBrain;
		}
		set
		{
			_StructureBrain = value;
		}
	}

	public StructuresData StructureInfo
	{
		get
		{
			if (!(structure != null))
			{
				return null;
			}
			return structure.Structure_Info;
		}
	}

	public List<TileGridTile> Grid
	{
		get
		{
			if (!(structure != null) || StructureInfo == null)
			{
				return new List<TileGridTile>();
			}
			return StructureInfo.Grid;
		}
	}

	public Vector3 PlacementPosition
	{
		get
		{
			return _PlacementPosition;
		}
		set
		{
			if (placementObject != null && value != _PlacementPosition)
			{
				AudioManager.Instance.PlayOneShot("event:/building/move_building_placement", placementObject.transform.position);
			}
			InputDelay = 0.2f;
			Shader.SetGlobalVector("_PlayerPosition", value);
			_PlacementPosition = value;
			CurrentTile = GetClosestTileAtWorldPosition(_PlacementPosition, 1.5f);
			Lerp = 0f;
			if (placementObject != null)
			{
				UpdateTileAvailability();
			}
		}
	}

	public static event BuildingEvent OnBuildingBeganMoving;

	public static event BuildingEvent OnBuildingPlaced;

	private void Awake()
	{
		Structure obj = structure;
		obj.OnBrainAssigned = (Action)Delegate.Combine(obj.OnBrainAssigned, new Action(OnBrainAssigned));
		Instance = this;
	}

	private void OnDestroy()
	{
		Structure obj = structure;
		obj.OnBrainAssigned = (Action)Delegate.Remove(obj.OnBrainAssigned, new Action(OnBrainAssigned));
		Instance = null;
	}

	private void OnBrainAssigned()
	{
		if (Grid.Count <= 0)
		{
			CreateFloodFill();
		}
	}

	private void OnEnable()
	{
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Combine(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(OnStructuresPlaced));
		PlacementRegions.Add(this);
	}

	private void OnDisable()
	{
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Remove(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(OnStructuresPlaced));
		PlacementRegions.Remove(this);
	}

	public void Play()
	{
		StartCoroutine(PlayRoutine());
	}

	public void PlayMove(Structure structure)
	{
		CurrentMode = Mode.Moving;
		CurrentStructureToMove = structure;
		CurrentStructureToMove.gameObject.SetActive(false);
		StructureType = structure.Brain.Data.Type;
		direction = 1;
		CurrentStructureToMove.gameObject.SetActive(false);
		PlacementGameObject = TypeAndPlacementObjects.GetByType(StructureType).PlacementObject;
		if (CurrentMode == Mode.Moving)
		{
			BuildingEvent onBuildingBeganMoving = PlacementRegion.OnBuildingBeganMoving;
			if (onBuildingBeganMoving != null)
			{
				onBuildingBeganMoving(structure.Structure_Info.ID);
			}
		}
		StartCoroutine(PlayRoutine());
	}

	private void OnStructuresPlaced()
	{
		if (!(structure == null) && structure.Brain != null)
		{
			structureBrain = structure.Brain as Structures_PlacementRegion;
			if (structureBrain != null)
			{
				structureBrain.ResourcesToPlace = new List<ResourcesAndCount>(ResourcesToPlace);
				structureBrain.PlaceWeeds = PlaceWeeds;
				structureBrain.PlaceRubble = PlaceRubble;
			}
		}
	}

	private IEnumerator PlayRoutine()
	{
		HUD_Manager.Instance.ShowEditMode(true);
		int num = 1;
		if (CurrentStructureToMove != null)
		{
			num = CurrentStructureToMove.Structure_Info.Direction;
		}
		else if (CurrentStructureToUpgrade != null)
		{
			num = CurrentStructureToMove.Structure_Info.Direction;
		}
		direction = num;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.InActive;
		placementObject = UnityEngine.Object.Instantiate(PlacementGameObject, base.transform.parent).GetComponent<PlacementObject>();
		placementObject.StructureType = StructureType;
		placementObject.transform.position = new Vector3(-7.6f, -5.61f, 0f);
		placementObject.transform.localScale = new Vector3(num, placementObject.transform.localScale.y, placementObject.transform.localScale.z);
		PreviousTile = CurrentTile;
		CurrentTile = GetClosestTileAtWorldPosition(placementObject.transform.position, 1.5f);
		if (StructureType == StructureBrain.TYPES.EDIT_BUILDINGS)
		{
			CurrentMode = Mode.Demolishing;
			isEditingBuildings = true;
		}
		if (CurrentMode != Mode.Demolishing && CurrentMode != Mode.Moving && placementUI == null && StructuresData.GetCost(StructureType).Count > 0)
		{
			placementUI = UnityEngine.Object.Instantiate(PlacementObjectUI, GameObject.FindWithTag("Canvas").transform);
		}
		isPath = StructureBrain.IsPath(StructureType);
		canPlaceObjectOnBuildings = isPath;
		GameManager.GetInstance().RemoveAllFromCamera();
		GameManager.GetInstance().AddToCamera(placementObject.gameObject);
		Time.timeScale = 0f;
		Debug.Log("TIME SCALE! " + Time.timeScale);
		GameManager.overridePlayerPosition = true;
		PlaceTiles();
		WeedManager.HideAll();
		if (isPath)
		{
			PathTileManager.Instance.ShowPathsBeingBuilt();
		}
		yield return StartCoroutine(PlaceObject());
		HUD_Manager.Instance.Show();
		Time.timeScale = 1f;
		_PlacementPosition = Vector3.zero;
		previousEditingPosition = Vector3.zero;
		ClearPrefabs();
		HUD_Manager.Instance.ShowEditMode(false);
		GameManager.GetInstance().CamFollowTarget.enabled = true;
		GameManager.GetInstance().RemoveFromCamera(placementObject.gameObject);
		LightingManager.Instance.inOverride = false;
		LightingManager.Instance.overrideSettings = null;
		LightingManager.Instance.transitionDurationMultiplier = 0.2f;
		LightingManager.Instance.lerpActive = false;
		LightingManager.Instance.UpdateLighting(true);
		GameManager.GetInstance().RemoveAllFromCamera();
		GameManager.GetInstance().AddPlayerToCamera();
		GameManager.GetInstance().CameraResetTargetZoom();
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
	}

	private bool IsNaturalObstruction(StructureBrain.TYPES type)
	{
		if (type != StructureBrain.TYPES.TREE && type != StructureBrain.TYPES.BERRY_BUSH && type != StructureBrain.TYPES.RUBBLE && type != StructureBrain.TYPES.RUBBLE_BIG && type != StructureBrain.TYPES.WATER_SMALL && type != StructureBrain.TYPES.WATER_MEDIUM)
		{
			return type == StructureBrain.TYPES.WATER_BIG;
		}
		return true;
	}

	public static PlacementRegion FindPlacementRegion(int ID)
	{
		PlacementRegion result = null;
		foreach (PlacementRegion placementRegion in PlacementRegions)
		{
			if (placementRegion.structureBrain.Data.ID == ID)
			{
				result = placementRegion;
				break;
			}
		}
		return result;
	}

	public void CreateDictionaryLookup()
	{
		foreach (TileGridTile item in Grid)
		{
			if (!GridTileLookup.ContainsKey(item.Position))
			{
				GridTileLookup.Add(item.Position, item);
			}
		}
	}

	public TileGridTile GetTileGridTile(Vector2Int Position)
	{
		TileGridTile tileGridTile = null;
		if (GridTileLookup.Count < Grid.Count)
		{
			CreateDictionaryLookup();
		}
		TileGridTile value;
		if (GridTileLookup.TryGetValue(Position, out value))
		{
			return value;
		}
		return null;
	}

	public TileGridTile GetTileGridTile(float x, float y)
	{
		TileGridTile tileGridTile = null;
		if (GridTileLookup.Count < Grid.Count)
		{
			CreateDictionaryLookup();
		}
		Vector2Int key = new Vector2Int((int)x, (int)y);
		TileGridTile value;
		if (GridTileLookup.TryGetValue(key, out value))
		{
			return value;
		}
		return null;
	}

	private void CreateFloodFill()
	{
		Count = 0;
		FloodFillCreateTiles(0, 0);
	}

	private void PlaceTiles()
	{
		Tiles = new List<PlacementTile>();
		foreach (TileGridTile item in Grid)
		{
			GameObject obj = UnityEngine.Object.Instantiate(PlacementSquare, base.transform, false);
			obj.transform.localPosition = new Vector3(item.Position.x, item.Position.y);
			PlacementTile component = obj.GetComponent<PlacementTile>();
			component.Position = new Vector3(item.Position.x, item.Position.y);
			component.GridPosition = new Vector2Int(item.Position.x, item.Position.y);
			Tiles.Add(component);
		}
	}

	private void PreviewTiles()
	{
		PreviewTilesList = new List<TileGridTile>();
		CreateFloodFill();
	}

	private void FloodFillCreateTiles(int x, int y)
	{
		if (GetTileGridTile(x, y) == null && !(polygonCollider2D.ClosestPoint(base.transform.TransformPoint(new Vector2(x, y))) != (Vector2)base.transform.TransformPoint(new Vector2(x, y))) && ++Count <= MaxTileCount)
		{
			if (Application.isEditor && !Application.isPlaying)
			{
				PreviewTilesList.Add(TileGridTile.Create(this, new Vector2Int(x, y), false, false));
			}
			else
			{
				Grid.Add(TileGridTile.Create(this, new Vector2Int(x, y), false, false));
			}
			FloodFillCreateTiles(x + 1, y);
			FloodFillCreateTiles(x - 1, y);
			FloodFillCreateTiles(x, y + 1);
			FloodFillCreateTiles(x, y - 1);
		}
	}

	private void ClearTiles()
	{
		foreach (PlacementTile tile in Tiles)
		{
			UnityEngine.Object.DestroyImmediate(tile.gameObject);
		}
		Tiles.Clear();
	}

	public PlacementTile GetTile(Vector3 Position)
	{
		foreach (PlacementTile tile in Tiles)
		{
			if (tile.Position == Position)
			{
				return tile;
			}
		}
		return null;
	}

	public PlacementTile GetClosestTileAtWorldPosition(Vector3 Position, float maxDistance = -1f)
	{
		if (Tiles == null)
		{
			return null;
		}
		float num = float.MaxValue;
		PlacementTile result = null;
		Position = base.transform.InverseTransformPoint(Position);
		foreach (PlacementTile tile in Tiles)
		{
			float num2 = Vector3.Distance(Position, tile.Position);
			if (num2 < num && (maxDistance == -1f || num2 < maxDistance))
			{
				num = num2;
				result = tile;
			}
		}
		return result;
	}

	public static Vector2Int GetVector3FromDirection(Direction Direction)
	{
		switch (Direction)
		{
		default:
			return new Vector2Int(Left.x, Left.y);
		case Direction.Right:
			return new Vector2Int(Right.x, Right.y);
		case Direction.Up:
			return new Vector2Int(Up.x, Up.y);
		case Direction.Down:
			return new Vector2Int(Down.x, Down.y);
		case Direction.UpLeft:
			return new Vector2Int(UpLeft.x, UpLeft.y);
		case Direction.DownLeft:
			return new Vector2Int(DownLeft.x, DownLeft.y);
		case Direction.UpRight:
			return new Vector2Int(UpRight.x, UpRight.y);
		case Direction.DownRight:
			return new Vector2Int(DownRight.x, DownRight.y);
		}
	}

	private bool IsValidPlacement(Vector3 Position, bool CheckOccupied, bool AllowObstructions)
	{
		bool result = true;
		int num = -1;
		while (++num < placementObject.Bounds.x)
		{
			int num2 = -1;
			while (++num2 < placementObject.Bounds.y)
			{
				Vector2Int position = new Vector2Int((int)Position.x + num, (int)Position.y + num2);
				TileGridTile tileGridTile = GetTileGridTile(position);
				if (tileGridTile == null)
				{
					result = false;
				}
				else
				{
					if (canPlaceObjectOnBuildings && !IsNaturalObstruction(tileGridTile.ObjectOnTile))
					{
						continue;
					}
					if (CheckOccupied && !tileGridTile.CanPlaceStructure)
					{
						_StructureBrain.GetOccupationAtPosition(position);
						result = false;
					}
					if (!tileGridTile.CanPlaceObstruction)
					{
						GetObstructionAtPosition(position, _StructureBrain.Data);
						if (!AllowObstructions)
						{
							result = false;
						}
					}
				}
			}
		}
		return result;
	}

	public void PlaceStructureAtGridPosition(StructureBrain.TYPES StructureType, Vector2Int Position, Vector2Int Bounds)
	{
		placementObject = null;
		TileGridTile tileGridTile = GetTileGridTile(Position.x, Position.y);
		if (tileGridTile != null && tileGridTile.CanPlaceStructure)
		{
			Build(StructureType, base.transform.TransformPoint(new Vector3(tileGridTile.Position.x, tileGridTile.Position.y)), tileGridTile.Position, Bounds, true, false);
		}
	}

	public void PlaceStructureAtWorldPosition(StructureBrain.TYPES StructureType, Vector3 Position, Vector2Int Bounds)
	{
		placementObject = null;
		Position = base.transform.InverseTransformPoint(Position);
		TileGridTile tileGridTile = GetTileGridTile(Position.x, Position.y);
		if (tileGridTile != null && tileGridTile.CanPlaceStructure)
		{
			Build(StructureType, base.transform.TransformPoint(new Vector3(tileGridTile.Position.x, tileGridTile.Position.y)), tileGridTile.Position, Bounds, true, false);
		}
	}

	public bool TryPlaceExistingStructureAtWorldPosition(Structure structure)
	{
		Vector3 vector = base.transform.InverseTransformPoint(structure.transform.position);
		vector.x = Mathf.Round(vector.x);
		vector.y = Mathf.Round(vector.y);
		TileGridTile tileGridTile = GetTileGridTile(vector.x, vector.y);
		if (tileGridTile == null)
		{
			return false;
		}
		if (!tileGridTile.CanPlaceStructure)
		{
			Debug.LogWarning("Existing structure being placed where structure cannot be placed!");
		}
		structure.Structure_Info.Bounds = StructuresData.GetInfoByType(structure.Type, structure.VariantIndex).Bounds;
		structure.Structure_Info.PlacementRegionPosition = new Vector3Int((int)base.transform.position.x, (int)base.transform.position.y, 0);
		structure.Structure_Info.GridTilePosition = tileGridTile.Position;
		structure.Brain.AddToGrid();
		return true;
	}

	public TileGridTile GetTileGridTileAtWorldPosition(Vector3 Position)
	{
		Position = base.transform.InverseTransformPoint(Position);
		foreach (TileGridTile item in Grid)
		{
			if ((float)item.Position.x == Position.x && (float)item.Position.y == Position.y)
			{
				return item;
			}
		}
		return null;
	}

	public TileGridTile GetClosestTileGridTileAtWorldPosition(Vector3 Position)
	{
		TileGridTile tileGridTile = Grid[0];
		foreach (TileGridTile item in Grid)
		{
			if (Vector3.Distance(item.WorldPosition, Position) < Vector3.Distance(tileGridTile.WorldPosition, Position))
			{
				tileGridTile = item;
			}
		}
		return tileGridTile;
	}

	private IEnumerator PlaceObject()
	{
		HUD_Manager.Instance.Hide(false);
		if (SettingsManager.Settings.Accessibility.ShowBuildModeFilter)
		{
			LightingManager.Instance.inOverride = true;
			LightingSettings.overrideLightingProperties = overrideLightingProperties;
			LightingManager.Instance.overrideSettings = LightingSettings;
			LightingManager.Instance.transitionDurationMultiplier = 0f;
			LightingManager.Instance.UpdateLighting(true, true);
		}
		StructuresData structuresData = StructuresData.GetInfoByType(StructureType, 0);
		if (CurrentMode == Mode.Moving)
		{
			Debug.Log("Moving!");
			LerpSpeed = 7f;
			PlacementPosition = CurrentStructureToMove.transform.position;
			placementObject.transform.position = CurrentTile.transform.position;
			structureBrain.ClearStructureFromGrid(CurrentStructureToMove.Brain);
			CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
			GameManager.RecalculatePaths();
		}
		else if (structuresData.IsUpgrade && StructureManager.GetAllStructuresOfType(structuresData.UpgradeFromType).Count > 0)
		{
			Debug.Log("Upgrading!");
			CurrentMode = Mode.Upgrading;
			LerpSpeed = 2f;
			CurrentStructureToUpgrade = null;
			ClearTiles();
			float num = float.MaxValue;
			Structure currentStructureToUpgrade = null;
			foreach (Structure structure2 in Structure.Structures)
			{
				if (structure2.Type == structuresData.UpgradeFromType)
				{
					float num2 = Vector3.Distance(new Vector3(Grid[0].Position.x, Grid[0].Position.y), structure2.transform.position);
					if (num2 < num)
					{
						num = num2;
						currentStructureToUpgrade = structure2;
					}
				}
			}
			Debug.Log("CurrentStructureToUpgrade " + CurrentStructureToUpgrade);
			CurrentStructureToUpgrade = currentStructureToUpgrade;
			if ((bool)CurrentStructureToUpgrade && (bool)placementObject)
			{
				placementObject.transform.position = CurrentStructureToUpgrade.transform.position;
			}
		}
		else
		{
			LerpSpeed = 7f;
			if (CurrentMode != Mode.Demolishing)
			{
				CurrentMode = (StructuresData.GetBuildOnlyOne(StructureType) ? Mode.Building : Mode.MultiBuild);
			}
			PlacementPosition = ((isEditingBuildings && previousEditingPosition != Vector3.zero) ? previousEditingPosition : placementObject.transform.position);
			placementObject.transform.position = CurrentTile.transform.position;
		}
		if ((bool)placementUI && (CurrentMode == Mode.Building || CurrentMode == Mode.MultiBuild || CurrentMode == Mode.Upgrading))
		{
			float t = 0f;
			while (true)
			{
				if (!(placementObject.GetComponentInChildren<Structure>(true) == null))
				{
					float num3;
					t = (num3 = t + Time.deltaTime);
					if (!(num3 > 2.5f))
					{
						break;
					}
				}
				yield return null;
			}
			placementUI.Play(placementObject, placementObject.GetComponentInChildren<Structure>(true));
		}
		yield return new WaitForSecondsRealtime(0.1f);
		Vector2 vector = default(Vector2);
		while (true)
		{
			float Speed = 0f;
			Lerp = 1f;
			bool Loop = true;
			bool Moving = false;
			while (Loop)
			{
				vector.x = InputManager.Gameplay.GetHorizontalAxis();
				vector.y = InputManager.Gameplay.GetVerticalAxis();
				InputDelay -= Time.unscaledDeltaTime;
				if (CurrentMode == Mode.Upgrading)
				{
					if (Lerp >= 0.5f && InputDelay < 0f && (Mathf.Abs(vector.x) >= 0.3f || Mathf.Abs(vector.y) >= 0.3f || InputManager.General.MouseInputActive))
					{
						InputDelay = 0.5f;
						Structure structure = null;
						foreach (Structure structure3 in Structure.Structures)
						{
							if (((CurrentMode == Mode.Demolishing && structure3.Structure_Info.GridTilePosition != StructuresData.NullPosition) || structure3.Type == structuresData.UpgradeFromType) && structure3 != CurrentStructureToUpgrade && structure3 != null)
							{
								float num4 = Vector3.Dot(vector.normalized, (structure3.transform.position - placementObject.transform.position).normalized);
								float num5 = ((structure == null) ? 0.25f : Vector3.Dot(vector.normalized, (structure.transform.position - placementObject.transform.position).normalized));
								float num6 = Vector3.Distance(placementObject.transform.position, structure3.transform.position);
								float num7 = ((structure == null) ? num6 : Vector3.Distance(placementObject.transform.position, structure.transform.position));
								if (num4 > num5 && (num6 < num7 || Mathf.Abs(num6 - num7) < 1f))
								{
									structure = structure3;
								}
							}
						}
						if (structure != null)
						{
							PreviousStructure = CurrentStructureToUpgrade;
							if (PreviousStructure != null)
							{
								PreviousStructure.gameObject.SetActive(true);
							}
							CurrentStructureToUpgrade = structure;
							Lerp = 0f;
						}
					}
					if (CurrentStructureToUpgrade != null)
					{
						Lerp += Time.unscaledDeltaTime * LerpSpeed;
						placementObject.transform.position = Vector3.Lerp(placementObject.transform.position, CurrentStructureToUpgrade.transform.position, Mathf.SmoothStep(0f, 1f, Lerp));
						if (Vector3.Distance(placementObject.transform.position, CurrentStructureToUpgrade.transform.position) < 1f)
						{
							CurrentStructureToUpgrade.gameObject.SetActive(false);
						}
					}
					if ((InputManager.Gameplay.GetPlaceMoveUpgradeButtonDown() || (isPath && InputManager.Gameplay.GetPlaceMoveUpgradeButtonHeld())) && CurrentStructureToUpgrade != null)
					{
						AudioManager.Instance.PlayOneShot("event:/building/place_building_spot", placementObject.transform.position);
						Loop = false;
					}
				}
				else
				{
					if (vector.magnitude <= 0f && InputManager.General.MouseInputActive && GameManager.GetInstance().CamFollowTarget.enabled)
					{
						GameManager.GetInstance().CamFollowTarget.enabled = false;
						GameManager.GetInstance().RemoveFromCamera(placementObject.gameObject);
					}
					else if (!InputManager.General.MouseInputActive && !GameManager.GetInstance().CamFollowTarget.enabled)
					{
						GameManager.GetInstance().CamFollowTarget.CurrentPosition = GameManager.GetInstance().CamFollowTarget.transform.position;
						GameManager.GetInstance().CamFollowTarget.enabled = true;
						GameManager.GetInstance().AddToCamera(placementObject.gameObject);
					}
					if (Mathf.Abs(vector.x) >= 0.3f || Mathf.Abs(vector.y) >= 0.3f || InputManager.General.MouseInputActive)
					{
						Vector3 vector2 = _003CPlaceObject_003Eg__GetDirection_007C102_0(vector.normalized);
						bool flag = false;
						if (vector.magnitude <= 0f && InputManager.General.MouseInputActive)
						{
							Vector3 mousePositionWorld = GetMousePositionWorld();
							PlacementTile closestTileAtWorldPosition = GetClosestTileAtWorldPosition(placementObject.transform.position);
							if (Vector3.Distance(base.transform.TransformPoint(closestTileAtWorldPosition.Position), mousePositionWorld) > Mathf.Max((float)placementObject.Bounds.x / 2f, 1.5f))
							{
								vector2 = _003CPlaceObject_003Eg__GetDirection_007C102_0((mousePositionWorld - placementObject.transform.position).normalized);
							}
							else
							{
								vector2 = Vector3Int.zero;
								flag = true;
							}
						}
						bool flag2 = false;
						if (InputManager.General.MouseInputActive)
						{
							Vector3 mousePositionWorld2 = GetMousePositionWorld();
							float num8 = Mathf.Abs(TownCentre.Instance.transform.position.x - mousePositionWorld2.x);
							float num9 = Mathf.Abs(TownCentre.Instance.transform.position.y - (mousePositionWorld2.y + (float)placementObject.Bounds.y / 2f));
							flag2 = num8 < 14f && num9 < 8.1f;
						}
						else if (CurrentTile != null)
						{
							float num10 = Mathf.Abs(TownCentre.Instance.transform.position.x - CurrentTile.transform.position.x);
							float num11 = Mathf.Abs(TownCentre.Instance.transform.position.y - (CurrentTile.transform.position.y + (float)placementObject.Bounds.y / 2f));
							flag2 = num10 < 14f && num11 < 8.1f;
						}
						if ((CurrentTile == null || !IsValidPlacement(CurrentTile.Position, false, true)) && PreviousTile != null && InputManager.General.MouseInputActive)
						{
							CurrentTile = PreviousTile;
						}
						if (CurrentTile != null && !(vector2 == Vector3Int.zero) && !_003CPlaceObject_003Eg__SetTile_007C102_1(CurrentTile.Position, vector2) && !_003CPlaceObject_003Eg__SetTile_007C102_1(CurrentTile.Position, vector2 + Vector3.left) && !_003CPlaceObject_003Eg__SetTile_007C102_1(CurrentTile.Position, vector2 + Vector3.right) && !_003CPlaceObject_003Eg__SetTile_007C102_1(CurrentTile.Position, vector2 + Vector3.up) && !_003CPlaceObject_003Eg__SetTile_007C102_1(CurrentTile.Position, vector2 + Vector3.down) && (!(vector.y > 0f) || !_003CPlaceObject_003Eg__SetTile_007C102_1(CurrentTile.Position, Up)) && vector.y < 0f)
						{
							_003CPlaceObject_003Eg__SetTile_007C102_1(CurrentTile.Position, Down);
						}
						if (shakeTween != null)
						{
							shakeTween.Kill();
							shakeTween = null;
						}
						if (vector.magnitude <= 0f && InputManager.General.MouseInputActive)
						{
							Vector3 mousePositionWorld3 = GetMousePositionWorld();
							Vector3 vector3 = base.transform.InverseTransformPoint(mousePositionWorld3);
							bool flag3 = IsValidPlacement(new Vector3(Mathf.RoundToInt(vector3.x), Mathf.RoundToInt(vector3.y), 0f), false, true);
							if (flag3 || flag2 || flag)
							{
								if (moveTween != null)
								{
									moveTween.Kill();
									moveTween = null;
								}
								placementObject.transform.position = mousePositionWorld3;
								if (flag3)
								{
									PreviousTile = CurrentTile;
								}
							}
							else if (CurrentTile != null && (moveTween == null || !moveTween.active))
							{
								moveTween = placementObject.transform.DOMove(CurrentTile.transform.position, TopSpeed * 2f).SetSpeedBased(true).SetEase(Ease.Linear)
									.SetUpdate(UpdateType.Late, true);
							}
							CurrentTile = GetClosestTileAtWorldPosition(mousePositionWorld3);
							Vector3 vector4 = GameManager.GetInstance().CamFollowTarget.GetComponent<Camera>().ScreenToViewportPoint(InputManager.General.GetMousePosition());
							vector4.x -= 0.5f;
							vector4.y -= 0.5f;
							bool flag4 = Vector3.Distance(CurrentTile.transform.position, GetMousePositionWorld()) < 5f;
							if (vector4.y > 0.3f && GameManager.GetInstance().CamFollowTarget.transform.position.y < -5f)
							{
								flag4 = true;
							}
							if ((flag4 || flag2) && (Mathf.Abs(vector4.x) > 0.3f || Mathf.Abs(vector4.y) > 0.3f))
							{
								cachedDirection = vector4;
								Speed = Mathf.Clamp(Speed + 1f, 0f, 20f);
							}
							else
							{
								Speed = Mathf.Clamp(Speed - 2f, 0f, 20f);
							}
							if (Speed != 0f)
							{
								GameManager.GetInstance().CamFollowTarget.transform.position += Time.unscaledDeltaTime * cachedDirection * Speed;
							}
						}
						if (CurrentTile != null && !flag2 && !InputManager.General.MouseInputActive)
						{
							if (moveTween == null || !moveTween.active)
							{
								moveTween = placementObject.transform.DOMove(CurrentTile.transform.position, TopSpeed).SetSpeedBased(true).SetEase(Ease.Linear)
									.SetUpdate(UpdateType.Late, true);
							}
						}
						else
						{
							placementObject.transform.position += TopSpeed * Time.unscaledDeltaTime * Vector3.Normalize(vector);
						}
						PlacementPosition = placementObject.transform.position;
						if (!Moving)
						{
							placementObject.SetScale(new Vector3(1.1f, 0.9f, 0.9f));
							Moving = true;
						}
					}
					else
					{
						if (Moving)
						{
							placementObject.SetScale(new Vector3(1.1f, 0.9f, 0.9f));
							Moving = false;
						}
						if (placementObject != null && CurrentTile != null)
						{
							moveTween.Kill();
							placementObject.transform.position = Vector3.Lerp(placementObject.transform.position, CurrentTile.transform.position, Time.unscaledDeltaTime * LerpToTileSpeed);
						}
					}
					if (InputManager.Gameplay.GetPlaceMoveUpgradeButtonDown() || (isPath && InputManager.Gameplay.GetPlaceMoveUpgradeButtonHeld()))
					{
						if (CurrentMode == Mode.Demolishing)
						{
							Structure hoveredStructure = GetHoveredStructure();
							if ((bool)hoveredStructure && hoveredStructure.Structure_Info.CanBeMoved)
							{
								ClearPrefabs();
								StopAllCoroutines();
								PlayMove(hoveredStructure);
								yield break;
							}
						}
						else
						{
							bool allowObstructions = CurrentMode == Mode.Building || CurrentMode == Mode.MultiBuild || CurrentMode == Mode.Moving;
							bool flag5 = true;
							if (isPath && CurrentTile != null && GetTileGridTile(CurrentTile.GridPosition) != null)
							{
								StructureBrain.TYPES tileTypeAtPosition = PathTileManager.Instance.GetTileTypeAtPosition(PlacementPosition);
								flag5 = tileTypeAtPosition == StructureBrain.TYPES.NONE || tileTypeAtPosition != StructureType;
							}
							if (flag5 && CurrentTile != null && IsValidPlacement(CurrentTile.Position, !isPath, allowObstructions))
							{
								AudioManager.Instance.PlayOneShot("event:/building/place_building_spot", placementObject.transform.position);
								Loop = false;
							}
							else if (!isPath)
							{
								Debug.Log("Cant build here");
								placementObject.gameObject.transform.DOKill();
								MonoSingleton<Indicator>.Instance.gameObject.transform.DOKill();
								shakeTween = placementObject.gameObject.transform.DOShakePosition(0.3f, new Vector3(0.5f, 0f), 20).SetUpdate(true);
								MonoSingleton<Indicator>.Instance.gameObject.transform.DOShakePosition(0.3f, new Vector3(0.5f, 0f), 20).SetUpdate(true);
								AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback", placementObject.transform.position);
							}
						}
					}
				}
				if (InputManager.Gameplay.GetHorizontalAxis() > -0.3f && InputManager.Gameplay.GetHorizontalAxis() < 0.3f && InputManager.Gameplay.GetVerticalAxis() > -0.3f && InputManager.Gameplay.GetVerticalAxis() < 0.3f)
				{
					InputDelay = 0f;
				}
				if (isPath || (GetPathAtPosition() != 0 && (placementObject == null || isEditingBuildings || isPath) && CurrentStructureToUpgrade == null && CurrentStructureToMove == null))
				{
					if (InputManager.Gameplay.GetInteract3ButtonHeld())
					{
						PathTileManager.Instance.DeleteTile(PlacementPosition);
					}
				}
				else if (InputManager.Gameplay.GetRemoveFlipButtonDown())
				{
					if ((bool)placementObject && StructureType != StructureBrain.TYPES.EDIT_BUILDINGS && StructuresData.CanBeFlipped(StructureType))
					{
						placementObject.transform.localScale = new Vector3(placementObject.transform.localScale.x * -1f, placementObject.transform.localScale.y, placementObject.transform.localScale.z);
						direction = (int)placementObject.transform.localScale.x;
					}
					else if (CurrentStructureToMove != null && StructuresData.CanBeFlipped(CurrentStructureToMove.Type))
					{
						CurrentStructureToMove.Structure_Info.Direction *= -1;
						CurrentStructureToMove.transform.localScale = new Vector3(CurrentStructureToMove.Structure_Info.Direction, CurrentStructureToMove.transform.localScale.y, CurrentStructureToMove.transform.localScale.z);
						direction = CurrentStructureToMove.Structure_Info.Direction;
					}
					else if (CurrentStructureToUpgrade != null && StructuresData.CanBeFlipped(CurrentStructureToUpgrade.Type))
					{
						CurrentStructureToUpgrade.Structure_Info.Direction *= -1;
						CurrentStructureToUpgrade.transform.localScale = new Vector3(CurrentStructureToUpgrade.Structure_Info.Direction, CurrentStructureToUpgrade.transform.localScale.y, CurrentStructureToUpgrade.transform.localScale.z);
						direction = CurrentStructureToUpgrade.Structure_Info.Direction;
					}
				}
				if (InputManager.UI.GetCancelButtonDown())
				{
					if (CurrentMode == Mode.Moving)
					{
						CurrentStructureToMove.Brain.AddToGrid();
						BuildingEvent onBuildingPlaced = PlacementRegion.OnBuildingPlaced;
						if (onBuildingPlaced != null)
						{
							onBuildingPlaced(CurrentStructureToMove.Structure_Info.ID);
						}
						SpriteRenderer[] componentsInChildren = CurrentStructureToMove.gameObject.GetComponentsInChildren<SpriteRenderer>();
						foreach (SpriteRenderer spriteRenderer in componentsInChildren)
						{
							if (spriteRenderer.gameObject.activeSelf && !spriteRenderer.CompareTag("IgnoreBuildRendering"))
							{
								spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
							}
						}
						if (isEditingBuildings)
						{
							previousEditingPosition = CurrentStructureToMove.transform.position;
							ClearPrefabs();
							StopAllCoroutines();
							CurrentMode = Mode.Demolishing;
							PlacementGameObject = TypeAndPlacementObjects.GetByType(StructureBrain.TYPES.EDIT_BUILDINGS).PlacementObject;
							StructureType = StructureBrain.TYPES.EDIT_BUILDINGS;
							Play();
							yield break;
						}
					}
					else
					{
						HUD_Manager.Instance.Show();
					}
					Time.timeScale = 1f;
					_PlacementPosition = Vector3.zero;
					previousEditingPosition = Vector3.zero;
					ClearPrefabs();
					isEditingBuildings = false;
					_PlacementPosition = Vector3.zero;
					previousEditingPosition = Vector3.zero;
					MonoSingleton<Indicator>.Instance.HasSecondaryInteraction = false;
					MonoSingleton<Indicator>.Instance.SecondaryText.text = "";
					MonoSingleton<Indicator>.Instance.HideTopInfo();
					GameManager.GetInstance().CamFollowTarget.enabled = true;
					GameManager.GetInstance().RemoveFromCamera(placementObject.gameObject);
					yield break;
				}
				if (InputManager.Gameplay.GetRemoveFlipButtonDown() || (isPath && InputManager.Gameplay.GetRemoveFlipButtonHeld()))
				{
					if (isPath)
					{
						PathTileManager.Instance.DeleteTile(PlacementPosition);
						if ((bool)placementUI && CurrentMode == Mode.MultiBuild)
						{
							placementUI.UpdateText(StructureType);
						}
					}
					else if (CurrentMode == Mode.Demolishing && GetHoveredStructure() != null && GetHoveredStructure().Brain.Data.IsDeletable)
					{
						Loop = false;
						Structure hoveredStructure2 = GetHoveredStructure();
						if (hoveredStructure2 != null)
						{
							DestroyBuilding(hoveredStructure2);
						}
					}
					else if (GetPathAtPosition() != 0 && CurrentMode != Mode.Building && CurrentMode != Mode.MultiBuild && CurrentMode != Mode.Moving)
					{
						PathTileManager.Instance.DeleteTile(PlacementPosition);
					}
				}
				yield return null;
			}
			switch (CurrentMode)
			{
			case Mode.Building:
			case Mode.MultiBuild:
			{
				bool manageCamera = CurrentMode == Mode.Building;
				Build(StructureType, CurrentTile.transform.position, CurrentTile.GridPosition, placementObject.Bounds, false, manageCamera);
				break;
			}
			case Mode.Upgrading:
				Upgrade();
				break;
			case Mode.Moving:
			{
				previousEditingPosition = CurrentTile.transform.position;
				BuildingEvent onBuildingPlaced2 = PlacementRegion.OnBuildingPlaced;
				if (onBuildingPlaced2 != null)
				{
					onBuildingPlaced2(CurrentStructureToMove.Structure_Info.ID);
				}
				MoveBuilding(CurrentTile.transform.position, CurrentTile.GridPosition);
				CurrentMode = Mode.Demolishing;
				PlacementGameObject = TypeAndPlacementObjects.GetByType(StructureBrain.TYPES.EDIT_BUILDINGS).PlacementObject;
				StructureType = StructureBrain.TYPES.EDIT_BUILDINGS;
				ClearPrefabs();
				StopAllCoroutines();
				Play();
				break;
			}
			}
			if ((CurrentMode != Mode.MultiBuild && CurrentMode != Mode.Demolishing && (CurrentMode != Mode.Upgrading || StructuresData.GetBuildOnlyOne(StructureType))) || !StructuresData.CanAfford(StructureType))
			{
				break;
			}
			if (CurrentMode == Mode.Upgrading)
			{
				GameManager.GetInstance().RemoveAllFromCamera();
				GameManager.GetInstance().AddToCamera(placementObject.gameObject);
			}
		}
		yield return new WaitForSecondsRealtime(0.15f);
	}

	private Vector3 GetMousePositionWorld()
	{
		Ray ray = GameManager.GetInstance().CamFollowTarget.GetComponent<Camera>().ScreenPointToRay(InputManager.General.GetMousePosition());
		float enter;
		plane.Raycast(ray, out enter);
		return ray.GetPoint(enter);
	}

	public void DestroyBuilding(Structure structure)
	{
		CurrentStructureToUpgrade = structure;
		DestroyBuilding();
		CurrentStructureToUpgrade = null;
		BiomeConstants.Instance.EmitDustCloudParticles(structure.Brain.Data.Position, 5, 1f, true);
	}

	private void DestroyBuilding()
	{
		for (int i = 0; i < CurrentStructureToUpgrade.Brain.Data.MultipleFollowerIDs.Count; i++)
		{
			DataManager.Instance.Followers_Imprisoned_IDs.Remove(CurrentStructureToUpgrade.Brain.Data.MultipleFollowerIDs[i]);
			DataManager.Instance.Followers_OnMissionary_IDs.Remove(CurrentStructureToUpgrade.Brain.Data.MultipleFollowerIDs[i]);
			DataManager.Instance.Followers_Elderly_IDs.Remove(CurrentStructureToUpgrade.Brain.Data.MultipleFollowerIDs[i]);
			DataManager.Instance.Followers_Transitioning_IDs.Remove(CurrentStructureToUpgrade.Brain.Data.MultipleFollowerIDs[i]);
		}
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (CurrentStructureToUpgrade.Brain.Data.FollowerID == allBrain.Info.ID || CurrentStructureToUpgrade.Brain.Data.MultipleFollowerIDs.Contains(allBrain.Info.ID))
			{
				FollowerTask currentTask = allBrain.CurrentTask;
				if (currentTask != null)
				{
					currentTask.Abort();
				}
			}
		}
		structureBrain.ClearStructureFromGrid(CurrentStructureToUpgrade.Brain);
		CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
		Structure component = CurrentStructureToUpgrade.GetComponent<Structure>();
		AudioManager.Instance.PlayOneShot("event:/building/finished_stone", CurrentStructureToUpgrade.gameObject);
		MMVibrate.Haptic(MMVibrate.HapticTypes.LightImpact);
		component.RemoveStructure();
		UnityEngine.Object.Destroy(CurrentStructureToUpgrade.gameObject);
		GameManager.RecalculatePaths();
	}

	public Structure GetHoveredStructure()
	{
		PlacementTile closestTileAtWorldPosition = GetClosestTileAtWorldPosition(PlacementPosition);
		if (closestTileAtWorldPosition == null)
		{
			return null;
		}
		TileGridTile tileGridTile = GetTileGridTile(closestTileAtWorldPosition.GridPosition.x, closestTileAtWorldPosition.GridPosition.y);
		Structure structure = null;
		foreach (Structure structure2 in Structure.Structures)
		{
			if (structure2.Type != StructureBrain.TYPES.EDIT_BUILDINGS && !IsNaturalObstruction(structure2.Type) && structure2.Type != StructureBrain.TYPES.BUILD_SITE && structure2.Type != StructureBrain.TYPES.BUILD_PLOT && structure2.Type == tileGridTile.ObjectOnTile)
			{
				if (structure2.Brain != null && tileGridTile != null && structure2.Brain.Data != null && structure2.Brain.Data.ID == tileGridTile.ObjectID)
				{
					structure = structure2;
					break;
				}
				if (structure == null || (structure2 != null && structure2.Brain != null && structure2.Brain.Data != null && tileGridTile != null && Vector3.Distance(structure2.Brain.Data.Position, tileGridTile.WorldPosition) < Vector3.Distance(structure.Brain.Data.Position, tileGridTile.WorldPosition)))
				{
					structure = structure2;
				}
			}
		}
		return structure;
	}

	public StructureBrain.TYPES GetPathAtPosition()
	{
		return PathTileManager.Instance.GetTileTypeAtPosition(PlacementPosition);
	}

	public Transform GetWeedAtPosition(Vector3 worldPosition)
	{
		Transform transform = null;
		foreach (WeedManager weedManager in WeedManager.WeedManagers)
		{
			if (transform == null || Vector3.Distance(weedManager.transform.position, worldPosition) < Vector3.Distance(transform.position, worldPosition))
			{
				transform = weedManager.transform;
			}
		}
		if (transform != null && Vector3.Distance(transform.position, worldPosition) < 1f)
		{
			return transform;
		}
		return null;
	}

	private void Upgrade()
	{
		DoUpgradeRoutine();
	}

	private void MoveBuilding(Vector3 Position, Vector2Int GridPosition)
	{
		CurrentStructureToMove.gameObject.SetActive(true);
		CurrentStructureToMove.Brain.Data.Direction = direction;
		CurrentStructureToMove.transform.localScale = new Vector3(direction, CurrentStructureToMove.transform.localScale.y, CurrentStructureToMove.transform.localScale.z);
		CurrentStructureToMove.gameObject.transform.position = Position;
		CurrentStructureToMove.Brain.Data.Position = Position;
		CurrentStructureToMove.Brain.Data.GridTilePosition = GridPosition;
		CurrentStructureToMove.gameObject.SetActive(true);
		CurrentStructureToMove.Brain.AddToGrid();
		MarkObstructionsForClearing(CurrentStructureToMove.Brain.Data.GridTilePosition, CurrentStructureToMove.Brain.Data.Bounds, CurrentStructureToMove.Brain.Data);
		SpriteRenderer[] componentsInChildren = CurrentStructureToMove.gameObject.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			if (spriteRenderer.gameObject.activeSelf && !spriteRenderer.CompareTag("IgnoreBuildRendering"))
			{
				spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
			}
		}
		foreach (Follower item in FollowerManager.ActiveLocationFollowers())
		{
			if ((item.Brain.DesiredLocation == PlayerFarming.Location && CurrentStructureToMove.Brain.Data.FollowerID == item.Brain.Info.ID && !(item.Brain.CurrentTask is FollowerTask_ChangeLocation)) || (item.Brain.CurrentTask != null && item.Brain.CurrentTask is FollowerTask_ChangeLocation && ((FollowerTask_ChangeLocation)item.Brain.CurrentTask).TargetLocation == PlayerFarming.Location))
			{
				FollowerTask currentTask = item.Brain.CurrentTask;
				if (currentTask != null)
				{
					currentTask.Abort();
				}
			}
		}
		StructureManager.StructureChanged onStructureMoved = StructureManager.OnStructureMoved;
		if (onStructureMoved != null)
		{
			onStructureMoved(CurrentStructureToMove.Brain.Data);
		}
		CurrentStructureToMove = null;
	}

	public void MarkObstructionsForClearing(Vector2Int GridPosition, Vector2Int Bounds, StructuresData data)
	{
		int num = -2;
		while (++num < Bounds.x + 1)
		{
			int num2 = -2;
			while (++num2 < Bounds.y + 1)
			{
				Vector2Int position = new Vector2Int(GridPosition.x + num, GridPosition.y + num2);
				TileGridTile tileGridTile = GetTileGridTile(position);
				if (tileGridTile != null && Instance != null)
				{
					Transform weedAtPosition = Instance.GetWeedAtPosition(tileGridTile.WorldPosition);
					if (weedAtPosition != null)
					{
						UnityEngine.Object.Destroy(weedAtPosition.gameObject);
					}
				}
			}
		}
	}

	public StructuresData GetObstructionAtPosition(Vector2Int Position, StructuresData data)
	{
		StructuresData result = null;
		foreach (StructuresData item in StructureManager.StructuresDataAtLocation(data.Location))
		{
			if (Position.x >= item.GridTilePosition.x && Position.x < item.GridTilePosition.x + item.Bounds.x && Position.y >= item.GridTilePosition.y && Position.y < item.GridTilePosition.y + item.Bounds.y && item.IsObstruction)
			{
				result = item;
			}
		}
		return result;
	}

	private void DoUpgradeRoutine()
	{
		StructureBrain.TYPES type = CurrentStructureToUpgrade.Type;
		for (int i = 0; i < CurrentStructureToUpgrade.Brain.Data.MultipleFollowerIDs.Count; i++)
		{
			DataManager.Instance.Followers_Imprisoned_IDs.Remove(CurrentStructureToUpgrade.Brain.Data.MultipleFollowerIDs[i]);
			DataManager.Instance.Followers_OnMissionary_IDs.Remove(CurrentStructureToUpgrade.Brain.Data.MultipleFollowerIDs[i]);
			DataManager.Instance.Followers_Elderly_IDs.Remove(CurrentStructureToUpgrade.Brain.Data.MultipleFollowerIDs[i]);
			DataManager.Instance.Followers_Transitioning_IDs.Remove(CurrentStructureToUpgrade.Brain.Data.MultipleFollowerIDs[i]);
		}
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (CurrentStructureToUpgrade.Brain.Data.FollowerID == allBrain.Info.ID || CurrentStructureToUpgrade.Brain.Data.MultipleFollowerIDs.Contains(allBrain.Info.ID))
			{
				FollowerTask currentTask = allBrain.CurrentTask;
				if (currentTask != null)
				{
					currentTask.Abort();
				}
			}
		}
		FollowerLocation location = CurrentStructureToUpgrade.Structure_Info.Location;
		LocationManager locationManager = LocationManager.LocationManagers[location];
		int num = -1;
		while (++num < StructuresData.GetCost(StructureType).Count)
		{
			Debug.Log(string.Concat(StructuresData.GetCost(StructureType)[num].CostItem, "  ", StructuresData.GetCost(StructureType)[num].CostValue));
			Inventory.ChangeItemQuantity((int)StructuresData.GetCost(StructureType)[num].CostItem, -StructuresData.GetCost(StructureType)[num].CostValue);
		}
		StructuresData infoByType = StructuresData.GetInfoByType(StructureType, 0);
		GameObject gameObject;
		Structure component;
		if (infoByType.IgnoreGrid)
		{
			gameObject = UnityEngine.Object.Instantiate(BuildSiteBuildingProjectPrefab, CurrentStructureToUpgrade.transform.position, Quaternion.identity, locationManager.StructureLayer);
			component = gameObject.GetComponent<Structure>();
			component.CreateStructure(StructureInfo.Location, gameObject.transform.position, placementObject.Bounds, StructureType);
			BuildSitePlotProject component2 = gameObject.GetComponent<BuildSitePlotProject>();
			component2.StructureInfo.Direction = direction;
			component2.StructureInfo.Inventory = CurrentStructureToUpgrade.Inventory;
			component2.StructureInfo.QueuedResources = CurrentStructureToUpgrade.Structure_Info.QueuedResources;
			component2.StructureInfo.ToBuildType = StructureType;
			component2.StructureInfo.FollowerID = infoByType.FollowerID;
			component2.StructureInfo.Bounds = placementObject.Bounds;
			component2.StructureInfo.PlacementRegionPosition = new Vector3Int((int)base.transform.position.x, (int)base.transform.position.y, 0);
			component2.Bounds = placementObject.Bounds;
			component2.StructureInfo.FollowerID = CurrentStructureToUpgrade.Structure_Info.FollowerID;
			component2.StructureInfo.MultipleFollowerIDs = CurrentStructureToUpgrade.Structure_Info.MultipleFollowerIDs;
			component2.StructureInfo.Inventory = CurrentStructureToUpgrade.Structure_Info.Inventory;
			component2.StructureInfo.QueuedResources = CurrentStructureToUpgrade.Structure_Info.QueuedResources;
			if (infoByType.IsUpgradeDestroyPrevious)
			{
				Debug.Log("REMOVE!");
				CurrentStructureToUpgrade.RemoveStructure();
				UnityEngine.Object.Destroy(CurrentStructureToUpgrade.gameObject);
			}
		}
		else
		{
			gameObject = UnityEngine.Object.Instantiate(BuildSitePrefab, CurrentStructureToUpgrade.transform.position, Quaternion.identity, locationManager.StructureLayer);
			component = gameObject.GetComponent<Structure>();
			component.CreateStructure(StructureInfo.Location, gameObject.transform.position, placementObject.Bounds, StructureType);
			BuildSitePlot component3 = gameObject.GetComponent<BuildSitePlot>();
			component3.StructureInfo.Direction = direction;
			component3.StructureInfo.Inventory = CurrentStructureToUpgrade.Inventory;
			component3.StructureInfo.QueuedResources = CurrentStructureToUpgrade.Structure_Info.QueuedResources;
			component3.StructureInfo.ToBuildType = StructureType;
			component3.StructureInfo.Bounds = placementObject.Bounds;
			component3.StructureInfo.FollowerID = infoByType.FollowerID;
			component3.StructureInfo.GridTilePosition = CurrentStructureToUpgrade.Brain.Data.GridTilePosition;
			component3.StructureInfo.PlacementRegionPosition = new Vector3Int((int)base.transform.position.x, (int)base.transform.position.y, 0);
			component3.Bounds = placementObject.Bounds;
			component3.StructureInfo.FollowerID = CurrentStructureToUpgrade.Structure_Info.FollowerID;
			component3.StructureInfo.MultipleFollowerIDs = CurrentStructureToUpgrade.Structure_Info.MultipleFollowerIDs;
			component3.StructureInfo.Inventory = CurrentStructureToUpgrade.Structure_Info.Inventory;
			component3.StructureInfo.QueuedResources = CurrentStructureToUpgrade.Structure_Info.QueuedResources;
			if (CurrentStructureToUpgrade.Structure_Info.IsUpgradeDestroyPrevious)
			{
				CurrentStructureToUpgrade.RemoveStructure();
				UnityEngine.Object.Destroy(CurrentStructureToUpgrade.gameObject);
			}
			MarkObstructionsForClearing(component3.StructureInfo.GridTilePosition, component3.StructureInfo.Bounds, component3.StructureBrain.Data);
			structureBrain.AddStructureToGrid(component3.StructureInfo, true);
			GameManager.GetInstance().RemoveAllFromCamera();
			GameManager.GetInstance().AddToCamera(gameObject);
			GameManager.RecalculatePaths();
			StructureManager.StructureChanged onStructureUpgraded = StructureManager.OnStructureUpgraded;
			if (onStructureUpgraded != null)
			{
				onStructureUpgraded(component.Brain.Data);
			}
		}
		GameManager.GetInstance().RemoveAllFromCamera();
		GameManager.GetInstance().AddToCamera(gameObject);
		GameManager.RecalculatePaths();
		if ((bool)placementUI)
		{
			placementUI.UpdateText(StructureType);
		}
		StructureManager.StructureChanged onStructureUpgraded2 = StructureManager.OnStructureUpgraded;
		if (onStructureUpgraded2 != null)
		{
			onStructureUpgraded2(component.Brain.Data);
		}
		if (DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.Materialistic))
		{
			switch (type)
			{
			case StructureBrain.TYPES.BED:
				CultFaithManager.AddThought(Thought.Cult_Materialistic_Trait_Hut, -1, 1f);
				break;
			case StructureBrain.TYPES.BED_2:
				CultFaithManager.AddThought(Thought.Cult_Materialistic_Trait_House, -1, 1f);
				break;
			}
		}
	}

	private void Build(StructureBrain.TYPES StructureType, Vector3 Position, Vector2Int GridPosition, Vector2Int Bounds, bool Free, bool ManageCamera)
	{
		if (!Free && !StructuresData.CanAfford(StructureType))
		{
			return;
		}
		if (!Free)
		{
			int num = -1;
			while (++num < StructuresData.GetCost(StructureType).Count)
			{
				Inventory.ChangeItemQuantity((int)StructuresData.GetCost(StructureType)[num].CostItem, -StructuresData.GetCost(StructureType)[num].CostValue);
			}
		}
		if ((bool)placementUI && CurrentMode == Mode.MultiBuild)
		{
			placementUI.UpdateText(StructureType);
		}
		LocationManager locationManager = LocationManager.LocationManagers[StructureInfo.Location];
		if (isPath)
		{
			PathTileManager.Instance.DeleteTile(Position);
			if ((bool)placementUI && CurrentMode == Mode.MultiBuild)
			{
				placementUI.UpdateText(StructureType);
			}
			PathTileManager.Instance.DisplayTile(StructureType, Position);
			if (!placingPathsPositions.Contains(Position))
			{
				placingPathsPositions.Add(Position);
			}
			if (CurrentTile != null)
			{
				MarkObstructionsForClearing(CurrentTile.GridPosition, Vector2Int.one, null);
			}
			PathTileManager.Instance.SetTile(StructureType, Position);
			return;
		}
		Structure s;
		if (StructuresData.CreateBuildSite(StructureType))
		{
			_StructureBrain.MarkObstructionsForClearing(GridPosition, Bounds);
			GameObject gameObject;
			if (!StructuresData.GetInfoByType(StructureType, 0).IsBuildingProject)
			{
				gameObject = UnityEngine.Object.Instantiate(BuildSitePrefab, Position, Quaternion.identity, locationManager.StructureLayer);
				s = gameObject.GetComponent<Structure>();
				s.CreateStructure(StructureInfo.Location, gameObject.transform.position, Bounds, StructureType);
				BuildSitePlot component = gameObject.GetComponent<BuildSitePlot>();
				component.StructureInfo.Direction = direction;
				component.StructureInfo.ToBuildType = StructureType;
				component.StructureInfo.Bounds = Bounds;
				component.StructureInfo.GridTilePosition = GridPosition;
				component.StructureInfo.PlacementRegionPosition = new Vector3Int((int)base.transform.position.x, (int)base.transform.position.y, 0);
				component.Bounds = Bounds;
				MarkObstructionsForClearing(component.StructureInfo.GridTilePosition, component.StructureInfo.Bounds, component.StructureBrain.Data);
				if (isPath)
				{
					component.StructureBrain.Build();
				}
			}
			else
			{
				gameObject = UnityEngine.Object.Instantiate(BuildSiteBuildingProjectPrefab, Position, Quaternion.identity, locationManager.StructureLayer);
				s = gameObject.GetComponent<Structure>();
				s.CreateStructure(StructureInfo.Location, gameObject.transform.position, Bounds, StructureType);
				BuildSitePlotProject component2 = gameObject.GetComponent<BuildSitePlotProject>();
				component2.StructureInfo.Direction = direction;
				component2.StructureInfo.ToBuildType = StructureType;
				component2.StructureInfo.Bounds = Bounds;
				component2.StructureInfo.GridTilePosition = GridPosition;
				component2.StructureInfo.PlacementRegionPosition = new Vector3Int((int)base.transform.position.x, (int)base.transform.position.y, 0);
				component2.Bounds = Bounds;
				component2.StructureBrain.MarkObstructionsForClearing(component2.StructureInfo.GridTilePosition, component2.StructureInfo.Bounds, true);
			}
			structureBrain.AddStructureToGrid(s.Structure_Info);
			if (ManageCamera)
			{
				GameManager.GetInstance().RemoveAllFromCamera();
				GameManager.GetInstance().AddToCamera(gameObject);
			}
			GameManager.RecalculatePaths();
			NewBuilding onNewBuilding = OnNewBuilding;
			if (onNewBuilding != null)
			{
				onNewBuilding(StructureType);
			}
			return;
		}
		StructuresData infoByType = StructuresData.GetInfoByType(StructureType, 0);
		structureBrain.AddStructureToGrid(infoByType);
		infoByType.PrefabPath = "Assets/" + infoByType.PrefabPath + ".prefab";
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(infoByType.PrefabPath, locationManager.StructureLayer);
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
		{
			obj.Result.transform.position = Position;
			WorkPlace component3 = obj.Result.GetComponent<WorkPlace>();
			if (component3 != null)
			{
				component3.SetID(obj.Result.transform.position.x + "_" + obj.Result.transform.position.y);
			}
			s = obj.Result.GetComponent<Structure>();
			if (s != null)
			{
				s.CreateStructure(StructureInfo.Location, obj.Result.transform.position, Bounds);
			}
			s.Structure_Info.Direction = direction;
			s.Structure_Info.GridTilePosition = GridPosition;
			s.Structure_Info.PlacementRegionPosition = new Vector3Int((int)base.transform.position.x, (int)base.transform.position.y, 0);
			if (ManageCamera)
			{
				GameManager.GetInstance().RemoveAllFromCamera();
				GameManager.GetInstance().AddToCamera(obj.Result);
			}
			GameManager.RecalculatePaths();
			NewBuilding onNewBuilding2 = OnNewBuilding;
			if (onNewBuilding2 != null)
			{
				onNewBuilding2(StructureType);
			}
		};
	}

	private void ClearPrefabs()
	{
		UnityEngine.Object.Destroy(placementObject.gameObject);
		if (CurrentStructureToMove != null)
		{
			UnityEngine.Object.Destroy(placementObject.gameObject);
		}
		if ((bool)placementUI)
		{
			UnityEngine.Object.Destroy(placementUI.gameObject);
		}
		foreach (PlacementTile tile in Tiles)
		{
			UnityEngine.Object.Destroy(tile.gameObject);
		}
		Tiles.Clear();
		WeedManager.ShowAll();
		GameManager.overridePlayerPosition = false;
		Interactor.CurrentInteraction = null;
		Interactor.PreviousInteraction = null;
		MonoSingleton<Indicator>.Instance.text.text = "";
		MonoSingleton<Indicator>.Instance.SecondaryText.text = "";
		MonoSingleton<Indicator>.Instance.Thirdtext.text = "";
		MonoSingleton<Indicator>.Instance.Fourthtext.text = "";
		MonoSingleton<Indicator>.Instance.HideTopInfo();
		MonoSingleton<Indicator>.Instance.Reset();
		if (isPath)
		{
			placingPathsPositions.Clear();
		}
		foreach (Structure structure in Structure.Structures)
		{
			SpriteRenderer[] componentsInChildren = structure.gameObject.GetComponentsInChildren<SpriteRenderer>(true);
			if (componentsInChildren.Length != 0)
			{
				SpriteRenderer[] array = componentsInChildren;
				foreach (SpriteRenderer spriteRenderer in array)
				{
					if (!spriteRenderer.gameObject.CompareTag("BuildingEffectRadius"))
					{
						spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
					}
				}
			}
			MeshRenderer[] componentsInChildren2 = structure.gameObject.GetComponentsInChildren<MeshRenderer>(true);
			foreach (MeshRenderer meshRenderer in componentsInChildren2)
			{
				if (meshRenderer.material.HasProperty("_Color"))
				{
					meshRenderer.material.color = new Color(meshRenderer.material.color.r, meshRenderer.material.color.g, meshRenderer.material.color.b, 1f);
				}
			}
		}
		CurrentMode = Mode.None;
		if (CurrentStructureToMove != null)
		{
			CurrentStructureToMove.gameObject.SetActive(true);
		}
		CurrentStructureToMove = null;
		if (CurrentStructureToUpgrade != null)
		{
			CurrentStructureToUpgrade.gameObject.SetActive(true);
		}
		CurrentStructureToUpgrade = null;
	}

	private void UpdateTileAvailability()
	{
		if (CurrentTile == null)
		{
			return;
		}
		foreach (PlacementTile tile3 in Tiles)
		{
			TileGridTile tileGridTile = GetTileGridTile(tile3.GridPosition.x, tile3.GridPosition.y);
			bool flag = tileGridTile.CanPlaceStructure || (canPlaceObjectOnBuildings && !IsNaturalObstruction(tileGridTile.ObjectOnTile) && tileGridTile.ObjectOnTile != StructureBrain.TYPES.BUILD_SITE);
			tile3.SetColor((flag || isPath) ? Color.white : Color.red, placementObject.transform.position);
		}
		bool flag2 = false;
		int num = -1;
		while (++num < placementObject.Bounds.x)
		{
			int num2 = -1;
			while (++num2 < placementObject.Bounds.y)
			{
				PlacementTile tile = GetTile(CurrentTile.Position + new Vector3(num, num2));
				if (tile != null)
				{
					TileGridTile tileGridTile2 = GetTileGridTile(tile.GridPosition);
					if (tileGridTile2 == null || !tileGridTile2.CanPlaceStructure)
					{
						flag2 = true;
						break;
					}
				}
				else
				{
					flag2 = true;
				}
			}
			if (flag2)
			{
				break;
			}
		}
		num = -1;
		while (++num < placementObject.Bounds.x)
		{
			int num3 = -1;
			while (++num3 < placementObject.Bounds.y)
			{
				PlacementTile tile2 = GetTile(CurrentTile.Position + new Vector3(num, num3));
				if (!(tile2 != null))
				{
					continue;
				}
				TileGridTile tileGridTile3 = GetTileGridTile(tile2.GridPosition);
				if (tileGridTile3 == null)
				{
					continue;
				}
				if (isEditingBuildings && CurrentStructureToMove == null)
				{
					tile2.SetColor(StaticColors.OrangeColor, tile2.transform.position);
					if (!(GetHoveredStructure() != null))
					{
						continue;
					}
					Vector2Int bounds = GetHoveredStructure().Brain.Data.Bounds;
					for (int i = 0; i < bounds.x; i++)
					{
						for (int j = 0; j < bounds.y; j++)
						{
							tileGridTile3 = GetTileGridTile(GetHoveredStructure().Brain.Data.GridTilePosition + new Vector2Int(i, j));
							if (tileGridTile3 != null)
							{
								tile2 = GetClosestTileAtWorldPosition(tileGridTile3.WorldPosition);
								if (tile2 != null)
								{
									tile2.SetColor(StaticColors.OrangeColor, tile2.transform.position);
								}
							}
						}
					}
				}
				else
				{
					tile2.SetColor((flag2 && !isPath) ? Color.red : Color.green, placementObject.transform.position);
				}
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (!Application.isEditor || Application.isPlaying)
		{
			return;
		}
		foreach (TileGridTile previewTiles in PreviewTilesList)
		{
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(new Vector3(previewTiles.Position.x, previewTiles.Position.y, 0f), new Vector3(0.8f, 0.8f, 0f));
		}
	}

	[CompilerGenerated]
	internal static Vector3 _003CPlaceObject_003Eg__GetDirection_007C102_0(Vector3 inputDir)
	{
		if (inputDir.x > 0f && inputDir.y < 0.5f && inputDir.y > -0.5f)
		{
			return Right;
		}
		if (inputDir.x > 0.5f && inputDir.y > 0.5f)
		{
			return UpRight;
		}
		if (inputDir.x < 0f && inputDir.y < 0.5f && inputDir.y > -0.5f)
		{
			return Left;
		}
		if (inputDir.x < -0.5f && inputDir.y > 0.5f)
		{
			return UpLeft;
		}
		if (inputDir.y < 0f && inputDir.x < 0.5f && inputDir.x > -0.5f)
		{
			return Down;
		}
		if (inputDir.y < 0f && inputDir.x > 0.5f)
		{
			return DownRight;
		}
		if (inputDir.y < 0f && inputDir.x < -0.5f)
		{
			return DownLeft;
		}
		if (inputDir.y > 0f && inputDir.x < 0.5f)
		{
			float x = inputDir.x;
			float num = -0.5f;
			return Up;
		}
		return Up;
	}

	[CompilerGenerated]
	private bool _003CPlaceObject_003Eg__SetTile_007C102_1(Vector3 position, Vector3 direction)
	{
		if (IsValidPlacement(position + direction, false, true))
		{
			PlacementTile tile = GetTile(position + direction);
			if (tile != CurrentTile)
			{
				PreviousTile = CurrentTile;
				CurrentTile = tile;
				return true;
			}
		}
		return false;
	}
}
