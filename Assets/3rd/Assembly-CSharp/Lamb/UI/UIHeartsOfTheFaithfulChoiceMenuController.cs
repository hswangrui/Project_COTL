using System;
using UnityEngine;

namespace Lamb.UI
{
	public class UIHeartsOfTheFaithfulChoiceMenuController : UIChoiceMenuBase<UIHeartsOfFaithfulChoiceInfoCard, UIHeartsOfTheFaithfulChoiceMenuController.Types>
	{
		public enum Types
		{
			Hearts,
			Strength
		}

		public Action<Types> OnChoiceMade;

		protected override void Configure()
		{
			_infoBox1.Configure(Types.Hearts, new Vector2(300f, 0f), new Vector2(-200f, 0f));
			_infoBox2.Configure(Types.Strength, new Vector2(-300f, 0f), new Vector2(200f, 0f));
		}

		protected override void OnLeftChoice()
		{
			base.OnLeftChoice();
			UIManager.PlayAudio("event:/hearts_of_the_faithful/select_hearts");
		}

		protected override void OnRightChoice()
		{
			base.OnRightChoice();
			UIManager.PlayAudio("event:/hearts_of_the_faithful/select_swords");
		}

		protected override void MadeChoice(UIChoiceInfoCard<Types> infoCard)
		{
			Action<Types> onChoiceMade = OnChoiceMade;
			if (onChoiceMade != null)
			{
				onChoiceMade(infoCard.Info);
			}
		}

		protected override void OnHideCompleted()
		{
			UIManager.PlayAudio("event:/hearts_of_the_faithful/increase_stat_text_appear");
		}
	}
}
