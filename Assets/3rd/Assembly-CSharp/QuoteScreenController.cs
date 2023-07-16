using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using MMTools;
using TMPro;
using UnityEngine;

public class QuoteScreenController : BaseMonoBehaviour
{
	public enum QuoteTypes
	{
		IntroQuote,
		QuoteBoss1,
		QuoteBoss2,
		QuoteBoss3,
		QuoteBoss4,
		QuoteBoss5,
		IntroQuote2,
		QuoteBoss6,
		QuoteBoss7,
		QuoteBoss8,
		QuoteBoss9
	}

	private static List<QuoteTypes> QuoteType;

	public float fadeIn = 1f;

	private static Action SkipCallback;

	private static Action Callback;

	public TextMeshProUGUI Text;

	private static int CURRENT_QUOTE_INDEX;

	private void Start()
	{
		AudioManager.Instance.PlayMusic("event:/music/menu/intro_narration_ambience");
		if (LocalizationManager.CurrentLanguage == "English")
		{
			QuoteTypes quoteTypes = QuoteType[0];
		}
		SetQuote(false);
		MonoSingleton<UIManager>.Instance.ForceBlockMenus = true;
	}

	private void SetQuote(bool FadeText)
	{
		if (FadeText)
		{
			Color TargetColor = Text.color;
			Text.DOColor(Color.black, 2f).OnComplete(delegate
			{
				Text.text = LocalizationManager.GetTranslation(string.Format("QUOTE/{0}", QuoteType[CURRENT_QUOTE_INDEX]));
				AudioManager.Instance.PlayOneShot("event:/dialogue/shop_tarot_clauneck/standard_clauneck");
				StartCoroutine(DoScreen(7f));
				Text.DOColor(TargetColor, 2f).OnComplete(delegate
				{
					StartCoroutine(SkipScreen());
				});
			});
		}
		else
		{
			AudioManager.Instance.PlayOneShotDelayed("event:/dialogue/shop_tarot_clauneck/standard_clauneck", 1f);
			Text.text = LocalizationManager.GetTranslation(string.Format("QUOTE/{0}", QuoteType[CURRENT_QUOTE_INDEX]));
			StartCoroutine(DoScreen(7f));
			StartCoroutine(SkipScreen());
			MMTransition.ResumePlay();
		}
	}

	public static void Init(List<QuoteTypes> QuoteType, Action SkipCallback, Action Callback)
	{
		CURRENT_QUOTE_INDEX = 0;
		QuoteScreenController.QuoteType = QuoteType;
		QuoteScreenController.SkipCallback = SkipCallback;
		QuoteScreenController.Callback = Callback;
	}

	private IEnumerator SkipScreen()
	{
		yield return new WaitForSeconds(0.5f);
		while (!Input.GetKeyDown(KeyCode.Escape))
		{
			yield return null;
		}
		AudioManager.Instance.StopCurrentMusic();
		MMTransition.StopCurrentTransition();
		StopAllCoroutines();
		Action skipCallback = SkipCallback;
		if (skipCallback != null)
		{
			skipCallback();
		}
	}

	private IEnumerator DoScreen(float waitTime)
	{
		yield return new WaitForSeconds(0.5f);
		MMVibrate.Rumble(0.025f, 0.05f, 3f, this);
		yield return new WaitForSeconds(waitTime - 0.5f);
		StopAllCoroutines();
		if (++CURRENT_QUOTE_INDEX < QuoteType.Count)
		{
			SetQuote(true);
			yield break;
		}
		AudioManager.Instance.StopCurrentMusic();
		Action callback = Callback;
		if (callback != null)
		{
			callback();
		}
	}

	private void OnDisable()
	{
		AudioManager.Instance.StopCurrentMusic();
		MonoSingleton<UIManager>.Instance.ForceBlockMenus = false;
	}
}
