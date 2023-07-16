using System.Collections;
using I2.Loc;
using KnuckleBones;
using Spine.Unity;
using UnityEngine;

namespace src.UI.Menus
{
	public class KBOpponent : KBPlayerBase
	{
		[SerializeField]
		private SkeletonGraphic _opponentSpine;

		private KBOpponentAI _ai;

		private KnucklebonesOpponent _opponent;

		protected override string _playDiceAnimation
		{
			get
			{
				return "knucklebones/play-dice";
			}
		}

		protected override string _playerIdleAnimation
		{
			get
			{
				return "animation";
			}
		}

		protected override string _playerTakeDiceAnimation
		{
			get
			{
				return "knucklebones/take-dice";
			}
		}

		protected override string _playerLostDiceAnimation
		{
			get
			{
				return "knucklebones/lose-dice";
			}
		}

		protected override string _playerWonAnimation
		{
			get
			{
				return "knucklebones/win-game";
			}
		}

		protected override string _playerWonLoop
		{
			get
			{
				return "knucklebones/win-game-loop";
			}
		}

		protected override string _playerLostAnimation
		{
			get
			{
				return "knucklebones/lose-game";
			}
		}

		protected override string _playerLostLoop
		{
			get
			{
				return "knucklebones/lose-game-loop";
			}
		}

		protected override SkeletonGraphic _spine
		{
			get
			{
				return _opponentSpine;
			}
		}

		public void Configure(KnucklebonesOpponent opponent, Vector2 contentOffscreenOffset, Vector2 tubOffscreenOffset)
		{
			_opponent = opponent;
			_ai = _opponent.Config.CreateAI();
			_playerName = GetLocalizedName();
			_opponentSpine.skeletonDataAsset = opponent.Config.Spine;
			_opponentSpine.initialSkinName = string.Empty;
			_opponentSpine.startingAnimation = string.Empty;
			_opponentSpine.Initialize(true);
			_opponentSpine.Skeleton.SetSkin(opponent.Config.InitialSkinName);
			_opponentSpine.AnimationState.SetAnimation(0, _playerIdleAnimation, true);
			_opponentSpine.transform.localPosition = opponent.Config.PositionOffset;
			_opponentSpine.transform.localScale = opponent.Config.Scale;
			Configure(contentOffscreenOffset, tubOffscreenOffset);
		}

		public override IEnumerator SelectTub(Dice dice)
		{
			int index = _ai.Evaluate(_diceTubs, dice);
			KBDiceTub diceTub = _diceTubs[index];
			yield return FinishTubSelection(dice, diceTub);
		}

		public void PlayEndGameVoiceover(bool victory)
		{
			if (victory)
			{
				AudioManager.Instance.PlayOneShot(_opponent.Config.DefeatAudio);
			}
			else
			{
				AudioManager.Instance.PlayOneShot(_opponent.Config.VictoryAudio);
			}
		}

		public override string GetLocalizedName()
		{
			return LocalizationManager.GetTranslation(_opponent.Config.OpponentName);
		}
	}
}
