using System;
using System.Collections.Generic;
using Rewired;
using src.UINavigator;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class InputController : BaseMonoBehaviour
	{
		[SerializeField]
		private InputIdentifier[] _buttons;

		private ControllerType[] _controllerTypes = new ControllerType[3]
		{
			ControllerType.Keyboard,
			ControllerType.Mouse,
			ControllerType.Joystick
		};

		private void OnEnable()
		{
			HideAll();
			UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
			instance.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Combine(instance.OnSelectionChanged, new Action<Selectable, Selectable>(OnSelectionChanged));
			UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
			instance2.OnDefaultSetComplete = (Action<Selectable>)Delegate.Combine(instance2.OnDefaultSetComplete, new Action<Selectable>(OnSelection));
		}

		private void OnDisable()
		{
			if (MonoSingleton<UINavigatorNew>.Instance != null)
			{
				UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
				instance.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Remove(instance.OnSelectionChanged, new Action<Selectable, Selectable>(OnSelectionChanged));
				UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
				instance2.OnDefaultSetComplete = (Action<Selectable>)Delegate.Remove(instance2.OnDefaultSetComplete, new Action<Selectable>(OnSelection));
			}
		}

		private void OnSelectionChanged(Selectable current, Selectable previous)
		{
			OnSelection(current);
		}

		private void OnSelection(Selectable current)
		{
			ControllerType[] controllerTypes = _controllerTypes;
			foreach (ControllerType controllerType in controllerTypes)
			{
				HideAll(controllerType);
			}
			KeybindItemProxy component;
			BindingItem component2;
			KeybindItemNonBindable component3;
			BindingPrompt component4;
			if (current.TryGetComponent<KeybindItemProxy>(out component))
			{
				if (component.KeybindItem.ShowKeyboardBinding)
				{
					Select(component.KeybindItem.KeyboardBinding);
				}
				if (component.KeybindItem.ShowMouseBinding)
				{
					Select(component.KeybindItem.MouseBinding);
				}
				if (component.KeybindItem.ShowJoystickBinding)
				{
					Select(component.KeybindItem.JoystickBinding);
				}
			}
			else if (current.TryGetComponent<BindingItem>(out component2))
			{
				Select(component2);
			}
			else if (current.TryGetComponent<KeybindItemNonBindable>(out component3))
			{
				InputIdentifier[] buttons = _buttons;
				foreach (InputIdentifier inputIdentifier in buttons)
				{
					if (inputIdentifier.Button == component3.Button)
					{
						inputIdentifier.Show();
					}
					else
					{
						inputIdentifier.Hide();
					}
				}
			}
			else if (current.TryGetComponent<BindingPrompt>(out component4))
			{
				InputIdentifier[] buttons = _buttons;
				foreach (InputIdentifier inputIdentifier2 in buttons)
				{
					if (inputIdentifier2.Button == component4.Button)
					{
						inputIdentifier2.Show();
					}
					else
					{
						inputIdentifier2.Hide();
					}
				}
			}
			else
			{
				HideAll();
			}
		}

		private void Select(BindingItem bindingItem)
		{
			if (bindingItem == null)
			{
				return;
			}
			Controller controller = InputManager.General.GetController(bindingItem.ControllerType);
			if (controller == null || controller.type != bindingItem.ControllerType)
			{
				return;
			}
			ControllerMap controllerMap = null;
			if (bindingItem.Category == 0)
			{
				controllerMap = InputManager.Gameplay.GetControllerMap(controller);
			}
			else if (bindingItem.Category == 1)
			{
				controllerMap = InputManager.UI.GetControllerMap(controller);
			}
			else
			{
				if (bindingItem.Category != 2)
				{
					return;
				}
				controllerMap = InputManager.PhotoMode.GetControllerMap(controller);
			}
			if (controllerMap == null)
			{
				return;
			}
			ActionElementMap actionElementMap = null;
			ActionElementMap[] elementMapsWithAction = controllerMap.GetElementMapsWithAction(bindingItem.Action);
			ActionElementMap[] array = elementMapsWithAction;
			foreach (ActionElementMap actionElementMap2 in array)
			{
				if (bindingItem.AxisContribution == actionElementMap2.axisContribution)
				{
					actionElementMap = actionElementMap2;
					break;
				}
			}
			if (actionElementMap == null)
			{
				if (elementMapsWithAction.Length == 0)
				{
					return;
				}
				actionElementMap = elementMapsWithAction[0];
			}
			InputIdentifier[] buttons;
			if (bindingItem.ControllerType == ControllerType.Keyboard)
			{
				buttons = _buttons;
				foreach (InputIdentifier inputIdentifier in buttons)
				{
					if (inputIdentifier.ControllerType == controller.type)
					{
						if (inputIdentifier.KeyboardKeyCode == actionElementMap.keyboardKeyCode)
						{
							inputIdentifier.Show();
						}
						else
						{
							inputIdentifier.Hide();
						}
					}
				}
				return;
			}
			if (bindingItem.ControllerType == ControllerType.Mouse)
			{
				buttons = _buttons;
				foreach (InputIdentifier inputIdentifier2 in buttons)
				{
					if (inputIdentifier2.ControllerType == controller.type)
					{
						if (inputIdentifier2.MouseInputElement == (MouseInputElement)actionElementMap.elementIdentifierId)
						{
							inputIdentifier2.Show();
						}
						else
						{
							inputIdentifier2.Hide();
						}
					}
				}
				return;
			}
			IGamepadTemplate template = controller.GetTemplate<IGamepadTemplate>();
			List<ControllerTemplateElementTarget> list = new List<ControllerTemplateElementTarget>();
			template.GetElementTargets(actionElementMap, list);
			int id = list[0].element.id;
			buttons = _buttons;
			foreach (InputIdentifier inputIdentifier3 in buttons)
			{
				if (inputIdentifier3.ControllerType == controller.type)
				{
					if (inputIdentifier3.Button == id)
					{
						inputIdentifier3.Show();
					}
					else
					{
						inputIdentifier3.Hide();
					}
				}
			}
		}

		private void HideAll()
		{
			HideAll(ControllerType.Keyboard, true);
			HideAll(ControllerType.Mouse, true);
			HideAll(ControllerType.Joystick, true);
		}

		private void HideAll(ControllerType controllerType, bool instant = false)
		{
			InputIdentifier[] buttons = _buttons;
			foreach (InputIdentifier inputIdentifier in buttons)
			{
				if (inputIdentifier.ControllerType == controllerType)
				{
					inputIdentifier.Hide(instant);
				}
			}
		}
	}
}
