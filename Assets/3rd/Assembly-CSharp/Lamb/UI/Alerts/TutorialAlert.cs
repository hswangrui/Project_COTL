namespace Lamb.UI.Alerts
{
	public class TutorialAlert : AlertBadge<TutorialTopic>
	{
		protected override AlertCategory<TutorialTopic> _source
		{
			get
			{
				return DataManager.Instance.Alerts.Tutorial;
			}
		}
	}
}
