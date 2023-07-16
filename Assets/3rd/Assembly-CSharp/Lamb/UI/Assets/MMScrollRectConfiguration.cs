using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.Assets
{
	[CreateAssetMenu(fileName = "Scroll Rect Configuration", menuName = "Massive Monster/Scroll Rect Configuration", order = 1)]
	public class MMScrollRectConfiguration : ScriptableObject
	{
		[Header("Defaults")]
		[SerializeField]
		private ScrollRect.MovementType _movementType = ScrollRect.MovementType.Elastic;

		[SerializeField]
		private float _elasticity = 0.1f;

		[SerializeField]
		private bool _inertia = true;

		[SerializeField]
		private float _decelerationRate = 0.135f;

		[SerializeField]
		private float _scrollSensitivity = 1f;

		[SerializeField]
		private ScrollRect.ScrollbarVisibility _horizontalScrollbarVisbility;

		[SerializeField]
		private ScrollRect.ScrollbarVisibility _verticalScrollbarVisibility;

		[Header("Custom")]
		[SerializeField]
		private AnimationCurve _scrollToEase;

		[SerializeField]
		private float _moveToTravelTime = 1f;

		public AnimationCurve ScrollToEase
		{
			get
			{
				return _scrollToEase;
			}
		}

		public float MoveToTravelTime
		{
			get
			{
				return _moveToTravelTime;
			}
		}

		public void ApplySettings(MMScrollRect mmScrollRect)
		{
			mmScrollRect.movementType = _movementType;
			mmScrollRect.elasticity = _elasticity;
			mmScrollRect.inertia = _inertia;
			mmScrollRect.decelerationRate = _decelerationRate;
			mmScrollRect.scrollSensitivity = _scrollSensitivity;
			mmScrollRect.horizontalScrollbarVisibility = _horizontalScrollbarVisbility;
			mmScrollRect.verticalScrollbarVisibility = _verticalScrollbarVisibility;
		}
	}
}
