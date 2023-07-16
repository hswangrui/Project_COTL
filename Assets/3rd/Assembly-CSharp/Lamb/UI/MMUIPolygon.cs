using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class MMUIPolygon : MaskableGraphic
	{
		[SerializeField]
		private Texture _texture;

		[SerializeField]
		private List<Vector2> _points;

		[SerializeField]
		[Range(1f, 1000f)]
		private float _roundToNearest = 1f;

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

		public List<Vector2> Points
		{
			get
			{
				return _points;
			}
			set
			{
				_points = value;
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
			vh.Clear();
			_verts.Clear();
			if (_points == null || _points.Count < 3)
			{
				return;
			}
			UIVertex uIVertex = default(UIVertex);
			uIVertex.color = color;
			UIVertex item = uIVertex;
			int[] array = new Triangulator(_points.ToArray()).Triangulate();
			foreach (Vector2 point in _points)
			{
				item.position = point;
				_verts.Add(item);
			}
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
