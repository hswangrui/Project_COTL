public class NotificationData
{
	public string Notification = "";

	public float DeltaDisplay;

	public int FollowerID = -1;

	public NotificationBase.Flair Flair;

	public string[] ExtraText;

	public NotificationData(string Notification, float DeltaDisplay, int FollowerID, NotificationBase.Flair Flair, params string[] ExtraText)
	{
		this.Notification = Notification;
		this.DeltaDisplay = DeltaDisplay;
		this.FollowerID = FollowerID;
		this.Flair = Flair;
		this.ExtraText = ExtraText;
	}
}
