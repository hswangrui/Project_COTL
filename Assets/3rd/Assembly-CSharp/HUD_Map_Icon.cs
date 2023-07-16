using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD_Map_Icon : BaseMonoBehaviour
{
	public enum RoomType
	{
		HOME,
		ENTRANCE_HALLWAY,
		ROOM,
		DOOR,
		POINT_OF_INTEREST
	}

	public Sprite Home;

	public Sprite EntranceHallway;

	public Sprite RoomSprite;

	public Sprite Door;

	public Sprite PointOfInterest;

	public Image image;

	private float Delay;

	public Room Room;

	private static List<HUD_Map_Icon> Icons = new List<HUD_Map_Icon>();

	private float scale;

	private float scaleSpeed;

	private float TargetScale = 1f;

	public void SetImage(RoomType type, float Delay, Room Room)
	{
		this.Delay = Delay;
		switch (type)
		{
		case RoomType.HOME:
			image.sprite = Home;
			break;
		case RoomType.ENTRANCE_HALLWAY:
			image.sprite = EntranceHallway;
			break;
		case RoomType.ROOM:
			image.sprite = RoomSprite;
			break;
		case RoomType.DOOR:
			image.sprite = Door;
			break;
		case RoomType.POINT_OF_INTEREST:
			image.sprite = PointOfInterest;
			break;
		}
		image.SetNativeSize();
		this.Room = Room;
	}

	public static HUD_Map_Icon GetIconByRoom(Room room)
	{
		foreach (HUD_Map_Icon icon in Icons)
		{
			if (icon.Room == room)
			{
				return icon;
			}
		}
		return null;
	}

	private void OnEnable()
	{
		base.transform.localScale = Vector3.zero;
		Icons.Add(this);
	}

	private void OnDisable()
	{
		Room = null;
		Icons.Remove(this);
	}

	private void Update()
	{
		if (!((Delay -= Time.deltaTime) > 0f))
		{
			scaleSpeed += (TargetScale - scale) * 0.3f;
			scale += (scaleSpeed *= 0.7f);
			base.transform.localScale = new Vector3(scale, scale, 1f);
		}
	}
}
