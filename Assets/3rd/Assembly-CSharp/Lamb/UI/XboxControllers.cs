using src.Extensions;
using UnityEngine;

namespace Lamb.UI
{
	public class XboxControllers : InputDisplay
	{
		[SerializeField]
		private RectTransform _xboxSeriesContainer;

		private GameObject _xboxSeries;

		public override void Configure(InputType inputType)
		{
			if (_xboxSeries == null)
			{
				_xboxSeries = GameObjectExtensions.Instantiate(MonoSingleton<UIManager>.Instance.XboxControllerTemplate, _xboxSeriesContainer).gameObject;
				_xboxSeries.transform.localPosition = Vector3.zero;
				_xboxSeries.transform.localScale = Vector3.one;
			}
			_xboxSeries.SetActive(inputType == InputType.XboxSeries);
		}
	}
}
