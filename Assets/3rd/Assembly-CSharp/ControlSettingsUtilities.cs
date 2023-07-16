using System;
using System.Collections.Generic;
using Rewired;
using src.Extensions;
using UnityEngine;

public class ControlSettingsUtilities
{
	public const int kCustomBindingsIndex = 3;

	public static Action OnGamepadLayoutChanged;

	public static Action<Binding> OnRebind;

	public static Action<int> OnBindingReset;

	public static Action OnGamepadPromptsChanged;

	public static void ApplyBindings(ControllerMap controllerMap, List<Binding> bindings)
	{
		foreach (Binding binding in bindings)
		{
			if (controllerMap.categoryId == binding.Category)
			{
				ApplyBinding(controllerMap, binding);
			}
		}
	}

	private static bool ApplyBinding(ControllerMap controllerMap, Binding binding)
	{
		ActionElementMap actionElementMap = controllerMap.GetActionElementMap(binding.Action, binding.AxisContribution);
		if (actionElementMap == null)
		{
			if (controllerMap.CreateElementMap(binding.ToElementAssigment()))
			{
				Debug.Log("Map created".Colour(Color.cyan));
				return true;
			}
			Debug.Log(string.Format("Unable to assign binding for {0} in Binding attempt!", binding.Action).Colour(Color.red));
			return false;
		}
		ElementAssignment elementAssignment = binding.ToElementAssignment(actionElementMap);
		if (controllerMap.ReplaceElementMap(elementAssignment))
		{
			Debug.Log("Map replaced".Colour(Color.cyan));
			return true;
		}
		Debug.Log(string.Format("Unable to assign binding for {0} in Binding attempt!", binding.Action).Colour(Color.red));
		return false;
	}

	private static List<Binding> GetBindings(ControllerType controllerType)
	{
		switch (controllerType)
		{
		case ControllerType.Keyboard:
			return SettingsManager.Settings.Control.KeyboardBindings;
		case ControllerType.Mouse:
			return SettingsManager.Settings.Control.MouseBindings;
		case ControllerType.Joystick:
			return SettingsManager.Settings.Control.GamepadBindings;
		default:
			return null;
		}
	}

	public static bool AddBinding(Binding binding)
	{
		List<Binding> bindings = GetBindings(binding.ControllerType);
		List<UnboundBinding> unboundBindings = GetUnboundBindings(binding.ControllerType);
		if (bindings != null)
		{
			RemoveBinding(bindings, binding);
			RemoveUnboundBinding(unboundBindings, binding);
			bindings.Add(binding);
			Action<Binding> onRebind = OnRebind;
			if (onRebind != null)
			{
				onRebind(binding);
			}
			return true;
		}
		return false;
	}

	private static bool RemoveBinding(List<Binding> bindings, Binding binding)
	{
		bool result = false;
		for (int num = bindings.Count - 1; num >= 0; num--)
		{
			Binding item = bindings[num];
			if (item.Category == binding.Category && item.Action == binding.Action && item.AxisContribution == binding.AxisContribution)
			{
				Debug.Log(string.Format("Cleared binding for action {0}", item.Action).Colour(Color.red));
				bindings.Remove(item);
				result = true;
			}
		}
		return result;
	}

	public static bool ResetBinding(ControllerMap controllerMap, int action, Pole axisContribution)
	{
		ActionElementMap actionElementMap = controllerMap.GetActionElementMap(action, axisContribution);
		if (actionElementMap != null)
		{
			if (controllerMap.DeleteElementMap(actionElementMap.id))
			{
				RemoveBinding(GetBindings(controllerMap.controllerType), actionElementMap.ToBinding());
				foreach (Binding defaultBinding in GetDefaultBindings(controllerMap.categoryId, controllerMap.controllerType))
				{
					if (defaultBinding.Action == action && defaultBinding.AxisContribution == axisContribution)
					{
						controllerMap.CreateElementMap(defaultBinding.ToElementAssigment());
					}
				}
				Debug.Log("Map Reset!".Colour(Color.yellow));
				Action<int> onBindingReset = OnBindingReset;
				if (onBindingReset != null)
				{
					onBindingReset(action);
				}
				return true;
			}
		}
		else
		{
			RemoveUnboundBinding(GetUnboundBindings(controllerMap.controllerType), action, controllerMap.categoryId, axisContribution);
			foreach (Binding defaultBinding2 in GetDefaultBindings(controllerMap.categoryId, controllerMap.controllerType))
			{
				if (defaultBinding2.Action == action && defaultBinding2.AxisContribution == axisContribution && controllerMap.CreateElementMap(defaultBinding2.ToElementAssigment()))
				{
					Action<int> onBindingReset2 = OnBindingReset;
					if (onBindingReset2 != null)
					{
						onBindingReset2(action);
					}
					return true;
				}
			}
		}
		Debug.Log("Map not Reset!".Colour(Color.red));
		return false;
	}

