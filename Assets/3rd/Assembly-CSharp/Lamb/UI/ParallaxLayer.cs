using UnityEngine;

namespace Lamb.UI
{
	[RequireComponent(typeof(RectTransform))]
	public class ParallaxLayer : MonoBehaviour
	{
		[SerializeField]
		private float _distance;

		[SerializeField]
		[HideInInspector]
		private RectTransform _rectTransform;

		public RectTransform RectTransform
		{
			get
			{
				return _rectTransform;
			}
		}

		public float Distance
		{
			get
			{
				return _distance;
			}
		}

		private void Reset()
		{
			_rectTransform = GetComponent<RectTransform>();
		}
	}
}
