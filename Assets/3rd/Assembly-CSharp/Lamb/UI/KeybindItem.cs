using System;
using I2.Loc;
using Rewired;
using RewiredConsts;
using src.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Lamb.UI
{
	public class KeybindItem : MonoBehaviour
	{
		[Header("Action")]
		[SerializeField]
		[ActionIdProperty(typeof(Category))]
		private int _category;

		[ActionIdProperty(typeof(RewiredConsts.Action))]
		[SerializeField]
		[FormerlySerializedAs("Action")]
		private int _action;

		[SerializeField]
		private Pole _axisContribution;

		[SerializeField]
		[TermsPopup("")]
		private string _bindingTerm;

		[SerializeField]
		private Localize _localize;

		[Header("Bindings")]
		[SerializeField]
		private bool _isRebindable = true;

		[SerializeField]
		private bool _showKeyboardBinding = true;

		[SerializeField]
		private BindingItem _keyboardBinding;

		[SerializeField]
		private bool _showMouseBinding = true;

		[SerializeField]
		private BindingItem _mousebinding;

		[SerializeField]
		private bool _showJoystickBinding = true;

		[SerializeField]
		private BindingItem _joystickBinding;

		[Header("Misc")]
		[SerializeField]
		private KeybindConflictLookup _keybindConflictLookup;

		[SerializeField]
		private GameObject _alertContainer;

		[SerializeField]
		private GameObject _lockContainer;

		[SerializeField]
		private MMButton _button;

		public int Category
		{
			get
			{
				return _category;
			}
		}

		public int Action
		{
			get
			{
				return _action;
			}
		}

		public Pole AxisContribution
		{
			get
			{
				return _axisContribution;
			}
		}

		public bool IsRebindable
		{
			get
			{
				return _isRebindable;
			}
		}

		public bool ShowKeyboardBinding
		{
			get
			{
				return _showKeyboardBinding;
			}
		}

		public bool ShowMouseBinding
		{
			get
			{
				return _showMouseBinding;
			}
		}

		public bool ShowJoystickBinding
		{
			get
			{
				return _showJoystickBinding;
			}
		}

		public BindingItem KeyboardBinding
		{
			get
			{
				return _keyboardBinding;
			}
		}

		public BindingItem MouseBinding
		{
			get
			{
				return _mousebinding;
			}
		}

		public BindingItem JoystickBinding
		{
			get
			{
				return _joystickBinding;
			}
		}

		public MMButton Button
		{
			get
			{
				return _button;
			}
		}

		public KeybindConflictLookup KeybindConflictLookup
		{
			get
			{
				return _keybindConflictLookup;
			}
		}

		private void OnValidate()
		{
			if (_localize != null)
			{
				_localize.Term = _bindingTerm;
			}
			if (_keyboardBinding != null)
			{
				if (_showKeyboardBinding)
				{
					_keyboardBinding.Category = _category;
					_keyboardBinding.Action = _action;
					_keyboardBinding.BindingTerm = _bindingTerm;
					_keyboardBinding.AxisContribution = _axisContribution;
					_keyboardBinding.ControllerType = ControllerType.Keyboard;
				}
				if (_keyboardBinding.gameObject.activeSelf != _showKeyboardBinding)
				{
					_keyboardBinding.gameObject.SetActive(_showKeyboardBinding);
				}
			}
			if (_mousebinding != null)
			{
				if (_showMouseBinding)
				{
					_mousebinding.Category = _category;
					_mousebinding.Action = _action;
					_mousebinding.BindingTerm = _bindingTerm;
					_mousebinding.AxisContribution = _axisContribution;
					_mousebinding.ControllerType = ControllerType.Mouse;
				}
				if (_mousebinding.gameObject.activeSelf != _showMouseBinding)
				{
					_mousebinding.gameObject.SetActive(_showMouseBinding);
				}
			}
			if (_joystickBinding != null)
			{
				if (_showJoystickBinding)
				{
					_joystickBinding.Category = _category;
					_joystickBinding.Action = _action;
					_joystickBinding.BindingTerm = _bindingTerm;
					_joystickBinding.AxisContribution = _axisContribution;
					_joystickBinding.ControllerType = ControllerType.Joystick;
				}
				if (_joystickBinding.gameObject.activeSelf != _showJoystickBinding)
				{
					_joystickBinding.gameObject.SetActive(_showJoystickBinding);
				}
			}
			if (_lockContainer != null && _lockContainer.activeSelf != !_isRebindable)
			{
				_lockContainer.SetActive(!_isRebindable);
			}
		}

		private void Awake()
		{
			UpdateBindingWarning();
		}

		private void OnEnable()
		{
			ControlSettingsUtilities.OnBindingReset = (Action<int>)Delegate.Combine(ControlSettingsUtilities.OnBindingReset, new Action<int>(OnBindingReset));
			ControlSettingsUtilities.OnRebind = (Action<Binding>)Delegate.Combine(ControlSettingsUtilities.OnRebind, new Action<Binding>(OnRebind));
			GeneralInputSource.OnBindingsReset = (System.Action)Delegate.Combine(GeneralInputSource.OnBindingsReset, new System.Action(UpdateBindingWarning));
		}

		private void OnDisable()
		{
			ControlSettingsUtilities.OnBindingReset = (Action<int>)Delegate.Remove(ControlSettingsUtilities.OnBindingReset, new Action<int>(OnBindingReset));
			ControlSettingsUtilities.OnRebind = (Action<Binding>)Delegate.Remove(ControlSettingsUtilities.OnRebind, new Action<Binding>(OnRebind));
			GeneralInputSource.OnBindingsReset = (System.Action)Delegate.Remove(GeneralInputSource.OnBindingsReset, new System.Action(UpdateBindingWarning));
		}

		private void OnRebind(Binding binding)
		{
			OnBindingReset(binding.Action);
		}

		private void OnBindingReset(int action)
		{
			if (action == _action)
			{
				UpdateBindingWarning();
			}
		}

		private void UpdateBindingWarning()
		{
			bool flag = false;
			if (_keyboardBinding != null && _showKeyboardBinding)
			{
				ControllerMap controllerMapForCategory = InputManager.General.GetControllerMapForCategory(_category, ControllerType.Keyboard);
				if (controllerMapForCategory != null && controllerMapForCategory.GetActionElementMap(_action, _axisContribution) != null)
				{
					flag = true;
				}
			}
			if (_mousebinding != null && _showMouseBinding)
			{
				ControllerMap controllerMapForCategory2 = InputManager.General.GetControllerMapForCategory(_category, ControllerType.Mouse);
				if (controllerMapForCategory2 != null && controllerMapForCategory2.GetActionElementMap(_action, _axisContribution) != null)
				{
					flag = true;
				}
			}
			if (_joystickBinding != null && _showJoystickBinding)
			{
				ControllerMap controllerMapForCategory3 = InputManager.General.GetControllerMapForCategory(_category, InputManager.General.GetLastActiveController());
				if (controllerMapForCategory3 != null && controllerMapForCategory3.GetActionElementMap(_action, _axisContribution) != null)
				{
					flag = true;
				}
			}
			_alertContainer.SetActive(!flag);
		}
	}
}
