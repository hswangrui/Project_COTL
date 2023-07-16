using System;

[Serializable]
public class CommandPosition
{
	public FollowerCommands FollowerCommand;

	public WheelPosition WheelPosition;

	public bool ShowNotification;

	public CommandPosition(FollowerCommands f, WheelPosition w, bool showNotification = false)
	{
		FollowerCommand = f;
		WheelPosition = w;
		ShowNotification = showNotification;
	}
}
