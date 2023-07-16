using System;
using Rewired;
using UnityEngine;

namespace Lamb.UI.SettingsMenu
{
	public class ControlSettings : UISubmenuBase
	{
		[Header("General")]
		[SerializeField]
		private MMScrollRect _scrollRect;

		[SerializeField]
		private GameObject _resetBindingPrompt;

		[SerializeField]
		private GameObject _unbindPrompt;

		[Header("Categories")]
		[SerializeField]
		private ControlsScreenBase[] _screens;

		private ControlsScreenBase _currentScreen;

		private InputType _currentInputType;

		public override void Awake()
		{
			base.Awake();
			UIMenuBase parent = _parent;
			parent.OnHide = (Action)Delegate.Combine(parent.OnHide, new Action(OnHideStarted));
		}

		private void OnEnable()
		{
			GeneralInputSource general = InputManager.General;
			general.OnActiveControllerChanged = (Action<Controller>)Delegate.Combine(general.OnActiveControllerChanged, new Action<Controller>(OnActiveControllerChanged));
		}

		private void OnDisable()
		{
			GeneralInputSource general = InputManager.General;
			general.OnActiveControllerChanged = (Action<Controller>)Delegate.Remove(general.OnActiveControllerChanged, new Action<Controller>(OnActiveControllerChanged));
			_resetBindingPrompt.SetActive(false);
			_unbindPrompt.SetActive(false);
		}

		protected override void OnShowStarted()
		{
			_scrollRect.normalizedPosition = Vector2.one;
			SetActiveController(InputManager.General.GetLastActiveController());
			UIManager.PlayAudio("event:/ui/change_selection");
		}

		protected override void OnHideStarted()
		{
			_resetBindingPrompt.SetActive(false);
			_unbindPrompt.SetActive(false);
			_scrollRect.enabled = false;
			ControlsScreenBase[] screens = _screens;
			for (int i = 0; i < screens.Length; i++)
			{
				screens[i].StopAllCoroutines();
			}
		}

		private void OnActiveControllerChanged(Controller controller)
		{
			SetActiveController(controller);
		}

		private void SetActiveController(Controller controller)
		{
			SetActiveInputType(ControlUtilities.GetCurrentInputType(controller));
		}

		private void SetActiveInputType(InputType inputType)
		{
			if (_currentInputType != inputType)
			{
				_scrollRect.enabled = false;
				_scrollRect.normalizedPosition = Vector2.one;
				ControlsScreenBase[] screens = _screens;
				foreach (ControlsScreenBase controlsScreenBase in screens)
				{
					if (controlsScreenBase.ValidInputType(inputType))
					{
						if (controlsScreenBase != _currentScreen)
						{
							_currentScreen = controlsScreenBase;
							_currentScreen.Show();
						}
						_currentScreen.Configure(inputType);
					}
					else if (controlsScreenBase.isActiveAndEnabled)
					{
						controlsScreenBase.Hide();
					}
				}
				_currentInputType = inputType;
			}
			_scrollRect.enabled = true;
		}

		private void Update()
		{
			if (_currentScreen != null)
			{
				_resetBindingPrompt.SetActive(_currentScreen.ShowBindingPrompts());
				_unbindPrompt.SetActive(_currentScreen.ShowBindingPrompts());
			}
		}

		public void Configure(SettingsData.ControlSettings controlSettings)
		{
			ControlsScreenBase[] screens = _screens;
			for (int i = 0; i < screens.Length; i++)
			{
				screens[i].Configure(controlSettings);
			}
		}

		public void Reset()
		{
			SettingsManager.Settings.Control = new SettingsData.ControlSettings();
			InputManager.General.ResetBindings();
		}
	}
}
