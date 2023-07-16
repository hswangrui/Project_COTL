using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HUD_Timer : BaseMonoBehaviour
{
	public delegate void PauseTimer();

	public delegate void UnPauseTimer();

	public LayoutElement layoutElementComponent;

	public static bool _TimerPaused;

	private static bool _TimerRunning;

	public HUD_TimerToken[] icons;

	private static float StartingTotalTime;

	public static bool TimerPaused
	{
		get
		{
			return _TimerPaused;
		}
		set
		{
			if (_TimerPaused != value)
			{
				if (value)
				{
					if (HUD_Timer.OnPauseTimer != null)
					{
						HUD_Timer.OnPauseTimer();
					}
				}
				else if (HUD_Timer.OnUnPauseTimer != null)
				{
					HUD_Timer.OnUnPauseTimer();
				}
			}
			_TimerPaused = value;
		}
	}

	public static bool TimerRunning
	{
		get
		{
			return _TimerRunning;
		}
		set
		{
			_TimerRunning = value;
			if (_TimerRunning)
			{
				StartingTotalTime = Timer;
			}
		}
	}

	public static float Timer
	{
		get
		{
			return DataManager.Instance.DUNGEON_TIME;
		}
		set
		{
			DataManager.Instance.DUNGEON_TIME = value;
		}
	}

	public static bool IsTimeUp
	{
		get
		{
			return Timer <= 0f;
		}
	}

	public static float Progress
	{
		get
		{
			return Mathf.Min(Timer / StartingTotalTime, 1f);
		}
	}

	public static event PauseTimer OnPauseTimer;

	public static event UnPauseTimer OnUnPauseTimer;

	public static float GetTimer()
	{
		return Timer;
	}

	public static bool GetTimeRunning()
	{
		return TimerRunning;
	}

	private void Start()
	{
		layoutElementComponent.ignoreLayout = true;
		UpdateAndSetTime();
	}

	private void Update()
	{
		UpdateAndSetTime();
		if (TimerRunning && layoutElementComponent.ignoreLayout)
		{
			layoutElementComponent.ignoreLayout = false;
		}
	}

	private void UpdateAndSetTime()
	{
		if (!IsTimeUp && Timer - Time.deltaTime <= 0f)
		{
			AudioManager.Instance.SetMusicCombatState();
		}
		if (TimerRunning && !TimerPaused)
		{
			Timer -= Time.deltaTime;
		}
		int num = -1;
		int num2 = Mathf.FloorToInt(Timer / 60f);
		float num3 = 0f;
		while (++num < icons.Length)
		{
			if (num > num2 || Timer <= 0f)
			{
				if (icons[num].gameObject.activeSelf)
				{
					icons[num].gameObject.SetActive(false);
				}
			}
			else if (num == num2)
			{
				if (!icons[num].gameObject.activeSelf)
				{
					StartCoroutine(EnableIcon(icons[num], num3 += 0.1f));
				}
				icons[num].fillAmount = (Timer - (float)(60 * num2)) / 60f;
			}
			else
			{
				icons[num].fillAmount = 1f;
				if (!icons[num].gameObject.activeSelf)
				{
					StartCoroutine(EnableIcon(icons[num], num3 += 0.1f));
				}
			}
		}
	}

	private IEnumerator EnableIcon(HUD_TimerToken icon, float Delay)
	{
		yield return new WaitForSeconds(Delay);
		icon.gameObject.SetActive(true);
	}
}
