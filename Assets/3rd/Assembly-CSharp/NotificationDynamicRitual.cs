using UnityEngine;
using UnityEngine.UI;

public class NotificationDynamicRitual : NotificationDynamicBase
{
	[SerializeField]
	protected Image _icon;

	[Header("Icons")]
	[SerializeField]
	private Sprite _holidayIcon;

	[SerializeField]
	private Sprite _workThroughNight;

	[SerializeField]
	private Sprite _fasterBuilding;

	[SerializeField]
	private Sprite _fasting;

	[SerializeField]
	private Sprite _fishing;

	[SerializeField]
	private Sprite _brainwashing;

	[SerializeField]
	private Sprite _enlightenment;

	[SerializeField]
	private Sprite _bloodMoon;

	public UpgradeSystem.Type Ritual { get; private set; }

	public override Color FullColour
	{
		get
		{
			return StaticColors.GreenColor;
		}
	}

	public override Color EmptyColour
	{
		get
		{
			return StaticColors.OrangeColor;
		}
	}

	public override void Configure(DynamicNotificationData data)
	{
		DynamicNotification_RitualActive dynamicNotification_RitualActive;
		if ((dynamicNotification_RitualActive = data as DynamicNotification_RitualActive) != null)
		{
			switch (dynamicNotification_RitualActive.Type)
			{
			case NotificationCentre.NotificationType.RitualHoliday:
				Ritual = UpgradeSystem.Type.Ritual_Holiday;
				break;
			case NotificationCentre.NotificationType.RitualWorkThroughNight:
				Ritual = UpgradeSystem.Type.Ritual_WorkThroughNight;
				break;
			case NotificationCentre.NotificationType.RitualFasterBuilding:
				Ritual = UpgradeSystem.Type.Ritual_FasterBuilding;
				break;
			case NotificationCentre.NotificationType.RitualFast:
				Ritual = UpgradeSystem.Type.Ritual_Fast;
				break;
			case NotificationCentre.NotificationType.RitualFishing:
				Ritual = UpgradeSystem.Type.Ritual_FishingRitual;
				break;
			case NotificationCentre.NotificationType.RitualBrainwashing:
				Ritual = UpgradeSystem.Type.Ritual_Brainwashing;
				break;
			case NotificationCentre.NotificationType.RitualEnlightenment:
				Ritual = UpgradeSystem.Type.Ritual_Enlightenment;
				break;
			case NotificationCentre.NotificationType.RitualHalloween:
				Ritual = UpgradeSystem.Type.Ritual_Halloween;
				break;
			}
		}
		base.Configure(data);
	}

	protected override void UpdateIcon()
	{
		DynamicNotification_RitualActive dynamicNotification_RitualActive;
		if ((dynamicNotification_RitualActive = base.Data as DynamicNotification_RitualActive) != null)
		{
			switch (dynamicNotification_RitualActive.Type)
			{
			case NotificationCentre.NotificationType.RitualHoliday:
				_icon.sprite = _holidayIcon;
				break;
			case NotificationCentre.NotificationType.RitualWorkThroughNight:
				_icon.sprite = _workThroughNight;
				break;
			case NotificationCentre.NotificationType.RitualFasterBuilding:
				_icon.sprite = _fasterBuilding;
				break;
			case NotificationCentre.NotificationType.RitualFast:
				_icon.sprite = _fasting;
				break;
			case NotificationCentre.NotificationType.RitualFishing:
				_icon.sprite = _fishing;
				break;
			case NotificationCentre.NotificationType.RitualBrainwashing:
				_icon.sprite = _brainwashing;
				break;
			case NotificationCentre.NotificationType.RitualEnlightenment:
				_icon.sprite = _enlightenment;
				break;
			case NotificationCentre.NotificationType.RitualHalloween:
				_icon.sprite = _bloodMoon;
				break;
			case NotificationCentre.NotificationType.EnemiesStronger:
			case NotificationCentre.NotificationType.Dynamic_Dissenter:
				break;
			}
		}
	}
}
