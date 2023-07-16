using UnityEngine;

public class DoorIsland : BaseMonoBehaviour
{
	public bool CheckOnEnable;

	public bool North;

	public bool East;

	public bool South;

	public bool West;

	private void OnEnable()
	{
		if (CheckOnEnable)
		{
			Init();
		}
		else if (RoomManager.Instance != null)
		{
			RoomManager.Instance.OnInitEnemies += Init;
		}
		else
		{
			Init();
		}
	}

	private void OnDisable()
	{
		if (!CheckOnEnable)
		{
			RoomManager.Instance.OnInitEnemies -= Init;
		}
	}

	private void Init()
	{
		Debug.Log(string.Concat("Init N: ", RoomManager.r.N_Link, "   E: ", RoomManager.r.E_Link, "  S:", RoomManager.r.S_Link, "  W:", RoomManager.r.W_Link));
		if (North && RoomManager.r.N_Link != null)
		{
			base.gameObject.SetActive(true);
		}
		else if (East && RoomManager.r.E_Link != null)
		{
			base.gameObject.SetActive(true);
		}
		else if (South && RoomManager.r.S_Link != null)
		{
			base.gameObject.SetActive(true);
		}
		else if (West && RoomManager.r.W_Link != null)
		{
			base.gameObject.SetActive(true);
		}
		else
		{
			base.gameObject.SetActive(false);
		}
	}
}
