using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FollowerFaithUI : BaseMonoBehaviour
{
	public enum Stats
	{
		Faith,
		Hunger,
		Illness
	}

	public Stats Stat;

	public Image FillImage;

	public Follower Follower;

	public float TargetScale = 0.8f;

	private float Value;

	private void OnEnable()
	{
		base.transform.localScale = Vector3.one * TargetScale * 0.5f;
		base.transform.DOScale(Vector3.one * TargetScale, 0.5f).SetEase(Ease.OutBack);
		switch (Stat)
		{
		case Stats.Hunger:
			if (Follower.Brain.Stats.Starvation <= 0f)
			{
				base.gameObject.SetActive(false);
			}
			break;
		case Stats.Illness:
			if (Follower.Brain.Stats.Illness <= 0f)
			{
				base.gameObject.SetActive(false);
			}
			break;
		}
	}

	private void Update()
	{
		switch (Stat)
		{
		case Stats.Faith:
			Value = Follower.Brain.Stats.Happiness / 100f;
			FillImage.color = ReturnColorBasedOnValue(Value);
			break;
		case Stats.Hunger:
			Value = (Follower.Brain.Stats.Satiation + (75f - Follower.Brain.Stats.Starvation)) / 175f;
			FillImage.color = ReturnColorBasedOnValueHunger(Value);
			break;
		case Stats.Illness:
			Value = 1f - Follower.Brain.Stats.Illness / 100f;
			FillImage.color = ReturnColorBasedOnValue(Value);
			break;
		}
		FillImage.fillAmount = Value;
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
		if ((double)f >= 0.5 && (double)f < 0.75)
		{
			return StaticColors.OrangeColor;
		}
		return StaticColors.GreenColor;
	}
}
