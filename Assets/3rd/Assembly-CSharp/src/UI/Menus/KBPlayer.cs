using System.Collections;
using System.Reflection;
using I2.Loc;
using KnuckleBones;
using Spine.Unity;
using src.UINavigator;
using UnityEngine;
using static UINavigator;

namespace src.UI.Menus
{
	public class KBPlayer : KBPlayerBase
	{
		[SerializeField]
		private SkeletonGraphic _lambSpine;

		private MMButton[] _tubSelectables;

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
				return "knucklebones/idle";
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
				return _lambSpine;
			}
		}

		public override void Configure(Vector2 contentOffscreenOffset, Vector2 tubOffscreenOffset)
		{
			_playerName = GetLocalizedName();
			_tubSelectables = new MMButton[_diceTubs.Count];
			for (int i = 0; i < _diceTubs.Count; i++)
			{
				_tubSelectables[i] = _diceTubs[i].GetComponent<MMButton>();
				_tubSelectables[i].interactable = false;
			}
			base.Configure(contentOffscreenOffset, tubOffscreenOffset);
		}

		public override IEnumerator SelectTub(Dice dice)
		{
			KBDiceTub diceTub = null;
			MMButton[] tubSelectables = _tubSelectables;
            bool chosen = false;
            foreach (MMButton button in tubSelectables)
            {
                button.interactable = true;
                button.onClick.AddListener(() =>
                {

                    int index = _tubSelectables.IndexOf(button);

                    if (_diceTubs[index].TrySelectTub())
                    {
                        diceTub = _diceTubs[index];
                        chosen = true;
                    }

                });
            }
            MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(_tubSelectables[1]);
		
			while (!chosen)
			{
				yield return null;
			}
			tubSelectables = _tubSelectables;
			foreach (MMButton obj in tubSelectables)
			{
				obj.interactable = false;
				obj.onClick.RemoveAllListeners();
			}
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			yield return FinishTubSelection(dice, diceTub);
		}


		private void Choose(int index)
        {
           
        }
        public override string GetLocalizedName()
		{
			return ScriptLocalization.NAMES_Knucklebones.Player;
		}
	}
}
