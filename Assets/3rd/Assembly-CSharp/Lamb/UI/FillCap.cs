using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	[ExecuteInEditMode]
	public class FillCap : BaseMonoBehaviour
	{
		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private Image _fillImage;

		[SerializeField]
		private RectTransform _fillRectTranform;

		private Vector2 _position;

		private void Update()
		{
			if (!(_rectTransform == null) && !(_fillImage == null) && !(_fillRectTranform == null))
			{
				_position.y = _fillRectTranform.rect.height * _fillImage.fillAmount;
				Debug.Log(_position.y);
				_rectTransform.anchoredPosition = _position;
			}
		}
	}
}
