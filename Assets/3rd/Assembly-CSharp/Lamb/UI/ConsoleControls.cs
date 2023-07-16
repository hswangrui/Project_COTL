using System;
using Rewired;
using src.Extensions;
using src.UINavigator;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class ConsoleControls : ControlsScreenBase
	{
		[Header("UI Controls")]
		[SerializeField]
		private MMSelectable_HorizontalSelector _controlLayout;

		[SerializeField]
		private MMHorizontalSelector _controlLayoutSelector;

		[Header("Layouts")]
		[SerializeField]
		private GamepadLayout _layout;

		[SerializeField]
		private GamepadBindings _bindings;

		private UIMenuBase _currentMenu;

		private string[] _allLayouts = new string[4] { "UI/Settings/Controls/Layout/LayoutA", "UI/Settings/Controls/Layout/LayoutB", "UI/Settings/Controls/Layout/LayoutC", "UI/Settings/Graphics/QualitySettingsOption/Custom" };

		public override void Awake()
		{
			base.Awake();
			_controlLayoutSelector.LocalizeContent = true;
			_controlLayoutSelector.PrefillContent(_allLayouts);
			MMHorizontalSelector controlLayoutSelector = _controlLayoutSelector;
			controlLayoutSelector.OnSelectionChanged = (Action<int>)Delegate.Combine(controlLayoutSelector.OnSelectionChanged, new Action<int>(OnLayoutSelectionChanged));
		}

		private void Update()
		{
			if (!(_currentMenu == _bindings) || MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable == null)
			{
				return;
			}
			KeybindItemProxy component;
			if (InputManager.UI.GetResetBindingButtonDown() && MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable.Selectable.gameObject.TryGetComponent<KeybindItemProxy>(out component))
			{
				if (!component.KeybindItem.IsRebindable)
				{
					return;
				}
				BindingItem joystickBinding = component.KeybindItem.JoystickBinding;
				ControllerMap controllerMapForCategory = InputManager.General.GetControllerMapForCategory(joystickBinding.Category, joystickBinding.ControllerType);
				if (controllerMapForCategory != null)
				{
					ControlSettingsUtilities.ResetBinding(controllerMapForCategory, joystickBinding.Action, joystickBinding.AxisContribution);
				}
			}
			KeybindItemProxy component2;
			if (InputManager.UI.GetUnbindButtonDown() && MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable.Selectable.gameObject.TryGetComponent<KeybindItemProxy>(out component2) && component2.KeybindItem.IsRebindable)
			{
				BindingItem joystickBinding2 = component2.KeybindItem.JoystickBinding;
				ControllerMap controllerMapForCategory2 = InputManager.General.GetControllerMapForCategory(joystickBinding2.Category, joystickBinding2.ControllerType);
				ActionElementMap actionElementMap = controllerMapForCategory2.GetActionElementMap(joystickBinding2.Action, joystickBinding2.AxisContribution);
				if (actionElementMap != null && ControlSettingsUtilities.DeleteElementMap(controllerMapForCategory2, actionElementMap))
				{
					SettingsManager.Settings.Control.GamepadBindingsUnbound.Add(actionElementMap.ToUnboundBinding());
				}
			}
		}

		public override bool ValidInputType(InputType inputType)
		{
			return inputType != InputType.Keyboard;
		}

		public override void Configure(InputType inputType)
		{
			_bindings.Configure(inputType);
			_layout.Configure(InputManager.General.GetLastActiveController());
			Configure(SettingsManager.Settings.Control);
		}

		public override void Configure(SettingsData.ControlSettings controlSettings)
		{
			_controlLayoutSelector.ContentIndex = controlSettings.GamepadLayout;
			UpdateScreen(controlSettings.GamepadLayout);
		}

		private void OnLayoutSelectionChanged(int index)
		{
			Debug.Log(string.Format("ControlSettings(Gamepad) - Layout changed to {0}", index).Colour(Color.yellow));
			SettingsManager.Settings.Control.GamepadLayout = index;
			ControlSettingsUtilities.SetGamepadLayout();
			UpdateScreen(index);
		}

		private void UpdateScreen(int layout)
		{
			UIMenuBase uIMenuBase = ((layout != 3) ? ((UISubmenuBase)_layout) : ((UISubmenuBase)_bindings));
			if (_currentMenu != uIMenuBase)
			{
				if (_currentMenu != null)
				{
					_currentMenu.Hide();
				}
				uIMenuBase.Show(_currentMenu == null);
				_currentMenu = uIMenuBase;
				Navigation navigation = _controlLayout.navigation;
				navigation.mode = Navigation.Mode.Explicit;
				if (_currentMenu == _bindings)
				{
					navigation.selectOnDown = _bindings.ProvideSelectableForLayoutSelector();
				}
				else
				{
					navigation.selectOnDown = _layout.ProvideSelectableForLayoutSelector();
				}
				_controlLayout.navigation = navigation;
			}
		}

		public override bool ShowBindingPrompts()
		{
			return _currentMenu == _bindings;
		}
	}
}
