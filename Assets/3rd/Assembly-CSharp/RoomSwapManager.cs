using MMRoomGeneration;
using MMTools;
using UnityEngine;
using UnityEngine.Events;

public class RoomSwapManager : BaseMonoBehaviour
{
	public static bool Testing = true;

	public static bool EnterTemple = false;

	public GameObject ChurchEntrance;

	public GameObject RunResults;

	public GameObject Church;

	public bool ChurchUpdatePathfindingAndCollisions;

	public GameObject Room;

	public GameObject Room_1;

	public GameObject Room_2;

	public bool RoomUpdatePathfindingAndCollisions;

	public static bool WalkedBack = false;

	public int TransitionInRoomId = -1;

	public int TransitionOutRoomId = -1;

	public string SoundParam = "";

	public string AtmosSoundParam = "";

	public UnityEvent CallbackOnRoom;

	public UnityEvent CallbackOnChurch;

	public bool ControlWeather;

	private void Awake()
	{
		if (Church != null && !Church.activeSelf)
		{
			Church.SetActive(true);
		}
	}

	public void ToggleChurch()
	{
		MMTransition.ResumePlay();
		if (Church.activeSelf)
		{
			ActivateRoom();
		}
		else
		{
			ActivateChurch();
		}
	}

	private void ActivateChurch()
	{
		if (ControlWeather)
		{
			WeatherSystemController.Instance.EnteredBuilding();
		}
		AudioManager.Instance.ToggleFilter("inside", true);
		if (AtmosSoundParam != "")
		{
			AudioManager.Instance.AdjustAtmosParameter(AtmosSoundParam, 1f);
		}
		if (SoundParam != "")
		{
			Debug.Log("Set music ID");
			AudioManager.Instance.SetMusicRoomID(TransitionInRoomId, SoundParam);
		}
		AudioManager.Instance.PlayOneShot("event:/enter_leave_buildings/enter_building", PlayerFarming.Instance.gameObject);
		Church.SetActive(true);
		if (ChurchUpdatePathfindingAndCollisions)
		{
			GenerateRoom.Instance.SetColliderAndUpdatePathfinding();
		}
		Room.gameObject.SetActive(false);
		if (Room_1 != null)
		{
			Room_1.gameObject.SetActive(false);
		}
		if (Room_2 != null)
		{
			Room_2.gameObject.SetActive(false);
		}
		UnityEvent callbackOnChurch = CallbackOnChurch;
		if (callbackOnChurch != null)
		{
			callbackOnChurch.Invoke();
		}
	}

	private void OnDisable()
	{
		AudioManager.Instance.ToggleFilter("inside", false);
	}

	private void ActivateRoom()
	{
		if (ControlWeather)
		{
			WeatherSystemController.Instance.ExitedBuilding();
		}
		AudioManager.Instance.ToggleFilter("inside", false);
		if (AtmosSoundParam != "")
		{
			AudioManager.Instance.AdjustAtmosParameter(AtmosSoundParam, 0f);
		}
		if (SoundParam != "")
		{
			Debug.Log("Set music ID");
			AudioManager.Instance.SetMusicRoomID(TransitionOutRoomId, SoundParam);
		}
		AudioManager.Instance.PlayOneShot("event:/enter_leave_buildings/leave_building", PlayerFarming.Instance.gameObject);
		Room.gameObject.SetActive(true);
		if (Room_1 != null)
		{
			Room_1.gameObject.SetActive(true);
		}
		if (Room_2 != null)
		{
			Room_2.gameObject.SetActive(true);
		}
		if (RoomUpdatePathfindingAndCollisions)
		{
			GenerateRoom.Instance.SetColliderAndUpdatePathfinding();
		}
		Church.SetActive(false);
		UnityEvent callbackOnRoom = CallbackOnRoom;
		if (callbackOnRoom != null)
		{
			callbackOnRoom.Invoke();
		}
	}
}
