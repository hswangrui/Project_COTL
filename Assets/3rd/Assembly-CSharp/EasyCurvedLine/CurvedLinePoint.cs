using UnityEngine;

namespace EasyCurvedLine
{
	public class CurvedLinePoint : BaseMonoBehaviour
	{
		[HideInInspector]
		public bool showGizmo = true;

		[HideInInspector]
		public float gizmoSize = 0.1f;

		[HideInInspector]
		public Color gizmoColor = new Color(1f, 0f, 0f, 0.5f);

		public bool StayLocked;

		public GameObject lockToGameObject;

		public Vector3 position;

		public Vector3 offset;

		public AnimationCurve animationCurve;

		public bool shouldAnimate;

		private float curveDeltaTime;

		public float MaxDist = 1f;

		private Vector3 currentPosition;

		private Vector3 OriginlPosition;

		private float CosAngle;

		public float CosSpeed = 10f;

		private void Start()
		{
			OriginlPosition = base.transform.position;
			currentPosition = base.transform.position;
			CosAngle = Random.Range(0, 360);
			if (lockToGameObject != null)
			{
				position = lockToGameObject.transform.position;
				base.gameObject.transform.position = position + offset;
			}
		}

		private void Update()
		{
			if (StayLocked)
			{
				position = lockToGameObject.transform.position;
				base.gameObject.transform.position = position + offset;
			}
			else if (animationCurve != null && shouldAnimate)
			{
				currentPosition = base.transform.position;
				currentPosition.z = OriginlPosition.z + MaxDist * Mathf.Cos(CosAngle += CosSpeed * Time.deltaTime);
				base.transform.position = currentPosition;
			}
		}

		private void OnDrawGizmos()
		{
			if (showGizmo)
			{
				Gizmos.color = gizmoColor;
				Gizmos.DrawSphere(base.transform.position, gizmoSize);
			}
			if (lockToGameObject != null)
			{
				position = lockToGameObject.transform.position;
				base.gameObject.transform.position = position + offset;
			}
		}

		private void OnDrawGizmosSelected()
		{
			if (lockToGameObject != null)
			{
				position = lockToGameObject.transform.position;
				base.gameObject.transform.position = position + offset;
			}
			CurvedLineRenderer component = base.transform.parent.GetComponent<CurvedLineRenderer>();
			if (component != null)
			{
				component.Update();
			}
		}
	}
}
