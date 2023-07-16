namespace src.Alerts
{
	public class TarotCardAlerts : AlertCategory<TarotCards.Card>
	{
		~TarotCardAlerts()
		{
		}

		private void OnTarotCardAdded(TarotCards.Card card)
		{
			AddOnce(card);
		}
	}
}
