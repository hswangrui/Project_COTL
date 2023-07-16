using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HUD_TimerToken : BaseMonoBehaviour
{
	public Image CircleImage;

	public Image HourGlassImage;

	public Image PauseImage;

	public RectTransform rectTransform;

	public bool FlashWhenActive;

	private Coroutine cWarningFlash;

	public float fillAmount
	{
		get
		{
			return CircleImage.fillAmount;
		}
		set
		{
			if (base.gameObject.activeSelf && FlashWhenActive)
			{
				if (value < 1f && cWarningFlash == null)
				{
					cWarningFlash = StartCoroutine(WarningFlash());
				}
				if (value >= 1f && cWarningFlash != null)
				{
					StopCoroutine(cWarningFlash);
					CircleImage.color = Color.white;
					CircleImage.rectTransform.localScale = Vector3.one;
				}
			}
			CircleImage.fillAmount = value;
		}
	}

	public bool TimerPaused
	{
		set
		{
			if (value)
			{
				HourGlassImage.enabled = false;
				PauseImage.enabled = true;
			}
			else
			{
				HourGlassImage.enabled = true;
				PauseImage.enabled = false;
			}
		}
	}

	private void OnEnable()
	{
		HUD_Timer.OnPauseTimer += OnPauseTimer;
		HUD_Timer.OnUnPauseTimer += OnUnPauseTimer;
		StartCoroutine(DoScale());
		TimerPaused = HUD_Timer.TimerPaused;
	}

	private IEnumerator WarningFlash()
	{
		bool Loop = true;
		float FlashRed = 180f;
		float FlashScale = 180f;
		while (Loop)
		{
			Image circleImage = CircleImage;
			Color white = Color.white;
			Color red = Color.red;
			float f;
			FlashRed = (f = FlashRed + Time.deltaTime * 4f);
			circleImage.color = Color.Lerp(white, red, 0.5f + 0.5f * Mathf.Cos(f));
			RectTransform obj = CircleImage.rectTransform;
			Vector3 one = Vector3.one;
			FlashScale = (f = FlashScale + Time.deltaTime * 5f);
			obj.localScale = one * (1f + 0.2f * Mathf.Cos(f));
			if (fillAmount < 1f)
			{
				yield return null;
			}
			else
			{
				Loop = false;
			}
		}
		CircleImage.color = Color.white;
		CircleImage.rectTransform.localScale = Vector3.one;
		cWarningFlash = null;
	}

	private void OnDisable()
	{
		HUD_Timer.OnPauseTimer -= OnPauseTimer;
		HUD_Timer.OnUnPauseTimer -= OnUnPauseTimer;
		StopAllCoroutines();
	}

	private void OnPauseTimer()
	{
		TimerPaused = true;
	}

	private void OnUnPauseTimer()
	{
		TimerPaused = false;
	}

	private IEnumerator DoScale()
	{
		float Scale = 0f;
		float ScaleSpeed2 = 0f;
		float Timer = 0f;
		rectTransform.localScale = Vector3.one * Scale;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (num < 2f)
			{
				ScaleSpeed2 += (1f - Scale) * 0.4f;
				float num2 = Scale;
				ScaleSpeed2 = (num = ScaleSpeed2 * 0.6f);
				Scale = num2 + num;
				rectTransform.localScale = Vector3.one * Scale;
				yield return null;
				continue;
			}
			break;
		}
	}
}
