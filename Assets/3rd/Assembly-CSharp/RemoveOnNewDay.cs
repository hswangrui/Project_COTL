using System;
using UnityEngine;

public class RemoveOnNewDay : BaseMonoBehaviour
{
	private enum State
	{
		Entering,
		Leaving,
		Idle
	}

	private State CurrentState;

	private float TransitionProgress;

	public GameObject TransitionObject;

	public Vector3 StartingPosition;

	private float RandomOffset;

	[SerializeField]
	private SimpleInventory inventory;

	private void Start()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
		CurrentState = State.Entering;
		RandomOffset = UnityEngine.Random.Range(0f, 3f);
		TransitionObject.transform.localPosition = new Vector3(0f, 0f, 0.5f);
	}

	private void Update()
	{
		switch (CurrentState)
		{
		case State.Entering:
			if ((RandomOffset -= Time.deltaTime) < 0f)
			{
				TransitionObject.transform.localPosition = Vector3.Lerp(new Vector3(0f, 0f, 0.5f), Vector3.zero, Mathf.SmoothStep(0f, 1f, (TransitionProgress += Time.deltaTime) / 1f));
			}
			if (TransitionProgress >= 1f)
			{
				TransitionObject.transform.localPosition = Vector3.zero;
				TransitionProgress = 0f;
				CurrentState = State.Idle;
			}
			break;
		case State.Leaving:
			if ((RandomOffset -= Time.deltaTime) < 0f)
			{
				TransitionObject.transform.localPosition = Vector3.Lerp(Vector3.zero, new Vector3(0f, 0f, 0.5f), Mathf.SmoothStep(0f, 1f, (TransitionProgress += Time.deltaTime) / 1f));
			}
			if (TransitionProgress >= 1f)
			{
				base.gameObject.Recycle();
			}
			if (inventory != null && DataManager.AllNecklaces.Contains(inventory.Item))
			{
				inventory.DropItem();
			}
			break;
		}
	}

	private void OnNewPhaseStarted()
	{
		if (TimeManager.CurrentPhase != DayPhase.Night)
		{
			RandomOffset = UnityEngine.Random.Range(0f, 3f);
			CurrentState = State.Leaving;
		}
	}

	private void OnDestroy()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
	}
}
