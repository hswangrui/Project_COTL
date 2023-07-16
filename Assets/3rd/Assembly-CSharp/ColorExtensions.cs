using UnityEngine;

public static class ColorExtensions
{
	public static Color ColourFromHex(this string hex)
	{
		Color color;
		if (ColorUtility.TryParseHtmlString(hex, out color))
		{
			return color;
		}
		return Color.magenta;
	}
}
