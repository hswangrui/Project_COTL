using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonDecorator : BaseMonoBehaviour
{
	public enum STRUCTURES
	{
		NONE,
		STRUCTURE_TILE1,
		STRUCTURE_TILE2,
		STRUCTURE_TILE3,
		STRUCTURE_TREE,
		STRUCTURE_TREE_STUMP,
		STRUCTURE_BUSH,
		STRUCTURE_GRASS,
		STRUCTURE_STATUE_SWORD,
		STRUCTURE_VILLAGE,
		STRUCTURE_STONE
	}

	public RuleTile Tile0;

	public RuleTile Tile1;

	public RuleTile Tile2;

	public RuleTile Tile3;

	public RuleTile Tile4;

	public Tilemap TileMap;

	private float xl;

	private float _x;

	private float yl;

	private float _y;

	private float sample;

	public int width = 50;

	public int height = 50;

	public Vector3 Size;

	private GridLayout gridLayout;

	private static DungeonDecorator instance;

	public SpriteRenderer ground;

	private void OnEnable()
	{
		instance = this;
	}

	private void OnDisable()
	{
		instance = null;
	}

	public static DungeonDecorator getInsance()
	{
		return instance;
	}

	public void Decorate(int w, int h, Room r)
	{
		width = w;
		height = h;
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				sample = r.PerlinNoise[i, j];
				if (!((double)sample > 0.4))
				{
					TileMap.SetTile(new Vector3Int(i - width / 2, j - height / 2, 0), Tile1);
				}
			}
		}
		gridLayout = Object.FindObjectOfType<GridLayout>();
		Size = gridLayout.CellToWorld(new Vector3Int(width, height, 0));
		if (ground != null)
		{
			ground.size = Size;
		}
	}

	private void AddCorner()
	{
	}

	public void AddGrass(Room r)
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				if (TileMap.GetTile(new Vector3Int(i - width / 2, j - height / 2, 0)) == Tile1)
				{
					Object.Instantiate(Resources.Load("Prefabs/Particles/Long Grass") as GameObject, base.transform.parent, true).transform.position = gridLayout.CellToWorld(new Vector3Int(i - width / 2, j - height / 2, 0));
				}
			}
		}
	}

	public void AddBlockedAreas(Room r)
	{
		for (int i = 0; i < r.Width; i++)
		{
			for (int j = 0; j < r.Height; j++)
			{
				if (r.RoomGrid[i, j] != 1)
				{
					continue;
				}
				for (int k = 0; k < 4; k++)
				{
					for (int l = 0; l < 4; l++)
					{
						TileMap.SetTile(new Vector3Int(i * 4 - width / 2 + k, j * 4 - height / 2 + l, 0), Tile4);
					}
				}
			}
		}
	}

	public void AddOuterWalls(Room r)
	{
		for (int i = -2; i < width + 2; i++)
		{
			for (int j = -2; j < height + 2; j++)
			{
				if ((i == -1 || i == -2 || i == width || i == width + 1 || j == -1 || j == height || j == -2 || j == height + 1) && TileMap.GetTile(new Vector3Int(i - width / 2, j - height / 2, 0)) != Tile1)
				{
					TileMap.SetTile(new Vector3Int(i - width / 2, j - height / 2, 0), Tile3);
				}
			}
		}
	}

	public void SetDoors(Room r, GameObject N, GameObject E, GameObject S, GameObject W)
	{
		N.transform.position = gridLayout.CellToWorld(new Vector3Int((int)(r.NorthDoor.x * 4f) - width / 2 + 2, (int)(r.NorthDoor.y * 4f) - height / 2, 0));
		E.transform.position = gridLayout.CellToWorld(new Vector3Int((int)(r.EastDoor.x * 4f) - width / 2, (int)(r.EastDoor.y * 4f) - height / 2 + 2, 0));
		S.transform.position = gridLayout.CellToWorld(new Vector3Int((int)(r.SouthDoor.x * 4f) - width / 2 + 2, -1 - height / 2, 0));
		W.transform.position = gridLayout.CellToWorld(new Vector3Int(-1 - width / 2, (int)(r.WestDoor.y * 4f) - height / 2 + 2, 0));
		if (r.N_Link != null)
		{
			for (int i = 0; i < 4; i++)
			{
				TileMap.SetTile(new Vector3Int((int)(r.NorthDoor.x * 4f) - width / 2 + i, (int)(r.NorthDoor.y * 4f) - height / 2, 0), Tile1);
				TileMap.SetTile(new Vector3Int((int)(r.NorthDoor.x * 4f) - width / 2 + i, (int)(r.NorthDoor.y * 4f) - height / 2 + 1, 0), Tile1);
			}
		}
		if (r.E_Link != null)
		{
			for (int j = 0; j < 4; j++)
			{
				TileMap.SetTile(new Vector3Int((int)r.EastDoor.x * 4 - width / 2, (int)r.EastDoor.y * 4 - height / 2 + j, 0), Tile1);
				TileMap.SetTile(new Vector3Int((int)r.EastDoor.x * 4 - width / 2 + 1, (int)r.EastDoor.y * 4 - height / 2 + j, 0), Tile1);
			}
		}
		if (r.S_Link != null)
		{
			for (int k = 0; k < 4; k++)
			{
				TileMap.SetTile(new Vector3Int((int)(r.SouthDoor.x * 4f) - width / 2 + k, -1 - height / 2, 0), Tile1);
				TileMap.SetTile(new Vector3Int((int)(r.SouthDoor.x * 4f) - width / 2 + k, -1 - height / 2 - 1, 0), Tile1);
			}
		}
		if (r.W_Link != null)
		{
			for (int l = 0; l < 4; l++)
			{
				TileMap.SetTile(new Vector3Int(-1 - width / 2, (int)r.WestDoor.y * 4 - height / 2 + l, 0), Tile1);
				TileMap.SetTile(new Vector3Int(-1 - width / 2 - 1, (int)r.WestDoor.y * 4 - height / 2 + l, 0), Tile1);
			}
		}
	}

	public void PlaceWallTiles(Room r)
	{
	}

	public void PlaceStructures(Room r)
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				switch (r.StructuresOld[i, j])
				{
				case 1:
				{
					GameObject obj2 = Object.Instantiate(Resources.Load("Prefabs/Tile-Breakable") as GameObject, GameObject.FindGameObjectWithTag("Structures Layer").transform, true);
					obj2.transform.position = gridLayout.CellToWorld(new Vector3Int(i - width / 2, j - height / 2, 0)) + gridLayout.cellSize / 2f;
					float num2 = 0.4f + r.PerlinNoiseRock[i, j] * 0.6f;
					SpriteRenderer[] componentsInChildren = obj2.GetComponentsInChildren<SpriteRenderer>();
					for (int k = 0; k < componentsInChildren.Length; k++)
					{
						componentsInChildren[k].color = new Color(num2, num2, num2, 1f);
					}
					break;
				}
				case 2:
				{
					GameObject obj3 = Object.Instantiate(Resources.Load("Prefabs/Tile-Breakable2") as GameObject, GameObject.FindGameObjectWithTag("Structures Layer").transform, true);
					obj3.transform.position = gridLayout.CellToWorld(new Vector3Int(i - width / 2, j - height / 2, 0)) + gridLayout.cellSize / 2f;
					float num2 = 0.4f + r.PerlinNoiseRock[i, j] * 0.6f;
					SpriteRenderer[] componentsInChildren = obj3.GetComponentsInChildren<SpriteRenderer>();
					for (int k = 0; k < componentsInChildren.Length; k++)
					{
						componentsInChildren[k].color = new Color(num2, num2, num2, 1f);
					}
					break;
				}
				case 3:
				{
					GameObject obj5 = Object.Instantiate(Resources.Load("Prefabs/Tile-Breakable3") as GameObject, GameObject.FindGameObjectWithTag("Structures Layer").transform, true);
					obj5.transform.position = gridLayout.CellToWorld(new Vector3Int(i - width / 2, j - height / 2, 0)) + gridLayout.cellSize / 2f;
					float num2 = 0.4f + r.PerlinNoiseRock[i, j] * 0.6f;
					SpriteRenderer[] componentsInChildren = obj5.GetComponentsInChildren<SpriteRenderer>();
					for (int k = 0; k < componentsInChildren.Length; k++)
					{
						componentsInChildren[k].color = new Color(num2, num2, num2, 1f);
					}
					break;
				}
				case 4:
					Object.Instantiate(Resources.Load("Prefabs/Environment/Base/Tree/Tree_Base") as GameObject, GameObject.FindGameObjectWithTag("Structures Layer").transform, true).transform.position = gridLayout.CellToWorld(new Vector3Int(i - width / 2, j - height / 2, 0)) + gridLayout.cellSize / 2f;
					break;
				case 6:
				{
					GameObject obj4 = Object.Instantiate(Resources.Load("Prefabs/Crops/Bush") as GameObject, GameObject.FindGameObjectWithTag("Structures Layer").transform, true);
					obj4.transform.position = gridLayout.CellToWorld(new Vector3Int(i - width / 2, j - height / 2, 0)) + gridLayout.cellSize / 2f;
					float b = 1f - (r.PerlinNoise[i, j] - 0.2f) / 0.15f * 0.35f;
					b = Mathf.Min(1f, b);
					obj4.transform.localScale = new Vector3(b, b, b);
					break;
				}
				case 7:
					if (Random.Range(0, 100) == 0)
					{
						Object.Instantiate(Resources.Load("Prefabs/Crops/Flower") as GameObject, GameObject.FindGameObjectWithTag("Structures Layer").transform, true).transform.position = gridLayout.CellToWorld(new Vector3Int(i - width / 2, j - height / 2, 0)) + gridLayout.cellSize / 2f;
					}
					else
					{
						Object.Instantiate(Resources.Load("Prefabs/Crops/Long Grass") as GameObject, GameObject.FindGameObjectWithTag("Structures Layer").transform, true).transform.position = gridLayout.CellToWorld(new Vector3Int(i - width / 2, j - height / 2, 0)) + gridLayout.cellSize / 2f;
					}
					break;
				case 5:
				{
					GameObject obj = Object.Instantiate(Resources.Load("Prefabs/Environment/Base/Tree/Tree_Base") as GameObject, GameObject.FindGameObjectWithTag("Structures Layer").transform, true);
					obj.transform.position = gridLayout.CellToWorld(new Vector3Int(i - width / 2, j - height / 2, 0)) + gridLayout.cellSize / 2f;
					obj.GetComponent<Tree>().StartAsStump();
					break;
				}
				case 8:
					Object.Instantiate(Resources.Load("Prefabs/Structures/Statue - Sword") as GameObject, GameObject.FindGameObjectWithTag("Structures Layer").transform, true).transform.position = gridLayout.CellToWorld(new Vector3Int(i - width / 2, j - height / 2, 0)) + gridLayout.cellSize / 2f;
					break;
				case 9:
					Object.Instantiate(Resources.Load("Prefabs/Structures/Village/Village") as GameObject, GameObject.FindGameObjectWithTag("Structures Layer").transform, true).transform.position = gridLayout.CellToWorld(new Vector3Int(i - width / 2, j - height / 2, 0)) + gridLayout.cellSize / 2f;
					break;
				case 10:
				{
					int num = Random.Range(0, 3);
					Object.Instantiate(Resources.Load("Prefabs/Environment/Base/Rocks/ROCK_" + num) as GameObject, GameObject.FindGameObjectWithTag("Structures Layer").transform, true).transform.position = gridLayout.CellToWorld(new Vector3Int(i - width / 2, j - height / 2, 0)) + gridLayout.cellSize / 2f;
					break;
				}
				}
			}
		}
	}

	public void UpdateStructures(Room r, Vector3 position, int value)
	{
		if (!(gridLayout == null))
		{
			position = gridLayout.WorldToCell(position);
			if (r.StructuresOld != null)
			{
				r.StructuresOld[(int)position.x + width / 2, (int)position.y + height / 2] = value;
			}
		}
	}

	public void PlaceEnemies(Room r)
	{
		if (r.isHome || r.pointOfInterest)
		{
			return;
		}
		for (int i = 0; i < r.Width; i++)
		{
			for (int j = 0; j < r.Height; j++)
			{
				if (r.RoomGrid[i, j] == 0 && Random.Range(0, 30) < 1)
				{
					Object.Instantiate(Resources.Load("Prefabs/Units/Enemy2") as GameObject, GameObject.FindGameObjectWithTag("Unit Layer").transform, true).transform.position = gridLayout.CellToWorld(new Vector3Int(i * 4 - width / 2 + 1, j * 4 - height / 2 + 2, 0)) + gridLayout.cellSize / 2f;
					Object.Instantiate(Resources.Load("Prefabs/Units/Enemy2") as GameObject, GameObject.FindGameObjectWithTag("Unit Layer").transform, true).transform.position = gridLayout.CellToWorld(new Vector3Int(i * 4 - width / 2 + 3, j * 4 - height / 2 + 2, 0)) + gridLayout.cellSize / 2f;
				}
			}
		}
	}

	public void PlaceWildLife(Room r)
	{
	}
}