	private static bool RemoveUnboundBinding(List<UnboundBinding> unboundBindings, UnboundBinding unboundBinding)
	{
		return RemoveUnboundBinding(unboundBindings, unboundBinding.Action, unboundBinding.Category, unboundBinding.AxisContribution);
	}

	private static bool RemoveUnboundBinding(List<UnboundBinding> unboundBindings, Binding binding)
	{
		return RemoveUnboundBinding(unboundBindings, binding.Action, binding.Category, binding.AxisContribution);
	}

	private static bool RemoveUnboundBinding(List<UnboundBinding> unboundBindings, int action, int category, Pole axisContribution)
	{
		bool result = false;
		for (int num = unboundBindings.Count - 1; num >= 0; num--)
		{
			UnboundBinding item = unboundBindings[num];
			if (item.Category == category && item.Action == action && item.AxisContribution == axisContribution)
			{
				Debug.Log(string.Format("Cleared unbound binding for action {0}", item.Action).Colour(Color.red));
				unboundBindings.Remove(item);
				result = true;
			}
		}
		return result;
	}

	public static void DeleteUnboundBindings(ControllerMap controllerMap, List<UnboundBinding> unboundBindings)
	{
		foreach (UnboundBinding unboundBinding in unboundBindings)
		{
			if (unboundBinding.Category == controllerMap.categoryId)
			{
				ActionElementMap actionElementMap = controllerMap.GetActionElementMap(unboundBinding.Action, unboundBinding.AxisContribution);
				if (actionElementMap != null)
				{
					DeleteElementMap(controllerMap, actionElementMap);
				}
			}
		}
	}

	public static bool DeleteElementMap(ControllerMap controllerMap, ActionElementMap actionElementMap)
	{
		if (controllerMap.DeleteElementMap(actionElementMap.id))
		{
			Action<int> onBindingReset = OnBindingReset;
			if (onBindingReset != null)
			{
				onBindingReset(actionElementMap.actionId);
			}
			return true;
		}
		return false;
	}

	private static List<Binding> GetDefaultBindings(int category, ControllerType controllerType)
	{
		switch (category)
		{
		case 0:
			return InputManager.Gameplay.GetDefaultBindingsForControllerType(controllerType);
		case 2:
			return InputManager.PhotoMode.GetDefaultBindingsForControllerType(controllerType);
		default:
			return InputManager.UI.GetDefaultBindingsForControllerType(controllerType);
		}
	}

	private static List<UnboundBinding> GetUnboundBindings(ControllerType controllerType)
	{
		switch (controllerType)
		{
		case ControllerType.Keyboard:
			return SettingsManager.Settings.Control.KeyboardBindingsUnbound;
		case ControllerType.Joystick:
			return SettingsManager.Settings.Control.GamepadBindingsUnbound;
		default:
			return SettingsManager.Settings.Control.MouseBindingsUnbound;
		}
	}

	public static void UpdateGamepadPrompts()
	{
		Action onGamepadPromptsChanged = OnGamepadPromptsChanged;
		if (onGamepadPromptsChanged != null)
		{
			onGamepadPromptsChanged();
		}
	}

	public static void SetGamepadLayout()
	{
		InputManager.UI.ApplyBindings();
		InputManager.Gameplay.ApplyBindings();
		InputManager.PhotoMode.ApplyBindings();
		Action onGamepadLayoutChanged = OnGamepadLayoutChanged;
		if (onGamepadLayoutChanged != null)
		{
			onGamepadLayoutChanged();
		}
	}
}
