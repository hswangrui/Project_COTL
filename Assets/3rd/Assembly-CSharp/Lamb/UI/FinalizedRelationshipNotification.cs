using System;

namespace Lamb.UI
{
	[Serializable]
	public class FinalizedRelationshipNotification : FinalizedNotification
	{
		public FollowerInfoSnapshot followerInfoSnapshotA;

		public FollowerInfoSnapshot followerInfoSnapshotB;

		public NotificationFollower.Animation FollowerAnimationA;

		public NotificationFollower.Animation FollowerAnimationB;
	}
}
