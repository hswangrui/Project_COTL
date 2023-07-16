using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Lamb.UI;
using Lamb.UI.DeathScreen;
using Map;
using MMBiomeGeneration;
using MMRoomGeneration;
using MMTools;
using UnityEngine;

public class Door : BaseMonoBehaviour
{
	public enum Direction
	{
		North,
		East,
		South,
		West
	}

	public Direction direction;

	public GenerateRoom.ConnectionTypes ConnectionType;

	public Transform PlayerPosition;

	public RoomLockController RoomLockController;

	public bool AutoLockBehind = true;

	public bool ForceReturnToBase;

	public bool ForceReturnToDoorRoom;

	public float EntranceGoToDistance = 7f;

	private Vector2Int NextRoom;

	private bool Used;

	public GameObject VisitedIcon;

	public static List<Door> Doors = new List<Door>();

	public static Door GetEntranceDoor()
	{
		foreach (Door door in Doors)
		{
			if (door.ConnectionType == GenerateRoom.ConnectionTypes.Entrance)
			{
				return door;
			}
		}
		return null;
	}

	public static Door GetFirstNonEntranceDoor()
	{
		foreach (Door door in Doors)
		{
			if (door.ConnectionType != GenerateRoom.ConnectionTypes.Entrance)
			{
				return door;
			}
		}
		return null;
	}

	private void OnEnable()
	{
		Used = false;
		Doors.Add(this);
		switch (direction)
		{
		case Direction.North:
			base.tag = "North Door";
			NextRoom = new Vector2Int(0, 1);
			break;
		case Direction.East:
			base.tag = "East Door";
			NextRoom = new Vector2Int(1, 0);
			break;
		case Direction.South:
			base.tag = "South Door";
			NextRoom = new Vector2Int(0, -1);
			break;
		case Direction.West:
			base.tag = "West Door";
			NextRoom = new Vector2Int(-1, 0);
			break;
		}
	}

	private void OnDisable()
	{
		Doors.Remove(this);
	}

