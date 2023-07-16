using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	[RequireComponent(typeof(CanvasRenderer))]
	public class MMUIRadialGraphic : MaskableGraphic
	{
		[SerializeField]
		private Texture _texture;

		[SerializeField]
		private float _outerRadius;

		[SerializeField]
		private float _radius;

		[SerializeField]
		private int _resolution;

		[SerializeField]
		private bool _drawInnerRadius = true;

		[SerializeField]
		private bool _drawOuterRadius = true;

		[SerializeField]
		[Range(0f, 360f)]
		private float _radialFill;

		private List<UIVertex> _verts = new List<UIVertex>();

		public override Texture mainTexture
		{
			get
			{
				if (!(_texture == null))
				{
					return _texture;
				}
				return Graphic.s_WhiteTexture;
			}
		}

		public float Radius
		{
			get
			{
				return _radius;
			}
			set
			{
				_radius = value;
				OnRectTransformDimensionsChange();
			}
		}

		public float OuterRadius
		{
			get
			{
				return _outerRadius;
			}
		}

		public float FullRadius
		{
			get
			{
				return _radius + _outerRadius;
			}
		}

		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
			SetVerticesDirty();
			SetMaterialDirty();
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();
			_verts.Clear();
			if (_resolution == 0)
			{
				return;
			}
			UIVertex uIVertex = default(UIVertex);
			uIVertex.color = color;
			UIVertex item = uIVertex;
			item.position = Vector2.zero;
			item.normal = Vector2.zero;
			item.uv0 = Vector2.zero;
			_verts.Add(item);
			for (int i = 0; i <= _resolution; i++)
			{
				float num = (float)i / (float)_resolution;
				float f = (float)Math.PI * 2f * num;
				Vector2 vector = new Vector2(Mathf.Cos(f), Mathf.Sin(f));
				Vector2 vector2 = vector * _radius;
				Vector2 vector3 = vector * (_radius + _outerRadius);
				item.position = vector2;
				item.normal = Vector2.zero;
				item.uv0 = new Vector2(num, _radius / FullRadius);
				_verts.Add(item);
				item.position = vector3;
				item.normal = vector;
				item.uv0 = new Vector2(num, 1f);
				_verts.Add(item);
			}
			foreach (UIVertex vert in _verts)
			{
				vh.AddVert(vert);
			}
			for (int j = 0; j < vh.currentVertCount - 4; j += 2)
			{
				if (_drawInnerRadius)
				{
					vh.AddTriangle(0, j + 3, j + 1);
				}
				if (_drawOuterRadius)
				{
					vh.AddTriangle(j + 1, j + 4, j + 2);
					vh.AddTriangle(j + 1, j + 3, j + 4);
				}
			}
		}
	}
}
