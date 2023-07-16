namespace Lamb.UI.Alerts
{
	public class LocationAlert : AlertBadge<FollowerLocation>
	{
		protected override AlertCategory<FollowerLocation> _source
		{
			get
			{
				return DataManager.Instance.Alerts.Locations;
			}
		}

		protected override bool HasAlertSingle()
		{
			if (DataManager.Instance.DiscoveredLocations.Contains(_alert) && !DataManager.Instance.VisitedLocations.Contains(_alert))
			{
				return true;
			}
			return _source.HasAlert(_alert);
		}
	}
}
