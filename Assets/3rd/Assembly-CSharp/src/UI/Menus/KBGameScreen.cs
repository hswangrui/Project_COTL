using System;
using System.Collections;
using System.Runtime.CompilerServices;
using DG.Tweening;
using KnuckleBones;
using Lamb.UI;
using src.UINavigator;
using src.Utilities;
using TMPro;
using UnityEngine;

namespace src.UI.Menus
{
	public class KBGameScreen : UISubmenuBase
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass21_0
		{
			public bool completed;

			internal void _003CDoGameLoop_003Eg__Complete_007C0()
			{
				completed = true;
			}
		}

		public Action<UIKnuckleBonesController.KnucklebonesResult> OnMatchFinished;

		[Header("Knuckle Bones")]
		[SerializeField]
		private UIMenuControlPrompts _controlPrompts;

		[SerializeField]
		private KBBackground _background;

		[SerializeField]
		private Transform _diceContainer;

		[SerializeField]
		private KBPlayer _player;

		[SerializeField]
		private KBOpponent _opponent;

		[SerializeField]
		private CanvasGroup _announcementCanvasGroup;

		[SerializeField]
		private RectTransform _announcementRectTransform;

		[SerializeField]
		private TextMeshProUGUI _announcementText;

		[SerializeField]
		private TextMeshProUGUI _winningsText;

		private KnucklebonesOpponent _opponentConfiguration;

		private Vector3 _announceTextStartingPosition;

		private UIMenuConfirmationWindow _exitConfirmationInstance;

		private bool _matchCompleted;

		private int _bet;

		public void Configure(KnucklebonesOpponent opponent, int bet)
		{
			_opponentConfiguration = opponent;
			_bet = bet;
		}

		public override void OnCancelButtonInput()
		{
			if (!_parent.CanvasGroup.interactable || !_canvasGroup.interactable || !(_exitConfirmationInstance == null) || _matchCompleted)
			{
				return;
			}
			_exitConfirmationInstance = Push(MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate);
			_exitConfirmationInstance.Configure(KnucklebonesModel.GetLocalizedString("Exit"), KnucklebonesModel.GetLocalizedString("AreYouSure"));
			UIMenuConfirmationWindow exitConfirmationInstance = _exitConfirmationInstance;
			exitConfirmationInstance.OnConfirm = (Action)Delegate.Combine(exitConfirmationInstance.OnConfirm, (Action)delegate
			{
				MonoSingleton<UINavigatorNew>.Instance.Clear();
				StopAllCoroutines();
				Action<UIKnuckleBonesController.KnucklebonesResult> onMatchFinished = OnMatchFinished;
				if (onMatchFinished != null)
				{
					onMatchFinished(UIKnuckleBonesController.KnucklebonesResult.Loss);
				}
			});
			UIMenuConfirmationWindow exitConfirmationInstance2 = _exitConfirmationInstance;
			exitConfirmationInstance2.OnCancel = (Action)Delegate.Combine(exitConfirmationInstance2.OnCancel, (Action)delegate
			{
				MonoSingleton<UINavigatorNew>.Instance.Clear();
			});
			UIMenuConfirmationWindow exitConfirmationInstance3 = _exitConfirmationInstance;
			exitConfirmationInstance3.OnHide = (Action)Delegate.Combine(exitConfirmationInstance3.OnHide, (Action)delegate
			{
				_exitConfirmationInstance = null;
			});
		}

		protected override void OnShowStarted()
		{
			_announceTextStartingPosition = _announcementRectTransform.position;
			_player.Configure(new Vector2(-800f, 0f), new Vector2(0f, -800f));
			_opponent.Configure(_opponentConfiguration, new Vector2(800f, 0f), new Vector2(0f, 800f));
			_controlPrompts.HideAcceptButton();
			_winningsText.gameObject.SetActive(false);
		}

		protected override IEnumerator DoShowAnimation()
		{
			AudioManager.Instance.PlayOneShot("event:/sermon/scroll_sermon_menu");
			_player.Show();
			_opponent.Show();
			_canvasGroup.DOFade(1f, 1.5f).SetEase(Ease.OutQuart).SetUpdate(true);
			yield return _background.TransitionToEndValues();
		}

		protected override void OnShowCompleted()
		{
			StartCoroutine(DoGameLoop());
		}

		protected override IEnumerator DoHideAnimation()
		{
			AudioManager.Instance.PlayOneShot("event:/sermon/scroll_sermon_menu");
			_player.Hide();
			_opponent.Hide();
			_canvasGroup.DOFade(0f, 1f).SetEase(Ease.OutQuart).SetUpdate(true);
			yield return _background.TransitionToStartValues();
			yield return new WaitForSecondsRealtime(0.5f);
		}

