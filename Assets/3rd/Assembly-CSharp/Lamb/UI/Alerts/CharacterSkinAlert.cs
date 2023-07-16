namespace Lamb.UI.Alerts
{
	public class CharacterSkinAlert : AlertBadge<string>
	{
		protected override AlertCategory<string> _source
		{
			get
			{
				return DataManager.Instance.Alerts.CharacterSkinAlerts;
			}
		}
	}
}
