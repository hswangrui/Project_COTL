using Lamb.UI.PauseDetails;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class TarotInfoCardController : UIInfoCardController<TarotInfoCard, TarotCards.TarotCard>
	{
		protected override bool IsSelectionValid(Selectable selectable, out TarotCards.TarotCard showParam)
		{
			showParam = null;
			TarotCardItem_Unlocked component;
			if (selectable.TryGetComponent<TarotCardItem_Unlocked>(out component))
			{
				showParam = new TarotCards.TarotCard(component.Type, 0);
				return TarotCards.IsUnlocked(component.Type);
			}
			TarotCardItem_Run component2;
			if (selectable.TryGetComponent<TarotCardItem_Run>(out component2))
			{
				showParam = component2.Card;
				return true;
			}
			return false;
		}

		protected override TarotCards.TarotCard DefaultShowParam()
		{
			return null;
		}
	}
}
