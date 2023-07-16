using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class UpgradeTreeEffectPlane : MaskableGraphic
	{
		[Serializable]
		private class BakedNodeData
		{
			public UpgradeTreeNode Node;

			public float AlphaContribution;

			public float GetUnavailability()
			{
				return Node.UnavailableWeight;
			}

			public float GetAvailability()
			{
				return Node.AvailableWeight;
			}

			public float GetUnlocked()
			{
				return Node.UnlockedWeight;
			}
		}

		[Serializable]
		private class BakedVertex
		{
			public Vector2 Position;

			public Color Color;

			public Vector2 UV;

			public List<BakedNodeData> BakedNodes;
		}

		[Header("Grid")]
		[SerializeField]
		private float _gridSize = 1f;

		[SerializeField]
		private int _horizontalPoints = 2;

		[SerializeField]
		private int _verticalPoints = 2;

		[Header("Effects")]
		[SerializeField]
		private float _falloff;

		[SerializeField]
		private float _verticalFalloff;

		[SerializeField]
		private AnimationCurve _verticalFalloffCurve;

		[SerializeField]
		private float _intensity = 1f;

		[SerializeField]
		private List<UpgradeTreeNode> _nodes = new List<UpgradeTreeNode>();

		[SerializeField]
		[HideInInspector]
		private List<BakedVertex> _bakedVertices = new List<BakedVertex>();

		private List<UIVertex> _verts = new List<UIVertex>();

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();
			_verts.Clear();
			if (_bakedVertices.Count < 3)
			{
				return;
			}
			UIVertex item = default(UIVertex);
			Color black = Color.black;
			for (int i = 0; i < _bakedVertices.Count; i++)
			{
				item.position = _bakedVertices[i].Position;
				item.uv0 = _bakedVertices[i].UV;
				black = _bakedVertices[i].Color;
				foreach (BakedNodeData bakedNode in _bakedVertices[i].BakedNodes)
				{
					black.r += bakedNode.AlphaContribution * bakedNode.GetUnavailability();
					black.g += bakedNode.AlphaContribution * bakedNode.GetAvailability();
					black.b += bakedNode.AlphaContribution * bakedNode.GetUnlocked();
				}
				black.r = Mathf.Clamp(black.r, 0f, 1f);
				black.g = Mathf.Clamp(black.g, 0f, 1f);
				black.b = Mathf.Clamp(black.b, 0f, 1f);
				black.a = Mathf.Clamp(black.a, 0f, 1f);
				item.color = black;
				_verts.Add(item);
			}
			foreach (UIVertex vert in _verts)
			{
				vh.AddVert(vert);
			}
			for (int j = 0; j < vh.currentVertCount - _horizontalPoints - 1; j++)
			{
				if (j <= 0 || (j + 1) % _horizontalPoints != 0)
				{
					vh.AddTriangle(j, j + _horizontalPoints, j + _horizontalPoints + 1);
					vh.AddTriangle(j, j + _horizontalPoints + 1, j + 1);
				}
			}
		}
	}
}
