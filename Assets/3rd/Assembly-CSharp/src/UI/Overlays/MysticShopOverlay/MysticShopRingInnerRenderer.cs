using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace src.UI.Overlays.MysticShopOverlay
{
	public class MysticShopRingInnerRenderer : MaskableGraphic
	{
		[SerializeField]
		private Texture _texture;

		[SerializeField]
		[Range(0f, 10f)]
		private int _segments = 4;

		[SerializeField]
		private float _radius = 100f;

		[SerializeField]
		[Range(0f, 500f)]
		private float _thickness;

		[SerializeField]
		[Range(0f, 250f)]
		private int _resolution;

		[SerializeField]
		[Range(0f, 3f)]
		private float _uvFix;

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

		public int Segments
		{
			get
			{
				return _segments;
			}
			set
			{
				_segments = value;
				OnRectTransformDimensionsChange();
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
			if (_resolution == 0)
			{
				return;
			}
			vh.Clear();
			_verts.Clear();
			int num = 0;
			int width = mainTexture.width;
			float num2 = 0f;
			float num3 = 360f / (float)_segments;
			float num4 = num3 / (float)_resolution;
			UIVertex uIVertex = default(UIVertex);
			uIVertex.color = color;
			uIVertex.normal = Vector2.zero;
			UIVertex item = uIVertex;
			for (int i = 0; i < _segments; i++)
			{
				num += _verts.Count;
				_verts.Clear();
				num2 = 0f;
				for (int j = 0; j <= _resolution; j++)
				{
					float f = (90f + num3 * (float)i + num4 * (float)j) * ((float)Math.PI / 180f);
					Vector2 vector = new Vector2(Mathf.Cos(f), Mathf.Sin(f));
					Vector2 vector2 = vector * (_radius - _thickness * 0.5f);
					item.position = vector2;
					if (j > 0)
					{
						num2 += Vector2.Distance(item.position, _verts[_verts.Count - 2].position);
					}
					item.uv0.y = 0f;
					item.uv0.x = num2 / (float)width * _uvFix;
					_verts.Add(item);
					Vector2 vector3 = vector * (_radius + _thickness * 0.5f);
					item.position = vector3;
					item.uv0.y = 1f;
					item.uv0.x = num2 / (float)width * _uvFix;
					_verts.Add(item);
				}
				foreach (UIVertex vert in _verts)
				{
					vh.AddVert(vert);
				}
				for (int k = num; k <= vh.currentVertCount - 4; k += 2)
				{
					vh.AddTriangle(k, k + 3, k + 1);
					vh.AddTriangle(k, k + 2, k + 3);
				}
			}
		}
	}
}
