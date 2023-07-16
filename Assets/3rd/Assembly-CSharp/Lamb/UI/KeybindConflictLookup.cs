using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Lamb.UI.Assets;
using Rewired;
using src.Extensions;
using UnityEngine;

namespace Lamb.UI
{
	[RequireComponent(typeof(KeybindItem))]
	public class KeybindConflictLookup : MonoBehaviour
	{
		[StructLayout(LayoutKind.Auto)]
		[CompilerGenerated]
		private struct _003C_003Ec__DisplayClass5_0
		{
			public Binding binding;

			public KeybindConflictLookup _003C_003E4__this;
		}

		private KeybindItem _keybindItem;

		private int[] _conflictingBindings;

		private void OnEnable()
		{
			ControlSettingsUtilities.OnRebind = (Action<Binding>)Delegate.Combine(ControlSettingsUtilities.OnRebind, new Action<Binding>(OnRebind));
		}

		private void OnDisable()
		{
			ControlSettingsUtilities.OnRebind = (Action<Binding>)Delegate.Remove(ControlSettingsUtilities.OnRebind, new Action<Binding>(OnRebind));
		}

		public void Configure(BindingConflictResolver bindingConflictResolver)
		{
			if (_keybindItem == null)
			{
				_keybindItem = GetComponent<KeybindItem>();
			}
			BindingConflictResolver.BindingEntry entry = bindingConflictResolver.GetEntry(_keybindItem);
			if (entry != null)
			{
				_conflictingBindings = new int[entry.ConflictingBindings.Count];
				for (int i = 0; i < _conflictingBindings.Length; i++)
				{
					_conflictingBindings[i] = entry.ConflictingBindings[i];
				}
			}
		}

		private void OnRebind(Binding binding)
		{
			_003C_003Ec__DisplayClass5_0 _003C_003Ec__DisplayClass5_ = default(_003C_003Ec__DisplayClass5_0);
			_003C_003Ec__DisplayClass5_.binding = binding;
			_003C_003Ec__DisplayClass5_._003C_003E4__this = this;
			if (_conflictingBindings != null && _conflictingBindings.Length != 0 && (_003C_003Ec__DisplayClass5_.binding.Action != _keybindItem.Action || _003C_003Ec__DisplayClass5_.binding.AxisContribution != _keybindItem.AxisContribution))
			{
				Debug.Log(("Check Conflict for " + base.gameObject.name).Colour(Color.yellow));
				if (_conflictingBindings.Contains(_003C_003Ec__DisplayClass5_.binding.Action) && !_003COnRebind_003Eg__CheckBind_007C5_0(0, ref _003C_003Ec__DisplayClass5_) && !_003COnRebind_003Eg__CheckBind_007C5_0(1, ref _003C_003Ec__DisplayClass5_))
				{
					_003COnRebind_003Eg__CheckBind_007C5_0(2, ref _003C_003Ec__DisplayClass5_);
				}
			}
		}

		[CompilerGenerated]
		private bool _003COnRebind_003Eg__CheckBind_007C5_0(int category, ref _003C_003Ec__DisplayClass5_0 P_1)
		{
			ControllerMap controllerMapForCategory = InputManager.General.GetControllerMapForCategory(category, P_1.binding.ControllerType);
			ActionElementMap actionElementMap = controllerMapForCategory.GetActionElementMap(_keybindItem.Action, _keybindItem.AxisContribution);
			if (actionElementMap == null)
			{
				return false;
			}
			if (P_1.binding.ControllerType == ControllerType.Keyboard)
			{
				if (actionElementMap.keyCode == P_1.binding.KeyCode)
				{
					if (ControlSettingsUtilities.DeleteElementMap(controllerMapForCategory, actionElementMap))
					{
						SettingsManager.Settings.Control.KeyboardBindingsUnbound.Add(actionElementMap.ToUnboundBinding());
					}
					Debug.Log(string.Format("Found a conflicting bind for Action {0} for {1} - {2}", P_1.binding.Action, P_1.binding.ControllerType, _keybindItem.gameObject.name).Colour(Color.red));
					return true;
				}
			}
			else if (P_1.binding.ControllerType == ControllerType.Mouse)
			{
				if (actionElementMap.elementIdentifierId == P_1.binding.ElementIdentifierID)
				{
					if (ControlSettingsUtilities.DeleteElementMap(controllerMapForCategory, actionElementMap))
					{
						SettingsManager.Settings.Control.MouseBindingsUnbound.Add(actionElementMap.ToUnboundBinding());
					}
					Debug.Log(string.Format("Found a conflicting bind for Action {0} for {1} - {2}", P_1.binding.Action, P_1.binding.ControllerType, _keybindItem.gameObject.name).Colour(Color.red));
					return true;
				}
			}
			else if (P_1.binding.ControllerType == ControllerType.Joystick && actionElementMap.elementIdentifierId == P_1.binding.ElementIdentifierID)
			{
				if (ControlSettingsUtilities.DeleteElementMap(controllerMapForCategory, actionElementMap))
				{
					SettingsManager.Settings.Control.GamepadBindingsUnbound.Add(actionElementMap.ToUnboundBinding());
				}
				Debug.Log(string.Format("Found a conflicting bind for Action {0} for {1} - {2}", P_1.binding.Action, P_1.binding.ControllerType, _keybindItem.gameObject.name).Colour(Color.red));
				return true;
			}
			return false;
		}
	}
}
