using System.Collections.Generic;

public static class IntExtensions
{
	private static readonly Dictionary<string, int> _numeralDict = new Dictionary<string, int>
	{
		{ "M", 1000 },
		{ "CM", 900 },
		{ "D", 500 },
		{ "CD", 400 },
		{ "C", 100 },
		{ "XC", 90 },
		{ "L", 50 },
		{ "XL", 40 },
		{ "X", 10 },
		{ "IX", 9 },
		{ "V", 5 },
		{ "IV", 4 },
		{ "I", 1 }
	};

	public static string ToRefreshRateString(this int refreshRate)
	{
		return string.Format("{0} hz", refreshRate);
	}

	public static bool ToBool(this int i)
	{
		return i > 0;
	}

	public static string ToNumeral(this int i)
	{
		if (!SettingsManager.Settings.Accessibility.RomanNumerals)
		{
			return i.ToString();
		}
		string text = string.Empty;
		if (i <= 0)
		{
			return text;
		}
		foreach (KeyValuePair<string, int> item in _numeralDict)
		{
			while (i >= item.Value)
			{
				text += item.Key;
				i -= item.Value;
			}
		}
		return text;
	}

	public static int GetStableHashCode(this string str)
	{
		int num = 352654597;
		int num2 = num;
		for (int i = 0; i < str.Length; i += 2)
		{
			num = ((num << 5) + num) ^ str[i];
			if (i == str.Length - 1)
			{
				break;
			}
			num2 = ((num2 << 5) + num2) ^ str[i + 1];
		}
		return num + num2 * 1566083941;
	}
}
