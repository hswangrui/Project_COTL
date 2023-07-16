using System;
using System.Collections;
using UnityEngine;

public class FollowerExclamationMark : BaseMonoBehaviour
{
	public enum IconType
	{
		Sleep,
		Food,
		Happiness,
		Illness
	}

	public Follower follower;

	public GameObject Image;

	public GameObject FoodIcon;

	public SpriteRenderer FoodIconRing;

	public GameObject HappinessIcon;

	public SpriteRenderer HappinessIconRing;

	public GameObject SleepinessIcon;

	public SpriteRenderer SleepinesssIconRing;

	public GameObject IllnessIcon;

	public SpriteRenderer IllnessIconRing;

	public Gradient RadialGradient;

	private void OnEnable()
	{
		StartCoroutine(Init());
	}

	private IEnumerator Init()
	{
		Image.SetActive(false);
		FoodIcon.SetActive(false);
		HappinessIcon.SetActive(false);
		SleepinessIcon.SetActive(false);
		IllnessIcon.SetActive(false);
		yield return new WaitForEndOfFrame();
		GetNotify();
		OnSatiationStateChanged(follower.Brain.Info.ID, (follower.Brain.Stats.Satiation <= 30f) ? FollowerStatState.On : FollowerStatState.Off, FollowerStatState.Off);
		FollowerBrainInfo info = follower.Brain.Info;
		info.OnReadyToPromote = (Action)Delegate.Combine(info.OnReadyToPromote, new Action(GetNotify));
		FollowerBrainInfo info2 = follower.Brain.Info;
		info2.OnPromotion = (Action)Delegate.Combine(info2.OnPromotion, new Action(GetNotify));
		FollowerBrainStats.OnSatiationStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Combine(FollowerBrainStats.OnSatiationStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnSatiationStateChanged));
		FoodIconRing.material.SetFloat("_Angle", 90f);
		FollowerBrainStats.OnHappinessStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Combine(FollowerBrainStats.OnHappinessStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnHappinessStateChanged));
		HappinessIconRing.material.SetFloat("_Angle", 90f);
		FollowerBrainStats.OnRestStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Combine(FollowerBrainStats.OnRestStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnRestStateChanged));
		SleepinesssIconRing.material.SetFloat("_Angle", 90f);
		FollowerBrainStats.OnIllnessStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Combine(FollowerBrainStats.OnIllnessStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnIllnessStateChanged));
		IllnessIconRing.material.SetFloat("_Angle", 90f);
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
		TimeManager.OnScheduleChanged = (Action)Delegate.Combine(TimeManager.OnScheduleChanged, new Action(OnNewPhaseStarted));
		if (follower.Brain.Stats.Satiation <= 0f)
		{
			FoodIcon.SetActive(true);
		}
		if (follower.Brain.Stats.Happiness <= 25f)
		{
			HappinessIcon.SetActive(true);
		}
		if (follower.Brain.Stats.Rest <= 20f)
		{
			SleepinessIcon.SetActive(true);
		}
		if (follower.Brain.Stats.Illness > 0f)
		{
			IllnessIcon.SetActive(true);
		}
	}

	public void ShowIcon(IconType Type)
	{
		switch (Type)
		{
		case IconType.Food:
			FoodIcon.SetActive(true);
			break;
		case IconType.Happiness:
			HappinessIcon.SetActive(true);
			break;
		case IconType.Sleep:
			SleepinessIcon.SetActive(true);
			break;
		case IconType.Illness:
			IllnessIcon.SetActive(true);
			break;
		}
	}

	public void HideIcon(IconType Type)
	{
		switch (Type)
		{
		case IconType.Food:
			if (follower.Brain.Stats.Satiation > 30f)
			{
				FoodIcon.SetActive(false);
			}
			break;
		case IconType.Happiness:
			if (follower.Brain.Stats.Happiness > 25f)
			{
				HappinessIcon.SetActive(false);
			}
			break;
		case IconType.Sleep:
			if (follower.Brain.Stats.Rest > 20f)
			{
				SleepinessIcon.SetActive(false);
			}
			break;
		}
	}

	private void OnNewPhaseStarted()
	{
		ScheduledActivity scheduledActivity = TimeManager.GetScheduledActivity(follower.Brain.HomeLocation);
		if (scheduledActivity == ScheduledActivity.Leisure)
		{
			HappinessIcon.SetActive(true);
		}
		else if (follower.Brain.Stats.Happiness > 25f)
		{
			HappinessIcon.SetActive(false);
		}
		if (scheduledActivity == ScheduledActivity.Leisure)
		{
			HappinessIcon.SetActive(true);
		}
		else if (follower.Brain.Stats.Happiness > 25f)
		{
			HappinessIcon.SetActive(false);
		}
	}

	private void OnHappinessStateChanged(int followerId, FollowerStatState newState, FollowerStatState oldState)
	{
		if (followerId == follower.Brain.Info.ID && TimeManager.GetScheduledActivity(follower.Brain.HomeLocation) != ScheduledActivity.Leisure)
		{
			HappinessIcon.SetActive(newState != FollowerStatState.Off);
		}
	}

	private void OnSatiationStateChanged(int followerId, FollowerStatState newState, FollowerStatState oldState)
	{
		if (followerId == follower.Brain.Info.ID)
		{
			FoodIcon.SetActive(newState == FollowerStatState.On);
		}
	}

	private void OnRestStateChanged(int followerId, FollowerStatState newState, FollowerStatState oldState)
	{
		if (followerId == follower.Brain.Info.ID)
		{
			SleepinessIcon.SetActive(newState == FollowerStatState.On);
		}
	}

	private void OnIllnessStateChanged(int followerId, FollowerStatState newState, FollowerStatState oldState)
	{
		if (followerId == follower.Brain.Info.ID)
		{
			IllnessIcon.SetActive(newState == FollowerStatState.On);
		}
	}

	private void Update()
	{
		FoodIconRing.material.SetFloat("_Arc1", 360f - (follower.Brain.Stats.Satiation + (75f - follower.Brain.Stats.Starvation)) / 175f * 360f);
		FoodIconRing.material.SetFloat("_Arc2", 0f);
		FoodIconRing.color = RadialGradient.Evaluate((follower.Brain.Stats.Satiation + (75f - follower.Brain.Stats.Starvation)) / 175f);
		HappinessIconRing.material.SetFloat("_Arc1", 360f - follower.Brain.Stats.Happiness / 100f * 360f);
		HappinessIconRing.material.SetFloat("_Arc2", 0f);
		HappinessIconRing.color = RadialGradient.Evaluate(follower.Brain.Stats.Happiness / 100f);
		SleepinesssIconRing.material.SetFloat("_Arc1", 360f - follower.Brain.Stats.Rest / 100f * 360f);
		SleepinesssIconRing.material.SetFloat("_Arc2", 0f);
		SleepinesssIconRing.color = RadialGradient.Evaluate(follower.Brain.Stats.Rest / 100f);
		IllnessIconRing.material.SetFloat("_Arc1", 360f - (1f - follower.Brain.Stats.Illness / 100f) * 360f);
		IllnessIconRing.material.SetFloat("_Arc2", 0f);
		IllnessIconRing.color = RadialGradient.Evaluate(1f - follower.Brain.Stats.Illness / 100f);
	}

	private void OnDisable()
	{
		FollowerBrainInfo info = follower.Brain.Info;
		info.OnReadyToPromote = (Action)Delegate.Remove(info.OnReadyToPromote, new Action(GetNotify));
		FollowerBrainInfo info2 = follower.Brain.Info;
		info2.OnPromotion = (Action)Delegate.Remove(info2.OnPromotion, new Action(GetNotify));
		FollowerBrainStats.OnSatiationStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Remove(FollowerBrainStats.OnSatiationStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnSatiationStateChanged));
		FollowerBrainStats.OnHappinessStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Remove(FollowerBrainStats.OnHappinessStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnHappinessStateChanged));
		FollowerBrainStats.OnRestStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Remove(FollowerBrainStats.OnRestStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnRestStateChanged));
		FollowerBrainStats.OnIllnessStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Remove(FollowerBrainStats.OnIllnessStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnIllnessStateChanged));
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
		TimeManager.OnScheduleChanged = (Action)Delegate.Remove(TimeManager.OnScheduleChanged, new Action(OnNewPhaseStarted));
	}

	private void GetNotify(int FollowerID)
	{
		if (FollowerID == follower.Brain.Info.ID)
		{
			GetNotify();
		}
	}

	private void GetNotify()
	{
		Hide();
	}

	private void Show()
	{
		if (!Image.activeSelf)
		{
			StartCoroutine(ShowRoutine());
		}
	}

	private IEnumerator ShowRoutine()
	{
		float Progress = 0f;
		float Duration = 0.5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			Image.transform.localScale = Vector3.one * Mathf.SmoothStep(0f, 1f, Progress / Duration);
			yield return null;
		}
		Image.SetActive(true);
	}

	public void HideAll()
	{
		base.gameObject.SetActive(false);
	}

	public void ShowAll()
	{
		base.gameObject.SetActive(true);
	}

	private void Hide()
	{
		if (Image.activeSelf)
		{
			StartCoroutine(HideRoutine());
		}
	}

	private IEnumerator HideRoutine()
	{
		float Progress = 0f;
		float Duration = 0.5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			Image.transform.localScale = Vector3.one * Mathf.SmoothStep(1f, 0f, Progress / Duration);
			yield return null;
		}
		Image.SetActive(false);
	}
}
