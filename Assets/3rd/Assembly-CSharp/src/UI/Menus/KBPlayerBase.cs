using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Febucci.UI;
using KnuckleBones;
using Spine.Unity;
using TMPro;
using UnityEngine;

namespace src.UI.Menus
{
	public abstract class KBPlayerBase : MonoBehaviour
	{
		[SerializeField]
		protected Dice _dicePrefab;

		[SerializeField]
		protected Transform _position;

		[SerializeField]
		protected List<KBDiceTub> _diceTubs = new List<KBDiceTub>();

		[SerializeField]
		protected TextMeshProUGUI _nameText;

		[SerializeField]
		protected TextMeshProUGUI _scoreText;

		[SerializeField]
		protected TextAnimator _textAnimator;

		[SerializeField]
		protected CanvasGroup _turnOverlay;

		[SerializeField]
		protected RectTransform _content;

		[SerializeField]
		protected RectTransform _tubsRect;

		protected string _playerName;

		private Vector2 _contentOriginPosition;

		private Vector2 _tubOriginPosition;

		private Vector2 _contentOffscreenOffset;

		private Vector2 _tubOffscreenOffset;

		protected abstract string _playDiceAnimation { get; }

		protected abstract string _playerIdleAnimation { get; }

		protected abstract string _playerTakeDiceAnimation { get; }

		protected abstract string _playerLostDiceAnimation { get; }

		protected abstract string _playerWonAnimation { get; }

		protected abstract string _playerWonLoop { get; }

		protected abstract string _playerLostAnimation { get; }

		protected abstract string _playerLostLoop { get; }

		protected abstract SkeletonGraphic _spine { get; }

		public Dice DicePrefab
		{
			get
			{
				return _dicePrefab;
			}
		}

		public Transform Position
		{
			get
			{
				return _position;
			}
		}

		public int Score
		{
			get
			{
				int num = 0;
				foreach (KBDiceTub diceTub in _diceTubs)
				{
					num += diceTub.Score;
				}
				return num;
			}
		}

		public virtual void Configure(Vector2 contentOffscreenOffset, Vector2 tubOffscreenOffset)
		{
			_scoreText.text = "";
			_contentOriginPosition = _content.localPosition;
			_tubOriginPosition = _tubsRect.localPosition;
			_contentOffscreenOffset = contentOffscreenOffset;
			_tubOffscreenOffset = tubOffscreenOffset;
			foreach (KBDiceTub diceTub in _diceTubs)
			{
				diceTub.OnDiceMatched = (Action)Delegate.Combine(diceTub.OnDiceMatched, new Action(OnDiceMatched));
				diceTub.OnDiceLost = (Action)Delegate.Combine(diceTub.OnDiceLost, new Action(OnDiceLost));
				KBDiceTub opponentTub = diceTub.OpponentTub;
				opponentTub.OnDiceLost = (Action)Delegate.Combine(opponentTub.OnDiceLost, new Action(OnDiceMatched));
			}
			_nameText.text = _playerName.Wave();
			Hide(true);
		}

		private void OnDiceMatched()
		{
			_spine.AnimationState.SetAnimation(0, _playerTakeDiceAnimation, false);
			_spine.AnimationState.AddAnimation(0, _playerIdleAnimation, true, 0f);
		}

		private void OnDiceLost()
		{
			_scoreText.text = Score.ToString();
			_spine.AnimationState.SetAnimation(0, _playerLostDiceAnimation, false);
			_spine.AnimationState.AddAnimation(0, _playerIdleAnimation, true, 0f);
		}

		public void Show(bool instant = false)
		{
			if (instant)
			{
				_content.localPosition = _contentOriginPosition;
				_tubsRect.localPosition = _tubOriginPosition;
			}
			else
			{
				_content.DOLocalMove(_contentOriginPosition, 1.5f).SetEase(Ease.OutQuart).SetUpdate(true);
				_tubsRect.DOLocalMove(_tubOriginPosition, 1.5f).SetEase(Ease.OutQuart).SetUpdate(true);
			}
		}

		public void Hide(bool instant = false)
		{
			if (instant)
			{
				_content.localPosition = _contentOriginPosition + _contentOffscreenOffset;
				_tubsRect.localPosition = _tubOriginPosition + _tubOffscreenOffset;
			}
			_content.DOLocalMove(_contentOriginPosition + _contentOffscreenOffset, 1f).SetEase(Ease.OutQuart).SetUpdate(true);
			_tubsRect.DOLocalMove(_tubOriginPosition + _tubOffscreenOffset, 1f).SetEase(Ease.OutQuart).SetUpdate(true);
		}

		public void HighlightMe()
		{
			_turnOverlay.DOFade(0f, 1f).SetUpdate(true);
			_textAnimator.enabled = true;
		}

		public void UnHighlightMe()
		{
			_turnOverlay.DOFade(1f, 1f).SetUpdate(true);
			_textAnimator.enabled = false;
		}

		public abstract IEnumerator SelectTub(Dice dice);

		protected IEnumerator FinishTubSelection(Dice dice, KBDiceTub diceTub)
		{
			yield return diceTub.AddDice(dice);
			_spine.AnimationState.SetAnimation(0, _playDiceAnimation, false);
			_spine.AnimationState.AddAnimation(0, _playerIdleAnimation, true, 0f);
			yield return new WaitForSecondsRealtime(0.5f);
			_scoreText.text = Score.ToString();
		}

		public bool CheckGameCompleted()
		{
			int num = 0;
			foreach (KBDiceTub diceTub in _diceTubs)
			{
				if (diceTub.Dice.Count >= 3)
				{
					num++;
				}
			}
			return num >= _diceTubs.Count;
		}

		public void FinalizeScores()
		{
			_scoreText.text = Score.ToString();
			foreach (KBDiceTub diceTub in _diceTubs)
			{
				diceTub.FinalizeScore();
			}
		}

		public void SetWonLoop()
		{
			_spine.AnimationState.SetAnimation(0, _playerWonAnimation, false);
			_spine.AnimationState.AddAnimation(0, _playerWonLoop, true, 0f);
		}

		public void SetLostLoop()
		{
			_spine.AnimationState.SetAnimation(0, _playerLostAnimation, false);
			_spine.AnimationState.AddAnimation(0, _playerLostLoop, true, 0f);
		}

		public abstract string GetLocalizedName();
	}
}
