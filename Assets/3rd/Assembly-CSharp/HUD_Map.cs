using System.Collections.Generic;
using UnityEngine;

public class HUD_Map : BaseMonoBehaviour
{
	public Transform IconParent;

	public GameObject TargetRoom;

	public GameObject TargetRoomPointer;

	private HUD_Map_Icon CurrentRoom;

	private HUD_Map_Icon CurrentTarget;

	public GameObject RoomIcon;

	public GameObject DoorIcon;

	public GameObject PlayerIcon;

	private float Timer;

	private List<GameObject> icons;

	public bool OnlyShowVisited;

	private void OnEnable()
	{
		icons = new List<GameObject>();
		if (WorldGen.rooms == null)
		{
			return;
		}
		foreach (Room room in WorldGen.rooms)
		{
			HUD_Map_Icon component = Object.Instantiate(RoomIcon, IconParent, true).GetComponent<HUD_Map_Icon>();
			component.gameObject.GetComponent<RectTransform>().localPosition = RoomPosition(room.x, room.y);
			float delay = Vector3.Distance(RoomPosition(RoomManager.CurrentX, RoomManager.CurrentY), RoomPosition(room.x, room.y));
			if (room.isHome)
			{
				component.SetImage(HUD_Map_Icon.RoomType.HOME, delay, room);
			}
			else if (room.isEntranceHall)
			{
				component.SetImage(HUD_Map_Icon.RoomType.ENTRANCE_HALLWAY, delay, room);
			}
			else if (room.pointOfInterest)
			{
				component.SetImage(HUD_Map_Icon.RoomType.POINT_OF_INTEREST, delay, room);
			}
			else
			{
				component.SetImage(HUD_Map_Icon.RoomType.ROOM, delay, room);
			}
			if (OnlyShowVisited && !room.visited)
			{
				component.gameObject.SetActive(false);
			}
			icons.Add(component.gameObject);
		}
		foreach (Room room2 in WorldGen.rooms)
		{
			if (room2.N_Link != null)
			{
				HUD_Map_Icon component2 = Object.Instantiate(DoorIcon, base.transform, true).GetComponent<HUD_Map_Icon>();
				component2.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(room2.x * 205 - WorldGen.WIDTH * 100, room2.y * 155 - WorldGen.HEIGHT * 75 + 80);
				float delay2 = Vector2.Distance(new Vector2(NavigateRooms.CurrentX, NavigateRooms.CurrentY), new Vector2(room2.x, room2.y)) * 0.05f;
				component2.SetImage(HUD_Map_Icon.RoomType.DOOR, delay2, null);
				icons.Add(component2.gameObject);
				if (OnlyShowVisited && !room2.visited)
				{
					component2.gameObject.SetActive(false);
				}
			}
			if (room2.E_Link != null)
			{
				HUD_Map_Icon component3 = Object.Instantiate(DoorIcon, base.transform, true).GetComponent<HUD_Map_Icon>();
				component3.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(room2.x * 205 - WorldGen.WIDTH * 100 + 100, room2.y * 155 - WorldGen.HEIGHT * 75);
				float delay3 = Vector2.Distance(new Vector2(NavigateRooms.CurrentX, NavigateRooms.CurrentY), new Vector2(room2.x, room2.y)) * 0.05f;
				component3.SetImage(HUD_Map_Icon.RoomType.DOOR, delay3, null);
				icons.Add(component3.gameObject);
				if (OnlyShowVisited && !room2.visited)
				{
					component3.gameObject.SetActive(false);
				}
			}
			if (room2.S_Link != null)
			{
				HUD_Map_Icon component4 = Object.Instantiate(DoorIcon, base.transform, true).GetComponent<HUD_Map_Icon>();
				component4.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(room2.x * 205 - WorldGen.WIDTH * 100, room2.y * 155 - WorldGen.HEIGHT * 75 - 80);
				float delay4 = Vector2.Distance(new Vector2(NavigateRooms.CurrentX, NavigateRooms.CurrentY), new Vector2(room2.x, room2.y)) * 0.05f;
				component4.SetImage(HUD_Map_Icon.RoomType.DOOR, delay4, null);
				icons.Add(component4.gameObject);
				if (OnlyShowVisited && !room2.visited)
				{
					component4.gameObject.SetActive(false);
				}
			}
			if (room2.W_Link != null)
			{
				HUD_Map_Icon component5 = Object.Instantiate(DoorIcon, base.transform, true).GetComponent<HUD_Map_Icon>();
				component5.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(room2.x * 205 - WorldGen.WIDTH * 100 - 100, room2.y * 155 - WorldGen.HEIGHT * 75);
				float delay5 = Vector2.Distance(new Vector2(NavigateRooms.CurrentX, NavigateRooms.CurrentY), new Vector2(room2.x, room2.y)) * 0.05f;
				component5.SetImage(HUD_Map_Icon.RoomType.DOOR, delay5, null);
				icons.Add(component5.gameObject);
				if (OnlyShowVisited && !room2.visited)
				{
					component5.gameObject.SetActive(false);
				}
			}
		}
		GameObject gameObject = Object.Instantiate(PlayerIcon, base.transform, true);
		gameObject.GetComponent<RectTransform>().localPosition = new Vector3(RoomManager.CurrentX * 205 - WorldGen.WIDTH * 100, RoomManager.CurrentY * 155 - WorldGen.HEIGHT * 75);
		icons.Add(gameObject.gameObject);
		CurrentRoom = HUD_Map_Icon.GetIconByRoom(RoomManager.r);
		TargetRoom.transform.localPosition = RoomPosition(CurrentRoom.Room.x, CurrentRoom.Room.y);
	}

	private Vector3 RoomPosition(int x, int y)
	{
		return new Vector3(x * 205 - WorldGen.WIDTH * 100, y * 155 - WorldGen.HEIGHT * 75);
	}

	private void OnDisable()
	{
		foreach (GameObject icon in icons)
		{
			Object.Destroy(icon.gameObject);
		}
		icons.Clear();
		icons = null;
	}

	private void Update()
	{
		if (InputManager.Gameplay.GetAttackButtonUp())
		{
			base.gameObject.SetActive(false);
		}
	}
}
