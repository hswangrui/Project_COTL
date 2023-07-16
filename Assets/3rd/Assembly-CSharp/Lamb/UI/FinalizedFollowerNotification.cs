using System;
using UnityEngine.Serialization;

namespace Lamb.UI
{
	[Serializable]
	public class FinalizedFollowerNotification : FinalizedNotification
	{
		[FormerlySerializedAs("FollowerInfo")]
		public FollowerInfoSnapshot followerInfoSnapshot;

		public NotificationFollower.Animation Animation;
	}
}
