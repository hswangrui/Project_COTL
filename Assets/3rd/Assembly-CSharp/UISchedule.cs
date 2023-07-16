using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UISchedule : BaseMonoBehaviour
{
	public Animator Animator;

	public UINavigator uiNavigator;

	public TextMeshProUGUI DawnLabel;

	public TextMeshProUGUI MorningLabel;

	public TextMeshProUGUI AfternoonLabel;

	public TextMeshProUGUI DuskLabel;

	public TextMeshProUGUI NightLabel;

	public TextMeshProUGUI MealTimeLabel;

	public Action CallbackClose;

	private Action _callback;

	private Dictionary<DayPhase, ScheduledActivity> _workingSchedule;

	private Dictionary<InstantActivity, int> _workingInstantActivities;

	private void Start()
	{
		UINavigator uINavigator = uiNavigator;
		uINavigator.OnClose = (UINavigator.Close)Delegate.Combine(uINavigator.OnClose, new UINavigator.Close(Close));
		_workingSchedule = new Dictionary<DayPhase, ScheduledActivity>();
		for (int i = 0; i < 5; i++)
		{
			_workingSchedule[(DayPhase)i] = TimeManager.GetScheduledActivity((DayPhase)i);
		}
		_workingInstantActivities = new Dictionary<InstantActivity, int>();
		for (int j = 0; j < 1; j++)
		{
			_workingInstantActivities[(InstantActivity)j] = TimeManager.GetInstantActivityHour((InstantActivity)j);
		}
		UpdateLabels();
	}

	private void Update()
	{
		if (DataManager.Instance.SchedulingEnabled)
		{
			if (uiNavigator.CurrentSelection < 5)
			{
				DayPhase currentSelection = (DayPhase)uiNavigator.CurrentSelection;
				if (InputManager.UI.GetPageNavigateLeftDown())
				{
					PrevActivity(currentSelection);
				}
				if (InputManager.UI.GetPageNavigateRightDown())
				{
					NextActivity(currentSelection);
				}
			}
			else
			{
				InstantActivity activity = (InstantActivity)(uiNavigator.CurrentSelection - 5);
				if (InputManager.UI.GetPageNavigateLeftDown())
				{
					PrevHour(activity);
				}
				if (InputManager.UI.GetPageNavigateRightDown())
				{
					NextHour(activity);
				}
			}
		}
		DawnLabel.color = ((TimeManager.CurrentPhase == DayPhase.Dawn) ? Color.yellow : Color.white);
		MorningLabel.color = ((TimeManager.CurrentPhase == DayPhase.Morning) ? Color.yellow : Color.white);
		AfternoonLabel.color = ((TimeManager.CurrentPhase == DayPhase.Afternoon) ? Color.yellow : Color.white);
		DuskLabel.color = ((TimeManager.CurrentPhase == DayPhase.Dusk) ? Color.yellow : Color.white);
		NightLabel.color = ((TimeManager.CurrentPhase == DayPhase.Night) ? Color.yellow : Color.white);
	}

	private void OnDisable()
	{
		UINavigator uINavigator = uiNavigator;
		uINavigator.OnClose = (UINavigator.Close)Delegate.Remove(uINavigator.OnClose, new UINavigator.Close(Close));
	}

	public void SelectDawn()
	{
		NextActivity(DayPhase.Dawn);
	}

	public void SelectMorning()
	{
		NextActivity(DayPhase.Morning);
	}

	public void SelectAfternoon()
	{
		NextActivity(DayPhase.Afternoon);
	}

	public void SelectDusk()
	{
		NextActivity(DayPhase.Dusk);
	}

	public void SelectNight()
	{
		NextActivity(DayPhase.Night);
	}

	public void SelectMealTime()
	{
		NextHour(InstantActivity.MealTime);
	}

	private void NextActivity(DayPhase phase)
	{
		if (DataManager.Instance.SchedulingEnabled)
		{
			ScheduledActivity value = (ScheduledActivity)((int)(_workingSchedule[phase] + 1) % 5);
			_workingSchedule[phase] = value;
			UpdateLabels();
		}
	}

	private void PrevActivity(DayPhase phase)
	{
		if (DataManager.Instance.SchedulingEnabled)
		{
			ScheduledActivity scheduledActivity = _workingSchedule[phase];
			ScheduledActivity value = ((scheduledActivity == ScheduledActivity.Work) ? ScheduledActivity.Count : scheduledActivity) - 1;
			_workingSchedule[phase] = value;
			UpdateLabels();
		}
	}

	private void NextHour(InstantActivity activity)
	{
		if (DataManager.Instance.SchedulingEnabled)
		{
			int value = (_workingInstantActivities[activity] + 1) % 20;
			_workingInstantActivities[activity] = value;
			UpdateLabels();
		}
	}

	private void PrevHour(InstantActivity activity)
	{
		if (DataManager.Instance.SchedulingEnabled)
		{
			int num = _workingInstantActivities[activity];
			int value = ((num == 0) ? 20 : num) - 1;
			_workingInstantActivities[activity] = value;
			UpdateLabels();
		}
	}

	private void UpdateLabels()
	{
		DawnLabel.text = string.Format("Dawn: {0}", _workingSchedule[DayPhase.Dawn]);
		MorningLabel.text = string.Format("Morning: {0}", _workingSchedule[DayPhase.Morning]);
		AfternoonLabel.text = string.Format("Afternoon: {0}", _workingSchedule[DayPhase.Afternoon]);
		DuskLabel.text = string.Format("Dusk: {0}", _workingSchedule[DayPhase.Dusk]);
		NightLabel.text = string.Format("Night: {0}", _workingSchedule[DayPhase.Night]);
		MealTimeLabel.text = string.Format("MealTime: {0:D2}:00", _workingInstantActivities[InstantActivity.MealTime]);
	}

	private void Close()
	{
		_callback = CallbackClose;
		StartCoroutine(CloseRoutine());
		if (!DataManager.Instance.SchedulingEnabled)
		{
			return;
		}
		bool flag = _workingSchedule[TimeManager.CurrentPhase] != TimeManager.GetScheduledActivity(TimeManager.CurrentPhase);
		foreach (KeyValuePair<DayPhase, ScheduledActivity> item in _workingSchedule)
		{
			TimeManager.SetScheduledActivity(item.Key, item.Value);
		}
		foreach (KeyValuePair<InstantActivity, int> workingInstantActivity in _workingInstantActivities)
		{
			TimeManager.SetInstantActivity(workingInstantActivity.Key, workingInstantActivity.Value);
		}
		if (flag)
		{
			TimeManager.SetOverrideScheduledActivity(ScheduledActivity.None);
			return;
		}
		Action onScheduleChanged = TimeManager.OnScheduleChanged;
		if (onScheduleChanged != null)
		{
			onScheduleChanged();
		}
	}

	private IEnumerator CloseRoutine()
	{
		Animator.Play("Base Layer.Out");
		yield return new WaitForSeconds(0.5f);
		Action callback = _callback;
		if (callback != null)
		{
			callback();
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
