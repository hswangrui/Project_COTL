using System;

public class GameplaySettingsUtilities
{
	public static Action<bool> OnShowFollowerNamesChanged;

	public static void UpdateShowFollowerNamesSetting(bool value)
	{
		SettingsManager.Settings.Game.ShowFollowerNames = value;
		Action<bool> onShowFollowerNamesChanged = OnShowFollowerNamesChanged;
		if (onShowFollowerNamesChanged != null)
		{
			onShowFollowerNamesChanged(value);
		}
	}
}