	public void Init(GenerateRoom.ConnectionTypes ConnectionType)
	{
		this.ConnectionType = ConnectionType;
		if (ConnectionType == GenerateRoom.ConnectionTypes.False)
		{
			base.gameObject.SetActive(false);
			return;
		}
		base.gameObject.SetActive(true);
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
		if (RoomLockController == null)
		{
			return;
		}
		VisitedIcon.SetActive(false);
		switch (direction)
		{
		case Direction.North:
			if (BiomeGenerator.Instance != null && BiomeGenerator.Instance.CurrentRoom.N_Room.Room != null)
			{
				VisitedIcon.SetActive(!BiomeGenerator.Instance.CurrentRoom.N_Room.Room.Visited);
			}
			break;
		case Direction.East:
			if (BiomeGenerator.Instance != null && BiomeGenerator.Instance.CurrentRoom.E_Room.Room != null)
			{
				VisitedIcon.SetActive(!BiomeGenerator.Instance.CurrentRoom.E_Room.Room.Visited);
			}
			break;
		case Direction.South:
			if (BiomeGenerator.Instance != null && BiomeGenerator.Instance.CurrentRoom.S_Room.Room != null)
			{
				VisitedIcon.SetActive(!BiomeGenerator.Instance.CurrentRoom.S_Room.Room.Visited);
			}
			break;
		case Direction.West:
			if (BiomeGenerator.Instance != null && BiomeGenerator.Instance.CurrentRoom.W_Room.Room != null)
			{
				VisitedIcon.SetActive(!BiomeGenerator.Instance.CurrentRoom.W_Room.Room.Visited);
			}
			break;
		}
		if (GenerateRoom.Instance != null && GenerateRoom.Instance.LockingDoors && ConnectionType != 0 && ConnectionType != GenerateRoom.ConnectionTypes.Entrance)
		{
			RoomLockController.gameObject.SetActive(true);
		}
		else if (ConnectionType == GenerateRoom.ConnectionTypes.Entrance && AutoLockBehind)
		{
			RoomLockController.gameObject.SetActive(true);
			RoomLockController.HideVisuals();
		}
		else
		{
			RoomLockController.gameObject.SetActive(false);
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!(collision.gameObject.tag == "Player") || Used || PlayerFarming.Instance.GoToAndStopping || ConnectionType == GenerateRoom.ConnectionTypes.False || ConnectionType == GenerateRoom.ConnectionTypes.LeaderBoss)
		{
			return;
		}
		Used = true;
		MMTransition.StopCurrentTransition();
		if (DungeonSandboxManager.Active && ConnectionType == GenerateRoom.ConnectionTypes.DoorRoom && MapManager.Instance.CurrentMap.GetFinalBossNode() != MapManager.Instance.CurrentNode)
		{
			ConnectionType = GenerateRoom.ConnectionTypes.NextLayer;
		}
		switch (ConnectionType)
		{
		case GenerateRoom.ConnectionTypes.True:
		case GenerateRoom.ConnectionTypes.Boss:
		case GenerateRoom.ConnectionTypes.Tarot:
		case GenerateRoom.ConnectionTypes.WeaponShop:
		case GenerateRoom.ConnectionTypes.RelicShop:
			MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 0.5f, "", ChangeRoom);
			break;
		case GenerateRoom.ConnectionTypes.Entrance:
		case GenerateRoom.ConnectionTypes.Exit:
		{
			Debug.Log("EXIT!~");
			if (ForceReturnToBase)
			{
				MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, "Base Biome 1", 1f, "", null);
				break;
			}
			if (ForceReturnToDoorRoom)
			{
				DataManager.ResetRunData();
				MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, "Base Biome 1", 1f, "", null);
				break;
			}
			UIWorldMapMenuController uIWorldMapMenuController = MonoSingleton<UIManager>.Instance.ShowWorldMap();
			uIWorldMapMenuController.Show();
			uIWorldMapMenuController.OnCancel = (Action)Delegate.Combine(uIWorldMapMenuController.OnCancel, (Action)delegate
			{
				Used = false;
				if (PlayerFarming.Instance != null)
				{
					PlayerFarming.Instance.GoToAndStop(PlayerPosition.position + GetDoorDirection() * EntranceGoToDistance, null, true);
				}
			});
			break;
		}
		case GenerateRoom.ConnectionTypes.DoorRoom:
			MonoSingleton<UIManager>.Instance.ShowDeathScreenOverlay(UIDeathScreenOverlayController.Results.Completed);
			DataManager.Instance.VisitedLocations.Remove(FollowerLocation.DoorRoom);
			break;
		case GenerateRoom.ConnectionTypes.DungeonFirstRoom:
			MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 0.5f, "", delegate
			{
				BiomeGenerator.Instance.FirstArrival = true;
				BiomeGenerator.Instance.DoFirstArrivalRoutine = true;
				BiomeGenerator.ChangeRoom(BiomeGenerator.Instance.RoomEntrance.x, BiomeGenerator.Instance.RoomEntrance.y);
			});
			break;
		case GenerateRoom.ConnectionTypes.NextLayer:
		{
			PlayerFarming.Instance.unitObject.UseDeltaTime = false;
			PlayerFarming.Instance.SpineUseDeltaTime(false);
			PlayerFarming.Instance.GoToAndStop(base.transform.position - GetDoorDirection() * 2f, null, false, true);
			TweenerCore<Vector3, Vector3, VectorOptions> tween = PlayerFarming.Instance.transform.DOMove(base.transform.position - GetDoorDirection() * 2f, 1f).SetUpdate(true).OnComplete(delegate
			{
				PlayerFarming.Instance.unitObject.UseDeltaTime = true;
				PlayerFarming.Instance.SpineUseDeltaTime(true);
				PlayerFarming.Instance.transform.position = base.transform.position;
			});
			if (DataManager.Instance.DungeonCompleted(BiomeGenerator.Instance.DungeonLocation) && !GameManager.SandboxDungeonEnabled && MapManager.Instance != null && MapManager.Instance.CurrentNode != null && MapManager.Instance.CurrentNode.nodeType == NodeType.MiniBossFloor)
			{
				ObjectPool.DestroyAll();
				GameManager.CurrentDungeonFloor = 1;
				GameManager.DungeonEndlessLevel++;
				StartCoroutine(NewMapRoutine());
				break;
			}
			bool cancelled = false;
			bool hidden = false;
			UIAdventureMapOverlayController uIAdventureMapOverlayController = MapManager.Instance.ShowMap();
			uIAdventureMapOverlayController.OnCancel = (Action)Delegate.Combine(uIAdventureMapOverlayController.OnCancel, (Action)delegate
			{
				Used = false;
				cancelled = true;
				tween.Complete();
				PlayerFarming.Instance.GoToAndStop(PlayerPosition.position + GetDoorDirection() * EntranceGoToDistance * 0.5f, null, true, true);
			});
			uIAdventureMapOverlayController.OnHidden = (Action)Delegate.Combine(uIAdventureMapOverlayController.OnHidden, (Action)delegate
			{
				hidden = true;
			});
			break;
		}
		case GenerateRoom.ConnectionTypes.LeaderBoss:
			break;
		}
	}

	public Vector3 GetDoorDirection()
	{
		Vector3 result = Vector3.zero;
		if (direction == Direction.East)
		{
			result = Vector3.left;
		}
		else if (direction == Direction.West)
		{
			result = Vector3.right;
		}
		else if (direction == Direction.North)
		{
			result = Vector3.down;
		}
		else if (direction == Direction.South)
		{
			result = Vector3.up;
		}
		return result;
	}

	public void PlayerFinishedEnteringDoor()
	{
		if (!AutoLockBehind && ConnectionType == GenerateRoom.ConnectionTypes.Entrance && (bool)GetComponent<Collider2D>())
		{
			ConnectionType = GenerateRoom.ConnectionTypes.True;
			GetComponent<Collider2D>().isTrigger = false;
			RoomLockController.gameObject.SetActive(true);
			RoomLockController.ShowVisuals();
			PlayerDistanceMovement[] componentsInChildren = GetComponentsInChildren<PlayerDistanceMovement>();
			foreach (PlayerDistanceMovement obj in componentsInChildren)
			{
				obj.ForceReset();
				obj.enabled = false;
			}
		}
	}

	private void ChangeRoom()
	{
		BiomeGenerator.ChangeRoom(NextRoom);
	}

	private IEnumerator NewMapRoutine()
	{
		UIAdventureMapOverlayController adventureMapOverlayController = MapManager.Instance.ShowMap(true);
		while (adventureMapOverlayController.IsShowing)
		{
			yield return null;
		}
		yield return adventureMapOverlayController.RegenerateMapRoutine();
	}
}
