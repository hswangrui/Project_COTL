namespace Lamb.UI.Alerts
{
	public class DoctrineAlert : AlertBadge<DoctrineUpgradeSystem.DoctrineType>
	{
		protected override AlertCategory<DoctrineUpgradeSystem.DoctrineType> _source
		{
			get
			{
				return DataManager.Instance.Alerts.Doctrine;
			}
		}
	}
}