		private IEnumerator DoGameLoop()
		{
			_diceContainer.DestroyAllChildren();
			bool playerTurn = BoolUtilities.RandomBool();
			if (DataManager.Instance.KnucklebonesFirstGameRatauStart)
			{
				playerTurn = false;
			}
			DataManager.Instance.KnucklebonesFirstGameRatauStart = false;
			_announcementRectTransform.position = _announceTextStartingPosition + new Vector3(800f, 0f);
			_announcementRectTransform.DOMove(_announceTextStartingPosition, 1f).SetUpdate(true);
			_announcementCanvasGroup.DOFade(1f, 0.5f).SetUpdate(true);
			string arg = (playerTurn ? _player.GetLocalizedName() : _opponent.GetLocalizedName());
			string localizedString = KnucklebonesModel.GetLocalizedString("RollsFirst");
			_announcementText.text = string.Format(localizedString, arg);
			yield return new WaitForSecondsRealtime(3f);
			_announcementRectTransform.DOMove(_announceTextStartingPosition + new Vector3(-800f, 0f), 1f).SetUpdate(true);
			_announcementCanvasGroup.DOFade(0f, 0.5f).SetUpdate(true);
			yield return new WaitForSecondsRealtime(1f);
			_003C_003Ec__DisplayClass21_0 _003C_003Ec__DisplayClass21_ = new _003C_003Ec__DisplayClass21_0();
			_003C_003Ec__DisplayClass21_.completed = false;
			while (!_003C_003Ec__DisplayClass21_.completed)
			{
				for (int i = 0; i < 2; i++)
				{
					if (playerTurn)
					{
						_controlPrompts.ShowAcceptButton();
						yield return PerformTurn(_player, _003C_003Ec__DisplayClass21_._003CDoGameLoop_003Eg__Complete_007C0);
						_controlPrompts.HideAcceptButton();
					}
					else
					{
						yield return PerformTurn(_opponent, _003C_003Ec__DisplayClass21_._003CDoGameLoop_003Eg__Complete_007C0);
					}
					if (_003C_003Ec__DisplayClass21_.completed)
					{
						_003C_003Ec__DisplayClass21_._003CDoGameLoop_003Eg__Complete_007C0();
						break;
					}
					playerTurn = !playerTurn;
				}
			}
			yield return DoEndGame();
		}

		private IEnumerator PerformTurn(KBPlayerBase player, Action onCompleted)
		{
			KBPlayerBase oppositePlayer = GetOppositePlayer(player);
			player.HighlightMe();
			oppositePlayer.UnHighlightMe();
			yield return new WaitForSecondsRealtime(0.5f);
			AudioManager.Instance.PlayOneShot("event:/knuckle_bones/die_roll");
			Dice currentDice = UnityEngine.Object.Instantiate(player.DicePrefab, player.Position.position, Quaternion.identity, _diceContainer);
			yield return currentDice.StartCoroutine(currentDice.RollRoutine(1f));
			yield return new WaitForSecondsRealtime(0.5f);
			while (_exitConfirmationInstance != null && player == _player)
			{
				yield return null;
			}
			yield return null;
			yield return player.SelectTub(currentDice);
			yield return new WaitForSecondsRealtime(0.5f);
			if (CheckGameCompleted(player))
			{
				if (onCompleted != null)
				{
					onCompleted();
				}
			}
		}

		private IEnumerator DoEndGame()
		{
			_matchCompleted = true;
			AudioManager.Instance.SetMusicRoomID(1, "ratau_home_id");
			int score = _player.Score;
			int score2 = _opponent.Score;
			UIKnuckleBonesController.KnucklebonesResult result = UIKnuckleBonesController.KnucklebonesResult.Draw;
			if (score == score2)
			{
				_player.SetLostLoop();
				_player.SetLostLoop();
				string localizedString = KnucklebonesModel.GetLocalizedString("Draw");
				_announcementText.text = localizedString;
				_winningsText.gameObject.SetActive(false);
			}
			else
			{
				KBPlayerBase kBPlayerBase = ((score > score2) ? ((KBPlayerBase)_player) : ((KBPlayerBase)_opponent));
				KBPlayerBase oppositePlayer = GetOppositePlayer(kBPlayerBase);
				kBPlayerBase.HighlightMe();
				oppositePlayer.HighlightMe();
				kBPlayerBase.FinalizeScores();
				oppositePlayer.FinalizeScores();
				kBPlayerBase.SetWonLoop();
				oppositePlayer.SetLostLoop();
				string localizedString2 = KnucklebonesModel.GetLocalizedString("Win");
				_announcementText.text = string.Format("{0} {1} - {2}", string.Format(localizedString2, kBPlayerBase.GetLocalizedName()), kBPlayerBase.Score, oppositePlayer.Score);
				_winningsText.gameObject.SetActive(_bet > 0);
				_opponent.PlayEndGameVoiceover(score > score2);
				if (score > score2)
				{
					result = UIKnuckleBonesController.KnucklebonesResult.Win;
					_winningsText.text = string.Format("+ {0} {1}", "<sprite name=\"icon_blackgold\">", _bet);
					AudioManager.Instance.PlayOneShot("event:/Stings/knucklebones_win");
				}
				else if (score < score2)
				{
					result = UIKnuckleBonesController.KnucklebonesResult.Loss;
					_winningsText.text = string.Format("- {0} {1}", "<sprite name=\"icon_blackgold\">", _bet).Colour(StaticColors.RedColor);
					AudioManager.Instance.PlayOneShot("event:/Stings/knucklebones_lose");
				}
			}
			_announcementRectTransform.position = _announceTextStartingPosition + new Vector3(800f, 0f);
			_announcementRectTransform.DOMove(_announceTextStartingPosition, 1f).SetUpdate(true);
			_announcementCanvasGroup.DOFade(1f, 0.5f).SetUpdate(true);
			yield return new WaitForSecondsRealtime(1.5f);
			_controlPrompts.HideCancelButton();
			_controlPrompts.ShowAcceptButton();
			while (!InputManager.UI.GetAcceptButtonDown())
			{
				yield return null;
			}
			Action<UIKnuckleBonesController.KnucklebonesResult> onMatchFinished = OnMatchFinished;
			if (onMatchFinished != null)
			{
				onMatchFinished(result);
			}
		}

		private KBPlayerBase GetOppositePlayer(KBPlayerBase player)
		{
			if (!(player == _player))
			{
				return _player;
			}
			return _opponent;
		}

		private bool CheckGameCompleted(KBPlayerBase player)
		{
			return player.CheckGameCompleted();
		}

		public void ForceEndGame()
		{
			StopAllCoroutines();
			StartCoroutine(DoEndGame());
		}
	}
}
