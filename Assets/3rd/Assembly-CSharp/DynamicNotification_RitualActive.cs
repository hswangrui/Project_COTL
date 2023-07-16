using System;

public class DynamicNotification_RitualActive : DynamicNotificationData
{
	private NotificationCentre.NotificationType _type;

	public override NotificationCentre.NotificationType Type
	{
		get
		{
			return _type;
		}
	}

	public override bool IsEmpty
	{
		get
		{
			if (_type == NotificationCentre.NotificationType.RitualHoliday)
			{
				return !FollowerBrainStats.IsHoliday;
			}
			if (_type == NotificationCentre.NotificationType.RitualWorkThroughNight)
			{
				return !FollowerBrainStats.IsWorkThroughTheNight;
			}
			if (_type == NotificationCentre.NotificationType.RitualFast)
			{
				return !FollowerBrainStats.Fasting;
			}
			if (_type == NotificationCentre.NotificationType.RitualFishing)
			{
				return !FollowerBrainStats.IsFishing;
			}
			if (_type == NotificationCentre.NotificationType.RitualBrainwashing)
			{
				return !FollowerBrainStats.BrainWashed;
			}
			if (_type == NotificationCentre.NotificationType.RitualEnlightenment)
			{
				return !FollowerBrainStats.IsEnlightened;
			}
			if (_type == NotificationCentre.NotificationType.RitualHalloween)
			{
				return !FollowerBrainStats.IsBloodMoon;
			}
			return true;
		}
	}

	public override bool HasProgress
	{
		get
		{
			return true;
		}
	}

	public override bool HasDynamicProgress
	{
		get
		{
			return true;
		}
	}

	public override float CurrentProgress
	{
		get
		{
			float num = 0f;
			if (_type == NotificationCentre.NotificationType.RitualHoliday)
			{
				num = (TimeManager.TotalElapsedGameTime - DataManager.Instance.LastHolidayDeclared) / 1200f;
			}
			else if (_type == NotificationCentre.NotificationType.RitualWorkThroughNight)
			{
				num = (TimeManager.TotalElapsedGameTime - DataManager.Instance.LastWorkThroughTheNight) / 3600f;
			}
			else if (_type == NotificationCentre.NotificationType.RitualFast)
			{
				num = (TimeManager.TotalElapsedGameTime - DataManager.Instance.LastFastDeclared) / 3600f;
			}
			else if (_type == NotificationCentre.NotificationType.RitualFishing)
			{
				num = (TimeManager.TotalElapsedGameTime - DataManager.Instance.LastFishingDeclared) / 3600f;
			}
			else if (_type == NotificationCentre.NotificationType.RitualBrainwashing)
			{
				num = (TimeManager.TotalElapsedGameTime - DataManager.Instance.LastBrainwashed) / 3600f;
			}
			else if (_type == NotificationCentre.NotificationType.RitualEnlightenment)
			{
				num = (TimeManager.TotalElapsedGameTime - DataManager.Instance.LastEnlightenment) / 3600f;
			}
			else if (_type == NotificationCentre.NotificationType.RitualHalloween)
			{
				num = (TimeManager.TotalElapsedGameTime - DataManager.Instance.LastHalloween) / 3600f;
			}
			num = 1f - num;
			if (num <= 0f)
			{
				Action dataChanged = DataChanged;
				if (dataChanged != null)
				{
					dataChanged();
				}
			}
			return num;
		}
	}

	public override float TotalCount
	{
		get
		{
			return 0f;
		}
	}

	public override string SkinName
	{
		get
		{
			return "";
		}
	}

	public override int SkinColor
	{
		get
		{
			return 0;
		}
	}

	public DynamicNotification_RitualActive(UpgradeSystem.Type type)
	{
		switch (type)
		{
		case UpgradeSystem.Type.Ritual_Holiday:
			_type = NotificationCentre.NotificationType.RitualHoliday;
			break;
		case UpgradeSystem.Type.Ritual_WorkThroughNight:
			_type = NotificationCentre.NotificationType.RitualWorkThroughNight;
			break;
		case UpgradeSystem.Type.Ritual_Fast:
			_type = NotificationCentre.NotificationType.RitualFast;
			break;
		case UpgradeSystem.Type.Ritual_FishingRitual:
			_type = NotificationCentre.NotificationType.RitualFishing;
			break;
		case UpgradeSystem.Type.Ritual_Brainwashing:
			_type = NotificationCentre.NotificationType.RitualBrainwashing;
			break;
		case UpgradeSystem.Type.Ritual_Enlightenment:
			_type = NotificationCentre.NotificationType.RitualEnlightenment;
			break;
		case UpgradeSystem.Type.Ritual_Halloween:
			_type = NotificationCentre.NotificationType.RitualHalloween;
			break;
		}
	}
}
