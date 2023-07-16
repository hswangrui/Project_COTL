using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaithIndicator : BaseMonoBehaviour
{
	public Follower follower;

	public GameObject ArrowUp;

	public GameObject ArrowDoubleUp;

	public GameObject ArrowDown;

	public GameObject ArrowDoubleDown;

	public GameObject Container;

	public SpriteRenderer Icon;

	public Sprite FaithIcon;

	public Sprite SicknessIcon;

	public Sprite HungerIcon;

	public Sprite SleepIcon;

	private List<Sprite> DisplayQueue = new List<Sprite>();

	private List<GameObject> DisplayQueueArrow = new List<GameObject>();

	private Coroutine cShowIcon;

	private void OnEnable()
	{
		Follower obj = follower;
		obj.OnFollowerBrainAssigned = (Action)Delegate.Combine(obj.OnFollowerBrainAssigned, new Action(OnFollowerBrainAssigned));
		FollowerBrainStats.OnIllnessChanged = (FollowerBrainStats.StatChangedEvent)Delegate.Combine(FollowerBrainStats.OnIllnessChanged, new FollowerBrainStats.StatChangedEvent(OnIllness));
		HideAll();
		if (follower.Brain != null)
		{
			OnFollowerBrainAssigned();
		}
	}

	private void OnFollowerBrainAssigned()
	{
		FollowerBrain brain = follower.Brain;
		brain.OnNewThought = (Action<float>)Delegate.Combine(brain.OnNewThought, new Action<float>(OnNewThought));
	}

	private void HideAll()
	{
		ArrowUp.SetActive(false);
		ArrowDoubleUp.SetActive(false);
		ArrowDown.SetActive(false);
		ArrowDoubleDown.SetActive(false);
		Container.SetActive(false);
	}

	private void OnDisable()
	{
		if (follower.Brain != null)
		{
			FollowerBrain brain = follower.Brain;
			brain.OnNewThought = (Action<float>)Delegate.Remove(brain.OnNewThought, new Action<float>(OnNewThought));
		}
		FollowerBrainStats.OnIllnessChanged = (FollowerBrainStats.StatChangedEvent)Delegate.Remove(FollowerBrainStats.OnIllnessChanged, new FollowerBrainStats.StatChangedEvent(OnIllness));
	}

	private void OnIllness(int followerID, float newValue, float oldValue, float change)
	{
		if (followerID == follower.Brain.Info.ID)
		{
			if (newValue < oldValue)
			{
				AddToQueue(SicknessIcon, ArrowUp);
			}
			if (newValue > oldValue)
			{
				AddToQueue(SicknessIcon, ArrowDown);
			}
		}
	}

	private void OnNewThought(float Delta)
	{
		GameObject arrow = null;
		if (Delta <= -7f)
		{
			arrow = ArrowDoubleDown;
		}
		else if (Delta < 0f)
		{
			arrow = ArrowDown;
		}
		else if (Delta >= 7f)
		{
			arrow = ArrowDoubleUp;
		}
		else if (Delta >= 0f)
		{
			arrow = ArrowUp;
		}
		AddToQueue(FaithIcon, arrow);
	}

	public void AddToQueue(Sprite Icon, GameObject Arrow)
	{
		DisplayQueue.Add(Icon);
		DisplayQueueArrow.Add(Arrow);
		if (DisplayQueue.Count <= 1)
		{
			PlayQueue();
		}
	}

	private void PlayQueue()
	{
		if (DisplayQueue.Count > 0)
		{
			if (cShowIcon != null)
			{
				StopCoroutine(cShowIcon);
			}
			cShowIcon = StartCoroutine(ShowIcon(DisplayQueue[0], DisplayQueueArrow[0]));
		}
	}

	private IEnumerator ShowIcon(Sprite Sprite, GameObject Arrow)
	{
		HideAll();
		Container.SetActive(true);
		Arrow.SetActive(true);
		Icon.sprite = Sprite;
		Container.transform.localPosition = Vector3.zero;
		float Progress = 0f;
		float Duration2 = 0.3f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration2))
			{
				break;
			}
			Container.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, Mathf.SmoothStep(0f, 1f, Progress / Duration2));
			yield return null;
		}
		yield return new WaitForSeconds(2.5f);
		Progress = 0f;
		Duration2 = 0.1f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration2))
			{
				break;
			}
			Container.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, Mathf.SmoothStep(1f, 0f, Progress / Duration2));
			yield return null;
		}
		Container.SetActive(false);
		DisplayQueue.RemoveAt(0);
		DisplayQueueArrow.RemoveAt(0);
		PlayQueue();
	}
}
