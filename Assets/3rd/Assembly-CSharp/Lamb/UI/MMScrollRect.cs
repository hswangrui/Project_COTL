using System;
using System.Collections;
using Lamb.UI.Assets;
using src.UINavigator;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class MMScrollRect : ScrollRect
	{
		[Header("MM Scroll Rect")]
		[SerializeField]
		private MMScrollRectConfiguration _scrollRectConfiguration;

		[SerializeField]
		private bool _ignoreSelection;

		[SerializeField]
		public float ScrollSpeedModifier = 1f;

		[SerializeField]
		private bool _scrollToBottom = true;

		private bool _initialSetToggle;

		private float MinY
		{
			get
			{
				return 0f;
			}
		}

		private float MaxY
		{
			get
			{
				return base.content.rect.height - base.viewport.rect.height;
			}
		}

		private void OnValidate()
		{
			if (_scrollRectConfiguration != null)
			{
				_scrollRectConfiguration.ApplySettings(this);
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			_initialSetToggle = false;
			if (!_ignoreSelection && MonoSingleton<UINavigatorNew>.Instance != null)
			{
				UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
				instance.OnDefaultSetComplete = (Action<Selectable>)Delegate.Combine(instance.OnDefaultSetComplete, new Action<Selectable>(OnDefaultSelectableSet));
				UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
				instance2.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Combine(instance2.OnSelectionChanged, new Action<Selectable, Selectable>(OnSelectableChanged));
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if (!_ignoreSelection && MonoSingleton<UINavigatorNew>.Instance != null)
			{
				UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
				instance.OnDefaultSetComplete = (Action<Selectable>)Delegate.Remove(instance.OnDefaultSetComplete, new Action<Selectable>(OnDefaultSelectableSet));
				UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
				instance2.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Remove(instance2.OnSelectionChanged, new Action<Selectable, Selectable>(OnSelectableChanged));
			}
		}

		private void OnDefaultSelectableSet(Selectable selectable)
		{
			if (!_initialSetToggle || !InputManager.General.MouseInputActive)
			{
				_initialSetToggle = true;
				StartCoroutine(DeferredDefaultCheck(selectable));
			}
		}

		private IEnumerator DeferredDefaultCheck(Selectable selectable)
		{
			yield return null;
			Focus(selectable.transform as RectTransform);
		}

		public void Focus(RectTransform rectTransform)
		{
			float canvasScale = MMCanvasScaler.CanvasScale;
			Vector3 position = rectTransform.position;
			Rect rect = rectTransform.rect;
			Vector3 position2 = base.viewport.transform.position;
			Rect rect2 = base.viewport.rect;
			Vector3 vector = position2;
			Vector3 vector2 = position2;
			vector2.y -= rect2.height * canvasScale;
			Vector3 vector3 = position;
			vector3.y += rect.height * canvasScale;
			Vector3 vector4 = position;
			vector4.y -= rect.height * canvasScale;
			if (vector3.y > vector.y || vector4.y < vector2.y)
			{
				Vector2 vector5 = default(Vector2);
				vector5.x = 0f;
				vector5.y = 0f - Mathf.Clamp(base.content.InverseTransformPoint(rectTransform.position).y + base.viewport.rect.height * 0.5f, 0f - MaxY, MinY);
				Vector2 anchoredPosition = vector5;
				base.content.anchoredPosition = anchoredPosition;
			}
		}

		private void OnSelectableChanged(Selectable newSelectable, Selectable previous)
		{
			ScrollTo(newSelectable);
		}

		private void ScrollTo(Selectable selectable)
		{
			if (InputManager.General.MouseInputActive || !selectable.transform.IsChildOf(base.transform))
			{
				return;
			}
			float canvasScale = MMCanvasScaler.CanvasScale;
			Vector3 position = selectable.transform.position;
			Rect rect = selectable.GetComponent<RectTransform>().rect;
			Vector3 position2 = base.viewport.transform.position;
			Rect rect2 = base.viewport.rect;
			Vector3 position3 = base.content.transform.position;
			Rect rect3 = base.content.rect;
			Vector3 vector = position3;
			Vector3 vector2 = position3;
			vector2.y -= rect3.height * canvasScale;
			Vector3 vector3 = position2;
			Vector3 vector4 = position2;
			vector4.y -= rect2.height * canvasScale;
			Vector3 vector5 = position;
			vector5.y += rect.height * canvasScale;
			Vector3 vector6 = position;
			vector6.y -= rect.height * canvasScale;
			if (!base.vertical || !(Mathf.Abs(MonoSingleton<UINavigatorNew>.Instance.RecentMoveVector.y) > 5f))
			{
				return;
			}
			float num = 0f;
			if (MonoSingleton<UINavigatorNew>.Instance.RecentMoveVector.y > 0f)
			{
				if (vector5.y > vector3.y)
				{
					num = vector3.y - vector5.y;
				}
				Selectable selectable2 = selectable.FindSelectableOnUp();
				if (selectable2 == null || !selectable2.transform.IsChildOf(base.content))
				{
					num = vector3.y - vector.y;
				}
			}
			else if (MonoSingleton<UINavigatorNew>.Instance.RecentMoveVector.y < 0f)
			{
				if (vector6.y < vector4.y)
				{
					num = vector4.y - vector6.y;
				}
				Selectable selectable3 = selectable.FindSelectableOnDown();
				if (_scrollToBottom && (selectable3 == null || !selectable3.transform.IsChildOf(base.content)))
				{
					num = vector4.y - vector2.y;
				}
			}
			if (canvasScale < 1f)
			{
				num *= 1f / canvasScale;
			}
			if (Mathf.Abs(num) > 0f)
			{
				StopAllCoroutines();
				StartCoroutine(DoScrollTo(ClampPosition(base.content.anchoredPosition + new Vector2(0f, num)), base.content.anchoredPosition, 0.2f));
			}
		}

		public void ScrollTo(RectTransform target)
		{
			StopAllCoroutines();
			StartCoroutine(DoScrollTo(target));
		}

		public IEnumerator ScrollToTop()
		{
			Vector2 anchoredPosition = base.content.anchoredPosition;
			Vector2 to = new Vector2(0f, MinY);
			yield return DoScrollTo(to, anchoredPosition);
		}

		public IEnumerator ScrollToBottom()
		{
			Vector2 anchoredPosition = base.content.anchoredPosition;
			Vector2 to = new Vector2(0f, MaxY);
			yield return DoScrollTo(to, anchoredPosition);
		}

		public IEnumerator DoScrollTo(RectTransform target)
		{
			Vector2 anchoredPosition = base.content.anchoredPosition;
			Vector2 vector = default(Vector2);
			vector.x = 0f;
			vector.y = 0f - Mathf.Clamp(base.content.InverseTransformPoint(target.parent.TransformPoint(target.localPosition)).y + base.viewport.rect.height * 0.5f, 0f - MaxY, MinY);
			Vector2 to = vector;
			yield return DoScrollTo(to, anchoredPosition);
		}

		public IEnumerator DoScrollTo(Vector2 to, Vector2 from)
		{
			float time = Mathf.Clamp(Mathf.Abs(to.y - from.y) / base.viewport.rect.height, 0f, 1f) * _scrollRectConfiguration.MoveToTravelTime / ScrollSpeedModifier;
			yield return DoScrollTo(to, from, time);
		}

		public IEnumerator DoScrollTo(Vector2 to, Vector2 from, float time)
		{
			float t = 0f;
			while (true)
			{
				float num;
				t = (num = t + Time.unscaledDeltaTime);
				if (!(num <= time))
				{
					break;
				}
				base.content.anchoredPosition = Vector2.Lerp(from, to, _scrollRectConfiguration.ScrollToEase.Evaluate(Mathf.Clamp(t / time, 0f, 1f)));
				yield return null;
			}
			base.content.anchoredPosition = to;
		}

		private Vector2 ClampPosition(Vector2 position)
		{
			Vector2 result = default(Vector2);
			result.x = position.x;
			result.y = Mathf.Clamp(position.y, MinY, MaxY);
			return result;
		}
	}
}
