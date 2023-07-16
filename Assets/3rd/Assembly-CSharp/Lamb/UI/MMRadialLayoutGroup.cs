using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	[ExecuteInEditMode]
	public class MMRadialLayoutGroup : MonoBehaviour, ILayoutGroup, ILayoutController
	{
		[Serializable]
		private enum StartingPosition
		{
			North,
			East,
			South,
			West
		}

		[SerializeField]
		private float _radius;

		[SerializeField]
		private StartingPosition _startingPosition;

		[SerializeField]
		[Range(0f, 360f)]
		private float _offset;

		[SerializeField]
		private bool _rotate;

		public float Radius
		{
			get
			{
				return _radius;
			}
			set
			{
				_radius = value;
				UpdateLayout();
			}
		}

		public float Offset
		{
			get
			{
				return _offset;
			}
			set
			{
				_offset = value;
				UpdateLayout();
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
			float num = AngleForStartingPosiiton(_startingPosition);
			num += (float)Math.PI / 180f * _offset;
			for (int j = 0; j < list.Count; j++)
			{
				float num2 = (float)j / (float)list.Count;
				float num3 = (float)Math.PI * 2f * num2;
				num3 += num;
				Vector2 vector = new Vector2(Mathf.Cos(0f - num3), Mathf.Sin(0f - num3)) * _radius;
				list[j].localPosition = vector;
				if (_rotate)
				{
					list[j].localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(vector.y, vector.x) * (180f / (float)Math.PI));
				}
			}
		}

		private float AngleForStartingPosiiton(StartingPosition startingPosition)
		{
			switch (startingPosition)
			{
			case StartingPosition.North:
				return -(float)Math.PI / 2f;
			case StartingPosition.East:
				return 0f;
			case StartingPosition.South:
				return (float)Math.PI / 2f;
			case StartingPosition.West:
				return (float)Math.PI;
			default:
				return 0f;
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
