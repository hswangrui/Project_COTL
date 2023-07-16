using System;
using System.Collections.Generic;
using MMRoomGeneration;
using UnityEngine;

namespace MMBiomeGeneration
{
	[Serializable]
	public class BiomeRoom
	{
		public enum Direction
		{
			None = 0,
			North = 1,
			East = 2,
			South = 3,
			West = 4
		}

		public static List<BiomeRoom> BiomeRooms = new List<BiomeRoom>();

		public Direction CriticalPathDirection;

		public bool IsCustom;

		public bool HasWeapon;

		public bool RegionSet;

		public int Region;

		public int Distance = -1;

		public int x;

		public int y;

		public int Seed;

		public System.Random RandomSeed;

		public RoomConnection N_Room;

		public RoomConnection E_Room;

		public RoomConnection S_Room;

		public RoomConnection W_Room;

		public bool Active;

		public bool Completed;

		public bool Discovered;

		public bool Visited;

		public bool Generated;

		public GameObject GameObject;

		public string GameObjectPath;

		public bool IsRespawnRoom;

		public bool IsDeathCatRoom;

		public bool IsBoss;

		private GenerateRoom _generateRoom;

		public List<Vector2Int> EmptyNeighbourPositions
		{
			get
			{
				List<Vector2Int> list = new List<Vector2Int>();
				if (GetRoom(x, y - 1) == null)
				{
					list.Add(new Vector2Int(x, y - 1));
				}
				if (GetRoom(x, y + 1) == null)
				{
					list.Add(new Vector2Int(x, y + 1));
				}
				if (GetRoom(x - 1, y) == null)
				{
					list.Add(new Vector2Int(x - 1, y));
				}
				if (GetRoom(x + 1, y) == null)
				{
					list.Add(new Vector2Int(x + 1, y));
				}
				return list;
			}
		}

		public List<Vector2Int> EmptyNeighboursWithEmptyNeighbours
		{
			get
			{
				List<Vector2Int> list = new List<Vector2Int>();
				if (GetRoom(x, y - 1) == null && EmptyNeighboursOfPosition(x, y - 1, IgnoreSouth: true).Count > 0)
				{
					list.Add(new Vector2Int(x, y - 1));
				}
				if (GetRoom(x, y + 1) == null && EmptyNeighboursOfPosition(x, y + 1, IgnoreSouth: true).Count > 0)
				{
					list.Add(new Vector2Int(x, y + 1));
				}
				if (GetRoom(x + 1, y) == null && EmptyNeighboursOfPosition(x + 1, y, IgnoreSouth: true).Count > 0)
				{
					list.Add(new Vector2Int(x + 1, y));
				}
				if (GetRoom(x - 1, y) == null && EmptyNeighboursOfPosition(x - 1, y, IgnoreSouth: true).Count > 0)
				{
					list.Add(new Vector2Int(x - 1, y));
				}
				return list;
			}
		}

		public List<Vector2Int> EmptyNeighbourPositionsIgnoreSouth
		{
			get
			{
				List<Vector2Int> list = new List<Vector2Int>();
				if (GetRoom(x, y + 1) == null)
				{
					list.Add(new Vector2Int(x, y + 1));
				}
				if (GetRoom(x - 1, y) == null)
				{
					list.Add(new Vector2Int(x - 1, y));
				}
				if (GetRoom(x + 1, y) == null)
				{
					list.Add(new Vector2Int(x + 1, y));
				}
				return list;
			}
		}

		public bool IsNorthEmpty => GetRoom(x, y + 1) == null;

		public int NumConnections
		{
			get
			{
				int num = 0;
				if (N_Room.Connected)
				{
					num++;
				}
				if (E_Room.Connected)
				{
					num++;
				}
				if (S_Room.Connected)
				{
					num++;
				}
				if (W_Room.Connected)
				{
					num++;
				}
				return num;
			}
		}

		public GenerateRoom generateRoom
		{
			get
			{
				if (_generateRoom == null && GameObject != null)
				{
					_generateRoom = GameObject.GetComponent<GenerateRoom>();
				}
				return _generateRoom;
			}
		}

		public bool DoAnyOfMyEmptyNeighboursHaveEmptyNeighbours()
		{
			foreach (Vector2Int item in EmptyNeighbourPositionsIgnoreSouth)
			{
				if (EmptyNeighboursOfPosition(item.x, item.y, IgnoreSouth: true).Count > 0)
				{
					return true;
				}
			}
			return false;
		}

		public static List<Vector2Int> EmptyNeighboursOfPosition(int x, int y, bool IgnoreSouth)
		{
			List<Vector2Int> list = new List<Vector2Int>();
			if (!IgnoreSouth && GetRoom(x, y - 1) == null)
			{
				list.Add(new Vector2Int(x, y - 1));
			}
			if (GetRoom(x, y + 1) == null)
			{
				list.Add(new Vector2Int(x, y + 1));
			}
			if (GetRoom(x - 1, y) == null)
			{
				list.Add(new Vector2Int(x - 1, y));
			}
			if (GetRoom(x + 1, y) == null)
			{
				list.Add(new Vector2Int(x + 1, y));
			}
			return list;
		}

		public BiomeRoom(int x, int y, int Seed, GameObject GameObject)
		{
			this.x = x;
			this.y = y;
			this.Seed = Seed;
			this.GameObject = GameObject;
			BiomeRooms.Add(this);
			RandomSeed = new System.Random(Seed);
		}

		public void Activate(BiomeRoom PrevRoom, bool ReuseGeneratorRoom)
		{
			if (PrevRoom != null && PrevRoom.GameObject != GameObject)
			{
				PrevRoom.GameObject.SetActive(value: false);
			}
			if (!GameObject.activeSelf)
			{
				GameObject.SetActive(value: true);
			}
			if (ReuseGeneratorRoom)
			{
				if (generateRoom != null)
				{
					generateRoom.Generate(Seed, N_Room.ConnectionType, E_Room.ConnectionType, S_Room.ConnectionType, W_Room.ConnectionType);
				}
			}
			else
			{
				if (generateRoom != null)
				{
					if (!Generated)
					{
						generateRoom.Generate(Seed, N_Room.ConnectionType, E_Room.ConnectionType, S_Room.ConnectionType, W_Room.ConnectionType);
					}
					else
					{
						generateRoom.GeneratedDecorations = true;
						generateRoom.SetColliderAndUpdatePathfinding();
					}
				}
				Generated = true;
			}
			Discovered = true;
			Visited = true;
		}

		public void Clear()
		{
			BiomeRooms.Remove(this);
			N_Room = (E_Room = (S_Room = (W_Room = null)));
		}

		public static BiomeRoom GetRoom(int x, int y)
		{
			foreach (BiomeRoom biomeRoom in BiomeRooms)
			{
				if (biomeRoom.x == x && biomeRoom.y == y)
				{
					return biomeRoom;
				}
			}
			return null;
		}

		public RoomConnection GetOppositeConnection(RoomConnection RoomConnection)
		{
			if (RoomConnection == N_Room)
			{
				return N_Room.Room.S_Room;
			}
			if (RoomConnection == E_Room)
			{
				return E_Room.Room.W_Room;
			}
			if (RoomConnection == S_Room)
			{
				return S_Room.Room.N_Room;
			}
			if (RoomConnection == W_Room)
			{
				return W_Room.Room.E_Room;
			}
			return null;
		}
	}
}
