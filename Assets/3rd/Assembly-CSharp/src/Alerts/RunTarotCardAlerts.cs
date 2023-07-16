namespace src.Alerts
{
	public class RunTarotCardAlerts : AlertCategory<TarotCards.Card>
	{
		public RunTarotCardAlerts()
		{
			TrinketManager.OnTrinketAdded += OnTarotCardAdded;
		}

		~RunTarotCardAlerts()
		{
			TrinketManager.OnTrinketAdded -= OnTarotCardAdded;
		}

		private void OnTarotCardAdded(TarotCards.Card card)
		{
			AddOnce(card);
		}
	}
}
