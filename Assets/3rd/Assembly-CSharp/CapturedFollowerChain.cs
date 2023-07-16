using System.Collections;
using Spine;
using UnityEngine;

public class CapturedFollowerChain : MonoBehaviour
{
	[SerializeField]
	private Transform fervourDropPosition;

	private FollowerManager.SpawnedFollower follower;

	public bool DroppingFervour { get; private set; }

	public void Init(FollowerManager.SpawnedFollower follower)
	{
		if ((bool)follower.Follower)
		{
			this.follower = follower;
			follower.Follower.gameObject.SetActive(false);
			follower.Follower.transform.parent = base.transform;
			follower.Follower.transform.localPosition = new Vector3(0f, -0.8f, 0f);
			follower.Follower.transform.localScale = new Vector3((Random.Range(0, 2) == 0) ? 1 : (-1), 1f, 1f);
			follower.Follower.SetBodyAnimation("FinalBoss/appear", false);
			follower.Follower.AddBodyAnimation("FinalBoss/idle", true, 0f);
			follower.Follower.Spine.AnimationState.Event += AnimationState_Event;
			StartCoroutine(FrameDelay());
		}
	}

	private IEnumerator FrameDelay()
	{
		yield return new WaitForEndOfFrame();
		follower.Follower.gameObject.SetActive(true);
		while (EnemyDeathCatBoss.Instance == null || EnemyDeathCatBoss.Instance.Spine == null)
		{
			yield return null;
		}
		EnemyDeathCatBoss.Instance.Spine.AnimationState.Event += AnimationState_Event;
	}

	public void DropFervour()
	{
		if (follower.Follower != null)
		{
			follower.Follower.SetBodyAnimation("FinalBoss/give-fervour", false);
			follower.Follower.AddBodyAnimation("FinalBoss/idle", true, 0f);
		}
		else
		{
			StartCoroutine(SpawnFervourIE());
		}
		DroppingFervour = true;
	}

	private IEnumerator SpawnFervourIE()
	{
		int Rewards = 20;
		if (follower.Follower == null)
		{
			Rewards /= 2;
		}
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= Rewards)
			{
				break;
			}
			BlackSoul blackSoul = InventoryItem.SpawnBlackSoul(1, fervourDropPosition.position, false, true);
			if (blackSoul != null)
			{
				blackSoul.SetAngle(Random.Range(0, 360), new Vector2(2f, 3f));
			}
			yield return new WaitForSeconds(0.05f);
		}
		DroppingFervour = false;
	}

	private void AnimationState_Event(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.Data.Name == "fervour")
		{
			StartCoroutine(SpawnFervourIE());
		}
		else if (e.Data.Name == "slam" && !DroppingFervour)
		{
			follower.Follower.SetBodyAnimation("FinalBoss/shake", false);
			follower.Follower.AddBodyAnimation("FinalBoss/idle", true, 0f);
		}
	}
}
