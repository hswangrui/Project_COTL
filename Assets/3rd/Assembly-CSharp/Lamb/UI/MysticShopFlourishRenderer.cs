using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	[RequireComponent(typeof(CanvasRenderer))]
	public class MysticShopFlourishRenderer : MaskableGraphic
	{
		[SerializeField]
		private Texture _texture;

		[SerializeField]
		[Range(0f, 500f)]
		private float _radius = 100f;

		[Header("Line")]
		[SerializeField]
		[Range(0f, 100f)]
		private int _resolution;

		[SerializeField]
		[Range(0f, 0.5f)]
		private float _fill = 0.25f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _fillScaler = 1f;

		[SerializeField]
		[Range(0f, 500f)]
		private float _centerSize;

		[SerializeField]
		[Range(0f, 3f)]
		private float _uvFix = 1f;

		[Header("Cap")]
		[SerializeField]
		[Range(0f, 360f)]
		private float _capSize;

		private RectTransform _rectTransform;

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

		public float Fill
		{
			get
			{
				return _fill;
			}
			set
			{
				_fill = value;
				OnRectTransformDimensionsChange();
			}
		}

		public float FillScaler
		{
			get
			{
				return _fillScaler;
			}
			set
			{
				_fillScaler = value;
				OnRectTransformDimensionsChange();
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			_rectTransform = GetComponent<RectTransform>();
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
			int num = 0;
			int width = mainTexture.width;
			Vector2 size = _rectTransform.rect.size;
			UIVertex uIVertex = default(UIVertex);
			uIVertex.color = color;
			uIVertex.normal = Vector2.zero;
			UIVertex item = uIVertex;
			float num2 = Mathf.Sin((float)Math.PI / 2f) * _radius;
			item.position.x = (0f - size.x) * 0.5f;
			item.position.y = (0f - size.y) * 0.5f + num2;
			item.uv0.x = 0f;
			item.uv0.y = 0f;
			_verts.Add(item);
			item.position.x = (0f - size.x) * 0.5f;
			item.position.y = size.y * 0.5f + num2;
			item.uv0.x = 0f;
			item.uv0.y = 0.5f;
			_verts.Add(item);
			item.position.x = size.x * 0.5f;
			item.position.y = size.y * 0.5f + num2;
			item.uv0.x = 0.5f;
			item.uv0.y = 0.5f;
			_verts.Add(item);
			item.position.x = size.x * 0.5f;
			item.position.y = (0f - size.y) * 0.5f + num2;
			item.uv0.x = 0.5f;
			item.uv0.y = 0f;
			_verts.Add(item);
			foreach (UIVertex vert in _verts)
			{
				vh.AddVert(vert);
			}
			for (int i = 0; i <= vh.currentVertCount - 4; i += 4)
			{
				vh.AddTriangle(i, i + 1, i + 2);
				vh.AddTriangle(i, i + 2, i + 3);
			}
			if (_resolution == 0)
			{
				return;
			}
			float num3 = 0f;
			float num4 = Mathf.Tan(_centerSize * 0.5f / _radius) * 57.29578f;
			float num5 = Mathf.Max(0f, 360f * _fill * _fillScaler - num4) / (float)_resolution;
			num += _verts.Count;
			_verts.Clear();
			for (int j = 0; j <= _resolution; j++)
			{
				float f = (90f + num4 + num5 * (float)j) * ((float)Math.PI / 180f);
				Vector2 vector = new Vector2(Mathf.Cos(f), Mathf.Sin(f));
				Vector2 vector2 = vector * (_radius - size.y * 0.5f);
				item.position = vector2;
				item.position.x *= -1f;
				if (j > 0)
				{
					num3 += Vector2.Distance(item.position, _verts[_verts.Count - 2].position);
				}
				item.uv0.y = 0.5f;
				item.uv0.x = num3 / (float)width * _uvFix;
				_verts.Add(item);
				Vector2 vector3 = vector * (_radius + size.y * 0.5f);
				item.position = vector3;
				item.position.x *= -1f;
				item.uv0.y = 1f;
				item.uv0.x = num3 / (float)width * _uvFix;
				_verts.Add(item);
			}
			foreach (UIVertex vert2 in _verts)
			{
				vh.AddVert(vert2);
			}
			for (int k = num; k <= vh.currentVertCount - 4; k += 2)
			{
				vh.AddTriangle(k + 1, k + 3, k);
				vh.AddTriangle(k + 3, k + 2, k);
			}
			num += _verts.Count;
			_verts.Clear();
			num3 = 0f;
			for (int l = 0; l <= _resolution; l++)
			{
				float f2 = (90f + num4 + num5 * (float)l) * ((float)Math.PI / 180f);
				Vector2 vector4 = new Vector2(Mathf.Cos(f2), Mathf.Sin(f2));
				Vector2 vector5 = vector4 * (_radius - size.y * 0.5f);
				item.position = vector5;
				if (l > 0)
				{
					num3 += Vector2.Distance(item.position, _verts[_verts.Count - 2].position);
				}
				item.uv0.y = 1f;
				item.uv0.x = num3 / (float)width * _uvFix;
				_verts.Add(item);
				Vector2 vector6 = vector4 * (_radius + size.y * 0.5f);
				item.position = vector6;
				item.uv0.y = 0.5f;
				item.uv0.x = num3 / (float)width * _uvFix;
				_verts.Add(item);
			}
			foreach (UIVertex vert3 in _verts)
			{
				vh.AddVert(vert3);
			}
			for (int m = num; m <= vh.currentVertCount - 4; m += 2)
			{
				vh.AddTriangle(m, m + 3, m + 1);
				vh.AddTriangle(m, m + 2, m + 3);
			}
			num += _verts.Count;
			_verts.Clear();
			float f3 = (90f + Mathf.Max(360f * _fill * _fillScaler, num4)) * ((float)Math.PI / 180f);
			Vector2 vector7 = new Vector2(Mathf.Cos(f3), Mathf.Sin(f3));
			Vector2 vector8 = vector7 * (_radius - size.y * 0.5f);
			item.position = vector8;
			item.position.x *= -1f;
			item.uv0.y = 0f;
			item.uv0.x = 1f;
			_verts.Add(item);
			Vector2 vector9 = vector7 * (_radius + size.y * 0.5f);
			item.position = vector9;
			item.position.x *= -1f;
			item.uv0.y = 0.5f;
			item.uv0.x = 1f;
			_verts.Add(item);
			vector7 = Quaternion.Euler(0f, 0f, 90f) * vector7;
			vector8 += vector7 * _capSize;
			item.position = vector8;
			item.position.x *= -1f;
			item.uv0.y = 0f;
			item.uv0.x = 0.75f;
			_verts.Add(item);
			vector9 += vector7 * _capSize;
			item.position = vector9;
			item.position.x *= -1f;
			item.uv0.y = 0.5f;
			item.uv0.x = 0.75f;
			_verts.Add(item);
			foreach (UIVertex vert4 in _verts)
			{
				vh.AddVert(vert4);
			}
			for (int n = num; n <= vh.currentVertCount - 4; n += 2)
			{
				vh.AddTriangle(n + 1, n + 3, n);
				vh.AddTriangle(n + 3, n + 2, n);
			}
			num += _verts.Count;
			_verts.Clear();
			float f4 = (90f + Mathf.Max(360f * _fill * _fillScaler, num4)) * ((float)Math.PI / 180f);
			Vector2 vector10 = new Vector2(Mathf.Cos(f4), Mathf.Sin(f4));
			Vector2 vector11 = vector10 * (_radius - size.y * 0.5f);
			item.position = vector11;
			item.uv0.y = 0f;
			item.uv0.x = 1f;
			_verts.Add(item);
			Vector2 vector12 = vector10 * (_radius + size.y * 0.5f);
			item.position = vector12;
			item.uv0.y = 0.5f;
			item.uv0.x = 1f;
			_verts.Add(item);
			vector10 = Quaternion.Euler(0f, 0f, 90f) * vector10;
			vector11 += vector10 * _capSize;
			item.position = vector11;
			item.uv0.y = 0f;
			item.uv0.x = 0.75f;
			_verts.Add(item);
			vector12 += vector10 * _capSize;
			item.position = vector12;
			item.uv0.y = 0.5f;
			item.uv0.x = 0.75f;
			_verts.Add(item);
			foreach (UIVertex vert5 in _verts)
			{
				vh.AddVert(vert5);
			}
			for (int num6 = num; num6 <= vh.currentVertCount - 4; num6 += 2)
			{
				vh.AddTriangle(num6, num6 + 3, num6 + 1);
				vh.AddTriangle(num6, num6 + 2, num6 + 3);
			}
		}
	}
}
