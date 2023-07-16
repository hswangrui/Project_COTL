using DG.Tweening;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMissionCell : BaseMonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[SerializeField]
	private Image missionIcon;

	[SerializeField]
	private TMP_Text missionName;

	[SerializeField]
	private Image[] starSlots;

	[SerializeField]
	private Sprite starIcon;

	[Space]
	[SerializeField]
	private Image buttonSprite;

	[SerializeField]
	private Sprite unselectedSprite;

	[SerializeField]
	private Sprite selectedSprite;

	[Space]
	[SerializeField]
	private TMP_Text rewardsText;

	[SerializeField]
	private TMP_Text expiryText;

	[Space]
	[SerializeField]
	private GameObject normalBorder;

	[SerializeField]
	private GameObject goldenBorder;

	[Space]
	[SerializeField]
	private Image decorationImage;

	[SerializeField]
	private GameObject skinObject;

	[SerializeField]
	private SkeletonGraphic followerSkinGraphic;

	public MissionManager.Mission Mission { get; private set; }

	public void Play(MissionManager.Mission mission, Sprite icon, int difficulty)
	{
		base.gameObject.SetActive(true);
		Mission = mission;
		missionIcon.sprite = icon;
		missionName.text = MissionManager.GetMissionName(mission);
		rewardsText.text = GetRewardsText();
		expiryText.text = MissionManager.GetExpiryFormatted(mission);
		for (int i = 0; i < starSlots.Length; i++)
		{
			if (i <= difficulty - 1)
			{
				starSlots[i].sprite = starIcon;
			}
		}
		normalBorder.SetActive(!mission.GoldenMission);
		goldenBorder.SetActive(mission.GoldenMission);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		buttonSprite.sprite = unselectedSprite;
		base.transform.DOScale(1f, 0.25f);
	}

	public void OnSelect(BaseEventData eventData)
	{
		buttonSprite.sprite = selectedSprite;
		base.transform.DOScale(1.25f, 0.25f);
	}

	private string GetRewardsText()
	{
		string text = "";
		for (int i = 0; i < Mission.Rewards.Length; i++)
		{
			int quantity = Mission.Rewards[i].quantity;
			if (quantity > 0)
			{
				text += FontImageNames.GetIconByType(Mission.Rewards[i].itemToBuy);
				text = text + quantity + " ";
			}
		}
		if (Mission.GoldenMission)
		{
			text += "\n+\t\t\n\n";
			if (Mission.Rewards[Mission.Rewards.Length - 1].itemToBuy == InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION)
			{
				decorationImage.gameObject.SetActive(true);
				decorationImage.sprite = TypeAndPlacementObjects.GetByType(Mission.Decoration).IconImage;
			}
			else if (Mission.Rewards[Mission.Rewards.Length - 1].itemToBuy == InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN)
			{
				skinObject.SetActive(true);
				followerSkinGraphic.Skeleton.SetSkin(Mission.FollowerSkin);
			}
		}
		return text;
	}
}
