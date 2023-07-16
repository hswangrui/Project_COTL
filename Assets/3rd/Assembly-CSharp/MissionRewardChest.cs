using System.Collections;
using Spine.Unity;
using UnityEngine;

public class MissionRewardChest : BaseMonoBehaviour
{
	public struct Reward
	{
		public InventoryItem.ITEM_TYPE RewardItemType;

		public int Quantity;
	}

	public enum ChestType
	{
		None,
		Wooden,
		Silver,
		Gold
	}

	public delegate void RewardEvent();

	public ChestType TypeOfChest;

	public SkeletonAnimation Spine;

	public GameObject Shadow;

	public GameObject Lighting;

	private int rewardAmount;

	private MissionManager.Mission mission;

	public event RewardEvent OnRewardsCollected;

	public void Play(MissionManager.Mission mission)
	{
		this.mission = mission;
	}

	public void ShowReward()
	{
		StartCoroutine(GiveRewardDelay());
	}

	private IEnumerator GiveRewardDelay()
	{
		Shadow.transform.localScale = new Vector3(3f, 2f, 1f);
		Spine.AnimationState.SetAnimation(0, "open", false);
		ChestOpenSfx();
		Spine.AnimationState.AddAnimation(0, "opened", true, 0f);
		yield return new WaitForSeconds(0.25f);
		int num = (mission.GoldenMission ? (mission.Rewards.Length - 1) : mission.Rewards.Length);
		for (int i = 0; i < num; i++)
		{
			PickUp pickUp = InventoryItem.Spawn(mission.Rewards[i].itemToBuy, Mathf.Clamp(mission.Rewards[i].quantity, 1, int.MaxValue), base.transform.position + Vector3.back, 0f);
			pickUp.SetInitialSpeedAndDiraction(3f + Random.Range(-0.5f, 1f), 270 + Random.Range(-90, 90));
			pickUp.OnPickedUp += ItemPickedUp;
			rewardAmount++;
			AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.gameObject);
		}
		if (mission.GoldenMission)
		{
			PickUp pickUp2 = InventoryItem.Spawn(mission.Rewards[num].itemToBuy, Mathf.Clamp(mission.Rewards[num].quantity, 1, int.MaxValue), base.transform.position + Vector3.back, 0f);
			pickUp2.SetInitialSpeedAndDiraction(3f + Random.Range(-0.5f, 1f), 270 + Random.Range(-90, 90));
			pickUp2.OnPickedUp += ItemPickedUp;
			FoundItemPickUp component = pickUp2.GetComponent<FoundItemPickUp>();
			if (mission.Rewards[num].itemToBuy == InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN)
			{
				component.FollowerSkinForceSelection = true;
				component.SkinToForce = mission.FollowerSkin;
			}
			else if (mission.Rewards[num].itemToBuy == InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION)
			{
				component.DecorationType = mission.Decoration;
			}
			rewardAmount++;
			AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.gameObject);
		}
	}

	private void ItemPickedUp(PickUp p)
	{
		rewardAmount--;
		if (rewardAmount <= 0)
		{
			RewardEvent onRewardsCollected = this.OnRewardsCollected;
			if (onRewardsCollected != null)
			{
				onRewardsCollected();
			}
		}
	}

	private void ChestOpenSfx()
	{
		switch (TypeOfChest)
		{
		case ChestType.Wooden:
			AudioManager.Instance.PlayOneShot("event:/chests/chest_small_open", base.gameObject);
			break;
		case ChestType.Gold:
			AudioManager.Instance.PlayOneShot("event:/chests/chest_big_open", base.gameObject);
			break;
		case ChestType.Silver:
			AudioManager.Instance.PlayOneShot("event:/chests/chest_big_open", base.gameObject);
			break;
		default:
			AudioManager.Instance.PlayOneShot("event:/chests/chest_small_open", base.gameObject);
			break;
		}
	}
}
