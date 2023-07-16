using System;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class RadialProgress : BaseMonoBehaviour
	{
		[SerializeField]
		private Image _squareRadial;

		[SerializeField]
		private Image _roundRadial;

		private float _progress;

		public float Progress
		{
			get
			{
				return _progress;
			}
			set
			{
				_progress = value;
				_squareRadial.fillAmount = _progress;
				_roundRadial.fillAmount = _progress;
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
			InputType currentInputType = ControlUtilities.GetCurrentInputType(controller);
			_squareRadial.gameObject.SetActive(currentInputType == InputType.Keyboard);
			_roundRadial.gameObject.SetActive(currentInputType != InputType.Keyboard);
		}
	}
}
