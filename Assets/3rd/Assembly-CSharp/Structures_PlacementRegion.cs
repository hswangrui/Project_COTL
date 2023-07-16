using System.Collections.Generic;
using UnityEngine;

public class Structures_PlacementRegion : StructureBrain
{
	public List<PlacementRegion.ResourcesAndCount> ResourcesToPlace = new List<PlacementRegion.ResourcesAndCount>();

	public bool PlaceWeeds = true;

	public bool PlaceRubble = true;

	public PlacementRegion.TileGridTile GetTileGridTile(Vector2Int Position)
	{
		if (Data.GridTileLookup != null && Data.GridTileLookup.Count != Data.Grid.Count)
		{
			Data.UpdateDictionaryLookup();
		}
		if (Data.GridTileLookup != null && Data.GridTileLookup.ContainsKey(Position))
		{
			return Data.GridTileLookup[Position];
		}
		foreach (PlacementRegion.TileGridTile item in Data.Grid)
		{
			if (item.Position.x == Position.x && item.Position.y == Position.y)
			{
				return item;
			}
		}
		return null;
	}

	public StructuresData GetObstructionAtPosition(Vector2Int Position)
	{
		StructuresData result = null;
		foreach (StructuresData item in StructureManager.StructuresDataAtLocation(Data.Location))
		{
			if (Position.x >= item.GridTilePosition.x && Position.x < item.GridTilePosition.x + item.Bounds.x && Position.y >= item.GridTilePosition.y && Position.y < item.GridTilePosition.y + item.Bounds.y && item.IsObstruction)
			{
				result = item;
			}
		}
		return result;
	}

	public StructureBrain GetOccupationAtPosition(Vector2Int Position)
	{
		StructureBrain result = null;
		foreach (StructureBrain allBrain in StructureBrain.AllBrains)
		{
			if (Position.x >= allBrain.Data.GridTilePosition.x && Position.x < allBrain.Data.GridTilePosition.x + allBrain.Data.Bounds.x && Position.y >= allBrain.Data.GridTilePosition.y && Position.y < allBrain.Data.GridTilePosition.y + allBrain.Data.Bounds.y && !allBrain.Data.IsObstruction)
			{
				result = allBrain;
			}
		}
		return result;
	}

	public Structures_Weeds GetWeedAtLocation(Vector2Int Position)
	{
		Structures_Weeds result = null;
		foreach (Structures_Weeds item in StructureManager.GetAllStructuresOfType<Structures_Weeds>())
		{
			Debug.Log(string.Concat("WEED  ", item.Data.GridTilePosition, "   ", item.Data.Position, "   ", Position));
			if (Position.x >= item.Data.GridTilePosition.x && Position.x < item.Data.GridTilePosition.x + item.Data.Bounds.x && Position.y >= item.Data.GridTilePosition.y && Position.y < item.Data.GridTilePosition.y + item.Data.Bounds.y && item.Data.IsObstruction)
			{
				result = item;
			}
		}
		return result;
	}

	public void ClearStructureFromGrid(StructureBrain structure)
	{
		ClearStructureFromGrid(structure, structure.Data.GridTilePosition);
	}

	public void ClearStructureFromGrid(StructureBrain structure, Vector2Int gridTilePosition)
	{
		if (GetTileGridTile(gridTilePosition) == null)
		{
			return;
		}
		Vector2Int bounds = structure.Data.Bounds;
		int num = -1;
		while (++num < bounds.x)
		{
			int num2 = -1;
			while (++num2 < bounds.y)
			{
				PlacementRegion.TileGridTile tileGridTile = GetTileGridTile(new Vector2Int(gridTilePosition.x + num, gridTilePosition.y + num2));
				if (tileGridTile != null)
				{
					if (structure.Data.IsObstruction)
					{
						tileGridTile.Obstructed = false;
					}
					else
					{
						tileGridTile.Occupied = false;
					}
					tileGridTile.ReservedForWaste = false;
					if (structure.Data.Type != TYPES.BUILD_SITE && tileGridTile.ObjectOnTile != TYPES.BUILD_SITE)
					{
						tileGridTile.OldObjectID = ((tileGridTile.ObjectID != -1) ? tileGridTile.ObjectID : tileGridTile.OldObjectID);
					}
					tileGridTile.ObjectID = -1;
					tileGridTile.ObjectOnTile = TYPES.NONE;
				}
			}
		}
		num = -2;
		while (++num < bounds.x + 1)
		{
			int num3 = -2;
			while (++num3 < bounds.y + 1)
			{
				PlacementRegion.TileGridTile tileGridTile2 = GetTileGridTile(new Vector2Int(gridTilePosition.x + num, gridTilePosition.y + num3));
				if (tileGridTile2 != null)
				{
					tileGridTile2.BlockNeighbouringTiles = Mathf.Max(0, tileGridTile2.BlockNeighbouringTiles - 1);
				}
			}
		}
	}

