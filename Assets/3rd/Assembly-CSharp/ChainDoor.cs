using System;
using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class ChainDoor : BaseMonoBehaviour
{
	public static ChainDoor Instance;

	public SkeletonAnimation Spine;

	private float Zoom;

	private float ZoomToAdd;

	private void OnEnable()
	{
		Instance = this;
	}

	private void Start()
	{
		if (DataManager.Instance.BossesCompleted.Contains(PlayerFarming.Location) || DungeonSandboxManager.Active)
		{
			ShowOpenDoor();
		}
	}

	private void ShowOpenDoor()
	{
		Spine.AnimationState.SetAnimation(0, "open", true);
	}

	public void Play(Action Callback)
	{
		StartCoroutine(PlayRoutine(Callback));
	}

	private IEnumerator PlayRoutine(Action Callback)
	{
		Zoom = 5f;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, Zoom += 3f);
		yield return new WaitForSeconds(1f);
		AudioManager.Instance.PlayOneShot("event:/door/chain_door_sequence");
		Spine.AnimationState.SetAnimation(0, "activate", false);
		Spine.AnimationState.AddAnimation(0, "open", true, 0f);
		Spine.AnimationState.Event += AnimationState_Event;
		GameManager.GetInstance().OnConversationNext(base.gameObject, Zoom += 5f);
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.4f, 4.2f);
		yield return new WaitForSeconds(5.2f);
		yield return new WaitForSeconds(2.5f);
		GameManager.GetInstance().OnConversationEnd();
		Spine.AnimationState.Event -= AnimationState_Event;
		if (Callback != null)
		{
			Callback();
		}
	}

	private void AnimationState_Event(TrackEntry trackEntry, global::Spine.Event e)
	{
		switch (e.Data.Name)
		{
		case "break":
			CameraManager.instance.ShakeCameraForDuration(0.5f, 0.7f, 0.3f);
			GameManager.GetInstance().OnConversationNext(base.gameObject, Zoom += 1f);
			break;
		case "chainbreak":
			CameraManager.instance.ShakeCameraForDuration(0.5f, 0.7f, 0.5f);
			GameManager.GetInstance().OnConversationNext(base.gameObject, Zoom += 3f);
			break;
		case "bigchainbreak":
			CameraManager.instance.ShakeCameraForDuration(0.5f, 0.7f, 0.7f);
			GameManager.GetInstance().OnConversationNext(base.gameObject, Zoom += 5f + (ZoomToAdd -= 2f));
			break;
		}
	}
}
