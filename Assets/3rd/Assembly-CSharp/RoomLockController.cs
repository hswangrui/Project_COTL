using System.Collections.Generic;
using MMBiomeGeneration;
using MMRoomGeneration;
using UnityEngine;

public class RoomLockController : BaseMonoBehaviour
{
	public delegate void RoomEvent();

	public bool Standalone;

	public Animator animator;

	public GameObject BlockingCollider;

	public GameObject VisualsContainer;

	public static List<RoomLockController> RoomLockControllers = new List<RoomLockController>();

	public SpriteRenderer BlockedSprite;

	public bool Open = true;

	public bool Completed;

	public static bool DoorsOpen = false;

	private bool inDungeon = true;

	private bool inCollision;

	private int count;

	public static event RoomEvent OnRoomCleared;

	private void Start()
	{
		if (BlockedSprite != null)
		{
			BlockedSprite.material = new Material(BlockedSprite.material);
		}
	}

	private void OnEnable()
	{
		RoomLockControllers.Add(this);
		BlockingCollider.SetActive(!Open);
	}

	private void OnDisable()
	{
		RoomLockControllers.Remove(this);
	}

	public void DoorUp()
	{
		Open = false;
		DoorsOpen = false;
		animator.Play("GoopWallIntro");
		BlockingCollider.SetActive(true);
	}

	public void DoorDown(bool forced = false)
	{
		if (!forced && GenerateRoom.Instance.LockEntranceBehindPlayer)
		{
			Door entranceDoor = Door.GetEntranceDoor();
			if ((((object)entranceDoor != null) ? entranceDoor.RoomLockController : null) == this)
			{
				return;
			}
		}
		Open = true;
		DoorsOpen = true;
		animator.Play("GoopWallDown");
		BlockingCollider.SetActive(false);
	}

	public static void CloseAll()
	{
		foreach (RoomLockController roomLockController in RoomLockControllers)
		{
			if (!roomLockController.Standalone)
			{
				roomLockController.DoorUp();
			}
		}
		if (Health.team2.Count > 0)
		{
			HUD_Manager.Instance.HideTopRight();
		}
	}

	public static void OpenAll()
	{
		foreach (RoomLockController roomLockController in RoomLockControllers)
		{
			roomLockController.DoorDown();
		}
	}

	public static void RoomCompleted(bool wasCombatRoom = false, bool doorsDown = true)
	{
		if (BiomeGenerator.Instance != null)
		{
			if (!BiomeGenerator.Instance.CurrentRoom.Completed && PlayerFarming.Health != null && PlayerFleeceManager.AmountToHealOnRoomComplete() > 0f)
			{
				float healing = PlayerFleeceManager.AmountToHealOnRoomComplete() * (float)TrinketManager.GetHealthAmountMultiplier();
				PlayerFarming.Health.Heal(healing);
				CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
				BiomeConstants.Instance.EmitHeartPickUpVFX(PlayerFarming.Instance.CameraBone.transform.position, 0f, "red", "burst_big");
			}
			BiomeGenerator.Instance.CurrentRoom.Completed = true;
		}
		foreach (RoomLockController roomLockController in RoomLockControllers)
		{
			if (doorsDown)
			{
				roomLockController.DoorDown();
			}
			roomLockController.Completed = true;
		}
		if (wasCombatRoom)
		{
			DeviceLightingManager.TransitionLighting(Color.yellow, Color.white, 0.6f, DeviceLightingManager.F_KEYS);
			RoomEvent onRoomCleared = RoomLockController.OnRoomCleared;
			if (onRoomCleared != null)
			{
				onRoomCleared();
			}
		}
		HUD_Manager.Instance.ShowTopRight();
	}

	public void HideVisuals()
	{
		if (VisualsContainer != null)
		{
			VisualsContainer.SetActive(false);
		}
	}

	public void ShowVisuals()
	{
		if (VisualsContainer != null)
		{
			VisualsContainer.SetActive(true);
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		count = 0;
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (!GameManager.IsDungeon(PlayerFarming.Location))
		{
			Debug.Log("not in dungeon");
		}
		else
		{
			if (!(PlayerFarming.Instance != null) || !(collision.gameObject == PlayerFarming.Instance.gameObject))
			{
				return;
			}
			count++;
			if (PlayerFarming.Instance._state.CURRENT_STATE == StateMachine.State.Moving)
			{
				if (!Completed && !Open && !inCollision && count > 5)
				{
					inCollision = true;
					animator.Play("GoopWallColliding");
					AudioManager.Instance.PlayOneShot("event:/Stings/generic_negative", PlayerFarming.Instance.transform.position);
				}
			}
			else
			{
				count = 0;
			}
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (!(PlayerFarming.Instance != null) || !Open || Completed || !(collision.gameObject == PlayerFarming.Instance.gameObject))
		{
			return;
		}
		inCollision = false;
		if (Standalone)
		{
			DoorUp();
			return;
		}
		if (BiomeGenerator.Instance != null)
		{
			BiomeGenerator.Instance.RoomBecameActive();
		}
		CloseAll();
	}
}
