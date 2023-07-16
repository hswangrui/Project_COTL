using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChoiceCard : BaseMonoBehaviour
{
	public TextMeshProUGUI TitleText;

	public TextMeshProUGUI SubtitleText;

	public CanvasGroup canvasGroup;

	public ChoiceReward Reward;

	public SkeletonGraphic FollowerSpine;

	public InventoryItemDisplay ItemDisplay;

	public TextMeshProUGUI PositiveTraitText;

	public TextMeshProUGUI PositiveTraitDescription;

	public TextMeshProUGUI NegativeTraitText;

	public TextMeshProUGUI NegativeTraitDescription;

	public TextMeshProUGUI CostText;

	public RectTransform CostContainer;

	public Image CostPrefab;

	private Vector3 InitPosition;

	private List<Image> Costs;

	public float xSpeed;

	public float xOffset;

	private Coroutine cShake;

	public void Play(ChoiceReward Reward, float Delay)
	{
		this.Reward = Reward;
		TitleText.text = Reward.Title;
		SubtitleText.text = (Reward.Locked ? "" : Reward.SubTitle);
		SetUpSpine();
		SetItemDisplay();
		SetTraits();
		SetUpCosts();
		StartCoroutine(PlayRoutine(Delay));
		InitPosition = CostText.transform.localPosition;
	}

	public void OnHighlighted()
	{
		SetCostColours(true);
	}

	public void OnDehighlighted()
	{
		SetCostColours(false);
	}

	private void SetUpCosts()
	{
		CostText.text = (Reward.Locked ? Reward.SubTitle : ((Reward.Cost == 0) ? "" : (FontImageNames.GetIconByType(Reward.Currency) + "x" + Reward.Cost)));
		if (Inventory.GetItemQuantity((int)Reward.Currency) < Reward.Cost)
		{
			CostText.color = Color.red;
		}
	}

	private void SetCostColours(bool PreviewPlay)
	{
	}

	private void SetItemDisplay()
	{
		if (Reward.Type == ChoiceReward.RewardType.Resource)
		{
			ItemDisplay.SetImage(Reward.ItemType);
		}
		else
		{
			ItemDisplay.gameObject.SetActive(false);
		}
	}

	private void SetTraits()
	{
		if (Reward.Type == ChoiceReward.RewardType.Follower)
		{
			PositiveTraitText.text = FollowerTrait.GetLocalizedTitle(Reward.FollowerInfo.Traits[0]);
			PositiveTraitDescription.text = FollowerTrait.GetLocalizedDescription(Reward.FollowerInfo.Traits[0]);
			NegativeTraitText.text = FollowerTrait.GetLocalizedTitle(Reward.FollowerInfo.Traits[1]);
			NegativeTraitDescription.text = FollowerTrait.GetLocalizedDescription(Reward.FollowerInfo.Traits[1]);
			PositiveTraitText.gameObject.SetActive(true);
			PositiveTraitDescription.gameObject.SetActive(true);
			NegativeTraitText.gameObject.SetActive(true);
			NegativeTraitDescription.gameObject.SetActive(true);
		}
		else
		{
			PositiveTraitText.gameObject.SetActive(false);
			PositiveTraitDescription.gameObject.SetActive(false);
			NegativeTraitText.gameObject.SetActive(false);
			NegativeTraitDescription.gameObject.SetActive(false);
		}
	}

	private void SetUpSpine()
	{
		if (Reward.Type == ChoiceReward.RewardType.Follower)
		{
			FollowerSpine.Skeleton.SetSkin(Reward.FollowerInfo.SkinName);
			FollowerSpine.timeScale = UnityEngine.Random.Range(0.9f, 1.2f);
			WorshipperData.SkinAndData colourData = WorshipperData.Instance.GetColourData(Reward.FollowerInfo.SkinName);
			if (colourData != null)
			{
				foreach (WorshipperData.SlotAndColor slotAndColour in colourData.SlotAndColours[Mathf.Clamp(Reward.FollowerInfo.SkinColour, 0, colourData.SlotAndColours.Count - 1)].SlotAndColours)
				{
					Slot slot = FollowerSpine.Skeleton.FindSlot(slotAndColour.Slot);
					if (slot != null)
					{
						slot.SetColor(slotAndColour.color);
					}
				}
			}
			FollowerSpine.AnimationState.SetAnimation(0, "spawn-in", true);
			if (UnityEngine.Random.value < 0.5f)
			{
				FollowerSpine.AnimationState.AddAnimation(0, "wave", true, 0f);
			}
			else
			{
				FollowerSpine.AnimationState.AddAnimation(0, "worship", true, 0f);
			}
		}
		else
		{
			FollowerSpine.gameObject.SetActive(false);
		}
	}

	private IEnumerator PlayRoutine(float Delay)
	{
		canvasGroup.alpha = 0f;
		yield return new WaitForSecondsRealtime(Delay);
		float Progress = 0f;
		float Duration = 0.5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < 0.5f))
			{
				break;
			}
			canvasGroup.alpha = Progress / Duration;
			yield return null;
		}
		canvasGroup.alpha = 1f;
	}

	public bool CanAfford()
	{
		return Inventory.GetItemQuantity((int)Reward.Currency) >= Reward.Cost;
	}

	public void Shake()
	{
		if (cShake != null)
		{
			StopCoroutine(cShake);
		}
		cShake = StartCoroutine(ShakeRoutine());
	}

	private IEnumerator ShakeRoutine()
	{
		Debug.Log("SHAKE!");
		xSpeed = 10f;
		CostText.transform.localPosition = InitPosition;
		Vector3 Direction = ((UnityEngine.Random.value < 0.5f) ? Vector3.left : (-Vector3.right));
		while (true)
		{
			xSpeed += (0f - xOffset) * 0.2f;
			xOffset += (xSpeed *= 0.8f);
			CostText.transform.localPosition = InitPosition + Direction * (xOffset * (Time.unscaledDeltaTime * 60f));
			yield return null;
		}
	}

	public bool Play(Action CancelCallBack)
	{
		return true;
	}
}
