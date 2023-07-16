using src.Extensions;
using UnityEngine;

namespace Lamb.UI
{
	public class PlaystationControllers : InputDisplay
	{
		[SerializeField]
		private RectTransform _dualShock4Container;

		[SerializeField]
		private RectTransform _dualSenseContainer;

		private GameObject _dualShock4;

		private GameObject _dualSense;

		public override void Configure(InputType inputType)
		{
			if (_dualShock4 == null && MonoSingleton<UIManager>.Instance.PS4ControllerTemplate != null)
			{
				_dualShock4 = GameObjectExtensions.Instantiate(MonoSingleton<UIManager>.Instance.PS4ControllerTemplate, _dualShock4Container).gameObject;
				_dualShock4.transform.localPosition = Vector3.zero;
				_dualShock4.transform.localScale = Vector3.one;
			}
			if (_dualSense == null && MonoSingleton<UIManager>.Instance.PS5ControllerTemplate != null)
			{
				_dualSense = GameObjectExtensions.Instantiate(MonoSingleton<UIManager>.Instance.PS5ControllerTemplate, _dualSenseContainer).gameObject;
				_dualSense.transform.localPosition = Vector3.zero;
				_dualSense.transform.localScale = Vector3.one;
			}
			_dualShock4.SetActive(inputType == InputType.DualShock4);
			_dualSense.SetActive(inputType == InputType.DualSense);
		}
	}
}
