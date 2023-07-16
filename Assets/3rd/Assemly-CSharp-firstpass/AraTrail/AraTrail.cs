using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Ara
{
	[ExecuteInEditMode]
	public class AraTrail : MonoBehaviour
	{
		public enum TrailAlignment
		{
			View,
			Velocity,
			Local
		}

		public enum TrailSpace
		{
			World,
			Self,
			Custom
		}

		public enum TrailSorting
		{
			OlderOnTop,
			NewerOnTop
		}

		public enum Timescale
		{
			Normal,
			Unscaled
		}

		public enum TextureMode
		{
			Stretch,
			Tile,
			WorldTile
		}

		public struct CurveFrame
		{
			public Vector3 position;

			public Vector3 normal;

			public Vector3 bitangent;

			public Vector3 tangent;

			public CurveFrame(Vector3 position, Vector3 normal, Vector3 bitangent, Vector3 tangent)
			{
				this.position = position;
				this.normal = normal;
				this.bitangent = bitangent;
				this.tangent = tangent;
			}

			public Vector3 Transport(Vector3 newTangent, Vector3 newPosition)
			{
				Vector3 vector = newPosition - position;
				float num = Vector3.Dot(vector, vector);
				Vector3 vector2 = normal - 2f / (num + 1E-05f) * Vector3.Dot(vector, normal) * vector;
				Vector3 vector3 = tangent - 2f / (num + 1E-05f) * Vector3.Dot(vector, tangent) * vector;
				Vector3 vector4 = newTangent - vector3;
				float num2 = Vector3.Dot(vector4, vector4);
				Vector3 rhs = vector2 - 2f / (num2 + 1E-05f) * Vector3.Dot(vector4, vector2) * vector4;
				Vector3 vector5 = Vector3.Cross(newTangent, rhs);
				normal = rhs;
				bitangent = vector5;
				tangent = newTangent;
				position = newPosition;
				return normal;
			}
		}

		public struct Point
		{
			public Vector3 position;

			public Vector3 velocity;

			public Vector3 tangent;

			public Vector3 normal;

			public Color color;

			public float thickness;

			public float life;

			public float texcoord;

			public bool discontinuous;

			public Point(Vector3 position, Vector3 velocity, Vector3 tangent, Vector3 normal, Color color, float thickness, float texcoord, float lifetime)
			{
				this.position = position;
				this.velocity = velocity;
				this.tangent = tangent;
				this.normal = normal;
				this.color = color;
				this.thickness = thickness;
				life = lifetime;
				this.texcoord = texcoord;
				discontinuous = false;
			}

			public static float CatmullRom(float p0, float p1, float p2, float p3, float t)
			{
				float num = t * t;
				return 0.5f * (2f * p1 + (0f - p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * num + (0f - p0 + 3f * p1 - 3f * p2 + p3) * num * t);
			}

			public static Point operator +(Point p1, Point p2)
			{
				return new Point(p1.position + p2.position, p1.velocity + p2.velocity, p1.tangent + p2.tangent, p1.normal + p2.normal, p1.color + p2.color, p1.thickness + p2.thickness, p1.texcoord + p2.texcoord, p1.life + p2.life);
			}

			public static Point operator -(Point p1, Point p2)
			{
				return new Point(p1.position - p2.position, p1.velocity - p2.velocity, p1.tangent - p2.tangent, p1.normal - p2.normal, p1.color - p2.color, p1.thickness - p2.thickness, p1.texcoord - p2.texcoord, p1.life - p2.life);
			}
		}

		public const float epsilon = 1E-05f;

		[Header("Overall")]
		[Tooltip("Trail cross-section asset, determines the shape of the emitted trail. If no asset is specified, the trail will be a simple strip.")]
		public TrailSection section;

		[Tooltip("Whether to use world or local space to generate and simulate the trail.")]
		public TrailSpace space;

		[Tooltip("Custom space to use when generating and simulating the trail")]
		public Transform customSpace;

		[Tooltip("Whether to use regular time.")]
		public Timescale timescale;

		[Tooltip("How to align the trail geometry: facing the camera (view) of using the transform's rotation (local).")]
		public TrailAlignment alignment;

		[Tooltip("Determines the order in which trail points will be rendered.")]
		public TrailSorting sorting;

		[Tooltip("Thickness multiplier, in meters.")]
		public float thickness = 0.1f;

		[Tooltip("Amount of smoothing iterations applied to the trail shape.")]
		[Range(1f, 8f)]
		public int smoothness = 1;

		[Tooltip("Calculate accurate thickness at sharp corners.")]
		public bool highQualityCorners;

		[Range(0f, 12f)]
		public int cornerRoundness = 5;

		[Header("Length")]
		[Tooltip("How should the thickness of the curve evolve over its lenght. The horizontal axis is normalized lenght (in the [0,1] range) and the vertical axis is a thickness multiplier.")]
		[FormerlySerializedAs("thicknessOverLenght")]
		public AnimationCurve thicknessOverLength = AnimationCurve.Linear(0f, 1f, 0f, 1f);

		[Tooltip("How should vertex color evolve over the trail's length.")]
		[FormerlySerializedAs("colorOverLenght")]
		public Gradient colorOverLength = new Gradient();

		[Header("Time")]
		[Tooltip("How should the thickness of the curve evolve with its lifetime. The horizontal axis is normalized lifetime (in the [0,1] range) and the vertical axis is a thickness multiplier.")]
		public AnimationCurve thicknessOverTime = AnimationCurve.Linear(0f, 1f, 0f, 1f);

		[Tooltip("How should vertex color evolve over the trail's lifetime.")]
		public Gradient colorOverTime = new Gradient();

		[Header("Emission")]
		public bool emit = true;

		[Tooltip("Initial thickness of trail points when they are first spawned.")]
		public float initialThickness = 1f;

		[Tooltip("Initial color of trail points when they are first spawned.")]
		public Color initialColor = Color.white;

		[Tooltip("Initial velocity of trail points when they are first spawned.")]
		public Vector3 initialVelocity = Vector3.zero;

		[Tooltip("Minimum amount of time (in seconds) that must pass before spawning a new point.")]
		public float timeInterval = 0.025f;

		[Tooltip("Minimum distance (in meters) that must be left between consecutive points in the trail.")]
		public float minDistance = 0.025f;

		[Tooltip("Duration of the trail (in seconds).")]
		public float time = 2f;

		[Header("Physics")]
		[Tooltip("Toggles trail physics.")]
		public bool enablePhysics;

		[Tooltip("Amount of seconds pre-simulated before the trail appears. Useful when you want a trail to be already simulating when the game starts.")]
		public float warmup;

		[Tooltip("Gravity affecting the trail.")]
		public Vector3 gravity = Vector3.zero;

		[Tooltip("Amount of speed transferred from the transform to the trail. 0 means no velocity is transferred, 1 means 100% of the velocity is transferred.")]
		[Range(0f, 1f)]
		public float inertia;

		[Tooltip("Amount of temporal smoothing applied to the velocity transferred from the transform to the trail.")]
		[Range(0f, 1f)]
		public float velocitySmoothing = 0.75f;

		[Tooltip("Amount of damping applied to the trail's velocity. Larger values will slow down the trail more as time passes.")]
		[Range(0f, 1f)]
		public float damping = 0.75f;

		[Header("Rendering")]
		public Material[] materials = new Material[1];

		public ShadowCastingMode castShadows = ShadowCastingMode.On;

		public bool receiveShadows = true;

		public bool useLightProbes = true;

		[Header("Texture")]
		[Tooltip("Quad mapping will send the shader an extra coordinate for each vertex, that can be used to correct UV distortion using tex2Dproj.")]
		public bool quadMapping;

		[Tooltip("How to apply the texture over the trail: stretch it all over its lenght, or tile it.")]
		public TextureMode textureMode;

		[Tooltip("Defines how many times are U coords repeated across the length of the trail.")]
		public float uvFactor = 1f;

		[Tooltip("Defines how many times are V coords repeated trough the width of the trail.")]
		public float uvWidthFactor = 1f;

		[Tooltip("When the texture mode is set to 'Tile', defines where to begin tiling from: 0 means the start of the trail, 1 means the end.")]
		[Range(0f, 1f)]
		public float tileAnchor = 1f;

		[HideInInspector]
		public ElasticArray<Point> points = new ElasticArray<Point>();

		private ElasticArray<Point> renderablePoints = new ElasticArray<Point>();

		private List<int> discontinuities = new List<int>();

		private Mesh mesh_;

		private Vector3 velocity = Vector3.zero;

		private Vector3 prevPosition;

		private float accumTime;

		private List<Vector3> vertices = new List<Vector3>();

		private List<Vector3> normals = new List<Vector3>();

		private List<Vector4> tangents = new List<Vector4>();

		private List<Vector4> uvs = new List<Vector4>();

		private List<Color> vertColors = new List<Color>();

		private List<int> tris = new List<int>();

		private Vector3 nextV = Vector3.zero;

		private Vector3 prevV = Vector3.zero;

		private Vector3 vertex = Vector3.zero;

		private Vector3 normal = Vector3.zero;

		private Vector3 bitangent = Vector3.zero;

		private Vector4 tangent = new Vector4(0f, 0f, 0f, 1f);

		private Vector4 texTangent = Vector4.zero;

		private Vector4 uv = Vector4.zero;

		private Color color;

		private Action<ScriptableRenderContext, Camera> renderCallback;

		public Vector3 Velocity
		{
			get
			{
				return velocity;
			}
		}

		private float DeltaTime
		{
			get
			{
				if (timescale != Timescale.Unscaled)
				{
					return Time.deltaTime;
				}
				return Time.unscaledDeltaTime;
			}
		}

		private float FixedDeltaTime
		{
			get
			{
				if (timescale != Timescale.Unscaled)
				{
					return Time.fixedDeltaTime;
				}
				return Time.fixedUnscaledDeltaTime;
			}
		}

		public Mesh mesh
		{
			get
			{
				return mesh_;
			}
		}

		public Matrix4x4 worldToTrail
		{
			get
			{
				switch (space)
				{
				case TrailSpace.World:
					return Matrix4x4.identity;
				case TrailSpace.Self:
					return base.transform.worldToLocalMatrix;
				case TrailSpace.Custom:
					if (!(customSpace != null))
					{
						return Matrix4x4.identity;
					}
					return customSpace.worldToLocalMatrix;
				default:
					return Matrix4x4.identity;
				}
			}
		}

		public event Action onUpdatePoints;

		public void OnValidate()
		{
			time = Mathf.Max(time, 1E-05f);
			warmup = Mathf.Max(0f, warmup);
		}

		public void Awake()
		{
			Warmup();
		}

		private void OnEnable()
		{
			prevPosition = base.transform.position;
			velocity = Vector3.zero;
			mesh_ = new Mesh();
			mesh_.name = "ara_trail_mesh";
			mesh_.MarkDynamic();
			AttachToCameraRendering();
		}

		private void OnDisable()
		{
			UnityEngine.Object.DestroyImmediate(mesh_);
			DetachFromCameraRendering();
		}

		private void AttachToCameraRendering()
		{
			renderCallback = delegate(ScriptableRenderContext cntxt, Camera cam)
			{
				UpdateTrailMesh(cam);
			};
			RenderPipelineManager.beginCameraRendering += renderCallback;
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(UpdateTrailMesh));
		}

		private void DetachFromCameraRendering()
		{
			RenderPipelineManager.beginCameraRendering -= renderCallback;
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(UpdateTrailMesh));
		}

		public void Clear()
		{
			points.Clear();
		}

		private void UpdateVelocity()
		{
			if (DeltaTime > 0f)
			{
				velocity = Vector3.Lerp((base.transform.position - prevPosition) / DeltaTime, velocity, velocitySmoothing);
			}
			prevPosition = base.transform.position;
		}

		private void LateUpdate()
		{
			UpdateVelocity();
			EmissionStep(DeltaTime);
			SnapLastPointToTransform();
			UpdatePointsLifecycle();
			if (this.onUpdatePoints != null)
			{
				this.onUpdatePoints();
			}
		}

		private void EmissionStep(float time)
		{
			accumTime += time;
			if (accumTime >= timeInterval && emit)
			{
				Vector3 vector = worldToTrail.MultiplyPoint3x4(base.transform.position);
				if (points.Count <= 1 || Vector3.Distance(vector, points[points.Count - 2].position) >= minDistance)
				{
					EmitPoint(vector);
					accumTime = 0f;
				}
			}
		}

		private void Warmup()
		{
			if (!Application.isPlaying || !enablePhysics)
			{
				return;
			}
			for (float num = warmup; num > FixedDeltaTime; num -= FixedDeltaTime)
			{
				PhysicsStep(FixedDeltaTime);
				EmissionStep(FixedDeltaTime);
				SnapLastPointToTransform();
				UpdatePointsLifecycle();
				if (this.onUpdatePoints != null)
				{
					this.onUpdatePoints();
				}
			}
		}

		private void PhysicsStep(float timestep)
		{
			float num = Mathf.Pow(1f - Mathf.Clamp01(damping), timestep);
			for (int i = 0; i < points.Count; i++)
			{
				Point value = points[i];
				value.velocity += gravity * timestep;
				value.velocity *= num;
				value.position += value.velocity * timestep;
				points[i] = value;
			}
		}

		private void FixedUpdate()
		{
			if (enablePhysics)
			{
				PhysicsStep(FixedDeltaTime);
			}
		}

		public void EmitPoint(Vector3 position)
		{
			float texcoord = 0f;
			if (points.Count > 0)
			{
				texcoord = points[points.Count - 1].texcoord + Vector3.Distance(position, points[points.Count - 1].position);
			}
			points.Add(new Point(position, initialVelocity + velocity * inertia, base.transform.right, base.transform.forward, initialColor, initialThickness, texcoord, time));
		}

		private void SnapLastPointToTransform()
		{
			if (points.Count <= 0)
			{
				return;
			}
			Point value = points[points.Count - 1];
			if (!emit)
			{
				value.discontinuous = true;
			}
			if (!value.discontinuous)
			{
				value.position = worldToTrail.MultiplyPoint3x4(base.transform.position);
				value.normal = base.transform.forward;
				value.tangent = base.transform.right;
				if (points.Count > 1)
				{
					value.texcoord = points[points.Count - 2].texcoord + Vector3.Distance(value.position, points[points.Count - 2].position);
				}
			}
			points[points.Count - 1] = value;
		}

		private void UpdatePointsLifecycle()
		{
			for (int num = points.Count - 1; num >= 0; num--)
			{
				Point value = points[num];
				value.life -= DeltaTime;
				points[num] = value;
				if (value.life <= 0f)
				{
					if (smoothness <= 1)
					{
						points.RemoveAt(num);
					}
					else if (points[Mathf.Min(num + 1, points.Count - 1)].life <= 0f && points[Mathf.Min(num + 2, points.Count - 1)].life <= 0f)
					{
						points.RemoveAt(num);
					}
				}
			}
		}

		private void ClearMeshData()
		{
			mesh_.Clear();
			vertices.Clear();
			normals.Clear();
			tangents.Clear();
			uvs.Clear();
			vertColors.Clear();
			tris.Clear();
		}

		private void CommitMeshData()
		{
			mesh_.SetVertices(vertices);
			mesh_.SetNormals(normals);
			mesh_.SetTangents(tangents);
			mesh_.SetColors(vertColors);
			mesh_.SetUVs(0, uvs);
			mesh_.SetTriangles(tris, 0, true);
		}

		private void RenderMesh(Camera cam)
		{
			Matrix4x4 inverse = worldToTrail.inverse;
			for (int i = 0; i < materials.Length; i++)
			{
				Graphics.DrawMesh(mesh_, inverse, materials[i], base.gameObject.layer, cam, 0, null, castShadows, receiveShadows, null, useLightProbes);
			}
		}

		public float GetLenght(ElasticArray<Point> input)
		{
			float num = 0f;
			for (int i = 0; i < input.Count - 1; i++)
			{
				num += Vector3.Distance(input[i].position, input[i + 1].position);
			}
			return num;
		}

		private ElasticArray<Point> GetRenderablePoints(int start, int end)
		{
			renderablePoints.Clear();
			if (smoothness <= 1)
			{
				for (int i = start; i <= end; i++)
				{
					renderablePoints.Add(points[i]);
				}
				return renderablePoints;
			}
			Point[] data = points.Data;
			float num = 1f / (float)smoothness;
			Point item = new Point(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Color.white, 0f, 0f, 0f);
			for (int j = start; j < end; j++)
			{
				int num2 = ((j == start) ? start : (j - 1));
				int num3 = ((j == end - 1) ? end : (j + 2));
				int num4 = j + 1;
				float p = data[num2].position[0];
				float p2 = data[num2].position[1];
				float p3 = data[num2].position[2];
				float p4 = data[num2].velocity[0];
				float p5 = data[num2].velocity[1];
				float p6 = data[num2].velocity[2];
				float p7 = data[num2].tangent[0];
				float p8 = data[num2].tangent[1];
				float p9 = data[num2].tangent[2];
				float p10 = data[num2].normal[0];
				float p11 = data[num2].normal[1];
				float p12 = data[num2].normal[2];
				float p13 = data[num2].color[0];
				float p14 = data[num2].color[1];
				float p15 = data[num2].color[2];
				float p16 = data[num2].color[3];
				float p17 = data[j].position[0];
				float p18 = data[j].position[1];
				float p19 = data[j].position[2];
				float p20 = data[j].velocity[0];
				float p21 = data[j].velocity[1];
				float p22 = data[j].velocity[2];
				float p23 = data[j].tangent[0];
				float p24 = data[j].tangent[1];
				float p25 = data[j].tangent[2];
				float p26 = data[j].normal[0];
				float p27 = data[j].normal[1];
				float p28 = data[j].normal[2];
				float p29 = data[j].color[0];
				float p30 = data[j].color[1];
				float p31 = data[j].color[2];
				float p32 = data[j].color[3];
				float p33 = data[num4].position[0];
				float p34 = data[num4].position[1];
				float p35 = data[num4].position[2];
				float p36 = data[num4].velocity[0];
				float p37 = data[num4].velocity[1];
				float p38 = data[num4].velocity[2];
				float p39 = data[num4].tangent[0];
				float p40 = data[num4].tangent[1];
				float p41 = data[num4].tangent[2];
				float p42 = data[num4].normal[0];
				float p43 = data[num4].normal[1];
				float p44 = data[num4].normal[2];
				float p45 = data[num4].color[0];
				float p46 = data[num4].color[1];
				float p47 = data[num4].color[2];
				float p48 = data[num4].color[3];
				float p49 = data[num3].position[0];
				float p50 = data[num3].position[1];
				float p51 = data[num3].position[2];
				float p52 = data[num3].velocity[0];
				float p53 = data[num3].velocity[1];
				float p54 = data[num3].velocity[2];
				float p55 = data[num3].tangent[0];
				float p56 = data[num3].tangent[1];
				float p57 = data[num3].tangent[2];
				float p58 = data[num3].normal[0];
				float p59 = data[num3].normal[1];
				float p60 = data[num3].normal[2];
				float p61 = data[num3].color[0];
				float p62 = data[num3].color[1];
				float p63 = data[num3].color[2];
				float p64 = data[num3].color[3];
				for (int k = 0; k < smoothness; k++)
				{
					float t = (float)k * num;
					item.life = Point.CatmullRom(data[num2].life, data[j].life, data[num4].life, data[num3].life, t);
					if (item.life > 0f)
					{
						item.position.x = Point.CatmullRom(p, p17, p33, p49, t);
						item.position.y = Point.CatmullRom(p2, p18, p34, p50, t);
						item.position.z = Point.CatmullRom(p3, p19, p35, p51, t);
						item.velocity.x = Point.CatmullRom(p4, p20, p36, p52, t);
						item.velocity.y = Point.CatmullRom(p5, p21, p37, p53, t);
						item.velocity.z = Point.CatmullRom(p6, p22, p38, p54, t);
						item.tangent.x = Point.CatmullRom(p7, p23, p39, p55, t);
						item.tangent.y = Point.CatmullRom(p8, p24, p40, p56, t);
						item.tangent.z = Point.CatmullRom(p9, p25, p41, p57, t);
						item.normal.x = Point.CatmullRom(p10, p26, p42, p58, t);
						item.normal.y = Point.CatmullRom(p11, p27, p43, p59, t);
						item.normal.z = Point.CatmullRom(p12, p28, p44, p60, t);
						item.color.r = Point.CatmullRom(p13, p29, p45, p61, t);
						item.color.g = Point.CatmullRom(p14, p30, p46, p62, t);
						item.color.b = Point.CatmullRom(p15, p31, p47, p63, t);
						item.color.a = Point.CatmullRom(p16, p32, p48, p64, t);
						item.thickness = Point.CatmullRom(data[num2].thickness, data[j].thickness, data[num4].thickness, data[num3].thickness, t);
						item.texcoord = Point.CatmullRom(data[num2].texcoord, data[j].texcoord, data[num4].texcoord, data[num3].texcoord, t);
						renderablePoints.Add(item);
					}
				}
			}
			if (points[end].life > 0f)
			{
				renderablePoints.Add(points[end]);
			}
			return renderablePoints;
		}

		private CurveFrame InitializeCurveFrame(Vector3 point, Vector3 nextPoint)
		{
			Vector3 vector = nextPoint - point;
			if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(vector.normalized, base.transform.forward)), 1f))
			{
				vector += base.transform.right * 0.01f;
			}
			return new CurveFrame(point, base.transform.forward, base.transform.up, vector);
		}

		private void UpdateTrailMesh(Camera cam)
		{
			if ((cam.cullingMask & (1 << base.gameObject.layer)) == 0)
			{
				return;
			}
			ClearMeshData();
			if (points.Count <= 1)
			{
				return;
			}
			Vector3 localCamPosition = worldToTrail.MultiplyPoint3x4(cam.transform.position);
			discontinuities.Clear();
			for (int i = 0; i < points.Count; i++)
			{
				if (points[i].discontinuous || i == points.Count - 1)
				{
					discontinuities.Add(i);
				}
			}
			int start = 0;
			for (int j = 0; j < discontinuities.Count; j++)
			{
				UpdateSegmentMesh(start, discontinuities[j], localCamPosition);
				start = discontinuities[j] + 1;
			}
			CommitMeshData();
			RenderMesh(cam);
		}

		private void UpdateSegmentMesh(int start, int end, Vector3 localCamPosition)
		{
			ElasticArray<Point> elasticArray = GetRenderablePoints(start, end);
			if (sorting == TrailSorting.NewerOnTop)
			{
				elasticArray.Reverse();
			}
			Point[] data = elasticArray.Data;
			if (elasticArray.Count <= 1)
			{
				return;
			}
			float num = Mathf.Max(GetLenght(elasticArray), 1E-05f);
			float num2 = 0f;
			float num3 = ((textureMode == TextureMode.Stretch) ? 0f : ((0f - uvFactor) * num * tileAnchor));
			if (sorting == TrailSorting.NewerOnTop)
			{
				num3 = 1f - num3;
			}
			CurveFrame frame = InitializeCurveFrame(data[elasticArray.Count - 1].position, data[elasticArray.Count - 2].position);
			int va = 1;
			int vb = 0;
			for (int num4 = elasticArray.Count - 1; num4 >= 0; num4--)
			{
				int num5 = Mathf.Max(num4 - 1, 0);
				int num6 = Mathf.Min(num4 + 1, elasticArray.Count - 1);
				nextV.x = data[num5].position.x - data[num4].position.x;
				nextV.y = data[num5].position.y - data[num4].position.y;
				nextV.z = data[num5].position.z - data[num4].position.z;
				prevV.x = data[num4].position.x - data[num6].position.x;
				prevV.y = data[num4].position.y - data[num6].position.y;
				prevV.z = data[num4].position.z - data[num6].position.z;
				float num7 = ((num5 == num4) ? prevV.magnitude : nextV.magnitude);
				nextV.Normalize();
				prevV.Normalize();
				if (alignment == TrailAlignment.Local)
				{
					tangent = data[num4].tangent.normalized;
				}
				else
				{
					tangent.x = (nextV.x + prevV.x) * 0.5f;
					tangent.y = (nextV.y + prevV.y) * 0.5f;
					tangent.z = (nextV.z + prevV.z) * 0.5f;
				}
				normal = data[num4].normal;
				if (alignment != TrailAlignment.Local)
				{
					normal = ((alignment == TrailAlignment.View) ? (localCamPosition - data[num4].position) : frame.Transport(tangent, data[num4].position));
				}
				normal.Normalize();
				if (alignment == TrailAlignment.Velocity)
				{
					bitangent = frame.bitangent;
				}
				else
				{
					bitangent.x = tangent.y * normal.z - tangent.z * normal.y;
					bitangent.y = tangent.z * normal.x - tangent.x * normal.z;
					bitangent.z = tangent.x * normal.y - tangent.y * normal.x;
				}
				bitangent.Normalize();
				float num8 = ((sorting == TrailSorting.OlderOnTop) ? (num2 / num) : ((num - num2) / num));
				float num9 = Mathf.Clamp01(1f - data[num4].life / time);
				num2 += num7;
				color = data[num4].color * colorOverTime.Evaluate(num9) * colorOverLength.Evaluate(num8);
				float sectionThickness = thickness * data[num4].thickness * thicknessOverTime.Evaluate(num9) * thicknessOverLength.Evaluate(num8);
				if (textureMode == TextureMode.WorldTile)
				{
					num3 = tileAnchor + data[num4].texcoord * uvFactor;
				}
				if (section != null)
				{
					AppendSection(data, ref frame, num4, elasticArray.Count, sectionThickness, num3);
				}
				else
				{
					AppendFlatTrail(data, ref frame, num4, elasticArray.Count, sectionThickness, num3, ref va, ref vb);
				}
				float num10 = ((textureMode == TextureMode.Stretch) ? (num7 / num) : num7);
				num3 += uvFactor * ((sorting == TrailSorting.NewerOnTop) ? (0f - num10) : num10);
			}
		}

		private void AppendSection(Point[] data, ref CurveFrame frame, int i, int count, float sectionThickness, float vCoord)
		{
			int segments = section.Segments;
			int num = segments + 1;
			for (int j = 0; j <= segments; j++)
			{
				normal.x = (section.vertices[j].x * bitangent.x + section.vertices[j].y * tangent.x) * sectionThickness;
				normal.y = (section.vertices[j].x * bitangent.y + section.vertices[j].y * tangent.y) * sectionThickness;
				normal.z = (section.vertices[j].x * bitangent.z + section.vertices[j].y * tangent.z) * sectionThickness;
				vertex.x = data[i].position.x + normal.x;
				vertex.y = data[i].position.y + normal.y;
				vertex.z = data[i].position.z + normal.z;
				texTangent.x = 0f - (normal.y * frame.tangent.z - normal.z * frame.tangent.y);
				texTangent.y = 0f - (normal.z * frame.tangent.x - normal.x * frame.tangent.z);
				texTangent.z = 0f - (normal.x * frame.tangent.y - normal.y * frame.tangent.x);
				texTangent.w = 1f;
				uv.x = (float)j / (float)segments * uvWidthFactor;
				uv.y = vCoord;
				uv.z = 0f;
				uv.w = 1f;
				vertices.Add(vertex);
				normals.Add(normal);
				tangents.Add(texTangent);
				uvs.Add(uv);
				vertColors.Add(color);
				if (j < segments && i < count - 1)
				{
					tris.Add(i * num + j);
					tris.Add(i * num + (j + 1));
					tris.Add((i + 1) * num + j);
					tris.Add(i * num + (j + 1));
					tris.Add((i + 1) * num + (j + 1));
					tris.Add((i + 1) * num + j);
				}
			}
		}

		private void AppendFlatTrail(Point[] data, ref CurveFrame frame, int i, int count, float sectionThickness, float vCoord, ref int va, ref int vb)
		{
			bool num = highQualityCorners && alignment != TrailAlignment.Local;
			Quaternion quaternion = Quaternion.identity;
			Vector3 vector = Vector3.zero;
			float num2 = 0f;
			float num3 = sectionThickness;
			Vector3 vector2 = bitangent;
			if (num)
			{
				Vector3 vector3 = ((i == 0) ? bitangent : Vector3.Cross(nextV, Vector3.Cross(bitangent, tangent)).normalized);
				if (cornerRoundness > 0)
				{
					vector2 = ((i == count - 1) ? (-bitangent) : Vector3.Cross(prevV, Vector3.Cross(bitangent, tangent)).normalized);
					num2 = ((i == 0 || i == count - 1) ? 1f : Mathf.Sign(Vector3.Dot(nextV, -vector2)));
					float num4 = ((i == 0 || i == count - 1) ? ((float)Math.PI) : Mathf.Acos(Mathf.Clamp(Vector3.Dot(vector3, vector2), -1f, 1f)));
					quaternion = Quaternion.AngleAxis(57.29578f * num4 / (float)cornerRoundness, normal * num2);
					vector = vector2 * sectionThickness * num2;
				}
				if (vector3.sqrMagnitude > 0.1f)
				{
					num3 = sectionThickness / Mathf.Max(Vector3.Dot(bitangent, vector3), 0.15f);
				}
			}
			if (num && cornerRoundness > 0)
			{
				if (num2 > 0f)
				{
					vertices.Add(data[i].position + vector2 * sectionThickness);
					vertices.Add(data[i].position - bitangent * num3);
				}
				else
				{
					vertices.Add(data[i].position + bitangent * num3);
					vertices.Add(data[i].position - vector2 * sectionThickness);
				}
			}
			else
			{
				vertices.Add(data[i].position + bitangent * num3);
				vertices.Add(data[i].position - bitangent * num3);
			}
			normals.Add(normal);
			normals.Add(normal);
			tangents.Add(tangent);
			tangents.Add(tangent);
			vertColors.Add(color);
			vertColors.Add(color);
			if (quadMapping)
			{
				uv.Set(vCoord * sectionThickness, (sorting == TrailSorting.NewerOnTop) ? (uvWidthFactor * sectionThickness) : 0f, 0f, sectionThickness);
				uvs.Add(uv);
				uv.Set(vCoord * sectionThickness, (sorting == TrailSorting.NewerOnTop) ? 0f : (uvWidthFactor * sectionThickness), 0f, sectionThickness);
				uvs.Add(uv);
			}
			else
			{
				uv.Set(vCoord, (sorting == TrailSorting.NewerOnTop) ? uvWidthFactor : 0f, 0f, 1f);
				uvs.Add(uv);
				uv.Set(vCoord, (sorting == TrailSorting.NewerOnTop) ? 0f : uvWidthFactor, 0f, 1f);
				uvs.Add(uv);
			}
			if (i < count - 1)
			{
				int num5 = vertices.Count - 1;
				tris.Add(num5);
				tris.Add(va);
				tris.Add(vb);
				tris.Add(vb);
				tris.Add(num5 - 1);
				tris.Add(num5);
			}
			va = vertices.Count - 1;
			vb = vertices.Count - 2;
			if (!num || cornerRoundness <= 0)
			{
				return;
			}
			for (int j = 0; j <= cornerRoundness; j++)
			{
				vertices.Add(data[i].position + vector);
				normals.Add(normal);
				tangents.Add(tangent);
				vertColors.Add(color);
				uv.Set(vCoord, (!(num2 > 0f)) ? 1 : 0, 0f, 1f);
				uvs.Add(uv);
				int num6 = vertices.Count - 1;
				tris.Add(num6);
				tris.Add(va);
				tris.Add(vb);
				if (num2 > 0f)
				{
					vb = num6;
				}
				else
				{
					va = num6;
				}
				vector = quaternion * vector;
			}
		}
	}
}
