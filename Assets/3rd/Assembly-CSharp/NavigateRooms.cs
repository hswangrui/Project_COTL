using UnityEngine;
using UnityEngine.UI;

public class NavigateRooms : BaseMonoBehaviour
{
	public GameObject North;

	public GameObject East;

	public GameObject South;

	public GameObject West;

	public GameObject ground;

	public Text text;

	public static int CurrentX = -1;

	public static int CurrentY = -1;

	public static int PrevCurrentX = -1;

	public static int PrevCurrentY = -1;

	private GameObject player;

	public static Room r;

	private bool init;

	public DungeonDecorator decorator;

	private static NavigateRooms instance;

	public static NavigateRooms GetInstance()
	{
		return instance;
	}

	private void Start()
	{
		instance = this;
	}

	private void Update()
	{
		if (!init)
		{
			if (CurrentX == -1 && CurrentY == -1)
			{
				CurrentX = WorldGen.startRoom.x;
				CurrentY = WorldGen.startRoom.y;
				CameraFollowTarget.Instance.distance = 30f;
			}
			r = WorldGen.getRoom(CurrentX, CurrentY);
			r.visited = true;
			if (r.N_Link == null)
			{
				North.SetActive(false);
			}
			if (r.E_Link == null)
			{
				East.SetActive(false);
			}
			if (r.S_Link == null)
			{
				South.SetActive(false);
			}
			if (r.W_Link == null)
			{
				West.SetActive(false);
			}
			decorator.Decorate(r.Width * 4, r.Height * 4, r);
			decorator.AddBlockedAreas(r);
			decorator.SetDoors(r, North, East, South, West);
			decorator.AddOuterWalls(r);
			decorator.PlaceWallTiles(r);
			decorator.PlaceStructures(r);
			decorator.PlaceEnemies(r);
			decorator.PlaceWildLife(r);
			GameManager.RecalculatePaths();
			player = Object.Instantiate(Resources.Load("Prefabs/Units/Player") as GameObject, GameObject.FindGameObjectWithTag("Unit Layer").transform, true);
			if (PrevCurrentX == -1 && PrevCurrentY == -1)
			{
				player.transform.position = new Vector3(0f, 0f, 0f);
			}
			else if (PrevCurrentX < CurrentX)
			{
				player.transform.position = West.transform.position + Vector3.right * 0.2f;
				player.GetComponent<StateMachine>().facingAngle = 0f;
			}
			else if (PrevCurrentX > CurrentX)
			{
				player.transform.position = East.transform.position + Vector3.left * 0.2f;
				player.GetComponent<StateMachine>().facingAngle = 180f;
			}
			else if (PrevCurrentY > CurrentY)
			{
				player.transform.position = North.transform.position + Vector3.down * 0.2f;
				player.GetComponent<StateMachine>().facingAngle = 270f;
			}
			else if (PrevCurrentY < CurrentY)
			{
				player.GetComponent<StateMachine>().facingAngle = 90f;
				player.transform.position = South.transform.position + Vector3.up * 0.2f;
			}
			PrevCurrentX = CurrentX;
			PrevCurrentY = CurrentY;
			CameraFollowTarget.Instance.SnapTo(player.transform.position);
			text.text = "X: " + CurrentX + "   Y: " + CurrentY + (r.isHome ? " HOME" : "") + (r.isEntranceHall ? " ENTRANCE HALL" : "") + (r.pointOfInterest ? " POINT OF INTEREST" : "");
			init = true;
		}
		if (West.activeSelf && player != null && player.transform.position.x < West.transform.position.x)
		{
			CurrentX--;
			player = null;
		}
		if (East.activeSelf && player != null && player.transform.position.x > East.transform.position.x)
		{
			CurrentX++;
			player = null;
		}
		if (South.activeSelf && player != null && player.transform.position.y < South.transform.position.y)
		{
			CurrentY--;
			player = null;
		}
		if (North.activeSelf && player != null && player.transform.position.y > North.transform.position.y)
		{
			CurrentY++;
			player = null;
		}
	}

	public bool WithinRoom(Vector3 position)
	{
		if (position.x < West.transform.position.x)
		{
			return false;
		}
		if (position.x > East.transform.position.x)
		{
			return false;
		}
		if (position.y > North.transform.position.y)
		{
			return false;
		}
		if (position.y < South.transform.position.y)
		{
			return false;
		}
		return true;
	}
}
