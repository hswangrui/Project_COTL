using System.Collections.Generic;
using UnityEngine;

public class Room : BaseMonoBehaviour
{
	public enum SpecificRoom
	{
		None,
		RandomSpecialinterest,
		CultDoor,
		Village,
		KeyShrine_1,
		KeyShrine_2,
		KeyShrine_3,
		Follower_Rescue,
		Tutorial_0,
		Tutorial_1,
		Tutorial_2,
		Goat_Spider,
		Goat_Tentacle
	}

	public int IslandChoice = -1;

	public int EnemyChoice = -1;

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

	public bool isBase;

	public bool isHome;

	public bool isEntranceHall;

	public bool pointOfInterest;

	public SpecificRoom CurrentSpecificRoom;

	public int[,] RoomGrid;

	public int[,] StructuresOld;

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

	public bool activeTeleporter;

	public string PrefabDir;

	public string prefabName;

	public bool ResourcesPlaced;

	public List<StructuresData> Structures;

	public List<Vector3> RecruitPositions;

	public static int PointOfInterestCount = -1;

	public int Seed;

	private static List<int> Combatrooms = new List<int>();

	public void NewPointOfInterestRoom()
	{
		switch (CurrentSpecificRoom)
		{
		case SpecificRoom.CultDoor:
			prefabName = "Special/Goat/Cult Door/Cult Door";
			break;
		case SpecificRoom.Village:
			prefabName = "Special/Villages/Horse Village";
			break;
		case SpecificRoom.KeyShrine_1:
			prefabName = "Special/Key Shrine/Key Shrine 1";
			break;
		case SpecificRoom.KeyShrine_2:
			prefabName = "Special/Key Shrine/Key Shrine 2";
			break;
		case SpecificRoom.KeyShrine_3:
			prefabName = "Special/Key Shrine/Key Shrine 3";
			break;
		case SpecificRoom.Follower_Rescue:
			prefabName = "Special/Follower Rescue/Follower Rescue";
			break;
		case SpecificRoom.Tutorial_0:
			prefabName = "Intro/Intro 1";
			break;
		case SpecificRoom.Tutorial_1:
			prefabName = "Intro/Intro 2";
			break;
		case SpecificRoom.Tutorial_2:
			prefabName = "Intro/Intro 3";
			break;
		case SpecificRoom.Goat_Spider:
			prefabName = "Special/Goat/Spider/Spider Room";
			break;
		case SpecificRoom.Goat_Tentacle:
			prefabName = "Special/Goat/Tentacle/Tentacle Room";
			break;
		default:
			PointOfInterestCount++;
			if (PointOfInterestCount > 1)
			{
				PointOfInterestCount = 0;
			}
			switch (PointOfInterestCount)
			{
			case 0:
				prefabName = "Special/Goat/First Meeting/Room 0";
				break;
			case 1:
				prefabName = "Special/Healing Room/Room_HealingRoom";
				break;
			}
			break;
		}
		PrefabDir = "_Rooms/Production/" + prefabName;
		cleared = true;
	}

	public void NewBaseRoom()
	{
		isBase = true;
		activeTeleporter = true;
		prefabName = "Entrance/Entrance Room";
		PrefabDir = "_Rooms/Production/" + prefabName;
		cleared = true;
		visited = true;
	}

	public void NewHomeRoom()
	{
		activeTeleporter = true;
		prefabName = "Entrance/Entrance Room";
		PrefabDir = "_Rooms/Production/" + prefabName;
		cleared = true;
	}

	public void NewRoom(int Seed)
	{
		cleared = true;
		prefabName = "Generated/Generated Room";
		PrefabDir = "_Rooms/Production/" + prefabName;
		this.Seed = Seed;
	}

	public void Clear()
	{
		Structures.Clear();
	}
}
