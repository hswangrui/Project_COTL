using Lamb.UI;
using UnityEngine;

namespace src.UI.Overlays.MysticShopOverlay
{
	[ExecuteInEditMode]
	public class RadiusController : MonoBehaviour
	{
		[Header("Master Radius")]
		[SerializeField]
		[Range(0f, 2f)]
		private float _expansion;

		[SerializeField]
		[Range(0f, 1000f)]
		private float _radius;

		[Header("Radial Graphic")]
		[SerializeField]
		[Range(0f, 1000f)]
		private float _radialGraphicOffset;

		[SerializeField]
		private MMUIRadialGraphic _radialGraphic;

		[Header("Radial Graphic Mask")]
		[SerializeField]
		[Range(0f, 1000f)]
		private float _radialGraphicMaskOffset;

		[SerializeField]
		private MMUIRadialGraphic _radialGraphicMask;

		[Header("Inner Ring")]
		[SerializeField]
		[Range(0f, 1000f)]
		private float _innerRingOffset;

		[SerializeField]
		private MysticShopRingInnerRenderer _innerRenderer;

		[Header("Radial Layout")]
		[SerializeField]
		[Range(0f, 1000f)]
		private float _radialGroupOffset;

		[SerializeField]
		private MMRadialLayoutGroup _radialLayoutGroup;

		[Header("Flourishes")]
		[SerializeField]
		[Range(0f, 2f)]
		private float _flourishExpansion;

		[SerializeField]
		[Range(0f, 1f)]
		private float _flourishFill;

		[SerializeField]
		private AnimationCurve _flourishFillCurve;

		[SerializeField]
		[Range(0f, 1000f)]
		private float _flourishOffset;

		[SerializeField]
		private AnimationCurve _flourishRadiusCurve;

		[SerializeField]
		private MysticShopFlourishRenderer[] _flourishes;

		[Header("Arms")]
		[SerializeField]
		private RectTransform _selector;

		[SerializeField]
		private RectTransform _upperArm;

		[SerializeField]
		private RectTransform _lowerArm;

		[Header("Planet Sticks")]
		[SerializeField]
		private RectTransform _stickA;

		[SerializeField]
		private RectTransform _stickB;

		public float Expansion
		{
			get
			{
				return _expansion;
			}
			set
			{
				_expansion = value;
				Update();
			}
		}

		private void Update()
		{
			float expansion = _expansion;
			if ((bool)_radialGraphic)
			{
				_radialGraphic.Radius = _radius * expansion + _radialGraphicOffset;
			}
			if ((bool)_radialGraphicMask)
			{
				_radialGraphicMask.Radius = _radius * expansion + _radialGraphicMaskOffset;
			}
			if ((bool)_innerRenderer)
			{
				_innerRenderer.Radius = _radius * expansion + _innerRingOffset;
			}
			if ((bool)_radialLayoutGroup)
			{
				_radialLayoutGroup.Radius = _radius * expansion + _radialGroupOffset;
			}
			if (_flourishes != null)
			{
				MysticShopFlourishRenderer[] flourishes = _flourishes;
				foreach (MysticShopFlourishRenderer obj in flourishes)
				{
					obj.Radius = _radius * _flourishRadiusCurve.Evaluate(_flourishExpansion) + _flourishOffset;
					obj.FillScaler = _flourishFillCurve.Evaluate(_flourishFill);
				}
			}
			if ((bool)_upperArm && (bool)_lowerArm && (bool)_selector)
			{
				_selector.anchoredPosition = new Vector2(0f, (0f - _radius) * (1f - expansion));
				_upperArm.anchoredPosition = new Vector2(0f, (0f - _radius) * (1f - expansion));
				_lowerArm.anchoredPosition = new Vector2(0f, _radius * (1f - expansion));
			}
			if ((bool)_stickA && (bool)_stickB)
			{
				_stickA.anchoredPosition = new Vector2(0f, (0f - _stickA.rect.height) * (1f - expansion));
				_stickB.anchoredPosition = new Vector2(0f, (0f - _stickB.rect.height) * (1f - expansion));
			}
		}
	}
}
