using src.Extensions;
using UnityEngine;

namespace Lamb.UI
{
	public class NintendoControllers : InputDisplay
	{
		[SerializeField]
		private RectTransform _joyConsDetachedContainer;

		[SerializeField]
		private RectTransform _joyConsDockedContainer;

		[SerializeField]
		private RectTransform _handheldContainer;

		[SerializeField]
		private RectTransform _proControllerContainer;

		private GameObject _joyConsDetached;

		private GameObject _joyConsDocked;

		private GameObject _handheld;

		private GameObject _proController;

		public override void Configure(InputType inputType)
		{
			if (_joyConsDocked == null)
			{
				_joyConsDocked = GameObjectExtensions.Instantiate(MonoSingleton<UIManager>.Instance.SwitchJoyConsDockedTemplate, _joyConsDockedContainer).gameObject;
				_joyConsDocked.transform.localPosition = Vector3.zero;
				_joyConsDocked.transform.localScale = Vector3.one;
			}
			if (_joyConsDetached == null)
			{
				_joyConsDetached = GameObjectExtensions.Instantiate(MonoSingleton<UIManager>.Instance.SwitchJoyConsTemplate, _joyConsDetachedContainer).gameObject;
				_joyConsDetached.transform.localPosition = Vector3.zero;
				_joyConsDetached.transform.localScale = Vector3.one;
			}
			if (_handheld == null)
			{
				_handheld = GameObjectExtensions.Instantiate(MonoSingleton<UIManager>.Instance.SwitchHandheldTemplate, _handheldContainer).gameObject;
				_handheld.transform.localPosition = Vector3.zero;
				_handheld.transform.localScale = Vector3.one;
			}
			if (_proController == null)
			{
				_proController = GameObjectExtensions.Instantiate(MonoSingleton<UIManager>.Instance.SwitchProControllerTemplate, _proControllerContainer).gameObject;
				_proController.transform.localPosition = Vector3.zero;
				_proController.transform.localScale = Vector3.one;
			}
			_joyConsDetached.SetActive(inputType == InputType.SwitchJoyConsDetached);
			_joyConsDocked.SetActive(inputType == InputType.SwitchJoyConsDocked);
			_handheld.SetActive(inputType == InputType.SwitchHandheld);
			_proController.SetActive(inputType == InputType.SwitchProController);
		}
	}
}
