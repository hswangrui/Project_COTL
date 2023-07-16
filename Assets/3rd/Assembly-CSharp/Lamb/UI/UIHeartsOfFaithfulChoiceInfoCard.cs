using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class UIHeartsOfFaithfulChoiceInfoCard : UIChoiceInfoCard<UIHeartsOfTheFaithfulChoiceMenuController.Types>
	{
		[SerializeField]
		private TextMeshProUGUI _levelText;

		[SerializeField]
		private TextMeshProUGUI _currentStat;

		[SerializeField]
		private TextMeshProUGUI _nextStat;

		protected override void ConfigureImpl(UIHeartsOfTheFaithfulChoiceMenuController.Types info)
		{
			switch (info)
			{
			case UIHeartsOfTheFaithfulChoiceMenuController.Types.Hearts:
				_currentStat.text = (DataManager.Instance.PLAYER_TOTAL_HEALTH / 2f).ToString();
				_nextStat.text = (DataManager.Instance.PLAYER_TOTAL_HEALTH / 2f + 0.5f).ToString();
				_levelText.text = (DataManager.Instance.PLAYER_HEARTS_LEVEL + 1).ToNumeral();
				break;
			case UIHeartsOfTheFaithfulChoiceMenuController.Types.Strength:
				_currentStat.text = (1f + 0.25f * (float)DataManager.Instance.PLAYER_DAMAGE_LEVEL).ToString();
				_nextStat.text = (1f + 0.25f * (float)DataManager.Instance.PLAYER_DAMAGE_LEVEL + 0.25f).ToString();
				_levelText.text = (DataManager.Instance.PLAYER_DAMAGE_LEVEL + 1).ToNumeral();
				break;
			}
		}
	}
}
