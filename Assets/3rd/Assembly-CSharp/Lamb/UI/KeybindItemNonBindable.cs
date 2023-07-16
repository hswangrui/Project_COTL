using System;
using I2.Loc;
using Rewired;
using UnityEngine;

namespace Lamb.UI
{
	public class KeybindItemNonBindable : MonoBehaviour
	{
		[SerializeField]
		[ActionIdProperty(typeof(GamepadTemplate))]
		private int _button;

		[SerializeField]
		[TermsPopup("")]
		private string _bindingTerm;

		[SerializeField]
		private ControllerType _controllerType;

		[SerializeField]
		private Localize _localize;

		[SerializeField]
		private MMButtonPrompt _controlPrompt;

		public int Button
		{
			get
			{
				return _button;
			}
		}

		public string BindingTerm
		{
			get
			{
				return _bindingTerm;
			}
		}

		public ControllerType ControllerType
		{
			get
			{
				return _controllerType;
			}
		}

		private void OnEnable()
		{
			GeneralInputSource general = InputManager.General;
			general.OnActiveControllerChanged = (Action<Controller>)Delegate.Combine(general.OnActiveControllerChanged, new Action<Controller>(OnActiveControllerChanged));
			OnActiveControllerChanged(InputManager.General.GetLastActiveController());
		}

		private void OnDisable()
		{
			GeneralInputSource general = InputManager.General;
			general.OnActiveControllerChanged = (Action<Controller>)Delegate.Remove(general.OnActiveControllerChanged, new Action<Controller>(OnActiveControllerChanged));
		}

		private void OnActiveControllerChanged(Controller controller)
		{
			Platform platformFromInputType = ControlUtilities.GetPlatformFromInputType(ControlUtilities.GetCurrentInputType(controller));
			if (_controlPrompt != null)
			{
				_controlPrompt.Platform = platformFromInputType;
			}
			ForceUpdate();
		}

		private void ForceUpdate()
		{
			if (_localize != null)
			{
				_localize.Term = _bindingTerm;
			}
			if (_controlPrompt != null)
			{
				_controlPrompt.Button = _button;
			}
		}

		private void OnValidate()
		{
			ForceUpdate();
		}
	}
}
