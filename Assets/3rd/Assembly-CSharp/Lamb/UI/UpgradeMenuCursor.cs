using System;
using System.Collections.Generic;
using DG.Tweening;
using src.UINavigator;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class UpgradeMenuCursor : MonoBehaviour
	{
		public Action OnAtRest;

		public Action<Vector2> OnCursorMoved;

		[Header("Settings")]
		[SerializeField]
		private float _horizontalSensitivity = 1f;

		[SerializeField]
		private float _verticalSensisitivty = 1f;

		[SerializeField]
		private float _selectionRadius = 100f;

		[Header("Components")]
		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		[ReadOnly]
		private List<Selectable> _validSelectables;

		[SerializeField]
		private RectTransform _cursorViewport;

		[SerializeField]
		private RectTransform _viewport;

		[SerializeField]
		private RectTransform _bounds;

		[SerializeField]
		private CanvasGroup _canvasGroup;

		private bool _snapToggle;

		public bool LockPosition;

		private Tween scaleTween;

		public RectTransform RectTransform
		{
			get
			{
				return _rectTransform;
			}
		}

		public CanvasGroup CanvasGroup
		{
			get
			{
				return _canvasGroup;
			}
		}

		private void OnEnable()
		{
			UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
			instance.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Combine(instance.OnSelectionChanged, new Action<Selectable, Selectable>(OnSelectionChanged));
			UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
			instance2.OnDefaultSetComplete = (Action<Selectable>)Delegate.Combine(instance2.OnDefaultSetComplete, new Action<Selectable>(OnSelection));
		}

		private void OnDisable()
		{
			if (MonoSingleton<UINavigatorNew>.Instance != null)
			{
				UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
				instance.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Remove(instance.OnSelectionChanged, new Action<Selectable, Selectable>(OnSelectionChanged));
				UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
				instance2.OnDefaultSetComplete = (Action<Selectable>)Delegate.Remove(instance2.OnDefaultSetComplete, new Action<Selectable>(OnSelection));
			}
		}

		private void OnSelectionChanged(Selectable current, Selectable previous)
		{
			OnSelection(current);
		}

		private void OnSelection(Selectable selectable)
		{
			if (InputManager.General.MouseInputActive)
			{
				_rectTransform.anchoredPosition = selectable.GetComponent<RectTransform>().anchoredPosition;
			}
		}

		private void Update()
		{
			if (LockPosition)
			{
				return;
			}
			if (InputManager.General.MouseInputActive)
			{
				if (_canvasGroup.alpha > 0f)
				{
					_canvasGroup.alpha -= Time.unscaledDeltaTime * 4f;
				}
			}
			else if (_canvasGroup.alpha < 1f)
			{
				_canvasGroup.alpha += Time.unscaledDeltaTime * 4f;
			}
			if (InputManager.General.MouseInputActive)
			{
				return;
			}
			float horizontalAxis = InputManager.UI.GetHorizontalAxis();
			float verticalAxis = InputManager.UI.GetVerticalAxis();
			if (Mathf.Abs(horizontalAxis) > 0.2f || Mathf.Abs(verticalAxis) > 0.2f)
			{
				if (base.transform.localScale != Vector3.one && (scaleTween == null || !scaleTween.active))
				{
					scaleTween = _rectTransform.DOScale(Vector3.one, 0.25f).SetUpdate(true);
				}
				else
				{
					_rectTransform.DOKill();
				}
				Vector2 anchoredPosition = _rectTransform.anchoredPosition;
				if (Mathf.Abs(horizontalAxis) > 0.2f)
				{
					anchoredPosition.x += InputManager.UI.GetHorizontalAxis() * 1080f * _horizontalSensitivity * Time.unscaledDeltaTime;
				}
				if (Mathf.Abs(verticalAxis) > 0.2f)
				{
					anchoredPosition.y += InputManager.UI.GetVerticalAxis() * 1080f * _verticalSensisitivty * Time.unscaledDeltaTime;
				}
				_rectTransform.anchoredPosition = anchoredPosition;
				_snapToggle = true;
			}
			else if (_snapToggle)
			{
				_snapToggle = false;
				float num = float.MaxValue;
				Selectable selectable = null;
				foreach (Selectable validSelectable in _validSelectables)
				{
					float num2 = Vector3.Distance(validSelectable.GetComponent<RectTransform>().anchoredPosition, _rectTransform.anchoredPosition);
					if (num2 < num)
					{
						selectable = validSelectable;
						num = num2;
					}
				}
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(selectable as IMMSelectable);
				_rectTransform.DOKill();
				_rectTransform.DOAnchorPos(selectable.GetComponent<RectTransform>().anchoredPosition, 0.5f).SetEase(Ease.OutSine).SetUpdate(true);
				_rectTransform.DOScale(selectable.transform.localScale, 0.25f).SetUpdate(true);
				Action onAtRest = OnAtRest;
				if (onAtRest != null)
				{
					onAtRest();
				}
			}
			foreach (Selectable validSelectable2 in _validSelectables)
			{
				if (MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable != validSelectable2 as IMMSelectable && Vector2.Distance(validSelectable2.GetComponent<RectTransform>().anchoredPosition, _rectTransform.anchoredPosition) < _selectionRadius)
				{
					MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(validSelectable2 as IMMSelectable);
					break;
				}
			}
			float num3 = 350f;
			float num4 = num3 * 0.5f;
			Rect rect = _bounds.rect;
			rect.width -= num4 * 2f;
			rect.width *= 0.5f;
			rect.height -= num4 * 2f;
			rect.height *= 0.5f;
			Rect rect2 = _viewport.rect;
			rect2.width -= num3 * 2f;
			rect2.width *= 0.5f;
			rect2.height -= num3 * 2f;
			rect2.height *= 0.5f;
			Vector2 anchoredPosition2 = _rectTransform.anchoredPosition;
			anchoredPosition2 = _rectTransform.parent.TransformPoint(anchoredPosition2);
			anchoredPosition2 = _cursorViewport.InverseTransformPoint(anchoredPosition2);
			float x = 0f;
			if (anchoredPosition2.x < 0f - rect2.width)
			{
				x = Mathf.Abs(anchoredPosition2.x) - rect2.width;
			}
			else if (anchoredPosition2.x > rect2.width)
			{
				x = Mathf.Abs(anchoredPosition2.x) - rect2.width;
				x = 0f - x;
			}
			float y = 0f;
			if (anchoredPosition2.y < 0f - rect2.height)
			{
				y = Mathf.Abs(anchoredPosition2.y) - rect2.height;
			}
			else if (anchoredPosition2.y > rect2.height)
			{
				y = Mathf.Abs(anchoredPosition2.y) - rect2.height;
				y = 0f - y;
			}
			Action<Vector2> onCursorMoved = OnCursorMoved;
			if (onCursorMoved != null)
			{
				onCursorMoved(new Vector2(x, y));
			}
			anchoredPosition2 = _rectTransform.anchoredPosition;
			anchoredPosition2.x = Mathf.Clamp(anchoredPosition2.x, 0f - rect.width, rect.width);
			anchoredPosition2.y = Mathf.Clamp(anchoredPosition2.y, 0f - rect.height, rect.height);
			_rectTransform.anchoredPosition = anchoredPosition2;
		}
	}
}
