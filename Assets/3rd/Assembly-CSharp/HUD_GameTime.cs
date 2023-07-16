using System;
using System.Text;
using TMPro;
using UnityEngine.UI;

public class HUD_GameTime : BaseMonoBehaviour
{
	public TextMeshProUGUI DayLabel;

	public TextMeshProUGUI TimeLabel;

	public TextMeshProUGUI PhaseLabel;

	public TextMeshProUGUI ScheduledLabel;

	public Image PhaseProgress;

	private void OnEnable()
	{
		SaveAndLoad.OnLoadComplete = (Action)Delegate.Combine(SaveAndLoad.OnLoadComplete, new Action(Init));
		TimeManager.OnNewDayStarted = (Action)Delegate.Combine(TimeManager.OnNewDayStarted, new Action(OnNewDay));
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhase));
		TimeManager.OnScheduleChanged = (Action)Delegate.Combine(TimeManager.OnScheduleChanged, new Action(OnScheduleChanged));
		if (SaveAndLoad.Loaded)
		{
			Init();
		}
	}

	private void OnDisable()
	{
		SaveAndLoad.OnLoadComplete = (Action)Delegate.Remove(SaveAndLoad.OnLoadComplete, new Action(Init));
		TimeManager.OnNewDayStarted = (Action)Delegate.Remove(TimeManager.OnNewDayStarted, new Action(OnNewDay));
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhase));
	}

	private void Init()
	{
		OnNewDay();
		OnNewPhase();
	}

	private void Update()
	{
		PhaseProgress.fillAmount = TimeManager.CurrentPhaseProgress;
	}

	private void OnNewDay()
	{
		PhaseLabel.text = string.Format("{0} ", TimeManager.CurrentPhase) + string.Format("Day {0}", TimeManager.CurrentDay);
	}

	private void OnNewPhase()
	{
		PhaseLabel.text = string.Format("{0} ", TimeManager.CurrentPhase) + string.Format("Day {0}", TimeManager.CurrentDay);
		SetScheduleLabel();
	}

	private void OnScheduleChanged()
	{
		SetScheduleLabel();
	}

	private void SetScheduleLabel()
	{
		if (DataManager.Instance.Followers.Count > 0)
		{
			ScheduledActivity scheduledActivity = TimeManager.GetScheduledActivity(TimeManager.CurrentPhase);
			ScheduledActivity scheduledActivity2 = TimeManager.GetScheduledActivity(FollowerLocation.Base);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(scheduledActivity);
			if (scheduledActivity != scheduledActivity2)
			{
				stringBuilder.Append(string.Format(" ({0})", scheduledActivity2));
			}
			ScheduledLabel.text = scheduledActivity2.ToString();
		}
		else
		{
			ScheduledLabel.text = "";
		}
	}
}
