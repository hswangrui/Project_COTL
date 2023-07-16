using Lamb.UI;
using UnityEngine;

[ExecuteInEditMode]
public class WorldMapParallax : BaseMonoBehaviour
{
	[SerializeField]
	private RectTransform _rectTransform;

	[SerializeField]
	private RectTransform _mapContainer;

	[SerializeField]
	private ParallaxLayer[] _layers;

	[Header("Settings")]
	[SerializeField]
	[Range(0f, 1000f)]
	private float _horizon;

	[SerializeField]
	[Range(0f, 2f)]
	private float _globalIntensity = 1f;

	[SerializeField]
	[Range(0f, 2f)]
	private float _horizontalIntensity = 1f;

	[SerializeField]
	[Range(0f, 2f)]
	private float _verticalIntensity = 1f;

	[SerializeField]
	[Range(0f, 2f)]
	private float _scaleStrength = 1f;

	[Header("Extents")]
	[SerializeField]
	private float _left;

	[SerializeField]
	private float _right;

	[SerializeField]
	private float _top;

	[SerializeField]
	private float _bottom;

	public RectTransform RectTransform
	{
		get
		{
			return _rectTransform;
		}
	}

	public RectTransform MapContainer
	{
		get
		{
			return _mapContainer;
		}
	}

	public float GlobalIntensity
	{
		get
		{
			return _globalIntensity;
		}
	}

	public float HorizontalIntensity
	{
		get
		{
			return _horizontalIntensity;
		}
	}

	public float VerticalIntensity
	{
		get
		{
			return _verticalIntensity;
		}
	}

	public void Update()
	{
		Vector2 anchoredPosition = ClampPosition(_rectTransform.anchoredPosition);
		_rectTransform.anchoredPosition = anchoredPosition;
		ParallaxLayer[] layers = _layers;
		foreach (ParallaxLayer parallaxLayer in layers)
		{
			float depthNormalized = GetDepthNormalized(parallaxLayer);
			Vector2 vector = default(Vector2);
			vector.x = anchoredPosition.x * depthNormalized * (_horizontalIntensity * _globalIntensity);
			vector.y = anchoredPosition.y * depthNormalized * (_verticalIntensity * _globalIntensity);
			Vector2 anchoredPosition2 = vector;
			parallaxLayer.RectTransform.anchoredPosition = anchoredPosition2;
			parallaxLayer.RectTransform.localScale = Vector3.one + _rectTransform.localScale * (1f - depthNormalized) * _scaleStrength;
		}
	}

	public Vector2 ClampPosition(Vector2 target)
	{
		Vector2 vector = _mapContainer.localScale;
		float num = 1f / (_horizontalIntensity * _globalIntensity);
		float num2 = 1f / (_verticalIntensity * _globalIntensity);
		target.x = Mathf.Clamp(target.x, _left * vector.x * num, _right * vector.x * num);
		target.y = Mathf.Clamp(target.y, _bottom * vector.y * num2, _top * vector.y * num2);
		return target;
	}

	public float GetDepthNormalized(ParallaxLayer layer)
	{
		return 1f - layer.Distance / _horizon;
	}
}
