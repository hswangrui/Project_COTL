using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGen : BaseMonoBehaviour
{
	public enum StartingRoom
	{
		Center,
		North,
		East,
		South,
		West
	}

	public delegate void WorldGeneratedAction();

	private enum dir
	{
		north,
		east,
		south,
		west
	}

	public int Seed;

	private System.Random RandomSeed;

	public static bool WorldGenerated;

	public int width;

	public int height;

	public static int WIDTH;

	public static int HEIGHT;

	public int roomDist = 30;

	public GameObject room;

	public GameObject link;

	public static List<Room> rooms;

	public bool TutorialRooms;

	public bool StartAtCrossRoad = true;

	public StartingRoom StartRoom;

	public bool InstantiatePrefabs = true;

	public int NumRoomsOfInterest = 2;

	public static Room startRoom;

	public static WorldGen Instance;

	public int TreeCount;

	public int RockCount;

	public int SeedFlowerCount;

	public int CottonPlantCount;

	public int FollowerCount;

	[HideInInspector]
	public int NumRegions;

	public event WorldGeneratedAction OnWorldGenerated;

	private void Awake()
	{
		if (Instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	public static void ClearGeneratedWorld()
	{
		ResetGeneratedWorldData();
		WorldGenerated = false;
		if (Instance != null)
		{
			int num = -1;
			while (++num < Instance.transform.childCount)
			{
				UnityEngine.Object.Destroy(Instance.transform.GetChild(num).gameObject);
			}
		}
		if (rooms != null)
		{
			rooms.Clear();
		}
	}

	public static void ResetGeneratedWorldData()
	{
		RoomManager.CurrentX = -1;
		RoomManager.CurrentY = -1;
		RoomManager.PrevCurrentX = -1;
		RoomManager.PrevCurrentY = -1;
		Room.PointOfInterestCount = -1;
		RoomManager.r = null;
	}

	private void CreateTutorialRooms()
	{
		WIDTH = 3;
		HEIGHT = 1;
		rooms = new List<Room>();
		int num = -1;
		int num2 = 0;
		while (++num < 3)
		{
			GameObject obj = new GameObject();
			obj.transform.position = new Vector3((float)(num * roomDist - width) + 0.5f, 0f, (float)(num2 * roomDist - height) + 0.5f);
			Room room = obj.AddComponent<Room>();
			room.x = num;
			room.y = 0;
			room.Structures = new List<StructuresData>();
			obj.transform.parent = base.gameObject.transform;
			obj.name = "Room: " + num + "_" + num2;
			room.CurrentSpecificRoom = (Room.SpecificRoom)(8 + num);
			room.NewPointOfInterestRoom();
			rooms.Add(room);
		}
		startRoom = rooms[0];
		rooms[0].E_Link = rooms[1];
		rooms[0].E_Room = rooms[1];
		rooms[1].W_Link = rooms[0];
		rooms[1].W_Room = rooms[0];
		rooms[1].E_Link = rooms[2];
		rooms[1].E_Room = rooms[2];
		rooms[2].W_Link = rooms[1];
		rooms[2].W_Room = rooms[1];
		rooms[2].E_Link = rooms[0];
		rooms[2].E_Room = rooms[0];
	}

	public static void GenerateNewWorld()
	{
		if (Instance != null)
		{
			Instance.Start();
		}
	}

	public void Start()
	{
		RandomSeed = new System.Random(Seed);
		if (!DataManager.Instance.Tutorial_Rooms_Completed && DataManager.Instance.Create_Tutorial_Rooms)
		{
			ResetGeneratedWorldData();
			CreateTutorialRooms();
		}
		else if (!WorldGenerated)
		{
			ResetGeneratedWorldData();
			WIDTH = width;
			HEIGHT = height;
			CreateAndConnectRooms();
		}
		WorldGenerated = true;
		if (this.OnWorldGenerated != null)
		{
			this.OnWorldGenerated();
		}
	}

	private void Clear()
	{
		foreach (Room room in rooms)
		{
			room.Clear();
		}
		rooms = new List<Room>();
	}

	private void CreateResources()
	{
		TreeCount = 50;
		RockCount = 30;
		SeedFlowerCount = 20;
		CottonPlantCount = 20;
		FollowerCount = 5;
	}

	private void CreateAndConnectRooms()
	{
		rooms = new List<Room>();
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				Room room;
				if (InstantiatePrefabs)
				{
					room = UnityEngine.Object.Instantiate(this.room, new Vector3((float)(i * roomDist - width) + 0.5f, 0f, (float)(j * roomDist - height) + 0.5f), Quaternion.identity).GetComponent<Room>();
				}
				else
				{
					GameObject obj = new GameObject();
					obj.transform.position = new Vector3((float)(i * roomDist - width) + 0.5f, 0f, (float)(j * roomDist - height) + 0.5f);
					room = obj.AddComponent<Room>();
					obj.transform.parent = base.gameObject.transform;
					obj.name = "Room: " + i + "_" + j;
				}
				room.x = i;
				room.y = j;
				rooms.Add(room);
			}
		}
		findNeighbours();
		switch (StartRoom)
		{
		case StartingRoom.Center:
			setCrossRoad(width / 2, height / 2);
			break;
		case StartingRoom.North:
			setCrossRoad(width / 2, height - 1);
			break;
		case StartingRoom.East:
			setCrossRoad(width - 1, height / 2);
			break;
		case StartingRoom.South:
			setCrossRoad(width / 2, 0);
			break;
		case StartingRoom.West:
			setCrossRoad(0, height / 2);
			break;
		}
		setLinks();
		Room room2 = null;
		foreach (Room room4 in rooms)
		{
			if (room4.isHome)
			{
				room2 = room4;
				room4.NewHomeRoom();
			}
			else if (room4.isEntranceHall)
			{
				room4.NewRoom(RandomSeed.Next(0, int.MaxValue));
			}
			else if (room4.pointOfInterest)
			{
				room4.NewPointOfInterestRoom();
			}
			else
			{
				room4.NewRoom(RandomSeed.Next(0, int.MaxValue));
			}
		}
		Room room3 = new Room();
		room3.NewBaseRoom();
		room3.x = WIDTH / 2;
		room3.y = -1;
		room3.N_Link = room2;
		room2.S_Link = room3;
		rooms.Add(room3);
	}

	private void findNeighbours()
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				Room room = getRoom(i, j);
				if (i + 1 < width)
				{
					room.E_Room = getRoom(i + 1, j);
				}
				if (i - 1 >= 0)
				{
					room.W_Room = getRoom(i - 1, j);
				}
				if (j + 1 < height)
				{
					room.N_Room = getRoom(i, j + 1);
				}
				if (j - 1 >= 0)
				{
					room.S_Room = getRoom(i, j - 1);
				}
			}
		}
	}

	private void setCrossRoad(int i, int j)
	{
		Room room = getRoom(i, j);
		room.isHome = true;
		if (StartAtCrossRoad)
		{
			startRoom = room;
		}
		if (room != null)
		{
			if (room.N_Room != null)
			{
				addLink(dir.north, room);
				room.N_Room.S_Link = room;
				room.N_Link = room.N_Room;
			}
			if (room.E_Room != null && room.E_Room.W_Room == room && room.E_Room.W_Link == null)
			{
				addLink(dir.east, room);
				room.E_Room.W_Link = room;
				room.E_Link = room.E_Room;
			}
			if (room.S_Room != null && room.S_Room.N_Room == room && room.S_Room.N_Link == null)
			{
				addLink(dir.south, room);
				room.S_Room.N_Link = room;
				room.S_Link = room.S_Room;
			}
			if (room.W_Room != null && room.W_Room.E_Room == room && room.W_Room.E_Link == null)
			{
				addLink(dir.west, room);
				room.W_Room.E_Link = room;
				room.W_Link = room.W_Room;
			}
		}
		if (!TutorialRooms)
		{
			return;
		}
		for (int k = 1; k < j + 1; k++)
		{
			room = getRoom(i - k, j);
			room.isEntranceHall = true;
			if (!StartAtCrossRoad)
			{
				startRoom = room;
			}
			if (room.E_Room.W_Room == room && room.E_Room.W_Link == null)
			{
				addLink(dir.east, room);
				room.E_Room.W_Link = room;
				room.E_Link = room.E_Room;
			}
		}
	}

	private void setLinks()
	{
		int num = 1;
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				Room room = getRoom(i, j);
				if (!room.isEntranceHall)
				{
					if (room.N_Room != null && RandomSeed.Next(0, 10) < num && !room.N_Room.isEntranceHall && room.N_Room.S_Room == room && room.N_Room.S_Link == null)
					{
						addLink(dir.north, room);
						room.N_Room.S_Link = room;
						room.N_Link = room.N_Room;
					}
					if (room.E_Room != null && RandomSeed.Next(0, 10) < num && !room.E_Room.isEntranceHall && room.E_Room.W_Room == room && room.E_Room.W_Link == null)
					{
						addLink(dir.east, room);
						room.E_Room.W_Link = room;
						room.E_Link = room.E_Room;
					}
					if (room.S_Room != null && RandomSeed.Next(0, 10) < num && !room.S_Room.isEntranceHall && room.S_Room.N_Room == room && room.S_Room.N_Link == null)
					{
						addLink(dir.south, room);
						room.S_Room.N_Link = room;
						room.S_Link = room.S_Room;
					}
					if (room.W_Room != null && RandomSeed.Next(0, 10) < num && !room.W_Room.isEntranceHall && room.W_Room.E_Room == room && room.W_Room.E_Link == null)
					{
						addLink(dir.west, room);
						room.W_Room.E_Link = room;
						room.W_Link = room.W_Room;
					}
				}
			}
		}
		checkLonely();
		applyIslands();
		while (NumRegions > 1)
		{
			connectIsland();
		}
	}

	private void checkLonely()
	{
		ArrayList arrayList = new ArrayList();
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				Room room = getRoom(i, j);
				if (room.E_Link == null && room.W_Link == null && room.S_Link == null && room.N_Link == null && !room.isEntranceHall)
				{
					arrayList.Add(room);
				}
			}
		}
		foreach (Room item in arrayList)
		{
			ArrayList arrayList2 = new ArrayList();
			if (item.N_Room != null && item.N_Room.S_Link == null && !item.N_Room.isEntranceHall)
			{
				arrayList2.Add(item.N_Room);
			}
			if (item.E_Room != null && item.E_Room.W_Link == null && !item.E_Room.isEntranceHall)
			{
				arrayList2.Add(item.E_Room);
			}
			if (item.S_Room != null && item.S_Room.N_Link == null && !item.S_Room.isEntranceHall)
			{
				arrayList2.Add(item.S_Room);
			}
			if (item.W_Room != null && item.W_Room.E_Link == null && !item.W_Room.isEntranceHall)
			{
				arrayList2.Add(item.W_Room);
			}
			if (arrayList2.Count > 0)
			{
				Room obj = (Room)arrayList2[RandomSeed.Next(0, arrayList2.Count)];
				if (obj == item.N_Room)
				{
					item.N_Room.S_Link = item;
					item.N_Link = item.N_Room;
					addLink(dir.north, item);
				}
				if (obj == item.E_Room)
				{
					item.E_Room.W_Link = item;
					item.E_Link = item.E_Room;
					addLink(dir.east, item);
				}
				if (obj == item.S_Room)
				{
					item.S_Room.N_Link = item;
					item.S_Link = item.S_Room;
					addLink(dir.south, item);
				}
				if (obj == item.W_Room)
				{
					item.W_Room.E_Link = item;
					item.W_Link = item.W_Room;
					addLink(dir.west, item);
				}
			}
		}
	}

	private void connectIsland()
	{
		Room room = rooms[RandomSeed.Next(0, rooms.Count)];
		if (room.N_Room != null && room.N_Room.region != room.region && !room.isEntranceHall && !room.N_Room.isEntranceHall)
		{
			room.N_Room.S_Link = room;
			room.N_Link = room.N_Room;
			addLink(dir.north, room);
			applyIslands();
		}
		else if (room.E_Room != null && room.E_Room.region != room.region && !room.isEntranceHall && !room.E_Room.isEntranceHall)
		{
			room.E_Room.W_Link = room;
			room.E_Link = room.E_Room;
			addLink(dir.east, room);
			applyIslands();
		}
		else if (room.S_Room != null && room.S_Room.region != room.region && !room.isEntranceHall && !room.S_Room.isEntranceHall)
		{
			room.S_Room.N_Link = room;
			room.S_Link = room.S_Room;
			addLink(dir.south, room);
			applyIslands();
		}
		else if (room.W_Room != null && room.W_Room.region != room.region && !room.isEntranceHall && !room.W_Room.isEntranceHall)
		{
			room.W_Room.E_Link = room;
			room.W_Link = room.W_Room;
			addLink(dir.west, room);
			applyIslands();
		}
	}

	private void resetRegions()
	{
		NumRegions = 0;
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				getRoom(i, j).regionSet = false;
			}
		}
	}

	private void applyIslands()
	{
		resetRegions();
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				Room room = getRoom(i, j);
				if (!room.regionSet)
				{
					Color randomColor = UnityEngine.Random.ColorHSV();
					floodFillColour(room, randomColor);
					NumRegions++;
				}
			}
		}
	}

	private void setRoomsOfInterest()
	{
		List<Room> list = new List<Room>();
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				Room room = getRoom(i, j);
				if (!room.isEntranceHall)
				{
					int num = 0;
					if (room.N_Link != null)
					{
						num++;
					}
					if (room.E_Link != null)
					{
						num++;
					}
					if (room.W_Link != null)
					{
						num++;
					}
					if (room.S_Link != null)
					{
						num++;
					}
					if (num == 1)
					{
						list.Add(room);
					}
				}
			}
		}
		int num2 = NumRoomsOfInterest + 1;
		while (--num2 > 0 && list.Count > 0)
		{
			int index = RandomSeed.Next(0, list.Count);
			list[index].pointOfInterest = true;
			list.RemoveAt(index);
		}
		int num3 = num2 * 5;
		while (--num3 > 0 && num2 > 0)
		{
			int index2 = RandomSeed.Next(0, rooms.Count);
			Room room2 = rooms[index2];
			if (!room2.isHome && !room2.pointOfInterest)
			{
				room2.pointOfInterest = true;
				num2--;
			}
		}
	}

	private Room GetSpecialRoom()
	{
		List<Room> list = new List<Room>();
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				Room room = getRoom(i, j);
				if (!room.isHome && !room.pointOfInterest)
				{
					int num = 0;
					if (room.N_Link != null)
					{
						num++;
					}
					if (room.E_Link != null)
					{
						num++;
					}
					if (room.W_Link != null)
					{
						num++;
					}
					if (room.S_Link != null)
					{
						num++;
					}
					if (num == 1)
					{
						list.Add(room);
					}
				}
			}
		}
		if (list.Count > 0)
		{
			int index = RandomSeed.Next(0, list.Count);
			return list[index];
		}
		int num2 = 500;
		while (--num2 > 0)
		{
			int index2 = RandomSeed.Next(0, rooms.Count);
			Room room2 = rooms[index2];
			if (!room2.isHome && !room2.pointOfInterest)
			{
				return room2;
			}
		}
		return null;
	}

	private Room SetSpecificRoom(bool N, bool E, bool S, bool W, int minX, int minY)
	{
		for (int i = minX; i < width; i++)
		{
			for (int j = minY; j < height; j++)
			{
				Room room = getRoom(i, j);
				if (room.pointOfInterest && room.CurrentSpecificRoom == Room.SpecificRoom.None && (N ? (room.N_Link != null) : (room.N_Link == null)) && (E ? (room.E_Link != null) : (room.E_Link == null)) && (W ? (room.W_Link != null) : (room.W_Link == null)) && (S ? (room.S_Link != null) : (room.S_Link == null)))
				{
					return room;
				}
			}
		}
		for (int k = minX; k < width; k++)
		{
			for (int l = minY; l < height; l++)
			{
				Room room2 = getRoom(k, l);
				if (!room2.pointOfInterest && (N ? (room2.N_Link != null) : (room2.N_Link == null)) && (E ? (room2.E_Link != null) : (room2.E_Link == null)) && (W ? (room2.W_Link != null) : (room2.W_Link == null)) && (S ? (room2.S_Link != null) : (room2.S_Link == null)))
				{
					return room2;
				}
			}
		}
		for (int m = minX; m < width; m++)
		{
			for (int n = minY; n < height; n++)
			{
				Room room3 = getRoom(m, n);
				int num = 0;
				if (room3.N_Link != null)
				{
					num++;
				}
				if (room3.E_Link != null)
				{
					num++;
				}
				if (room3.W_Link != null)
				{
					num++;
				}
				if (room3.S_Link != null)
				{
					num++;
				}
				if (num == 1)
				{
					room3.N_Link = (N ? room3.N_Room : null);
					if (room3.N_Room != null)
					{
						room3.N_Room.S_Link = (N ? room3 : null);
					}
					room3.E_Link = (E ? room3.E_Room : null);
					if (room3.E_Room != null)
					{
						room3.E_Room.W_Link = (E ? room3 : null);
					}
					room3.W_Link = (W ? room3.W_Room : null);
					if (room3.W_Room != null)
					{
						room3.W_Room.E_Link = (W ? room3 : null);
					}
					room3.S_Link = (S ? room3.S_Room : null);
					if (room3.S_Room != null)
					{
						room3.S_Room.N_Link = (S ? room3 : null);
					}
					return room3;
				}
			}
		}
		return null;
	}

	private void floodFillColour(Room r, Color randomColor)
	{
		if (!(r == null) && !r.regionSet)
		{
			r.regionSet = true;
			r.region = NumRegions;
			floodFillColour(r.N_Link, randomColor);
			floodFillColour(r.E_Link, randomColor);
			floodFillColour(r.S_Link, randomColor);
			floodFillColour(r.W_Link, randomColor);
		}
	}

	public static Room getRoom(int _x, int _y)
	{
		foreach (Room room in rooms)
		{
			if (room.x == _x && room.y == _y)
			{
				return room;
			}
		}
		return null;
	}

	private void addLink(dir _dir, Room r)
	{
		if (InstantiatePrefabs)
		{
			Vector3 vector = default(Vector3);
			GameObject gameObject = UnityEngine.Object.Instantiate(link, new Vector3((float)(r.x * roomDist - width) + 0.5f, 0f, (float)(r.y * roomDist - height) + 0.5f) + Vector3.down, Quaternion.identity);
			switch (_dir)
			{
			case dir.north:
				vector = Vector3.forward * roomDist / 2f;
				gameObject.transform.localScale = new Vector3(4f, 1f, roomDist);
				break;
			case dir.east:
				vector = Vector3.right * roomDist / 2f;
				gameObject.transform.localScale = new Vector3(roomDist, 1f, 4f);
				break;
			case dir.south:
				vector = Vector3.back * roomDist / 2f;
				gameObject.transform.localScale = new Vector3(4f, 1f, roomDist);
				break;
			case dir.west:
				vector = Vector3.left * roomDist / 2f;
				gameObject.transform.localScale = new Vector3(roomDist, 1f, 4f);
				break;
			}
			gameObject.transform.position = gameObject.transform.position + vector;
		}
	}
}
