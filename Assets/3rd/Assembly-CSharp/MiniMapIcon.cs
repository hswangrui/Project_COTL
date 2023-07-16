using System;
using MMBiomeGeneration;
using MMRoomGeneration;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapIcon : BaseMonoBehaviour
{
	public Text text;

	public RectTransform RT;

	public GameObject QuestionMark;

	public RectTransform IconObject;

	public GameObject North;

	public GameObject East;

	public GameObject South;

	public GameObject West;

	public Image outlineImage;

	public Image image;

	public int X;

	public int Y;

	public RectTransform rectTransform;

	public BiomeRoom room;

	public Image Icon;

	public GameObject IconContainer;

	public bool HasTeleporter
	{
		get
		{
			return false;
		}
	}

	public void ShowIcon(float opacity = 1f)
	{
		SetIcons();
		IconObject.gameObject.SetActive(true);
		QuestionMark.SetActive(false);
		Color color = image.color;
		color.a = opacity;
		image.color = color;
	}

	public void SetSelfToQuestionMark()
	{
		IconObject.gameObject.SetActive(false);
		if (room.IsRespawnRoom)
		{
			return;
		}
		try
		{
			if (room.N_Room != null && room.N_Room.Room != null && room.N_Room.Connected && room.N_Room.Room.Visited && room.N_Room.Room.S_Room.Room == room && !room.Visited)
			{
				QuestionMark.SetActive(true);
			}
			if (room.E_Room != null && room.E_Room.Room != null && room.E_Room.Connected && room.E_Room.Room.Visited && room.E_Room.Room.W_Room.Room == room && !room.Visited)
			{
				QuestionMark.SetActive(true);
			}
			if (room.S_Room != null && room.S_Room.Room != null && room.S_Room.Connected && room.S_Room.Room.Visited && room.S_Room.Room.N_Room.Room == room && !room.Visited)
			{
				QuestionMark.SetActive(true);
			}
			if (room.W_Room != null && room.W_Room.Room != null && room.W_Room.Connected && room.W_Room.Room.Visited && room.W_Room.Room.E_Room.Room == room && !room.Visited)
			{
				QuestionMark.SetActive(true);
			}
		}
		catch (Exception)
		{
			Debug.Log("t");
		}
	}

	public Vector2 Init(BiomeRoom room, Sprite icon, float Scale, Vector3 Position)
	{
		this.room = room;
		X = room.x;
		Y = room.y;
		SetIcons();
		outlineImage.enabled = false;
		RT.localScale = new Vector3(Scale, Scale);
		image.sprite = icon;
		RT.localPosition = Position;
		rectTransform = RT;
		return RT.rect.size * RT.localScale;
	}

	private void SetIcons()
	{
		text.text = "";
		if (room.generateRoom.MapIcon != null)
		{
			Icon.gameObject.SetActive(true);
			Icon.sprite = room.generateRoom.MapIcon;
		}
		else
		{
			Icon.gameObject.SetActive(false);
		}
		if (!room.IsRespawnRoom)
		{
			if (room.N_Room != null && !room.N_Room.Connected)
			{
				North.SetActive(false);
			}
			if (room.N_Room != null && room.N_Room.ConnectionType == GenerateRoom.ConnectionTypes.NextLayer)
			{
				North.SetActive(false);
			}
			if (room.E_Room != null && !room.E_Room.Connected)
			{
				East.SetActive(false);
			}
			if (room.S_Room != null && !room.S_Room.Connected)
			{
				South.SetActive(false);
			}
			if (room.W_Room != null && !room.W_Room.Connected)
			{
				West.SetActive(false);
			}
		}
	}

	public void ShowTeleporter()
	{
	}
}
