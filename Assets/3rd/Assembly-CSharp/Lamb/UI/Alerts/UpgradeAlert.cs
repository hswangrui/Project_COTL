namespace Lamb.UI.Alerts
{
	public class UpgradeAlert : AlertBadge<UpgradeSystem.Type>
	{
		protected override AlertCategory<UpgradeSystem.Type> _source
		{
			get
			{
				return DataManager.Instance.Alerts.Upgrades;
			}
		}
	}
}