	public void AddStructureToGrid(StructuresData structure, Vector2Int gridTilePosition, bool upgrade = false)
	{
		int num = -1;
		while (++num < structure.Bounds.x)
		{
			int num2 = -1;
			while (++num2 < structure.Bounds.y)
			{
				PlacementRegion.TileGridTile tileGridTile = GetTileGridTile(new Vector2Int(gridTilePosition.x + num, gridTilePosition.y + num2));
				if (tileGridTile != null)
				{
					if (structure.IsObstruction)
					{
						tileGridTile.Obstructed = true;
					}
					else if (!structure.DoesNotOccupyGrid)
					{
						tileGridTile.Occupied = true;
					}
					if (structure.Type != TYPES.BUILD_SITE && tileGridTile.ObjectOnTile != TYPES.BUILD_SITE)
					{
						tileGridTile.OldObjectID = ((tileGridTile.ObjectID != -1) ? tileGridTile.ObjectID : tileGridTile.OldObjectID);
					}
					tileGridTile.ObjectOnTile = structure.Type;
					tileGridTile.ObjectID = structure.ID;
					tileGridTile.IsUpgrade = upgrade;
				}
			}
		}
		switch (structure.Type)
		{
		case TYPES.TREE:
		case TYPES.RUBBLE:
		case TYPES.WEEDS:
		case TYPES.BERRY_BUSH:
		case TYPES.RUBBLE_BIG:
			return;
		}
		num = -2;
		while (++num < structure.Bounds.x + 1)
		{
			int num3 = -2;
			while (++num3 < structure.Bounds.y + 1)
			{
				PlacementRegion.TileGridTile tileGridTile2 = GetTileGridTile(new Vector2Int(gridTilePosition.x + num, gridTilePosition.y + num3));
				if (tileGridTile2 != null)
				{
					tileGridTile2.BlockNeighbouringTiles++;
				}
			}
		}
	}

	public void AddStructureToGrid(StructuresData structure, bool upgrade = false)
	{
		AddStructureToGrid(structure, structure.GridTilePosition, upgrade);
	}

	public int GetPreviousUpgradeID(StructuresData structure)
	{
		int num = -1;
		while (++num < structure.Bounds.x)
		{
			int num2 = -1;
			while (++num2 < structure.Bounds.y)
			{
				PlacementRegion.TileGridTile tileGridTile = GetTileGridTile(new Vector2Int(structure.GridTilePosition.x + num, structure.GridTilePosition.y + num2));
				if (tileGridTile != null && tileGridTile.Occupied && tileGridTile.IsUpgrade)
				{
					return tileGridTile.OldObjectID;
				}
			}
		}
		return -1;
	}

	public void MarkObstructionsForClearing(Vector2Int GridPosition, Vector2Int Bounds)
	{
		int num = -1;
		while (++num < Bounds.x)
		{
			int num2 = -1;
			while (++num2 < Bounds.y)
			{
				Vector2Int position = new Vector2Int(GridPosition.x + num, GridPosition.y + num2);
				PlacementRegion.TileGridTile tileGridTile = GetTileGridTile(position);
				if (tileGridTile != null && tileGridTile.Obstructed)
				{
					StructuresData obstructionAtPosition = GetObstructionAtPosition(position);
					if (obstructionAtPosition != null)
					{
						obstructionAtPosition.Prioritised = true;
					}
					else
					{
						tileGridTile.Obstructed = false;
					}
				}
			}
		}
	}
}
