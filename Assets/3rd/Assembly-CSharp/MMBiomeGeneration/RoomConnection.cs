using MMRoomGeneration;

namespace MMBiomeGeneration
{
	public class RoomConnection
	{
		public BiomeRoom Room;

		public GenerateRoom.ConnectionTypes ConnectionType;

		public bool Connected
		{
			get
			{
				if (ConnectionType != 0 && ConnectionType != GenerateRoom.ConnectionTypes.Exit && ConnectionType != GenerateRoom.ConnectionTypes.Entrance)
				{
					return ConnectionType != GenerateRoom.ConnectionTypes.DoorRoom;
				}
				return false;
			}
		}

		public RoomConnection(BiomeRoom Room)
		{
			this.Room = Room;
		}

		public RoomConnection(GenerateRoom.ConnectionTypes ConnectionType)
		{
			Room = null;
			this.ConnectionType = ConnectionType;
		}

		public void SetConnection(GenerateRoom.ConnectionTypes ConnectionType)
		{
			this.ConnectionType = ConnectionType;
		}

		public void SetConnectionAndRoom(BiomeRoom Room, GenerateRoom.ConnectionTypes ConnectionType)
		{
			this.Room = Room;
			this.ConnectionType = ConnectionType;
		}
	}
}
