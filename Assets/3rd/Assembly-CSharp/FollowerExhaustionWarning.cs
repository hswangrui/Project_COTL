using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class FollowerExhaustionWarning : MonoBehaviour, IUpdateManually
{
	public Follower Follower;

	public GameObject Container;

	public SpriteRenderer ProgressBar;

	public Gradient Gradient;

	private bool IsPlaying;

	private Vector3 originalScale;

	private const float DELAY_BETWEEN_UPDATES = 0.6f;

	private float delayTimer;

	private void OnEnable()
	{
		StartCoroutine(Init());
		FollowerBrainStats.OnExhaustionStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Combine(FollowerBrainStats.OnExhaustionStateChanged, new FollowerBrainStats.StatStateChangedEvent(ToggleWarning));
	}

	private void Awake()
	{
		originalScale = Container.transform.localScale;
		Container.SetActive(false);
		DisableCanvasAnimations();
	}

	private void Start()
	{
		Follower follower = Follower;
		follower.OnFollowerBrainAssigned = (Action)Delegate.Combine(follower.OnFollowerBrainAssigned, new Action(OnBrainAssigned));
		if (Follower.Brain != null)
		{
			OnBrainAssigned();
		}
	}

	private void OnBrainAssigned()
	{
		Follower follower = Follower;
		follower.OnFollowerBrainAssigned = (Action)Delegate.Remove(follower.OnFollowerBrainAssigned, new Action(OnBrainAssigned));
		FollowerBrain brain = Follower.Brain;
		brain.OnStateChanged = (Action<FollowerState, FollowerState>)Delegate.Combine(brain.OnStateChanged, new Action<FollowerState, FollowerState>(OnStateChanged));
	}

	private void OnStateChanged(FollowerState newState, FollowerState oldState)
	{
		if (newState != null && newState.Type == FollowerStateType.Exhausted)
		{
			StartCoroutine(Init());
		}
	}

	private IEnumerator Init()
	{
		yield return new WaitForEndOfFrame();
		if (Follower != null && Follower.Brain != null && Follower.Brain.Stats != null)
		{
			FollowerState currentState = Follower.Brain.CurrentState;
			if (currentState != null && currentState.Type == FollowerStateType.Exhausted)
			{
				Show();
			}
		}
	}

	private void OnDestroy()
	{
		FollowerBrainStats.OnExhaustionStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Remove(FollowerBrainStats.OnExhaustionStateChanged, new FollowerBrainStats.StatStateChangedEvent(ToggleWarning));
		if (Follower != null && Follower.Brain != null)
		{
			FollowerBrain brain = Follower.Brain;
			brain.OnStateChanged = (Action<FollowerState, FollowerState>)Delegate.Remove(brain.OnStateChanged, new Action<FollowerState, FollowerState>(OnStateChanged));
		}
	}

	private void OnDisable()
	{
		FollowerBrainStats.OnExhaustionStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Remove(FollowerBrainStats.OnExhaustionStateChanged, new FollowerBrainStats.StatStateChangedEvent(ToggleWarning));
		if (Follower != null && Follower.Brain != null)
		{
			FollowerBrain brain = Follower.Brain;
			brain.OnStateChanged = (Action<FollowerState, FollowerState>)Delegate.Remove(brain.OnStateChanged, new Action<FollowerState, FollowerState>(OnStateChanged));
		}
	}

	private void DisableCanvasAnimations()
	{
	}

	private void ToggleWarning(int followerid, FollowerStatState newstate, FollowerStatState oldstate)
	{
		if (Follower != null && Follower.Brain != null)
		{
			FollowerState currentState = Follower.Brain.CurrentState;
			if (currentState != null && currentState.Type == FollowerStateType.Exhausted)
			{
				Show();
				return;
			}
		}
		Hide();
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
				float num = Mathf.Max(Follower.Brain.Stats.Exhaustion / 100f, 0.1f);
				Color color = Gradient.Evaluate(num);
				ProgressBar.color = new Color(color.r, color.g, color.b, num);
			}
		}
		else if (!IsPlaying && Follower.Brain.Stats.Exhaustion > 0f)
		{
			Show();
		}
	}

	private void Show()
	{
		if (!(this == null) && !Container.activeInHierarchy)
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
		if (!(this == null) && Container.activeInHierarchy)
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
