using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class StructureManager
{
	public delegate void StructuresPlaced();

	public delegate void StructureChanged(StructuresData structure);

	public static Dictionary<FollowerLocation, List<StructureBrain>> StructureBrains = new Dictionary<FollowerLocation, List<StructureBrain>>();

	public static StructuresPlaced OnStructuresPlaced;

	public static StructureChanged OnStructureAdded;

	public static StructureChanged OnStructureMoved;

	public static StructureChanged OnStructureUpgraded;

	public static StructureChanged OnStructureRemoved;

	public static List<StructureBrain> StructuresAtLocation(FollowerLocation location)
	{
		List<StructureBrain> value = null;
		if (location == FollowerLocation.None)
		{
			value = new List<StructureBrain>();
		}
		else if (!StructureBrains.TryGetValue(location, out value))
		{
			value = new List<StructureBrain>();
			StructureBrains[location] = value;
		}
		return value;
	}

	public static List<StructuresData> StructuresDataAtLocation(FollowerLocation location)
	{
		switch (location)
		{
		case FollowerLocation.Base:
			return DataManager.Instance.BaseStructures;
		case FollowerLocation.Hub1:
			return DataManager.Instance.HubStructures;
		case FollowerLocation.HubShore:
			return DataManager.Instance.HubShoreStructures;
		case FollowerLocation.Hub1_Main:
			return DataManager.Instance.Hub1_MainStructures;
		case FollowerLocation.Hub1_Berries:
			return DataManager.Instance.Hub1_BerriesStructures;
		case FollowerLocation.Hub1_Forest:
			return DataManager.Instance.Hub1_ForestStructures;
		case FollowerLocation.Hub1_RatauInside:
			return DataManager.Instance.Hub1_RatauInsideStructures;
		case FollowerLocation.Hub1_RatauOutside:
			return DataManager.Instance.Hub1_RatauOutsideStructures;
		case FollowerLocation.Hub1_Sozo:
			return DataManager.Instance.Hub1_SozoStructures;
		case FollowerLocation.Hub1_Swamp:
			return DataManager.Instance.Hub1_SwampStructures;
		case FollowerLocation.Dungeon_Logs1:
			return DataManager.Instance.Dungeon_Logs1Structures;
		case FollowerLocation.Dungeon_Logs2:
			return DataManager.Instance.Dungeon_Logs2Structures;
		case FollowerLocation.Dungeon_Logs3:
			return DataManager.Instance.Dungeon_Logs3Structures;
		case FollowerLocation.Dungeon_Food1:
			return DataManager.Instance.Dungeon_Food1Structures;
		case FollowerLocation.Dungeon_Food2:
			return DataManager.Instance.Dungeon_Food2Structures;
		case FollowerLocation.Dungeon_Food3:
			return DataManager.Instance.Dungeon_Food3Structures;
		case FollowerLocation.Dungeon_Stone1:
			return DataManager.Instance.Dungeon_Stone1Structures;
		case FollowerLocation.Dungeon_Stone2:
			return DataManager.Instance.Dungeon_Stone2Structures;
		case FollowerLocation.Dungeon_Stone3:
			return DataManager.Instance.Dungeon_Stone3Structures;
		default:
			return new List<StructuresData>();
		}
	}

	public static void BuildStructure(FollowerLocation location, StructuresData data, Vector3 position, Vector2Int bounds, bool animateIn = true, Action<GameObject> callback = null, Action locationChangedCallback = null, bool emitParticles = true)
	{
		data.CreateStructure(location, position, bounds);
		AddStructure(location, data, emitParticles);
		if (!DataManager.Instance.HistoryOfStructures.Contains(data.Type))
		{
			DataManager.Instance.HistoryOfStructures.Add(data.Type);
		}
		LocationManager locationManager;
		if (!LocationManager.LocationManagers.TryGetValue(location, out locationManager) || !(locationManager != null))
		{
			return;
		}
		if (!data.PrefabPath.Contains("Assets"))
		{
			data.PrefabPath = "Assets/" + data.PrefabPath + ".prefab";
		}
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(data.PrefabPath);
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
		{
			GameObject gameObject = locationManager.PlaceStructure(data, obj.Result.gameObject);
			if (location == PlayerFarming.Location)
			{
				Vector3 position2 = gameObject.transform.position + Vector3.forward * 1f;
				Vector3 position3 = gameObject.transform.position;
				gameObject.transform.position = position2;
				gameObject.transform.localScale = new Vector3(data.Direction, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
				if (animateIn)
				{
					gameObject.transform.DOMove(position3, 0.5f).SetEase(Ease.OutBack);
				}
				else
				{
					gameObject.transform.position = position3;
				}
				AudioManager.Instance.SetFollowersSing(1f);
				Action<GameObject> action = callback;
				if (action != null)
				{
					action(gameObject);
				}
			}
			else
			{
				Action action2 = locationChangedCallback;
				if (action2 != null)
				{
					action2();
				}
			}
		};
	}

	private static IEnumerator LerpStructure(Transform transform)
	{
		float Progress = 0f;
		float Duration = 1f;
		Vector3 StartPosition = transform.position - Vector3.back * 2f;
		Vector3 TargetPosition = transform.position;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			transform.position = Vector3.Lerp(StartPosition, TargetPosition, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		transform.position = TargetPosition;
	}

	public static StructureBrain AddStructure(FollowerLocation location, StructuresData data, bool emitParticles = true, bool save = true)
	{
		if (save)
		{
			StructuresDataAtLocation(location).Add(data);
		}
		ConvertFromUpgrade(data);
		StructureBrain orCreateBrain = StructureBrain.GetOrCreateBrain(data);
		StructureChanged onStructureAdded = OnStructureAdded;
		if (onStructureAdded != null)
		{
			onStructureAdded(data);
		}
		ObjectiveManager.CheckObjectives(Objectives.TYPES.PLACE_STRUCTURES);
		if (ShouldStructureEmitVFXWhenAdded(data.Type) && emitParticles && BiomeConstants.Instance != null && PlayerFarming.Location == FollowerLocation.Base)
		{
			BiomeConstants.Instance.EmitSmokeInteractionVFX(data.Position + new Vector3(0f, (float)data.Bounds.y / 2f - 0.5f, 0f), new Vector3(data.Bounds.x, data.Bounds.y, 1f));
		}
		return orCreateBrain;
	}

	public static void ConvertFromUpgrade(StructuresData structure)
	{
		Structures_PlacementRegion structures_PlacementRegion = StructureBrain.FindPlacementRegion(structure);
		if (structures_PlacementRegion != null)
		{
			int previousUpgradeID = structures_PlacementRegion.GetPreviousUpgradeID(structure);
			structure.ID = ((previousUpgradeID != -1) ? previousUpgradeID : structure.ID);
		}
	}

	public static void RemoveStructure(StructureBrain brain)
	{
		brain.Data.Destroyed = true;
		StructuresDataAtLocation(brain.Data.Location).Remove(brain.Data);
		StructuresAtLocation(brain.Data.Location).Remove(brain);
		StructureBrain.RemoveBrain(brain);
		StructureChanged onStructureRemoved = OnStructureRemoved;
		if (onStructureRemoved != null)
		{
			onStructureRemoved(brain.Data);
		}
	}

	public static void UpdateWeeds(FollowerLocation location)
	{
		List<Structures_Weeds> list = new List<Structures_Weeds>();
		List<StructureBrain> list2 = StructuresAtLocation(location);
		foreach (StructureBrain item in list2)
		{
			if (item.Data.Type == StructureBrain.TYPES.WEEDS)
			{
				list.Add(item as Structures_Weeds);
			}
		}
		List<Structures_PlacementRegion> list3 = new List<Structures_PlacementRegion>();
		foreach (StructureBrain item2 in list2)
		{
			if (item2.Data.Type == StructureBrain.TYPES.PLACEMENT_REGION)
			{
				list3.Add(item2 as Structures_PlacementRegion);
			}
		}
		if (location == FollowerLocation.Base)
		{
			bool flag = true;
			int num = 0;
			if (TimeManager.CurrentDay - DataManager.Instance.LastDayTreesAtBase < 2)
			{
				flag = false;
			}
			else
			{
				foreach (StructureBrain item3 in list2)
				{
					if (item3.Data.Type == StructureBrain.TYPES.TREE)
					{
						num++;
					}
				}
			}
			if (flag && num < 5)
			{
				DataManager.Instance.LastDayTreesAtBase = TimeManager.CurrentDay;
				PlantSaplings(location, list3);
			}
		}
		if (list.Count <= 0)
		{
			CreateWeeds(location, list3);
		}
		else if (list.Count < 120)
		{
			GrowWeeds(location, list, list3);
		}
	}

	private static void GrowWeeds(FollowerLocation location, List<Structures_Weeds> Weeds, List<Structures_PlacementRegion> PlacementRegions)
	{
		foreach (Structures_Weeds Weed in Weeds)
		{
			foreach (Structures_PlacementRegion PlacementRegion in PlacementRegions)
			{
				if (PlacementRegion.PlaceWeeds && !(new Vector3Int((int)PlacementRegion.Data.Position.x, (int)PlacementRegion.Data.Position.y, 0) != Weed.Data.PlacementRegionPosition) && !(UnityEngine.Random.value <= 0.35f))
				{
					List<PlacementRegion.TileGridTile> list = new List<PlacementRegion.TileGridTile>();
					PlacementRegion.TileGridTile tileGridTile = PlacementRegion.GetTileGridTile(Weed.Data.GridTilePosition + new Vector2Int(-1, 0));
					if (tileGridTile != null && tileGridTile.CanPlaceObstruction)
					{
						list.Add(tileGridTile);
					}
					tileGridTile = PlacementRegion.GetTileGridTile(Weed.Data.GridTilePosition + new Vector2Int(1, 0));
					if (tileGridTile != null && tileGridTile.CanPlaceObstruction)
					{
						list.Add(tileGridTile);
					}
					tileGridTile = PlacementRegion.GetTileGridTile(Weed.Data.GridTilePosition + new Vector2Int(0, 1));
					if (tileGridTile != null && tileGridTile.CanPlaceObstruction)
					{
						list.Add(tileGridTile);
					}
					tileGridTile = PlacementRegion.GetTileGridTile(Weed.Data.GridTilePosition + new Vector2Int(0, -1));
					if (tileGridTile != null && tileGridTile.CanPlaceObstruction)
					{
						list.Add(tileGridTile);
					}
					if (list.Count > 0)
					{
						UnityEngine.Random.Range(0, list.Count);
					}
				}
			}
		}
	}

	private static void CreateWeeds(FollowerLocation location, List<Structures_PlacementRegion> PlacementRegions)
	{
		foreach (Structures_PlacementRegion PlacementRegion in PlacementRegions)
		{
			int num = UnityEngine.Random.Range(3, 5);
			List<PlacementRegion.TileGridTile> list = new List<PlacementRegion.TileGridTile>();
			foreach (PlacementRegion.TileGridTile item in PlacementRegion.Data.Grid)
			{
				if (item.CanPlaceObstruction)
				{
					list.Add(item);
				}
			}
			while (num > 0 && num < list.Count)
			{
				int index = UnityEngine.Random.Range(0, list.Count);
				PlaceWeed(location, list[index], PlacementRegion, -1, 0);
				list.RemoveAt(index);
				num--;
			}
		}
	}

	public static void PlantSaplings(FollowerLocation location, List<Structures_PlacementRegion> PlacementRegions)
	{
		Debug.Log("PlantSaplings " + PlacementRegions.Count);
		foreach (Structures_PlacementRegion PlacementRegion in PlacementRegions)
		{
			List<PlacementRegion.TileGridTile> list = new List<PlacementRegion.TileGridTile>();
			foreach (PlacementRegion.TileGridTile item in PlacementRegion.Data.Grid)
			{
				if (item.CanPlaceObstruction)
				{
					list.Add(item);
				}
			}
			int num = UnityEngine.Random.Range(1, 4);
			while (num > 0 && num < list.Count)
			{
				Debug.Log("PLLANT A RANDOM SAPLING!");
				int index = UnityEngine.Random.Range(0, list.Count);
				PlaceSapling(location, list[index], PlacementRegion);
				list.RemoveAt(index);
				num--;
			}
		}
	}

	private static void PlaceWeed(FollowerLocation location, PlacementRegion.TileGridTile t, Structures_PlacementRegion p, int WeedType, int growthStageOffset)
	{
		foreach (KeyValuePair<FollowerLocation, LocationManager> locationManager in LocationManager.LocationManagers)
		{
			if (locationManager.Key != location || !(locationManager.Value != null))
			{
				continue;
			}
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Prefabs/Structures/Buildings/Weeds.prefab", (locationManager.Value.StructureLayer != null) ? locationManager.Value.StructureLayer : locationManager.Value.transform, true);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				if (!(obj.Result == null))
				{
					GameObject result = obj.Result;
					result.transform.position = t.WorldPosition;
					result.GetComponent<WeedManager>().WeedTypeChosen = WeedType;
					result.GetComponent<WeedManager>().GrowthStageOffset = growthStageOffset;
				}
			};
			break;
		}
	}

	private static void PlaceSapling(FollowerLocation location, PlacementRegion.TileGridTile t, Structures_PlacementRegion p)
	{
		StructuresData infoByType = StructuresData.GetInfoByType(StructureBrain.TYPES.TREE, 0);
		Vector3 worldPosition = t.WorldPosition;
		infoByType.DontLoadMe = false;
		infoByType.IsSapling = true;
		infoByType.GrowthStage = 1f;
		infoByType.PlacementRegionPosition = new Vector3Int((int)p.Data.Position.x, (int)p.Data.Position.y, 0);
		infoByType.GridTilePosition = t.Position;
		BuildStructure(location, infoByType, worldPosition, new Vector2Int(1, 1));
	}

	public static void PlaceRubble(FollowerLocation location, List<Structures_PlacementRegion> placementRegions = null)
	{
		List<Structures_PlacementRegion> list = new List<Structures_PlacementRegion>();
		if (placementRegions != null)
		{
			list.AddRange(placementRegions);
		}
		foreach (StructureBrain item in StructuresAtLocation(location))
		{
			if (item.Data.Type == StructureBrain.TYPES.PLACEMENT_REGION && !list.Contains(item as Structures_PlacementRegion))
			{
				list.Add(item as Structures_PlacementRegion);
			}
		}
		foreach (Structures_PlacementRegion item2 in list)
		{
			List<PlacementRegion.TileGridTile> list2 = new List<PlacementRegion.TileGridTile>();
			if (!item2.Data.WeedsAndRubblePlaced)
			{
				foreach (PlacementRegion.ResourcesAndCount item3 in item2.ResourcesToPlace)
				{
					int num = 0;
					int num2 = item3.Count + UnityEngine.Random.Range(item3.RandomVariation.x, item3.RandomVariation.y + 1);
					while (num2 > 0 && num < 999)
					{
						num++;
						int index = UnityEngine.Random.Range(0, item2.Data.Grid.Count);
						PlacementRegion.TileGridTile tileGridTile = item2.Data.Grid[index];
						if (item3.MinMaxDistanceFromCenter != Vector2.zero)
						{
							for (int i = 0; i < 32; i++)
							{
								Vector3 vector = UnityEngine.Random.insideUnitCircle;
								if (vector.y > 0f)
								{
									vector.y = Mathf.Lerp(item3.MinMaxDistanceFromCenter.x / 2f, item3.MinMaxDistanceFromCenter.y / 2f, vector.y);
								}
								else
								{
									vector.y = Mathf.Lerp(0f - item3.MinMaxDistanceFromCenter.x / 2f, 0f - item3.MinMaxDistanceFromCenter.y / 2f, Mathf.Abs(vector.y));
								}
								if (vector.x > 0f)
								{
									vector.x = Mathf.Lerp(item3.MinMaxDistanceFromCenter.x, item3.MinMaxDistanceFromCenter.y, vector.x);
								}
								else
								{
									vector.x = Mathf.Lerp(0f - item3.MinMaxDistanceFromCenter.x, 0f - item3.MinMaxDistanceFromCenter.y, Mathf.Abs(vector.x));
								}
								tileGridTile = GetClosestTileGridTileAtWorldPosition(new Vector3(0f, -12f, 0f) + vector, item2.Data.Grid);
								if (tileGridTile != null)
								{
									break;
								}
							}
						}
						bool flag = true;
						if (item3.MinDistanceBetweenSameStructure > 0f)
						{
							foreach (StructureBrain item4 in GetAllStructuresOfType(item3.Resource))
							{
								if (Vector3.Distance(item4.Data.Position, tileGridTile.WorldPosition) < item3.MinDistanceBetweenSameStructure)
								{
									flag = false;
									break;
								}
							}
						}
						if (!(tileGridTile.CanPlaceObstruction && flag))
						{
							continue;
						}
						StructuresData infoByType = StructuresData.GetInfoByType(item3.Resource, 0);
						infoByType.VariantIndex = item3.Variant;
						infoByType.CanRegrow = false;
						infoByType.DontLoadMe = false;
						Vector3 worldPosition = tileGridTile.WorldPosition;
						infoByType.Bounds = new Vector2Int(infoByType.TILE_WIDTH, infoByType.TILE_HEIGHT);
						int num3 = -1;
						while (++num3 < infoByType.Bounds.x)
						{
							int num4 = -1;
							while (++num4 < infoByType.Bounds.y)
							{
								PlacementRegion.TileGridTile tileGridTile2 = item2.GetTileGridTile(tileGridTile.Position + new Vector2Int(num3, num4));
								if (tileGridTile2 == null || !tileGridTile2.CanPlaceStructure)
								{
									flag = false;
									break;
								}
							}
						}
						if (!flag)
						{
							continue;
						}
						foreach (PlacementRegion.Direction blockNeighbouringTile in item3.BlockNeighbouringTiles)
						{
							PlacementRegion.TileGridTile tileGridTile3 = item2.GetTileGridTile(tileGridTile.Position + PlacementRegion.GetVector3FromDirection(blockNeighbouringTile));
							if (tileGridTile3 != null)
							{
								tileGridTile3.Obstructed = true;
								list2.Add(tileGridTile3);
							}
						}
						infoByType.PlacementRegionPosition = new Vector3Int((int)item2.Data.Position.x, (int)item2.Data.Position.y, 0);
						infoByType.GridTilePosition = tileGridTile.Position;
						BuildStructure(location, infoByType, worldPosition, new Vector2Int(infoByType.TILE_WIDTH, infoByType.TILE_HEIGHT), false);
						item2.AddStructureToGrid(infoByType);
						num2--;
					}
				}
			}
			foreach (PlacementRegion.TileGridTile item5 in item2.Data.Grid)
			{
				float num5 = LocationManager._Instance.Random.Next(0, 100);
				if (item5.CanPlaceObstruction && num5 <= 40f && item2.PlaceWeeds)
				{
					PlaceWeed(location, item5, item2, LocationManager._Instance.Random.Next(0, 3), LocationManager._Instance.Random.Next(0, 3));
				}
			}
			foreach (PlacementRegion.TileGridTile item6 in list2)
			{
				item6.Obstructed = false;
			}
			item2.Data.WeedsAndRubblePlaced = true;
		}
		CreateWeeds(location, list);
		DataManager.Instance.PlacedRubble = true;
	}

	public static List<StructureBrain> GetStructuresFromRole(FollowerRole role)
	{
		List<StructureBrain> list = new List<StructureBrain>();
		switch (role)
		{
		case FollowerRole.Builder:
			list.AddRange(GetAllStructuresOfType(StructureBrain.TYPES.BUILD_SITE));
			break;
		case FollowerRole.Chef:
			list.AddRange(GetAllStructuresOfType<Structures_CookingFire>(FollowerLocation.Base));
			break;
		case FollowerRole.Farmer:
			list.AddRange(GetAllStructuresOfType<Structures_FarmerStation>(FollowerLocation.Base));
			break;
		case FollowerRole.Forager:
			list.AddRange(GetAllStructuresOfType(StructureBrain.TYPES.BERRY_BUSH));
			break;
		case FollowerRole.Janitor:
			list.AddRange(GetAllStructuresOfType(StructureBrain.TYPES.JANITOR_STATION));
			break;
		case FollowerRole.Refiner:
			list.AddRange(GetAllStructuresOfType(StructureBrain.TYPES.REFINERY));
			list.AddRange(GetAllStructuresOfType(StructureBrain.TYPES.REFINERY_2));
			break;
		case FollowerRole.Lumberjack:
			list.AddRange(GetAllStructuresOfType(StructureBrain.TYPES.TREE));
			list.AddRange(GetAllStructuresOfType(StructureBrain.TYPES.LUMBERJACK_STATION));
			list.AddRange(GetAllStructuresOfType(StructureBrain.TYPES.LUMBERJACK_STATION_2));
			break;
		case FollowerRole.StoneMiner:
			list.AddRange(GetAllStructuresOfType(StructureBrain.TYPES.RUBBLE));
			list.AddRange(GetAllStructuresOfType(StructureBrain.TYPES.RUBBLE_BIG));
			list.AddRange(GetAllStructuresOfType(StructureBrain.TYPES.BLOODSTONE_MINE));
			list.AddRange(GetAllStructuresOfType(StructureBrain.TYPES.BLOODSTONE_MINE_2));
			break;
		case FollowerRole.Worshipper:
			list.AddRange(GetAllStructuresOfType(StructureBrain.TYPES.SHRINE));
			list.AddRange(GetAllStructuresOfType(StructureBrain.TYPES.SHRINE_II));
			list.AddRange(GetAllStructuresOfType(StructureBrain.TYPES.SHRINE_III));
			list.AddRange(GetAllStructuresOfType(StructureBrain.TYPES.SHRINE_IV));
			break;
		}
		return list;
	}

	public static PlacementRegion.TileGridTile GetClosestTileGridTileAtWorldPosition(Vector3 Position, List<PlacementRegion.TileGridTile> grid, float maxDistance = float.PositiveInfinity)
	{
		PlacementRegion.TileGridTile tileGridTile = null;
		foreach (PlacementRegion.TileGridTile item in grid)
		{
			if ((tileGridTile == null || Vector3.Distance(item.WorldPosition, Position) < Vector3.Distance(tileGridTile.WorldPosition, Position)) && Vector3.Distance(item.WorldPosition, Position) < maxDistance)
			{
				tileGridTile = item;
			}
		}
		return tileGridTile;
	}

	public static StructureBrain.TYPES GetStructureTypeByID(int ID)
	{
		StructureBrain value;
		if (StructureBrain.BrainsByID.TryGetValue(ID, out value))
		{
			return value.Data.Type;
		}
		return StructureBrain.TYPES.NONE;
	}

	public static T GetStructureByID<T>(int ID) where T : StructureBrain
	{
		StructureBrain value;
		if (StructureBrain.BrainsByID.TryGetValue(ID, out value) && value is T)
		{
			return (T)value;
		}
		return null;
	}

	public static bool StructureTypeExists(StructureBrain.TYPES type)
	{
		foreach (StructureBrain allBrain in StructureBrain.AllBrains)
		{
			if (allBrain.Data.Type == type)
			{
				return true;
			}
		}
		return false;
	}

	public static bool StructureTypeExists(StructureBrain.TYPES type, FollowerLocation location)
	{
		foreach (StructureBrain item in StructuresAtLocation(location))
		{
			if (item.Data.Type == type || item.Data.ToBuildType == type)
			{
				return true;
			}
		}
		return false;
	}

	public static List<StructureBrain> GetAllStructuresOfType(StructureBrain.TYPES type)
	{
		List<StructureBrain> list = new List<StructureBrain>();
		foreach (StructureBrain allBrain in StructureBrain.AllBrains)
		{
			if (allBrain.Data.Type == type)
			{
				list.Add(allBrain);
			}
		}
		return list;
	}

	public static bool IsBuilt(StructureBrain.TYPES structureType)
	{
		return GetAllStructuresOfType(structureType).Count > 0;
	}

	public static bool IsBuilding(StructureBrain.TYPES structureType)
	{
		if (!BuildSitePlot.StructureOfTypeUnderConstruction(structureType))
		{
			return BuildSitePlotProject.StructureOfTypeUnderConstruction(structureType);
		}
		return true;
	}

	public static bool IsAnyUpgradeBuiltOrBuilding(StructureBrain.TYPES type)
	{
		List<StructureBrain.TYPES> upgradePath = StructuresData.GetUpgradePath(type);
		if (upgradePath != null)
		{
			for (int i = upgradePath.IndexOf(type); i < upgradePath.Count; i++)
			{
				if (IsBuilt(upgradePath[i]) || IsBuilding(upgradePath[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static int GetWasteCount()
	{
		int num = 0;
		num += GetAllStructuresOfType(FollowerLocation.Base, StructureBrain.TYPES.POOP).Count;
		num += GetAllStructuresOfType(FollowerLocation.Base, StructureBrain.TYPES.VOMIT).Count;
		foreach (Structures_DeadWorshipper item in GetAllStructuresOfType<Structures_DeadWorshipper>(FollowerLocation.Base))
		{
			num = ((!item.Data.Rotten || item.Data.BodyWrapped) ? (num + 2) : (num + 4));
		}
		foreach (Structures_Outhouse item2 in GetAllStructuresOfType<Structures_Outhouse>(FollowerLocation.Base))
		{
			if (item2.IsFull)
			{
				num += 3;
			}
		}
		foreach (Structures_Morgue item3 in GetAllStructuresOfType<Structures_Morgue>(FollowerLocation.Base))
		{
			if (item3.IsFull)
			{
				num += 5;
			}
		}
		foreach (Structures_Meal item4 in GetAllStructuresOfType<Structures_Meal>(FollowerLocation.Base))
		{
			if (item4.Data != null && (item4.Data.Rotten || item4.Data.Burned))
			{
				num++;
			}
		}
		return num;
	}

	public static List<StructureBrain> GetAllStructuresOfType(FollowerLocation location, StructureBrain.TYPES type)
	{
		List<StructureBrain> list = new List<StructureBrain>();
		foreach (StructureBrain item in StructuresAtLocation(location))
		{
			if (item.Data.Type == type)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static int GetTotalHomesCount(bool includeBuildSites = false, bool includeUpgradeSites = false)
	{
		int num = 0;
		foreach (StructureBrain allBrain in StructureBrain.AllBrains)
		{
			if (allBrain.Data.Type == StructureBrain.TYPES.BED && !allBrain.Data.IsCollapsed)
			{
				num++;
			}
			if (allBrain.Data.Type == StructureBrain.TYPES.BED_2 && !allBrain.Data.IsCollapsed)
			{
				num++;
			}
			if (allBrain.Data.Type == StructureBrain.TYPES.BED_3 && !allBrain.Data.IsCollapsed)
			{
				num++;
			}
			if (allBrain.Data.Type == StructureBrain.TYPES.SHARED_HOUSE && !allBrain.Data.IsCollapsed)
			{
				num += 3;
			}
			if (includeBuildSites)
			{
				if ((allBrain.Data.Type == StructureBrain.TYPES.BUILD_SITE || allBrain.Data.Type == StructureBrain.TYPES.BUILDSITE_BUILDINGPROJECT) && (allBrain.Data.ToBuildType == StructureBrain.TYPES.SLEEPING_BAG || allBrain.Data.ToBuildType == StructureBrain.TYPES.BED || allBrain.Data.ToBuildType == StructureBrain.TYPES.BED_2 || allBrain.Data.ToBuildType == StructureBrain.TYPES.BED_3))
				{
					num++;
				}
				if ((allBrain.Data.Type == StructureBrain.TYPES.BUILD_SITE || allBrain.Data.Type == StructureBrain.TYPES.BUILDSITE_BUILDINGPROJECT) && allBrain.Data.ToBuildType == StructureBrain.TYPES.SHARED_HOUSE)
				{
					num += 3;
				}
			}
			else if (includeUpgradeSites)
			{
				if ((allBrain.Data.Type == StructureBrain.TYPES.BUILD_SITE || allBrain.Data.Type == StructureBrain.TYPES.BUILDSITE_BUILDINGPROJECT) && (allBrain.Data.ToBuildType == StructureBrain.TYPES.BED_2 || allBrain.Data.ToBuildType == StructureBrain.TYPES.BED_3))
				{
					num++;
				}
				if ((allBrain.Data.Type == StructureBrain.TYPES.BUILD_SITE || allBrain.Data.Type == StructureBrain.TYPES.BUILDSITE_BUILDINGPROJECT) && allBrain.Data.ToBuildType == StructureBrain.TYPES.SHARED_HOUSE)
				{
					num += 3;
				}
			}
		}
		return num;
	}

	public static List<T> GetAllStructuresOfType<T>() where T : StructureBrain
	{
		List<T> list = new List<T>();
		foreach (StructureBrain allBrain in StructureBrain.AllBrains)
		{
			if (allBrain is T)
			{
				list.Add((T)allBrain);
			}
		}
		return list;
	}

	public static List<T> GetAllStructuresOfType<T>(FollowerLocation location) where T : StructureBrain
	{
		List<T> list = new List<T>();
		foreach (StructureBrain item in StructuresAtLocation(location))
		{
			if (item is T)
			{
				list.Add((T)item);
			}
		}
		if (location == FollowerLocation.Church)
		{
			foreach (StructureBrain item2 in StructuresAtLocation(FollowerLocation.Base))
			{
				if (item2 is T)
				{
					list.Add((T)item2);
				}
			}
		}
		return list;
	}

	public static List<Structures_FarmerPlot> GetAllUnwateredPlots(FollowerLocation location)
	{
		List<Structures_FarmerPlot> list = new List<Structures_FarmerPlot>();
		foreach (Structures_FarmerPlot item in GetAllStructuresOfType<Structures_FarmerPlot>(location))
		{
			if (item.CanWater() && !item.ReservedForWatering)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static List<Structures_FarmerPlot> GetAllUnseededPlots(FollowerLocation location)
	{
		List<Structures_FarmerPlot> list = new List<Structures_FarmerPlot>();
		foreach (Structures_FarmerPlot item in GetAllStructuresOfType<Structures_FarmerPlot>(location))
		{
			if (item.CanPlantSeed() && !item.ReservedForWatering && Structures_SiloSeed.GetClosestSeeder(item.Data.Position, item.Data.Location, item.GetPrioritisedSeedType()) != null)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static List<Structures_FarmerPlot> GetAllUnfertilizedPlots(FollowerLocation location)
	{
		List<Structures_FarmerPlot> list = new List<Structures_FarmerPlot>();
		foreach (Structures_FarmerPlot item in GetAllStructuresOfType<Structures_FarmerPlot>(location))
		{
			if (item.CanFertilize() && !item.ReservedForFertilizing)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static List<Structures_BerryBush> GetAllUnpickedPlots(FollowerLocation location)
	{
		List<Structures_BerryBush> list = new List<Structures_BerryBush>();
		foreach (Structures_BerryBush item in GetAllStructuresOfType<Structures_BerryBush>(location))
		{
			if (!item.ReservedForTask && !item.BerryPicked && !item.Data.Destroyed)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static List<Structures_Weeds> GetAllAvailableWeeds(FollowerLocation location)
	{
		List<Structures_Weeds> list = new List<Structures_Weeds>();
		foreach (Structures_Weeds item in GetAllStructuresOfType<Structures_Weeds>(location))
		{
			if (!item.ReservedForTask && !item.ReservedByPlayer)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static List<Structures_BerryBush> GetAllAvailableBushes(FollowerLocation location)
	{
		List<Structures_BerryBush> list = new List<Structures_BerryBush>();
		foreach (Structures_BerryBush item in GetAllStructuresOfType<Structures_BerryBush>(location))
		{
			if (!item.ReservedForTask && !item.ReservedByPlayer && !item.BerryPicked)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static List<Structures_Rubble> GetAllAvailableRubble(FollowerLocation location)
	{
		List<Structures_Rubble> list = new List<Structures_Rubble>();
		foreach (Structures_Rubble item in GetAllStructuresOfType<Structures_Rubble>(location))
		{
			if (!item.ReservedForTask && !item.ReservedByPlayer)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static List<Structures_Tree> GetAllAvailableTrees(FollowerLocation location)
	{
		List<Structures_Tree> list = new List<Structures_Tree>();
		foreach (Structures_Tree item in GetAllStructuresOfType<Structures_Tree>(location))
		{
			if (!item.ReservedForTask && !item.ReservedByPlayer && !item.TreeChopped)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static List<Structures_Waste> GetAllAvailableWaste(FollowerLocation location)
	{
		List<Structures_Waste> list = new List<Structures_Waste>();
		foreach (Structures_Waste item in GetAllStructuresOfType<Structures_Waste>(location))
		{
			if (!item.ReservedForTask && !item.ReservedByPlayer)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static Structures_DeadWorshipper GetClosestUnburiedCorpse(FollowerLocation location, Vector3 position, int offset)
	{
		Structures_DeadWorshipper result = null;
		float num = float.MaxValue;
		SortedList<float, Structures_DeadWorshipper> sortedList = new SortedList<float, Structures_DeadWorshipper>();
		foreach (Structures_DeadWorshipper item in GetAllStructuresOfType<Structures_DeadWorshipper>(location))
		{
			if (!item.ReservedForTask)
			{
				float num2 = Vector3.Distance(position, item.Data.Position);
				if (num2 < num)
				{
					sortedList.Add(num2, item);
				}
			}
		}
		if (sortedList.Count > offset)
		{
			result = sortedList.Values[offset];
		}
		return result;
	}

	public static Dwelling.DwellingAndSlot GetFreeDwellingAndSlot(FollowerLocation location, FollowerInfo follower)
	{
		foreach (Structures_Bed item in GetAllStructuresOfType<Structures_Bed>(location))
		{
			if (item.ReservedForTask || item.Data.FollowersClaimedSlots.Contains(follower.ID) || item.Data.IsCollapsed)
			{
				continue;
			}
			bool flag = true;
			foreach (FollowerInfo follower2 in DataManager.Instance.Followers)
			{
				if ((follower2.DwellingID == item.Data.ID && item.Data.MultipleFollowerIDs.Count >= item.SlotCount) || (follower2.PreviousDwellingID != Dwelling.NO_HOME && follower2 != follower && follower2.PreviousDwellingID == item.Data.ID && follower2.DwellingID == Dwelling.NO_HOME))
				{
					flag = false;
					break;
				}
			}
			if (flag && item.Data.MultipleFollowerIDs.Count >= item.SlotCount)
			{
				for (int num = item.Data.MultipleFollowerIDs.Count - 1; num >= 0; num--)
				{
					if (FollowerInfo.GetInfoByID(item.Data.MultipleFollowerIDs[num]) != null)
					{
						flag = false;
					}
					else
					{
						item.Data.MultipleFollowerIDs.RemoveAt(num);
					}
				}
			}
			if (!flag)
			{
				continue;
			}
			int num2 = 0;
			foreach (int multipleFollowerID in item.Data.MultipleFollowerIDs)
			{
				FollowerInfo infoByID = FollowerInfo.GetInfoByID(multipleFollowerID);
				if (infoByID == null || infoByID.DwellingSlot != num2)
				{
					break;
				}
				num2++;
			}
			return new Dwelling.DwellingAndSlot(item.Data.ID, num2, item.Level);
		}
		return null;
	}

	public static PlacementRegion.TileGridTile GetCloseTile(Vector3 Position, FollowerLocation location)
	{
		List<Structures_PlacementRegion> list = new List<Structures_PlacementRegion>();
		foreach (StructureBrain item in StructuresAtLocation(location))
		{
			if (item.Data.Type == StructureBrain.TYPES.PLACEMENT_REGION)
			{
				list.Add(item as Structures_PlacementRegion);
			}
		}
		PlacementRegion.TileGridTile result = null;
		float num = float.MaxValue;
		foreach (Structures_PlacementRegion item2 in list)
		{
			foreach (PlacementRegion.TileGridTile item3 in item2.Data.Grid)
			{
				if (item3 != null && !item3.Occupied && !item3.Obstructed && !item3.ReservedForWaste)
				{
					float num2 = Vector2.Distance(Position, item3.WorldPosition);
					if (num2 < num)
					{
						result = item3;
						num = num2;
					}
				}
			}
		}
		return result;
	}

	public static PlacementRegion.TileGridTile GetBestWasteTile(FollowerLocation location)
	{
		PlacementRegion.TileGridTile tileGridTile = null;
		List<PlacementRegion.TileGridTile> list = new List<PlacementRegion.TileGridTile>();
		List<Structures_PlacementRegion> list2 = new List<Structures_PlacementRegion>();
		List<StructureBrain> list3 = StructuresAtLocation(location);
		foreach (StructureBrain item in list3)
		{
			if (item.Data.Type == StructureBrain.TYPES.PLACEMENT_REGION)
			{
				list2.Add(item as Structures_PlacementRegion);
			}
		}
		foreach (Structures_PlacementRegion item2 in list2)
		{
			int num = 50;
			while (--num >= 0)
			{
				PlacementRegion.TileGridTile tileGridTile2 = item2.Data.Grid[UnityEngine.Random.Range(0, item2.Data.Grid.Count)];
				if (tileGridTile2 != null && !tileGridTile2.Occupied && !tileGridTile2.Obstructed && !tileGridTile2.ReservedForWaste)
				{
					list.Add(tileGridTile2);
				}
			}
			if (list.Count <= 0)
			{
				num = 50;
				while (--num >= 0)
				{
					PlacementRegion.TileGridTile tileGridTile3 = item2.Data.Grid[UnityEngine.Random.Range(0, item2.Data.Grid.Count)];
					if (tileGridTile3 != null && !tileGridTile3.Occupied)
					{
						list.Add(tileGridTile3);
					}
				}
			}
			if (list.Count <= 0)
			{
				continue;
			}
			float num2 = float.MinValue;
			foreach (PlacementRegion.TileGridTile item3 in list)
			{
				float num3 = float.MaxValue;
				foreach (StructureBrain item4 in list3)
				{
					if (item4.Data.Type != StructureBrain.TYPES.PLACEMENT_REGION)
					{
						float num4 = Vector3.Distance(item4.Data.Position, item3.WorldPosition);
						if (num4 < num3)
						{
							num3 = num4;
						}
					}
				}
				if (num2 < num3)
				{
					num2 = num3;
					tileGridTile = item3;
				}
			}
			if (tileGridTile != null)
			{
				break;
			}
		}
		return tileGridTile;
	}

	private static bool ShouldStructureEmitVFXWhenAdded(StructureBrain.TYPES type)
	{
		switch (type)
		{
		case StructureBrain.TYPES.NONE:
		case StructureBrain.TYPES.TREE:
		case StructureBrain.TYPES.PLACEMENT_REGION:
		case StructureBrain.TYPES.MEAL:
		case StructureBrain.TYPES.RUBBLE:
		case StructureBrain.TYPES.WEEDS:
		case StructureBrain.TYPES.POOP:
		case StructureBrain.TYPES.MEAL_MEAT:
		case StructureBrain.TYPES.MEAL_GREAT:
		case StructureBrain.TYPES.MEAL_GRASS:
		case StructureBrain.TYPES.MEAL_GOOD_FISH:
		case StructureBrain.TYPES.MEAL_FOLLOWER_MEAT:
		case StructureBrain.TYPES.MEAL_MUSHROOMS:
		case StructureBrain.TYPES.MEAL_POOP:
		case StructureBrain.TYPES.MEAL_ROTTEN:
			return false;
		default:
			return true;
		}
	}

	public static void Reset()
	{
		StructureBrains.Clear();
		foreach (StructureBrain item in new List<StructureBrain>(StructureBrain.AllBrains))
		{
			StructureBrain.RemoveBrain(item);
		}
	}

	public static void BuildAllStructures()
	{
		foreach (Structures_BuildSite item in GetAllStructuresOfType<Structures_BuildSite>())
		{
			item.BuildProgress = StructuresData.BuildDurationGameMinutes(item.Data.ToBuildType);
		}
		foreach (Structures_BuildSiteProject item2 in GetAllStructuresOfType<Structures_BuildSiteProject>())
		{
			item2.BuildProgress = StructuresData.BuildDurationGameMinutes(item2.Data.ToBuildType);
		}
	}

	public static void BreakRandomBeds()
	{
		List<Structures_Bed> list = new List<Structures_Bed>(GetAllStructuresOfType<Structures_Bed>());
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].IsCollapsed || list[num].Data.Type == StructureBrain.TYPES.BED_3 || list[num].Data.Type == StructureBrain.TYPES.SHARED_HOUSE)
			{
				list.Remove(list[num]);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			list[i].Collapse();
		}
	}

	public static void ClearAllWaste()
	{
		List<StructureBrain> allStructuresOfType = GetAllStructuresOfType(StructureBrain.TYPES.VOMIT);
		List<StructureBrain> allStructuresOfType2 = GetAllStructuresOfType(StructureBrain.TYPES.POOP);
		for (int num = allStructuresOfType.Count - 1; num >= 0; num--)
		{
			StructureBrain vomit = allStructuresOfType[num];
			GameManager.GetInstance().StartCoroutine(_003CClearAllWaste_003Eg__Delay_007C54_0(UnityEngine.Random.Range(0f, 1f), delegate
			{
				BiomeConstants.Instance.EmitBloodSplatter(vomit.Data.Position, Vector3.back, Color.green);
				BiomeConstants.Instance.EmitBloodDieEffect(vomit.Data.Position, Vector3.back, Color.green);
				AudioManager.Instance.PlayOneShot("event:/followers/poop_pop", vomit.Data.Position);
				Vomit.SpawnLoot(vomit.Data.Position);
				vomit.Remove();
			}));
		}
		for (int num2 = allStructuresOfType2.Count - 1; num2 >= 0; num2--)
		{
			StructureBrain poop = allStructuresOfType2[num2];
			GameManager.GetInstance().StartCoroutine(_003CClearAllWaste_003Eg__Delay_007C54_0(UnityEngine.Random.Range(0f, 1f), delegate
			{
				BiomeConstants.Instance.EmitSmokeExplosionVFX(poop.Data.Position);
				AudioManager.Instance.PlayOneShot("event:/followers/poop_pop", poop.Data.Position);
				poop.Remove();
			}));
		}
	}

	[CompilerGenerated]
	internal static IEnumerator _003CClearAllWaste_003Eg__Delay_007C54_0(float delay, Action callback)
	{
		yield return new WaitForSeconds(delay);
		if (callback != null)
		{
			callback();
		}
	}
}
