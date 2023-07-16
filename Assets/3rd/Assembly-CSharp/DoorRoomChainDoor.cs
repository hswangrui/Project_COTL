using System.Collections;
using MMTools;
using Spine.Unity;
using UnityEngine;

public class DoorRoomChainDoor : BaseMonoBehaviour
{
	public SkeletonAnimation Spine;

	private float Distance = 8f;

	public DeleteIfBossCompleted Door1;

	public DeleteIfBossCompleted Door2;

	public DeleteIfBossCompleted Door3;

	public DeleteIfBossCompleted Door4;

	public DeleteIfBossCompleted Door5;

	private bool DoorActive;

	private bool Used;

	private void Start()
	{
		if (DataManager.Instance.BossesCompleted.Count == 0)
		{
			Spine.AnimationState.SetAnimation(0, "closed", true);
		}
		else
		{
			Spine.AnimationState.SetAnimation(0, "broken" + DataManager.Instance.DoorRoomChainProgress, true);
			DoorActive = DataManager.Instance.DoorRoomChainProgress >= 5;
		}
		if (DataManager.Instance.DoorRoomChainProgress < DataManager.Instance.BossesCompleted.Count)
		{
			Play();
		}
	}

	private void Play()
	{
		StartCoroutine(PlayRoutine());
	}

	private IEnumerator PlayRoutine()
	{
		while (PlayerFarming.Instance == null)
		{
			yield return null;
		}
		while (Vector3.Distance(PlayerFarming.Instance.transform.position, base.transform.position) > Distance)
		{
			yield return null;
		}
		SimpleSetCamera.DisableAll();
		DataManager.Instance.DoorRoomChainProgress++;
		if (DataManager.Instance.BossesCompleted.Count > 0)
		{
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(base.gameObject, 12f);
			AudioManager.Instance.PlayOneShot("event:/door/chain_break_sequence");
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
		}
		yield return new WaitForSeconds(0.5f);
		if (DataManager.Instance.DoorRoomChainProgress == 4)
		{
			Play();
			yield break;
		}
		DeleteIfBossCompleted TargetDoor = null;
		if (DataManager.Instance.DoorRoomChainProgress < 5)
		{
			if (DataManager.Instance.BossesCompleted.Count <= 0)
			{
				TargetDoor = Door1;
			}
			else
			{
				switch (DataManager.Instance.BossesCompleted[DataManager.Instance.BossesCompleted.Count - 1])
				{
				case FollowerLocation.Dungeon1_1:
					TargetDoor = Door2;
					break;
				case FollowerLocation.Dungeon1_2:
					TargetDoor = Door3;
					break;
				case FollowerLocation.Dungeon1_3:
					TargetDoor = Door4;
					break;
				case FollowerLocation.Dungeon1_4:
					TargetDoor = Door5;
					break;
				}
			}
		}
		else
		{
			DoorActive = true;
		}
		if (TargetDoor != null)
		{
			GameManager.GetInstance().OnConversationNext(TargetDoor.CameraPosition, 8f);
			yield return new WaitForSeconds(1f);
			CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
			Object.Destroy(TargetDoor.gameObject);
			AudioManager.Instance.PlayOneShot("event:/door/cross_disappear");
			if (!DataManager.Instance.DoorRoomBossLocksDestroyed.Contains(TargetDoor.Location))
			{
				DataManager.Instance.DoorRoomBossLocksDestroyed.Add(TargetDoor.Location);
			}
			yield return new WaitForSeconds(1f);
			GameManager.GetInstance().OnConversationEnd();
			SimpleSetCamera.EnableAll();
		}
		else
		{
			GameManager.GetInstance().OnConversationEnd();
			SimpleSetCamera.EnableAll();
		}
		yield return new WaitForSeconds(0.5f);
		if (DataManager.Instance.DoorRoomChainProgress < DataManager.Instance.BossesCompleted.Count)
		{
			Play();
		}
	}

	private void Test(FollowerLocation Location)
	{
		DataManager.Instance.BossesCompleted.Add(Location);
		Play();
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, Distance, Color.yellow);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (DoorActive && collision.gameObject.tag == "Player" && !Used)
		{
			Used = true;
			MMTransition.StopCurrentTransition();
			if (!DataManager.Instance.DeathCatBeaten)
			{
				MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, "Dungeon Final", 1f, "", null);
			}
		}
	}
}
