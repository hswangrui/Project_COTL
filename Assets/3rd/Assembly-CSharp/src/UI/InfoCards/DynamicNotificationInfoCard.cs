using I2.Loc;
using Lamb.UI;
using TMPro;
using UnityEngine;

namespace src.UI.InfoCards
{
	public class DynamicNotificationInfoCard : UIInfoCardBase<DynamicNotificationData>
	{
		[SerializeField]
		private TextMeshProUGUI _notificationTitle;

		[SerializeField]
		private TextMeshProUGUI _notificationDescription;

		[SerializeField]
		private TextMeshProUGUI _icon;

		public override void Configure(DynamicNotificationData config)
		{
			switch (config.Type)
			{
			case NotificationCentre.NotificationType.Dynamic_Homeless:
				_icon.text = "\uf236";
				_notificationTitle.text = ScriptLocalization.UI_DynamicNotification_NotEnoughBeds.Title;
				_notificationDescription.text = string.Format(ScriptLocalization.UI_DynamicNotification_NotEnoughBeds.Description, config.TotalCount, DataManager.Instance.Followers.Count);
				break;
			case NotificationCentre.NotificationType.Dynamic_Starving:
				_icon.text = "\uf623";
				_notificationTitle.text = ScriptLocalization.UI_DynamicNotification_FollowersStarving.Title;
				_notificationDescription.text = string.Format(ScriptLocalization.UI_DynamicNotification_FollowersStarving.Description, config.TotalCount, DataManager.Instance.Followers.Count);
				break;
			case NotificationCentre.NotificationType.Dynamic_Sick:
				_icon.text = "<sprite name=\"icon_Sickness\">";
				_notificationTitle.text = ScriptLocalization.UI_DynamicNotification_Illness.Title;
				_notificationDescription.text = string.Format(ScriptLocalization.UI_DynamicNotification_Illness.Description, config.TotalCount, DataManager.Instance.Followers.Count);
				break;
			case NotificationCentre.NotificationType.Exhausted:
				_icon.text = "<sprite name=\"icon_Sleep\">";
				_notificationTitle.text = ScriptLocalization.UI_DynamicNotification_Exhausted.Title;
				_notificationDescription.text = string.Format(ScriptLocalization.UI_DynamicNotification_Exhausted.Description, config.TotalCount, DataManager.Instance.Followers.Count);
				break;
			case NotificationCentre.NotificationType.Dynamic_Dissenter:
				_icon.text = "<sprite name=\"icon_Faith\">";
				_notificationTitle.text = ScriptLocalization.Tutorial_UI.Dissenter;
				_notificationDescription.text = ScriptLocalization.Tutorial_UI_Dissenter.Info1;
				break;
			}
		}
	}
}
