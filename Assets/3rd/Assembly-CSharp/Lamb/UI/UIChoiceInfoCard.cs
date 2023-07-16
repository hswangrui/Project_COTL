using System;
using System.Collections;
using DG.Tweening;
using FMOD.Studio;
using MMTools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lamb.UI
{
	public abstract class UIChoiceInfoCard<T> : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
	{
		private const float kHoldDuration = 3f;

		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private RectTransform _cardContainer;

		[SerializeField]
		private GameObject _holdActionContainer;

		[SerializeField]
		private RadialProgress _radialProgress;

		[SerializeField]
		private MMButton _button;

		[SerializeField]
		private Image _selectedSymbol;

		[SerializeField]
		private Sprite _selectedSymbolSprite;

		[SerializeField]
		private Sprite _unSelectedSymbolSprite;

		[SerializeField]
		private Image _unselectedOverlay;

		[SerializeField]
		private Image _redOutline;

		[SerializeField]
		private Image _whiteFlash;

		private Vector2 _defaultAnchoredPosition;

		private Vector2 _offscreenPosition;

		protected T _info;

		public MMButton Button
		{
			get
			{
				return _button;
			}
		}

		public RectTransform CardContainer
		{
			get
			{
				return _cardContainer;
			}
		}

		public RectTransform RectTransform
		{
			get
			{
				return _rectTransform;
			}
		}

		public Image WhiteFlash
		{
			get
			{
				return _whiteFlash;
			}
		}

		public T Info
		{
			get
			{
				return _info;
			}
		}

		public void Configure(T info, Vector2 defaultAnchoredPosition, Vector2 offscreenPosition)
		{
			_info = info;
			_redOutline.rectTransform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
			_defaultAnchoredPosition = defaultAnchoredPosition;
			_offscreenPosition = offscreenPosition;
			_rectTransform.anchoredPosition = offscreenPosition;
			_rectTransform.localScale = Vector3.one;
			DoDeselect();
			_button.enabled = false;
			_button.interactable = false;
			ConfigureImpl(info);
		}

		protected abstract void ConfigureImpl(T info);

		private void OnEnable()
		{
			_holdActionContainer.SetActive(false);
			_radialProgress.Progress = 0f;
		}

		public void OnSelect(BaseEventData eventData)
		{
			_rectTransform.DOKill();
			_rectTransform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.1f).SetEase(Ease.InOutSine).SetUpdate(true);
			_redOutline.rectTransform.DOKill();
			_redOutline.rectTransform.DOScale(1f, 0.5f).SetEase(Ease.OutQuad).SetUpdate(true);
			_unselectedOverlay.DOKill();
			_unselectedOverlay.color = new Vector4(1f, 1f, 1f, 1f);
			_unselectedOverlay.DOFade(0f, 0.5f).SetEase(Ease.OutCirc).SetUpdate(true);
			_selectedSymbol.sprite = _selectedSymbolSprite;
			_selectedSymbol.color = StaticColors.RedColor;
		}

		public void OnDeselect(BaseEventData eventData)
		{
			DoDeselect();
		}

		private void DoDeselect()
		{
			ResetTweens();
			_unselectedOverlay.DOKill();
			_unselectedOverlay.color = new Vector4(1f, 1f, 1f, 0f);
			_unselectedOverlay.DOFade(1f, 0.5f).SetEase(Ease.OutCirc).SetUpdate(true);
			_selectedSymbol.sprite = _unSelectedSymbolSprite;
			_selectedSymbol.color = StaticColors.OffWhiteColor;
		}

		public void ResetTweens()
		{
			_redOutline.rectTransform.DOKill();
			_redOutline.rectTransform.DOScale(0.8f, 0.5f).SetEase(Ease.OutQuad).SetUpdate(true);
			_rectTransform.DOKill();
			_rectTransform.DOScale(new Vector3(1f, 1f, 1f), 0.3f).SetEase(Ease.InOutSine).SetUpdate(true);
		}

		public void SendOffscreen()
		{
			_rectTransform.DOAnchorPos(_offscreenPosition, 0.5f).SetEase(Ease.InOutBack).SetUpdate(true);
		}

		public void BringOnscreen()
		{
			_rectTransform.DOAnchorPos(_defaultAnchoredPosition, 0.6f).SetEase(Ease.OutBack).SetUpdate(true);
		}

		public IEnumerator PerformHoldAction(Action onCancel)
		{
			_holdActionContainer.SetActive(true);
			_holdActionContainer.transform.localScale = Vector3.one * 1.2f;
			_holdActionContainer.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
			if (SettingsManager.Settings.Accessibility.HoldActions)
			{
				EventInstance? loopingSound = null;
				float progress = 0f;
				while (progress < 3f)
				{
					float num = progress / 3f;
					if (InputManager.UI.GetAcceptButtonHeld() || InputManager.Gameplay.GetInteractButtonHeld())
					{
						if (!loopingSound.HasValue)
						{
							loopingSound = AudioManager.Instance.CreateLoop("event:/hearts_of_the_faithful/draw_power_loop", true);
						}
						progress = Mathf.Clamp(progress + Time.unscaledDeltaTime, 0f, 3f);
					}
					else
					{
						progress = Mathf.Clamp(progress - Time.unscaledDeltaTime * 5f, 0f, 3f);
					}
					if (loopingSound.HasValue)
					{
						loopingSound.GetValueOrDefault().setParameterByName("power", num);
					}
					_redOutline.rectTransform.localScale = Vector3.Lerp(new Vector3(0.97f, 0.97f, 0.97f), new Vector3(1.2f, 1.2f, 1.2f), progress / 6f);
					_rectTransform.localScale = Vector3.one * 1.1f * (1f + progress / 40f);
					_cardContainer.localPosition = UnityEngine.Random.insideUnitCircle * progress * 2f;
					_radialProgress.Progress = num;
					if (MMConversation.CURRENT_CONVERSATION.DoctrineResponses.Count > 1 && InputManager.UI.GetCancelButtonDown())
					{
						onCancel();
						_holdActionContainer.SetActive(false);
						_radialProgress.Progress = 0f;
						UIManager.PlayAudio("event:/sermon/scroll_sermon_menu");
						if (loopingSound.HasValue)
						{
							AudioManager.Instance.StopLoop(loopingSound.Value);
						}
						yield break;
					}
					yield return null;
				}
				if (loopingSound.HasValue)
				{
					AudioManager.Instance.StopLoop(loopingSound.Value);
				}
			}
			else
			{
				while (!InputManager.UI.GetAcceptButtonHeld() && !InputManager.Gameplay.GetInteractButtonHeld())
				{
					if (MMConversation.CURRENT_CONVERSATION.DoctrineResponses.Count > 1 && InputManager.UI.GetCancelButtonDown())
					{
						onCancel();
						_holdActionContainer.SetActive(false);
						UIManager.PlayAudio("event:/sermon/scroll_sermon_menu");
						yield break;
					}
					yield return null;
				}
			}
			UIManager.PlayAudio("event:/hearts_of_the_faithful/draw_power_end");
			_holdActionContainer.SetActive(false);
		}

		public void ShowSelection(bool state)
		{
			_selectedSymbol.enabled = state;
			_unselectedOverlay.enabled = state;
		}
	}
}
