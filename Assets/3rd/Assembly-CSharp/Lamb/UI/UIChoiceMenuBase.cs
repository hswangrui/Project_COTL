using System;
using System.Collections;
using DG.Tweening;
using MMTools;
using Spine.Unity;
using src.UINavigator;
using UnityEngine;

namespace Lamb.UI
{
	public abstract class UIChoiceMenuBase<T, U> : UIMenuBase where T : UIChoiceInfoCard<U>
	{
		[SerializeField]
		protected UIMenuControlPrompts _controlPrompts;

		[SerializeField]
		protected T _infoBox1;

		[SerializeField]
		protected T _infoBox2;

		[SerializeField]
		private SkeletonGraphic _crownSpine;

		[SerializeField]
		private RectTransform _crownEye;

		[SerializeField]
		private RectTransform _eyePosLeft;

		[SerializeField]
		private RectTransform _eyePosRight;

		[SerializeField]
		private RectTransform _eyePosDown;

		[SerializeField]
		private RectTransform _crownMovePos;

		[SerializeField]
		private CanvasGroup _bwBackground;

		public void Start()
		{
			_bwBackground.alpha = 0f;
			_crownSpine.rectTransform.position -= new Vector3(0f, -800f);
			_controlPrompts.HideCancelButton();
			Configure();
			MMButton button = _infoBox1.Button;
			button.OnSelected = (Action)Delegate.Combine(button.OnSelected, new Action(OnInfoBoxLeftSelected));
			_infoBox1.Button.onClick.AddListener(OnLeftChoice);
			MMButton button2 = _infoBox2.Button;
			button2.OnSelected = (Action)Delegate.Combine(button2.OnSelected, new Action(OnInfoBoxRightSelected));
			_infoBox2.Button.onClick.AddListener(OnRightChoice);
		}

		protected abstract void Configure();

		protected override void OnShowStarted()
		{
			OverrideDefault(_infoBox1.Button);
		}

		protected override IEnumerator DoShowAnimation()
		{
			_bwBackground.DOFade(1f, 1f).SetUpdate(true);
			if (MMConversation.CURRENT_CONVERSATION.DoctrineResponses.Count == 1)
			{
				_infoBox1.ShowSelection(false);
				_infoBox1.RectTransform.DOLocalMove(new Vector2(0f, 0f), 0.6f).SetEase(Ease.OutBack).SetUpdate(true);
				_crownSpine.gameObject.SetActive(false);
			}
			else
			{
				_controlPrompts.ShowAcceptButton();
				_infoBox1.BringOnscreen();
				_infoBox2.BringOnscreen();
				_crownSpine.rectTransform.DOLocalMoveY(0f, 2f).SetEase(Ease.InOutCirc).SetUpdate(true);
			}
			yield return new WaitForSecondsRealtime(0.8f);
			UIManager.PlayAudio("event:/sermon/sermon_menu_appear");
			_infoBox1.Button.enabled = true;
			_infoBox1.Button.interactable = true;
			_infoBox2.Button.enabled = true;
			_infoBox2.Button.interactable = true;
			if (MMConversation.CURRENT_CONVERSATION.DoctrineResponses.Count == 1)
			{
				OnInfoBoxLeftSelected();
				OnLeftChoice();
			}
			else
			{
				ActivateNavigation();
			}
			yield return new WaitForSecondsRealtime(1f);
		}

		protected virtual void OnInfoBoxLeftSelected()
		{
			UIManager.PlayAudio("event:/sermon/scroll_sermon_menu");
			_crownEye.DOKill();
			_crownEye.DOLocalMove(_eyePosLeft.localPosition, 0.6f).SetEase(Ease.OutQuad).SetUpdate(true);
		}

		protected virtual void OnInfoBoxRightSelected()
		{
			UIManager.PlayAudio("event:/sermon/scroll_sermon_menu");
			_crownEye.DOKill();
			_crownEye.DOLocalMove(_eyePosRight.localPosition, 0.6f).SetEase(Ease.OutQuad).SetUpdate(true);
		}

		protected virtual void OnLeftChoice()
		{
			StartCoroutine(DoHoldToUnlock(_infoBox1));
		}

		protected virtual void OnRightChoice()
		{
			StartCoroutine(DoHoldToUnlock(_infoBox2));
		}

		private T OppositeChoice(T choice)
		{
			if (!((UnityEngine.Object)choice == (UnityEngine.Object)_infoBox1))
			{
				return _infoBox1;
			}
			return _infoBox2;
		}

