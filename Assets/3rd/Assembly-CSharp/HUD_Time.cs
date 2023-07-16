using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD_Time : BaseMonoBehaviour
{
	private Vector3 StartPos;

	private Vector3 MovePos;

	public TextMeshProUGUI DayLabel;

	public Transform Clockhand;

	[Header("Red Overlay")]
	[SerializeField]
	private Image _redOverlay;

	[SerializeField]
	private Color _nightTimeRed;

	[SerializeField]
	private Color _defaultRed;

	[SerializeField]
	private CanvasGroup _speedUpTime;

	private bool timescaleChanged;

	private void OnEnable()
	{
		SaveAndLoad.OnLoadComplete = (Action)Delegate.Combine(SaveAndLoad.OnLoadComplete, new Action(Init));
		TimeManager.OnNewDayStarted = (Action)Delegate.Combine(TimeManager.OnNewDayStarted, new Action(OnNewDay));
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
		DayLabel.text = string.Format("{0}", TimeManager.CurrentDay);
		if (SaveAndLoad.Loaded)
		{
			Init();
		}
		_speedUpTime.alpha = 0f;
	}

	private void Init()
	{
		OnNewDay();
	}

	private void Update()
	{
		Clockhand.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -360f, TimeManager.CurrentDayProgress));
		if (Time.timeScale > 1f)
		{
			if (!timescaleChanged)
			{
				_speedUpTime.DOFade(1f, 0.5f);
				timescaleChanged = true;
			}
		}
		else if (timescaleChanged)
		{
			_speedUpTime.DOFade(0f, 0.5f);
			timescaleChanged = false;
		}
	}

	private void OnNewDay()
	{
		DayLabel.text = string.Format("{0}", TimeManager.CurrentDay);
		DayLabel.transform.DOKill();
		DayLabel.transform.DOShakeScale(0.75f, 0.5f);
	}

	private void OnDisable()
	{
		SaveAndLoad.OnLoadComplete = (Action)Delegate.Remove(SaveAndLoad.OnLoadComplete, new Action(Init));
		TimeManager.OnNewDayStarted = (Action)Delegate.Remove(TimeManager.OnNewDayStarted, new Action(OnNewDay));
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
	}

	private void OnNewPhaseStarted()
	{
		_redOverlay.DOKill();
		if (TimeManager.CurrentPhase == DayPhase.Night)
		{
			_redOverlay.DOColor(_nightTimeRed, 1f).SetUpdate(true);
		}
		else
		{
			_redOverlay.DOColor(_defaultRed, 1f).SetUpdate(true);
		}
	}
}
