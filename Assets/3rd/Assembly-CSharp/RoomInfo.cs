using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RoomInfo : BaseMonoBehaviour
{
	public AudioClip Music;

	[HideInInspector]
	public string ID;

	private static List<RoomInfo> RoomInfos = new List<RoomInfo>();

	public bool ForceClan;

	public void Init()
	{
		RoomInfos.Add(this);
	}

	private void OnDestroy()
	{
		RoomInfos.Remove(this);
	}

	public static RoomInfo GetByID(string ID)
	{
		foreach (RoomInfo roomInfo in RoomInfos)
		{
			if (roomInfo.ID == ID)
			{
				return roomInfo;
			}
		}
		return null;
	}
}
