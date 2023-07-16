using System;
using System.Collections;
using DG.Tweening;
using src.UI.InfoCards;
using src.UI.Items;
using src.UINavigator;
using UnityEngine;

namespace Lamb.UI
{
	public class UITutorialMenuController : UIMenuBase
	{
		private string kSelectedTutorialAnimationState = "Selected";

		private string kCancelSelectionAnimationState = "Cancelled";

		[SerializeField]
		private TutorialMenuItem[] _menuItems;

		[SerializeField]
		private TutorialInfoCardController _infoCardController;

		[SerializeField]
		private UIMenuControlPrompts _controlPrompts;

		[SerializeField]
		private RectTransform _cardContainer;

		[SerializeField]
		private RectTransform _rootContainer;

		[SerializeField]
		private GameObject _previousPagePrompt;

		[SerializeField]
		private GameObject _nextPagePrompt;

		[SerializeField]
		private UINavigatorFollowElement _followElement;

		[SerializeField]
		private GameObject _contentContainer;

		public override void Awake()
		{
			base.Awake();
			TutorialMenuItem[] menuItems = _menuItems;
			foreach (TutorialMenuItem tutorialMenuItem in menuItems)
			{
				tutorialMenuItem.gameObject.SetActive(DataManager.Instance.RevealedTutorialTopics.Contains(tutorialMenuItem.Topic));
				tutorialMenuItem.OnTopicChosen = (Action<TutorialMenuItem>)Delegate.Combine(tutorialMenuItem.OnTopicChosen, new Action<TutorialMenuItem>(OnTutorialTopicChosen));
			}
		}

		protected override void OnShowStarted()
		{
			UIManager.PlayAudio("event:/ui/open_menu");
			_previousPagePrompt.SetActive(false);
			_nextPagePrompt.SetActive(false);
		}

		private void OnTutorialTopicChosen(TutorialMenuItem tutorialMenuItem)
		{
			_infoCardController.enabled = false;
			OverrideDefaultOnce(MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable.Selectable);
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			SetActiveStateForMenu(false);
			StartCoroutine(FocusTutorialCard(tutorialMenuItem));
		}

		private IEnumerator FocusTutorialCard(TutorialMenuItem item)
		{
			_controlPrompts.HideAcceptButton();
			TutorialInfoCard currentCard = _infoCardController.CurrentCard;
			currentCard.RectTransform.SetParent(_rootContainer, true);
			currentCard.RectTransform.DOLocalMove(Vector3.zero, 1f).SetEase(Ease.InOutBack).SetUpdate(true);
			yield return _animator.YieldForAnimation(kSelectedTutorialAnimationState);
			currentCard.Animator.enabled = false;
			bool inputBuffer = false;
			while (!InputManager.UI.GetCancelButtonDown())
			{
				if (InputManager.UI.GetHorizontalAxis() < -0.2f)
				{
					if (!inputBuffer)
					{
						inputBuffer = true;
						UIManager.PlayAudio("event:/ui/arrow_change_selection");
						yield return currentCard.PreviousPage();
					}
					yield return null;
				}
				else if (InputManager.UI.GetHorizontalAxis() > 0.2f)
				{
					if (!inputBuffer)
					{
						inputBuffer = true;
						UIManager.PlayAudio("event:/ui/arrow_change_selection");
						yield return currentCard.NextPage();
					}
					yield return null;
				}
				else
				{
					inputBuffer = false;
					yield return null;
				}
			}
			Vector2 vector = _rootContainer.InverseTransformPoint(_cardContainer.TransformPoint(Vector3.zero));
			currentCard.RectTransform.DOLocalMove(vector, 1f).SetEase(Ease.InOutBack).SetUpdate(true)
				.OnComplete(delegate
				{
					currentCard.RectTransform.SetParent(_cardContainer, true);
				});
			yield return _animator.YieldForAnimation(kCancelSelectionAnimationState);
			currentCard.Animator.enabled = true;
			_controlPrompts.ShowAcceptButton();
			SetActiveStateForMenu(true);
			_infoCardController.enabled = true;
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				Hide();
			}
		}

		protected override void OnHideStarted()
		{
			UIManager.PlayAudio("event:/ui/close_menu");
			_followElement.enabled = false;
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