		private IEnumerator DoHoldToUnlock(T choice)
		{
			_controlPrompts.HideAcceptButton();
			T val = OppositeChoice(choice);
			_crownSpine.rectTransform.anchoredPosition = new Vector2(0f, 0f);
			_crownSpine.rectTransform.DOLocalMoveY(_crownMovePos.localPosition.y, 1f).SetEase(Ease.InOutCirc).SetUpdate(true);
			UIManager.PlayAudio("event:/sermon/sermon_menu_appear");
			UIManager.PlayAudio("event:/sermon/select_upgrade");
			choice.Button.enabled = false;
			choice.Button.interactable = false;
			val.Button.enabled = false;
			val.Button.interactable = false;
			choice.ResetTweens();
			val.ResetTweens();
			choice.RectTransform.DOLocalMove(Vector3.zero, 1f).SetEase(Ease.InOutBack).SetUpdate(true);
			val.SendOffscreen();
			choice.ShowSelection(false);
			_crownEye.DOKill();
			_crownEye.DOLocalMove(_eyePosDown.localPosition, 1f).SetEase(Ease.OutQuad).SetUpdate(true);
			if (MMConversation.mmConversation != null)
			{
				MMConversation.mmConversation.SpeechBubble.gameObject.SetActive(false);
			}
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			if (MMConversation.CURRENT_CONVERSATION.DoctrineResponses.Count > 1)
			{
				yield return new WaitForSecondsRealtime(1f);
			}
			choice.RectTransform.DOScale(Vector3.one * 1.1f, 0.5f).SetEase(Ease.InOutBack).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.5f);
			if (MMConversation.CURRENT_CONVERSATION.DoctrineResponses.Count > 1)
			{
				_controlPrompts.ShowCancelButton();
			}
			bool confirmed = true;
			yield return choice.PerformHoldAction(delegate
			{
				confirmed = false;
				StartCoroutine(DoCancelChoice(choice));
			});
			if (confirmed)
			{
				UIManager.PlayAudio("event:/upgrade_statue/upgrade_unlock");
				UIManager.PlayAudio("event:/sermon/select_upgrade");
				_crownEye.DOLocalMove(Vector3.zero, 0.4f).SetEase(Ease.OutQuad).SetUpdate(true);
				choice.CardContainer.localPosition = Vector3.zero;
				choice.RectTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutBack).SetUpdate(true);
				choice.WhiteFlash.color = Color.white;
				choice.WhiteFlash.DOColor(new Color(1f, 1f, 1f, 0f), 0.3f).SetUpdate(true);
				if (CameraManager.instance != null)
				{
					CameraManager.instance.ShakeCameraForDuration(1.2f, 1.5f, 0.3f);
				}
				if (GameManager.GetInstance() != null)
				{
					GameManager.GetInstance().HitStop();
				}
				yield return new WaitForSecondsRealtime(0.4f);
				_crownSpine.transform.DOMove(_crownSpine.rectTransform.position - new Vector3(0f, -800f), 1.25f).SetUpdate(true);
				_bwBackground.DOFade(0f, 1f).SetUpdate(true);
				choice.WhiteFlash.color = Color.white;
				yield return new WaitForSecondsRealtime(0.25f);
				choice.WhiteFlash.DOColor(new Color(1f, 1f, 1f, 0f), 1f).SetUpdate(true);
				choice.RectTransform.DOShakePosition(0.5f, new Vector3(10f, 10f, 0f)).SetUpdate(true);
				yield return new WaitForSecondsRealtime(0.5f);
				choice.RectTransform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
				yield return new WaitForSecondsRealtime(0.5f);
				MadeChoice(choice);
				yield return null;
				Hide(true);
			}
		}

		protected abstract void MadeChoice(UIChoiceInfoCard<U> infoCard);

		private IEnumerator DoCancelChoice(T choice)
		{
			_controlPrompts.HideCancelButton();
			T otherChoice = OppositeChoice(choice);
			OverrideDefault(choice.Button);
			UIManager.PlayAudio("event:/ui/go_back");
			choice.CardContainer.localPosition = Vector3.zero;
			choice.RectTransform.DOKill();
			choice.RectTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutBack).SetUpdate(true);
			_crownEye.DOKill();
			_crownEye.DOLocalMove(Vector3.zero, 0.4f).SetEase(Ease.OutQuad).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.6f);
			choice.ShowSelection(true);
			choice.BringOnscreen();
			yield return new WaitForSecondsRealtime(0.2f);
			otherChoice.BringOnscreen();
			StartCoroutine(DoShowAnimation());
			yield return new WaitForSecondsRealtime(0.8f);
			if (MMConversation.mmConversation != null && MMConversation.CURRENT_CONVERSATION.DoctrineResponses.Count > 1 && MMConversation.CURRENT_CONVERSATION.DoctrineResponses[0].RewardLevel < 5)
			{
				MMConversation.mmConversation.SpeechBubble.gameObject.SetActive(true);
			}
		}
	}
}
