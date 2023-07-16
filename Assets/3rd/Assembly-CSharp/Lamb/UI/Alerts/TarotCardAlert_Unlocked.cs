namespace Lamb.UI.Alerts
{
	public class TarotCardAlert_Unlocked : TarotCardAlertBase
	{
		protected override AlertCategory<TarotCards.Card> _source
		{
			get
			{
				return DataManager.Instance.Alerts.TarotCardAlerts;
			}
		}
	}
}
