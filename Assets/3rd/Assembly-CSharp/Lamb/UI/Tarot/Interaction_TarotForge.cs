using System;
using I2.Loc;
using src.Extensions;
using UnityEngine;

namespace Lamb.UI.Tarot
{
	public class Interaction_TarotForge : Interaction
	{
		public GameObject Menu;

		public GameObject PlayerLookTo;

		private string sString;

		private string relicString;

		private bool Activating;

		private void Start()
		{
			UpdateLocalisation();
			HasSecondaryInteraction = DataManager.Instance.OnboardedRelics;
		}

		public override void UpdateLocalisation()
		{
			base.UpdateLocalisation();
			sString = (Activating ? "" : ScriptLocalization.Interactions.Look);
		}

		public override void GetLabel()
		{
			base.Label = sString;
		}

		public override void GetSecondaryLabel()
		{
			if (HasSecondaryInteraction)
			{
				base.SecondaryLabel = ScriptLocalization.Interactions.ViewRelics;
			}
		}

		public override void OnInteract(StateMachine state)
		{
			if (!Activating)
			{
				base.OnInteract(state);
				PlayerFarming.Instance.GoToAndStop(base.gameObject, PlayerLookTo, false, false, delegate
				{
					OpenMenu(MonoSingleton<UIManager>.Instance.TarotCardsMenuTemplate.Instantiate());
				});
			}
		}

		public override void OnSecondaryInteract(StateMachine state)
		{
			if (!Activating)
			{
				base.OnSecondaryInteract(state);
				PlayerFarming.Instance.GoToAndStop(base.gameObject, PlayerLookTo, false, false, delegate
				{
					OpenMenu(MonoSingleton<UIManager>.Instance.RelicMenuTemplate.Instantiate());
				});
			}
		}

		private void OpenMenu(UIMenuBase menu)
		{
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.InActive;
			GameManager.GetInstance().CameraSetOffset(new Vector3(-3f, 0f, 0f));
			HUD_Manager.Instance.Hide(false, 0);
			menu.Show();
			menu.OnHide = (Action)Delegate.Combine(menu.OnHide, (Action)delegate
			{
				HUD_Manager.Instance.Show(0);
				base.HasChanged = true;
			});
			menu.OnHidden = (Action)Delegate.Combine(menu.OnHidden, (Action)delegate
			{
				GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0f, 0f));
				PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
				Activating = false;
			});
		}
	}
}
