using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	[RequireComponent(typeof(CanvasRenderer))]
	public class MMUILineRenderer : MaskableGraphic, ISerializationCallbackReceiver
	{
		[Serializable]
		public enum FillStyle
		{
			Standard,
			Reverse
		}

		[Serializable]
		public class BranchPoint
		{
			public Vector2 Point;

			public List<Branch> Branches = new List<Branch>();

			public float x
			{
				get
				{
					return Point.x;
				}
			}

			public float y
			{
				get
				{
					return Point.y;
				}
			}

			public BranchPoint(BranchPoint point)
			{
				Point = point.Point;
			}

			public BranchPoint(Vector2 point)
			{
				Point = point;
			}

			public BranchPoint(float x, float y)
			{
				Point = new Vector2(x, y);
			}

			public void AddBranch()
			{
				Branches.Add(new Branch
				{
					Points = new List<BranchPoint>
					{
						new BranchPoint(Point)
					}
				});
			}

			public Branch AddNewBranch()
			{
				Branch branch = new Branch();
				Branches.Add(branch);
				return branch;
			}
		}

		[Serializable]
		public class Branch
		{
			public int Hash;

			public List<BranchPoint> Points = new List<BranchPoint>();

			public FillStyle FillStyle;

			[SerializeField]
			private Color _color = Color.white;

			[SerializeField]
			[Range(0f, 1f)]
			private float _fill = 1f;

			public float TotalLength;

			private bool _isDirty;

			public bool IsDirty
			{
				get
				{
					if (Points != null)
					{
						foreach (BranchPoint point in Points)
						{
							if (point.Branches == null)
							{
								continue;
							}
							foreach (Branch branch in point.Branches)
							{
								if (branch.IsDirty)
								{
									return true;
								}
							}
						}
					}
					return _isDirty;
				}
				set
				{
					_isDirty = value;
					if (Points == null)
					{
						return;
					}
					foreach (BranchPoint point in Points)
					{
						if (point.Branches == null)
						{
							continue;
						}
						foreach (Branch branch in point.Branches)
						{
							branch.IsDirty = value;
						}
					}
				}
			}

			public Color Color
			{
				get
				{
					return _color;
				}
				set
				{
					_color = value;
					_isDirty = true;
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
					_isDirty = true;
				}
			}

			public Branch()
			{
				if (Hash == 0)
				{
					System.Random random = new System.Random(Guid.NewGuid().GetHashCode());
					Hash = random.Next(int.MinValue, int.MaxValue).ToString().GetStableHashCode();
				}
			}

			public Branch FindBranchByHash(int hash)
			{
				if (Hash == hash)
				{
					return this;
				}
				foreach (BranchPoint point in Points)
				{
					if (point.Branches == null || point.Branches.Count <= 0)
					{
						continue;
					}
					foreach (Branch branch2 in point.Branches)
					{
						Branch branch = branch2.FindBranchByHash(hash);
						if (branch != null)
						{
							return branch;
						}
					}
				}
				return null;
			}

			public void SetAllFillModes(FillStyle fillStyle)
			{
				FillStyle = fillStyle;
				foreach (BranchPoint point in Points)
				{
					if (point.Branches == null || point.Branches.Count <= 0)
					{
						continue;
					}
					foreach (Branch branch in point.Branches)
					{
						branch.SetAllFillModes(fillStyle);
					}
				}
			}
		}

		[SerializeField]
		private Texture _texture;

		[SerializeField]
		private float _width = 10f;

		[SerializeField]
		[HideInInspector]
		private string _serializedRoot;

		[NonSerialized]
		private Branch _root = new Branch();

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

		public Branch Root
		{
			get
			{
				return _root;
			}
		}

		public List<BranchPoint> Points
		{
			get
			{
				return _root.Points;
			}
			set
			{
				_root.Points = value;
				_root.IsDirty = true;
				UpdateValues();
			}
		}

		public float Fill
		{
			get
			{
				return _root.Fill;
			}
			set
			{
				_root.Fill = value;
			}
		}

		public Color Color
		{
			get
			{
				return _root.Color;
			}
			set
			{
				_root.Color = value;
			}
		}

		public float Width
		{
			get
			{
				return _width;
			}
			set
			{
				_width = value;
				_root.IsDirty = true;
			}
		}

		public Texture Texture
		{
			get
			{
				return _texture;
			}
			set
			{
				_texture = value;
				UpdateRendering();
			}
		}

		public void OnBeforeSerialize()
		{
			_serializedRoot = JsonUtility.ToJson(_root);
		}

		public void OnAfterDeserialize()
		{
			_root = JsonUtility.FromJson<Branch>(_serializedRoot);
		}

		public void UpdateRendering()
		{
			SetVerticesDirty();
			SetMaterialDirty();
		}

		public void UpdateValues()
		{
			GetLongestDistance();
		}

		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
			UpdateRendering();
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();
			_verts.Clear();
			DrawLine(_root, null, vh);
		}

		private float GetLength(List<BranchPoint> branch)
		{
			float num = 0f;
			for (int i = 0; i < branch.Count - 1; i++)
			{
				num += Vector2.Distance(branch[i].Point, branch[i + 1].Point);
			}
			return num;
		}

		private void Update()
		{
			if (Application.isPlaying && _root.IsDirty)
			{
				UpdateRendering();
				_root.IsDirty = false;
			}
		}

		private float GetLongestDistance()
		{
			int num = 0;
			float num2 = 0f;
			int num3 = 0;
			Dictionary<List<BranchPoint>, float> dictionary = new Dictionary<List<BranchPoint>, float> { { _root.Points, 0f } };
			_root.TotalLength = GetLength(_root.Points);
			while (num3 < dictionary.Keys.Count)
			{
				List<BranchPoint> list = dictionary.Keys.ToList()[num3];
				float num4 = dictionary[list];
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].Branches != null && list[i].Branches.Count > 0)
					{
						foreach (Branch branch in list[i].Branches)
						{
							List<BranchPoint> list2 = new List<BranchPoint>();
							list2.Add(new BranchPoint(list[i]));
							list2.AddRange(branch.Points);
							dictionary.Add(list2, num4);
							branch.TotalLength = GetLength(list2);
						}
					}
					if (i < list.Count - 1)
					{
						num4 += Vector2.Distance(list[i].Point, list[i + 1].Point);
					}
				}
				if (num4 > num2)
				{
					num2 = num4;
				}
				num3++;
				num++;
				if (num > 100000)
				{
					Debug.Log("Number of iterations exceeded maximum. Check algorithm!".Colour(Color.red));
					break;
				}
			}
			return num2;
		}

		private void DrawLine(Branch branch, BranchPoint startingPoint, VertexHelper vertexHelper)
		{
			UIVertex uIVertex = default(UIVertex);
			uIVertex.color = color;
			UIVertex v = uIVertex;
			Vector2 uv = Vector3.zero;
			float num = 0f;
			float num2 = (float)Math.PI / 2f;
			float num3 = 0f;
			float totalLength = branch.TotalLength;
			totalLength *= branch.Fill;
			List<BranchPoint> list = new List<BranchPoint>(branch.Points);
			if (startingPoint != null)
			{
				list.Insert(0, startingPoint);
			}
			if (branch.FillStyle == FillStyle.Reverse)
			{
				list.Reverse();
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Branches != null && list[i].Branches.Count > 0)
				{
					foreach (Branch branch2 in list[i].Branches)
					{
						DrawLine(branch2, new BranchPoint(list[i]), vertexHelper);
					}
				}
				if (!(num3 > totalLength) && i != list.Count - 1)
				{
					int currentVertCount = vertexHelper.currentVertCount;
					vertexHelper.AddTriangle(currentVertCount, currentVertCount + 2, currentVertCount + 1);
					vertexHelper.AddTriangle(currentVertCount + 1, currentVertCount + 2, currentVertCount + 3);
					float num4 = Mathf.Atan2(list[i + 1].y - list[i].y, list[i + 1].x - list[i].x);
					Vector2 vector = new Vector2(Mathf.Cos(num4 + num2), Mathf.Sin(num4 + num2));
					Vector2 vector2 = new Vector2(Mathf.Cos(num4 - num2), Mathf.Sin(num4 - num2));
					float num5 = Vector2.Distance(list[i].Point, list[i + 1].Point);
					num3 += num5;
					if (num3 > totalLength)
					{
						num = (num3 - totalLength) / num5;
						num = 1f - num;
					}
					else
					{
						num = 1f;
					}
					uv.x = 0f;
					v.position = list[i].Point + vector * _width;
					v.uv0 = uv;
					v.color = branch.Color;
					vertexHelper.AddVert(v);
					uv.x = 1f;
					v.position = list[i].Point + vector2 * _width;
					v.uv0 = uv;
					v.color = branch.Color;
					vertexHelper.AddVert(v);
					uv.x = 0f;
					if (_texture != null)
					{
						uv.y += num5 * num / (float)_texture.height;
					}
					else
					{
						uv.y += 1f;
					}
					v.position = Vector2.Lerp(list[i].Point, list[i + 1].Point, num) + vector * _width;
					v.uv0 = uv;
					v.color = branch.Color;
					vertexHelper.AddVert(v);
					uv.x = 1f;
					v.position = Vector2.Lerp(list[i].Point, list[i + 1].Point, num) + vector2 * _width;
					v.uv0 = uv;
					v.color = branch.Color;
					vertexHelper.AddVert(v);
					continue;
				}
				break;
			}
		}
	}
}
