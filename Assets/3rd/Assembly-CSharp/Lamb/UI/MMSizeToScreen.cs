using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	[ExecuteAlways]
	[RequireComponent(typeof(RectTransform))]
	public class MMSizeToScreen : MonoBehaviour
	{
		[SerializeField]
		private RectTransform _canvasRectTransform;

		[SerializeField]
		private LayoutElement _layoutElement;

		private void Awake()
		{
			_layoutElement = GetComponent<LayoutElement>();
		}

		private void Update()
		{
			if (!(_canvasRectTransform == null))
			{
				LayoutElement layoutElement = _layoutElement;
				float preferredHeight = (_layoutElement.minHeight = _canvasRectTransform.rect.height);
				layoutElement.preferredHeight = preferredHeight;
				LayoutElement layoutElement2 = _layoutElement;
				preferredHeight = (_layoutElement.minWidth = _canvasRectTransform.rect.width);
				layoutElement2.preferredWidth = preferredHeight;
			}
		}
	}
}
