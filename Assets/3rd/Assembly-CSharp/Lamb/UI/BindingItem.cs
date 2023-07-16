using MMTools;
using Rewired;
using UnityEngine;

namespace Lamb.UI
{
	public class BindingItem : MonoBehaviour
	{
		[SerializeField]
		[HideInInspector]
		private int _category;

		[SerializeField]
		[HideInInspector]
		private int _action;

		[SerializeField]
		[HideInInspector]
		private Pole _axisContribution;

		[SerializeField]
		private ControllerType _controllerType;

		[SerializeField]
		private MMControlPrompt _controlPrompt;

		[SerializeField]
		private MMButton _bindingButton;

		[SerializeField]
		[HideInInspector]
		private string _bindingTerm;

		public MMButton BindingButton
		{
			get
			{
				return _bindingButton;
			}
		}

		public string BindingTerm
		{
			get
			{
				return _bindingTerm;
			}
			set
			{
				_bindingTerm = value;
			}
		}

		public int Category
		{
			get
			{
				return _category;
			}
			set
			{
				_category = value;
				_controlPrompt.Category = _category;
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
				_action = value;
				_controlPrompt.Action = _action;
			}
		}

		public Pole AxisContribution
		{
			get
			{
				return _axisContribution;
			}
			set
			{
				_axisContribution = value;
				_controlPrompt.AxisContribution = (int)_axisContribution;
			}
		}

		public ControllerType ControllerType
		{
			get
			{
				return _controllerType;
			}
			set
			{
				_controllerType = value;
			}
		}

		private void OnValidate()
		{
			if (_controlPrompt != null)
			{
				_controlPrompt.Category = _category;
				_controlPrompt.AxisContribution = (int)_axisContribution;
				_controlPrompt.Action = _action;
				if (_controllerType == ControllerType.Mouse)
				{
					_controlPrompt.PrioritizeMouse = true;
				}
			}
		}
	}
}
