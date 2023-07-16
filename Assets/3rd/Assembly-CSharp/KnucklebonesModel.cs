using I2.Loc;

public class KnucklebonesModel
{
	public const int NumDice = 3;

	public const int MaxOpponentDifficulty = 10;

	public static string GetLocalizedString(string str)
	{
		return LocalizationManager.GetTranslation("UI/Knucklebones/" + str);
	}
}
