using System;
using I2.Loc;
using TMPro;
using Unify;
using UnityEngine;
using UnityEngine.UI;

public class LanguageModifier : MonoBehaviour
{
	public LanguageModifier Parent;

	public string Token;

	public UnifyManager.Platform[] Platforms;

	public string EnglishText;

	public string GermanText;

	public string FrenchText;

	public string SpanishText;

	public string RussianText;

	public string SChineseText;

	public string TChineseText;

	public string JapaneseText;

	public string KoreanText;

	public string PortugueseText;

	public string BrazilianPortugueseText;

	public string TurkishText;

	public string ItalianText;

	private Text text;

	private TextMeshProUGUI text_TMP;

	private UnifyManager unifyManager;

	public void OnEnable()
	{
		if (unifyManager == null)
		{
			unifyManager = UnifyManager.Get();
			if (unifyManager != null)
			{
				UnifyManager obj = unifyManager;
				obj.OnPlatformDetailsChanged = (UnifyManager.PlatformDetailsChanged)Delegate.Combine(obj.OnPlatformDetailsChanged, new UnifyManager.PlatformDetailsChanged(OnPlatformDetailsChanged));
			}
		}
		Process();
	}

	public void OnDestroy()
	{
		if (unifyManager != null)
		{
			UnifyManager obj = unifyManager;
			obj.OnPlatformDetailsChanged = (UnifyManager.PlatformDetailsChanged)Delegate.Remove(obj.OnPlatformDetailsChanged, new UnifyManager.PlatformDetailsChanged(OnPlatformDetailsChanged));
		}
	}

	public void OnPlatformDetailsChanged()
	{
		Process();
	}

	private string Replace(string source, string replacement, string token = null)
	{
		if (token == null || token.Length <= 0)
		{
			return replacement;
		}
		return source.Replace(token, replacement);
	}

	private void Process()
	{
		if (Platforms.Length != 0)
		{
			bool flag = false;
			UnifyManager.Platform[] platforms = Platforms;
			for (int i = 0; i < platforms.Length; i++)
			{
				if (platforms[i] == UnifyManager.platform)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				return;
			}
		}
		if (Parent != null)
		{
			Parent.Process();
		}
		string source;
		try
		{
			this.text = GetComponent<Text>();
			source = this.text.text;
		}
		catch
		{
			text_TMP = GetComponent<TextMeshProUGUI>();
			source = text_TMP.text;
		}
		string text = EnglishText;
		switch (LocalizationManager.CurrentLanguage)
		{
		case "English":
			text = EnglishText;
			break;
		case "French":
			text = FrenchText;
			break;
		case "Italian":
			text = ItalianText;
			break;
		case "German":
			text = GermanText;
			break;
		case "Spanish":
			text = SpanishText;
			break;
		case "Russian":
			text = RussianText;
			break;
		case "Turkish":
			text = TurkishText;
			break;
		case "Chinese (Traditional)":
			text = TChineseText;
			break;
		case "Chinese (Simplified)":
			text = SChineseText;
			break;
		case "Japanese":
			text = JapaneseText;
			break;
		case "Korean":
			text = KoreanText;
			break;
		case "Portuguese (Brazil)":
		case "Portuguese":
			text = BrazilianPortugueseText;
			break;
		}
		if (text == null || text.Length <= 0)
		{
			text = EnglishText;
		}
		if (this.text != null)
		{
			this.text.text = Replace(source, text, Token);
		}
		else
		{
			text_TMP.text = Replace(source, text, Token);
		}
	}
}
