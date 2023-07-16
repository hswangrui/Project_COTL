using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using I2.Loc;
using Lamb.UI.Assets;
using Rewired;
using src.Extensions;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class UIControlBindingOverlayController : UIMenuBase
	{
		[StructLayout(LayoutKind.Auto)]
		[CompilerGenerated]
		private struct _003C_003Ec__DisplayClass21_0
		{
			public UIControlBindingOverlayController _003C_003E4__this;

			public ControllerPollingInfo pollingInfo;
		}

		[StructLayout(LayoutKind.Auto)]
		[CompilerGenerated]
		private struct _003C_003Ec__DisplayClass21_1
		{
			public ActionElementMap aem;
		}

		[SerializeField]
		private TextMeshProUGUI _header;

		[SerializeField]
		private Localize _promptLocalize;

		[SerializeField]
		private TextMeshProUGUI _prompt;

		private int _category;

		private int _action;

		private Pole _axisContribution;

		private ControllerType _controllerType;

		private InputMapper _inputMapper;

		private string _term;

		private Controller _controller;

		private ControllerMap _controllerMap;

		private ControllerMap _uiControllerMap;

		private ControllerMap _photoModeControllerMap;

		private ControllerMap _cancelMap;

		private ControllerMap _targetControllerMap;

		private ActionElementMap _actionElementMap;

		private BindingConflictResolver _bindingConflictResolver;

		private BindingConflictResolver.BindingEntry _bindingEntry;

		public void Show(BindingConflictResolver bindingConflictResolver, string term, int category, int action, Pole axisContribution, ControllerType controllerType, bool instant = false)
		{
			_bindingConflictResolver = bindingConflictResolver;
			_bindingEntry = _bindingConflictResolver.GetEntry(category, action);
			_term = term;
			_category = category;
			_action = action;
			_axisContribution = axisContribution;
			_controllerType = controllerType;
			Show(instant);
		}

		protected override IEnumerator DoShowAnimation()
		{
			if (_controllerType == ControllerType.Keyboard)
			{
				InputManager.General.RemoveController(ControllerType.Mouse);
			}
			yield return null;
			_controller = InputManager.General.GetController(_controllerType);
			_controllerMap = InputManager.Gameplay.GetControllerMap(_controller);
			_uiControllerMap = InputManager.UI.GetControllerMap(_controller);
			_photoModeControllerMap = InputManager.PhotoMode.GetControllerMap(_controller);
			if (_controllerType == ControllerType.Mouse)
			{
				_cancelMap = InputManager.UI.GetControllerMap(InputManager.General.GetController(ControllerType.Keyboard));
			}
			else
			{
				_cancelMap = InputManager.UI.GetControllerMap(_controller);
			}
			if (_category == 0)
			{
				_targetControllerMap = _controllerMap;
			}
			else if (_category == 1)
			{
				_targetControllerMap = _uiControllerMap;
			}
			else if (_category == 2)
			{
				_targetControllerMap = _photoModeControllerMap;
			}
			ActionElementMap[] elementMapsWithAction = _targetControllerMap.GetElementMapsWithAction(_action);
			foreach (ActionElementMap actionElementMap in elementMapsWithAction)
			{
				if (actionElementMap.axisContribution == _axisContribution)
				{
					_actionElementMap = actionElementMap;
					break;
				}
			}
			ActionElementMap actionElementMap2 = _cancelMap.GetElementMapsWithAction(61)[0];
			_header.text = LocalizationManager.GetTranslation(_term);
			if (_controllerType == ControllerType.Joystick)
			{
				_promptLocalize.Term = "UI/Settings/Controls/Bindings/BindPromptAlt";
			}
			else
			{
				_promptLocalize.Term = "UI/Settings/Controls/Bindings/BindPrompt";
				_prompt.text = string.Format(_prompt.text, actionElementMap2.keyboardKeyCode.ToString());
			}
			yield return _003C_003En__0();
		}

		protected override void OnShowCompleted()
		{
			InputMapper.Context mappingContext = new InputMapper.Context
			{
				controllerMap = _targetControllerMap,
				actionId = _action,
				actionElementMapToReplace = _actionElementMap,
				actionRange = _axisContribution.ToAxisRange()
			};
			_inputMapper = InputMapper.Default;
			InputMapper.Options options = _inputMapper.options;
			options.isElementAllowedCallback = (Predicate<ControllerPollingInfo>)Delegate.Combine(options.isElementAllowedCallback, new Predicate<ControllerPollingInfo>(IsElementAllowed));
			_inputMapper.options.allowKeyboardModifierKeyAsPrimary = true;
			_inputMapper.options.holdDurationToMapKeyboardModifierKeyAsPrimary = 0f;
			_inputMapper.Start(mappingContext);
			_inputMapper.InputMappedEvent += InputMappedEvent;
			_inputMapper.ConflictFoundEvent += ConflictFoundEvent;
		}

		private bool IsElementAllowed(ControllerPollingInfo pollingInfo)
		{
			_003C_003Ec__DisplayClass21_0 _003C_003Ec__DisplayClass21_ = default(_003C_003Ec__DisplayClass21_0);
			_003C_003Ec__DisplayClass21_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass21_.pollingInfo = pollingInfo;
			UnityEngine.Debug.Log("Is Element Allowed?".Colour(Color.yellow));
			ActionElementMap[] elementMapsWithAction = _cancelMap.GetElementMapsWithAction(61);
			foreach (ActionElementMap actionElementMap in elementMapsWithAction)
			{
				if (_003C_003Ec__DisplayClass21_.pollingInfo.controllerType == ControllerType.Keyboard && actionElementMap.keyCode == _003C_003Ec__DisplayClass21_.pollingInfo.keyboardKey)
				{
					UnityEngine.Debug.Log("Key Matched Disallowed Action - Element not allowed!".Colour(Color.red));
					return false;
				}
				if (_003C_003Ec__DisplayClass21_.pollingInfo.controllerType == ControllerType.Joystick && actionElementMap.elementIdentifierId == _003C_003Ec__DisplayClass21_.pollingInfo.elementIdentifierId)
				{
					UnityEngine.Debug.Log("Key Matched Disallowed Action - Element not allowed!".Colour(Color.red));
					return false;
				}
			}
			if (_bindingEntry != null && _bindingEntry.ConflictingBindings != null)
			{
				_003C_003Ec__DisplayClass21_1 _003C_003Ec__DisplayClass21_2 = default(_003C_003Ec__DisplayClass21_1);
				foreach (int conflictingBinding in _bindingEntry.ConflictingBindings)
				{
					_003C_003Ec__DisplayClass21_2.aem = _cancelMap.GetActionElementMap(conflictingBinding, _axisContribution);
					if (_003C_003Ec__DisplayClass21_2.aem != null)
					{
						if (_003CIsElementAllowed_003Eg__CheckLockedConflict_007C21_0(1, conflictingBinding, ref _003C_003Ec__DisplayClass21_, ref _003C_003Ec__DisplayClass21_2))
						{
							return false;
						}
						if (_003CIsElementAllowed_003Eg__CheckLockedConflict_007C21_0(0, conflictingBinding, ref _003C_003Ec__DisplayClass21_, ref _003C_003Ec__DisplayClass21_2))
						{
							return false;
						}
						if (_003CIsElementAllowed_003Eg__CheckLockedConflict_007C21_0(2, conflictingBinding, ref _003C_003Ec__DisplayClass21_, ref _003C_003Ec__DisplayClass21_2))
						{
							return false;
						}
					}
				}
			}
			if (_003C_003Ec__DisplayClass21_.pollingInfo.elementType == ControllerElementType.Axis)
			{
				if (_003C_003Ec__DisplayClass21_.pollingInfo.elementIdentifierId == 4 || _003C_003Ec__DisplayClass21_.pollingInfo.elementIdentifierId == 5)
				{
					return true;
				}
				UnityEngine.Debug.Log("Element Disallowed Action - Axis not allowed!".Colour(Color.red));
				return false;
			}
			return true;
		}

		private void InputMappedEvent(InputMapper.InputMappedEventData data)
		{
			if (_controllerType == ControllerType.Keyboard)
			{
				UnityEngine.Debug.Log(string.Format("Keyboard Input Mapped Event - Map {0} to {1}", data.actionElementMap.actionId, data.actionElementMap.keyCode).Colour(Color.yellow));
			}
			else if (_controllerType == ControllerType.Mouse)
			{
				UnityEngine.Debug.Log(string.Format("Mouse Input Mapped Event - Map {0} to {1}", data.actionElementMap.actionId, (MouseInputElement)data.actionElementMap.elementIdentifierId).Colour(Color.yellow));
			}
			else if (_controllerType == ControllerType.Joystick)
			{
				UnityEngine.Debug.Log(string.Format("Joystick Input Mapped Event - Map {0} to {1}", data.actionElementMap.actionId, data.actionElementMap.elementIdentifierId).Colour(Color.yellow));
			}
			Hide();
			ControlSettingsUtilities.AddBinding(data.actionElementMap.ToBinding());
		}

		private void ConflictFoundEvent(InputMapper.ConflictFoundEventData data)
		{
			UnityEngine.Debug.Log("Conflict Found Event".Colour(Color.red));
			data.responseCallback(InputMapper.ConflictResponse.Add);
		}

		private void Update()
		{
			if (InputManager.UI.GetCancelBindingButtonDown())
			{
				CancelBinding();
			}
		}

		private void CancelBinding()
		{
			if (_inputMapper != null && _inputMapper.status == InputMapper.Status.Listening)
			{
				_inputMapper.Stop();
			}
			if (_canvasGroup.interactable)
			{
				Hide();
			}
		}

		protected override IEnumerator DoHide()
		{
			yield return null;
			yield return _003C_003En__1();
		}

		protected override void OnHideCompleted()
		{
			if (_controllerType == ControllerType.Keyboard)
			{
				InputManager.General.AddController(ControllerType.Mouse);
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		[CompilerGenerated]
		[DebuggerHidden]
		private IEnumerator _003C_003En__0()
		{
			return base.DoShowAnimation();
		}

		[CompilerGenerated]
		private bool _003CIsElementAllowed_003Eg__CheckLockedConflict_007C21_0(int category, int conflictAction, ref _003C_003Ec__DisplayClass21_0 P_2, ref _003C_003Ec__DisplayClass21_1 P_3)
		{
			BindingConflictResolver.BindingEntry entry = _bindingConflictResolver.GetEntry(category, conflictAction);
			if (entry != null && P_3.aem.elementIdentifierId == P_2.pollingInfo.elementIdentifierId)
			{
				if (entry.LockedOnGamepad && _controller.type == ControllerType.Joystick)
				{
					return true;
				}
				if (entry.LockedOnKeyboard && _controller.type == ControllerType.Keyboard)
				{
					return true;
				}
				if (entry.LockedOnMouse && _controller.type == ControllerType.Mouse)
				{
					return true;
				}
			}
			return false;
		}

		[CompilerGenerated]
		[DebuggerHidden]
		private IEnumerator _003C_003En__1()
		{
			return base.DoHide();
		}
	}
}
