using I2.Loc;
using Spine.Unity;
using UnityEngine;

namespace Lamb.UI
{
	public class HistoricalNotificationRelationship : HistoricalNotificationBase<FinalizedRelationshipNotification>
	{
		[SerializeField]
		private SkeletonGraphic _followerSpineA;

		[SerializeField]
		private SkeletonGraphic _followerSpineB;

		protected override void ConfigureImpl(FinalizedRelationshipNotification finalizedNotification)
		{
			_followerSpineA.ConfigureFollowerSkin(finalizedNotification.followerInfoSnapshotA);
			_followerSpineA.AnimationState.SetAnimation(0, NotificationFollower.GetAnimation(finalizedNotification.FollowerAnimationA), true);
			_followerSpineB.ConfigureFollowerSkin(finalizedNotification.followerInfoSnapshotB);
			_followerSpineB.AnimationState.SetAnimation(0, NotificationFollower.GetAnimation(finalizedNotification.FollowerAnimationB), true);
		}

		protected override string GetLocalizedDescription(FinalizedRelationshipNotification finalizedNotification)
		{
			return string.Format(LocalizationManager.GetTranslation(finalizedNotification.LocKey), finalizedNotification.followerInfoSnapshotA.Name, finalizedNotification.followerInfoSnapshotB.Name);
		}
	}
}
