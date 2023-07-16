using UnityEngine;

namespace EasyCurvedLine
{
	[RequireComponent(typeof(LineRenderer))]
	public class CurvedLineRenderer : BaseMonoBehaviour
	{
		public float lineSegmentSize = 0.15f;

		public float lineWidth = 0.1f;

		[Tooltip("Enable this to set a custom width for the line end")]
		public bool useCustomEndWidth;

		[Tooltip("Custom width for the line end")]
		public float endWidth = 0.1f;

		[Header("Gizmos")]
		public bool showGizmos = true;

		public float gizmoSize = 0.1f;

		public Color gizmoColor = new Color(1f, 0f, 0f, 0.5f);

		public Vector3 Offset3 = new Vector3(0f, 0f, 0f);

		public CurvedLinePoint[] LinePoints = new CurvedLinePoint[0];

		private Vector3[] linePositions = new Vector3[0];

		private Vector3[] linePositionsOld = new Vector3[0];

		public void Update()
		{
			GetPoints();
			SetPointsToLine();
		}

		private void GetPoints()
		{
			LinePoints = GetComponentsInChildren<CurvedLinePoint>();
			linePositions = new Vector3[LinePoints.Length];
			for (int i = 0; i < LinePoints.Length; i++)
			{
				linePositions[i] = LinePoints[i].transform.position + Offset3;
			}
		}

		private void SetPointsToLine()
		{
			if (linePositionsOld.Length != linePositions.Length)
			{
				linePositionsOld = new Vector3[linePositions.Length];
			}
			bool flag = false;
			for (int i = 0; i < linePositions.Length; i++)
			{
				if (linePositions[i] != linePositionsOld[i])
				{
					flag = true;
				}
			}
			if (flag)
			{
				LineRenderer component = GetComponent<LineRenderer>();
				Vector3[] array = LineSmoother_.SmoothLine(linePositions, lineSegmentSize);
				component.positionCount = array.Length;
				component.SetPositions(array);
				component.startWidth = lineWidth;
				component.endWidth = (useCustomEndWidth ? endWidth : lineWidth);
			}
		}

		private void OnDrawGizmosSelected()
		{
			Update();
		}

		private void OnDrawGizmos()
		{
			if (LinePoints.Length == 0)
			{
				GetPoints();
			}
			CurvedLinePoint[] array = LinePoints;
			foreach (CurvedLinePoint obj in array)
			{
				obj.showGizmo = showGizmos;
				obj.gizmoSize = gizmoSize;
				obj.gizmoColor = gizmoColor;
			}
		}
	}
}
