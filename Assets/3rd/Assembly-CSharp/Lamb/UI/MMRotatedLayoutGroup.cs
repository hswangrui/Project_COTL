using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	[ExecuteInEditMode]
	public class MMRotatedLayoutGroup : MonoBehaviour, ILayoutGroup, ILayoutController
	{
		[SerializeField]
		[Range(0f, 360f)]
		private float _offset;

		public float Offset
		{
			get
			{
				return _offset;
			}
			set
			{
				_offset = value;
			}
		}

		private void UpdateLayout()
		{
			List<Transform> list = new List<Transform>();
			for (int i = 0; i < base.transform.childCount; i++)
			{
				LayoutElement component;
				if (base.transform.GetChild(i).gameObject.activeSelf && (!base.transform.GetChild(i).TryGetComponent<LayoutElement>(out component) || !component.ignoreLayout))
				{
					list.Add(base.transform.GetChild(i));
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				list[j].rotation = Quaternion.Euler(0f, 0f, 360f / (float)list.Count * (float)j + _offset);
			}
		}

		public void SetLayoutHorizontal()
		{
			UpdateLayout();
		}

		public void SetLayoutVertical()
		{
			UpdateLayout();
		}

		private void OnTransformChildrenChanged()
		{
			UpdateLayout();
		}
	}
}
