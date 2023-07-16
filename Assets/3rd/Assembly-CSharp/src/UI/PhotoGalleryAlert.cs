using Lamb.UI.Alerts;

namespace src.UI
{
	public class PhotoGalleryAlert : AlertBadge<string>
	{
		protected override AlertCategory<string> _source
		{
			get
			{
				return DataManager.Instance.Alerts.GalleryAlerts;
			}
		}
	}
}
