using System.Collections;
using FMOD.Studio;
using FMODUnity;
using I2.Loc;
using MMBiomeGeneration;
using MMRoomGeneration;
using MMTools;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;

public class HubManager : BaseMonoBehaviour
{
	public HubLocationManager HubLocationManager;

	public GenerateRoom Room;

	[TermsPopup("")]
	public string DisplayName;

	public bool GenerateOnLoad;

	public bool InitiateDoors;

	public bool DoFirstArrivalRoutine;

	public static HubManager Instance;

	public int hubMusicParameter = -1;

	[EventRef]
	public string hubMusicPath;

	[EventRef]
	public string hubAtmosPath;

	public bool ToggleInside;

	private EventInstance hubAtmosInstance;

	public Door North;

	public Door East;

	public Door South;

	public Door West;

	private GameObject Player;

	private StateMachine PlayerState;

	private void OnEnable()
	{
		Instance = this;
	}

	private void Start()
	{
		StartCoroutine(PlaceAndPositionPlayer());
	}

	private void InitSpriteShapes()
	{
		SpriteShapeRenderer[] obj = (SpriteShapeRenderer[])Object.FindObjectsOfType(typeof(SpriteShapeRenderer));
		CommandBuffer commandBuffer = new CommandBuffer();
		commandBuffer.GetTemporaryRT(0, 256, 256, 0);
		commandBuffer.SetRenderTarget(0);
		SpriteShapeRenderer[] array = obj;
		foreach (SpriteShapeRenderer spriteShapeRenderer in array)
		{
			SpriteShapeController component = spriteShapeRenderer.gameObject.GetComponent<SpriteShapeController>();
			if (spriteShapeRenderer != null && component != null && !spriteShapeRenderer.isVisible)
			{
				component.BakeMesh();
				commandBuffer.DrawRenderer(spriteShapeRenderer, spriteShapeRenderer.sharedMaterial);
			}
		}
		commandBuffer.ReleaseTemporaryRT(0);
		Graphics.ExecuteCommandBuffer(commandBuffer);
	}

	private void InitDoors()
	{
		GameObject obj = GameObject.FindGameObjectWithTag("North Door");
		North = (((object)obj != null) ? obj.GetComponent<Door>() : null);
		GameObject obj2 = GameObject.FindGameObjectWithTag("East Door");
		East = (((object)obj2 != null) ? obj2.GetComponent<Door>() : null);
		GameObject obj3 = GameObject.FindGameObjectWithTag("South Door");
		South = (((object)obj3 != null) ? obj3.GetComponent<Door>() : null);
		GameObject obj4 = GameObject.FindGameObjectWithTag("West Door");
		West = (((object)obj4 != null) ? obj4.GetComponent<Door>() : null);
		if (Room.North != 0)
		{
			Door north = North;
			if ((object)north != null)
			{
				north.Init(Room.North);
			}
		}
		if (Room.East != 0)
		{
			Door east = East;
			if ((object)east != null)
			{
				east.Init(Room.East);
			}
		}
		if (Room.South != 0)
		{
			Door south = South;
			if ((object)south != null)
			{
				south.Init(Room.South);
			}
		}
		if (Room.West != 0)
		{
			Door west = West;
			if ((object)west != null)
			{
				west.Init(Room.West);
			}
		}
	}

	private IEnumerator PlaceAndPositionPlayer()
	{
		while (LocationManager.GetLocationState(HubLocationManager.Location) != LocationState.Active)
		{
			yield return null;
		}
		Room.SetColliderAndUpdatePathfinding();
		if (BiomeGenerator.Instance == null)
		{
			AudioManager.Instance.PlayMusic(hubMusicPath);
			AudioManager.Instance.PlayAtmos(hubAtmosPath);
			AudioManager.Instance.SetMusicCombatState(false);
			if (hubMusicParameter == -1)
			{
				AudioManager.Instance.SetMusicRoomID(Room.roomMusicID);
			}
			else
			{
				AudioManager.Instance.SetMusicRoomID(hubMusicParameter, "shore_id");
			}
			if (ToggleInside)
			{
				AudioManager.Instance.ToggleFilter("inside", true);
			}
		}
		Player = HubLocationManager.PlacePlayer();
		GameManager.GetInstance().CameraSnapToPosition(PlayerFarming.Instance.CameraBone.transform.position);
		yield return new WaitForEndOfFrame();
		if (GenerateOnLoad)
		{
			InitDoors();
		}
		else if (InitiateDoors)
		{
			InitDoors();
		}
		if (DoFirstArrivalRoutine)
		{
			GameManager.GetInstance().OnConversationNew(true, true);
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 6f);
			GameManager.GetInstance().CameraSetZoom(6f);
			GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0f, -1f));
			Player.transform.position = Door.GetEntranceDoor().PlayerPosition.position;
			PlayerState = Player.GetComponent<StateMachine>();
			StartCoroutine(DelayPlayerGoToAndStop());
		}
		MMTransition.ResumePlay();
		SimulationManager.UnPause();
		yield return new WaitForEndOfFrame();
		HUD_Manager.Instance.Hidden = false;
		HUD_Manager.Instance.Hide(true, 0, true);
	}

	private IEnumerator DelayPlayerGoToAndStop()
	{
		yield return new WaitForSeconds(0.5f);
		Door door = Door.GetEntranceDoor();
		float entranceGoToDistance = door.EntranceGoToDistance;
		Vector3 targetPosition = PlayerFarming.Instance.transform.position + door.GetDoorDirection() * entranceGoToDistance;
		PlayerFarming.Instance.GoToAndStop(targetPosition, null, true, false, delegate
		{
			door = Door.GetFirstNonEntranceDoor();
			if (door != null)
			{
				PlayerState.facingAngle = Utils.GetAngle(PlayerState.transform.position, door.transform.position);
			}
			StartCoroutine(DelayEndConversation());
		});
		yield return new WaitForSeconds(0.5f);
	}

	private IEnumerator DelayEndConversation()
	{
		yield return new WaitForSeconds(0.3f);
		GameManager.GetInstance().OnConversationEnd(false);
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
	}

	private void OnDestroy()
	{
		AudioManager.Instance.StopCurrentMusic();
		AudioManager.Instance.StopCurrentAtmos();
	}
}
