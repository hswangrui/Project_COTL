using I2.Loc;
using Spine.Unity;
using UnityEngine;

namespace Lamb.UI
{
	public class HistoricalNotificationFollower : HistoricalNotificationBase<FinalizedFollowerNotification>
	{
		[SerializeField]
		private SkeletonGraphic _followerSpine;

		protected override void ConfigureImpl(FinalizedFollowerNotification finalizedNotification)
		{
			_followerSpine.ConfigureFollowerSkin(finalizedNotification.followerInfoSnapshot);
			_followerSpine.AnimationState.SetAnimation(0, NotificationFollower.GetAnimation(finalizedNotification.Animation), true);
		}

		protected override string GetLocalizedDescription(FinalizedFollowerNotification finalizedNotification)
		{
			if (finalizedNotification.followerInfoSnapshot != null)
			{
				return string.Format(LocalizationManager.GetTranslation(finalizedNotification.LocKey), finalizedNotification.followerInfoSnapshot.Name);
			}
			return "";
		}
	}
}
