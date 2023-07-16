using System;
using I2.Loc;
using Lamb.UI;
using src.Extensions;
using UnityEngine;

namespace src
{
	public class Interaction_RelicBook : Interaction
	{
		private bool _activating;

		protected override void OnEnable()
		{
			base.OnEnable();
			HasSecondaryInteraction = true;
		}

		public override void GetLabel()
		{
			base.Label = ScriptLocalization.Interactions.Look;
		}

		public override void GetSecondaryLabel()
		{
			base.SecondaryLabel = ScriptLocalization.UI_Generic.TarotCards;
		}

		public override void OnInteract(StateMachine state)
		{
			if (!_activating)
			{
				base.OnInteract(state);
				PlayerFarming.Instance.GoToAndStop(base.gameObject, base.gameObject, false, false, OpenRelicMenu);
			}
		}

		public override void OnSecondaryInteract(StateMachine state)
		{
			if (!_activating)
			{
				base.OnSecondaryInteract(state);
				PlayerFarming.Instance.GoToAndStop(base.gameObject, base.gameObject, false, false, OpenTarotMenu);
			}
		}

		private void OpenRelicMenu()
		{
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.InActive;
			GameManager.GetInstance().CameraSetOffset(new Vector3(-3f, 0f, 0f));
			HUD_Manager.Instance.Hide(false, 0);
			UIRelicMenuController uIRelicMenuController = MonoSingleton<UIManager>.Instance.RelicMenuTemplate.Instantiate();
			uIRelicMenuController.Show();
			uIRelicMenuController.OnHide = (Action)Delegate.Combine(uIRelicMenuController.OnHide, (Action)delegate
			{
				HUD_Manager.Instance.Show(0);
			});
			uIRelicMenuController.OnHidden = (Action)Delegate.Combine(uIRelicMenuController.OnHidden, (Action)delegate
			{
				GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0f, 0f));
				PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
				_activating = false;
				base.HasChanged = true;
			});
		}

		private void OpenTarotMenu()
		{
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.InActive;
			GameManager.GetInstance().CameraSetOffset(new Vector3(-3f, 0f, 0f));
			HUD_Manager.Instance.Hide(false, 0);
			UITarotCardsMenuController uITarotCardsMenuController = MonoSingleton<UIManager>.Instance.TarotCardsMenuTemplate.Instantiate();
			uITarotCardsMenuController.Show();
			uITarotCardsMenuController.OnHide = (Action)Delegate.Combine(uITarotCardsMenuController.OnHide, (Action)delegate
			{
				HUD_Manager.Instance.Show(0);
			});
			uITarotCardsMenuController.OnHidden = (Action)Delegate.Combine(uITarotCardsMenuController.OnHidden, (Action)delegate
			{
				GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0f, 0f));
				PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
				_activating = false;
				base.HasChanged = true;
			});
		}
	}
}
