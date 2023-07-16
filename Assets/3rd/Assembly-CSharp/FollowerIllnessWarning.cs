using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class FollowerIllnessWarning : MonoBehaviour, IUpdateManually
{
	public Follower Follower;

	public GameObject Container;

	public SpriteRenderer ProgressBar;

	public Gradient Gradient;

	private const float DELAY_BETWEEN_UPDATES = 0.6f;

	private float delayTimer;

	private bool IsPlaying;

	private Vector3 originalScale;

	private void OnEnable()
	{
		StartCoroutine(Init());
	}

	private void Awake()
	{
		originalScale = Container.transform.localScale;
		Container.SetActive(false);
		DisableCanvasAnimations();
	}

	private IEnumerator Init()
	{
		yield return new WaitForEndOfFrame();
		if (Follower != null && Follower.Brain != null && Follower.Brain.Stats != null && Follower.Brain.Info.CursedState == Thought.Ill)
		{
			Show();
		}
		FollowerBrainStats.OnIllnessStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Combine(FollowerBrainStats.OnIllnessStateChanged, new FollowerBrainStats.StatStateChangedEvent(ToggleWarning));
	}

	private void OnDisable()
	{
		FollowerBrainStats.OnIllnessStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Remove(FollowerBrainStats.OnIllnessStateChanged, new FollowerBrainStats.StatStateChangedEvent(ToggleWarning));
	}

	private void DisableCanvasAnimations()
	{
	}

	private void ToggleWarning(int followerid, FollowerStatState newstate, FollowerStatState oldstate)
	{
		if (Follower != null && Follower.Brain != null && Follower.Brain.Stats != null && Follower.Brain.Info.CursedState == Thought.Ill)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public void UpdateManually()
	{
		Update();
	}

	private void Update()
	{
		if (!(Follower != null) || Follower.Brain == null || Follower.Brain.Stats == null)
		{
			return;
		}
		if (IsPlaying && ProgressBar != null)
		{
			delayTimer -= Time.deltaTime;
			if (delayTimer <= 0f)
			{
				delayTimer = 0.6f;
				float num = Mathf.Max(Follower.Brain.Stats.Illness / 100f, 0.1f);
				Color color = Gradient.Evaluate(num);
				ProgressBar.color = new Color(color.r, color.g, color.b, num);
			}
		}
		else if (!IsPlaying && Follower.Brain.Info.CursedState == Thought.Ill)
		{
			Show();
		}
	}

	public void Show()
	{
		if (!Container.activeInHierarchy)
		{
			Container.SetActive(true);
			Container.transform.localScale = Vector3.zero;
			Container.transform.DOKill();
			Container.transform.DOScale(originalScale, 0.5f).SetEase(Ease.OutBack);
			IsPlaying = true;
		}
	}

	public void Hide()
	{
		if (Container.activeInHierarchy)
		{
			Container.transform.localScale = originalScale;
			Container.transform.DOKill();
			Container.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).OnComplete(delegate
			{
				Container.SetActive(false);
			})
				.OnComplete(delegate
				{
					Container.SetActive(false);
				});
			IsPlaying = false;
		}
	}
}
