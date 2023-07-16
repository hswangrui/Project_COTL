using System;
using MMTools;
using UnityEngine;

namespace Lamb.UI.Menus.DoctrineChoicesMenu
{
	public class UIDoctrineChoicesMenuController : UIChoiceMenuBase<UIDoctrineChoiceInfoBox, DoctrineResponse>
	{
		private int _cachedMMConversationSortingOrder;

		private Canvas _mmConversationCanvas;

		protected override void OnShowStarted()
		{
			base.OnShowStarted();
			if (MMConversation.mmConversation != null && MMConversation.mmConversation.TryGetComponent<Canvas>(out _mmConversationCanvas))
			{
				_cachedMMConversationSortingOrder = _mmConversationCanvas.sortingOrder;
				if (_canvas != null)
				{
					_mmConversationCanvas.sortingOrder = _canvas.sortingOrder + 1;
				}
			}
		}

		protected override void OnHideCompleted()
		{
			if (MMConversation.mmConversation != null)
			{
				MMConversation.mmConversation.SpeechBubble.gameObject.SetActive(true);
			}
			if (_mmConversationCanvas != null)
			{
				_mmConversationCanvas.sortingOrder = _cachedMMConversationSortingOrder;
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		protected override void Configure()
		{
			_infoBox1.Configure(MMConversation.CURRENT_CONVERSATION.DoctrineResponses[0], new Vector2(-660f, 0f), new Vector2(-1160f, 0f));
			if (MMConversation.CURRENT_CONVERSATION.DoctrineResponses.Count > 1)
			{
				if (MMConversation.CURRENT_CONVERSATION.DoctrineResponses[0].RewardLevel >= 5 && MMConversation.mmConversation != null)
				{
					MMConversation.mmConversation.SpeechBubble.gameObject.SetActive(false);
				}
				_infoBox2.Configure(MMConversation.CURRENT_CONVERSATION.DoctrineResponses[1], new Vector2(660f, 0f), new Vector2(1160f, 0f));
				return;
			}
			_controlPrompts.HideAcceptButton();
			_infoBox1.RectTransform.localPosition = new Vector2(0f, -540f);
			_infoBox2.gameObject.SetActive(false);
			if (MMConversation.mmConversation != null)
			{
				MMConversation.mmConversation.SpeechBubble.gameObject.SetActive(false);
			}
		}

		protected override void MadeChoice(UIChoiceInfoCard<DoctrineResponse> infoCard)
		{
			if (MMConversation.mmConversation != null)
			{
				if (MMConversation.CURRENT_CONVERSATION.DoctrineResponses.Count > 1 && infoCard.Info.RewardLevel < 5)
				{
					MMConversation.mmConversation.SpeechBubble.gameObject.SetActive(true);
				}
				Action action = null;
				if (infoCard == _infoBox1 && MMConversation.CURRENT_CONVERSATION.DoctrineResponses[0] != null)
				{
					action = MMConversation.CURRENT_CONVERSATION.DoctrineResponses[0].Callback;
				}
				if (infoCard == _infoBox2 && MMConversation.CURRENT_CONVERSATION.DoctrineResponses[1] != null)
				{
					action = MMConversation.CURRENT_CONVERSATION.DoctrineResponses[1].Callback;
				}
				CultFaithManager.AddThought(Thought.Cult_NewDoctrine, -1, 1f);
				MMConversation.mmConversation.Close();
				if (action != null)
				{
					action();
				}
			}
		}
	}
}
