using System;
using System.Collections;
using I2.Loc;
using Lamb.UI;
using MMTools;
using UnityEngine;

public class Interaction_BaseDoor : Interaction
{
	public GameObject PlayerPosition;

	public SimpleSetCamera SimpleSetCamera;

	public GameObject DoorToMove;

	private int Cost = 2;

	public GameObject ReceiveDevotion;

	private Vector3 OpenDoorPosition = new Vector3(0f, -2.5f, 4f);

	public static Interaction_BaseDoor Instance;

	public BoxCollider2D CollideForDoor;

	private bool Unlocked;

	private string sOpenDoor;

	private bool Used;

	public override void OnEnableInteraction()
	{
		ActivateDistance = 3f;
		base.OnEnableInteraction();
		Unlocked = DataManager.Instance.UnlockedBossTempleDoor.Contains(FollowerLocation.Base);
		if (Unlocked)
		{
			DoorToMove.transform.localPosition = OpenDoorPosition;
		}
		else
		{
			DoorToMove.transform.localPosition = Vector3.zero;
		}
		Used = false;
		OpenDoor();
		Instance = this;
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
		base.Label = "";
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (DataManager.Instance.Followers.Count < Cost)
		{
			AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback", base.gameObject);
			MonoSingleton<Indicator>.Instance.PlayShake();
			return;
		}
		GameManager.GetInstance().OnConversationNew();
		SimpleSetCamera.Play();
		PlayerFarming.Instance.GoToAndStop(PlayerPosition, base.gameObject, false, false, delegate
		{
			StartCoroutine(EnterTemple());
		});
	}

	public void Play()
	{
		StartCoroutine(FrameDelayOpenDoor());
	}

	private IEnumerator FrameDelayOpenDoor()
	{
		yield return new WaitForEndOfFrame();
		StartCoroutine(EnterTemple());
		GameManager.GetInstance().OnConversationNew();
		SimpleSetCamera.Play();
	}

	private IEnumerator EnterTemple()
	{
		yield return new WaitForSeconds(2.5f);
		if (!DataManager.Instance.UnlockedBossTempleDoor.Contains(FollowerLocation.Base))
		{
			DataManager.Instance.UnlockedBossTempleDoor.Add(FollowerLocation.Base);
		}
		AudioManager.Instance.PlayOneShot("event:/door/door_unlock", base.gameObject);
		yield return new WaitForSeconds(0.5f);
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, 0.3f);
		yield return new WaitForSeconds(0.2f);
		float Progress = 0f;
		float Duration = 3f;
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, Duration);
		AudioManager.Instance.PlayOneShot("event:/door/door_lower", base.gameObject);
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
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, 0.3f);
		AudioManager.Instance.PlayOneShot("event:/door/door_done", base.gameObject);
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().OnConversationEnd();
		SimpleSetCamera.Reset();
		Unlocked = true;
		OpenDoor();
		DataManager.Instance.ShrineDoor = true;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Player" && !Used)
		{
			Used = true;
			MMTransition.StopCurrentTransition();
			UIWorldMapMenuController uIWorldMapMenuController = MonoSingleton<UIManager>.Instance.ShowWorldMap();
			uIWorldMapMenuController.Show();
			uIWorldMapMenuController.OnCancel = (Action)Delegate.Combine(uIWorldMapMenuController.OnCancel, (Action)delegate
			{
				Used = false;
			});
		}
	}
}
