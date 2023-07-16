using System;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FollowerInformationBox : FollowerSelectItem, IPoolListener
{
	public SkeletonGraphic FollowerSpine;

	public TextMeshProUGUI FollowerName;

	public TextMeshProUGUI FollowerRole;

	[SerializeField]
	private Image _adorationLevel;

	public Image HungerLevel;

	public Image IllnessLevel;

	public Image TiredLevel;

	public LayoutElement ItemLayoutElement;

	public List<FollowerThoughtObject> FollowerThoughts = new List<FollowerThoughtObject>();

	private string tmpstring;

	private string AgeString;

	public FollowerBrain followBrain;

	public GameObject ItemParent;

	public TMP_Text itemText;

	private string LevelIcon = "\uf102";

	private Tween punchTween;

	public InventoryItem.ITEM_TYPE ItemCostType { get; private set; }

	public int Cost { get; private set; }

	protected override void ConfigureImpl()
	{
		ItemParent.SetActive(false);
		if (_button != null)
		{
			_button.onClick.AddListener(delegate
			{
				if (Cost == 0 || Inventory.GetItemQuantity(ItemCostType) >= Cost * -1)
				{
					Action<FollowerInfo> onFollowerSelected = OnFollowerSelected;
					if (onFollowerSelected != null)
					{
						onFollowerSelected(_followerInfo);
					}
				}
				else
				{
					InvalidShake();
				}
			});
			MMButton button = _button;
			button.OnConfirmDenied = (Action)Delegate.Combine(button.OnConfirmDenied, new Action(InvalidShake));
		}
		FollowerName.text = _followerInfo.GetNameFormatted();
		if (_followerInfo.MemberDuration == 0)
		{
			tmpstring = LocalizationManager.GetTranslation("UI/FollowerInfo/MemberStatusNew");
		}
		else
		{
			tmpstring = string.Format(LocalizationManager.GetTranslation("UI/FollowerInfo/MemberDuration"), _followerInfo.MemberDuration);
		}
		AgeString = string.Format(LocalizationManager.GetTranslation("UI/FollowerInfo/Age"), _followerInfo.Age);
		if (FollowerRole != null)
		{
			FollowerRole.text = tmpstring + " | " + AgeString;
		}
		if (FollowerSpine != null)
		{
			FollowerSpine.ConfigureFollower(_followerInfo);
		}
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.Info.ID == _followerInfo.ID)
			{
				followBrain = allBrain;
				break;
			}
		}
		if (followBrain == null)
		{
			_adorationLevel.fillAmount = _followerInfo.Adoration / 100f;
			HungerLevel.transform.parent.parent.gameObject.SetActive(false);
			IllnessLevel.transform.parent.parent.gameObject.SetActive(false);
			TiredLevel.transform.parent.parent.gameObject.SetActive(false);
			if (DataManager.Instance.Followers_Dead.Contains(_followerInfo))
			{
				FollowerSpine.SetFaceAnimation("dead", true);
				FollowerSpine.SetAnimation("dead");
			}
		}
		else
		{
			_adorationLevel.fillAmount = followBrain.Stats.Adoration / followBrain.Stats.MAX_ADORATION;
			HungerLevel.fillAmount = (followBrain.Stats.Satiation + (75f - followBrain.Stats.Starvation)) / 175f;
			HungerLevel.color = ReturnColorBasedOnValueHunger(HungerLevel.fillAmount);
			IllnessLevel.fillAmount = (_followerInfo.Illness / 100f - 1f) * -1f;
			IllnessLevel.color = ReturnColorBasedOnValue(IllnessLevel.fillAmount);
			TiredLevel.fillAmount = _followerInfo.Rest / 100f;
			TiredLevel.color = ReturnColorBasedOnValue(TiredLevel.fillAmount);
		}
		TiredLevel.transform.parent.parent.gameObject.SetActive(false);
		int i = 0;
		List<ThoughtData> list = new List<ThoughtData>();
		list.Clear();
		foreach (ThoughtData thought in _followerInfo.Thoughts)
		{
			list.Add(thought);
		}
		list.Reverse();
		foreach (ThoughtData item in list)
		{
			if (i <= FollowerThoughts.Count - 1)
			{
				FollowerThoughts[i].gameObject.SetActive(true);
				FollowerThoughts[i].Init(item);
				i++;
			}
		}
		for (; i <= FollowerThoughts.Count - 1; i++)
		{
			FollowerThoughts[i].gameObject.SetActive(false);
		}
	}

	public void ShowRewardItem(InventoryItem.ITEM_TYPE itemType, int quantity)
	{
		itemText.text = FontImageNames.GetIconByType(itemType) + " x" + quantity;
		ItemParent.SetActive(true);
	}

	public void ShowCostItem(InventoryItem.ITEM_TYPE itemType, int cost, bool ForceRed = true)
	{
		itemText.text = FontImageNames.GetIconByType(itemType) + " " + cost;
		ItemParent.SetActive(true);
		if (ForceRed)
		{
			itemText.color = Color.red;
		}
		ItemCostType = itemType;
		Cost = cost * -1;
	}

	public void ShowItemCostValue(InventoryItem.ITEM_TYPE itemType, int cost)
	{
		StructuresData.ItemCost itemCost = new StructuresData.ItemCost(itemType, cost);
		ItemParent.SetActive(true);
		Cost = cost;
		ItemCostType = itemType;
		itemText.text = itemCost.ToStringShowQuantity();
		itemText.fontSizeMax = 28f;
		ItemLayoutElement.preferredWidth = 190f;
	}

	public void ShowFaithGain(int gain, float MAX_ADORATION)
	{
		ItemParent.SetActive(true);
		itemText.text = "<font=\"Font Awesome 6 Pro-Solid-900 SDF\"><color=red> " + LevelIcon + " </color></font>+" + (float)gain / MAX_ADORATION * 100f + "%";
		itemText.color = StaticColors.OffWhiteColor;
	}

	public void ShowDevotionGain(int gain)
	{
		ItemParent.SetActive(true);
		itemText.text = "<font=\"Font Awesome 6 Pro-Solid-900 SDF\"><color=red> <sprite name=\"icon_Faith\"> </color></font>+" + gain + "%";
		itemText.color = StaticColors.OffWhiteColor;
	}

	public void ShowMarried()
	{
		itemText.text = "<sprite name=\"icon_Married\">";
		ItemParent.SetActive(true);
		_button.Confirmable = false;
	}

	public void InvalidShake()
	{
		if (punchTween != null)
		{
			punchTween.Complete();
		}
		punchTween = base.transform.DOPunchPosition(Vector3.right * 10f, 0.15f, 1).SetEase(Ease.InOutBack).SetUpdate(true);
	}

	public void ShowObjective()
	{
		if (!string.IsNullOrEmpty(itemText.text))
		{
			itemText.text += "\n";
		}
		itemText.text += "<sprite name=\"icon_NewIcon\">";
		ItemParent.SetActive(true);
	}

	private Color ReturnColorBasedOnValue(float f)
	{
		if (f >= 0f && (double)f < 0.25)
		{
			return StaticColors.RedColor;
		}
		if ((double)f >= 0.25 && (double)f < 0.5)
		{
			return StaticColors.OrangeColor;
		}
		return StaticColors.GreenColor;
	}

	private Color ReturnColorBasedOnValueHunger(float f)
	{
		if (f >= 0f && (double)f < 0.5)
		{
			return StaticColors.RedColor;
		}
		if ((double)f >= 0.5 && (double)f < 0.7)
		{
			return StaticColors.OrangeColor;
		}
		return StaticColors.GreenColor;
	}

	public void OnRecycled()
	{
		FollowerSpine.SetFaceAnimation("Emotions/emotion-normal", true);
		FollowerSpine.SetAnimation("idle", true);
		_adorationLevel.transform.parent.parent.gameObject.SetActive(true);
		HungerLevel.transform.parent.parent.gameObject.SetActive(true);
		IllnessLevel.transform.parent.parent.gameObject.SetActive(true);
		TiredLevel.transform.parent.parent.gameObject.SetActive(false);
		ItemParent.SetActive(false);
		_followerInfo = null;
		_button.interactable = true;
		_button.onClick.RemoveAllListeners();
		_button.OnConfirmDenied = null;
		_button.Confirmable = true;
		OnFollowerSelected = null;
		itemText.text = string.Empty;
		itemText.fontSizeMax = 40f;
		ItemLayoutElement.preferredWidth = 130f;
	}
}
