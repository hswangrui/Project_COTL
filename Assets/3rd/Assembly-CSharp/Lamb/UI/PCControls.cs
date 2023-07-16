using Rewired;
using src.Extensions;
using src.UINavigator;
using UnityEngine;

namespace Lamb.UI
{
	public class PCControls : ControlsScreenBase
	{
		[SerializeField]
		private PCBindings _bindings;

		private void Update()
		{
			if (MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable == null)
			{
				return;
			}
			BindingItem component;
			if (InputManager.UI.GetResetBindingButtonDown() && MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable.Selectable.gameObject.TryGetComponent<BindingItem>(out component))
			{
				ControllerMap controllerMapForCategory = InputManager.General.GetControllerMapForCategory(component.Category, component.ControllerType);
				if (controllerMapForCategory != null)
				{
					ControlSettingsUtilities.ResetBinding(controllerMapForCategory, component.Action, component.AxisContribution);
				}
			}
			BindingItem component2;
			if (!InputManager.UI.GetUnbindButtonDown() || !MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable.Selectable.gameObject.TryGetComponent<BindingItem>(out component2))
			{
				return;
			}
			ControllerMap controllerMapForCategory2 = InputManager.General.GetControllerMapForCategory(component2.Category, component2.ControllerType);
			ActionElementMap actionElementMap = controllerMapForCategory2.GetActionElementMap(component2.Action, component2.AxisContribution);
			if (actionElementMap != null && ControlSettingsUtilities.DeleteElementMap(controllerMapForCategory2, actionElementMap))
			{
				if (component2.ControllerType == ControllerType.Keyboard)
				{
					SettingsManager.Settings.Control.KeyboardBindingsUnbound.Add(actionElementMap.ToUnboundBinding());
				}
				else if (component2.ControllerType == ControllerType.Mouse)
				{
					SettingsManager.Settings.Control.MouseBindingsUnbound.Add(actionElementMap.ToUnboundBinding());
				}
			}
		}

		protected override void OnShowStarted()
		{
			_bindings.Show(true);
		}

		public override void Configure(InputType inputType)
		{
			_bindings.Configure(inputType);
		}

		public override void Configure(SettingsData.ControlSettings controlSettings)
		{
		}

		public override bool ValidInputType(InputType inputType)
		{
			if (inputType == InputType.Keyboard)
			{
				return true;
			}
			return false;
		}

		public override bool ShowBindingPrompts()
		{
			return true;
		}
	}
}
