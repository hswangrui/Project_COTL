using UnityEngine;

namespace Lamb.UI
{
	[ExecuteInEditMode]
	public class MaskStabilizer : BaseMonoBehaviour
	{
		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private RectTransform _mask;

		[SerializeField]
		private RectTransform _parent;

		public void Update()
		{
			if (!(_rectTransform == null) && !(_mask == null) && !(_parent == null))
			{
				_rectTransform.anchoredPosition = _mask.InverseTransformPoint(_parent.TransformPoint(Vector2.zero));
			}
		}
	}
}
