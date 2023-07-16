using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	[RequireComponent(typeof(RectTransform))]
	public class MMUIDialogWindow : MaskableGraphic
	{
		[SerializeField]
		private Texture _texture;

		[SerializeField]
		private float _arrowWidth;

		[SerializeField]
		private float _arrowHeight;

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
			UIVertex uIVertex = default(UIVertex);
			uIVertex.color = color;
			UIVertex item = uIVertex;
			Vector2 size = _rectTransform.rect.size;
			item.uv0.x = 0f;
			item.uv0.y = 0f;
			item.position.x = (0f - size.x) * 0.5f;
			item.position.y = size.y * 0.5f;
			_verts.Add(item);
			item.position.x = size.x * 0.5f;
			_verts.Add(item);
			item.position.y = (0f - size.y) * 0.5f;
			_verts.Add(item);
			item.position.x = _arrowWidth * 0.5f;
			_verts.Add(item);
			item.position.x = 0f;
			item.position.y = (0f - size.y) * 0.5f - _arrowHeight;
			_verts.Add(item);
			item.position.x = (0f - _arrowWidth) * 0.5f;
			item.position.y = (0f - size.y) * 0.5f;
			_verts.Add(item);
			item.position.x = (0f - size.x) * 0.5f;
			_verts.Add(item);
			int[] array = new int[15]
			{
				0, 5, 6, 0, 3, 5, 0, 1, 3, 1,
				2, 3, 5, 3, 4
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
