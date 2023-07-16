using System;

public abstract class DynamicNotificationData
{
	public Action DataChanged;

	public abstract NotificationCentre.NotificationType Type { get; }

	public abstract bool IsEmpty { get; }

	public abstract bool HasProgress { get; }

	public abstract bool HasDynamicProgress { get; }

	public abstract float CurrentProgress { get; }

	public abstract float TotalCount { get; }

	public abstract string SkinName { get; }

	public abstract int SkinColor { get; }

	public DynamicNotificationData()
	{
	}
}
