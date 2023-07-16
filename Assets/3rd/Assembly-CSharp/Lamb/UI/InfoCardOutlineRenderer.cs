using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	[RequireComponent(typeof(CanvasRenderer))]
	public class InfoCardOutlineRenderer : MaskableGraphic
	{
		[StructLayout(LayoutKind.Auto)]
		[CompilerGenerated]
		private struct _003C_003Ec__DisplayClass21_0
		{
			public Vector2 position;

			public Vector2 uv;

			public UIVertex vert;

			public InfoCardOutlineRenderer _003C_003E4__this;

			public float length;

			public float capSizeNormalized;

			public float uvLength;

			public float scale;

			public float angle;

			public Vector2 left;

			public float halfPI;

			public Vector2 right;

			public Color colorTransparent;
		}

		[SerializeField]
		private Texture _texture;

		[Header("Outline Properties")]
		[SerializeField]
		private bool _showBadge = true;

		[SerializeField]
		[Range(0f, 7f)]
		private int _badgeVariant;

		[SerializeField]
		[Range(1f, 100f)]
		private float _cornerSize = 100f;

		[SerializeField]
		[Range(1f, 100f)]
		private float _capSize = 10f;

		[SerializeField]
		[Range(-100f, 100f)]
		private float _offset;

		[Header("Lines")]
		[SerializeField]
		private bool _topLine = true;

		[SerializeField]
		private bool _bottomLine = true;

		[SerializeField]
		private bool _leftLine = true;

		[SerializeField]
		private bool _rightLine = true;

		[Header("Gaps")]
		[SerializeField]
		[Range(0f, 250f)]
		private float _topGap;

		[SerializeField]
		[Range(0f, 250f)]
		private float _leftGap;

		[SerializeField]
		[Range(0f, 250f)]
		private float _rightGap;

		[SerializeField]
		[Range(0f, 250f)]
		private float _bottomGap;

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

		public int BadgeVariant
		{
			get
			{
				return _badgeVariant;
			}
			set
			{
				_badgeVariant = value;
				SetVerticesDirty();
				SetMaterialDirty();
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
			_003C_003Ec__DisplayClass21_0 _003C_003Ec__DisplayClass21_ = default(_003C_003Ec__DisplayClass21_0);
			_003C_003Ec__DisplayClass21_._003C_003E4__this = this;
			vh.Clear();
			_verts.Clear();
			_003C_003Ec__DisplayClass21_.scale = (float)_texture.width / 4f / _cornerSize;
			Rect rect = base.rectTransform.rect;
			_003C_003Ec__DisplayClass21_.halfPI = (float)Math.PI / 2f;
			float num = rect.width * 0.5f + _offset;
			float num2 = rect.height * 0.5f + _offset;
			_003C_003Ec__DisplayClass21_.colorTransparent = new Color(color.r, color.g, color.b, 0f);
			_003C_003Ec__DisplayClass21_.vert = new UIVertex
			{
				color = color,
				normal = Vector2.zero
			};
			_003COnPopulateMesh_003Eg__AddSquare_007C21_0(new Vector2(0f - num, num2), new Vector2(0f, 0.5f), ref _003C_003Ec__DisplayClass21_);
			_003COnPopulateMesh_003Eg__AddSquare_007C21_0(new Vector2(num - _cornerSize, num2), new Vector2(0.25f, 0.5f), ref _003C_003Ec__DisplayClass21_);
			_003COnPopulateMesh_003Eg__AddSquare_007C21_0(new Vector2(num - _cornerSize, 0f - num2 + _cornerSize), new Vector2(0.5f, 0.5f), ref _003C_003Ec__DisplayClass21_);
			_003COnPopulateMesh_003Eg__AddSquare_007C21_0(new Vector2(0f - num, 0f - num2 + _cornerSize), new Vector2(0.75f, 0.5f), ref _003C_003Ec__DisplayClass21_);
			if (_topLine)
			{
				if (_showBadge)
				{
					_003COnPopulateMesh_003Eg__AddSquare_007C21_0(new Vector2((0f - _cornerSize) * 0.5f, num2), new Vector2(0.25f * (float)_badgeVariant - Mathf.Floor((float)_badgeVariant / 4f), 1f - 0.25f * Mathf.Floor((float)_badgeVariant / 4f)), ref _003C_003Ec__DisplayClass21_);
					_003COnPopulateMesh_003Eg__AddLine_007C21_1(new Vector2(0f - num + _cornerSize, num2 - _cornerSize * 0.5f), new Vector2((0f - _cornerSize) * 0.5f, num2 - _cornerSize * 0.5f), ref _003C_003Ec__DisplayClass21_);
					_003COnPopulateMesh_003Eg__AddLine_007C21_1(new Vector2(num - _cornerSize, num2 - _cornerSize * 0.5f), new Vector2(_cornerSize * 0.5f, num2 - _cornerSize * 0.5f), ref _003C_003Ec__DisplayClass21_);
				}
				else
				{
					_003COnPopulateMesh_003Eg__AddLine_007C21_1(new Vector2(0f - num + _cornerSize, num2 - _cornerSize * 0.5f), new Vector2(num - _cornerSize, num2 - _cornerSize * 0.5f), ref _003C_003Ec__DisplayClass21_);
				}
			}
			if (_leftLine)
			{
				if (_leftGap > 0f)
				{
					_003COnPopulateMesh_003Eg__AddLine_007C21_1(new Vector2(0f - num + _cornerSize * 0.5f, num2 - _cornerSize), new Vector2(0f - num + _cornerSize * 0.5f, _leftGap * 0.5f), ref _003C_003Ec__DisplayClass21_);
					_003COnPopulateMesh_003Eg__AddLine_007C21_1(new Vector2(0f - num + _cornerSize * 0.5f, 0f - num2 + _cornerSize), new Vector2(0f - num + _cornerSize * 0.5f, (0f - _leftGap) * 0.5f), ref _003C_003Ec__DisplayClass21_);
				}
				else
				{
					_003COnPopulateMesh_003Eg__AddLine_007C21_1(new Vector2(0f - num + _cornerSize * 0.5f, num2 - _cornerSize), new Vector2(0f - num + _cornerSize * 0.5f, 0f - num2 + _cornerSize), ref _003C_003Ec__DisplayClass21_);
				}
			}
			if (_rightLine)
			{
				if (_rightGap > 0f)
				{
					_003COnPopulateMesh_003Eg__AddLine_007C21_1(new Vector2(num - _cornerSize * 0.5f, num2 - _cornerSize), new Vector2(num - _cornerSize * 0.5f, _rightGap * 0.5f), ref _003C_003Ec__DisplayClass21_);
					_003COnPopulateMesh_003Eg__AddLine_007C21_1(new Vector2(num - _cornerSize * 0.5f, 0f - num2 + _cornerSize), new Vector2(num - _cornerSize * 0.5f, (0f - _rightGap) * 0.5f), ref _003C_003Ec__DisplayClass21_);
				}
				else
				{
					_003COnPopulateMesh_003Eg__AddLine_007C21_1(new Vector2(num - _cornerSize * 0.5f, num2 - _cornerSize), new Vector2(num - _cornerSize * 0.5f, 0f - num2 + _cornerSize), ref _003C_003Ec__DisplayClass21_);
				}
			}
			if (_bottomLine)
			{
				if (_bottomGap > 0f)
				{
					_003COnPopulateMesh_003Eg__AddLine_007C21_1(new Vector2(0f - num + _cornerSize, 0f - num2 + _cornerSize * 0.5f), new Vector2((0f - _bottomGap) * 0.5f, 0f - num2 + _cornerSize * 0.5f), ref _003C_003Ec__DisplayClass21_);
					_003COnPopulateMesh_003Eg__AddLine_007C21_1(new Vector2(num - _cornerSize, 0f - num2 + _cornerSize * 0.5f), new Vector2(_bottomGap * 0.5f, 0f - num2 + _cornerSize * 0.5f), ref _003C_003Ec__DisplayClass21_);
				}
				else
				{
					_003COnPopulateMesh_003Eg__AddLine_007C21_1(new Vector2(0f - num + _cornerSize, 0f - num2 + _cornerSize * 0.5f), new Vector2(num - _cornerSize, 0f - num2 + _cornerSize * 0.5f), ref _003C_003Ec__DisplayClass21_);
				}
			}
			foreach (UIVertex vert in _verts)
			{
				vh.AddVert(vert);
			}
			for (int i = 0; i <= vh.currentVertCount - 4; i += 4)
			{
				vh.AddTriangle(i, i + 1, i + 3);
				vh.AddTriangle(i + 3, i + 1, i + 2);
			}
		}

		[CompilerGenerated]
		private void _003COnPopulateMesh_003Eg__AddSquare_007C21_0(Vector2 squarePosition, Vector2 squareUV, ref _003C_003Ec__DisplayClass21_0 P_2)
		{
			P_2.position = squarePosition;
			P_2.uv = squareUV;
			P_2.vert.position = P_2.position;
			P_2.vert.uv0 = P_2.uv;
			_verts.Add(P_2.vert);
			P_2.position.x += _cornerSize;
			P_2.uv.x += 0.25f;
			P_2.vert.position = P_2.position;
			P_2.vert.uv0 = P_2.uv;
			_verts.Add(P_2.vert);
			P_2.position.y -= _cornerSize;
			P_2.uv.y -= 0.25f;
			P_2.vert.position = P_2.position;
			P_2.vert.uv0 = P_2.uv;
			_verts.Add(P_2.vert);
			P_2.position.x -= _cornerSize;
			P_2.uv.x -= 0.25f;
			P_2.vert.position = P_2.position;
			P_2.vert.uv0 = P_2.uv;
			_verts.Add(P_2.vert);
		}

		[CompilerGenerated]
		private void _003COnPopulateMesh_003Eg__AddLine_007C21_1(Vector2 from, Vector2 to, ref _003C_003Ec__DisplayClass21_0 P_2)
		{
			P_2.length = Vector2.Distance(from, to);
			P_2.capSizeNormalized = _capSize / P_2.length;
			P_2.uvLength = P_2.length / ((float)_texture.width / P_2.scale);
			P_2.angle = Mathf.Atan2(to.y - from.y, to.x - from.x);
			P_2.left.x = Mathf.Cos(P_2.angle + P_2.halfPI);
			P_2.left.y = Mathf.Sin(P_2.angle + P_2.halfPI);
			P_2.right.x = Mathf.Cos(P_2.angle - P_2.halfPI);
			P_2.right.y = Mathf.Sin(P_2.angle - P_2.halfPI);
			_003COnPopulateMesh_003Eg__Add_007C21_2(from, Vector2.Lerp(from, to, P_2.capSizeNormalized), new Vector2[4]
			{
				new Vector2(0f, 0.25f),
				new Vector2(P_2.capSizeNormalized, 0.25f),
				new Vector2(P_2.capSizeNormalized, 0f),
				new Vector2(0f, 0f)
			}, new Color[4] { P_2.colorTransparent, color, color, P_2.colorTransparent }, ref P_2);
			_003COnPopulateMesh_003Eg__Add_007C21_2(Vector2.Lerp(from, to, P_2.capSizeNormalized), Vector2.Lerp(from, to, 1f - P_2.capSizeNormalized), new Vector2[4]
			{
				new Vector2(P_2.capSizeNormalized, 0.25f),
				new Vector2(P_2.uvLength - P_2.capSizeNormalized * 2f, 0.25f),
				new Vector2(P_2.uvLength - P_2.capSizeNormalized * 2f, 0f),
				new Vector2(P_2.capSizeNormalized, 0f)
			}, new Color[4] { color, color, color, color }, ref P_2);
			_003COnPopulateMesh_003Eg__Add_007C21_2(Vector2.Lerp(from, to, 1f - P_2.capSizeNormalized), to, new Vector2[4]
			{
				new Vector2(P_2.uvLength - P_2.capSizeNormalized * 2f, 0.25f),
				new Vector2(P_2.uvLength, 0.25f),
				new Vector2(P_2.uvLength, 0f),
				new Vector2(P_2.uvLength - P_2.capSizeNormalized * 2f, 0f)
			}, new Color[4] { color, P_2.colorTransparent, P_2.colorTransparent, color }, ref P_2);
		}

		[CompilerGenerated]
		private void _003COnPopulateMesh_003Eg__Add_007C21_2(Vector2 addFrom, Vector2 addTo, Vector2[] uvs, Color[] colors, ref _003C_003Ec__DisplayClass21_0 P_4)
		{
			P_4.vert.position = addFrom + P_4.left * (_cornerSize * 0.5f);
			P_4.vert.uv0 = uvs[0];
			P_4.vert.color = colors[0];
			_verts.Add(P_4.vert);
			P_4.vert.position = addTo + P_4.left * (_cornerSize * 0.5f);
			P_4.vert.uv0 = uvs[1];
			P_4.vert.color = colors[1];
			_verts.Add(P_4.vert);
			P_4.vert.position = addTo + P_4.right * (_cornerSize * 0.5f);
			P_4.vert.uv0 = uvs[2];
			P_4.vert.color = colors[2];
			_verts.Add(P_4.vert);
			P_4.vert.position = addFrom + P_4.right * (_cornerSize * 0.5f);
			P_4.vert.uv0 = uvs[3];
			P_4.vert.color = colors[3];
			_verts.Add(P_4.vert);
		}
	}
}
