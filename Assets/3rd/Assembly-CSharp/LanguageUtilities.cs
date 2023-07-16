using Unify;
using UnityEngine;

public class LanguageUtilities
{
	public static readonly string[] AllLanguages = new string[10] { "English", "Japanese", "Russian", "French", "German", "Spanish", "Portuguese (Brazil)", "Chinese (Simplified)", "Chinese (Traditional)", "Korean" };

	public static readonly string[] AllLanguagesLocalizations = new string[10] { "UI/Settings/Game/Languages/English", "UI/Settings/Game/Languages/Japanese", "UI/Settings/Game/Languages/Russian", "UI/Settings/Game/Languages/French", "UI/Settings/Game/Languages/German", "UI/Settings/Game/Languages/Spanish", "UI/Settings/Game/Languages/Portuguese (Brazil)", "UI/Settings/Game/Languages/Chinese (Simplified)", "UI/Settings/Game/Languages/Chinese (Traditional)", "UI/Settings/Game/Languages/Korean" };

	internal static string SystemLanguageToLanguage(SystemLanguage language)
	{
		string text = "English";
		switch (language)
		{
		case SystemLanguage.Japanese:
			return "Japanese";
		case SystemLanguage.Russian:
			return "Russian";
		case SystemLanguage.French:
			return "French";
		case SystemLanguage.German:
			return "German";
		case SystemLanguage.Spanish:
			return "Spanish";
		case SystemLanguage.Portuguese:
			return "Portuguese (Brazil)";
		case SystemLanguage.ChineseTraditional:
			return "Chinese (Traditional)";
		case SystemLanguage.ChineseSimplified:
			return "Chinese (Simplified)";
		case SystemLanguage.Korean:
			return "Korean";
		default:
			return "English";
		}
	}

	public static string GetDefaultLanguage()
	{
		string text = "English";
		switch (UnifyManager.language)
		{
		case SystemLanguage.Japanese:
			text = "Japanese";
			break;
		case SystemLanguage.Russian:
			text = "Russian";
			break;
		case SystemLanguage.French:
			text = "French";
			break;
		case SystemLanguage.German:
			text = "German";
			break;
		case SystemLanguage.Spanish:
			text = "Spanish";
			break;
		case SystemLanguage.Portuguese:
			text = "Portuguese (Brazil)";
			break;
		case SystemLanguage.ChineseTraditional:
			text = "Chinese (Traditional)";
			break;
		case SystemLanguage.ChineseSimplified:
			text = "Chinese (Simplified)";
			break;
		case SystemLanguage.Korean:
			text = "Korean";
			break;
		case SystemLanguage.English:
			text = "English";
			break;
		}
		Debug.Log("Load system language:" + text);
		return text;
	}
}
