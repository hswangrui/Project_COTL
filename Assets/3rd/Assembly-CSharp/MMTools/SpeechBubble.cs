using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MMTools
{
	public class SpeechBubble : MonoBehaviour
	{
		private const float kScreenMarginVertical = 90f;

		private const float kScreenMarginHorizontal = 45f;

		private TextMeshProUGUI TextComponent;

		private RectTransform BubbleRT;

		private RectTransform RT;

		private float width;

		private float height;

		private Vector2 Scale;

		private Vector3 ScaleSpeed;

		[Range(0.1f, 1f)]
		public float Spring = 0.3f;

		[Range(0.1f, 1f)]
		public float Dampening = 0.65f;

		public float MaxSpeed = 50f;

		public Canvas canvas;

		private RectTransform CanvasRect;

		public Image Bubble;

		public Vector2 Margin = new Vector2(200f, 100f);

		private Vector3 _offset = Vector3.zero;

		public float ScreenOffset = 400f;

		private Vector3 TargetPosition;

		private Vector3 _rendererBoundsOffset;

		private Transform _targetTransform;

		private MeshRenderer _targetMeshRenderer;

		public bool ForceOffset { get; set; }

		public void OnEnable()
		{
		}

		public void Reset(TextMeshProUGUI TextComponent)
		{
			this.TextComponent = TextComponent;
			this.TextComponent.text = "";
			Scale = (ScaleSpeed = Vector2.zero);
			RT = GetComponent<RectTransform>();
			BubbleRT = Bubble.GetComponent<RectTransform>();
			BubbleRT.sizeDelta = Scale;
			RT.sizeDelta = Scale;
			CanvasRect = canvas.GetComponent<RectTransform>();
			base.transform.localScale = Vector3.one;
		}

		private void Update()
		{
			width = TextComponent.preferredWidth + Margin.x;
			if (width > TextComponent.rectTransform.sizeDelta.x)
			{
				width = TextComponent.rectTransform.sizeDelta.x + Margin.x;
			}
			height = TextComponent.preferredHeight + Margin.y;
			ScaleSpeed.x = Mathf.Min(ScaleSpeed.x, MaxSpeed);
			ScaleSpeed.y = Mathf.Min(ScaleSpeed.y, MaxSpeed);
			ScaleSpeed.x = Mathf.Max(ScaleSpeed.x, 0f - MaxSpeed);
			ScaleSpeed.y = Mathf.Max(ScaleSpeed.y, 0f - MaxSpeed);
			BubbleRT.sizeDelta = Scale;
			RT.sizeDelta = Scale;
		}

		private void LateUpdate()
		{
			if (!(_targetTransform == null))
			{
				Vector3 position = _targetTransform.position;
				if (_targetMeshRenderer != null)
				{
					position += _rendererBoundsOffset;
					Debug.DrawLine(_targetTransform.position, position, Color.green);
				}
				position += _offset;
				position = Camera.main.WorldToViewportPoint(position);
				position *= CanvasRect.rect.size;
				position.y += BubbleRT.rect.height * 0.5f;
				if (_targetMeshRenderer == null || ForceOffset)
				{
					position.y += ScreenOffset;
				}
				RT.anchoredPosition = KeepFullyOnScreen(position);
			}
		}

		private void FixedUpdate()
		{
			ScaleSpeed.x += (width - Scale.x) * Spring;
			Scale.x += (ScaleSpeed.x *= Dampening);
			ScaleSpeed.y += (height - Scale.y) * Spring;
			Scale.y += (ScaleSpeed.y *= Dampening);
		}

		private Vector3 KeepFullyOnScreen(Vector3 newPos)
		{
			Rect rect = CanvasRect.rect;
			Rect rect2 = BubbleRT.rect;
			newPos.x = Mathf.Clamp(newPos.x, rect2.width * 0.5f + 45f, rect.width - rect2.width * 0.5f - 45f);
			newPos.y = Mathf.Clamp(newPos.y, rect2.height * 0.5f + 90f, rect.height - rect2.height * 0.5f - 90f);
			return newPos;
		}

		public void SetPosition(Vector3 Position)
		{
			TargetPosition = Camera.main.WorldToViewportPoint(Position);
			TargetPosition *= CanvasRect.rect.size;
		}

		public void SetTarget(Transform newTarget, Vector3 offset)
		{
			_offset = offset;
			if (_targetTransform != newTarget)
			{
				_targetTransform = newTarget;
				SkeletonAnimation component;
				if (!newTarget.TryGetComponent<SkeletonAnimation>(out component))
				{
					component = newTarget.GetComponentInChildren<SkeletonAnimation>();
				}
				if (component != null)
				{
					MeshRenderer component2;
					if (component.TryGetComponent<MeshRenderer>(out component2))
					{
						_targetMeshRenderer = component2;
					}
					else
					{
						_targetMeshRenderer = null;
					}
				}
			}
			if (_targetMeshRenderer != null)
			{
				Bounds bounds = _targetMeshRenderer.bounds;
				Vector3 a = new Vector3(0f, bounds.min.y, bounds.min.z);
				Vector3 b = new Vector3(0f, bounds.max.y, bounds.max.z);
				float num = Vector3.Distance(a, b);
				_rendererBoundsOffset = Quaternion.Euler(_targetMeshRenderer.transform.eulerAngles) * new Vector3(0f, num + 0.5f, 0f);
			}
			else
			{
				_rendererBoundsOffset = Vector3.zero;
			}
		}

		public void ClearTarget()
		{
			_targetTransform = null;
			_targetMeshRenderer = null;
		}

		public float GetAngle(Vector3 fromPosition, Vector3 toPosition)
		{
			return Mathf.Atan2(toPosition.y - fromPosition.y, toPosition.x - fromPosition.x) * 57.29578f;
		}

		public void HidePrompt()
		{
		}
	}
}
