using System;
using System.Collections;
using System.Collections.Generic;
using MMRoomGeneration;
using MMTools;
using UnityEngine;

public class RoomManager : BaseMonoBehaviour
{
	public delegate void InitEnemiesAction();

	public delegate void ChangeRoomDelegate();

	public static Room r = null;

	public bool TestSingleRoom = true;

	public bool RandomFollowers = true;

	private GameObject North;

	private GameObject East;

	private GameObject South;

	private GameObject West;

	public static int CurrentX = -1;

	public static int CurrentY = -1;

	public static int PrevCurrentX = -1;

	public static int PrevCurrentY = -1;

	private bool EntryToPortalRoom = true;

	public bool IsTeleporting;

	private GameObject player;

	public CameraFollowTarget Camera;

	private static RoomManager _Instance;

	public bool LimitTime = true;

	private List<GameObject> TimeCops = new List<GameObject>();

	private float TimeCopTimer;

	private float ForestHeadTimer = 5f;

	private int NumHeads;

	private List<RoomInfo> RoomPrefabs;

	public RoomInfo CurrentRoomPrefab;

	public static RoomManager Instance
	{
		get
		{
			return _Instance;
		}
		set
		{
			_Instance = value;
		}
	}

	public event InitEnemiesAction OnInitEnemies;

	public event ChangeRoomDelegate OnChangeRoom;

	private void OnEnable()
	{
		if (!Application.isEditor)
		{
			TestSingleRoom = false;
		}
		Instance = this;
		Health.OnDieAny += OnDieAny;
	}

	private void OnDieAny(Health Victim)
	{
		if (Health.team2.Count <= 0 && Victim.team == Health.Team.Team2 && !r.cleared)
		{
			UnlockDoors();
			r.cleared = true;
		}
	}

	private void Start()
	{
		if (WorldGen.WorldGenerated)
		{
			OnWorldGenerated();
		}
		else
		{
			WorldGen.Instance.OnWorldGenerated += OnWorldGenerated;
		}
	}

	private void Update()
	{
		if (LimitTime && HUD_Timer.IsTimeUp && HUD_Timer.TimerRunning)
		{
			SpawnTimeCops();
		}
	}

	private void SpawnTimeCops()
	{
		if (NumHeads <= 0)
		{
			ForestHeadTimer -= Time.deltaTime;
		}
		if (TimeCops.Count < 10 && (TimeCopTimer -= Time.deltaTime) <= 0f && player != null)
		{
			float num = UnityEngine.Random.Range(3, 6);
			float f = (float)UnityEngine.Random.Range(0, 360) * ((float)Math.PI / 180f);
			Vector3 vector = new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
			GameObject gameObject = null;
			if (ForestHeadTimer < 0f && NumHeads <= 0)
			{
				NumHeads++;
				vector = new Vector3(10f * Mathf.Cos(f), 10f * Mathf.Sin(f));
				gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Units/Forest Enemies/Enemy Forest Head"), player.transform.position + vector, Quaternion.identity) as GameObject;
			}
			else
			{
				gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Units/Forest Enemies/Enemy Forest Mushroom"), player.transform.position + vector, Quaternion.identity) as GameObject;
			}
			Health componentInChildren = gameObject.GetComponentInChildren<Health>();
			componentInChildren.OnDie += RemoveTimeCop;
			TimeCops.Add(componentInChildren.gameObject);
			TimeCopTimer = 3f;
		}
	}

	private void RemoveTimeCop(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		TimeCops.Remove(Victim.gameObject);
		Victim.OnDie -= RemoveTimeCop;
	}

	private void OnDisable()
	{
		if (WorldGen.Instance != null)
		{
			WorldGen.Instance.OnWorldGenerated -= OnWorldGenerated;
		}
		Health.OnDieAny -= OnDieAny;
	}

	private void OnWorldGenerated()
	{
		StartCoroutine(DoWorldGeneration());
	}

	private IEnumerator DoWorldGeneration()
	{
		if (CurrentX == -1 && CurrentY == -1)
		{
			CurrentX = WorldGen.startRoom.x;
			CurrentY = WorldGen.startRoom.y;
			CameraFollowTarget.Instance.distance = 30f;
		}
		RoomPrefabs = new List<RoomInfo>();
		yield return StartCoroutine(CreateRooms());
		ChangeRoom();
		MMTransition.ResumePlay();
	}

