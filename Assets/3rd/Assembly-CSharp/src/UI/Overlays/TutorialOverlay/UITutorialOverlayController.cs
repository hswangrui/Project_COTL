using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Lamb.UI;
using src.UI.InfoCards;
using UnityEngine;

namespace src.UI.Overlays.TutorialOverlay
{
	public class UITutorialOverlayController : UIMenuBase
	{
		[SerializeField]
		private TutorialInfoCard _tutorialInfoCard;

		[SerializeField]
		private UIMenuControlPrompts _controlPrompts;

		[SerializeField]
		private GameObject _prevPrompt;

		[SerializeField]
		private GameObject _nextPrompt;

		private TutorialTopic _topic;

		private float _delay;

		private bool _leftDown;

		private bool _rightDown;

		public override void Awake()
		{
			base.Awake();
			_canvasGroup.alpha = 0f;
		}

		public void Show(TutorialTopic topic, float delay = 0f, bool instant = false)
		{
			_topic = topic;
			_delay = delay;
			Show(instant);
		}

		protected override void OnShowStarted()
		{
			if (!SettingsManager.Settings.Game.ShowTutorials)
			{
				Hide(true);
				return;
			}
			AudioManager.Instance.PauseActiveLoops();
			UIManager.PlayAudio("event:/ui/open_menu");
			UIManager.PlayAudio("event:/Stings/church_bell");
			_controlPrompts.HideAcceptButton();
			_controlPrompts.HideCancelButton();
			_tutorialInfoCard.Show(_topic);
			TutorialInfoCard tutorialInfoCard = _tutorialInfoCard;
			tutorialInfoCard.OnLeftArrowClicked = (Action)Delegate.Combine(tutorialInfoCard.OnLeftArrowClicked, (Action)delegate
			{
				_leftDown = true;
			});
			TutorialInfoCard tutorialInfoCard2 = _tutorialInfoCard;
			tutorialInfoCard2.OnRightArrowClicked = (Action)Delegate.Combine(tutorialInfoCard2.OnRightArrowClicked, (Action)delegate
			{
				_rightDown = true;
			});
			_prevPrompt.SetActive(false);
			_nextPrompt.SetActive(true);
		}

		protected override IEnumerator DoShowAnimation()
		{
			yield return new WaitForSecondsRealtime(_delay);
			yield return _003C_003En__0();
			yield return new WaitForSecondsRealtime(0.5f);
		}

		protected override void OnShowCompleted()
		{
			StartCoroutine(RunOverlay());
		}

		private IEnumerator RunOverlay()
		{
			bool seenAllPages = false;
			bool inputBuffer = false;
			_tutorialInfoCard.Animator.enabled = false;
			while (true)
			{
				if (!_leftDown)
				{
					_leftDown = InputManager.UI.GetHorizontalAxis() < -0.2f;
				}
				if (!_rightDown)
				{
					_rightDown = InputManager.UI.GetHorizontalAxis() > 0.2f;
				}
				if (_leftDown)
				{
					if (!inputBuffer)
					{
						inputBuffer = true;
						yield return _tutorialInfoCard.PreviousPage();
					}
				}
				else if (_rightDown)
				{
					if (!inputBuffer)
					{
						inputBuffer = true;
						yield return _tutorialInfoCard.NextPage();
						if (!seenAllPages && _tutorialInfoCard.Page == _tutorialInfoCard.NumPages)
						{
							seenAllPages = true;
							_controlPrompts.ShowAcceptButton();
						}
					}
				}
				else
				{
					inputBuffer = false;
				}
				_prevPrompt.SetActive(_tutorialInfoCard.Page > 0);
				_nextPrompt.SetActive(_tutorialInfoCard.Page < _tutorialInfoCard.NumPages);
				_leftDown = false;
				_rightDown = false;
				if (seenAllPages && (InputManager.UI.GetCancelButtonDown() || InputManager.UI.GetAcceptButtonDown()))
				{
					break;
				}
				yield return null;
			}
			Hide();
		}

		protected override void OnHideStarted()
		{
			base.OnHideStarted();
			AudioManager.Instance.ResumeActiveLoops();
			UIManager.PlayAudio("event:/ui/close_menu");
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		[CompilerGenerated]
		[DebuggerHidden]
		private IEnumerator _003C_003En__0()
		{
			return base.DoShowAnimation();
		}
	}
}
