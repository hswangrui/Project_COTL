using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathTileManager : BaseMonoBehaviour
{
	[Serializable]
	public struct TileData
	{
		public StructureBrain.TYPES Type;

		public RuleTile Tile;

		public Tilemap TileMap;

		public GroundType sfxSound;
	}

	public static PathTileManager Instance;

	[SerializeField]
	private TileData[] tiles;

	private GridLayout gridLayout;

	private PlacementRegion placementRegion;

	private void Awake()
	{
		Instance = this;
		gridLayout = GetComponent<GridLayout>();
		placementRegion = GetComponentInParent<PlacementRegion>();
	}

	private void Start()
	{
		if (!(placementRegion != null))
		{
			return;
		}
		Structure structure = placementRegion.structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, (Action)delegate
		{
			for (int num = placementRegion.structureBrain.Data.pathData.Count - 1; num >= 0; num--)
			{
				if (placementRegion.structureBrain.Data.pathData[num].PathID != -1)
				{
					if ((!DataManager.Instance.DLC_Cultist_Pack && GetTileID(StructureBrain.TYPES.TILE_FLOWERS) == placementRegion.structureBrain.Data.pathData[num].PathID) || (!DataManager.Instance.DLC_Heretic_Pack && GetTileID(StructureBrain.TYPES.TILE_OLDFAITH) == placementRegion.structureBrain.Data.pathData[num].PathID))
					{
						placementRegion.structureBrain.Data.pathData.RemoveAt(num);
					}
					else
					{
						SetTile(placementRegion.structureBrain.Data.pathData[num].WorldPosition, placementRegion.structureBrain.Data.pathData[num].PathID);
					}
				}
			}
		});
	}

	private void SetTile(Vector3 worldPosition, int tileID)
	{
		PlacementRegion.TileGridTile closestTileGridTileAtWorldPosition = placementRegion.GetClosestTileGridTileAtWorldPosition(worldPosition);
		if (closestTileGridTileAtWorldPosition != null)
		{
			closestTileGridTileAtWorldPosition.PathID = tileID;
			Vector3Int position = gridLayout.WorldToCell(worldPosition);
			GetTileMap(tiles[tileID].Type).SetTile(position, tiles[tileID].Tile);
		}
	}

	public void SetTile(StructureBrain.TYPES type, Vector3 worldPosition)
	{
		DeleteTile(worldPosition);
		PlacementRegion.TileGridTile closestTileGridTileAtWorldPosition = placementRegion.GetClosestTileGridTileAtWorldPosition(worldPosition);
		if (closestTileGridTileAtWorldPosition != null)
		{
			int num = (closestTileGridTileAtWorldPosition.PathID = GetTileID(type));
			placementRegion.structureBrain.Data.SetPathData(closestTileGridTileAtWorldPosition.Position, closestTileGridTileAtWorldPosition.WorldPosition, num);
			Vector3Int position = gridLayout.WorldToCell(worldPosition);
			GetTileMap(tiles[num].Type).SetTile(position, tiles[num].Tile);
		}
	}

	public void ShowPathsBeingBuilt()
	{
		List<Structures_BuildSite> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_BuildSite>();
		if (allStructuresOfType.Count <= 0)
		{
			return;
		}
		foreach (Structures_BuildSite item in allStructuresOfType)
		{
			if (StructureBrain.IsPath(item.Data.ToBuildType))
			{
				DisplayTile(item.Data.ToBuildType, item.Data.Position);
			}
		}
	}

	public void HidePathsBeingBuilt()
	{
		List<Structures_BuildSite> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_BuildSite>();
		if (allStructuresOfType.Count <= 0)
		{
			return;
		}
		foreach (Structures_BuildSite item in allStructuresOfType)
		{
			if (StructureBrain.IsPath(item.Data.ToBuildType))
			{
				HideTile(item.Data.Position);
			}
		}
	}

	public StructureBrain.TYPES GetTileTypeAtPosition(Vector3 worldPosition)
	{
		TileData[] array = tiles;
		for (int i = 0; i < array.Length; i++)
		{
			TileData tileData = array[i];
			Vector3Int position = gridLayout.WorldToCell(worldPosition);
			TileBase tile = tileData.TileMap.GetTile(position);
			if (tile != null)
			{
				int tileID = GetTileID(tile);
				if (tileID != -1)
				{
					return tiles[tileID].Type;
				}
			}
		}
		return StructureBrain.TYPES.NONE;
	}

	public GroundType GetTileSoundAtPosition(Vector3 worldPosition)
	{
		TileData[] array = tiles;
		for (int i = 0; i < array.Length; i++)
		{
			TileData tileData = array[i];
			Vector3Int position = gridLayout.WorldToCell(worldPosition);
			TileBase tile = tileData.TileMap.GetTile(position);
			if (tile != null)
			{
				int tileID = GetTileID(tile);
				if (tileID != -1)
				{
					return tiles[tileID].sfxSound;
				}
			}
		}
		return GroundType.None;
	}

	public void DeleteTile(Vector3 worldPosition)
	{
		StructureBrain.TYPES tYPES = StructureBrain.TYPES.NONE;
		PlacementRegion.TileGridTile closestTileGridTileAtWorldPosition = placementRegion.GetClosestTileGridTileAtWorldPosition(worldPosition);
		if (closestTileGridTileAtWorldPosition != null)
		{
			closestTileGridTileAtWorldPosition.PathID = -1;
			placementRegion.structureBrain.Data.SetPathData(closestTileGridTileAtWorldPosition.Position, closestTileGridTileAtWorldPosition.WorldPosition, -1);
		}
		worldPosition = closestTileGridTileAtWorldPosition.WorldPosition;
		Vector3Int position = gridLayout.WorldToCell(worldPosition);
		TileData[] array = tiles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].TileMap.SetTile(position, null);
		}
		if (tYPES != 0)
		{
			for (int j = 0; j < StructuresData.GetCost(tYPES).Count; j++)
			{
				Inventory.AddItem((int)StructuresData.GetCost(tYPES)[j].CostItem, StructuresData.GetCost(tYPES)[j].CostValue);
			}
		}
	}

	public void DisplayTile(StructureBrain.TYPES type, Vector3 worldPosition)
	{
		int tileID = GetTileID(type);
		Vector3Int position = gridLayout.WorldToCell(worldPosition);
		GetTileMap(type).SetTile(position, tiles[tileID].Tile);
	}

	public void HideTile(Vector3 worldPosition)
	{
		TileData[] array = tiles;
		for (int i = 0; i < array.Length; i++)
		{
			TileData tileData = array[i];
			Vector3Int position = gridLayout.WorldToCell(worldPosition);
			GetTileMap(tileData.Type).SetTile(position, null);
		}
	}

	private Tilemap GetTileMap(StructureBrain.TYPES type)
	{
		TileData[] array = tiles;
		for (int i = 0; i < array.Length; i++)
		{
			TileData tileData = array[i];
			if (tileData.Type == type)
			{
				return tileData.TileMap;
			}
		}
		return null;
	}

	public int GetTileID(StructureBrain.TYPES type)
	{
		for (int i = 0; i < tiles.Length; i++)
		{
			if (tiles[i].Type == type)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetTileID(TileBase tile)
	{
		for (int i = 0; i < tiles.Length; i++)
		{
			if (tiles[i].Tile == tile)
			{
				return i;
			}
		}
		return -1;
	}
}
