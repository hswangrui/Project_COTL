using UnityEngine;

public class RoomTest : BaseMonoBehaviour
{
	public GameObject Tile;

	public GameObject EmptyTile;

	public GameObject DoorTile;

	private int[,] RoomGrid;

	private Vector2 NorthDoor;

	private Vector2 EastDoor;

	private Vector2 SouthDoor;

	private Vector2 WestDoor;

	private bool NorthEast;

	private bool NorthSouth;

	private bool NorthWest;

	private bool EastSouth;

	private bool EastWest;

	private bool SouthWest;

	private int Width;

	private int Height;

	private int HORIZONTAL;

	private int VERTICAL;

	private Vector2 start;

	private int targetX;

	private int targetY;

	private int MidX;

	private int MidY;

	private void Start()
	{
		NewRoom(Random.Range(5, 20), Random.Range(5, 20));
	}

	private void NewRoom(int width, int height)
	{
		Width = width;
		Height = height;
		RoomGrid = new int[Width, Height];
		SetDoorPositions();
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if ((double)Random.Range(0f, 10f) < 3.5)
				{
					RoomGrid[i, j] = 1;
				}
			}
		}
		CreatePath(NorthDoor, SouthDoor);
		CreatePath(EastDoor, WestDoor);
		SetPath(NorthDoor, WestDoor, VERTICAL);
		SetPath(NorthDoor, EastDoor, VERTICAL);
		SetPath(EastDoor, SouthDoor, HORIZONTAL);
		for (int k = 0; k < Width; k++)
		{
			for (int l = 0; l < Height; l++)
			{
				switch (RoomGrid[k, l])
				{
				case 0:
					Object.Instantiate(EmptyTile, base.transform.parent, true).transform.position = new Vector3(k, l, 0f);
					break;
				case 1:
					Object.Instantiate(Tile, base.transform.parent, true).transform.position = new Vector3(k, l, 0f);
					break;
				}
			}
		}
	}

	private void SetDoorPositions()
	{
		NorthDoor = new Vector2(Random.Range(1, Width - 1), Height);
		Object.Instantiate(DoorTile, new Vector3(NorthDoor.x, NorthDoor.y, 0f), Quaternion.identity);
		EastDoor = new Vector2(Width, Random.Range(1, Height - 1));
		Object.Instantiate(DoorTile, new Vector3(EastDoor.x, EastDoor.y, 0f), Quaternion.identity);
		SouthDoor = new Vector2(Random.Range(1, Width - 1), -1f);
		Object.Instantiate(DoorTile, new Vector3(SouthDoor.x, SouthDoor.y, 0f), Quaternion.identity);
		WestDoor = new Vector2(-1f, Random.Range(1, Height - 1));
		Object.Instantiate(DoorTile, new Vector3(WestDoor.x, WestDoor.y, 0f), Quaternion.identity);
	}

	private void SetPath(Vector2 _start, Vector2 _end, int InitialDir)
	{
		_start.x = Mathf.Min(_start.x, Width - 1);
		_start.x = Mathf.Max(_start.x, 0f);
		_start.y = Mathf.Min(_start.y, Height - 1);
		_start.y = Mathf.Max(_start.y, 0f);
		_end.x = Mathf.Min(_end.x, Width - 1);
		_end.x = Mathf.Max(_end.x, 0f);
		_end.y = Mathf.Min(_end.y, Height - 1);
		_end.y = Mathf.Max(_end.y, 0f);
		RoomGrid[(int)_start.x, (int)_start.y] = 0;
		if (InitialDir == VERTICAL)
		{
			while (_start.y < _end.y)
			{
				_start.y += 1f;
				RoomGrid[(int)_start.x, (int)_start.y] = 0;
			}
			while (_start.y > _end.y)
			{
				_start.y -= 1f;
				RoomGrid[(int)_start.x, (int)_start.y] = 0;
			}
			while (_start.x < _end.x)
			{
				_start.x += 1f;
				RoomGrid[(int)_start.x, (int)_start.y] = 0;
			}
			while (_start.x > _end.x)
			{
				_start.x -= 1f;
				RoomGrid[(int)_start.x, (int)_start.y] = 0;
			}
		}
		if (InitialDir == HORIZONTAL)
		{
			while (_start.x < _end.x)
			{
				_start.x += 1f;
				RoomGrid[(int)_start.x, (int)_start.y] = 0;
			}
			while (_start.x > _end.x)
			{
				_start.x -= 1f;
				RoomGrid[(int)_start.x, (int)_start.y] = 0;
			}
			while (_start.y < _end.y)
			{
				_start.y += 1f;
				RoomGrid[(int)_start.x, (int)_start.y] = 0;
			}
			while (_start.y > _end.y)
			{
				_start.y -= 1f;
				RoomGrid[(int)_start.x, (int)_start.y] = 0;
			}
		}
	}

	private void CreatePath(Vector2 _start, Vector2 end)
	{
		start = _start;
		targetX = (int)end.x;
		if (targetX < 0)
		{
			targetX = 0;
		}
		if (targetX > Width)
		{
			targetX = Width;
		}
		targetY = (int)end.y;
		if (targetY < 0)
		{
			targetY = 0;
		}
		if (targetY > Height)
		{
			targetY = Height;
		}
		start.x = Mathf.Min(start.x, Width - 1);
		start.x = Mathf.Max(start.x, 0f);
		start.y = Mathf.Min(start.y, Height - 1);
		start.y = Mathf.Max(start.y, 0f);
		MidX = (int)Mathf.Abs((float)targetX - start.x);
		MidY = (int)Mathf.Abs((float)targetY - start.y);
		if (Mathf.Abs(start.x - (float)targetX) > Mathf.Abs(start.y - (float)targetY))
		{
			while (start.x < (float)targetX)
			{
				start.x += 1f;
				RoomGrid[(int)start.x, (int)start.y] = 0;
				if ((int)start.x == MidX)
				{
					MoveVertically();
				}
			}
			while (start.x > (float)targetX)
			{
				start.x -= 1f;
				RoomGrid[(int)start.x, (int)start.y] = 0;
				if ((int)start.x == MidX)
				{
					MoveVertically();
				}
			}
			return;
		}
		while (start.y < (float)targetY)
		{
			start.y += 1f;
			RoomGrid[(int)start.x, (int)start.y] = 0;
			if ((int)start.y == MidY)
			{
				MoveHorizontally();
			}
		}
		while (start.y > (float)targetY)
		{
			start.y -= 1f;
			RoomGrid[(int)start.x, (int)start.y] = 0;
			if ((int)start.y == MidY)
			{
				MoveHorizontally();
			}
		}
	}

	private void MoveVertically()
	{
		while (start.y < (float)targetY)
		{
			start.y += 1f;
			RoomGrid[(int)start.x, (int)start.y] = 0;
		}
		while (start.y > (float)targetY)
		{
			start.y -= 1f;
			RoomGrid[(int)start.x, (int)start.y] = 0;
		}
	}

	private void MoveHorizontally()
	{
		while (start.x > (float)targetX)
		{
			start.x -= 1f;
			RoomGrid[(int)start.x, (int)start.y] = 0;
		}
		while (start.x < (float)targetX)
		{
			start.x += 1f;
			RoomGrid[(int)start.x, (int)start.y] = 0;
		}
	}
}
