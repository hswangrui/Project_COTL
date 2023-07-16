using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Lamb.UI;
using Lamb.UI.Assets;
using UnityEngine;
using UnityEngine.UI;

public class UIRandomWheel : UIMenuBase
{
	public class Segment
	{
		public InventoryItem.ITEM_TYPE reward;

		public float probability;

		public Segment()
		{
		}

		public Segment(InventoryItem.ITEM_TYPE reward, float probability)
		{
			this.reward = reward;
			this.probability = probability;
		}
	}

	[Serializable]
	public struct WheelSegment
	{
		public Image ImageFill;

		[HideInInspector]
		public CanvasGroup ImageCanvasGroup;

		public RectTransform RectTransform;

		public Image Icon;

		[HideInInspector]
		public CanvasGroup IconCanvasGroup;
	}

	private const string kXPositionProperty = "_XPosition";

	private const string kYPositionProperty = "_YPosition";

	[SerializeField]
	private RectTransform _container;

	[SerializeField]
	private Transform _arrowPivot;

	[SerializeField]
	private InventoryIconMapping _inventoryIconMapping;

	[SerializeField]
	private WheelSegment[] _wheelSegments;

	[SerializeField]
	private AnimationCurve _curve;

	[SerializeField]
	private MMRadialLayoutGroup _radialLayoutGroup;

	[SerializeField]
	private MMUIRadialGraphic _radialGraphic;

	[SerializeField]
	private ParticleSystem _confettiLeft;

	[SerializeField]
	private ParticleSystem _confettiRight;

	private Segment[] _segments;

	private Vector2[] _segmentMinMaxes;

	private Material _radialMaterial;

	private List<InventoryItem.ITEM_TYPE> excludedItems;

	public Segment ChosenSegment { get; private set; }

	public WheelSegment[] WheelSegments
	{
		get
		{
			return _wheelSegments;
		}
	}

	public float SpeedMultiplier { get; set; } = 1f;


	public override void Awake()
	{
		base.Awake();
		for (int i = 0; i < _wheelSegments.Length; i++)
		{
			_wheelSegments[i].ImageCanvasGroup = _wheelSegments[i].ImageFill.GetComponent<CanvasGroup>();
			_wheelSegments[i].RectTransform.gameObject.SetActive(false);
			_wheelSegments[i].Icon.gameObject.SetActive(false);
			_wheelSegments[i].IconCanvasGroup = _wheelSegments[i].Icon.GetComponent<CanvasGroup>();
		}
		_radialMaterial = new Material(_radialGraphic.material);
		_radialGraphic.material = _radialMaterial;
		if ((bool)_confettiLeft)
		{
			_confettiLeft.Stop();
			_confettiLeft.Clear();
		}
		if ((bool)_confettiRight)
		{
			_confettiRight.Stop();
			_confettiRight.Clear();
		}
	}

	public void Show(Segment[] segments, bool instant = false, List<InventoryItem.ITEM_TYPE> excludedItems = null)
	{
		this.excludedItems = excludedItems;
		base.gameObject.SetActive(true);
		_segments = segments;
		_segmentMinMaxes = new Vector2[_segments.Length];
		float num = 0f;
		for (int i = 0; i < segments.Length; i++)
		{
			_wheelSegments[i].ImageFill.fillAmount = segments[i].probability;
			_wheelSegments[i].RectTransform.gameObject.SetActive(true);
			_wheelSegments[i].Icon.gameObject.SetActive(true);
			_wheelSegments[i].RectTransform.rotation = Quaternion.Euler(new Vector3(0f, 0f, num * -360f));
			num += segments[i].probability;
			_segmentMinMaxes[i] = new Vector2((num - segments[i].probability) * 360f, num * 360f);
			if (i == 0)
			{
				_radialLayoutGroup.Offset = num * 0.5f * 360f;
			}
			_wheelSegments[i].Icon.sprite = _inventoryIconMapping.GetImage(segments[i].reward);
			if (segments[i].reward == InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION)
			{
				_wheelSegments[i].ImageFill.color = StaticColors.TwitchPurple;
			}
			else
			{
				_wheelSegments[i].ImageFill.color = StaticColors.GreyColor;
			}
		}
		base.transform.localScale = Vector3.zero;
		Show(instant);
	}

