using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	[RequireComponent(typeof(RectTransform))]
	public class MMUITiledRenderer : MaskableGraphic
	{
		[SerializeField]
		private Sprite _sprite;

		[SerializeField]
		private bool _tileVertical;

		[SerializeField]
		private bool _tileHorizonal;

		private List<UIVertex> _verts = new List<UIVertex>();

		private RectTransform _rectTransform;

		private Canvas _cachedCanvas;

		public override Texture mainTexture
		{
			get
			{
				if (!(_sprite.texture == null))
				{
					return _sprite.texture;
				}
				return Graphic.s_WhiteTexture;
			}
		}

		private Canvas _canvas
		{
			get
			{
				if (_cachedCanvas == null)
				{
					Canvas[] componentsInParent = base.gameObject.GetComponentsInParent<Canvas>();
					foreach (Canvas canvas in componentsInParent)
					{
						if (canvas.isActiveAndEnabled)
						{
							_cachedCanvas = canvas;
							break;
						}
					}
				}
				return _cachedCanvas;
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
			Vector2 vector = new Vector2(mainTexture.width, mainTexture.height);
			if (_canvas != null)
			{
				vector.x *= _canvas.referencePixelsPerUnit / _sprite.pixelsPerUnit;
				vector.y *= _canvas.referencePixelsPerUnit / _sprite.pixelsPerUnit;
			}
			UIVertex uIVertex = default(UIVertex);
			uIVertex.color = color;
			UIVertex item = uIVertex;
			Vector2 uv = new Vector2(0f, 1f);
			item.position.x = (0f - size.x) * 0.5f;
			item.position.y = size.y * 0.5f;
			item.uv0 = uv;
			_verts.Add(item);
			item.position.x = size.x * 0.5f;
			if (_tileHorizonal)
			{
				uv.x = size.x / vector.x;
			}
			else
			{
				uv.x = 1f;
			}
			item.uv0 = uv;
			_verts.Add(item);
			item.position.y = (0f - size.y) * 0.5f;
			uv.y = 0f;
			item.uv0 = uv;
			_verts.Add(item);
			item.position.x = (0f - size.x) * 0.5f;
			uv.x = 0f;
			item.uv0 = uv;
			_verts.Add(item);
			int[] array = new int[6] { 0, 1, 3, 3, 1, 2 };
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
