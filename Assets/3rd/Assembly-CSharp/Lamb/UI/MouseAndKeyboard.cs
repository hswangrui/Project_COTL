using src.Extensions;
using UnityEngine;

namespace Lamb.UI
{
	public class MouseAndKeyboard : InputDisplay
	{
		[SerializeField]
		private RectTransform _keyboardContainer;

		[SerializeField]
		private RectTransform _mouseContainer;

		private GameObject _keyboard;

		private GameObject _mouse;

		public override void Configure(InputType inputType)
		{
			if (_keyboard == null)
			{
				_keyboard = GameObjectExtensions.Instantiate(MonoSingleton<UIManager>.Instance.KeyboardTemplate, _keyboardContainer).gameObject;
				_keyboard.transform.localPosition = Vector3.zero;
				_keyboard.transform.localScale = Vector3.one;
			}
			if (_mouse == null)
			{
				_mouse = GameObjectExtensions.Instantiate(MonoSingleton<UIManager>.Instance.MouseTemplate, _mouseContainer).gameObject;
				_mouse.transform.localPosition = Vector3.zero;
				_mouse.transform.localScale = Vector3.one;
			}
			_keyboard.SetActive(inputType == InputType.Keyboard);
			_mouse.SetActive(inputType == InputType.Keyboard);
		}
	}
}
