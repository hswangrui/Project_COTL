using UnityEngine;

namespace Lamb.UI
{
	public abstract class UIHistoricalNotification : MonoBehaviour
	{
		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private MMSelectable _selectable;

		public RectTransform RectTransform
		{
			get
			{
				return _rectTransform;
			}
		}

		public MMSelectable Selectable
		{
			get
			{
				return _selectable;
			}
		}
	}
}