	private IEnumerator CreateRooms()
	{
		if (TestSingleRoom)
		{
			yield break;
		}
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num < WorldGen.rooms.Count)
			{
				MMTransition.UpdateProgress((float)i / (float)WorldGen.rooms.Count);
				Room room = WorldGen.rooms[i];
				GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load(room.PrefabDir), Vector3.zero, Quaternion.identity) as GameObject;
				CurrentRoomPrefab = gameObject.GetComponent<RoomInfo>();
				RoomPrefabs.Add(CurrentRoomPrefab);
				CurrentRoomPrefab.ID = "Room_" + room.x + "_" + room.y;
				CurrentRoomPrefab.Init();
				r = room;
				gameObject.GetComponent<GenerateRoom>();
				CurrentRoomPrefab.gameObject.SetActive(false);
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator PlaceResources(Room RoomToPlace)
	{
		Transform RoomToPlaceTransform = RoomInfo.GetByID("Room_" + RoomToPlace.x + "_" + RoomToPlace.y).transform;
		for (int i = 0; i < RoomToPlace.Structures.Count; i++)
		{
			StructuresData structuresData = RoomToPlace.Structures[i];
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load(structuresData.PrefabPath) as GameObject, RoomToPlaceTransform, true);
			gameObject.transform.position = structuresData.Position + structuresData.Offset;
			Structure structure = gameObject.GetComponent<Structure>();
			if (structure == null)
			{
				structure = gameObject.GetComponentInChildren<Structure>();
			}
			if (structure != null)
			{
				structure.Brain = StructureBrain.GetOrCreateBrain(structuresData);
			}
			yield return null;
		}
		RoomToPlace.ResourcesPlaced = true;
	}

	private void ChangeRoom()
	{
		if (CurrentRoomPrefab != null)
		{
			CurrentRoomPrefab.gameObject.SetActive(false);
		}
		Room room = r;
		r = WorldGen.getRoom(CurrentX, CurrentY);
		GameManager.GetInstance().RemoveAllFromCamera();
		if (TestSingleRoom)
		{
			CurrentRoomPrefab = UnityEngine.Object.FindObjectOfType<RoomInfo>();
			if (this.OnInitEnemies != null)
			{
				this.OnInitEnemies();
			}
		}
		else
		{
			CurrentRoomPrefab = RoomInfo.GetByID("Room_" + r.x + "_" + r.y);
			CurrentRoomPrefab.gameObject.SetActive(true);
		}
		if (CurrentRoomPrefab != null && CurrentRoomPrefab.Music != null)
		{
			AmbientMusicController.PlayTrack(CurrentRoomPrefab.Music, 3f);
		}
		else if (room == null)
		{
			AmbientMusicController.PlayAmbient(0f);
		}
		else
		{
			AmbientMusicController.StopTrackAndResturnToAmbient();
		}
		GetDoors();
		if (!TestSingleRoom)
		{
			this.OnInitEnemies();
		}
		PlaceAndPositionPlayer();
		if (r.cleared && !TestSingleRoom)
		{
			UnlockDoors();
		}
		GameManager.RecalculatePaths();
		r.visited = true;
		if (this.OnChangeRoom != null)
		{
			this.OnChangeRoom();
		}
	}

	public void RemoveStructure(StructuresData structure)
	{
		r.Structures.Remove(structure);
	}

	public void PlaceAndPositionPlayer(bool ForceCentrePlayer = false)
	{
		if (player == null)
		{
			player = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Units/Player") as GameObject, GameObject.FindGameObjectWithTag("Unit Layer").transform, true);
		}
		if (IsTeleporting)
		{
			if (Interaction_Teleporter.Instance != null)
			{
				player.transform.position = Interaction_Teleporter.Instance.transform.position;
			}
			if (Interaction_TeleporterMap.Instance != null)
			{
				player.transform.position = Interaction_TeleporterMap.Instance.transform.position;
			}
			player.GetComponent<PlayerFarming>().TimedAction(1f, null, "teleport-in");
			IsTeleporting = false;
		}
		else if (ForceCentrePlayer)
		{
			if (Interaction_Teleporter.Instance != null)
			{
				player.transform.position = Interaction_Teleporter.Instance.transform.position;
			}
		}
		else if (EntryToPortalRoom)
		{
			EntryToPortalRoom = false;
			player.GetComponent<StateMachine>().facingAngle = 90f;
			if (South != null)
			{
				player.transform.position = South.transform.position + Vector3.up * 2.5f;
			}
			if (!TestSingleRoom)
			{
				PlayerFarming component = player.GetComponent<PlayerFarming>();
				GameObject gameObject = new GameObject();
				if (South != null)
				{
					gameObject.transform.position = South.transform.position + Vector3.up * 5f;
				}
				component.GoToAndStop(gameObject, null, true);
			}
		}
		else if (PrevCurrentX == -1 && PrevCurrentY == -1)
		{
			player.transform.position = new Vector3(0f, 0f, 0f);
		}
		else if (PrevCurrentX < CurrentX)
		{
			player.transform.position = West.transform.position + Vector3.right * 0.5f;
			player.GetComponent<StateMachine>().facingAngle = 0f;
			foreach (GameObject timeCop in TimeCops)
			{
				Vector3 vector = UnityEngine.Random.insideUnitCircle * 3f;
				timeCop.transform.position = West.transform.position + Vector3.right * -5f + vector;
			}
		}
		else if (PrevCurrentX > CurrentX)
		{
			player.transform.position = East.transform.position + Vector3.left * 0.5f;
			player.GetComponent<StateMachine>().facingAngle = 180f;
			foreach (GameObject timeCop2 in TimeCops)
			{
				Vector3 vector2 = UnityEngine.Random.insideUnitCircle * 3f;
				timeCop2.transform.position = East.transform.position + Vector3.left * -5f + vector2;
			}
		}
		else if (PrevCurrentY > CurrentY)
		{
			player.transform.position = North.transform.position + Vector3.down * 0.5f;
			player.GetComponent<StateMachine>().facingAngle = 270f;
			foreach (GameObject timeCop3 in TimeCops)
			{
				Vector3 vector3 = UnityEngine.Random.insideUnitCircle * 3f;
				timeCop3.transform.position = North.transform.position + Vector3.down * -5f + vector3;
			}
		}
		else if (PrevCurrentY < CurrentY)
		{
			player.GetComponent<StateMachine>().facingAngle = 90f;
			player.transform.position = South.transform.position + Vector3.up * 0.5f;
			foreach (GameObject timeCop4 in TimeCops)
			{
				Vector3 vector4 = UnityEngine.Random.insideUnitCircle * 3f;
				timeCop4.transform.position = South.transform.position + Vector3.up * -5f + vector4;
			}
		}
		GameManager.GetInstance().CameraSnapToPosition(player.transform.position);
		GameManager.GetInstance().AddPlayerToCamera();
		if (RandomFollowers)
		{
			int num = DataManager.Instance.Followers_Recruit.Count;
			while (++num <= 4)
			{
			}
		}
		else if (PrevCurrentX == -1 && PrevCurrentY == -1)
		{
			int num2 = -1;
			while (++num2 < DataManager.Instance.Followers_Recruit.Count)
			{
			}
			num2 = -1;
			while (++num2 < DataManager.Instance.Followers_Demons_IDs.Count)
			{
				UnityEngine.Object.Instantiate(Resources.Load(new List<string> { "Prefabs/Units/Demons/Demon_Shooty", "Prefabs/Units/Demons/Demon_Chomp" }[DataManager.Instance.Followers_Demons_Types[num2]]) as GameObject, GameObject.FindGameObjectWithTag("Unit Layer").transform, true).transform.position = player.transform.position;
			}
		}
		PrevCurrentX = CurrentX;
		PrevCurrentY = CurrentY;
	}

	private void GetDoors()
	{
		North = GameObject.FindGameObjectWithTag("North Door");
		East = GameObject.FindGameObjectWithTag("East Door");
		South = GameObject.FindGameObjectWithTag("South Door");
		West = GameObject.FindGameObjectWithTag("West Door");
	}

	private void UnlockDoors()
	{
		HUD_Timer.TimerPaused = false;
	}

	public void BlockDoors()
	{
		HUD_Timer.TimerPaused = true;
	}

	public void UnbockDoors()
	{
		HUD_Timer.TimerPaused = false;
	}

	public void ChangeRoom(Vector3Int Direction)
	{
		CurrentX += Direction.x;
		CurrentY += Direction.y;
		ChangeRoom();
	}

	public void ChangeRoom(int X, int Y)
	{
		CurrentX = X;
		CurrentY = Y;
		ChangeRoom();
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
