using UnityEngine;

public class RoomOld : BaseMonoBehaviour
{
	public int x;

	public int y;

	public Room N_Room;

	public Room E_Room;

	public Room S_Room;

	public Room W_Room;

	public Room N_Link;

	public Room E_Link;

	public Room S_Link;

	public Room W_Link;

	public bool regionSet;

	public int region;

	public bool isHome;

	public bool isEntranceHall;

	public bool pointOfInterest;

	public int[,] RoomGrid;

	public int[,] Structures;

	public float[,] PerlinNoise;

	public float[,] PerlinNoiseRock;

	public Vector2 NorthDoor;

	public Vector2 EastDoor;

	public Vector2 SouthDoor;

	public Vector2 WestDoor;

	public int Width;

	public int Height;

	public bool visited;

	public bool cleared;

	private int HORIZONTAL;

	private int VERTICAL;

	private Vector2 start;

	private int targetX;

	private int targetY;

	private int MidX;

	private int MidY;

	public void NewPointOfInterestRoom(int width, int height)
	{
		Width = width;
		Height = height;
		RoomGrid = new int[Width, Height];
		SetDoorPositions();
		CreatePath(NorthDoor, SouthDoor);
		CreatePath(EastDoor, WestDoor);
		SetPath(NorthDoor, WestDoor, VERTICAL);
		SetPath(NorthDoor, EastDoor, VERTICAL);
		SetPath(EastDoor, SouthDoor, HORIZONTAL);
		PerlinNoise = new float[Width * 4, Height * 4];
		for (int i = 0; i < Width * 4; i++)
		{
			for (int j = 0; j < Height * 4; j++)
			{
				if (i <= 1 || i >= Width * 4 - 2 || j <= 1 || j >= height * 4 - 2)
				{
					PerlinNoise[i, j] = 1f;
				}
			}
		}
		Structures = new int[Width * 4, Height * 4];
		if (Random.Range(0, 100) < 60)
		{
			Structures[Width * 2, Height * 2] = 9;
		}
		else
		{
			Structures[Width * 2, Height * 2] = 8;
		}
	}

	public void NewRoom(int width, int height)
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
		PerlinNoise = new float[Width * 4, Height * 4];
		float num = Random.Range(0, 1);
		float num2 = Random.Range(0, 1);
		for (int k = 0; k < Width * 4; k++)
		{
			for (int l = 0; l < Height * 4; l++)
			{
				PerlinNoise[k, l] = Mathf.PerlinNoise((float)k * 0.15f + num, (float)l * 0.15f + num2);
			}
		}
		PerlinNoiseRock = new float[Width * 4, Height * 4];
		num = Random.Range(0, 1);
		num2 = Random.Range(0, 1);
		for (int m = 0; m < Width * 4; m++)
		{
			for (int n = 0; n < Height * 4; n++)
			{
				PerlinNoiseRock[m, n] = Mathf.PerlinNoise((float)m * 0.25f + num, (float)n * 0.25f + num2);
			}
		}
		Structures = new int[Width * 4, Height * 4];
		for (int num3 = 0; num3 < Width; num3++)
		{
			for (int num4 = 0; num4 < Height; num4++)
			{
				int num7 = RoomGrid[num3, num4];
				int num8 = 1;
			}
		}
		for (int num5 = 0; num5 < Width * 4; num5++)
		{
			for (int num6 = 0; num6 < Height * 4; num6++)
			{
				if (RoomGrid[num5 / 4, num6 / 4] != 0 || Structures[num5, num6] != 0)
				{
					continue;
				}
				if ((double)PerlinNoise[num5, num6] <= 0.2)
				{
					Structures[num5, num6] = 4;
					if (num5 - 1 > 0)
					{
						Structures[num5 - 1, num6] = 6;
					}
					if (num5 + 1 < Width)
					{
						Structures[num5 + 1, num6] = 6;
					}
					if (num6 + 1 < Height)
					{
						Structures[num5, num6 + 1] = 6;
					}
					if (num6 - 1 > 0)
					{
						Structures[num5, num6 - 1] = 6;
					}
				}
				else if ((double)PerlinNoise[num5, num6] > 0.2 && (double)PerlinNoise[num5, num6] <= 0.3)
				{
					Structures[num5, num6] = 6;
				}
				else if ((double)PerlinNoise[num5, num6] > 0.3 && (double)PerlinNoise[num5, num6] <= 0.4)
				{
					Structures[num5, num6] = 7;
				}
				else if (PerlinNoise[num5, num6] >= 0.8f)
				{
					Structures[num5, num6] = 10;
				}
			}
		}
	}

	private void SetDoorPositions()
	{
		NorthDoor = new Vector2(Random.Range(1, Width - 1), Height);
		EastDoor = new Vector2(Width, Random.Range(1, Height - 1));
		SouthDoor = new Vector2(Random.Range(1, Width - 1), -1f);
		WestDoor = new Vector2(-1f, Random.Range(1, Height - 1));
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
