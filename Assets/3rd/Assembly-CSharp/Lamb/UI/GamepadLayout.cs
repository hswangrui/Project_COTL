using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class GamepadLayout : UISubmenuBase
	{
		[SerializeField]
		private Selectable _selectable;

		[SerializeField]
		private InputDisplay[] _controllers;

		[SerializeField]
		private BindingPrompt[] _bindingPrompts;

		private void OnEnable()
		{
			ControlSettingsUtilities.OnGamepadLayoutChanged = (Action)Delegate.Combine(ControlSettingsUtilities.OnGamepadLayoutChanged, new Action(OnGamepadLayoutChanged));
		}

		private void OnDisable()
		{
			ControlSettingsUtilities.OnGamepadLayoutChanged = (Action)Delegate.Remove(ControlSettingsUtilities.OnGamepadLayoutChanged, new Action(OnGamepadLayoutChanged));
		}

		protected override void OnShowStarted()
		{
			OnGamepadLayoutChanged();
		}

		private void OnGamepadLayoutChanged()
		{
			Configure(InputManager.General.GetLastActiveController());
		}

		public void Configure(Controller controller)
		{
			if (controller.type == ControllerType.Keyboard || controller.type == ControllerType.Mouse)
			{
				return;
			}
			ControllerMap controllerMapForCategory = InputManager.General.GetControllerMapForCategory(0, controller);
			if (controllerMapForCategory == null || controllerMapForCategory.layoutId == 3)
			{
				return;
			}
			ControllerMap controllerMapForCategory2 = InputManager.General.GetControllerMapForCategory(1, controller);
			InputType currentInputType = ControlUtilities.GetCurrentInputType(controller);
			Platform platformFromInputType = ControlUtilities.GetPlatformFromInputType(currentInputType);
			IGamepadTemplate template = controller.GetTemplate<IGamepadTemplate>();
			List<ControllerTemplateElementTarget> list = new List<ControllerTemplateElementTarget>();
			InputDisplay[] controllers = _controllers;
			for (int i = 0; i < controllers.Length; i++)
			{
				controllers[i].Configure(currentInputType);
			}
			BindingPrompt[] bindingPrompts = _bindingPrompts;
			foreach (BindingPrompt bindingPrompt in bindingPrompts)
			{
				bindingPrompt.Platform = platformFromInputType;
				bindingPrompt.Clear();
				foreach (ActionElementMap allMap in controllerMapForCategory.AllMaps)
				{
					template.GetElementTargets(allMap, list);
					bindingPrompt.TryAddAction(list[0], allMap);
				}
				foreach (ActionElementMap allMap2 in controllerMapForCategory2.AllMaps)
				{
					if (IsValidUIAction(allMap2))
					{
						template.GetElementTargets(allMap2, list);
						bindingPrompt.TryAddAction(list[0], allMap2);
					}
				}
				bindingPrompt.FinalizeBinding();
			}
		}

		private bool IsValidUIAction(ActionElementMap actionElementMap)
		{
			int actionId = actionElementMap.actionId;
			if (actionId == 39 || (uint)(actionId - 43) <= 1u)
			{
				return true;
			}
			return false;
		}

		public Selectable ProvideSelectableForLayoutSelector()
		{
			return _selectable;
		}
	}
}
