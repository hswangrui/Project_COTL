using System;

namespace src.Alerts
{
	public class CurseAlerts : AlertCategory<TarotCards.Card>
	{
		public CurseAlerts()
		{
			DataManager.OnCurseUnlocked = (Action<TarotCards.Card>)Delegate.Combine(DataManager.OnCurseUnlocked, new Action<TarotCards.Card>(OnCurseUnlocked));
		}

		~CurseAlerts()
		{
			if (DataManager.Instance != null)
			{
				DataManager.OnCurseUnlocked = (Action<TarotCards.Card>)Delegate.Remove(DataManager.OnCurseUnlocked, new Action<TarotCards.Card>(OnCurseUnlocked));
			}
		}

		private void OnCurseUnlocked(TarotCards.Card curse)
		{
			AddOnce(curse);
		}
	}
}
