using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class MMDynamicVerticalLayoutGroup : MonoBehaviour, ILayoutGroup, ILayoutController
	{
		[SerializeField]
		private float _padding;

		private List<RectTransform> _cachedTransforms = new List<RectTransform>();

		private void OnEnable()
		{
			FlagForUpdate();
		}

		public void SetLayoutHorizontal()
		{
		}

		public void SetLayoutVertical()
		{
			FlagForUpdate();
		}

		private void OnTransformChildrenChanged()
		{
			FlagForUpdate();
		}

		private void FlagForUpdate()
		{
			if (base.gameObject.activeInHierarchy)
			{
				UpdateLayout();
			}
		}

		private void Update()
		{
			Vector2 zero = Vector2.zero;
			for (int i = 0; i < _cachedTransforms.Count; i++)
			{
				zero.x = _cachedTransforms[i].anchoredPosition.x;
				if (i > 0)
				{
					zero.y = _cachedTransforms[i - 1].anchoredPosition.y;
					zero.y -= _cachedTransforms[i - 1].sizeDelta.y;
					zero.y -= _padding;
				}
				_cachedTransforms[i].anchoredPosition -= (_cachedTransforms[i].anchoredPosition - zero) / 2f;
			}
		}

		private void UpdateLayout()
		{
			List<RectTransform> list = new List<RectTransform>();
			foreach (RectTransform item in base.transform)
			{
				LayoutElement component;
				if (item.gameObject.activeSelf && (!item.TryGetComponent<LayoutElement>(out component) || !component.ignoreLayout))
				{
					list.Add(item);
				}
			}
			Vector2 zero = Vector2.zero;
			for (int i = 0; i < list.Count; i++)
			{
				zero.x = list[i].anchoredPosition.x;
				if (i > 0)
				{
					zero.y = list[i - 1].anchoredPosition.y;
					zero.y -= list[i - 1].sizeDelta.y;
					zero.y -= _padding;
				}
				if (!_cachedTransforms.Contains(list[i]))
				{
					_cachedTransforms.Add(list[i]);
					list[i].anchoredPosition = zero;
				}
			}
			_cachedTransforms = list;
		}
	}
}
