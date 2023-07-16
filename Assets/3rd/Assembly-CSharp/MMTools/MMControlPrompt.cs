using System;
using System.Collections.Generic;
using Rewired;
using RewiredConsts;
using src.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MMTools
{
	public class MMControlPrompt : MonoBehaviour
	{
		[Header("Binding")]
		[SerializeField]
		[ActionIdProperty(typeof(Category))]
		[FormerlySerializedAs("Category")]
		private int _category;

		[SerializeField]
		[ActionIdProperty(typeof(RewiredConsts.Action))]
		[FormerlySerializedAs("Action")]
		private int _action;

		[ActionIdProperty(typeof(Pole))]
		public int AxisContribution;

		[SerializeField]
		private bool _prioritizeMouse = true;

		[SerializeField]
		private bool _prioritizeMouseFallbackToKeyboard = true;

		[Header("Prompt")]
		[SerializeField]
		private ControlMappings _controlMappings;

		[SerializeField]
		private TextMeshProUGUI _text;

		[SerializeField]
		private TextMeshProUGUI _iconText;

		[SerializeField]
		private Image _icon;

		public int Category
		{
			get
			{
				return _category;
			}
			set
			{
				if (_category != value)
				{
					_category = value;
					ForceUpdate();
				}
			}
		}

		public int Action
		{
			get
			{
				return _action;
			}
			set
			{
				if (_action != value)
				{
					_action = value;
					ForceUpdate();
				}
			}
		}

		public bool PrioritizeMouse
		{
			get
			{
				return _prioritizeMouse;
			}
			set
			{
				if (_prioritizeMouse != value)
				{
					_prioritizeMouse = value;
					ForceUpdate();
				}
			}
		}

		private void OnEnable()
		{
			GeneralInputSource general = InputManager.General;
			general.OnActiveControllerChanged = (Action<Controller>)Delegate.Combine(general.OnActiveControllerChanged, new Action<Controller>(OnActiveControllerChanged));
			ControlSettingsUtilities.OnGamepadLayoutChanged = (System.Action)Delegate.Combine(ControlSettingsUtilities.OnGamepadLayoutChanged, new System.Action(ForceUpdate));
			ControlSettingsUtilities.OnRebind = (Action<Binding>)Delegate.Combine(ControlSettingsUtilities.OnRebind, new Action<Binding>(OnRebind));
			ControlSettingsUtilities.OnBindingReset = (Action<int>)Delegate.Combine(ControlSettingsUtilities.OnBindingReset, new Action<int>(OnBindingReset));
			ControlSettingsUtilities.OnGamepadPromptsChanged = (System.Action)Delegate.Combine(ControlSettingsUtilities.OnGamepadPromptsChanged, new System.Action(ForceUpdate));
			GeneralInputSource.OnBindingsReset = (System.Action)Delegate.Combine(GeneralInputSource.OnBindingsReset, new System.Action(ForceUpdate));
			ForceUpdate();
		}

		private void OnDisable()
		{
			GeneralInputSource general = InputManager.General;
			general.OnActiveControllerChanged = (Action<Controller>)Delegate.Remove(general.OnActiveControllerChanged, new Action<Controller>(OnActiveControllerChanged));
			ControlSettingsUtilities.OnGamepadLayoutChanged = (System.Action)Delegate.Remove(ControlSettingsUtilities.OnGamepadLayoutChanged, new System.Action(ForceUpdate));
			ControlSettingsUtilities.OnRebind = (Action<Binding>)Delegate.Remove(ControlSettingsUtilities.OnRebind, new Action<Binding>(OnRebind));
			ControlSettingsUtilities.OnBindingReset = (Action<int>)Delegate.Remove(ControlSettingsUtilities.OnBindingReset, new Action<int>(OnBindingReset));
			ControlSettingsUtilities.OnGamepadPromptsChanged = (System.Action)Delegate.Remove(ControlSettingsUtilities.OnGamepadPromptsChanged, new System.Action(ForceUpdate));
			GeneralInputSource.OnBindingsReset = (System.Action)Delegate.Remove(GeneralInputSource.OnBindingsReset, new System.Action(ForceUpdate));
		}

		public void ForceUpdate()
		{
			if (InputManager.General.GetLastActiveController() != null)
			{
				OnActiveControllerChanged(InputManager.General.GetLastActiveController());
			}
		}

		private void OnRebind(Binding binding)
		{
			if (binding.Action == Action)
			{
				ForceUpdate();
			}
		}

		private void OnBindingReset(int action)
		{
			if (action == Action)
			{
				ForceUpdate();
			}
		}

		private void OnActiveControllerChanged(Controller controller)
		{
			if (controller != null)
			{
				if (controller.type != ControllerType.Joystick && _prioritizeMouse)
				{
					Controller controller2 = InputManager.General.GetController(ControllerType.Mouse);
					if (controller2 != null && controller2.isConnected && controller2.enabled)
					{
						controller = controller2;
					}
				}
				else if (controller.type == ControllerType.Mouse)
				{
					controller = InputManager.General.GetController(ControllerType.Keyboard);
				}
			}
			if (controller != null)
			{
				ActionElementMap actionElementMap = GetActionElementMap(controller);
				if (!AssignPrompt(actionElementMap, controller) && _prioritizeMouse && controller.type == ControllerType.Mouse && _prioritizeMouseFallbackToKeyboard)
				{
					controller = InputManager.General.GetController(ControllerType.Keyboard);
					actionElementMap = GetActionElementMap(controller);
					AssignPrompt(actionElementMap, controller);
				}
			}
		}

		private ActionElementMap GetActionElementMap(Controller controller)
		{
			ControllerMap controllerMap = null;
			if (Category == 0)
			{
				controllerMap = InputManager.Gameplay.GetControllerMap(controller);
			}
			else if (Category == 1)
			{
				controllerMap = InputManager.UI.GetControllerMap(controller);
			}
			else if (Category == 2)
			{
				controllerMap = InputManager.PhotoMode.GetControllerMap(controller);
			}
			if (controllerMap == null)
			{
				Debug.Log(string.Format("Unable to determine Controller Map! - {0}", Category).Colour(Color.red));
				return null;
			}
			return controllerMap.GetActionElementMap(Action, (Pole)AxisContribution);
		}

		private bool AssignPrompt(ActionElementMap actionElementMap, Controller controller)
		{
			Debug.Log(controller.name);
			if (actionElementMap == null)
			{
				Debug.Log(string.Format("Unable to determine ActionElementMap for {0}!", Action).Colour(Color.red));
				_iconText.font = _controlMappings.GetFontForPlatform(Platform.PC);
				_iconText.fontSize = 50f;
				_iconText.enableAutoSizing = false;
				_iconText.fontStyle = FontStyles.Normal;
				_icon.gameObject.SetActive(false);
				_text.gameObject.SetActive(false);
				_iconText.gameObject.SetActive(true);
				_iconText.text = "--";
				_iconText.verticalAlignment = VerticalAlignmentOptions.Capline;
				return false;
			}
			if (actionElementMap.controllerMap.controllerType == ControllerType.Keyboard)
			{
				bool isSpecialCharacter;
				string keyboardCode = ControlMappings.GetKeyboardCode(actionElementMap.keyboardKeyCode, out isSpecialCharacter);
				if (!isSpecialCharacter)
				{
					_text.text = keyboardCode;
					_text.gameObject.SetActive(true);
					_text.fontSize = 30f;
					_text.fontSizeMin = 10f;
					_text.fontSizeMax = 30f;
					_text.enableAutoSizing = true;
					_iconText.gameObject.SetActive(false);
					_iconText.fontStyle = FontStyles.Normal;
					_icon.gameObject.SetActive(true);
				}
				else
				{
					SetSpecialPCIcon(keyboardCode);
				}
			}
			else if (actionElementMap.controllerMap.controllerType == ControllerType.Mouse)
			{
				SetSpecialPCIcon(ControlMappings.GetMouseCode((MouseInputElement)actionElementMap.elementIdentifierId, (Pole)AxisContribution));
			}
			else if (actionElementMap.controllerMap.controllerType == ControllerType.Joystick)
			{
				Platform platformFromInputType = ControlUtilities.GetPlatformFromInputType(ControlUtilities.GetCurrentInputType(controller));
				IGamepadTemplate template = controller.GetTemplate<IGamepadTemplate>();
				List<ControllerTemplateElementTarget> list = new List<ControllerTemplateElementTarget>();
				template.GetElementTargets(actionElementMap, list);
				_iconText.font = _controlMappings.GetFontForPlatform(platformFromInputType);
				_iconText.fontSize = 42f;
				_iconText.enableAutoSizing = false;
				_iconText.fontStyle = FontStyles.Normal;
				_icon.gameObject.SetActive(false);
				_text.gameObject.SetActive(false);
				_iconText.gameObject.SetActive(true);
				_iconText.verticalAlignment = VerticalAlignmentOptions.Geometry;
				_iconText.text = ControlMappings.GetControllerCodeFromID(list[0].element.id);
			}
			return true;
		}

		private void SetSpecialPCIcon(string icon)
		{
			_iconText.font = _controlMappings.GetFontForPlatform(Platform.PC);
			_iconText.text = icon;
			_iconText.fontSize = 70f;
			_iconText.fontStyle = FontStyles.Bold;
			_iconText.enableAutoSizing = false;
			_iconText.gameObject.SetActive(true);
			_iconText.verticalAlignment = VerticalAlignmentOptions.Geometry;
			_text.gameObject.SetActive(false);
			_icon.gameObject.SetActive(false);
		}
	}
}
