namespace Lamb.UI.Alerts
{
	public class CurseAlert : AlertBadge<TarotCards.Card>
	{
		protected override AlertCategory<TarotCards.Card> _source
		{
			get
			{
				return DataManager.Instance.Alerts.Curses;
			}
		}
	}
}
