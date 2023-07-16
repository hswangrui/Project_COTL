using System;
using System.Collections;
using MMTools;
using Spine.Unity;
using UnityEngine;

public class BaseChainDoor : BaseMonoBehaviour
{
	public SkeletonAnimation Spine;

	private float Distance = 8f;

	public Interaction_BaseDungeonDoor Door1;

	public Interaction_BaseDungeonDoor Door2;

	public Interaction_BaseDungeonDoor Door3;

	public Interaction_BaseDungeonDoor Door4;

	private bool DoorActive;

	public static BaseChainDoor Instance;

	public Vector3 OffsetPosition;

	private bool Used;

	private void OnEnable()
	{
		Instance = this;
		StartCoroutine(WaitForPlayer());
	}

	private IEnumerator WaitForPlayer()
	{
		while (PlayerFarming.Instance == null)
		{
			yield return null;
		}
		while (Vector3.Distance(PlayerFarming.Instance.transform.position, base.transform.position + OffsetPosition) > Distance)
		{
			yield return null;
		}
		Play();
	}

	private void Start()
	{
		if (DataManager.Instance.BossesCompleted.Count == 0 || DataManager.Instance.DoorRoomChainProgress == 0)
		{
			Spine.AnimationState.SetAnimation(0, "closed", true);
			return;
		}
		Spine.AnimationState.SetAnimation(0, "broken" + DataManager.Instance.DoorRoomChainProgress, true);
		DoorActive = DataManager.Instance.DoorRoomChainProgress >= 5;
	}

	private void Play()
	{
		StartCoroutine(PlayRoutine());
	}

	public void PlayDoor1(Action CallBack)
	{
		StartCoroutine(PlayDoor1Routine(CallBack));
	}

	private IEnumerator PlayDoor1Routine(Action CallBack)
	{
		DataManager.Instance.DoorRoomChainProgress = -1;
		yield return StartCoroutine(PlayRoutine());
		yield return new WaitForSeconds(0.5f);
		if (CallBack != null)
		{
			CallBack();
		}
	}

	public void BlockAllDoors()
	{
		Door1.Block();
		Door2.Block();
		Door3.Block();
		Door4.Block();
	}

	public void UnblockAllDoors()
	{
		Door1.Unblock();
		Door2.Unblock();
		Door3.Unblock();
		Door4.Unblock();
	}

	private IEnumerator PlayRoutine()
	{
		while (PlayerFarming.Instance == null)
		{
			yield return null;
		}
		while (PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.CustomAnimation)
		{
			yield return null;
		}
		bool doChainDoorRoutine = DataManager.Instance.DoorRoomChainProgress < DataManager.Instance.BossesCompleted.Count || DataManager.Instance.DoorRoomChainProgress == 4;
		if (doChainDoorRoutine)
		{
			SimpleSetCamera.DisableAll();
			Debug.Log(("DataManager.Instance.DoorRoomChainProgress " + DataManager.Instance.DoorRoomChainProgress).Colour(Color.red));
			DataManager.Instance.DoorRoomChainProgress++;
			Debug.Log(("DataManager.Instance.DoorRoomChainProgress " + DataManager.Instance.DoorRoomChainProgress).Colour(Color.red));
			if (DataManager.Instance.BossesCompleted.Count > 0)
			{
				Debug.Log("aaa".Colour(Color.red));
				GameManager.GetInstance().OnConversationNew();
				GameManager.GetInstance().OnConversationNext(base.gameObject, 12f);
				if (DataManager.Instance.DoorRoomChainProgress != 5)
				{
					AudioManager.Instance.PlayOneShot("event:/door/chain_break_sequence");
				}
				else
				{
					AudioManager.Instance.PlayOneShot("event:/door/chain_door_final");
				}
				yield return new WaitForSeconds(0.5f);
				Spine.AnimationState.SetAnimation(0, "break" + DataManager.Instance.DoorRoomChainProgress, false);
				Spine.AnimationState.AddAnimation(0, "broken" + DataManager.Instance.DoorRoomChainProgress, true, 0f);
				if (DataManager.Instance.DoorRoomChainProgress == 5)
				{
					yield return new WaitForSeconds(5.4666667f);
				}
				else
				{
					yield return new WaitForSeconds(1.5f);
				}
				Debug.Log("finished chain!".Colour(Color.red));
			}
			yield return new WaitForSeconds(0.5f);
			if (DataManager.Instance.DoorRoomChainProgress == 4)
			{
				Play();
				yield break;
			}
		}
		Debug.Log("A");
		Interaction_BaseDungeonDoor TargetDoor = null;
		if (DataManager.Instance.DoorRoomChainProgress < 5 && DataManager.Instance.DoorRoomDoorsProgress != DataManager.Instance.BossesCompleted.Count)
		{
			if (DataManager.Instance.BossesCompleted.Count <= 0 && !Door1.Unlocked)
			{
				TargetDoor = Door1;
			}
			else if (DataManager.Instance.BossesCompleted.Count >= 1 && Door2.GetFollowerCount() && !Door2.Unlocked)
			{
				TargetDoor = Door2;
			}
			else if (DataManager.Instance.BossesCompleted.Count >= 2 && Door3.GetFollowerCount() && !Door3.Unlocked)
			{
				TargetDoor = Door3;
			}
			else if (DataManager.Instance.BossesCompleted.Count >= 3 && Door4.GetFollowerCount() && !Door4.Unlocked)
			{
				TargetDoor = Door4;
			}
		}
		if (DataManager.Instance.DoorRoomChainProgress >= 5)
		{
			DoorActive = true;
		}
		if (TargetDoor != null)
		{
			DataManager.Instance.DoorRoomDoorsProgress = DataManager.Instance.BossesCompleted.Count;
			if (TargetDoor == Door1)
			{
				TargetDoor.DoorLights.SetActive(false);
			}
			GameManager.GetInstance().OnConversationNew();
			TargetDoor.SimpleSetCamera.Play();
			yield return new WaitForSeconds(0.5f);
			AudioManager.Instance.PlayOneShot("event:/door/cross_disappear");
			yield return new WaitForSeconds(0.5f);
			if (TargetDoor == Door1)
			{
				TargetDoor.FadeDoorLight();
			}
			yield return new WaitForSeconds(2f);
			GameManager.GetInstance().OnConversationEnd();
			TargetDoor.SimpleSetCamera.Reset();
			SimpleSetCamera.EnableAll();
		}
		else if (doChainDoorRoutine)
		{
			GameManager.GetInstance().OnConversationEnd();
			SimpleSetCamera.EnableAll();
		}
		if (doChainDoorRoutine)
		{
			yield return new WaitForSeconds(0.5f);
			if (DataManager.Instance.DoorRoomChainProgress < DataManager.Instance.BossesCompleted.Count)
			{
				Play();
			}
		}
	}

	private void Test(FollowerLocation Location)
	{
		DataManager.Instance.BossesCompleted.Add(Location);
		Play();
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position + OffsetPosition, Distance, Color.yellow);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (DoorActive && collision.gameObject.tag == "Player" && !Used)
		{
			Used = true;
			GameManager.NewRun("", false);
			MMTransition.StopCurrentTransition();
			MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, "Dungeon Final", 1f, "", FadeSave);
		}
	}

	private void FadeSave()
	{
		SaveAndLoad.Save();
	}
}
