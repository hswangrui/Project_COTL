using System;
using UnityEngine.Serialization;

namespace Lamb.UI
{
	[Serializable]
	public class FinalizedFaithNotification : FinalizedNotification
	{
		public float FaithDelta;

		[FormerlySerializedAs("FollowerInfo")]
		public FollowerInfoSnapshot followerInfoSnapshot;
	}
}
