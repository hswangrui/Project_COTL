using System.Collections;
using I2.Loc;
using MMTools;
using UnityEngine;

public class Inteaction_DoorRoomDoor : Interaction
{
	public int FollowerCount = 1;

	public FollowerLocation Location;

	public string SceneName;

	public BoxCollider2D CollideForDoor;

	public GameObject PlayerPosition;

	public SimpleSetCamera SimpleSetCamera;

	public GameObject DoorToMove;

	private Vector3 OpenDoorPosition = new Vector3(0f, -2.5f, 4f);

	private bool Used;

	private bool Unlocked;

	private SimpleBark Bark;

	private string sOpenDoor;

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		Used = false;
		Unlocked = DataManager.Instance.UnlockedDungeonDoor.Contains(Location);
		if (Unlocked)
		{
			DoorToMove.transform.localPosition = OpenDoorPosition;
		}
		else
		{
			DoorToMove.transform.localPosition = Vector3.zero;
		}
		OpenDoor();
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

	private void Start()
	{
		Bark = GetComponentInChildren<SimpleBark>();
		Bark.Translate = false;
		int num = 0;
		int num2 = -1;
		while (++num2 <= 4)
		{
			Debug.Log(num2);
			if (DataManager.HasKeyPieceFromLocation(Location, num2))
			{
				num++;
			}
		}
		Bark.Entries[0].TermToSpeak = LocalizationManager.Sources[0].GetTranslation(Bark.Entries[0].TermToSpeak) + " - " + num + "/4x <sprite name=\"icon_key\">";
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
	}

	public override void GetLabel()
	{
		base.Label = (Unlocked ? "" : sOpenDoor);
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		GameManager.GetInstance().OnConversationNew();
		SimpleSetCamera.Play();
		PlayerFarming.Instance.GoToAndStop(PlayerPosition, base.gameObject, false, false, delegate
		{
			StartCoroutine(EnterTemple());
		});
	}

	private IEnumerator EnterTemple()
	{
		if (!DataManager.Instance.UnlockedDungeonDoor.Contains(Location))
		{
			Debug.Log("ADD ME! " + Location);
			DataManager.Instance.UnlockedDungeonDoor.Add(Location);
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
			GetFloor(Location, false);
			MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, SceneName, 1f, "", FadeSave);
		}
	}

	public static void GetFloor(FollowerLocation Location, bool InDungeon)
	{
		DataManager.LocationAndLayer locationAndLayer = DataManager.LocationAndLayer.ContainsLocation(Location, DataManager.Instance.CachePreviousRun);
		int num = 0;
		int num2 = 4;
		num = DataManager.Instance.GetDungeonLayer(Location);
		bool flag = num >= num2 || DataManager.Instance.DungeonCompleted(Location);
		if (GameManager.Layer2)
		{
			num = DataManager.GetGodTearNotches(Location) + 1;
		}
		DataManager.Instance.DungeonBossFight = num >= num2 && !DataManager.Instance.DungeonCompleted(Location, GameManager.Layer2);
		if (flag)
		{
			num = DataManager.RandomSeed.Next(1, num2 + 1);
			if (locationAndLayer != null)
			{
				while (num == locationAndLayer.Layer)
				{
					num = DataManager.RandomSeed.Next(1, num2 + 1);
				}
			}
		}
		GameManager.DungeonUseAllLayers = flag;
		GameManager.NextDungeonLayer(num);
		GameManager.NewRun("", InDungeon, Location);
		if (locationAndLayer != null)
		{
			locationAndLayer.Layer = num;
			Debug.Log("Now set cached layer to: " + locationAndLayer.Layer);
		}
		else
		{
			DataManager.Instance.CachePreviousRun.Add(new DataManager.LocationAndLayer(Location, num));
		}
	}

	private void FadeSave()
	{
	}
}