	protected override IEnumerator DoShow()
	{
		yield return _003C_003En__0();
		yield return new WaitForSecondsRealtime(0.5f);
	}

	protected override void OnShowCompleted()
	{
		base.OnShowCompleted();
		SpinWheel();
	}

	protected override IEnumerator DoHide()
	{
		int rand = UnityEngine.Random.Range(0, 2);
		if (rand == 0 && (bool)_confettiLeft)
		{
			ParticleSystem confettiLeft = _confettiLeft;
			if ((object)confettiLeft != null)
			{
				confettiLeft.Play();
			}
		}
		else if ((bool)_confettiRight)
		{
			ParticleSystem confettiRight = _confettiRight;
			if ((object)confettiRight != null)
			{
				confettiRight.Play();
			}
		}
		yield return new WaitForSecondsRealtime(0.1f / SpeedMultiplier);
		if (rand == 0 && (bool)_confettiRight)
		{
			ParticleSystem confettiRight2 = _confettiRight;
			if ((object)confettiRight2 != null)
			{
				confettiRight2.Play();
			}
		}
		else if ((bool)_confettiLeft)
		{
			ParticleSystem confettiLeft2 = _confettiLeft;
			if ((object)confettiLeft2 != null)
			{
				confettiLeft2.Play();
			}
		}
		yield return new WaitForSecondsRealtime(1.5f / SpeedMultiplier);
		yield return _003C_003En__1();
	}

	protected override void OnHideCompleted()
	{
		base.OnHideCompleted();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		Vector2 vector = Utils.DegreeToVector2(_arrowPivot.rotation.eulerAngles.z + 270f);
		vector.Normalize();
		_radialMaterial.SetFloat("_XPosition", vector.x);
		_radialMaterial.SetFloat("_YPosition", vector.y);
		float num = 360f - _arrowPivot.rotation.normalized.eulerAngles.z;
		UnityEngine.Debug.Log(num);
		for (int i = 0; i < _segmentMinMaxes.Length; i++)
		{
			if (num > _segmentMinMaxes[i].x && num < _segmentMinMaxes[i].y)
			{
				_wheelSegments[i].IconCanvasGroup.alpha = 1f;
				_wheelSegments[i].ImageCanvasGroup.alpha = 1f;
			}
			else
			{
				_wheelSegments[i].IconCanvasGroup.alpha = 0.6f;
				_wheelSegments[i].ImageCanvasGroup.alpha = 0.6f;
			}
		}
	}

	public void SpinWheel()
	{
		ChosenSegment = null;
		while (ChosenSegment == null)
		{
			Segment segment = _segments[UnityEngine.Random.Range(0, _segments.Length)];
			if (UnityEngine.Random.value < segment.probability && (excludedItems == null || !excludedItems.Contains(segment.reward)))
			{
				ChosenSegment = segment;
			}
		}
		Vector2 minMaxAngleFromSegment = GetMinMaxAngleFromSegment(ChosenSegment);
		float num = UnityEngine.Random.Range(minMaxAngleFromSegment.x, minMaxAngleFromSegment.y);
		int num2 = UnityEngine.Random.Range(6, 9);
		float num3 = UnityEngine.Random.Range(4.5f, 5.5f);
		ShortcutExtensions.DORotate(endValue: new Vector3(0f, 0f, num * -360f - (float)(360 * num2)), target: _arrowPivot.transform, duration: num3 / SpeedMultiplier, mode: RotateMode.LocalAxisAdd).SetEase(_curve).SetUpdate(true)
			.OnComplete(delegate
			{
				Hide();
			});
	}

	private Vector2 GetMinMaxAngleFromSegment(Segment segment)
	{
		int num = _segments.IndexOf(segment);
		float num2 = 0f;
		float y = 0f;
		for (int i = 0; i < _segments.Length; i++)
		{
			if (i < num)
			{
				num2 += _segments[i].probability;
				continue;
			}
			y = num2 + _segments[i].probability;
			break;
		}
		return new Vector2(num2, y);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_radialMaterial != null)
		{
			UnityEngine.Object.Destroy(_radialMaterial);
			_radialMaterial = null;
		}
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private IEnumerator _003C_003En__0()
	{
		return base.DoShow();
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private IEnumerator _003C_003En__1()
	{
		return base.DoHide();
	}
}
