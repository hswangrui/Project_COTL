namespace Lamb.UI.Alerts
{
	public class RelicAlert : AlertBadge<RelicType>
	{
		protected override AlertCategory<RelicType> _source
		{
			get
			{
				return DataManager.Instance.Alerts.RelicAlerts;
			}
		}
	}
}
