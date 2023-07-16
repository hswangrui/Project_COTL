using Rewired.Dev;

namespace RewiredConsts {

public static class Player
{
	[PlayerIdFieldInfo(friendlyName = "System")]
	public const int System = 9999999;

	[PlayerIdFieldInfo(friendlyName = "Player0")]
	public const int Player0 = 0;

	[PlayerIdFieldInfo(friendlyName = "Player1")]
	public const int Player1 = 1;

	[PlayerIdFieldInfo(friendlyName = "Player2")]
	public const int Player2 = 2;

	[PlayerIdFieldInfo(friendlyName = "Player3")]
	public const int Player3 = 3;
}
}