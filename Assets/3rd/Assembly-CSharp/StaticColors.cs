using UnityEngine;

public class StaticColors : BaseMonoBehaviour
{
	public static Color RedColor = new Color(0.9921569f, 0.1137255f, 0.01176471f, 1f);

	public static Color DarkRedColor = new Color(0.47f, 0.11f, 0.18f, 1f);

	public static Color GreenColor = new Color(0.003921569f, 71f / 85f, 0.6352941f, 1f);

	public static Color DarkGreenColor = new Color(0.019f, 0.313f, 0.294f, 1f);

	public static Color OrangeColor = new Color(1f, 0.6156863f, 0.003921569f, 1f);

	public static Color OffWhiteColor = new Color(49f / 51f, 0.9294118f, 71f / 85f, 1f);

	public static Color LightGreyColor = new Color(0.8f, 0.8f, 0.8f, 1f);

	public static Color GreyColor = new Color(0.43f, 0.427f, 0.411f);

	public static string GreyColorHex = "<color=#6E6666>";

	public static string OffWhiteHex = "<color=#F5EDD5>";

	public static string YellowColorHex = "#FFD201";

	public static string BlueColourHex = "#039ca1";

	public static string DarkBlueColourHex = "#2561af";

	public static Color TwitchPurple = new Color(0.4588235f, 0.294117f, 0.9058823f, 1f);

	public static Color BlueColor = BlueColourHex.ColourFromHex();

	public static Color DarkGreyColor = new Color(0.27f, 0.25f, 0.26f, 1f);

	public const float LOW_THRESHOLD = 0.25f;

	public static Color DarkBlueColor
	{
		get
		{
			return DarkBlueColourHex.ColourFromHex();
		}
	}

	public static Color ColorForThreshold(float value)
	{
		if (value >= 0f && value < 0.25f)
		{
			return RedColor;
		}
		if ((double)value >= 0.25 && (double)value < 0.45)
		{
			return OrangeColor;
		}
		return GreenColor;
	}
}
