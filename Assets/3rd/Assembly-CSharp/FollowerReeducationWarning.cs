using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class FollowerReeducationWarning : MonoBehaviour, IUpdateManually
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
		if (Follower != null && Follower.Brain != null && Follower.Brain.Stats != null && Follower.Brain.Info.CursedState == Thought.Dissenter)
		{
			Show();
		}
		if (Follower != null && Follower.Brain != null)
		{
			FollowerBrain brain = Follower.Brain;
			brain.OnBecomeDissenter = (Action)Delegate.Combine(brain.OnBecomeDissenter, new Action(ToggleWarning));
			FollowerBrainStats stats = Follower.Brain.Stats;
			stats.OnReeducationComplete = (Action)Delegate.Combine(stats.OnReeducationComplete, new Action(ToggleWarning));
		}
	}

	private void OnDisable()
	{
		if (Follower != null && Follower.Brain != null)
		{
			FollowerBrain brain = Follower.Brain;
			brain.OnBecomeDissenter = (Action)Delegate.Remove(brain.OnBecomeDissenter, new Action(ToggleWarning));
			FollowerBrainStats stats = Follower.Brain.Stats;
			stats.OnReeducationComplete = (Action)Delegate.Remove(stats.OnReeducationComplete, new Action(ToggleWarning));
		}
	}

	private void DisableCanvasAnimations()
	{
	}

	public void ToggleWarning()
	{
		if (Follower != null && Follower.Brain != null && Follower.Brain.Stats != null && Follower.Brain.Stats.Reeducation < 100f)
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
		if (IsPlaying && ProgressBar != null && (delayTimer -= Time.deltaTime) <= 0f)
		{
			delayTimer -= Time.deltaTime;
			if (delayTimer <= 0f)
			{
				delayTimer = 0.6f;
				float num = Mathf.Max(Follower.Brain.Stats.Reeducation / 100f, 0.1f);
				Color color = Gradient.Evaluate(num);
				ProgressBar.color = new Color(color.r, color.g, color.b, num);
				if (Follower.Brain.Info.CursedState != Thought.Dissenter)
				{
					Hide();
				}
			}
		}
		else if (!IsPlaying && Follower.Brain.Info.CursedState == Thought.Dissenter)
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
