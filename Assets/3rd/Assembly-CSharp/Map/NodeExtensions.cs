namespace Map
{
	public static class NodeExtensions
	{
		public static bool ShouldIncrementRandomRoomsEncountered(this NodeType nodeType)
		{
			switch (nodeType)
			{
			case NodeType.Sherpa:
			case NodeType.Knucklebones:
			case NodeType.Special_Teleporter:
			case NodeType.special_Challenge:
			case NodeType.Special_RewardChoice:
			case NodeType.Special_HappyFollower:
			case NodeType.Special_DissentingFollower:
			case NodeType.Rare_Gold:
			case NodeType.Lore_Haro:
			case NodeType.Special_HealthChoice:
				return true;
			default:
				return false;
			}
		}
	}
}
