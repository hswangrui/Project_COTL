using System.Collections;
using I2.Loc;
using MMBiomeGeneration;
using MMRoomGeneration;
using MMTools;
using UnityEngine;

public class Interaction_BiomeDoor : Interaction
{
	public int FollowerCount = 1;

	public GameObject PlayerPosition;

	public SimpleSetCamera SimpleSetCamera;

	public GameObject DoorToMove;

	private Vector3 OpenDoorPosition = new Vector3(0f, -2.5f, 4f);

	public BoxCollider2D CollideForDoor;

	private bool Unlocked;

	private string sOpenDoor;

	private bool Used;

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		Used = false;
		Debug.Log("DataManager.Instance.UnlockedBossTempleDoor.Contains(MMBiomeGeneration.BiomeGenerator.Instance.DungeonLocation) " + DataManager.Instance.UnlockedBossTempleDoor.Contains(BiomeGenerator.Instance.DungeonLocation).ToString() + "  " + BiomeGenerator.Instance.DungeonLocation);
		Unlocked = DataManager.Instance.UnlockedBossTempleDoor.Contains(BiomeGenerator.Instance.DungeonLocation);
		if (Unlocked)
		{
			DoorToMove.transform.localPosition = OpenDoorPosition;
		}
		else
		{
			DoorToMove.transform.localPosition = Vector3.zero;
		}
		OpenDoor();
		BiomeGenerator.OnBiomeChangeRoom += OnBiomeChangeRoom;
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		BiomeGenerator.OnBiomeChangeRoom -= OnBiomeChangeRoom;
	}

	private void OnBiomeChangeRoom()
	{
		BiomeGenerator.OnBiomeChangeRoom -= OnBiomeChangeRoom;
		BiomeGenerator.Instance.North.Init(GenerateRoom.ConnectionTypes.False);
	}

	private void Start()
	{
		UpdateLocalisation();
	}

	private void OpenDoor()
	{
		if (Unlocked)
		{
			CollideForDoor.enabled = true;
		}
		else
		{
			CollideForDoor.enabled = false;
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sOpenDoor = ScriptLocalization.Interactions.UnlockDoor;
	}

	public override void GetLabel()
	{
		base.Label = (Unlocked ? "" : (sOpenDoor + " <sprite name=\"icon_Followers\">x" + FollowerCount));
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (DataManager.Instance.Followers.Count >= FollowerCount || CheatConsole.BuildingsFree)
		{
			GameManager.GetInstance().OnConversationNew();
			SimpleSetCamera.Play();
			PlayerFarming.Instance.GoToAndStop(PlayerPosition, base.gameObject, false, false, delegate
			{
				StartCoroutine(EnterTemple());
			});
		}
		else
		{
			MonoSingleton<Indicator>.Instance.PlayShake();
		}
	}

	private IEnumerator EnterTemple()
	{
		if (!DataManager.Instance.UnlockedBossTempleDoor.Contains(BiomeGenerator.Instance.DungeonLocation))
		{
			Debug.Log("ADD ME! " + BiomeGenerator.Instance.DungeonLocation);
			DataManager.Instance.UnlockedBossTempleDoor.Add(BiomeGenerator.Instance.DungeonLocation);
		}
		AudioManager.Instance.PlayOneShot("event:/door/door_unlock", base.gameObject);
		yield return new WaitForSeconds(1f);
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, 0.3f);
		yield return new WaitForSeconds(0.2f);
		AudioManager.Instance.PlayOneShot("event:/door/door_lower", base.gameObject);
		float Progress = 0f;
		float Duration = 3f;
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, Duration);
		Vector3 StartingPosition = DoorToMove.transform.position;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			DoorToMove.transform.position = Vector3.Lerp(StartingPosition, StartingPosition + OpenDoorPosition, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/door/door_done", base.gameObject);
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, 0.3f);
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().OnConversationEnd();
		SimpleSetCamera.Reset();
		Unlocked = true;
		OpenDoor();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Player" && !Used)
		{
			Used = true;
			MMTransition.StopCurrentTransition();
			MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 0.5f, "", ChangeRoom);
		}
	}

	private void ChangeRoom()
	{
		BiomeGenerator.Instance.FirstArrival = true;
		BiomeGenerator.Instance.DoFirstArrivalRoutine = true;
		BiomeGenerator.ChangeRoom(BiomeGenerator.Instance.RoomEntrance.x, BiomeGenerator.Instance.RoomEntrance.y);
	}
}
