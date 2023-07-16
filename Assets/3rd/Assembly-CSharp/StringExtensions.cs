using System.Text.RegularExpressions;
using UnityEngine;

public static class StringExtensions
{
	public static string Colour(this string str, Color colour)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(colour) + ">" + str + "</color>";
	}

	public static string Colour(this string str, string hexColour)
	{
		return str.Colour(hexColour.ColourFromHex());
	}

	public static string Bold(this string str)
	{
		return "<b>" + str + "</b>";
	}

	public static string Size(this string str, int size)
	{
		return string.Format("<size={0}>{1}</size>", size, str);
	}

	public static string Italic(this string str)
	{
		return "<i>" + str + "</i>";
	}

	public static string NewLine(this string str)
	{
		return str + "\n";
	}

	public static string Wave(this string str)
	{
		return "<wave>" + str + "</wave>";
	}

	public static string StripHtml(this string str)
	{
		return Regex.Replace(str, "<.*?>", string.Empty);
	}

	public static string StripColourHtml(this string str)
	{
		return Regex.Replace(str, "<color.*?>", string.Empty).Replace("</color>", string.Empty);
	}

	public static string StripNumbers(this string str)
	{
		return Regex.Replace(str, "[\\d-]", string.Empty);
	}

	public static string StripWhitespace(this string str)
	{
		return Regex.Replace(str, "\\s+", "");
	}
}
