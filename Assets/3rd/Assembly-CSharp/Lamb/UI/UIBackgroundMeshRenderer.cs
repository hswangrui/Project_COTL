using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	[RequireComponent(typeof(RectTransform))]
	public class UIBackgroundMeshRenderer : MaskableGraphic
	{
		[SerializeField]
		private Texture _texture;

		[Header("Background Properties")]
		[SerializeField]
		[Range(0f, 1000f)]
		private float _falloff;

		private List<UIVertex> _verts = new List<UIVertex>();

		private RectTransform _rectTransform;

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

		protected override void OnEnable()
		{
			base.OnEnable();
			if (_rectTransform == null)
			{
				_rectTransform = GetComponent<RectTransform>();
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
			Vector2 size = _rectTransform.rect.size;
			Vector2 vector = new Vector2(100f, 100f);
			UIVertex uIVertex = default(UIVertex);
			uIVertex.color = color;
			uIVertex.tangent = new Vector4(0f, 0f, (size.x + _falloff) / (vector.x + 2.5f), 0f);
			UIVertex item = uIVertex;
			Vector2 uv = new Vector2(0f, 0f);
			item.position.x = (0f - size.x) * 0.5f;
			item.position.y = size.y * 0.5f;
			item.uv0 = uv;
			_verts.Add(item);
			item.position.x = size.x * 0.5f;
			uv.x = size.x / vector.x;
			item.uv0 = uv;
			_verts.Add(item);
			item.position.y = (0f - size.y) * 0.5f;
			uv.y = size.y / vector.y;
			item.uv0 = uv;
			_verts.Add(item);
			item.position.x = (0f - size.x) * 0.5f;
			uv.x = 0f;
			item.uv0 = uv;
			_verts.Add(item);
			item.position.x = size.x * 0.5f + _falloff;
			item.position.y = size.y * 0.5f;
			uv.x = (size.x + _falloff) / vector.x;
			uv.y = 0f;
			item.uv0 = uv;
			_verts.Add(item);
			item.position.y = (0f - size.y) * 0.5f;
			uv.y = size.y / vector.y;
			item.uv0 = uv;
			_verts.Add(item);
			int[] array = new int[12]
			{
				0, 1, 3, 3, 1, 2, 1, 4, 2, 2,
				4, 5
			};
			foreach (UIVertex vert in _verts)
			{
				vh.AddVert(vert);
			}
			for (int i = 0; i < array.Length; i += 3)
			{
				vh.AddTriangle(array[i], array[i + 1], array[i + 2]);
			}
		}
	}
}
